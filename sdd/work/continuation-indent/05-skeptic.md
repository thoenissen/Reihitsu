Role: skeptic
Commit: f5ebbd8
Verdict: #953: output question (with spec amendments); #1852: proceed; #1896: proceed (one row for 04's question 1); #1818: proceed

This handover checks the amended spec diff (be4ccf4) against the maintainer's answers in `00-lead.md`, `04-spec-author.md`, the four change docs under `sdd/changes/2026-11/`, and the code on `main`. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

Fixtures were run with `scripts/format.sh` (CLI, whole document) in `/tmp/claude-0/-home-user-Reihitsu/067a7317-225f-510e-8f9b-6cda6989da93/scratchpad/sk5/`. The tree holds `main`'s code (the branch changes only specs and `sdd/work`). Columns are 0-based.

## thoenissen/Reihitsu#953

### Premise

The premise was settled in `02-skeptic.md` and still holds. A caller reaches every new mechanism through whole-document formatting (CLI) and through the code fixes that format a node holding a gap (RH5302, RH5305, RH5308, RH7101, RH2001, RH3202 family). This handover covers what the answers newly brought in: the ten joins (FMT-BREAK-041), the combinator move (FMT-BREAK-017), the two new shapes (FMT-UNSUP-017, FMT-UNSUP-018), and FMT-INDENT-026.

Spot checks of the spec author's *before* outputs (main):

- `class E // derived⏎    : B⏎{⏎}` gives `class E // derived⏎: B;` (`:` in column 0, empty body turned into `;`). This matches the change doc.
- `record Foo(⏎int A,⏎int B)⏎: Base(A);` gives `: Base(A);` in column 0. This is the #1633 case, which the base-list join removes.
- Pre-joined layouts are already sound on `main`, so each join only removes lines. Checked: `x is 1⏎or 2⏎or 3` (`or` in column 18, under `is`), `case 1⏎or 2:` (`or` under `1`), arm `1⏎or 3 => 4` (`or` under `1`), `x is 1⏎or 2⏎and 3` (`and` also in column 18), `M(x is 1⏎or 2, …)` (`or` under `is`), `{ P: 1⏎or 2 }` (`or` under `1`), `Func<int,int> f = x => x > 0⏎? 1⏎: 2`, `x => new⏎{`, `x => x switch⏎{`, `return b⏎&& c⏎? 1⏎: 2`, `return [⏎1,⏎2⏎]`, `o is [⏎1,⏎2⏎]`.

### Mechanism and candidates

_N/A_ (behavior change). The candidates were enumerated from the syntax types each join keys on, not from the answer's wording. Each finding below names its defining type.

### Breaking inputs

**Finding 1 — the `is` of a type test splits one visual shape in two (output question 1 below).** FMT-BREAK-041 and FMT-UNSUP-017 key the `is` gap on `IsPatternExpressionSyntax`. Roslyn parses `o is string` and `o is Color.Red` as `BinaryExpressionSyntax` (`IsExpression`), and FMT-BREAK-017 moves that `is` to the start of the next line. Observed on `main` (`IsType.cs`):

```csharp
// input
        var a = o is
        string;
        var b = o is
        string s;
```

`main` gives `var a = o⏎                is string;` (`is` in column 16), and `string s;` stays in column 8. Under the spec as written, `b` becomes `var b = o is string s;` while `a` keeps `o⏎is string`. The reader cannot see which parse applies. The other side, `as`, has no pattern counterpart and is not in question.

**Finding 2 — FMT-BREAK-017's new "right pattern MUST join it" contradicts FMT-BREAK-022 (spec gap, internal).** The amended sentence has no exception for a right operand whose opening delimiter FMT-BREAK-022 puts on its own line. FMT-BREAK-041 has that exception for the `is` gap. FMT-BREAK-017 does not. Observed on `main` (`CombProp.cs`):

```csharp
// input
        var a = s is null or
        {
        Length: 2
        };
        var c = o is int or
        (
        long
        or short
        );
```

`main` gives `or` alone on its line and `{` / `(` on the next, both in column 18:

```csharp
        var a = s is null
                  or
                  {
                      Length: 2
                  };
```

Read literally, the amended FMT-BREAK-017 demands `or {` and `or (`. FMT-BREAK-022, which is unchanged, demands that `{` (property pattern, not in a `case` label) and `(` (parenthesized pattern) start their own line. The unchanged requirement decides, so `main`'s output stays and no output is in question. The spec author adds the same exception FMT-BREAK-041 carries, referring to FMT-BREAK-022 rather than restating its condition. A test pins `main`'s output for `s is null or⏎{…}` and `o is int or⏎(…)`; none found.

**Finding 3 — the combinator join's refusal leaves a line left of the chain (evidence for 04's question 3).** Under 04's question 3 A (no declaration), the combinator with the comment stays at the line end. The line behind it takes FMT-INDENT-034's column, measured from the line where the whole chain `(1 or 2) or 3` starts:

```csharp
// input (main gives 3; in column 8)
        var k = x is
        1 or
        2 or // c
        3;
```

Under A:

```csharp
        var k = x is 1
                  or 2 or // c
            3;
```

`3;` in column 12 stands left of `or 2` in column 18. FMT-INDENT-034 was written to remove exactly that layout. The lead should show this output in question 3. It strengthens the recommendation for B.

**Finding 4 — modifier gaps that are not "modifier and type" (output question 2 below).** Candidates enumerated from `MemberDeclarationSyntax`: field, property, indexer, method, operator, and local function take a type behind their modifiers and are joined. Type declarations (`class`, `struct`, `interface`, `record`, `enum`), `DelegateDeclarationSyntax` (`delegate`), `ConstructorDeclarationSyntax` (a name), `ConversionOperatorDeclarationSyntax` (`implicit`/`explicit`), and event fields and events (`event`) take no type directly behind the last modifier. They are not joined. Under FMT-INDENT-034 their next token becomes a continuation line. Observed today (`Modifiers.cs`): `public⏎class N`, `public⏎C()`, `public static⏎implicit operator`, and `public⏎delegate void D()` all keep column 4. Under the spec as written they move to column 8, while `public⏎void M()` joins. 04's question 1 lists only `private static⏎readonly` and `public⏎event`.

**Finding 5 — the spec author's "internal choices" on gap scope change visible output.** These were recorded as internal choices in `04-spec-author.md`, but each gives a different CLI output depending on the reading:

- `await⏎foreach` and `await⏎using`
- `new⏎[] { 1 }` and `new⏎()`
- a parameter's modifiers (`this⏎int x`)
- the gap between two modifiers

They belong in 04's question 1 as rows. `new⏎[]` also touches RH6022's spec example (`var e = new⏎[] { 1 };` is the "not reported" side, `sdd/specs/analyzer/RH6022.md:43`). That example stays valid for the analyzer either way.

**Finding 6 — the same family has two treatments.** FMT-BREAK-016 joins both sides of the `=` of declarators, properties, enum members, and parameters. FMT-BREAK-041 joins only the gap in front of an anonymous-object member's `=`. `A =⏎1` stays a continuation line (04's question 1 row). The query `let` clause has the same shape (`let y⏎= x` and `let y =⏎x`), and FMT-INDENT-035's own example shows `let y⏎    = x` as a continuation line. Add `let` as a row to question 1 (see #1896). If the answer is "join", the natural home is FMT-BREAK-016 (both sides, P34: one treatment per syntax family), not FMT-BREAK-041. That placement is internal.

**Finding 7 — FMT-BREAK-041 and FMT-UNSUP-017 word the keyword gap differently (internal).** FMT-BREAK-041 says "the `return` … and the first token of what follows it". FMT-UNSUP-017 says "a return statement with an expression", and FMT-BREAK-041 states "the gaps are exactly those FMT-UNSUP-017 names". `return⏎;` (kept as is by `main`) and `throw⏎;` are decided only by that cross-reference. Copy "with an expression" into FMT-BREAK-041.

**Finding 8 — the change doc's special-case accounting is inconsistent, and a kept guard is missing.**

- `continuation-join.md` lists "the join refusal in each of the ten joins" under *Deleted*. It also calls FMT-BREAK-001's refusal for the combinator join "the same check every other join of the phase uses, not a case of its own". Both statements cannot hold. Either the per-join refusal counts as a special case, in which case the combinator join keeps one and 04's question 3 B deletes it, or it does not, in which case the *Deleted* bullet deletes nothing and only `CreateReturnStatement` is deleted. Make the two sections agree. AGENTS.md requires that a declared shape delete something, and `CreateReturnStatement` does qualify, so the declaration stands either way.
- Missing under *Checked and kept*: `GetOnlyMemberExpressionBodyRewriteUtilities.HasLayoutOnlySeams` (`src/Reihitsu.Core/GetOnlyMemberExpressionBodyRewriteUtilities.cs:318-337`) checks `returnStatement.ReturnKeyword.TrailingTrivia` and `expression.GetFirstToken().LeadingTrivia`, which is exactly FMT-UNSUP-017's `return` gap. In the formatter's accessor and get-only transforms that check becomes unreachable. The RH3209/RH3210 code fix calls `TryConvertToExpressionBody` on unformatted input (`GetOnlyMemberExpressionBodyCodeFixProviderBase.cs:76`), so the check still serves another caller and is kept. FMT-STRUCT-012's "after `return`, before the returned expression" clause is in the same position. The `throw` handling of the same conversion (`ThrowOperandPrecedenceExpressionBodyTests.CommentAfterThrowKeywordStaysBeforeOpeningParenthesis`) is likewise unreachable through the formatter. List these, with the reason they are kept.

**Finding 9 — FMT-UNSUP-011's `Why:` becomes false.** It says "the pattern combinators … are laid out by other requirements, which hold no such refusal, so declaring them here would delete nothing". The amended FMT-BREAK-017 gives the combinator join exactly such a refusal. Reword it, whatever 04's question 3 answers.

**Finding 10 — directive idioms the declaration refuses (confirmation, output question 3 below).** FMT-UNSUP-017 includes directives, as answer 1d A says. These multi-targeting shapes are formatted on `main` and refused under the spec. Two fixtures were run.

`Idioms.cs`, run exactly as below. `main` formats it: `x;` moves to column 8, and `ToString(…) => f;` becomes a block.

```csharp
using System;

public partial class Foo
#if NET
    : IFormattable
#endif
{
#if NET
    public
#else
    internal
#endif
    static int Count(int x)
    {
        return
#if NET
            x + 1;
#else
            x;
#endif
    }

    public string ToString(string f, IFormatProvider p) => f;
}
```

`Modifiers.cs` holds `#if NET⏎    public⏎#else⏎    internal⏎#endif⏎    int P() => 1;`, and `main` converts it to a block.

- `return⏎#if …`: refused in every build (this is FMT-UNSUP-017's own example). This alone refuses `Idioms.cs`.
- Conditional accessibility on a member directly in front of the type (`Modifiers.cs`'s `P`): refused in every build, because the `#endif` or the disabled `internal` stands between the last modifier and the type. With another modifier behind the group (`Idioms.cs`'s `… #endif⏎static int Count`), the directive is between two modifiers and is not refused. On a type declaration it is not refused today, but it would be under question 2 B.
- A conditional base list (`class Foo⏎#if NET⏎: IFormattable⏎#endif`): refused only where `NET` is defined, such as code fixes in an IDE that defines it.

The refusal applies to the whole file. Every other formatting in such a file is lost, not only the gap.

Failure classes walked:

- FC-002: two-sided examples are present for every gap. Finding 1 is a missing side.
- FC-003: blank line in a gap, covered.
- FC-010/FC-017 (idempotency): no join makes a later anchor earlier, and the pre-joined probes above are fixed points on `main` for the shapes checked. The test implementer still runs two passes and CRLF.
- FC-013: no analyzer reports a joined layout. RH5302 covers `&&`/`||` only, and RH5049 and SHARED-COMMENT-007 exclude pattern operators. P11 cases are under *Follow-up candidates*.
- FC-020/FC-021: Finding 10.
- FC-027: nesting (joins inside arguments, arms, lambdas, `is` in arguments).
- FC-028: the joins happen inside node-level roots. Checked against the RH5302 and RH5305 examples in `continuation-join.md`. A root that starts behind a gap (RH5306's list pattern behind `is // note`) does not hold the shape (FMT-UNSUP-003), and its fix stays offered.

Candidate inputs for the test implementer (CLI unless stated):

| Input | Expected | Existing test |
|---|---|---|
| FMT-BREAK-041's three examples, FMT-BREAK-017's chain example, FMT-INDENT-026's new example | as the spec states | none found |
| `s is null or⏎{⏎Length: 2⏎}`, `o is int or⏎(⏎long⏎or short⏎)` | `main`'s output (Finding 2) | none found |
| `x is 1 or⏎2 and⏎3` | `1` / `or 2` / `and 3`, combinators in column 18 | none found |
| `M(x is 1 or⏎2, o)`, `o is { P: 1 or⏎2 }` | `or` under `is` (column 20) and under `1` (column 25) | none found |
| `o is⏎string` and `o is⏎string s` | per output question 1 | `UnsupportedBinaryOperatorCommentTests` pins the comment side of the type test |
| `return⏎⏎x`, `return⏎;`, `throw⏎;` | joined; `return⏎;` and `throw⏎;` as `main` (Finding 7) | none found |
| each of the ten refused gaps, one-line and multi-line, plus the directive example | refused with the gap's reason and location | none found |
| RH5306 fix of a list pattern behind `v is // note` | fix offered (root does not hold `is`) | `RH5306…Tests.VerifyFixLaysOutListPatternBehindTrailingCommentOfPrecedingToken` (+CRLF) pins it |
| RH3209 fix of `get { return /* c */ _value; }` | unchanged (guard kept, Finding 8) | `RH3209…AnalyzerTests` (line ~1284) pins it |

Existing full-pipeline tests whose input becomes refused (they change, and their requirement coverage must move elsewhere):

- `WithExpressionSpacingTests.CommentInFrontOfWithKeepsItsPlace`, and the `/** a */` variant near line 288 (FMT-SPACE-025, FMT-SPACE-013)
- `ThrowOperandPrecedenceExpressionBodyTests.CommentAfterThrowKeywordStaysBeforeOpeningParenthesis`, and the accessor variant near line 791
- `DelimitedDocumentationInExpressionTests.MovesDelimitedDocumentationBehindLambdaArrow`
- `ExpressionBodiedAccessorTests.CommentAfterReturnKeywordKeepsBlock` (FMT-STRUCT-012)

The phase unit test `SameLineGapRulePhaseTests.DocumentationCommentInFrontOfTheOperandOfAwaitKeepsItsPlace` runs the phase alone and keeps passing. No caller passes its input any more.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `PrecheckPhase` | must change (FMT-UNSUP-017, FMT-UNSUP-018) |
| `LineBreakPhase` (new join, `BinaryOperatorLineBreakRewriter`'s combinator move) | must change |
| `IndentationPhase` (FMT-INDENT-026 content, FMT-INDENT-034/035; `ListElementAligner` guard deleted) | must change |
| `StructuralTransformPhase` (`ExpressionBodyToBlockConverter.CreateReturnStatement`) | must change (#1852) |
| `Reihitsu.Core` `GetOnlyMemberExpressionBodyRewriteUtilities` | must stay (Finding 8) |
| Code fixes through the formatter | no code change; outputs change, and fixes are withheld for the new shapes via `FindUnsupportedSyntax` |
| Clarity fixes that do not call the formatter (RH3001, RH3002, RH3003, RH3005, RH3104, RH3105, whose tests hold `return /* x */`, `new /* x */`, `throw /* note */`) | not involved: no formatter call, so nothing is withheld |
| Analyzers | not involved (no conflict found) |
| Rule pages | not involved |
| CLI | output changes and new refusal reasons; no code change beyond the reasons |
| `AGENTS.md` phase table | must change: the PrecheckPhase shape list gets the 11th and 12th shapes; the LineBreakPhase row gets the joins, the combinator move, and the removal of blank lines in joined gaps (the lead routes it, as 04 notes) |

### Refuse option

The maintainer has already chosen to declare (1d A, 4b A). Refusing locally instead of joining is not an option, because the joins are answered.

- Declaring the combinator gap is 04's question 3, which I support (Finding 3).
- For the type-test `is`, declaring needs no new shape, because FMT-UNSUP-011 already refuses a comment there.
- For Finding 4's rows, joining them under B extends FMT-UNSUP-017's modifier gap. It refuses `#if X⏎public⏎#else⏎internal⏎#endif⏎class Foo`, a polyfill idiom, and deletes nothing beyond the generic refusal.

No simpler option found.

### Output questions

**1 · Formatter — `is` followed by a type on the next line (Finding 1)**

Input:

```csharp
        var a = o is
            string;
        var b = o is
            string s;
```

Before (`main`):

```csharp
        var a = o
                is string;
        var b = o is
        string s;
```

After, A (spec as written): `a` as on `main`; `var b = o is string s;`.

After, B (recommended):

```csharp
        var a = o is string;
        var b = o is string s;
```

Why B: the two read as the same construct, and the maintainer's answer joins `is` with what follows. A comment there is already refused (FMT-UNSUP-011).

**2 · Formatter — a line break behind the last modifier where no type follows (Finding 4; add as rows to 04's question 1)**

Input / Before (`main`, all lines in column 4):

```csharp
    public
    class N;

    public
    C()
    {
    }

    public static
    implicit operator int(C c) => 1;

    public
    delegate void D();
```

After, A (spec as written): `class N;`, `C()`, `implicit operator …`, and `delegate void D();` move to column 8, as continuation lines. Meanwhile `public⏎void M()` becomes `public void M()`.

After, B: each line joins, giving `public class N;`, `public C()`, `public static implicit operator int(C c)`, and `public delegate void D();`. A comment or directive behind the last modifier is then refused for these declarations too, which refuses `#if X⏎public⏎#else⏎internal⏎#endif⏎class Foo`.

Why B, with a caveat: one rule for "behind the last modifier" reads the same for every declaration. If the maintainer considers the conditional-accessibility idiom on types common, A.

Further rows for 04's question 1 (Findings 5 and 6), each "join (B) or continuation line (A)":

- `await⏎foreach (…)`
- `await⏎using (…)`
- `new⏎[] { 1 }`
- `new⏎()`
- `this⏎int x`
- `private static⏎readonly` (already listed)
- `public⏎event` (already listed)
- `let y⏎= x` and `let y =⏎x`
- `o is not⏎null` (`not` and its operand)

**3 · CLI — directive idioms the declaration refuses (confirmation of 1d A, Finding 10)**

Input: the `Idioms.cs` fixture above, and `Modifiers.cs`'s `P`.

Before (`main`): both formatted, including `=> f;` and `=> 1;` becoming blocks.

After, A (recommended, as answered): the file is unchanged, and the CLI reports `unsupported syntax: a comment or directive between return, throw, await, or new and what follows it`. In a build that defines `NET`, code fixes formatting `Foo` are also withheld for the base list.

After, B: directives are left out of FMT-UNSUP-017 (comments only). These lines stay where they are as continuation lines, and each join keeps FMT-BREAK-001's refusal.

Why A: it is the maintainer's answer, and the same choice written around whole statements and members is formatted. The confirmation exists because the 1d question did not show multi-targeting code. Special cases B would keep: the join refusal for directives in the ten gaps.

04's questions 2, 5, 6, and 7: agreed with the recommendations. Question 4: agreed with A. Question 3: recommend B, and show Finding 3's output as its A.

### Internal choices

- Finding 2: add a FMT-BREAK-022 exception to FMT-BREAK-017 (recommended).
- Finding 7: wording.
- Finding 8: change-doc accounting.
- Finding 9: FMT-UNSUP-011's `Why:`.
- Finding 6: if `let` and the anonymous `=` are joined on both sides, place them in FMT-BREAK-016 rather than FMT-BREAK-041.
- If 04's question 1 B or question 2 B is chosen, FMT-STRUCT-021's example (`private static⏎    readonly int _g`) changes too.

### Issue goals

No goal to drop beyond `02-skeptic.md`.

### Architecture findings

_None._ Finding 1 sits on a border between two rewriters: the binary-operator move owns the type-test `is`, and the new join owns the pattern `is`. Today it is an output question, not a disagreement between components. Finding 2's exception must refer to FMT-BREAK-022, not restate its condition, so the rule stays stated in one place.

### Follow-up candidates

- P11, new-rule candidates (not this run): a pattern-combinator counterpart of RH5302 (no rule reports `and`/`or` at a line end), and an anonymous-object member `=` counterpart of RH5111 (RH5111-001 lists declarators, properties, parameters, and enum members, not `NameEquals`). The other joined gaps are reported by no rule and are plain P11 cases.
- A multi-line positional pattern (`t is⏎(⏎1,⏎2⏎)`): after the `is` join, its elements and `)` become continuation lines in column 12 (`);` under the elements). FMT-BREAK-022 and FMT-INDENT do not cover positional patterns. This is a P34 candidate like the `for` header one in `02-skeptic.md`.
- `04-spec-author.md`'s `case (⏎…⏎):` candidate: agreed.

## thoenissen/Reihitsu#1852

### Premise

Unchanged from `02-skeptic.md`. The `return` value is now joined (FMT-BREAK-041), and a comment or directive behind `return` is refused (FMT-UNSUP-017). The directive form is #953's output question 3.

### Mechanism and candidates

_N/A._

### Breaking inputs

FMT-STRUCT-006's `Third` example. The RH3202, RH3205, RH3206, and RH3208 fixes of `int Get() =>⏎⏎2;`. `return⏎⏎x;` with whitespace on the blank line. `ExpressionBodyNextLineExpressionTests.BlankLineBeforeExpressionKeepsExpressionOnOwnLine` changes. `ExpressionBodiedAccessorTests.CommentAfterReturnKeywordKeepsBlock` becomes a refusal (#953, Finding 8).

### Surfaces

As in `02-skeptic.md`, plus `LineBreakPhase` (the `return` join).

### Refuse option

_None_ beyond #953.

### Output questions

_None_ (covered by #953's question 3 for the directive form).

### Internal choices

_None._

### Issue goals

None to drop.

### Architecture findings

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1896

### Premise

Unchanged. The amendment did not touch FMT-INDENT-035. Its interaction with the joins was read from the specs, not run: by FMT-BREAK-041 and FMT-INDENT-035, `return⏎from x in a⏎select x` joins to `return from …`, and the clauses align under `from` (column 15). The test implementer should include it.

### Mechanism and candidates

_N/A._

### Breaking inputs

As in `02-skeptic.md`.

### Surfaces

As in `02-skeptic.md`.

### Refuse option

_None._

### Output questions

One row for 04's question 1 (from #953, Finding 6): `let y⏎= x` and `let y =⏎x`. FMT-INDENT-035's example shows `let y⏎    = x` as a continuation line, while the answers join an anonymous-object member's `=`. If the answer is "join", FMT-INDENT-035's example changes.

### Internal choices

_None._

### Issue goals

None to drop.

### Architecture findings

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1818

### Premise

Unchanged. The issue's inputs hold no gap that FMT-UNSUP-017 refuses: the `#if` stands in front of a declarator, not behind a modifier. In event fields and locals, FMT-UNSUP-010 does not apply. FMT-INDENT-034's declarator example stands.

### Mechanism and candidates

_N/A._

### Breaking inputs

As in `02-skeptic.md` (FC-020 members: `#else`/`#elif` branches, a declarator behind `#endif` first after the type).

### Surfaces

Indentation phase only.

### Refuse option

_None._

### Output questions

_None._

### Internal choices

_None._

### Issue goals

None to drop.

### Architecture findings

_None._

### Follow-up candidates

_None._

## Whole run

- Amendments for the spec author before the batch:
  - Finding 2 (FMT-BREAK-017 exception)
  - Finding 7 (FMT-BREAK-041 wording)
  - Finding 8 (special-case accounting and the kept Core guard)
  - Finding 9 (FMT-UNSUP-011 `Why:`)
  - the `AGENTS.md` phase table, if the lead routes it there
- New output questions for batch 2: #953's questions 1 to 3, plus the added rows for 04's question 1.
- Only `scripts/format.sh` on scratch fixtures was run. No build or tests were run, and no repository file other than this handover changed.
