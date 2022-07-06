// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Management.Automation;
using PSRule.Pipeline;

namespace PSRule.Tool
{
    internal sealed class ClientHost : HostContext
    {
        private InvocationContext _Invocation;
        private readonly bool _Verbose;
        private readonly bool _Debug;

        public ClientHost(InvocationContext invocation, bool verbose, bool debug)
        {
            _Invocation = invocation;
            _Verbose = verbose;
            _Debug = debug;
        }

        public override void Error(ErrorRecord errorRecord)
        {
            _Invocation.Console.Error.WriteLine(errorRecord.Exception.Message);
        }

        public override void Warning(string text)
        {
            _Invocation.Console.WriteLine(text);
        }

        public override bool ShouldProcess(string target, string action)
        {
            return true;
        }

        public override void Information(InformationRecord informationRecord)
        {
            if (informationRecord?.MessageData is HostInformationMessage info)
                _Invocation.Console.WriteLine(info.Message);
        }
    }
}
