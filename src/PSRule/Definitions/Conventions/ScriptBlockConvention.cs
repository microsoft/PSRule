// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Host;
using PSRule.Pipeline;
using PSRule.Runtime;
using System;
using System.Collections;
using System.Management.Automation;

namespace PSRule.Definitions.Conventions
{
    internal sealed class ScriptBlockConvention : BaseConvention, ILanguageBlock, IDisposable
    {
        private readonly LanguageScriptBlock _Begin;
        private readonly LanguageScriptBlock _Process;
        private readonly LanguageScriptBlock _End;

        private bool _Disposed;

        internal ScriptBlockConvention(SourceFile source, ResourceMetadata metadata, ResourceHelpInfo info, LanguageScriptBlock begin, LanguageScriptBlock process, LanguageScriptBlock end, ActionPreference errorPreference)
            : base(metadata.Name)
        {
            Info = info;
            Source = source;
            Id = ResourceHelper.GetId(source.ModuleName, metadata.Name);
            _Begin = begin;
            _Process = process;
            _End = end;
        }

        public string Id { get; }

        public SourceFile Source { get; }

        public ResourceHelpInfo Info { get; }

        string ILanguageBlock.Module => Source.ModuleName;

        string ILanguageBlock.SourcePath => Source.Path;

        public override void Begin(RunspaceContext context, IEnumerable input)
        {
            InvokeConventionBlock(_Begin, input);
        }

        public override void Process(RunspaceContext context, IEnumerable input)
        {
            InvokeConventionBlock(_Process, input);
        }

        public override void End(RunspaceContext context, IEnumerable input)
        {
            InvokeConventionBlock(_End, input);
        }

        private static void InvokeConventionBlock(LanguageScriptBlock block, IEnumerable input)
        {
            if (block == null)
                return;

            try
            {
                block.Invoke();
            }
            finally
            {

            }
        }

        #region IDisposable

        private void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                if (disposing)
                {
                    if (_Begin != null)
                        _Begin.Dispose();

                    if (_Process != null)
                        _Process.Dispose();

                    if (_End != null)
                        _End.Dispose();
                }
                _Disposed = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable
    }
}
