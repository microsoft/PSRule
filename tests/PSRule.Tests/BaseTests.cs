// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using PSRule.Pipeline.Emitters;

namespace PSRule;

/// <summary>
/// A base class for all tests.
/// </summary>
public abstract class BaseTests
{
    #region Helper methods

    protected static string GetSourcePath(string fileName)
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
    }

    internal static InternalFileInfo GetFileInfo(string fileName)
    {
        var file = new FileInfo(GetSourcePath(fileName));
        return new InternalFileInfo(file.FullName, file.Extension);
    }
    protected static string ReadFileAsString(string fileName)
    {
        var file = GetFileInfo(fileName);
        using var stream = file.GetFileStream();
        using var reader = stream.AsTextReader();
        return reader.ReadToEnd();
    }

    #endregion Helper methods
}
