Role: test implementer
Commit: 377aa6c
Verdict: tests written

I worked from `00-lead.md` (*Maintainer answers* through batch 3, *Confirmed examples*), `05-spec-author.md`, `04-skeptic.md`, `03-spec-author.md`, the change doc, and FMT-UNSUP-023, FMT-BREAK-046, FMT-INDENT-010/-023, FMT-BLANK-009/-013/-018, and FMT-REGION-005 as of b15784d. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

Every expected value comes from a spec example, a confirmed example, a change-doc entry, or a requirement's text. To find the existing tests the declaration breaks, I used a throwaway copy of the FMT-UNSUP-023 check in a scratch worktree outside the repository. It was never committed, and the worktree is removed. It only located the affected tests and checked the new region tests' locations. No expected value comes from it (FC-035). A static scanner of test literals and spec examples located the array tests and stale spec examples.

On this head (production code = `main`):
- `scripts/test.sh --project formatter`: 103 failures, all in the tests listed below as "fails until implemented".
- Analyzer focused run: 3 failures (`RH5305`/`RH5206` withheld-fix tests and the `RH5410` formatter test).
- CLI `CliHiddenCodeEndToEndTests`: 6 failures (the new DataRows).
- `scripts/test.sh --project architecture`: 221 passed. FMT-BREAK-046 and FMT-UNSUP-023 are now covered.

Each new failure is the specified difference:
- refused inputs: today the formatter formats them;
- array tests: today's output differs only in the brace lines and the lines behind them;
- withheld fixes: today a fix is offered.

## Tests

New files:
- `tests/Reihitsu.Formatter.Test/Unit/Precheck/UnsupportedRegionInsideMemberTests.cs` (U below);
- `tests/Reihitsu.Formatter.Test/Regression/LineBreaks/ArrayInitializerBraceLineTests.cs` (A below).

New helper constant: `RegionInsideMemberReason` in `FormatterTestsBase`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| U `SpecExampleRegionDirectivesInsideMembersAreUnsupported` (12,9) | FMT-UNSUP-023 | `AssertUnsupportedFor`, location | fails until implemented |
| U `SpecExampleRegionDirectivesBetweenMembersAreSupported` | FMT-UNSUP-023 | `AssertSupported` + `AssertRuleResult(input)` | passes |
| U `SpecExampleRegionDirectivesBetweenTheMembersOfAnExtensionBlockAreSupported`, `RegionDirectivesBetweenTheMembersOfAnExtensionBlockAreFormatted` (entry 16) | FMT-UNSUP-023 | `AssertSupported`, `AssertRuleResult` | passes |
| U `SpecExampleEveryRegionDirectiveOfAFileWithTopLevelStatementsIsUnsupported` (3,1), `RegionBetweenTheMembersOfATypeInAFileWithTopLevelStatementsIsUnsupported`, `EveryRegionDirectiveOfAFileWithTopLevelStatementsIsUnsupported` (5 rows: first statement, between statements, inside a statement, around a using, end of file) | FMT-UNSUP-023 | `AssertUnsupportedFor` | fails until implemented |
| U `RegionsOfAFileWithoutATopLevelStatementAreSupported` | FMT-UNSUP-023 | `AssertSupported` + `AssertRuleResult` | passes |
| U `RegionInAMethodBodyIsUnsupported` (5,9), `RegionInAFieldInitializerIsUnsupported` (4,19), `RegionInAParameterListIsUnsupported` (4,12), `RegionInTheAttributeArgumentsOfAMemberIsUnsupported` (11,8), `RegionInTheHeaderOfATypeIsUnsupported` (10,11): change-doc entries 1–4, 14 | FMT-UNSUP-023 | `AssertUnsupportedFor`, location | fails until implemented |
| U `RegionInsideAMemberIsUnsupported` (32 rows, listed under *Rows* below) | FMT-UNSUP-023 | `AssertUnsupportedFor` | fails until implemented |
| U `RegionInsideAnEnumMemberIsUnsupported`, `RegionClosedInsideAMemberIsUnsupportedAtItsEndRegion` | FMT-UNSUP-023 | `AssertUnsupportedFor` | fails until implemented |
| U `RegionThatEndsBehindAMembersLastTokenIsSupported` (type's `}`, next member) | FMT-UNSUP-023 | `AssertSupported` | passes |
| U `RegionInTheHeaderOfATypeANamespaceOrAnExtensionBlockIsUnsupported` (9 rows, listed under *Rows* below) | FMT-UNSUP-023 | `AssertUnsupportedFor` | fails until implemented |
| U `RegionBetweenMembersIsSupported` (14 rows, listed under *Rows* below) | FMT-UNSUP-023 | `AssertSupported` | passes |
| U `RegionInsideAMethodOfANestedTypeIsUnsupported`, `RegionInsideAMemberOfAnExtensionBlockIsUnsupported` (FC-039) | FMT-UNSUP-023 | `AssertUnsupportedFor` | fails until implemented |
| U `RegionInDisabledTextIsNotChecked` (X undefined / defined) | FMT-UNSUP-023 | `AssertSupported` / `AssertUnsupportedFor` | undefined passes, defined fails until implemented |
| U `OtherDirectiveInsideAMemberIsSupported` (`#if`, `#pragma`, `#nullable`) | FMT-UNSUP-023 | `AssertSupported` | passes |
| U `OtherShapesKeepTheirReason` (FMT-UNSUP-010, -009, -012, -013, -017 reasons for a region) | FMT-UNSUP-023 | `AssertUnsupportedFor` | passes (those shapes refuse today) |
| U `NodeLevelInputIsNotRefusedForTheRegionInFrontOfItsFirstToken` (statement and expression excepted; the `if`, the method, and the document refused) | FMT-UNSUP-003, FMT-UNSUP-023 | `FindUnsupportedSyntax`, `FormatNode` | fails until implemented |
| U `MethodBelowARegionBetweenMembersIsNotRefusedByAnyEntryPoint` (document, `FormatNode`, `FormatNodeInDocumentAsync`; FC-028, FC-041) | FMT-UNSUP-002, FMT-UNSUP-023 | entry points | passes |
| U `FormatNodeInDocumentAsyncRefusesAMethodThatHoldsARegionInItsBody` (replaces the deleted `FormatNodeInDocumentAsyncKeepsRegionInsideFormattedMethod`) | FMT-UNSUP-002, FMT-UNSUP-023 | entry point | fails until implemented |
| U `TypeOfAFileWithTopLevelStatementsIsDecidedByItsOwnMembers` | FMT-UNSUP-003, FMT-UNSUP-023 | entry points | fails until implemented (the document assertion) |
| U `ContextEntryPointRefusesAMethodContextForARegionInFrontOfTheTargetParameter` (FC-043) | FMT-UNSUP-002, FMT-UNSUP-023 | `FormatNodeInDocumentWithContextAsync` | fails until implemented |
| U `ContextEntryPointDoesNotRefuseAnEnumContextForARegionInFrontOfTheTargetMember` | FMT-UNSUP-002, FMT-UNSUP-023 | `FormatNodeInDocumentWithContextAsync` | passes |
| U `FindUnsupportedSyntaxAnswersForABuiltNode` | FMT-UNSUP-023 | `FindUnsupportedSyntax` | fails until implemented |
| U `OutputOfTheFormatterIsNotUnsupported` (8 rows: empty type, enum trailing comma, attribute split, field split, using order, expression body to block, get-only to expression body, doc comment; FC-038) | FMT-UNSUP-005, FMT-UNSUP-023 | two passes of `FormatSyntaxTree` | passes |
| `CliHiddenCodeEndToEndTests.RegionDirectiveInsideAMemberIsReportedAsUnsupported` (entries 1–4, 14, 15: CLI line, exit code 2, file unchanged) | FMT-UNSUP-023 | `Program.Main` | fails until implemented |
| `RH5305…AnalyzerTests.VerifyNoFixIsOfferedForACollectionExpressionThatHoldsARegion` (entry 6, LF and CRLF) | SHARED-FIX-010, FMT-UNSUP-023 | `VerifyNoCodeFixIsOffered` | fails until implemented |
| `RH5206…AnalyzerTests.VerifyNoFixIsOfferedForASwitchExpressionThatHoldsARegion` (entry 6, LF and CRLF) | SHARED-FIX-010, FMT-UNSUP-023 | `VerifyNoCodeFixIsOffered` | fails until implemented |
| A `SpecExampleMultiLineArrayInitializersPutTheirBracesOnLinesOfTheirOwn` | FMT-BREAK-046 | `AssertRuleResult` | fails until implemented |
| A `SpecExampleBracesAreDecidedOnTheLinesTheOtherRulesLeave` | FMT-BREAK-046, FMT-INDENT-023 | `AssertRuleResult` | fails until implemented |
| A `DeclarationInitializerWithAMultiLineAndASingleLineRow` (7), `ImplicitArrayCreationSpanningSeveralLines` (8), `ArrayCreationWithASingleLineAndAMultiLineRow` (9), `RowWhoseElementListWrapsKeepsTheElementLineBreak` (10), `FieldInitializerSpanningSeveralLines` (11), `StackallocSpanningSeveralLines` (19), `InitializerIsJudgedOnTheLinesTheOtherRulesLeave` (20), `LineBreakInsideAVerbatimStringMakesTheInitializerMultiLine` (18) | FMT-BREAK-046 | `AssertRuleResult` | fails until implemented |
| A `SingleLineArrayInitializersStayAsWritten` (13, plus `new int[2]`, jagged, `stackalloc`) | FMT-BREAK-046 | `AssertRuleResult(input)` | passes |
| A `CommentInFrontOfAMultiLineRowKeepsItsLineAndTheBraceMovesBelowIt` (17) | FMT-BREAK-046, FMT-INDENT-023 | `AssertRuleResult` | fails until implemented |
| A `CommentsNextToTheBracesKeepTheirLines` (batch 3, answer A) | FMT-BREAK-046 | `AssertRuleResult` | fails until implemented |
| A `PropertyInitializerSpanningSeveralLines`, `ArrayCreationWithASizeSpanningSeveralLines`, `ElementsTheFormatterLaysOutOverSeveralLinesMakeTheInitializerMultiLine` (anonymous object, block lambda, switch expression), `DeclarationBraceWrittenBelowTheEqualsStaysOnTheEqualsLine`, `MultiLineRawStringElementMovesWithItsOpeningQuotes` (FC-022), `LineCommentBehindAnElementMakesTheInitializerMultiLine`, `CommentBehindTheCommaBehindARowKeepsItsLine`, `CommentBetweenARowsClosingBraceAndItsCommaStaysOnTheBracesLine` (FC-019, FC-021), `ConditionalGroupBetweenRowsIsKept` (FC-019, FC-020), `TokensBehindTheClosingBraceStayOnItsLine` (`.Length`, `)`), `InterpolatedStringAsTheFirstElementInAnArgumentStartsTheLineBehindTheBrace`, `ArrayCreationInAnAttributeArgument` | FMT-BREAK-046 | `AssertRuleResult` | fails until implemented |
| A `EmptyInitializerOnTwoLinesKeepsItsBraces` | FMT-BREAK-046 | `AssertRuleResult` | passes (unchanged) |
| A `ThreeDimensionalArrayIsJudgedForEachInitializer` | FMT-BREAK-046, FMT-INDENT-023 | `AssertRuleResult` | fails until implemented |
| A `SpecExampleRowsAreLaidOutLikeComplexElementInitializers` (entry 12) | FMT-INDENT-023 | `AssertRuleResult` | fails until implemented |

Rows of the U DataRow tests:
- `RegionInsideAMemberIsUnsupported` (32 rows):
  - bodies: a method body, an empty block, a local function's body, around a local function, a lambda in a field initializer, a constructor, an operator;
  - switch statements: a switch section, in front of a switch label;
  - statements and expressions: between `if` and `else`, a method chain, an anonymous object, a switch expression, a list pattern, a parenthesized pattern, an argument list;
  - initializers: field and property collection expressions, a field array initializer, a collection initializer;
  - accessors: property, indexer, and event accessor lists, an accessor body;
  - declaration parts: a parameter list, attribute arguments, between two attribute lists, between the parameter list and the body, between an expression body and its `;`, a delegate's parameter list.
- `RegionInTheHeaderOfATypeANamespaceOrAnExtensionBlockIsUnsupported` (9 rows): a base list, a type parameter list, a primary constructor, a constraint clause, between a type's attribute lists, in front of a type's `{`, a namespace's `{`, and an extension block's `{`, and an extension block's parameter list.
- `RegionBetweenMembersIsSupported` (14 rows): `[assembly:]`, `[module:]`, `extern alias`, a using inside a namespace, a file-scoped namespace to end of file, an enum's `}`, a namespace's `}`, a delegate, an event field, an attributed member, interface members, struct members, a nested type at depth 2 and its members, a type to end of file.

Every formatter test runs LF and CRLF, a second pass, and the per-phase stability check through `AssertRuleResult`, `AssertSupported`, or the closure loop.

### Existing tests changed, and why

Regions moved between members, so the requirement's point survives there. All pass on this head.

| Test | Change | Requirement / answer |
|---|---|---|
| `DirectiveBeforeEndRegionBlankLineTests.InsertsNoBlankLineInFrontOfDirectiveWhenRegionDirectiveEndsTheGap` | the new FMT-BLANK-009 example (class `D`, `int _a;`) | FMT-BLANK-009 (clarified), FMT-UNSUP-023 |
| same file: `KeepsDirectiveDirectlyBelowMemberWhenRegionDirectiveEndsTheGapInTypeBody` (renamed from `…Statement…InMethodBody`), `InsertsNoBlankLineInFrontOfConditionalBlockWhenRegionDirectiveEndsTheGap`, `KeepsAuthorBlankLineInFrontOfDirectiveWhenRegionDirectiveEndsTheGap`, `InsertsBlankLineBelowEndRegionInFrontOfDirectiveBeforeClosingBrace`, `KeepsBlankLineBelowEndRegionInFrontOfDirectiveBeforeClosingBrace` | method body changed to type body | FMT-UNSUP-023 |
| `GluedCommentBlankLineTests.KeepsBlankLinesOfGapWithDirectiveInFrontOfGluedComment` | the new FMT-BLANK-018 example | FMT-BLANK-018 (clarified) |
| `LineStartIndentationTests.RegionDirectivesTakeTheMemberColumn` (renamed from `…MemberOrLabelColumn`) | the new FMT-INDENT-010 example (namespace, nested type) | FMT-INDENT-010 (changed) |
| `RegionFormattingRequirementTests.RegionClosedInsideNestedTypeBodyKeepsItsDirectivesAndIsNamed` (renamed), `RegionLineFormTests.SeveralSpacesInRegionLineOfStraddlingPairBecomeOneSpace` | the straddle crosses a nested type body | FMT-REGION-005 (clarified) |
| `StraddlingRegionPairTests.RegionOpenedInsideANestedTypeBodyAndClosedOutsideIsPreservedAsAPair` (new; replaces 4 deleted) | straddle across a nested type | FMT-REGION-005 |
| `BlankLineBeforeIfDirectiveAfterStatementTests.RegionDirectiveBetweenMembersHoldsNoDoubledBlankLine` (renamed from `…Statements…`) | fields instead of statements | FMT-UNSUP-023 |
| `ReihitsuFormatterTests.FormatNodePreservesRegionsInsideDetachedNode`, `FormatNodeInDocumentAsyncPreservesRegionsInsideTargetRoot`, `FormatNodeFormatsDetachedNodeHoldingAForeignLineBreak` | region between the members of a class, not in a method body | FMT-UNSUP-023 (FMT-PIPE-028's point kept) |
| `ReihitsuFormatterNodeBlankLineTests.KeepsBlankLineAboveNestedTypeHoldingARegion` (renamed from `…PropertyHoldingARegion`) | the target is a nested type. It passed vacuously with the refusal (expected = input). The helper `FormatTarget` now asserts that the target is not refused. | FMT-UNSUP-023, FC-017 |
| `ReihitsuFormatterNodeRegionTests` helper `FormatOnce` | now asserts that the target is not refused. All its tests still pass. | FC-017 |
| `RH5024…FormatterTests.VerifyFormatterKeepsBlankLineBelowRegionDirectiveAndAnalyzerReportsNothing` | region in a nested type in front of its `}` | FMT-UNSUP-023 |

Deleted. Each one's position is refused now, and no position between members keeps its point. The refusal for each position is a row of `RegionInsideAMemberIsUnsupported` or `RegionInTheHeaderOf…`:
- `DirectiveBeforeEndRegionBlankLineTests`: 6 tests at a collection expression's `]`, a list pattern's `]`, and a parenthesized pattern's `)`.
- `ClosingBracketBlankLineTests`: `KeepsAndInsertsBlankLineBelowEndRegionBeforeClosingBracket`, `KeepsBlankLineBelowEndRegionBeforeClosingParenthesisOfParenthesizedPattern`.
- `PositionalPatternClosingParenthesisBlankLineTests.KeepsAndInsertsBlankLineBelowEndRegionBeforeClosingParenthesisOfPositionalPatternClause`.
- `GluedCommentBlankLineTests.KeepsBlankLineBelowEndRegionInFrontOfCommentGluedToListPatternBracket`.
- `DelimitedDocumentationBeforeBlockStatementTests`: `KeepsRegionDirectiveBeforeBlockStatement`, `SeparatesDelimitedDocumentationBeforeRegionAndBlockStatementInOnePass`.
- `RegionDirectiveLineAboveTests`: `…AfterMethodBodyBrace…`, `…AfterSwitchBlockBrace…`, `…AfterAccessorListBrace…`. The type and namespace variants stay.
- `RegionDirectlyAfterOpeningBraceTests`: 4 `EmptyRegionAfter{Method,StatementBlock,SwitchStatement,AccessorBody}Brace…` tests and `CommentFirstAfterMethodBraceIsSeparatedFromRegionHoldingStatement`.
- `RegionFormattingFullPipelineTests`: 10 tests on regions in accessor lists, accessor bodies, statements, blocks, and local functions. `InactiveRegionInsideAccessorListIsStillNamed` stays.
- `LineStartIndentationTests.RegionDirectivesInsideABlockTakeTheStatementColumn`.
- `MethodChainAlignmentTests.RegionAboveCommentExemptChainWithSharedInvokedLinkLineStartsEveryLinkAtRootColumn`.
- `ChainedContinuationKeywordTests.RegionBetweenAlreadyBracedIfAndElseStaysUnchanged`.
- `LineBreaksConsolidationStabilityTests.IfAndWhileRegionBeforeBlockNormalizeIdentically`.
- `ControlFlowBraceElsePlacementTests.RegionBetweenUnbracedThenBranchAndElseIsPreserved`.
- `ExpressionBodiedAccessorTests.RegionInsideBodyKeepsBlock`, `RegionInsideExpressionKeepsBlock` (FMT-STRUCT-013).
- `ExpressionBodyDirectiveGuardTests.MethodWithRegionAfterExpressionIsNotConverted` (FMT-STRUCT-007).
- `GetOnlyMemberExpressionBodyTests.RegionInsideAccessorListKeepsAccessorList` (FMT-STRUCT-015).
- `RegionLineFormTests.SeveralSpacesInRegionLineInsideMethodBodyBecomeOneSpace`.
- `StraddlingRegionPairTests`: 4 tests on a method body or an accessor list.
- `ReihitsuFormatterNodeRegionTests.FormatNodeInDocumentAsyncKeepsRegionInsideFormattedMethod`. The refusal test in U replaces it.
- `RH5025`/`RH5026`/`RH5027` `…FormatterTests.VerifyFormatterKeepsBlankLineBelowRegionDirectiveAndAnalyzerReportsNothing`. A `{` with a region above it, `else`, and a do-`while` all stand inside a member. Their requirements keep 1 other tagged test each.
- `Unit/BlankLines/BlankLineRegionDirectivePhaseTests.cs` (whole file, 2 phase-level tests tagged FMT-BLANK-013). They asserted the switch-label example that FMT-BLANK-013 removed for an input no caller reaches any more (`AGENTS.md`, *Supported inputs*).

Array layout changed per FMT-BREAK-046 (batch 1b, answer 3A: every array initializer). All fail until implemented:
- `InitializerLeadingCommaTests.SpecificationExampleArrayInitializerCommaMovesAndKeepsTheOtherLineBreaks`. Its `int[] y` output is FMT-BREAK-038's spec example, which is stale; see *Spec findings*.
- `InitializerLeadingCommaTests.CommaInImplicitArrayAndStackallocInitializersEndsTheElementsLine`.
- `TrailingCommaCommentOwnerTests.CommentsOfRemovedTrailingCommasBelowTheLastElementLeadTheClosingBrace` and `TrailingCommentOfAnInitializerCommaOnTheNextLineLeadsTheClosingBrace`. The first asserts the FMT-STRUCT-027 spec example, which is stale; see *Spec findings*.
- `TrailingCommaDocumentationCommentTests.DocumentationCommentInFrontOfTheArrayInitializerTrailingCommaStaysBehindTheLastElement` (3 documentation modes).
- `TabReplacementTests.InitializerElementBehindTabAndDelimitedDocumentationCommentFormatsLikeFourSpaces`.
- `LongExpressionLayoutTests.ArrayInitializerWithCommentAndDirectiveKeepsBoth`.
- `MethodChainAlignmentTests`, 6 tests: `ArgumentListUnder…`, `StatementLambdaUnder…`, `TernaryUnder…`, `MultiLineArrayInitializerWithTrailingCloseBrace…`, `MultiLineImplicitArrayInitializerWithTrailingCloseBrace…`, `ChainRootedInParenthesizedArrayCreation…`.
  - The continuation lines are aligned to the first link, which now stands behind `}`. So each moves left by the same amount as `}`.
  - This follows the existing `ChainRootedInCollectionExpressionAlignsToInvokedLink` layout (`].Select` / ` .ToList()`).
  - The summaries that said "closing brace shares its line with an element" are reworded.
- `RH5410…FormatterTests.VerifyFormatterPreservesSameLineCommentOnFinalArrayInitializerItem` (comment behind the last element keeps its line).

Checked and left unchanged:
- The `ArgumentListLineStartTests` (848, 1022, 1078) and `AttachedPartLineStartTests` (420) inputs become one line after the join (answer 6A). They stay as written.
- `UnsupportedListSeparatorCommentTests` rows: refused, or checked for stability only.
- `CliHiddenCodeEndToEndTests.BinaryOperatorDirectiveSource`: refused.
- `CliLanguageVersionEndToEndTests.TopLevelUnionLocalFormatted`: one line.
- `StructuralTransformPhaseIntegrationTests` (584–595) is phase-level, has no tag, and checks trailing-comma removal. It stays.
- The `ArgumentListLineStartTests.RegionDirectiveInTheGapIsUnsupported` row and `RegionFormattingFullPipelineTests.RegionBehindTheArrowOfALambdaIsUnsupported` keep FMT-UNSUP-014 and FMT-UNSUP-017, which are listed before FMT-UNSUP-023.
- The skeptic named `TrailingCommaCommentOwnerTests` 35–122 (only the two full-pipeline tests change; the phase-tree tests stay) and `ArgumentListLineStartTests` 849/1023/1079 (unchanged, one line).

## Spec findings (for the lead)

- **FMT-BREAK-038's example output is stale.** `sdd/specs/formatter/line-breaks.md` around line 3496 still shows `int[] y = { 1,⏎2 };`. FMT-BREAK-046 gives `int[] y = {` / `1,` / `2` / `};`. The test asserts the FMT-BREAK-046 layout.
- **FMT-STRUCT-027's example output is stale.** `sdd/specs/formatter/structural-transforms.md:1566` shows `int[] _a = new[] { 1,⏎2⏎⏎// comma⏎};`. FMT-BREAK-046 gives `new[]` / `{` under `new` / `1,` / `2` / blank / `// comma` / `};`. The test asserts the FMT-BREAK-046 layout.
- **FMT-BLANK-019 still has its region sentence.** It says the formatter keeps or inserts the blank line below a region directive in front of a `]` or `)`. FMT-UNSUP-023 now refuses every such position. Its region tests are deleted.
- **The region clauses of FMT-STRUCT-007 and FMT-STRUCT-013 can no longer be observed through the formatter.** These are a region in the span from `=>` to `;`, and a region in an accessor's block. The change doc keeps the shared predicates for the analyzers, but the formatter requirements still state them. Their formatter tests are deleted.

The spec scanner (array mode over `sdd/specs` and `documentation`) found no other stale array example in a formatter output block. The analyzer-spec hits are markup or inputs.

## Not covered, and why

- **FC-040 (documentation mode):** the shape is a directive, and `///` or `/** */` text never holds or moves a region directive, so the parse does not change with the mode. One closure row has a `///` comment below a region.
- **Location of every row:** each DataRow checks the reported span with `AssertLocationIsShape`. Line and column are checked for the spec examples and change-doc entries 1–4, 14, 15, and for the CLI.
- **FC-043 on the code-fix side for an enum member (RH5111):** the formatter-level context test pins the decision. Whether RH5111 asks `FindUnsupportedSyntax` about the context is unchanged by this run, and no RH5111 region test exists.
- **Every other code fix that formats a member (RH5101, RH5304, RH5306, RH5307, …):**
  - They reach the refusal through the one shared path (`ReihitsuFormatter` entry points → precheck).
  - The analyzer suite run with the throwaway check had no code-fix failures: no existing fixture formats a node with a region inside.
  - The two entry-6 fixtures are the tests.
- **Fix All / one-pass convergence:** no fix changes behavior except by being withheld.
- **FC-013 (analyzer parity for FMT-BREAK-046):** no rule reports the brace layout (follow-up F5, P11).
- **A comment that leads the line of a `new[]` brace (`new[]⏎/* c */ {`):** the column of a comment-led line in front of a creation's brace is not stated. The row case (entry 17) and `new[] /* c */ {` (batch 3) cover the rule.
- **FC-023/-024/-025/-026/-036 and FC-018:** these concern identifiers, renames, `partial`, and expression-bodied kinds. No changed code depends on them.
- **FC-011:** no loop is added.
- **`BlankLine*`/`Region*` phase guards the change doc keeps:** they are exercised by the remaining between-member tests.
- **Self-hosting:** after FMT-BREAK-046 is implemented, `tests/Reihitsu.SelfHosting.Test/Analyzers/AnalyzerViolationReportTests.cs:217` (`new[]⏎{ $"…",` with the first element on the brace's line) will be reformatted. I did not pre-format it: today's formatter turns the edited layout into `{$"…"`, which is the defect in follow-up 1. The cleaner should reformat it once the change is in.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~UnsupportedRegionInsideMemberTests|FullyQualifiedName~ArrayInitializerBraceLineTests"
scripts/test.sh --project formatter
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5305CollectionExpressionsShouldBeFormattedCorrectlyAnalyzerTests|FullyQualifiedName~RH5206SwitchExpressionBracesShouldBeAnchoredAnalyzerTests|FullyQualifiedName~Formatter.Formatting.RH5024|FullyQualifiedName~Formatter.Formatting.RH5410"
scripts/test.sh --project cli --filter "FullyQualifiedName~CliHiddenCodeEndToEndTests"
scripts/test.sh --project architecture
scripts/test.sh --project selfhosting
```

## Follow-up candidates

1. **Space dropped in an argument's array initializer.** Inside an argument list, today's formatter joins an interpolated string that is the first element of a multi-line array initializer onto the `{` and drops the space. `M(null, new[]⏎{⏎$"a",⏎$"b"⏎});` becomes `{$"a",`. A plain string literal is not joined. FMT-BREAK-046 now requires that element to start the line behind `{`, so `InterpolatedStringAsTheFirstElementInAnArgumentStartsTheLineBehindTheBrace` covers it in this run. The dropped space alone (FMT-SPACE) may still deserve its own issue if the implementation leaves it.
2. **From 03/05, unchanged:** F4 (RH7303 and the positions FMT-UNSUP-023 refuses), F5 (no rule reports the FMT-BREAK-046 layout), and F3 as narrowed.
