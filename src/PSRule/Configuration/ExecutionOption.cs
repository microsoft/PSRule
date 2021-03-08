// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.ComponentModel;

namespace PSRule.Configuration
{
    public sealed class ExecutionOption : IEquatable<ExecutionOption>
    {
        private const LanguageMode DEFAULT_LANGUAGEMODE = Configuration.LanguageMode.FullLanguage;
        private const bool DEFAULT_INCONCLUSIVEWARNING = true;
        private const bool DEFAULT_NOTPROCESSEDWARNING = true;

        internal static readonly ExecutionOption Default = new ExecutionOption
        {
            LanguageMode = DEFAULT_LANGUAGEMODE,
            InconclusiveWarning = DEFAULT_INCONCLUSIVEWARNING,
            NotProcessedWarning = DEFAULT_NOTPROCESSEDWARNING
        };

        public ExecutionOption()
        {
            LanguageMode = null;
            InconclusiveWarning = null;
            NotProcessedWarning = null;
        }

        public ExecutionOption(ExecutionOption option)
        {
            if (option == null)
                return;

            LanguageMode = option.LanguageMode;
            InconclusiveWarning = option.InconclusiveWarning;
            NotProcessedWarning = option.NotProcessedWarning;
        }

        public override bool Equals(object obj)
        {
            return obj is ExecutionOption option && Equals(option);
        }

        public bool Equals(ExecutionOption other)
        {
            return other != null &&
                LanguageMode == other.LanguageMode &&
                InconclusiveWarning == other.InconclusiveWarning &&
                InconclusiveWarning == other.InconclusiveWarning;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine
            {
                int hash = 17;
                hash = hash * 23 + (LanguageMode.HasValue ? LanguageMode.Value.GetHashCode() : 0);
                hash = hash * 23 + (InconclusiveWarning.HasValue ? InconclusiveWarning.Value.GetHashCode() : 0);
                hash = hash * 23 + (NotProcessedWarning.HasValue ? NotProcessedWarning.Value.GetHashCode() : 0);
                return hash;
            }
        }

        internal static ExecutionOption Combine(ExecutionOption o1, ExecutionOption o2)
        {
            var result = new ExecutionOption(o1)
            {
                LanguageMode = o1.LanguageMode ?? o2.LanguageMode,
                InconclusiveWarning = o1.InconclusiveWarning ?? o2.InconclusiveWarning,
                NotProcessedWarning = o1.NotProcessedWarning ?? o2.NotProcessedWarning
            };
            return result;
        }

        [DefaultValue(null)]
        public LanguageMode? LanguageMode { get; set; }

        [DefaultValue(null)]
        public bool? InconclusiveWarning { get; set; }

        [DefaultValue(null)]
        public bool? NotProcessedWarning { get; set; }
    }
}
