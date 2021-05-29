// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Rules;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PSRule.Configuration
{
    /// <summary>
    /// Options for generating and formatting output.
    /// </summary>
    public sealed class OutputOption : IEquatable<OutputOption>
    {
        private const ResultFormat DEFAULT_AS = ResultFormat.Detail;
        private const BannerFormat DEFAULT_BANNER = BannerFormat.Default;
        private const OutputEncoding DEFAULT_ENCODING = OutputEncoding.Default;
        private const OutputFormat DEFAULT_FORMAT = OutputFormat.None;
        private const RuleOutcome DEFAULT_OUTCOME = RuleOutcome.Processed;
        private const OutputStyle DEFAULT_STYLE = OutputStyle.Detect;

        internal static readonly OutputOption Default = new OutputOption
        {
            As = DEFAULT_AS,
            Banner = DEFAULT_BANNER,
            Encoding = DEFAULT_ENCODING,
            Format = DEFAULT_FORMAT,
            Outcome = DEFAULT_OUTCOME,
            Style = DEFAULT_STYLE,
        };

        public OutputOption()
        {
            As = null;
            Banner = null;
            Culture = null;
            Encoding = null;
            Format = null;
            Path = null;
            Style = null;
        }

        public OutputOption(OutputOption option)
        {
            if (option == null)
                return;

            As = option.As;
            Banner = option.Banner;
            Culture = option.Culture;
            Encoding = option.Encoding;
            Format = option.Format;
            Outcome = option.Outcome;
            Path = option.Path;
            Style = option.Style;
        }

        public override bool Equals(object obj)
        {
            return obj is OutputOption option && Equals(option);
        }

        public bool Equals(OutputOption other)
        {
            return other != null &&
                As == other.As &&
                Banner == other.Banner &&
                Culture == other.Culture &&
                Encoding == other.Encoding &&
                Format == other.Format &&
                Outcome == other.Outcome &&
                Path == other.Path &&
                Style == other.Style;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine
            {
                int hash = 17;
                hash = hash * 23 + (As.HasValue ? As.Value.GetHashCode() : 0);
                hash = hash * 23 + (Banner.HasValue ? Banner.Value.GetHashCode() : 0);
                hash = hash * 23 + (Culture != null ? Culture.GetHashCode() : 0);
                hash = hash * 23 + (Encoding.HasValue ? Encoding.Value.GetHashCode() : 0);
                hash = hash * 23 + (Format.HasValue ? Format.Value.GetHashCode() : 0);
                hash = hash * 23 + (Outcome.HasValue ? Outcome.Value.GetHashCode() : 0);
                hash = hash * 23 + (Path != null ? Path.GetHashCode() : 0);
                hash = hash * 23 + (Style.HasValue ? Style.Value.GetHashCode() : 0);
                return hash;
            }
        }

        internal static OutputOption Combine(OutputOption o1, OutputOption o2)
        {
            var result = new OutputOption(o1)
            {
                As = o1.As ?? o2.As,
                Banner = o1.Banner ?? o2.Banner,
                Culture = o1.Culture ?? o2.Culture,
                Encoding = o1.Encoding ?? o2.Encoding,
                Format = o1.Format ?? o2.Format,
                Outcome = o1.Outcome ?? o2.Outcome,
                Path = o1.Path ?? o2.Path,
                Style = o1.Style ?? o2.Style
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
        public string[] Culture { get; set; }

        /// <summary>
        /// The encoding to use when writing results to file.
        /// </summary>
        [DefaultValue(null)]
        public OutputEncoding? Encoding { get; set; }

        /// <summary>
        /// The output format.
        /// </summary>
        [DefaultValue(null)]
        public OutputFormat? Format { get; set; }

        /// <summary>
        /// The outcome of rule results to return.
        /// </summary>
        [DefaultValue(null)]
        public RuleOutcome? Outcome { get; set; }

        /// <summary>
        /// The file path location to save results.
        /// </summary>
        [DefaultValue(null)]
        public string Path { get; set; }

        /// <summary>
        /// The style that results will be presented in.
        /// </summary>
        [DefaultValue(null)]
        public OutputStyle? Style { get; set; }

        internal void Load(EnvironmentHelper env)
        {
            if (env.TryEnum("PSRULE_OUTPUT_AS", out ResultFormat value))
                As = value;

            if (env.TryEnum("PSRULE_OUTPUT_BANNER", out BannerFormat banner))
                Banner = banner;

            if (env.TryStringArray("PSRULE_OUTPUT_CULTURE", out string[] culture))
                Culture = culture;

            if (env.TryEnum("PSRULE_OUTPUT_ENCODING", out OutputEncoding encoding))
                Encoding = encoding;

            if (env.TryEnum("PSRULE_OUTPUT_FORMAT", out OutputFormat format))
                Format = format;

            if (env.TryEnum("PSRULE_OUTPUT_OUTCOME", out RuleOutcome outcome))
                Outcome = outcome;

            if (env.TryString("PSRULE_OUTPUT_PATH", out string path))
                Path = path;

            if (env.TryEnum("PSRULE_OUTPUT_STYLE", out OutputStyle style))
                Style = style;
        }

        internal void Load(Dictionary<string, object> index)
        {
            if (index.TryPopEnum("Output.As", out ResultFormat value))
                As = value;

            if (index.TryPopEnum("Output.Banner", out BannerFormat banner))
                Banner = banner;

            if (index.TryPopStringArray("Output.Culture", out string[] culture))
                Culture = culture;

            if (index.TryPopEnum("Output.Encoding", out OutputEncoding encoding))
                Encoding = encoding;

            if (index.TryPopEnum("Output.Format", out OutputFormat format))
                Format = format;

            if (index.TryPopEnum("Output.Outcome", out RuleOutcome outcome))
                Outcome = outcome;

            if (index.TryPopString("Output.Path", out string path))
                Path = path;

            if (index.TryPopEnum("Output.Style", out OutputStyle style))
                Style = style;
        }
    }
}
