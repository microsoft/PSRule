// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Security;

namespace PSRule
{
    internal static class EnvironmentHelper
    {
        private const char UNDERSCORE = '_';

        internal static bool TryString(string key, out string value)
        {
            value = null;
            var variable = System.Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrEmpty(variable))
                return false;

            value = variable;
            return true;
        }

        internal static bool TrySecureString(string key, out SecureString value)
        {
            value = null;
            if (!TryString(key, out string variable))
                return false;

            value = new NetworkCredential("na", variable).SecurePassword;
            return true;
        }

        internal static bool TryInt(string key, out int value)
        {
            var variable = System.Environment.GetEnvironmentVariable(key);
            if (!int.TryParse(variable, out value))
                return false;

            return true;
        }

        internal static bool TryBool(string key, out bool value)
        {
            var variable = System.Environment.GetEnvironmentVariable(key);
            if (!bool.TryParse(variable, out value))
                return false;

            return true;
        }

        private static string CombineKey(string prefix, string key)
        {
            return string.IsNullOrEmpty(prefix) ? key : string.Concat(prefix, UNDERSCORE, key);
        }
    }
}
