// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;

namespace PSRule
{
    internal static class ResourceHelpInfoExtensions
    {
        internal static void Update(this IResourceHelpInfo info, IResourceHelpInfo other)
        {
            if (info == null || other == null)
                return;

            info.Synopsis.Update(other.Synopsis);
            if (other.Description != null && other.Description.HasValue)
                info.Description.Text = other.Description.Text;
        }
    }
}
