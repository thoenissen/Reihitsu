Role: test implementer
Commit: fbb9214
Verdict: tests written

Lane: specs and tests for existing behavior (widened to tooling). Every test below is a characterization test: it must pass on the unchanged code, and it does. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

All new tests are diagnostics-only, as the spec examples are: SHARED-DIAG-003 covers only what is reported, and the fix side belongs to `shared/code-fixes.md` (decision `squad/decisions/2026-10-18-shared-diag-003-literal-value.md`). Expected values come from the spec examples in `sdd/specs/shared/diagnostics.md` at `fbb9214`, copied verbatim. Spans use `{|#n:…|}` markup with the rule's message (the repository convention), which is stricter than the spec's `{|RH####:…|}` because it also checks the message. Each test sits in its rule's analyzer test class, next to the rule's own tests.

## Tests

New tests (all under `src/Reihitsu.Analyzer.Test/`):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Documentation/RH8101ElementParametersMustBeDocumentedAnalyzerTests.VerifyVerbatimParameterIsMatchedToItsDocumentationByItsValue` | SHARED-DIAG-002 example 1; FC-023 | `Verify(source, Diagnostics)` | pass: only `@event` reported |
| `Design/RH2003NotImplementedExceptionShouldNotBeUsedAnalyzerTests.VerifyTypeNameWrittenWithVerbatimPrefixOrInnerTriviaIsReportedLikeThePlainName` | SHARED-DIAG-002 example 2; FC-023 | `Verify(source, Diagnostics × 3)` | pass: all three type names reported |
| `Design/RH2003…VerifyOtherTypeNameWrittenWithVerbatimPrefixOrInnerTriviaIsNotReported` | SHARED-DIAG-002, other side of the comparison (FC-003): `@ArgumentException`, `System . ArgumentException`, and `System./* c */ArgumentException` are not reported | `Verify(source)` | pass |
| `Formatting/RH6002CommasMustBeSpacedCorrectlyAnalyzerTests.VerifyCommasInHoleAndBetweenLiteralsAreReportedButNotInLiteralOrCommentText` | SHARED-DIAG-003 RH6002 example; FC-022 (`"// c"`) | `Verify(source, Diagnostics × 2)` | pass |
| `Formatting/RH6002…VerifyCommasInDocumentationCommentCrefsAreNotReported` | SHARED-DIAG-003, skeptic candidate: `cref="M(int,int)"` and `cref="M(int ,int)"` in `///` are comment text | `Verify(source)` | pass |
| `Formatting/RH6003SemicolonsMustBeSpacedCorrectlyAnalyzerTests.VerifySpaceAfterLiteralIsReportedButNotInsideLiteralText` | SHARED-DIAG-003 RH6003 example | `Verify(source, Diagnostics)` | pass |
| `Formatting/RH5601UseTabsCorrectlyAnalyzerTests.VerifyTabsInLiteralAndCommentTextAreNotReportedButTabsInHoleCodeAre` | SHARED-DIAG-003 RH5601 example (each literal kind, format clause, each comment kind, hole expression, alignment clause); FC-002, FC-003 | `Verify(source, Diagnostics × 2)`; `→` replaced by a real tab | pass |
| `Formatting/RH5601…VerifyTabsInLiteralAndCommentTextAreNotReportedButTabsInHoleCodeAreWithCarriageReturnLineFeed` | same, CRLF | `NormalizeToCarriageReturnLineFeed` | pass |
| `Formatting/RH5023CodeMustNotContainMultipleBlankLinesInARowAnalyzerTests.VerifyEmptyLinesInsideRawStringAndBlockCommentAreNotReported` | SHARED-DIAG-003 RH5023 example | `Verify(source)` | pass |
| `Formatting/RH5023…VerifyEmptyLinesInsideRawStringAndBlockCommentAreNotReportedWithCarriageReturnLineFeed` | same, CRLF | `NormalizeToCarriageReturnLineFeed` | pass |
| `Clarity/RH3201CommentsMustContainTextAnalyzerTests.EmptyCommentIsReportedBecauseCommentTextIsTheRuleSubject` | SHARED-DIAG-003 exemption, comment-subject side | `Verify(source, Diagnostics)` | pass |
| `Formatting/RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzerTests.VerifyMisalignedRawStringIsReportedBecauseTheLiteralIsTheRuleSubject` | SHARED-DIAG-003 exemption, literal-subject side (misaligned reported, aligned not) | `Verify(source, Diagnostics)` | pass |
| `Formatting/RH5202…VerifyMisalignedRawStringIsReportedBecauseTheLiteralIsTheRuleSubjectWithCarriageReturnLineFeed` | same, CRLF | `NormalizeToCarriageReturnLineFeed` | pass |

Existing tests now also tagged `[Requirement("SHARED-DIAG-003")]`, because each one directly asserts a statement of the requirement (only the attribute was added):

- RH5601, tab in literal or comment text not reported:
  - `VerifyNoDiagnosticsForTabInMultiLineRawStringLiteral`, `…SingleLineRawStringLiteral`, `…Utf8StringLiteral`, `…Utf8RawStringLiteral`;
  - `…SingleLineComment`, `…MultiLineComment`, `…SingleLineDocumentationComment`, `…MultiLineDocumentationComment`;
  - `VerifyNoDiagnosticsForTabInRegularVerbatimInterpolatedAndCharacterLiterals`.
  - `VerifyNoDiagnosticsForTabInDisabledText` is not tagged: inactive code is a non-goal of the spec.
- RH5023, lines inside a literal or comment are not blank lines: `VerifyRawStringsDoNotProduceDiagnostics`, `VerifyVerbatimStringsDoNotProduceDiagnostics`, `VerifyMultiLineCommentBlankLinesDoNotProduceDiagnostics`, `VerifyLinesOfCommentsDisabledCodeAndStringsAreNotCounted`, `VerifyBlankLinesInsideInterpolatedStringsAreNotReported`.
- RH6002, an alignment clause is code: `VerifySpaceAfterInterpolationAlignmentCommaIsDetectedAndFixed`, `VerifySpaceBeforeInterpolationAlignmentCommaIsStillDetectedAndFixed` (this is the skeptic's `$"{1 ,5}"` candidate).

Results:

- `TestCategory=SHARED-DIAG-002|TestCategory=SHARED-DIAG-003` in the analyzer project: 29 passed (13 new, 16 tagged).
- The eight touched classes in full: 126 passed.
- Architecture `RequirementCoverageTests`, `SpecConsistencyTests`, and `SpecStructureTests`: 39 passed. `EveryRequirementOfAnEnforcedSpecIsCovered` was the one failure in `04-spec-author.md`, and it now passes.

How each test can fail (static reasoning on the analyzers; no production code was touched or mutated):

- RH8101: a comparison by spelling (`Identifier.Text`) would no longer match `@class` to `name="class"` and would add a diagnostic.
- RH2003: the pre-filter compares the right-hand identifier by `ValueText`. With `Identifier.Text`, `@NotImplementedException` drops out of the report. With a whole-name text comparison, the two trivia-split names drop out.
- RH5601, RH6002, RH6003, RH5023, RH3201, RH5202: every marked span is an expected diagnostic, and every unmarked literal, comment, or cref is an expected absence. A report inside content, or a missing report on code, fails the verifier's exact diagnostic-set comparison. `04-spec-author.md` also confirmed by mutation that removing the RH5202 markup or one RH5601 marker fails.

## Scenarios (bug fix)

_N/A._ This run specifies existing behavior; there is no reproduction.

## Not covered, and why

- The issue's own examples (`class @class`, `namespace A . B`, `"a";···`, `$"{new int[]{1}.Length}"`, the empty line inside a raw string) and the using-rule case: they break the requirements today and are tracked in #1071, #1074, #1075, and #1105. Per `00-lead.md` and `04-spec-author.md`, no test for them in this run.
- SHARED-DIAG-002 "derives other text from it, such as a file name": the only such rule, RH4001, breaks the requirement (#1071). No passing example exists.
- SHARED-DIAG-002, qualified names with `global::`, type arguments, or `@` on an inner part: the spec takes no stance on `global::` (#1105 territory). I added no example beyond the spec.
- Skeptic's RH5602 candidates (trailing spaces inside `// abc···` and inside the first line of `/* a···⏎b */`): not tested. These spaces sit at the end of the comment, and the spec's non-goal "Whether a rule reports whitespace next to a comment — the spec of that rule" makes that boundary RH5602's own spec's call, not SHARED-DIAG-003's. RH5602 is also among the rules tracked in #1074/#1075.
- Skeptic's RH3003, RH3204, RH3202, and RH5202 fix-side candidates: their fixes respell a literal. The fix side is out of SHARED-DIAG-003 (decision `2026-10-18-shared-diag-003-literal-value.md`) and belongs to SHARED-FIX-003 (#1068). RH3003 and RH3204 report the whole literal or the whole string, so nothing inside the text is reported. The requirement does not reach them.
- RH5601's carve-outs (a tab next to a literal is not reported; a tab in a comment on a directive line is reported): these conflict with the "whitespace before and after a literal is code" bullet. They are tracked in #1071/#1074/#1075 per `04-spec-author.md`, so I wrote no test.
- CRLF variants for RH8101, RH2003, RH6002, RH6003, and RH3201: each asserted span lies on a single line, and none of these analyzers reads line breaks for these inputs, so no line-ending dimension applies. RH5601, RH5023, and RH5202 do read line structure, and each has a CRLF variant.
- Failure classes:
  - Applied: FC-002 and FC-003 (each qualifier and both sides of the exemption have a test), FC-017 (exact diagnostic sets, CRLF variants), FC-022, FC-023, FC-033 (line endings).
  - Not applicable, because the run changes no production code and no fix or formatter path is under test: FC-001, FC-004 to FC-016, FC-018 to FC-021, FC-024 to FC-032, FC-034 to FC-037. FC-035 does not apply: the expected values are the spec's own examples.
- Formatter parity (`FormatterTestsBase`): not involved. The requirements state what an analyzer reports, and `03-skeptic.md` marks the formatter as not involved.

## Commands

```shell
scripts/test.sh --project analyzer --filter "TestCategory=SHARED-DIAG-002|TestCategory=SHARED-DIAG-003"
scripts/test.sh --project architecture --filter "FullyQualifiedName~RequirementCoverageTests|FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"
```
