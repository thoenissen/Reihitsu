# RH5102 does not report an argument list whose arguments all start on one line

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | RH5102-001 (changed); FMT-BREAK-012 (Why reworded, behavior unchanged) |
| Depends on | [rh5109-parameters-on-one-line.md](rh5109-parameters-on-one-line.md) |

## What

RH5102 decides whether an argument list is multi-line by its arguments alone. A list is multi-line when two of its arguments start on different lines, or when an argument spans several lines; only such a list is reported when two of its arguments start on the same line. A list whose arguments all start on one line, none of them spanning lines, is not reported, also when that line is not the line of the opening parenthesis, also when the closing parenthesis stands on a later line, a comment holding it there included, and also when a comment or a directive keeps the first argument off the line of the opening parenthesis. Until now, a list counted as multi-line when its opening and closing parentheses were on different lines, so a line break behind `(` or in front of `)` was enough to report a list whose arguments share one line, and the fix split it.

The code fix is unchanged: it lays out every list that is still reported as the formatter does (RH5102-003). The rule page is unchanged: it already says that arguments are either all on a single line or each on its own line, and never ties that to the parentheses.

## Why

thoenissen/Reihitsu#1755 found that RH5102 and the formatter read the same list differently, as thoenissen/Reihitsu#1736 found for RH5109. FMT-BREAK-012 counts a list whose elements all start on one line as single-line, also when a line break stands behind its opening parenthesis, and joins it (`M(⏎1, 2);` becomes `M(1, 2);`). RH5102 reported that list and its fix split it into one argument per line, which the formatter keeps, because its arguments then start on different lines. One input therefore had two stable results, depending on whether the code fix or the CLI ran first.

The maintainer decided (2026-11-15, on thoenissen/Reihitsu#1755): option A — RH5102 does not report an argument list whose arguments all stand on one line, even when that line is not the line of the opening parenthesis, as decided for RH5109.

## Visible output changes

**1. A line break behind the opening parenthesis only**

```csharp
// input
class C
{
    void N()
    {
        M(
            1, 2);
    }

    void M(params int[] values)
    {
    }
}
```

```csharp
// before: RH5102 reports the argument list; the fix gives
class C
{
    void N()
    {
        M(1,
          2);
    }

    void M(params int[] values)
    {
    }
}
```

```csharp
// after: not reported by RH5102, no RH5102 fix offered; the input stays as written
class C
{
    void N()
    {
        M(
            1, 2);
    }

    void M(params int[] values)
    {
    }
}
```

RH5101 still reports the first argument below `(` (RH5101-001), and its fix joins the list as the formatter does: `M(1, 2);`.

**2. A line break in front of the closing parenthesis only**

```csharp
// input
class C
{
    void N()
    {
        M(1, 2
          );
    }

    void M(params int[] values)
    {
    }
}
```

```csharp
// before: RH5102 reports the argument list; the fix delegates to the formatter, which keeps
// a line break in front of an argument list's `)` (FMT-BREAK-012), so it gives
class C
{
    void N()
    {
        M(1,
          2
          );
    }

    void M(params int[] values)
    {
    }
}
```

```csharp
// after: not reported, no fix offered; the input stays as written
class C
{
    void N()
    {
        M(1, 2
          );
    }

    void M(params int[] values)
    {
    }
}
```

The *before* output of this entry is derived from RH5102-003 and FMT-BREAK-012, not run; the test implementer confirms it on `main`.

**3. A comment holds the closing parenthesis on a later line**

```csharp
// input
class C
{
    void N()
    {
        M(1, 2 // c
          );
    }

    void M(params int[] values)
    {
    }
}
```

```csharp
// before: RH5102 reports the argument list; no fix is offered (RH5102-004)
```

```csharp
// after: not reported; the input stays as written
class C
{
    void N()
    {
        M(1, 2 // c
          );
    }

    void M(params int[] values)
    {
    }
}
```

**4. A comment or a directive keeps the arguments off the line of the opening parenthesis**

```csharp
// input
class C
{
    void N()
    {
        M( // c
            1, 2);
    }

    void M(params int[] values)
    {
    }
}
```

```csharp
// before: RH5102 reports the argument list; no fix is offered (RH5102-004)
```

```csharp
// after: not reported; the input stays as written
class C
{
    void N()
    {
        M( // c
            1, 2);
    }

    void M(params int[] values)
    {
    }
}
```

The same holds for a directive in that place:

```csharp
// input
class C
{
    void N()
    {
        M(
#pragma warning disable CS0168
            1, 2);
    }

    void M(params int[] values)
    {
    }
}
```

```csharp
// before: RH5102 reports the argument list; no fix is offered (RH5102-004)
```

```csharp
// after: not reported; the input stays as written
class C
{
    void N()
    {
        M(
#pragma warning disable CS0168
            1, 2);
    }

    void M(params int[] values)
    {
    }
}
```

RH5038 still reports the comment inside the argument list, as before. The formatter lays both lists out one argument per line, as before (FMT-BREAK-012).

**5. Unchanged: a list whose arguments start on different lines, or with an argument that spans lines**

```csharp
// input
class C
{
    void N()
    {
        M(
            1, 2,
            3);
        M(1,
          2);
    }

    void M(params int[] values)
    {
    }
}
```

```csharp
// before and after: RH5102 reports the first list (the shape M(1,⏎2) is not reported);
// the fix gives
class C
{
    void N()
    {
        M(1,
          2,
          3);
        M(1,
          2);
    }

    void M(params int[] values)
    {
    }
}
```

A list such as `M(⏎1, () =>⏎{⏎})`, whose arguments start on one line but whose lambda spans several lines, stays reported: an argument that spans lines makes the list multi-line (FMT-BREAK-013).

## Decisions

### Arguments alone decide whether the list is multi-line

The maintainer chose option A of thoenissen/Reihitsu#1755 (2026-11-15): an argument list whose arguments all stand on one line is not reported by RH5102, whichever line that is, as decided for RH5109. Entry 1 is the case the issue asked about.

### Entries 2, 3, and 4 follow the same reading

Entries 2 to 4 are further shapes whose arguments all start on one line; the maintainer's answer covers them by its wording ("whose arguments all stand on one line"), and RH5109 decided the same shapes the same way. For entries 1 and 2 the rule then reads the list as the formatter keeps or produces it: the formatter joins entry 1 and keeps the line break of entry 2. For entry 3 the formatter keeps the `)` behind the comment. For entry 4 the formatter lays the list out one argument per line, and the rule's reading differs from it: the formatter may do more than the rule ([P11](../../README.md#the-formatter-may-do-more-than-the-analyzers-p11)). The comment is reported by RH5038.

### The fix and the rule page are unchanged

The fix still lays out every reported list through the formatter (RH5102-003). The rule page states the rule by its arguments, so it reads the same under the new reading.

### FMT-BREAK-012's reason for entry 4 is reworded

FMT-BREAK-012's Why justified laying out a list with a held first element one element per line partly by RH5102 reporting the other layout. That is no longer so; the Why now gives the held first element as the reason and names P11 for the rules not reporting it. The formatter's behavior is unchanged.

## Alternatives considered

- **Keep reporting a list wrapped only behind `(` and let the fix split it (option B of thoenissen/Reihitsu#1755)** — rejected by the maintainer: the formatter joins such a list (FMT-BREAK-012), so the code fix and the CLI produce two different stable results from one input.
- **Report a list whose first argument a comment or directive holds off the `(` line, as FMT-BREAK-012 lays it out, without a fix** — rejected: the arguments all start on one line, and RH5109 decided the same shape the other way; a comment in the list is RH5038's.
- **Keep reporting a list whose `)` a line break or a comment holds on a later line** — rejected: the formatter keeps that line break in an argument list, so the rule would report a shape the formatter keeps (SHARED-DIAG-010).
