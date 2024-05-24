// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Host;
using PSRule.Rules;

namespace PSRule.Pipeline;

internal sealed class InvokeRulePipeline : RulePipeline, IPipeline
{
    private readonly RuleOutcome _Outcome;
    private readonly DependencyGraph<RuleBlock> _RuleGraph;

    // A per rule summary of rules that have been processed and the outcome
    private readonly Dictionary<string, RuleSummaryRecord> _Summary;

    private readonly bool _IsSummary;
    private readonly SuppressionFilter _SuppressionFilter;
    private readonly SuppressionFilter _SuppressionGroupFilter;
    private readonly List<InvokeResult> _Completed;

    // Track whether Dispose has been called.
    private bool _Disposed;

    internal InvokeRulePipeline(PipelineContext context, Source[] source, IPipelineWriter writer, RuleOutcome outcome)
        : base(context, source, context.Reader, writer)
    {
        _RuleGraph = HostHelper.GetRuleBlockGraph(Source, Context);
        RuleCount = _RuleGraph.Count;
        if (RuleCount == 0)
            Context.WarnRuleNotFound();

        _Outcome = outcome;
        _IsSummary = context.Option.Output.As.Value == ResultFormat.Summary;
        _Summary = _IsSummary ? new Dictionary<string, RuleSummaryRecord>() : null;
        var allRuleBlocks = _RuleGraph.GetAll();
        var resourceIndex = new ResourceIndex(allRuleBlocks);
        _SuppressionFilter = new SuppressionFilter(Context, context.Option.Suppression, resourceIndex);
        _SuppressionGroupFilter = new SuppressionFilter(Pipeline.SuppressionGroup, resourceIndex);

        _Completed = new List<InvokeResult>();
    }

    public int RuleCount { get; private set; }

    /// <inheritdoc/>
    public override void Process(PSObject sourceObject)
    {
        try
        {
            Reader.Enqueue(sourceObject);
            while (Reader.TryDequeue(out var next))
            {
                // TODO: Temporary workaround to cast interface
                if (next is TargetObject to)
                {

                    var result = ProcessTargetObject(to);
                    _Completed.Add(result);
                    Writer.WriteObject(result, false);
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
        if (_Completed.Count > 0)
        {
            var completed = _Completed.ToArray();
            _Completed.Clear();
            Context.End(completed);
        }

        if (_IsSummary)
            Writer.WriteObject(_Summary.Values.Where(r => _Outcome == RuleOutcome.All || (r.Outcome & _Outcome) > 0).ToArray(), true);

        Writer.End();
    }

    private InvokeResult ProcessTargetObject(TargetObject targetObject)
    {
        try
        {
            Context.EnterTargetObject(targetObject);
            var result = new InvokeResult();
            var ruleCounter = 0;
            var suppressedRuleCounter = 0;
            var suppressionGroupCounter = new Dictionary<ISuppressionInfo, int>(new ISuppressionInfoComparer());

            // Process rule blocks ordered by dependency graph
            foreach (var ruleBlockTarget in _RuleGraph.GetSingleTarget())
            {
                // Enter rule block scope
                Context.EnterLanguageScope(ruleBlockTarget.Value.Source);
                var ruleRecord = Context.EnterRuleBlock(ruleBlock: ruleBlockTarget.Value);
                ruleCounter++;

                try
                {
                    if (Context.Binding.ShouldFilter)
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
                    else if (_SuppressionGroupFilter.TrySuppressionGroup(ruleId: ruleRecord.RuleId, targetObject, out var suppression))
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
                        Result.HadFailures = true;
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
                    Context.ExitRuleBlock();
                    if (ShouldOutput(ruleRecord.Outcome))
                        result.Add(ruleRecord);

                    Context.ExitLanguageScope(ruleBlockTarget.Value.Source);
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
        finally
        {
            Context.ExitTargetObject();
        }
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
