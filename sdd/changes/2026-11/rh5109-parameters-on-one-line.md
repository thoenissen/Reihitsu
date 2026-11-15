# RH5109 does not report a parameter list whose parameters all start on one line

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | RH5109-001 (changed), RH5109-003 (example changed) |

## What

RH5109 decides whether a parameter list is multi-line by its parameters alone. A list is multi-line when two of its parameters start on different lines, or when a parameter spans several lines; only such a list is reported when two of its parameters start on the same line. A list whose parameters all start on one line, none of them spanning lines, is not reported, also when that line is not the line of the opening parenthesis, also when the closing parenthesis stands on a later line, a comment holding it there included, and also when a comment or a directive keeps the first parameter off the line of the opening parenthesis. Until now, a list counted as multi-line when its opening and closing parentheses were on different lines, so a line break behind `(` or in front of `)` was enough to report a list whose parameters share one line, and the fix split it.

The rule page's description says that the parameters, not the list's parentheses, decide whether the list spans several lines.

## Why

thoenissen/Reihitsu#1736 found that RH5109 and the formatter read the same list differently. FMT-BREAK-012 counts a list whose elements all start on one line as single-line, also when a line break stands behind its opening parenthesis, and joins it (`void N(⏎int a, int b)` becomes `void N(int a, int b)`). RH5109 reported that list and its fix split it into one parameter per line (`void N(int a,⏎       int b)`), which the formatter keeps, because its parameters then start on different lines. One input therefore had two stable results, depending on whether the code fix or the CLI ran first.

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

**3. A comment holds the closing parenthesis on a later line**

```csharp
// input
class C
{
    void N(int a, int b // c
          )
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
    void N(int a, int b // c
          )
    {
    }
}
```

**4. A comment or a directive keeps the parameters off the line of the opening parenthesis**

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

The same holds for a directive in that place:

```csharp
// input
class C
{
    void N(
#pragma warning disable CS0168
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
    void N(
#pragma warning disable CS0168
        int a, int b)
    {
    }
}
```

RH5035 still reports the comment inside the parameter list, as before. The formatter lays both lists out one parameter per line, as before.

**5. Unchanged: a list whose parameters start on different lines**

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

**6. Rule page `documentation/rules/RH5109.md`, Description**

Before:

> Once the list spans multiple lines — including when a single parameter's type wraps across lines — every parameter must start on its own line.

After:

> Once the parameters span multiple lines — when they start on different lines, or when a single parameter's type wraps across lines — every parameter must start on its own line. A line break directly after the opening parenthesis or before the closing parenthesis alone does not make the list multi-line.

## Decisions

### Parameters alone decide whether the list is multi-line

The maintainer chose option A of thoenissen/Reihitsu#1736 (triage, 2026-11-15): a parameter list whose parameters all start on one line is not reported by RH5109, whichever line that is; whether such a layout should be reported belongs to another rule.

### Entries 1, 2, 3 and 6: wrapped only around the parameters

The maintainer chose A (2026-11-15): `void N(⏎int a, int b)`, `void N(int a, int b⏎)`, and the same list with its `)` held on a later line by a comment are not reported, and the rule page's Description changes as entry 6 shows. For entries 1 and 2 the rule then reads the list as FMT-BREAK-012 reads it by its elements, so the rule and the formatter agree. For entry 3 the formatter keeps the `)` behind the comment, and the rule no longer reports what the formatter keeps.

### Entry 4: a comment or directive keeps the first parameter off the `(` line

The maintainer chose A (2026-11-15): a list whose first parameter a comment or a directive keeps off the line of `(`, and whose parameters all start on one line, is not reported. Here the rule's reading differs from FMT-BREAK-012, which lays such a list out one parameter per line: the formatter may do more than the rule ([P11](../../README.md#the-formatter-may-do-more-than-the-analyzers-p11)). The comment is reported by RH5035.

### The fix is unchanged

The fix still rebuilds every reported list with the first parameter behind `(` and one parameter per line (RH5109-003). Its example for a list wrapped only behind `(` is replaced by a list whose parameters start on two lines below `(`, which is still reported.

## Alternatives considered

- **Keep reporting a list wrapped only behind `(` and let the fix split it (option B of thoenissen/Reihitsu#1736)** — rejected by the maintainer: the formatter joins such a list (FMT-BREAK-012), so the code fix and the CLI produce two different stable results from one input.
- **Report a list whose first parameter a comment or directive holds off the `(` line, as FMT-BREAK-012 lays it out, without a fix** — rejected by the maintainer (entry 4): the parameters all start on one line, and whether such a layout is reported belongs to another rule.
- **Keep reporting a list whose `)` a comment holds on a later line, without a fix** — rejected by the maintainer (entry 3): the formatter keeps that shape, and the parameters all start on one line.
