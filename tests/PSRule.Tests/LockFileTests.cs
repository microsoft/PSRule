// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using PSRule.Pipeline.Dependencies;

namespace PSRule;

public sealed class LockFileTests
{
    [Fact]
    public void ReadFile()
    {
        var lockFile = LockFile.Read(GetSourcePath("test.lock.json"));
        Assert.True(lockFile.Modules.TryGetValue("PSRule.Rules.MSFT.OSS", out var item));
        Assert.Equal("1.1.0", item.Version.ToString());

        Assert.True(lockFile.Modules.TryGetValue("psrule.rules.msft.oss", out item));
        Assert.Equal("1.1.0", item.Version.ToString());
    }

    #region Helper methods

    private static string GetSourcePath(string fileName)
    {
        return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
    }

    #endregion Helper methods
}
