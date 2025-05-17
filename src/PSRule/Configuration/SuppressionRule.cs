// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Configuration;

/// <summary>
/// A suppression rule, that specifies TargetNames that will not be processed by individual rules.
/// </summary>
public sealed class SuppressionRule
{
    /// <summary>
    /// Create an empty suppression rule.
    /// </summary>
    public SuppressionRule()
    {

    }

    /// <summary>
    /// Create an instance with specified targets.
    /// </summary>
    private SuppressionRule(string[] targetNames)
    {
        TargetName = targetNames;
    }

    /// <summary>
    /// One of more target names to suppress.
    /// </summary>
    public string[] TargetName { get; set; }

    /// <summary>
    /// Create a suppression rule from a string.
    /// </summary>
    public static implicit operator SuppressionRule(string value)
    {
        return FromString(value);
    }

    /// <summary>
    /// Create a suppresion rule from a string array.
    /// </summary>
    public static implicit operator SuppressionRule(string[] value)
    {
        return FromString(value);
    }

    internal static SuppressionRule FromString(params string[] value)
    {
        return new SuppressionRule(value);
    }

    internal static SuppressionRule? FromObject(object value)
    {
        if (value is string)
            return FromString(value.ToString());

        return value.GetType().IsArray ? FromString(((object[])value).OfType<string>().ToArray()) : null;
    }
}
