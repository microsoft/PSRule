// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline.Emitters;

namespace PSRule.Emitters;

/// <summary>
/// Unit tests for <see cref="EmitterBuilder"/>.
/// </summary>
public sealed class EmitterBuilderTests : BaseTests
{
    /// <summary>
    /// Test that a collection contains the default emitters.
    /// </summary>
    [Fact]
    public void Build_WhenNull_ShouldAddDefaultEmitters()
    {
        // Check for default emitters.
        var collection = new EmitterBuilder().Build(new TestEmitterContext());
        Assert.Equal(4, collection.Count);
    }
}
