// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Resources;
using System.IO;
using System.Text;

namespace PSRule.Pipeline.Output
{
    /// <summary>
    /// An output writer that writes output to disk.
    /// </summary>
    internal sealed class FileOutputWriter : PipelineWriter
    {
        private readonly Encoding _Encoding;
        private readonly string _Path;
        private readonly ShouldProcess _ShouldProcess;

        internal FileOutputWriter(PipelineWriter inner, PSRuleOption option, Encoding encoding, string path, ShouldProcess shouldProcess)
            : base(inner, option)
        {
            _Encoding = encoding;
            _Path = path;
            _ShouldProcess = shouldProcess;
        }

        public override void WriteObject(object sendToPipeline, bool enumerateCollection)
        {
            WriteToFile(sendToPipeline);
        }

        private void WriteToFile(object o)
        {
            var rootedPath = PSRuleOption.GetRootedPath(_Path);
            var parentPath = Directory.GetParent(rootedPath);
            if (!parentPath.Exists && _ShouldProcess(target: parentPath.FullName, action: PSRuleResources.ShouldCreatePath))
            {
                Directory.CreateDirectory(path: parentPath.FullName);
            }
            if (_ShouldProcess(target: rootedPath, action: PSRuleResources.ShouldWriteFile))
            {
                File.WriteAllText(path: rootedPath, contents: o.ToString(), encoding: _Encoding);
            }
        }
    }
}
