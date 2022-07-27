// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json.Linq;
using Xunit;

namespace PSRule
{
    public sealed class JsonHelperTests
    {
        [Fact]
        public void JTokenToPSObject()
        {
            var actual = JsonHelper.ToPSObject(GetJObject());
            Assert.NotNull(actual);
        }

        #region Helper methods

        private JToken GetJObject()
        {
            return JObject.Parse("{ \"metadata\": {}, \"parameters\": { \"sku\": { \"type\": \"string\", \"defaultValue\": \"Developer\", \"allowValues\": [ \"Developer\", \"Standard\", \"Premium\" ] } } }");
        }

        #endregion Helper methods
    }
}
