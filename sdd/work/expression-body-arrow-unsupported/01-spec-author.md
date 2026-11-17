Role: spec author
Commit: ceb8e16
Verdict: change doc written

Inputs: thoenissen/Reihitsu#1851 with both comments (both restate that the maintainer decided to declare the shape in a change of its own), `00-lead.md`, the preceding change `sdd/changes/2026-11/expression-body-arrow-comment-unsupported.md` (FMT-UNSUP-012). My prompt carried no intended fix or suspected cause.

Change doc: `sdd/changes/2026-11/expression-body-arrow-leading-comment-unsupported.md` (`Status: Draft`), with *Visible output changes* 1–11, *Decisions*, *Special cases* (*Deleted*, *Checked and kept*), and *Alternatives considered*.

## Changed requirements

- FMT-UNSUP-013 — Comments and directives between the arrow of an expression body and the code in front of it are unsupported — added (`sdd/specs/formatter/unsupported-syntax.md`, ledger updated)
- FMT-UNSUP-012 — the "Behind the `=>`" condition now says the gap in front of the `=>` is FMT-UNSUP-013's; its "other side" example loses `int A /* c2 */ => 1;` and `int B()⏎// c3⏎=> 1;` (renumbered c1–c6) — changed
- FMT-STRUCT-006 — no comment or directive in the arrow's leading trivia to carry; the paragraph on own-line comments in front of the arrow, the "directly at the arrow" definition, and the second example are removed — changed
- FMT-STRUCT-007 — the clause "or in the arrow's leading trivia" for a region directive is removed; a directive in front of the arrow is refused (FMT-UNSUP-013) — changed
- FMT-STRUCT-015 — seams "around `get`" / "in front of the getter's arrow" become "in front of `get`"; the gap between `get` and the arrow is refused (FMT-UNSUP-013) — changed
- FMT-STRUCT-026 — wording: "a comment that FMT-STRUCT-006 carries in front of the converted statement's semicolon" — changed
- SHARED-BODY-001 — names a `#region`/`#endregion` in the leading trivia of `=>` as not reported (moved from FMT-STRUCT-007), and a comment between the token in front of `=>` and `=>` as reported with the fix withheld — changed
- SHARED-BODY-003 — a comment, directive, or disabled text in front of `=>` withholds the fix — changed
- RH5401-004 — withheld for a wrapped expression (format-and-compare), and for a comment/directive/disabled text in front of or behind the `=>` through the precheck; still reported (SHARED-DIAG-010) — changed; `Uses` gains FMT-UNSUP-013, SHARED-DIAG-010
- RH7101-007 — FMT-UNSUP-013 added to the list of shapes that withhold the fix — changed
- `AGENTS.md` — the `PrecheckPhase` row names the seventh shape (FMT-UNSUP-013)

No rule page changes (withheld fixes only; `general-notes.md` covers them). No code-owner changes.

Checks run:
- `scripts/format.sh` on `main` for every change-doc input (scratch fixtures): every *Before* block in the change doc is the real output. Also checked: a conditional group around a whole property is formatted (spec claim in FMT-UNSUP-013's *Why*), and `int F(/* c */ int a) => a;` is formatted.
- `scripts/apply-fix.sh` on `main`: RH3202, RH3203, RH3208, RH5110, RH7101 are `fixed` on the change-doc inputs (results quoted in changes 10–11); RH5401 is `no-fix-offered` on `int B // note⏎=> 1;` and `int D⏎/* note */ => 1;`.
- `scripts/test.sh --project architecture`: 197 passed, 1 failed — `EveryRequirementOfAnEnforcedSpecIsCovered`: `FMT-UNSUP-013: no test covers it`. Expected until the test implementer adds the covering tests; every spec-structure and consistency test passes.
- Refusal locations in the change doc and FMT-UNSUP-013's examples were counted by hand from the inputs (first comment/directive in the gap, 1-based line and column).

## Examples for the tests

- FMT-UNSUP-013: its four conditions (each with both sides), the node-level sentence (arrow clause alone does not hold the shape), the FMT-UNSUP-010 overlap, the reason text, and the three examples (refused set at `(5,11)`, directive at `(4,1)` in every build, the supported side).
- FMT-UNSUP-012: the changed supported-side example.
- FMT-STRUCT-006, FMT-STRUCT-007, FMT-STRUCT-015: the cases they no longer specify are refused now.
- SHARED-BODY-001: a member with `#region` in the leading trivia of `=>` is not reported (now stated here, no longer through FMT-STRUCT-007); a member with a comment in front of `=>` is reported and gets no fix.
- SHARED-BODY-003, RH5401-004, RH7101-007: fix withheld for the shape, diagnostic kept.
- Change doc *Visible output changes* 1–11: CLI and code-fix inputs with their before/after.

Fact for the test implementer: test sources whose inputs hold a comment or directive in front of an expression-body arrow (found by a multiline `rg` for a comment or directive directly in front of `=>`; some hits are lambdas, which stay supported): `Idempotency/IdempotencyTests.cs` (`LeadingArrowComment`), `Regression/LineBreaks/AccessorListAccessorLineBreakTests.cs` (`set /* c */ => _x = value;`), `Regression/LineBreaks/CommentJoinTests.cs`, `Unit/LineBreaks/JoinSeparatorPhaseTests.cs` (`int P // p⏎=> 1;`), `Regression/Structural/ExpressionBodiedConstructorTests.cs`, `ExpressionBodiedIndexerTests.cs`, `ExpressionBodiedMethodTests.cs`, `ExpressionBodyArrowCommentTests.cs`, `GetOnlyMemberExpressionBodyTests.cs`, `StructuralTransformSeparatorFinalOutputTests.cs`, `Unit/StructuralTransforms/StructuralTransformSeparatorTests.cs`, `Unit/Precheck/UnsupportedExpressionBodyArrowCommentTests.cs` (FMT-UNSUP-012's supported side), and in the analyzer tests `RH5040…AnalyzerTests.cs`, `RH5401…AnalyzerTests.cs`. Lambda-only hits: `StrandedDocumentationLineStartTests.cs`, `Spacing/ArrowSpacingTests.cs`.

## Output questions

The declaration itself is the maintainer's answer to escalation 10 (#1851); the change doc's *Visible output changes* 1–11 all follow from the span the issue defines (trailing trivia of the token in front of `=>` plus the leading trivia of `=>`; comments, directives, disabled text; every expression-body owner). Two points are still open:

**1 · RH5401 · Expression-style get-only properties should be single-lined — comment in front of the arrow**

Input:
```csharp
class C
{
    int P // c
        => 1;
}
```

Before (`main`): reported on the whole property; no fix offered.

After, A (recommended): reported on the whole property; no fix offered (unchanged).

After, B: not reported.

Why A: a shape the formatter refuses as unsupported is not one it keeps, so the shared rule for layout rules asks for the report, and the conflict the issue describes disappears with the declaration itself.

Note for the lead: the issue recommends B, believing it removes a SHARED-DIAG-010 conflict; SHARED-DIAG-010's last sentence already settles refused shapes the other way, so A needs no spec exception and B would need one. A changes nothing visible; the lead may judge it decided and not ask.

**2 · CLI · reason reported for the new unsupported shape**

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

After, A (recommended):
```csharp
class C
{
    int P // c
        => 1;
}
```
The CLI reports: `error: <file>(3,11): unsupported syntax: a comment or directive between the arrow of an expression body and the code in front of it. The file was left unchanged.`
The code fixes that format this property are no longer offered on it (here RH5401's, which was not offered before either).
Declared in this work.

After, B: the same, with the issue's wording `a comment or directive between an expression body's arrow and the code in front of it`.

Why A: it matches the reason of the sibling shape behind the arrow, "between the arrow of an expression body and its expression", so the two read as a pair.
Special cases the shape deletes: the line-break phase's refusal to put a property's arrow on the property's line, the conversion's move of comments in front of the arrow into the new block, and the get-only property lift's checks of the trivia behind `get` and in front of the getter's arrow.

Drafted only in case the lead judges the one-line case not covered by the maintainer's answer (the issue's span includes it):

**3 · Formatter · block comment in front of the arrow on one line**

Input:
```csharp
class C
{
    int A /* c */ => 1;

    int M() /* c */ => 1;
}
```

Before (`main`):
```csharp
class C
{
    int A /* c */ => 1;

    int M() /* c */
    {
        return 1;
    }
}
```

After, A (recommended): input unchanged; the CLI reports `error: <file>(3,11): unsupported syntax: a comment or directive between the arrow of an expression body and the code in front of it. The file was left unchanged.`; the code fixes that format these members (RH3202 among them) are no longer offered on them. Declared in this work.

After, B: as before (`main`): only a gap with a line break is refused.

Why A: the get-only property lift deletes `get`, so its check of a comment behind `get` goes only if `get /* c */ => _v;` on one line is refused too, and the boundary then matches the other operator shapes.

## Internal choices

- FMT-UNSUP-013 is a requirement of its own rather than a widening of FMT-UNSUP-012 (alternative: one requirement for both gaps, which would change FMT-UNSUP-012's shipped reason text).
- A node-level input holds the shape only when it holds both the `=>` and the token in front of it; the arrow clause alone does not (alternative: count the arrow's leading trivia alone). No caller formats an arrow clause alone.
- The region-in-arrow-leading-trivia case moves from FMT-STRUCT-007 to SHARED-BODY-001 (the analyzers still decide by `BlocksRewrite`), mirroring how the FMT-UNSUP-012 change moved the directive-behind-arrow case.
- SHARED-BODY-001 states only what I verified in `BlocksRewrite`: comments never block the report, and a region directive in the arrow's leading trivia does. It does not claim how other directives in front of `=>` are reported (their outcome depends on where the group's partner lies).
- FMT-BREAK-026 and FMT-BREAK-001 are not edited: FMT-BREAK-001 is generic and FMT-BREAK-026 names no comment case.
- `AGENTS.md` precheck row updated with the seventh shape.
- No rule page changes.

## Follow-up candidates

_None._
