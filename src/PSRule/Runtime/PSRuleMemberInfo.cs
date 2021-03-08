// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime
{
    internal sealed class PSRuleTargetInfo
    {
        internal const string PropertyName = "_PSRule";

        public string PSPath { get; internal set; }

        public string PSParentPath { get; internal set; }

        public string PSChildName { get; internal set; }
    }
}
