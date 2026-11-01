# The spacing phase is the only owner of same-line gaps

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-SPACE-002 (changed), FMT-SPACE-012 (changed), FMT-SPACE-014 (changed), FMT-SPACE-016 to FMT-SPACE-027 (added), the fourth non-goal of `horizontal-spacing.md` (narrowed), FMT-BREAK-024 (reworded), FMT-BREAK-026 (changed), FMT-BREAK-031 (added), FMT-STRUCT-026 (changed) |
| Depends on | [Every formatter phase returns a tree that round-trips and keeps foreign annotations](../2026-10/phase-tree-round-trip.md) |

## What

The spacing phase decides the whitespace of every gap between two tokens on one line. The line-break phase only adds and removes line breaks (FMT-BREAK-031):

- **A join writes the minimal separator.** Where the line-break phase joins two tokens onto one line, it leaves no whitespace between them, unless the two tokens written without whitespace would not read back as the same two tokens — one token (`protected` and `internal`, `out` and `var`), another token pair (`-` and `-b`), or the start of a comment (`/` and `*p`). There it writes one space. Whether two tokens read back as written is answered by the shared lexer-based helper that FMT-STRUCT-026 already uses. Which separator a join writes no longer depends on the kind of the tokens it joins, and every join asks the helper, also the joins of an accessor list and of a binary operator that today write no separator at all.
- **The line-break phase writes no other whitespace into a gap that stays on one line.** Today five places of the phase write spaces of their own — the assignment rewriter in front of a joined value, the conditional rewriter behind `?` and `:`, the `case … when` rewriter in front of `when`, the property-layout rewriter on both sides of the arrow of an expression-bodied property or indexer (also when nothing is joined: `int P =>1;` becomes `int P => 1;`), and the accessor-list layout inside a one-line accessor list (also when nothing is joined: `int P {get;set;}` becomes `int P { get; set; }`) — and the list-element join chooses one space or none by token kind. All of them go. One writer stays as a recorded exception: the attribute-list merge shared with the attribute-list code fixes writes one space behind a merged list, which the spacing phase decides alike, until the merge moves into the phase that owns it (thoenissen/Reihitsu#1492).
- **The spacing phase gets the rules that keep today's output where the join or the line-break phase spaced a pair.** Each rule is keyed by the construct, applies to every gap on one line, joined or written so, and is a visible output change the maintainer decided (see `Decisions`):
  - every attribute-target colon: no space before it, one space after it (FMT-SPACE-016, replacing the `return:` exception of FMT-SPACE-012);
  - every arrow `=>` — lambda, expression body of a member or accessor, switch-expression arm: one space on each side (FMT-SPACE-017);
  - the `?` and `:` of a conditional expression: one space on each side (FMT-SPACE-018);
  - the colon of a name or expression colon — named argument, tuple element, subpattern: no space before it, one space after it (FMT-SPACE-019);
  - the `when` of a `case` guard, a switch-expression arm, or a `catch` filter: one space in front of it (FMT-SPACE-020);
  - the type of a parameter, tuple element, variable declaration, declaration expression, declaration pattern, `foreach` variable, `catch` declaration, or `from`/`join` range variable and the name behind it, and a recursive or list pattern and its designation: one space (FMT-SPACE-021);
  - an accessor list on one line: one space behind its `{` and between two accessors (FMT-SPACE-022);
  - the operator of a relational pattern and its operand: one space (FMT-SPACE-023);
  - a lambda modifier (`static`, `async`), a modifier of an argument, parameter, type, or expression (`ref`, `out`, `in`, `readonly`, `scoped`, `this`, `params`), `await`, `stackalloc`, `var` in front of a parenthesized designation, the pattern `not`, and the `in` of `foreach`, `from`, and `join`: one space behind the keyword (FMT-SPACE-024);
  - `with`: one space on each side (FMT-SPACE-025);
  - an explicit lambda return type and the lambda's `(`: one space (FMT-SPACE-026);
  - a function pointer's `*` and its `managed` or `unmanaged`: one space (FMT-SPACE-027).
- **The gap behind a `/** */` comment the right token carries follows the pair's rule.** Today a rule's decision reaches that gap only when the rule allows no space, and the whitespace there is removed; a gap the rule wants spaced keeps what the author wrote, none or several spaces. The decision now applies in both directions, as it does behind a comment the left token carries (FMT-SPACE-013), so the comment cases of the rules above read like their pairs without a comment (FMT-SPACE-014, entry 14). The whitespace in front of the comment stays as written.
- **Arrows created by a structural transform are spaced by the spacing phase.** FMT-STRUCT-026 kept one space on each side of the `=>` the accessor and get-only-member transforms create until a spacing rule owns that gap. FMT-SPACE-017 owns it now, so the transforms write no whitespace there; the final output does not change.

Every pair the line-break phase spaced today and that would read back without a space is decided by one of these rules, so no joined output changes; the rules change the same pairs where the author wrote them on one line (entries 1 to 9, 11 to 13), also behind a `/** */` comment the right token carries (entry 14). The pairs that no rule covers keep the gap the spacing phase finds and only have their runs of whitespace collapsed (FMT-SPACE-002); for a joined pair, that gap is the minimal separator, which equals today's separator for every such pair.

## Why

The same pair was spaced differently depending on whether the author wrote it on one line or the line-break phase joined it, and the line-break phase spaced some constructs on its own even when it joined nothing:

- `Foo(name:"x")` keeps its zero gap, while `Foo(name:` followed by `"x")` on the next line becomes `Foo(name: "x")`.
- `int P =>1;` becomes `int P => 1;`, while the lambda `y =>y` and the switch arm `1 =>1` stay as written; FMT-PIPE-014 requires every arrow to be spaced alike.
- `[return :Obsolete]` becomes `[return:Obsolete]`, because `return` is a keyword of FMT-SPACE-012, while `[field :NonSerialized]` keeps its space; a third policy writes the colon tight where the list-element join joins it. FMT-PIPE-014 requires every attribute-target colon to be spaced alike.

A spacing decision in the line-break phase is a second owner of a decision FMT-SPACE makes: the two can disagree, and whichever runs last wins only where it has a rule. Two joins wrote no separator at all, so the phase's own text was another program. `protected` and `internal` — or `internal` and `protected`, `private` and `protected`, `protected` and `private` — on two lines of an auto-property's accessor list become `protectedinternal set;` in the final output today, which does not compile; whole-document formatting, the CLI, and the RH5408 code fix all produce it. `a -` followed by `-b` becomes `a⏎--b` in the phase's output, which the spacing phase repairs only because `-` has a rule (FMT-PIPE-021).

The minimal separator follows the phase-boundary contract of FMT-STRUCT-026: a phase writes only what its text needs to parse back to its tree, and the phase that owns a layout decision writes it. A space as the neutral separator would need new rules for every pair the join keeps tight today — the parenthesis of every parameter list (`void M (int a)`), every invocation inside a chain (`items.Where (x => x)`), a cast operand, a range, a pointer type — and would change the examples of FMT-BREAK-011 and FMT-BREAK-020. The minimal separator keeps all of those. The pairs it would join without a space although the join spaced them today are many: a name colon, a conditional operator, a `when`, a type and its name, a relational pattern, a keyword and its operand, `with`, a lambda return type, a function pointer's calling convention. Each gets a rule, so that today's joined output stays and the same pair reads alike on one line. The maintainer already decided the output of some of them for the formatter — every arrow (thoenissen/Reihitsu#958, thoenissen/Reihitsu#1514), the conditional operator and named arguments (thoenissen/Reihitsu#920), the relational pattern (thoenissen/Reihitsu#912) — and decided the others in this run.

## Visible output changes

Every entry shows the output the maintainer chose; the answers are recorded in `Decisions`. Entry 10 was not a choice. Entry 14 applies the rules of the other entries and FMT-SPACE-010 behind a comment; it is a lead decision (see `Decisions`).

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

        try
        {
        }
        catch (System.Exception e)when (e != null)
        {
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

        try
        {
        }
        catch (System.Exception e) when (e != null)
        {
        }
    }
}
```

The space behind `when` is not decided: `case int i when(i > 0):` stays as written.

### 6. One space between a type or pattern and the name it declares (FMT-SPACE-021)

```csharp
using System.Linq;

class C
{
    void M(int?a, int[]b, (int, int)c, object o, int?[] xs)
    {
        int?z = null;

        N(out int?w);

        foreach (int?x in xs)
        {
        }

        try
        {
        }
        catch (System.Exception?e)
        {
        }

        var q = from int?y in xs select y;
        var p = o is { }r && o is [1]l;
    }
}
```

Before: unchanged.

After:

```csharp
using System.Linq;

class C
{
    void M(int? a, int[] b, (int, int) c, object o, int?[] xs)
    {
        int? z = null;

        N(out int? w);

        foreach (int? x in xs)
        {
        }

        try
        {
        }
        catch (System.Exception? e)
        {
        }

        var q = from int? y in xs select y;
        var p = o is { } r && o is [1] l;
    }
}
```

A joined `from int?⏎y in xs` keeps `from int? y in xs`, as before.

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

### 9. One space behind a modifier or prefix keyword (FMT-SPACE-024)

```csharp
using System.Linq;
using System.Threading.Tasks;

static class C
{
    static void E(this(int, int) t, params(int, int)[] a)
    {
    }

    static async Task M(int a, int[] xs, object o, Task t)
    {
        N(async(x) => x, static(int v) => v);
        N(ref(a), in(a));
        await(t);

        var b = o is not"x" && o is not> 5;
        var c = o is var(u, w);

        foreach (var e in(xs))
        {
        }

        var q = from z in(xs) select z;
    }

    static unsafe void F(int* p)
    {
        N(out*p);

        var s = stackalloc[] { 1 };
    }
}
```

Before: unchanged.

After:

```csharp
using System.Linq;
using System.Threading.Tasks;

static class C
{
    static void E(this (int, int) t, params (int, int)[] a)
    {
    }

    static async Task M(int a, int[] xs, object o, Task t)
    {
        N(async (x) => x, static (int v) => v);
        N(ref (a), in (a));
        await (t);

        var b = o is not "x" && o is not > 5;
        var c = o is var (u, w);

        foreach (var e in (xs))
        {
        }

        var q = from z in (xs) select z;
    }

    static unsafe void F(int* p)
    {
        N(out *p);

        var s = stackalloc [] { 1 };
    }
}
```

The same pairs wrapped inside an argument or parameter, such as `N(await⏎(t));` or `N(o is not⏎"x");`, are joined with the space, as before. The same holds in front of a lambda's return type (`static (int, int) () => (1, 2)`, `ref (int, int) () => ref s_t`), behind the `ref` of a function pointer's parameter (`delegate*<ref (int, int), void>`) and of a ref expression (`return ref (x);`), and behind the `var` of a deconstruction (`var (u, w) = s_t;`). `typeof(C)`, `default(int)`, and `nameof(a)` keep their gap, and so does a local function's modifier (`static(int, int) K()`). The `ref` and `readonly` of a local's, a `foreach` variable's, a field's, or a return type's type are outside the rule and keep the author's spacing, as on `main` (`ref(int, int) r = ref t;`, like `public(int, int) G;`); no join reaches them.

The one-line forms of these positions (14A):

```csharp
unsafe class C
{
    static (int, int) s_t;
    static int _x;

    static ref int M(bool b, ref int x, ref int y)
    {
        var(u, w) = s_t;
        N(ref(int, int) () => ref s_t, static(int, int) () => (1, 2));

        delegate*<ref(int, int), void> f = null;
        ref var z = ref(_x);

        return ref(x);
    }
}
```

Before: unchanged.

After:

```csharp
unsafe class C
{
    static (int, int) s_t;
    static int _x;

    static ref int M(bool b, ref int x, ref int y)
    {
        var (u, w) = s_t;
        N(ref (int, int) () => ref s_t, static (int, int) () => (1, 2));

        delegate*<ref (int, int), void> f = null;
        ref var z = ref (_x);

        return ref (x);
    }
}
```

### 10. An accessor modifier pair on two lines compiles (FMT-BREAK-031)

```csharp
class C
{
    public int P
    {
        get;
        protected
        internal set;
    }

    public int Q
    {
        get;
        private
        protected set;
    }
}
```

Before:

```csharp
class C
{
    public int P { get; protectedinternal set; }

    public int Q { get; privateprotected set; }
}
```

After:

```csharp
class C
{
    public int P { get; protected internal set; }

    public int Q { get; private protected set; }
}
```

`internal⏎protected` and `protected⏎private` gave `internalprotected` and `protectedprivate` and give `internal protected` and `protected private`. The RH5408 code fix, which lays out the accessor list through the formatter, produced the same non-compiling output for `get;⏎protected⏎internal set;` and produces `protected internal set;`. Today's output does not compile (FMT-PIPE-003); the minimal separator writes the space the two keywords need. This entry is not a choice: both separators fix it.

### 11. One space on each side of `with` (FMT-SPACE-025)

```csharp
class C
{
    void M(P p, P[] ps)
    {
        var a = p.Q()with { X = 1 };
        var b = ps[0]with { X = 2 };
    }
}
```

Before:

```csharp
class C
{
    void M(P p, P[] ps)
    {
        var a = p.Q()with
                     {
                         X = 1
                     };
        var b = ps[0]with
                     {
                         X = 2
                     };
    }
}
```

After:

```csharp
class C
{
    void M(P p, P[] ps)
    {
        var a = p.Q() with
                      {
                          X = 1
                      };
        var b = ps[0] with
                      {
                          X = 2
                      };
    }
}
```

The initializer is laid out over several lines before and after (FMT-BREAK-021); only the space in front of `with` and the column of the initializer behind it change.

### 12. One space between an explicit lambda return type and its parameter list (FMT-SPACE-026)

```csharp
using System.Collections.Generic;

class C
{
    void M()
    {
        N(int(x) => x, List<int>(x) => x);
    }
}
```

Before: unchanged.

After:

```csharp
using System.Collections.Generic;

class C
{
    void M()
    {
        N(int (x) => x, List<int> (x) => x);
    }
}
```

### 13. One space behind a function pointer's `*` (FMT-SPACE-027)

```csharp
unsafe class C
{
    void M(delegate*unmanaged[Cdecl]<int, void> f)
    {
    }
}
```

Before: unchanged.

After:

```csharp
unsafe class C
{
    void M(delegate* unmanaged[Cdecl]<int, void> f)
    {
    }
}
```

### 14. The gap behind a `/** */` comment in front of a token follows the pair's rule (FMT-SPACE-014)

```csharp
class C
{
    int M(bool c, int b)
    {
        var d = b /** sum */+b;
        var e = b /** sum */   + b;
        var f = b  /** sum */+ b;

        return c ? b /** yes */:b;
    }
}
```

Before:

```csharp
class C
{
    int M(bool c, int b)
    {
        var d = b /** sum */+ b;
        var e = b /** sum */   + b;
        var f = b  /** sum */+ b;

        return c ? b /** yes */:b;
    }
}
```

After:

```csharp
class C
{
    int M(bool c, int b)
    {
        var d = b /** sum */ + b;
        var e = b /** sum */ + b;
        var f = b  /** sum */ + b;

        return c ? b /** yes */ : b;
    }
}
```

The whitespace in front of each comment stays as written (`b  /** sum */`). A gap behind the comment that the rule wants empty was already emptied and stays so (`[Obsolete /** why */ ]` gives `[Obsolete /** why */]`). The new rules' comment cases follow the same way, for example `M(b /** x */ :b)` gives `M(b /** x */: b)`. Where the documentation-comment phase moves the comment onto its own line in front of the token — in front of a token that starts a node, such as the operand of a relational pattern or a `when` — no same-line gap is left.

### Unchanged

Every joined output of the line-break phase stays as it is: the joins that were tight stay tight under the minimal separator, and every pair the joins and the line-break phase spaced is spaced by one of the rules above. The examples of FMT-BREAK-011, FMT-BREAK-015, FMT-BREAK-016, FMT-BREAK-018, FMT-BREAK-020, FMT-BREAK-024, FMT-BREAK-026, and FMT-BREAK-028 keep their output. Every final output that changes is a pair one of the rules decides and that the author wrote on one line (entries 1 to 9 and 11 to 14), the one-line attributed accessor list of entry 7, and the non-compiling output of entry 10. Code-fix outputs change only through the formatter, where the fixed node holds such a pair. No rule page changes; no analyzer reports any of the pairs above.

## Decisions

### The line-break phase joins with the minimal separator

*Decided by the maintainer in this run (1B).* No space at a join unless the two tokens would read differently, over one space as the neutral separator; see `Why` and `Alternatives considered`.

### Every arrow gets one space on each side now

*Decided by the maintainer in this run (2B), confirming earlier answers.* Chosen over keeping the author's spacing until thoenissen/Reihitsu#958 is implemented: the maintainer had already decided this output — `x => x` for a lambda (thoenissen/Reihitsu#920) and one space for every arrow kind (thoenissen/Reihitsu#912, recorded on thoenissen/Reihitsu#958; expression bodies in thoenissen/Reihitsu#1514). Keeping the author's spacing would change `int P => 1;` to `int P =>1;` for one release, and with the minimal separator a joined arrow would read `x =>x` and `int T =>5;`, against the examples of FMT-BREAK-015 and FMT-BREAK-026.

### Every attribute-target colon gets one space behind it

*Decided by the maintainer in this run (3B).* Earlier, the maintainer had decided only that every attribute target is spaced alike (thoenissen/Reihitsu#912); this run decides the space behind the colon. Chosen over keeping the author's spacing behind it, because under that alternative the output still depends on how the author wrote the colon, and `[return: Obsolete]` is the form the examples of FMT-SPACE-012 and FMT-BREAK-027 already show.

### The conditional operator, named arguments, and relational patterns follow earlier answers

*Confirmed by the maintainer in this run (4B, 5B, 9B).* `b ? 1 : 2` and `M(b: b)` were decided in thoenissen/Reihitsu#920, `x is > 5` in thoenissen/Reihitsu#912. Tuple element names, named attribute arguments, and subpattern names are spaced like named arguments as the same family (5B).

### Every pair the join spaced today gets a rule

*Decided by the maintainer in this run (6C, 7C, 8B, 10B, 11B, 12B, 13B).* `when`, a type and its name, a modifier or prefix keyword and its operand, `with`, a lambda return type, and a function pointer's calling convention get a rule each, so that the join's output stays and the same pair written on one line reads alike. Without the rules, the minimal separator would write `case (1, 2)when b:`, `int?a`, `not"x"`, `await(t)`, `out*p`, `p.Q()with`, `List<int>(x) => x`, and `delegate*unmanaged`.

### One-line keyword positions no join reaches

*Decided by the maintainer in this run (14A).* One space also goes behind the keyword where the pair is written on one line, outside an argument: the `ref` of a ref expression in a statement (`return ref (x);`, `ref var z = ref (_x);`), a lambda's `static`, `async`, or `ref` in front of a tuple return type (`static (int, int) () => (1, 2)`, `ref (int, int) () => ref s_t`), and the `ref` of a function pointer's parameter (`delegate*<ref (int, int), void>`). Question 10 had shown only arguments and parameters; these positions are the same keywords in front of the same operands. The `ref` and `readonly` of a local's, a field's, a `foreach` variable's, or a return type's type stay outside the rule (lead decision; the output question goes to a follow-up).

### The rules are keyed by construct and apply to every gap on one line

The spacing phase cannot tell a joined gap from one the author wrote, and it must not: a rule that applied only to joined gaps would keep the difference this change removes. Each new rule therefore also changes one-line input, and each covers its whole family (FMT-PIPE-014; the maintainer chose the whole family for `when` and declared names, 6C and 7C): the `catch` filter's `when` with the other guards, the `foreach`, `catch`, and range variables with the other declared names, the `in` of `foreach` with the `in` of a query.

### A rule decides the gap behind a right-carried comment in both directions

*Lead decision in this run (row 19), following the maintainer's answers for each pair.* The rule's decision was already applied behind a comment the left token carries (FMT-SPACE-013) and, when it allows no space, behind one the right token carries. Applying it only in that one direction would leave `/** yes */:b` and `/** sum */+b` tighter than the conditional-operator and operator rules demand, while the same pairs without a comment are spaced. The whitespace in front of the comment stays the author's, as before.

### Structural transforms create the arrow without spaces

FMT-SPACE-017 owns the gap, which FMT-STRUCT-026 named as the condition for dropping the transforms' spaces around a created `=>` (thoenissen/Reihitsu#1514). The created tokens still carry no elastic trivia.

### The attribute-list merge keeps its spaces for now

The merge of a declaration's attribute lists is shared with the attribute-list code fixes and writes one space behind the merged `]` and behind a token left touching a joined token. The spacing phase decides the same space (FMT-SPACE-003, FMT-SPACE-008), so the output does not depend on it. FMT-BREAK-031 records it as an exception until the merge moves into the phase that owns it (thoenissen/Reihitsu#1492), instead of changing a helper the code fixes share in this run.

## Alternatives considered

- **One space as the neutral separator** (the issue's first reading) — rejected: every joined pair that is tight today and has no rule would get a space, among them the parenthesis of a parameter list (`void M (int a)`, against FMT-BREAK-011's example), an invocation in a chain (`items.Where (x => x)`, against FMT-BREAK-020's example), a cast operand (`(int) value`), a range (`start ..end`), and a function-pointer type (`delegate * unmanaged [Cdecl]`); rules for those pairs are not decided anywhere.
- **Keep today's separator table and move it into the spacing phase, applied only to joined gaps** — rejected: the line-break phase would have to mark its joins for the spacing phase, and the same pair would stay spaced differently when joined and when written on one line, which is the defect this change removes.
- **Keep the author's spacing around every arrow until thoenissen/Reihitsu#958 is implemented** (the issue's proposal) — rejected; see `Decisions`.
- **No space behind an attribute-target colon unless the author wrote one** (the issue's proposal: `[field :NonSerialized]` becomes `[field:NonSerialized]`) — rejected; see `Decisions`.
- **No rules for the pairs the join spaced today** — rejected: with the minimal separator, the joins the line-break phase makes today would read `a⏎? 1` as `?1`, `name:⏎"x"` as `name:"x"`, `case (1, 2)⏎when` as `(1, 2)when`, `int?⏎a` as `int?a`, `int P {get;set;}` as `int P {get;set; }`, `is >⏎5` as `is >5`, `not⏎"x"` as `not"x"`, `await⏎(t)` as `await(t)`, `out⏎*p` as `out*p`, `p.Q()⏎with` as `p.Q()with`, `List<int>⏎(x) => x` as `List<int>(x) => x`, and `delegate*⏎unmanaged` as `delegate*unmanaged`.
- **Rules keyed by the declarations and guards a join reaches only** (leaving `foreach (int?x in xs)`, `catch (Exception?e)`, and the `catch` filter's `when` alone) — rejected: FMT-PIPE-014 asks for each family to be spaced alike, and these members would keep the author's gap while their siblings are normalized.
- **Strip the attribute-list merge's spaces in this run** — rejected: the output does not change, and the helper is shared with code fixes whose move is thoenissen/Reihitsu#1492.
