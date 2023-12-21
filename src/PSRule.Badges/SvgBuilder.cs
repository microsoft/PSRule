// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;

namespace PSRule.Badges;

/// <summary>
/// Generate SVG badges.
/// </summary>
internal sealed class SvgBuilder
{
    private readonly int _Width;
    private readonly int _Height;
    private readonly int _TextScale;
    private readonly int _MidPoint;
    private readonly int _Rounding;
    private readonly int _BorderPadding;
    private readonly int _MidPadding;
    private readonly StringBuilder _Builder;

    public SvgBuilder(int width, int height, int textScale, int midPoint, int rounding, int borderPadding, int midPadding)
    {
        _Width = width;
        _Height = height;
        _TextScale = textScale;
        _MidPoint = midPoint;
        _Rounding = rounding;
        _BorderPadding = borderPadding;
        _MidPadding = midPadding;
        _Builder = new StringBuilder();
    }

    public void Backfill(string fill)
    {
        var l = _MidPoint;
        var r = _Width - _MidPoint;

        Append("<linearGradient id=\"a\" x2=\"0\" y2=\"100%\"><stop offset=\"0.0\" stop-opacity=\"0.0\" stop-color=\"#000\" /><stop offset=\"1.0\" stop-opacity=\"0.2\" stop-color=\"#000\" /></linearGradient>");
        Append($"<clipPath id=\"c\"><rect width=\"{_Width}\" height=\"20.0\" rx=\"{_Rounding}\" /></clipPath>");
        Append("<g clip-path=\"url(#c)\">");
        Append($"<rect width=\"{l}\" height=\"20.0\" fill=\"#555555\" />");
        Append($"<rect width=\"{r}\" height=\"20.0\" fill=\"{fill}\" x=\"{l}\" />");
        Append($"<rect width=\"{_Width}\" height=\"20.0\" fill=\"url(#a)\" />");
        Append("</g>");
    }

    public void TextBlock(string left, string right, int fontSize)
    {
        var leftWidth = BadgeResources.Measure(left);
        var rightWidth = BadgeResources.Measure(right);

        Append($"<g fill=\"#fff\" text-anchor=\"middle\" font-family=\"Verdana,sans-serif\" text-rendering=\"geometricPrecision\" font-size=\"{fontSize}px\">");
        var l = _MidPoint / 2;
        var r = _MidPoint + _MidPadding + (rightWidth / 2);

        Text(left, leftWidth, l, 14.0);
        Text(right, rightWidth, r, 14.0);
        Append("</g>");
    }

    private void Text(string text, double width, double x, double y)
    {
        TextDropShadow(text, width, x, y);

        x = Math.Round(x * _TextScale);
        y = Math.Round(y * _TextScale);
        width = Math.Round(width * _TextScale);
        Append($"<text x=\"{x}\" y=\"{y}\" fill=\"#fff\" transform=\"scale(.1)\" textLength=\"{width}\">{text}</text>");
    }

    private void TextDropShadow(string text, double width, double x, double y)
    {
        x = Math.Round(x * _TextScale);
        y = Math.Round((y + 1) * _TextScale);
        width = Math.Round(width * _TextScale);

        Append($"<text aria-hidden=\"true\" x=\"{x}\" y=\"{y}\" fill=\"#000\" fill-opacity=\"0.3\" transform=\"scale(.1)\" textLength=\"{width}\">{text}</text>");
    }

    public void End()
    {
        Append("</svg>");
    }

    private void Append(string s)
    {
        _Builder.AppendLine(s);
    }

    /// <summary>
    /// Write opening tag for SVG.
    /// </summary>
    /// <param name="text">The text for screen readers.</param>
    public void Begin(string text)
    {
        Append($"<svg width=\"{_Width}\" height=\"{_Height}\" viewPort=\"0 0 {_Width} {_Height}\" xmlns=\"http://www.w3.org/2000/svg\" role=\"img\" aria-label=\"{text}\">");
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return _Builder.ToString();
    }
}
