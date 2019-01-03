using System.Collections.Generic;

namespace PSRule.Host
{
    internal sealed class DependencyGraph<T> where T : IDependencyTarget
    {
        private readonly Dictionary<string, DependencyTarget> _Index;
        private readonly DependencyTarget[] _Targets;

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

        public sealed class DependencyTarget
        {
            public readonly DependencyGraph<T> Graph;
            public readonly T Value;

            internal DependencyTargetState State;

            public DependencyTarget(DependencyGraph<T> graph, T value)
            {
                Graph = graph;
                Value = value;
            }

            public bool Skipped
            {
                get { return State == DependencyTargetState.DependencyFail; }
            }

            public void Pass()
            {
                State = DependencyTargetState.Pass;
            }

            public void Fail()
            {
                State = DependencyTargetState.Fail;
            }
        }

        public IEnumerable<DependencyTarget> GetSingleTarget()
        {
            foreach (var target in _Targets)
            {
                if (target.Value.DependsOn != null && target.Value.DependsOn.Length > 0)
                {
                    // Process each dependency
                    foreach (var d in target.Value.DependsOn)
                    {
                        var dTarget = _Index[d];

                        // Check if dependency was already completed
                        if (dTarget.State == DependencyTargetState.Pass)
                        {
                            continue;
                        }
                        else if (dTarget.State == DependencyTargetState.Fail || dTarget.State == DependencyTargetState.DependencyFail)
                        {
                            target.State = DependencyTargetState.DependencyFail;
                            break;
                        }

                        yield return dTarget;
                    }
                }

                //if (target.State == DependencyTargetState.FailDependency)
                //{
                //    continue;
                //}

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
    }
}
