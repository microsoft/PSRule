// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;

namespace PSRule.Pipeline.Runs;

/// <summary>
/// A collection of runs.
/// </summary>
internal sealed class RunCollection : IEnumerable<Run>
{
    private readonly List<Run> _Runs;

    public RunCollection()
    {
        _Runs = [];
    }

    /// <summary>
    /// Add a run to the collection.
    /// Ignoring runs with no rules.
    /// </summary>
    public void Add(Run run)
    {
        if (run == null || run.Rules.Count == 0)
            return;

        _Runs.Add(run);
    }

    /// <summary>
    /// The number of runs in the collection.
    /// </summary>
    public int Count => _Runs.Count;

    /// <summary>
    /// The number of rules in the collection.
    /// </summary>
    public int RuleCount => _Runs.Sum(r => r.Rules.Count);

    #region IEnumerable<Run>

    public IEnumerator<Run> GetEnumerator()
    {
        return _Runs.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion IEnumerable<Run>
}
