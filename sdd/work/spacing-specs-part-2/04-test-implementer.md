Role: test implementer
Commit: 3ad57c9
Verdict: tests written

Issue thoenissen/Reihitsu#987, lane *specs and tests for existing behavior*. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

All 49 new requirement IDs now have covering tests. Every new test is built from a spec example block, or from a candidate input the skeptic listed, and it passes on the unchanged code. No production file changed.

How the tests were derived:

- The `csharp` blocks were extracted mechanically from `sdd/specs/analyzer/RH6013.md` … `RH6024.md` and `sdd/specs/shared/operator-spacing.md`. They were pasted unchanged, spec markup included (`{|RH6013: |}`).
- In the sources, `→` and `·` are expanded by the new helper `ExpandWhitespaceMarkers`. So the line-end runs stay visible, and the test text matches the spec.
- The verifier checks each reported span exactly. It also runs the single fixes one after another and Fix All, so every before/after block is verified on both paths.
- A block that says "is reported, and no code fix is offered" uses the new helper `VerifyNoCodeFixIsOffered`. The helper first verifies the marked diagnostics. It then runs the analyzer on the source without markup, asks the provider for actions on every diagnostic, and asserts that there are none.
- Mutation check of that helper: I removed the comment line from `RH6022…VerifyFixIsNotOfferedWhenCommentFollowsOnNextLine`, and the test failed with `Assert.IsEmpty(actions)`. Then I restored the file.

Test infrastructure added (test project only):

- `src/Reihitsu.Analyzer.Test/Base/AnalyzerTestsBase{TAnalyzer}.cs`: `ExpandWhitespaceMarkers(string)`.
- `src/Reihitsu.Analyzer.Test/Base/AnalyzerTestsBase{TAnalyzer,TCodeFix}.cs`: `VerifyNoCodeFixIsOffered(string, Action<CSharpAnalyzerVerifierTest<TAnalyzer>>)`.
- Both are additive. No existing caller changes.

## Tests

All test files are under `src/Reihitsu.Analyzer.Test/Formatting/`. "+ CRLF" means a second test with the suffix `CarriageReturnLineFeed`, run on `NormalizeToCarriageReturnLineFeed` of the same source and fixed source.

New tests:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH6013…Tests.VerifyWhitespaceAfterOpeningAttributeBracketIsReportedOnEveryAttributeTarget` + CRLF | RH6013-001, RH6013-002; FC-033 | `Verify(source)` | pass |
| `RH6013…Tests.VerifyFixDeletesWhitespaceAfterOpeningAttributeBracket` + CRLF | RH6013-003, SHARED-WS-001 | `Verify(source, fixedSource)` | pass |
| `RH6013…Tests.VerifyFixIsNotOfferedWhenCommentFollowsOnNextLine` + CRLF | RH6013-001, RH6013-003, SHARED-WS-002; FC-019 | `VerifyNoCodeFixIsOffered` | pass |
| `RH6013…Tests.VerifyFixIsNotOfferedWhenDirectiveFollowsOnNextLine` | RH6013-001, RH6013-003, SHARED-WS-002; FC-005, FC-020 | `VerifyNoCodeFixIsOffered` | pass |
| `RH6014…Tests.VerifyWhitespaceBeforeSameLineClosingAttributeBracketIsReportedOnEveryAttributeTarget` + CRLF | RH6014-001, RH6014-002 | `Verify(source)` | pass |
| `RH6014…Tests.VerifyFixDeletesWhitespaceBeforeClosingAttributeBracket` | RH6014-003, SHARED-WS-001 | `Verify(source, fixedSource)` | pass |
| `RH6015…Tests.VerifyWhitespaceBeforeNullableQuestionMarkIsReportedWhereverTypeIsWritten` + CRLF | RH6015-001, RH6015-003 | `Verify(source)` | pass |
| `RH6015…Tests.VerifyConditionalQuestionMarkAndCrefAreNotReported` | RH6015-002 | `Verify(source)` | pass |
| `RH6015…Tests.VerifyFixDeletesWhitespaceBeforeNullableQuestionMark` | RH6015-004, SHARED-WS-001 | `Verify(source, fixedSource)` | pass |
| `RH6016…Tests.VerifyWhitespaceNextToEveryMemberAccessSymbolIsReported` | RH6016-001, RH6016-003; FC-022 | `Verify(source)` | pass |
| `RH6016…Tests.VerifyLineBrokenSidesCommentsAndOtherQuestionMarksAreNotReported` | RH6016-002 | `Verify(source)` | pass |
| `RH6016…Tests.VerifyFixRemovesWhitespaceOnEachReportedSideOnly` + CRLF | RH6016-001, RH6016-003, RH6016-004; FC-003, FC-019, FC-020 | `Verify(source, fixedSource)` | pass |
| `RH6016…Tests.VerifyCommentOnUnreportedSideKeepsFix` | RH6016-001, RH6016-005, SHARED-GAP-001 | `Verify(source, fixedSource)` | pass |
| `RH6017…Tests.VerifyWhitespaceBeforeSameLinePostfixOperatorIsReported` + CRLF | RH6017-001, RH6017-003 | `Verify(source)` | pass |
| `RH6017…Tests.VerifyPrefixOperatorsAndOperatorDeclarationsAreNotReported` | RH6017-002 | `Verify(source)` | pass |
| `RH6017…Tests.VerifyFixDeletesWhitespaceBeforePostfixOperator` | RH6017-004, SHARED-WS-001 | `Verify(source, fixedSource)` | pass |
| `RH6018…Tests.VerifyWhitespaceAfterUnaryMinusIsReported` + CRLF | RH6018-001, RH6018-003; FC-022, FC-033 | `Verify(source)` | pass |
| `RH6018…Tests.VerifyWhitespaceThatKeepsTwoMinusSignsApartIsNotReported` | RH6018-002 | `Verify(source)` | pass |
| `RH6018…Tests.VerifyFixDeletesWhitespaceAfterUnaryMinus` + CRLF | RH6018-004, SHARED-WS-001 | `Verify(source, fixedSource)` | pass |
| `RH6018…Tests.VerifyFixIsNotOfferedWhenCommentFollowsOnNextLine` + CRLF | RH6018-001, RH6018-004, SHARED-WS-002; FC-019 | `VerifyNoCodeFixIsOffered` | pass |
| `RH6018…Tests.VerifyFixIsNotOfferedWhenDirectiveFollowsOnNextLine` | RH6018-001, RH6018-004, SHARED-WS-002; FC-005, FC-020 | `VerifyNoCodeFixIsOffered` | pass |
| `RH6019…Tests.VerifyWhitespaceAfterUnaryPlusIsReported` + CRLF | RH6019-001, RH6019-003; FC-033 | `Verify(source)` | pass |
| `RH6019…Tests.VerifyWhitespaceThatKeepsTwoPlusSignsApartIsNotReported` | RH6019-002 | `Verify(source)` | pass |
| `RH6019…Tests.VerifyFixDeletesWhitespaceAfterUnaryPlus` + CRLF | RH6019-004, SHARED-WS-001 | `Verify(source, fixedSource)` | pass |
| `RH6019…Tests.VerifyFixIsNotOfferedWhenCommentFollowsOnNextLine` + CRLF | RH6019-001, RH6019-004, SHARED-WS-002; FC-019 | `VerifyNoCodeFixIsOffered` | pass |
| `RH6019…Tests.VerifyFixIsNotOfferedWhenDirectiveFollowsOnNextLine` | RH6019-001, RH6019-004, SHARED-WS-002; FC-005, FC-020 | `VerifyNoCodeFixIsOffered` | pass |
| `RH6020…Tests.VerifyWhitespaceAfterAddressOfOrIndirectionIsReported` + CRLF | RH6020-001, RH6020-003; FC-033 | `Verify(source)` | pass |
| `RH6020…Tests.VerifyWhitespaceThatKeepsTwoAddressOfOperatorsApartIsNotReported` | RH6020-002 | `Verify(source)`, `CompilerDiagnostics.None` (the example does not compile, CS0211) | pass |
| `RH6020…Tests.VerifyFixDeletesWhitespaceAfterAddressOfOrIndirection` | RH6020-004, SHARED-WS-001 | `Verify(source, fixedSource)` | pass |
| `RH6020…Tests.VerifyFixIsNotOfferedWhenCommentFollowsOnNextLine` + CRLF | RH6020-001, RH6020-004, SHARED-WS-002; FC-019 | `VerifyNoCodeFixIsOffered` | pass |
| `RH6020…Tests.VerifyFixIsNotOfferedWhenDirectiveFollowsOnNextLine` | RH6020-001, RH6020-004, SHARED-WS-002; FC-005, FC-020 | `VerifyNoCodeFixIsOffered` | pass |
| `RH6021…Tests.VerifyUnspacedBaseListAndConstructorInitializerColonsAreReported` | RH6021-001, RH6021-003 | `Verify(source)` | pass |
| `RH6021…Tests.VerifySpacedOrLineBrokenColonsAndOtherColonsAreNotReported` | RH6021-002 | `Verify(source)` | pass |
| `RH6021…Tests.VerifyFixPutsOneSpaceOnEachReportedSide` + CRLF | RH6021-001, RH6021-004, RH6021-005, SHARED-GAP-001; FC-003 | `Verify(source, fixedSource)` | pass |
| `RH6021…Tests.VerifyFixIsNotOfferedWhenCommentIsInRewrittenGap` | RH6021-001, RH6021-005, SHARED-GAP-001; FC-019 | `VerifyNoCodeFixIsOffered` | pass |
| `RH6022…Tests.VerifyWhitespaceAfterNewOfImplicitlyTypedArrayIsReported` + CRLF | RH6022-001, RH6022-002; FC-022, FC-033 | `Verify(source)` | pass |
| `RH6022…Tests.VerifyFixDeletesWhitespaceAfterNewOfImplicitlyTypedArray` + CRLF | RH6022-003, SHARED-WS-001 | `Verify(source, fixedSource)` | pass |
| `RH6022…Tests.VerifyFixIsNotOfferedWhenDirectiveFollowsOnNextLine` + CRLF | RH6022-001, RH6022-003, SHARED-WS-002; FC-005, FC-020 | `VerifyNoCodeFixIsOffered` | pass |
| `RH6022…Tests.VerifyFixIsNotOfferedWhenCommentFollowsOnNextLine` | RH6022-001, RH6022-003, SHARED-WS-002; FC-019 | `VerifyNoCodeFixIsOffered` | pass |
| `RH6023…Tests.VerifyEveryAssignmentAndInitializerOperatorIsChecked` + CRLF | RH6023-001, SHARED-OPSPACE-001, SHARED-OPSPACE-002, SHARED-OPSPACE-003; FC-022 | `Verify(source)` | pass |
| `RH6023…Tests.VerifyFixPutsOneSpaceOnBothSidesOfAssignmentOperators` | RH6023-002, SHARED-OPSPACE-004 | `Verify(source, fixedSource)` | pass |
| `RH6024…Tests.VerifyEveryBinaryOperatorIsChecked` + CRLF | RH6024-001, SHARED-OPSPACE-001, SHARED-OPSPACE-003; FC-022 | `Verify(source)` | pass |
| `RH6024…Tests.VerifyDirectiveConditionsAndPatternsAreNotReported` | RH6024-002 (mis-spaced `is` before patterns, the skeptic's correction) | `Verify(source)` | pass |
| `RH6024…Tests.VerifyFixPutsOneSpaceOnBothSidesOfBinaryOperators` | RH6024-003, SHARED-OPSPACE-004 | `Verify(source, fixedSource)` | pass |
| `RH6024…Tests.VerifyGapThatIsNotExactlyOneSpaceIsReported` + CRLF | SHARED-OPSPACE-001, SHARED-OPSPACE-003; FC-003 (comment on the other side) | `Verify(source)` | pass |
| `RH6024…Tests.VerifyGapsWithOneSpaceLineBreakOrCommentAreNotReported` + CRLF | SHARED-OPSPACE-002; FC-019 | `Verify(source)` | pass |
| `RH6024…Tests.VerifyGapWithDirectiveIsNotReported` + CRLF | SHARED-OPSPACE-002; FC-020 | `Verify(source)` | pass |
| `RH6024…Tests.VerifyFixLeavesOneSpaceInEachGap` | SHARED-OPSPACE-004 | `Verify(source, fixedSource)` | pass |

Existing tests I tagged. Their assertions are unchanged, and all pass:

| Test class | Tagged tests → requirements |
|---|---|
| RH6013 | `VerifyNoDiagnosticsWhenCodeIsClean` → 001. `VerifyIssueIsDetectedAndFixed` → 001, 002, 003, SHARED-WS-001 |
| RH6014 | `VerifyNoDiagnosticsWhenCodeIsClean` → 001. `VerifyIssueIsDetectedAndFixed` → 001, 002, 003, SHARED-WS-001. `VerifyTabBeforeClosingAttributeBracketIsDetectedAndFixed` → 001, 003, SHARED-WS-001. `VerifyContinuationLineClosingAttributeBracketDoesNotProduceDiagnostic` (+ CRLF) → 001. `VerifyDirectiveAndDisabledTextBeforeContinuationLineClosingAttributeBracketDoNotProduceDiagnostic` (+ CRLF) → 001 |
| RH6015 | `VerifyIssueIsDetectedAndFixed` → 001, 003, 004, SHARED-WS-001. `VerifyTabBeforeNullableTypeSymbolIsDetectedAndFixed` → 001, 004, SHARED-WS-001. `VerifyContinuationLineNullableTypeSymbolDoesNotProduceDiagnostic` (+ CRLF) → 001. `VerifyDirectiveAndDisabledTextBeforeContinuationLineNullableTypeSymbolDoNotProduceDiagnostic` (+ CRLF) → 001. `VerifySpaceBeforeNullableQuestionMarkInsideCrefIsIgnored` → 002 |
| RH6016 | `VerifyNoDiagnosticsWhenCodeIsClean`, `VerifyMethodChainsDoNotProduceDiagnostics`, `VerifyTightConditionalAccessDoesNotProduceDiagnostics`, `VerifyTernaryQuestionMarkDoesNotProduceDiagnostics`, `VerifyNullableTypeQuestionMarkDoesNotProduceDiagnostics`, `VerifyLineBrokenConditionalAccessDoesNotProduceDiagnostics` → 002. `VerifyIssueIsDetectedAndFixed`, `VerifyConditionalAccessLeadingSpaceIsDetectedAndFixed`, `VerifySpacedConditionalAccessReportsBothSymbolsAndIsFixed` → 001, 003, 004. `VerifyOnlyExtraneousTrailingSpaceIsRemovedWhenLeadingSideIsLineBroken`, `VerifyOnlyExtraneousLeadingSpaceIsRemovedWhenTrailingSideIsLineBroken`, `VerifyConditionalElementAccessLeadingSpaceIsDetectedAndFixed`, `VerifyPointerMemberAccessSpacesAreDetectedAndFixed` → 001, 004. `VerifyDirectiveOnLineBrokenSideSurvivesFix` → 004. `VerifyCommentOnLineBrokenSideSurvivesFix` → 004, 005, SHARED-GAP-001 |
| RH6017 | `VerifyNoDiagnosticsWhenCodeIsClean` → 001. `VerifyIssueIsDetectedAndFixed` → 001, 003, 004, SHARED-WS-001. `VerifyContinuationLineIncrementWithPrecedingCommentDoesNotProduceDiagnostic` (+ CRLF) → 001. `VerifyLineLeadingIncrementDoesNotProduceDiagnostics`, `VerifyOperatorOverloadDeclarationsDoNotProduceDiagnostics` → 002 |
| RH6018 / RH6019 | `VerifyIssueIsDetectedAndFixed` → 001, 003, 004, SHARED-WS-001. The two `VerifyNoDiagnosticWhenOperandStartsWith…` tests → 002 |
| RH6020 | `VerifyNoDiagnosticsWhenCodeIsClean` → 001. `VerifyIssueIsDetectedAndFixed` → 001, 003, 004, SHARED-WS-001. `VerifyNoDiagnosticWhenAddressOfOperandStartsWithAddressOf` → 002 |
| RH6021 | `VerifyNoDiagnosticsWhenCodeIsClean`, `VerifyBaseListColonOnContinuationLineIsNotFlagged`, `VerifyConstructorInitializerColonOnContinuationLineIsNotFlagged` → 002. `VerifyIssueIsDetectedAndFixed` → 001, 003, 004. `VerifyConstructorInitializerColonIsDetectedAndFixed`, `VerifyOnlyMissingTrailingSpaceIsFixedWhenLeadingSideIsLineBroken`, `VerifyOnlyMissingLeadingSpaceIsFixedWhenTrailingSideIsLineBroken` → 001, 004. `VerifyDirectiveOnLineBrokenSideSurvivesFix` → 004. `VerifyCommentOnLineBrokenSideSurvivesFix` → 004, 005, SHARED-GAP-001 |
| RH6022 | `VerifyNoDiagnosticsWhenCodeIsClean` → 001. `VerifyIssueIsDetectedAndFixed` → 001, 002, 003, SHARED-WS-001 |
| RH6023 | `VerifyNoDiagnosticsWhenCodeIsClean`, `VerifyWrappedAssignmentIsIgnored` → 001, SHARED-OPSPACE-002. The six `…AssignmentIsDetectedAndFixed` tests → 001, 002, SHARED-OPSPACE-001, SHARED-OPSPACE-004, and SHARED-OPSPACE-003 where both sides are wrong. `VerifyAssignmentInsideStringLiteralIsIgnored`, `VerifyRegisteredKindsCoverEveryAssignmentExpressionKind` → 001 |
| RH6024 | `VerifyNoDiagnosticsWhenCodeIsClean`, `VerifyWrappedBinaryOperatorIsIgnored`, `VerifyWrappedKeywordOperatorIsIgnored` → 001, SHARED-OPSPACE-002. The six `…IsDetectedAndFixed` tests → 001, 003, SHARED-OPSPACE-001, SHARED-OPSPACE-004, and SHARED-OPSPACE-003 where both sides are wrong. `VerifyGenericTypeArgumentsAreIgnored`, `VerifyRegisteredKindsCoverEveryBinaryExpressionKind` → 001. `VerifyDirectiveConditionOperatorIsIgnored` → 002 |

Observation, not a finding: the RH6015-001 example field `int ?[] _c;` under `#nullable enable` raises CS8618, and the repository verifier reports nullable warnings as errors (`CSharpVerifierHelper.GetNullableWarnings`). The test declares it with markup `{|CS8618:_c|}`, which leaves the source text unchanged. The spec example itself is fine, because CS8618 is a warning in a normal compilation.

## Scenarios (bug fix)

_N/A._ No bug fix in this run.

## Not covered, and why

These existing tests are deliberately left untagged:

- Same-line or next-line "comment, then whitespace, then the checked token" cases, which by the lead's ruling are thoroughly covered by thoenissen/Reihitsu#1078 and are out of the specs. They stay as characterization tests:
  - RH6014 `VerifyFixIsNotOfferedForSameLineClosingAttributeBracketWithPrecedingComment` (+ CRLF) and `VerifyFixIsNotOfferedForContinuationLineClosingAttributeBracketWithPrecedingComment` (+ CRLF);
  - RH6015 `VerifyFixIsNotOfferedForSameLineNullableTypeSymbolWithPrecedingComment` (+ CRLF) and `VerifyFixIsNotOfferedForContinuationLineNullableTypeSymbolWithPrecedingComment` (+ CRLF);
  - RH6017 `VerifyFixIsNotOfferedForSameLineIncrementWithPrecedingComment` (+ CRLF);
  - RH6016 `VerifyNoActionIsOfferedWhenCommentIsInFlaggedGap` (`string. /* comment */Empty`).
- RH6021 `VerifyNoActionIsOfferedWhenCommentIsInFlaggedGap` (`:/* comment */`). This is open question Q3, thoenissen/Reihitsu#1108, and is out of the spec.
- Tests that feed the provider a hand-made diagnostic on a token the analyzer does not report: RH6016 and RH6021 `VerifyNoActionIsOfferedWhen…AlreadySpaced`/`…NeitherSideHasExtraSpace`, and RH6020 `VerifyFixIsNotOfferedForNestedAddressOf`. No requirement states provider behavior for an unreported location.
- RH6017 `VerifyMalformedStandalonePrefixIncrementDoesNotProduceDiagnostics`: syntax-invalid input, which no requirement covers.

Candidates and failure classes left out:

- FC-008 / FC-026 / FC-027, the skeptic's N2 Fix All shapes (`a+b+c`, `a==b&&b>0`, `d[k=1]=2`, `s ? . Length`, …): not encoded. Fix All needs more than one pass today, which violates SHARED-FIX-002. A test of the specified result fails, and encoding the current result would assert a defect. They are for the lead to file. No spec example contains them, and every spec block passed Fix All in its `Verify` run.
- FC-009, Q2/N3 (no-op operator fix on a file with a syntax error or a case-mismatched `<AUTO-GENERATED>` marker): not encoded, for the same reason. Both are filed or decided by P7.
- FC-030, Q1/N4 (the operator fix formats the whole node): no test asserts the whole-node result. The spec blocks have no lambda, initializer, or wrap next to a reported operator.
- FC-013, N1 (explicit-interface dot): out of the spec, open question.
- The RH6023/RH6024 fix with a comment in one gap (`a /* c */=1`): SHARED-OPSPACE-004 does not constrain a gap that holds a comment, and the result touches thoenissen/Reihitsu#1077. No test was added. The analyzer side, a comment on one side and a reported other side, is covered by `VerifyGapThatIsNotExactlyOneSpaceIsReported`.
- A directive in a reported gap, for RH6016, RH6021, RH6023, and RH6024: a directive always brings a line break, and a gap with a line break is never reported (RH6016-002, RH6021-002, SHARED-OPSPACE-002). The directive cases on line-broken sides are covered by `RH6016…VerifyFixRemovesWhitespaceOnEachReportedSideOnly`, `RH6016…`/`RH6021…VerifyDirectiveOnLineBrokenSideSurvivesFix`, and `RH6024…VerifyGapWithDirectiveIsNotReported`.
- A directive for RH6014, RH6015, and RH6017, where the fix deletes the run in front of the token: a directive in front of the token puts it at the start of a line, which is not reported. That is covered by the tagged `…DirectiveAndDisabledText…` tests.
- FC-010 / FC-012 / FC-028 / FC-029 (formatter stability, layout pairs): no formatter or layout change in this lane.
- FC-018 / FC-036 (renames), FC-023 / FC-024 / FC-025: no naming fix, and no construct of these rules depends on identifier shape or member kind.
- FC-035: expected values come from the spec blocks, not from running the code.
- One-pass convergence of each fix (FC-007): covered by `Verify(source, fixedSource)`, which applies the fixes and re-analyzes the result.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH601|FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH602"
scripts/test.sh --project architecture --filter "FullyQualifiedName~RequirementCoverageTests"
```
