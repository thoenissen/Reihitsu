Role: test implementer
Commit: e4ab9df
Verdict: tests written

The prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for. This round derives the tests the second amendment requires (`06-spec-author.md`, decision `2026-10-25-phase-prediction-removal-first-on-line-anchor.md`, approval `2026-10-25-phase-prediction-removal-second-amendment.md`). It also changes the existing tests whose expected output the approved spec changes.

Expected values come from:
- the spec examples of SHARED-INIT-002, RH5303-003, RH5304-002, FMT-INDENT-023, and FMT-INDENT-030;
- the columns in the change doc's second table.

The lines that the spec and the change doc leave open, such as the blank line above a comment that follows `=` or `return`, are taken from today's output. The change doc says they stay. Every fixed output and every "new layout" source was checked to be a fixed point of today's document formatter (`scripts/format.sh` on stripped copies in the scratchpad). The columns stated in the change doc match it for every row except the nested initializer.

"Fails" below means the test fails on e4ab9df plus these files, for the reason given. "Passes" means it passes there and must keep passing. Columns are 0-based.

## Tests

### Existing tests changed because the spec changes their expected output

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Reihitsu.Analyzer.Test.Formatting.RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzerTests.VerifyNoDiagnosticForCommentPrefixedComplexElementTokens`. Source changed to the new layout: brace in 45, members in 49, `/* Keep close. */ }` in 45. | SHARED-INIT-001, SHARED-INIT-002, RH5303-003, SHARED-COMMENT-011 (tags unchanged) | `Verify(source)` | fails: RH5303 reports the complex element (11,46)-(15,65) |
| `Reihitsu.Analyzer.Test.Formatting.RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzerTests.VerifyNoDiagnosticForCommentPrefixedComplexElementTokens`. Source changed: brace in 57, members in 61, close in 57. | SHARED-INIT-001, SHARED-INIT-002, RH5304-002 (tags unchanged) | `Verify(source)` | fails: RH5304 reports the assignment |
| `Reihitsu.Analyzer.Test.Formatter.Formatting.RH5303CollectionInitializerShouldBeFormattedCorrectlyFormatterTests.VerifyFormatterKeepsCommentPrefixedComplexElementStable`. Source changed to the new layout (45/49/45), which is the output in `05-implementer.md`. | SHARED-INIT-002, FMT-INDENT-023, FMT-INDENT-030 (newly tagged) | `VerifyFormatter(source)`, LF+CRLF | fails: RH5303 reports the source |
| `Reihitsu.Analyzer.Test.Formatter.Formatting.RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyFormatterTests.VerifyFormatterKeepsCommentPrefixedComplexElementStable`. Source changed (57/61/57). | SHARED-INIT-002, FMT-INDENT-023, FMT-INDENT-030 (newly tagged) | `VerifyFormatter(source)`, LF+CRLF | fails: RH5304 reports the source |

`VerifyFormatterKeepsCommentPrefixedCollectionStable` (`/* Keep open. */ {` of a creation initializer) keeps its expectation, as `06-spec-author.md` states. It passes.

### New analyzer and code-fix tests: `RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzerTests`

All are on `Verify(source, fixedSource, diagnostics)` unless they are "no diagnostic". The verifier also runs the Fix All modes on the fixed source.

| Test | Requirement / failure class | Expected now |
|---|---|---|
| `VerifyCommentPrefixedComplexElementMeasuredFromTheCommentIsDetectedAndFixed`: the former clean layout (29/25) is reported on the whole complex element and fixed to 45/49/45 | SHARED-INIT-002, RH5303-003, RH5303-004, RH5303-005 | fails: no diagnostic |
| `VerifyNoDiagnosticForComplexElementAlignedToItsBraceBehindLeadingComment`: RH5303-003 example 1 (`/* e */ {`, `/* k */ 1` and `2` in 32, `/* c */ }` in 28) | SHARED-INIT-002, RH5303-003 | fails: reported (9,29)-(12,38) |
| `VerifyComplexElementAlignedToTheLeadingCommentIsDetectedAndFixed`: RH5303-003 example 2, reported on the whole complex element and fixed to the example 1 layout | SHARED-INIT-002, RH5303-003, RH5303-004, RH5303-005 | fails: no diagnostic |
| `VerifyExpressionOutOfPlaceInComplexElementBehindLeadingCommentIsDetectedAndFixed`: RH5303-003 example 3 (`1` in 24, `2` in 32, `}` in 28), reported on `1` and fixed to `1` in 32 | SHARED-INIT-002, RH5303-003, RH5303-004, RH5303-005 | fails: today's diagnostic is on the whole element (9,29)-(12,30), not on `1` (10,25) |
| `VerifyNoDiagnosticForComplexElementsWithCommentInFrontOfTheBraceAndOnTheLineAbove`: SHARED-INIT-002 example 1. Both sides of the qualifier: `/* e */ {` measured from 28, and `// f` above with the brace at 20. | SHARED-INIT-002, RH5303-003 | fails: first element reported |
| `VerifyComplexElementMeasuredFromTheCommentInFrontOfItsBraceIsDetectedAndFixed`: SHARED-INIT-002 example 2, reported on the whole complex element and fixed to 32/28 | SHARED-INIT-002, RH5303-003, RH5303-004, RH5303-005 | fails: no diagnostic |
| `VerifyNoDiagnosticForComplexElementAlignedToItsBraceBehindTheLastLineOfAMultiLineComment`: `/* a` / `   b */ {` with the brace in 28, the expressions in 32, and `}` in 28 | SHARED-INIT-002, RH5303-003 | fails: reported (10,29)-(13,30) |
| `VerifyComplexElementBehindTheLastLineOfAMultiLineCommentMeasuredFromTheCommentIsDetectedAndFixed`: the same element with the expressions in 24 and `}` in 20, reported and fixed to 32/28 | SHARED-INIT-002, RH5303-003, RH5303-004, RH5303-005; FC-028 | fails: no diagnostic. Once the analyzer reports it, the fix path also has to give 32/28. See the FC-028 tests below, which fail today on the target the RH5303 fix formats. |
| `VerifyTwoComplexElementsMeasuredFromTheirLeadingCommentsAreDetectedAndFixed`: two comment-led elements in one initializer, two diagnostics, fixed together | SHARED-INIT-002, RH5303-003, RH5303-004, RH5303-005; FC-008 | fails: no diagnostic |

### New analyzer and code-fix tests: `RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzerTests`

| Test | Requirement / failure class | Expected now |
|---|---|---|
| `VerifyCommentPrefixedComplexElementMeasuredFromTheCommentIsDetectedAndFixed`: the former clean layout (41/37) is reported on the whole assignment and fixed to 57/61/57 | SHARED-INIT-002, RH5304-002, RH5304-003, RH5304-004 | fails: no diagnostic |
| `VerifyNoDiagnosticForComplexElementAlignedToItsBraceBehindLeadingComment`: RH5304-002 new example 1 (`D = {` / `/* e */ {`, expressions in 40, `}` in 36) | SHARED-INIT-002, RH5304-002 | fails: reported (13,21)-(18,26) |
| `VerifyComplexElementAlignedToTheLeadingCommentIsDetectedAndFixed`: RH5304-002 new example 2 (32/28), reported on the whole assignment and fixed to example 1 | SHARED-INIT-002, RH5304-002, RH5304-003, RH5304-004 | fails: no diagnostic |
| `VerifyNoDiagnosticForComplexElementAlignedToItsBraceBehindTheLastLineOfAMultiLineComment`: `/* a` / `   b */ {` with the brace in 36, expressions in 40, `}` in 36 | SHARED-INIT-002, RH5304-002 | fails: reported |
| `VerifyComplexElementBehindTheLastLineOfAMultiLineCommentMeasuredFromTheCommentIsDetectedAndFixed`: the same element at 32/28, reported and fixed to 40/36 | SHARED-INIT-002, RH5304-002, RH5304-003, RH5304-004; FC-028 | fails: no diagnostic. `scripts/apply-fix.sh RH5304` on a variant that today's analyzer reports (outer `}` misplaced) gives 46/42 for this element, not 40/36. |
| `VerifyTwoAssignmentsWithComplexElementsMeasuredFromTheirLeadingCommentsAreDetectedAndFixed`: `D = {` and `E = {` each with a comment-led complex element, two diagnostics, fixed together | SHARED-INIT-002, RH5304-002, RH5304-003, RH5304-004; FC-008 | fails: no diagnostic |

### New formatter parity tests in `Reihitsu.Analyzer.Test.Formatter.Formatting`

All use `VerifyFormatter(source, fixedSource, diagnostics)`. It checks the diagnostic before formatting, the output, analyzer-clean output, and a second pass, under LF and CRLF.

| Test | Requirement / failure class | Expected now |
|---|---|---|
| `RH5303…FormatterTests.VerifyFormatterAlignsCommentPrefixedComplexElementToItsBrace`: 29/25 → 49/45 | SHARED-INIT-002, FMT-INDENT-023, FMT-INDENT-030; FC-013 | fails: no diagnostic before formatting |
| `RH5303…FormatterTests.VerifyFormatterAlignsComplexElementToItsBraceBehindTheLastLineOfAMultiLineComment`: `/* Keep` / `   element. */ {` with the brace in 40, expressions 29/25 → 44/40 | SHARED-INIT-002, FMT-INDENT-023, FMT-INDENT-030; FC-013 | fails: no diagnostic before formatting |
| `RH5303…FormatterTests.VerifyFormatterAlignsCollectionCreationToNewBehindCommentThatLeadsTheLine`: change doc row 5 (collection creation). Today's output, braces in 8, is reported on the creation. The output after the change is braces in 16 and the element in 20. | RH5303-001, FMT-INDENT-022, FMT-INDENT-030; FC-013 | passes |
| `RH5304…FormatterTests.VerifyFormatterAlignsCommentPrefixedComplexElementToItsBrace`: 41/37 → 61/57 | SHARED-INIT-002, FMT-INDENT-023, FMT-INDENT-030; FC-013 | fails: no diagnostic before formatting |
| `RH5304…FormatterTests.VerifyFormatterAlignsComplexElementToItsBraceBehindTheLastLineOfAMultiLineComment`: brace in 52, 41/37 → 56/52 | SHARED-INIT-002, FMT-INDENT-023, FMT-INDENT-030; FC-013 | fails: no diagnostic before formatting |
| `RH5301ObjectInitializerShouldBeFormattedCorrectlyFormatterTests.VerifyFormatterAlignsObjectCreationToNewBehindCommentThatLeadsTheLine`: change doc row 5 (object creation). Braces 8 → 16, member 12 → 20. | RH5301-001, FMT-INDENT-022, FMT-INDENT-030; FC-013 | passes |
| `RH5305CollectionExpressionsShouldBeFormattedCorrectlyFormatterTests.VerifyFormatterAlignsCollectionExpressionToItsBracketBehindCommentThatLeadsTheLine`: change doc row 4 (collection expression). Elements 12 → 20, `]` 8 → 16. | RH5305-001, FMT-INDENT-025, FMT-INDENT-030; FC-013 | passes |

### New formatter tests: `Reihitsu.Formatter.Test.Regression.Indentation.CommentLedFirstTokenAnchorTests`

All tests on `AssertRuleResult` run two passes under LF and CRLF.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ComplexElementAlignsToItsBraceBehindCommentThatLeadsTheLine`: the FMT-INDENT-030 spec example, including the `// f` element that keeps 20/24 | FMT-INDENT-030, FMT-INDENT-023 | `AssertRuleResult` | passes |
| `ComplexElementAlignsToItsBraceBehindTheLastLineOfAMultiLineComment` (brace 28 → 32/28) | FMT-INDENT-030, FMT-INDENT-023 | same | passes |
| `NestedInitializerAlignsToItsBraceBehindCommentThatLeadsTheLine`: the FMT-INDENT-023 spec example (`X` in 32, `}` in 28) | FMT-INDENT-023, FMT-INDENT-030 | same | **fails**: `X` in 24, `}` in 20 (today's output) |
| `NestedInitializerBelowCommentOnTheLineAboveKeepsItsBraceAtTheMemberColumn`: the qualifier's other side | FMT-INDENT-023, FMT-INDENT-030 | `AssertRuleResult(source)` | passes |
| `ArrayInitializerAlignsToItsBraceBehindCommentThatLeadsTheLine` (row 3: 20/16) | FMT-INDENT-023, FMT-INDENT-030 | `AssertRuleResult` | passes |
| `ArrayInitializerBelowCommentOnTheLineAboveKeepsItsBraceAtTheLineColumn` | FMT-INDENT-023, FMT-INDENT-030 | `AssertRuleResult(source)` | passes |
| `CreationInitializerBraceBehindCommentThatLeadsTheLineMovesNoOtherLine` (`/* o */ {` anchors nothing) | FMT-INDENT-022, FMT-INDENT-030 | `AssertRuleResult(source)` | passes |
| `CollectionExpressionAlignsToItsBracketBehindCommentThatLeadsTheLine` (row 4) | FMT-INDENT-025, FMT-INDENT-030 | `AssertRuleResult` | passes |
| `CollectionExpressionBelowCommentOnTheLineAboveKeepsItsBracketAtTheLineColumn` | FMT-INDENT-025, FMT-INDENT-030 | `AssertRuleResult(source)` | passes |
| `ListPatternAlignsToItsBracketBehindCommentThatLeadsTheLine` (row 4, behind `is`) | FMT-INDENT-025, FMT-INDENT-030 | `AssertRuleResult` | passes |
| `ObjectCreationAlignsToNewBehindCommentThatLeadsTheLine`, `CollectionCreationAlignsToNewBehindCommentThatLeadsTheLine` (row 5) | FMT-INDENT-022, FMT-INDENT-030 | same | pass |
| `AnonymousObjectAlignsToNewBehindCommentThatLeadsTheLine` (row 5) | FMT-INDENT-024, FMT-INDENT-030 | same | passes |
| `SwitchExpressionAlignsToGoverningExpressionBehindCommentThatLeadsTheLine` (row 6) | FMT-INDENT-027, FMT-INDENT-030 | same | passes |
| `LambdaBlockAlignsToLambdaBehindCommentThatLeadsTheLine` (row 7) | FMT-INDENT-028, FMT-INDENT-030 | same | passes |
| `BinaryOperatorAlignsToLeftOperandBehindCommentThatLeadsTheLine` (row 8) | FMT-INDENT-018, FMT-INDENT-030 | same | passes |
| `ConditionalBranchesAlignToConditionBehindCommentThatLeadsTheLine` (row 9) | FMT-INDENT-020, FMT-INDENT-030 | same | passes |
| `ChainLinkAlignsToFirstLinkOfChainThatStartsBehindCommentThatLeadsTheLine` (row 10) | FMT-INDENT-029, FMT-INDENT-030 | same | passes |
| `PatternCombinatorAlignsToIsBehindCommentThatLeadsTheLine` (row 11) | FMT-INDENT-019, FMT-INDENT-030 | same | passes |
| `FormatNodeInDocumentAlignsComplexElementToItsBraceBehindTheLastLineOfAMultiLineComment`: the outer braces are aligned. Each target is formatted twice, under LF and CRLF, through `FormatNodeInDocumentAsync`: first the `LocalDeclarationStatementSyntax`, then the `EqualsValueClauseSyntax`, which is the node the RH5303 fix's `TryGetFormatTarget` selects for a declaration. Expected: the document-level output, 32/28. | FMT-INDENT-030; FC-028 | `AdhocWorkspace` | **fails**: on the `EqualsValueClauseSyntax` target the expressions land in 38 and `}` in 34. The statement target passes. |
| `FormatNodeInDocumentAlignsComplexElementToItsBraceBehindTheLastLineOfAMultiLineCommentWhenTheInitializerBraceMoves`: the outer `{` is in 14. This is the fixture in `06-spec-author.md`. The test also asserts `AssertRuleResult` (document level: outer brace 16, 32/28). | FMT-INDENT-030; FC-028 | `AssertRuleResult` + `AdhocWorkspace` | **fails**: same as above on the equals-value target (38/34); the document level passes |
| `FormatNodeInDocumentAlignsComplexElementToItsBraceBehindCommentThatLeadsTheLine`: the one-line side (`/* e */ {`), both targets | FMT-INDENT-030; FC-028 | `AdhocWorkspace` | passes |

## Scenarios

This is a behavior change, not a bug fix, so there are no reproductions. Observations that bear on the failing tests:

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Nested initializer `Child =` / `/* c */ {` / `X = 2` / `}` | LF, CRLF | formatter, document | fails | `X` 24, `}` 20 vs 32/28 (FMT-INDENT-023 example) |
| Complex element behind `/* a` / `b */ {`, outer `{` in 14 | LF, CRLF | `scripts/apply-fix.sh RH5303` | no-progress | 38/34 vs 32/28 (document format gives 32/28) |
| Same element in `D = {` with the outer `}` misplaced | LF, CRLF | `scripts/apply-fix.sh RH5304` | no-progress | 46/42 vs 40/36. The one-line variant `/* e */ {` gives 40/36 through the same fix. |
| Same element, `FormatNodeInDocumentAsync` on the equals-value clause | LF, CRLF | formatter, node level | fails | 38/34 vs 32/28. The statement target gives 32/28. |

## Not covered, and why

- **Nested complex element** (FC-027; the recursive branch of the complex-element check): `{ 1, { 2, 3 } }` inside a collection initializer is a syntax error. `scripts/format.sh` skips such a fixture as "syntax errors". No valid input reaches that member.
- **Two comments in front of the brace on its line**: SHARED-INIT-001 covers a token "in front of which at most one comment starts on the token's own line". Two comments are outside the requirement the analyzer change follows. The formatter side of two leading comments is covered by `CommentLedLineAlignmentTests.ArgumentAlignsToFirstArgumentBehindTwoCommentsThatLeadTheLine` from the earlier round.
- **`/** */` in front of a complex element's brace**: not added. A delimited documentation comment that leads a line is covered on the formatter side (`ArgumentAlignsToFirstArgumentBehindDelimitedDocumentationCommentThatLeadsTheLine`, earlier round). The analyzer side has no spec example for it.
- **A directive in the gap** (FC-019, FC-020): a directive fills its own line, so it can only stand above the brace. That is the "comment on the line above" side, where the brace starts its line. The `// f` and the `…BelowCommentOnTheLineAbove…` tests cover that side.
- **A one-line comment led by the closing brace's line**: the closing brace led by a same-line comment is in RH5303-003 example 1/2 (`/* c */ }`) and in the Data fixtures (`/* Keep close. */ }`). A comment that starts above a closing brace is unchanged by SHARED-INIT-002. The existing `VerifyStandaloneCommentBeforeComplexCloseBraceIsDetectedAndFixed` (RH5304) covers it, and it passes.
- **Line endings of the analyzer tests**: `Verify` runs one line ending, as every analyzer test does. Line-ending dimensions of the layout are covered by the `VerifyFormatter`, `AssertRuleResult`, and `AdhocWorkspace` tests above, which run LF and CRLF.
- **Rows 6–11 on an analyzer**: no rule checks the switch-expression braces, the lambda block under the lambda, the binary operator, the conditional branches, or the pattern combinator against a comment-led anchor. RH5201 (chain) was not checked for row 10 (follow-up candidate 2).
- **Failure classes that cannot reach this round**:
  - FC-004, FC-005, FC-020, FC-021 as join cases: no line is joined and no token is moved across a gap.
  - FC-006, FC-018, FC-022–FC-025, FC-029, FC-036: no syntax rewrite, no rename, no literal, no member kind.
  - FC-009: the fixes register only for a reported diagnostic, and the reported shapes all change the document.
  - FC-014: no new rule.
  - FC-031, FC-032, FC-037: for the auditor.

## Follow-up candidates

1. RH5303's `TryGetFormatTarget` formats the `EqualsValueClauseSyntax` of a declaration. The `StatementSyntax` would be the wider candidate, but the equals-value clause is found first among the ancestors. Only the equals-value target shows the multi-line divergence above. This is an observation for the implementer's diagnosis, not a proposed fix. The change doc lists the node-level column steps (`ReihitsuFormatterHelpers.ComputeTokenColumn`, `AdjustNodeIndentation`) under "Not changed here". If the failing FC-028 tests need them changed, that may be escalation 5.
2. RH5201 against a chain whose first token stands behind a comment that leads its line (change doc row 10) was not checked.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~CommentLedFirstTokenAnchorTests|FullyQualifiedName~CommentLedLineAlignmentTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5301|FullyQualifiedName~RH5303|FullyQualifiedName~RH5304|FullyQualifiedName~RH5305"
scripts/test.sh --project architecture
```

Observed on e4ab9df plus these files:
- full formatter project: `Failed: 3, Passed: 2970, Total: 2973`. The three failures are `NestedInitializerAlignsToItsBraceBehindCommentThatLeadsTheLine` and the two multi-line `FormatNodeInDocument…` tests.
- the analyzer filter: 23 failed. They are the tests listed as "fails" above. All others passed.
- full analyzer project: `Failed: 23, Passed: 6179, Total: 6202`. The 23 failures are the same 23 tests as in the filter run. The two tests disputed in `05-implementer.md` are among them in their changed form. No other analyzer test fails.
- architecture project: `Passed: 84, Failed: 0`. That run includes the requirement-tag checks for the new tags.
