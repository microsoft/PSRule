// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

internal sealed class ResultDetail : IResultDetail
{
    private readonly IList<ResultReason> _Reason;

    internal ResultDetail()
    {
        _Reason = [];
    }

    internal int Count => _Reason.Count;

    internal void Add(ResultReason reason)
    {
        _Reason.Add(reason);
    }

    internal string[] GetReasonStrings()
    {
        return _Reason.GetStrings();
    }

    IEnumerable<IResultReason> IResultDetail.Reason => _Reason;
}
