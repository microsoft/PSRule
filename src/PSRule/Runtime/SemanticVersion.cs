// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
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

        [Flags]
        internal enum CompareFlag
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

        [DebuggerDisplay("{_Major}.{_Minor}.{_Patch}")]
        internal sealed class Constraint
        {
            private readonly CompareFlag _Flag;
            private readonly int _Major;
            private readonly int _Minor;
            private readonly int _Patch;
            private readonly PRID _PRID;

            internal Constraint(int major, int minor, int patch, PRID prid, CompareFlag flag)
            {
                _Flag = flag == CompareFlag.None ? CompareFlag.Equals : flag;
                _Major = major;
                _Minor = minor;
                _Patch = patch;
                _PRID = prid;
            }

            public bool Stable => IsStable(_PRID);

            public static bool TryParse(string value, out Constraint constraint)
            {
                return TryParseConstraint(value, out constraint);
            }

            public bool Equals(System.Version version)
            {
                return Equals(version.Major, version.Minor, version.Build, null);
            }

            public bool Equals(Version version)
            {
                return Equals(version.Major, version.Minor, version.Patch, version.PreRelease);
            }

            public bool Equals(int major, int minor, int patch, PRID prid)
            {
                if (_Flag == CompareFlag.Equals)
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
                if (GuardLessOrEqual(major, minor, patch, prid))
                    return false;

                return true;
            }

            private bool GuardLessOrEqual(int major, int minor, int patch, PRID prid)
            {
                return _Flag == (CompareFlag.LessThan | CompareFlag.Equals) && !(LT(major, minor, patch, prid) || EQ(major, minor, patch, prid));
            }

            private bool GaurdLess(int major, int minor, int patch, PRID prid)
            {
                return _Flag == CompareFlag.LessThan && !LT(major, minor, patch, prid);
            }

            private bool GuardGreaterOrEqual(int major, int minor, int patch, PRID prid)
            {
                return _Flag == (CompareFlag.GreaterThan | CompareFlag.Equals) && !(GT(major, minor, patch, prid) || EQ(major, minor, patch, prid));
            }

            private bool GuardGreater(int major, int minor, int patch, PRID prid)
            {
                return _Flag == CompareFlag.GreaterThan && !GT(major, minor, patch, prid);
            }

            private bool GuardMinor(int minor, int patch)
            {
                return _Flag == CompareFlag.MinorUplift && (minor < _Minor || (minor == _Minor && patch < _Patch));
            }

            private bool GuardPatch(int minor, int patch)
            {
                return _Flag == CompareFlag.PatchUplift && (minor != _Minor || patch < _Patch);
            }

            private bool GuardMajor(int major)
            {
                return (_Flag == CompareFlag.MinorUplift || _Flag == CompareFlag.PatchUplift) && major != _Major;
            }

            private bool GuardPRID(PRID prid)
            {
                return Stable && !IsStable(prid);
            }

            private bool EQ(int major, int minor, int patch, PRID prid)
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

            /// <summary>
            /// Greater Than.
            /// </summary>
            /// <remarks>
            /// When constraint is not pre-release the compared version must not be any pre-release version and be greater.
            /// When constraint is a pre-release the compared version must be:
            /// - A non-pre-release version of the >= major.minor.patch.
            /// - A pre-release version == major.minor.patch with a greater pre-release identifer.
            /// </remarks>
            private bool GT(int major, int minor, int patch, PRID prid)
            {
                var versionStable = prid == null || prid.Stable;
                if (Stable && versionStable && GTCore(major, minor, patch))
                    return true;

                return (versionStable && GTCore(major, minor, patch)) ||
                    (!Stable && versionStable && EQCore(major, minor, patch)) ||
                    (EQCore(major, minor, patch) && PR(prid) > 0);
            }

            private bool LT(int major, int minor, int patch, PRID prid)
            {
                if (prid != null && !prid.Stable)
                    return EQCore(major, minor, patch) && PR(prid) < 0;

                return major < _Major ||
                    (major == _Major && minor < _Minor) ||
                    (major == _Major && minor == _Minor && patch < _Patch) ||
                    (major == _Major && minor == _Minor && patch == _Patch && PR(prid) < 0);
            }

            /// <summary>
            /// Compare pre-release.
            /// </summary>
            private int PR(PRID prid)
            {
                return _PRID.Compare(prid);
            }

            private static bool IsStable(PRID prid)
            {
                return prid == null || prid.Stable;
            }
        }

        internal sealed class Version : IComparable<Version>, IEquatable<Version>
        {
            public readonly int Major;
            public readonly int Minor;
            public readonly int Patch;
            public readonly PRID PreRelease;
            public readonly string Build;

            internal Version(int major, int minor, int patch, PRID prerelease, string build)
            {
                Major = major;
                Minor = minor;
                Patch = patch;
                PreRelease = prerelease;
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

        internal sealed class PRID
        {
            public static readonly PRID Empty = new PRID();

            private readonly string _Prerelease;
            private readonly bool _Numeric;

            private PRID()
            {
                _Prerelease = null;
            }

            internal PRID(string identifier, bool numeric)
            {
                _Prerelease = string.IsNullOrEmpty(identifier) ? null : identifier;
                _Numeric = numeric;
            }

            public bool Stable => _Prerelease == null;

            public int Compare(PRID identifier)
            {
                if (identifier == null || identifier.Stable)
                    return Stable ? 0 : 1;
                else if (Stable)
                    return -1;

                if (identifier._Numeric)
                    return _Numeric ? string.Compare(identifier._Prerelease, _Prerelease, StringComparison.Ordinal) : 1;

                if (identifier._Prerelease.Length == _Prerelease.Length)
                    return string.Compare(identifier._Prerelease, _Prerelease, StringComparison.Ordinal);

                var compareLength = identifier._Prerelease.Length > _Prerelease.Length ? _Prerelease.Length : identifier._Prerelease.Length;
                var left = identifier._Prerelease.Substring(0, compareLength);
                var right = _Prerelease.Substring(0, compareLength);
                if (left == right)
                    return identifier._Prerelease.Length == compareLength ? -1 : 1;

                return string.Compare(left, right, StringComparison.Ordinal);
            }

            public override string ToString()
            {
                return _Prerelease.ToString();
            }
        }

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
                if (EOF || ++_Position >= _Value.Length)
                    return;

                _Current = _Value[_Position];
            }

            internal void GetConstraint(out CompareFlag flag)
            {
                flag = CompareFlag.None;
                while (!EOF && IsConstraint(_Current))
                {
                    if (_Current == MINOR)
                        flag = CompareFlag.MinorUplift;
                    else if (_Current == PATCH)
                        flag = CompareFlag.PatchUplift;
                    else if (_Current == EQUAL)
                        flag |= CompareFlag.Equals;
                    else if (_Current == GREATER)
                        flag |= CompareFlag.GreaterThan;
                    else if (_Current == LESS)
                        flag |= CompareFlag.LessThan;

                    Next();
                }
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

                    if (TryDigit(out int digit))
                        segments[segmentIndex++] = digit;

                    if (IsSeparator(_Current))
                        Next();

                    if (IsWildcard(_Current))
                    {
                        segments[segmentIndex++] = -1;
                        Next();
                    }

                    if (_Current == DASH || _Current == PLUS)
                        return true;
                }
                return segmentIndex > 0;
            }

            internal bool TryPrerelease(out PRID identifier)
            {
                identifier = PRID.Empty;
                if (EOF || _Current != DASH)
                    return true;

                Next();
                var start = _Position;
                if (EOF)
                    return false;

                bool numeric = true;
                while (!EOF && IsPrereleaseChar(_Current, ref numeric))
                    Next();

                if (_Position - start == 0)
                    return false;

                // No leading 0 if numeric
                var id = _Value.Substring(start, _Position - start);
                if (numeric && id.Length > 1 && id[0] == ZERO)
                    return false;

                identifier = new PRID(id, numeric);
                return true;
            }

            internal bool TryBuild(out string label)
            {
                label = string.Empty;
                if (EOF || _Current != PLUS)
                    return true;

                Next();
                var start = _Position;
                if (_Current == ZERO)
                    return false;

                while (!EOF && IsBuildChar(_Current))
                    Next();

                label = _Value.Substring(start, _Position - start);
                return label.Length > 0;
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
                return IsVersionDigit(c) || IsSeparator(c) || IsWildcard(c) || c == DASH || c == PLUS;
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

        public static bool TryParseConstraint(string value, out Constraint constraint)
        {
            constraint = null;
            if (string.IsNullOrEmpty(value))
                return true;

            var stream = new VersionStream(value);
            while (!stream.EOF)
            {
                stream.GetConstraint(out CompareFlag flag);
                if (!stream.TrySegments(out int[] segments))
                    return false;

                if (!stream.TryPrerelease(out PRID prerelease) || !stream.TryBuild(out _) || !stream.EOF)
                    return false;

                constraint = new Constraint(segments[0], segments[1], segments[2], prerelease, flag);
            }
            return true;
        }

        public static bool TryParseVersion(string value, out Version version)
        {
            version = null;
            if (string.IsNullOrEmpty(value))
                return false;

            var stream = new VersionStream(value);
            if (!stream.TrySegments(out int[] segments))
                return false;

            if (!stream.TryPrerelease(out PRID prerelease))
                return false;

            stream.TryBuild(out string build);
            version = new Version(segments[0], segments[1], segments[2], prerelease, build);
            return true;
        }
    }
}
