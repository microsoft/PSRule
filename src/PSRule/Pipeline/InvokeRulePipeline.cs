// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Definitions.Rules;
using PSRule.Host;
using PSRule.Options;
using PSRule.Pipeline.Runs;
using PSRule.Rules;
using PSRule.Runtime;

namespace PSRule.Pipeline;

internal sealed class InvokeRulePipeline : RulePipeline, IPipeline
{
    private readonly RuleOutcome _Outcome;
    private readonly RunCollection _Runs;

    // A per rule summary of rules that have been processed and the outcome
    private readonly Dictionary<string, RuleSummaryRecord> _Summary;

    private readonly bool _IsSummary;
    private SuppressionFilter _SuppressionFilter;
    private SuppressionFilter _SuppressionGroupFilter;
    private readonly List<InvokeResult> _Completed;
    private readonly ExecutionActionPreference _ExecutionNoValidInputOption;

    // Track whether Dispose has been called.
    // private bool _Disposed;

    internal InvokeRulePipeline(PipelineContext context, Source[] source, RuleOutcome outcome)
        : base(context, source)
    {
        var runBuilder = new RunCollectionBuilder(Pipeline.ResourceCache, Pipeline.Writer, context.Option, context.LanguageScope, context.RunInstance);

        _Runs = runBuilder.WithBaselinesOrDefault(Context).Build();

        RuleCount = _Runs.RuleCount;

        if (RuleCount == 0)
        {
            context.Writer.LogNoMatchingRules(context.Option.Execution?.NoMatchingRules ?? ExecutionOption.Default.NoMatchingRules!.Value);
        }

        _Outcome = outcome;
        _IsSummary = context.Option.Output.As.Value == ResultFormat.Summary;
        _Summary = _IsSummary ? [] : null;
        _Completed = [];
        _ExecutionNoValidInputOption = context.Option.Execution?.NoValidInput ?? ExecutionOption.Default.NoValidInput!.Value;
    }

    public int RuleCount { get; private set; }

    /// <inheritdoc/>
    public override void Begin()
    {
        base.Begin();

        var resourceIndex = new ResourceIndex(Pipeline.ResourceCache.OfType<IRuleV1>());

        _SuppressionFilter = new SuppressionFilter(Context, Pipeline.Option.Suppression, resourceIndex);
        _SuppressionGroupFilter = new SuppressionFilter(Pipeline.SuppressionGroup, resourceIndex);
    }

    /// <inheritdoc/>
    public override void Process(PSObject sourceObject)
    {
        try
        {
            Pipeline.Reader.Enqueue(sourceObject);
            while (Pipeline.Reader.TryDequeue(out var next))
            {
                // TODO: Temporary workaround to cast interface
                if (next is TargetObject to)
                {

                    // var result = ProcessTargetObject(to);
                    ProcessTargetObject(to);

                    // _Completed.Add(result);
                    // Pipeline.Writer.WriteObject(result, false);
                }
            }
        }
        catch (Exception)
        {
            End();
            throw;
        }
    }

    /// <inheritdoc/>
    public override void End()
    {
        if (_Completed.Count == 0)
        {
            Context.Writer.LogNoValidInput(_ExecutionNoValidInputOption);
        }

        if (_Completed.Count > 0)
        {
            var completed = _Completed.ToArray();
            _Completed.Clear();
            Context.End(completed);
        }

        if (_IsSummary)
        {
            Pipeline.Writer.WriteObject(_Summary.Values.Where(r => _Outcome == RuleOutcome.All || (r.Outcome & _Outcome) > 0).ToArray(), true);
        }

        Pipeline.Writer.End(Result);
    }

    /// <summary>
    /// Process each run with the target object.
    /// </summary>
    private void ProcessTargetObject(TargetObject targetObject)
    {
        try
        {
            Context.EnterTargetObject(targetObject);
            foreach (var run in _Runs)
            {
                var result = InvokeRun(run, targetObject);
                _Completed.Add(result);
                Pipeline.Writer.WriteObject(result, false);
            }
        }
        finally
        {
            Context.ExitTargetObject();
        }
    }

    /// <summary>
    /// Invoke the run for the target object.
    /// </summary>
    private InvokeResult InvokeRun(IRun run, TargetObject targetObject)
    {
        var result = new InvokeResult(run);
        var ruleCounter = 0;
        var suppressedRuleCounter = 0;
        var suppressionGroupCounter = new Dictionary<ISuppressionInfo, int>(new ISuppressionInfoComparer());

        // Process rule blocks ordered by dependency graph
        foreach (var ruleBlockTarget in run.Rules.GetSingleTarget())
        {
            var ruleBlock = ruleBlockTarget.Value;

            // Enter rule block scope
            var ruleRecord = Context.EnterRuleBlock(run, ruleBlock: ruleBlock);
            ruleCounter++;

            try
            {
                if (Context.Binding != null && Context.Binding.ShouldFilter)
                    continue;

                // Check if dependency failed
                if (ruleBlockTarget.Skipped)
                {
                    ruleRecord.OutcomeReason = RuleOutcomeReason.DependencyFail;
                }
                // Check for suppression
                else if (_SuppressionFilter.Match(id: ((ILanguageBlock)ruleBlock).Id, targetName: ruleRecord.TargetName))
                {
                    ruleRecord.OutcomeReason = RuleOutcomeReason.Suppressed;
                    suppressedRuleCounter++;

                    if (!_IsSummary)
                        Context.RuleSuppressed(ruleId: ruleRecord.RuleId);
                }
                // Check for suppression group
                else if (_SuppressionGroupFilter.TrySuppressionGroup(ruleId: ((ILanguageBlock)ruleBlock).Id, targetObject, out var suppression))
                {
                    ruleRecord.OutcomeReason = RuleOutcomeReason.Suppressed;
                    if (!_IsSummary)
                    {
                        Context.RuleSuppressionGroup(ruleId: ruleRecord.RuleId, suppression);
                    }
                    else
                    {
                        suppressionGroupCounter[suppression] = suppressionGroupCounter.TryGetValue(suppression, out var count) ? ++count : 1;
                    }
                }
                else
                {
                    HostHelper.InvokeRuleBlock(context: Context, ruleBlock: ruleBlock, ruleRecord: ruleRecord);
                    if (ruleRecord.OutcomeReason == RuleOutcomeReason.PreconditionFail)
                        ruleCounter--;
                }

                // Report outcome to dependency graph
                if (ruleRecord.Outcome == RuleOutcome.Pass)
                {
                    ruleBlockTarget.Pass();
                }
                else if (ruleRecord.Outcome == RuleOutcome.Fail)
                {
                    Result.Fail(ruleRecord.Level);
                    ruleBlockTarget.Fail();
                }
                else if (ruleRecord.Outcome == RuleOutcome.Error)
                {
                    Result.HadErrors = true;
                    ruleBlockTarget.Fail();
                }

                AddToSummary(ruleBlock: ruleBlockTarget.Value, outcome: ruleRecord.Outcome);
            }
            finally
            {
                // Exit rule block scope
                Context.ExitRuleBlock(ruleBlock: ruleBlockTarget.Value);
                if (ShouldOutput(ruleRecord.Outcome))
                    result.Add(ruleRecord);
            }
        }

        if (ruleCounter == 0)
            Context.WarnObjectNotProcessed();

        if (_IsSummary)
        {
            if (suppressedRuleCounter > 0)
                Context.WarnRuleCountSuppressed(ruleCount: suppressedRuleCounter);

            foreach (var keyValuePair in suppressionGroupCounter)
                Context.RuleSuppressionGroupCount(suppression: keyValuePair.Key, count: keyValuePair.Value);
        }
        return result;
    }

    private bool ShouldOutput(RuleOutcome outcome)
    {
        return _Outcome == RuleOutcome.All || (outcome & _Outcome) > 0;
    }

    /// <summary>
    /// Add rule result to summary.
    /// </summary>
    private void AddToSummary(IRuleBlock ruleBlock, RuleOutcome outcome)
    {
        if (!_IsSummary || ruleBlock is not ILanguageBlock languageBlock)
            return;

        if (!_Summary.TryGetValue(languageBlock.Id.Value, out var s))
        {
            s = new RuleSummaryRecord(
                ruleId: languageBlock.Id.Value,
                ruleName: ruleBlock.Name,
                tag: ruleBlock.Tag,
                info: ruleBlock.Info
            );
            _Summary.Add(languageBlock.Id.Value, s);
        }
        s.Add(outcome);
    }

    // protected override void Dispose(bool disposing)
    // {
    //     if (!_Disposed)
    //     {
    //         // if (disposing)
    //         //     _Runs.Dispose();

    //         _Disposed = true;
    //     }
    //     base.Dispose(disposing);
    // }
}
