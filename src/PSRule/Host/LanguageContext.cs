using PSRule.Rules;
using System;

namespace PSRule.Host
{
    internal sealed class LanguageContext
    {
        [ThreadStatic]
        internal static DetailResult _Rule;
    }
}
