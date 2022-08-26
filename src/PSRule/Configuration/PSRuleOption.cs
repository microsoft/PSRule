// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Threading;
using Newtonsoft.Json;
using PSRule.Definitions.Baselines;
using PSRule.Resources;
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
    /// <remarks>
    /// See <see href="https://aka.ms/ps-rule/options"/>.
    /// </remarks>
    public sealed class PSRuleOption : IEquatable<PSRuleOption>, IBaselineV1Spec
    {
        private const string DEFAULT_FILENAME = "ps-rule.yaml";

        private const char Backslash = '\\';
        private const char Slash = '/';

        /// <summary>
        /// The original source path the options were loaded from if applicable.
        /// </summary>
        private string _SourcePath;

        private static readonly PSRuleOption Default = new()
        {
            Binding = BindingOption.Default,
            Convention = ConventionOption.Default,
            Execution = ExecutionOption.Default,
            Include = IncludeOption.Default,
            Input = InputOption.Default,
            Logging = LoggingOption.Default,
            Output = OutputOption.Default,
            Rule = RuleOption.Default,
        };

        /// <summary>
        /// A callback that is overridden by PowerShell so that the current working path can be retrieved.
        /// </summary>
        private static PathDelegate _GetWorkingPath = () => Directory.GetCurrentDirectory();

        /// <summary>
        /// Sets the current culture to use when processing rules unless otherwise specified.
        /// </summary>
        private static CultureInfo _CurrentCulture = Thread.CurrentThread.CurrentCulture;

        /// <summary>
        /// Create an empty PSRule options object.
        /// </summary>
        public PSRuleOption()
        {
            // Set defaults
            Binding = new BindingOption();
            Configuration = new ConfigurationOption();
            Convention = new ConventionOption();
            Execution = new ExecutionOption();
            Include = new IncludeOption();
            Input = new InputOption();
            Logging = new LoggingOption();
            Output = new OutputOption();
            Pipeline = new PipelineHook();
            Repository = new RepositoryOption();
            Requires = new RequiresOption();
            Rule = new RuleOption();
            Suppression = new SuppressionOption();
        }

        private PSRuleOption(string sourcePath, PSRuleOption option)
        {
            _SourcePath = sourcePath;

            // Set from existing option instance
            Binding = new BindingOption(option?.Binding);
            Configuration = new ConfigurationOption(option?.Configuration);
            Convention = new ConventionOption(option?.Convention);
            Execution = new ExecutionOption(option?.Execution);
            Include = new IncludeOption(option?.Include);
            Input = new InputOption(option?.Input);
            Logging = new LoggingOption(option?.Logging);
            Output = new OutputOption(option?.Output);
            Pipeline = new PipelineHook(option?.Pipeline);
            Repository = new RepositoryOption(option?.Repository);
            Requires = new RequiresOption(option?.Requires);
            Rule = new RuleOption(option?.Rule);
            Suppression = new SuppressionOption(option?.Suppression);
        }

        /// <summary>
        /// Options that affect property binding.
        /// </summary>
        public BindingOption Binding { get; set; }

        /// <summary>
        /// Allows configuration key/ values to be specified that can be used within rule definitions.
        /// </summary>
        public ConfigurationOption Configuration { get; set; }

        /// <summary>
        /// Options that configure conventions.
        /// </summary>
        public ConventionOption Convention { get; set; }

        /// <summary>
        /// Options that configure the execution sandbox.
        /// </summary>
        public ExecutionOption Execution { get; set; }

        /// <summary>
        /// Options that affect source locations imported for execution.
        /// </summary>
        public IncludeOption Include { get; set; }

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
        /// Configures pipeline hooks.
        /// </summary>
        [YamlIgnore]
        [JsonIgnore]
        public PipelineHook Pipeline { get; set; }

        /// <summary>
        /// Options for repository properties that are used by PSRule.
        /// </summary>
        public RepositoryOption Repository { get; set; }

        /// <summary>
        /// Specifies the required version of a module to use.
        /// </summary>
        public RequiresOption Requires { get; set; }

        /// <summary>
        /// Options for that affect which rules are executed by including and filtering discovered rules.
        /// </summary>
        public RuleOption Rule { get; set; }

        /// <summary>
        /// A set of suppression rules.
        /// </summary>
        public SuppressionOption Suppression { get; set; }

        /// <summary>
        /// Return options as YAML.
        /// </summary>
        /// <returns>PSRule options serialized as YAML.</returns>
        /// <remarks>
        /// Called from PowerShell.
        /// </remarks>
        public string ToYaml()
        {
            var yaml = GetYaml();
            return string.IsNullOrEmpty(_SourcePath)
                ? yaml
                : string.Concat(
                    string.Format(
                        Thread.CurrentThread.CurrentCulture,
                        PSRuleResources.OptionsSourceComment,
                        _SourcePath),
                    Environment.NewLine,
                    yaml);
        }

        /// <summary>
        /// Create a new object instance with the same options set.
        /// </summary>
        /// <returns>A new <see cref="PSRuleOption"/> instance.</returns>
        public PSRuleOption Clone()
        {
            return new PSRuleOption(sourcePath: _SourcePath, option: this);
        }

        /// <summary>
        /// Create a <see cref="PSRuleOption"/> instance from PSRule defaults.
        /// </summary>
        /// <returns>A new <see cref="PSRuleOption"/> instance.</returns>
        public static PSRuleOption FromDefault()
        {
            return Default.Clone();
        }

        /// <summary>
        /// Merge two option instances by repacing any unset properties from <paramref name="o1"/> with <paramref name="o2"/> values.
        /// Values from <paramref name="o1"/> that are set are not overridden.
        /// </summary>
        /// <returns>A new <see cref="PSRuleOption"/> instance combining options from both instances.</returns>
        private static PSRuleOption Combine(PSRuleOption o1, PSRuleOption o2)
        {
            var result = new PSRuleOption(o1?._SourcePath ?? o2?._SourcePath, o1);
            result.Binding = BindingOption.Combine(result.Binding, o2?.Binding);
            result.Configuration = ConfigurationOption.Combine(result.Configuration, o2?.Configuration);
            result.Convention = ConventionOption.Combine(result.Convention, o2?.Convention);
            result.Execution = ExecutionOption.Combine(result.Execution, o2?.Execution);
            result.Include = IncludeOption.Combine(result.Include, o2?.Include);
            result.Input = InputOption.Combine(result.Input, o2?.Input);
            result.Logging = LoggingOption.Combine(result.Logging, o2?.Logging);
            result.Repository = RepositoryOption.Combine(result.Repository, o2?.Repository);
            result.Output = OutputOption.Combine(result.Output, o2?.Output);
            return result;
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
        /// <param name="path">A file or directory to read options from.</param>
        /// <returns>An options object.</returns>
        /// <remarks>
        /// This method is called from PowerShell.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Avoid nested conditional expressions that increase complexity.")]
        public static PSRuleOption FromFile(string path)
        {
            // Get a rooted file path instead of directory or relative path
            var filePath = GetFilePath(path);

            // Fallback to defaults even if file does not exist when silentlyContinue is true
            if (!File.Exists(filePath))
                throw new FileNotFoundException(PSRuleResources.OptionsNotFound, filePath);

            return FromEnvironment(FromYaml(path: filePath, yaml: File.ReadAllText(filePath)));
        }

        /// <summary>
        /// Load a YAML formatted PSRuleOption object from disk.
        /// </summary>
        /// <param name="path">A file or directory to read options from.</param>
        /// <returns>An options object.</returns>
        /// <remarks>
        /// This method is called from PowerShell.
        /// </remarks>
        public static PSRuleOption FromFileOrEmpty(string path)
        {
            // Get a rooted file path instead of directory or relative path
            var filePath = GetFilePath(path);

            // Return empty options if file does not exist
            return !File.Exists(filePath) ? new PSRuleOption() : FromEnvironment(FromYaml(path: filePath, yaml: File.ReadAllText(filePath)));
        }

        /// <summary>
        /// Load a YAML formatted PSRuleOption object from disk.
        /// </summary>
        /// <returns>An options object.</returns>
        /// <remarks>
        /// This method is called from PowerShell.
        /// </remarks>
        public static PSRuleOption FromFileOrEmpty()
        {
            return FromFileOrEmpty(GetWorkingPath());
        }

        /// <summary>
        /// Load a YAML formatted PSRuleOption object from disk.
        /// </summary>
        /// <param name="option"></param>
        /// <param name="path">A file or directory to read options from.</param>
        /// <returns>An options object.</returns>
        /// <remarks>
        /// This method is called from PowerShell.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Avoid nested conditional expressions that increase complexity.")]
        public static PSRuleOption FromFileOrEmpty(PSRuleOption option, string path)
        {
            if (option == null)
                return FromFileOrEmpty(path);

            return string.IsNullOrEmpty(option._SourcePath) ? Combine(option, FromFileOrEmpty(path)) : option;
        }

        private static PSRuleOption FromYaml(string path, string yaml)
        {
            var d = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeConverter(new FieldMapYamlTypeConverter())
                .WithTypeConverter(new SuppressionRuleYamlTypeConverter())
                .WithTypeConverter(new PSObjectYamlTypeConverter())
                .WithNodeTypeResolver(new PSOptionYamlTypeResolver())
                .Build();

            var option = d.Deserialize<PSRuleOption>(yaml) ?? new PSRuleOption();
            option._SourcePath = path;
            return option;
        }

        /// <summary>
        /// Read PSRule options from environment variables.
        /// </summary>
        /// <param name="option">An existing options object to set. If <c>null</c> an empty options object is used.</param>
        /// <returns>An options object.</returns>
        /// <remarks>
        /// Any environment variables that are set will override options set in the specified <paramref name="option"/> object.
        /// </remarks>
        private static PSRuleOption FromEnvironment(PSRuleOption option)
        {
            option ??= new PSRuleOption();

            // Start loading matching values
            var env = EnvironmentHelper.Default;
            option.Convention.Load(env);
            option.Execution.Load(env);
            option.Include.Load(env);
            option.Input.Load(env);
            option.Logging.Load(env);
            option.Output.Load(env);
            option.Repository.Load(env);
            option.Requires.Load(env);
            BaselineOption.Load(option, env);
            return option;
        }

        /// <summary>
        /// Read PSRule options from a hashtable.
        /// </summary>
        /// <param name="hashtable">A hashtable to read options from.</param>
        /// <returns>An options object.</returns>
        /// <remarks>
        /// A null or empty hashtable will return an empty options object.
        /// </remarks>
        public static PSRuleOption FromHashtable(Hashtable hashtable)
        {
            var option = new PSRuleOption();
            if (hashtable == null || hashtable.Count == 0)
                return option;

            // Start loading matching values
            var index = BuildIndex(hashtable);
            option.Convention.Load(index);
            option.Execution.Load(index);
            option.Include.Load(index);
            option.Input.Load(index);
            option.Logging.Load(index);
            option.Output.Load(index);
            option.Repository.Load(index);
            option.Requires.Load(index);
            BaselineOption.Load(option, index);
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

        /// <summary>
        /// Configures PSRule to use the culture of the current thread at runtime.
        /// </summary>
        [DebuggerStepThrough]
        public static void UseCurrentCulture()
        {
            UseCurrentCulture(Thread.CurrentThread.CurrentCulture);
        }

        /// <summary>
        /// Configures PSRule to use the specified culture at runtime.
        /// </summary>
        /// <param name="culture">A valid culture.</param>
        [DebuggerStepThrough]
        public static void UseCurrentCulture(string culture)
        {
            UseCurrentCulture(CultureInfo.CreateSpecificCulture(culture));
        }

        /// <summary>
        /// Configures PSRule to use the specified culture at runtime. 
        /// </summary>
        /// <param name="culture">A valid culture.</param>
        public static void UseCurrentCulture(CultureInfo culture)
        {
            _CurrentCulture = culture;
        }

        /// <summary>
        /// Gets the current working path being used by PSRule.
        /// </summary>
        /// <returns>The current working path.</returns>
        public static string GetWorkingPath()
        {
            return _GetWorkingPath();
        }

        /// <summary>
        /// Get the current culture being used by PSRule.
        /// </summary>
        /// <returns>The current culture.</returns>
        public static CultureInfo GetCurrentCulture()
        {
            return _CurrentCulture;
        }

        /// <summary>
        /// Convert from hashtable to options by processing key values. This enables -Option @{ } from PowerShell.
        /// </summary>
        /// <param name="hashtable">A hashtable to read options from.</param>
        /// <returns>An options object.</returns>
        public static implicit operator PSRuleOption(Hashtable hashtable)
        {
            return FromHashtable(hashtable);
        }

        /// <summary>
        /// Convert from string to options by loading the yaml file from disk. This enables -Option '.\ps-rule.yaml' from PowerShell.
        /// </summary>
        /// <param name="path">A file or directory to read options from.</param>
        /// <returns>An options object.</returns>
        public static implicit operator PSRuleOption(string path)
        {
            return FromFile(path);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is PSRuleOption option && Equals(option);
        }

        /// <inheritdoc/>
        public bool Equals(PSRuleOption other)
        {
            return other != null &&
                Binding == other.Binding &&
                Configuration == other.Configuration &&
                Convention == other.Convention &&
                Execution == other.Execution &&
                Include == other.Include &&
                Input == other.Input &&
                Logging == other.Logging &&
                Output == other.Output &&
                Suppression == other.Suppression &&
                Pipeline == other.Pipeline &&
                Repository == other.Repository &&
                Rule == other.Rule;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine
            {
                var hash = 17;
                hash = hash * 23 + (Binding != null ? Binding.GetHashCode() : 0);
                hash = hash * 23 + (Configuration != null ? Configuration.GetHashCode() : 0);
                hash = hash * 23 + (Convention != null ? Convention.GetHashCode() : 0);
                hash = hash * 23 + (Execution != null ? Execution.GetHashCode() : 0);
                hash = hash * 23 + (Include != null ? Include.GetHashCode() : 0);
                hash = hash * 23 + (Input != null ? Input.GetHashCode() : 0);
                hash = hash * 23 + (Logging != null ? Logging.GetHashCode() : 0);
                hash = hash * 23 + (Output != null ? Output.GetHashCode() : 0);
                hash = hash * 23 + (Suppression != null ? Suppression.GetHashCode() : 0);
                hash = hash * 23 + (Pipeline != null ? Pipeline.GetHashCode() : 0);
                hash = hash * 23 + (Repository != null ? Repository.GetHashCode() : 0);
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
                if (string.Equals(ext, ".yaml", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(ext, ".yml", StringComparison.OrdinalIgnoreCase))
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
        /// <param name="path">A full or relative path.</param>
        /// <param name="normalize">When set to <c>true</c> the returned path uses forward slashes instead of backslashes.</param>
        /// <param name="basePath">The base path to use. When <c>null</c> of unspecified, the current working path will be used.</param>
        /// <returns>A absolute path.</returns>
        internal static string GetRootedPath(string path, bool normalize = false, string basePath = null)
        {
            basePath ??= GetWorkingPath();
            var rootedPath = Path.IsPathRooted(path) ? Path.GetFullPath(path) : Path.GetFullPath(Path.Combine(basePath, path));
            return normalize ? rootedPath.Replace(Backslash, Slash) : rootedPath;
        }

        /// <summary>
        /// Get a full base path instead of a relative path that may be passed from PowerShell.
        /// </summary>
        /// <param name="path">A full or relative path.</param>
        /// <param name="normalize">When set to <c>true</c> the returned path uses forward slashes instead of backslashes.</param>
        /// <returns>A absolute base path.</returns>
        /// <remarks>
        /// A base path always includes a trailing <c>/</c>.
        /// </remarks>
        internal static string GetRootedBasePath(string path, bool normalize = false)
        {
            var rootedPath = GetRootedPath(path);
            var basePath = rootedPath.Length > 0 && IsSeparator(rootedPath[rootedPath.Length - 1])
                ? rootedPath
                : string.Concat(rootedPath, Path.DirectorySeparatorChar);
            return normalize ? basePath.Replace(Backslash, Slash) : basePath;
        }

        /// <summary>
        /// Build index to allow mapping values.
        /// </summary>
        [DebuggerStepThrough]
        internal static Dictionary<string, object> BuildIndex(Hashtable hashtable)
        {
            var index = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (DictionaryEntry entry in hashtable)
                index.Add(entry.Key.ToString(), entry.Value);

            return index;
        }

        /// <summary>
        /// Determines if the working path file system is case sensitive.
        /// </summary>
        [DebuggerStepThrough]
        internal static bool IsCaseSentitive()
        {
            var lower = GetWorkingPath().ToLower(Thread.CurrentThread.CurrentCulture);
            if (!Directory.Exists(lower))
                return true;

            var upper = GetWorkingPath().ToUpper(Thread.CurrentThread.CurrentCulture);
            return !Directory.Exists(upper);
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
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeConverter(new FieldMapYamlTypeConverter())
                .Build();
            return s.Serialize(this);
        }

        [DebuggerStepThrough]
        private static bool IsSeparator(char c)
        {
            return c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar || c == '/' || c == '\\';
        }
    }
}
