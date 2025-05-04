// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using PSRule.Configuration;
using PSRule.Pipeline;
using PSRule.Runtime;

namespace PSRule;

internal sealed class TestWriter(PSRuleOption option) : PipelineWriter(null, option, null)
{
    internal List<(EventId eventId, string message, Exception exception)> Errors = [];
    internal List<string> Warnings = [];
    internal List<object> Information = [];
    internal List<object> Output = [];

    public override void WriteObject(object sendToPipeline, bool enumerateCollection)
    {
        if (sendToPipeline == null)
            return;

        if (enumerateCollection && sendToPipeline is IEnumerable<object> enumerable)
        {
            Output.AddRange(enumerable);
        }
        else
        {
            Output.Add(sendToPipeline);
        }
    }

    public override bool IsEnabled(LogLevel logLevel)
    {
        return logLevel == LogLevel.Information ||
            logLevel == LogLevel.Warning ||
            logLevel == LogLevel.Error ||
            logLevel == LogLevel.Critical;
    }

    public override void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        switch (logLevel)
        {
            case LogLevel.Information:
                Information.Add(formatter(state, exception));
                break;

            case LogLevel.Warning:
                Warnings.Add(formatter(state, exception));
                break;

            case LogLevel.Error:
            case LogLevel.Critical:
                Errors.Add(new(eventId, formatter(state, exception), exception));
                break;
        }
    }
}
