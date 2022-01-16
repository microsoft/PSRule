// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Definitions.SuppressionGroups;
using PSRule.Pipeline;
using PSRule.Runtime;

namespace PSRule.Rules
{
    [DebuggerDisplay("{_Index.Count}")]
    internal sealed class SuppressionFilter
    {
        private readonly HashSet<SuppressionKey> _Index;
        private readonly bool _IsEmpty;

        private readonly ResourceIndex _ResourceIndex;

        private readonly Dictionary<string, List<SuppressionGroupVisitor>> _RuleSuppressionGroupIndex;

        public SuppressionFilter(RunspaceContext context, SuppressionOption option, ResourceIndex resourceIndex)
        {
            if (option == null || option.Count == 0 || resourceIndex.IsEmpty())
            {
                _IsEmpty = true;
            }
            else
            {
                _ResourceIndex = resourceIndex;
                _Index = Index(context, option);
                _IsEmpty = _Index.Count == 0;
            }
        }

        public SuppressionFilter(IEnumerable<SuppressionGroupVisitor> suppressionGroups, ResourceIndex resourceIndex)
        {
            _ResourceIndex = resourceIndex;
            _RuleSuppressionGroupIndex = new Dictionary<string, List<SuppressionGroupVisitor>>();
            IndexSuppressionGroups(suppressionGroups);
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

        private HashSet<SuppressionKey> Index(RunspaceContext context, SuppressionOption option)
        {
            var index = new HashSet<SuppressionKey>();

            // Read suppress rules into index combined key (RuleName + TargetName)
            foreach (var rule in option)
            {
                // Only add suppresion entries for rules that are loaded
                if (!_ResourceIndex.TryFind(rule.Key, out var blockId, out var kind))
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

        /// <summary>
        /// Attempts to fetch suppression group from rule suppression group index
        /// </summary>
        /// <param name="ruleId">The key rule id which indexes suppression groups</param>
        /// <param name="targetObject">Th target object we are invoking</param>
        /// <param name="suppressionGroupId">The Id of the matched suppression group</param>
        /// <returns>Boolean indicating if suppression group has been found</returns>
        public bool TrySuppressionGroup(string ruleId, TargetObject targetObject, out string suppressionGroupId)
        {
            suppressionGroupId = string.Empty;
            if (_RuleSuppressionGroupIndex.TryGetValue(ruleId, out var suppressionGroupVisitors))
            {
                var found = suppressionGroupVisitors.FirstOrDefault(visitor => visitor.Match(targetObject.Value));

                if (found != null)
                {
                    suppressionGroupId = found.Id;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Index suppression groups by rule
        /// </summary>
        /// <param name="suppressionGroups">The suppression group collection</param>
        private void IndexSuppressionGroups(IEnumerable<SuppressionGroupVisitor> suppressionGroups)
        {
            foreach (var group in suppressionGroups)
            {
                foreach (var rule in group.Rule)
                {
                    // Only add suppresion entries for rules that are loaded
                    if (!_ResourceIndex.TryFind(rule, out var blockId, out _))
                        continue;

                    var ruleId = blockId.Value;

                    if (!_RuleSuppressionGroupIndex.ContainsKey(rule))
                        _RuleSuppressionGroupIndex.Add(ruleId, new List<SuppressionGroupVisitor>());

                    _RuleSuppressionGroupIndex[ruleId].Add(group);
                }
            }
        }
    }
}
