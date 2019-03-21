using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Rql.NET;
using Xunit;

namespace tests
{
    public class TestClass
    {
        public long T_Long { get; set; }
        public float T_Float { get; set; }
        public int T_Int { get; set; }
        public string T_String { get; set; }
        public bool T_Bool { get; set; }
        public DateTime T_DateTime { get; set; }
        public DateTime T_DateTime2 { get; set; }
    }

    public class TestClass2
    {
        public int X_Int { get; set; }
        public string X_String { get; set; }
        public bool X_Bool { get; set; }
        public DateTime X_DateTime { get; set; }
    }

    public class ParserTests
    {
        [Fact]
        public void Complex()
        {
            var raw = @"{ 
                            ""t_Long"": 3,
                            ""$or"" : [ 
                                {""t_Long"" : {""$gte"" : 1 }},
                                {""t_Long"" : {""$lte"" : 5 }},
                                {""t_Long"" : 20 }
                            ],
                            ""t_Bool"": true,
                            ""$and"" : { ""t_String"": {""$like"":""%testing%"", ""$neq"" : ""test""} },
                            ""t_Int"" : { ""$in"" : [1,2] }
                        }
                  ";
            var (result, errs) = RqlParser.Parse<TestClass>(raw);
            Assert.True(errs == null);
            Assert.Equal(
                "T_Long = @t_Long AND ( T_Long >= @t_Long2 OR T_Long <= @t_Long3 OR T_Long = @t_Long4 ) AND T_Bool = @t_Bool AND ( T_String LIKE @t_String AND T_String != @t_String2 ) AND T_Int IN @t_Int",
                result.Filter);
            Console.WriteLine(result.Filter);
        }

        [Fact]
        public void Play()
        {
            IRqlParser<TestClass> test = new Parser<TestClass>();
            var raw = @"{ 
                            ""t_Long"": 3,
                            ""$or"" : [ 
                                {""t_Long"" : {""$gte"" : 1 }},
                                {""t_Long"" : {""$lte"" : 5 }},
                                {""t_Long"" : 20 }
                            ],
                            ""t_Bool"": true,
                            ""$and"" : { ""t_String"": {""$like"":""%testing%"", ""$neq"" : ""test""} },
                            ""t_Int"" : { ""$in"" : [1,2] }
                        }
                  ";
            var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(raw);
            Console.WriteLine(values);
        }


        [Fact]
        public void SimpleAnd()
        {
            var raw = @"{   
                            ""t_Long"": 3,
                            ""t_Int"": 2, 
                            ""t_Float"": 3.4, 
                            ""t_String"": ""str"", 
                            ""t_Bool"": true,
                            ""t_DateTime"" : ""2019-03-20T01:21:25.467589-05:00"",
                            ""t_DateTime2"" : 1553063286
                        }
            ";
            var (result, errs) = RqlParser.Parse<TestClass>(raw);
            Assert.True(errs == null);
            Assert.Equal(
                "T_Long = @t_Long AND T_Int = @t_Int AND T_Float = @t_Float AND T_String = @t_String AND T_Bool = @t_Bool AND T_DateTime = @t_DateTime AND T_DateTime2 = @t_DateTime2",
                result.Filter);
        }
    }
}