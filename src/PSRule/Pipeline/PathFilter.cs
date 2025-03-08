// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;

namespace PSRule.Pipeline;

/// <summary>
/// Filters paths based on predefined rules.
/// </summary>
internal sealed class PathFilter : IPathFilter
{
    // Path separators
    private const char SLASH = '/';
    private const char BACKSLASH = '\\';

    // Operators
    private const char STAR = '*'; // Match multiple characters except '/'
    private const char QUESTION = '?'; // Match any character except '/'
    private const char HASH = '#'; // Comment
    private const char EXCLAMATION = '!'; // Include a previously excluded path

    private readonly string _BasePath;
    private readonly PathFilterExpression[] _Expression;
    private readonly bool _MatchResult;

    private PathFilter(string basePath, PathFilterExpression[] expression, bool matchResult)
    {
        _BasePath = NormalDirectoryPath(basePath);
        _Expression = expression ?? [];
        _MatchResult = matchResult;
    }

    #region PathStream

    [DebuggerDisplay("Path = {_Path}, Position = {_Position}, Current = {_Current}")]
    private sealed class PathStream
    {
        private readonly string _Path;
        private int _Position;
        private char _Current;
        private char _Last;

        public PathStream(string path)
        {
            _Path = path;
            Reset();
        }

        /// <summary>
        /// Resets the cursor to the start of the path stream.
        /// </summary>
        public void Reset()
        {
            _Position = -1;
            _Current = char.MinValue;
            _Last = char.MinValue;
            if (_Path[0] == EXCLAMATION)
                Next();
        }

        /// <summary>
        /// Move to the next character.
        /// </summary>
        public bool Next()
        {
            _Position++;
            if (_Position >= _Path.Length)
            {
                _Last = _Path.Length == 0 ? char.MinValue : _Path[_Path.Length - 1];
                _Current = char.MinValue;
                return false;
            }
            _Last = _Position <= 0 ? char.MinValue : _Path[_Position - 1];
            _Current = _Path[_Position];
            return true;
        }

        public bool TryMatch(PathStream other, int offset)
        {
            return other.Peak(offset, out var c) && IsMatch(c);
        }

        public bool IsUnmatchedSingle(PathStream other, int offset)
        {
            return other.Peak(offset, out var c) && IsWildcardQ(c) && other.Peak(offset + 1, out var cnext) && IsMatch(cnext);
        }

        private bool IsMatch(char c)
        {
            return _Current == c ||
                (IsSeparator(_Current) && IsSeparator(c)) ||
                (!IsSeparator(_Current) && IsWildcardQ(c));
        }

        /// <summary>
        /// Determine if the current character sequence is ** or **/.
        /// </summary>
        public bool IsAnyMatchEnding(int offset = 0)
        {
            if (!IsWildcardAA(offset))
                return false;

            var pos = _Position + offset;

            // Ends in **
            if (pos + 1 == _Path.Length - 1)
                return true;

            // Ends in **/
            return pos + 2 == _Path.Length - 1 && IsSeparator(_Path[pos + 2]);
        }

        public bool SkipMatchAA()
        {
            if (!IsWildcardAA())
                return false;

            Skip(2); // Skip **
            SkipSeparator(); // Skip **/
            SkipMatchAA(); // Skip **/**/
            return true;
        }

        public bool SkipMatchA()
        {
            if (!IsWildcardA())
                return false;

            Skip(1);
            return true;
        }

        /// <summary>
        /// Determines if the last character was a separator.
        /// </summary>
        private bool LastIsSeparator()
        {
            return IsSeparator(_Last);
        }

        /// <summary>
        /// Skip a number of characters from the current position.
        /// </summary>
        /// <param name="count">The amount of character to skip from the current position.</param>
        /// <returns>Returns <c>true</c> if there is more characters to match.</returns>
        private bool Skip(int count)
        {
            if (count > 1)
            {
                _Position += count - 1;
            }
            return Next();
        }

        [DebuggerStepThrough]
        private void SkipSeparator()
        {
            if (IsSeparator(_Current))
                Next();
        }

        /// <summary>
        /// Determine if the current character sequence is **.
        /// </summary>
        [DebuggerStepThrough]
        private bool IsWildcardAA(int offset = 0)
        {
            var pos = _Position + offset;
            return pos + 1 < _Path.Length && _Path[pos] == STAR && _Path[pos + 1] == STAR;
        }

        [DebuggerStepThrough]
        private bool IsWildcardA(int offset = 0)
        {
            var pos = _Position + offset;
            return pos < _Path.Length && _Path[pos] == STAR;
        }

        [DebuggerStepThrough]
        private static bool IsWildcardQ(char c)
        {
            return c == QUESTION;
        }

        [DebuggerStepThrough]
        private static bool IsSeparator(char c)
        {
            return c == SLASH || c == BACKSLASH;
        }

        /// <summary>
        /// Match **
        /// </summary>
        public bool TryMatchAA(PathStream other, int start)
        {
            var offset = start;
            do
            {
                if (IsUnmatchedSingle(other, offset))
                    offset++;

                // Determine if fully matched to end or the next any match
                if (other.IsWildcardAA(offset))
                {
                    other.Skip(offset);
                    return true;
                }
                else if (other.IsWildcardA(offset) && TryMatchA(other, offset + 1))
                {
                    return true;
                }

                // Try to match the remaining
                if (TryMatch(other, offset))
                {
                    offset++;
                }
                else
                {
                    offset = start;
                }

                if (offset + other._Position >= other._Path.Length)
                {
                    Next();
                    var endOfMatch = !other.Skip(offset);
                    return _Position == _Path.Length || endOfMatch && other.LastIsSeparator();
                }
            } while (Next());
            return false;
        }

        /// <summary>
        /// Match *
        /// </summary>
        public bool TryMatchA(PathStream other, int start)
        {
            var offset = start;
            do
            {
                // Determine if fully matched to end or the next any match
                if (other.IsWildcardAA(offset))
                {
                    other.Skip(offset);
                    return true;
                }

                // Try to match the remaining
                if (TryMatch(other, offset))
                {
                    offset++;
                }
                else
                {
                    offset = start;
                }

                if (offset + other._Position >= other._Path.Length)
                {
                    Next();
                    other.Skip(offset);
                    return _Position == _Path.Length;
                }
            } while (Next());
            return false;
        }

        private bool Peak(int offset, out char c)
        {
            if (offset + _Position >= _Path.Length)
            {
                c = char.MinValue;
                return false;
            }
            c = _Path[offset + _Position];
            return true;
        }
    }

    #endregion PathStream

    #region PathFilterExpression

    [DebuggerDisplay("{_Expression}")]
    private sealed class PathFilterExpression
    {
        private readonly PathStream _Expression;
        private readonly bool _Include;

        private PathFilterExpression(string expression, bool include)
        {
            _Expression = new PathStream(expression);
            _Include = include;
        }

        public static PathFilterExpression Create(string expression)
        {
            if (string.IsNullOrEmpty(expression))
                throw new ArgumentNullException(nameof(expression));

            var actualInclude = true;
            if (expression[0] == EXCLAMATION)
                actualInclude = false;

            return new PathFilterExpression(expression, actualInclude);
        }

        /// <summary>
        /// Determine if the path matches the expression.
        /// </summary>
        public void Match(string path, ref bool include)
        {
            // Only process if the result would change
            if (string.IsNullOrWhiteSpace(path) || _Include == include || !Match(path))
                return;

            include = !include;
        }

        /// <summary>
        /// Determines if the path matches the expression.
        /// </summary>
        private bool Match(string path)
        {
            _Expression.Reset();
            var stream = new PathStream(path);
            while (stream.Next() && _Expression.Next())
            {
                // Match characters
                if (stream.TryMatch(_Expression, 0))
                    continue;

                // Skip ? when zero characters are being matched
                else if (stream.IsUnmatchedSingle(_Expression, 0))
                    _Expression.Next();

                // Match ending wildcards e.g. src/** or src/**/
                else if (_Expression.IsAnyMatchEnding())
                    break;

                // Match ending with depth e.g. src/**/bin/
                else if (_Expression.SkipMatchAA())
                {
                    if (!stream.TryMatchAA(_Expression, 0))
                        return false;
                }

                // Match wildcard *
                else if (_Expression.SkipMatchA())
                {
                    if (!stream.TryMatchA(_Expression, 0))
                        return false;
                }

                else return false;
            }
            return true;
        }
    }

    #endregion PathFilterExpression

    #region Public methods

    /// <summary>
    /// Create a path filter from a base path.
    /// </summary>
    /// <param name="basePath">The base path for comparing relative paths.</param>
    /// <param name="expression">An expression to match.</param>
    /// <param name="matchResult">
    /// Determine if the expressions should match or ignore paths.
    /// When <paramref name="matchResult"/> is <c>true</c> only paths that match the expressions return <c>true</c>.
    /// When <paramref name="matchResult"/> is <c>false</c> only paths that do not match the expressions return <c>true</c>.
    /// </param>
    /// <returns>Returns a <see cref="PathFilter"/>.</returns>
    public static PathFilter Create(string basePath, string expression, bool matchResult = true)
    {
        return !ShouldSkipExpression(expression)
            ? new PathFilter(
                basePath,
                [PathFilterExpression.Create(expression)],
                matchResult)
            : new PathFilter(basePath, null, matchResult);
    }

    /// <summary>
    /// Create a path filter from a base path.
    /// </summary>
    /// <param name="basePath">The base path for comparing relative paths.</param>
    /// <param name="expression">One or more expressions to match.</param>
    /// <param name="matchResult">
    /// Determine if the expressions should match or ignore paths.
    /// When <paramref name="matchResult"/> is <c>true</c> only paths that match the expressions return <c>true</c>.
    /// When <paramref name="matchResult"/> is <c>false</c> only paths that do not match the expressions return <c>true</c>.
    /// </param>
    /// <returns>Returns a <see cref="PathFilter"/>.</returns>
    public static PathFilter Create(string basePath, string[] expression, bool matchResult = true)
    {
        var result = new List<PathFilterExpression>(expression.Length);
        for (var i = 0; i < expression.Length; i++)
            if (!ShouldSkipExpression(expression[i]))
                result.Add(PathFilterExpression.Create(expression[i]));

        return result.Count == 0
            ? new PathFilter(basePath, null, matchResult)
            : new PathFilter(basePath, [.. result], matchResult);
    }

    /// <summary>
    /// Determine if the specific path is matched.
    /// </summary>
    /// <param name="path">The path to evaluate.</param>
    public bool Match(string path)
    {
        var start = 0;

        // Check if path is within base path
        if (Path.IsPathRooted(path))
        {
            if (!path.StartsWith(_BasePath))
                return !_MatchResult;

            start = _BasePath.Length;
        }
        var cleanPath = start > 0 ? path.Remove(0, start) : path;

        // Include unless excluded
        var result = false;

        // Compare expressions
        for (var i = 0; i < _Expression.Length; i++)
            _Expression[i].Match(cleanPath, ref result);

        // Flip the result if a match should return false
        return _MatchResult ? result : !result;
    }

    #endregion Public methods

    private static bool ShouldSkipExpression(string expression)
    {
        return string.IsNullOrEmpty(expression) || expression[0] == HASH;
    }

    private static string NormalDirectoryPath(string path)
    {
        var c = path[path.Length - 1];
        return c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar
            ? path
            : string.Concat(path, Path.DirectorySeparatorChar);
    }
}
