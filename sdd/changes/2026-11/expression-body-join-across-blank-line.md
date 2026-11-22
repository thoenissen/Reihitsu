# An expression body joins the created `return` across a blank line

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-STRUCT-006 |
| Depends on | [continuation-join.md](continuation-join.md) |

## What

When the formatter converts an expression body to a block (FMT-STRUCT-004, FMT-STRUCT-005), an expression written on a later line than the arrow joins the created `return` keyword also when blank lines stand between the arrow and the expression. FMT-STRUCT-006 said that an expression "written on the line after the arrow" joins, which left the case of a blank line open: `main` keeps the line break behind the created `return`, while for a body converted to an expression statement the blank line already disappears: the expression starts the created statement on the line after `{`, and the blank line behind `{` is removed. This change covers the second question of thoenissen/Reihitsu#1852.

## Why

The line break behind the arrow is layout and has no meaning inside a `return` statement; a blank line there separates nothing. For a body converted to an expression statement the blank line already disappears, so the result should not depend on the statement the conversion creates, nor on a blank line the author left behind the arrow.

## Visible output changes

The formatter (whole-document formatting):

```csharp
class C
{
    int Get() =>

        2;
}
```

Before (`main`):

```csharp
class C
{
    int Get()
    {
        return
        2;
    }
}
```

After:

```csharp
class C
{
    int Get()
    {
        return 2;
    }
}
```

The code fixes that convert an expression body into a block with a `return` statement — RH3202 (methods), RH3205 (operators), RH3206 (conversion operators), and RH3208 (local functions) — give the same result: the RH3202 fix of `Get` above writes `return⏎        2;` on `main` and `return 2;` after the change.

## Decisions

### A blank line does not stop the join (maintainer, answer 2A)

The maintainer chose the join. The line-break phase joins every `return` and its value across line breaks and blank lines (FMT-BREAK-041, [continuation-join.md](continuation-join.md)), so the converted body gives the same result as a `return` written by hand.

## Alternatives considered

- **Keep the line break behind a blank line** — rejected by the maintainer: the result would depend on whether the converted body creates a `return` or an expression statement, and on a blank line that carries no meaning.
