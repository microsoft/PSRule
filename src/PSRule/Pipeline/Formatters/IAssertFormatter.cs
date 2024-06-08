// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;

namespace PSRule.Pipeline.Formatters;

internal interface IAssertFormatter : IPipelineWriter
{
    void Result(InvokeResult result);

    void Error(ErrorRecord errorRecord);

    void Warning(WarningRecord warningRecord);

    void End(int total, int fail, int error);
}
