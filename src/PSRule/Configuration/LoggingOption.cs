// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PSRule.Configuration
{
    /// <summary>
    /// Options for logging outcomes to a informational streams.
    /// </summary>
    public sealed class LoggingOption : IEquatable<LoggingOption>
    {
        private const OutcomeLogStream DEFAULT_RULEFAIL = OutcomeLogStream.None;
        private const OutcomeLogStream DEFAULT_RULEPASS = OutcomeLogStream.None;

        internal static readonly LoggingOption Default = new LoggingOption
        {
            RuleFail = DEFAULT_RULEFAIL,
            RulePass = DEFAULT_RULEPASS,
            LimitVerbose = null,
            LimitDebug = null
        };

        public LoggingOption()
        {
            LimitDebug = null;
            LimitVerbose = null;
            RuleFail = null;
            RulePass = null;
        }

        public LoggingOption(LoggingOption option)
        {
            if (option == null)
                return;

            LimitDebug = option.LimitDebug;
            LimitVerbose = option.LimitVerbose;
            RuleFail = option.RuleFail;
            RulePass = option.RulePass;
        }

        public override bool Equals(object obj)
        {
            return obj is LoggingOption option && Equals(option);
        }

        public bool Equals(LoggingOption other)
        {
            return other != null &&
                LimitDebug == other.LimitDebug &&
                LimitVerbose == other.LimitVerbose &&
                RuleFail == other.RuleFail &&
                RulePass == other.RulePass;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine
            {
                int hash = 17;
                hash = hash * 23 + (LimitDebug != null ? LimitDebug.GetHashCode() : 0);
                hash = hash * 23 + (LimitVerbose != null ? LimitVerbose.GetHashCode() : 0);
                hash = hash * 23 + (RuleFail.HasValue ? RuleFail.Value.GetHashCode() : 0);
                hash = hash * 23 + (RulePass.HasValue ? RulePass.Value.GetHashCode() : 0);
                return hash;
            }
        }

        internal static LoggingOption Combine(LoggingOption o1, LoggingOption o2)
        {
            var result = new LoggingOption(o1)
            {
                LimitDebug = o1.LimitDebug ?? o2.LimitDebug,
                LimitVerbose = o1.LimitVerbose ?? o2.LimitVerbose,
                RuleFail = o1.RuleFail ?? o2.RuleFail,
                RulePass = o1.RulePass ?? o2.RulePass
            };
            return result;
        }

        /// <summary>
        /// Limits debug messages to a list of named debug scopes.
        /// </summary>
        [DefaultValue(null)]
        public string[] LimitDebug { get; set; }

        /// <summary>
        /// Limits verbose messages to a list of named verbose scopes.
        /// </summary>
        [DefaultValue(null)]
        public string[] LimitVerbose { get; set; }

        /// <summary>
        /// Log fail outcomes for each rule to a specific informational stream.
        /// </summary>
        [DefaultValue(null)]
        public OutcomeLogStream? RuleFail { get; set; }

        /// <summary>
        /// Log pass outcomes for each rule to a specific informational stream.
        /// </summary>
        [DefaultValue(null)]
        public OutcomeLogStream? RulePass { get; set; }

        internal void Load(EnvironmentHelper env)
        {
            if (env.TryStringArray("PSRULE_LOGGING_LIMITDEBUG", out string[] limitDebug))
                LimitDebug = limitDebug;

            if (env.TryStringArray("PSRULE_LOGGING_LIMITVERBOSE", out string[] limitVerbose))
                LimitVerbose = limitVerbose;

            if (env.TryEnum("PSRULE_LOGGING_RULEFAIL", out OutcomeLogStream ruleFail))
                RuleFail = ruleFail;

            if (env.TryEnum("PSRULE_LOGGING_RULEPASS", out OutcomeLogStream rulePass))
                RulePass = rulePass;
        }

        internal void Load(Dictionary<string, object> index)
        {
            if (index.TryPopStringArray("Logging.LimitDebug", out string[] limitDebug))
                LimitDebug = limitDebug;

            if (index.TryPopStringArray("Logging.LimitVerbose", out string[] limitVerbose))
                LimitVerbose = limitVerbose;

            if (index.TryPopEnum("Logging.RuleFail", out OutcomeLogStream ruleFail))
                RuleFail = ruleFail;

            if (index.TryPopEnum("Logging.RulePass", out OutcomeLogStream rulePass))
                RulePass = rulePass;
        }
    }
}
