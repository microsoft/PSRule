using PSRule.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

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

        public static IEnumerable<PSObject> ConvertFromYaml(PSObject targetObject)
        {
            if (!(targetObject.BaseObject is string))
            {
                return new PSObject[] { targetObject };
            }

            var d = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .WithNodeTypeResolver(new HashtableTypeResolver())
                .Build();

            var reader = new StringReader(targetObject.BaseObject.ToString());
            var parser = new Parser(reader);

            parser.Expect<StreamStart>();

            var result = new List<PSObject>();

            while (parser.Accept<DocumentStart>())
            {
                var item = d.Deserialize<Hashtable>(parser: parser);
                result.Add(new PSObject(item));
            }

            return result.ToArray();
        }
    }
}
