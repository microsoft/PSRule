// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PSRule.Pipeline.Output
{
    internal sealed class YamlOutputWriter : SerializationOutputWriter<object>
    {
        internal YamlOutputWriter(PipelineWriter inner, PSRuleOption option)
            : base(inner, option) { }

        protected override string Serialize(object[] o)
        {
            return ToYaml(o);
        }

        internal static string ToYaml(object[] o)
        {
            var s = new SerializerBuilder()
                .DisableAliases()
                .WithTypeInspector(f => new FieldYamlTypeInspector())
                .WithTypeInspector(inspector => new SortedPropertyYamlTypeInspector(inspector))
                .WithTypeConverter(new HashtableYamlTypeConverter())
                .WithTypeConverter(new PSObjectYamlTypeConverter())
                .WithTypeConverter(new FieldMapYamlTypeConverter())
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults)
                .Build();

            return s.Serialize(o);
        }
    }
}
