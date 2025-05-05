// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using PSRule.Configuration;

namespace PSRule.Pipeline.Output;

/// <summary>
/// An output writer that generates SARIF output.
/// </summary>
internal sealed class SarifOutputWriter(Source[]? source, PipelineWriter inner, PSRuleOption option, ShouldProcess? shouldProcess)
    : SerializationOutputWriter<InvokeResult>(inner, option, shouldProcess)
{
    private readonly SarifBuilder _Builder = new(source, option);
    private readonly Encoding _Encoding = option.Output.GetEncoding();

    public override void WriteObject(object sendToPipeline, bool enumerateCollection)
    {
        if (sendToPipeline is not InvokeResult result)
            return;

        Add(result);
    }

    protected override string Serialize(InvokeResult[] o)
    {
        for (var i = 0; o != null && i < o.Length; i++)
        {
            var run = o[i].Run;
            var records = o[i].AsRecord();
            for (var j = 0; j < records.Length; j++)
            {
                _Builder.Add(run, records[j]);
            }
        }
        var log = _Builder.Build();
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, _Encoding, bufferSize: 1024, leaveOpen: true);
        log.Save(writer);
        stream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
