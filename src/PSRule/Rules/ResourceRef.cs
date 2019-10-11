// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Rules
{
    internal abstract class ResourceRef
    {
        public readonly string Id;
        public readonly ResourceKind Kind;

        protected ResourceRef(string id, ResourceKind kind)
        {
            Kind = kind;
            Id = id;
        }
    }
}
