// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;
using PSRule.Rules;

namespace PSRule.Configuration;

/// <summary>
/// Options for generating and formatting output.
/// </summary>
public sealed class OutputOption : IEquatable<OutputOption>
{
    private const ResultFormat DEFAULT_AS = ResultFormat.Detail;
    private const BannerFormat DEFAULT_BANNER = BannerFormat.Default;
    private const OutputEncoding DEFAULT_ENCODING = OutputEncoding.Default;
    private const FooterFormat DEFAULT_FOOTER = FooterFormat.Default;
    private const OutputFormat DEFAULT_FORMAT = OutputFormat.None;
    private const int DEFAULT_JSON_INDENT = 0;
    private const RuleOutcome DEFAULT_OUTCOME = RuleOutcome.Processed;
    private const bool DEFAULT_SARIF_PROBLEMS_ONLY = true;
    private const OutputStyle DEFAULT_STYLE = OutputStyle.Detect;

    internal static readonly OutputOption Default = new()
    {
        As = DEFAULT_AS,
        Banner = DEFAULT_BANNER,
        Encoding = DEFAULT_ENCODING,
        Footer = DEFAULT_FOOTER,
        Format = DEFAULT_FORMAT,
        JsonIndent = DEFAULT_JSON_INDENT,
        Outcome = DEFAULT_OUTCOME,
        SarifProblemsOnly = DEFAULT_SARIF_PROBLEMS_ONLY,
        Style = DEFAULT_STYLE,
    };

    /// <summary>
    /// Creates an empty output option.
    /// </summary>
    public OutputOption()
    {
        As = null;
        Banner = null;
        Culture = null;
        CsvDetailedColumns = null;
        Encoding = null;
        Footer = null;
        Format = null;
        JobSummaryPath = null;
        JsonIndent = null;
        Path = null;
        SarifProblemsOnly = null;
        Style = null;
    }

    /// <summary>
    /// Creates a output option by copying an existing instance.
    /// </summary>
    /// <param name="option">The option instance to copy.</param>
    public OutputOption(OutputOption option)
    {
        if (option == null)
            return;

        As = option.As;
        Banner = option.Banner;
        Culture = option.Culture;
        CsvDetailedColumns = option.CsvDetailedColumns;
        Encoding = option.Encoding;
        Footer = option.Footer;
        Format = option.Format;
        JobSummaryPath = option.JobSummaryPath;
        JsonIndent = option.JsonIndent;
        Outcome = option.Outcome;
        Path = option.Path;
        SarifProblemsOnly = option.SarifProblemsOnly;
        Style = option.Style;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return obj is OutputOption option && Equals(option);
    }

    /// <inheritdoc/>
    public bool Equals(OutputOption other)
    {
        return other != null &&
            As == other.As &&
            Banner == other.Banner &&
            Culture == other.Culture &&
            CsvDetailedColumns == other.CsvDetailedColumns &&
            Encoding == other.Encoding &&
            Footer == other.Footer &&
            Format == other.Format &&
            JobSummaryPath == other.JobSummaryPath &&
            JsonIndent == other.JsonIndent &&
            Outcome == other.Outcome &&
            Path == other.Path &&
            SarifProblemsOnly == other.SarifProblemsOnly &&
            Style == other.Style;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked // Overflow is fine
        {
            var hash = 17;
            hash = hash * 23 + (As.HasValue ? As.Value.GetHashCode() : 0);
            hash = hash * 23 + (Banner.HasValue ? Banner.Value.GetHashCode() : 0);
            hash = hash * 23 + (Culture != null ? Culture.GetHashCode() : 0);
            hash = hash * 23 + (CsvDetailedColumns != null ? CsvDetailedColumns.GetHashCode() : 0);
            hash = hash * 23 + (Encoding.HasValue ? Encoding.Value.GetHashCode() : 0);
            hash = hash * 23 + (Footer.HasValue ? Footer.Value.GetHashCode() : 0);
            hash = hash * 23 + (Format.HasValue ? Format.Value.GetHashCode() : 0);
            hash = hash * 23 + (JobSummaryPath != null ? JobSummaryPath.GetHashCode() : 0);
            hash = hash * 23 + (JsonIndent.HasValue ? JsonIndent.Value.GetHashCode() : 0);
            hash = hash * 23 + (Outcome.HasValue ? Outcome.Value.GetHashCode() : 0);
            hash = hash * 23 + (Path != null ? Path.GetHashCode() : 0);
            hash = hash * 23 + (SarifProblemsOnly.HasValue ? SarifProblemsOnly.Value.GetHashCode() : 0);
            hash = hash * 23 + (Style.HasValue ? Style.Value.GetHashCode() : 0);
            return hash;
        }
    }

    internal static OutputOption Combine(OutputOption o1, OutputOption o2)
    {
        var result = new OutputOption(o1)
        {
            As = o1?.As ?? o2?.As,
            Banner = o1?.Banner ?? o2?.Banner,
            Culture = o1?.Culture ?? o2?.Culture,
            CsvDetailedColumns = o1?.CsvDetailedColumns ?? o2?.CsvDetailedColumns,
            Encoding = o1?.Encoding ?? o2?.Encoding,
            Footer = o1?.Footer ?? o2?.Footer,
            Format = o1?.Format ?? o2?.Format,
            JobSummaryPath = o1?.JobSummaryPath ?? o2?.JobSummaryPath,
            JsonIndent = o1?.JsonIndent ?? o2?.JsonIndent,
            Outcome = o1?.Outcome ?? o2?.Outcome,
            Path = o1?.Path ?? o2?.Path,
            SarifProblemsOnly = o1?.SarifProblemsOnly ?? o2?.SarifProblemsOnly,
            Style = o1?.Style ?? o2?.Style,
        };
        return result;
    }

    /// <summary>
    /// The type of result to produce.
    /// </summary>
    [DefaultValue(null)]
    public ResultFormat? As { get; set; }

    /// <summary>
    /// The information displayed for Assert-PSRule banner.
    /// </summary>
    [DefaultValue(null)]
    public BannerFormat? Banner { get; set; }

    /// <summary>
    /// One or more cultures to use for generating output.
    /// </summary>
    [DefaultValue(null)]
    public string[]? Culture { get; set; }

    /// <summary>
    /// The columns to include in CSV detailed output.
    /// </summary>
    [DefaultValue(null)]
    public string[]? CsvDetailedColumns { get; set; }

    /// <summary>
    /// The encoding to use when writing results to file.
    /// </summary>
    [DefaultValue(null)]
    public OutputEncoding? Encoding { get; set; }

    /// <summary>
    /// The information displayed for Assert-PSRule footer.
    /// </summary>
    [DefaultValue(null)]
    public FooterFormat? Footer { get; set; }

    /// <summary>
    /// The output format.
    /// </summary>
    [DefaultValue(null)]
    public OutputFormat? Format { get; set; }

    /// <summary>
    /// The path to a job summary output file.
    /// </summary>
    [DefaultValue(null)]
    public string? JobSummaryPath { get; set; }

    /// <summary>
    /// The indentation for JSON output.
    /// </summary>
    [DefaultValue(null)]
    public int? JsonIndent { get; set; }

    /// <summary>
    /// The outcome of rule results to return.
    /// </summary>
    [DefaultValue(null)]
    public RuleOutcome? Outcome { get; set; }

    /// <summary>
    /// The file path location to save results.
    /// </summary>
    [DefaultValue(null)]
    public string? Path { get; set; }

    /// <summary>
    /// The style that results will be presented in.
    /// </summary>
    [DefaultValue(null)]
    public OutputStyle? Style { get; set; }

    /// <summary>
    /// Determines if SARIF output only includes rules with fail or error outcomes.
    /// </summary>
    [DefaultValue(null)]
    public bool? SarifProblemsOnly { get; set; }

    internal void Load()
    {
        if (Environment.TryEnum("PSRULE_OUTPUT_AS", out ResultFormat value))
            As = value;

        if (Environment.TryEnum("PSRULE_OUTPUT_BANNER", out BannerFormat banner))
            Banner = banner;

        if (Environment.TryStringArray("PSRULE_OUTPUT_CULTURE", out var culture))
            Culture = culture;

        if (Environment.TryStringArray("PSRULE_OUTPUT_CSVDETAILEDCOLUMNS", out var csvDetailedColumns))
            CsvDetailedColumns = csvDetailedColumns;

        if (Environment.TryEnum("PSRULE_OUTPUT_ENCODING", out OutputEncoding encoding))
            Encoding = encoding;

        if (Environment.TryEnum("PSRULE_OUTPUT_FOOTER", out FooterFormat footer))
            Footer = footer;

        if (Environment.TryEnum("PSRULE_OUTPUT_FORMAT", out OutputFormat format))
            Format = format;

        if (Environment.TryString("PSRULE_OUTPUT_JOBSUMMARYPATH", out var jobSummaryPath))
            JobSummaryPath = jobSummaryPath;

        if (Environment.TryInt("PSRULE_OUTPUT_JSONINDENT", out var jsonIndent))
            JsonIndent = jsonIndent;

        if (Environment.TryEnum("PSRULE_OUTPUT_OUTCOME", out RuleOutcome outcome))
            Outcome = outcome;

        if (Environment.TryString("PSRULE_OUTPUT_PATH", out var path))
            Path = path;

        if (Environment.TryEnum("PSRULE_OUTPUT_STYLE", out OutputStyle style))
            Style = style;

        if (Environment.TryBool("PSRULE_OUTPUT_SARIFPROBLEMSONLY", out var sarifProblemsOnly))
            SarifProblemsOnly = sarifProblemsOnly;
    }

    internal void Load(Dictionary<string, object> index)
    {
        if (index.TryPopEnum("Output.As", out ResultFormat value))
            As = value;

        if (index.TryPopEnum("Output.Banner", out BannerFormat banner))
            Banner = banner;

        if (index.TryPopStringArray("Output.Culture", out var culture))
            Culture = culture;

        if (index.TryPopStringArray("Output.CsvDetailedColumns", out var csvDetailedColumns))
            CsvDetailedColumns = csvDetailedColumns;

        if (index.TryPopEnum("Output.Encoding", out OutputEncoding encoding))
            Encoding = encoding;

        if (index.TryPopEnum("Output.Footer", out FooterFormat footer))
            Footer = footer;

        if (index.TryPopEnum("Output.Format", out OutputFormat format))
            Format = format;

        if (index.TryPopString("Output.JobSummaryPath", out var jobSummaryPath))
            JobSummaryPath = jobSummaryPath;

        if (index.TryPopValue<int>("Output.JsonIndent", out var jsonIndent))
            JsonIndent = jsonIndent;

        if (index.TryPopEnum("Output.Outcome", out RuleOutcome outcome))
            Outcome = outcome;

        if (index.TryPopString("Output.Path", out var path))
            Path = path;

        if (index.TryPopEnum("Output.Style", out OutputStyle style))
            Style = style;

        if (index.TryPopBool("Output.SarifProblemsOnly", out var sarifProblemsOnly))
            SarifProblemsOnly = sarifProblemsOnly;
    }
}
