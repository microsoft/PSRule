// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Immutable;
using PSRule.Data;
using PSRule.Definitions;
using PSRule.Emitters;
using PSRule.Runtime;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NodeDeserializers;

namespace PSRule.Pipeline.Emitters;

/// <summary>
/// An <seealso cref="PSRule.Emitters.IEmitter"/> for processing YAML.
/// </summary>
internal sealed class YamlEmitter : FileEmitter
{
    private const string FORMAT = "yaml";

    private static readonly string[] _DefaultTypes = [".yaml", ".yml"];

    private readonly ILogger<YamlEmitter> _Logger;
    private readonly PSObjectYamlTypeConverter _TypeConverter;
    private readonly IDeserializer _Deserializer;
    private readonly ImmutableHashSet<string> _Types;

    public YamlEmitter(ILogger<YamlEmitter> logger, IEmitterConfiguration emitterConfiguration)
    {
        if (emitterConfiguration == null) throw new ArgumentNullException(nameof(emitterConfiguration));

        _Logger = logger ?? throw new NullReferenceException(nameof(logger));
        _TypeConverter = new PSObjectYamlTypeConverter();
        _Deserializer = GetDeserializer();
        _Types = emitterConfiguration.GetFormatTypes(FORMAT, _DefaultTypes).ToImmutableHashSet();
    }

    /// <summary>
    /// Accept the file if it is a YAML file.
    /// </summary>
    protected override bool AcceptsFilePath(IEmitterContext context, IFileInfo info)
    {
        return info != null && _Types.Contains(info.Extension);
    }

    /// <inheritdoc/>
    protected override bool VisitFile(IEmitterContext context, IFileStream stream)
    {
        using var reader = stream.AsTextReader();
        if (reader == null) return false;

        try
        {
            var parser = new YamlEmitterParser(reader, stream.Info);
            var result = new List<TargetObject>();
            parser.TryConsume<StreamStart>(out _);
            while (parser.Current is DocumentStart)
            {
                var item = _Deserializer.Deserialize<TargetObject[]>(parser);
                if (item == null || item.Length == 0)
                    continue;

                //VisitItems(context, item, stream.Info);
                context.Emit(item);
            }
            return true;
        }
        catch (Exception ex)
        {
            if (stream.Info != null && !string.IsNullOrEmpty(stream.Info.Path))
            {
                _Logger.ErrorReadFileFailed(stream.Info.Path, ex);
            }
            throw;
        }
        finally
        {
            reader.Dispose();
        }
        //return false;
    }

    protected override bool AcceptsString(IEmitterContext context)
    {
        return context.Format == Options.InputFormat.Yaml;
    }

    protected override bool VisitString(IEmitterContext context, string content)
    {
        if (string.IsNullOrEmpty(content)) return false;

        using var reader = new StringReader(content);

        try
        {
            var parser = new Parser(reader);
            var result = new List<TargetObject>();
            parser.TryConsume<StreamStart>(out _);
            while (parser.Current is DocumentStart)
            {
                var item = _Deserializer.Deserialize<TargetObject[]>(parser);
                if (item == null || item.Length == 0)
                    continue;

                context.Emit(item);
            }
            return true;
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            reader.Dispose();
        }
    }

    private IDeserializer GetDeserializer()
    {
        return new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .WithTypeConverter(_TypeConverter)
            .WithTypeMapping<IResourceAnnotations, ResourceAnnotations>()
            .WithTypeMapping<IResourceTags, ResourceTags>()
            .WithTypeMapping<IResourceLabels, ResourceLabels>()
            .WithNodeDeserializer(
                inner => new TargetObjectYamlDeserializer(inner),
                s => s.InsteadOf<YamlConvertibleNodeDeserializer>())
            .Build();
    }

    //private static void VisitItems(IEmitterContext context, IEnumerable<TargetObject> items, IFileInfo sourceInfo)
    //{
    //    if (items == null)
    //        return;

    //    foreach (var item in items)
    //    {
    //        if (item == null)
    //            return;

    //        //var value = new TargetObject(item);
    //        //NoteSource(value, sourceInfo);

    //        context.Emit(item);
    //    }
    //}

    //private static void NoteSource(TargetObject value, IFileInfo source)
    //{
    //    if (value == null || source == null)
    //        return;

    //    //value.Value.UseTargetInfo(out var targetInfo);
    //    //targetInfo.UpdateSource(source);
    //    //value.Source.AddRange(targetInfo.Source.ToArray());
    //    //value.Issue.AddRange(targetInfo.Issue.ToArray());
    //}
}
