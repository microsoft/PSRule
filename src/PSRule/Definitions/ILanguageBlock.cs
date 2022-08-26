// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
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
        /// Obsolete. The source file path.
        /// Replaced by <see cref="Source"/>.
        /// </summary>
        [Obsolete("Use Source property instead.")]
        string SourcePath { get; }

        /// <summary>
        /// Obsolete. The source module.
        /// Replaced by <see cref="Source"/>.
        /// </summary>
        [Obsolete("Use Source property instead.")]
        string Module { get; }

        /// <summary>
        /// The source location for the block.
        /// </summary>
        SourceFile Source { get; }
    }
}
