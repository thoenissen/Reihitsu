Role: test implementer
Commit: 10cbeb1 (tests written on this head; production code = `main`)
Verdict: tests written

My prompt named the issue, the slug, the lane, and the handovers. It carried no intended fix or suspected cause. Expected values come from FMT-UNSUP-011, the amended RH2001-007, RH5302-005, RH7101-007, and FMT-UNSUP-005 examples, and the change doc's visible output changes. No value comes from a patched copy of the code.

To find every existing test whose input holds the shape, I did not rely on `rg`. I wrote a throwaway Roslyn scanner, kept in the scratchpad and not committed. It parses every string literal in `src/*Test*` (markup stripped), with and without `X`/`DEBUG`/`A`/`B` defined. It reports each `BinaryExpressionSyntax` whose operator trailing trivia or right-operand first-token leading trivia holds a comment, a directive, or disabled text. I also ran it over all repository sources: none holds the shape, so `FormatterSelfHostingTests` is unaffected.

## Tests

New file: `src/Reihitsu.Formatter.Test/Unit/Precheck/UnsupportedBinaryOperatorCommentTests.cs`. New helper constant: `FormatterTestsBase.BinaryOperatorCommentReason`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `UnsupportedBinaryOperatorCommentTests.SpecExampleCommentsBehindBinaryOperatorsAreUnsupported` (location 5:22) | FMT-UNSUP-011 | `AssertUnsupportedFor` | red |
| `…EachBinaryExpressionOfTheSpecExampleIsUnsupported` (5 rows) | FMT-UNSUP-011 | `AssertUnsupportedFor` | red |
| `…CommentBehindEveryBinaryOperatorIsUnsupported` (22 operators: arithmetic, shift incl. `>>>`, relational, equality, bitwise, logical, `??`, `is` type, `as`) | FMT-UNSUP-011 | `AssertUnsupportedFor` | red |
| `…SpecExampleIsFollowedByAnEnumMemberIsUnsupported` (location 10:21) | FMT-UNSUP-011 | `AssertUnsupportedFor` | red |
| `…CommentBehindAPatternOperatorIsSupported` (constant, null, `not`, declaration, property, relational/`or`, `and` patterns) | FMT-UNSUP-011 (other side, FC-003) | `AssertSupported` | green |
| `…EveryCommentPositionInTheGapIsUnsupported` (line comment, own line, behind blank line, multi-line `/* */`, no whitespace, behind a tab, two comments) | FMT-UNSUP-011 | `AssertUnsupportedFor` | red |
| `…DocumentationCommentBehindAnOperatorIsUnsupportedInEveryDocumentationMode` (`/** */` same line and own line, `///` own line and trailing; Parse/None/Diagnose) | FMT-UNSUP-011 (FC-040) | `AssertUnsupportedFor` | red |
| `…SpecExampleConditionalOperandBehindATrailingOperatorIsUnsupported` (DEBUG defined / not) | FMT-UNSUP-011 (FC-020) | `AssertUnsupportedFor` | red |
| `…EveryDirectiveBehindAnOperatorIsUnsupported` (`#pragma`, `#region`, `#nullable`, `#if false`, `#if/#elif/#else`, comment in front of a directive) | FMT-UNSUP-011 (FC-005, FC-020) | `AssertUnsupportedFor` | red |
| `…DisabledTextBehindAnOperatorIsUnsupportedInEveryBuild` (X defined / not) | FMT-UNSUP-011 | `AssertUnsupportedFor` | red |
| `…DirectivesInFrontOfLeadingOperatorsAreSupportedInEveryBuild` (the `A⏎#if X⏎\| B⏎#endif⏎\| C` boundary) | FMT-UNSUP-011 (FC-003) | `AssertSupported` | green |
| `…EveryBinaryExpressionIsCheckedWithItsOwnOperator` (chain 1st/2nd, nested left/right, `&& o is`, lambda, argument, interpolation hole, query, collection, switch arm, conditional branch, expression statement, assigned value) | FMT-UNSUP-011 (FC-027, FC-039) | `AssertUnsupportedFor` | red |
| `…SpecExampleCommentsInFrontOfTheOperatorsAreSupported` (DEBUG defined and not) | FMT-UNSUP-011 | `AssertSupported` | green |
| `…CommentOutsideTheShapeIsSupported` (behind left operand, line comment behind it, comment/directive line above the operator, inside right operand, behind a unary that starts it, behind the right operand, `=`, `+=`, `??=`, `..`, unary) | FMT-UNSUP-011 (FC-003) | `AssertSupported` | green |
| `…CommentBehindAConditionalOperatorInFrontOfABinaryExpressionKeepsTheReasonOfAConditional` | FMT-UNSUP-011 | `AssertUnsupportedFor` | green (FMT-UNSUP-008) |
| `…CommentBehindAnOperatorOfADeclarationKeepsTheReasonOfItsDeclaration` (local value, field 1 / several declarators → 011; attribute argument of a field → 004; of a method → 011) | FMT-UNSUP-011 | `AssertUnsupportedFor` | red, except the field-attribute row, which is green |
| `…DirectiveBehindAnOperatorOfAFieldValueKeepsTheReasonOfItsDeclaration` (several declarators → 010, one declarator → 011; X defined / not) | FMT-UNSUP-011 | `AssertUnsupportedFor` | red (its 010 half already holds) |
| `…FirstShapeInTheTextIsReported` (011 vs 004, 008, 011) | FMT-UNSUP-001, FMT-UNSUP-011 | `AssertUnsupportedFor` | red |
| `…FormatDocumentAsyncRefusesTheShape` | FMT-UNSUP-011 | direct, LF+CRLF | red |
| `…NodeLevelEntryPointsRefuseATargetThatHoldsTheShape` (`FormatNode`, `FormatNodeInDocumentAsync` on the method and on the binary expression, context entry point; the method beside it formatted) | FMT-UNSUP-011 | direct, LF+CRLF | red |
| `…FindUnsupportedSyntaxAnswersForTheBinaryExpressionAndNotForPartOfIt` (binary, root reported; right operand, left operand null; `FormatNode(right operand)` not refused) | FMT-UNSUP-011, FMT-UNSUP-003 (FC-041) | direct | red |
| `…FindUnsupportedSyntaxGivesTheAnswerOfThePrecheck` (4 rows) | FMT-UNSUP-011 | direct | green (both null today; it holds the predicate identity once implemented) |
| `…OutputOfAFormattedBinaryExpressionIsNotUnsupported` (13 rows: comments in front of operators the line-break phase moves, lone operator, blank line, `///` and `/** */` in Parse/None/Diagnose, the skeptic's G.cs fixtures d1–d6) | FMT-UNSUP-005, FMT-UNSUP-011 (FC-038, FC-010) | two passes, LF+CRLF | green |
| `…RightOperandIsJoinedBehindTheMovedOperator` | FMT-BREAK-017 | `AssertRuleResult` | green |
| `UnsupportedSyntaxClosureTests.SpecExampleFieldSplitKeepsACommentInsideAValueInsideThatValue`: changed to the amended example `1 /* c */ + 2` | FMT-UNSUP-005 | `AssertRuleResult` | green |
| `UnsupportedVariableDeclarationTests.CommentInsideAValueInFrontOfItsLastTokenIsSupported`, `…CommentInsideALaterValueInFrontOfItsLastTokenIsSupported`, `…CommentInFrontOfTheLastTokenOfARightNestedValueIsSupported`: the token in front of the comment is now a unary `-` instead of a binary `+` (old inputs now covered by the 011 rows above) | FMT-UNSUP-004 | `AssertSupported` | green |
| `UnsupportedConditionalOperatorCommentTests.CommentOutsideTheShapeIsSupported`, row "Null-coalescing operator": `s ?? /* c */ ""` → `s /* c */ ?? ""` | FMT-UNSUP-008 | `AssertSupported` | green |
| `FieldDeclarationSplitTests.CommentInsideTheValueOfALaterDeclaratorStaysInThatValue`: `2 + /* two */ 3` → `2 /* two */ + 3` | FMT-STRUCT-022 | `AssertRuleResult` | green |
| `MultiLineCommentGapSpacingTests.SpaceBehindMultiLineCommentInFrontOfBinaryOperatorBecomesOneSpaceAsBehindSingleLineComment` (renamed), `…SpaceDemandedBehindMultiLineCommentIsInsertedAsBehindSingleLineComment`: comment moved in front of the operator, as in the FMT-SPACE-013 example | FMT-SPACE-001, FMT-SPACE-013 | `AssertRuleResult` | green |
| `IdempotencyTests.CommentJoinIsIdempotent`: `First() + // binary comment⏎Second()` → `First() // binary comment⏎+ Second()`, so the case is formatted, not refused | FMT-PIPE-004 | direct | green |
| `FirstCommentBehindOpeningTokenTests.CommentBehindLessThanOperatorGetsBlankLine`: binary `<` method `N` removed; relational pattern kept | FMT-BLANK-011 | `AssertBlankLinesAbove` | green |
| `JoinSeparatorPhaseTests.CommentInTheGapStillRefusesEveryJoin` / `DirectiveInTheGapStillRefusesEveryJoin`: binary-operand members (`// y`, `CS0004`) removed, because once the precheck refuses them they are unreachable phase input | FMT-BREAK-031 | phase alone | green |
| `LoneOperatorJoinBlankLineTests.LoneOperatorFollowedByCommentIsUnsupported` (was `…GainsNoBlankLine`, FMT-BREAK-030) | FMT-UNSUP-011 | `AssertUnsupportedFor` | red |
| `DelimitedDocumentationInExpressionTests.DelimitedDocumentationBehind{Addition,Coalesce,ConditionalAnd}OperatorIsUnsupported` (were `Moves…`, untagged) | FMT-UNSUP-011 | `AssertUnsupportedFor` | red |
| `CliHiddenCodeEndToEndTests.CommentOrDirectiveBehindABinaryOperatorIsReportedAsUnsupported` (change-doc examples 1 `(5,22)`, 3 `(4,21)` incl. `class C{` left unchanged, 4 `(8,1)`; in-place run, exit 2, file unchanged) | FMT-UNSUP-011 | `Program.Main` | red (exit 0) |
| `RH5302…VerifySpecExampleCommentBehindAnOperatorOfTheChainWithholdsTheFix` (LF/CRLF) | RH5302-005 | `VerifyNoCodeFixIsOffered` | red |
| `RH5302…VerifyCommentBehindMisalignedLeadingOperatorWithholdsFix` (LF/CRLF) | RH5302-005 | `VerifyNoCodeFixIsOffered` | red |
| `RH5302…VerifyCommentBehindAnotherOperatorOfTheChainWithholdsFix` (LF/CRLF) | RH5302-005 | `VerifyNoCodeFixIsOffered` | red |
| `RH5302…VerifyCommentBehindAnAlignedLeadingOperatorOfTheChainWithholdsFix` | RH5302-005 | `VerifyNoCodeFixIsOffered` | red |
| `RH5302…VerifyCommentBehindAnArithmeticOperatorInsideAnOperandWithholdsFix` (LF/CRLF) | RH5302-005 | `VerifyNoCodeFixIsOffered` | red |
| `RH5302…VerifyDisabledTextBehindAnOperatorWithholdsFixInEveryBuild` (DEBUG defined / not) | RH5302-005 | `VerifyNoCodeFixIsOffered` | green (the guard to be deleted withholds it today) |
| `RH5302…VerifySpecExampleCommentBehindAnOperatorOutsideTheChainDoesNotWithholdFix` (LF/CRLF) | RH5302-005 | `VerifyCodeFixWithEndOfLine` | green |
| `RH5302…VerifyOwnLineCommentAboveOperandWithholdsTheFixForTheWholeChain` (LF/CRLF; was `…LeavesOnlyItsOwnGapUnfixed`, which asserted the `\|\|` fix) | RH5302-005 | `VerifyNoCodeFixIsOffered` | red |
| `RH2001…VerifyFixIsOfferedForACommentInsideTheInitializerValue`: now the spec example `2 /* two */ + Offset(/* three */ 3)` → `… Offset(/* three */3)`, LF/CRLF | RH2001-007 | `VerifyCodeFixWithEndOfLine` | green |
| `RH2001…VerifyNoFixIsOfferedForACommentBehindAnOperatorOfTheInitializerValue` (spec example, LF/CRLF) | RH2001-007 | `VerifyNoCodeFixIsOffered` | red |
| `RH2001…VerifyNoFixIsOfferedForALineCommentBehindAnOperatorOfTheInitializerValue` | RH2001-007 | `VerifyNoCodeFixIsOffered` | red |
| `RH7101…SpecExampleCommentBehindABinaryOperatorWithholdsTheFix` (LF/CRLF) | RH7101-007 | `VerifyNoCodeFixIsOffered` | red |
| `RH7101…CommentBehindABinaryOperatorInAnInitializerValueWithholdsTheFix` | RH7101-007 | `VerifyNoCodeFixIsOffered` | red |
| `RH7101…CommentBehindABinaryOperatorElsewhereInTheTypeWithholdsTheFix` (method, nested type, directive) | RH7101-007 | `VerifyNoCodeFixIsOffered` | red |
| `RH7101…CommentOfAnotherRefusedShapeElsewhereInTheTypeWithholdsTheFix` (FMT-UNSUP-008, FMT-UNSUP-009) | RH7101-007 | `VerifyNoCodeFixIsOffered` | green (clarified existing behavior) |
| `RH7101…CommentInFrontOfABinaryOperatorElsewhereInTheTypeDoesNotWithholdTheFix` (LF/CRLF) | RH7101-007 | `VerifyCodeFixWithEndOfLine` | green |
| `RH3202…VerifyNoFixIsOfferedForACommentBehindABinaryOperator` (change-doc example 6, LF/CRLF) | SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | red |

Removed (the input is now refused before the phase; refusal covered above):
- `CommentJoinTests.BinaryOperatorIsNotCollapsedIntoTrailingComment` and `DirectiveJoinTests.BinaryOperatorIsNotCollapsedAcrossDirective` (both FMT-BREAK-001, phase alone).
- `TabReplacementTests.BinaryOperandBehindTabAndDelimitedDocumentationCommentFormatsLikeFourSpaces` (FMT-TAB-001). Its expected output held the shape; the tab case is the "Block comment behind a tab" row.

Every red test fails for the specified reason, not for a compile or layout error:
- Formatter: "must report the input as unsupported" or "must return a refused input unchanged". The formatter currently formats.
- CLI: exit code 0 instead of 2.
- Analyzer: diagnostics match the markup, and "No code fix may be offered for the diagnostic at …".

Runs on this head:
- formatter: 86 failed / 5517 passed. All 86 are the red rows above.
- analyzer, filtered to RH5302|RH2001|RH7101|RH3202: 22 failed / 528 passed.
- CLI class: 3 failed / 15 passed.
- architecture: 185/185 passed. `EveryRequirementOfAnEnforcedSpecIsCovered` is now satisfied for FMT-UNSUP-011.

Existing tests whose input holds the shape and that I deliberately left as they are:
- `RH5302…VerifyOperatorFollowedByLineCommentIsNotFixed`, `VerifyOperatorFollowedByDirectiveIsNotFixed`, `VerifyCommentBehindOperatorAtEndOfLineWithholdsFix`: still no fix, tagged RH5302-005.
- `UnsupportedFieldDeclarationDirectiveTests` row "Conditional group inside the value of the last declarator": FMT-UNSUP-010's reason, which is exactly the tie FMT-UNSUP-011 states.
- Analyzer-only inputs in RH3003, RH3005, RH3006, RH3201, RH5020, RH5035, RH5038, RH5039, RH5049, RH5104, RH6024 tests, plus `CommentPositionUtilitiesTests`: none of these fixes formats through the formatter (checked against the providers that call `ReihitsuFormatter`), and analyzers do not consult the precheck.

## Scenarios (bug fix)

_N/A: behavior change._

## Not covered, and why

- **FMT-UNSUP-006 throw sites.** None. The change adds no phase refusal, and FMT-UNSUP-006 says no phase refuses yet.
- **The deleted line-break guard, tested directly.** Once the precheck refuses its only input, it is unreachable. Two things cover its deletion: the closure rows (the join still runs, and its output is not refused) and `RightOperandIsJoinedBehindTheMovedOperator`.
- **Other formatter-delegating fixes named in change 6** (RH5111, RH5101, RH5102, RH5308, RH5403): one representative (RH3202) plus RH5302, RH2001, and RH7101. Each fix asks `FindUnsupportedSyntax` about its own node. The predicate is one call site (`ReihitsuFormatter.FindUnsupportedSyntax` → the precheck predicate), but the node passed differs per fix, so this is no proof for the others (FC-043). Those fixes already have the FC-043 tests from the earlier shapes, and the node they pass does not change in this run.
- **FC-006, FC-018, FC-022–FC-026, FC-029, FC-036:** the change rewrites nothing, renames nothing, and adds no literal handling.
- **FC-007/FC-008:** no new fix output. The only offered fixes in new tests use `Verify…`, which checks re-analysis.
- **FC-011/FC-012:** no phase logic is added.
- **FC-013/FC-014:** analyzers are unchanged, and SHARED-DIAG-010 allows reporting a refused shape.
- **FC-044:** the `is` decision is syntactic by spec; the enum-member example pins it.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~UnsupportedBinaryOperatorCommentTests|FullyQualifiedName~DelimitedDocumentationInExpressionTests|FullyQualifiedName~LoneOperatorJoinBlankLineTests|FullyQualifiedName~UnsupportedSyntaxClosureTests|FullyQualifiedName~UnsupportedVariableDeclarationTests|FullyQualifiedName~UnsupportedConditionalOperatorCommentTests|FullyQualifiedName~MultiLineCommentGapSpacingTests|FullyQualifiedName~FieldDeclarationSplitTests|FullyQualifiedName~IdempotencyTests|FullyQualifiedName~FirstCommentBehindOpeningTokenTests|FullyQualifiedName~JoinSeparatorPhaseTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5302|FullyQualifiedName~RH2001|FullyQualifiedName~RH7101|FullyQualifiedName~RH3202"
scripts/test.sh --project cli --filter "FullyQualifiedName~CliHiddenCodeEndToEndTests"
scripts/test.sh --project architecture
```

## Follow-up candidates

_None._ The open candidates in `00-lead.md` (update thoenissen/Reihitsu#953; RH7101 should stop formatting the whole type) stand. This stage adds nothing.
