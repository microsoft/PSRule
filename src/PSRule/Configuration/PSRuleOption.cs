using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PSRule.Configuration
{
    /// <summary>
    /// A delgate to allow callback to PowerShell to get current working path.
    /// </summary>
    public delegate string PathDelegate();

    public sealed class PSRuleOption
    {
        public PSRuleOption()
        {
            // Set defaults
            Suppression = new SuppressionOption();
            Execution = new ExecutionOption();
        }

        public PSRuleOption(PSRuleOption option)
        {
            // Set from existing option instance
            Suppression = new SuppressionOption(option.Suppression);
            Execution = new ExecutionOption
            {
                LanguageMode = option.Execution.LanguageMode
            };
        }

        /// <summary>
        /// A callback that is overridden by PowerShell so that the current working path can be retrieved.
        /// </summary>
        public static PathDelegate GetWorkingPath = () => Directory.GetCurrentDirectory();

        /// <summary>
        /// A set of suppression rules.
        /// </summary>
        public SuppressionOption Suppression { get; set; }

        /// <summary>
        /// Options that affect script execution.
        /// </summary>
        public ExecutionOption Execution { get; set; }

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
                    return new PSRuleOption();
                }
            }

            return FromYaml(File.ReadAllText(rootedPath));
        }

        public static PSRuleOption FromYaml(string yaml)
        {
            var d = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithNamingConvention(new CamelCaseNamingConvention())
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

            if (index.TryGetValue("execution.languagemode", out value))
            {
                option.Execution.LanguageMode = (LanguageMode)Enum.Parse(typeof(LanguageMode), (string)value);
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
