// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Rules
{
    internal interface IRuleExtent
    {
        string File { get; }

        int StartLineNumber { get; }
    }

    internal sealed class RuleExtent : IRuleExtent
    {
        internal RuleExtent(string file, int startLineNumber)
        {
            File = file;
            StartLineNumber = startLineNumber;
        }

        public string File { get; }

        public int StartLineNumber { get; }
    }
}
