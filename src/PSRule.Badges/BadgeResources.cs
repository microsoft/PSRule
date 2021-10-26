// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace PSRule.Badges
{
    /// <summary>
    /// A helper class for working with badge resources.
    /// </summary>
    internal static class BadgeResources
    {
        private const string DEFAULT_CULTURE_RESOURCE = "PSRule.Badges.Resources.en.json";

        private static char[] _Char;
        private static double[] _Width;

        /// <summary>
        /// Load pre-calculated widths for characters.
        /// </summary>
        private static void Load()
        {
            if (_Char != null && _Width != null)
                return;

            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(DEFAULT_CULTURE_RESOURCE);
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            var d = JsonConvert.DeserializeObject<object[][]>(json);
            _Char = new char[d.Length];
            _Width = new double[d.Length];
            for (var i = 0; i < d.Length; i++)
            {
                _Char[i] = Convert.ToChar(d[i][0]);
                _Width[i] = Convert.ToDouble(d[i][1]);
            }
        }

        /// <summary>
        /// Get the width in pixels for a character.
        /// </summary>
        private static double GetWidth(char c)
        {
            Load();
            return Find(c);
        }

        /// <summary>
        /// Find the width in pixels for a character.
        /// </summary>
        private static double Find(char c)
        {
            var index = Array.BinarySearch(_Char, c);
            if (index >= 0)
                return _Width[index];

            return 0d;
        }

        public static double Measure(string s)
        {
            var length = 0d;
            for (var i = 0; i < s.Length; i++)
                length += GetWidth(s[i]);

            return length;
        }
    }
}
