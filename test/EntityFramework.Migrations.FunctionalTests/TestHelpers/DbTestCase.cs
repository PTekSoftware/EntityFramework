// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Migrations.Model;

namespace System.Data.Entity.Migrations
{
    using System.Data.Common;
    using System.Globalization;

    public enum DatabaseProvider
    {
        SqlClient,
        SqlServerCe
    }

    public enum ProgrammingLanguage
    {
        CSharp,
        VB
    }

    public class BlankSlate : DbContext
    {
    }

    public abstract class DbTestCase
    {
        private DatabaseProvider _databaseProvider = DatabaseProvider.SqlClient;
        private ProgrammingLanguage _programmingLanguage = ProgrammingLanguage.CSharp;

        public DatabaseProvider DatabaseProvider
        {
            get { return _databaseProvider; }
            set
            {
                _databaseProvider = value;
                TestDatabase = _databaseProviderFixture.TestDatabases[_databaseProvider];
            }
        }

        public ProgrammingLanguage ProgrammingLanguage
        {
            get { return _programmingLanguage; }
            set
            {
                _programmingLanguage = value;
                CodeGenerator = _databaseProviderFixture.CodeGenerators[_programmingLanguage];
                MigrationCompiler = _databaseProviderFixture.MigrationCompilers[_programmingLanguage];
            }
        }

        public TestDatabase TestDatabase { get; private set; }

        public MigrationCodeGenerator CodeGenerator { get; private set; }

        public MigrationCompiler MigrationCompiler { get; private set; }

        public virtual void Init(DatabaseProvider provider, ProgrammingLanguage language)
        {
            try
            {
                _databaseProvider = provider;
                _programmingLanguage = language;

                TestDatabase = _databaseProviderFixture.TestDatabases[_databaseProvider];
                CodeGenerator = _databaseProviderFixture.CodeGenerators[_programmingLanguage];
                MigrationCompiler = _databaseProviderFixture.MigrationCompilers[_programmingLanguage];
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                throw;
            }
        }

        public bool IsSqlCe
        {
            get { return _databaseProvider == DatabaseProvider.SqlServerCe; }
        }

        public void WhenSqlCe(Action action)
        {
            if (_databaseProvider == DatabaseProvider.SqlServerCe)
            {
                action();
            }
        }

        public void WhenNotSqlCe(Action action)
        {
            if (_databaseProvider != DatabaseProvider.SqlServerCe)
            {
                action();
            }
        }

        public Migrator CreateMigrator<TContext>()
        {
            throw new NotImplementedException();
        }

        public void ResetDatabase()
        {
            if (DatabaseExists())
            {
                TestDatabase.ResetDatabase();
            }
            else
            {
                TestDatabase.EnsureDatabase();
            }
        }

        public void DropDatabase()
        {
            if (DatabaseExists())
            {
                TestDatabase.DropDatabase();
            }
        }

        public bool DatabaseExists()
        {
            return TestDatabase.Exists();
        }

        public bool TableExists(string name)
        {
            return Info.TableExists(name);
        }

        public bool ColumnExists(string table, string name)
        {
            return Info.ColumnExists(table, name);
        }

        public int GetColumnIndex(string table, string name)
        {
            return Info.GetColumnIndex(table, name);
        }

        public string ConnectionString
        {
            get { return TestDatabase.ConnectionString; }
        }

        public DbProviderFactory ProviderFactory
        {
            get { return DbProviderFactories.GetFactory(TestDatabase.ProviderName); }
        }

        public string ProviderManifestToken
        {
            get { return TestDatabase.ProviderManifestToken; }
        }

        public MigrationSqlGenerator SqlGenerator
        {
            get { return TestDatabase.SqlGenerator; }
        }

        public InfoContext Info
        {
            get { return TestDatabase.Info; }
        }

        public void ExecuteOperations(params MigrationOperation[] operations)
        {
            using (var connection = ProviderFactory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;

                foreach (var migrationStatement in SqlGenerator.Generate(operations, ProviderManifestToken))
                {
                    using (var command = connection.CreateCommand())
                    {
                        if (connection.State
                            != ConnectionState.Open)
                        {
                            connection.Open();
                        }

                        command.CommandText = migrationStatement.Sql;
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        protected string GenerateUniqueMigrationName(string migrationName)
        {
            return DateTime.UtcNow.ToString("yyyyMMddHHmmssf", CultureInfo.InvariantCulture) + "_" + migrationName;
        }
    }
}
