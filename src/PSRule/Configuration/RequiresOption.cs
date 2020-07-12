// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace PSRule.Configuration
{
    public sealed class RequiresOption : KeyMapDictionary<string>
    {
        private const string KEYMAP_PREFIX = "Requires.";

        public RequiresOption()
            : base() { }

        internal RequiresOption(RequiresOption option)
            : base(option) { }

        internal void Load(IDictionary<string, object> dictionary)
        {
            base.Load(KEYMAP_PREFIX, dictionary);
        }
    }
}
