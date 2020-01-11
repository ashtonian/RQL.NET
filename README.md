# RQL.NET

`RQL.NET` is a resource query language for .NET intended for use with REST apps. It provides a simple, hackable api for creating dynamic sql queries. It is intended to sit between a web application and a SQL based database. It converts user submitted JSON query structures (inspired by mongodb query syntax) to sql queries, handling validation and type conversions. It was inspired by and is compatible via the JSON interface with [rql (golang)](https://github.com/a8m/rql).

// TODO: simple screen shot

## Why

When creating a simple CRUD api its often a requirement to provide basic collection filtering functionality. Without implementing some other heavy layer (graphql, odata, ef), usually I would end up having to write code to support each field for a given class. Often by adding a query parameter for each field, and when using aggregate functions having an separate composite parameter for that aggregate ie `updated_at_gt=x&updated_at_lt=y`. Outside of that being cumbersome for lots of fields, this begins to totally breakdown when needing to apply a disjunction between two conditions using aggregate functions, something like `SELECT * FROM TABLE WHERE is_done = 1 OR (updated_at_lt < X AND updated_at_lt > Y)`.

## Getting Started

Reflection is used once for a given type per application and cached *when* using the default cache implementation.

### Supported Operations

int,string,bool |$eq| =
int,string,bool |$neq|!=
int,string,bool |$lt| <
int,string,bool |$gt| >
int,string,bool |$lte| <=
int,string,bool |$gte| >=
int,string,bool |$like| like
int,string,bool |$or| or
int,string,bool |$nor| nor
int,string,bool |$not|not
int,string,bool |$and|and
int,string,bool |$in|in
int,string,bool |$nin|nin


## Hackability

This library was structured to be a highly configurable parser. Most of the parser's components can be overriding directly or via a delegate or interface implementation via the [`Defaults`](Rql.NET/Defaults.cs) class. Most notably [`Defaults.DefaultConverter`](Rql.NET/Defaults.cs) and [`Defaults.DefaultValidator`](Rql.NET/DefaultTypeValidator.cs).

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
}
```

## TODO

- [ ] Document
  - [ ] attributes via a "do it all class"
  - [ ] simple quick start example
  - [ ] integrations - dapper, dapper.crud/extensions(limit+offset), sql mapper
- [ ] Release
  - [ ] enable multi platform targeting
  - [ ] auto build / publish
  - [ ] share/publish
- [ ] fix stricter validation - right side init object is and, or/nor is array
- [ ] fix empty object validation #1

## vNext

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
  - [ ] IOpMapper
  - [ ] Validators
  - [ ] IEnumberable and [] types
- [ ] benchmark tests, run on PRs
- [ ] would be cool to generate part of a swagger documentation from a class spec..
- [ ] js client lib
- [ ] contributing guidelines and issue template
- [ ] remove 3rd party dependency on JSON.NET, use own tree parser. Initial leaf spec: {left, v, right, isField, isOp, fieldSpecProperties...}
- [ ] typed more actionable errors
- [ ] official postgres / mongo support. Starting point: IOpMapper.
  - [ ] consider splitting package into RQL.Core and RQL.MSSQL to allow for RQL.Postgres or RQL.Mongo
