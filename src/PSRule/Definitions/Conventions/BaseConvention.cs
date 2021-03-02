// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Resources;
using PSRule.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;

namespace PSRule.Definitions.Conventions
{
    internal sealed class ConventionFilter : IResourceFilter
    {
        private readonly HashSet<string> _Include;
        private readonly WildcardPattern _WildcardMatch;

        public ConventionFilter(string[] include)
        {
            _Include = include == null || include.Length == 0 ? null : new HashSet<string>(include, StringComparer.OrdinalIgnoreCase);
            _WildcardMatch = null;
            if (include != null && include.Length > 0 && WildcardPattern.ContainsWildcardCharacters(include[0]))
            {
                if (include.Length > 1)
                    throw new NotSupportedException(PSRuleResources.MatchSingleName);

                _WildcardMatch = new WildcardPattern(include[0]);
            }
        }

        public bool Match(string name, TagSet tag)
        {
            if (_Include == null)
                return false;

            return _Include.Contains(name) || MatchWildcard(name);
        }

        private bool MatchWildcard(string name)
        {
            if (_WildcardMatch == null)
                return false;

            return _WildcardMatch.IsMatch(name);
        }
    }

    internal abstract class BaseConvention : IConvention
    {
        protected BaseConvention(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public virtual void Begin(RunspaceContext context, IEnumerable input)
        {

        }

        public virtual void Process(RunspaceContext context, IEnumerable input)
        {

        }

        public virtual void End(RunspaceContext context, IEnumerable input)
        {

        }
    }
}
