// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Annotations;
using System;
using System.Collections.Generic;

namespace PSRule.Rules
{
    internal sealed class SpecFactory
    {
        private readonly Dictionary<string, ISpecDescriptor> _Descriptors;

        public SpecFactory()
        {
            _Descriptors = new Dictionary<string, ISpecDescriptor>();
            foreach (var d in Specs.BuiltinTypes)
            {
                With(d);
            }
        }

        public bool TryDescriptor(string name, out ISpecDescriptor descriptor)
        {
            return _Descriptors.TryGetValue(name, out descriptor);
        }

        public void With<T, TOption>(string name) where T : Resource<TOption>, IResource where TOption : Spec, new()
        {
            var descriptor = new SpecDescriptor<T, TOption>(name);
            _Descriptors.Add(descriptor.Name, descriptor);
        }

        private void With(ISpecDescriptor descriptor)
        {
            _Descriptors.Add(descriptor.Name, descriptor);
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    internal sealed class SpecOptionAnnotationAttribute : Attribute
    {
        public SpecOptionAnnotationAttribute()
        {
            // Do nothing yet
        }

        /// <summary>
        /// The property to configure when using the flat style. If a property is not configured then the flat style is not supported.
        /// </summary>
        public string PropertyName { get; set; }
    }

    internal sealed class SpecDescriptor<T, TOption> : ISpecDescriptor where T : Resource<TOption>, IResource where TOption : Spec, new()
    {
        private Action<Spec, string> _DefaultPropertySetter;

        public SpecDescriptor(string name)
        {
            Name = name;
            Bind();
        }

        public string Name { get; set; }

        public Type SpecType
        {
            get { return typeof(TOption); }
        }

        public Type StepType
        {
            get { return typeof(T); }
        }

        public bool SupportsFlat { get; private set; }

        public IResource CreateInstance(SourceFile source, ResourceMetadata metadata, CommentMetadata comment, object option)
        {
            var info = new ResourceHelpInfo(comment.Synopsis);
            return (IResource)Activator.CreateInstance(typeof(T), source, metadata, info, option);
        }

        private void Bind()
        {
            var annotation = SpecType.GetCustomAttributes(typeof(SpecOptionAnnotationAttribute), false);
            if (annotation != null && annotation.Length > 0)
            {
                var attribute = (SpecOptionAnnotationAttribute)annotation[0];
                if (!string.IsNullOrEmpty(attribute.PropertyName))
                {
                    SupportsFlat = true;

                    var bindProperty = SpecType.GetProperty(attribute.PropertyName);
                    if (bindProperty == null)
                    {
                        throw new Exception($"Option type does not have the specified property: {attribute.PropertyName}");
                    }

                    // Create a lambda to convert and set the property
                    _DefaultPropertySetter = (Spec option, string value) =>
                        SetDefaultProperty(bindProperty.SetValue, bindProperty.PropertyType, option, value);
                }
            }
        }

        private void SetDefaultProperty(Action<object, object> set, Type propertyType, Spec option, string value)
        {
            object v = value;
            if (!propertyType.IsAssignableFrom(typeof(string)))
            {
                v = Convert.ChangeType(v, propertyType);
            }
            set.Invoke(option, v);
        }
    }

    internal interface ISpecDescriptor
    {
        string Name { get; }

        Type SpecType { get; }

        IResource CreateInstance(SourceFile source, ResourceMetadata metadata, CommentMetadata comment, object option);
    }
}
