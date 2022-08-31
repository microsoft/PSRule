// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using PSRule.Configuration;

namespace PSRule
{
    internal static partial class Engine
    {
        internal static string GetLocalPath()
        {
            return PSRuleOption.GetRootedBasePath(Path.GetDirectoryName(AppContext.BaseDirectory));
        }

        internal static string GetVersion()
        {
            return _Version;
        }
    }
}
