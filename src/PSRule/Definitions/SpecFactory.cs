// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using PSRule.Annotations;
using PSRule.Pipeline;

namespace PSRule.Definitions
{
    internal sealed class SpecFactory
    {
        private readonly Dictionary<string, ISpecDescriptor> _Descriptors;

        public SpecFactory()
        {
            _Descriptors = new Dictionary<string, ISpecDescriptor>();
            foreach (var d in Specs.BuiltinTypes)
                With(d);
        }

        public bool TryDescriptor(string apiVersion, string name, out ISpecDescriptor descriptor)
        {
            var fullName = Spec.GetFullName(apiVersion, name);
            return _Descriptors.TryGetValue(fullName, out descriptor);
        }

        public void With<T, TSpec>(string name, string apiVersion) where T : Resource<TSpec>, IResource where TSpec : Spec, new()
        {
            var descriptor = new SpecDescriptor<T, TSpec>(name, apiVersion);
            _Descriptors.Add(descriptor.FullName, descriptor);
        }

        private void With(ISpecDescriptor descriptor)
        {
            _Descriptors.Add(descriptor.FullName, descriptor);
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    internal sealed class SpecAttribute : Attribute
    {
        public SpecAttribute()
        {

        }

        public SpecAttribute(string apiVersion, string kind)
        {
            ApiVersion = apiVersion;
            Kind = kind;
        }

        public string ApiVersion { get; }

        public string Kind { get; }
    }

    internal sealed class SpecDescriptor<T, TSpec> : ISpecDescriptor where T : Resource<TSpec>, IResource where TSpec : Spec, new()
    {
        public SpecDescriptor(string apiVersion, string name)
        {
            ApiVersion = apiVersion;
            Name = name;
            FullName = Spec.GetFullName(apiVersion, name);
        }

        public string Name { get; }

        public string ApiVersion { get; }

        public string FullName { get; }

        public Type SpecType => typeof(TSpec);

        public IResource CreateInstance(SourceFile source, ResourceMetadata metadata, CommentMetadata comment, ISourceExtent extent, object spec)
        {
            var info = new ResourceHelpInfo(metadata.Name)
            {
                Synopsis = new InfoString(comment.Synopsis, null)
            };
            return (IResource)Activator.CreateInstance(typeof(T), ApiVersion, source, metadata, info, extent, spec);
        }
    }

    internal interface ISpecDescriptor
    {
        string Name { get; }

        string ApiVersion { get; }

        string FullName { get; }

        Type SpecType { get; }

        IResource CreateInstance(SourceFile source, ResourceMetadata metadata, CommentMetadata comment, ISourceExtent extent, object spec);
    }
}
