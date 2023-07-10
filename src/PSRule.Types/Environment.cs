// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Security;
using PSRule.Data;

namespace PSRule
{
    /// <summary>
    /// A helper for accessing environment variables.
    /// </summary>
    public static class Environment
    {
        private static readonly char[] STRINGARRAYMAP_ITEMSEPARATOR = new char[] { ',' };
        private static readonly char[] STRINGARRAY_SEPARATOR = new char[] { ';' };
        private static readonly char[] LINUX_PATH_ENV_SEPARATOR = new char[] { ':' };
        private static readonly char[] WINDOWS_PATH_ENV_SEPARATOR = new char[] { ';' };

        private const char STRINGARRYAMAP_PAIRSEPARATOR = '=';
        private const string PATH_ENV = "PATH";
        private const string DEFAULT_CREDENTIAL_USERNAME = "na";
        private const string TF_BUILD = "TF_BUILD";
        private const string GITHUB_ACTIONS = "GITHUB_ACTIONS";

        /// <summary>
        /// Determine if the environment is running within Azure Pipelines.
        /// </summary>
        public static bool IsAzurePipelines()
        {
            return TryBool(TF_BUILD, out var azp) && azp;
        }

        /// <summary>
        /// Determines if the environment is running within GitHub Actions.
        /// </summary>
        public static bool IsGitHubActions()
        {
            return TryBool(GITHUB_ACTIONS, out var gh) && gh;
        }

        /// <summary>
        /// Determine if the environment is running within Visual Studio Code.
        /// </summary>
        public static bool IsVisualStudioCode()
        {
            return TryString("TERM_PROGRAM", out var term) && term == "vscode";
        }

        /// <summary>
        /// Get the run identifier for the current environment.
        /// </summary>
        public static string GetRunId()
        {
            if (TryString("PSRULE_RUN_ID", out var runId))
                return runId;

            return TryString("BUILD_REPOSITORY_NAME", out var prefix) && TryString("BUILD_BUILDID", out var suffix) ||
                TryString("GITHUB_REPOSITORY", out prefix) && TryString("GITHUB_RUN_ID", out suffix)
                ? string.Concat(prefix, "/", suffix)
                : null;
        }

        /// <summary>
        /// Try to get the environment variable as a <see cref="string"/>.
        /// </summary>
        public static bool TryString(string key, out string value)
        {
            return TryVariable(key, out value) && !string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Try to get the environment variable as a <see cref="SecureString"/>.
        /// </summary>
        public static bool TrySecureString(string key, out SecureString value)
        {
            value = null;
            if (!TryString(key, out var variable))
                return false;

            value = new NetworkCredential(DEFAULT_CREDENTIAL_USERNAME, variable).SecurePassword;
            return true;
        }

        /// <summary>
        /// Try to get the environment variable as an <see cref="int"/>.
        /// </summary>
        public static bool TryInt(string key, out int value)
        {
            value = default;
            return TryVariable(key, out var variable) && int.TryParse(variable, out value);
        }

        /// <summary>
        /// Try to get the environment variable as a <see cref="bool"/>.
        /// </summary>
        public static bool TryBool(string key, out bool value)
        {
            value = default;
            return TryVariable(key, out var variable) && TryParseBool(variable, out value);
        }

        /// <summary>
        /// Try to get the environment variable as a enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        public static bool TryEnum<TEnum>(string key, out TEnum value) where TEnum : struct
        {
            value = default;
            return TryVariable(key, out var variable) && Enum.TryParse(variable, ignoreCase: true, out value);
        }

        /// <summary>
        /// Try to get the environment variable as an array of strings.
        /// </summary>
        public static bool TryStringArray(string key, out string[] value)
        {
            value = default;
            if (!TryVariable(key, out var variable))
                return false;

            value = variable.Split(STRINGARRAY_SEPARATOR, options: StringSplitOptions.RemoveEmptyEntries);
            return value != null;
        }

        /// <summary>
        /// Try to get the environment variable as a <see cref="StringArrayMap"/>.
        /// </summary>
        public static bool TryStringArrayMap(string key, out StringArrayMap value)
        {
            value = default;
            if (!TryVariable(key, out var variable))
                return false;

            var pairs = variable.Split(STRINGARRAY_SEPARATOR, options: StringSplitOptions.RemoveEmptyEntries);
            if (pairs == null)
                return false;

            var map = new StringArrayMap();
            for (var i = 0; i < pairs.Length; i++)
            {
                var index = pairs[i].IndexOf(STRINGARRYAMAP_PAIRSEPARATOR);
                if (index < 1 || index + 1 >= pairs[i].Length) continue;

                var left = pairs[i].Substring(0, index);
                var right = pairs[i].Substring(index + 1);
                var pair = right.Split(STRINGARRAYMAP_ITEMSEPARATOR, StringSplitOptions.RemoveEmptyEntries);
                map[left] = pair;
            }
            value = map;
            return true;
        }

        /// <summary>
        /// Try to get the PATH environment variable.
        /// </summary>
        public static bool TryPathEnvironmentVariable(out string[] value)
        {
            return TryPathEnvironmentVariable(PATH_ENV, out value);
        }

        /// <summary>
        /// Try to get a PATH environment variable with a specific name.
        /// </summary>
        public static bool TryPathEnvironmentVariable(string key, out string[] value)
        {
            value = default;
            if (!TryVariable(key, out var variable))
                return false;

            var separator = System.Environment.OSVersion.Platform == PlatformID.Win32NT ? WINDOWS_PATH_ENV_SEPARATOR : LINUX_PATH_ENV_SEPARATOR;
            value = variable.Split(separator, options: StringSplitOptions.RemoveEmptyEntries);
            return value != null;
        }

        /// <summary>
        /// Try to get any environment variable with a specific prefix.
        /// </summary>
        public static IEnumerable<KeyValuePair<string, object>> GetByPrefix(string prefix)
        {
            var env = System.Environment.GetEnvironmentVariables();
            var enumerator = env.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var key = enumerator.Key.ToString();
                if (key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    yield return new KeyValuePair<string, object>(key, enumerator.Value);
            }
        }

        private static bool TryVariable(string key, out string variable)
        {
            variable = System.Environment.GetEnvironmentVariable(key);
            return variable != null;
        }

        private static bool TryParseBool(string variable, out bool value)
        {
            if (bool.TryParse(variable, out value))
                return true;

            if (int.TryParse(variable, out var ivalue))
            {
                value = ivalue > 0;
                return true;
            }
            return false;
        }
    }
}
