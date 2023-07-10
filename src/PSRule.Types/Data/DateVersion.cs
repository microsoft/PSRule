// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;

namespace PSRule.Data
{
    /// <summary>
    /// An date version constraint.
    /// </summary>
    public interface IDateVersionConstraint
    {
        /// <summary>
        /// Determines if the date version meets the requirments of the constraint.
        /// </summary>
        bool Equals(DateVersion.Version version);
    }

    /// <summary>
    /// A helper for comparing date version strings.
    /// An date version is represented as YYYY-MM-DD-prerelease.
    /// </summary>
    public static class DateVersion
    {
        private const char EQUAL = '=';
        private const char GREATER = '>';
        private const char LESS = '<';
        private const char DOT = '.';
        private const char DASH = '-';
        private const char PLUS = '+';
        private const char ZERO = '0';
        private const char PIPE = '|';
        private const char SPACE = ' ';
        private const char AT = '@';

        private const string FLAG_PRERELEASE = "@prerelease ";
        private const string FLAG_PRE = "@pre ";

        /// <summary>
        /// A comparison operation for a version constraint.
        /// </summary>
        [Flags]
        internal enum ComparisonOperator
        {
            None = 0,

            /// <summary>
            /// YYYY-MM-DD bits must match.
            /// </summary>
            Equals = 1,

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

        /// <summary>
        /// An date version constraint.
        /// </summary>
        public sealed class VersionConstraint : IDateVersionConstraint
        {
            private List<ConstraintExpression> _Constraints;

            /// <inheritdoc/>
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

            internal void Join(int year, int month, int day, PR prid, ComparisonOperator flag, JoinOperator join, bool includePrerelease)
            {
                _Constraints ??= new List<ConstraintExpression>();
                _Constraints.Add(new ConstraintExpression(
                    year,
                    month,
                    day,
                    prid,
                    flag,
                    join == JoinOperator.None ? JoinOperator.Or : join,
                    includePrerelease
                ));
            }
        }

        [DebuggerDisplay("{_Year}.{_Month}.{_Day}")]
        internal sealed class ConstraintExpression : IDateVersionConstraint
        {
            private readonly ComparisonOperator _Flag;
            private readonly int _Year;
            private readonly int _Month;
            private readonly int _Day;
            private readonly PR _PRID;
            private readonly bool _IncludePrerelease;

            internal ConstraintExpression(int year, int month, int day, PR prid, ComparisonOperator flag, JoinOperator join, bool includePrerelease)
            {
                _Flag = flag == ComparisonOperator.None ? ComparisonOperator.Equals : flag;
                _Year = year;
                _Month = month;
                _Day = day;
                _PRID = prid;
                Join = join;
                _IncludePrerelease = includePrerelease;
            }

            public bool Stable => IsStable(_PRID);

            public JoinOperator Join { get; }

            public static bool TryParse(string value, out IDateVersionConstraint constraint)
            {
                return TryParseConstraint(value, out constraint);
            }

            public bool Equals(Version version)
            {
                return Equals(version.Year, version.Month, version.Day, version.Prerelease);
            }

            public bool Equals(int year, int month, int day, PR prid)
            {
                if (_Flag == ComparisonOperator.Equals)
                    return EQ(year, month, day, prid);

                // Fail when pre-release should not be included
                if (GuardPRID(prid))
                    return false;

                // Fail when not greater
                if (GuardGreater(year, month, day, prid))
                    return false;

                // Fail when not greater or equal to
                if (GuardGreaterOrEqual(year, month, day, prid))
                    return false;

                // Fail when not less
                if (GaurdLess(year, month, day, prid))
                    return false;

                // Fail with not less or equal to
                return !GuardLessOrEqual(year, month, day, prid);
            }

            private bool GuardLessOrEqual(int year, int month, int day, PR prid)
            {
                return _Flag == (ComparisonOperator.LessThan | ComparisonOperator.Equals) && !(LT(year, month, day, prid) || EQ(year, month, day, prid));
            }

            private bool GaurdLess(int year, int month, int day, PR prid)
            {
                return _Flag == ComparisonOperator.LessThan && !LT(year, month, day, prid);
            }

            private bool GuardGreaterOrEqual(int year, int month, int day, PR prid)
            {
                return _Flag == (ComparisonOperator.GreaterThan | ComparisonOperator.Equals) && !(GT(year, month, day, prid) || EQ(year, month, day, prid));
            }

            private bool GuardGreater(int year, int month, int day, PR prid)
            {
                return _Flag == ComparisonOperator.GreaterThan && !GT(year, month, day, prid);
            }

            private bool GuardPRID(PR prid)
            {
                return !_IncludePrerelease && Stable && !IsStable(prid);
            }

            private bool EQ(int year, int month, int day, PR prid)
            {
                return EQCore(year, month, day) && PR(prid) == 0;
            }

            private bool EQCore(int year, int month, int day)
            {
                return (_Year == -1 || _Year == year) &&
                    (_Month == -1 || _Month == month) &&
                    (_Day == -1 || _Day == day);
            }

            private bool GTCore(int year, int month, int day)
            {
                return (year > _Year) ||
                    (year == _Year && month > _Month) ||
                    (year == _Year && month == _Month && day > _Day);
            }

            private bool LTCore(int year, int month, int day)
            {
                return (year < _Year) ||
                    (year == _Year && month < _Month) ||
                    (year == _Year && month == _Month && day < _Day);
            }

            /// <summary>
            /// Greater Than.
            /// </summary>
            private bool GT(int year, int month, int day, PR prid)
            {
                return !IsStable(prid) && !_IncludePrerelease
                    ? EQCore(year, month, day) && PR(prid) < 0
                    : GTCore(year, month, day) || (EQCore(year, month, day) && PR(prid) < 0);
            }

            /// <summary>
            /// Less Than.
            /// </summary>
            private bool LT(int year, int month, int day, PR prid)
            {
                return !IsStable(prid) && !_IncludePrerelease
                    ? EQCore(year, month, day) && PR(prid) > 0
                    : LTCore(year, month, day) || (EQCore(year, month, day) && PR(prid) > 0);
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

        /// <summary>
        /// An date version.
        /// </summary>
        public sealed class Version : IComparable<Version>, IEquatable<Version>
        {
            /// <summary>
            /// The year part of the version.
            /// </summary>
            public readonly int Year;

            /// <summary>
            /// The month part of the version.
            /// </summary>
            public readonly int Month;

            /// <summary>
            /// The day part of the version.
            /// </summary>
            public readonly int Day;

            /// <summary>
            /// The pre-release part of the version.
            /// </summary>
            public readonly PR Prerelease;

            internal Version(int year, int month, int day, PR prerelease)
            {
                Year = year;
                Month = month;
                Day = day;
                Prerelease = prerelease;
            }

            /// <inheritdoc/>
            public override string ToString()
            {
                return string.Concat(Year, DASH, Month, DASH, Day);
            }

            /// <inheritdoc/>
            public override bool Equals(object obj)
            {
                return obj is Version version && Equals(version);
            }

            /// <inheritdoc/>
            public override int GetHashCode()
            {
                unchecked // Overflow is fine
                {
                    var hash = 17;
                    hash = hash * 23 + Year.GetHashCode();
                    hash = hash * 23 + Month.GetHashCode();
                    hash = hash * 23 + Day.GetHashCode();
                    hash = hash * 23 + (Prerelease != null ? Prerelease.GetHashCode() : 0);
                    return hash;
                }
            }

            /// <summary>
            /// Compare the version against another version.
            /// </summary>
            public bool Equals(Version other)
            {
                return other != null &&
                    Equals(other.Year, other.Month, other.Day);
            }

            /// <summary>
            /// Compare the version against another version based on YYYY-MM-DD.
            /// </summary>
            public bool Equals(int year, int month, int day)
            {
                return year == Year &&
                    month == Month &&
                    day == Day;
            }

            /// <summary>
            /// Compare the version against another version.
            /// </summary>
            public int CompareTo(Version other)
            {
                if (other == null)
                    return 1;

                if (Year != other.Year)
                    return Year > other.Year ? 32 : -32;

                if (Month != other.Month)
                    return Month > other.Month ? 16 : -16;

                if (Day != other.Day)
                    return Day > other.Day ? 8 : -8;

                if ((Prerelease == null || Prerelease.Stable) && (other.Prerelease == null || other.Prerelease.Stable))
                    return 0;

                if (Prerelease != null && !Prerelease.Stable && other.Prerelease != null && !other.Prerelease.Stable)
                    return Prerelease.CompareTo(other.Prerelease);

                return Prerelease == null || Prerelease.Stable ? 1 : -1;
            }
        }

        /// <summary>
        /// An date version pre-release identifier.
        /// </summary>
        [DebuggerDisplay("{Value}")]
        public sealed class PR
        {
            internal static readonly PR Empty = new();
            private static readonly char[] SEPARATORS = new char[] { DOT };

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

            /// <summary>
            /// The string value of a pre-release identifier.
            /// </summary>
            public string Value { get; }

            /// <summary>
            /// Is the pre-release identifier empty, indicating a stable release.
            /// </summary>
            public bool Stable => _Identifiers == null;

            /// <summary>
            /// Compare the pre-release identifer to another pre-release identifier.
            /// </summary>
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

            /// <inheritdoc/>
            public override bool Equals(object obj)
            {
                return obj is PR prerelease && Value.Equals(prerelease.Value);
            }

            /// <inheritdoc/>
            public override int GetHashCode()
            {
                return Value.GetHashCode();
            }

            /// <inheritdoc/>
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
                    if (_Current == EQUAL)
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
                segments = new int[] { -1, -1, -1 };
                var segmentIndex = 0;
                while (!EOF)
                {
                    if (!IsAllowedChar(_Current))
                        return false;

                    if (TryDigit(out var digit))
                    {
                        segments[segmentIndex++] = digit;
                        if (segments.Length <= segmentIndex)
                            return true;
                    }

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
                return c == EQUAL || c == GREATER || c == LESS;
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
                return c == DASH;
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
                return char.IsDigit(c) || IsLetter(c) || c == DASH || c == DOT;
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

        /// <summary>
        /// Try to parse a version constraint from the provided string.
        /// </summary>
        public static bool TryParseConstraint(string value, out IDateVersionConstraint constraint, bool includePrerelease = false)
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
                c.Join(segments[0], segments[1], segments[2], prerelease, comparison, stream.GetJoin(), includePrerelease);
            }
            return true;
        }

        /// <summary>
        /// Try to parse a version from the provided string.
        /// </summary>
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

            version = new Version(segments[0], segments[1], segments[2], prerelease);
            return true;
        }
    }
}
