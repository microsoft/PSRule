namespace PSRule.Host
{
    internal interface IDependencyTarget
    {
        string RuleId { get; }

        string[] DependsOn { get; }
    }
}