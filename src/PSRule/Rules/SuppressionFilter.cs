// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Runtime;

namespace PSRule.Rules
{
    [DebuggerDisplay("{_Index.Count}")]
    internal sealed class SuppressionFilter
    {
        private readonly HashSet<SuppressionKey> _Index;
        private readonly bool _IsEmpty;

        public SuppressionFilter(RunspaceContext context, SuppressionOption option, IEnumerable<IResource> rules)
        {
            if (option == null || option.Count == 0 || rules == null)
            {
                _IsEmpty = true;
            }
            else
            {
                _Index = Index(context, option, rules);
                _IsEmpty = _Index.Count == 0;
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
                if (string.IsNullOrEmpty(ruleName))
                    throw new ArgumentNullException(nameof(ruleName));

                if (string.IsNullOrEmpty(targetName))
                    throw new ArgumentNullException(nameof(targetName));

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
                    var rol5 = ((uint)h1 << 5) | ((uint)h1 >> 27);
                    return ((int)rol5 + h1) ^ h2;
                }
            }
        }

        public bool Match(ResourceId id, string targetName)
        {
            return !_IsEmpty &&
                !string.IsNullOrEmpty(targetName) &&
                _Index.Contains(new SuppressionKey(id.Value, targetName));
        }

        private static HashSet<SuppressionKey> Index(RunspaceContext context, SuppressionOption option, IEnumerable<IResource> rules)
        {
            var resolver = new ResourceIndex(rules);
            var index = new HashSet<SuppressionKey>();

            // Read suppress rules into index combined key (RuleName + TargetName)
            foreach (var rule in option)
            {
                // Only add suppresion entries for rules that are loaded
                if (!resolver.TryFind(rule.Key, out var blockId, out var kind))
                    continue;

                if (kind == ResourceIdKind.Alias)
                    context.WarnAliasSuppression(blockId.Value, rule.Key);

                foreach (var targetName in rule.Value.TargetName)
                {
                    var key = new SuppressionKey(blockId.Value, targetName);
                    if (!index.Contains(key))
                        index.Add(key);
                }
            }
            return index;
        }
    }
}