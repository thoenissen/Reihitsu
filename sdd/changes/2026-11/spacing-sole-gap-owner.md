# The spacing phase is the only owner of same-line gaps

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | FMT-SPACE-002 (changed), FMT-SPACE-012 (changed), FMT-SPACE-016 to FMT-SPACE-023 (added), the fourth non-goal of `horizontal-spacing.md` (narrowed), FMT-BREAK-026 (changed), FMT-BREAK-031 (added), FMT-STRUCT-026 (changed) |
| Depends on | [Every formatter phase returns a tree that round-trips and keeps foreign annotations](../2026-10/phase-tree-round-trip.md) |

## What

The spacing phase decides the whitespace of every gap between two tokens on one line. The line-break phase only adds and removes line breaks (FMT-BREAK-031):

- **A join writes the minimal separator.** Where the line-break phase joins two tokens onto one line, it leaves no whitespace between them, unless the two tokens written without whitespace would not read back as the same two tokens — one token (`protected` and `internal`, `out` and `var`), another token pair (`-` and `-b`), or the start of a comment (`/` and `*p`). There it writes one space. Whether two tokens read back as written is answered by the shared lexer-based helper that FMT-STRUCT-026 already uses. Which separator a join writes no longer depends on the kind of the tokens it joins.
- **The line-break phase writes no other whitespace into a gap that stays on one line.** Today five places of the phase write spaces of their own — the assignment rewriter in front of a joined value, the conditional rewriter behind `?` and `:`, the `case … when` rewriter in front of `when`, the property-layout rewriter on both sides of the arrow of an expression-bodied property or indexer (also when nothing is joined: `int P =>1;` becomes `int P => 1;`), and the accessor-list layout inside a one-line accessor list (also when nothing is joined: `int P {get;set;}` becomes `int P { get; set; }`) — and the list-element join chooses one space or none by token kind. All of them go.
- **The spacing phase gets the rules that keep today's output where the join or the line-break phase spaced a pair.** Each rule is keyed by the construct, applies to every gap on one line, joined or written so, and is decided by the maintainer as a visible output change (questions of the run, see `Decisions`):
  - every attribute-target colon: no space before it, one space after it (FMT-SPACE-016, replacing the `return:` exception of FMT-SPACE-012);
  - every arrow `=>` — lambda, expression body of a member or accessor, switch-expression arm: one space on each side (FMT-SPACE-017);
  - the `?` and `:` of a conditional expression: one space on each side (FMT-SPACE-018);
  - the colon of a name or expression colon — named argument, tuple element, subpattern: no space before it, one space after it (FMT-SPACE-019);
  - the `when` of a `case` guard or switch-expression arm: one space in front of it (FMT-SPACE-020);
  - the type of a parameter, tuple element, variable declaration, declaration expression, or declaration pattern and the name behind it: one space (FMT-SPACE-021);
  - an accessor list on one line: one space behind its `{` and between two accessors (FMT-SPACE-022);
  - the operator of a relational pattern and its operand: one space (FMT-SPACE-023).
- **Arrows created by a structural transform are spaced by the spacing phase.** FMT-STRUCT-026 kept one space on each side of the `=>` the accessor and get-only-member transforms create until a spacing rule owns that gap. FMT-SPACE-017 owns it now, so the transforms write no whitespace there; the final output does not change.

The pairs that no rule covers keep the gap the spacing phase finds and only have their runs of whitespace collapsed (FMT-SPACE-002). For a joined pair, that gap is the minimal separator: a pair that reads back as written is joined without a space (visible output change 9).

## Why

The same pair was spaced differently depending on whether the author wrote it on one line or the line-break phase joined it, and the line-break phase spaced some constructs on its own even when it joined nothing:

- `Foo(name:"x")` keeps its zero gap, while `Foo(name:` followed by `"x")` on the next line becomes `Foo(name: "x")`.
- `int P =>1;` becomes `int P => 1;`, while the lambda `y =>y` and the switch arm `1 =>1` stay as written; FMT-PIPE-014 requires every arrow to be spaced alike.
- `[return :Obsolete]` becomes `[return:Obsolete]`, because `return` is a keyword of FMT-SPACE-012, while `[field :NonSerialized]` keeps its space; a third policy writes the colon tight where the list-element join joins it. FMT-PIPE-014 requires every attribute-target colon to be spaced alike.

A spacing decision in the line-break phase is a second owner of a decision FMT-SPACE makes: the two can disagree, and whichever runs last wins only where it has a rule. The join also wrote zero gaps where the two tokens read as one, so the phase's own text was another program — `protected` and `internal` on two lines of an auto-property's accessor list become `protectedinternal set;` in the final output today, which does not compile, and `a -` followed by `-b` becomes `a⏎--b` in the phase's output, which the spacing phase repairs only because `-` has a rule (FMT-PIPE-021).

The minimal separator follows the phase-boundary contract of FMT-STRUCT-026: a phase writes only what its text needs to parse back to its tree, and the phase that owns a layout decision writes it. A space as the neutral separator would need new rules for every pair the join keeps tight today — the parenthesis of every parameter list (`void M (int a)`), every invocation inside a chain (`items.Where (x => x)`), a cast operand, a range, a pointer type — and would change the examples of FMT-BREAK-011 and FMT-BREAK-020. The minimal separator keeps all of those, and the pairs it would space differently from today are the ones the rules above decide, most of which the maintainer already decided for the formatter (thoenissen/Reihitsu#958, thoenissen/Reihitsu#1440, thoenissen/Reihitsu#1514, thoenissen/Reihitsu#1383).

## Visible output changes

Every entry shows the output of the recommended options. Each is an output question of the run; the alternatives and their outputs are in the run's questions and, once answered, in `Decisions`.

### 1. Every attribute-target colon: no space before, one space after (FMT-SPACE-016)

```csharp
using System;

[assembly:CLSCompliant(true)]

class C
{
    [field :NonSerialized]
    int _f;

    [return:Obsolete]
    int M()
    {
        return 1;
    }

    [method : Obsolete]
    void N([param :Obsolete] int a)
    {
    }
}
```

Before:

```csharp
using System;

[assembly:CLSCompliant(true)]

class C
{
    [field :NonSerialized]
    int _f;

    [return:Obsolete]
    int M()
    {
        return 1;
    }

    [method : Obsolete]
    void N([param :Obsolete] int a)
    {
    }
}
```

After:

```csharp
using System;

[assembly: CLSCompliant(true)]

class C
{
    [field: NonSerialized]
    int _f;

    [return: Obsolete]
    int M()
    {
        return 1;
    }

    [method: Obsolete]
    void N([param: Obsolete] int a)
    {
    }
}
```

`[return :Obsolete]` and `[return  :  Obsolete]` give `[return: Obsolete]` too; before, they gave `[return:Obsolete]` and `[return: Obsolete]`.

### 2. Every arrow: one space on each side (FMT-SPACE-017)

```csharp
using System;

class C
{
    int _p;

    int A
    {
        get =>_p;
        set=>_p = value;
    }

    int P =>1;

    void M(int x)
    {
        Func<int, int> f = y=>y;
        Func<int, int> g = (int y)=>y;
        var s = x switch
                {
                    1=>2,
                    _ =>3
                };
    }
}
```

Before: unchanged, except `int P =>1;`, which became `int P => 1;`.

After:

```csharp
using System;

class C
{
    int _p;

    int A
    {
        get => _p;
        set => _p = value;
    }

    int P => 1;

    void M(int x)
    {
        Func<int, int> f = y => y;
        Func<int, int> g = (int y) => y;
        var s = x switch
                {
                    1 => 2,
                    _ => 3
                };
    }
}
```

The hand-written accessor arrow of FMT-STRUCT-026's elastic-trivia example, `get=>"";`, becomes `get => "";`.

### 3. The conditional operator: one space on each side of `?` and `:` (FMT-SPACE-018)

```csharp
class C
{
    int M(bool b)
    {
        var r = b?1:2;

        return b ? 1:2;
    }
}
```

Before: unchanged.

After:

```csharp
class C
{
    int M(bool b)
    {
        var r = b ? 1 : 2;

        return b ? 1 : 2;
    }
}
```

### 4. Name colons: no space before, one space after (FMT-SPACE-019)

```csharp
class C
{
    void M(bool b, int c, object o)
    {
        M(b:b, c :1, o: o);

        var t = (A:1, B :2);
        var p = o is string { Length:1 };
    }
}
```

Before: unchanged.

After:

```csharp
class C
{
    void M(bool b, int c, object o)
    {
        M(b: b, c: 1, o: o);

        var t = (A: 1, B: 2);
        var p = o is string { Length: 1 };
    }
}
```

### 5. One space in front of `when` (FMT-SPACE-020)

```csharp
class C
{
    void M(object o, bool b)
    {
        switch (o)
        {
            case (1, 2)when b:
                break;
        }
    }
}
```

Before: unchanged.

After:

```csharp
class C
{
    void M(object o, bool b)
    {
        switch (o)
        {
            case (1, 2) when b:
                break;
        }
    }
}
```

The space behind `when` is not decided: `case int i when(i > 0):` stays as written.

### 6. One space between a type and the name it declares (FMT-SPACE-021)

```csharp
class C
{
    void M(int?a, int[]b, (int, int)c)
    {
        int?z = null;

        N(out int?w);
    }
}
```

Before: unchanged.

After:

```csharp
class C
{
    void M(int? a, int[] b, (int, int) c)
    {
        int? z = null;

        N(out int? w);
    }
}
```

### 7. An accessor list on one line: one space behind `{` and between accessors (FMT-SPACE-022)

```csharp
using System;

class C
{
    int P {get;set;}

    int Q {[Obsolete] get;set;}
}
```

Before:

```csharp
using System;

class C
{
    int P { get; set; }

    int Q {[Obsolete] get;set; }
}
```

After:

```csharp
using System;

class C
{
    int P { get; set; }

    int Q { [Obsolete] get; set; }
}
```

Only the one-line list with an attributed accessor changes; FMT-BREAK-024 leaves it on its line, and the accessor-list layout spaced only the lists it collapses.

### 8. One space behind the operator of a relational pattern (FMT-SPACE-023)

```csharp
class C
{
    bool M(int x)
    {
        return x is >5 and <10;
    }
}
```

Before: unchanged.

After:

```csharp
class C
{
    bool M(int x)
    {
        return x is > 5 and < 10;
    }
}
```

### 9. A joined pair that no rule covers keeps the minimal separator (FMT-BREAK-031)

A wrap inside a list element (FMT-BREAK-015) between a keyword and a parenthesis or bracket is joined without a space, because the two tokens read back as written:

```csharp
class C
{
    unsafe void M(int a, object o)
    {
        N(static
          (int value) => value);
        N(int
          (x) => x);
        N(in
          (a));
        N(o is var
          (x, y));
        N(stackalloc
          [] { 1 });
    }
}
```

Before:

```csharp
class C
{
    unsafe void M(int a, object o)
    {
        N(static (int value) => value);
        N(int (x) => x);
        N(in (a));
        N(o is var (x, y));
        N(stackalloc [] { 1 });
    }
}
```

After:

```csharp
class C
{
    unsafe void M(int a, object o)
    {
        N(static(int value) => value);
        N(int(x) => x);
        N(in(a));
        N(o is var(x, y));
        N(stackalloc[] { 1 });
    }
}
```

The same pairs written on one line keep the author's gap, as before.

### 10. An accessor modifier pair on two lines compiles

```csharp
class C
{
    public int P
    {
        get;
        protected
        internal set;
    }
}
```

Before:

```csharp
class C
{
    public int P { get; protectedinternal set; }
}
```

After:

```csharp
class C
{
    public int P { get; protected internal set; }
}
```

Today's output does not compile (FMT-PIPE-003); the minimal separator writes the space the two keywords need.

### Unchanged

Every other final output of the formatter, the analyzers, the code fixes, and the CLI stays as it is, the examples of FMT-BREAK-011, FMT-BREAK-015, FMT-BREAK-016, FMT-BREAK-018, FMT-BREAK-020, FMT-BREAK-024, FMT-BREAK-026, and FMT-BREAK-028 included: the joins that were tight stay tight under the minimal separator, and the pairs the joins and the line-break phase spaced are spaced by the rules above. No rule page changes; no analyzer reports any of the pairs above.

## Decisions

### The line-break phase joins with the minimal separator

*Pending the maintainer's answer.* Recommended over one space as the neutral separator; see `Why` and `Alternatives considered`.

### Every arrow gets one space on each side now

*Pending the maintainer's answer.* Recommended over keeping the author's spacing until thoenissen/Reihitsu#958 is implemented: the maintainer already decided this output for every arrow (thoenissen/Reihitsu#958 for lambdas and switch-expression arms, thoenissen/Reihitsu#1514 for expression bodies), keeping the author's spacing would change `int P => 1;` to `int P =>1;` for one release, and with the minimal separator a joined arrow would read `x =>x` and `int T =>5;`, against the examples of FMT-BREAK-015 and FMT-BREAK-026.

### Every attribute-target colon gets one space behind it

*Pending the maintainer's answer.* Recommended over keeping the author's spacing behind it: thoenissen/Reihitsu#1383 records the output `[return: Obsolete]`, `[assembly: X]`, `[field: X]` "whatever the target and input spacing", and FMT-SPACE-012's and FMT-BREAK-027's examples already show that form.

### The rules are keyed by construct and apply to every gap on one line

The spacing phase cannot tell a joined gap from one the author wrote, and it must not: a rule that applied only to joined gaps would keep the difference this change removes. Each new rule therefore also changes one-line input (entries 1 to 8).

### Structural transforms create the arrow without spaces

FMT-SPACE-017 owns the gap, which FMT-STRUCT-026 named as the condition for dropping the transforms' spaces around a created `=>` (thoenissen/Reihitsu#1514). The created tokens still carry no elastic trivia.

## Alternatives considered

- **One space as the neutral separator** (the issue's first reading) — rejected as recommended: every joined pair that is tight today and has no rule would get a space, among them the parenthesis of a parameter list (`void M (int a)`, against FMT-BREAK-011's example), an invocation in a chain (`items.Where (x => x)`, against FMT-BREAK-020's example), a cast operand (`(int) value`), a range (`start ..end`), and a function-pointer type (`delegate * unmanaged [Cdecl]`); rules for those pairs are not decided anywhere.
- **Keep today's separator table and move it into the spacing phase, applied only to joined gaps** — rejected: the line-break phase would have to mark its joins for the spacing phase, and the same pair would stay spaced differently when joined and when written on one line, which is the defect this change removes.
- **Keep the author's spacing around every arrow until thoenissen/Reihitsu#958 is implemented** (the issue's proposal) — rejected as recommended; see `Decisions`.
- **No space behind an attribute-target colon unless the author wrote one** (the issue's proposal: `[field :NonSerialized]` becomes `[field:NonSerialized]`) — rejected as recommended; see `Decisions`.
- **No rules for the conditional operator, name colons, `when`, the type in front of a name, one-line accessor lists, and relational patterns** — rejected as recommended: with the minimal separator, the joins the line-break phase makes today would read `a⏎? 1` as `?1`, `name:⏎"x"` as `name:"x"`, `case (1, 2)⏎when` as `(1, 2)when`, `int?⏎a` as `int?a`, `int P {get;set;}` as `int P {get;set; }`, and `is >⏎5` as `is >5`.
- **Rules for the remaining keyword-before-parenthesis pairs of entry 9** — not proposed: no output for them is decided anywhere, the inputs are rare, and the same pairs written on one line are not normalized either.
