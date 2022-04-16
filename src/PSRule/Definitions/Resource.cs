// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
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
    public enum ResourceKind
    {
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

    [Flags]
    public enum ResourceFlags
    {
        None = 0,

        Obsolete = 1
    }

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
        /// Flags for the resource.
        /// </summary>
        ResourceFlags Flags { get; }

        /// <summary>
        /// The source location of the resource.
        /// </summary>
        ISourceExtent Extent { get; }
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

    public sealed class ResourceAnnotations : Dictionary<string, object>
    {

    }

    public sealed class ResourceTags : Dictionary<string, string>
    {
        private Hashtable _Hashtable;

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
            if (key == null || value == null || !(key is string k) || !ContainsKey(k))
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

        public string ToViewString()
        {
            var sb = new StringBuilder();
            var i = 0;

            foreach (var kv in this)
            {
                if (i > 0)
                    sb.Append(Environment.NewLine);

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

    public sealed class ResourceMetadata
    {
        public ResourceMetadata()
        {
            Annotations = new ResourceAnnotations();
            Tags = new ResourceTags();
        }

        /// <summary>
        /// The name of the resource.
        /// </summary>
        public string Name { get; set; }

        public string Ref { get; set; }

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

    [DebuggerDisplay("Kind = {Kind}, Id = {Id}")]
    public abstract class Resource<TSpec> where TSpec : Spec, new()
    {
        protected Resource(ResourceKind kind, string apiVersion, SourceFile source, ResourceMetadata metadata, ResourceHelpInfo info, ISourceExtent extent, TSpec spec)
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

        [YamlIgnore()]
        public ResourceId Id { get; }

        /// <summary>
        /// The name of the resource.
        /// </summary>
        [YamlIgnore()]
        public string Name { get; }

        /// <summary>
        /// The file path where the resource is defined.
        /// </summary>
        [YamlIgnore()]
        public SourceFile Source { get; }

        [YamlIgnore()]
        public readonly ResourceHelpInfo Info;

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

    public abstract class InternalResource<TSpec> : Resource<TSpec>, IResource, IAnnotated<ResourceAnnotation> where TSpec : Spec, new()
    {
        private readonly Dictionary<Type, ResourceAnnotation> _Annotations;

        private protected InternalResource(ResourceKind kind, string apiVersion, SourceFile source, ResourceMetadata metadata, ResourceHelpInfo info, ISourceExtent extent, TSpec spec)
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

        string ILanguageBlock.SourcePath => Source.Path;

        string ILanguageBlock.Module => Source.Module;

        ResourceKind IResource.Kind => Kind;

        string IResource.ApiVersion => ApiVersion;

        string IResource.Name => Name;

        // Not supported with base resources.
        ResourceId? IResource.Ref => null;

        // Not supported with base resources.
        ResourceId[] IResource.Alias => null;

        ResourceTags IResource.Tags => Metadata.Tags;

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
        /// Checks each RuleName and converts each to a RuleId.
        /// </summary>
        /// <param name="name">An array of names. Qualified names (RuleIds) supplied are left intact.</param>
        /// <returns>An array of RuleIds.</returns>
        internal static ResourceId[] GetRuleId(string defaultScope, string[] name, ResourceIdKind kind)
        {
            if (name == null)
                return null;

            var result = new ResourceId[name.Length];
            for (var i = 0; i < name.Length; i++)
                result[i] = name[i].IndexOf(SCOPE_SEPARATOR) > 0 ? ResourceId.Parse(name[i], kind) : new ResourceId(defaultScope, name[i], kind);

            return (result.Length == 0) ? null : result;
        }

        internal static ResourceId? GetIdNullable(string scope, string name, ResourceIdKind kind)
        {
            return string.IsNullOrEmpty(name) ? null : (ResourceId?)new ResourceId(scope, name, kind);
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
