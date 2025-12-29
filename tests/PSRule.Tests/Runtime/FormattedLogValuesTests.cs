// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using XAssert = Xunit.Assert;

namespace PSRule.Runtime;

/// <summary>
/// Unit  tests for <see cref="FormattedLogValues"/>.
/// </summary>
public sealed class FormattedLogValuesTests
{
    [Fact]
    public void ToString_WithSimpleString_ShouldReturnOriginalMessage()
    {
        var logValues = new FormattedLogValues("Test message with no values");

        XAssert.Equal("Test message with no values", logValues.ToString());

        XAssert.Single(logValues);

        XAssert.Equal("{OriginalFormat}", logValues[0].Key);
        XAssert.Equal("Test message with no values", logValues[0].Value);
    }

    [Fact]
    public void ToString_WithValues_ShouldFormatMessage()
    {
        var logValues = new FormattedLogValues("Value1: {0}, Value2: {1}", 42, "Test");

        XAssert.Equal("Value1: 42, Value2: Test", logValues.ToString());

        XAssert.Equal(3, logValues.Count);

        XAssert.Equal("0", logValues[0].Key);
        XAssert.Equal(42, logValues[0].Value);

        XAssert.Equal("1", logValues[1].Key);
        XAssert.Equal("Test", logValues[1].Value);

        XAssert.Equal("{OriginalFormat}", logValues[2].Key);
        XAssert.Equal("Value1: {0}, Value2: {1}", logValues[2].Value);
    }
}
