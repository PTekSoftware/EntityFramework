// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class SqlServerAsNoTrackingTest : AsNoTrackingTestBase<SqlServerNorthwindQueryFixture>
    {
        public SqlServerAsNoTrackingTest(SqlServerNorthwindQueryFixture fixture)
            : base(fixture)
        {
        }

        public override void Can_get_current_values()
        {
            base.Can_get_current_values();
        }
    }
}
