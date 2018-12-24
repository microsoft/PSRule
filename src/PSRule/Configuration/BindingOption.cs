namespace PSRule.Configuration
{
    /// <summary>
    /// Options tht affect property binding of TargetName.
    /// </summary>
    public sealed class BindingOption
    {
        public BindingOption()
        {

        }

        public BindingOption(BindingOption option)
        {
            TargetName = option.TargetName;
        }

        /// <summary>
        /// One or more property names to use to bind TargetName.
        /// </summary>
        public string[] TargetName { get; set; }
    }
}
