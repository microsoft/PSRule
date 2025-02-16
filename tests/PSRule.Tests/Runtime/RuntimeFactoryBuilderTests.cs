// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using _Assert = Xunit.Assert;

namespace PSRule.Runtime;

#nullable enable

/// <summary>
/// Unit tests for <see cref="RuntimeFactoryBuilder"/>.
/// </summary>
public sealed class RuntimeFactoryBuilderTests
{
    [Fact]
    public void BuildFromAssembly_WithTestAssembly_ShouldFindFactories()
    {
        var logger = new Mock<ILogger>(MockBehavior.Loose);
        logger.Setup(m => m.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        // Should expect 1 error log for BadRuntimeFactory.
        logger.Setup(m => m.Log(It.IsAny<LogLevel>(), It.Is<EventId>(id => id.Id == 14), It.IsAny<FormattedLogValues>(), It.IsAny<InvalidOperationException>(), It.IsAny<Func<FormattedLogValues, Exception?, string>>()));

        var builder = new RuntimeFactoryBuilder(logger.Object);
        var container = builder.BuildFromAssembly("test", [typeof(RuntimeFactoryBuilderTests).Assembly]);

        _Assert.NotNull(container);
        _Assert.Equal("test", container.Scope);
        _Assert.Contains(container.Factories, f => f.GetType() == typeof(SimpleRuntimeFactory));

        logger.VerifyAll();
    }

    [Fact]
    public void BuildFromAssembly_WithNoAssembly_ShouldThrow()
    {
        var builder = new RuntimeFactoryBuilder(null);
        _Assert.Throws<ArgumentException>(() => builder.BuildFromAssembly("test", null));
        _Assert.Throws<ArgumentException>(() => builder.BuildFromAssembly("test", []));
    }

    [Fact]
    public void BuildFromAssembly_WithNoScope_ShouldThrow()
    {
        var builder = new RuntimeFactoryBuilder(null);
        _Assert.Throws<ArgumentNullException>(() => builder.BuildFromAssembly(null, [typeof(RuntimeFactoryBuilderTests).Assembly]));
        _Assert.Throws<ArgumentNullException>(() => builder.BuildFromAssembly("", [typeof(RuntimeFactoryBuilderTests).Assembly]));
    }
}

#nullable restore
