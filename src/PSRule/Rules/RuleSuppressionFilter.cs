// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace PSRule.Rules
{
    [DebuggerDisplay("{_Index.Count}")]
    internal sealed class RuleSuppressionFilter
    {
        private readonly HashSet<SuppressionKey> _Index;
        private readonly bool _IsEmpty;

        public RuleSuppressionFilter(SuppressionOption option)
        {
            if (option == null || option.Count == 0)
            {
                _IsEmpty = true;
            }
            else
            {
                _Index = new HashSet<SuppressionKey>();
                Index(option);
            }
        }

        [DebuggerDisplay("{HashCode}, RuleName = {RuleName}, TargetName = {TargetName}")]
        private sealed class SuppressionKey
        {
            public readonly string RuleName;
            public readonly string TargetName;
            private readonly int HashCode;

            public SuppressionKey(string ruleName, string targetName)
            {
                if (string.IsNullOrEmpty(ruleName) || string.IsNullOrEmpty(targetName))
                    throw new ArgumentNullException();

                RuleName = ruleName;
                TargetName = targetName;
                HashCode = CombineHashCode();
            }

            public override int GetHashCode()
            {
                return HashCode;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is SuppressionKey))
                    return false;

                var k2 = obj as SuppressionKey;
                return HashCode == k2.HashCode &&
                    StringComparer.OrdinalIgnoreCase.Equals(TargetName, k2.TargetName) &&
                    StringComparer.OrdinalIgnoreCase.Equals(RuleName, k2.RuleName);
            }

            private int CombineHashCode()
            {
                var h1 = RuleName.ToUpper(Thread.CurrentThread.CurrentCulture).GetHashCode();
                var h2 = TargetName.ToUpper(Thread.CurrentThread.CurrentCulture).GetHashCode();
                unchecked
                {
                    // Get combined hash code for key
                    uint rol5 = ((uint)h1 << 5) | ((uint)h1 >> 27);
                    return ((int)rol5 + h1) ^ h2;
                }
            }
        }

        public bool Match(string ruleName, string targetName)
        {
            if (_IsEmpty || string.IsNullOrEmpty(ruleName) || string.IsNullOrEmpty(targetName))
                return false;

            return _Index.Contains(new SuppressionKey(ruleName, targetName));
        }

        private void Index(SuppressionOption option)
        {
            // Read suppress rules into index combined key (RuleName + TargetName)
            foreach (var rule in option)
            {
                foreach (var targetName in rule.Value.TargetName)
                {
                    var key = new SuppressionKey(rule.Key, targetName);
                    if (!_Index.Contains(key))
                        _Index.Add(key);
                }
            }
        }
    }
}
