// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
internal sealed class SpecAttribute : Attribute
{
    public SpecAttribute()
    {

    }

    public SpecAttribute(string apiVersion, string kind)
    {
        ApiVersion = apiVersion;
        Kind = kind;
    }

    public string ApiVersion { get; }

    public string Kind { get; }
}
