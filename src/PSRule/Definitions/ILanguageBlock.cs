// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using PSRule.Pipeline;

namespace PSRule.Definitions
{
    public interface ILanguageBlock
    {
        ResourceId Id { get; }

        [Obsolete("Use Source property instead.")]
        string SourcePath { get; }

        [Obsolete("Use Source property instead.")]
        string Module { get; }

        SourceFile Source { get; }
    }
}
