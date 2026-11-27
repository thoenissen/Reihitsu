# Single-letter query range variables and extension-block receivers

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | RH4124-003, RH4122-004 |

## What

RH4124 reports a range variable of a query expression whose name is a single letter — the variable a `from`, `let`, or `join` clause, or the `into` of a `join` clause or a query continuation, declares — as it reports a single-letter lambda parameter. RH4122 reports the receiver parameter of a C# 14 extension block, such as `t` in `extension(string t)`, as it reports the `this` parameter of an extension method. Neither rule has a code fix, and none is added (SHARED-SINGLE, *Non-goals*).

## Why

Both are gaps where an equivalent name is checked today. `values.Select(x => x)` is reported by RH4124, while `from x in values select x`, which the compiler translates into that call, is not. `public static int Square(this int s)` is reported by RH4122, while `extension(int s) { … }`, the other way to declare the same receiver, is not; RH4105 already checks the casing of that receiver. The maintainer decided both cases in question round 3 (2026-10-18), recorded on the issue this change implements.

## Visible output changes

### RH4124 · Single-letter lambda and anonymous method parameter names should not be used — query range variables

```csharp
// input
using System.Linq;

class C
{
    void M(int[] values, int[] others)
    {
        var doubled = from x in values
                      let y = x * 2
                      join o in others on y equals o into g
                      select g.Count() into k
                      select k;
    }
}
```

```csharp
// before: not reported
```

```csharp
// after: x, y, o, g, and k are each reported, no code fix offered
        var doubled = from {|RH4124:x|} in values
                      let {|RH4124:y|} = x * 2
                      join {|RH4124:o|} in others on y equals o into {|RH4124:g|}
                      select g.Count() into {|RH4124:k|}
                      select k;
```

The diagnostic's title and message stay "Single-letter lambda and anonymous method parameter names should not be used" unless the maintainer decides otherwise (see *Decisions*).

### RH4122 · Single-letter parameter names should not be used — extension-block receiver

```csharp
// input
public static class E
{
    extension(string t)
    {
        public bool IsBlank => t.Length == 0;
    }
}
```

```csharp
// before: not reported
```

```csharp
// after: reported on t, no code fix offered
    extension(string {|RH4122:t|})
```

A receiver that names only its type, `extension(string)`, stays unreported.

### Rule pages

- `RH4124.md`, *Description*: "Lambda and anonymous method parameters should use names longer than one letter." becomes "Lambda and anonymous method parameters, and the range variables of a query expression introduced by `from`, `let`, `join`, and `into`, should use names longer than one letter."
- `RH4122.md`, *Description*: "This rule also applies to class and struct primary constructor parameters." becomes "This rule also applies to class and struct primary constructor parameters and to the receiver of an extension block, such as `extension(string text)`."

## Decisions

### Query range variables are checked by RH4124

The maintainer decided that the range variables introduced by `from`, `let`, `join`, and `into` are checked by RH4124 like lambda parameters, not by RH4125 (local variables) or a new rule. A range variable is the parameter of the lambdas the query translates into, so it shares RH4124's configuration.

### The extension-block receiver is checked by RH4122

The maintainer decided that RH4122 reports the receiver of an extension block, as it reports the `this` parameter of an extension method. RH4122's exceptions for overrides, explicit interface implementations, and record primary constructors do not apply to a receiver.

### Every clause that declares a range variable

The requirement names the cause — a query clause that declares a range variable with an identifier of its own — rather than the four keywords of the decision, so the `from` clause that starts a query, a `from` with an explicit type, and both kinds of `into` (behind `join` and in a query continuation behind `select` or `group`) are covered alike. A use of a range variable in a later clause is not a declaration and is not reported.

### The title of RH4124

Open: whether RH4124's title and message keep naming only lambda and anonymous method parameters once range variables are reported. Until the maintainer answers, the texts stay unchanged.

## Alternatives considered

- **Report range variables under RH4125 (local variables)** — rejected by the maintainer's decision: a range variable is not a local of the surrounding method but the parameter of the query's lambdas.
- **A new rule for range variables** — rejected: a range variable names the same value as the lambda parameter RH4124 already checks, and one rule keeps the two forms of one query configured alike.
- **A code fix that renames** — rejected: no single-letter rule offers one, because a meaningful name cannot be derived from a single letter (SHARED-SINGLE, *Non-goals*).
