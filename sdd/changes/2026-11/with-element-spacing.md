# No space between `with` and its argument list in a collection expression

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-SPACE-002, FMT-SPACE-028 (new), FMT-BREAK-022 |
| Depends on | [Roslyn 5.12 and C# 15 as the newest supported language version](roslyn-512-csharp-15.md) |

## What

The horizontal-spacing phase gains a rule for the C# 15 `with(…)` element of a collection expression: the whitespace between `with` and the `(` of its argument list is removed (FMT-SPACE-028), and the rule takes its place in FMT-SPACE-002's order next to the `with`-expression rule. A comment in the gap stays where it is, and the decision applies behind it (FMT-SPACE-013, FMT-SPACE-014).

FMT-BREAK-022 states that a `with(…)` element is an element like any other of a wrapped collection expression and starts its own line. This writes down what the line-break phase already does; it changes no output.

## Why

No spacing rule covers the pair `with` `(` of a `with(…)` element: the `with` rule (FMT-SPACE-025) covers the `with` of a `with` expression only. The phase therefore only collapses the gap (FMT-SPACE-002), and `with (comparer)` and `with(comparer)` both stay. The element reads as a call that passes arguments to the collection's construction, and the C# documentation writes it without a space, as the repository writes `new()` (FMT-SPACE-012).

## Visible output changes

**1. The space between `with` and `(` is removed.**

```csharp
// input
HashSet<string> set = [with ( StringComparer.OrdinalIgnoreCase ) , "Hello", "HELLO"];
List<string> e = [with ()];
```

```csharp
// before
HashSet<string> set = [with (StringComparer.OrdinalIgnoreCase), "Hello", "HELLO"];
List<string> e = [with ()];
```

```csharp
// after
HashSet<string> set = [with(StringComparer.OrdinalIgnoreCase), "Hello", "HELLO"];
List<string> e = [with()];
```

**2. With a comment in the gap, the space behind the comment is removed.**

```csharp
// input
List<string> d = [with /* size */ (4)];
```

```csharp
// before
List<string> d = [with /* size */ (4)];
```

```csharp
// after
List<string> d = [with /* size */(4)];
```

**3. The RH5305 code fix removes the space as well.**

The fix formats the collection expression it reports through the formatter, so the `with(…)` element it lays out loses the space of change 1. This is a consequence of change 1, not a decision of its own.

```csharp
// input
List<string> x = [with (4),
    "a"];
```

```csharp
// before
List<string> x = [
                     with (4),
                     "a"
                 ];
```

```csharp
// after
List<string> x = [
                     with(4),
                     "a"
                 ];
```

The layout of a wrapped collection expression that starts with a `with(…)` element does not change (FMT-BREAK-022).

## Decisions

### No space between `with` and `(`

Maintainer answer (change 1): the whitespace between `with` and `(` is removed, also in front of an empty argument list (`with()`), and not kept as the author's gap collapsed to one space (FMT-SPACE-028). The maintainer accepted with this answer that the code fixes which format a `with(…)` element through the formatter write it the same way, such as the RH5305 fix (change 3).

### A comment in the gap is treated as in every other gap

FMT-SPACE-013 and FMT-SPACE-014 decide every rule's gap that holds a comment; the new rule adds no exception, so `with /* size */ (4)` becomes `with /* size */(4)`, as `M( /* four */ a)` becomes `M( /* four */a)` under FMT-SPACE-004.

## Alternatives considered

- **Keep the gap as written, collapsed (today's output)** — not recommended: the output then depends on how the author wrote the element, while the decided direction for keyword and invocation parentheses (thoenissen/Reihitsu#1440) removes that space.
- **One space, as the `with` expression gets (FMT-SPACE-025)** — rejected: `with` there is a binary operator between two operands; the element's `with` takes an argument list, like `new()`.
