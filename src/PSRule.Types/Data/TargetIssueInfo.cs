// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace PSRule.Data;

/// <summary>
/// An issue reported by a downstream tool.
/// </summary>
public sealed class TargetIssueInfo : IEquatable<TargetIssueInfo>
{
    private const string PROPERTY_TYPE = "type";
    private const string PROPERTY_NAME = "name";
    private const string PROPERTY_PATH = "path";
    private const string PROPERTY_MESSAGE = "message";

    /// <summary>
    /// Create an empty issue.
    /// </summary>
    public TargetIssueInfo()
    {
        // Do nothing
    }

    /// <summary>
    /// The type of issue.
    /// </summary>
    [JsonProperty(PropertyName = PROPERTY_TYPE)]
    public string? Type { get; internal set; }

    /// <summary>
    /// The name of the issue.
    /// </summary>
    [JsonProperty(PropertyName = PROPERTY_NAME)]
    public string? Name { get; internal set; }

    /// <summary>
    /// The object path reported by the issue.
    /// </summary>
    [JsonProperty(PropertyName = PROPERTY_PATH)]
    public string? Path { get; internal set; }

    /// <summary>
    /// A localized message describing the issue.
    /// </summary>
    [JsonProperty(PropertyName = PROPERTY_MESSAGE)]
    public string? Message { get; internal set; }

    /// <inheritdoc/>
    public bool Equals(TargetIssueInfo other)
    {
        return other != null &&
            Type == other.Type &&
            Name == other.Name &&
            Path == other.Path &&
            Message == other.Message;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return obj is TargetIssueInfo info && Equals(info);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked // Overflow is fine
        {
            var hash = 17;
            hash = hash * 23 + (Type != null ? Type.GetHashCode() : 0);
            hash = hash * 23 + (Name != null ? Name.GetHashCode() : 0);
            hash = hash * 23 + (Path != null ? Path.GetHashCode() : 0);
            hash = hash * 23 + (Message != null ? Message.GetHashCode() : 0);
            return hash;
        }
    }
}
