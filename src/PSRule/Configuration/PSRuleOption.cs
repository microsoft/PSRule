using PSRule.Resources;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PSRule.Configuration
{
    /// <summary>
    /// A delgate to allow callback to PowerShell to get current working path.
    /// </summary>
    internal delegate string PathDelegate();

    /// <summary>
    /// A structure that stores PSRule configuration options.
    /// </summary>
    public sealed class PSRuleOption : IEquatable<PSRuleOption>
    {
        private const string DEFAULT_FILENAME = "ps-rule.yaml";

        private static readonly PSRuleOption Default = new PSRuleOption
        {
            Binding = BindingOption.Default,
            Execution = ExecutionOption.Default,
            Input = InputOption.Default,
            Logging = LoggingOption.Default,
            Output = OutputOption.Default
        };

        /// <summary>
        /// A callback that is overridden by PowerShell so that the current working path can be retrieved.
        /// </summary>
        private static PathDelegate _GetWorkingPath = () => Directory.GetCurrentDirectory();

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

        /// <summary>
        /// Save the PSRuleOption to disk as YAML.
        /// </summary>
        /// <param name="path">The file or directory path to save the YAML file.</param>
        public void ToFile(string path)
        {
            // Get a rooted file path instead of directory or relative path
            var filePath = GetFilePath(path: path);
            File.WriteAllText(path: filePath, contents: ToYaml());
        }

        /// <summary>
        /// Load a YAML formatted PSRuleOption object from disk.
        /// </summary>
        /// <param name="path">The file or directory path to load options from.</param>
        /// <param name="silentlyContinue">When false, if the file does not exist, and exception will be raised.</param>
        /// <returns></returns>
        public static PSRuleOption FromFile(string path, bool silentlyContinue = false)
        {
            // Get a rooted file path instead of directory or relative path
            var filePath = GetFilePath(path: path);

            // Fallback to defaults even if file does not exist when silentlyContinue is true
            if (!File.Exists(filePath))
            {
                if (!silentlyContinue)
                {
                    throw new FileNotFoundException(PSRuleResources.OptionsNotFound, filePath);
                }
                else
                {
                    // Use the default options
                    return Default.Clone();
                }
            }

            return FromYaml(yaml: File.ReadAllText(filePath));
        }

        /// <summary>
        /// Load a YAML formatted PSRuleOption object from disk.
        /// </summary>
        /// <param name="path">The file for directory path to load options from.</param>
        /// <returns></returns>
        public static PSRuleOption FromFileOrDefault(string path)
        {
            // Get a rooted file path instead of directory or relative path
            var filePath = GetFilePath(path: path);

            // Fallback to defaults even if file does not exist when silentlyContinue is true
            if (!File.Exists(filePath))
            {
                return new PSRuleOption();
            }

            return FromYaml(yaml: File.ReadAllText(filePath));
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

        public static void UseExecutionContext(EngineIntrinsics executionContext)
        {
            if (executionContext == null)
            {
                _GetWorkingPath = () => Directory.GetCurrentDirectory();

                return;
            }

            _GetWorkingPath = () => executionContext.SessionState.Path.CurrentFileSystemLocation.Path;
        }

        public static string GetWorkingPath()
        {
            return _GetWorkingPath();
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

            if (index.TryGetValue("output.as", out value))
            {
                option.Output.As = (ResultFormat)Enum.Parse(typeof(ResultFormat), (string)value);
            }

            if (index.TryGetValue("output.encoding", out value))
            {
                option.Output.Encoding = (OutputEncoding)Enum.Parse(typeof(OutputEncoding), (string)value);
            }

            if (index.TryGetValue("output.format", out value))
            {
                option.Output.Format = (OutputFormat)Enum.Parse(typeof(OutputFormat), (string)value);
            }

            if (index.TryGetValue("output.path", out value))
            {
                option.Output.Path = (string)value;
            }

            return option;
        }

        /// <summary>
        /// Convert from string to options by loading the yaml file from disk. This enables -Option '.\psrule.yml' from PowerShell.
        /// </summary>
        /// <param name="path"></param>
        public static implicit operator PSRuleOption(string path)
        {
            var option = FromFile(path: path, silentlyContinue: false);

            return option;
        }

        public override bool Equals(object obj)
        {
            return obj != null &&
                obj is PSRuleOption &&
                Equals(obj as PSRuleOption);
        }
        public override int GetHashCode()
        {
            unchecked // Overflow is fine
            {
                int hash = 17;
                hash = hash * 23 + (Baseline != null ? Baseline.GetHashCode() : 0);
                hash = hash * 23 + (Binding != null ? Binding.GetHashCode() : 0);
                hash = hash * 23 + (Execution != null ? Execution.GetHashCode() : 0);
                hash = hash * 23 + (Input != null ? Input.GetHashCode() : 0);
                hash = hash * 23 + (Logging != null ? Logging.GetHashCode() : 0);
                hash = hash * 23 + (Output != null ? Output.GetHashCode() : 0);
                hash = hash * 23 + (Suppression != null ? Suppression.GetHashCode() : 0);
                hash = hash * 23 + (Pipeline != null ? Pipeline.GetHashCode() : 0);
                return hash;
            }
        }

        public bool Equals(PSRuleOption other)
        {
            return other != null &&
                Baseline == other.Baseline &&
                Binding == other.Binding &&
                Execution == other.Execution &&
                Input == other.Input &&
                Logging == other.Logging &&
                Output == other.Output &&
                Suppression == other.Suppression &&
                Pipeline == other.Pipeline;
        }

        /// <summary>
        /// Get a fully qualified file path.
        /// </summary>
        /// <param name="path">A file or directory path.</param>
        /// <returns></returns>
        public static string GetFilePath(string path)
        {
            var rootedPath = GetRootedPath(path);
            if (Path.HasExtension(rootedPath))
            {
                return rootedPath;
            }

            // Check if default files exist and 
            return UseFilePath(path: path, name: "ps-rule.yaml") ??
                UseFilePath(path: path, name: "ps-rule.yml") ??
                UseFilePath(path: path, name: "psrule.yaml") ??
                UseFilePath(path: path, name: "psrule.yml") ??
                Path.Combine(path, DEFAULT_FILENAME);
        }

        /// <summary>
        /// Get a full path instead of a relative path that may be passed from PowerShell.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static string GetRootedPath(string path)
        {
            return Path.IsPathRooted(path) ? path : Path.Combine(GetWorkingPath(), path);
        }

        /// <summary>
        /// Determine if the combined file path is exists.
        /// </summary>
        /// <param name="path">A directory path where a options file may be stored.</param>
        /// <param name="name">A file name of an options file.</param>
        /// <returns>Returns a file path if the file exists or null if the file does not exist.</returns>
        private static string UseFilePath(string path, string name)
        {
            var filePath = Path.Combine(path, name);
            return File.Exists(filePath) ? filePath : null;
        }
    }
}
