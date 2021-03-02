// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Runtime;
using System.Collections;

namespace PSRule.Definitions
{
    internal interface IConvention
    {
        string Name { get; }

        void Begin(RunspaceContext context, IEnumerable input);

        void Process(RunspaceContext context, IEnumerable input);

        void End(RunspaceContext context, IEnumerable input);
    }
}
