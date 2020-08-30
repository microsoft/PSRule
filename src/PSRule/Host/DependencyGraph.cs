// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace PSRule.Host
{
    internal sealed class DependencyGraph<T> : IDisposable where T : IDependencyTarget
    {
        private readonly Dictionary<string, DependencyTarget> _Index;
        private readonly DependencyTarget[] _Targets;

        // Track whether Dispose has been called.
        private bool _Disposed;

        public DependencyGraph(T[] targets)
        {
            _Targets = new DependencyTarget[targets.Length];
            _Index = new Dictionary<string, DependencyTarget>(targets.Length);
            Prepare(targets);
        }

        public int Count
        {
            get
            {
                return _Targets.Length;
            }
        }

        public enum DependencyTargetState : byte
        {
            None = 0,

            Pass = 1,

            Fail = 2,

            DependencyFail = 3
        }

        internal sealed class DependencyTarget
        {
            public readonly DependencyGraph<T> Graph;
            public readonly T Value;

            private DependencyTargetState State;

            public DependencyTarget(DependencyGraph<T> graph, T value)
            {
                Graph = graph;
                Value = value;
            }

            public bool Skipped
            {
                get { return State == DependencyTargetState.DependencyFail; }
            }

            public bool Failed
            {
                get { return State == DependencyTargetState.Fail || State == DependencyTargetState.DependencyFail; }
            }

            public bool Passed
            {
                get { return State == DependencyTargetState.Pass; }
            }

            public void Pass()
            {
                State = DependencyTargetState.Pass;
            }

            public void Fail()
            {
                State = DependencyTargetState.Fail;
            }

            public void DependencyFail()
            {
                State = DependencyTargetState.DependencyFail;
            }
        }

        public IEnumerable<DependencyTarget> GetSingleTarget()
        {
            for (var t = 0; t < _Targets.Length; t++)
            {
                var target = _Targets[t];
                if (target.Value.DependsOn != null && target.Value.DependsOn.Length > 0)
                {
                    // Process each dependency
                    for (var d = 0; d < target.Value.DependsOn.Length; d++)
                    {
                        var dTarget = _Index[target.Value.DependsOn[d]];

                        // Check if dependency was already completed
                        if (dTarget.Passed)
                        {
                            continue;
                        }
                        else if (dTarget.Failed)
                        {
                            target.DependencyFail();
                            break;
                        }
                        yield return dTarget;
                    }
                }
                yield return target;
            }
        }

        private void Prepare(T[] targets)
        {
            for (var i = 0; i < targets.Length; i++)
            {
                _Targets[i] = new DependencyTarget(this, targets[i]);
                _Index.Add(targets[i].RuleId, _Targets[i]);
            }
        }

        #region IDisposable

        void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                if (disposing && _Targets != null && _Targets.Length > 0 && typeof(T) is IDisposable)
                {
                    for (var i = 0; i < _Targets.Length; i++)
                    {
                        ((IDisposable)_Targets[i].Value).Dispose();
                    }
                }

                _Index.Clear();
                _Disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion IDisposable
    }
}
