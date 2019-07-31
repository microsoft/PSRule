namespace PSRule
{
    internal static class StringExtensions
    {
        public static bool IsUri(this string s)
        {
            return s.StartsWith("http://") || s.StartsWith("https://");
        }
    }
}
