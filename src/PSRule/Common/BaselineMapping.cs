// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Definitions.Baselines;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization.NamingConventions;

namespace PSRule
{
    /// <summary>
    /// This class provides encapsulation for baseline mapping
    /// </summary>
    internal static class BaselineMapping
    {
        internal static void MapBaseline(IEmitter emitter, Baseline baseline)
        {
            emitter.Emit(new MappingStart());
            emitter.Emit(new Comment($"Synopsis: {baseline.Synopsis}", isInline: false));

            MapPropertyName(emitter, nameof(baseline.ApiVersion));
            emitter.Emit(new Scalar(baseline.ApiVersion));

            MapPropertyName(emitter, nameof(baseline.Kind));
            string kind = Enum.GetName(typeof(ResourceKind), baseline.Kind);
            emitter.Emit(new Scalar(kind));

            MapPropertyName(emitter, nameof(baseline.Metadata));
            MapResourceMetadata(emitter, baseline.Metadata);

            MapPropertyName(emitter, nameof(baseline.Spec));
            MapBaselineSpec(emitter, baseline.Spec);

            emitter.Emit(new MappingEnd());
        }

        private static void MapPropertyName(IEmitter emitter, string propertyName)
        {
            emitter.Emit(new Scalar(CamelCaseNamingConvention.Instance.Apply(propertyName)));
        }

        private static void MapResourceMetadata(IEmitter emitter, ResourceMetadata resourceMetadata)
        {
            emitter.Emit(new MappingStart());

            MapPropertyName(emitter, nameof(resourceMetadata.Annotations));

            emitter.Emit(new MappingStart());
            foreach (KeyValuePair<string, object> kvp in resourceMetadata.Annotations)
            {
                emitter.Emit(new Scalar(kvp.Key));
                emitter.Emit(new Scalar(kvp.Value.ToString()));
            }
            emitter.Emit(new MappingEnd());

            MapPropertyName(emitter, nameof(resourceMetadata.Name));
            emitter.Emit(new Scalar(resourceMetadata.Name));

            MapPropertyName(emitter, nameof(resourceMetadata.Tags));

            emitter.Emit(new MappingStart());
            foreach (KeyValuePair<string, string> kvp in resourceMetadata.Tags)
            {
                emitter.Emit(new Scalar(kvp.Key));
                emitter.Emit(new Scalar(kvp.Value));
            }
            emitter.Emit(new MappingEnd());

            emitter.Emit(new MappingEnd());
        }

        private static void MapBaselineSpec(IEmitter emitter, BaselineSpec baselineSpec)
        {
            emitter.Emit(new MappingStart());

            MapPropertyName(emitter, nameof(baselineSpec.Binding));
            MapBindingOption(emitter, baselineSpec.Binding);

            MapPropertyName(emitter, nameof(baselineSpec.Configuration));
            MapConfigurationOption(emitter, baselineSpec.Configuration);

            MapPropertyName(emitter, nameof(baselineSpec.Convention));
            MapConventionOption(emitter, baselineSpec.Convention);

            MapPropertyName(emitter, nameof(baselineSpec.Rule));
            MapRuleOption(emitter, baselineSpec.Rule);

            emitter.Emit(new MappingEnd());
        }

        private static void MapBindingOption(IEmitter emitter, BindingOption bindingOption)
        {
            emitter.Emit(new MappingStart());

            if (bindingOption?.Field != null)
            {
                MapPropertyName(emitter, nameof(bindingOption.Field));

                emitter.Emit(new MappingStart());

                foreach (KeyValuePair<string, string[]> kvp in bindingOption.Field)
                {
                    emitter.Emit(new Scalar(kvp.Key));
                    MapStringArraySequence(emitter, kvp.Value);
                }

                emitter.Emit(new MappingEnd());
            }

            if ((bindingOption?.IgnoreCase).HasValue)
            {
                MapPropertyName(emitter, nameof(bindingOption.IgnoreCase));
                emitter.Emit(new Scalar(bindingOption.IgnoreCase.ToString()));
            }

            if (bindingOption?.NameSeparator != null)
            {
                MapPropertyName(emitter, nameof(bindingOption.NameSeparator));
                emitter.Emit(new Scalar(bindingOption.NameSeparator));
            }

            if ((bindingOption?.PreferTargetInfo).HasValue)
            {
                MapPropertyName(emitter, nameof(bindingOption.PreferTargetInfo));
                emitter.Emit(new Scalar(bindingOption.PreferTargetInfo.ToString()));
            }

            if (bindingOption?.TargetName != null)
            {
                MapPropertyName(emitter, nameof(bindingOption.TargetName));
                MapStringArraySequence(emitter, bindingOption.TargetName);
            }

            if (bindingOption?.TargetType != null)
            {
                MapPropertyName(emitter, nameof(bindingOption.TargetType));
                MapStringArraySequence(emitter, bindingOption.TargetType);
            }

            if ((bindingOption?.UseQualifiedName).HasValue)
            {
                MapPropertyName(emitter, nameof(bindingOption.UseQualifiedName));
                emitter.Emit(new Scalar(bindingOption.UseQualifiedName.ToString()));
            }

            emitter.Emit(new MappingEnd());
        }

        private static void MapConfigurationOption(IEmitter emitter, ConfigurationOption configurationOption)
        {
            emitter.Emit(new MappingStart());

            if (configurationOption != null)
            {
                foreach (KeyValuePair<string, object> kvp in configurationOption)
                {
                    emitter.Emit(new Scalar(kvp.Key));

                    if (kvp.Value is PSObject[] psObjects)
                    {
                        MapPSObjectArraySequence(emitter, psObjects);
                    }
                    else
                    {
                        emitter.Emit(new Scalar(kvp.Value.ToString()));
                    }
                }
            }

            emitter.Emit(new MappingEnd());
        }

        private static void MapConventionOption(IEmitter emitter, ConventionOption conventionOption)
        {
            emitter.Emit(new MappingStart());

            if (conventionOption?.Include != null)
            {
                MapPropertyName(emitter, nameof(conventionOption.Include));
                MapStringArraySequence(emitter, conventionOption.Include);
            }

            emitter.Emit(new MappingEnd());
        }

        private static void MapRuleOption(IEmitter emitter, RuleOption ruleOption)
        {
            emitter.Emit(new MappingStart());

            if (ruleOption?.Exclude != null)
            {
                MapPropertyName(emitter, nameof(ruleOption.Exclude));
                MapStringArraySequence(emitter, ruleOption.Exclude);
            }

            if (ruleOption?.Include != null)
            {
                MapPropertyName(emitter, nameof(ruleOption.Include));
                MapStringArraySequence(emitter, ruleOption.Include);
            }

            if ((ruleOption?.IncludeLocal).HasValue)
            {
                MapPropertyName(emitter, nameof(ruleOption.IncludeLocal));
                emitter.Emit(new Scalar(ruleOption.IncludeLocal.ToString()));
            }

            if (ruleOption?.Tag != null)
            {
                MapPropertyName(emitter, nameof(ruleOption.Tag));
                MapHashtable(emitter, ruleOption.Tag);
            }

            emitter.Emit(new MappingEnd());
        }

        private static void MapHashtable(IEmitter emitter, Hashtable hashtable)
        {
            emitter.Emit(new MappingStart());

            foreach (DictionaryEntry entry in hashtable)
            {
                emitter.Emit(new Scalar(entry.Key.ToString()));

                if (entry.Value is string entryValue)
                {
                    emitter.Emit(new Scalar(entryValue));
                }

                else if (entry.Value is string[] entryValues)
                {
                    MapStringArraySequence(emitter, entryValues);
                }

                else if (entry.Value is PSObject[] psObjects)
                {
                    MapPSObjectArraySequence(emitter, psObjects);
                }

                else
                {
                    emitter.Emit(new Scalar(entry.Value.ToString()));
                }
            }

            emitter.Emit(new MappingEnd());
        }

        private static void MapStringArraySequence(IEmitter emitter, string[] sequence)
        {
            emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Block));

            foreach (string item in sequence)
            {
                emitter.Emit(new Scalar(item));
            }

            emitter.Emit(new SequenceEnd());
        }

        private static void MapPSObjectArraySequence(IEmitter emitter, PSObject[] sequence)
        {
            emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Block));

            foreach (PSObject obj in sequence)
            {
                IEnumerable<PSPropertyInfo> noteProperties = obj.Properties
                    .Where(prop => prop.MemberType == PSMemberTypes.NoteProperty);

                if (noteProperties.Any())
                {
                    emitter.Emit(new MappingStart());

                    foreach (PSPropertyInfo propertyInfo in noteProperties)
                    {
                        emitter.Emit(new Scalar(propertyInfo.Name));
                        emitter.Emit(new Scalar(propertyInfo.Value.ToString()));
                    }

                    emitter.Emit(new MappingEnd());
                }
                else
                {
                    emitter.Emit(new Scalar(obj.BaseObject.ToString()));
                }
            }

            emitter.Emit(new SequenceEnd());
        }
    }
}