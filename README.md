# RQL.NET

`RQL.NET` is a resource query language for .NET intended for use with REST apps. It provides a simple, hackable api for creating dynamic sql queries. It is intended to sit between a web application and a SQL based database. It converts user submitted JSON query structures (inspired by mongodb query syntax) to sql queries, handling validation and type conversions. It was inspired by and is mostly compatible via the JSON interface with [rql (golang)](https://github.com/a8m/rql).

// TODO: simple screen shot

## Why

When creating a simple CRUD api its often a requirement to provide basic collection filtering functionality. Without implementing some other heavy layer (graphql, odata, ef), usually I would end up having to write code to support each field for a given class. Often by adding a query parameter for each field, and when using aggregate functions having an separate composite parameter for that aggregate ie `updated_at_gt=x&updated_at_lt=y`. Outside of that being cumbersome for lots of fields, this begins to totally breakdown when needing to apply a disjunction between two conditions using aggregate functions, something like `SELECT * FROM TABLE WHERE is_done = 1 OR (updated_at_lt < X AND updated_at_lt > Y)`.

## Getting Started

### Basic

```c#
// Statically parse raw json
var (dbExpression, err) = RqlParser.Parse<TestClass>(rawJson);

// TODO: daper or std sql usage
// TODO: assert show sql example
```

### Alternatives

```c#
// Alternatively parse a `RqlExpression` object - useful for avoiding nasty C# json string literals
var rqlExpression = new RqlExpression
{
  // TODO: create filter obj
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
```

// TODO: webapi, dapper,dapper.crud, mapper

### Common Customizations

```c#
public class SomeClass {
        [Rql.NET.Ops.Dissallowed()]
        [Rql.NET.ColumnName("type")]
        [Rql.NET.FieldName("type")]
}

```

### RQL Operations

  | RQL Op  | SQL Op |           Json Types |
  | :------ | :----: | -------------------: |
  | `$eq`   |  `=`   | number, string, bool |
  | `$neq`  |  `!=`  | number, string, bool |
  | `$lt`   |  `<`   |         number, bool |
  | `$gt`   |  `>`   |         number, bool |
  | `$lte`  |  `<=`  |         number, bool |
  | `$gte`  |  `>=`  |         number, bool |
  | `$like` | `like` |             string[] |
  | `$in`   |  `in`  |   number[], string[] |
  | `$nin`  | `nin`  |   number[], string[] |
  | `$or`   |  `or`  |                    - |
  | `$not`  | `not`  |                    - |
  | `$and`  | `and`  |                    - |
  | `$nor`  |   -    |                  n/a |

## Hackability

This library was structured to be a highly configurable parser. Most of the parser's components can be overriding directly or via a delegate or interface implementation via the [`Defaults`](Rql.NET/Defaults.cs) class. Most notably [`Defaults.DefaultConverter`](Rql.NET/Defaults.cs) and [`Defaults.DefaultValidator`](Rql.NET/DefaultTypeValidator.cs). Additionally many of the data structures and internal builders are exposed via public constructors to enable this packge to be used as a library. You could also implement a [custom class specification](Rql.NET/ClassSpecBuilder.cs), [field specification](Rql.NET/ClassSpecBuilder.cs), and [operation mapper](Rql.NET/IOpMapper.cs) relatively easily.

```c#
public static class Defaults
{
    public static string SortSeparator = ",";
    public static string Prefix = "$";
    public static int Offset = 0;
    public static int Limit = 10000;
    public static Func<IParameterTokenizer> DefaultTokenizerFactory = () => new NamedTokenizer();
    public static Func<string, Type, object, IError> DefaultValidator = DefaultTypeValidator.Validate;
    public static Func<string, string> DefaultColumnNamer = x => x;
    public static Func<string, string> DefaultFieldNamer = x => {...};
    public static IOpMapper DefaultOpMapper = new SqlMapper();
    public static ClassSpecCache SpecCache = new InMemoryClassSpecCache();
    public static Func<string, Type, object, (object, IError)> DefaultConverter =
        (fieldName, type, raw) =>
        {
          ...
        };
}
```

## Note on Performance

The parser uses reflection and by **default** its done once per class and cached. When using the typed parse statements `Parse<T>(RqlExpression exp)` and `Parse(RqlExpression exp)` there is a redundant json serialization and then deserialization because this was built piggy backing off the JContainer tree structure from JSON.NET. To avoid this penalty use the `Parse<T>(string exp)` and `Parse(string exp)` calls.

## TODO

- [ ] better coverage
- [ ] Release
  - [ ] enable multi platform targeting
  - [ ] auto build / publish
  - [ ] share/publish
- [ ] fix stricter validation - right side init object is and, or/nor is array
- [ ] fix empty object validation #1

## vNext

- [ ] document removing of token prefix to use with c# dynamic json literals
- [ ] attributes
  - [ ] class level
  - [ ] CustomTypeConverter
  - [ ] CustomValidator
  - [ ] DefaultSort
- [ ] nested "complex" data types support via joins or sub queries or..
- [ ] option to ignore validation
- [ ] better C# side query building solution because escaped json strings suck in .NET
- [ ] consider adjusting output to be an expression tree that people can access for hackability
- [ ] Better Test coverage
  - [ ] all attributes
  - [ ] all ops
  - [ ] Validators
  - [ ] IEnumberable and [] types
- [ ] benchmark tests, run on PRs
- [ ] would be cool to generate part of a swagger documentation from a class spec..
- [ ] js client lib
- [ ] contributing guidelines and issue template
- [ ] remove 3rd party dependency on JSON.NET, use own tre parse statementse  Initial leaf spec: {left, v, right, isField, iParse(RqlExpression exp)s there is a redudant Op, fieldSpecProperties...}
- [ ] typed more actionable errors
- [ ] official postgres / mongo support. Starting point: IOpMapper.
  - [ ] consider splitting package into RQL.Core and RQL.MSSQL to allow for RQL.Postgres or RQL.Mongo
