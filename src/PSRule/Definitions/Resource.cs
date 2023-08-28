// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Diagnostics;
using System.Text;
using PSRule.Converters.Yaml;
using PSRule.Definitions.Rules;
using PSRule.Pipeline;
using PSRule.Runtime;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.NodeDeserializers;

namespace PSRule.Definitions
{
    /// <summary>
    /// The type of resource.
    /// </summary>
    public enum ResourceKind
    {
        /// <summary>
        /// Unknown or empty.
        /// </summary>
        None = 0,

        /// <summary>
        /// A rule resource.
        /// </summary>
        Rule = 1,

        /// <summary>
        /// A baseline resource.
        /// </summary>
        Baseline = 2,

        /// <summary>
        /// A module configuration resource.
        /// </summary>
        ModuleConfig = 3,

        /// <summary>
        /// A selector resource.
        /// </summary>
        Selector = 4,

        /// <summary>
        /// A convention.
        /// </summary>
        Convention = 5,

        /// <summary>
        /// A suppression group.
        /// </summary>
        SuppressionGroup = 6
    }

    /// <summary>
    /// Additional flags that indicate the status of the resource.
    /// </summary>
    [Flags]
    public enum ResourceFlags
    {
        /// <summary>
        /// No flags are set.
        /// </summary>
        None = 0,

        /// <summary>
        /// The resource is obsolete.
        /// </summary>
        Obsolete = 1
    }

    /// <summary>
    /// A resource langange block.
    /// </summary>
    public interface IResource : ILanguageBlock
    {
        /// <summary>
        /// The type of resource.
        /// </summary>
        ResourceKind Kind { get; }

        /// <summary>
        /// The API version of the resource.
        /// </summary>
        string ApiVersion { get; }

        /// <summary>
        /// The name of the resource.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// An optional reference identifer for the resource.
        /// </summary>
        ResourceId? Ref { get; }

        /// <summary>
        /// Any additional aliases for the resource.
        /// </summary>
        ResourceId[] Alias { get; }

        /// <summary>
        /// Any resource tags.
        /// </summary>
        ResourceTags Tags { get; }

        /// <summary>
        /// Any taxonomy references.
        /// </summary>
        ResourceLabels Labels { get; }

        /// <summary>
        /// Flags for the resource.
        /// </summary>
        ResourceFlags Flags { get; }

        /// <summary>
        /// The source location of the resource.
        /// </summary>
        ISourceExtent Extent { get; }

        /// <summary>
        /// Additional information about the resource.
        /// </summary>
        IResourceHelpInfo Info { get; }
    }

    internal interface IResourceVisitor
    {
        bool Visit(IResource resource);
    }

    internal abstract class ResourceRef
    {
        public readonly string Id;
        public readonly ResourceKind Kind;

        protected ResourceRef(string id, ResourceKind kind)
        {
            Kind = kind;
            Id = id;
        }
    }

    /// <summary>
    /// A base resource annotation.
    /// </summary>
    internal abstract class ResourceAnnotation
    {

    }

    /// <summary>
    /// Annotation used to flag validation issues.
    /// </summary>
    internal sealed class ValidateResourceAnnotation : ResourceAnnotation
    {

    }

    /// <summary>
    /// A resource object.
    /// </summary>
    public sealed class ResourceObject
    {
        internal ResourceObject(IResource block)
        {
            Block = block;
        }

        internal IResource Block { get; }

        internal bool Visit(IResourceVisitor visitor)
        {
            return Block != null && visitor != null && visitor.Visit(Block);
        }
    }

    internal sealed class ResourceBuilder
    {
        private readonly List<ILanguageBlock> _Output;
        private readonly IDeserializer _Deserializer;

        internal ResourceBuilder()
        {
            _Output = new List<ILanguageBlock>();
            _Deserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeConverter(new FieldMapYamlTypeConverter())
                .WithTypeConverter(new StringArrayMapConverter())
                .WithTypeConverter(new StringArrayConverter())
                .WithNodeDeserializer(
                    inner => new ResourceNodeDeserializer(new LanguageExpressionDeserializer(inner)),
                    s => s.InsteadOf<ObjectNodeDeserializer>())
                .Build();
        }

        internal void FromFile(SourceFile file)
        {
            using var reader = new StreamReader(file.Path);
            var parser = new YamlDotNet.Core.Parser(reader);
            parser.TryConsume<StreamStart>(out _);
            while (parser.Current is DocumentStart)
            {
                var item = _Deserializer.Deserialize<ResourceObject>(parser: parser);
                if (item == null || item.Block == null)
                    continue;

                _Output.Add(item.Block);
            }
        }

        internal IEnumerable<ILanguageBlock> Build()
        {
            return _Output.Count == 0 ? Array.Empty<ILanguageBlock>() : _Output.ToArray();
        }
    }

    /// <summary>
    /// Additional resource annotations.
    /// </summary>
    public sealed class ResourceAnnotations : Dictionary<string, object>
    {

    }

    /// <summary>
    /// Additional resource taxonomy references.
    /// </summary>
    public sealed class ResourceLabels : Dictionary<string, string[]>
    {
        /// <summary>
        /// Create an empty set of resource labels.
        /// </summary>
        public ResourceLabels() : base(StringComparer.OrdinalIgnoreCase) { }

        /// <summary>
        /// Convert from a hashtable to resource labels.
        /// </summary>
        internal static ResourceLabels FromHashtable(Hashtable hashtable)
        {
            if (hashtable == null || hashtable.Count == 0)
                return null;

            var annotations = new ResourceLabels();
            foreach (DictionaryEntry kv in hashtable)
            {
                var key = kv.Key.ToString();
                if (hashtable.TryGetStringArray(key, out var value))
                    annotations[key] = value;
            }
            return annotations;
        }

        internal bool Contains(string key, string[] value)
        {
            if (!TryGetValue(key, out var actual))
                return false;

            if (value == null || value.Length == 0 || (value.Length == 1 && value[0] == "*"))
                return true;

            for (var i = 0; i < value.Length; i++)
            {
                if (Array.IndexOf(actual, value[i]) != -1)
                    return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Additional resource tags.
    /// </summary>
    public sealed class ResourceTags : Dictionary<string, string>
    {
        private Hashtable _Hashtable;

        /// <summary>
        /// Create an empty set of resource tags.
        /// </summary>
        public ResourceTags() : base(StringComparer.OrdinalIgnoreCase) { }

        /// <summary>
        /// Convert from a hashtable to resource tags.
        /// </summary>
        internal static ResourceTags FromHashtable(Hashtable hashtable)
        {
            if (hashtable == null || hashtable.Count == 0)
                return null;

            var tags = new ResourceTags();
            foreach (DictionaryEntry kv in hashtable)
                tags[kv.Key.ToString()] = kv.Value.ToString();

            return tags;
        }

        /// <summary>
        /// Convert from a dictionary of string pairs to resource tags.
        /// </summary>
        internal static ResourceTags FromDictionary(Dictionary<string, string> dictionary)
        {
            if (dictionary == null)
                return null;

            var tags = new ResourceTags();
            foreach (var kv in dictionary)
                tags[kv.Key] = kv.Value;

            return tags;
        }

        /// <summary>
        /// Convert resource tags to a hashtable.
        /// </summary>
        public Hashtable ToHashtable()
        {
            _Hashtable ??= new ReadOnlyHashtable(this, StringComparer.OrdinalIgnoreCase);
            return _Hashtable;
        }

        /// <summary>
        /// Check if a specific resource tag exists.
        /// </summary>
        internal bool Contains(object key, object value)
        {
            if (key == null || value == null || key is not string k || !ContainsKey(k))
                return false;

            if (TryArray(value, out var values))
            {
                for (var i = 0; i < values.Length; i++)
                {
                    if (Comparer.Equals(values[i], this[k]))
                        return true;
                }
                return false;
            }
            var v = value.ToString();
            return v == "*" || Comparer.Equals(v, this[k]);
        }

        private static bool TryArray(object o, out string[] values)
        {
            values = null;
            if (o is string[] sArray)
            {
                values = sArray;
                return true;
            }
            if (o is IEnumerable<object> oValues)
            {
                var result = new List<string>();
                foreach (var obj in oValues)
                    result.Add(obj.ToString());

                values = result.ToArray();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Convert the resourecs tags to a display string for PowerShell views.
        /// </summary>
        /// <returns></returns>
        public string ToViewString()
        {
            var sb = new StringBuilder();
            var i = 0;

            foreach (var kv in this)
            {
                if (i > 0)
                    sb.Append(System.Environment.NewLine);

                sb.Append(kv.Key);
                sb.Append('=');
                sb.Append('\'');
                sb.Append(kv.Value);
                sb.Append('\'');
                i++;
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// Additional resource metadata.
    /// </summary>
    public sealed class ResourceMetadata
    {
        /// <summary>
        /// Create an empty set of metadata.
        /// </summary>
        public ResourceMetadata()
        {
            Annotations = new ResourceAnnotations();
            Tags = new ResourceTags();
            Labels = new ResourceLabels();
        }

        /// <summary>
        /// The name of the resource.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A non-localized display name for the resource.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// A non-localized description of the resource.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// A opaque reference for the resource.
        /// </summary>
        public string Ref { get; set; }

        /// <summary>
        /// Additional aliases for the resource.
        /// </summary>
        public string[] Alias { get; set; }

        /// <summary>
        /// Any resource annotations.
        /// </summary>
        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
        public ResourceAnnotations Annotations { get; set; }

        /// <summary>
        /// Any resource tags.
        /// </summary>
        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
        public ResourceTags Tags { get; set; }

        /// <summary>
        /// Any taxonomy references.
        /// </summary>
        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
        public ResourceLabels Labels { get; set; }

        /// <summary>
        /// A URL to documentation for the resource.
        /// </summary>
        public string Link { get; set; }
    }

    /// <summary>
    /// The source location of the resource.
    /// </summary>
    public sealed class ResourceExtent
    {
        /// <summary>
        /// The file where the resource is located.
        /// </summary>
        public string File { get; set; }

        /// <summary>
        /// The name of the module if the resource is contained within a module.
        /// </summary>
        public string Module { get; set; }
    }

    /// <summary>
    /// A base class for resources.
    /// </summary>
    /// <typeparam name="TSpec">The type for the resource specification.</typeparam>
    [DebuggerDisplay("Kind = {Kind}, Id = {Id}")]
    public abstract class Resource<TSpec> where TSpec : Spec, new()
    {
        /// <summary>
        /// Create a resource.
        /// </summary>
        protected internal Resource(ResourceKind kind, string apiVersion, SourceFile source, ResourceMetadata metadata, IResourceHelpInfo info, ISourceExtent extent, TSpec spec)
        {
            Kind = kind;
            ApiVersion = apiVersion;
            Info = info;
            Source = source;
            Extent = extent;
            Spec = spec;
            Metadata = metadata;
            Name = metadata.Name;
            Id = new ResourceId(source.Module, Name, ResourceIdKind.Id);
        }

        /// <summary>
        /// The resource identifier for the resource.
        /// </summary>
        [YamlIgnore()]
        public ResourceId Id { get; }

        /// <summary>
        /// The name of the resource.
        /// </summary>
        [YamlIgnore()]
        public string Name { get; }

        /// <summary>
        /// The name of the module where the resource is defined.
        /// </summary>
        [YamlIgnore()]
        public string Module => Source.Module;

        /// <summary>
        /// The file path where the resource is defined.
        /// </summary>
        [YamlIgnore()]
        public SourceFile Source { get; }

        /// <summary>
        /// Information about the resource.
        /// </summary>
        [YamlIgnore()]
        public IResourceHelpInfo Info { get; }

        /// <summary>
        /// Resource metadata details.
        /// </summary>
        public ResourceMetadata Metadata { get; }

        /// <summary>
        /// The type of resource.
        /// </summary>
        public ResourceKind Kind { get; }

        /// <summary>
        /// The API version of the resource.
        /// </summary>
        public string ApiVersion { get; }

        /// <summary>
        /// The child specification of the resource.
        /// </summary>
        public TSpec Spec { get; }

        /// <summary>
        /// The source location of the resource.
        /// </summary>
        public ISourceExtent Extent { get; }
    }

    /// <summary>
    /// A base class for built-in resource types.
    /// </summary>
    /// <typeparam name="TSpec">The type of the related <seealso cref="Spec"/> for the resource.</typeparam>
    public abstract class InternalResource<TSpec> : Resource<TSpec>, IResource, IAnnotated<ResourceAnnotation> where TSpec : Spec, new()
    {
        private readonly Dictionary<Type, ResourceAnnotation> _Annotations;

        private protected InternalResource(ResourceKind kind, string apiVersion, SourceFile source, ResourceMetadata metadata, IResourceHelpInfo info, ISourceExtent extent, TSpec spec)
            : base(kind, apiVersion, source, metadata, info, extent, spec)
        {
            _Annotations = new Dictionary<Type, ResourceAnnotation>();
            Obsolete = ResourceHelper.IsObsolete(metadata);
            Flags |= ResourceHelper.IsObsolete(metadata) ? ResourceFlags.Obsolete : ResourceFlags.None;
        }

        [YamlIgnore()]
        internal readonly bool Obsolete;

        [YamlIgnore()]
        internal ResourceFlags Flags { get; }

        ResourceKind IResource.Kind => Kind;

        string IResource.ApiVersion => ApiVersion;

        string IResource.Name => Name;

        // Not supported with base resources.
        ResourceId? IResource.Ref => null;

        // Not supported with base resources.
        ResourceId[] IResource.Alias => null;

        ResourceTags IResource.Tags => Metadata.Tags;

        ResourceLabels IResource.Labels => Metadata.Labels;

        ResourceFlags IResource.Flags => Flags;

        TAnnotation IAnnotated<ResourceAnnotation>.GetAnnotation<TAnnotation>()
        {
            return _Annotations.TryGetValue(typeof(TAnnotation), out var annotation) ? (TAnnotation)annotation : null;
        }

        void IAnnotated<ResourceAnnotation>.SetAnnotation<TAnnotation>(TAnnotation annotation)
        {
            _Annotations[typeof(TAnnotation)] = annotation;
        }
    }

    internal static class ResourceHelper
    {
        private const string ANNOTATION_OBSOLETE = "obsolete";

        private const char SCOPE_SEPARATOR = '\\';

        internal static string GetIdString(string scope, string name)
        {
            return name.IndexOf(SCOPE_SEPARATOR) >= 0
                ? name
                : string.Concat(
                LanguageScope.Normalize(scope),
                SCOPE_SEPARATOR,
                name
            );
        }

        internal static void ParseIdString(string defaultScope, string id, out string scope, out string name)
        {
            ParseIdString(id, out scope, out name);
            scope ??= LanguageScope.Normalize(defaultScope);
        }

        internal static void ParseIdString(string id, out string scope, out string name)
        {
            scope = null;
            name = null;
            if (string.IsNullOrEmpty(id))
                return;

            var scopeSeparator = id.IndexOf(SCOPE_SEPARATOR);
            scope = scopeSeparator >= 0 ? id.Substring(0, scopeSeparator) : null;
            name = id.Substring(scopeSeparator + 1);
        }

        /// <summary>
        /// Checks each resource name and converts each into a full qualified <seealso cref="ResourceId"/>.
        /// </summary>
        /// <param name="defaultScope">The default scope to use if the resource name if not fully qualified.</param>
        /// <param name="name">An array of names. Qualified names (RuleIds) supplied are left intact.</param>
        /// <param name="kind">The <seealso cref="ResourceIdKind"/> of the <seealso cref="ResourceId"/>.</param>
        /// <returns>An array of RuleIds.</returns>
        internal static ResourceId[] GetRuleId(string defaultScope, string[] name, ResourceIdKind kind)
        {
            if (name == null || name.Length == 0)
                return null;

            var result = new ResourceId[name.Length];
            for (var i = 0; i < name.Length; i++)
                result[i] = GetRuleId(defaultScope, name[i], kind);

            return (result.Length == 0) ? null : result;
        }

        internal static ResourceId GetRuleId(string defaultScope, string name, ResourceIdKind kind)
        {
            return name.IndexOf(SCOPE_SEPARATOR) > 0 ? ResourceId.Parse(name, kind) : new ResourceId(defaultScope, name, kind);
        }

        internal static ResourceId? GetIdNullable(string scope, string name, ResourceIdKind kind)
        {
            return string.IsNullOrEmpty(name) ? null : new ResourceId(scope, name, kind);
        }

        internal static bool IsObsolete(ResourceMetadata metadata)
        {
            return metadata != null &&
                metadata.Annotations != null &&
                metadata.Annotations.TryGetBool(ANNOTATION_OBSOLETE, out var obsolete)
                && obsolete.GetValueOrDefault(false);
        }

        internal static SeverityLevel GetLevel(SeverityLevel? level)
        {
            return !level.HasValue || level.Value == SeverityLevel.None ? RuleV1.DEFAULT_LEVEL : level.Value;
        }
    }
}
