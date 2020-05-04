// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions
{
    internal interface IResourceFilter
    {
        bool Match(string name, TagSet tag);
    }
}
