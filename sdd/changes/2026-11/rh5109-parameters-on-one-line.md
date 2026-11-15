# RH5109 does not report a parameter list whose parameters all start on one line

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | RH5109-001 (changed), RH5109-003 (example changed) |

## What

RH5109 decides whether a parameter list is multi-line by its parameters alone. A list is multi-line when two of its parameters start on different lines, or when a parameter spans several lines; only such a list is reported when two of its parameters start on the same line. A list whose parameters all start on one line, none of them spanning lines, is not reported, also when that line is not the line of the opening parenthesis, and also when the closing parenthesis stands on a later line. Until now, a list counted as multi-line when its opening and closing parentheses were on different lines, so a line break behind `(` or in front of `)` was enough to report a list whose parameters share one line, and the fix split it.

The rule page's description says that the parameters, not the list's parentheses, decide whether the list spans several lines.

## Why

thoenissen/Reihitsu#1736 found that RH5109 and the formatter read the same list differently. FMT-BREAK-012 counts a list whose elements all start on one line as single-line, also when a line break stands behind its opening parenthesis, and joins it (`void N(⏎int a, int b)` becomes `void N(int a, int b)`). RH5109 reported that list and its fix split it into one parameter per line, a result the formatter joins again, so the code fix and the CLI disagreed about the same code.

The maintainer decided (2026-11-15, triage of thoenissen/Reihitsu#1736): RH5109 does not report a parameter list whose parameters all stand on one line, even when that line is not the line of the opening parenthesis. Whether such a layout should be reported at all belongs to a different rule, not to RH5109.

## Visible output changes

**1. A line break behind the opening parenthesis only**

```csharp
// input
class C
{
    void N(
        int a, int b)
    {
    }
}
```

```csharp
// before: RH5109 reports the list on its `(`; the fix gives
class C
{
    void N(int a,
           int b)
    {
    }
}
```

```csharp
// after: not reported, no fix offered; the input stays as written
class C
{
    void N(
        int a, int b)
    {
    }
}
```

**2. A line break in front of the closing parenthesis only**

```csharp
// input
class C
{
    void N(int a, int b
          )
    {
    }
}
```

```csharp
// before: RH5109 reports the list on its `(`; the fix gives
class C
{
    void N(int a,
           int b)
    {
    }
}
```

```csharp
// after: not reported, no fix offered; the input stays as written
class C
{
    void N(int a, int b
          )
    {
    }
}
```

**3. A comment keeps the parameters off the line of the opening parenthesis**

```csharp
// input
class C
{
    void N( // c
        int a, int b)
    {
    }
}
```

```csharp
// before: RH5109 reports the list on its `(`; no fix is offered (RH5109-004)
```

```csharp
// after: not reported; the input stays as written
class C
{
    void N( // c
        int a, int b)
    {
    }
}
```

RH5035 still reports the comment inside the parameter list, as before.

**4. Unchanged: a list whose parameters start on different lines**

```csharp
// input
class C
{
    void N(
        int a, int b,
        int c)
    {
    }
}
```

```csharp
// before and after: RH5109 reports the list on its `(`; the fix gives
class C
{
    void N(int a,
           int b,
           int c)
    {
    }
}
```

**5. Rule page `documentation/rules/RH5109.md`, Description**

Before:

> Once the list spans multiple lines — including when a single parameter's type wraps across lines — every parameter must start on its own line.

After:

> Once the parameters span multiple lines — when they start on different lines, or when a single parameter's type wraps across lines — every parameter must start on its own line. A line break directly after the opening parenthesis or before the closing parenthesis alone does not make the list multi-line.

## Decisions

### Parameters alone decide whether the list is multi-line

The maintainer chose option A of thoenissen/Reihitsu#1736 (2026-11-15): a parameter list whose parameters all start on one line is not reported by RH5109, whichever line that is. The rule reads a list as multi-line exactly when FMT-BREAK-012 does by its elements, so for the shapes of entries 1 and 2 the rule and the formatter agree. A list whose first parameter a comment or a directive keeps off the line of `(` (entry 3) is single-line for RH5109 under the same answer — its parameters all start on one line — while FMT-BREAK-012 lays it out one parameter per line; the formatter may do more than the rule ([P11](../../README.md#the-formatter-may-do-more-than-the-analyzers-p11)), and the comment is reported by RH5035.

### The fix is unchanged

The fix still rebuilds every reported list with the first parameter behind `(` and one parameter per line (RH5109-003). Its example for a list wrapped only behind `(` is replaced by a list whose parameters start on two lines below `(`, which is still reported.

## Alternatives considered

- **Keep reporting a list wrapped only behind `(` and let the fix split it (option B of thoenissen/Reihitsu#1736)** — rejected by the maintainer: the formatter joins such a list (FMT-BREAK-012), so the fix result is not a fixed point of the formatter, and the decided reading of FMT-BREAK-012 is that the elements alone make a list multi-line.
- **Report a list whose first parameter a comment or directive holds off the `(` line, as FMT-BREAK-012 lays it out** — not taken: the maintainer's answer covers every list whose parameters all stand on one line, and whether such a layout is reported belongs to another rule.
