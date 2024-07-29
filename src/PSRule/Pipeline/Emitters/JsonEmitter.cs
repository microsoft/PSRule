// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using Newtonsoft.Json;
using PSRule.Data;
using PSRule.Emitters;
using PSRule.Runtime;

namespace PSRule.Pipeline.Emitters;

/// <summary>
/// An <seealso cref="IEmitter"/> for processing JSON.
/// </summary>
internal sealed class JsonEmitter : FileEmitter
{
    private const string EXTENSION_JSON = ".json";
    private const string EXTENSION_JSONC = ".jsonc";
    private const string EXTENSION_SARIF = ".sarif";

    private readonly ILogger<JsonEmitter> _Logger;
    private readonly JsonSerializerSettings _Settings;
    private readonly JsonSerializer _Deserializer;
    private readonly HashSet<string> _Extensions;

    public JsonEmitter(ILogger<JsonEmitter> logger)
    {
        _Logger = logger;
        _Settings = new JsonSerializerSettings
        {

        };
        _Deserializer = JsonSerializer.CreateDefault(_Settings); // Think about caching this.
        _Deserializer.Converters.Add(new PSObjectArrayJsonConverter(null));
        _Extensions = new HashSet<string>();
    }

    /// <summary>
    /// Accept the file if it is a JSON file.
    /// </summary>
    protected override bool AcceptsFilePath(IEmitterContext context, IFileInfo info)
    {
        return info != null && (info.Extension == EXTENSION_JSON ||
            info.Extension == EXTENSION_JSONC ||
            info.Extension == EXTENSION_SARIF ||
            (_Extensions != null && _Extensions.Contains(info.Extension)));
    }

    /// <inheritdoc/>
    protected override bool VisitFile(IEmitterContext context, IFileStream stream)
    {
        using var reader = stream.AsTextReader();
        if (reader == null) return false;

        using var jsonReader = new JsonEmitterParser(reader, stream.Info);
        try
        {
            //var d = new JsonSerializer(); // Think about caching this.
            //d.Converters.Add(new PSObjectArrayJsonConverter(stream.Info));
            var value = _Deserializer.Deserialize<PSObject[]>(jsonReader);
            VisitItems(context, value, stream.Info);
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
    }

    protected override bool VisitString(IEmitterContext context, string content)
    {
        if (string.IsNullOrEmpty(content)) return false;

        using var reader = new StringReader(content);
        using var jsonReader = new JsonTextReader(new StringReader(content));
        try
        {
            var d = new JsonSerializer(); // Think about caching this.
            d.Converters.Add(new PSObjectArrayJsonConverter(null));
            var value = d.Deserialize<PSObject[]>(jsonReader);
            VisitItems(context, value, null);
            return true;
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {

        }
    }

    protected override bool AcceptsString(IEmitterContext context)
    {
        return context.Format == Options.InputFormat.Json;
    }

    private static void VisitItems(IEmitterContext context, IEnumerable<PSObject> items, IFileInfo sourceInfo)
    {
        if (items == null)
            return;

        foreach (var item in items)
        {
            if (item == null)
                return;

            var value = new TargetObject(item);
            context.Emit(value);
        }
    }
}
