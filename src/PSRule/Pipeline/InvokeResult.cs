// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Rules;
using System.Collections.Generic;

namespace PSRule.Pipeline
{
    /// <summary>
    /// A result for a target object.
    /// </summary>
    public sealed class InvokeResult
    {
        private readonly List<RuleRecord> _Record;
        private RuleOutcome _Outcome;
        private long _Time;
        private int _Total;
        private int _Error;
        private int _Fail;

        internal InvokeResult()
        {
            _Record = new List<RuleRecord>();
            _Time = 0;
            _Total = 0;
            _Error = 0;
            _Fail = 0;
        }

        /// <summary>
        /// The execution time of all rules in milliseconds.
        /// </summary>
        internal long Time => _Time;

        internal int Total => _Total;

        internal int Error => _Error;

        internal int Fail => _Fail;

        internal int Pass => _Total - _Error - _Fail;

        internal RuleOutcome Outcome => _Outcome;

        /// <summary>
        /// Get the individual records for the target object.
        /// </summary>
        /// <returns>Returns an enumeration of RuleRecords.</returns>
        public RuleRecord[] AsRecord()
        {
            return _Record.ToArray();
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
            return _Outcome == RuleOutcome.Pass || _Outcome == RuleOutcome.Fail || _Outcome == RuleOutcome.Error;
        }

        /// <summary>
        /// Add a record to the result.
        /// </summary>
        /// <param name="ruleRecord">The record after processing a rule.</param>
        internal void Add(RuleRecord ruleRecord)
        {
            _Outcome = GetWorstCase(ruleRecord.Outcome);
            _Time += ruleRecord.Time;
            _Total++;

            if (ruleRecord.Outcome == RuleOutcome.Error)
                _Error++;

            if (ruleRecord.Outcome == RuleOutcome.Fail)
                _Fail++;

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
