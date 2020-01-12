using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient; // TODO: Microsoft.Data.SqlClient
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using RQL.NET;
using Xunit;

namespace tests
{
    public class Examples
    {
        private IConnectionFactory _connectionFactory;
        public class SomeClass
        {
            // prevents operations
            [RQL.NET.Ops.Disallowed("$like", "$eq")]
            // overrides column namer
            [RQL.NET.ColumnName("type")]
            // overrides (json) namer
            [RQL.NET.FieldName("type")]
            // ignores entirely
            [RQL.NET.Ignore.Sort]
            // prevents sorting
            [RQL.NET.Ignore]
            // prevents filtering
            [RQL.NET.Ignore.Filter]
            public string SomeProp { get; set; }
        }

        public class Item
        {
            public bool IsDone { get; set; }
            public DateTime UpdatedAt { get; set; }
        }

        [Fact]
        public void QueryExamples()
        {
            var rqlExpression = new RqlExpression
            {
                Filter = new Dictionary<string, object>()
                {
                    ["isDone"] = true,
                    ["$or"] = new List<object>()
                    {
                        new Dictionary<string, object>(){
                            ["updatedAt"] = new Dictionary<string,object>(){
                                ["$lt"] = "2020/01/02",
                                ["$gt"] = 1577836800,
                            }
                        }
                    },
                },
                Limit = 1000,
                Offset = 0,
                Sort = new List<string>() { "-updatedAt" },
            };

            var (result, errs) = RqlParser.Parse<Item>(rqlExpression);
            Assert.True(errs == null);
            var expectation = "IsDone = @isDone AND ( UpdatedAt < @updatedAt AND UpdatedAt > @updatedAt2 )";
            Assert.Equal(expectation, result.Filter);
            Assert.True(result.Limit == 1000);
            Assert.True(result.Offset == 0);
            Assert.True(result.Sort == "UpdatedAt DESC");
            Assert.Equal(result.Parameters["@isDone"], true);
            Assert.Equal(result.Parameters["@updatedAt"], new DateTime(2020, 01, 02));
            Assert.Equal(result.Parameters["@updatedAt2"], new DateTime(2020, 01, 01));
        }

        // [Fact]
        public void RobustUse()
        {
            var rawJson = "";

            // Statically parse raw json
            var (dbExpression, errs) = RqlParser.Parse<TestClass>(rawJson);

            // Alternatively parse a `RqlExpression` useful for avoiding nasty C# json string literals
            var rqlExpression = new RqlExpression
            {
                Filter = new Dictionary<string, object>() { },
            };
            (dbExpression, errs) = RqlParser.Parse<TestClass>(rqlExpression);

            // Alternatively you can use a generic instance
            IRqlParser<TestClass> genericParser = new RqlParser<TestClass>();
            (dbExpression, errs) = genericParser.Parse(rawJson);
            (dbExpression, errs) = genericParser.Parse(rqlExpression);

            // Alternatively you can use a non-generic instance
            var classSpec = new ClassSpecBuilder().Build(typeof(TestClass));
            IRqlParser parser = new RqlParser(classSpec);
            (dbExpression, errs) = parser.Parse(rawJson);
            (dbExpression, errs) = parser.Parse(rqlExpression);
        }

        // [Fact]
        public async void ADOCommand()
        {
            DbExpression dbExpression = new DbExpression { };
            using (var connection = await _connectionFactory.GetConnection())
            using (SqlCommand command = new SqlCommand())
            {
                command.CommandText = $"SELECT * FROM TestClass WHERE ${dbExpression.Filter} LIMIT ${dbExpression.Limit} OFFSET ${dbExpression.Offset} ORDER BY ${dbExpression.Sort}";
                command.Parameters.AddRange(dbExpression.Parameters.Select(x => new SqlParameter(x.Key, x.Value)).ToArray());
                using (var reader = command.ExecuteReader())
                {
                    // do stuff
                }
            }
        }

        // [Fact]
        public async void DapperSimpleCRUD()
        {
            DbExpression dbExpression = new DbExpression { };
            using (var connection = await _connectionFactory.GetConnection())
            {
                connection.Open();
                var parameters = new DynamicParameters(dbExpression.Parameters);
                var page = Utility.GetPage(dbExpression.Offset, dbExpression.Limit);
                var where = $"WHERE {dbExpression.Filter}";
                var results = (await connection.GetListPagedAsync<TestClass>(page, dbExpression.Limit, where, dbExpression.Sort, parameters)).ToList();
                // do stuff
            }
        }

        // [Fact]
        public async void Dapper()
        {
            DbExpression dbExpression = new DbExpression { };
            using (var connection = await _connectionFactory.GetConnection())
            {
                connection.Open();
                var parameters = new DynamicParameters(dbExpression.Parameters);
                var sql = $"SELECT * FROM TestClass WHERE ${dbExpression.Filter} LIMIT ${dbExpression.Limit} OFFSET ${dbExpression.Offset} ORDER BY ${dbExpression.Sort}";
                var results = await connection.QueryAsync<TestClass>(sql, parameters);
                // do stuff
            }
        }
    }

    public interface IConnectionFactory
    {
        Task<IDbConnection> GetConnection();
    }

    public class ConnectionFactory : IConnectionFactory
    {
        private readonly string _connectionString;

        public ConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IDbConnection> GetConnection()
        {
            return await Task.Run(() => new SqlConnection(_connectionString));
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
