namespace PSRule.Runtime
{
    /// <summary>
    /// The type of NameToken.
    /// </summary>
    internal enum NameTokenType
    {
        /// <summary>
        /// The token represents a field/ property of an object.
        /// </summary>
        Field = 0,

        /// <summary>
        /// The token is an index in an object.
        /// </summary>
        Index = 1
    }

    /// <summary>
    /// A token for expressing a path through a tree of fields.
    /// </summary>
    internal sealed class NameToken
    {
        /// <summary>
        /// The name of the field if the token type if Field.
        /// </summary>
        public string Name;

        /// <summary>
        /// The index if the token type if Index.
        /// </summary>
        public int Index;

        /// <summary>
        /// The next token.
        /// </summary>
        public NameToken Next;

        /// <summary>
        /// The type of the token.
        /// </summary>
        public NameTokenType Type = NameTokenType.Field;
    }
}
