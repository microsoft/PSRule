// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;

namespace PSRule.Options;

/// <summary>
/// 
/// </summary>
public sealed class ReplacementStrings : StringMap<string>
{
    /// <summary>
    /// 
    /// </summary>
    public ReplacementStrings() : base() { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="items"></param>
    public ReplacementStrings(IDictionary<string, string> items) : base(items) { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="replacementStrings"></param>
    public ReplacementStrings(ReplacementStrings replacementStrings) : base(replacementStrings) { }
}
