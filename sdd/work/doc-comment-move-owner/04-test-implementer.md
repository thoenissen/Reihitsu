Role: test implementer
Commit: cea4238
Verdict: tests written

My prompt named the handovers and decisions to read; it carried no intended fix or suspected cause. Expected values come from the spec examples (FMT-DOC-001, FMT-DOC-003, FMT-BLANK-012) and, for decision #9's unpinned shapes, from today's output observed with `scripts/format.sh` and `scripts/apply-fix.sh RH3202` on cea4238. No change doc lists a visible output change, so every full-pipeline, node-level, and code-fix test passes today and must keep passing. The tests that fail today are the phase-level ones that state the new ownership (decisions #6, #8).

## Tests

New files:
- `src/Reihitsu.Formatter.Test/Unit/DocumentationComments/DelimitedDocumentationRelocationPhaseTests.cs`: `DocumentationCommentFormattingPhase` run alone, LF and CRLF. Lines are compared without the whitespace at their start, because the column belongs to FMT-INDENT. Trailing whitespace and the number of line breaks are compared, so a blank line inserted by this phase fails the test.
- `src/Reihitsu.Formatter.Test/Unit/BlankLines/BlankLinePhaseDocumentationCommentTests.cs`: `BlankLinePhase` run alone, LF and CRLF, exact output.
- `src/Reihitsu.Formatter.Test/Regression/FullPipeline/DelimitedDocumentationRelocationTests.cs`: `AssertRuleResult`, which runs LF and CRLF, two passes, the per-phase round trip, and the phase-stability check.

Added to existing files:
- `ReihitsuFormatterNodeBlankLineTests`: 4 node-level tests through `FormatNodeInDocumentAsync`, LF and CRLF, second pass.
- `RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests`: 3 code-fix tests (`Verify`, LF and CRLF).
- `DocumentationCommentRequirementTests.DocumentationBehindOpeningBraceMovesOntoItsOwnLine`: gained `[Requirement("FMT-BLANK-012")]`. Its input and output are FMT-BLANK-012's new opening-brace example. The assertion is unchanged.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| **DelimitedDocumentationRelocationPhaseTests** (documentation phase alone) | | | |
| StartsNewLineInFrontOfDelimitedDocumentationBehindFieldWithCodeBehindIt | FMT-DOC-001 | phase alone | fails |
| StartsNewLineInFrontOfDelimitedDocumentationEndingFieldLine | FMT-DOC-001 | phase alone | fails |
| StartsNewLineInFrontOfDelimitedDocumentationBehindEnumMember | FMT-DOC-001 | phase alone | fails |
| StartsNewLineInFrontOfDelimitedDocumentationBehindTrailingBlockComment | FMT-DOC-001 | phase alone | fails |
| StartsNewLineInFrontOfDelimitedDocumentationBehindMultiLineTrailingBlockComment | FMT-DOC-001; FC-021 | phase alone | fails |
| StartsNewLineInFrontOfDelimitedDocumentationWrittenDirectlyBehindField | FMT-DOC-001; gap "none" | phase alone | fails |
| StartsNewLineOnlyInFrontOfFirstOfTwoDelimitedDocumentationComments | FMT-DOC-001; FC-002 (first comment) | phase alone | fails |
| KeepsBlockCommentBehindMovedDelimitedDocumentation | FMT-DOC-001; FC-021 | phase alone | fails |
| StartsNewLineInFrontOfDelimitedDocumentationFollowedByRegionDirective | FMT-DOC-001; FC-020 | phase alone | fails |
| StartsNewLineInFrontOfDelimitedDocumentationFollowedByActiveConditionalSection | FMT-DOC-001; FC-020 | phase alone | fails |
| KeepsTextOfMovedMultiLineDelimitedDocumentation | FMT-DOC-001, FMT-DOC-012; FC-027 | phase alone | fails |
| StartsNewLinesInFrontOfDelimitedAndFollowingSingleLineDocumentation | FMT-DOC-001, FMT-DOC-002 | phase alone | fails |
| StartsNewLineInFrontOfDelimitedDocumentationBehindStatement | — (undecided position; today's set) | phase alone | fails |
| StartsNewLineInFrontOfDelimitedDocumentationBehindUsingDirective | — (undecided position; change doc decision) | phase alone | fails |
| KeepsStrandedDelimitedDocumentationBeforeClosingBrace | FMT-DOC-003 | phase alone | passes |
| KeepsStrandedDelimitedDocumentationBeforeMemberAccessDot | FMT-DOC-003 | phase alone | passes |
| KeepsStrandedDelimitedDocumentationFollowedByInactiveConditionalSection | FMT-DOC-003; FC-020 | phase alone | passes |
| KeepsDelimitedDocumentationEndingOpeningBraceLine | FMT-INDENT-006; FC-003 (`{`) | phase alone | passes |
| KeepsDelimitedDocumentationBehindOpeningBraceWithCodeBehindIt | — (decision #5) | phase alone | passes |
| KeepsDelimitedDocumentationBehindCaseLabel | — (switch-label colon); FC-003 | phase alone | passes |
| KeepsDelimitedDocumentationBehindBlockCommentOnItsOwnLine | — (earlier comment of the same trivia); FC-003 | phase alone | passes |
| KeepsDelimitedDocumentationBehindBannerCommentAtFileStart | — (earlier comment, no preceding token); FC-003 | phase alone | passes |
| KeepsDelimitedDocumentationThatAlreadyStartsItsLine | FMT-DOC-001; FC-003 | phase alone | passes |
| **BlankLinePhaseDocumentationCommentTests** (blank-line phase alone; decision #6) | | | |
| InsertsNoLineBreakInFrontOfDelimitedDocumentationBehindFieldWithCodeBehindIt | FMT-BLANK-012 | phase alone | fails |
| InsertsNoLineBreakInFrontOfDelimitedDocumentationEndingFieldLine | FMT-BLANK-012 | phase alone | fails |
| InsertsNoLineBreakInFrontOfDelimitedDocumentationBehindEnumMember | FMT-BLANK-012 | phase alone | fails |
| InsertsNoLineBreakInFrontOfDelimitedDocumentationBehindTrailingBlockComment | FMT-BLANK-012 | phase alone | fails |
| InsertsNoLineBreakInFrontOfDelimitedDocumentationBehindStatement | FMT-BLANK-012 | phase alone | fails |
| InsertsNoLineBreakInFrontOfDelimitedDocumentationBehindUsingDirective | FMT-BLANK-012 | phase alone | fails |
| InsertsNoLineBreakInFrontOfSingleLineDocumentationBehindField | FMT-BLANK-012 | phase alone | fails |
| KeepsStrandedDelimitedDocumentationWithoutBlankLine | FMT-BLANK-012 | phase alone | passes |
| SeparatesDelimitedDocumentationStartingItsLineFromField | FMT-BLANK-012 | phase alone | passes |
| SeparatesDelimitedDocumentationStartingItsLineFromEnumMember | FMT-BLANK-012 | phase alone | passes |
| SeparatesSingleLineDocumentationStartingItsLineFromField | FMT-BLANK-012 | phase alone | passes |
| KeepsDocumentationStartingFirstLineOfTypeBodyWithoutBlankLine | FMT-BLANK-012 (first-in-block) | phase alone | passes |
| **DelimitedDocumentationRelocationTests** (full pipeline, today's output) | | | |
| MovesDelimitedDocumentationBehindEnumMemberBelowBlankLine | FMT-DOC-001, FMT-BLANK-012 | AssertRuleResult | passes |
| MovesDelimitedDocumentationBehindTrailingBlockCommentAndKeepsCodeBehindIt | FMT-DOC-001, FMT-BLANK-012 | AssertRuleResult | passes |
| MovesDelimitedDocumentationBehindFieldAndKeepsCodeBehindIt | FMT-DOC-001, FMT-BLANK-012 | AssertRuleResult | passes |
| MovesDelimitedDocumentationWrittenDirectlyBehindField | FMT-DOC-001 | AssertRuleResult | passes |
| MovesDelimitedDocumentationSeparatedFromFieldByTab | FMT-DOC-001; gap "tab" | AssertRuleResult | passes |
| MovesDelimitedDocumentationBehindMultiLineTrailingBlockComment | FMT-DOC-001; FC-021 | AssertRuleResult | passes |
| MovesMultiLineDelimitedDocumentationWithoutRewritingItsText | FMT-DOC-001, FMT-DOC-012; FC-027 | AssertRuleResult | passes |
| MovesFirstOfTwoDelimitedDocumentationCommentsAndKeepsSecondBehindIt | — (decision #9) | AssertRuleResult | passes |
| MovesFirstOfTwoDelimitedDocumentationCommentsEndingFieldLine | — (decision #9) | AssertRuleResult | passes |
| MovesDelimitedDocumentationAndKeepsBlockCommentBehindIt | FMT-DOC-001; FC-021 | AssertRuleResult | passes |
| MovesDelimitedDocumentationAboveLineComment | FMT-DOC-001; FC-019 | AssertRuleResult | passes |
| MovesDelimitedDocumentationAndSplitsFollowingSingleLineDocumentation | FMT-DOC-001, FMT-DOC-002 (decision #9) | AssertRuleResult | passes |
| MovesDelimitedDocumentationFollowedByRegionDirective | FMT-DOC-001; FC-019, FC-020 | AssertRuleResult | passes |
| MovesDelimitedDocumentationFollowedByActiveConditionalSection | FMT-DOC-001; FC-020 | AssertRuleResult | passes |
| KeepsDelimitedDocumentationFollowedByInactiveConditionalSection | FMT-DOC-003; FC-020 | AssertRuleResult | passes |
| KeepsStrandedDelimitedDocumentationBeforeClosingBrace | FMT-DOC-003, FMT-BLANK-012 | AssertRuleResult | passes |
| SeparatesStrandedDocumentationOnItsOwnLine | FMT-BLANK-012 | AssertRuleResult | passes |
| KeepsDelimitedDocumentationEndingOpeningBraceLine | FMT-INDENT-006 | AssertRuleResult | passes |
| CarriesDelimitedDocumentationWithCodeBehindOpeningBraceOfTypeOnSameLine | — (decision #5) | AssertRuleResult | passes |
| CarriesDelimitedDocumentationWithCodeBehindOpeningBraceOfType | — (decision #5) | AssertRuleResult | passes |
| CarriesDelimitedDocumentationWithCodeBehindOpeningBraceOfBlock | — (decision #5) | AssertRuleResult | passes |
| KeepsDelimitedDocumentationBehindBlockCommentOnItsOwnLine | — (decision #9) | AssertRuleResult | passes |
| KeepsDelimitedDocumentationBehindBannerCommentAtFileStart | — | AssertRuleResult | passes |
| KeepsDelimitedDocumentationWithStatementBehindCaseLabel | — | AssertRuleResult | passes |
| MovesDelimitedDocumentationBehindStatementBeforeNextStatement | — (characterization) | AssertRuleResult | passes |
| MovesDelimitedDocumentationInsideArgumentList | — (characterization, decision #9) | AssertRuleResult | passes |
| MovesDelimitedDocumentationBeforeElseClause | — (characterization, decision #9) | AssertRuleResult | passes |
| KeepsStrandedDelimitedDocumentationBeforeMemberAccessDot | FMT-DOC-003 | AssertRuleResult | passes |
| **ReihitsuFormatterNodeBlankLineTests** (node level) | | | |
| MovesFirstOfTwoDelimitedDocumentationCommentsBehindPreviousMember | — (N1); FC-028 | AssertFormatsTarget | passes |
| KeepsTwoDelimitedDocumentationCommentsBehindPreviousMemberWhenTargetSharesTheirLine | — (N1 with N4); FC-028 | AssertFormatsTarget | passes |
| KeepsDelimitedDocumentationBehindPreviousMemberWhenTargetSharesItsLine | FMT-PIPE-010 (N4); FC-028 | AssertFormatsTarget | passes |
| KeepsDelimitedDocumentationBehindOpeningBraceWhenTargetSharesItsLine | — (N2); FC-028 | AssertFormatsTarget | passes |
| **RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests** (code fix) | | | |
| VerifyFixMovesFirstOfTwoDelimitedDocumentationCommentsBehindPreviousMember | — (N1 through the fix); FC-007 | Verify (LF, CRLF) | passes |
| VerifyFixKeepsDelimitedDocumentationBehindOpeningBraceWhenMethodSharesItsLine | — (N2 through the fix) | Verify (LF, CRLF) | passes |
| VerifyFixKeepsDelimitedDocumentationBehindPreviousMemberWhenMethodSharesItsLine | — (N4 through the fix) | Verify (LF, CRLF) | passes |
| DocumentationCommentRequirementTests.DocumentationBehindOpeningBraceMovesOntoItsOwnLine (tag added) | FMT-DOC-001, FMT-BLANK-012 | AssertRuleResult | passes |

Failing on cea4238: 21 tests (14 documentation-phase, 7 blank-line-phase). Each fails on its own expected-versus-actual difference:
- The documentation phase leaves ` /** doc */` behind the code (for example `int A; /** doc */ int B;` stays on one line).
- The blank-line phase alone rewrites `int A; /** doc */` to `int A; ` / blank / `/** doc */`.
- For `///` behind a field, the blank-line phase alone does the same: `int A; /// <inheritdoc/>` becomes `int A; ` / blank / `/// <inheritdoc/>`. So today it also inserts a line break in front of a single-line comment when it runs without the documentation phase. Decision #6 and the narrowed FMT-BLANK-012 forbid that for both forms.

Every other new test passes. Run on cea4238 with these files: formatter project 3473 passed and 21 failed, those 21 exactly; RH3202 class 26/26; architecture 92/92.

Observed outputs that differ from a handover's description (today's output, as pinned):
- `} /** d */ else` gives `}` / blank / `/** d */ else`. The comment and `else` stay on one line. 02-skeptic.md describes `/** d */` / `else`.
- `case 1: /** d */ M(2);` is unchanged.
- `s.Trim() /** d */ .ToString()` gives `s.Trim() /** d */.ToString()`: only spacing changes, and the comment stays.
- `int A; /** d */` followed by `#if DEBUG` / `int B;` / `#endif`, with DEBUG undefined, is not moved: the following token is `}`, so the comment is stranded. With `#if !DEBUG` the comment is moved.
- RH3202 on `private int _a; /** d */ public int M() => 1;` (N4) gives `private int _a; /** d */ public int M()`, with the method's braces at column 20 below `public`.

## Not covered, and why

- **The using-directive shape at full pipeline.** `DelimitedDocumentationBlankLineConvergenceTests.SeparatesDelimitedDocumentationFromPrecedingUsingDirectiveInOnePass` already pins it, including its `PhaseReversalExceptionList` entry (the blank-line flip-flop of thoenissen/Reihitsu#1497). I wrote a duplicate and removed it: under `AssertRuleResult` it fails on that known reversal, which only that entry exempts. The phase-level tests cover the using shape.
- **Other existing pins.** Comments before a parameter, a block statement, and the other undecided positions are already pinned by `DelimitedDocumentationBlankLineConvergenceTests`, `DelimitedDocumentationBeforeBlockStatementTests`, and the node-level `SeparatesDelimitedDocumentation*` / `MovesDelimitedDocumentation*` / `KeepsDelimitedDocumentationBehind*Stable` tests. I did not duplicate them.
- **"No directive precedes the comment in the leading trivia" (the `IsPrecededByDirective` condition).** I test only one side. A directive ends its line, so a documentation comment behind a directive in the same leading trivia always starts its line. The "directive in front, comment not starting its line" member cannot be constructed. The directive-behind side is covered (FC-020 rows).
- **FC-028, `FormatNode` (detached).** It has no preceding token, so neither phase moves anything there today or after the change. 02-skeptic.md says the `BlankLineTokenCleanupRewriter` documentation boundary must stay. Existing tests already pin that path; it gets no new test.
- **FC-007 / FC-008 (code-fix convergence, Fix All).** The three RH3202 tests run the verifier's re-analysis, which checks one-pass convergence. RH3202's existing `GetFixAllScenario` covers Fix All, and these shapes add no co-occurring diagnostics.
- **FC-013 / FC-014 (analyzer–formatter disagreement).** No formatter output changes, so no analyzer result can flip. RH5204, RH8302, RH8303, and RH5114 are not involved (02-skeptic.md, *Surfaces*).
- **FC-015 / FC-016 (single owner, no private copy of the `{` / switch-label / first-comment facts).** This is structural (decision #8) and not observable through output. The phase-level tests pin both sides of each fact; the copy itself is for the auditor to check.
- **FC-004, FC-006, FC-018, FC-022 to FC-026, FC-029, FC-036.** None of them reach this change: no tokens are reordered, no rewrite changes semantics, there are no literals and no renames, and the change only moves a line start.
- **FC-031.** Code-comment drift is checked on the implementation, not in tests.
- **Line-break phase alone.** Decision #6 narrowed Done-when 2 to the blank-line phase, and brace placement keeps the `{ /** d */ code` case (decision #5). The three `Carries…` full-pipeline tests pin that result.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~DelimitedDocumentationRelocationPhaseTests|FullyQualifiedName~BlankLinePhaseDocumentationCommentTests"
scripts/test.sh --project formatter --filter "FullyQualifiedName~DelimitedDocumentationRelocationTests|FullyQualifiedName~ReihitsuFormatterNodeBlankLineTests|FullyQualifiedName~DocumentationCommentRequirementTests|FullyQualifiedName~DelimitedDocumentationBlankLineConvergenceTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests"
```
