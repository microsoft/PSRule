// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions
{
    internal sealed class ResultDetail : IResultDetailV2
    {
        private readonly IList<ResultReason> _Reason;

        internal ResultDetail()
        {
            _Reason = new List<ResultReason>();
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

        IEnumerable<IResultReasonV2> IResultDetailV2.Reason => _Reason;
    }
}
