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
            if (!(sourceObject.BaseObject is string))
            {
                return new PSObject[] { sourceObject };
            }

            var value = JsonConvert.DeserializeObject<PSObject[]>(sourceObject.BaseObject.ToString(), new PSObjectArrayJsonConverter());

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
            if (!(sourceObject.BaseObject is string))
            {
                return new PSObject[] { sourceObject };
            }

            var d = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithTypeConverter(new PSObjectYamlTypeConverter())
                .WithNodeTypeResolver(new PSObjectYamlTypeResolver())
                .Build();

            var reader = new StringReader(sourceObject.BaseObject.ToString());
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
