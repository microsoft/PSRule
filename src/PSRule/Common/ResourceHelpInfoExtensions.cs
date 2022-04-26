// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;

namespace PSRule
{
    internal static class ResourceHelpInfoExtensions
    {
        public static void Update(this IResourceHelpInfo info, IResourceHelpInfo other)
        {
            if (info == null || other == null)
                return;

            if (other.Synopsis != null && other.Synopsis.HasValue)
                info.Synopsis = other.Synopsis;

            if (other.Description != null && other.Description.HasValue)
                info.Description = other.Description;
        }
    }
}
