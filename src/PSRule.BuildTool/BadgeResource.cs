// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace PSRule.BuildTool;

internal sealed class BadgeResourceOption
{
    public string OutputPath { get; set; }
}

/// <summary>
/// Builds badge resources such as measuring characters.
/// </summary>
[SupportedOSPlatform("windows")]
internal sealed class BadgeResource
{
    public static int Build(BadgeResourceOption options, ClientContext clientContext)
    {
        // Guard non-Windows platforms.
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            clientContext.Logger.LogError("This tool supports execution on Windows platforms only.");
            return 1;
        }

        var c = GetChars();
        var set = new object[c.Length][];
        var padding = GetPadding();

        for (var i = 0; i < c.Length; i++)
        {
            set[i] = [c[i], Measure(c[i]) - padding];
        }

        var json = JsonConvert.SerializeObject(set);
        File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), options.OutputPath ?? "en.json"), json);

        return 0;
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
