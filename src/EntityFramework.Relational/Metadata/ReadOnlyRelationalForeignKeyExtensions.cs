// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Utilities;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Metadata
{
    public class ReadOnlyRelationalForeignKeyExtensions : IRelationalForeignKeyExtensions
    {
        protected const string NameAnnotation = RelationalAnnotationNames.Prefix + RelationalAnnotationNames.Name;

        private readonly IForeignKey _foreignKey;

        public ReadOnlyRelationalForeignKeyExtensions([NotNull] IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            _foreignKey = foreignKey;
        }

        public virtual string Name
        {
            get { return _foreignKey[NameAnnotation]; }
        }

        protected virtual IForeignKey ForeignKey
        {
            get { return _foreignKey; }
        }
    }
}
