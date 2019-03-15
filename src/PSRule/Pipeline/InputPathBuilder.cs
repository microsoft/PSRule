using System.Collections.Generic;
using System.Linq;

namespace PSRule.Pipeline
{
    public sealed class InputPathBuilder
    {
        private readonly HashSet<string> _Items;

        public InputPathBuilder()
        {
            _Items = new HashSet<string>();
        }

        public void Add(string path)
        {
            if (!_Items.Contains(path))
            {
                _Items.Add(path);
            }
        }

        public IEnumerable<string> Build()
        {
            return _Items.ToArray();
        }
    }
}
