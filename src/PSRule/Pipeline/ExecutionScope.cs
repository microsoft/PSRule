namespace PSRule.Pipeline
{
    internal enum ExecutionScope : byte
    {
        None = 0,

        /// <summary>
        /// Execution is occuring at the script. This occurs during discovery.
        /// </summary>
        Script = 1,

        /// <summary>
        /// Execution is occuring in the main rule condition block.
        /// </summary>
        Condition = 2,

        /// <summary>
        /// Execution is occuring in a rule precondition.
        /// </summary>
        Precondition = 3,
    }
}
