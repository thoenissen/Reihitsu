Role: spec author
Commit: e46b94c
Verdict: #953: change doc written; #1852: change doc written; #1896: change doc written; #1818: change doc written

Amendment round. This handover supersedes `01-spec-author.md` for the output questions and the changed-requirement list; 01 stays the record of how the first draft was established (probe over specs, rule pages, and sources). The prompt carried no intended fix, suspected cause, or hoped-for verdict.

Carried out: the skeptic's findings for the spec author in `02-skeptic.md` (*Whole run*), as decided in `00-lead.md` rows 1 and 2:

1. Finding 1 (FC-028): new line-start-column fact in FMT-PIPE-024; new bullet and RH5302/RH5308 example in FMT-INDENT-034.
2. Finding 2: SHARED-ATTR-022 reverted to `main` (`[B]` in column 8); the RH5514 bullet and SHARED-ATTR-022 removed from `continuation-indent.md`.
3. `for` and `foreach` headers (with `throw`, `yield return`, `await`, cast, `new` named) added to the change doc's visible changes.
4. Q2's *Why* corrected, and the same wrong claim corrected where it was also written: `expression-body-join-across-blank-line.md` (What, Why) and FMT-STRUCT-006's `Why:`.
5. The pattern-chain staircase added to Q1 and to the change doc's visible changes.

Checked: every new "before" with `scripts/format.sh` and `scripts/apply-fix.sh RH5302` at e46b94c (RH5302 fix today: `not null` and `&& x > 0;` both in column 16; CLI: `not null` 8, `&&` 16; staircase `1 or`/`2 or`/`3;` all 8; `for`/`foreach` continuation lines 8; `void B() =>⏎⏎System.Console.WriteLine();` gives the statement on the line after `{` with no blank line). `scripts/test.sh --project architecture`: 197 passed, 1 failed — `EveryRequirementOfAnEnforcedSpecIsCovered` for FMT-INDENT-034 and FMT-INDENT-035 (no test yet; expected, the test implementer adds them). The Uses checks pass.

## thoenissen/Reihitsu#953

### Changed requirements

As in 01, with these differences:

- FMT-INDENT-034 — changed in this round: new bullet **A root formatted on its own** (a continuation whose construct starts on a node-level root's first line measures from the line-start column; `FormatNode` measures from the root's first token); `Why:` states how the column stays entry-point independent; new example: the RH5302 fix and whole-document formatting both give `not null` column 12, with the RH5308 sentence beside it. Uses of `formatter/indentation.md` gain FMT-PIPE-023, FMT-PIPE-024.
- FMT-PIPE-024 — Node-level input facts are plain values — changed: new fact **Line-start column** (the document column of the first token or comment on the line of the root's first token, counted as FMT-PIPE-017 counts; given by `FormatNodeInDocumentAsync` for the target and by `FormatNodeInDocumentWithContextAsync` for the context; none for `FormatNode`); the indentation-phase bullet reads it; "No fact describes the code in front of the root" now says "beyond the column at which its line begins", with the reason that it is a document fact, not a column a phase predicts; `Why:` names it.
- SHARED-ATTR-022 — no longer changed (reverted to `main`; the RH5514 split fix calls no formatter, SHARED-ATTR-017).
- Change doc `continuation-indent.md`: Affected requirements drop SHARED-ATTR-022 and add FMT-PIPE-024; *What* gains the line-start paragraph; *Visible output changes* gain "A pattern chain whose first operand starts a line" and "Wrapped `for` and `foreach` headers"; *Code fixes* drop RH5514 and gain RH5302/RH5308; *Decisions* gain "A node formatted on its own measures from where the document begins its line".

### Examples for the tests

As in 01, plus: the FMT-INDENT-034 RH5302 example through `scripts/apply-fix.sh RH5302` **and** the CLI (both `not null` 12, `&& x > 0;` 16), and the RH5308 input `var s = o is⏎not null ? 1 :⏎2;` through the fix and the CLI (`not null` 12, `? 1`/`: 2;` 20). Drop from 01: the SHARED-ATTR-022 (RH5514) example — its existing test keeps column 8. The staircase and the `for`/`foreach` headers are in the change doc only; the skeptic's table lists them as test inputs.

### Output questions

**1 · Formatter — where a continuation inside a construct is measured from (FMT-INDENT-034)**

The maintainer's decision (#921 Q1) says "one level deeper than the line on which its statement or member starts"; its examples all have the construct starting on the statement's line. Two readings differ where it does not, and they differ in both directions.

Input (also today's output on `main`):

```csharp
class C
{
    int M(int x, object o)
    {
        var w = new
                {
                    A // c
        = 1
                };
        var d = o is
                  (
        int or long
                  );
        var k = x is
        1 or
        2 or
        3;

        return x switch
               {
                   1
        => 2,
                   _ => 0,
               };
    }
}
```

Before (`main`): as the input.

After, A (recommended) — from the line of the nearest construct that starts on an earlier line:

```csharp
                    A // c
                        = 1
...
                  (
                      int or long
                  );
        var k = x is
            1 or
                2 or
                3;
...
                   1
                       => 2,
```

After, B — from the statement's line:

```csharp
                    A // c
            = 1
...
                  (
            int or long
                  );
        var k = x is
            1 or
            2 or
            3;
...
                   1
            => 2,
```

Why A: B leaves a line left of the construct it belongs to, outside its own braces or parentheses; the counter-example is the pattern chain, where A gives a staircase (`1 or 2` starts on the line of `1`, so `2` and `3` continue it) and B reads better, but that shape needs a line break behind `is` that people rarely write, and `case 1 or⏎2 or⏎3:` has no staircase under A. A curried lambda `x =>⏎(y) =>⏎y` staircases the same way under A, which there matches the nesting. The spec is written on A.

**2 · Formatter — expression body converted across a blank line (FMT-STRUCT-006, #1852)**

Input:

```csharp
int Get() =>

    2;
```

Before (`main`): `int Get()⏎{⏎    return⏎    2;⏎}` (value in the column of `return`).

After, A (recommended): `int Get()⏎{⏎    return 2;⏎}`.

After, B: `int Get()⏎{⏎    return⏎        2;⏎}` (the break stays; the value is a continuation line).

Why A: the blank line carries no meaning, and for a body converted to an expression statement it already disappears on `main` (`void B() =>⏎⏎Console.WriteLine();` gives the statement on the line after `{`, with no blank line). Same for the RH3202/RH3205/RH3206/RH3208 fixes. The spec is written on A.

**3 · Formatter — query clauses outside a list element (FMT-INDENT-035, #1896)**

Unchanged from 01.

Input / Before (`main`):

```csharp
var q = from x in arr
let y = x
select y;
```

After, A (recommended): clauses under `from`.

```csharp
var q = from x in arr
        let y = x
        select y;
```

After, B: the general continuation rule (one level deeper than the statement).

```csharp
var q = from x in arr
    let y = x
    select y;
```

After, C: unchanged.

Why A: FMT-INDENT-013 already aligns the clauses under `from` inside a list element. The spec is written on A.

**4 · Formatter — a wrapped list of omitted type arguments (FMT-INDENT-015)**

Unchanged from 01; the shape (omitted type arguments kept wrapped by a comment) is rare.

Input / Before (`main`):

```csharp
        var type = typeof(System.Collections.Generic.Dictionary<, // c
        >);
```

After, A (recommended): `>);` one level deeper (column 12), as any continuation line.

After, B: unchanged; FMT-INDENT-015 keeps its explicit block-column clause as an exception to FMT-INDENT-034.

Why A: FMT-INDENT-015's reason (no column to align to) is exactly the general rule's case, and an exception would be a special case. The spec is written on A.

**Confirmations (applications of the decided rule, #921 Q1, #918 Q5, #913 Q2)** — the lead's batch can show them as such: a `return` value on the next line (#1852), a declarator behind `#endif` (#1818), wrapped `for`/`foreach` headers (now in the change doc), and the RH5302/RH5308 fix results, which follow whole-document formatting because RH5302-004 and RH5308-004 already require "as the formatter formats it in place".

### Internal choices

- As in 01.
- Finding 1: the line-start column is given by both node-in-document entry points (target / context), not only `FormatNodeInDocumentAsync`; alternative: only where the skeptic found a failing caller. Chosen because it is one value with one definition, and it equals the first-token column wherever the root starts its line, so no current result of `FormatNodeInDocumentWithContextAsync` changes.
- Finding 1: measuring from the root's first token rejected (contradicts RH5302-004/RH5308-004 and gives 20 where the CLI gives 12).
- RH5302 and RH5308 specs are not edited: their requirements already decide the output; the example sits in FMT-INDENT-034.

### Follow-up candidates

- As in 01 (comments on #1633, #1634, #1905, #1542).
- From the skeptic: a wrapped `for` header aligned under the token behind `(`. Input `for (var i = 0;⏎i < 3;⏎i++)`; before (`main`): `i < 3;` and `i++)` in column 8; after A (this run): column 12; after B: column 13, under `var`. Not this run's.

## thoenissen/Reihitsu#1852

### Changed requirements

As in 01. FMT-STRUCT-006's `Why:` corrected in this round: "for a body converted to an expression statement the blank line disappears as well" replaces the claim that such a body joins across it. `expression-body-join-across-blank-line.md` *What* and *Why* corrected the same way.

### Examples for the tests

As in 01. The test `ExpressionBodyNextLineExpressionTests.BlankLineBeforeExpressionKeepsExpressionOnOwnLine` pins today's other side and changes under Q2 A (skeptic).

### Output questions

Question 2 above.

### Internal choices

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1896

### Changed requirements

As in 01 (FMT-INDENT-035 added, FMT-INDENT-013 changed).

### Examples for the tests

As in 01.

### Output questions

Question 3 above.

### Internal choices

As in 01 (`into` clauses belong to the outer query).

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1818

### Changed requirements

As in 01 (FMT-INDENT-034 declarator example).

### Examples for the tests

As in 01, plus the skeptic's FC-020 members (`#else`/`#elif` branches, a declarator behind `#endif` first after the type).

### Output questions

None of its own; a confirmation (see question block above).

### Internal choices

_None._

### Follow-up candidates

_None._

## Files

- `sdd/specs/formatter/indentation.md` (FMT-INDENT-034, Uses)
- `sdd/specs/formatter/pipeline.md` (FMT-PIPE-024)
- `sdd/specs/formatter/structural-transforms.md` (FMT-STRUCT-006 `Why:`)
- `sdd/specs/shared/attribute-targets.md` (reverted to `main`)
- `sdd/changes/2026-11/continuation-indent.md`
- `sdd/changes/2026-11/expression-body-join-across-blank-line.md`
