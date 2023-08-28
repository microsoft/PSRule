// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;

namespace PSRule.Definitions
{
    /// <summary>
    /// A language block.
    /// </summary>
    public interface ILanguageBlock
    {
        /// <summary>
        /// The unique identifier for the block.
        /// </summary>
        ResourceId Id { get; }

        /// <summary>
        /// The source location for the block.
        /// </summary>
        SourceFile Source { get; }
    }
}
