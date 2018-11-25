using PSRule.Commands;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace PSRule.Host
{
    public sealed class EnvironmentVariable : PSVariable
    {
        public EnvironmentVariable(string name)
            : base(name, null, ScopedItemOptions.ReadOnly)
        {

        }

        public override object Value
        {
            get
            {
                //return LanguageContext._Environment;
                return string.Empty;
            }
            set
            {
                // Do nothing
            }
        }
    }

    internal static class HostState
    {

        internal static SessionStateCmdletEntry[] BuiltInCmdlets = new SessionStateCmdletEntry[]
        {
            new SessionStateCmdletEntry("New-RuleDefinition", typeof(NewRuleDefinitionCommand), null),
        };

        public static InitialSessionState CreateDefault()
        {
            var state = InitialSessionState.CreateDefault();

            state.Commands.Add(BuiltInCmdlets);
            state.Commands.Add(BuiltInAliases);
            state.Commands.Add(GetSpec);

            state.Variables.Add(new SessionStateVariableEntry("LanguageContext", new LanguageContext(), string.Empty, ScopedItemOptions.ReadOnly));

            //ResourceCommandBinder.Bind(state);

#if !NET472
            state.ExecutionPolicy = Microsoft.PowerShell.ExecutionPolicy.RemoteSigned;
#endif
            return state;
        }

        internal static SessionStateAliasEntry[] BuiltInAliases
        {
            get
            {
                const ScopedItemOptions ReadOnly = ScopedItemOptions.ReadOnly;

                return new SessionStateAliasEntry[]
                {
                    new SessionStateAliasEntry("rule", "New-RuleDefinition", string.Empty, ReadOnly),
                };
            }
        }

        internal static SessionStateFunctionEntry[] GetSpec
        {
            get
            {
                return new SessionStateFunctionEntry[]
                {
                    new SessionStateFunctionEntry("DOK.TestSpec", "")
                };
            }
        }
    }
}
