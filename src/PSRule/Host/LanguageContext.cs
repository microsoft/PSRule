using PSRule.Rules;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;

namespace PSRule.Host
{
    public sealed class LanguageContext
    {
        [ThreadStatic]
        internal static RuleResult _Rule;

        public LanguageContext()
        {
            Functions = new Dictionary<string, ScriptBlock>();
            Variables = new Dictionary<string, PSVariable>();
        }

        public LanguageContext(IDictionary<string, ScriptBlock> functions, IDictionary<string, PSVariable> variables)
        {
            Functions = new Dictionary<string, ScriptBlock>(functions, System.StringComparer.OrdinalIgnoreCase);
            Variables = new Dictionary<string, PSVariable>(variables, System.StringComparer.OrdinalIgnoreCase);
        }

        public Dictionary<string, ScriptBlock> Functions { get; private set; }

        public Dictionary<string, PSVariable> Variables { get; private set; }

        public LanguageContext New()
        {
            var context = new LanguageContext(Functions, Variables);
            context.SetVariable("LanguageContext", context);

            return context;
        }

        public Collection<PSObject> InvokeWithContext(ScriptBlock scriptBlock, params object[] args)
        {
            return scriptBlock.InvokeWithContext(Functions, Variables.Values.ToList(), args);
        }

        public void SetVariable(string name, object value)
        {
            var psVar = new PSVariable(name, value, ScopedItemOptions.ReadOnly);
            Variables[name] = psVar;
        }
    }
}
