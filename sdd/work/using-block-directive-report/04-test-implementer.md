Role: test implementer
Commit: see the commit that adds this file (parent ad6167a)
Verdict: tests written

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

Every expected value comes from the spec diff (RH7201-001 to RH7207-001, -004, SHARED-USING-009) and the change doc's cases 1 to 6. None comes from running patched code. Run on head ad6167a with production code unchanged: 45 test rows fail. Each fails with `Mismatch between number of diagnostics returned, expected "N" actual "0"` (N = 1 or 2), or, for the Fix All test, `expected "2" actual "1"`, because the compilation-unit block is not reported. That is the right reason for every row. All other rows pass, 311 in the filter below, including every new not-reported and reported-and-fixed test.

Test helper added (test code only): `AnalyzerTestsBase<TAnalyzer, TCodeFix>.VerifyNoCodeFixIsOffered(string source, Action<CSharpAnalyzerVerifierTest<TAnalyzer>> onConfigure, params string[] preprocessorSymbols)`. It does what the existing overloads do, but takes preprocessor symbols and an analyzer configuration (here `CompilerDiagnostics.None`) together. The existing overloads take one or the other, and case 6 (`Newtonsoft.Json` under `NETFRAMEWORK`) needs both.

## Tests

Paths: `tests/Reihitsu.Analyzer.Test/Ordering/RH720{1..6}*AnalyzerTests.cs`, `tests/Reihitsu.Analyzer.Test/Formatting/RH7207*AnalyzerTests.cs`, `tests/Reihitsu.Analyzer.Test/Formatter/Formatting/RH7207*FormatterTests.cs`. "Flipped" = an existing test that asserted silence, renamed and rewritten to the new expectation.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH7201 `DisabledConditionalUsingBlockIsReportedWithoutCodeFix` (flipped) | RH7201-001, RH7201-004, SHARED-USING-009 | `VerifyNoCodeFixIsOffered` | fails (0 of 1) |
| RH7201 `BlockWithPreprocessorDirectiveIsReportedWithoutCodeFix` (flipped; spec example) | RH7201-001, RH7201-004, SHARED-USING-009 | `VerifyNoCodeFixIsOffered` + CompilerDiagnostics.None | fails |
| RH7201 `CompiledConditionalSystemDirectiveAtTheEndOfTheBlockIsReportedWithoutCodeFix` (change doc case 5, DEBUG) | RH7201-001, RH7201-004, SHARED-USING-009 | new overload, `DEBUG` | fails |
| RH7201 `BlockLedByADefineDirectiveIsReportedWithoutCodeFix` (`#define` kind) | RH7201-001, RH7201-004, SHARED-USING-009 | `VerifyNoCodeFixIsOffered` | fails |
| RH7202 `DisabledConditionalUsingBlockIsReportedWithoutCodeFix` (flipped) | RH7202-001, RH7202-004, SHARED-USING-009 | `VerifyNoCodeFixIsOffered` | fails |
| RH7202 `BlockWithPreprocessorDirectiveIsReportedWithoutCodeFix` (flipped; spec example) | RH7202-001, RH7202-004, SHARED-USING-009 | `VerifyNoCodeFixIsOffered` | fails |
| RH7202 `BlockWithRegionDirectiveIsReportedWithoutCodeFix` (case 3, `#region`) | RH7202-001, RH7202-004, SHARED-USING-009 | `VerifyNoCodeFixIsOffered` | fails |
| RH7203 `DisabledConditionalUsingBlockIsReportedWithoutCodeFix` (flipped) | RH7203-001, RH7203-004, SHARED-USING-009 | `VerifyNoCodeFixIsOffered` | fails |
| RH7203 `BlockWithPreprocessorDirectiveIsReportedWithoutCodeFix` (flipped; spec example, no DEBUG) | RH7203-001, RH7203-004, SHARED-USING-009 | `VerifyNoCodeFixIsOffered` | fails |
| RH7203 `BlockWithCompiledConditionalGroupIsReportedWithoutCodeFix` (spec example, DEBUG: two reports) | RH7203-001, RH7203-004, SHARED-USING-009 | new overload, `DEBUG` | fails (0 of 2) |
| RH7203 `BlockLedByANullableDirectiveIsReportedWithoutCodeFix` (SHARED-USING-009 example 2) | RH7203-001, RH7203-004, SHARED-USING-009 | `VerifyNoCodeFixIsOffered` | fails |
| RH7203 `BlockOfANamespaceWithPreprocessorDirectiveIsReportedWithoutCodeFix` (scope: block namespace) | RH7203-001, RH7203-004, SHARED-USING-009 | `VerifyNoCodeFixIsOffered` | fails |
| RH7203 `BlockOfAFileScopedNamespaceWithPreprocessorDirectiveIsReportedWithoutCodeFix` (scope: file-scoped, `#region`) | RH7203-001, RH7203-004, SHARED-USING-009 | `VerifyNoCodeFixIsOffered` | fails |
| RH7203 `GlobalSectionOfABlockWithPreprocessorDirectiveIsReportedWithoutCodeFix` (global + local section) | RH7203-001, RH7203-004, SHARED-USING-002, SHARED-USING-009 | `VerifyNoCodeFixIsOffered` | fails (0 of 2) |
| RH7203 `DirectiveAfterTheBlockDoesNotPreventReportAndFix` (SHARED-USING-009 example, LF + CRLF) | RH7203-001, RH7203-004, SHARED-USING-009 | `VerifyCodeFixWithEndOfLine` | passes |
| RH7203 `FixAllOrganizesOnlyTheBlockWithoutPreprocessorDirective` (FC-008/FC-009: unsafe compilation-unit block + safe namespace block) | RH7203-004, SHARED-USING-009 | `Verify(source, fixed, cfg)`, incremental and Fix All iterations = 1 | fails (1 of 2) |
| RH7203 `PragmaBetweenTokensOfDirectiveIsReportedWithoutCodeFix` (flipped; SHARED-USING-009 example / case 4, LF + CRLF) | RH7203-001, RH7203-004, SHARED-USING-009 | `VerifyNoCodeFixIsOffered` | fails |
| RH7203 `NameSplitByPragmaIsReportedOnTheWholeNameWithoutCodeFix` (location across a directive, LF + CRLF) | RH7203-001, RH7203-003, RH7203-004, SHARED-USING-009 | `VerifyNoCodeFixIsOffered` | fails |
| RH7203 `ConditionalGroupBetweenTokensOfDirectiveIsReportedWithoutCodeFix` (flipped; build without `A`, LF + CRLF) | RH7203-001, RH7203-004, SHARED-USING-009 | `VerifyNoCodeFixIsOffered` | fails |
| RH7203 `ConditionalGroupBetweenTokensOfDirectiveIsNotReportedWhenTheBuildCompilesAnOrderedBlock` (build with `A`) | RH7203-001, SHARED-USING-009 | `VerifyWithPreprocessorSymbols` | passes |
| RH7203 `ConditionalGroupAcrossTokensOfTwoDirectivesIsReportedWithoutCodeFix` (flipped; build without `A`: two reports) | RH7203-001, RH7203-004, SHARED-USING-009 | `VerifyNoCodeFixIsOffered` | fails (0 of 2) |
| RH7203 `ConditionalGroupAcrossTokensOfTwoDirectivesIsNotReportedWhenTheBuildCompilesAnOrderedBlock` (build with `A`) | RH7203-001, SHARED-USING-009 | `VerifyWithPreprocessorSymbols` | passes |
| RH7204 `DisabledConditionalUsingBlockIsReportedWithoutCodeFix` (flipped) | RH7204-001, RH7204-004, SHARED-USING-009 | `VerifyNoCodeFixIsOffered` | fails |
| RH7204 `BlockWithPreprocessorDirectiveIsReportedWithoutCodeFix` (flipped; spec example) | RH7204-001, RH7204-004, SHARED-USING-009 | `VerifyNoCodeFixIsOffered` | fails |
| RH7204 `BlockWithPragmaDirectiveIsReportedWithoutCodeFix` (case 3, `#pragma`) | RH7204-001, RH7204-004, SHARED-USING-009 | `VerifyNoCodeFixIsOffered` | fails |
| RH7205 `DisabledConditionalUsingBlockIsReportedWithoutCodeFix` (flipped) | RH7205-001, RH7205-004, SHARED-USING-009 | `VerifyNoCodeFixIsOffered` | fails |
| RH7205 `BlockWithPreprocessorDirectiveIsReportedWithoutCodeFix` (flipped; spec example) | RH7205-001, RH7205-004, SHARED-USING-009 | `VerifyNoCodeFixIsOffered` | fails |
| RH7205 `BlockWithRegionDirectiveIsReportedWithoutCodeFix` (case 3, `#region`) | RH7205-001, RH7205-004, SHARED-USING-009 | `VerifyNoCodeFixIsOffered` | fails |
| RH7205 `CompiledConditionalRegularDirectiveBelowAStaticDirectiveIsReportedWithoutCodeFix` (case 3 note, DEBUG) | RH7205-001, RH7205-004, SHARED-USING-009 | new overload, `DEBUG` | fails |
| RH7205 `ExcludedConditionalRegularDirectiveBelowAStaticDirectiveIsNotReported` (same block, no DEBUG; other side) | RH7205-001, SHARED-USING-009 | `Verify` | passes |
| RH7206 `DisabledConditionalUsingBlockIsReportedWithoutCodeFix` (flipped) | RH7206-001, RH7206-004, SHARED-USING-009 | `VerifyNoCodeFixIsOffered` | fails |
| RH7206 `BlockWithPreprocessorDirectiveIsReportedWithoutCodeFix` (flipped; spec example / case 3) | RH7206-001, RH7206-004, SHARED-USING-009 | `VerifyNoCodeFixIsOffered` | fails |
| RH7206 `CompiledConditionalStaticDirectiveIsReportedWithoutCodeFix` (DEBUG) | RH7206-001, RH7206-004, SHARED-USING-009 | new overload, `DEBUG` | fails |
| RH7207 `BlockWithRegionDirectiveInCanonicalOrderIsNotReportedForItsBlankLines` (renamed from `NoDiagnosticWhenRegionDirectivePreventsSafeReordering`; same input and assertion) | RH7207-001, SHARED-USING-009 | `Verify` | passes |
| RH7207 `DisabledConditionalUsingBlockIsReportedWithoutCodeFix` (flipped) | RH7207-001, RH7207-004, SHARED-USING-009 | `VerifyNoCodeFixIsOffered` | fails |
| RH7207 `BlockWithPreprocessorDirectiveIsReportedWithoutCodeFix` (flipped; spec example, case 1) | RH7207-001, RH7207-004, SHARED-USING-009 | `VerifyNoCodeFixIsOffered` | fails |
| RH7207 `BlockWithCompiledConditionalGroupIsReportedOnceWithoutCodeFix` (case 1, DEBUG) | RH7207-001, RH7207-003, RH7207-004, SHARED-USING-009 | new overload, `DEBUG` | fails |
| RH7207 `BlockWithPreprocessorDirectiveInCanonicalOrderIsNotReportedForAMissingBlankLine` (spec negative example) | RH7207-001, SHARED-USING-009 | `Verify` | passes |
| RH7207 `BlockWithCompiledConditionalGroupInCanonicalOrderIsNotReportedForAMissingBlankLine` (same, DEBUG) | RH7207-001, SHARED-USING-009 | `VerifyWithPreprocessorSymbols` | passes |
| RH7207 `BlockWithPreprocessorDirectiveInCanonicalOrderIsNotReportedForABlankLineInsideAGroup` | RH7207-001, SHARED-USING-009 | `Verify` | passes |
| RH7207 `BlockWithPreprocessorDirectiveOutOfOrderAndWithoutBlankLineIsReportedOnceWithoutCodeFix` (both aspects wrong, one diagnostic) | RH7207-001, RH7207-003, RH7207-004, SHARED-USING-009 | `VerifyNoCodeFixIsOffered` | fails |
| RH7207 `PragmaBetweenTokensOfDirectiveIsReportedWithoutCodeFix` (case 4, LF + CRLF) | RH7207-001, RH7207-004, SHARED-USING-009 | `VerifyNoCodeFixIsOffered` | fails |
| RH7207 `CompiledConditionalGroupAtTheEndOfTheBlockIsReportedWithoutCodeFix` (case 5 / skeptic case 1a, DEBUG; with and without the blank line) | RH7207-001, RH7207-004, SHARED-USING-009 | new overload, `DEBUG` | fails |
| RH7207 `ExcludedConditionalGroupAtTheEndOfTheBlockDoesNotPreventReportAndFix` (case 5, no DEBUG) | RH7207-001, RH7207-004, SHARED-USING-009 | `Verify(source, fixed)` | passes |
| RH7207 `ConditionalAlternativeOutOfOrderInTheCompiledBuildIsReportedWithoutCodeFix` (case 6, NETFRAMEWORK) | RH7207-001, RH7207-004, SHARED-USING-009 | new overload, `NETFRAMEWORK` | fails |
| RH7207 `ConditionalAlternativeInOrderInTheCompiledBuildIsNotReported` (case 6, other build) | RH7207-001, SHARED-USING-009 | `Verify` | passes |
| RH7207 `BlockLedByANullableDirectiveIsReportedWithoutCodeFix` (flipped; case 2) | RH7207-001, RH7207-004, SHARED-USING-009 | `VerifyNoCodeFixIsOffered` | fails |
| RH7207 formatter `FormatterKeepsAReportedBlockWithAConditionalDirective` (flipped from `FormatterLeavesABlockWithAConditionalDirectiveUnchanged`; FC-013) | SHARED-USING-009 | `VerifyFormatterKeepsReportedShape` (LF + CRLF) | fails |
| RH7207 formatter `FormatterKeepsAReportedBlockLedByANullableDirective` (flipped from `FormatterLeavesABlockLedByANullableDirectiveUnchanged`) | SHARED-USING-009 | `VerifyFormatterKeepsReportedShape` | fails |
| RH7207 formatter `FormattedBlankLinesAroundARegionInsideAGroupAreNotReported` (lead decision 1 / skeptic e.cs in order; FC-013) | SHARED-USING-009 | `VerifyFormatter(source, fixed)` (LF + CRLF, second pass) | passes |

## Scenarios (bug fix)

_N/A_ (behavior change).

## Not covered, and why

- **Existing tests that stay unchanged.** `NoCodeFixWhenUsingDirectivesCannotBeSafelyReordered` (RH7201) and `NoCodeFixWhenConditionalDirectiveWouldBeMoved` (RH7207) find their location by hand and stay valid. `DirectiveAfterTheBlockDoesNotPreventOrganizing` (RH7207) and `FormatterOrganizesABlockFollowedByAConditionalDirective` pin the "directive after the block" side for RH7207 and the formatter. All `HashLine…` / `CommentedOutConditionalDirectivesDoNotPreventReportAndFix` tests pin the comment-text side of SHARED-USING-009. `HashLineReadAsDirectiveByAnotherBuildIsReportedWithoutFix` holds no directive in the block's trivia in the compiled build. `DiagnosticWhenMissingBlankLineBetweenGroups` pins "the same block without a directive is reported for its blank line", the second half of the SHARED-USING-009 blank-line example.
- **Formatter keeps the block (FMT-USING-004).** Already pinned in `Reihitsu.Formatter.Test/Regression/UsingDirectives/UsingDirectiveRequirementTests.cs` / `UsingDirectiveOrderingTests.cs`. The spec does not change it, so no new formatter-only test.
- **`UsingDirectiveOrderingSafety` (Core).** Must stay as it is. `UsingDirectiveOrderingSafetyTests` is unchanged.
- **`#if false`, `#elif`.** Same disabled-text path as the excluded `#if FEATURE` tests (the excluded directive is absent from the syntax list), so they get no own test. Every directive kind the spec names has a test: `#if` excluded and compiled, `#if/#else`, `#nullable`, `#pragma` (block and between tokens), `#region`/`#endregion`, `#define`.
- **Directive in trailing trivia of a using.** A directive must start its own line, so it is always leading trivia of the next token. The parser cannot produce this position.
- **The RH7202/RH7204/RH7205/RH7206 lines of change doc case 3 where RH7207 reports too.** Each test class runs only its own analyzer, so each rule's share is checked in its own class. RH7207's own location on an alias target (`using A = {|RH7207:System.Text|}`) is RH7207-003, unchanged, and pinned by `DiagnosticOfABlockStartingWithAnAliasIsOnTheAliasTarget`.
- **Failure classes walked.** FC-003: both sides of the build condition (DEBUG / A / NETFRAMEWORK), of order wrong vs. right in RH7207, and of directive inside vs. after the block are covered. FC-008/FC-009: the Fix All mixed-block test and a no-action assertion on every reported diagnostic are covered. FC-013: the formatter parity tests are covered. FC-020: directive kind × position is covered. FC-005/FC-019: comment vs. directive is covered by the existing `HashLine…` tests. FC-010/FC-012 (formatter idempotency): no formatter change. A second pass is still run by `VerifyFormatter` / `VerifyFormatterKeepsReportedShape` under LF and CRLF. FC-007: no fix is offered for an unsafe block, and the safe-block fix is unchanged. FC-038 to FC-043: nothing is declared unsupported (maintainer answer 4). FC-018/FC-036, FC-022 to FC-029: the change does not reach renames, literals, or expression shapes.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Ordering.RH720|FullyQualifiedName~RH7207UsingDirectives"
```

## Notes for the implementer

- `FixAllOrganizesOnlyTheBlockWithoutPreprocessorDirective` sets `NumberOfIncrementalIterations = 1` and `NumberOfFixAllIterations = 1`. The iteration counts are test mechanics: one fix organizes the namespace block, and the compilation-unit diagnostic has no action. They could not be confirmed before the analyzer reports. If the verifier reports a different count once the analyzers report, the lead decides whether the count changes. The expected texts stay.

## Follow-up candidates

_None._
