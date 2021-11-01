// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Newtonsoft.Json;
using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Definitions.Baselines;

namespace PSRule
{
    internal static class BaselineJsonSerializationMapper
    {
        internal static void MapBaseline(JsonWriter writer, JsonSerializer serializer, Baseline baseline)
        {
            writer.WriteStartObject();

            writer.WriteComment($"Synopsis: {baseline.Synopsis}");

            if (baseline?.ApiVersion != null)
            {
                MapPropertyName(writer, nameof(baseline.ApiVersion));
                writer.WriteValue(baseline.ApiVersion);
            }

            if (baseline?.Kind != null)
            {
                MapPropertyName(writer, nameof(baseline.Kind));
                var kind = Enum.GetName(typeof(ResourceKind), baseline.Kind);
                writer.WriteValue(kind);
            }

            if (baseline?.Metadata != null)
            {
                MapPropertyName(writer, nameof(baseline.Metadata));
                MapResourceMetadata(writer, serializer, baseline.Metadata);
            }

            if (baseline?.Spec != null)
            {
                MapPropertyName(writer, nameof(baseline.Spec));
                MapBaselineSpec(writer, serializer, baseline.Spec);
            }

            writer.WriteEndObject();
        }

        private static void MapPropertyName(JsonWriter writer, string propertyName)
        {
            writer.WritePropertyName(propertyName.ToCamelCase());
        }

        private static void MapResourceMetadata(JsonWriter writer, JsonSerializer serializer, ResourceMetadata resourceMetadata)
        {
            writer.WriteStartObject();

            if (!(resourceMetadata?.Annotations).NullOrEmpty())
            {
                MapPropertyName(writer, nameof(resourceMetadata.Annotations));
                MapDictionary(writer, serializer, resourceMetadata.Annotations);
            }

            if (resourceMetadata?.Name != null)
            {
                MapPropertyName(writer, nameof(resourceMetadata.Name));
                writer.WriteValue(resourceMetadata.Name);
            }

            if (!(resourceMetadata?.Tags).NullOrEmpty())
            {
                MapPropertyName(writer, nameof(resourceMetadata.Tags));
                MapDictionary(writer, serializer, resourceMetadata.Tags);
            }

            writer.WriteEndObject();
        }

        private static void MapBaselineSpec(JsonWriter writer, JsonSerializer serializer, BaselineSpec baselineSpec)
        {
            writer.WriteStartObject();

            if (baselineSpec?.Binding != null)
            {
                MapPropertyName(writer, nameof(baselineSpec.Binding));
                MapBindingOption(writer, serializer, baselineSpec.Binding);
            }

            if (baselineSpec?.Configuration != null)
            {
                MapPropertyName(writer, nameof(baselineSpec.Configuration));
                MapDictionary(writer, serializer, baselineSpec.Configuration);
            }

            if (baselineSpec?.Convention != null)
            {
                MapPropertyName(writer, nameof(baselineSpec.Convention));
                MapConventionOption(writer, baselineSpec.Convention);
            }

            if (baselineSpec?.Rule != null)
            {
                MapPropertyName(writer, nameof(baselineSpec.Rule));
                MapRuleOption(writer, serializer, baselineSpec.Rule);
            }

            writer.WriteEndObject();
        }

        private static void MapBindingOption(JsonWriter writer, JsonSerializer serializer, BindingOption bindingOption)
        {
            writer.WriteStartObject();

            if (bindingOption?.Field != null)
            {
                MapPropertyName(writer, nameof(bindingOption.Field));
                MapDictionary(writer, serializer, bindingOption.Field.GetFieldMap);
            }

            if ((bindingOption?.IgnoreCase).HasValue)
            {
                MapPropertyName(writer, nameof(bindingOption.IgnoreCase));
                serializer.Serialize(writer, bindingOption.IgnoreCase);
            }

            if (bindingOption?.NameSeparator != null)
            {
                MapPropertyName(writer, nameof(bindingOption.NameSeparator));
                writer.WriteValue(bindingOption.NameSeparator);
            }

            if ((bindingOption?.PreferTargetInfo).HasValue)
            {
                MapPropertyName(writer, nameof(bindingOption.PreferTargetInfo));
                serializer.Serialize(writer, bindingOption.PreferTargetInfo);
            }

            if (bindingOption?.TargetName != null)
            {
                MapPropertyName(writer, nameof(bindingOption.TargetName));
                MapStringArraySequence(writer, bindingOption.TargetName);
            }

            if (bindingOption?.TargetType != null)
            {
                MapPropertyName(writer, nameof(bindingOption.TargetType));
                MapStringArraySequence(writer, bindingOption.TargetType);
            }

            if ((bindingOption?.UseQualifiedName).HasValue)
            {
                MapPropertyName(writer, nameof(bindingOption.UseQualifiedName));
                serializer.Serialize(writer, bindingOption.UseQualifiedName);
            }

            writer.WriteEndObject();
        }

        private static void MapConventionOption(JsonWriter writer, ConventionOption conventionOption)
        {
            writer.WriteStartObject();

            if (conventionOption?.Include != null)
            {
                MapPropertyName(writer, nameof(conventionOption.Include));
                MapStringArraySequence(writer, conventionOption.Include);
            }

            writer.WriteEndObject();
        }

        private static void MapRuleOption(JsonWriter writer, JsonSerializer serializer, RuleOption ruleOption)
        {
            writer.WriteStartObject();

            if (ruleOption?.Exclude != null)
            {
                MapPropertyName(writer, nameof(ruleOption.Exclude));
                MapStringArraySequence(writer, ruleOption.Exclude);
            }

            if (ruleOption?.Include != null)
            {
                MapPropertyName(writer, nameof(ruleOption.Include));
                MapStringArraySequence(writer, ruleOption.Include);
            }

            if ((ruleOption?.IncludeLocal).HasValue)
            {
                MapPropertyName(writer, nameof(ruleOption.IncludeLocal));
                serializer.Serialize(writer, ruleOption.IncludeLocal);
            }

            if (ruleOption?.Tag != null)
            {
                MapPropertyName(writer, nameof(ruleOption.Tag));
                MapDictionary(writer, serializer, ruleOption.Tag.ToDictionary());
            }

            writer.WriteEndObject();
        }

        private static void MapDictionary<T>(JsonWriter writer, JsonSerializer serializer, IDictionary<string, T> dictionary)
        {
            writer.WriteStartObject();

            foreach (var kvp in dictionary.ToSortedDictionary())
            {
                MapPropertyName(writer, kvp.Key);

                if (kvp.Value is string stringValue)
                {
                    writer.WriteValue(stringValue);
                }

                else if (kvp.Value is string[] stringValues)
                {
                    MapStringArraySequence(writer, stringValues);
                }

                else if (kvp.Value is PSObject[] psObjects)
                {
                    MapPSObjectArraySequence(writer, serializer, psObjects);
                }

                else
                {
                    serializer.Serialize(writer, kvp.Value);
                }
            }

            writer.WriteEndObject();
        }

        private static void MapStringArraySequence(JsonWriter writer, string[] sequence)
        {
            writer.WriteStartArray();

            foreach (var item in sequence.OrderBy(item => item))
            {
                writer.WriteValue(item);
            }

            writer.WriteEndArray();
        }

        private static void MapPSObjectArraySequence(JsonWriter writer, JsonSerializer serializer, PSObject[] sequence)
        {
            writer.WriteStartArray();

            foreach (var obj in sequence)
            {
                if (obj.BaseObject == null || obj.HasNoteProperty())
                {
                    writer.WriteStartObject();

                    foreach (var propertyInfo in obj.Properties.OrderBy(prop => prop.Name))
                    {
                        MapPropertyName(writer, propertyInfo.Name);
                        serializer.Serialize(writer, propertyInfo.Value);
                    }

                    writer.WriteEndObject();
                }
                else
                {
                    serializer.Serialize(writer, obj.BaseObject);
                }
            }

            writer.WriteEndArray();
        }
    }
}