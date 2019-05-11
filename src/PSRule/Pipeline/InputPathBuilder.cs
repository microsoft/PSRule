using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

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
            if (string.IsNullOrEmpty(path) || _Items.Contains(path))
            {
                return;
            }
            _Items.Add(path);
        }

        public void Add(System.IO.FileInfo[] fileInfo)
        {
            if (fileInfo == null || fileInfo.Length == 0)
            {
                return;
            }

            for (var i = 0; i < fileInfo.Length; i++)
            {
                if (!_Items.Contains(fileInfo[i].FullName))
                {
                    _Items.Add(fileInfo[i].FullName);
                }
            }
        }

        public void Add(PathInfo[] pathInfo)
        {
            if (pathInfo == null || pathInfo.Length == 0)
            {
                return;
            }

            for (var i = 0; i < pathInfo.Length; i++)
            {
                if (!_Items.Contains(pathInfo[i].Path))
                {
                    _Items.Add(pathInfo[i].Path);
                }
            }
        }

        public IEnumerable<string> Build()
        {
            return _Items.ToArray();
        }
    }
}
