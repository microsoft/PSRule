// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Pipeline.Dependencies;

/// <summary>
/// Tests for the lock file.
/// </summary>
public sealed class LockFileTests : BaseTests
{
    [Fact]
    public void Read_WhenValidLockFile_ShouldReturnInstance()
    {
        var lockFile = LockFile.Read(GetSourcePath("test.lock.json"));
        Assert.NotNull(lockFile);

        Assert.True(lockFile.Modules.TryGetValue("PSRule.Rules.MSFT.OSS", out var item));
        Assert.Equal("1.1.0", item.Version.ToString());
        Assert.Equal(IntegrityAlgorithm.SHA512, item.Integrity.Algorithm);
        Assert.Equal("4oEbkAT3VIQQlrDUOpB9qKkbNU5BMktvkDCriws4LgCMUiyUoYMcN0XovljAIW4FO0cmP7mP6A8Z7MPNGlgK7Q==", item.Integrity.Hash);
        Assert.Null(item.IncludePrerelease);

        // Test string casing.
        Assert.True(lockFile.Modules.TryGetValue("psrule.rules.msft.oss", out item));
        Assert.Equal("1.1.0", item.Version.ToString());
        Assert.Equal(IntegrityAlgorithm.SHA512, item.Integrity.Algorithm);
        Assert.Equal("4oEbkAT3VIQQlrDUOpB9qKkbNU5BMktvkDCriws4LgCMUiyUoYMcN0XovljAIW4FO0cmP7mP6A8Z7MPNGlgK7Q==", item.Integrity.Hash);
        Assert.Null(item.IncludePrerelease);

        // Test second module.
        Assert.True(lockFile.Modules.TryGetValue("PSRule.Rules.Azure", out item));
        Assert.Equal("1.39.3", item.Version.ToString());
        Assert.Equal(IntegrityAlgorithm.SHA512, item.Integrity.Algorithm);
        Assert.Equal("BS6NhS0xlt7+iLoBWchc72I3/LAi1bWum9jV48aKNfQ/02lzrCiSgUHu3Svc0sS3oICdSfO3zoxlcI24Oo3Zfw==", item.Integrity.Hash);
        Assert.True(item.IncludePrerelease);
    }
}
