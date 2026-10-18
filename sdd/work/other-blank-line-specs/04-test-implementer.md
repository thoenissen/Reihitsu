Role: test implementer
Commit: 3acfe41
Verdict: tests written

Lane: specs and tests for existing behavior. My prompt carried no intended fix, suspected cause, or hoped-for verdict. No production code was touched. All files changed are the twelve `src/Reihitsu.Analyzer.Test/Formatting/RH50xx…AnalyzerTests.cs` classes of the rules in scope.

## What was done

- **Tags on existing tests.** 74 existing test methods carry `[Requirement]` tags now. A test is tagged only where its input exercises the requirement. Negative tests whose input has no blank line at all prove nothing for a "blank line is not reported" requirement (FC-017), so they stay untagged. These are the `VerifyNoDiagnosticsWhenCodeIsClean` tests of RH5022 to RH5027, RH5025's raw-string, comment and disabled-code tests, RH5026's identifier, raw-string, comment and disabled-code tests, RH5027's `VerifyWhileStatementsDoNotProduceDiagnostics`, RH5031's `VerifyNoDirectiveTreeDoesNotProduceDiagnostics`, and RH5023's two fix-base tests (`VerifyNonWhitespaceSpanWithholdsFix`, `VerifyEmptySpanStillRegistersFix`). Their requirements are covered by the new example tests instead.
- **Left untagged because of an open question:** `RH5032…Tests.VerifyRegionBeforeClosingBraceIsDetectedAndFixed`. Q9 names it as the RH5032 half of the RH5024/RH5032 cycle. RH5011's `SameLineFixIsNotOfferedAcrossDirectiveAndSkippedSyntax` also stays untagged. Its input (`break; #if FEATURE`) puts a directive on a code line, and RH5011-006 does not describe that shape.
- **55 new tests.** Each one comes from a spec example or from a requirement's own wording. Expected values were taken from the spec text and never from running the code. No new test uses an input of Q1 to Q11 in `03-spec-author.md`. All new tests passed on first run on the unchanged code, so the spec examples match current behavior. To check that the harness is not vacuous, I made two temporary mutations and reverted both: an RH5028 expected fixed text set to `" "` made the test fail, and the markup-only analyzer tests fail on any unexpected diagnostic.
- **CRLF.** Every rule with a fix got a CRLF test through `ApplyCodeFixAsync`, with `Assert.AreEqual` on the exact CRLF result. Where the rule reads blank lines, the input contains a whitespace-only line. That covers "a line holding nothing but whitespace counts as blank", which raw string literals cannot express.
- `scripts/format.sh` on all twelve files: `Formatted 0 of 12 file(s)`.

## Tests

New tests (all in `src/Reihitsu.Analyzer.Test/Formatting/`, "Expected now" = pass on the unchanged code; every one passes):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH5011…`VerifyBreakFollowedByStatementOrSwitchSectionIsReported` | RH5011-001, -003 (RH5011-001 example) | `Verify(source)` markup | pass |
| RH5011…`VerifyBreakBeforeClosingBraceOrDirectiveIsNotReported` | RH5011-002; FC-020 `#pragma` | `Verify(source)` | pass |
| RH5011…`VerifySeparateLineFixInsertsBlankLineBelowBreakLine` | RH5011-001, -004; FC-021 trailing `//` | `Verify(source, fixed)` | pass |
| RH5011…`VerifyCommentBetweenBreakAndSameLineStatementIsReportedWithoutFix` | RH5011-001, -006 | `Verify(source, source)` (reported, unchanged) | pass |
| RH5020…`VerifyOwnLineCommentsWithoutPrecedingBlankLineAreReported` | RH5020-001, -004, SHARED-BLANK-002 | `Verify(source)` | pass |
| RH5020…`VerifyFirstCommentOfFileTypeBlockOrSwitchSectionIsNotReported` | RH5020-002 | `Verify(source)` | pass |
| RH5020…`VerifyCommentFollowingCommentOrDirectiveAfterLineBreakIsNotReported` | RH5020-003 (not-reported side of the boundary pair), SHARED-BLANK-002 | `Verify(source)` | pass |
| RH5020…`VerifyCommentFollowingCommentsOnPrecedingTokenLineIsReported` | RH5020-003 (reported side), SHARED-BLANK-002; FC-003, FC-021 | `Verify(source)` | pass |
| RH5020…`VerifyFixInsertsEmptyLineAboveEachComment` | RH5020-001, -005 | `Verify(source, fixed)` | pass |
| RH5020…`VerifyFixInsertsCarriageReturnLineFeedEmptyLineAboveComment` | RH5020-005; CRLF | `ApplyCodeFixAsync` | pass |
| RH5022…`VerifyBlankLineBelowOpeningBraceOfTypeInitializerAndMemberIsReported` | RH5022-001, -003 | `Verify(source)` | pass |
| RH5022…`VerifyBraceInStringCommentOrDisabledCodeIsNotConsidered` | RH5022-002; FC-022 | `Verify(source)` | pass |
| RH5022…`VerifyFixDeletesBlankLinesBelowOpeningBraces` | RH5022-004, SHARED-WS-001 | `Verify(source, fixed)` | pass |
| RH5022…`VerifyFixDeletesWhitespaceOnlyLineBelowOpeningBraceWithCarriageReturnLineFeed` | RH5022-001, -004, SHARED-WS-001; CRLF | `ApplyCodeFixAsync` | pass |
| RH5023…`VerifyEachBlankLineAfterFirstOfRunIsReportedAndDeleted` | RH5023-001, -003, -004, SHARED-WS-001 | `Verify(source, fixed)` (Fix All, default iterations) | pass |
| RH5023…`VerifyLinesOfCommentsDisabledCodeAndStringsAreNotCounted` | RH5023-002; FC-022 | `Verify(source)` | pass |
| RH5023…`VerifyBlankLinesInsideInterpolatedStringsAreNotReported` | RH5023-002 (requirement text: interpolated strings) | `Verify(source)` | pass |
| RH5023…`VerifyFixDeletesWhitespaceOnlySecondBlankLineWithCarriageReturnLineFeed` | RH5023-001, -004, SHARED-WS-001; CRLF | `ApplyCodeFixAsync` | pass |
| RH5024…`VerifyBlankLineAboveClosingBraceIsReported` | RH5024-001, -004; FC-021 `} // note` | `Verify(source)` | pass |
| RH5024…`VerifyBlankLineSeparatedFromClosingBraceByCommentOrDirectiveIsNotReported` | RH5024-002; FC-019/FC-020 | `Verify(source)` | pass |
| RH5024…`VerifyBraceInStringCommentOrDisabledCodeIsNotConsidered` | RH5024-003; FC-022 | `Verify(source)` | pass |
| RH5024…`VerifyFixDeletesBlankLinesAboveClosingBraces` | RH5024-005, SHARED-WS-001 | `Verify(source, fixed)` | pass |
| RH5024…`VerifyFixDeletesWhitespaceOnlyLineAboveClosingBraceWithCarriageReturnLineFeed` | RH5024-001, -005, SHARED-WS-001; CRLF | `ApplyCodeFixAsync` | pass |
| RH5025…`VerifyBlankLineAboveOpeningBraceThatContinuesCodeIsReported` | RH5025-001, -003 (all eleven shapes of the example); FC-027 | `Verify(source)` | pass |
| RH5025…`VerifyBlankLineSeparatedByCommentOrBraceOutsideCodeIsNotReported` | RH5025-002; FC-019 | `Verify(source)` | pass |
| RH5025…`VerifyFixDeletesBlankLinesAboveOpeningBraces` | RH5025-004, SHARED-WS-001 | `Verify(source, fixed)` | pass |
| RH5025…`VerifyFixDeletesWhitespaceOnlyLineAboveOpeningBraceWithCarriageReturnLineFeed` | RH5025-001, -004, SHARED-WS-001; CRLF | `ApplyCodeFixAsync` | pass |
| RH5025…`VerifyBlankLineAboveExtensionBlockBraceIsReported` | untagged characterization (skeptic candidate, see below) | `Verify(source)` | pass |
| RH5026…`VerifyBlankLineAboveChainedKeywordIsReported` | RH5026-001, -003; FC-026/FC-027 `else if` | `Verify(source)` | pass |
| RH5026…`VerifyKeywordThatDoesNotBeginItsLineIsNotConsidered` | RH5026-002 | `Verify(source)` | pass |
| RH5026…`VerifyFixDeletesBlankLinesAboveChainedKeywords` | RH5026-004, SHARED-WS-001 | `Verify(source, fixed)` | pass |
| RH5026…`VerifyFixDeletesWhitespaceOnlyLineAboveChainedKeywordWithCarriageReturnLineFeed` | RH5026-001, -004, SHARED-WS-001; CRLF | `ApplyCodeFixAsync` | pass |
| RH5027…`VerifyBlankLineAboveDoStatementFooterIsReported` | RH5027-001 (both arms: bare `while`, `} while`), -003; FC-003 | `Verify(source)` | pass |
| RH5027…`VerifyBlankLineAboveWhileStatementIsNotReported` | RH5027-002 | `Verify(source)` | pass |
| RH5027…`VerifyFixDeletesBlankLineAboveDoStatementFooter` | RH5027-004, SHARED-WS-001 | `Verify(source, fixed)` | pass |
| RH5027…`VerifyFixDeletesWhitespaceOnlyLineAboveDoStatementFooterWithCarriageReturnLineFeed` | RH5027-001, -004, SHARED-WS-001; CRLF | `ApplyCodeFixAsync` | pass |
| RH5028…`VerifyBlankLinesInFrontOfFirstContentLineAreReportedAsOneSpan` | RH5028-001, -003, SHARED-WS-001 | `Verify(source, fixed)` | pass |
| RH5028…`VerifyWhitespaceOnlyFileIsReportedOverWholeFileAndEmptied` | RH5028-001, -003 | `Verify(source, string.Empty)` | pass |
| RH5028…`VerifyEmptyFileIsNotReported` | RH5028-002 | `Verify(string.Empty)` | pass |
| RH5028…`VerifyFileStartingWithCommentFollowedByBlankLineIsNotReported` | RH5028-002 | `Verify(source)` | pass |
| RH5028…`VerifyBlankLinesInFrontOfDirectiveAreReportedAndDeleted` | RH5028-001 (directive as first content), -003 | `Verify(source, fixed)` | pass |
| RH5028…`VerifyFixDeletesLeadingBlankLinesWithCarriageReturnLineFeed` | RH5028-001, -003, SHARED-WS-001; CRLF, comment as first content | `ApplyCodeFixAsync` | pass |
| RH5030…`VerifyStatementAfterClosingBraceWithoutBlankLineIsReported` | RH5030-001, -002, SHARED-BLANK-003, SHARED-BLANK-004; FC-020 `#pragma`, same-line pair | `Verify(source)` | pass |
| RH5030…`VerifyFixInsertsBlankLineDirectlyBeforeNextStatement` | RH5030-003, SHARED-BLANK-003; FC-019/FC-020/FC-021 | `Verify(source, fixed)` | pass |
| RH5030…`VerifyFixInsertsCarriageReturnLineFeedBlankLineBeforeNextStatement` | RH5030-003; CRLF | `ApplyCodeFixAsync` | pass |
| RH5031…`VerifyRegionDirectiveWithoutBlankLineAboveIsReportedOnItsText` | RH5031-001, -002, SHARED-BLANK-005, SHARED-REGION-001 | `Verify(source)` | pass |
| RH5031…`VerifyTrailingBraceCommentRemovesExemptionAndDisabledDirectivesAreNotReported` | RH5031-001, SHARED-BLANK-005, SHARED-BLANK-007 | `Verify(source)` | pass |
| RH5031…`VerifyFixInsertsEmptyLineAboveDirective` | RH5031-003, SHARED-REGION-002 | `Verify(source, fixed)` | pass |
| RH5031…`VerifyFixKeepsCommentAboveDirectiveInPlace` | RH5031-003, SHARED-REGION-002 (SHARED-REGION-002 example) | `Verify(source, fixed)` | pass |
| RH5031…`VerifyFixInsertsCarriageReturnLineFeedEmptyLineAboveDirective` | RH5031-003, SHARED-REGION-002; CRLF | `ApplyCodeFixAsync` | pass |
| RH5032…`VerifyRegionDirectiveWithoutBlankLineBelowIsReportedOnItsText` | RH5032-001, -002, SHARED-BLANK-006, SHARED-REGION-001 | `Verify(source)` | pass |
| RH5032…`VerifyEndRegionOnLastLineOfFileIsNotReported` | RH5032-001, SHARED-BLANK-006 | `Verify(source)` | pass |
| RH5032…`VerifyFixInsertsEmptyLineBelowDirective` | RH5032-003, SHARED-REGION-002 | `Verify(source, fixed)` | pass |
| RH5032…`VerifyFixKeepsCommentBelowDirectiveInPlace` | RH5032-003, SHARED-REGION-002 (requirement text: comment below the directive) | `Verify(source, fixed)` | pass |
| RH5032…`VerifyFixInsertsCarriageReturnLineFeedEmptyLineBelowDirective` | RH5032-003, SHARED-REGION-002; CRLF | `ApplyCodeFixAsync` | pass |

Existing tests tagged (method → IDs):

| Class | Tagged methods |
|---|---|
| RH5011 | `VerifyDiagnosticForBreakStatementWithoutFollowingBlankLine` → -001, -003, -004; `VerifyNoDiagnosticForBreakStatementWithFollowingBlankLine` → -001, SHARED-BLANK-001; `VerifyNoDiagnosticForBreakStatementBeforeClosingBrace`, `VerifyNoDiagnosticForBreakStatementFollowedByDirective` → -002; `VerifyDiagnosticForBreakStatementWhenCommentLineDirectlyFollowsIt` → -001, -004, SHARED-BLANK-001; `SameLineBreakFixSeparatesAndConverges`, `SameLineFixPreservesExactTabIndentationAndConverges` → -005; `SeparateLineFixPreservesCarriageReturnLineFeedAndConverges` → -004; `SameLineFixIsNotOfferedAcrossComment` → -006 |
| RH5020 | 13 methods across -001 to -005; `VerifyOwnLineBlockCommentIsDetectedButInlineCommentIsExcluded` also SHARED-BLANK-002; `VerifyNoDiagnosticForCommentWithPrecedingBlankLine` also SHARED-BLANK-001 |
| RH5022 | `VerifyIssueIsDetectedAndFixed` → -001, -003, -004, SHARED-WS-001; raw-string, multi-line-comment, disabled-code tests → -002 |
| RH5023 | `VerifyIssueIsDetectedAndFixed` → -001, -003, -004, SHARED-WS-001; raw, verbatim, comment, disabled tests → -002 |
| RH5024 | `VerifyIssueIsDetectedAndFixed` → -001, -004, -005, SHARED-WS-001; trailing-semicolon and endregion-before-brace tests → -001, -005; raw, comment, disabled tests → -003; `VerifyBlankLineBeforeTrailingEndRegionDoesNotProduceDiagnostics` → -002 |
| RH5025, RH5026, RH5027 | `VerifyIssueIsDetectedAndFixed` → -001, -003, -004, SHARED-WS-001 |
| RH5028 | `VerifyNoDiagnosticsWhenFileStartsWithContent` → -002; `VerifyLeadingBlankLinesAreDetectedAndFixed` → -001, -003, SHARED-WS-001 |
| RH5030 | 19 methods: the reported-and-fixed tests → -001, -003 (+ -002 on the if-block test), SHARED-BLANK-003 or SHARED-BLANK-004 for the `break` shapes; the else/catch/finally/do-while and last-statement tests → -001; blank-line-present and same-line tests → -001, SHARED-BLANK-003; terminal-break test → -001, SHARED-BLANK-004 |
| RH5031 | 6 methods: -001; the two fix tests also -002, -003, SHARED-REGION-001/-002; SHARED-BLANK-005 or SHARED-BLANK-007 as the input decides |
| RH5032 | 6 methods: -001; the two fix tests also -002, -003, SHARED-REGION-001/-002; SHARED-BLANK-006 or SHARED-BLANK-007 as the input decides |

Every requirement in the `03-spec-author.md` list now has at least one covering test. The list is RH5011-001 to RH5032-003, SHARED-REGION-001/-002, and SHARED-BLANK-002, -003, -005, -006, -007. `RequirementCoverageTests` passes.

## Scenarios (bug fix)

_N/A — no bug fix in this lane._

## Skeptic candidates the skeptic left for me to run

| Input | Line ending | Surface | Result |
|---|---|---|---|
| `static class E` / `{` / `extension(int i)` / blank / `{` … (C# 14 extension block) | LF | RH5025 analyzer | Reported, on the blank line. Kept as the untagged characterization test `VerifyBlankLineAboveExtensionBlockBraceIsReported`. RH5025-001 does not name an extension block explicitly ("namespace, type, member, accessor list, …"). The spec author can decide whether the list should say so. I did not tag it because the wording leaves this open. |
| `case 1:` / blank / `{` (braced switch section) | LF | RH5025 analyzer | Reported. This shape is named in Q2, so I did not commit a test for it (scratch run only, reverted). |

## Not covered, and why

- **Inputs of Q1–Q11** (`03-spec-author.md`). No test was added or tagged for them. This covers the interpolation brace, sibling braces, a one-line `do`, `else`/`do`-`while` after `break`, a block comment on the `break` or `}` line, the EOF blank line, `{ // note`, `#region` above `}`, a `///` comment before the blank line, and runs of two blank lines next to braces or keywords.
- **FC-007 one-pass convergence.** Every `Verify(source, fixed)` runs the verifier with its default iteration counts. No new test overrides `NumberOfIncrementalIterations` or `NumberOfFixAllIterations`, so a fix needing an extra pass would fail.
- **FC-008 Fix All across several diagnostics.** Each class's existing `GetFixAllScenario` already covers two or more diagnostics in one document (two documents for RH5028). The multi-diagnostic `Verify(source, fixed)` tests above also run Fix All. The scenario override cannot carry a `[Requirement]` tag, because the test method lives in the base class.
- **FC-013 / FC-010 formatter parity and idempotency.** These are not involved in this lane, because no formatter or analyzer code changed. The existing `src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH50xx…FormatterTests` classes were not touched and not tagged.
- **FC-018, FC-023 to FC-025, FC-028, FC-029, FC-036.** They cannot reach these rules: no rename, no identifier handling, no expression-bodied rewrite, no detached formatting, no paired layout. The fixes insert or delete line breaks only.
- **SHARED-WS-002** (comment or directive in a token gap). None of the specs in scope uses it.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5011|FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5020|FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5022|FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5023|FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5024|FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5025|FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5026|FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5027|FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5028|FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5030|FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5031|FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5032"
scripts/test.sh --project architecture --filter "FullyQualifiedName~RequirementCoverageTests|FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"
```

Results on 3acfe41: analyzer filter 172 passed, 0 failed. The architecture filter passed 39 of 39, and the full architecture project passed 72 of 72.
