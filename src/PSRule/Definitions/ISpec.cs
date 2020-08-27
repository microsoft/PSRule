// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Host;
using System.Collections.Generic;

namespace PSRule.Definitions
{
    public enum ResourceKind : byte
    {
        None = 0,

        /// <summary>
        /// A baseline resource.
        /// </summary>
        Baseline = 1,
        
        /// <summary>
        /// A module configuration resource.
        /// </summary>
        ModuleConfig = 2
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

    public sealed class ResourceAnnotations : Dictionary<string, object>
    {

    }

    public sealed class ResourceMetadata
    {
        public ResourceMetadata()
        {
            Annotations = new ResourceAnnotations();
        }

        public string Name { get; set; }

        public ResourceAnnotations Annotations { get; set; }
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
        protected Resource(ResourceMetadata metadata)
        {
            Metadata = metadata;
        }

        public ResourceMetadata Metadata { get; }

        public abstract TSpec Spec { get; }
    }

    internal static class ResourceHelper
    {
        private const string ANNOTATION_OBSOLETE = "obsolete";

        internal static bool IsObsolete(ResourceMetadata metadata)
        {
            if (metadata == null || metadata.Annotations == null || !metadata.Annotations.TryGetBool(ANNOTATION_OBSOLETE, out bool? obsolete))
                return false;

            return obsolete.GetValueOrDefault(false);
        }
    }

    public abstract class Spec
    {

    }

    internal static class Specs
    {
        public readonly static ISpecDescriptor[] BuiltinTypes = new ISpecDescriptor[]
        {
            new SpecDescriptor<Baseline, BaselineSpec>("Baseline"),
            new SpecDescriptor<ModuleConfig, ModuleConfigSpec>("ModuleConfig"),
        };
    }
}
