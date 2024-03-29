// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PSRule.Options;

/// <summary>
/// Configures the language mode PowerShell code executes as within PSRule runtime.
/// Does not affect YAML or JSON expressions.
/// <seealso href="https://microsoft.github.io/PSRule/latest/concepts/PSRule/en-US/about_PSRule_Options/#executionlanguagemode"/>
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum LanguageMode
{
    /// <summary>
    /// Executes with all language features.
    /// </summary>
    FullLanguage = 0,

    /// <summary>
    /// Executes in constrained language mode that restricts the types and methods that can be used.
    /// </summary>
    ConstrainedLanguage = 1
}
