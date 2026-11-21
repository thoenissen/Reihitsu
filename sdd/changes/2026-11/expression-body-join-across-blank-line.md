# An expression body joins the created `return` across a blank line

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | FMT-STRUCT-006 |
| Depends on | [continuation-indent.md](continuation-indent.md) |

## What

When the formatter converts an expression body to a block (FMT-STRUCT-004, FMT-STRUCT-005), an expression written on a later line than the arrow joins the created `return` keyword also when blank lines stand between the arrow and the expression. FMT-STRUCT-006 said that an expression "written on the line after the arrow" joins, which left the case of a blank line open: `main` keeps the line break behind the created `return`, while it already joins the same expression into a created expression statement. This change covers the second question of thoenissen/Reihitsu#1852.

## Why

The line break behind the arrow is layout and has no meaning inside a `return` statement; a blank line there separates nothing. A body converted to an expression statement already joins across it, so the conversion should not depend on the statement it creates, nor on a blank line the author left behind the arrow.

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

Under the other reading — the blank line keeps the line break — the output would be `return` on its own line and `2;` one level deeper (FMT-INDENT-034).

## Decisions

### A blank line does not stop the join

Open for the maintainer (output question of thoenissen/Reihitsu#1852). The spec is written on the recommended reading: the join does not depend on the blank line.

## Alternatives considered

- **Keep the line break behind a blank line** — rejected as the recommendation: the result would depend on whether the converted body creates a `return` or an expression statement, and on a blank line that carries no meaning.
