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
    internal static class BaselineYamlMapping
    {
        internal static void MapBaseline(IEmitter emitter, Baseline baseline)
        {
            emitter.Emit(new MappingStart());

            emitter.Emit(new Comment($"Synopsis: {baseline.Synopsis}", isInline: false));

            if (baseline?.ApiVersion != null)
            {
                MapPropertyName(emitter, nameof(baseline.ApiVersion));
                emitter.Emit(new Scalar(baseline.ApiVersion));
            }

            if (baseline?.Kind != null)
            {
                MapPropertyName(emitter, nameof(baseline.Kind));
                string kind = Enum.GetName(typeof(ResourceKind), baseline.Kind);
                emitter.Emit(new Scalar(kind));
            }

            if (baseline?.Metadata != null)
            {
                MapPropertyName(emitter, nameof(baseline.Metadata));
                MapResourceMetadata(emitter, baseline.Metadata);
            }

            if (baseline?.Spec != null)
            {
                MapPropertyName(emitter, nameof(baseline.Spec));
                MapBaselineSpec(emitter, baseline.Spec);
            }

            emitter.Emit(new MappingEnd());
        }

        private static void MapPropertyName(IEmitter emitter, string propertyName)
        {
            emitter.Emit(new Scalar(CamelCaseNamingConvention.Instance.Apply(propertyName)));
        }

        private static void MapResourceMetadata(IEmitter emitter, ResourceMetadata resourceMetadata)
        {
            emitter.Emit(new MappingStart());

            if (resourceMetadata?.Annotations != null && resourceMetadata?.Annotations.Count > 0)
            {
                MapPropertyName(emitter, nameof(resourceMetadata.Annotations));
                MapDictionary(emitter, resourceMetadata.Annotations);
            }

            if (resourceMetadata?.Name != null)
            {
                MapPropertyName(emitter, nameof(resourceMetadata.Name));
                emitter.Emit(new Scalar(resourceMetadata.Name));
            }

            if (resourceMetadata?.Tags != null && resourceMetadata?.Tags.Count > 0)
            {
                MapPropertyName(emitter, nameof(resourceMetadata.Tags));
                MapDictionary(emitter, resourceMetadata.Tags);
            }

            emitter.Emit(new MappingEnd());
        }

        private static void MapBaselineSpec(IEmitter emitter, BaselineSpec baselineSpec)
        {
            emitter.Emit(new MappingStart());

            if (baselineSpec?.Binding != null)
            {
                MapPropertyName(emitter, nameof(baselineSpec.Binding));
                MapBindingOption(emitter, baselineSpec.Binding);
            }

            if (baselineSpec?.Configuration != null)
            {
                MapPropertyName(emitter, nameof(baselineSpec.Configuration));
                MapDictionary(emitter, baselineSpec.Configuration);
            }

            if (baselineSpec?.Convention != null)
            {
                MapPropertyName(emitter, nameof(baselineSpec.Convention));
                MapConventionOption(emitter, baselineSpec.Convention);
            }

            if (baselineSpec?.Rule != null)
            {
                MapPropertyName(emitter, nameof(baselineSpec.Rule));
                MapRuleOption(emitter, baselineSpec.Rule);
            }

            emitter.Emit(new MappingEnd());
        }

        private static void MapBindingOption(IEmitter emitter, BindingOption bindingOption)
        {
            emitter.Emit(new MappingStart());

            if (bindingOption?.Field != null)
            {
                MapPropertyName(emitter, nameof(bindingOption.Field));
                MapDictionary(emitter, bindingOption.Field.GetFieldMap);
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

        private static void MapDictionary<T>(IEmitter emitter, IDictionary<string, T> dictionary)
        {
            emitter.Emit(new MappingStart());

            foreach (KeyValuePair<string, T> kvp in dictionary)
            {
                emitter.Emit(new Scalar(kvp.Key));

                if (kvp.Value is string stringValue)
                {
                    emitter.Emit(new Scalar(stringValue));
                }

                else if (kvp.Value is string[] stringValues)
                {
                    MapStringArraySequence(emitter, stringValues);
                }

                else if (kvp.Value is PSObject[] psObjects)
                {
                    MapPSObjectArraySequence(emitter, psObjects);
                }

                else
                {
                    emitter.Emit(new Scalar(kvp.Value.ToString()));
                }
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