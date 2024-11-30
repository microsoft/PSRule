// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using PSRule.Definitions;
using PSRule.Definitions.Selectors;

namespace PSRule.Pipeline;

/// <summary>
/// Units tests for <see cref="ResourceCache"/>.
/// </summary>
public sealed class ResourceCacheTests
{
    [Fact]
    public void Import_WhenNullResource_ShouldReturnException()
    {
        var cache = new ResourceCache([]);

        Assert.Throws<ArgumentNullException>(() => cache.Import(null));
    }

    [Fact]
    public void Import_WhenValidSelector_ShouldReturnTrue()
    {
        var cache = new ResourceCache([]);
        var selector = new SelectorV1("", new SourceFile("", default, SourceType.Yaml, ""), new ResourceMetadata { Name = "test" }, default, default, new SelectorV1Spec());

        Assert.True(cache.Import(selector));
        Assert.Single(cache.OfType<SelectorV1>());
    }
}
