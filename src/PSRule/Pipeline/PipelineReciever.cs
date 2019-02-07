using Newtonsoft.Json;
using PSRule.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace PSRule.Pipeline
{
    public delegate IEnumerable<PSObject> VisitTargetObject(PSObject targetObject);
    public delegate IEnumerable<PSObject> VisitTargetObjectAction(PSObject targetObject, VisitTargetObject next);

    public static class PipelineReceiverActions
    {
        public static IEnumerable<PSObject> PassThru(PSObject targetObject)
        {
            yield return targetObject;
        }

        public static IEnumerable<PSObject> ConvertFromJson(PSObject sourceObject)
        {
            if (!(sourceObject.BaseObject is string))
            {
                return new PSObject[] { sourceObject };
            }

            var result = new List<PSObject>();

            var value = JsonConvert.DeserializeObject<PSObject[]>(sourceObject.BaseObject.ToString(), new PSObjectArrayJsonConverter());

            return value;
        }

        public static IEnumerable<PSObject> ConvertFromYaml(PSObject sourceObject)
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
                result.Add(item);
            }

            return result.ToArray();
        }
    }
}
