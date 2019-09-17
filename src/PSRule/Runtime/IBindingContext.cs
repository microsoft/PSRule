namespace PSRule.Runtime
{
    internal interface IBindingContext
    {
        bool GetNameToken(string expression, out NameToken nameToken);

        void CacheNameToken(string expression, NameToken nameToken);
    }
}
