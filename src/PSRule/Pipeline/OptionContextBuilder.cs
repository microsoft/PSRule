// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using PSRule.Configuration;

namespace PSRule.Pipeline
{
    /// <summary>
    /// A helper to create an <see cref="OptionContext"/>.
    /// </summary>
    internal sealed class OptionContextBuilder
    {
        private readonly OptionContext _OptionContext;

        /// <summary>
        /// Create a builder with parameter and workspace options set.
        /// </summary>
        /// <param name="option">The workspace options.</param>
        /// <param name="include">A list of rule identifiers to include set by parameters. If not set all rules that meet filters are included.</param>
        /// <param name="tag">A tag filter to determine which rules are included by parameters.</param>
        /// <param name="convention">A list of conventions to include by parameters.</param>
        internal OptionContextBuilder(PSRuleOption option, string[] include = null, Hashtable tag = null, string[] convention = null)
        {
            _OptionContext = new OptionContext();
            Parameter(include, tag, convention);
            Workspace(option);
        }

        /// <summary>
        /// Build an <see cref="OptionContext"/>.
        /// </summary>
        /// <returns></returns>
        internal OptionContext Build()
        {
            return _OptionContext;
        }

        private void Parameter(string[] include, Hashtable tag, string[] convention)
        {
            _OptionContext.Add(new OptionContext.BaselineScope(
                type: OptionContext.ScopeType.Parameter,
                include: include,
                tag: tag,
                convention: convention));
        }

        private void Workspace(PSRuleOption option)
        {
            _OptionContext.Add(new OptionContext.BaselineScope(
                type: OptionContext.ScopeType.Workspace,
                baselineId: null,
                moduleName: null,
                option: option,
                obsolete: false));

            _OptionContext.Add(new OptionContext.ConfigScope(
                type: OptionContext.ScopeType.Workspace,
                moduleName: null,
                option: option));
        }
    }
}
