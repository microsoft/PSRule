// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using System.Text;
using PSRule.Configuration;
using PSRule.Resources;
using PSRule.Runtime;

namespace PSRule.Pipeline.Output;

/// <summary>
/// An output writer that writes output to disk.
/// </summary>
internal sealed class FileOutputWriter : PipelineWriter
{
    private const string Source = "PSRule";

    private readonly Encoding _Encoding;
    private readonly string _Path;
    private readonly bool _WriteHost;

    internal FileOutputWriter(PipelineWriter inner, PSRuleOption option, Encoding encoding, string path, ShouldProcess shouldProcess, bool writeHost)
        : base(inner, option, shouldProcess)
    {
        _Encoding = encoding;
        _Path = path;
        _WriteHost = writeHost;
    }

    public override void WriteObject(object sendToPipeline, bool enumerateCollection)
    {
        WriteToFile(sendToPipeline);
    }

    private void WriteToFile(object o)
    {
        var rootedPath = Environment.GetRootedPath(_Path);
        if (CreateFile(rootedPath))
        {
            File.WriteAllText(path: rootedPath, contents: o.ToString(), encoding: _Encoding);
            InfoOutputPath(rootedPath);
        }
    }

    private void InfoOutputPath(string rootedPath)
    {
        if (!Option.Output.Footer.GetValueOrDefault(OutputOption.Default.Footer.Value).HasFlag(FooterFormat.OutputFile))
            return;

        var message = string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.InfoOutputPath, rootedPath);
        if (_WriteHost)
        {
            WriteHost(new HostInformationMessage
            {
                Message = message
            });
        }
        else
        {
            this.LogInformation(EventId.None, message);
        }
    }
}
