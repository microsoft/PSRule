// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Management.Automation;
using PSRule.Badges;
using PSRule.Data;
using PSRule.Pipeline;

namespace PSRule.Runtime;

#nullable enable

/// <summary>
/// A set of context properties that are exposed at runtime through the $PSRule variable.
/// </summary>
public sealed class PSRule : ScopedItem
{
    private ITargetSourceCollection? _Source;
    private IInputCollection? _Input;
    private ITargetIssueCollection? _Issue;
    private IBadgeBuilder? _BadgeBuilder;
    private IRepositoryRuntimeInfo? _Repository;

    /// <summary>
    /// Create an empty instance.
    /// </summary>
    public PSRule() { }

    internal PSRule(LegacyRunspaceContext context)
        : base(context) { }

    private sealed class PSRuleSource : ScopedItem, ITargetSourceCollection
    {
        internal PSRuleSource(LegacyRunspaceContext context)
            : base(context) { }

        public TargetSourceInfo? this[string type]
        {
            get
            {
                return GetContext().TargetObject?.Source[type];
            }
        }
    }

    private sealed class PSRuleIssue : ScopedItem, ITargetIssueCollection
    {
        internal PSRuleIssue(LegacyRunspaceContext context)
            : base(context) { }

        public TargetIssueInfo[]? Get(string? type = null)
        {
            return GetContext().TargetObject?.Issue?.Get(type);
        }

        public bool Any(string? type = null)
        {
            return GetContext().TargetObject?.Issue?.Any(type) ?? false;
        }
    }

    private sealed class PSRuleInput : ScopedItem, IInputCollection
    {
        internal PSRuleInput(LegacyRunspaceContext context)
            : base(context) { }

        /// <inheritdoc/>
        public void Add(string path)
        {
            var context = GetContext();
            if (string.IsNullOrEmpty(path) || context.Pipeline.Reader == null)
                return;

            context.Pipeline.Reader.Add(path);
        }
    }

    private sealed class PSRuleRepository : ScopedItem, IRepositoryRuntimeInfo
    {
        private InputFileInfoCollection? _ChangedFiles;

        internal PSRuleRepository(LegacyRunspaceContext context)
            : base(context)
        {
            Url = GetContext().Pipeline.Option.Repository.Url;
            BaseRef = GetContext().Pipeline.Option.Repository.BaseRef;
        }

        /// <inheritdoc/>
        public string Url { get; }

        /// <inheritdoc/>
        public string BaseRef { get; }

        /// <inheritdoc/>
        public IInputFileInfoCollection GetChangedFiles()
        {
            _ChangedFiles ??= new InputFileInfoCollection(Environment.GetWorkingPath(), GitHelper.TryGetChangedFiles(BaseRef, "d", null, out var files) ? files : null);
            return _ChangedFiles;
        }
    }

    /// <summary>
    /// Exposes the badge API for used within conventions.
    /// </summary>
    public IBadgeBuilder Badges
    {
        get
        {
            RequireScope(RunspaceScope.ConventionEnd);
            _BadgeBuilder ??= new BadgeBuilder();
            return _BadgeBuilder;
        }
    }

    /// <summary>
    /// Custom data set by the rule for this target object.
    /// </summary>
    public Hashtable? Data
    {
        get
        {
            RequireScope(RunspaceScope.Rule | RunspaceScope.Precondition | RunspaceScope.ConventionBegin | RunspaceScope.ConventionProcess);
            return GetContext()?.TargetObject?.RequireData();
        }
    }

    /// <summary>
    /// A set of custom fields bound for the target object.
    /// </summary>
    /// <remarks>
    /// This property can only be accessed from a rule or pre-condition.
    /// </remarks>
    /// <exception cref="RuntimeScopeException">
    /// Thrown when accessing this property outside of a rule or pre-condition.
    /// </exception>
    public Hashtable? Field
    {
        get
        {
            RequireScope(RunspaceScope.Rule | RunspaceScope.Precondition);
            return GetContext().RuleRecord?.Field;
        }
    }

    /// <summary>
    /// A list of pre-defined input path that are included.
    /// </summary>
    /// <remarks>
    /// This property can only be accessed from an initialize convention block.
    /// </remarks>
    /// <exception cref="RuntimeScopeException">
    /// Thrown when accessing this property outside of an initialize convention block.
    /// </exception>
    public IInputCollection Input => GetInput();

    /// <summary>
    /// Information about the repository that is currently being used.
    /// </summary>
    public IRepositoryRuntimeInfo Repository => GetRepository();

    /// <summary>
    /// An aggregated set of results from executing PSRule rules.
    /// </summary>
    /// <remarks>
    /// This property can only be accessed from an end convention block.
    /// </remarks>
    /// <exception cref="RuntimeScopeException">
    /// Thrown when accessing this property outside of an end convention block.
    /// </exception>
    public IEnumerable<InvokeResult>? Output
    {
        get
        {
            RequireScope(RunspaceScope.ConventionEnd);
            return GetContext().Output;
        }
    }

    /// <summary>
    /// The source information for the location the target object originated from.
    /// </summary>
    public ITargetSourceCollection Source => GetSource();

    /// <summary>
    /// Any issues reported by downstream tools and annotated to the target object.
    /// </summary>
    public ITargetIssueCollection Issue => GetIssue();

    /// <summary>
    /// The current target object.
    /// </summary>
    public object? TargetObject => GetContext().TargetObject?.Value;

    /// <summary>
    /// The bound name of the target object.
    /// </summary>
    public string? TargetName => GetContext().RuleRecord?.TargetName;

    /// <summary>
    /// The bound type of the target object.
    /// </summary>
    public string? TargetType => GetContext().RuleRecord?.TargetType;

    /// <summary>
    /// The bound scope of the target object.
    /// </summary>
    public string[]? Scope => GetContext().TargetObject?.Scope;

    /// <summary>
    /// Attempts to read content from disk.
    /// </summary>
    public object[] GetContent(object sourceObject)
    {
        if (sourceObject == null)
            return [];

        var baseObject = ExpressionHelpers.GetBaseObject(sourceObject);
        if (baseObject is not InputFileInfo && baseObject is not FileInfo && baseObject is not Uri)
            return [sourceObject];

        var cacheKey = baseObject.ToString();
        if (GetContext().Pipeline.ContentCache.TryGetValue(cacheKey, out var result))
            return result;

        var items = PipelineReceiverActions.DetectInputFormat(new TargetObject(new PSObject(baseObject)), PipelineReceiverActions.PassThru).ToArray();
        result = new object[items.Length];
        for (var i = 0; i < items.Length; i++)
            result[i] = items[i].Value;

        GetContext().Pipeline.ContentCache.Add(cacheKey, result);
        return result;
    }

    /// <summary>
    /// Attempts to read content from disk and extract a field from each object.
    /// </summary>
    public PSObject[] GetContentField(PSObject sourceObject, string field)
    {
        var content = GetContent(sourceObject);
        if (IsEmptyContent(content) || string.IsNullOrEmpty(field))
            return [];

        var result = new List<PSObject>();
        for (var i = 0; i < content.Length; i++)
        {
            if (ObjectHelper.GetPath(null, content[i], field, false, out object value) && value != null)
            {
                if (value is IEnumerable e)
                {
                    foreach (var item in e)
                    {
                        result.Add(PSObject.AsPSObject(item));
                    }
                }
                else
                    result.Add(PSObject.AsPSObject(value));
            }

        }
        return [.. result];
    }

    /// <summary>
    /// Attempts to read content from disk and return the first object or null.
    /// </summary>
    public object? GetContentFirstOrDefault(object sourceObject)
    {
        var content = GetContent(sourceObject);
        return IsEmptyContent(content) ? null : content[0];
    }

    private static bool IsEmptyContent(object[] content)
    {
        return content == null || content.Length == 0;
    }

    /// <summary>
    /// Evaluate an object path expression and returns the resulting objects.
    /// </summary>
    public object[] GetPath(object sourceObject, string path)
    {
        return (!ObjectHelper.GetPath(
            bindingContext: GetContext()?.Pipeline,
            targetObject: sourceObject,
            path: path,
            caseSensitive: false,
            out object[] value)) ? [] : value;
    }

    /// <summary>
    /// Imports source objects into the pipeline for processing.
    /// </summary>
    /// <remarks>
    /// This method can only be called from a convention initialize or begin block.
    /// </remarks>
    /// <param name="sourceObject">One or more objects to import into the pipeline.</param>
    /// <exception cref="RuntimeScopeException">
    /// Thrown when accessing this method outside of a convention initialize or begin block.
    /// </exception>
    public void Import(PSObject[] sourceObject)
    {
        ImportWithType(null, sourceObject);
    }

    /// <summary>
    /// Imports source objects into the pipeline for processing.
    /// </summary>
    /// <remarks>
    /// This method can only be called from a convention initialize or begin block.
    /// </remarks>
    /// <param name="type">The target type to assign to any objects imported into the pipeline.</param>
    /// <param name="sourceObject">One or more objects to import into the pipeline.</param>
    /// <exception cref="RuntimeScopeException">
    /// Thrown when accessing this method outside of a convention initialize or begin block.
    /// </exception>
    public void ImportWithType(string? type, PSObject[] sourceObject)
    {
        if (sourceObject == null || sourceObject.Length == 0)
            return;

        RequireScope(RunspaceScope.ConventionInitialize | RunspaceScope.ConventionBegin);
        for (var i = 0; i < sourceObject.Length; i++)
        {
            if (sourceObject[i] == null)
                continue;

            GetContext().Pipeline.Reader?.Enqueue(sourceObject[i], targetType: type, skipExpansion: true);
        }
    }

    /// <summary>
    /// Add a reusable singleton object into PSRule runtime that can be reference across multiple rules or conventions. To retrieve the singleton call <seealso cref="GetService"/>.
    /// </summary>
    /// <param name="id">A unique identifier for the object.</param>
    /// <param name="service">A instance of the singleton.</param>
    /// <remarks>
    /// If either <paramref name="id"/> or <paramref name="service"/> is null or empty the singleton is ignored.
    /// This method can only be called from a convention initialize block.
    /// </remarks>
    /// <exception cref="RuntimeScopeException">
    /// Thrown when accessing this method outside of a convention initialize block.
    /// </exception>
    public void AddService(string id, object service)
    {
        if (service == null || string.IsNullOrEmpty(id))
            return;

        RequireScope(RunspaceScope.ConventionInitialize);
        GetContext().AddService(id, service);
    }

    /// <summary>
    /// Configure services for dependency injection.
    /// </summary>
    /// <param name="configure"></param>
    /// <exception cref="RuntimeScopeException">
    /// Thrown when accessing this method outside of a convention initialize block.
    /// </exception>
    public void ConfigureServices(Action<IRuntimeServiceCollection>? configure)
    {
        if (configure == null)
            return;

        RequireScope(RunspaceScope.ConventionInitialize);
        GetContext()?.Scope?.ConfigureServices(configure);
    }

    /// <summary>
    /// Retrieve a reusable singleton object from the PSRule runtime that has previously been stored with <see cref="AddService"/>.
    /// </summary>
    /// <param name="id">The unique identifier for the object.</param>
    /// <returns>The singleton instance or null if an object with the specified <paramref name="id"/> was not found.</returns>
    /// <exception cref="RuntimeScopeException">
    /// Thrown when accessing this method outside of PSRule.
    /// </exception>
    public object? GetService(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        RequireScope(RunspaceScope.Runtime);
        return GetContext().GetService(id);
    }

    #region Helper methods

    private ITargetSourceCollection GetSource()
    {
        RequireScope(RunspaceScope.Target);
        _Source ??= new PSRuleSource(GetContext());
        return _Source;
    }

    private IInputCollection GetInput()
    {
        RequireScope(RunspaceScope.ConventionInitialize);
        _Input ??= new PSRuleInput(GetContext());
        return _Input;
    }

    private ITargetIssueCollection GetIssue()
    {
        RequireScope(RunspaceScope.Target);
        _Issue ??= new PSRuleIssue(GetContext());
        return _Issue;
    }

    private IRepositoryRuntimeInfo GetRepository()
    {
        _Repository ??= new PSRuleRepository(GetContext());
        return _Repository;
    }

    #endregion Helper methods
}

#nullable restore
