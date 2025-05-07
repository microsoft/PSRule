// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Management.Automation;
using System.Net;
using Newtonsoft.Json;
using PSRule.Data;
using PSRule.Help;
using PSRule.Runtime;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NodeDeserializers;

namespace PSRule.Pipeline;

internal delegate IEnumerable<TargetObject> VisitTargetObject(TargetObject sourceObject);
internal delegate IEnumerable<TargetObject> VisitTargetObjectAction(TargetObject sourceObject, VisitTargetObject next);

internal static class PipelineReceiverActions
{
    private const string JSON = ".json";
    private const string JSONC = ".jsonc";
    private const string YAML = ".yaml";
    private const string YML = ".yml";
    private const string MD = ".md";
    private const string MARKDOWN = ".markdown";
    private const string PSD1 = ".psd1";

    private static readonly TargetObject[] EmptyArray = [];

    public static IEnumerable<TargetObject> PassThru(TargetObject targetObject)
    {
        yield return targetObject;
    }

    public static IEnumerable<ITargetObject> DetectInputFormat(ITargetObject targetObject, VisitTargetObject next)
    {
        var pathExtension = GetPathExtension(targetObject);

        // Handle JSON
        if (pathExtension == JSON || pathExtension == JSONC)
        {
            return ConvertFromJson(targetObject, next);
        }
        // Handle YAML
        else if (pathExtension == YAML || pathExtension == YML)
        {
            return ConvertFromYaml(targetObject, next);
        }
        // Handle Markdown
        else if (pathExtension == MD || pathExtension == MARKDOWN)
        {
            return ConvertFromMarkdown(targetObject, next);
        }
        // Handle PowerShell Data
        else if (pathExtension == PSD1)
        {
            return ConvertFromPowerShellData(targetObject, next);
        }
        return [targetObject];
    }

    public static IEnumerable<ITargetObject> ConvertFromJson(ITargetObject targetObject, VisitTargetObject next)
    {
        // Only attempt to deserialize if the input is a string, file or URI
        if (!IsAcceptedType(targetObject))
            return [targetObject];

        var reader = ReadAsReader(targetObject, out var sourceInfo);
        try
        {
            var d = new JsonSerializer(); // Think about caching this.
            d.Converters.Add(new PSObjectArrayJsonConverter(sourceInfo));
            var value = d.Deserialize<PSObject[]>(AsJsonTextReader(reader));
            return VisitItems(value, sourceInfo, next);
        }
        catch (Exception ex)
        {
            if (sourceInfo != null && !string.IsNullOrEmpty(sourceInfo.File))
            {
                LegacyRunspaceContext.CurrentThread.Writer.ErrorReadFileFailed(sourceInfo.File, ex);
                return EmptyArray;
            }
            throw;
        }
        finally
        {
            reader.Dispose();
        }
    }

    public static IEnumerable<ITargetObject> ConvertFromYaml(ITargetObject targetObject, VisitTargetObject next)
    {
        // Only attempt to deserialize if the input is a string, file or URI
        if (!IsAcceptedType(targetObject))
            return [targetObject];

        var reader = ReadAsReader(targetObject, out var sourceInfo);
        var d = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .WithTypeConverter(new PSObjectYamlTypeConverter())
            .WithNodeDeserializer(
                inner => new PSObjectYamlDeserializer(inner, sourceInfo),
                s => s.InsteadOf<YamlConvertibleNodeDeserializer>())
            .Build();

        try
        {
            var parser = new Parser(reader);
            var result = new List<ITargetObject>();
            parser.TryConsume<StreamStart>(out _);
            while (parser.Current is DocumentStart)
            {
                var item = d.Deserialize<PSObject[]>(parser);
                if (item == null)
                    continue;

                var items = VisitItems(item, sourceInfo, next);
                if (items == null)
                    continue;

                result.AddRange(items);
            }

            return result.Count == 0 ? EmptyArray : result.ToArray();
        }
        catch (Exception ex)
        {
            if (sourceInfo != null && !string.IsNullOrEmpty(sourceInfo.File))
            {
                LegacyRunspaceContext.CurrentThread.Writer.ErrorReadFileFailed(sourceInfo.File, ex);
                return EmptyArray;
            }
            throw;
        }
        finally
        {
            reader.Dispose();
        }
    }

    public static IEnumerable<ITargetObject> ConvertFromMarkdown(ITargetObject targetObject, VisitTargetObject next)
    {
        // Only attempt to deserialize if the input is a string or a file
        if (!IsAcceptedType(targetObject))
            return [targetObject];

        var markdown = ReadAsString(targetObject, out var sourceInfo);
        var value = MarkdownConvert.DeserializeObject(markdown);
        return VisitItems(value, sourceInfo, next);
    }

    public static IEnumerable<ITargetObject> ConvertFromPowerShellData(ITargetObject targetObject, VisitTargetObject next)
    {
        // Only attempt to deserialize if the input is a string or a file
        if (!IsAcceptedType(targetObject))
            return [targetObject];

        var data = ReadAsString(targetObject, out var sourceInfo);
        var ast = System.Management.Automation.Language.Parser.ParseInput(data, out _, out _);
        var hashtables = ast.FindAll(item => item is System.Management.Automation.Language.HashtableAst, false);
        if (hashtables == null)
            return EmptyArray;

        var result = new List<PSObject>();
        foreach (var hashtable in hashtables)
        {
            if (hashtable?.Parent?.Parent?.Parent?.Parent == ast)
                result.Add(PSObject.AsPSObject(hashtable.SafeGetValue()));
        }
        var value = result.ToArray();
        return VisitItems(value, sourceInfo, next);
    }

    public static IEnumerable<TargetObject> ReadObjectPath(TargetObject targetObject, VisitTargetObject source, string objectPath, bool caseSensitive)
    {
        if (!ObjectHelper.GetPath(
            bindingContext: null,
            targetObject: targetObject.Value,
            path: objectPath,
            caseSensitive: caseSensitive,
            value: out object nestedObject))
            return EmptyArray;

        var nestedType = nestedObject.GetType();
        if (typeof(IEnumerable).IsAssignableFrom(nestedType))
        {
            var result = new List<TargetObject>();
            foreach (var item in (nestedObject as IEnumerable))
                result.Add(new TargetObject(PSObject.AsPSObject(item)));

            return result.ToArray();
        }
        else
        {
            return [new(PSObject.AsPSObject(nestedObject), targetObject.Source)];
        }
    }

    private static string? GetPathExtension(ITargetObject targetObject)
    {
        var baseObject = GetBaseObject(targetObject.Value);

        if (baseObject is InputFileInfo inputFileInfo)
            return inputFileInfo.Extension;

        if (baseObject is FileInfo fileInfo)
            return fileInfo.Extension;

        if (baseObject is Uri uri)
            return Path.GetExtension(uri.OriginalString);

        return null;
    }

    private static object? GetBaseObject(object o)
    {
        if (o is not PSObject pso) return o;

        return pso.BaseObject == null ? o : pso.BaseObject;
    }

    private static bool IsAcceptedType(ITargetObject targetObject)
    {
        var baseObject = GetBaseObject(targetObject.Value);

        return baseObject is string ||
            baseObject is InputFileInfo ||
            baseObject is FileInfo ||
            baseObject is Uri;
    }

    private static string ReadAsString(ITargetObject targetObject, out TargetSourceInfo? sourceInfo)
    {
        sourceInfo = null;
        var baseObject = GetBaseObject(targetObject.Value);

        if (baseObject is string)
        {
            return baseObject.ToString();
        }
        else if (baseObject is InputFileInfo inputFileInfo)
        {
            sourceInfo = new TargetSourceInfo(inputFileInfo);
            using var reader = new StreamReader(inputFileInfo.FullName);
            return reader.ReadToEnd();
        }
        else if (baseObject is FileInfo fileInfo)
        {
            sourceInfo = new TargetSourceInfo(fileInfo);
            using var reader = new StreamReader(fileInfo.FullName);
            return reader.ReadToEnd();
        }
        else
        {
            var uri = baseObject as Uri;
            sourceInfo = new TargetSourceInfo(uri);
            using var webClient = new WebClient();
            return webClient.DownloadString(uri);
        }
    }

    private static TextReader ReadAsReader(ITargetObject targetObject, out TargetSourceInfo? sourceInfo)
    {
        sourceInfo = null;
        var baseObject = GetBaseObject(targetObject.Value);

        if (baseObject is string)
        {
            return new StringReader(baseObject.ToString());
        }
        else if (baseObject is InputFileInfo inputFileInfo)
        {
            sourceInfo = new TargetSourceInfo(inputFileInfo);
            return new StreamReader(inputFileInfo.FullName);
        }
        else if (baseObject is FileInfo fileInfo)
        {
            sourceInfo = new TargetSourceInfo(fileInfo);
            return new StreamReader(fileInfo.FullName);
        }
        else
        {
            var uri = baseObject as Uri;
            sourceInfo = new TargetSourceInfo(uri);
            using var webClient = new WebClient();
            return new StringReader(webClient.DownloadString(uri));
        }
    }

    private static IEnumerable<ITargetObject> VisitItem(PSObject value, TargetSourceInfo sourceInfo, VisitTargetObject next)
    {
        if (value == null)
            return EmptyArray;

        var items = next(new TargetObject(value));
        if (items == null)
            return EmptyArray;

        foreach (var i in items)
        {
            NoteSource(i, sourceInfo);
        }
        return items;
    }

    private static IEnumerable<ITargetObject> VisitItems(IEnumerable<PSObject> value, TargetSourceInfo sourceInfo, VisitTargetObject next)
    {
        if (value == null)
            return EmptyArray;

        var result = new List<ITargetObject>();
        foreach (var item in value)
            result.AddRange(VisitItem(item, sourceInfo, next));

        return result.Count == 0 ? EmptyArray : result.ToArray();
    }

    private static void NoteSource(ITargetObject value, TargetSourceInfo source)
    {
        if (value is not TargetObject to || source == null || value.Value is not PSObject pso)
            return;

        pso.UseTargetInfo(out var targetInfo);
        targetInfo.UpdateSource(source);
        to.Source.AddRange(targetInfo.Source.ToArray());
        to.Issue.AddRange(targetInfo.Issue.ToArray());
    }

    private static JsonTextReader AsJsonTextReader(TextReader reader)
    {
        return new JsonTextReader(reader);
    }
}
