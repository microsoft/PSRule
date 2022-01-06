// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Automation;
using PSRule.Pipeline;
using PSRule.Resources;
using PSRule.Runtime;

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

        ResourceKind IResourceFilter.Kind => ResourceKind.Convention;

        public bool Match(IResource resource)
        {
            return _Include != null && (_Include.Contains(resource.Name) || _Include.Contains(resource.Id) || MatchWildcard(resource.Name) || MatchWildcard(resource.Id));
        }

        private bool MatchWildcard(string name)
        {
            return _WildcardMatch != null && _WildcardMatch.IsMatch(name);
        }
    }

    [DebuggerDisplay("{Id}")]
    internal abstract class BaseConvention : IConvention
    {
        protected BaseConvention(SourceFile source, string name)
        {
            Source = source;
            Name = name;
            Id = ResourceHelper.GetIdString(Source.ModuleName, name);
        }

        public SourceFile Source { get; }

        public string Id { get; }

        public string Name { get; }

        public string SourcePath => Source.Path;

        public string Module => Source.ModuleName;

        public virtual void Initialize(RunspaceContext context, IEnumerable input)
        {

        }

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
