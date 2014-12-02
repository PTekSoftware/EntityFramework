﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.AzureTableStorage.Utilities
{
    internal static class EntityTypeExtensions
    {
        [CanBeNull]
        public static IProperty TryGetPropertyByColumnName([NotNull] this IEntityType entityType, [NotNull] string name)
        {
            Check.NotNull(entityType, "entityType");
            Check.NotEmpty(name, "name");

            return entityType.Properties.FirstOrDefault(s => s.AzureTableStorage().Column == name);
        }

        [NotNull]
        public static IProperty GetPropertyByColumnName([NotNull] this IEntityType entityType, [NotNull] string name)
        {
            Check.NotNull(entityType, "entityType");
            Check.NotEmpty(name, "name");

            var property = TryGetPropertyByColumnName(entityType, name);
            if (property == null)
            {
                throw new ModelItemNotFoundException(Strings.PropertyWithStorageNameNotFound(name, entityType.Name));
            }
            return property;
        }
    }
}
