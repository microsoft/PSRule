using Newtonsoft.Json;
using PSRule.Runtime;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace PSRule.Pipeline
{
    public delegate IEnumerable<PSObject> VisitTargetObject(PSObject sourceObject);
    public delegate IEnumerable<PSObject> VisitTargetObjectAction(PSObject sourceObject, VisitTargetObject next);

    public static class PipelineReceiverActions
    {
        public static IEnumerable<PSObject> PassThru(PSObject targetObject)
        {
            yield return targetObject;
        }

        public static IEnumerable<PSObject> ConvertFromJson(PSObject sourceObject, VisitTargetObject next)
        {
            // Only attempt to deserialize if the input is a string or a file
            if (!(sourceObject.BaseObject is string || sourceObject.BaseObject is FileInfo))
            {
                return new PSObject[] { sourceObject };
            }

            var json = string.Empty;

            if (sourceObject.BaseObject is string)
            {
                json = sourceObject.BaseObject.ToString();
            }
            else
            {
                var fileInfo = sourceObject.BaseObject as FileInfo;
                var reader = new StreamReader(fileInfo.FullName);
                json = reader.ReadToEnd();
            }

            var value = JsonConvert.DeserializeObject<PSObject[]>(json, new PSObjectArrayJsonConverter());

            if (value == null)
            {
                return null;
            }

            var result = new List<PSObject>();

            foreach (var item in value)
            {
                var items = next(item);

                if (items == null)
                {
                    continue;
                }

                result.AddRange(items);
            }

            if (result.Count == 0)
            {
                return null;
            }

            return result.ToArray();
        }

        public static IEnumerable<PSObject> ConvertFromYaml(PSObject sourceObject, VisitTargetObject next)
        {
            // Only attempt to deserialize if the input is a string or a file
            if (!(sourceObject.BaseObject is string || sourceObject.BaseObject is FileInfo))
            {
                return new PSObject[] { sourceObject };
            }

            var d = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithTypeConverter(new PSObjectYamlTypeConverter())
                .WithNodeTypeResolver(new PSObjectYamlTypeResolver())
                .Build();

            TextReader reader = null;

            if (sourceObject.BaseObject is string)
            {
                reader = new StringReader(sourceObject.BaseObject.ToString());
            }
            else
            {
                var fileInfo = sourceObject.BaseObject as FileInfo;
                reader = new StreamReader(fileInfo.FullName);
            }

            var parser = new Parser(reader);

            parser.Expect<StreamStart>();

            var result = new List<PSObject>();

            while (parser.Accept<DocumentStart>())
            {
                var item = d.Deserialize<PSObject>(parser: parser);

                if (item == null)
                {
                    continue;
                }

                var items = next(item);

                if (items == null)
                {
                    continue;
                }

                result.AddRange(items);
            }

            if (result.Count == 0)
            {
                return null;
            }

            return result.ToArray();
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
    }
}
