// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Data;
using PSRule.Definitions.Baselines;
using PSRule.Definitions.Expressions;
using PSRule.Definitions.Selectors;
using PSRule.Pipeline;
using PSRule.Resources;
using PSRule.Runtime;

namespace PSRule.Definitions.Rules;

/// <summary>
/// An enhanced rule filter that supports both traditional filtering and selector-based filtering.
/// This filter is used specifically for baseline rule selection.
/// </summary>
internal sealed class BaselineRuleFilter : IResourceFilter
{
    private readonly RuleFilter _baseFilter;
    private readonly LanguageIf? _selector;
    private readonly IExpressionContext? _context;

    /// <summary>
    /// Create a baseline rule filter that combines traditional filtering with selector support.
    /// </summary>
    /// <param name="include">Only accept these rules by name.</param>
    /// <param name="tag">Only accept rules that have these tags.</param>
    /// <param name="exclude">Rule that are always excluded by name.</param>
    /// <param name="includeLocal">Determine if local rules are automatically included.</param>
    /// <param name="labels">Only accept rules that have these labels.</param>
    /// <param name="selector">An optional selector expression to dynamically filter rules.</param>
    /// <param name="context">Expression context for selector evaluation.</param>
    public BaselineRuleFilter(string[] include, Hashtable tag, string[] exclude, bool? includeLocal, ResourceLabels labels, LanguageIf? selector, IExpressionContext? context)
    {
        _baseFilter = new RuleFilter(include, tag, exclude, includeLocal, labels);
        _selector = selector;
        _context = context;
    }

    /// <summary>
    /// Create a baseline rule filter from existing RuleOption and optional selector.
    /// </summary>
    public static BaselineRuleFilter FromRuleOption(RuleOption ruleOption, LanguageIf? selector, IExpressionContext? context)
    {
        return new BaselineRuleFilter(
            ruleOption.Include, 
            ruleOption.Tag, 
            ruleOption.Exclude, 
            ruleOption.IncludeLocal, 
            ruleOption.Labels, 
            selector, 
            context);
    }

    ResourceKind IResourceFilter.Kind => ResourceKind.Rule;

    /// <summary>
    /// Matches if the rule passes both traditional filters and selector evaluation.
    /// </summary>
    /// <returns>Return true if rule is matched, otherwise false.</returns>
    public bool Match(IResource resource)
    {
        // First apply traditional filtering
        if (!_baseFilter.Match(resource))
            return false;

        // If no selector is defined, we're done
        if (_selector == null || _context == null)
            return true;

        // Evaluate the selector against the rule
        return EvaluateSelector(resource);
    }

    private bool EvaluateSelector(IResource resource)
    {
        try
        {
            // Convert the rule to a target object for selector evaluation
            var targetObject = CreateRuleTargetObject(resource);
            var visitor = new SelectorVisitor(_context, _selector.Expression);
            return visitor.Match(targetObject);
        }
        catch
        {
            // If selector evaluation fails, default to excluding the rule
            return false;
        }
    }

    /// <summary>
    /// Create a target object from a rule resource that can be evaluated by selectors.
    /// This exposes rule properties as fields that can be queried by expressions.
    /// </summary>
    private static ITargetObject CreateRuleTargetObject(IResource resource)
    {
        var properties = new PSObject();
        
        // Add basic rule properties
        properties.Properties.Add(new PSNoteProperty("Name", resource.Name));
        properties.Properties.Add(new PSNoteProperty("Module", resource.Module));
        properties.Properties.Add(new PSNoteProperty("Kind", resource.Kind.ToString()));
        properties.Properties.Add(new PSNoteProperty("ApiVersion", resource.ApiVersion));
        
        // Add help information
        if (resource.Info != null)
        {
            properties.Properties.Add(new PSNoteProperty("Synopsis", resource.Info.Synopsis?.Text));
            properties.Properties.Add(new PSNoteProperty("Description", resource.Info.Description?.Text));
            properties.Properties.Add(new PSNoteProperty("DisplayName", resource.Info.DisplayName));
        }

        // Add tags if available
        if (resource.Tags != null)
        {
            var tags = new PSObject();
            foreach (var tag in resource.Tags)
            {
                tags.Properties.Add(new PSNoteProperty(tag.Key.ToString(), tag.Value));
            }
            properties.Properties.Add(new PSNoteProperty("Tags", tags));
        }

        // Add labels if available
        if (resource.Labels != null)
        {
            var labels = new PSObject();
            foreach (var label in resource.Labels)
            {
                labels.Properties.Add(new PSNoteProperty(label.Key, label.Value));
            }
            properties.Properties.Add(new PSNoteProperty("Labels", labels));
        }

        // Add rule-specific properties if this is a rule
        if (resource is IRuleV1 rule)
        {
            properties.Properties.Add(new PSNoteProperty("Level", rule.Level.ToString()));
            properties.Properties.Add(new PSNoteProperty("Recommendation", rule.Recommendation?.Text));
            
            // Add severity as both Level and Severity for compatibility
            properties.Properties.Add(new PSNoteProperty("Severity", rule.Level.ToString()));
        }

        // Add annotations from metadata
        if (resource.Metadata?.Annotations != null)
        {
            var annotations = new PSObject();
            foreach (var annotation in resource.Metadata.Annotations)
            {
                annotations.Properties.Add(new PSNoteProperty(annotation.Key, annotation.Value));
            }
            properties.Properties.Add(new PSNoteProperty("Annotations", annotations));
        }

        return new TargetObject(properties);
    }
}