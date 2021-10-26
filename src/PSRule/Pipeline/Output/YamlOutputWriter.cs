// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using PSRule.Configuration;
using PSRule.Definitions.Baselines;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
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
            if (o[0] is IEnumerable<Baseline> baselines)
            {
                return ToBaselineYaml(baselines);
            }

            return ToYaml(o);
        }

        internal static string ToYaml(object[] o)
        {
            var s = new SerializerBuilder()
                .DisableAliases()
                .WithTypeInspector(f => new FieldYamlTypeInspector())
                .WithTypeInspector(inspector => new SortedPropertyYamlTypeInspector(inspector))
                .WithTypeConverter(new PSObjectYamlTypeConverter())
                .WithTypeConverter(new FieldMapYamlTypeConverter())
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults)
                .Build();

            return s.Serialize(o);
        }

        internal static string ToBaselineYaml(IEnumerable<Baseline> baselines)
        {
            using StringWriter output = new StringWriter();
            IEmitter emitter = new Emitter(output, bestIndent: 2, bestWidth: int.MaxValue, isCanonical: false);

            emitter.Emit(new StreamStart());

            foreach (Baseline baseline in baselines)
            {
                emitter.Emit(new DocumentStart());
                BaselineYamlMapping.MapBaseline(emitter, baseline);
                emitter.Emit(new DocumentEnd(true));
            }

            emitter.Emit(new StreamEnd());

            return output.ToString();
        }
    }
}
