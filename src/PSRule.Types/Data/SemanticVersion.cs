// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Text;

namespace PSRule.Data;

/// <summary>
/// A helper for comparing semantic version strings.
/// </summary>
public static class SemanticVersion
{
    private const char MINOR = '^';
    private const char PATCH = '~';
    private const char EQUAL = '=';
    private const char V_UPPER = 'V';
    private const char V_LOWER = 'v';
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

    /// <summary>
    /// A semantic version constraint.
    /// </summary>
    public sealed class VersionConstraint : ISemanticVersionConstraint
    {
        private List<ConstraintExpression>? _Constraints;
        private string? _ConstraintString;

        private readonly bool _IncludePrerelease;

        /// <summary>
        /// A version constraint that accepts any version including pre-releases.
        /// </summary>
        public static readonly VersionConstraint Any = new(includePrerelease: true);

        /// <summary>
        /// A version constraint that accepts any stable version.
        /// </summary>
        public static readonly VersionConstraint AnyStable = new(includePrerelease: false);

        internal VersionConstraint(bool includePrerelease)
        {
            _IncludePrerelease = includePrerelease;
        }

        /// <inheritdoc/>
        public bool Accepts(Version? version)
        {
            if (version is null) return false;
            if (_Constraints == null || _Constraints.Count == 0)
                return version.Stable || _IncludePrerelease;

            var match = false;
            var i = 0;
            while (!match && i < _Constraints.Count)
            {
                var result = _Constraints[i].Accepts(version);

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

        /// <inheritdoc/>
        public override string ToString()
        {
            return _ConstraintString ??= GetConstraintString();
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        internal void Join(int major, int minor, int patch, PR prid, ComparisonOperator flag, JoinOperator join, bool includePrerelease)
        {
            _ConstraintString = null;
            _Constraints ??= [];
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

        private string GetConstraintString()
        {
            if (_Constraints == null || _Constraints.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();
            for (var i = 0; i < _Constraints.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(SPACE);

                    if (_Constraints[i].Join == JoinOperator.Or)
                    {
                        sb.Append(PIPE);
                        sb.Append(PIPE);
                    }
                    else if (_Constraints[i].Join == JoinOperator.And)
                    {
                        sb.Append(SPACE);
                    }

                    sb.Append(SPACE);
                }

                sb.Append(_Constraints[i].ToString());
            }
            return sb.ToString();
        }
    }

    [DebuggerDisplay("{_Major}.{_Minor}.{_Patch}")]
    internal sealed class ConstraintExpression : ISemanticVersionConstraint
    {
        private readonly ComparisonOperator _Flag;
        private readonly int _Major;
        private readonly int _Minor;
        private readonly int _Patch;
        private readonly PR _PRID;
        private readonly bool _IncludePrerelease;

        private string? _ExpressionString;

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

        public override string ToString()
        {
            return _ExpressionString ??= GetExpressionString();
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public static bool TryParse(string value, out ISemanticVersionConstraint constraint)
        {
            return TryParseConstraint(value, out constraint);
        }

        public bool Accepts(System.Version version)
        {
            return Accepts(version.Major, version.Minor, version.Build, null);
        }

        /// <inheritdoc/>
        public bool Accepts(Version? version)
        {
            return version is not null && Accepts(version.Major, version.Minor, version.Patch, version.Prerelease);
        }

        public bool Accepts(int major, int minor, int patch, PR? prid)
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
            if (GuardLess(major, minor, patch, prid))
                return false;

            // Fail with not less or equal to
            return !GuardLessOrEqual(major, minor, patch, prid);
        }

        private bool GuardLessOrEqual(int major, int minor, int patch, PR? prid)
        {
            return _Flag == (ComparisonOperator.LessThan | ComparisonOperator.Equals) && !(LT(major, minor, patch, prid) || EQ(major, minor, patch, prid));
        }

        private bool GuardLess(int major, int minor, int patch, PR? prid)
        {
            return _Flag == ComparisonOperator.LessThan && !LT(major, minor, patch, prid);
        }

        private bool GuardGreaterOrEqual(int major, int minor, int patch, PR? prid)
        {
            return _Flag == (ComparisonOperator.GreaterThan | ComparisonOperator.Equals) && !(GT(major, minor, patch, prid) || EQ(major, minor, patch, prid));
        }

        private bool GuardGreater(int major, int minor, int patch, PR? prid)
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

        private bool GuardPRID(PR? prid)
        {
            return !_IncludePrerelease && Stable && !IsStable(prid);
        }

        private bool EQ(int major, int minor, int patch, PR? prid)
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
        private bool GT(int major, int minor, int patch, PR? prid)
        {
            return !IsStable(prid) && !_IncludePrerelease
                ? EQCore(major, minor, patch) && PR(prid) < 0
                : GTCore(major, minor, patch) || (EQCore(major, minor, patch) && PR(prid) < 0);
        }

        /// <summary>
        /// Less Than.
        /// </summary>
        private bool LT(int major, int minor, int patch, PR? prid)
        {
            return !IsStable(prid) && !_IncludePrerelease
                ? EQCore(major, minor, patch) && PR(prid) > 0
                : LTCore(major, minor, patch) || (EQCore(major, minor, patch) && PR(prid) > 0);
        }

        /// <summary>
        /// Compare pre-release.
        /// </summary>
        private int PR(PR? prid)
        {
            return _PRID.CompareTo(prid);
        }

        private static bool IsStable(PR? prid)
        {
            return prid == null || prid.Stable;
        }

        private string GetExpressionString()
        {
            var sb = new StringBuilder();
            switch (_Flag)
            {
                case ComparisonOperator.Equals:
                    sb.Append(EQUAL);
                    break;

                case ComparisonOperator.PatchUplift:
                    sb.Append(PATCH);
                    break;

                case ComparisonOperator.MinorUplift:
                    sb.Append(MINOR);
                    break;

                case ComparisonOperator.GreaterThan:
                    sb.Append(GREATER);
                    break;

                case ComparisonOperator.LessThan:
                    sb.Append(LESS);
                    break;

                case ComparisonOperator.GreaterThan | ComparisonOperator.Equals:
                    sb.Append(GREATER);
                    sb.Append(EQUAL);
                    break;

                case ComparisonOperator.LessThan | ComparisonOperator.Equals:
                    sb.Append(LESS);
                    sb.Append(EQUAL);
                    break;
            }

            sb.Append(_Major);
            sb.Append(DOT);
            sb.Append(_Minor);
            sb.Append(DOT);
            sb.Append(_Patch);
            if (_PRID != null && !string.IsNullOrEmpty(_PRID.Value))
            {
                sb.Append(DASH);
                sb.Append(_PRID.Value);
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// A semantic version.
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
    public sealed class Version : IComparable<Version>, IEquatable<Version>
    {
        private string? _VersionString;
        private string? _ShortVersionString;

        /// <summary>
        /// The major part of the version.
        /// </summary>
        public readonly int Major;

        /// <summary>
        /// The minor part of the version.
        /// </summary>
        public readonly int Minor;

        /// <summary>
        /// The patch part of the version.
        /// </summary>
        public readonly int Patch;

        /// <summary>
        /// The pre-release part of the version.
        /// </summary>
        public readonly PR Prerelease;

        /// <summary>
        /// The build part of the version.
        /// </summary>
        public readonly string Build;

        internal Version(int major, int minor, int patch, PR prerelease, string build)
        {
            Major = major >= 0 ? major : 0;
            Minor = minor >= 0 ? minor : 0;
            Patch = patch >= 0 ? patch : 0;
            Prerelease = prerelease;
            Build = build;
        }

        /// <summary>
        /// Determines if the version is stable or a pre-release.
        /// </summary>
        public bool Stable => Prerelease == null || Prerelease.Stable;

        /// <summary>
        /// Try to parse a semantic version from a string.
        /// </summary>
        public static bool TryParse(string value, out Version? version)
        {
            return TryParseVersion(value, out version);
        }

        /// <summary>
        /// Get the version as a string.
        /// </summary>
        public override string ToString()
        {
            return _VersionString ??= GetVersionString(simple: false);
        }

        /// <summary>
        /// Get the version as a string returning only the major.minor.patch part of the version.
        /// </summary>
        public string ToShortString()
        {
            return _ShortVersionString ??= GetVersionString(simple: true);
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
                hash = hash * 23 + Major.GetHashCode();
                hash = hash * 23 + Minor.GetHashCode();
                hash = hash * 23 + Patch.GetHashCode();
                hash = hash * 23 + (Prerelease != null ? Prerelease.GetHashCode() : 0);
                hash = hash * 23 + (Build != null ? Build.GetHashCode() : 0);
                return hash;
            }
        }

        /// <inheritdoc/>
        public static bool operator ==(Version? a, Version? b)
        {
            return a is null && b is null ||
                a is not null && a.Equals(b) ||
                b is not null && b.Equals(a);
        }

        /// <inheritdoc/>
        public static bool operator !=(Version? a, Version? b)
        {
            return !(a is null && b is null ||
                a is not null && a.Equals(b) ||
                b is not null && b.Equals(a));
        }

        /// <summary>
        /// Compare the version against another version.
        /// </summary>
        public bool Equals(Version? other)
        {
            return other is not null &&
                Equals(other.Major, other.Minor, other.Patch, other.Prerelease?.Value);
        }

        /// <summary>
        /// Compare the version against another version based on major.minor.patch.
        /// </summary>
        public bool Equals(int major, int minor, int patch, string? prerelease = null)
        {
            return major == Major &&
                minor == Minor &&
                patch == Patch &&
                new PR(prerelease).Equals(Prerelease);
        }

        /// <summary>
        /// Compare the version against another version.
        /// </summary>
        public int CompareTo(Version? other)
        {
            if (other is null)
                return 1;

            if (Major != other.Major)
                return Major > other.Major ? 32 : -32;

            if (Minor != other.Minor)
                return Minor > other.Minor ? 16 : -16;

            if (Patch != other.Patch)
                return Patch > other.Patch ? 8 : -8;

            return Prerelease != other.Prerelease ? PR.Compare(Prerelease, other.Prerelease) : 0;
        }

        /// <summary>
        /// Returns a version string.
        /// </summary>
        /// <param name="simple">When <c>true</c>, only return the major.minor.patch version.</param>
        private string GetVersionString(bool simple = false)
        {
            var size = 5 + (!simple && Prerelease != null && !Prerelease.Stable ? 2 : 0) + (!simple && Build != null && Build.Length > 0 ? 2 : 0);
            var parts = new object[size];

            parts[0] = Major;
            parts[1] = DOT;
            parts[2] = Minor;
            parts[3] = DOT;
            parts[4] = Patch;

            if (size > 5)
            {
                var next = 5;
                if (Prerelease != null && !Prerelease.Stable)
                {
                    parts[next++] = DASH;
                    parts[next++] = Prerelease.Value;
                }

                if (Build != null && Build.Length > 0)
                {
                    parts[next++] = PLUS;
                    parts[next++] = Build;
                }
            }
            return string.Concat(parts);
        }
    }

    /// <summary>
    /// A semantic version pre-release identifier.
    /// </summary>
    [DebuggerDisplay("{Value}")]
    public sealed class PR : IComparable<PR>, IEquatable<PR>
    {
        internal static readonly PR Empty = new();
        private static readonly char[] SEPARATORS = new char[] { DOT };

        private readonly string[]? _Identifiers;

        private PR()
        {
            Value = string.Empty;
            _Identifiers = null;
        }

        internal PR(string? value)
        {
            Value = value == null || string.IsNullOrEmpty(value) ? string.Empty : value;
            _Identifiers = value == null || string.IsNullOrEmpty(value) ? null : value.Split(SEPARATORS, StringSplitOptions.RemoveEmptyEntries);
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
        /// Compare the pre-release identifier to another pre-release identifier.
        /// </summary>
        public int CompareTo(PR? other)
        {
            if (other is null || other.Stable || other._Identifiers == null)
                return Stable ? 0 : -1;
            else if (Stable || _Identifiers == null)
                return 1;

            var i = -1;
            var left = _Identifiers;
            var right = other._Identifiers;

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
        public bool Equals(PR? other)
        {
            if (other is null)
                return Stable;

            return Stable && other.Stable ||
                Value.Equals(other.Value);
        }


        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is PR other && Equals(other);
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

        /// <summary>
        /// Compare two <see cref="PR"/> instances.
        /// </summary>
        public static int Compare(PR pr1, PR pr2)
        {
            if (pr1 == pr2) return 0;
            if (pr1 == null || pr1.Stable) return 1;
            if (pr2 == null || pr2.Stable) return -1;

            return pr1.CompareTo(pr2);
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
            if (!EOF && (_Current == V_UPPER || _Current == V_LOWER))
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
            segments = [-1, -1, -1, -1];
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
            return c == DOT;
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
    public static bool TryParseConstraint(string value, out ISemanticVersionConstraint constraint, bool includePrerelease = false)
    {
        var c = new VersionConstraint(includePrerelease);
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

    /// <summary>
    /// Try to parse a version from the provided string.
    /// </summary>
    public static bool TryParseVersion(string value, out Version? version)
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
