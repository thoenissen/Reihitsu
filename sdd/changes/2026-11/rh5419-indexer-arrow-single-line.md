# Expression-bodied indexers keep their arrow on the line of the closing bracket

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | new spec: RH5419 (RH5419-001 to RH5419-004) |

## What

A new Layout rule, **RH5419 — Expression style get-only indexers should be single lined** (Layout, Warning, enabled, Fix All), reports an expression-bodied indexer whose `=>` or the first token of whose expression is not on the line of the `]` that closes its parameter list (RH5419-001). The diagnostic covers the whole indexer declaration (RH5419-002). The code fix formats the indexer as the formatter does in place, which joins the arrow and the start of the expression onto the line of the `]` (RH5419-003). It is withheld where the expression stays on several lines, where the formatter refuses the indexer for a comment, a directive, or disabled text around the `=>`, and in the attribute-list cases where RH5401's fix is withheld for a property (RH5419-004). A parameter list that stays on several lines does not withhold the fix.

RH5401 stays properties-only. The formatter is unchanged: it already joins the arrow of an indexer (FMT-BREAK-026).

## Why

The formatter joins the arrow and the start of the expression of an expression-bodied indexer onto the line of its `]` (FMT-BREAK-026), and no rule reports the indexer it rewrites: RH5401 names properties only. An indexer is a different construct from a property, so it gets a rule of its own rather than a widened RH5401 ([P11](../../README.md#the-formatter-may-do-more-than-the-analyzers-p11), a new-rule candidate), and properties and indexers can be configured on their own, as RH3209 and RH3210 split them (thoenissen/Reihitsu#851). Since RH3210 makes the expression-bodied indexer the canonical form of a get-only indexer, an arrow that drops onto the next line is now a shape users meet.

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

**2. RH5419 — a parameter list that spans several lines: the arrow belongs on the line of the `]`, and the fix is offered.**

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
}
```

```csharp
// before: not reported
```

```csharp
// after: RH5419 on the first two indexers; the third is not reported. The fix gives
class C
{
    int this[short a,
             short b] => a;

    int this[uint a,
             uint b] => 1;

    int this[byte a,
             byte b] => a;
}
```

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
}
```

```csharp
// before: not reported
```

```csharp
// after: RH5419 on the second indexer only, and no code fix is offered; the code stays as written
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

**5. RH5419 — attribute lists: laid out by the fix, and withheld where RH5401 withholds the fix for a property.**

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

**6. A new rule page, `documentation/rules/RH5419.md`, and a row for RH5419 in the package README.** Its example:

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

### The line of the closing bracket is the signature line

The token in front of an indexer's `=>` is always the `]` of its parameter list, and the formatter joins the arrow onto that token's line (FMT-BREAK-026). The rule measures against the same line, so a formatter run clears every diagnostic, also for a parameter list that the formatter keeps on several lines.

### The fix judges the expression, not the parameter list

RH5401's fix is withheld where the property's own code stays on several lines, because a wrapped expression would end up in a shape the author did not choose. A property's signature practically never wraps, so its own code and its expression coincide there. An indexer's parameter list wraps on its own, and the fix keeps it as the formatter keeps it; so RH5419's fix is withheld only where the code from the `=>` through the `;` stays on several lines.

### The attribute-list guards are RH5401's

The attribute lists of an indexer are those of a member, as for a property. RH5419-004 refers to RH5401-004 for them instead of restating it, so the two fixes cannot drift apart.

## Alternatives considered

- **Widen RH5401 to indexers** — rejected: the issue asks for separate configuration, and P11 gives a different construct a rule of its own.
- **Withhold the fix wherever the indexer's own code stays on several lines, as for a property** — the issue's wording ("offered when the whole declaration fits on one line"). It would withhold the fix for every indexer with a wrapped parameter list, although the formatter clears the diagnostic there and keeps the parameter list as written. Left to the maintainer as an output question.
- **Move RH5401-003 and RH5401-004 into a shared spec for both rules** — rejected for this change: it would retire two RH5401 requirements and re-tag their tests without changing any behavior; referencing RH5401's requirements by ID keeps one statement of the rule.
