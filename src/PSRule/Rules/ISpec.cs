using PSRule.Host;

namespace PSRule.Rules
{
    public enum ResourceKind : byte
    {
        None = 0,

        Baseline = 1
    }

    internal interface IResource : ILanguageBlock
    {
        ResourceKind Kind { get; }

        string Id { get; }

        string Name { get; }
    }

    public sealed class ResourceObject
    {
        internal ResourceObject(IResource block)
        {
            Block = block;
        }

        internal IResource Block { get; }
    }

    public sealed class ResourceMetadata
    {
        public string Name { get; set; }
    }

    public sealed class ResourceExtent
    {
        public string File { get; set; }

        public string Module { get; set; }
    }

    public sealed class ResourceHelpInfo
    {
        internal ResourceHelpInfo(string synopsis)
        {
            Synopsis = synopsis;
        }

        public string Synopsis { get; private set; }
    }

    public abstract class Resource<TSpec> where TSpec : Spec, new()
    {
        public abstract TSpec Spec { get; }
    }

    public abstract class Spec
    {

    }

    internal static class Specs
    {
        public readonly static ISpecDescriptor[] BuiltinTypes = new ISpecDescriptor[]
        {
            new SpecDescriptor<Baseline, BaselineSpec>("Baseline"),
        };
    }
}
