// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Rules
{
    internal interface IResourceFilter
    {
        bool Match(string name, TagSet tag);
    }
}
