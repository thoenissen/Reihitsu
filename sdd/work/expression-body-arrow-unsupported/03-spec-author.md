Role: spec author
Commit: b18e641
Verdict: change doc written

Inputs: `00-lead.md` (*Decisions taken* 3 and 4), `01-spec-author.md`, `02-skeptic.md`, the change doc, FMT-UNSUP-013, SHARED-BODY-001, and `ExpressionBodyRewriteUtilities.BlocksRewrite`. My prompt carried no intended fix, suspected cause, or hoped-for verdict. Production code on this branch equals `main`, so every *Before* below is `main`'s output. Fixtures lived in the scratchpad, outside the repository. The change doc stays `Status: Draft`.

## Changed requirements

- FMT-UNSUP-013 — *Why* reworded (decision 3): the FMT-BREAK-026 join refusal is named for properties and indexers only; for an accessor, whose arrow FMT-BREAK-026 does not join, the cost is the get-only lift's seam checks (FMT-STRUCT-015). The statement on conditional groups is corrected (*Breaking input A*): a group that closes directly in front of the `=>` — around the signature or a constraint clause — or that chooses the `=>` puts a directive into the gap and is refused; only a group around the whole member, from in front of its first token through behind its `;`, is formatted. It also says that RH3202–RH3208 and RH5401 report most members that hold such a directive as a whole (without IDs: the formatter spec's `Uses` row lists no analyzer requirement). Normative sentence and examples unchanged — clarified
- SHARED-BODY-001 — states the conditional-group cases (decision 4): a conditional directive in the trailing trivia of the token in front of `=>` or in the leading trivia of `=>` does not by itself keep the member from being reported; a group that closes directly in front of `=>` (signature, constraint clause) and a group that chooses `=>` and closes behind the `;` are reported with the fix withheld (FMT-UNSUP-013, SHARED-FIX-010); a member is not reported only when a directive of its group lies in the span from `=>` through the `;` (FMT-STRUCT-007). *Why* extended; new example (RH3202, group around the signature: reported, no fix) with its other side (the group around the whole member: reported in the active branch, fixed as the formatter converts it) — clarified
- Change doc `sdd/changes/2026-11/expression-body-arrow-leading-comment-unsupported.md`: *Why* split into property/indexer (join refusal) and accessor (lift seams) bullets; the conditional-group sentence corrected as in FMT-UNSUP-013; *Directives and disabled text belong to the shape* names changes 9 and 12 as the cost; new *Visible output changes* 12; *Unchanged* gains "a conditional group around the whole member is formatted as before".

No requirement added or retired, no `Uses` row or code owner changed, no rule page changed.

Checks run:
- `scripts/format.sh` on change 12's input (one file, the two methods and the property): output identical byte for byte to the change doc's *Before*. Also on each member alone (skeptic's `d1`, `d2`, `d4`): same results.
- `scripts/apply-fix.sh RH3202` on change 12's input: `2 fixed` (LF, CRLF); on `d1` and `d2` alone: `4 fixed`, each the formatter's block. `scripts/apply-fix.sh RH5401` on the property alone: `2 no-fix-offered`.
- `scripts/apply-fix.sh RH3202` and `scripts/format.sh` on the whole-member group (SHARED-BODY-001's new other-side example, change 12's alternative): `fixed`, output equal to the example in both.
- Refusal locations of change 12 (`(9,1)`; `Make` alone at its `#if NET7_0_OR_GREATER`; `P` alone at its `#endif`) are **derived** from FMT-UNSUP-013's location rule, not observed: the precheck check does not exist on this branch, so the CLI cannot report it yet. Counted by hand: without the symbols, `#if NET8_0`, the disabled signature, and `#else` are in the leading trivia of the active `public`; only `#endif` (line 9, column 1) is in the leading trivia of `=>`.
- `scripts/test.sh --project architecture`: 197 passed, 1 failed — `EveryRequirementOfAnEnforcedSpecIsCovered` (`FMT-UNSUP-013: no test covers it`), expected until the test implementer adds the covering tests. A first run also failed `UsesListsExactlyTheReferencedRequirements` because my FMT-UNSUP-013 wording cited SHARED-BODY-001 and RH5401-004; I removed the IDs from that sentence and it passes.

## Examples for the tests

- SHARED-BODY-001: the new RH3202 example (group around the signature closing in front of `=>`: reported, no fix — after the change; today the fix is offered), the constraint-clause and arrow-choosing variants named in its text, and the other side (group around the whole member: reported, fixed).
- Change doc *Visible output changes* 12: formatter refusal at `(9,1)`, RH3202 withheld on both methods, RH5401 unchanged.
- Everything listed in `01-spec-author.md` *Examples for the tests* stands.

## Output questions

The change doc's complete *Visible output changes*, drafted in the `AGENTS.md` output-question form. Numbers 1–12 are the change doc's; 12 is new. `Q2` is my earlier output question 2 (reason wording), kept under its label; earlier questions 1 and 3 are decided (lead decisions 1 and 2) and not repeated. The lead renumbers when it batches.

Every *After, A* below that refuses reads, unless stated otherwise: input unchanged; the CLI reports `error: <file>(<line>,<column>): unsupported syntax: a comment or directive between the arrow of an expression body and the code in front of it. The file was left unchanged.` and exits with code 2; declared in this work. Each *After, B* is "as before (`main`): this place is not part of the shape". The special cases the shape deletes, the same for every question: the line-break phase's refusal to put a property's or indexer's arrow on the member's line, the conversion's move of comments in front of the arrow into the new block, and the get-only property lift's checks of the trivia behind `get` and in front of the getter's arrow.

**1 · Formatter and RH5401 · Expression-style get-only properties should be single-lined — `//` comment behind a property's name**

Input:
```csharp
class C
{
    int P // c
        => 1;
}
```

Before (`main`):
```csharp
class C
{
    int P // c
    => 1;
}
```

After, A (recommended): input unchanged. The CLI reports `error: <file>(3,11): unsupported syntax: a comment or directive between the arrow of an expression body and the code in front of it. The file was left unchanged.` Code fixes no longer offered on this property: RH5401's, which was not offered before either; RH5401 still reports it. Declared in this work.

After, B: as before (`main`).

Why A: today the arrow lands in a column no rule places, and the comment written above the property is formatted.

**2 · Formatter · comment in front of an indexer's arrow, at a property's arrow, and on one line**

Input:
```csharp
class C
{
    int this[int i] // c
        => i;

    int Q
        /* c */ => 2;

    int A /* c */ => 1;
}
```

Before (`main`):
```csharp
class C
{
    int this[int i] // c
    => i;

    int Q
    /* c */ => 2;

    int A /* c */ => 1;
}
```

After, A (recommended): input unchanged; the CLI reports it at `(3,21)`. `A` alone, which `main` kept, is refused too. Code fixes no longer offered: any that formats these members (RH5401 on `Q`, not offered before either). Declared in this work.

After, B: as before (`main`).

Why A: the indexer and `Q` are misplaced today, and one boundary for both gaps of the arrow matches the other operator shapes.

**3 · Formatter · comment behind an accessor's keyword**

Input:
```csharp
class C
{
    private int _v;

    int V
    {
        get // c
            => _v;
    }
}
```

Before (`main`):
```csharp
class C
{
    private int _v;

    int V
    {
        get // c
        => _v;
    }
}
```

After, A (recommended): input unchanged; the CLI reports it at `(7,13)`. No code fix that formats this property is offered on it. Declared in this work.

After, B: as before (`main`).

Why A: the get-only lift's checks of this gap go away with it.

**4 · Formatter and RH3202 · Expression-style methods should not be used — block comment at a method's arrow, and on one line**

Input:
```csharp
class C
{
    int B()
        /* c */ => 1;

    int M() /* c */ => 1;
}
```

Before (`main`):
```csharp
class C
{
    int B()
    /* c */ {
        return 1;
    }

    int M() /* c */
    {
        return 1;
    }
}
```

After, A (recommended): input unchanged; the CLI reports it at `(4,9)`. RH3202's fix, and every other fix that formats these methods, is no longer offered on them; RH3202 still reports both. Declared in this work.

After, B: as before (`main`).

Why A: `/* c */ {` is a placement no requirement specifies.

**5 · Formatter and RH3202 · Expression-style methods should not be used — comment on a line of its own in front of the arrow, and behind the signature**

Input:
```csharp
class C
{
    int Other()
        // before arrow
        => 1;

    void Run() // run
        => Foo();

    void Foo()
    {
    }
}
```

Before (`main`):
```csharp
class C
{
    int Other()
    {
        // before arrow
        return 1;
    }

    void Run() // run
    {
        Foo();
    }

    void Foo()
    {
    }
}
```

After, A (recommended): input unchanged; the CLI reports it at `(4,9)`. RH3202's fix is no longer offered on `Other` or `Run`; RH3202 still reports both. Declared in this work. The same comment written above the method is formatted, before and after.

After, B: as before (`main`).

Why A: this is the case the declaration costs most, but RH5040 reports the comment, and the conversion's comment move is one of the special cases it deletes.

**6 · Formatter · a one-line comment in front of the arrow refuses the whole file**

Input:
```csharp
class C{
    int A /* c */ => 1;
}
```

Before (`main`):
```csharp
class C
{
    int A /* c */ => 1;
}
```

After, A (recommended): input unchanged, `class C{` included; the CLI reports it at `(2,11)`. Declared in this work.

After, B: as before (`main`).

Why A: the get-only lift's check behind `get` goes only if `get /* c */ => _v;` on one line is refused too.

**7 · Formatter, RH3202 and RH3203 · comment in front of the arrow behind a constructor initializer and behind a constraint clause**

Input:
```csharp
class B
{
    public B(int v)
    {
    }
}

class C : B
{
    private int _v;

    public C() : base(1) // c
        => _v = 1;

    T Make<T>()
        where T : new() // c
        => new T();
}
```

Before (`main`):
```csharp
class B
{
    public B(int v)
    {
    }
}

class C : B
{
    private int _v;

    public C()
        : base(1) // c
    {
        _v = 1;
    }

    T Make<T>()
        where T : new() // c
    {
        return new T();
    }
}
```

After, A (recommended): input unchanged; the CLI reports it at `(12,26)`. RH3203's fix on the constructor and RH3202's on `Make` are no longer offered; both still report. Declared in this work.

After, B: as before (`main`).

Why A: the token in front of the arrow is the boundary for every owner alike.

**8 · Formatter and RH3208 · Expression-style local functions should not be used — comment behind a local function's signature**

Input:
```csharp
class C
{
    void M()
    {
        int Local() // c
            => 1;

        Local();
    }
}
```

Before (`main`):
```csharp
class C
{
    void M()
    {
        int Local() // c
        {
            return 1;
        }

        Local();
    }
}
```

After, A (recommended): input unchanged; the CLI reports it at `(5,21)`. RH3208's fix is no longer offered on `Local`; RH3208 still reports it. Declared in this work.

After, B: as before (`main`).

Why A: a local function converts through the same code as a method.

**9 · Formatter · a conditional group that chooses the arrow, and a region in front of the arrow**

Input, parsed without `DEBUG`:
```csharp
class C
{
    int A
#if DEBUG
        => 1;
#else
        => 2;
#endif

    int M()
#if DEBUG
        => 1;
#else
        => 0;
#endif
}
```

Before (`main`):
```csharp
class C
{
    int A
#if DEBUG
        => 1;
#else
    => 2;
#endif

    int M()
#if DEBUG
        => 1;
#else
    {
        return 0;
    }
#endif
}
```

After, A (recommended): input unchanged; the CLI reports it at `(4,1)`, in every build. RH3202's fix on `M` is no longer offered; RH3202 still reports `M`. Declared in this work.

After, B: as before (`main`).

And the region case, input:
```csharp
class C
{
    int M()
        #region R
        => 1;
        #endregion
}
```

Before (`main`):
```csharp
class C
{
    int M()

    #region R

    => 1;

    #endregion // R
}
```

After, A (recommended): input unchanged; the CLI reports it at `(4,9)`. RH3202 does not report it, before and after. Declared in this work.

After, B: as before (`main`).

Why A: the property's arrow is misplaced today, and the join refusal tests directives as it tests comments.

**10 · RH3202 · Expression-style methods should not be used — fix on a comment in front of the arrow** (RH3203, RH3205, RH3206, RH3207, RH3208 alike)

Input:
```csharp
class C
{
    int Other()
    // before arrow
    => 1;

    void Run() // run
        // first
        => Foo();

    void Foo()
    {
    }
}
```

Before (`main`), RH3202's fix:
```csharp
class C
{
    int Other()
    {
        // before arrow
        return 1;
    }

    void Run() // run
    {
        // first
        Foo();
    }

    void Foo()
    {
    }
}
```

After, A (recommended): no fix offered on either; both stay reported. Declared in this work.

After, B: as before (`main`).

Why A: a fix that formats a member the formatter refuses is withheld, as for every declared shape.

**11 · RH5110 · Generic type constraints should be on their own line, and RH7101 · Do not combine fields — fixes that format a node holding the shape**

Input (RH5110):
```csharp
class C
{
    T Make<T>() where T : new() // c
        => new T();
}
```

Before (`main`), RH5110's fix:
```csharp
class C
{
    T Make<T>()
        where T : new() // c
    {
        return new T();
    }
}
```

After, A (recommended): no fix offered; the diagnostic stays. Declared in this work.

After, B: as before (`main`).

Input (RH7101):
```csharp
class C
{
    private int _a, _b;

    int A // c
        => _a;
}
```

Before (`main`), RH7101's fix: `private int _a;` and `private int _b;` on two lines, the rest unchanged.

After, A (recommended): no fix offered; the diagnostic stays. Declared in this work.

After, B: as before (`main`).

Why A: both fixes format the node that holds the shape, so they are withheld like every other fix that does.

**12 · Formatter and RH3202 · Expression-style methods should not be used — conditional group around the signature or a constraint clause, in front of a shared arrow** (new)

Input, parsed without `NET8_0` and `NET7_0_OR_GREATER`:
```csharp
using System.Numerics;

class C
{
#if NET8_0
    public int M(int a)
#else
    public int M(long a)
#endif
        => 1;

    T Make<T>()
#if NET7_0_OR_GREATER
        where T : INumber<T>
#endif
        => default;

#if NET8_0
    public int P
#else
    public long P
#endif
        => 1;
}
```

Before (`main`), the formatter, and RH3202's fix alike for the two methods:
```csharp
using System.Numerics;

class C
{
#if NET8_0
    public int M(int a)
#else
    public int M(long a)
#endif
    {
        return 1;
    }

    T Make<T>()
#if NET7_0_OR_GREATER
        where T : INumber<T>
#endif
    {
        return default;
    }

#if NET8_0
    public int P
#else
    public long P
#endif
    => 1;
}
```

After, A (recommended): input unchanged. The CLI reports `error: <file>(9,1): unsupported syntax: a comment or directive between the arrow of an expression body and the code in front of it. The file was left unchanged.` (location derived from the spec, not yet observed). RH3202's fix, and every other fix that formats these methods, is no longer offered on them; RH3202 still reports both, and RH5401 still reports `P` with no fix, as before. Declared in this work.

After, B: as before (`main`): a conditional directive in front of the arrow is not part of the shape.

Why A: the maintainer's answer covers directives in this place, and with B the property join refusal and the get-only lift's checks, which test directives and comments alike, would stay. Writing the group around the whole member keeps the formatting and the fix, before and after.
Special cases the shape deletes: the line-break phase's refusal to put a property's arrow on the property's line, the conversion's move of comments in front of the arrow into the new block, and the get-only property lift's checks of the trivia behind `get` and in front of the getter's arrow.

**Q2 · CLI · reason reported for the new unsupported shape** (unchanged from `01-spec-author.md`)

Input:
```csharp
class C
{
    int P // c
        => 1;
}
```

Before (`main`):
```csharp
class C
{
    int P // c
    => 1;
}
```

After, A (recommended): input unchanged; the CLI reports `error: <file>(3,11): unsupported syntax: a comment or directive between the arrow of an expression body and the code in front of it. The file was left unchanged.` Declared in this work.

After, B: the same, with the issue's wording `a comment or directive between an expression body's arrow and the code in front of it`.

Why A: it pairs with the sibling shape's reason, "between the arrow of an expression body and its expression".

Unchanged, listed in the change doc and not asked: RH5401 reporting with no fix; RH5040 and RH3202–RH3208 reporting; a comment above the member; a comment inside the signature; a comment behind the arrow (FMT-UNSUP-012's reason); a lambda's and a switch arm's `=>`; a conditional group around the whole member (formatted and fixed, as change 12's alternative shows).

## Internal choices

- SHARED-BODY-001 states the boundary by cause (a conditional directive in front of `=>` does not by itself block the report; a directive of the group in the span from `=>` through `;` does, under FMT-STRUCT-007), with the two observed cases as explanation, rather than listing only the two cases the skeptic named. Alternative: list only those two cases; it would leave a group wholly inside the arrow's leading trivia unspecified. Matches `BlocksRewrite`, whose spans start at the arrow's `SpanStart` (region: its full-span start).
- FMT-UNSUP-013's *Why* names the reporting rules without requirement IDs, so the formatter spec's `Uses` row stays free of analyzer requirements.
- Change doc *Why* gains a separate bullet for the accessor instead of folding it into the conversion bullet.
- Change 12 combines the skeptic's `d1`, `d2`, and `d4` in one input, with the per-member locations stated beside the file's location.

## Follow-up candidates

_None_ new. The skeptic's accessor-arrow candidate (lead decision 5) stands.
