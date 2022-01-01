// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;

namespace PSRule.Runtime.ObjectPath
{
    internal enum PathTokenType
    {
        None = 0,

        /// <summary>
        /// Token: $
        /// </summary>
        RootRef,

        /// <summary>
        /// Token: @
        /// </summary>
        CurrentRef,

        /// <summary>
        /// Token: .Name
        /// </summary>
        DotSelector,

        /// <summary>
        /// Token: [index]
        /// </summary>
        IndexSelector,

        /// <summary>
        /// Token: [*]
        /// </summary>
        IndexWildSelector,

        StartFilter,
        ComparisonOperator,
        Boolean,
        EndFilter,
        String,
        Integer,
        LogicalOperator,

        StartGroup,
        EndGroup,

        /// <summary>
        /// Token: !
        /// </summary>
        NotOperator,

        /// <summary>
        /// Token: ..
        /// </summary>
        DescendantSelector,

        /// <summary>
        /// Token: .*
        /// </summary>
        DotWildSelector,

        ArraySliceSelector,
        UnionIndexSelector,
        UnionQuotedMemberSelector,
    }

    internal enum PathTokenOption
    {
        None = 0,

        CaseSensitive
    }

    internal enum FilterOperator
    {
        None = 0,

        // Comparison
        Equal,
        NotEqual,
        LessOrEqual,
        Less,
        GreaterOrEqual,
        Greater,
        RegEx,

        // Logical
        Or,
        And,
    }

    internal interface IPathToken
    {
        PathTokenType Type { get; }

        PathTokenOption Option { get; }

        object Arg { get; }

        T As<T>();
    }

    [DebuggerDisplay("Type = {Type}, Arg = {Arg}")]
    internal sealed class PathToken : IPathToken
    {
        public readonly static PathToken RootRef = new PathToken(PathTokenType.RootRef);
        public readonly static PathToken CurrentRef = new PathToken(PathTokenType.CurrentRef);

        public PathTokenType Type { get; }

        public PathTokenOption Option { get; }

        public PathToken(PathTokenType type)
        {
            Type = type;
        }

        public PathToken(PathTokenType type, object arg, PathTokenOption option = PathTokenOption.None)
        {
            Type = type;
            Arg = arg;
            Option = option;
        }

        public object Arg { get; }

        public T As<T>()
        {
            return Arg is T result ? result : default;
        }
    }

    internal interface ITokenWriter
    {
        IPathToken Last { get; }

        void Add(IPathToken token);
    }

    internal interface ITokenReader
    {
        IPathToken Current { get; }

        bool Next(out IPathToken token);

        bool Consume(PathTokenType type);

        bool Peak(out IPathToken token);
    }

    internal sealed class TokenReader : ITokenReader
    {
        private readonly IPathToken[] _Tokens;
        private readonly int _Last;

        private int _Index;

        public TokenReader(IPathToken[] tokens)
        {
            _Tokens = tokens;
            _Last = tokens.Length - 1;
            _Index = -1;
        }

        public IPathToken Current { get; private set; }

        public bool Consume(PathTokenType type)
        {
            return (Peak(out IPathToken token) && token.Type == type) ? Next() : false;
        }

        public bool Next(out IPathToken token)
        {
            token = null;
            if (!Next())
                return false;

            token = Current;
            return true;
        }

        private bool Next()
        {
            Current = _Index < _Last ? _Tokens[++_Index] : null;
            return Current != null;
        }

        public bool Peak(out IPathToken token)
        {
            token = _Index < _Last ? _Tokens[_Index + 1] : null;
            return token != null;
        }
    }
}
