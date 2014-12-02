// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.InMemory;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class InMemoryDatabaseExtensions
    {
        public static InMemoryDatabaseFacade AsInMemory([NotNull] this Database database)
        {
            Check.NotNull(database, "database");

            var sqliteDatabase = database as InMemoryDatabaseFacade;

            if (sqliteDatabase == null)
            {
                throw new InvalidOperationException(Strings.InMemoryNotInUse);
            }

            return sqliteDatabase;
        }
    }
}
