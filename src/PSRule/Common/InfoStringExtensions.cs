// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;

namespace PSRule;

internal static class InfoStringExtensions
{
    internal static void Update(this InfoString i1, InfoString i2)
    {
        if (i1 == null || i2 == null || !i2.HasValue)
            return;

        i1.Text = i2.Text;
        i1.Markdown = i2.Markdown;
    }
}
