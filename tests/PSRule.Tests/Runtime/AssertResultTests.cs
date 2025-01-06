// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime;

/// <summary>
/// Tests for <see cref="AssertResult"/>.
/// </summary>
public sealed class AssertResultTests
{
    [Fact]
    public void AddReason_WithText_ShouldAddReason()
    {
        var result = new AssertResult(Operand.FromPath("name"), false, "reason 1", null);
        result.AddReason("reason 2");

        Xunit.Assert.Equal("Path name: reason 1 reason 2", result.ToString());
    }

    [Fact]
    public void AddReason_WithDuplicate_ShouldNotAddReason()
    {
        // With operand.
        var result = new AssertResult(Operand.FromPath("name"), false, "reason 1", null);
        result.AddReason(Operand.FromPath("name"), "reason 1");

        Xunit.Assert.Equal("Path name: reason 1", result.ToString());

        // Without operand.
        result = new AssertResult(null, false, "reason 1", null);
        result.AddReason("reason 1");

        Xunit.Assert.Equal("reason 1", result.ToString());
    }
}
