// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.CommandLine.Invocation;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace PSRule.BuildTool
{
    internal sealed class BadgeResourceOption
    {
        public string OutputPath { get; set; }
    }

    /// <summary>
    /// Builds badge resources such as measuring characters.
    /// </summary>
    internal sealed class BadgeResource
    {
        public static void Build(BadgeResourceOption options, InvocationContext invocation)
        {
            // Guard non-Windows platforms.
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                invocation.Console.Error.Write("This tool supports execution on Windows platforms only.");
                invocation.ExitCode = 1;
                return;
            }

            var c = GetChars();
            var set = new object[c.Length][];
            var padding = GetPadding();
            for (var i = 0; i < c.Length; i++)
                set[i] = new object[2] { c[i], Measure(c[i]) - padding };

            var json = JsonConvert.SerializeObject(set);
            File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), options.OutputPath ?? "en.json"), json);
        }

        private static char[] GetChars()
        {
            var c = " !/0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".ToCharArray();
            Array.Sort(c);
            return c;
        }

        /// <summary>
        /// Calculate padding for GDI+.
        /// </summary>
        private static double GetPadding()
        {
            var test = "PSRule";
            var s_length = Measure(test);
            var c_length = 0d;
            for (var i = 0; i < test.Length; i++)
                c_length += Measure(test[i]);

            return (c_length - s_length) / 5;
        }

        private static double Measure(char c)
        {
            return Measure(c.ToString());
        }

        private static double Measure(string s)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return 0;

            using var font = new Font("Verdana", 11f, GraphicsUnit.Pixel);
            using var g = Graphics.FromHwnd(IntPtr.Zero);
            var size = g.MeasureString(s, font);
            return size.Width;
        }
    }
}
