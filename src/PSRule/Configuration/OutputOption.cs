// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Rules;
using System;
using System.ComponentModel;

namespace PSRule.Configuration
{
    /// <summary>
    /// Options for generating and formatting output.
    /// </summary>
    public sealed class OutputOption : IEquatable<OutputOption>
    {
        private const ResultFormat DEFAULT_AS = ResultFormat.Detail;
        private const OutputEncoding DEFAULT_ENCODING = OutputEncoding.Default;
        private const OutputFormat DEFAULT_FORMAT = OutputFormat.None;
        private const RuleOutcome DEFAULT_OUTCOME = RuleOutcome.Processed;
        private const OutputStyle DEFAULT_STYLE = OutputStyle.Client;

        internal static readonly OutputOption Default = new OutputOption
        {
            As = DEFAULT_AS,
            Encoding = DEFAULT_ENCODING,
            Format = DEFAULT_FORMAT,
            Outcome = DEFAULT_OUTCOME,
            Style = DEFAULT_STYLE,
        };

        public OutputOption()
        {
            As = null;
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
            var result = new OutputOption(o1);
            result.As = o1.As ?? o2.As;
            result.Culture = o1.Culture ?? o2.Culture;
            result.Encoding = o1.Encoding ?? o2.Encoding;
            result.Format = o1.Format ?? o2.Format;
            result.Outcome = o1.Outcome ?? o2.Outcome;
            result.Path = o1.Path ?? o2.Path;
            result.Style = o1.Style ?? o2.Style;
            return result;
        }

        /// <summary>
        /// The type of result to produce.
        /// </summary>
        [DefaultValue(null)]
        public ResultFormat? As { get; set; }

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

        [DefaultValue(null)]
        public RuleOutcome? Outcome { get; set; }

        /// <summary>
        /// The file path location to save results.
        /// </summary>
        [DefaultValue(null)]
        public string Path { get; set; }

        [DefaultValue(null)]
        public OutputStyle? Style { get; set; }
    }
}
