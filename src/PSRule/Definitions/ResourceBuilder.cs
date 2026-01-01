// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Converters.Yaml;
using PSRule.Pipeline;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.NodeDeserializers;

namespace PSRule.Definitions;

internal sealed class ResourceBuilder
{
    private readonly List<ILanguageBlock> _Output;
    private readonly IDeserializer _Deserializer;

    internal ResourceBuilder(IResourceDiscoveryContext context)
    {
        _Output = [];
        _Deserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithTypeMapping<IResourceAnnotations, ResourceAnnotations>()
            .WithTypeMapping<IResourceTags, ResourceTags>()
            .WithTypeMapping<IResourceLabels, ResourceLabels>()
            .WithTypeConverter(new FieldMapYamlTypeConverter())
            .WithTypeConverter(new StringArrayMapConverter())
            .WithTypeConverter(new StringArrayConverter())
            .WithTypeConverter(new CapabilityOptionYamlConverter())
            .WithTypeConverter(new ResourceIdReferenceYamlConverter())
            .WithNodeDeserializer(
                inner => new ResourceNodeDeserializer(context, new LanguageExpressionDeserializer(context, inner)),
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
        return _Output.Count == 0 ? [] : _Output.ToArray();
    }
}
