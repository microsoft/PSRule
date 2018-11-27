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

    public sealed class RuleVariable : PSVariable
    {
        public RuleVariable(string name)
            : base(name, null, ScopedItemOptions.ReadOnly)
        {

        }

        public override object Value
        {
            get
            {
                return LanguageContext._Rule;
            }
            set
            {

            }
        }
    }

    public sealed class TargetObjectVariable : PSVariable
    {
        public TargetObjectVariable(string name)
            : base (name, null, ScopedItemOptions.ReadOnly)
        {

        }

        public override object Value
        {
            get
            {
                return LanguageContext._Rule?.TargetObject;
            }
            set
            {

            }
        }
    }

    internal static class HostState
    {

        internal static SessionStateCmdletEntry[] BuiltInCmdlets = new SessionStateCmdletEntry[]
        {
            new SessionStateCmdletEntry("New-RuleDefinition", typeof(NewRuleDefinitionCommand), null),
            new SessionStateCmdletEntry("Set-PSRuleHint", typeof(SetPSRuleHintCommand), null),
            new SessionStateCmdletEntry("Assert-Exists", typeof(AssertExistsCommand), null),
            new SessionStateCmdletEntry("Assert-Within", typeof(AssertWithinCommand), null),
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
                    new SessionStateAliasEntry("hint", "Set-PSRuleHint", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("exists", "Assert-Exists", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("within", "Assert-Within", string.Empty, ReadOnly),
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
