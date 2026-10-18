Role: test implementer
Commit: cd4ba13
Verdict: tests written

The coordinator's prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing had to be ignored. No production code was touched. Every test below passes on the unchanged code, as this lane requires. The lists of untagged tests in `03-spec-author.md` and `05-spec-author.md` were applied.

## Tests

All files are under `src/Reihitsu.Analyzer.Test/`. Every test class derives from `BatchCodeFixTestsBase`. "Verify" means `Verify(source, fixedSource, …)`, i.e. the analyzer, the fix, re-analysis, and the framework's Fix All pass. "Verify (analyzer)" means `Verify(source, …)`. "No-fix" means the analyzer `Verify` plus `GetCodeFixActionsAsync` on the reported span, asserting that no action is registered.

### New tests (46)

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH5601 `VerifyTabsBetweenTokensAndBeforeTrailingCommentAreDetectedAndFixed` | RH5601-001, -004 | Verify | pass |
| RH5601 `VerifyTabInDefineDirectiveIsDetectedAndFixed` | RH5601-001, -004 | Verify | pass |
| RH5601 `VerifyConsecutiveTabsAreReportedOneByOne` | RH5601-003, -004 | Verify | pass |
| RH5601 `VerifyNoDiagnosticsForTabInRegularVerbatimInterpolatedAndCharacterLiterals` | RH5601-002, FC-022 | Verify (analyzer) | pass |
| RH5602 `VerifyTrailingWhitespaceAfterBraceStatementAndCharacterLiteralIsDetectedAndFixed` | RH5602-001, -003, -004 | Verify | pass |
| RH5602 `VerifyWhitespaceOnlyLineIsReportedAsAWholeAndBecomesEmpty` | RH5602-001, -003, -004 | Verify | pass |
| RH5602 `VerifyWhitespaceInsideALineIsNotReported` | RH5602-001 (negative side, FC-003) | Verify (analyzer) | pass |
| RH5602 `VerifyInterpolatedVerbatimStringsDoNotProduceDiagnostics` | RH5602-002, FC-022 | Verify (analyzer) | pass |
| RH5603 `VerifySeveralTrailingLineBreaksAreOneDiagnosticAndAreDeleted` | RH5603-001, -003, -004 | Verify | pass |
| RH5603 `VerifyTrailingCarriageReturnLineFeedsAreDetectedAndFixed` | RH5603-001, -003, -004, FC-033 | Verify | pass |
| RH5603 `VerifyMixedTrailingLineBreaksAreDetectedAndFixed` | RH5603-001, -004 | Verify | pass |
| RH5603 `VerifyTrailingLineBreakAfterFinalCommentIsDetectedAndFixed` | RH5603-001, -004 | Verify | pass |
| RH5603 `VerifyNoDiagnosticsWhenFileEndsWithCommentAfterBlankLines` | RH5603-002 | Verify (analyzer) | pass |
| RH5604 `VerifyLfAtTheEndOfADirectiveIsDetectedWhenCrLfIsPredominant` | RH5604-001, -004 | Verify (analyzer), suppression check skipped like the sibling fixtures | pass |
| RH5604 `VerifyLineEndingsInsideInterpolatedVerbatimStringsDoNotProduceDiagnostics` | RH5604-003, FC-022 | Verify (analyzer) | pass |
| RH5604 `VerifyCodeFixConvertsEveryLineEndingWhicheverDiagnosticItIsAppliedTo` | RH5604-005, FC-017 (exact raw text, no normalization) | `GetCodeFixActionsAsync` on each of the two diagnostics, applied once | pass |
| RH6001 `VerifyEveryListedStatementKeywordIsDetectedAndFixed` | RH6001-001, -003, -004, FC-027 (all 10 keywords, including the `while` of `do`) | Verify | pass |
| RH6001 `VerifyCommentAloneDoesNotSeparateKeywordAndFixInsertsSpaceBeforeComment` | RH6001-001, -004, FC-019 | Verify | pass |
| RH6001 `VerifyKeywordSeparatedBySeveralSpacesOrALineBreakIsNotReported` | RH6001-002, FC-003 | Verify (analyzer) | pass |
| RH6001 `VerifyCallLikeKeywordsAreNotChecked` | RH6001-002 | Verify (analyzer) | pass |
| RH6003 `VerifyFieldReturnAndForHeaderSemicolonsAreDetectedAndFixed` | RH6003-001, -003, -004 | Verify | pass |
| RH6003 `VerifySemicolonStartingItsLineAfterALineCommentIsNotReported` | RH6003-002 | Verify (analyzer) | pass |
| RH6003 `VerifyFixIsOfferedWhenACommentFollowsTheSemicolon` | RH6003-004, SHARED-WS-001, SHARED-WS-002 (outside-gap arm), FC-021 | Verify | pass |
| RH6004 `VerifyIndentedConditionalDirectivesAndTabIndentationAreDetectedAndFixed` | RH6004-001, -003, -004, SHARED-WS-001, FC-020 | Verify | pass |
| RH6004 `VerifyIndentedDefineDirectiveIsDetectedAndFixed` | RH6004-001, -004 | Verify | pass |
| RH6004 `VerifyVerbatimStringContentAndNestedInactiveDirectivesDoNotProduceDiagnostics` | RH6004-002, FC-022 | Verify (analyzer) | pass |
| RH6005 `VerifyCommentAloneAfterOperatorKeywordIsDetectedAndFixed` | RH6005-001, -003, -004, FC-019 (operator and conversion operator) | Verify | pass |
| RH6005 `VerifyConversionOperatorFollowedBySpaceIsNotReported` | RH6005-002 | Verify (analyzer) | pass |
| RH6006 `VerifyWhitespaceBeforeClosingParenthesisAndAtTheEndOfTheLineIsDetectedAndFixed` | RH6006-001, -002, -003 (tab, `( )`, end of line, line break not reported) | Verify | pass |
| RH6006 `VerifyFixIsNotOfferedWhenACommentIsOnALaterLineOfTheGap` | RH6006-001, -003, SHARED-WS-002, FC-019 | No-fix | pass |
| RH6006 `VerifyFixIsNotOfferedWhenADirectiveIsInTheGap` | RH6006-001, -003, SHARED-WS-002, FC-005, FC-020 | No-fix | pass |
| RH6006 `VerifyFixIsOfferedWhenACommentFollowsTheNextToken` | RH6006-003, SHARED-WS-001, SHARED-WS-002 (outside-gap arm) | Verify | pass |
| RH6007 `VerifyWhitespaceAfterOtherOpeningSquareBracketsIsDetectedAndFixed` | RH6007-001, -002, -003, FC-027 (array type, rank, creation, collection expression, list pattern, end of line) | Verify | pass |
| RH6007 `VerifyAttributeListAndLineBreakAfterOpeningBracketAreNotReported` | RH6007-001 (negative side) | Verify (analyzer) | pass |
| RH6007 `VerifyFixIsNotOfferedWhenACommentIsOnALaterLineOfTheGap` | RH6007-001, -003, SHARED-WS-002, FC-019 | No-fix | pass |
| RH6007 `VerifyFixIsNotOfferedWhenADirectiveIsInTheGap` | RH6007-001, -003, SHARED-WS-002, FC-020 | No-fix | pass |
| RH6008 `VerifyWhitespaceBeforeOtherClosingSquareBracketsIsDetectedAndFixed` | RH6008-001, -002, -003 | Verify | pass |
| RH6008 `VerifyAttributeListAndClosingBracketStartingItsLineAreNotReported` | RH6008-001 (negative side, also after an end-of-line comment) | Verify (analyzer) | pass |
| RH6009 `VerifyOpeningBracesOfOtherConstructsAreDetectedAndFixed` | RH6009-001, -003, -004, FC-027 (method body, initializer, property pattern, switch expression) | Verify | pass |
| RH6009 `VerifyOpeningBraceAfterBlockCommentIsDetectedAndFixed` | RH6009-001, -004, FC-019 | Verify | pass |
| RH6009 `VerifyOpeningBracePrecededBySeveralSpacesIsNotReported` | RH6009-001 (any width) | Verify (analyzer) | pass |
| RH6010 `VerifyClosingBracesOfInitializersAreDetectedAndFixed` | RH6010-001, -003, -004 (including the empty pair `{}`) | Verify | pass |
| RH6010 `VerifyClosingBraceAfterBlockCommentIsDetectedAndFixed` | RH6010-001, -004, FC-019 | Verify | pass |
| RH6010 `VerifyClosingBracePrecededBySeveralSpacesIsNotReported` | RH6010-001 (any width) | Verify (analyzer) | pass |
| RH6011 `VerifyClassTypeParameterListAndMethodTypeArgumentListAreDetectedAndFixed` | RH6011-001, -003, -004 | Verify | pass |
| RH6012 `VerifyNestedAndUnboundGenericClosingBracketsAreDetectedAndFixed` | RH6012-001, -003, -004 (nested `>`, `Dictionary<, >`) | Verify | pass |

The four no-fix tests are not vacuous. With the directive lines (RH6006) or the comment line (RH6007) removed from the input, both fail with `Expected collection to be empty`. That mutation was reverted.

### Existing tests tagged (no assertion changed)

| Class | Tagged tests → requirements |
|---|---|
| RH5601 | `VerifyIssueIsDetectedAndFixed` → 001, 003, 004; `VerifyIssueIsDetectedAndFixedForTabInPragmaDirective` → 001, 004; the nine `VerifyNoDiagnosticsForTabIn…` tests → 002 |
| RH5602 | `VerifyTrailingWhitespaceIsDetectedAndFixed` → 001, 003, 004; `VerifyTrailingWhitespaceAfterMultiLineCommentIsStillFlagged` → 001, 004; `VerifyRawStringsDoNotProduceDiagnostics`, `VerifyVerbatimStringsDoNotProduceDiagnostics` → 002 |
| RH5603 | `VerifyTrailingNewlineIsDetectedAndFixed` → 001, 003, 004; `VerifyNoDiagnosticsWhenFileDoesNotEndWithNewline` → 002 |
| RH5604 | `VerifyMixedLineEndingsAreDetectedAndFixed` → 001, 004, 005; the XML, tie, and delimited-XML detect-and-fix tests → 001, 005; `VerifyCrLf…/VerifyLf…AreDetectedWhen…IsPredominant` → 001, 004; `VerifyFirstEncounteredLineEndingWinsWhenCountsAreTied` → 001; LF-only, CRLF-only, and uniform-XML tests → 002; verbatim and raw literal tests → 003 |
| RH6001 | `VerifyIssueIsDetectedAndFixed` → 001, 003, 004; `VerifyNoDiagnosticsWhenCodeIsClean`, `VerifyTypeofDoesNotProduceDiagnostics` → 002 |
| RH6003 | `VerifyIssueIsDetectedAndFixed` → 001, 003, 004, SHARED-WS-001; tab test → 001, 004; the four continuation-line-indentation and directive/disabled-text tests → 002; `VerifyFixIsNotOfferedForSameLineSemicolonWithPrecedingComment` (+CRLF) → 001, 004, SHARED-WS-002 |
| RH6004 | `VerifyIssueIsDetectedAndFixed` → 001, 003, 004, SHARED-WS-001; region, raw-string, comment, and inactive-directive tests → 002 |
| RH6005 | `VerifyIssueIsDetectedAndFixed` → 001, 003, 004; clean-code and cref tests → 002 |
| RH6006 | `VerifyIssueIsDetectedAndFixed` → 001, 002, 003, SHARED-WS-001; clean code → 001 |
| RH6007 | `VerifyIssueIsDetectedAndFixed` → 001, 002, 003, SHARED-WS-001; clean code → 001 |
| RH6008 | `VerifyIssueIsDetectedAndFixed` → 001, 002, 003, SHARED-WS-001; same-line comment no-fix (+CRLF) → 001, 003, SHARED-WS-002; multi-line collection expression → 001 |
| RH6009 / RH6010 | `VerifyIssueIsDetectedAndFixed` → 001, 003, 004; clean code → 001; interpolated-string test → 002 |
| RH6011 | detect-and-fix (`List <int>`) → 001, 003, 004, SHARED-WS-001; type-parameter and tab tests → 001, 004; `VerifyCommentBeforeOpeningGenericBracketDoesNotBroadenDiagnosticSpan` → 001, 003; the four continuation-line and directive/disabled-text tests (`<` starts its line) → 001; same-line comment no-fix (+CRLF) → 004, SHARED-WS-002; cref → 002 |
| RH6012 | detect-and-fix → 001, 003, 004, SHARED-WS-001; type-parameter and tab tests → 001, 004; same-line comment no-fix (+CRLF) → 001, 003, 004, SHARED-WS-002; the four continuation-line and directive/disabled-text tests → 001; cref → 002 |

All 60 IDs and SHARED-WS-001/002 are covered (`EveryRequirementOfAnEnforcedSpecIsCovered` passes).

## Scenarios (bug fix)

_N/A._ This lane characterizes existing behavior.

## Not covered, and why

Deliberately left untagged, because the spec leaves their inputs out (per `03-spec-author.md` and `05-spec-author.md`) or does not decide them:

- RH6003, RH6011, RH6012: `VerifyFixIsNotOfferedForContinuationLine…WithPrecedingComment` and their CRLF and type-parameter twins. The continuation-line comment case is open question RH6003/RH6011/RH6012 Q1.
- RH6006: `VerifyFixIsNotOfferedWhenSpaceSeparatesAComment` (open question RH6006 Q1).
- RH5602: the `//`, multi-line comment, and documentation-comment tests, `VerifyIssueIsDetectedAndFixedForTrailingWhitespaceInPragmaDirective` (thoenissen/Reihitsu#959, #960), and `VerifyNoDiagnosticsForTrailingWhitespaceInDisabledText`. RH5602-001 claims no report for whitespace after disabled text, and no requirement decides the negative.
- RH5604: `VerifyLoneCarriageReturnInsideXmlDocumentationIsNormalizedToLf` (lone `\r`, pipeline Q1), and the block-comment and disabled-text tests (non-goal).
- RH6008: `VerifyContinuationLineClosingBracketWithPrecedingCommentDoesNotProduceDiagnostic` (+CRLF). Its `]` neither starts its line nor follows a token that ends on that line, so RH6008-001 does not decide it in either direction.
- RH5601: `VerifyCodeFixDoesNotModifyRawStringContent` and `VerifyCodeFixDoesNotOfferActionForTabInDisabledText`. They drive the fix with a span the analyzer never reports, so they test a defensive guard that no requirement states.
- RH6003: `VerifySharedWhitespaceSpanPolicyIsLineBounded`, `VerifyNonWhitespaceSpanWithholdsFix`, and `VerifyEmptySpanStillRegistersFix` are unit tests of helpers and of synthetic spans.
- `RemoveWhitespaceRunCodeFixProviderBaseTests`: every test uses a synthetic span in leading trivia, a truncated document, or document start, which RH6006 never reports. SHARED-WS-002 speaks of "the reported whitespace", so the guard is covered instead through real diagnostics (the RH6006, RH6007, and RH6003 tests above). The same-line `( /* keep */` shape is also open question RH6006 Q1.
- Formatter parity tests under `Formatter/Formatting/RH56…`/`RH60…FormatterTests` verify the formatter, not an analyzer requirement, so they are untagged, as for RH6002.

None of the new tests uses an input that `05-spec-author.md` lists as excluded: a directive line before a literal, `#if DEBUG··`, `/// <summary>··`, a final directive or disabled text, braces inside an interpolation expression, a continuation-line comment, or a same-line comment after `(`/`[`.

Failure classes walked:

- FC-019/020/021 apply to every SHARED-WS-002 user, but the directive arm can reach only the "after" rules (RH6006, RH6007), which have a gap that may span lines. For RH6003, RH6008, RH6011, and RH6012, the specs claim a run only when the previous token ends on the same line and the run directly precedes the token. For those claimed inputs the gap lies on one line and cannot hold a directive; their same-line comment arm is tested. A directive in the gap of RH6003, RH6011, or RH6012 is reachable only together with a continuation-line comment, which is the excluded open-question shape, so it is not tested.
- The RH6004 gap is the indentation of a directive line, so it can hold no comment.
- FC-003 is tested on both sides: run against no run, same line against a line that starts with the token, one space against several (RH6001, RH6009, RH6010), and space against tab.
- FC-022 is covered by literal tests for RH5601, RH5602, RH5604, and RH6004.
- FC-033 and FC-017: RH5603 has CRLF and mixed endings, and RH5604 asserts exact raw text after one application.
- FC-007/008: one-pass convergence and Fix All already run in the framework's `Verify` and in the inherited `FixAllCorrectsEveryDiagnostic`.
- Not applicable, because the lane changes no code: FC-001/002 (no guard is changed), FC-004/006/023–026/028–030/036 (no rewrite of syntax shape, renames, or member kinds), FC-010–012/014 (no formatter change), FC-013 (parity is the formatter tests' concern and the specs' Why), FC-015/016, FC-018, FC-031/032/034/035/037.

Observation for the spec checker, not a finding against the code: `RH5602CodeMustNotContainTrailingWhitespaceCodeFixProvider` derives from `CodeFixProvider` directly, not from either code owner of `shared/whitespace-removal-fixes.md`, yet RH5602's `Uses` lists SHARED-WS-001. Its tests (tagged RH5602-004) pass the SHARED-WS-001 behavior.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5601UseTabs|FullyQualifiedName~RH5602CodeMust|FullyQualifiedName~RH5603File|FullyQualifiedName~RH5604Code|FullyQualifiedName~RH6001Key|FullyQualifiedName~RH6003Semi|FullyQualifiedName~RH6004Pre|FullyQualifiedName~RH6005Op|FullyQualifiedName~RH6006Open|FullyQualifiedName~RH6007Open|FullyQualifiedName~RH6008Clos|FullyQualifiedName~RH6009Open|FullyQualifiedName~RH6010Clos|FullyQualifiedName~RH6011Open|FullyQualifiedName~RH6012Clos|FullyQualifiedName~RemoveWhitespaceRun"
scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests|FullyQualifiedName~RequirementCoverageTests"
```

Results on this tree:

- Analyzer filter: 229 passed (the baseline before this role was 183).
- Architecture filter: 39 passed. The full architecture project: 72 passed.
- `FullyQualifiedName~Reihitsu.Analyzer.Test.SelfHosting`: 17 passed.
- `scripts/format.sh` over the 15 changed test files: 0 files changed.
