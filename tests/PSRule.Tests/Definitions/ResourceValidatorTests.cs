// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;

namespace PSRule.Definitions;

/// <summary>
/// Unit tests for <see cref="ResourceValidator"/>.
/// </summary>
public sealed class ResourceValidatorTests : ContextBaseTests
{
    [Fact]
    public void Visit_WithKnownResource_ShouldReturnTrue()
    {
        var sources = GetSource("Selectors.Rule.yaml");
        var resourcesCache = GetResourceCache(option: GetOption(), sources: sources).ToArray();

        var option = GetOption();
        var writer = new TestWriter(option);
        var validator = new ResourceValidator(writer);

        for (var i = 0; i < resourcesCache.Length; i++)
        {
            var resource = resourcesCache[i];
            var result = validator.Visit(resource);
            Assert.True(result);
        }
    }
}
