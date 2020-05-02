// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using PSRule.Definitions;
using PSRule.Resources;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Threading;
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
    public sealed class PSRuleOption : IEquatable<PSRuleOption>, IBaselineSpec
    {
        private const string DEFAULT_FILENAME = "ps-rule.yaml";

        private string SourcePath;

        public static readonly PSRuleOption Default = new PSRuleOption
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

        /// <summary>
        /// Sets the current culture to use when processing rules unless otherwise specified.
        /// </summary>
        private static CultureInfo _CurrentCulture = Thread.CurrentThread.CurrentCulture;

        public PSRuleOption()
        {
            // Set defaults
            Binding = new BindingOption();
            Configuration = new ConfigurationOption();
            Execution = new ExecutionOption();
            Input = new InputOption();
            Logging = new LoggingOption();
            Output = new OutputOption();
            Pipeline = new PipelineHook();
            Rule = new RuleOption();
            Suppression = new SuppressionOption();
        }

        private PSRuleOption(string sourcePath, PSRuleOption option)
        {
            SourcePath = sourcePath;

            // Set from existing option instance
            Binding = new BindingOption(option.Binding);
            Configuration = new ConfigurationOption(option.Configuration);
            Input = new InputOption(option.Input);
            Logging = new LoggingOption(option.Logging);
            Output = new OutputOption(option.Output);
            Suppression = new SuppressionOption(option.Suppression);
            Execution = new ExecutionOption(option.Execution);
            Pipeline = new PipelineHook(option.Pipeline);
            Rule = new RuleOption(option.Rule);
        }

        /// <summary>
        /// Options tht affect property binding of TargetName.
        /// </summary>
        public BindingOption Binding { get; set; }

        public ConfigurationOption Configuration { get; set; }

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

        [YamlIgnore]
        [JsonIgnore]
        public PipelineHook Pipeline { get; set; }

        public RuleOption Rule { get; set; }

        /// <summary>
        /// Return options as YAML.
        /// </summary>
        /// <remarks>
        /// Called from PowerShell.
        /// </remarks>
        public string ToYaml()
        {
            var yaml = GetYaml();
            if (string.IsNullOrEmpty(SourcePath))
            {
                return yaml;
            }

            return string.Concat(
                string.Format(PSRuleResources.OptionsSourceComment, SourcePath),
                Environment.NewLine,
                yaml
            );
        }

        public PSRuleOption Clone()
        {
            return new PSRuleOption(sourcePath: SourcePath, option: this);
        }

        /// <summary>
        /// Save the PSRuleOption to disk as YAML.
        /// </summary>
        /// <param name="path">The file or directory path to save the YAML file.</param>
        public void ToFile(string path)
        {
            // Get a rooted file path instead of directory or relative path
            var filePath = GetFilePath(path: path);
            File.WriteAllText(path: filePath, contents: GetYaml());
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
            return FromYaml(path: filePath, yaml: File.ReadAllText(filePath));
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
                return new PSRuleOption();

            return FromYaml(path: filePath, yaml: File.ReadAllText(filePath));
        }

        public static PSRuleOption FromYaml(string path, string yaml)
        {
            var d = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .WithTypeConverter(new FieldMapYamlTypeConverter())
                .WithTypeConverter(new SuppressionRuleYamlTypeConverter())
                .Build();
            var option = d.Deserialize<PSRuleOption>(yaml) ?? new PSRuleOption();
            option.SourcePath = path;
            return option;
        }

        /// <summary>
        /// Set working path from PowerShell host environment.
        /// </summary>
        /// <param name="executionContext">An $ExecutionContext object.</param>
        /// <remarks>
        /// Called from PowerShell.
        /// </remarks>
        public static void UseExecutionContext(EngineIntrinsics executionContext)
        {
            if (executionContext == null)
            {
                _GetWorkingPath = () => Directory.GetCurrentDirectory();
                return;
            }
            _GetWorkingPath = () => executionContext.SessionState.Path.CurrentFileSystemLocation.Path;
        }

        public static void UseCurrentCulture()
        {
            UseCurrentCulture(Thread.CurrentThread.CurrentCulture);
        }

        public static void UseCurrentCulture(string culture)
        {
            UseCurrentCulture(CultureInfo.CreateSpecificCulture(culture));
        }

        public static void UseCurrentCulture(CultureInfo culture)
        {
            _CurrentCulture = culture;
        }

        public static string GetWorkingPath()
        {
            return _GetWorkingPath();
        }

        public static CultureInfo GetCurrentCulture()
        {
            return _CurrentCulture;
        }

        /// <summary>
        /// Convert from hashtable to options by processing key values. This enables -Option @{ } from PowerShell.
        /// </summary>
        /// <param name="hashtable"></param>
        public static implicit operator PSRuleOption(Hashtable hashtable)
        {
            var option = new PSRuleOption();

            // Build index to allow mapping
            var index = BuildIndex(hashtable);

            // Start loading matching values

            if (index.TryPopValue("execution.languagemode", out object value))
            {
                option.Execution.LanguageMode = (LanguageMode)Enum.Parse(typeof(LanguageMode), (string)value);
            }
            if (index.TryPopValue("execution.inconclusivewarning", out value))
            {
                option.Execution.InconclusiveWarning = bool.Parse(value.ToString());
            }
            if (index.TryPopValue("execution.notprocessedwarning", out value))
            {
                option.Execution.NotProcessedWarning = bool.Parse(value.ToString());
            }
            if (index.TryPopValue("input.format", out value))
            {
                option.Input.Format = (InputFormat)Enum.Parse(typeof(InputFormat), (string)value);
            }
            if (index.TryPopValue("input.objectpath", out value))
            {
                option.Input.ObjectPath = (string)value;
            }
            if (index.TryPopValue("input.targettype", out value))
            {
                option.Input.TargetType = AsStringArray(value);
            }
            if (index.TryPopValue("logging.limitdebug", out value))
            {
                option.Logging.LimitDebug = AsStringArray(value);
            }
            if (index.TryPopValue("logging.limitverbose", out value))
            {
                option.Logging.LimitVerbose = AsStringArray(value);
            }
            if (index.TryPopValue("logging.rulefail", out value))
            {
                option.Logging.RuleFail = (OutcomeLogStream)Enum.Parse(typeof(OutcomeLogStream), (string)value);
            }
            if (index.TryPopValue("logging.rulepass", out value))
            {
                option.Logging.RulePass = (OutcomeLogStream)Enum.Parse(typeof(OutcomeLogStream), (string)value);
            }
            if (index.TryPopValue("output.as", out value))
            {
                option.Output.As = (ResultFormat)Enum.Parse(typeof(ResultFormat), (string)value);
            }
            if (index.TryPopValue("output.culture", out value))
            {
                option.Output.Culture = AsStringArray(value);
            }
            if (index.TryPopValue("output.encoding", out value))
            {
                option.Output.Encoding = (OutputEncoding)Enum.Parse(typeof(OutputEncoding), (string)value);
            }
            if (index.TryPopValue("output.format", out value))
            {
                option.Output.Format = (OutputFormat)Enum.Parse(typeof(OutputFormat), (string)value);
            }
            if (index.TryPopValue("output.path", out value))
            {
                option.Output.Path = (string)value;
            }
            if (index.TryPopValue("output.style", out value))
            {
                option.Output.Style = (OutputStyle)Enum.Parse(typeof(OutputStyle), (string)value);
            }

            BaselineOption.Load(option, index);
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
            return obj is PSRuleOption option && Equals(option);
        }

        public bool Equals(PSRuleOption other)
        {
            return other != null &&
                Binding == other.Binding &&
                Configuration == other.Configuration &&
                Execution == other.Execution &&
                Input == other.Input &&
                Logging == other.Logging &&
                Output == other.Output &&
                Suppression == other.Suppression &&
                Pipeline == other.Pipeline &&
                Rule == other.Rule;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine
            {
                int hash = 17;
                hash = hash * 23 + (Binding != null ? Binding.GetHashCode() : 0);
                hash = hash * 23 + (Configuration != null ? Configuration.GetHashCode() : 0);
                hash = hash * 23 + (Execution != null ? Execution.GetHashCode() : 0);
                hash = hash * 23 + (Input != null ? Input.GetHashCode() : 0);
                hash = hash * 23 + (Logging != null ? Logging.GetHashCode() : 0);
                hash = hash * 23 + (Output != null ? Output.GetHashCode() : 0);
                hash = hash * 23 + (Suppression != null ? Suppression.GetHashCode() : 0);
                hash = hash * 23 + (Pipeline != null ? Pipeline.GetHashCode() : 0);
                hash = hash * 23 + (Rule != null ? Rule.GetHashCode() : 0);
                return hash;
            }
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
                var ext = Path.GetExtension(rootedPath);
                if (string.Equals(ext, ".yaml", StringComparison.OrdinalIgnoreCase) || string.Equals(ext, ".yml", StringComparison.OrdinalIgnoreCase))
                {
                    return rootedPath;
                }
            }

            // Check if default files exist and 
            return UseFilePath(path: rootedPath, name: "ps-rule.yaml") ??
                UseFilePath(path: rootedPath, name: "ps-rule.yml") ??
                UseFilePath(path: rootedPath, name: "psrule.yaml") ??
                UseFilePath(path: rootedPath, name: "psrule.yml") ??
                Path.Combine(rootedPath, DEFAULT_FILENAME);
        }

        /// <summary>
        /// Get a full path instead of a relative path that may be passed from PowerShell.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static string GetRootedPath(string path)
        {
            return Path.IsPathRooted(path) ? path : Path.GetFullPath(Path.Combine(GetWorkingPath(), path));
        }

        internal static Dictionary<string, object> BuildIndex(Hashtable hashtable)
        {
            var index = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (DictionaryEntry entry in hashtable)
                index.Add(entry.Key.ToString(), entry.Value);

            return index;
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

        private string GetYaml()
        {
            var s = new SerializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .WithTypeConverter(new FieldMapYamlTypeConverter())
                .Build();
            return s.Serialize(this);
        }

        private static string[] AsStringArray(object value)
        {
            return value.GetType().IsArray ? ((object[])value).OfType<string>().ToArray() : new string[] { value.ToString() };
        }
    }
}
