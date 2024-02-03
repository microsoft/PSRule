// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using PSRule.Configuration;

namespace PSRule;

internal static class OutputOptionExtensions
{
    /// <summary>
    /// Get the character encoding for the specified output encoding.
    /// </summary>
    public static Encoding GetEncoding(this OutputOption option)
    {
        var defaultEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        return option == null || !option.Encoding.HasValue
            ? defaultEncoding
            : option.Encoding switch
            {
                OutputEncoding.UTF8 => Encoding.UTF8,
                OutputEncoding.UTF7 => Encoding.UTF7,
                OutputEncoding.Unicode => Encoding.Unicode,
                OutputEncoding.UTF32 => Encoding.UTF32,
                OutputEncoding.ASCII => Encoding.ASCII,
                _ => defaultEncoding,
            };
    }
}
