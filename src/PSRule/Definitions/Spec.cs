// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions.Selectors;

namespace PSRule.Definitions
{
    public abstract class Spec
    {
        private const string FullNameSeparator = "/";

        protected Spec() { }

        public static string GetFullName(string apiVersion, string name)
        {
            if (string.IsNullOrEmpty(apiVersion))
                apiVersion = Specs.V1;

            return string.Concat(apiVersion, FullNameSeparator, name);
        }
    }

    internal static class Specs
    {
        internal const string V1 = "github.com/microsoft/PSRule/v1";

        internal const string Baseline = "Baseline";
        internal const string ModuleConfig = "ModuleConfig";
        internal const string Selector = "Selector";

        public readonly static ISpecDescriptor[] BuiltinTypes = new ISpecDescriptor[]
        {
            new SpecDescriptor<Baseline, BaselineSpec>(V1, Baseline),
            new SpecDescriptor<ModuleConfig, ModuleConfigSpec>(V1, ModuleConfig),
            new SpecDescriptor<SelectorV1, SelectorV1Spec>(V1, Selector),
        };
    }
}
