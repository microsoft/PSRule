// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Host;
using PSRule.Options;
using PSRule.Pipeline.Runs;
using PSRule.Rules;
using PSRule.Runtime;

namespace PSRule.Pipeline;

internal sealed class InvokeRulePipeline : RulePipeline, IPipeline
{
    private readonly RuleOutcome _Outcome;
    private readonly DependencyGraph<RuleBlock> _RuleGraph;

    // A per rule summary of rules that have been processed and the outcome
    private readonly Dictionary<string, RuleSummaryRecord> _Summary;

    private readonly bool _IsSummary;
    private SuppressionFilter _SuppressionFilter;
    private SuppressionFilter _SuppressionGroupFilter;
    private readonly List<InvokeResult> _Completed;
    private readonly ExecutionActionPreference _ExecutionNoValidInputOption;

    // Track whether Dispose has been called.
    private bool _Disposed;

    internal InvokeRulePipeline(PipelineContext context, Source[] source, RuleOutcome outcome)
        : base(context, source)
    {
        _RuleGraph = HostHelper.GetRuleBlockGraph(Context);
        RuleCount = _RuleGraph.Count;
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

        var allRuleBlocks = _RuleGraph.GetAll();
        var resourceIndex = new ResourceIndex(allRuleBlocks);

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
            foreach (var run in Context.Runs)
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
        foreach (var ruleBlockTarget in _RuleGraph.GetSingleTarget())
        {
            // Enter rule block scope
            var ruleRecord = Context.EnterRuleBlock(ruleBlock: ruleBlockTarget.Value);
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
                else if (_SuppressionFilter.Match(id: ruleBlockTarget.Value.Id, targetName: ruleRecord.TargetName))
                {
                    ruleRecord.OutcomeReason = RuleOutcomeReason.Suppressed;
                    suppressedRuleCounter++;

                    if (!_IsSummary)
                        Context.RuleSuppressed(ruleId: ruleRecord.RuleId);
                }
                // Check for suppression group
                else if (_SuppressionGroupFilter.TrySuppressionGroup(ruleId: ruleBlockTarget.Value.Id, targetObject, out var suppression))
                {
                    ruleRecord.OutcomeReason = RuleOutcomeReason.Suppressed;
                    if (!_IsSummary)
                        Context.RuleSuppressionGroup(ruleId: ruleRecord.RuleId, suppression);
                    else
                        suppressionGroupCounter[suppression] = suppressionGroupCounter.TryGetValue(suppression, out var count) ? ++count : 1;
                }
                else
                {
                    HostHelper.InvokeRuleBlock(context: Context, ruleBlock: ruleBlockTarget.Value, ruleRecord: ruleRecord);
                    if (ruleRecord.OutcomeReason == RuleOutcomeReason.PreconditionFail)
                        ruleCounter--;
                }

                // Report outcome to dependency graph
                if (ruleRecord.Outcome == RuleOutcome.Pass)
                {
                    ruleBlockTarget.Pass();
                    Context.Pass();
                }
                else if (ruleRecord.Outcome == RuleOutcome.Fail)
                {
                    Result.Fail(ruleRecord.Level);
                    ruleBlockTarget.Fail();
                    Context.Fail();
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
    private void AddToSummary(RuleBlock ruleBlock, RuleOutcome outcome)
    {
        if (!_IsSummary || ruleBlock == null)
            return;

        if (!_Summary.TryGetValue(ruleBlock.Id.Value, out var s))
        {
            s = new RuleSummaryRecord(
                ruleId: ruleBlock.Id.Value,
                ruleName: ruleBlock.Name,
                tag: ruleBlock.Tag,
                info: ruleBlock.Info
            );
            _Summary.Add(ruleBlock.Id.Value, s);
        }
        s.Add(outcome);
    }

    protected override void Dispose(bool disposing)
    {
        if (!_Disposed)
        {
            if (disposing)
                _RuleGraph.Dispose();

            _Disposed = true;
        }
        base.Dispose(disposing);
    }
}
