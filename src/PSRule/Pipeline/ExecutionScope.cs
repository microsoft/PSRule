// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Pipeline
{
    internal enum ExecutionScope
    {
        None = 0,

        /// <summary>
        /// Execution is occuring at the script. This occurs during discovery.
        /// </summary>
        Script = 1,

        /// <summary>
        /// Execution is occuring in the main rule condition block.
        /// </summary>
        Condition = 2,

        /// <summary>
        /// Execution is occuring in a rule precondition.
        /// </summary>
        Precondition = 3,

        /// <summary>
        /// Execution is currently parsing YAML objects.
        /// </summary>
        Resource = 4
    }
}
