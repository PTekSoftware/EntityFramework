// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Builders;
using Xunit;

namespace System.Data.Entity.Migrations
{
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.VB)]
    public class CreateTableScenarios : DbTestCase
    {
        private class CreateOobTableFkMigration : Migration
        {
            public override void Up(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.CreateTable(
                    "Oob_Principal", t => new
                        {
                            Id = t.Int()
                        })
                    .PrimaryKey("PK", t => t.Id);

                migrationBuilder.CreateTable(
                    "Oob_Dependent", t => new
                        {
                            Id = t.Int(),
                            Fk = t.Int()
                        })
                    .ForeignKey("FK", t => t.Fk, "Oob_Principal", new[] { "Id" });
            }
        }

        [MigrationsTheory]
        public void Can_create_oob_table_with_inline_fk()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.ApplyMigrations();

            migrator = CreateMigrator<ShopContext_v1>(new CreateOobTableFkMigration());

            migrator.ApplyMigrations();

            var principalTable = Info.Tables.SingleOrDefault(t => t.Name == "Oob_Principal");
            Assert.NotNull(principalTable);
            Assert.Equal(1, principalTable.Columns.Count());
            Assert.True(principalTable.Columns.Any(c => c.Name == "Id" && c.Type == "int"));
            var principalPrimaryKey = principalTable.Constraints.OfType<PrimaryKeyConstraintInfo>().SingleOrDefault();
            Assert.NotNull(principalPrimaryKey);
            Assert.Equal(1, principalPrimaryKey.KeyColumnUsages.Count());
            Assert.True(principalPrimaryKey.KeyColumnUsages.Any(kcu => kcu.ColumnName == "Id"));
            var dependentTable = Info.Tables.SingleOrDefault(t => t.Name == "Oob_Dependent");
            Assert.Equal(2, dependentTable.Columns.Count());
            Assert.True(dependentTable.Columns.Any(c => c.Name == "Id" && c.Type == "int"));
            Assert.True(dependentTable.Columns.Any(c => c.Name == "Fk" && c.Type == "int"));
            var foreignKey = dependentTable.Constraints.OfType<ReferentialConstraintInfo>().SingleOrDefault();
            Assert.NotNull(foreignKey);
            Assert.Equal(1, foreignKey.KeyColumnUsages.Count());
            Assert.True(foreignKey.KeyColumnUsages.Any(kcu => kcu.ColumnName == "Fk"));
            Assert.Equal(1, foreignKey.UniqueConstraint.KeyColumnUsages.Count());
            Assert.True(
                foreignKey.UniqueConstraint.KeyColumnUsages.Any(kcu => kcu.ColumnTableName == "Oob_Principal" && kcu.ColumnName == "Id"));
        }

        private class CreateOobTableInvalidFkMigration : Migration
        {
            public override void Up(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.CreateTable(
                    "Oob_Dependent", t => new
                        {
                            Id = t.Int(),
                            Fk = t.Int()
                        })
                    .ForeignKey("FK", t => t.Fk, "Oob_Principal", new[] { "Id" });
            }
        }

        [MigrationsTheory]
        public void Throws_on_create_oob_table_with_invalid_fk()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.ApplyMigrations();

            migrator = CreateMigrator<ShopContext_v1>(new CreateOobTableInvalidFkMigration());

            Assert.Throws<MigrationsException>(() => migrator.ApplyMigrations())
                  .ValidateMessage("PartialFkOperation", "Oob_Dependent", "Fk");
        }

        private class CreateCustomColumnNameMigration : Migration
        {
            public override void Up(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.CreateTable(
                    "Foo", t => new
                        {
                            Id = t.Int(name: "12 Foo Id")
                        });
            }
        }

        [MigrationsTheory]
        public void Can_create_table_with_custom_column_name()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.ApplyMigrations();

            migrator = CreateMigrator<ShopContext_v1>(new CreateCustomColumnNameMigration());

            migrator.ApplyMigrations();

            Assert.True(ColumnExists("Foo", "12 Foo Id"));
        }

        private class CreateCustomClusteredIndex : Migration
        {
            public override void Up(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.CreateTable(
                    "Foo", t => new
                                    {
                                        Id = t.Int(nullable: false),
                                        Ix = t.Int()
                                    })
                    .PrimaryKey("PK", t => t.Id, clustered: false)
                    .Index("IX", t => t.Ix, clustered: true);
            }
        }

        [MigrationsTheory]
        public void Can_create_table_with_custom_clustered_index()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v2>(new CreateCustomClusteredIndex());

            migrator.ApplyMigrations();
        }

        private class CreateTableWithTableAnnotationMigration : Migration
        {
            public override void Up(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.CreateTable(
                    "Foo", t => new
                    {
                        Id = t.Int(),
                        Beanie = t.String(),
                        Boo = t.String()
                    },
                    new Dictionary<string, object>
                    {
                        {
                            CollationAttribute.AnnotationName,
                            new CollationAttribute("Finnish_Swedish_CS_AS")
                        }
                    })
                    .PrimaryKey("PK", t => t.Id);
            }
        }

        [MigrationsTheory]
        [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)] // No collation on add column in CE
        public void Can_create_table_with_custom_table_annotations()
        {
            ResetDatabase();

            var sqlGenerators = new[]
            {
                Tuple.Create<string, MigrationSqlGenerator>(
                    SqlProviderServices.ProviderInvariantName,
                    new SqlServerMigrationSqlGeneratorWtihCollations()),
            };

            var migrator = CreateMigrator<ShopContext_v1>(sqlGenerators: sqlGenerators);
            migrator.ApplyMigrations();

            migrator = CreateMigrator<ShopContext_v1>(new CreateTableWithTableAnnotationMigration(), sqlGenerators);
            migrator.ApplyMigrations();

            var column = Info.Columns.Single(c => c.TableName == "Foo" && c.Name == "Beanie");
            Assert.Equal("Finnish_Swedish_CS_AS", column.Collation);

            column = Info.Columns.Single(c => c.TableName == "Foo" && c.Name == "Boo");
            Assert.Equal("Finnish_Swedish_CS_AS", column.Collation);
        }

        private class CreateTableWithColumnAnnotationMigration : Migration
        {
            public override void Up(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.CreateTable(
                    "Foo", t => new
                    {
                        Id = t.Int(),
                        Bar = t.String(
                            annotations:
                                new Dictionary<string, AnnotationValues>
                                {
                                    {
                                        CollationAttribute.AnnotationName,
                                        new AnnotationValues(null, new CollationAttribute("Finnish_Swedish_CS_AS"))
                                    }
                                })
                    })
                    .PrimaryKey("PK", t => t.Id);
            }
        }

        [MigrationsTheory]
        [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)] // No collation on add column in CE
        public void Can_create_table_with_custom_column_annotations()
        {
            ResetDatabase();

            var sqlGenerators = new[]
            {
                Tuple.Create<string, MigrationSqlGenerator>(
                    SqlProviderServices.ProviderInvariantName,
                    new SqlServerMigrationSqlGeneratorWtihCollations()),
            };

            var migrator = CreateMigrator<ShopContext_v1>(sqlGenerators: sqlGenerators);
            migrator.ApplyMigrations();

            migrator = CreateMigrator<ShopContext_v1>(new CreateTableWithColumnAnnotationMigration(), sqlGenerators);
            migrator.ApplyMigrations();

            var column = Info.Columns.Single(c => c.TableName == "Foo" && c.Name == "Bar");
            Assert.Equal("Finnish_Swedish_CS_AS", column.Collation);
        }

        private class CreateTableWithAllAnnotationMigration : Migration
        {
            public override void Up(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.CreateTable(
                    "Foo", t => new
                    {
                        Id = t.Int(),
                        Beanie = t.String(),
                        Bar = t.String(
                            annotations:
                                new Dictionary<string, AnnotationValues>
                                {
                                    {
                                        CollationAttribute.AnnotationName,
                                        new AnnotationValues(null, new CollationAttribute("Icelandic_CS_AS"))
                                    }
                                }),
                        Boo = t.String()
                    },
                    new Dictionary<string, object>
                    {
                        {
                            CollationAttribute.AnnotationName,
                            new CollationAttribute("Finnish_Swedish_CS_AS")
                        }
                    })
                    .PrimaryKey("PK", t => t.Id);
            }
        }

        [MigrationsTheory]
        [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)] // No collation on add column in CE
        public void Can_create_table_with_custom_table_and_columnannotations()
        {
            ResetDatabase();

            var sqlGenerators = new[]
            {
                Tuple.Create<string, MigrationSqlGenerator>(
                    SqlProviderServices.ProviderInvariantName,
                    new SqlServerMigrationSqlGeneratorWtihCollations()),
            };

            var migrator = CreateMigrator<ShopContext_v1>(sqlGenerators: sqlGenerators);
            migrator.ApplyMigrations();

            migrator = CreateMigrator<ShopContext_v1>(new CreateTableWithAllAnnotationMigration(), sqlGenerators);
            migrator.ApplyMigrations();

            var column = Info.Columns.Single(c => c.TableName == "Foo" && c.Name == "Beanie");
            Assert.Equal("Finnish_Swedish_CS_AS", column.Collation);

            column = Info.Columns.Single(c => c.TableName == "Foo" && c.Name == "Boo");
            Assert.Equal("Finnish_Swedish_CS_AS", column.Collation);

            column = Info.Columns.Single(c => c.TableName == "Foo" && c.Name == "Bar");
            Assert.Equal("Icelandic_CS_AS", column.Collation);
        }

        protected override void ModifyMigrationsConfiguration(DbMigrationsConfiguration configuration)
        {
            configuration.CodeGenerator.AnnotationGenerators[CollationAttribute.AnnotationName] = () => new CollationCSharpCodeGenerator();
        }
    }
}
