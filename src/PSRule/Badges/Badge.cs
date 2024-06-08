// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Badges;

/// <summary>
/// An instance of a badge created by the Badge API.
/// </summary>
internal sealed class Badge : IBadge
{
    private readonly string _LeftText;
    private readonly string _RightText;
    private readonly double _LeftWidth;
    private readonly double _RightWidth;
    private readonly int _MidPadding;
    private readonly int _BorderPadding;
    private readonly string _Fill;

    internal Badge(string left, string right, string fill)
    {
        _LeftWidth = BadgeResources.Measure(left);
        _RightWidth = BadgeResources.Measure(right);

        _LeftText = left;
        _RightText = right;
        _MidPadding = 3;
        _BorderPadding = 7;
        _Fill = fill;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return ToSvg();
    }

    /// <inheritdoc/>
    public string ToSvg()
    {
        var w = (int)Math.Round(_LeftWidth + _RightWidth + 2 * _BorderPadding + 2 * _MidPadding);
        var x = (int)Math.Round(_LeftWidth + _BorderPadding + _MidPadding);

        var builder = new SvgBuilder(
            width: w,
            height: 20,
            textScale: 10,
            midPoint: x,
            rounding: 2,
            borderPadding: _BorderPadding,
            midPadding: _MidPadding);
        builder.Begin(string.Concat(_LeftText, ": ", _RightText));
        builder.Backfill(_Fill);
        builder.TextBlock(_LeftText, _RightText, 110);
        builder.End();
        return builder.ToString();
    }

    /// <inheritdoc/>
    public void ToFile(string path)
    {
        path = Environment.GetRootedPath(path);
        var parentPath = Directory.GetParent(path);
        if (!parentPath.Exists)
            Directory.CreateDirectory(path: parentPath.FullName);

        File.WriteAllText(path, contents: ToSvg());
    }
}
