// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;
using System.Collections;
using System.Dynamic;
using System.Management.Automation.Language;

namespace PSRule.Runtime
{
    public sealed class LocalizedData : DynamicObject
    {
        private const string DATA_FILENAME = "PSRule-rules.psd1";

        private static readonly Hashtable Empty = new Hashtable();

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var hashtable = TryGetLocalized();
            if (hashtable.Count > 0 && binder != null && !string.IsNullOrEmpty(binder.Name) && hashtable.ContainsKey(binder.Name))
            {
                result = hashtable[binder.Name];
                return true;
            }
            result = null;
            return false;
        }

        private static Hashtable TryGetLocalized()
        {
            var path = GetFilePath();
            if (path == null)
                return Empty;

            if (RunspaceContext.CurrentThread.Pipeline.LocalizedDataCache.ContainsKey(path))
                return RunspaceContext.CurrentThread.Pipeline.LocalizedDataCache[path];

            var ast = System.Management.Automation.Language.Parser.ParseFile(path, out Token[] tokens, out ParseError[] errors);
            var data = ast.Find(a => a is HashtableAst, false);
            if (data != null)
            {
                var result = (Hashtable)data.SafeGetValue();
                RunspaceContext.CurrentThread.Pipeline.LocalizedDataCache[path] = result;
                return result;
            }
            return Empty;
        }

        private static string GetFilePath()
        {
            return RunspaceContext.CurrentThread.GetLocalizedPath(DATA_FILENAME);
        }
    }
}
