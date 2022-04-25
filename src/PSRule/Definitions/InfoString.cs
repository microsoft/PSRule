// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions
{
    public sealed class InfoString
    {
        private string _Text;
        private string _Markdown;

        internal InfoString() { }

        internal InfoString(string text, string markdown)
        {
            Text = text;
            Markdown = markdown ?? text;
        }

        public bool HasValue
        {
            get { return Text != null || Markdown != null; }
        }

        public string Text
        {
            get { return _Text; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _Text = value;
            }
        }

        public string Markdown
        {
            get { return _Markdown; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _Markdown = value;
            }
        }
    }
}
