// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using PSRule.Configuration;
using PSRule.Pipeline;

namespace PSRule.Runtime.Binding;

#nullable enable

internal sealed class TargetBindingContext : ITargetBindingContext
{
    private readonly bool _PreferTargetInfo;
    private readonly bool _IgnoreCase;
    private readonly bool _UseQualifiedName;
    private readonly FieldMap? _Field;
    private readonly string[]? _TargetName;
    private readonly string[]? _TargetType;
    private readonly string _NameSeparator;
    private readonly BindTargetMethod? _BindTargetName;
    private readonly BindTargetMethod? _BindTargetType;
    private readonly BindTargetMethod? _BindField;
    private readonly HashSet<string>? _TypeFilter;

    public TargetBindingContext(BindingOption? bindingOption, BindTargetMethod? bindTargetName, BindTargetMethod? bindTargetType, BindTargetMethod? bindField, HashSet<string>? typeFilter)
    {
        _PreferTargetInfo = bindingOption?.PreferTargetInfo ?? BindingOption.Default.PreferTargetInfo!.Value;
        _IgnoreCase = bindingOption?.IgnoreCase ?? BindingOption.Default.IgnoreCase!.Value;
        _UseQualifiedName = bindingOption?.UseQualifiedName ?? BindingOption.Default.UseQualifiedName!.Value;
        _Field = bindingOption?.Field;
        _TargetName = bindingOption?.TargetName;
        _TargetType = bindingOption?.TargetType;
        _NameSeparator = bindingOption?.NameSeparator ?? BindingOption.Default.NameSeparator;
        _BindTargetName = bindTargetName;
        _BindTargetType = bindTargetType;
        _BindField = bindField;
        _TypeFilter = typeFilter;
    }

    public ITargetBindingResult Bind(TargetObject o)
    {
        var targetNamePath = ".";
        var targetName = _PreferTargetInfo && o.TargetName != null ? o.TargetName : _BindTargetName(_TargetName, !_IgnoreCase, _PreferTargetInfo, o.Value, out targetNamePath);
        var targetTypePath = ".";
        var targetType = _PreferTargetInfo && o.TargetType != null ? o.TargetType : _BindTargetType(_TargetType, !_IgnoreCase, _PreferTargetInfo, o.Value, out targetTypePath);
        var shouldFilter = !(_TypeFilter == null || _TypeFilter.Contains(targetType));

        // Bind custom fields
        var field = BindField(_BindField, [_Field], !_IgnoreCase, o.Value);
        return Bind(targetName, targetNamePath, targetType, targetTypePath, field);
    }

    public ITargetBindingResult Bind(object o)
    {
        var targetName = _BindTargetName(_TargetName, !_IgnoreCase, _PreferTargetInfo, o, out var targetNamePath);
        var targetType = _BindTargetType(_TargetType, !_IgnoreCase, _PreferTargetInfo, o, out var targetTypePath);
        var shouldFilter = !(_TypeFilter == null || _TypeFilter.Contains(targetType));

        // Bind custom fields
        var field = BindField(_BindField, [_Field], !_IgnoreCase, o);
        return Bind(targetName, targetNamePath, targetType, targetTypePath, field);
    }

    private TargetBindingResult Bind(string targetName, string targetNamePath, string targetType, string targetTypePath, Hashtable? field)
    {
        var shouldFilter = !(_TypeFilter == null || _TypeFilter.Contains(targetType));

        // Use qualified name
        if (_UseQualifiedName)
            targetName = string.Concat(targetType, _NameSeparator, targetName);

        return new TargetBindingResult
        (
            targetName: targetName,
            targetNamePath: targetNamePath,
            targetType: targetType,
            targetTypePath: targetTypePath,
            shouldFilter: shouldFilter,
            field: field
        );
    }

    /// <summary>
    /// Bind additional fields.
    /// </summary>
    private static ImmutableHashtable? BindField(BindTargetMethod? bindField, FieldMap?[] map, bool caseSensitive, object o)
    {
        if (map == null || map.Length == 0 || bindField == null)
            return null;

        var hashtable = new ImmutableHashtable();
        for (var i = 0; i < map.Length; i++)
        {
            if (map[i] == null || map[i]!.Count == 0)
                continue;

            foreach (var field in map[i]!)
            {
                if (hashtable.ContainsKey(field.Key))
                    continue;

                hashtable.Add(field.Key, bindField(field.Value, caseSensitive, false, o, out _));
            }
        }
        hashtable.Protect();
        return hashtable;
    }
}

#nullable restore
