namespace PSRule.Rules
{
    internal interface IResourceFilter
    {
        bool Match(string name, TagSet tag);
    }
}
