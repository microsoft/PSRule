using PSRule.Rules;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PSRule.Pipeline
{
    internal sealed class YamlOutputWriter : PipelineWriter
    {
        private readonly List<object> _Result;

        public YamlOutputWriter(WriteOutput output) : base(output)
        {
            _Result = new List<object>();
        }

        public override void Write(object o, bool enumerate)
        {
            if (o is InvokeResult result)
            {
                _Result.AddRange(result.AsRecord());
                return;
            }
            if (o is IEnumerable<Rule> rule)
            {
                _Result.AddRange(rule);
                return;
            }
            _Result.Add(o);
        }

        public override void End()
        {
            WriteObjectYaml();
        }

        private void WriteObjectYaml()
        {
            var s = new SerializerBuilder()
                .WithTypeInspector(f => new FieldYamlTypeInspector())
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            var yaml = s.Serialize(_Result.ToArray());
            base.Write(yaml, false);
        }
    }
}
