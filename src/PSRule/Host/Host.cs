using PSRule.Commands;
using PSRule.Pipeline;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace PSRule.Host
{
    /// <summary>
    /// A dynamic variable used during Rule execution.
    /// </summary>
    internal sealed class RuleVariable : PSVariable
    {
        public RuleVariable(string name)
            : base(name, null, ScopedItemOptions.ReadOnly)
        {

        }

        public override object Value
        {
            get
            {
                return PipelineContext.CurrentThread.RuleRecord;
            }
        }
    }

    /// <summary>
    /// A dynamic variable used during Rule execution.
    /// </summary>
    internal sealed class TargetObjectVariable : PSVariable
    {
        public TargetObjectVariable(string name)
            : base (name, null, ScopedItemOptions.ReadOnly)
        {

        }

        public override object Value
        {
            get
            {
                return PipelineContext.CurrentThread.TargetObject;
            }
        }
    }

    internal static class HostState
    {
        /// <summary>
        /// Define language commands.
        /// </summary>
        private static SessionStateCmdletEntry[] BuiltInCmdlets = new SessionStateCmdletEntry[]
        {
            new SessionStateCmdletEntry("New-RuleDefinition", typeof(NewRuleDefinitionCommand), null),
            new SessionStateCmdletEntry("Set-PSRuleHint", typeof(SetPSRuleHintCommand), null),
            new SessionStateCmdletEntry("Assert-Exists", typeof(AssertExistsCommand), null),
            new SessionStateCmdletEntry("Assert-Within", typeof(AssertWithinCommand), null),
            new SessionStateCmdletEntry("Assert-Match", typeof(AssertMatchCommand), null),
            new SessionStateCmdletEntry("Assert-TypeOf", typeof(AssertTypeOfCommand), null),
            new SessionStateCmdletEntry("Assert-AllOf", typeof(AssertAllOfCommand), null),
            new SessionStateCmdletEntry("Assert-AnyOf", typeof(AssertAnyOfCommand), null),
        };

        /// <summary>
        /// Define language aliases.
        /// </summary>
        private static SessionStateAliasEntry[] BuiltInAliases
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
                    new SessionStateAliasEntry("match", "Assert-Match", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("typeof", "Assert-TypeOf", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("allof", "Assert-AllOf", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("anyof", "Assert-AnyOf", string.Empty, ReadOnly),
                };
            }
        }

        /// <summary>
        /// Create a default session state.
        /// </summary>
        /// <returns></returns>
        public static InitialSessionState CreateSessionState()
        {
            var state = InitialSessionState.CreateDefault();

            // Add in language elements
            state.Commands.Add(BuiltInCmdlets);
            state.Commands.Add(BuiltInAliases);

#if !NET472 && Windows
            // Set execution policy
            state.ExecutionPolicy = Microsoft.PowerShell.ExecutionPolicy.RemoteSigned;
#endif
            return state;
        }
    }
}
