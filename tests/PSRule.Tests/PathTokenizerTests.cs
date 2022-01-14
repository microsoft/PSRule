// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Runtime.ObjectPath;
using Xunit;

namespace PSRule
{
    /// <summary>
    /// Tests for JSONPath tokenenizer.
    /// </summary>
    public sealed class PathTokenizerTests
    {
        [Fact]
        public void Get()
        {
            var token = PathTokenizer.Get("$[*].Properties.logs[?(@.enabled==true)].category");

            Assert.Equal(11, token.Length);
            Assert.Equal(PathTokenType.RootRef, token[0].Type);
            Assert.Equal(PathTokenType.IndexWildSelector, token[1].Type);
            Assert.Equal(PathTokenType.DotSelector, token[2].Type);
            Assert.Equal("Properties", token[2].As<string>());
            Assert.Equal(PathTokenType.DotSelector, token[3].Type);
            Assert.Equal("logs", token[3].As<string>());
            Assert.Equal(PathTokenType.StartFilter, token[4].Type);
            Assert.Equal(PathTokenType.CurrentRef, token[5].Type);
            Assert.Equal(PathTokenType.DotSelector, token[6].Type);
            Assert.Equal("enabled", token[6].As<string>());
            Assert.Equal(PathTokenType.ComparisonOperator, token[7].Type);
            Assert.Equal(FilterOperator.Equal, token[7].As<FilterOperator>());
            Assert.Equal(PathTokenType.Boolean, token[8].Type);
            Assert.True(token[8].As<bool>());
            Assert.Equal(PathTokenType.EndFilter, token[9].Type);
            Assert.Equal(PathTokenType.DotSelector, token[10].Type);
            Assert.Equal("category", token[10].As<string>());
        }

        /// <summary>
        /// Check tokenizer against simple test cases.
        /// </summary>
        [Fact]
        public void SimpleTestCases()
        {
            var path = new string[]
            {
                "store",
                ".",
                "@",
                "$.",
                "'store.property'",
                "$[10]",
                "$[*]",
                "$['store.property']",
                "$[\"store.property\"]",
                "\"store.property\"",
            };

            // store
            var token = PathTokenizer.Get(path[0]);
            Assert.Single(token);
            Assert.Equal(PathTokenType.DotSelector, token[0].Type);
            Assert.Equal("store", token[0].As<string>());

            // .
            token = PathTokenizer.Get(path[1]);
            Assert.Single(token);
            Assert.Equal(PathTokenType.RootRef, token[0].Type);

            // @
            token = PathTokenizer.Get(path[2]);
            Assert.Single(token);
            Assert.Equal(PathTokenType.RootRef, token[0].Type);

            // $.
            token = PathTokenizer.Get(path[3]);
            Assert.Single(token);
            Assert.Equal(PathTokenType.RootRef, token[0].Type);

            // 'store.property'
            token = PathTokenizer.Get(path[4]);
            Assert.Single(token);
            Assert.Equal(PathTokenType.DotSelector, token[0].Type);
            Assert.Equal("store.property", token[0].As<string>());

            // $[10]
            token = PathTokenizer.Get(path[5]);
            Assert.Equal(2, token.Length);
            Assert.Equal(PathTokenType.RootRef, token[0].Type);
            Assert.Equal(PathTokenType.IndexSelector, token[1].Type);
            Assert.Equal(10, token[1].As<int>());

            // $[*]
            token = PathTokenizer.Get(path[6]);
            Assert.Equal(2, token.Length);
            Assert.Equal(PathTokenType.RootRef, token[0].Type);
            Assert.Equal(PathTokenType.IndexWildSelector, token[1].Type);

            // $['store.property']
            token = PathTokenizer.Get(path[7]);
            Assert.Equal(2, token.Length);
            Assert.Equal(PathTokenType.RootRef, token[0].Type);
            Assert.Equal(PathTokenType.DotSelector, token[1].Type);
            Assert.Equal("store.property", token[1].As<string>());

            // $["store.property"]
            token = PathTokenizer.Get(path[8]);
            Assert.Equal(2, token.Length);
            Assert.Equal(PathTokenType.RootRef, token[0].Type);
            Assert.Equal(PathTokenType.DotSelector, token[1].Type);
            Assert.Equal("store.property", token[1].As<string>());

            // "store.property"
            token = PathTokenizer.Get(path[9]);
            Assert.Single(token);
            Assert.Equal(PathTokenType.DotSelector, token[0].Type);
            Assert.Equal("store.property", token[0].As<string>());
        }

        [Fact]
        public void PathTests()
        {
            var path = new string[]
            {
                "$['store'].book[0].author",
            };

            // $['store'].book[0].author
            var token = PathTokenizer.Get(path[0]);
            Assert.Equal(5, token.Length);
            Assert.Equal(PathTokenType.RootRef, token[0].Type);
            Assert.Equal(PathTokenType.DotSelector, token[1].Type);
            Assert.Equal("store", token[1].As<string>());
            Assert.Equal(PathTokenType.DotSelector, token[2].Type);
            Assert.Equal("book", token[2].As<string>());
            Assert.Equal(PathTokenType.IndexSelector, token[3].Type);
            Assert.Equal(0, token[3].As<int>());
            Assert.Equal(PathTokenType.DotSelector, token[4].Type);
            Assert.Equal("author", token[4].As<string>());
        }

        [Fact]
        public void MemberNameSchema()
        {
            var path = new string[]
            {
                "$schema"
            };

            // $schema
            var token = PathTokenizer.Get(path[0]);
            Assert.Single(token);
            Assert.Equal(PathTokenType.DotSelector, token[0].Type);
            Assert.Equal("$schema", token[0].As<string>());
        }

        [Fact]
        public void MemberNameWithUnderscore()
        {
            var path = new string[]
            {
                "member_name",
            };

            // member_name
            var token = PathTokenizer.Get(path[0]);
            Assert.Single(token);
            Assert.Equal(PathTokenType.DotSelector, token[0].Type);
            Assert.Equal("member_name", token[0].As<string>());
        }

        [Fact]
        public void MemberNameWithDash()
        {
            var path = new string[]
            {
                "member-name",
                "-member-name-",
            };

            // member-name
            var token = PathTokenizer.Get(path[0]);
            Assert.Single(token);
            Assert.Equal(PathTokenType.DotSelector, token[0].Type);
            Assert.Equal("member-name", token[0].As<string>());

            // -member-name-
            token = PathTokenizer.Get(path[1]);
            Assert.Single(token);
            Assert.Equal(PathTokenType.DotSelector, token[0].Type);
            Assert.Equal("member-name", token[0].As<string>());
        }

        [Fact]
        public void MemberNameWithOption()
        {
            var path = new string[]
            {
                "$.name",
                "$+name"
            };

            // $.name
            var token = PathTokenizer.Get(path[0]);
            Assert.Equal(2, token.Length);
            Assert.Equal(PathTokenType.RootRef, token[0].Type);
            Assert.Equal(PathTokenType.DotSelector, token[1].Type);
            Assert.Equal(PathTokenOption.None, token[1].Option);
            Assert.Equal("name", token[1].As<string>());

            // $+name
            token = PathTokenizer.Get(path[1]);
            Assert.Equal(2, token.Length);
            Assert.Equal(PathTokenType.RootRef, token[0].Type);
            Assert.Equal(PathTokenType.DotSelector, token[1].Type);
            Assert.Equal(PathTokenOption.CaseSensitive, token[1].Option);
            Assert.Equal("name", token[1].As<string>());
        }

        [Fact]
        public void MemberNameQuoted()
        {
            var path = new string[]
            {
                "'store.property'",
                "\"store.property\"",
                "['store.property']",
                "[\"store.property\"]",
            };

            // 'store.property'
            var token = PathTokenizer.Get(path[0]);
            Assert.Single(token);
            Assert.Equal(PathTokenType.DotSelector, token[0].Type);
            Assert.Equal("store.property", token[0].As<string>());

            // "store.property"
            token = PathTokenizer.Get(path[1]);
            Assert.Single(token);
            Assert.Equal(PathTokenType.DotSelector, token[0].Type);
            Assert.Equal("store.property", token[0].As<string>());

            // ['store.property']
            token = PathTokenizer.Get(path[2]);
            Assert.Single(token);
            Assert.Equal(PathTokenType.DotSelector, token[0].Type);
            Assert.Equal("store.property", token[0].As<string>());

            // ["store.property"]
            token = PathTokenizer.Get(path[3]);
            Assert.Single(token);
            Assert.Equal(PathTokenType.DotSelector, token[0].Type);
            Assert.Equal("store.property", token[0].As<string>());
        }

        [Fact]
        public void FilterBoolean()
        {
            var path = new string[]
            {
                "$[?(@.enabled==true)]",
                "$[?@.enabled==false]",
            };

            // $[?(@.enabled==true)]
            var actual = PathTokenizer.Get(path[0]);
            Assert.Equal(7, actual.Length);
            Assert.Equal(PathTokenType.RootRef, actual[0].Type);
            Assert.Equal(PathTokenType.StartFilter, actual[1].Type);
            Assert.Equal(PathTokenType.CurrentRef, actual[2].Type);
            Assert.Equal(PathTokenType.DotSelector, actual[3].Type);
            Assert.Equal("enabled", actual[3].As<string>());
            Assert.Equal(PathTokenType.ComparisonOperator, actual[4].Type);
            Assert.Equal(FilterOperator.Equal, actual[4].As<FilterOperator>());
            Assert.Equal(PathTokenType.Boolean, actual[5].Type);
            Assert.True(actual[5].As<bool>());
            Assert.Equal(PathTokenType.EndFilter, actual[6].Type);

            // $[?(@.enabled==false)]
            actual = PathTokenizer.Get(path[1]);
            Assert.Equal(7, actual.Length);
            Assert.Equal(PathTokenType.RootRef, actual[0].Type);
            Assert.Equal(PathTokenType.StartFilter, actual[1].Type);
            Assert.Equal(PathTokenType.CurrentRef, actual[2].Type);
            Assert.Equal(PathTokenType.DotSelector, actual[3].Type);
            Assert.Equal("enabled", actual[3].As<string>());
            Assert.Equal(PathTokenType.ComparisonOperator, actual[4].Type);
            Assert.Equal(FilterOperator.Equal, actual[4].As<FilterOperator>());
            Assert.Equal(PathTokenType.Boolean, actual[5].Type);
            Assert.False(actual[5].As<bool>());
            Assert.Equal(PathTokenType.EndFilter, actual[6].Type);
        }

        [Fact]
        public void FilterInteger()
        {
            var path = new string[]
            {
                "$[?(@.price<10)]",
                "$[?(@.price < 10)]",
            };

            // $[?(@.price<10)]
            var actual = PathTokenizer.Get(path[0]);
            Assert.Equal(7, actual.Length);
            Assert.Equal(PathTokenType.RootRef, actual[0].Type);
            Assert.Equal(PathTokenType.StartFilter, actual[1].Type);
            Assert.Equal(PathTokenType.CurrentRef, actual[2].Type);
            Assert.Equal(PathTokenType.DotSelector, actual[3].Type);
            Assert.Equal("price", actual[3].As<string>());
            Assert.Equal(PathTokenType.ComparisonOperator, actual[4].Type);
            Assert.Equal(FilterOperator.Less, actual[4].As<FilterOperator>());
            Assert.Equal(PathTokenType.Integer, actual[5].Type);
            Assert.Equal(10, actual[5].As<int>());
            Assert.Equal(PathTokenType.EndFilter, actual[6].Type);

            // $[?(@.price < 10)]
            actual = PathTokenizer.Get(path[1]);
            Assert.Equal(7, actual.Length);
            Assert.Equal(PathTokenType.RootRef, actual[0].Type);
            Assert.Equal(PathTokenType.StartFilter, actual[1].Type);
            Assert.Equal(PathTokenType.CurrentRef, actual[2].Type);
            Assert.Equal(PathTokenType.DotSelector, actual[3].Type);
            Assert.Equal("price", actual[3].As<string>());
            Assert.Equal(PathTokenType.ComparisonOperator, actual[4].Type);
            Assert.Equal(FilterOperator.Less, actual[4].As<FilterOperator>());
            Assert.Equal(PathTokenType.Integer, actual[5].Type);
            Assert.Equal(10, actual[5].As<int>());
            Assert.Equal(PathTokenType.EndFilter, actual[6].Type);
        }

        [Fact]
        public void FilterString()
        {
            var path = new string[]
            {
                "$[?(@.id=='1')]",
                "$[?(@.id == \"1\")]",
            };

            // $[?(@.id=='1')]
            var actual = PathTokenizer.Get(path[0]);
            //Assert.Equal(7, actual.Length);
            Assert.Equal(PathTokenType.RootRef, actual[0].Type);
            Assert.Equal(PathTokenType.StartFilter, actual[1].Type);
            Assert.Equal(PathTokenType.CurrentRef, actual[2].Type);
            Assert.Equal(PathTokenType.DotSelector, actual[3].Type);
            Assert.Equal("id", actual[3].As<string>());
            Assert.Equal(PathTokenType.ComparisonOperator, actual[4].Type);
            Assert.Equal(FilterOperator.Equal, actual[4].As<FilterOperator>());
            Assert.Equal(PathTokenType.String, actual[5].Type);
            Assert.Equal("1", actual[5].As<string>());
            Assert.Equal(PathTokenType.EndFilter, actual[6].Type);

            // $[?(@.id == "1")]
            actual = PathTokenizer.Get(path[1]);
            Assert.Equal(7, actual.Length);
            Assert.Equal(PathTokenType.RootRef, actual[0].Type);
            Assert.Equal(PathTokenType.StartFilter, actual[1].Type);
            Assert.Equal(PathTokenType.CurrentRef, actual[2].Type);
            Assert.Equal(PathTokenType.DotSelector, actual[3].Type);
            Assert.Equal("id", actual[3].As<string>());
            Assert.Equal(PathTokenType.ComparisonOperator, actual[4].Type);
            Assert.Equal(FilterOperator.Equal, actual[4].As<FilterOperator>());
            Assert.Equal(PathTokenType.String, actual[5].Type);
            Assert.Equal("1", actual[5].As<string>());
            Assert.Equal(PathTokenType.EndFilter, actual[6].Type);
        }

        [Fact]
        public void FilterExists()
        {
            var path = new string[]
            {
                "$[?@.Spec.Properties.Kind].TargetName",
            };

            // $[?@.Spec.Properties.Kind].TargetName
            var actual = PathTokenizer.Get(path[0]);
            Assert.Equal(8, actual.Length);
            Assert.Equal(PathTokenType.RootRef, actual[0].Type);
            Assert.Equal(PathTokenType.StartFilter, actual[1].Type);
            Assert.Equal(PathTokenType.CurrentRef, actual[2].Type);
            Assert.Equal(PathTokenType.DotSelector, actual[3].Type);
            Assert.Equal("Spec", actual[3].As<string>());
            Assert.Equal(PathTokenType.DotSelector, actual[4].Type);
            Assert.Equal("Properties", actual[4].As<string>());
            Assert.Equal(PathTokenType.DotSelector, actual[5].Type);
            Assert.Equal("Kind", actual[5].As<string>());
            Assert.Equal(PathTokenType.EndFilter, actual[6].Type);
            Assert.Equal(PathTokenType.DotSelector, actual[7].Type);
            Assert.Equal("TargetName", actual[7].As<string>());
        }

        [Fact]
        public void FilterNot()
        {
            var path = new string[]
            {
                "$[?(!@.enabled)]",
                "$[?!@.enabled]"
            };

            // $[?(!@.enabled)]
            var actual = PathTokenizer.Get(path[0]);
            Assert.Equal(6, actual.Length);
            Assert.Equal(PathTokenType.RootRef, actual[0].Type);
            Assert.Equal(PathTokenType.StartFilter, actual[1].Type);
            Assert.Equal(PathTokenType.NotOperator, actual[2].Type);
            Assert.Equal(PathTokenType.CurrentRef, actual[3].Type);
            Assert.Equal(PathTokenType.DotSelector, actual[4].Type);
            Assert.Equal("enabled", actual[4].As<string>());
            Assert.Equal(PathTokenType.EndFilter, actual[5].Type);

            // $[?!@.enabled]
            actual = PathTokenizer.Get(path[1]);
            Assert.Equal(6, actual.Length);
            Assert.Equal(PathTokenType.RootRef, actual[0].Type);
            Assert.Equal(PathTokenType.StartFilter, actual[1].Type);
            Assert.Equal(PathTokenType.NotOperator, actual[2].Type);
            Assert.Equal(PathTokenType.CurrentRef, actual[3].Type);
            Assert.Equal(PathTokenType.DotSelector, actual[4].Type);
            Assert.Equal("enabled", actual[4].As<string>());
            Assert.Equal(PathTokenType.EndFilter, actual[5].Type);
        }

        [Fact]
        public void FilterOr()
        {
            var path = new string[]
            {
                "$[?(@.on == true || @.enabled == true)]",
                "$[?(@.on || @.enabled == true)]",
            };

            // $[?(@.on == true || @.enabled == true)]
            var actual = PathTokenizer.Get(path[0]);
            Assert.Equal(12, actual.Length);
            Assert.Equal(PathTokenType.RootRef, actual[0].Type);
            Assert.Equal(PathTokenType.StartFilter, actual[1].Type);

            Assert.Equal(PathTokenType.CurrentRef, actual[2].Type);
            Assert.Equal(PathTokenType.DotSelector, actual[3].Type);
            Assert.Equal("on", actual[3].As<string>());
            Assert.Equal(PathTokenType.ComparisonOperator, actual[4].Type);
            Assert.Equal(FilterOperator.Equal, actual[4].As<FilterOperator>());
            Assert.Equal(PathTokenType.Boolean, actual[5].Type);
            Assert.True(actual[5].As<bool>());

            Assert.Equal(PathTokenType.LogicalOperator, actual[6].Type);
            Assert.Equal(FilterOperator.Or, actual[6].As<FilterOperator>());

            Assert.Equal(PathTokenType.CurrentRef, actual[7].Type);
            Assert.Equal(PathTokenType.DotSelector, actual[8].Type);
            Assert.Equal("enabled", actual[8].As<string>());
            Assert.Equal(FilterOperator.Equal, actual[9].As<FilterOperator>());
            Assert.Equal(PathTokenType.Boolean, actual[10].Type);
            Assert.True(actual[10].As<bool>());

            Assert.Equal(PathTokenType.EndFilter, actual[11].Type);
        }

        [Fact]
        public void ArraySlice()
        {
            var path = new string[]
            {
                "$.items[-1:]",
                "$.items[1:2:-1]",
                "$.items[:2]",
                "$.items[::2]",
                "$.items[::-1].id",
                "$.items[:1].id",
            };

            // $.items[-1:]
            var token = PathTokenizer.Get(path[0]);
            Assert.Equal(3, token.Length);
            Assert.Equal(PathTokenType.RootRef, token[0].Type);
            Assert.Equal(PathTokenType.DotSelector, token[1].Type);
            Assert.Equal("items", token[1].As<string>());
            Assert.Equal(PathTokenType.ArraySliceSelector, token[2].Type);
            Assert.Equal(-1, token[2].As<int?[]>()[0]);
            Assert.Null(token[2].As<int?[]>()[1]);
            Assert.Null(token[2].As<int?[]>()[2]);

            // $.items[1:2:-1]
            token = PathTokenizer.Get(path[1]);
            Assert.Equal(3, token.Length);
            Assert.Equal(PathTokenType.RootRef, token[0].Type);
            Assert.Equal(PathTokenType.DotSelector, token[1].Type);
            Assert.Equal("items", token[1].As<string>());
            Assert.Equal(PathTokenType.ArraySliceSelector, token[2].Type);
            Assert.Equal(1, token[2].As<int?[]>()[0]);
            Assert.Equal(2, token[2].As<int?[]>()[1]);
            Assert.Equal(-1, token[2].As<int?[]>()[2]);

            // $.items[:2]
            token = PathTokenizer.Get(path[2]);
            Assert.Equal(3, token.Length);
            Assert.Equal(PathTokenType.RootRef, token[0].Type);
            Assert.Equal(PathTokenType.DotSelector, token[1].Type);
            Assert.Equal("items", token[1].As<string>());
            Assert.Equal(PathTokenType.ArraySliceSelector, token[2].Type);
            Assert.Null(token[2].As<int?[]>()[0]);
            Assert.Equal(2, token[2].As<int?[]>()[1]);
            Assert.Null(token[2].As<int?[]>()[2]);

            // $.items[::2]
            token = PathTokenizer.Get(path[3]);
            Assert.Equal(3, token.Length);
            Assert.Equal(PathTokenType.RootRef, token[0].Type);
            Assert.Equal(PathTokenType.DotSelector, token[1].Type);
            Assert.Equal("items", token[1].As<string>());
            Assert.Equal(PathTokenType.ArraySliceSelector, token[2].Type);
            Assert.Null(token[2].As<int?[]>()[0]);
            Assert.Null(token[2].As<int?[]>()[1]);
            Assert.Equal(2, token[2].As<int?[]>()[2]);

            // $.items[::-1].id
            token = PathTokenizer.Get(path[4]);
            Assert.Equal(4, token.Length);
            Assert.Equal(PathTokenType.RootRef, token[0].Type);
            Assert.Equal(PathTokenType.DotSelector, token[1].Type);
            Assert.Equal("items", token[1].As<string>());
            Assert.Equal(PathTokenType.ArraySliceSelector, token[2].Type);
            Assert.Null(token[2].As<int?[]>()[0]);
            Assert.Null(token[2].As<int?[]>()[1]);
            Assert.Equal(-1, token[2].As<int?[]>()[2]);
            Assert.Equal(PathTokenType.DotSelector, token[3].Type);
            Assert.Equal("id", token[3].As<string>());

            // $.items[:1].id
            token = PathTokenizer.Get(path[5]);
            Assert.Equal(4, token.Length);
            Assert.Equal(PathTokenType.RootRef, token[0].Type);
            Assert.Equal(PathTokenType.DotSelector, token[1].Type);
            Assert.Equal("items", token[1].As<string>());
            Assert.Equal(PathTokenType.ArraySliceSelector, token[2].Type);
            Assert.Null(token[2].As<int?[]>()[0]);
            Assert.Equal(1, token[2].As<int?[]>()[1]);
            Assert.Null(token[2].As<int?[]>()[2]);
            Assert.Equal(PathTokenType.DotSelector, token[3].Type);
            Assert.Equal("id", token[3].As<string>());
        }

        [Fact]
        public void Union()
        {
            var path = new string[]
            {
                "$.items[1,2]",
                "$.items[ 1 , 2 ]",
                "$.items['name','value']",
                "$.items[ \"name\" , \"value\" ]",
            };

            // $.items[1,2]
            var token = PathTokenizer.Get(path[0]);
            Assert.Equal(3, token.Length);
            Assert.Equal(PathTokenType.RootRef, token[0].Type);
            Assert.Equal(PathTokenType.DotSelector, token[1].Type);
            Assert.Equal("items", token[1].As<string>());
            Assert.Equal(PathTokenType.UnionIndexSelector, token[2].Type);
            Assert.Equal(1, token[2].As<int[]>()[0]);
            Assert.Equal(2, token[2].As<int[]>()[1]);

            // $.items[ 1 , 2 ]
            token = PathTokenizer.Get(path[1]);
            Assert.Equal(3, token.Length);
            Assert.Equal(PathTokenType.RootRef, token[0].Type);
            Assert.Equal(PathTokenType.DotSelector, token[1].Type);
            Assert.Equal("items", token[1].As<string>());
            Assert.Equal(PathTokenType.UnionIndexSelector, token[2].Type);
            Assert.Equal(1, token[2].As<int[]>()[0]);
            Assert.Equal(2, token[2].As<int[]>()[1]);

            // $.items['name','value']
            token = PathTokenizer.Get(path[2]);
            Assert.Equal(3, token.Length);
            Assert.Equal(PathTokenType.RootRef, token[0].Type);
            Assert.Equal(PathTokenType.DotSelector, token[1].Type);
            Assert.Equal("items", token[1].As<string>());
            Assert.Equal(PathTokenType.UnionQuotedMemberSelector, token[2].Type);
            Assert.Equal("name", token[2].As<string[]>()[0]);
            Assert.Equal("value", token[2].As<string[]>()[1]);

            // $.items[ "name" , "value" ]
            token = PathTokenizer.Get(path[3]);
            Assert.Equal(3, token.Length);
            Assert.Equal(PathTokenType.RootRef, token[0].Type);
            Assert.Equal(PathTokenType.DotSelector, token[1].Type);
            Assert.Equal("items", token[1].As<string>());
            Assert.Equal(PathTokenType.UnionQuotedMemberSelector, token[2].Type);
            Assert.Equal("name", token[2].As<string[]>()[0]);
            Assert.Equal("value", token[2].As<string[]>()[1]);
        }

        /// <summary>
        /// Check tokenizer against standard test cases. https://goessner.net/articles/JsonPath/index.html
        /// </summary>
        [Fact]
        public void StandardTestCases()
        {
            var path = new string[]
            {
                "$.store.book[*].author",
                "$..author",
                "$.store.*",
                "$.store..price",
                "$..book[2]",
                "$..book[(@.length-1)]",
                "$..book[-1:]",
                "$..book[0,1]",
                "$..book[:2]",
                "$..book[?(@.isbn)]",
                "$..book[?(@.price<10)]",
                "$..*"
            };

            // $.store.book[*].author
            var token = PathTokenizer.Get(path[0]);
            Assert.Equal(5, token.Length);
            Assert.Equal(PathTokenType.RootRef, token[0].Type);
            Assert.Equal(PathTokenType.DotSelector, token[1].Type);
            Assert.Equal("store", token[1].As<string>());
            Assert.Equal(PathTokenType.DotSelector, token[2].Type);
            Assert.Equal("book", token[2].As<string>());
            Assert.Equal(PathTokenType.IndexWildSelector, token[3].Type);
            Assert.Equal(PathTokenType.DotSelector, token[4].Type);
            Assert.Equal("author", token[4].As<string>());

            // $..author
            token = PathTokenizer.Get(path[1]);
            Assert.Equal(2, token.Length);
            Assert.Equal(PathTokenType.RootRef, token[0].Type);
            Assert.Equal(PathTokenType.DescendantSelector, token[1].Type);
            Assert.Equal("author", token[1].As<string>());

            // $.store.*
            token = PathTokenizer.Get(path[2]);
            Assert.Equal(3, token.Length);
            Assert.Equal(PathTokenType.RootRef, token[0].Type);
            Assert.Equal(PathTokenType.DotSelector, token[1].Type);
            Assert.Equal("store", token[1].As<string>());
            Assert.Equal(PathTokenType.DotWildSelector, token[2].Type);

            // $.store..price
            token = PathTokenizer.Get(path[3]);
            Assert.Equal(3, token.Length);
            Assert.Equal(PathTokenType.RootRef, token[0].Type);
            Assert.Equal(PathTokenType.DotSelector, token[1].Type);
            Assert.Equal("store", token[1].As<string>());
            Assert.Equal(PathTokenType.DescendantSelector, token[2].Type);
            Assert.Equal("price", token[2].As<string>());

            // $..book[2]
            token = PathTokenizer.Get(path[4]);
            Assert.Equal(3, token.Length);
            Assert.Equal(PathTokenType.RootRef, token[0].Type);
            Assert.Equal(PathTokenType.DescendantSelector, token[1].Type);
            Assert.Equal("book", token[1].As<string>());
            Assert.Equal(PathTokenType.IndexSelector, token[2].Type);
            Assert.Equal(2, token[2].As<int>());

            // $..book[(@.length-1)]
            token = PathTokenizer.Get(path[5]);

            // $..book[-1:]
            token = PathTokenizer.Get(path[6]);
            Assert.Equal(3, token.Length);
            Assert.Equal(PathTokenType.RootRef, token[0].Type);
            Assert.Equal(PathTokenType.DescendantSelector, token[1].Type);
            Assert.Equal("book", token[1].As<string>());
            Assert.Equal(PathTokenType.ArraySliceSelector, token[2].Type);

            // $..book[0,1]
            token = PathTokenizer.Get(path[7]);
            Assert.Equal(3, token.Length);
            Assert.Equal(PathTokenType.RootRef, token[0].Type);
            Assert.Equal(PathTokenType.DescendantSelector, token[1].Type);
            Assert.Equal("book", token[1].As<string>());
            Assert.Equal(PathTokenType.UnionIndexSelector, token[2].Type);
            Assert.Equal(0, token[2].As<int[]>()[0]);
            Assert.Equal(1, token[2].As<int[]>()[1]);

            // $..book[:2]
            token = PathTokenizer.Get(path[8]);
            Assert.Equal(3, token.Length);
            Assert.Equal(PathTokenType.RootRef, token[0].Type);
            Assert.Equal(PathTokenType.DescendantSelector, token[1].Type);
            Assert.Equal("book", token[1].As<string>());
            Assert.Equal(PathTokenType.ArraySliceSelector, token[2].Type);

            // $..book[?(@.isbn)]
            token = PathTokenizer.Get(path[9]);
            Assert.Equal(6, token.Length);
            Assert.Equal(PathTokenType.RootRef, token[0].Type);
            Assert.Equal(PathTokenType.DescendantSelector, token[1].Type);
            Assert.Equal("book", token[1].As<string>());
            Assert.Equal(PathTokenType.StartFilter, token[2].Type);
            Assert.Equal(PathTokenType.CurrentRef, token[3].Type);
            Assert.Equal(PathTokenType.DotSelector, token[4].Type);
            Assert.Equal("isbn", token[4].As<string>());
            Assert.Equal(PathTokenType.EndFilter, token[5].Type);

            // $..book[?(@.price<10)]
            token = PathTokenizer.Get(path[10]);
            Assert.Equal(8, token.Length);
            Assert.Equal(PathTokenType.RootRef, token[0].Type);
            Assert.Equal(PathTokenType.DescendantSelector, token[1].Type);
            Assert.Equal("book", token[1].As<string>());
            Assert.Equal(PathTokenType.StartFilter, token[2].Type);
            Assert.Equal(PathTokenType.CurrentRef, token[3].Type);
            Assert.Equal(PathTokenType.DotSelector, token[4].Type);
            Assert.Equal("price", token[4].As<string>());
            Assert.Equal(PathTokenType.ComparisonOperator, token[5].Type);
            Assert.Equal(FilterOperator.Less, token[5].As<FilterOperator>());
            Assert.Equal(PathTokenType.Integer, token[6].Type);
            Assert.Equal(10, token[6].As<int>());
            Assert.Equal(PathTokenType.EndFilter, token[7].Type);

            // $..*
            token = PathTokenizer.Get(path[11]);
        }
    }
}
