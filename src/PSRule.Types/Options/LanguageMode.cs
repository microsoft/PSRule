// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Options;

/// <summary>
/// Configures the language mode PowerShell code executes as within PSRule runtime.
/// Does not affect YAML or JSON expressions.
/// <seealso href="https://microsoft.github.io/PSRule/latest/concepts/PSRule/en-US/about_PSRule_Options/#executionlanguagemode"/>
/// </summary>
public enum LanguageMode
{
    /// <summary>
    /// PowerShell code executes unconstrained.
    /// </summary>
    FullLanguage = 0,

    /// <summary>
    /// PowerShell code executes in constrained language mode that restricts the types and methods that can be used.
    /// </summary>
    ConstrainedLanguage = 1
}
