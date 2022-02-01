// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;

namespace PSRule
{
    public sealed class ReadOnlyHashtable : Hashtable
    {
        internal ReadOnlyHashtable(IDictionary dictionary, IEqualityComparer equalityComparer)
            : base(dictionary, equalityComparer) { }

        public override bool IsReadOnly => true;
    }
}
