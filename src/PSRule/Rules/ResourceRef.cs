namespace PSRule.Rules
{
    internal abstract class ResourceRef
    {
        public readonly string Id;
        public readonly ResourceKind Kind;

        public ResourceRef(string id, ResourceKind kind)
        {
            Kind = kind;
            Id = id;
        }
    }
}
