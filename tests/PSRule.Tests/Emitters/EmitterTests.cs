// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using PSRule.Options;
using PSRule.Runtime;

namespace PSRule.Emitters;

#nullable enable

public abstract class EmitterTests : BaseTests
{
    protected static IEmitterConfiguration GetEmitterConfiguration(IDictionary<string, object>? configuration = default, (string key, string[]? types, bool? enabled, KeyValuePair<string, string>[]? replace)[]? format = default)
    {
        var formatOption = new FormatOption();
        for (var i = 0; format != null && i < format.Length; i++)
        {
            formatOption.Add(format[i].key, new FormatType { Type = format[i].types, Enabled = format[i].enabled, Replace = format[i].replace != null ? [.. format[i].replace!] : null });
        }

        return new InternalEmitterConfiguration(new InternalConfiguration(configuration ?? new Dictionary<string, object>()), formatOption);
    }
}

#nullable restore
