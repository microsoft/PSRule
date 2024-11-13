// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using PSRule.Host;
using PSRule.Runtime;

namespace PSRule;

public sealed class ModuleConfigTests : ContextBaseTests
{
    [Theory]
    [InlineData("ModuleConfig.Rule.yaml")]
    [InlineData("ModuleConfig.Rule.jsonc")]
    public void ReadModuleConfig(string path)
    {
        var context = new RunspaceContext(GetPipelineContext());
        var configuration = HostHelper.GetModuleConfigForTests(GetSource(path), context).ToArray();
        Assert.NotNull(configuration);
        Assert.Equal("Configuration1", configuration[0].Name);
    }
}
