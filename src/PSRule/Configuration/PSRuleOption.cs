using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PSRule.Configuration
{
    /// <summary>
    /// A delgate to allow callback to PowerShell to get current working path.
    /// </summary>
    public delegate string PathDelegate();

    /// <summary>
    /// A structure that stores PSRule configuration options.
    /// </summary>
    public sealed class PSRuleOption
    {
        private static readonly PSRuleOption Default = new PSRuleOption
        {
            Binding = BindingOption.Default,
            Execution = ExecutionOption.Default,
            Input = InputOption.Default,
            Logging = LoggingOption.Default,
            Output = OutputOption.Default
        };

        public PSRuleOption()
        {
            // Set defaults
            Baseline = new BaselineOption();
            Binding = new BindingOption();
            Input = new InputOption();
            Logging = new LoggingOption();
            Output = new OutputOption();
            Suppression = new SuppressionOption();
            Execution = new ExecutionOption();
            Pipeline = new PipelineHook();
        }

        public PSRuleOption(PSRuleOption option)
        {
            // Set from existing option instance
            Baseline = new BaselineOption(option.Baseline);
            Binding = new BindingOption(option.Binding);
            Input = new InputOption(option.Input);
            Logging = new LoggingOption(option.Logging);
            Output = new OutputOption(option.Output);
            Suppression = new SuppressionOption(option.Suppression);
            Execution = new ExecutionOption(option.Execution);
            Pipeline = new PipelineHook(option.Pipeline);
        }

        /// <summary>
        /// A callback that is overridden by PowerShell so that the current working path can be retrieved.
        /// </summary>
        public static PathDelegate GetWorkingPath = () => Directory.GetCurrentDirectory();

        /// <summary>
        /// Options that specify the rules to evaluate or exclude and their configuration.
        /// </summary>
        public BaselineOption Baseline { get; set; }

        /// <summary>
        /// Options tht affect property binding of TargetName.
        /// </summary>
        public BindingOption Binding { get; set; }

        /// <summary>
        /// Options that affect script execution.
        /// </summary>
        public ExecutionOption Execution { get; set; }

        /// <summary>
        /// Options that affect how input types are processed.
        /// </summary>
        public InputOption Input { get; set; }

        /// <summary>
        /// Options for logging outcomes to a informational streams.
        /// </summary>
        public LoggingOption Logging { get; set; }

        /// <summary>
        /// Options that affect how output is generated.
        /// </summary>
        public OutputOption Output { get; set; }

        /// <summary>
        /// A set of suppression rules.
        /// </summary>
        public SuppressionOption Suppression { get; set; }

        [YamlIgnore()]
        public PipelineHook Pipeline { get; set; }

        public string ToYaml()
        {
            var s = new SerializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            return s.Serialize(this);
        }

        public PSRuleOption Clone()
        {
            return new PSRuleOption(this);
        }

        public static PSRuleOption FromFile(string path, bool silentlyContinue = false)
        {
            // Ensure that a full path instead of a path relative to PowerShell is used for .NET methods
            var rootedPath = GetRootedPath(path);

            // Fallback to defaults even if file does not exist when silentlyContinue is true
            if (!File.Exists(rootedPath))
            {
                if (!silentlyContinue)
                {
                    throw new FileNotFoundException("", rootedPath);
                }
                else
                {
                    // Use the default options
                    return Default.Clone();
                }
            }

            return FromYaml(File.ReadAllText(rootedPath));
        }

        public static PSRuleOption FromYaml(string yaml)
        {
            var d = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .WithTypeConverter(new SuppressionRuleYamlTypeConverter())
                .Build();

            return d.Deserialize<PSRuleOption>(yaml) ?? new PSRuleOption();
        }

        /// <summary>
        /// Convert from hashtable to options by processing key values. This enables -Option @{ } from PowerShell.
        /// </summary>
        /// <param name="hashtable"></param>
        public static implicit operator PSRuleOption(Hashtable hashtable)
        {
            var option = new PSRuleOption();

            // Build index to allow mapping
            var index = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            foreach (DictionaryEntry entry in hashtable)
            {
                index.Add(entry.Key.ToString(), entry.Value);
            }

            // Start loading matching values

            object value;

            if (index.TryGetValue("baseline.rulename", out value))
            {
                if (value.GetType().IsArray)
                {
                    option.Baseline.RuleName = ((object[])value).OfType<string>().ToArray();
                }
                else
                {
                    option.Baseline.RuleName = new string[] { value.ToString() };
                }
            }

            if (index.TryGetValue("baseline.exclude", out value))
            {
                if (value.GetType().IsArray)
                {
                    option.Baseline.Exclude = ((object[])value).OfType<string>().ToArray();
                }
                else
                {
                    option.Baseline.Exclude = new string[] { value.ToString() };
                }
            }

            if (index.TryGetValue("binding.ignorecase", out value))
            {
                option.Binding.IgnoreCase = bool.Parse(value.ToString());
            }

            if (index.TryGetValue("binding.targetname", out value))
            {
                if (value.GetType().IsArray)
                {
                    option.Binding.TargetName = ((object[])value).OfType<string>().ToArray();
                }
                else
                {
                    option.Binding.TargetName = new string[] { value.ToString() };
                }
            }

            if (index.TryGetValue("binding.targettype", out value))
            {
                if (value.GetType().IsArray)
                {
                    option.Binding.TargetType = ((object[])value).OfType<string>().ToArray();
                }
                else
                {
                    option.Binding.TargetType = new string[] { value.ToString() };
                }
            }

            if (index.TryGetValue("execution.languagemode", out value))
            {
                option.Execution.LanguageMode = (LanguageMode)Enum.Parse(typeof(LanguageMode), (string)value);
            }

            if (index.TryGetValue("execution.inconclusivewarning", out value))
            {
                option.Execution.InconclusiveWarning = bool.Parse(value.ToString());
            }

            if (index.TryGetValue("execution.notprocessedwarning", out value))
            {
                option.Execution.NotProcessedWarning = bool.Parse(value.ToString());
            }

            if (index.TryGetValue("input.format", out value))
            {
                option.Input.Format = (InputFormat)Enum.Parse(typeof(InputFormat), (string)value);
            }

            if (index.TryGetValue("input.objectpath", out value))
            {
                option.Input.ObjectPath = (string)value;
            }

            if (index.TryGetValue("logging.rulefail", out value))
            {
                option.Logging.RuleFail = (OutcomeLogStream)Enum.Parse(typeof(OutcomeLogStream), (string)value);
            }

            if (index.TryGetValue("logging.rulepass", out value))
            {
                option.Logging.RulePass = (OutcomeLogStream)Enum.Parse(typeof(OutcomeLogStream), (string)value);
            }

            if (index.TryGetValue("output.format", out value))
            {
                option.Output.Format = (OutputFormat)Enum.Parse(typeof(OutputFormat), (string)value);
            }

            return option;
        }

        /// <summary>
        /// Convert from string to options by loading the yaml file from disk. This enables -Option '.\psrule.yml' from PowerShell.
        /// </summary>
        /// <param name="path"></param>
        public static implicit operator PSRuleOption(string path)
        {
            var option = FromFile(path);

            return option;
        }

        /// <summary>
        /// Get a full path instead of a relative path that may be passed from PowerShell.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string GetRootedPath(string path)
        {
            return Path.IsPathRooted(path) ? path : Path.Combine(GetWorkingPath(), path);
        }
    }
}
