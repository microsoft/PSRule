using Newtonsoft.Json;
using PSRule.Configuration;
using PSRule.Rules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PSRule.Pipeline
{
    /// <summary>
    /// The default pipeline stream to handle objects passed from the PowerShell pipeline.
    /// </summary>
    internal sealed class PowerShellPipelineStream : IPipelineStream
    {
        private readonly Action<object, bool> _OutputVisitor;
        private readonly bool _ReturnBoolean;
        private readonly ResultFormat _ResultFormat;
        private readonly OutputFormat _OutputFormat;
        private readonly List<InvokeResult> _Results;

        private readonly string[] _InputPath;

        public PowerShellPipelineStream(PSRuleOption option, Action<object, bool> output, bool returnBoolean, string[] inputPath)
        {
            _OutputVisitor = output;
            _ReturnBoolean = returnBoolean;
            _ResultFormat = option.Output.As.Value;
            _OutputFormat = option.Output.Format.Value;

            if (_OutputFormat != OutputFormat.None)
            {
                _Results = new List<InvokeResult>();
            }

            _InputPath = inputPath;
        }

        public IStreamManager Manager { get; set; }

        public void Begin()
        {
            if (_InputPath != null)
            {
                // Read each file

                foreach (var p in _InputPath)
                {
                    if (p.StartsWith("http://") || p.StartsWith("https://"))
                    {
                        Manager.Process(targetObject: PSObject.AsPSObject(new Uri(p)));
                    }
                    else
                    {
                        Manager.Process(targetObject: PSObject.AsPSObject(new FileInfo(p)));
                    }
                }
            }
        }

        public void Process(PSObject targetObject)
        {
            Manager.Process(targetObject);
        }

        public void End(IEnumerable<RuleSummaryRecord> summary)
        {
            if (_Results != null && _ResultFormat == ResultFormat.Detail)
            {
                if (_OutputFormat == OutputFormat.Json)
                {
                    WriteObjectJson(o: GetRecords());
                }
                else if (_OutputFormat == OutputFormat.Yaml)
                {
                    WriteObjectYaml(o: GetRecords());
                }
                else if (_OutputFormat == OutputFormat.NUnit3)
                {
                    WriteObjectNUnit3(_Results);
                    _Results.Clear();
                }
                else if (_OutputFormat == OutputFormat.Csv)
                {
                    WriteObjectCSV(_Results);
                    _Results.Clear();
                }
            }

            if (_ResultFormat == ResultFormat.Summary)
            {
                var results = summary.ToArray();

                if (_OutputFormat == OutputFormat.Json)
                {
                    WriteObjectJson(results);
                }
                else if (_OutputFormat == OutputFormat.Yaml)
                {
                    WriteObjectYaml(results);
                }
                else
                {
                    WriteObject(results, expandCollection: true);
                }
            }
        }

        public void Output(InvokeResult result)
        {
            if (_ReturnBoolean)
            {
                WriteObject(result.IsSuccess(), false);
            }
            else
            {
                if (_OutputFormat == OutputFormat.None)
                {
                    WriteObject(result.AsRecord(), true);
                }
                else
                {
                    _Results.Add(result);
                }
            }
        }

        private void WriteObject(object o, bool expandCollection)
        {
            _OutputVisitor(o, expandCollection);
        }

        private void WriteObjectJson(IEnumerable<RuleRecord> o)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            var json = JsonConvert.SerializeObject(o, settings: settings);

            _OutputVisitor(json, false);
        }

        private void WriteObjectJson(IEnumerable<RuleSummaryRecord> o)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            var json = JsonConvert.SerializeObject(o, settings: settings);

            _OutputVisitor(json, false);
        }

        private void WriteObjectYaml(IEnumerable<RuleRecord> o)
        {
            var s = new SerializerBuilder()
                .WithTypeInspector(f => new FieldYamlTypeInspector())
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            var yaml = s.Serialize(o);

            _OutputVisitor(yaml, false);
        }

        private void WriteObjectYaml(IEnumerable<RuleSummaryRecord> o)
        {
            var s = new SerializerBuilder()
                .WithTypeInspector(f => new FieldYamlTypeInspector())
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            var yaml = s.Serialize(o);

            _OutputVisitor(yaml, false);
        }

        private void WriteObjectNUnit3(IEnumerable<InvokeResult> o)
        {
            var s = new NUnit3Serializer();
            var xml = s.Serialize(o);

            _OutputVisitor(xml, false);
        }

        private void WriteObjectCSV(IEnumerable<InvokeResult> o)
        {
            var s = new CSVSerializer();
            var csv = s.Serialize(o);

            _OutputVisitor(csv, false);
        }

        private RuleRecord[] GetRecords()
        {
            var results = _Results.SelectMany(r => r.AsRecord()).ToArray();
            _Results.Clear();

            return results;
        }
    }
}
