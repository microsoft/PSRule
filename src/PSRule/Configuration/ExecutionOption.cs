// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;

namespace PSRule.Configuration
{
    public sealed class ExecutionOption
    {
        private const LanguageMode DEFAULT_LANGUAGEMODE = Configuration.LanguageMode.FullLanguage;
        private const bool DEFAULT_INCONCLUSIVEWARNING = true;
        private const bool DEFAULT_NOTPROCESSEDWARNING = true;

        public static readonly ExecutionOption Default = new ExecutionOption
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
            LanguageMode = option.LanguageMode;
            InconclusiveWarning = option.InconclusiveWarning;
            NotProcessedWarning = option.NotProcessedWarning;
        }

        [DefaultValue(null)]
        public LanguageMode? LanguageMode { get; set; }

        [DefaultValue(null)]
        public bool? InconclusiveWarning { get; set; }

        [DefaultValue(null)]
        public bool? NotProcessedWarning { get; set; }
    }
}
