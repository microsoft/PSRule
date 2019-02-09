﻿using PSRule.Configuration;
using PSRule.Rules;
using System;

namespace PSRule.Pipeline
{
    public abstract class RulePipeline : IDisposable
    {
        internal readonly PipelineContext _Context;
        protected readonly PSRuleOption _Option;
        protected readonly RuleSource[] _Source;
        protected readonly RuleFilter _Filter;

        // Track whether Dispose has been called.
        private bool _Disposed = false;

        internal RulePipeline(PipelineContext context, PSRuleOption option, RuleSource[] source, RuleFilter filter)
        {
            _Context = context;
            _Option = option;
            _Source = source;
            _Filter = filter;
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                if (disposing)
                {
                    _Context.Dispose();
                }

                _Disposed = true;
            }
        }

        #endregion IDisposable
    }
}
