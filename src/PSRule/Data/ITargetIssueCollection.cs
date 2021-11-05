// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace PSRule.Data
{
    public interface ITargetIssueCollection
    {
        TargetIssueInfo[] Get(string type = null);

        bool Any(string type = null);
    }

    internal sealed class TargetIssueCollection : ITargetIssueCollection
    {
        private List<TargetIssueInfo> _Items;

        internal TargetIssueCollection() { }

        public bool Any(string type = null)
        {
            return Get(type).Length > 0;
        }

        public TargetIssueInfo[] Get(string type = null)
        {
            if (_Items == null)
                return Array.Empty<TargetIssueInfo>();

            return type == null ? _Items.ToArray() : _Items.Where(i => StringComparer.OrdinalIgnoreCase.Equals(i.Type, type)).ToArray();
        }

        internal void AddRange(TargetIssueInfo[] issueInfo)
        {
            for (var i = 0; issueInfo != null && i < issueInfo.Length; i++)
                Add(issueInfo[i]);
        }

        private void Add(TargetIssueInfo issueInfo)
        {
            if (issueInfo == null)
                return;

            if (string.IsNullOrEmpty(issueInfo.Type))
                return;

            if (_Items == null)
                _Items = new List<TargetIssueInfo>();

            _Items.Add(issueInfo);
        }
    }
}
