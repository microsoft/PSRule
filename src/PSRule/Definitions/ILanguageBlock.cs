// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions
{
    public interface ILanguageBlock
    {
        ResourceId Id { get; }

        string SourcePath { get; }

        string Module { get; }
    }
}
