// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading;
using PSRule.Runtime;

namespace PSRule.Definitions
{
    internal sealed class ResultReason : IResultReasonV2
    {
        private string _Formatted;
        private string _Message;
        private readonly IOperand _Operand;

        internal ResultReason(IOperand operand, string text, object[] args)
        {
            _Operand = operand;
            Text = text;
            Args = args;
        }

        public string Path => _Operand.Path;

        public string Text { get; }

        public object[] Args { get; }

        public string Message
        {
            get
            {
                _Message ??= Args == null || Args.Length == 0 ? Text : string.Format(Thread.CurrentThread.CurrentCulture, Text, Args);
                return _Message;
            }
        }

        public override string ToString()
        {
            return Format();
        }

        public string Format()
        {
            _Formatted ??= string.Concat(
                _Operand?.ToString(),
                Message
            );
            return _Formatted;
        }
    }
}
