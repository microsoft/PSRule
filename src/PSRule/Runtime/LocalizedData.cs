// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;
using System.Collections;
using System.Dynamic;
using System.IO;
using System.Management.Automation.Language;

namespace PSRule.Runtime
{
    public sealed class LocalizedData : DynamicObject
    {
        private static readonly Hashtable Empty = new Hashtable();

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var hashtable = TryGetLocalized();

            if (hashtable.ContainsKey(binder.Name))
            {
                result = hashtable[binder.Name];
                return true;
            }

            result = null;
            return false;
        }

        private Hashtable TryGetLocalized()
        {
            var path = GetFilePath();

            if (path == null)
            {
                return Empty;
            }

            if (PipelineContext.CurrentThread.DataCache.ContainsKey(path))
            {
                return PipelineContext.CurrentThread.DataCache[path];
            }

            var ast = System.Management.Automation.Language.Parser.ParseFile(path, out Token[] tokens, out ParseError[] errors);
            var data = ast.Find(a => a is HashtableAst, false);

            if (data != null)
            {
                var result = (Hashtable)data.SafeGetValue();
                PipelineContext.CurrentThread.DataCache[path] = result;
                return result;
            }

            return Empty;
        }

        private string GetFilePath()
        {
            var helpPath = PipelineContext.CurrentThread.RuleBlock.Source.HelpPath;
            var culture = PipelineContext.CurrentThread.Culture;

            for (var i = 0; i < culture.Length; i++)
            {
                var path = Path.Combine(helpPath, string.Concat(culture[i], "/PSRule-rules.psd1"));

                if (File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }
    }
}
