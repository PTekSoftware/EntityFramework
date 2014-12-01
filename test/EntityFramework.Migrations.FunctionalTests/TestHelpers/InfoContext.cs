// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;

namespace System.Data.Entity.Migrations
{
    public class InfoContext : DbContext
    {
        public IQueryable<TableInfo> Tables
        {
            get { return Set<TableInfo>().AsNoTracking(); }
        }

        public IQueryable<ColumnInfo> Columns
        {
            get { return Set<ColumnInfo>().AsNoTracking(); }
        }

        public IQueryable<TableConstraintInfo> TableConstraints
        {
            get { return Set<TableConstraintInfo>().AsNoTracking(); }
        }

        public IQueryable<KeyColumnUsageInfo> KeyColumnUsages
        {
            get { return Set<KeyColumnUsageInfo>().AsNoTracking(); }
        }

        public bool ColumnExists(string tableName, string columnName)
        {
            var tuple = ParseTableName(tableName);
            var candidates = Columns.Where(c => c.Table.Name == tuple.Item2 && c.Name == columnName).Include(c => c.Table).ToList();

            if (!candidates.Any())
            {
                return false;
            }

            return candidates.Any(c => SchemaEquals(tuple.Item1, c.Table));
        }

        public int GetColumnIndex(string tableName, string columnName)
        {
            var tuple = ParseTableName(tableName);
            var columnNames = Columns.Where(c => c.Table.Name == tuple.Item2).Select(c => c.Name).ToList();
            return columnNames.IndexOf(columnName);
        }

        public bool TableExists(string name)
        {
            var tuple = ParseTableName(name);
            var candidates = Tables.Where(t => t.Name == tuple.Item2).ToList();

            if (!candidates.Any())
            {
                return false;
            }

            return candidates.Any(t => SchemaEquals(tuple.Item1, t));
        }

        public override int SaveChanges()
        {
            throw new InvalidOperationException("This context is read-only.");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TableInfo>(b =>
                {
                    b.ForRelational().Table("TABLES", "INFORMATION_SCHEMA");
                    b.Property(t => t.Schema).ForRelational().Column("TABLE_SCHEMA");
                    b.Property(t => t.Name).ForRelational().Column("TABLE_NAME");
                    b.Key(t => new { t.Schema, t.Name });
                    b.OneToMany(t => t.Columns, t => t.Table)
                        .ForeignKey(t => new { t.TableSchema, t.TableName });
                    b.OneToMany(t => t.Constraints, t => t.Table)
                        .ForeignKey(t => new { t.TableSchema, t.TableName });
                });

            modelBuilder.Entity<ColumnInfo>(b =>
                {
                    b.ForRelational().Table("COLUMNS", "INFORMATION_SCHEMA");
                    b.Property(t => t.TableSchema).ForRelational().Column("TABLE_SCHEMA");
                    b.Property(t => t.TableName).ForRelational().Column("TABLE_NAME");
                    b.Property(t => t.Name).ForRelational().Column("COLUMN_NAME");
                    b.Property(t => t.Position).ForRelational().Column("ORDINAL_POSITION");
                    b.Property(t => t.Default).ForRelational().Column("COLUMN_DEFAULT");
                    b.Property(t => t.IsNullable).ForRelational().Column("IS_NULLABLE");
                    b.Property(t => t.Type).ForRelational().Column("DATA_TYPE");
                    b.Property(t => t.MaxLength).ForRelational().Column("CHARACTER_MAXIMUM_LENGTH");
                    b.Property(t => t.NumericPrecision).ForRelational().Column("NUMERIC_PRECISION");
                    b.Property(t => t.Scale).ForRelational().Column("NUMERIC_SCALE");
                    b.Property(t => t.DateTimePrecision).ForRelational().Column("DATETIME_PRECISION");
                    b.Property(c => c.Scale).HasColumnType("int");
                    b.Property(c => c.Collation).ForRelational().Column("COLLATION_NAME");
                    b.Key(t => new { t.TableSchema, t.TableName, t.Name });
                });

            modelBuilder.Entity<TableConstraintInfo>(b =>
                {
                    b.ForRelational().Table("TABLE_CONSTRAINTS", "INFORMATION_SCHEMA");
                    b.Property(t => t.Schema).ForRelational().Column("CONSTRAINT_SCHEMA");
                    b.Property(t => t.Name).ForRelational().Column("CONSTRAINT_NAME");
                    b.Property(t => t.TableSchema).ForRelational().Column("TABLE_SCHEMA");
                    b.Property(t => t.TableName).ForRelational().Column("TABLE_NAME");
                    b.Key(t => new { t.Schema, t.Name });
                });

            var uniqueConstraint = modelBuilder.Entity<UniqueConstraintInfo>();
            uniqueConstraint.Map(m => m.Requires("CONSTRAINT_TYPE").HasValue("UNIQUE"));

            var primaryKeyConstraint = modelBuilder.Entity<PrimaryKeyConstraintInfo>();
            primaryKeyConstraint.Map(m => m.Requires("CONSTRAINT_TYPE").HasValue("PRIMARY KEY"));

            var foreignKeyConstraint = modelBuilder.Entity<ForeignKeyConstraintInfo>();
            foreignKeyConstraint.Map(m => m.Requires("CONSTRAINT_TYPE").HasValue("FOREIGN KEY"));

            modelBuilder.Entity<ReferentialConstraintInfo>(b =>
                {
                    b.ForRelational().Table("REFERENTIAL_CONSTRAINTS", "INFORMATION_SCHEMA");
                    b.Property(rc => rc.UniqueConstraintSchema).ForRelational().Column("UNIQUE_CONSTRAINT_SCHEMA");
                    b.Property(rc => rc.UniqueConstraintName).ForRelational().Column("UNIQUE_CONSTRAINT_NAME");
                    b.Property(rc => rc.DeleteRule).ForRelational().Column("DELETE_RULE");
                    b.ManyToOne(t => t.UniqueConstraint, t => t.ReferentialConstraints)
                        .ForeignKey(t => new { t.UniqueConstraintSchema, t.UniqueConstraintName });
                });

            modelBuilder.Entity<KeyColumnUsageInfo>(b =>
                {
                    b.ForRelational().Table("KEY_COLUMN_USAGE", "INFORMATION_SCHEMA");
                    b.Property(t => t.ConstraintSchema).ForRelational().Column("CONSTRAINT_SCHEMA");
                    b.Property(t => t.ConstraintName).ForRelational().Column("CONSTRAINT_NAME");
                    b.Property(t => t.ColumnTableSchema).ForRelational().Column("TABLE_SCHEMA");
                    b.Property(t => t.ColumnTableName).ForRelational().Column("TABLE_NAME");
                    b.Property(t => t.ColumnName).ForRelational().Column("COLUMN_NAME");
                    b.Property(t => t.Position).ForRelational().Column("ORDINAL_POSITION");
                    b.Key(t => new { t.ConstraintSchema, t.ConstraintName, t.ColumnTableSchema, t.ColumnTableName, t.ColumnName });
                    b.ManyToOne(t => t.Constraint, t => t.KeyColumnUsages)
                        .ForeignKey(t => new { t.ConstraintSchema, t.ConstraintName });
                    b.ManyToOne(t => t.Column, t => t.KeyColumnUsages)
                        .ForeignKey(t => new { t.ColumnTableSchema, t.ColumnTableName, t.ColumnName });
                });
        }

        private static Tuple<string, string> ParseTableName(string name)
        {
            var lastDot = name.LastIndexOf('.');

            if (lastDot == -1)
            {
                return new Tuple<string, string>(null, name);
            }

            return new Tuple<string, string>(
                name.Substring(0, lastDot),
                name.Substring(lastDot + 1));
        }

        private bool SchemaEquals(string schema, TableInfo table)
        {
            if (!_supportsSchema
                || string.IsNullOrWhiteSpace(schema))
            {
                return true;
            }

            return table.Schema == schema;
        }
    }

    #region Entity types

    public class TableInfo
    {
        public TableInfo()
        {
            Columns = new HashSet<ColumnInfo>();
            Constraints = new HashSet<TableConstraintInfo>();
        }

        public string Schema { get; set; }
        public string Name { get; set; }

        public virtual ICollection<ColumnInfo> Columns { get; protected set; }
        public virtual ICollection<TableConstraintInfo> Constraints { get; protected set; }
    }

    public class ColumnInfo
    {
        public ColumnInfo()
        {
            KeyColumnUsages = new HashSet<KeyColumnUsageInfo>();
        }

        public string TableSchema { get; set; }
        public string TableName { get; set; }
        public virtual TableInfo Table { get; set; }

        public string Name { get; set; }
        public int Position { get; set; }
        public string Default { get; set; }
        public string IsNullable { get; set; }
        public string Type { get; set; }
        public int? MaxLength { get; set; }
        public byte? NumericPrecision { get; set; }
        public short? Scale { get; set; }
        public short? DateTimePrecision { get; set; }
        public string Collation { get; set; }

        public virtual ICollection<KeyColumnUsageInfo> KeyColumnUsages { get; protected set; }
    }

    public class TableConstraintInfo
    {
        public string Schema { get; set; }
        public string Name { get; set; }

        public string TableSchema { get; set; }
        public string TableName { get; set; }
        public virtual TableInfo Table { get; set; }
    }

    public abstract class KeyConstraintInfo : TableConstraintInfo
    {
        public KeyConstraintInfo()
        {
            KeyColumnUsages = new HashSet<KeyColumnUsageInfo>();
        }

        public virtual ICollection<KeyColumnUsageInfo> KeyColumnUsages { get; protected set; }
    }

    public abstract class UniqueConstraintInfoBase : KeyConstraintInfo
    {
        protected UniqueConstraintInfoBase()
        {
            ReferentialConstraints = new HashSet<ReferentialConstraintInfo>();
        }

        public virtual ICollection<ReferentialConstraintInfo> ReferentialConstraints { get; protected set; }
    }

    public class UniqueConstraintInfo : UniqueConstraintInfoBase
    {
    }

    public class PrimaryKeyConstraintInfo : UniqueConstraintInfoBase
    {
    }

    public abstract class ForeignKeyConstraintInfo : KeyConstraintInfo
    {
    }

    public class ReferentialConstraintInfo : ForeignKeyConstraintInfo
    {
        public string UniqueConstraintSchema { get; set; }
        public string UniqueConstraintName { get; set; }
        public virtual UniqueConstraintInfoBase UniqueConstraint { get; set; }

        public string DeleteRule { get; set; }
    }

    public class KeyColumnUsageInfo
    {
        public string ConstraintSchema { get; set; }
        public string ConstraintName { get; set; }
        public KeyConstraintInfo Constraint { get; set; }

        public string ColumnTableSchema { get; set; }
        public string ColumnTableName { get; set; }
        public string ColumnName { get; set; }
        public ColumnInfo Column { get; set; }

        public int Position { get; set; }
    }

    #endregion
}
