// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;

namespace PSRule.Runtime.ObjectPath;

/// <summary>
/// An expression function that returns one or more values when successful.
/// </summary>
internal delegate bool PathExpressionFn(IPathExpressionContext context, object input, out IEnumerable<object> value, out bool enumerable);

/// <summary>
/// A function for filter objects that simply returns true or false.
/// </summary>
internal delegate bool PathExpressionFilterFn(IPathExpressionContext context, object input);

/// <summary>
/// A path expression using JSONPath inspired syntax.
/// </summary>
[DebuggerDisplay("{Path}")]
public sealed class PathExpression
{
    private readonly PathExpressionFn _Expression;

    [DebuggerStepThrough]
    private PathExpression(string path, PathExpressionFn expression, bool isArray)
    {
        Path = path;
        IsArray = isArray;
        _Expression = expression;
    }

    /// <summary>
    /// The path expression.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Specifies if the result is an array.
    /// </summary>
    public bool IsArray { get; }

    /// <summary>
    /// Create the expression from the specified path.
    /// </summary>
    public static PathExpression Create(string path)
    {
        var tokens = PathTokenizer.Get(path);
        var builder = new PathExpressionBuilder();
        return new PathExpression(path, builder.Build(tokens), builder.IsArray);
    }

    /// <summary>
    /// Use the expression to return an array of results.
    /// </summary>
    [DebuggerStepThrough]
    public bool TryGet(object o, bool caseSensitive, out object[]? value)
    {
        value = null;
        if (!TryGet(o, caseSensitive, out var result, out _))
            return false;

        value = result.ToArray();
        return true;
    }

    /// <summary>
    /// Use the expression to return a single or an array of results.
    /// </summary>
    [DebuggerStepThrough]
    public bool TryGet(object o, bool caseSensitive, out object? value)
    {
        value = null;
        if (!TryGet(o, caseSensitive, out var result, out var enumerable))
            return false;

        var items = result.ToArray();
        value = IsArray || enumerable ? items : items[0];
        return true;
    }

    /// <summary>
    /// Use the path to selector one or more values from the object.
    /// </summary>
    /// <param name="o">The object to navigate the path for.</param>
    /// <param name="caseSensitive">Determines if member name matching is case-sensitive.</param>
    /// <param name="value">The values selected from the object.</param>
    /// <param name="enumerable">Determines if <paramref name="value"/> is enumerable.</param>
    /// <returns>Returns true when the path exists within the object. Returns false if the path does not exist.</returns>
    private bool TryGet(object o, bool caseSensitive, out IEnumerable<object> value, out bool enumerable)
    {
        var context = new PathExpressionContext(o, caseSensitive);
        return _Expression.Invoke(context, o, out value, out enumerable);
    }
}
