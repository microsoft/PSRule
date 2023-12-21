// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;

namespace PSRule;

public sealed class ResourceHelperTests
{
    [Fact]
    public void ParseIdString()
    {
        ResourceHelper.ParseIdString(null, "Module1\\Resource1", out var moduleName, out var name);
        Assert.Equal("Module1", moduleName);
        Assert.Equal("Resource1", name);

        ResourceHelper.ParseIdString("Module2", "Module1\\Resource1", out moduleName, out name);
        Assert.Equal("Module1", moduleName);
        Assert.Equal("Resource1", name);

        ResourceHelper.ParseIdString("Module2", ".\\Resource1", out moduleName, out name);
        Assert.Equal(".", moduleName);
        Assert.Equal("Resource1", name);

        ResourceHelper.ParseIdString(null, ".\\Resource1", out moduleName, out name);
        Assert.Equal(".", moduleName);
        Assert.Equal("Resource1", name);

        ResourceHelper.ParseIdString(null, "Resource1", out moduleName, out name);
        Assert.Equal(".", moduleName);
        Assert.Equal("Resource1", name);

        ResourceHelper.ParseIdString("Module2", "Resource1", out moduleName, out name);
        Assert.Equal("Module2", moduleName);
        Assert.Equal("Resource1", name);
    }
}
