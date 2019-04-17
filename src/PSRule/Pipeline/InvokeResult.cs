using PSRule.Rules;
using System.Collections.Generic;

namespace PSRule.Pipeline
{
    /// <summary>
    /// A result for a target object.
    /// </summary>
    public sealed class InvokeResult
    {
        public readonly string TargetName;

        private readonly List<RuleRecord> _Record;
        private RuleOutcome _Outcome;
        private float _Time;

        internal InvokeResult(string targetName)
        {
            TargetName = targetName;
            _Record = new List<RuleRecord>();
            _Time = 0f;
        }

        internal float Time
        {
            get { return _Time; }
        }

        /// <summary>
        /// Get the individual records for the target object.
        /// </summary>
        /// <returns>Returns an enumeration of RuleRecords.</returns>
        public IEnumerable<RuleRecord> AsRecord()
        {
            return _Record;
        }

        /// <summary>
        /// Get an overall pass or fail for the target object.
        /// </summary>
        /// <returns>Returns true if object passed and false if object failed.</returns>
        public bool IsSuccess()
        {
            return _Outcome == RuleOutcome.Pass || _Outcome == RuleOutcome.None;
        }

        public bool IsProcessed()
        {
            return _Outcome == RuleOutcome.Processed;
        }

        /// <summary>
        /// Add a record to the result.
        /// </summary>
        /// <param name="ruleRecord">The record after processing a rule.</param>
        internal void Add(RuleRecord ruleRecord)
        {
            _Outcome = GetWorstCase(ruleRecord.Outcome);
            _Time += ruleRecord.Time;
            _Record.Add(ruleRecord);
        }

        private RuleOutcome GetWorstCase(RuleOutcome outcome)
        {
            if (outcome == RuleOutcome.Error || _Outcome == RuleOutcome.Error)
            {
                return RuleOutcome.Error;
            }
            else if (outcome == RuleOutcome.Fail || _Outcome == RuleOutcome.Fail)
            {
                return RuleOutcome.Fail;
            }
            else if (outcome == RuleOutcome.Pass || _Outcome == RuleOutcome.Pass)
            {
                return RuleOutcome.Pass;
            }

            return outcome;
        }
    }
}
