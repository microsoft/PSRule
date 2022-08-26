// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions.Baselines;
using PSRule.Definitions.ModuleConfigs;
using PSRule.Definitions.Rules;
using PSRule.Definitions.Selectors;
using PSRule.Definitions.SuppressionGroups;

namespace PSRule.Definitions
{
    /// <summary>
    /// The base class for a resource specification.
    /// </summary>
    public abstract class Spec
    {
        private const string FullNameSeparator = "/";

        /// <summary>
        /// Create an instance of the resource specication.
        /// </summary>
        protected Spec() { }

        /// <summary>
        /// Get a fully qualified name for the resource type.
        /// </summary>
        /// <param name="apiVersion">The specific API version of the resource.</param>
        /// <param name="name">The type name of the resource.</param>
        /// <returns>A fully qualified type name string.</returns>
        public static string GetFullName(string apiVersion, string name)
        {
            return string.Concat(apiVersion, FullNameSeparator, name);
        }
    }

    internal static class Specs
    {
        /// <summary>
        /// The API version for V1 resources.
        /// </summary>
        internal const string V1 = "github.com/microsoft/PSRule/v1";

        // Resource names for different types of resources.
        internal const string Rule = "Rule";
        internal const string Baseline = "Baseline";
        internal const string ModuleConfig = "ModuleConfig";
        internal const string Selector = "Selector";
        internal const string SuppressionGroup = "SuppressionGroup";

        /// <summary>
        /// The built-in resource types.
        /// </summary>
        public readonly static ISpecDescriptor[] BuiltinTypes = new ISpecDescriptor[]
        {
            new SpecDescriptor<RuleV1, RuleV1Spec>(V1, Rule),
            new SpecDescriptor<Baseline, BaselineSpec>(V1, Baseline),
            new SpecDescriptor<ModuleConfigV1, ModuleConfigV1Spec>(V1, ModuleConfig),
            new SpecDescriptor<SelectorV1, SelectorV1Spec>(V1, Selector),
            new SpecDescriptor<SuppressionGroupV1, SuppressionGroupV1Spec>(V1, SuppressionGroup)
        };
    }
}
