Role: test implementer
Commit: 5230122
Verdict: tests written

The run is *specs and tests for existing behavior*, widened to tooling. Every test here covers behavior that exists today and passes on the unchanged code. The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the spec text and its examples (`git diff origin/main -- sdd/specs`) and from the maintainer's answers in `squad/decisions/2026-10-23-1415-*.md`.

## Tests

New tests are marked (new). The others are existing tests that only gained or lost a tag.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH3103…Tests.ShorthandIsAskedForOnlyWhereTheGrammarAdmitsIt` (new) | SHARED-DIAG-011 example 1 (both sides in one source), RH3103-002 | `Verify(source, fixedSource)` | pass |
| `RH3103…Tests.NullableGenericInNameofIsNotReported` | + SHARED-DIAG-011 (not-reported side) | `Verify(source)` | pass |
| `RH3103…Tests.NullableGenericInTypeofIsReportedAndFixed` | + SHARED-DIAG-011 (reported side) | `Verify(source, fixedSource)` | pass |
| `RH3103…Tests.DottedNullableGenericAsNameofArgumentDoesNotOfferFix`, `…GlobalDotted…` | − SHARED-FIX-003; RH3103-006 kept | `VerifyNoCodeFixIsOffered` | pass |
| `RH3003…Tests.StringEmptyIsAskedForOnlyWhereNoConstantIsRequired` (new) | SHARED-DIAG-011 example 2 (both sides), RH3003-002 | `Verify(source, fixedSource)` | pass |
| `RH3003…Tests.EmptyStringInConstFieldIsNotReported`, `…EmptyLiteralsWhereAConstantIsRequiredAreNotReported` | + SHARED-DIAG-011 | `Verify(source)` | pass |
| `RH2004…Tests.VerifyStaticConstructorAndExplicitInterfaceImplementationsAreNotReported` | + SHARED-DIAG-011 example 3 (modifier not allowed) | `Verify(source)` | pass |
| `RH5412…Tests.VerifySpecificationExampleIsNotReportedUnderCSharp11` | + SHARED-DIAG-011 example 4, C# 11 side | `Verify` + language-version transform | pass |
| `RH5412…Tests.VerifyEmptyClassIsReportedUnderCSharp12` | + SHARED-DIAG-011 example 4, C# 12 side | `Verify` + language-version transform | pass |
| `RH3001…Tests.NegationAsAWholeIsPatternIsReportedWithoutACodeFix` | + SHARED-DIAG-011 example 5 (can be written, reported); + SHARED-FIX-003 (its "still reported" sentence) | `VerifyNoCodeFixIsOffered` | pass |
| `RH4106…Tests.NoFixIsOfferedWhenTheNewNameIsTaken` | + SHARED-DIAG-011 (a collision does not rule the change out; the RH4106 pointer) | `Verify(source, source)` | pass |
| `RH7103…Tests.FixCarriesADocumentationCommentBehindCodeWithTheMovedDeclaration` (LF, CRLF) | + SHARED-FIX-003 (fifth condition, offered side: the comment still documents `Create`) | `ApplyCodeFixAsync` (mode `Parse`) + syntax assertions | pass |
| `RH7103…Tests.FixCarriesADocumentationCommentBehindCodeAndAConditionalPairWithTheMovedDeclaration` (new; LF, CRLF) | SHARED-DOC-004 (`#if DEBUG`/`#endif` example), SHARED-FIX-003 fifth condition (offered side), SHARED-ORDER-006 | `Verify(source)` for the report; `ApplyCodeFixAsync` (mode `Parse`) + syntax assertions | pass |
| `RH7103…Tests.FixLeavesACommentBehindCodeWhereItStandsWhenDocumentationCommentsAreNotParsed` (new; LF, CRLF) | SHARED-DOC-004 mode-`None` example, SHARED-ORDER-002; FC-033 | `Verify(source, fixedSource, ApplyDocumentationModeNoneToTestProject)` with the exact result | pass |
| `RH5406…Tests.FixWritesLineFeedForTheLineBreakItKeepsInsideTheWrappedBodyInADocumentWhoseLineBreaksAreLoneCarriageReturns` (new) | SHARED-FIX-008 ("keeps" in the changed span, lone `\r`); FC-017, FC-033 | `ApplyCodeFixAsync` on input built in the test | pass |

Notes on the new tests:

- **Conditional-pair test.** Mode `Parse` is the `CSharpParseOptions` default that `ApplyCodeFixAsync` uses; the result is reparsed with `DocumentationMode.Parse`. The test asserts these facts:
  - `Create` comes before `Run`.
  - `Create`'s leading trivia holds, in this order, the documentation comment, `#if DEBUG` and `#endif`.
  - The summary text is `Creates`.
  - No comment and no directive is left in `Run`.
  - Each of the three appears exactly once.

  Where the comment starts after the move is not asserted (SHARED-DOC-004 leaves it uncovered).
- **Mode-`None` test.** It compares the whole result. The result has no blank line between `Create` and `Run`. I derived that from SHARED-ORDER-002 (separators stay where they were, and the input has none). My first draft guessed a blank line and failed on exactly that line, which I corrected from the requirement. The test is falsifiable on the harness dimension (FC-033): without the mode-`None` transform the same test fails, because the fix then carries the comment and rewrites it as a three-line `<summary>` above `Create`. I ran that mutation and then reverted it.
- **RH5406 test.** It checks that the input has no `\n`. It then asserts the exact substring `"            M(a,\n              b);"`. The break between `M(a,` and `b);` is kept from the input and lies inside the body the fix wraps, and it is `\n`. This is the candidate the spec author asked me to verify, and it conforms today. The break behind the new `}` is not asserted (#1388; decision `2026-10-23-1415-conforming-examples.md`).

## Not covered, and why

- **SHARED-FIX-003 fifth condition, withheld side.** No conforming input exists today: RH7309 has no comment guard, and the RH7101 split keeps the comment on the first field. Both are follow-ups (#1262; the RH7101 candidate in `03-spec-author.md`). I added no `#pragma` fixture, because the fix is withheld there for the directive reason and the tag would be incidental (spec author and skeptic).
- **SHARED-DOC-004 "a fix that cannot carry it MUST NOT be offered".** Its example is the RH7309 region case, which does not conform today (#1262). A `#pragma` between the comment and `Create` is withheld by the ordering-move guard, not because the comment cannot travel, so it does not exercise this clause.
- **SHARED-DIAG-011, the shapes that do not conform:**
  - dotted `nameof`, `Nullable<int>.Equals`, `typeof(Nullable<>)`, `nameof(Nullable<int>.HasValue)`, the C# 11 alias and `using static System.Nullable<int>;` belong to #1417;
  - RH3104 `new S();` has no tracker yet;
  - RH5417 inside an interpolation hole under C# 10 has no tracker yet.

  Per decision Q4 (b), I wrote no test that asserts the future behavior.
- **SHARED-FIX-008, the break behind the new `}` in lone-`\r` RH5406, and the RH7309 text swap.** Neither conforms today (#1388 and the untracked RH7309 candidate). The "outside the span" clause (Q3 (b)) says the requirement does not cover such a break, so there is nothing to assert for it.
- **Why-only changes to RH3103-002, RH3003-002, RH2004-003 and RH5412-001.** Their existing tests stay tagged and unchanged. The requirement behavior did not change.
- **Failure classes:**
  - Covered: FC-003 (both sides of each SHARED-DIAG-011 clause; mode `Parse` and mode `None`), FC-017 (exact substring and full-text assertions, LF and CRLF), FC-033 (lone `\r` built in the test; the mode-`None` transform proven necessary by mutation), FC-004/FC-005/FC-020 (a directive pair in the moved trivia), FC-007 (the verifier's iterative fix check in the mode-`None` test).
  - Not applicable: FC-008 Fix All (the class's existing `GetFixAllScenario` covers it, and the run adds no co-occurring shape), FC-010–FC-014 (no formatter change), FC-018/FC-036 (no rename), FC-022–FC-029 (no new rewrite shape).
- **No formatter-side test.** No formatter phase or `FormatterTestsBase` parity is involved (skeptic: Surfaces).

## Open findings

None. Every assertion derived from the spec held on the unchanged code.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests.FixLeaves|FullyQualifiedName~RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests.FixCarries|FullyQualifiedName~RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzerTests.FixWrites|FullyQualifiedName~ShorthandIsAskedForOnlyWhereTheGrammarAdmitsIt|FullyQualifiedName~StringEmptyIsAskedForOnlyWhereNoConstantIsRequired"
scripts/test.sh --project analyzer
scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests|FullyQualifiedName~RequirementCoverageTests"
```

## Whole run

- `scripts/test.sh --project analyzer`: 6050 passed, 0 failed.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests|FullyQualifiedName~RequirementCoverageTests"`: 39 passed, 0 failed. `EveryRequirementOfAnEnforcedSpecIsCovered` now passes for SHARED-DIAG-011.
- `scripts/format.sh` over the eight changed test files: the second run changed 0 files.
- `scripts/lane-check.sh tooling --head worktree`: confirmed, claimed tooling, diff needs tooling.
- No production code was touched.
