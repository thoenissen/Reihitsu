# Expression-bodied indexers are single lined

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | new spec: RH5419 (RH5419-001 to RH5419-004); SHARED-DIAG-010 (changed: RH5419 is an exception); non-goal pointers in RH5107, RH5108, RH5109, RH5401, FMT-BREAK-026 (`Why:`), and the SHARED-GETONLY spec |

## What

A new Layout rule, **RH5419 — Expression style get-only indexers should be single lined** (Layout, Warning, enabled, Fix All), reports an expression-bodied indexer whose parameter list spans several lines, or whose `=>` or the first token of whose expression is not on the line of the `]` that closes its parameter list (RH5419-001). The diagnostic covers the whole indexer declaration (RH5419-002). The code fix puts the parameter list on one line and then formats the indexer as the formatter does in place, which joins the arrow and the start of the expression onto the line of the `]` (RH5419-003). It is withheld where a comment, a directive, or disabled text stands inside a parameter list on several lines, where the indexer's code from its first token behind its attribute lists through its `;` stays on several lines — a literal or the expression that the formatter keeps wrapped, or a comment or a directive in front of the `[` that keeps a line break, such as a line comment behind `this` — where the formatter refuses the indexer for a comment, a directive, or disabled text around the `=>`, and in the attribute-list cases where RH5401's fix is withheld for a property (RH5419-004).

RH5401 stays properties-only. The formatter is unchanged: it already joins the arrow of an indexer (FMT-BREAK-026), and it keeps a parameter list on several lines (FMT-BREAK-012). RH5419 reports such a parameter list although the formatter keeps it, which SHARED-DIAG-010 records as an exception; a formatter that joins the parameter list of an expression-bodied indexer is a follow-up.

## Why

The formatter joins the arrow and the start of the expression of an expression-bodied indexer onto the line of its `]` (FMT-BREAK-026), and no rule reports the indexer it rewrites: RH5401 names properties only. An indexer is a different construct from a property, so it gets a rule of its own rather than a widened RH5401 ([P11](../../README.md#the-formatter-may-do-more-than-the-analyzers-p11), a new-rule candidate), and properties and indexers can be configured on their own, as RH3209 and RH3210 split them (thoenissen/Reihitsu#851). Since RH3210 makes the expression-bodied indexer the canonical form of a get-only indexer, an arrow that drops onto the next line is now a shape users meet. The maintainer decided that the rule asks for the whole indexer on one line, its parameter list included.

## Visible output changes

"Before" is today's `main`, where the rule does not exist. Each diagnostic named under "after" is new.

**1. RH5419 — an arrow or expression below the closing bracket is reported, and the fix joins it.**

```csharp
// input
public class Sample
{
    private readonly int[] _values = new int[3];

    public int this[int index]
        => _values[index];

    public int this[long index] =>
        _values[index];
}
```

```csharp
// before: not reported
```

```csharp
// after: RH5419 on both indexer declarations, each from its first token through its `;`; the fix gives
public class Sample
{
    private readonly int[] _values = new int[3];

    public int this[int index] => _values[index];

    public int this[long index] => _values[index];
}
```

**2. RH5419 — a parameter list that spans several lines is reported, also with the arrow on the line of the `]`, and the fix joins it.**

```csharp
// input
class C
{
    int this[short a,
             short b]
        => a;

    int this[
        uint a,
        uint b]
        => 1;

    int this[byte a,
             byte b] => a;

    int this[char a, char b] => a;
}
```

```csharp
// before: not reported
```

```csharp
// after: RH5419 on the first three indexers; the fourth is not reported. The fix gives
class C
{
    int this[short a, short b] => a;

    int this[uint a, uint b] => 1;

    int this[byte a, byte b] => a;

    int this[char a, char b] => a;
}
```

A whole-document formatter run does not clear the diagnostic of a parameter list on several lines; it gives the second indexer as `int this[uint a,` with `uint b] => 1;` aligned on the next line, and keeps the first and third on two lines (FMT-BREAK-012). The formatter's get-only transform (FMT-STRUCT-015) and the RH3210 fix keep a wrapped parameter list as well, so RH5419 reports the indexer they produce from a get-only indexer with such a list, and its fix joins it.

**3. RH5419 — an expression that continues on later lines is not reported when it starts on the line of the `]`; one that does not is reported without a fix.**

```csharp
// input
class C
{
    int this[char a, char b] => a
                                + b;

    int this[int i] =>
        i
        + 1;

    int this[byte a,
             byte b] => Foo(a,
                            b);

    int Foo(byte a, byte b) => a;
}
```

```csharp
// before: not reported
```

```csharp
// after: RH5419 on the second and third indexers, and no code fix is offered; the code stays as written
```

**4. RH5419 — a comment around the arrow: reported, no code fix.**

```csharp
// input
class C
{
    int this[long i] // note
        => 1;

    int this[short i] => // note
        1;
}
```

```csharp
// before: not reported
```

```csharp
// after: RH5419 on both indexers, and no code fix is offered; the code stays as written
```

**5. RH5419 — a comment or a directive inside a parameter list on several lines: reported, no code fix; a block comment inside a parameter list on one line does not withhold the fix.**

```csharp
// input
class C
{
    int this[byte a, // note
             byte b] => a;

    int this[
#if DEBUG
        uint a
#else
        ulong a
#endif
        ] => 1;

    int this[short a /* c */]
        => a;
}
```

```csharp
// before: not reported
```

```csharp
// after: RH5419 on all three indexers; the fix is offered for the third only and gives
class C
{
    int this[byte a, // note
             byte b] => a;

    int this[
#if DEBUG
        uint a
#else
        ulong a
#endif
        ] => 1;

    int this[short a /* c */] => a;
}
```

**6. RH5419 — attribute lists: laid out by the fix, and withheld where RH5401 withholds the fix for a property.**

```csharp
// input
using System;

class C
{
    [Obsolete] int this[int i]
        => i;

    [Obsolete, CLSCompliant(false)]
    int this[long i]
        => 1;

    [Obsolete]
    // note
    int this[byte i]
        => 1;

    [Obsolete,

     CLSCompliant(false)]
    int this[char c]
        => 1;
}
```

```csharp
// before: not reported
```

```csharp
// after: RH5419 on all four indexers; the fix is offered for the first two only and gives
using System;

class C
{
    [Obsolete]
    int this[int i] => i;

    [Obsolete]
    [CLSCompliant(false)]
    int this[long i] => 1;

    [Obsolete]
    // note
    int this[byte i]
        => 1;

    [Obsolete,

     CLSCompliant(false)]
    int this[char c]
        => 1;
}
```

**7. A new rule page, `documentation/rules/RH5419.md`, and a row for RH5419 in the package README.** Its example:

```csharp
// violation
public int this[int index]
    => index * 2;
```

```csharp
// correction
public int this[int index] => index * 2;
```

## Decisions

### A separate rule, not a widened RH5401

RH5401 names properties, and an indexer is a different construct. A rule of its own lets a project configure the two kinds independently, as RH3209 and RH3210 do for the get-only accessor list.

### The rule as specified, with its title (maintainer, batch 1, Q1: A)

The maintainer accepted the reported shapes, the withheld fixes around the arrow and for the attribute lists, the title "Expression style get-only indexers should be single lined", and the rule page's example.

### The whole indexer on one line, parameter list included (maintainer, batch 1, Q2 follow-up: D)

The maintainer wants the parameter list of an expression-bodied indexer on one line, not wrapped. RH5419 therefore also reports a parameter list that spans several lines, also when the arrow already stands on the line of the `]` (RH5419-001), and its fix joins the whole indexer onto one line (RH5419-003). The fix is withheld when a comment, a directive, or disabled text stands inside a parameter list on several lines (RH5419-004): joining the list would move it. The reported span stays the whole indexer (RH5419-002).

### The fix joins the parameter list; the formatter is a follow-up

The maintainer allowed changing the formatter in this run if the fix needs it. The fix does not: removing the line breaks inside the parameter list is a cheap edit of the gaps between its tokens, and formatting the result through the formatter keeps the joined list on one line (FMT-BREAK-012), so the fix's result is what formatting it gives (SHARED-FIX-007). Joining indexer parameter lists in the formatter would change every whole-document run, against the requirement that an indexer's parameter list is laid out as a method's is (FMT-BREAK-011, FMT-BREAK-012), and needs its own decision; it is recorded as a follow-up. Until then RH5419 reports a parameter list the formatter keeps on several lines, an exception to SHARED-DIAG-010 that the spec records, as it records RH5111's.

### The line of the closing bracket is the signature line

The token in front of an indexer's `=>` is always the `]` of its parameter list, and the formatter joins the arrow onto that token's line (FMT-BREAK-026). The rule measures the arrow and the start of the expression against the same line. A line break between `this` and the `[` is not reported: the formatter joins it (FMT-BREAK-011), and RH5108 leaves the bracketed parameter list of an indexer out.

### A block comment inside a parameter list on one line does not withhold the fix

The fix joins nothing inside a parameter list that already stands on one line, so a block comment there stays where it is; only a parameter list on several lines that holds a comment, a directive, or disabled text withholds the fix.

### A comment or a directive in front of the `[` that keeps a line break withholds the fix

The maintainer's answer asks for the whole indexer on one line. When a comment, a directive, or disabled text between `this` and the `[` keeps a line break in front of the `[` — a line comment behind `this`, a comment on a line of its own in front of the `[`, a directive between them — the fix could join the parameter list and the arrow but not the declaration, so it would write a two-line indexer nobody decided. That input is rare, so the fix is withheld for it and the diagnostic stays (RH5419-004): the one-line check measures the indexer from its first token behind its attribute lists through its `;`, the span RH5401's fix measures for a property. A block comment on the line of `this` (`int this /* c */ [`) and a bare line break between `this` and the `[`, which the formatter joins (FMT-BREAK-011), still get the fix, because the result is one line.

```csharp
// input
class C
{
    int this // note
        [sbyte a,
         sbyte b] => a;
}
```

```csharp
// after: RH5419 on the indexer, and no code fix is offered; the code stays as written
```

### The attribute-list guards are RH5401's

The attribute lists of an indexer are those of a member, as for a property. RH5419-004 refers to RH5401-004 for them instead of restating it, so the two fixes cannot drift apart.

## Alternatives considered

- **Widen RH5401 to indexers** — rejected: the issue asks for separate configuration, and P11 gives a different construct a rule of its own.
- **Keep a parameter list on several lines and offer the fix as the formatter lays it out** (batch 1, Q2: A) — rejected by the maintainer: the indexer belongs on one line.
- **Withhold the fix wherever the indexer's own code stays on several lines, as for a property** (batch 1, Q2: B; the issue's wording "offered when the whole declaration fits on one line") — rejected by the maintainer in favour of joining the parameter list.
- **Report a parameter list on several lines only when the arrow is not on the line of its `]`** (batch 1, Q2 follow-up: C) — rejected by the maintainer.
- **Join the parameter list of an expression-bodied indexer in the formatter in this run** — deferred: the fix does not need it (see *Decisions*), and it changes every whole-document run; a follow-up.
- **Offer the fix when a comment or a directive in front of the `[` keeps a line break, writing the indexer on two lines** (`int this // note` above `[sbyte a, sbyte b] => a;`) — rejected: the maintainer asked for the indexer on one line, and the input is rare enough to leave to the author.
- **Move RH5401-003 and RH5401-004 into a shared spec for both rules** — rejected for this change: it would retire two RH5401 requirements and re-tag their tests without changing any behavior; referencing RH5401's requirements by ID keeps one statement of the rule.
