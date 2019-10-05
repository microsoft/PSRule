using Newtonsoft.Json;
using PSRule.Parser;
using PSRule.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Net;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace PSRule.Pipeline
{
    public delegate IEnumerable<PSObject> VisitTargetObject(PSObject sourceObject);
    public delegate IEnumerable<PSObject> VisitTargetObjectAction(PSObject sourceObject, VisitTargetObject next);

    public static class PipelineReceiverActions
    {
        private const string PropertyName_PSPath = "PSPath";
        private const string PropertyName_PSParentPath = "PSParentPath";
        private const string PropertyName_PSChildName = "PSChildName";

        private sealed class PSSourceInfo
        {
            public readonly string PSPath;
            public readonly string PSChildName;
            public readonly string PSParentPath;

            public PSSourceInfo(FileInfo info)
            {
                PSPath = info.FullName;
                PSChildName = info.Name;
                PSParentPath = info.DirectoryName;
            }

            public PSSourceInfo(Uri uri)
            {
                PSPath = uri.AbsoluteUri;
            }
        }

        public static IEnumerable<PSObject> PassThru(PSObject targetObject)
        {
            yield return targetObject;
        }

        public static IEnumerable<PSObject> DetectInputFormat(PSObject sourceObject, VisitTargetObject next)
        {
            string pathExtension = null;

            if (sourceObject.BaseObject is FileInfo)
            {
                var fileInfo = sourceObject.BaseObject as FileInfo;
                pathExtension = fileInfo.Extension;
            }
            else if (sourceObject.BaseObject is Uri)
            {
                var uri = sourceObject.BaseObject as Uri;
                pathExtension = Path.GetExtension(uri.OriginalString);
            }

            // Handle JSON
            if (pathExtension == ".json")
            {
                return ConvertFromJson(sourceObject: sourceObject, next: next);
            }
            // Handle YAML
            else if (pathExtension == ".yaml" || pathExtension == ".yml")
            {
                return ConvertFromYaml(sourceObject: sourceObject, next: next);
            }
            // Handle Markdown
            else if (pathExtension == ".md")
            {
                return ConvertFromMarkdown(sourceObject: sourceObject, next: next);
            }
            return new PSObject[] { sourceObject };
        }

        public static IEnumerable<PSObject> ConvertFromJson(PSObject sourceObject, VisitTargetObject next)
        {
            // Only attempt to deserialize if the input is a string, file or URI
            if (!IsAcceptedType(sourceObject: sourceObject))
                return new PSObject[] { sourceObject };

            var json = ReadAsString(sourceObject, out PSSourceInfo source);
            var value = JsonConvert.DeserializeObject<PSObject[]>(json, new PSObjectArrayJsonConverter());
            NoteSource(value, source);
            return VisitItems(value, next);
        }

        public static IEnumerable<PSObject> ConvertFromYaml(PSObject sourceObject, VisitTargetObject next)
        {
            // Only attempt to deserialize if the input is a string, file or URI
            if (!IsAcceptedType(sourceObject: sourceObject))
                return new PSObject[] { sourceObject };

            var d = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithTypeConverter(new PSObjectYamlTypeConverter())
                .WithNodeTypeResolver(new PSObjectYamlTypeResolver())
                .Build();

            var reader = ReadAsReader(sourceObject, out PSSourceInfo source);
            var parser = new YamlDotNet.Core.Parser(reader);
            parser.Expect<StreamStart>();

            var result = new List<PSObject>();
            while (parser.Accept<DocumentStart>())
            {
                var item = d.Deserialize<PSObject>(parser: parser);

                if (item == null)
                    continue;

                NoteSource(item, source);
                var items = next(item);

                if (items == null)
                    continue;

                result.AddRange(items);
            }

            if (result.Count == 0)
                return null;

            return result.ToArray();
        }

        public static IEnumerable<PSObject> ConvertFromMarkdown(PSObject sourceObject, VisitTargetObject next)
        {
            // Only attempt to deserialize if the input is a string or a file
            if (!IsAcceptedType(sourceObject: sourceObject))
                return new PSObject[] { sourceObject };

            var markdown = ReadAsString(sourceObject, out PSSourceInfo source);
            var value = MarkdownConvert.DeserializeObject(markdown);
            NoteSource(value, source);
            return VisitItems(value, next);
        }

        public static IEnumerable<PSObject> ReadObjectPath(PSObject sourceObject, VisitTargetObject source, string objectPath, bool caseSensitive)
        {
            if (!ObjectHelper.GetField(bindingContext: null, targetObject: sourceObject, name: objectPath, caseSensitive: caseSensitive, value: out object nestedObject))
            {
                return null;
            }

            var nestedType = nestedObject.GetType();

            if (typeof(IEnumerable).IsAssignableFrom(nestedType))
            {
                var result = new List<PSObject>();

                foreach (var item in (nestedObject as IEnumerable))
                {
                    result.Add(PSObject.AsPSObject(item));
                }

                return result.ToArray();
            }
            else
            {
                return new PSObject[] { PSObject.AsPSObject(nestedObject) };
            }
        }

        private static bool IsAcceptedType(PSObject sourceObject)
        {
            return sourceObject.BaseObject is string || sourceObject.BaseObject is FileInfo || sourceObject.BaseObject is Uri;
        }

        private static string ReadAsString(PSObject sourceObject, out PSSourceInfo sourceInfo)
        {
            sourceInfo = null;
            if (sourceObject.BaseObject is string)
            {
                return sourceObject.BaseObject.ToString();
            }
            else if (sourceObject.BaseObject is FileInfo fileInfo)
            {
                sourceInfo = new PSSourceInfo(fileInfo);
                using (var reader = new StreamReader(fileInfo.FullName))
                {
                    return reader.ReadToEnd();
                }
            }
            else
            {
                var uri = sourceObject.BaseObject as Uri;
                sourceInfo = new PSSourceInfo(uri); 
                using (var webClient = new WebClient())
                {
                    return webClient.DownloadString(uri);
                }
            }
        }

        private static TextReader ReadAsReader(PSObject sourceObject, out PSSourceInfo sourceInfo)
        {
            sourceInfo = null;
            if (sourceObject.BaseObject is string)
            {
                return new StringReader(sourceObject.BaseObject.ToString());
            }
            else if (sourceObject.BaseObject is FileInfo fileInfo)
            {
                sourceInfo = new PSSourceInfo(fileInfo);
                return new StreamReader(fileInfo.FullName);
            }
            else
            {
                var uri = sourceObject.BaseObject as Uri;
                sourceInfo = new PSSourceInfo(uri);
                using (var webClient = new WebClient())
                {
                    return new StringReader(webClient.DownloadString(uri));
                }
            }
        }

        private static IEnumerable<PSObject> VisitItems(PSObject[] value, VisitTargetObject next)
        {
            if (value == null)
                return null;

            var result = new List<PSObject>();

            foreach (var item in value)
            {
                var items = next(item);
                if (items == null)
                    continue;

                result.AddRange(items);
            }

            if (result.Count == 0)
                return null;

            return result.ToArray();
        }

        private static void NoteSource(PSObject[] value, PSSourceInfo source)
        {
            if (value == null || value.Length == 0 || source == null)
                return;

            for (var i = 0; i < value.Length; i++)
                NoteSource(value[i], source);
        }

        private static void NoteSource(PSObject value, PSSourceInfo source)
        {
            if (value == null || source == null)
                return;

            if (!value.HasProperty(PropertyName_PSPath))
                value.Properties.Add(new PSNoteProperty(PropertyName_PSPath, source.PSPath));

            if (!value.HasProperty(PropertyName_PSParentPath))
                value.Properties.Add(new PSNoteProperty(PropertyName_PSParentPath, source.PSParentPath));

            if (!value.HasProperty(PropertyName_PSChildName))
                value.Properties.Add(new PSNoteProperty(PropertyName_PSChildName, source.PSChildName));
        }
    }
}
