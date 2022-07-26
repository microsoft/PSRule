// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PSRule
{
    internal static class JsonHelper
    {
        internal static PSObject ToPSObject(JToken token)
        {
            return token.ToObject<PSObject>(SingleObjectSerializer());
        }

        private static JsonSerializer SingleObjectSerializer()
        {
            var s = new JsonSerializer();
            s.Converters.Add(new PSObjectJsonConverter());
            return s;
        }
    }
}
