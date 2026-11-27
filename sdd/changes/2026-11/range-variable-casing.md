# RH4105 checks the casing of query range variables

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | RH4105-002, RH4105-005 (added), RH4105-006 (added) |

## What

RH4105 reports a range variable of a query expression whose name is not in camelCase, like a lambda parameter (thoenissen/Reihitsu#1978). A range variable is the variable that a `from`, `let`, or `join` clause, or the `into` of a `join` clause or of a query continuation, declares with an identifier of its own. A range variable named `_` is not reported.

The rule's title and message widen from *Method parameter names should be in camelCase* to *Parameter and query range variable names should be in camelCase*.

The rule's code fix renames a reported range variable and every reference to it, as it renames a parameter. It is withheld where a reference to the range variable gives an anonymous object member its name, such as `select new { Value }`: the rename would rename that member as well.

## Why

A range variable names the same value that a lambda parameter names in method syntax: `from value in values select value` is `values.Select(value => value)`. RH4105 reported the lambda parameter but not the range variable, and no other casing rule checked it. RH4124 already checks range variables under the parameter rules for single-letter names (RH4124-003). The maintainer chose this option on 2026-11-27 (thoenissen/Reihitsu#1978, option A), with the fix offered only if it is comprehensive and safe for range variables.

The fix is comprehensive: the Roslyn renamer renames a range variable declared by each of the five clauses and every reference to it, also across a query continuation, and the result compiles. Running it on

```csharp
var a = from Value in values
        let Doubled = Value * 2
        join Row in rows on Value equals Row.Id into Matches
        select new { Value, Doubled, Count = Matches.Count() } into Projected
        where Projected.Count > 0
        select Projected;
```

renamed all five range variables and compiled. It also renamed the members `Value` and `Doubled` of the anonymous object to `value` and `doubled`, because the compiler names a member without a name of its own after the expression's identifier; that changes the text `ToString()` returns and the names a serializer or reflection sees, which SHARED-FIX-003 forbids. Projecting range variables into an anonymous object is a common shape of query syntax, so the fix is withheld for it (RH4105-006), by a cheap check of the query's own anonymous object members.

## Visible output changes

### 1. RH4105 · Parameter and query range variable names should be in camelCase — range variables

```csharp
using System.Linq;

class C
{
    int[] M(int[] values, int[] others)
    {
        return (from Value in values
                let Doubled = Value * 2
                join int Other in others on Doubled equals Other into Matches
                select Matches.Count() into Count
                select Count).ToArray();
    }
}
```

Before: not reported.

After: RH4105 is reported on `Value`, `Doubled`, `Other`, `Matches`, and `Count`, and the fix gives:

```csharp
using System.Linq;

class C
{
    int[] M(int[] values, int[] others)
    {
        return (from value in values
                let doubled = value * 2
                join int other in others on doubled equals other into matches
                select matches.Count() into count
                select count).ToArray();
    }
}
```

### 2. RH4105 — a range variable that names an anonymous object member

```csharp
using System.Linq;

class C
{
    object M(int[] values)
    {
        return (from Value in values
                select new { Value }).First();
    }
}
```

Before: not reported.

After: RH4105 is reported on `Value`; no code fix is offered.

### 3. RH4105 — a range variable named `_`

```csharp
using System.Linq;

class C
{
    int M(int[] values)
    {
        return (from _ in values
                select 1).Count();
    }
}
```

Before: not reported. After: not reported.

### 4. Title and message

Before: *Method parameter names should be in camelCase*, as the title and as the message of every RH4105 diagnostic.

After: *Parameter and query range variable names should be in camelCase*.

### 5. Rule page

`documentation/rules/RH4105.md` takes the new title and says that the rule covers the range variables of query expressions.

## Decisions

### Range variables are checked by RH4105

Maintainer, 2026-11-27 (thoenissen/Reihitsu#1978, option A). The clauses are the ones RH4124-003 names for single-letter range variables, so both rules check the same declarations.

### The title and message widen

Maintainer, 2026-11-27: the title names both kinds of declaration the rule checks. The rule keeps its ID, so existing suppressions keep working; the message keeps no placeholder and stays equal to the title.

### The fix is offered, and withheld for an anonymous object member named after the range variable

The maintainer allowed the fix only if it is comprehensive and safe for range variables. It is comprehensive (see *Why*), and the one unsafe result found — a renamed anonymous object member — is withheld under SHARED-FIX-003. The check is on the query expression that declares the range variable, since a range variable cannot be referenced outside it.

## Alternatives considered

- **No fix for range variables** — rejected: the rename is comprehensive, and the unsafe case is cheap to recognize and withhold.
- **A rule of its own for range variables** — rejected by the maintainer's choice of option A: a range variable plays the role of a lambda parameter, and RH4124 groups it with lambda parameters already.
- **Leave range variables unchecked and document the gap** (option B of thoenissen/Reihitsu#1978) — rejected by the maintainer.
