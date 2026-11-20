// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using PSRule.Runtime;

namespace PSRule.Tests.Runtime;

/// <summary>
/// Tests for convention registration via IRuntimeServiceCollection.
/// </summary>
public sealed class ConventionRegistrationTests
{
    /// <summary>
    /// Test that conventions can be registered via AddConvention method.
    /// </summary>
    [Fact]
    public void AddConvention_WhenCalled_ShouldRegisterConvention()
    {
        // Arrange
        var scope = new LanguageScope("test", null);
        var serviceCollection = scope as IRuntimeServiceCollection;

        // Act
        serviceCollection.AddConvention<TestConvention>();

        // Assert
        var conventions = scope.GetConventions();
        Assert.Single(conventions);
        Assert.Equal(typeof(TestConvention), conventions.First());
    }

    /// <summary>
    /// Test that multiple conventions can be registered.
    /// </summary>
    [Fact]
    public void AddConvention_WhenMultipleConventions_ShouldRegisterAll()
    {
        // Arrange
        var scope = new LanguageScope("test", null);
        var serviceCollection = scope as IRuntimeServiceCollection;

        // Act
        serviceCollection.AddConvention<TestConvention>();
        serviceCollection.AddConvention<TestConvention2>();

        // Assert
        var conventions = scope.GetConventions().ToArray();
        Assert.Equal(2, conventions.Length);
        Assert.Contains(typeof(TestConvention), conventions);
        Assert.Contains(typeof(TestConvention2), conventions);
    }

    /// <summary>
    /// Test that GetConventions returns empty collection when no conventions are registered.
    /// </summary>
    [Fact]
    public void GetConventions_WhenNoConventions_ShouldReturnEmpty()
    {
        // Arrange
        var scope = new LanguageScope("test", null);

        // Act
        var conventions = scope.GetConventions();

        // Assert
        Assert.Empty(conventions);
    }

    /// <summary>
    /// Test convention class for testing.
    /// </summary>
    private sealed class TestConvention
    {
    }

    /// <summary>
    /// Test convention class for testing.
    /// </summary>
    private sealed class TestConvention2
    {
    }
}
