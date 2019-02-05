using PSRule.Pipeline;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Xunit;

namespace PSRule
{
    public sealed class HashtableSetDeserializerTests
    {
        [Fact]
        public void DeserializeObjects()
        {
            var yaml = GetYamlContent();
            var actual = PipelineReceiverActions.ConvertFromYaml(yaml).Select(o => o.BaseObject).OfType<Hashtable>().ToArray();

            Assert.Equal(4, actual.Length);
        }

        private string GetYamlContent()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ObjectFromFile.yaml");
            return File.ReadAllText(path);
        }
    }
}
