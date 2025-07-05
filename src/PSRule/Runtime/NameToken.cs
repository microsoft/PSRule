// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;

namespace PSRule.Runtime;

#pragma warning disable CS8618 // Keyword required is not supported in .NET Standard 2.0

/// <summary>
/// A token for expressing a path through a tree of fields.
/// </summary>
[DebuggerDisplay("{Type}, Name = {Name}, Index = {Index}")]
internal sealed class NameToken
{
    /// <summary>
    /// The name of the field if the token type if Field.
    /// </summary>
    public string Name;

    /// <summary>
    /// The index if the token type if Index.
    /// </summary>
    public int Index;

    /// <summary>
    /// The next token.
    /// </summary>
    public NameToken Next;

    /// <summary>
    /// The type of the token.
    /// </summary>
    public NameTokenType Type = NameTokenType.Field;
}

#pragma warning restore CS8618
