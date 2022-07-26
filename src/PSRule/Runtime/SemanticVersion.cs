// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace PSRule.Runtime
{
    /// <summary>
    /// A helper for comparing semantic version strings.
    /// </summary>
    internal static class SemanticVersion
    {
        private const char MINOR = '^';
        private const char PATCH = '~';
        private const char EQUAL = '=';
        private const char VUPPER = 'V';
        private const char VLOWER = 'v';
        private const char GREATER = '>';
        private const char LESS = '<';
        private const char SEPARATOR = '.';
        private const char DASH = '-';
        private const char PLUS = '+';
        private const char ZERO = '0';
        private const char PIPE = '|';
        private const char SPACE = ' ';
        private const char AT = '@';

        private const string FLAG_PRERELEASE = "@prerelease ";
        private const string FLAG_PRE = "@pre ";

        [Flags]
        internal enum ComparisonOperator
        {
            None = 0,

            /// <summary>
            /// Major.Minor.Patch bits must match.
            /// </summary>
            Equals = 1,

            /// <summary>
            /// Major.Minor bits must match, Patch can equal to or greater.
            /// </summary>
            PatchUplift = 2,

            /// <summary>
            /// Major bit must match, Minor.Patch can be equal to or greater.
            /// </summary>
            MinorUplift = 4,

            GreaterThan = 8,

            LessThan = 16
        }

        internal enum JoinOperator
        {
            None = 0,
            And = 1,
            Or = 2
        }

        [Flags]
        internal enum ConstraintModifier
        {
            None = 0,

            Prerelease = 1
        }

        internal interface IConstraint
        {
            bool Equals(Version version);
        }

        internal sealed class VersionConstraint : IConstraint
        {
            private List<ConstraintExpression> _Constraints;

            public bool Equals(Version version)
            {
                if (_Constraints == null || _Constraints.Count == 0)
                    return true;

                var match = false;
                var i = 0;
                while (!match && i < _Constraints.Count)
                {
                    var result = _Constraints[i].Equals(version);

                    // True OR
                    if (result && _Constraints[i].Join == JoinOperator.Or)
                        return true;

                    // True AND
                    if (result && _Constraints[i].Join == JoinOperator.And && ++i < _Constraints.Count)
                        continue;

                    // False OR
                    if (_Constraints[i].Join == JoinOperator.Or && ++i < _Constraints.Count)
                        continue;

                    // False AND
                    while (++i < _Constraints.Count)
                    {
                        // Move to after the next OR.
                        if (_Constraints[i].Join == JoinOperator.Or)
                        {
                            i++;
                            continue;
                        }
                    }
                }
                return false;
            }

            internal void Join(int major, int minor, int patch, PR prid, ComparisonOperator flag, JoinOperator join, bool includePrerelease)
            {
                if (_Constraints == null)
                    _Constraints = new List<ConstraintExpression>();

                _Constraints.Add(new ConstraintExpression(
                    major,
                    minor,
                    patch,
                    prid,
                    flag,
                    join == JoinOperator.None ? JoinOperator.Or : join,
                    includePrerelease
                ));
            }
        }

        [DebuggerDisplay("{_Major}.{_Minor}.{_Patch}")]
        internal sealed class ConstraintExpression : IConstraint
        {
            private readonly ComparisonOperator _Flag;
            private readonly int _Major;
            private readonly int _Minor;
            private readonly int _Patch;
            private readonly PR _PRID;
            private readonly bool _IncludePrerelease;

            internal ConstraintExpression(int major, int minor, int patch, PR prid, ComparisonOperator flag, JoinOperator join, bool includePrerelease)
            {
                _Flag = flag == ComparisonOperator.None ? ComparisonOperator.Equals : flag;
                _Major = major;
                _Minor = minor;
                _Patch = patch;
                _PRID = prid;
                Join = join;
                _IncludePrerelease = includePrerelease;
            }

            public bool Stable => IsStable(_PRID);

            public JoinOperator Join { get; }

            public static bool TryParse(string value, out IConstraint constraint)
            {
                return TryParseConstraint(value, out constraint);
            }

            public bool Equals(System.Version version)
            {
                return Equals(version.Major, version.Minor, version.Build, null);
            }

            public bool Equals(Version version)
            {
                return Equals(version.Major, version.Minor, version.Patch, version.Prerelease);
            }

            public bool Equals(int major, int minor, int patch, PR prid)
            {
                if (_Flag == ComparisonOperator.Equals)
                    return EQ(major, minor, patch, prid);

                // Fail when pre-release should not be included
                if (GuardPRID(prid))
                    return false;

                // Fail when major is less
                if (GuardMajor(major))
                    return false;

                // Fail when patch is less
                if (GuardPatch(minor, patch))
                    return false;

                // Fail when minor is less
                if (GuardMinor(minor, patch))
                    return false;

                // Fail when not greater
                if (GuardGreater(major, minor, patch, prid))
                    return false;

                // Fail when not greater or equal to
                if (GuardGreaterOrEqual(major, minor, patch, prid))
                    return false;

                // Fail when not less
                if (GaurdLess(major, minor, patch, prid))
                    return false;

                // Fail with not less or equal to
                return !GuardLessOrEqual(major, minor, patch, prid);
            }

            private bool GuardLessOrEqual(int major, int minor, int patch, PR prid)
            {
                return _Flag == (ComparisonOperator.LessThan | ComparisonOperator.Equals) && !(LT(major, minor, patch, prid) || EQ(major, minor, patch, prid));
            }

            private bool GaurdLess(int major, int minor, int patch, PR prid)
            {
                return _Flag == ComparisonOperator.LessThan && !LT(major, minor, patch, prid);
            }

            private bool GuardGreaterOrEqual(int major, int minor, int patch, PR prid)
            {
                return _Flag == (ComparisonOperator.GreaterThan | ComparisonOperator.Equals) && !(GT(major, minor, patch, prid) || EQ(major, minor, patch, prid));
            }

            private bool GuardGreater(int major, int minor, int patch, PR prid)
            {
                return _Flag == ComparisonOperator.GreaterThan && !GT(major, minor, patch, prid);
            }

            private bool GuardMinor(int minor, int patch)
            {
                return _Flag == ComparisonOperator.MinorUplift && (minor < _Minor || (minor == _Minor && patch < _Patch));
            }

            private bool GuardPatch(int minor, int patch)
            {
                return _Flag == ComparisonOperator.PatchUplift && (minor != _Minor || patch < _Patch);
            }

            private bool GuardMajor(int major)
            {
                return (_Flag == ComparisonOperator.MinorUplift || _Flag == ComparisonOperator.PatchUplift) && major != _Major;
            }

            private bool GuardPRID(PR prid)
            {
                return !_IncludePrerelease && Stable && !IsStable(prid);
            }

            private bool EQ(int major, int minor, int patch, PR prid)
            {
                return EQCore(major, minor, patch) && PR(prid) == 0;
            }

            private bool EQCore(int major, int minor, int patch)
            {
                return (_Major == -1 || _Major == major) &&
                    (_Minor == -1 || _Minor == minor) &&
                    (_Patch == -1 || _Patch == patch);
            }

            private bool GTCore(int major, int minor, int patch)
            {
                return (major > _Major) ||
                    (major == _Major && minor > _Minor) ||
                    (major == _Major && minor == _Minor && patch > _Patch);
            }

            private bool LTCore(int major, int minor, int patch)
            {
                return (major < _Major) ||
                    (major == _Major && minor < _Minor) ||
                    (major == _Major && minor == _Minor && patch < _Patch);
            }

            /// <summary>
            /// Greater Than.
            /// </summary>
            private bool GT(int major, int minor, int patch, PR prid)
            {
                return !IsStable(prid) && !_IncludePrerelease
                    ? EQCore(major, minor, patch) && PR(prid) < 0
                    : GTCore(major, minor, patch) || (EQCore(major, minor, patch) && PR(prid) < 0);
            }

            /// <summary>
            /// Less Than.
            /// </summary>
            private bool LT(int major, int minor, int patch, PR prid)
            {
                return !IsStable(prid) && !_IncludePrerelease
                    ? EQCore(major, minor, patch) && PR(prid) > 0
                    : LTCore(major, minor, patch) || (EQCore(major, minor, patch) && PR(prid) > 0);
            }

            /// <summary>
            /// Compare pre-release.
            /// </summary>
            private int PR(PR prid)
            {
                return _PRID.CompareTo(prid);
            }

            private static bool IsStable(PR prid)
            {
                return prid == null || prid.Stable;
            }
        }

        internal sealed class Version : IComparable<Version>, IEquatable<Version>
        {
            public readonly int Major;
            public readonly int Minor;
            public readonly int Patch;
            public readonly PR Prerelease;
            public readonly string Build;

            internal Version(int major, int minor, int patch, PR prerelease, string build)
            {
                Major = major;
                Minor = minor;
                Patch = patch;
                Prerelease = prerelease;
                Build = build;
            }

            public static bool TryParse(string value, out Version version)
            {
                return TryParseVersion(value, out version);
            }

            public override string ToString()
            {
                return string.Concat(Major, '.', Minor, '.', Patch);
            }

            public override bool Equals(object obj)
            {
                return obj is Version version && Equals(version);
            }

            public override int GetHashCode()
            {
                unchecked // Overflow is fine
                {
                    var hash = 17;
                    hash = hash * 23 + Major.GetHashCode();
                    hash = hash * 23 + Minor.GetHashCode();
                    hash = hash * 23 + Patch.GetHashCode();
                    hash = hash * 23 + (Prerelease != null ? Prerelease.GetHashCode() : 0);
                    hash = hash * 23 + (Build != null ? Build.GetHashCode() : 0);
                    return hash;
                }
            }

            public bool Equals(Version other)
            {
                return other != null &&
                    Equals(other.Major, other.Minor, other.Patch);
            }

            public bool Equals(int major, int minor, int patch)
            {
                return major == Major &&
                    minor == Minor &&
                    patch == Patch;
            }

            public int CompareTo(Version other)
            {
                if (other == null)
                    return 1;

                if (Major != other.Major)
                    return Major > other.Major ? 32 : -32;

                if (Minor != other.Minor)
                    return Minor > other.Minor ? 16 : -16;

                if (Patch != other.Patch)
                    return Patch > other.Patch ? 8 : -8;

                return 0;
            }
        }

        [DebuggerDisplay("{Value}")]
        internal sealed class PR
        {
            internal static readonly PR Empty = new PR();
            private static readonly char[] SEPARATORS = new char[] { SEPARATOR };

            private readonly string[] _Identifiers;

            private PR()
            {
                Value = string.Empty;
                _Identifiers = null;
            }

            internal PR(string value)
            {
                Value = string.IsNullOrEmpty(value) ? string.Empty : value;
                _Identifiers = string.IsNullOrEmpty(value) ? null : value.Split(SEPARATORS, StringSplitOptions.RemoveEmptyEntries);
            }

            public string Value { get; }

            public bool Stable => _Identifiers == null;

            public int CompareTo(PR pr)
            {
                if (pr == null || pr.Stable)
                    return Stable ? 0 : -1;
                else if (Stable)
                    return 1;

                var i = -1;
                var left = _Identifiers;
                var right = pr._Identifiers;

                while (++i < left.Length && i < right.Length)
                {
                    var leftNumeric = false;
                    var rightNumeric = false;
                    if (long.TryParse(left[i], out var l))
                        leftNumeric = true;

                    if (long.TryParse(right[i], out var r))
                        rightNumeric = true;

                    if (leftNumeric != rightNumeric)
                        return leftNumeric ? -1 : 1;

                    if (leftNumeric && rightNumeric && l == r)
                        continue;

                    if (leftNumeric && rightNumeric)
                        return l.CompareTo(r);

                    var result = string.Compare(left[i], right[i], StringComparison.Ordinal);
                    if (result == 0)
                        continue;

                    return result;
                }
                if (left.Length == right.Length)
                    return 0;

                return left.Length > right.Length ? 1 : -1;
            }

            public override bool Equals(object obj)
            {
                return obj is PR prerelease && Value.Equals(prerelease.Value);
            }

            public override int GetHashCode()
            {
                return Value.GetHashCode();
            }

            public override string ToString()
            {
                return Value.ToString();
            }
        }

        [DebuggerDisplay("Current = {_Current}, Position = {_Position}, Value = {_Value}")]
        private sealed class VersionStream
        {
            private readonly string _Value;
            private int _Position;
            private char _Current;

            internal VersionStream(string value)
            {
                _Value = value;
                _Position = 0;
                _Current = _Value.Length > 0 ? _Value[0] : char.MinValue;
            }

            internal bool EOF => _Position >= _Value.Length;

            internal void Next()
            {
                Next(1);
            }

            private void Next(int count)
            {
                _Position += count;
                if (EOF || _Position >= _Value.Length)
                    return;

                _Current = _Value[_Position];
            }

            internal void Operator(out ComparisonOperator comparison)
            {
                comparison = ComparisonOperator.None;
                while (!EOF && IsConstraint(_Current))
                {
                    if (_Current == MINOR)
                        comparison = ComparisonOperator.MinorUplift;
                    else if (_Current == PATCH)
                        comparison = ComparisonOperator.PatchUplift;
                    else if (_Current == EQUAL)
                        comparison |= ComparisonOperator.Equals;
                    else if (_Current == GREATER)
                        comparison |= ComparisonOperator.GreaterThan;
                    else if (_Current == LESS)
                        comparison |= ComparisonOperator.LessThan;

                    Next();
                }
            }

            internal void Flags(out ConstraintModifier flag)
            {
                flag = ConstraintModifier.None;
                if (EOF || _Current != AT)
                    return;

                if (HasFlag(FLAG_PRE) || HasFlag(FLAG_PRERELEASE))
                    flag |= ConstraintModifier.Prerelease;
            }

            private bool HasFlag(string value)
            {
                if (string.Compare(_Value, _Position, value, 0, value.Length, false) != 0)
                    return false;

                Next(value.Length);
                return true;
            }

            private void SkipLeading()
            {
                if (!EOF && (_Current == VUPPER || _Current == VLOWER))
                    Next();
            }

            internal bool TryDigit(out int digit)
            {
                var pos = _Position;
                var count = 0;
                while (!EOF && IsVersionDigit(_Current))
                {
                    count++;
                    Next();
                }
                digit = count > 0 ? int.Parse(_Value.Substring(pos, count), Thread.CurrentThread.CurrentCulture) : 0;
                return count > 0;
            }

            internal bool TrySegments(out int[] segments)
            {
                segments = new int[] { -1, -1, -1, -1 };
                var segmentIndex = 0;
                SkipLeading();
                while (!EOF)
                {
                    if (!IsAllowedChar(_Current))
                        return false;

                    if (TryDigit(out var digit))
                        segments[segmentIndex++] = digit;

                    if (IsSeparator(_Current))
                        Next();

                    if (IsWildcard(_Current))
                    {
                        segments[segmentIndex++] = -1;
                        Next();
                    }

                    if (IsIdentifier(_Current))
                        return true;

                    if (IsJoin(_Current))
                        return true;
                }
                return segmentIndex > 0;
            }

            internal bool Prerelease(out PR identifier)
            {
                identifier = PR.Empty;
                if (EOF || _Current != DASH)
                    return true;

                Next();
                var start = _Position;
                if (EOF)
                    return false;

                var numeric = true;
                while (!EOF && IsPrereleaseChar(_Current, ref numeric))
                    Next();

                if (_Position - start == 0)
                    return false;

                // No leading 0 if numeric
                var id = _Value.Substring(start, _Position - start);
                if (numeric && id.Length > 1 && id[0] == ZERO)
                    return false;

                identifier = new PR(id);
                return true;
            }

            internal void Build(out string label)
            {
                label = string.Empty;
                if (EOF || _Current != PLUS)
                    return;

                Next();
                var start = _Position;
                if (_Current == ZERO)
                    return;

                while (!EOF && IsBuildChar(_Current))
                    Next();

                label = _Value.Substring(start, _Position - start);
            }

            /// <summary>
            /// 1.2.3 || 3.4.5
            /// >=1.2.3 &lt;3.4.5
            /// </summary>
            internal JoinOperator GetJoin()
            {
                var result = JoinOperator.None;
                while ((_Current == SPACE || _Current == PIPE) && !EOF)
                {
                    if (result == JoinOperator.None && _Current == SPACE)
                        result = JoinOperator.And;

                    if (_Current == PIPE)
                        result = JoinOperator.Or;

                    Next();
                }
                return _Current != SPACE || _Current != PIPE ? result : JoinOperator.Or;
            }

            [DebuggerStepThrough()]
            private static bool IsConstraint(char c)
            {
                return c == MINOR || c == PATCH || c == EQUAL || c == GREATER || c == LESS;
            }

            [DebuggerStepThrough()]
            private static bool IsVersionDigit(char c)
            {
                return char.IsDigit(c);
            }

            [DebuggerStepThrough()]
            private static bool IsWildcard(char c)
            {
                return c == '*' || c == 'X' || c == 'x';
            }

            [DebuggerStepThrough()]
            private static bool IsSeparator(char c)
            {
                return c == SEPARATOR;
            }

            [DebuggerStepThrough()]
            private static bool IsAllowedChar(char c)
            {
                return IsVersionDigit(c) || IsSeparator(c) || IsWildcard(c) || IsIdentifier(c);
            }

            [DebuggerStepThrough()]
            private static bool IsPrereleaseChar(char c, ref bool numeric)
            {
                if (numeric && char.IsDigit(c))
                    return true;

                numeric = false;
                return char.IsDigit(c) || IsLetter(c) || c == DASH || c == SEPARATOR;
            }

            [DebuggerStepThrough()]
            private static bool IsBuildChar(char c)
            {
                return char.IsDigit(c) || IsLetter(c);
            }

            [DebuggerStepThrough()]
            private static bool IsIdentifier(char c)
            {
                return c == DASH || c == PLUS;
            }

            [DebuggerStepThrough()]
            private static bool IsJoin(char c)
            {
                return c == SPACE || c == PIPE;
            }

            /// <summary>
            /// Is the character within the reduced set of allowed characters. a-z or A-Z.
            /// </summary>
            [DebuggerStepThrough()]
            private static bool IsLetter(char c)
            {
                var nc = (int)c;
                return (nc >= 0x41 && nc <= 0x5a) || (nc >= 0x61 && nc <= 0x7a);
            }
        }

        public static bool TryParseConstraint(string value, out IConstraint constraint, bool includePrerelease = false)
        {
            var c = new VersionConstraint();
            constraint = c;
            if (string.IsNullOrEmpty(value))
                return true;

            var stream = new VersionStream(value);
            stream.Flags(out var flags);
            if (flags.HasFlag(ConstraintModifier.Prerelease))
                includePrerelease = true;

            while (!stream.EOF)
            {
                stream.Operator(out var comparison);
                if (!stream.TrySegments(out var segments))
                    return false;

                stream.Prerelease(out var prerelease);
                stream.Build(out _);

                c.Join(segments[0], segments[1], segments[2], prerelease, comparison, stream.GetJoin(), includePrerelease);
            }
            return true;
        }

        public static bool TryParseVersion(string value, out Version version)
        {
            version = null;
            if (string.IsNullOrEmpty(value))
                return false;

            var stream = new VersionStream(value);
            if (!stream.TrySegments(out var segments))
                return false;

            if (!stream.Prerelease(out var prerelease))
                return false;

            stream.Build(out var build);
            version = new Version(segments[0], segments[1], segments[2], prerelease, build);
            return true;
        }
    }
}
