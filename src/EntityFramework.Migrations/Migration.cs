﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Builders;

namespace Microsoft.Data.Entity.Migrations
{
    public abstract class Migration : MetadataBase
    {
        public abstract void Up([NotNull] MigrationBuilder migrationBuilder);

        public virtual void Down([NotNull] MigrationBuilder migrationBuilder)
        {            
        }
    }
}
