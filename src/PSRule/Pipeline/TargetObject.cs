// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Diagnostics;
using System.Management.Automation;
using Newtonsoft.Json.Linq;
using PSRule.Data;

namespace PSRule.Pipeline;

/// <summary>
/// An instance of an object that is processed by PSRule.
/// </summary>
[DebuggerDisplay("Type = {Type}, Name = {Name}")]
public sealed class TargetObject : ITargetObject
{
    private readonly Dictionary<Type, object> _Annotations;

    private Hashtable? _Data;

    internal TargetObject(PSObject o)
        : this(o, null) { }

    internal TargetObject(PSObject o, TargetSourceCollection? source)
    {
        o.ConvertTargetInfoProperty();
        o.ConvertTargetInfoType();
        Source = ReadSourceInfo(o, source);
        Issue = ReadIssueInfo(o, null);
        Name = o.GetTargetName();
        Type = o.GetTargetType();
        Scope = o.GetScope();
        Path = o.GetTargetPath();
        Value = Convert(o);
        _Annotations = [];
    }

    internal TargetObject(PSObject o, string? name = null, string? type = null, string[]? scope = null)
        : this(o, null)
    {
        if (name != null && !string.IsNullOrWhiteSpace(name))
            Name = name;

        if (type != null && !string.IsNullOrWhiteSpace(type))
            Type = type;

        if (scope != null && scope.Length > 0)
            Scope = scope;
    }

    /// <summary>
    /// The actual value of the object.
    /// </summary>
    internal object Value { get; }

    internal TargetSourceCollection Source { get; private set; }

    internal TargetIssueCollection Issue { get; private set; }

    /// <inheritdoc/>
    public string? Name { get; }

    /// <inheritdoc/>
    public string? Type { get; }

    /// <inheritdoc/>
    public string[]? Scope { get; }

    /// <inheritdoc/>
    public string? Path { get; }

    IEnumerable<TargetSourceInfo> ITargetObject.Source => Source.GetSourceInfo() ?? [];

    object ITargetObject.Value => Value;

    ITargetSourceMap? ITargetObject.SourceMap => null;

    /// <inheritdoc/>
    public Hashtable? GetData()
    {
        return _Data == null || _Data.Count == 0 ? null : _Data;
    }

    internal Hashtable RequireData()
    {
        _Data ??= [];
        return _Data;
    }

    /// <inheritdoc/>
    public TAnnotation? GetAnnotation<TAnnotation>() where TAnnotation : class
    {
        return _Annotations.TryGetValue(typeof(TAnnotation), out var value) ? (TAnnotation)value : null;
    }

    /// <inheritdoc/>
    public void SetAnnotation<TAnnotation>(TAnnotation value) where TAnnotation : class
    {
        _Annotations[typeof(TAnnotation)] = value ?? throw new ArgumentNullException(nameof(value));
    }

    private static TargetSourceCollection ReadSourceInfo(PSObject o, TargetSourceCollection? source)
    {
        var result = source ?? new TargetSourceCollection();
        if (ExpressionHelpers.GetBaseObject(o) is ITargetInfo targetInfo)
            result.Add(targetInfo.Source);

        result.AddRange(o.GetSourceInfo());
        return result;
    }

    private static TargetIssueCollection ReadIssueInfo(PSObject o, TargetIssueCollection? issue)
    {
        var result = issue ?? new TargetIssueCollection();
        result.AddRange(o.GetIssueInfo());
        return result;
    }

    private static PSObject Convert(PSObject o)
    {
        return o.BaseObject is JToken token ? JsonHelper.ToPSObject(token) : o;
    }
}
