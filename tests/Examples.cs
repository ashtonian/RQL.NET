using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient; // TODO: Microsoft.Data.SqlClient
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Rql.NET;
using Xunit;

namespace tests
{
    public class Examples
    {
        private IConnectionFactory _connectionFactory;
        public class SomeClass
        {
            // prevents operations
            [Rql.NET.Ops.Disallowed("$like", "$eq")]
            // overrides column namer
            [Rql.NET.ColumnName("type")]
            // overrides (json) namer
            [Rql.NET.FieldName("type")]
            // ignores entirely
            [Rql.NET.Ignore.Sort]
            // prevents sorting
            [Rql.NET.Ignore]
            // prevents filtering
            [Rql.NET.Ignore.Filter]
            public string SomeProp { get; set; }
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
