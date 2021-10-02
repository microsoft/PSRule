// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Host;
using PSRule.Pipeline;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        Convention = 5
    }

    internal interface IResource : ILanguageBlock
    {
        ResourceKind Kind { get; }

        string ApiVersion { get; }

        string Name { get; }

        ResourceTags Tags { get; }
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

    internal abstract class ResourceAnnotation
    {

    }

    internal sealed class ValidateResourceAnnotation : ResourceAnnotation
    {
        public bool ApiVersionNotSet { get; internal set; }
    }

    public sealed class ResourceObject
    {
        internal ResourceObject(IResource block)
        {
            Block = block;
        }

        internal IResource Block { get; }
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
                    inner => new LanguageBlockDeserializer(new LanguageExpressionDeserializer(inner)),
                    s => s.InsteadOf<ObjectNodeDeserializer>())
                .Build();
        }

        internal void FromFile(SourceFile file)
        {
            using (var reader = new StreamReader(file.Path))
            {
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
            return new Hashtable(this, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Check if a specific resource tag exists.
        /// </summary>
        internal bool Contains(object key, object value)
        {
            if (key == null || value == null || !(key is string k) || !ContainsKey(k))
                return false;

            if (TryArray(value, out string[] values))
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

        public string Name { get; set; }

        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
        public ResourceAnnotations Annotations { get; set; }

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

    public abstract class Resource<TSpec> where TSpec : Spec, new()
    {
        protected Resource(ResourceKind kind, string apiVersion, SourceFile source, ResourceMetadata metadata, ResourceHelpInfo info, TSpec spec)
        {
            Kind = kind;
            ApiVersion = apiVersion;
            Info = info;
            Source = source;
            Spec = spec;
            Id = ResourceHelper.GetId(source.ModuleName, metadata.Name);
            Metadata = metadata;
            Name = metadata.Name;
        }

        [YamlIgnore()]
        public readonly string Id;

        [YamlIgnore()]
        public string Name { get; }

        /// <summary>
        /// The file path where the resource is defined.
        /// </summary>
        [YamlIgnore()]
        public readonly SourceFile Source;

        [YamlIgnore()]
        public readonly ResourceHelpInfo Info;

        public ResourceMetadata Metadata { get; }

        public ResourceKind Kind { get; }

        public string ApiVersion { get; }

        public TSpec Spec { get; }
    }

    public abstract class InternalResource<TSpec> : Resource<TSpec>, IResource, IAnnotated<ResourceAnnotation> where TSpec : Spec, new()
    {
        private readonly Dictionary<Type, ResourceAnnotation> _Annotations;

        internal InternalResource(ResourceKind kind, string apiVersion, SourceFile source, ResourceMetadata metadata, ResourceHelpInfo info, TSpec spec)
            : base(kind, apiVersion, source, metadata, info, spec)
        {
            _Annotations = new Dictionary<Type, ResourceAnnotation>();
            Obsolete = ResourceHelper.IsObsolete(metadata);
        }

        [YamlIgnore()]
        internal readonly bool Obsolete;

        string ILanguageBlock.Id => Id;

        string ILanguageBlock.SourcePath => Source.Path;

        string ILanguageBlock.Module => Source.ModuleName;

        ResourceKind IResource.Kind => Kind;

        string IResource.ApiVersion => ApiVersion;

        string IResource.Name => Name;

        ResourceTags IResource.Tags => Metadata.Tags;

        TAnnotation IAnnotated<ResourceAnnotation>.GetAnnotation<TAnnotation>()
        {
            return _Annotations.TryGetValue(typeof(TAnnotation), out ResourceAnnotation annotation) ? (TAnnotation)annotation : null;
        }

        void IAnnotated<ResourceAnnotation>.SetAnnotation<TAnnotation>(TAnnotation annotation)
        {
            _Annotations[typeof(TAnnotation)] = annotation;
        }
    }

    internal static class ResourceHelper
    {
        private const string ANNOTATION_OBSOLETE = "obsolete";

        private const string LooseModuleName = ".";
        private const char ModuleSeparator = '\\';

        internal static string GetId(string moduleName, string name)
        {
            if (name.IndexOf(ModuleSeparator) >= 0)
                return name;

            return string.Concat(string.IsNullOrEmpty(moduleName) ? LooseModuleName : moduleName, ModuleSeparator, name);
        }

        internal static bool IsObsolete(ResourceMetadata metadata)
        {
            if (metadata == null || metadata.Annotations == null || !metadata.Annotations.TryGetBool(ANNOTATION_OBSOLETE, out bool? obsolete))
                return false;

            return obsolete.GetValueOrDefault(false);
        }
    }
}
