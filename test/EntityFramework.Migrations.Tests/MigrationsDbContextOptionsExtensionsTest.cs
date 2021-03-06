// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Migrations;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Tests
{
    public class MigrationsDbContextOptionsExtensionsTest
    {
        [Fact]
        public void Can_add_extension_with_migration_assembly()
        {
            var options = new DbContextOptions<DbContext>();

            options = options.UseMigrationAssembly(typeof(string).Assembly);

            var extension = MigrationsOptionsExtension.Extract(options);

            Assert.Same(typeof(string).Assembly, extension.MigrationAssembly);
        }

        [Fact]
        public void Can_add_extension_with_migration_namespace()
        {
            var options = new DbContextOptions<DbContext>();

            options = options.UseMigrationNamespace("Foo");

            var extension = MigrationsOptionsExtension.Extract(options);

            Assert.Equal("Foo", extension.MigrationNamespace);
        }
    }
}
