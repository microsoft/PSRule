using Newtonsoft.Json;
using PSRule.Configuration;
using PSRule.Rules;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Management.Automation;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.TypeResolvers;
using System.IO;
using System.Net.Http;
using System.Net;

namespace PSRule.Pipeline
{
    /// <summary>
    /// The default pipeline stream to handle objects passed from the PowerShell pipeline.
    /// </summary>
    internal sealed class PowerShellPipelineStream : IPipelineStream
    {
        private readonly Action<object, bool> _OutputVisitor;
        private readonly bool _ReturnBoolean;
        private readonly OutputFormat _OutputFormat;
        private readonly List<InvokeResult> _Results;
        private readonly string[] _InputPath;

        public PowerShellPipelineStream(PSRuleOption option, Action<object, bool> output, bool returnBoolean, string[] inputPath)
        {
            _OutputVisitor = output;
            _ReturnBoolean = returnBoolean;
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
            var webClient = new WebClient();

            if (_InputPath != null)
            {
                // Read each file

                foreach (var p in _InputPath)
                {
                    if (p.StartsWith("http://") || p.StartsWith("https://"))
                    {
                        Manager.Process(targetObject: PSObject.AsPSObject(webClient.DownloadString(p)));
                    }
                    else
                    {
                        Manager.Process(targetObject: PSObject.AsPSObject(File.ReadAllText(p)));
                    }
                }
            }
        }

        public void Process(PSObject targetObject)
        {
            Manager.Process(targetObject);
        }

        public void End()
        {
            if (_Results != null)
            {
                var results = _Results.SelectMany(r => r.AsRecord()).ToArray();
                _Results.Clear();

                if (_OutputFormat == OutputFormat.Json)
                {
                    WriteObjectJson(results);
                }
                else if (_OutputFormat == OutputFormat.Yaml)
                {
                    WriteObjectYaml(results);
                }
            }
        }

        public void Output(InvokeResult result)
        {
            if (_ReturnBoolean)
            {
                WriteObject(result.AsBoolean(), false);
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
            var settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
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
    }
}
