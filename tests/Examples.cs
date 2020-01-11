using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Rql.NET;
using Xunit;

namespace tests
{
    public class Examples
    {
        public class SomeClass
        {   // Rql.NET.RqlOp.LIKE - cant use internals because they are const and c# doesn't like that
            [Rql.NET.Ops.Disallowed("$like", "$eq")]
            [Rql.NET.ColumnName("type")]
            [Rql.NET.FieldName("type")]
            [Rql.NET.Ignore]
            [Rql.NET.Ignore.Sort]
            [Rql.NET.Ignore.Filter]
            public string SomeProperty { get; set; }
        }

        // TODO: doesn't do anything currently but document usage, intended to verify all dbExpression property values are equal
        [Fact]
        public void RobustUse()
        {
            var rawJson = "";

            // Statically parse raw json
            var (dbExpression, err) = RqlParser.Parse<TestClass>(rawJson);

            // Alternatively parse a `RqlExpression` useful for avoiding nasty C# json string literals
            var rqlExpression = new RqlExpression
            {
                Filter = new Dictionary<string, object>() { },
            };
            (dbExpression, err) = RqlParser.Parse<TestClass>(rqlExpression);

            // Alternatively you can use a generic instance
            IRqlParser<TestClass> genericParser = new RqlParser<TestClass>();
            (dbExpression, err) = genericParser.Parse(rawJson);
            (dbExpression, err) = genericParser.Parse(rqlExpression);

            // Alternatively you can use a non-generic instance
            var classSpec = new ClassSpecBuilder().Build(typeof(TestClass));
            IRqlParser parser = new RqlParser(classSpec);
            (dbExpression, err) = parser.Parse(rawJson);
            (dbExpression, err) = parser.Parse(rqlExpression);
        }

        // TODO: doesn't do anything currently intended to verify dapper.crud integration
        [Fact]
        public void DapperCrudExtensions()
        {
            // using (var connection = await _connectionFactory.GetConnection())
            // {
            //     connection.Open();

            //     var parameters = new DynamicParameters(expression.Parameters);

            //     var page = Utility.GetPage(expression.Offset, expression.Limit);
            //     var where = $"WHERE {expression.Filter}";

            //     var items = (await connection.GetListPagedAsync<Item>(page, expression.Limit, where, expression.Sort, parameters)).ToList();
            // }
        }

        [Fact]
        public void Dapper()
        {
            // using (var connection = await _connectionFactory.GetConnection())
            // {
            //     connection.Open();
            //     // var parameters = new DynamicParameters(expression.Parameters);
            //     var sql = $"";
            //     var sites = await connection.QueryAsync<SiteSetting>(sql, parameters);
            //     return sites.SingleOrDefault();
            // }
        }

        [Fact]
        public void WebApi()
        {
            // [HttpPost("api/search")]
            // public async Task<ActionResult<PagedResult<Items>>> Search([FromBody] dynamic rql)
            // {
            //     var (dbExpression, errs) = _searchParser.Parse((rql as object).ToString());
            // }
        }
    }

    public static class Utility
    {
        public static int GetPage(int offset, int perPage)
        {
            if (offset <= 0 || perPage <= 0) return 1;
            var res = Math.Floor(offset / (double)perPage);
            if (res < 1) return 1;
            return (int)res;
        }

        public static bool IsEqual(DbExpression a, DbExpression b)
        {
            return a.Filter == b.Filter
            && a.Limit == b.Limit
            && a.Offset == b.Offset
            && a.Parameters == b.Parameters
            && a.Sort == b.Sort;
        }
    }
}
