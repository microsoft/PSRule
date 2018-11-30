namespace PSRule.Host
{
    internal interface IDependencyTarget
    {
        string Id { get; }

        string[] DependsOn { get; }
    }
}