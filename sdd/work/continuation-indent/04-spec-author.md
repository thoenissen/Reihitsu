Role: spec author
Commit: a89c0dd
Verdict: #953: change doc written (with output questions); #1852: change doc written; #1896: change doc written; #1818: change doc written

Amendment round after the maintainer's answers (batches 1, 1b, 1c and the consolidated final answers in `00-lead.md`). This handover supersedes `03-spec-author.md`; 01 and 03 stay the record of how the first drafts were established. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

Every change doc of the run has `Status: Accepted`, as the lead asked. Seven cases the answers do not name are listed under *Output questions*; the specs are written on the recommended or the existing reading for each, and say so where it matters.

Checked: every new or changed example's *before* through `scripts/format.sh` at a89c0dd; every *after* that only joins or moves lines by formatting the joined input with the same script (the remaining layout matches today's output); the RH5302, RH5305, RH5308, and RH7101 fixes through `scripts/apply-fix.sh` (LF and CRLF); the FMT-UNSUP-017 and FMT-UNSUP-018 examples parse. `scripts/test.sh --project architecture`: 197 passed, 1 failed — `EveryRequirementOfAnEnforcedSpecIsCovered` for FMT-INDENT-034, FMT-INDENT-035, FMT-BREAK-041, FMT-UNSUP-017, FMT-UNSUP-018 (no tests yet; expected). The Uses and requirement-ID checks pass.

## Change docs

- `sdd/changes/2026-11/continuation-indent.md` — rewritten for the answers: what remains a continuation line, the parenthesized pattern's content (1e A), the line-start column with a new example; Accepted.
- `sdd/changes/2026-11/continuation-join.md` — **new**: the ten joins (1b A), the pattern-chain layout (1c A), the declaration of comments and directives in the joined gaps (1d A), with *Special cases*; Accepted.
- `sdd/changes/2026-11/omitted-type-argument-comment-unsupported.md` — **new**: the declaration of 4b A, with *Special cases*; Accepted.
- `sdd/changes/2026-11/expression-body-join-across-blank-line.md` — decision recorded as the maintainer's (2A), now depends on `continuation-join.md`; Accepted.

## thoenissen/Reihitsu#953

### Changed requirements

- FMT-BREAK-041 — A continuation behind a keyword, a pattern's `is`, an arm, a base list, a modifier, a filter, a loop header, a lambda, `with`, or a member name joins its line — added (1b A). Exception: the opening delimiter of a multi-line property or parenthesized pattern keeps its line below `is`.
- FMT-BREAK-017 — Binary operators start the continuation line — changed: a combinator (`and`, `or`) at the end of a line moves to the start of the next and its right pattern joins it (1c A); a comment behind a combinator keeps it at the line end (FMT-BREAK-001); new example.
- FMT-BREAK-022 — clarified: the opening parenthesis of a wrapped parenthesized pattern starts its own line (today's behavior, observed with `scripts/format.sh` in an `is` expression and a `case` label).
- FMT-UNSUP-017 — Comments and directives in a gap the line-break phase joins are unsupported — added (1d A); one requirement, a table of the ten gaps with a reason each; overlaps with FMT-UNSUP-004/010 give their reason.
- FMT-UNSUP-018 — Comments and directives inside a list of omitted type arguments are unsupported — added (4b A).
- FMT-UNSUP-009 — clarified: `typeof(Dictionary<, // c⏎>)` is refused by FMT-UNSUP-018.
- FMT-UNSUP-010 — other-side example: in a build that defines `X`, `#endif` between `public` and `int` is refused by FMT-UNSUP-017.
- FMT-UNSUP-011, FMT-UNSUP-012, FMT-UNSUP-013 — clarified: the pattern `is`, the lambda `=>`, and the switch-arm `=>` gaps are FMT-UNSUP-017's; other-side examples changed (`M3() /* c7 */ is 5`, `() => (/* c5 */ 1)`, `1 => "one", // c6`).
- FMT-INDENT-015 — changed: the omitted-type-argument exception is deleted; such a list never reaches the phase wrapped.
- FMT-INDENT-026 — changed (1e A): the pattern inside a multi-line parenthesized pattern is indented four columns right of `(`, whatever its kind; new example.
- FMT-INDENT-034 — changed: examples now show only lines no join removes (`x += // c⏎3`, values inside an anonymous-object member and a switch arm, declarators, the verbatim hole, the RH5302/RH5308 fix with `(x += // c⏎3) > 0`, the negative example); the *Construct it continues* example is `y += // c⏎3 + 4`; a paragraph states which lines remain.
- FMT-PIPE-024 — unchanged from 03 (line-start column kept; lead decision row 1).
- FMT-STRUCT-006 — `Why:` names the `return` join of FMT-BREAK-041.
- SHARED-BLANK-008 — example changed to the `]` behind an omitted array size (the omitted-type-argument input is now refused); Uses drop FMT-INDENT-015.
- RH2001-007, RH7101-007 — clarified: the shape lists name FMT-UNSUP-017 (and FMT-UNSUP-018 for RH7101); the fix is withheld for such fields.
- Unchanged from 03 and kept: FMT-INDENT-002, FMT-INDENT-013, FMT-DOC-003, FMT-SPACE-001, FMT-STRUCT-007, FMT-STRUCT-021, FMT-BLANK-020, FMT-PIPE-017, SHARED-BODY-001, SHARED-USING-009, RH5111-004.

### Examples for the tests

FMT-BREAK-041 (all three examples), FMT-BREAK-017 (pattern-chain example), FMT-UNSUP-017 and FMT-UNSUP-018 (refused, directive, other side; reason and location per row of the table), FMT-INDENT-026 (new example), FMT-INDENT-034 (all examples; the RH5302 example through `scripts/apply-fix.sh RH5302` **and** the CLI, both `3) > 0` in column 12, and the RH5308 sentence the same way), SHARED-BLANK-008 (array rank), the changed other sides of FMT-UNSUP-010 to 013, RH2001-007 (`= new /* three */ Counter();` withholds the fix), RH7101-007.

Tests that pin today's behavior and change: `UnsupportedListSeparatorCommentTests` row "Comment behind the comma of an omitted type argument" (now refused under FMT-UNSUP-018), `ZeroWidthTokenLineStartTests` (omitted list), `ExpressionBodyNextLineExpressionTests.BlankLineBeforeExpressionKeepsExpressionOnOwnLine`, the other-side tests of FMT-UNSUP-012 and FMT-UNSUP-013 (lambda and switch-arm comments), and the tests 02-skeptic.md lists for column 8.

### Output questions

Cases the answers do not name. The specs are written on the reading marked *(spec as written)*.

**1 · Formatter — line breaks the maintainer's list does not name: join or indent?**

Input (also today's output on `main`):

```csharp
using System;
using System.Collections.Generic;

class C
{
    private static
    readonly int _g = 1;

    public
    event Action E;

    IEnumerable<int> M(int x, object o, int[] a)
    {
        foreach (var y in
        a)
        {
        }

        var c = (int)
        x;
        var r = o switch
                {
                    int i
        when i > 0 => 1,
                    _ => 0,
                };
        var w = new
                {
                    A =
        1
                };

        yield return
        x;
    }
}
```

After, A *(spec as written)* — each line is a continuation line, one level deeper than the line of its construct:

```csharp
    private static
        readonly int _g = 1;

    public
        event Action E;
...
        foreach (var y in
            a)
...
        var c = (int)
            x;
...
                    int i
                        when i > 0 => 1,
...
                    A =
                        1
...
        yield return
            x;
```

After, B — each gap is joined like the gaps of 1b A, and a comment or a directive in it is declared unsupported like 1d A:

```csharp
    private static readonly int _g = 1;

    public event Action E;
...
        foreach (var y in a)
...
        var c = (int)x;
...
                    int i when i > 0 => 1,
...
                    A = 1
...
        yield return x;
```

Why B: these gaps are the same kind as the joined ones (a keyword or a modifier and what it governs, an arm's guard, a member's value), and joining them leaves fewer lines that need a column. A keeps the run's scope to the gaps the maintainer named. Answerable per row.

**2 · Formatter — a pattern chain wrapped at only some combinators (FMT-BREAK-017)**

Input / Before (`main`):

```csharp
        var k = x is 1 or 2 or
        3;
```

After, A (recommended, *spec as written*) — only the combinator at the line end moves, as for binary operators:

```csharp
        var k = x is 1 or 2
                  or 3;
```

After, B — once the chain wraps, every combinator starts its line:

```csharp
        var k = x is 1
                  or 2
                  or 3;
```

Why A: the formatter never wraps what the author wrote on one line, for binary operators as for combinators.

**3 · Formatter — a comment behind a combinator (FMT-BREAK-017)**

Input:

```csharp
        var k = x is 1 or // one or two
            2;
```

Before (`main`): `2;` in column 8.

After, A *(spec as written)* — the combinator stays at the line end (the join is refused), and `2;` is a continuation line in column 12.

After, B (recommended) — declared unsupported, like a comment behind a binary operator: the file is unchanged, and the CLI reports `error: <file>(…): unsupported syntax: a comment or directive between a pattern combinator and its right pattern. The file was left unchanged.` Declared in this run.

Why B: it is the pattern counterpart of FMT-UNSUP-011 and keeps the new combinator join free of a refusal; the comment belongs above the statement. Special case it would delete: the join refusal in the combinator move.

**4 · Formatter — a directive inside a list of omitted type arguments (FMT-UNSUP-018)**

Input:

```csharp
        var t = typeof(System.Collections.Generic.Dictionary<,
#if X
#endif
            >);
```

Before (`main`): `>);` in column 8.

After, A (recommended, *spec as written*) — refused like a comment: the file is unchanged, and the CLI reports `error: <file>(…): unsupported syntax: a comment or directive inside a list of omitted type arguments. The file was left unchanged.` Declared in this run.

After, B — only comments are refused; `>);` becomes a continuation line in column 12, and the indentation phase keeps its case for a wrapped list of omitted type arguments.

Why A: the maintainer's answer deletes the FMT-INDENT-015 exception, which only the directive case would still need.

**5 · Formatter — the column of a line left inside an arm or an anonymous-object member (FMT-INDENT-034)**

The answers replaced question 1 with joins and named no column for the lines that remain inside such a construct.

Input / Before (`main`):

```csharp
        var w = new
                {
                    A = x += // c
        3
                };
```

After, A (recommended, *spec as written*) — from the line of the construct (`A`, column 20): `3` in column 24.

After, B — from the statement's line: `3` in column 12, left of the braces.

Why A: B puts the line left of the construct it belongs to.

**6 · CLI — the reasons of FMT-UNSUP-017 (confirmation)**

One reason per gap, naming the place, such as `a comment or directive between is and its pattern` and `a comment or directive in front of the : of a base list`; the ten texts are in FMT-UNSUP-017's table and in `continuation-join.md`. Recommendation: keep; one generic reason for all ten would not tell the author where to look.

**7 · Formatter — `is` followed by a multi-line parenthesized pattern (FMT-BREAK-041, confirmation)**

Input:

```csharp
        return o is (
            int or long
        );
```

After, A (recommended, *spec as written*) — `(` keeps its own line below `is`, as on `main`; only the content moves (1e A):

```csharp
        return o is
                 (
                     int or long
                 );
```

After, B — the `is` join wins, `(` stays behind `is`, the content four columns right of `(`, and `)` under `is`.

Why A: the maintainer answered 1e on that layout, and the property pattern's brace keeps its own line the same way.

### Internal choices

- One FMT-UNSUP requirement for the ten gaps with a reason per gap; alternatives: ten requirements, or one requirement with one reason (question 6 asks for the texts).
- FMT-BREAK-017 and FMT-INDENT-026 are amended rather than given new IDs: the combinator move is the operator rule applied to patterns, and the parenthesized content uses the anchor FMT-INDENT-026 already names. FMT-INDENT-019 already gives the combinators' columns and is not changed.
- The gap lists are scoped to what the answers name: `await` of an await expression only (not `await foreach`/`await using`); `new` of an object or array creation only (not an anonymous object, `new()`, or `new[]`); the modifier gap of members, local functions, and local declarations (not parameters, not two modifiers); the lambda gap for an expression body only (a block body's brace is not a gap 1b joins).
- The anonymous-object member's `=` is joined, because 1d declares it and a declared gap is one 1b joins.
- The line-start column of FMT-PIPE-024 stays (lead decision row 1); its example changes to `(x += // c⏎3) > 0 &&⏎y`, which reproduces the fix/CLI divergence on `main` (CLI 8, RH5302 and RH5308 fixes 16).
- FMT-UNSUP-017's modifier gap depends on the build where the modifier itself is conditional (FMT-UNSUP-010's other side, `#if X⏎public⏎#endif⏎int _single;`); written as such rather than reading disabled text.
- Code owners: the new join's rewriter does not exist yet, so `line-breaks.md` and `sdd/specs/README.md` name no new owner; the implementer adds it.
- `AGENTS.md`'s phase table (the PrecheckPhase shape list, the LineBreakPhase row) does not list FMT-UNSUP-017, FMT-UNSUP-018, or the joins; it is not a spec, so it is left for the lead to route.

### Follow-up candidates

- Comments on #1634, #1905, #1542 (their "Before" output changes under FMT-INDENT-034), as in 03. #1633 (a base list whose `:` starts its line): the `:` now joins the type's line (FMT-BREAK-041), so its case no longer exists; comment there instead.
- Dropped: a wrapped `for` header aligned under `(` (02-skeptic.md) — `for` headers are joined now.
- A parenthesized pattern in a `case` label: `case (⏎int⏎or long⏎):` gives `case` alone on its line with `(` on the next (FMT-BREAK-022's own-line rule, observed on `main`), which looks wrong. Not this run's issues.

## thoenissen/Reihitsu#1852

### Changed requirements

- FMT-STRUCT-006 — as in 03 (join across a blank line, 2A), `Why:` names FMT-BREAK-041.
- The `return` value on its own line is joined (FMT-BREAK-041), not indented (Q5 → 1b A); a comment behind `return` is refused (FMT-UNSUP-017).

### Examples for the tests

FMT-STRUCT-006 (`Third`), FMT-BREAK-041 (`return⏎⏎x`), and the RH3202, RH3205, RH3206, RH3208 fixes of `int Get() =>⏎⏎2;` (`return 2;`).

### Output questions

_None._

### Internal choices

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1896

### Changed requirements

As in 03 (FMT-INDENT-035 added, FMT-INDENT-013 changed); answer 3A.

### Examples for the tests

FMT-INDENT-035.

### Output questions

_None._

### Internal choices

As in 01 (`into` clauses belong to the outer query).

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1818

### Changed requirements

FMT-INDENT-034's declarator example, unchanged (answer 6A).

### Examples for the tests

FMT-INDENT-034's declarator example, plus the skeptic's FC-020 members (`#else`/`#elif` branches, a declarator behind `#endif` first after the type).

### Output questions

_None._

### Internal choices

_None._

### Follow-up candidates

_None._

## Files

- `sdd/specs/formatter/line-breaks.md` (FMT-BREAK-017, FMT-BREAK-022, FMT-BREAK-041, Uses)
- `sdd/specs/formatter/unsupported-syntax.md` (FMT-UNSUP-009 to 013, FMT-UNSUP-017, FMT-UNSUP-018, Uses)
- `sdd/specs/formatter/indentation.md` (FMT-INDENT-015, FMT-INDENT-026, FMT-INDENT-034, Uses)
- `sdd/specs/formatter/structural-transforms.md` (FMT-STRUCT-006, Uses)
- `sdd/specs/shared/blank-line-spacing.md` (SHARED-BLANK-008, Uses)
- `sdd/specs/analyzer/RH2001.md`, `sdd/specs/analyzer/RH7101.md` (shape lists, Uses)
- `sdd/specs/requirement-ids.txt` (FMT-BREAK-041, FMT-UNSUP-017, FMT-UNSUP-018)
- `sdd/changes/2026-11/continuation-indent.md`, `continuation-join.md`, `omitted-type-argument-comment-unsupported.md`, `expression-body-join-across-blank-line.md`
