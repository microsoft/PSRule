// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace PSRule.Definitions
{
    public interface IConditionResult
    {
        bool HadErrors { get; }

        int Count { get; }

        int Pass { get; }
    }

    public interface ICondition : IDisposable
    {
        IConditionResult If();
    }
}
