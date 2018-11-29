using System.ComponentModel;

namespace PSRule.Configuration
{
    public sealed class ExecutionOption
    {
        private const LanguageMode DEFAULT_LANGUAGEMODE = LanguageMode.FullLanguage;

        public ExecutionOption()
        {
            LanguageMode = DEFAULT_LANGUAGEMODE;
        }

        [DefaultValue(DEFAULT_LANGUAGEMODE)]
        public LanguageMode LanguageMode { get; set; }
    }
}
