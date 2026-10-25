Role: test implementer
Commit: 9d9e7c1
Verdict: tests written

The prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for. This is repair round 2. It follows the decisions recorded at the end of `00-lead.md` and the specs as of 9d9e7c1 (`18-spec-author.md`, `19-spec-author.md`, `21-spec-author.md`, and the "For the test implementer" section of `20-skeptic.md`).

Expected values come from three sources:
- the spec examples (FMT-PIPE-017, FMT-RAW-005, FMT-RAW-007, FMT-INDENT-006, RH5303-005, RH5304-004);
- the change doc's third table (a01, a02);
- the skeptic's stated outputs (h1, the C2a input, the C4 input, the comment-led misindented statement).

For the inputs whose element stands behind a multi-line comment's last line, `20-skeptic.md` advises asserting "fix output equals document-level output" rather than pinning columns, because thoenissen/Reihitsu#1382 will change those outputs. Those tests do exactly that.

I checked every such input with `scripts/format.sh`: document-level formatting changes only the code the fix formats.

"Fails" below means the test fails on 9d9e7c1 plus these files, for the reason given. "Passes" means it passes there and must keep passing. Columns are 0-based. A `→` stands for a tab; the test sources write tabs as `\t`.

## Tests

### New shared helper

`AnalyzerTestsBase<TAnalyzer, TCodeFix>.VerifyCodeFixMatchesFormatter(source)` (`src/Reihitsu.Analyzer.Test/Base/AnalyzerTestsBase{TAnalyzer,TCodeFix}.cs`) does four things:
1. checks the diagnostics marked in the source;
2. applies the first offered action to the first diagnostic (`ApplyCodeFixAsync`);
3. asserts that the result equals `Format(source)`, the formatter's output for the whole document;
4. asserts that the analyzer reports nothing on the result (FC-007, FC-013).

### FMT-PIPE-017 at the entry points: `Reihitsu.Formatter.Test.Unit.ReihitsuFormatterNodeColumnTests` (new)

`FormatNodeInDocumentAsync`, or `…WithContextAsync` where stated, on an `AdhocWorkspace` document. Each test checks the first pass and that a second pass changes nothing, under LF and CRLF.

| Test | Requirement / failure class | Expected now |
|---|---|---|
| `EqualsValueTargetBehindTheLastLineOfAMultiLineCommentIsLaidOutFromNewWhereItStands` (spec example v1: braces at 24, `1` at 28; also `AssertRuleResult` on the document) | FMT-PIPE-017, FMT-INDENT-030; FC-028 | fails: braces at 30 |
| `EqualsValueTargetHoldingRawStringMovesContentUnderTheOpeningQuotes` (spec example: content and closing at 16) | FMT-PIPE-017, FMT-RAW-001; FC-022, FC-028 | fails: content and closing at 10 |
| `StatementTargetIndentedWithTabsCountsEachOfItsTabsAsFourColumns` (spec example: `Call(1,` at 8 and `2);` at 13; the method lines keep their tabs) | FMT-PIPE-017, FMT-TAB-001 | fails: 2 and 7 |
| `EqualsValueTargetBehindTabsCountsEachTabInFrontOfItAsOneColumn` (spec example n1: ten spaces before the braces) | FMT-PIPE-017, SHARED-FIX-007 | passes |
| `EqualsValueTargetBehindTabInsideCommentCountsTheTabAsOneColumn` (C2a: braces at `new`'s character column, 22) | FMT-PIPE-017, FMT-TAB-003 | passes |
| `MisindentedStatementTargetLedByCommentKeepsItsFirstTokenAndAlignsTheWrappedArgument` (`     /* c */ Call(1,` stays, `2` at 18) | FMT-PIPE-017 | passes |
| `MethodTargetInTwoSpaceClassKeepsItsLinesRelativeToItsFirstToken` (reading X) | FMT-PIPE-017 | passes |
| `TypeTargetKeepsItsLeadingCommentAsItStands` (C4: `// c` stays at 0 above `  class D`) | FMT-PIPE-017, FMT-PIPE-010 | passes |
| `ChainTargetWithContextBehindTheLastLineOfAMultiLineCommentAlignsTheWrappedLink` (`FormatNodeInDocumentWithContextAsync`: the invocation is the target, the equals-value clause the context, and `.ToUpper` lands at 25 under `.Trim`; also `AssertRuleResult`) | FMT-PIPE-017, FMT-INDENT-029; FC-028 | fails: `.ToUpper` at 31 |

### FMT-PIPE-017 through code fixes: `Reihitsu.Analyzer.Test.Formatting.RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests`

All new tests use `Verify(source, fixedSource, diagnostics)`.

| Test | Requirement / failure class | Expected now |
|---|---|---|
| `VerifyFixLaysOutMethodIndentedWithTabsFromTheColumnTheTabsStandFor` (a01: `int Get()` at 8, `return 1;` at 12, class lines keep their tabs) | FMT-PIPE-017, FMT-TAB-001 | fails: 2 and 6 |
| `VerifyFixMovesRawStringOfMethodIndentedWithTabsUnderTheOpeningQuotes` (a02: quotes, content, and closing at 19) | FMT-PIPE-017, FMT-RAW-001 | fails |
| `VerifyFixKeepsMethodOfTwoSpaceClassInItsColumn` (n4, first half) | FMT-PIPE-017 | passes |
| `VerifyFixMovesRawStringOfMethodInTwoSpaceClassUnderTheOpeningQuotes` (n4, second half: everything at 13) | FMT-PIPE-017, FMT-RAW-001 | fails: content and closing at 15 |
| `VerifyFixMovesInterpolatedRawStringOfMethodInTwoSpaceClassWithItsHoleLines` (h1: every line of the literal at 14, `x` at 18) | FMT-PIPE-017, FMT-RAW-001, FMT-RAW-007; FC-022 | fails: `a {` and the closing marker at 16 |

### RH5303-005 and retired RH5303-006: `RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzerTests`

| Test | Change | Requirement / failure class | Expected now |
|---|---|---|---|
| `VerifyComplexElementBehindTheLastLineOfAMultiLineCommentIsDetectedAndFixedAsTheFormatterLaysItOut` | Rewritten from `…NoCodeFixIsOffered`; same input; `VerifyCodeFixMatchesFormatter` | RH5303-005, FMT-PIPE-017, SHARED-INIT-002, RH5303-003, RH5303-004 | fails: no action is registered (the guard) |
| `VerifyComplexElementBelowAMultiLineCommentThatEndsAboveItsBraceIsFixed` | Retagged; RH5303-006 removed | RH5303-003, RH5303-004, RH5303-005 | passes |
| `VerifyMisplacedBracesAroundComplexElementBehindTheLastLineOfAMultiLineCommentAreDetectedAndFixedAsTheFormatterLaysThemOut` | Rewritten from `…NoCodeFixIsOffered` | RH5303-001, RH5303-004, RH5303-005, FMT-PIPE-017 | fails: no action |
| `VerifyMisplacedBracesAroundComplexElementBehindLeadingCommentAreDetectedAndFixed` | Retagged | RH5303-001, RH5303-004, RH5303-005 | passes |
| `VerifyComplexElementBehindTheLastLineOfAMultiLineCommentInAssignmentStatementIsDetectedAndFixedAsTheFormatterLaysItOut` | Rewritten | RH5303-005, FMT-PIPE-017, SHARED-INIT-002, RH5303-003 | fails: no action |
| `VerifyComplexElementBehindTheLastLineOfAMultiLineCommentInExpressionBodyIsDetectedAndFixedAsTheFormatterLaysItOut` | Rewritten | RH5303-005, FMT-PIPE-017, SHARED-INIT-002, RH5303-003; FC-027 | fails: no action |
| `VerifyFixAllLaysOutTwoCreationsBehindTheLastLineOfAMultiLineCommentFromNew` | Replaces `VerifyFixAllFixesOnlyTheCreationWithoutComplexElementBehindTheLastLineOfAMultiLineComment`. Two v1 creations, both fixed to 24/28. The `MarkupMode.Allow` lines of cceed9a and the `Microsoft.CodeAnalysis.Testing` using are removed. | RH5303-005, FMT-PIPE-017; FC-008 | fails: braces at 30 |
| `VerifyCreationBehindTheLastLineOfAMultiLineCommentIsFixedFromNewWhereItStands` | New; spec example v1 | RH5303-005, FMT-PIPE-017 | fails: braces at 30 |
| `VerifyCreationBehindTabsIsFixedFromNewInItsCharacterColumn` | New; n1 through the fix. The verifier's re-analysis shows that the result is not reported again (SHARED-FIX-001). | RH5303-005, FMT-PIPE-017, SHARED-FIX-007 | passes |
| `VerifyObjectCreationElementBehindTheLastLineOfAMultiLineCommentIsFixedAsTheFormatterLaysItOut` | New; auditor member | RH5303-005, FMT-PIPE-017; FC-016 | fails: output differs from the formatter's |
| `VerifyCollectionCreationElementBehindTheLastLineOfAMultiLineCommentIsFixedAsTheFormatterLaysItOut` | New; auditor member | same | fails: same |
| `VerifyCallElementBehindTheLastLineOfAMultiLineCommentIsFixedAsTheFormatterLaysItOut` | New; auditor member | same | fails: same |
| `VerifyLogicalElementBehindTheLastLineOfAMultiLineCommentIsFixedAsTheFormatterLaysItOut` | New; auditor member | same | fails: same |
| `VerifyObjectCreationElementBehindTheLastLineOfAMultiLineCommentInExpressionBodyIsFixedAsTheFormatterLaysItOut` | New; auditor member, arrow target | same; FC-027 | fails: same |

The auditor-member tests and the four rewritten tests assert "equals the formatter's output", not columns.

### RH5304-004 and retired RH5304-005: `RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzerTests`

| Test | Change | Requirement / failure class | Expected now |
|---|---|---|---|
| `VerifyComplexElementBehindTheLastLineOfAMultiLineCommentIsDetectedAndFixedAsTheFormatterLaysItOut` | Rewritten from `…NoCodeFixIsOffered` | RH5304-004, FMT-PIPE-017, SHARED-INIT-002, RH5304-002, RH5304-003 | fails: no action |
| `VerifySplitAssignmentNextToComplexElementBehindTheLastLineOfAMultiLineCommentIsDetectedAndFixedAsTheFormatterLaysItOut` | Rewritten | RH5304-001, RH5304-003, RH5304-004, FMT-PIPE-017 | fails: no action |
| `VerifySplitAssignmentNextToComplexElementBehindLeadingCommentIsDetectedAndFixed`, `VerifyComplexElementBelowAMultiLineCommentThatEndsAboveItsBraceIsFixed` | Retagged; RH5304-005 removed | as before, without RH5304-005 | pass |
| `VerifyFixAllLaysOutAssignmentsOfTwoCreationsBehindTheLastLineOfAMultiLineComment` | Replaces the old Fix All test. Two v2 creations; `MarkupMode.Allow` removed. | RH5304-004, FMT-PIPE-017; FC-008 | fails |
| `VerifyAssignmentInCreationBehindTheLastLineOfAMultiLineCommentIsFixedFromWhereTheCreationStands` | New; spec example v2: `L = {` at 28, `1` at 36, `}` at 32 | RH5304-004, FMT-PIPE-017 | fails: creation braces at 30 |
| `VerifySplitAssignmentWithObjectCreationElementBehindTheLastLineOfAMultiLineCommentIsFixedAsTheFormatterLaysItOut` | New; auditor member | RH5304-004, FMT-PIPE-017; FC-016 | fails: output differs from the formatter's |

### RH5301 auditor member: `RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzerTests`

| Test | Requirement / failure class | Expected now |
|---|---|---|
| `VerifyCreationWithComplexElementBehindTheLastLineOfAMultiLineCommentIsFixedAsTheFormatterLaysItOut` (outer `{` misplaced; the member `D = new Dictionary<int, int>` holds a complex element behind `b */ {`) | RH5301-004, FMT-PIPE-017; FC-016 | fails: output differs from the formatter's |

### Clarified examples and audit findings (formatter project)

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Regression.Cleanup.ByteOrderMarkTests.RawStringOnTheLineOfTheByteOrderMarkIsAlignedToTheQuotesCountedWithTheByteOrderMark` (FMT-RAW-005 example: content and closing at column 9) | FMT-RAW-005; audit finding 4 | `AssertRuleResult` | fails: column 8 |
| `Unit.RawStringAlignment.RawStringInterpolationHoleTests.CommentAboveCommentLedHoleLineTakesTheColumnOfTheLeadingComment` (FMT-RAW-007 example: `// above` at 17) | FMT-RAW-007, FMT-INDENT-006; audit finding 3 | `AssertRuleResult` | passes |
| `Regression.Indentation.CommentLedLineAlignmentTests.CommentAboveCommentLedLineTakesTheColumnOfTheLeadingComment` (FMT-INDENT-006 inline example) | FMT-INDENT-006 | `AssertRuleResult` | passes |
| `Unit.ReihitsuFormatterNodeBlankLineTests.MovesSingleLineDocumentationBehindOpeningBraceOfMethodBodyWhenTheTargetHoldsATab` (audit finding 2, member a) | — (FC-028; statement documentation is not decided by FMT-DOC-001) | `AssertFormatsTarget`, LF and CRLF, two passes | fails: `{ /// Doc` kept |
| `…MovesSingleLineDocumentationBehindOpeningBraceOfTypeWhenTheTargetHoldsATab` (member b) | FMT-DOC-001, FMT-PIPE-010; FC-028 | same | fails: same |
| `…MovesSingleLineDocumentationBehindCaseLabelWhenTheTargetHoldsATab` (member c) | — (FC-028) | same | fails: `case 1: /// Doc` kept |
| `…MovesSingleLineDocumentationBehindOpeningBraceOfMethodBodyWhenTheTargetIsIndentedWithTabs` (member d; the comment and `M();` at 8, from the target's two tabs) | FMT-PIPE-017; FC-028 | same | fails: comment kept, `M();` at 2 |
| `Unit.RawStringAlignment.IndentationPhaseRawStringMoveTests.MisalignedRawStringInAttachedMethodBelowTheFirstLineIsCorrected` (audit finding 7: `IndentationPhase.Execute` on the attached method of a document, starting on line 2) | FMT-RAW-001; FC-017 | phase-level, exact output | passes. This is the test the auditor found missing for the `lineOffset` branch. |

Audit finding 5: I reworded the class summary of `Regression/Spacing/TabGapSpacingTests.cs` and the summary of `TabIndentationIsStillExpandedToSpaces`. Both now name the tab replacement phase instead of the cleanup phase. Comments only.

## Not covered, and why

- **The h1 shape under document-level formatting of a two-space file**: the indentation phase reindents a two-space file to four spaces, so h1's two-space columns exist only at node level. The RH3202 test covers that.
- **The C5 restore in `FormatNodeInDocumentWithContextAsync` for a target that starts a later line of the context**: `20-skeptic.md` found no caller that reaches a failing member. FMT-PIPE-017 puts the context node's lines that are not written back outside the requirement. The one context test above covers a mid-line target.
- **C6, a byte order mark in front of a node-level target's first token**: no test. Only string input reaches it, and no code fix formats a direct child of the root on the byte-order-mark line in the existing tests. The phase-level byte-order-mark case is covered by the FMT-RAW-005 test.
- **Each auditor member under CRLF**: the analyzer tests run one line ending, as all analyzer tests do. The entry-point tests above run LF and CRLF on the same mechanism.
- **Failure classes**:
  - FC-004, FC-005, FC-019–FC-021: no join, and no gap the fixes edit; comments in front of anchors are part of every member above.
  - FC-006, FC-018, FC-023–FC-025, FC-029, FC-036: no rewrite, rename, or member-kind logic.
  - FC-009: no fix is withheld any more.
  - FC-014: no new rule.
  - FC-031, FC-032, FC-037: for the auditor.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~ReihitsuFormatterNodeColumnTests|FullyQualifiedName~ReihitsuFormatterNodeBlankLineTests|FullyQualifiedName~IndentationPhaseRawStringMoveTests|FullyQualifiedName~RawStringInterpolationHoleTests|FullyQualifiedName~CommentLedLineAlignmentTests|FullyQualifiedName~ByteOrderMarkTests|FullyQualifiedName~TabGapSpacingTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3202|FullyQualifiedName~RH5301|FullyQualifiedName~RH5303|FullyQualifiedName~RH5304"
scripts/test.sh --project architecture
```

Observed on 9d9e7c1 plus these files:
- formatter filter: `Failed: 9, Passed: 184, Total: 193`.
- analyzer filter: `Failed: 21, Passed: 146, Total: 167`.
- architecture: `Passed: 89`. This includes `EveryRequirementReferenceNamesAnActiveRequirement` (no test tags RH5303-006 or RH5304-005 any more) and `EveryRequirementOfAnEnforcedSpecIsCovered` (FMT-PIPE-017 is now covered).

The failures in both filter runs are exactly the tests marked "fails" above. In the full projects the same tests fail, and no others:
- formatter: `Failed: 9, Passed: 2978, Total: 2987`;
- analyzer: `Failed: 21, Passed: 6205, Total: 6226`.
