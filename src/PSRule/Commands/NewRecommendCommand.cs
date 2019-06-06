using System.Management.Automation;

namespace PSRule.Commands
{
    /// <summary>
    /// The Recommend keyword.
    /// </summary>
    [Cmdlet(VerbsCommon.New, RuleLanguageNouns.Recommendation)]
    internal sealed class NewRecommendationCommand : RuleKeyword
    {
        [Parameter(Mandatory = false, Position = 0)]
        public string Message { get; set; }

        protected override void ProcessRecord()
        {
            var result = GetResult();

            if (MyInvocation.BoundParameters.ContainsKey(nameof(Message)) && string.IsNullOrEmpty(result.Info.Recommendation))
            {
                result.Info.Recommendation = Message;
            }
        }
    }
}
