// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Data;

/// <summary>
/// A collection of issues reported by a downstream tool.
/// </summary>
public interface ITargetIssueCollection
{
    /// <summary>
    /// Get any issues from the collection that match the specified type.
    /// </summary>
    /// <param name="type">The type of the issue.</param>
    /// <returns>Returns issues that match the specified <paramref name="type"/>.</returns>
    TargetIssueInfo[] Get(string type = null);

    /// <summary>
    /// Check if the collection contains any of the specified issue type.
    /// </summary>
    /// <param name="type">The type of the issue.</param>
    /// <returns>Returns <c>true</c> if any the collection contains any issues matching the specified <paramref name="type"/>.</returns>
    bool Any(string type = null);
}

/// <summary>
/// A collection of issues reported by a downstream tool.
/// </summary>
internal sealed class TargetIssueCollection : ITargetIssueCollection
{
    private List<TargetIssueInfo> _Items;

    internal TargetIssueCollection() { }

    /// <inheritdoc/>
    public bool Any(string type = null)
    {
        return Get(type).Length > 0;
    }

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Avoid nested conditional expressions that increase complexity.")]
    public TargetIssueInfo[] Get(string type = null)
    {
        if (_Items == null)
            return Array.Empty<TargetIssueInfo>();

        return type == null ? _Items.ToArray() : _Items.Where(i => StringComparer.OrdinalIgnoreCase.Equals(i.Type, type)).ToArray();
    }

    /// <summary>
    /// Add one or more issues into the collection.
    /// </summary>
    /// <param name="issueInfo">An array of <see cref="TargetIssueInfo"/> instance to add to the collection.</param>
    internal void AddRange(TargetIssueInfo[] issueInfo)
    {
        for (var i = 0; issueInfo != null && i < issueInfo.Length; i++)
            Add(issueInfo[i]);
    }

    private void Add(TargetIssueInfo issueInfo)
    {
        if (issueInfo == null || string.IsNullOrEmpty(issueInfo.Type))
            return;

        _Items ??= new List<TargetIssueInfo>();
        _Items.Add(issueInfo);
    }
}
