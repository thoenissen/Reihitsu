Role: test implementer
Commit: 1ef666a
Verdict: #953: tests written; #1852: tests written; #1896: tests written; #1818: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the spec diff (`git diff origin/main -- sdd/specs`), the change docs under `sdd/changes/2026-11/`, `06-spec-author.md`, and the lead's decisions 5 to 8 in `00-lead.md`; none from a patched copy of the code. Every new and changed test was run on this head (`main`'s code): it fails for the difference the spec makes, or, where it pins an other side or a closure, it passes. Columns below are 0-based.

New reason constants for FMT-UNSUP-017 (one per table row), FMT-UNSUP-018, and FMT-UNSUP-019 are in `src/Reihitsu.Formatter.Test/Helpers/FormatterTestsBase.cs`; their texts are the spec's reasons verbatim.

## thoenissen/Reihitsu#953

### Tests

New files (all under `src/Reihitsu.Formatter.Test/`):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Regression/LineBreaks/ContinuationJoinTests.SpecExampleEveryGapIsJoined` | FMT-BREAK-041 | `AssertRuleResult` (LF, CRLF, 2nd pass, phase stability) | fails: every gap kept |
| `…ContinuationJoinTests.SpecExampleBlankLineInTheGapGoesWithTheJoin`, `ReturnValueOnTheNextLineJoinsTheReturn` | FMT-BREAK-041 | `AssertRuleResult` | fails |
| `…ContinuationJoinTests.SpecExampleMultiLineParenthesizedPatternKeepsItsOpeningParenthesisBelowIs` | FMT-BREAK-041, FMT-INDENT-026 | `AssertRuleResult` | fails: content in column 8 |
| `…ContinuationJoinTests.MultiLinePropertyPatternKeepsItsOpeningBraceBelowIs` | FMT-BREAK-041 (exception) | `AssertRuleResult` | passes (other side) |
| `…ContinuationJoinTests.SpecExampleEqualsOfAnAnonymousObjectMemberAndOfALetClauseJoinsBothSides` | FMT-BREAK-016 | `AssertRuleResult` | fails |
| `…ContinuationJoinTests.EachGapIsJoined` (20 rows), `EachGapOfABlockStatementIsJoined` (4), `EachArmGapIsJoined` (3), `ModifierGapsOfOtherDeclarationsAreJoined` | FMT-BREAK-041 | `AssertRuleResult` | fail |
| `…ContinuationJoinTests.ModifierGapOfAnAccessorIsJoined` | FMT-BREAK-041 | `FormatWithPipeline` + `AssertSecondPassIsStable` | passes: `main` already joins it through the accessor-list layout |
| `…ContinuationJoinTests.GapsOutsideTheListKeepTheirLineBreak` (6), `LineBreakBehindTheColonOfABaseListIsKept`, `StatementWithoutAnExpressionKeepsTheLineBreakInFrontOfItsSemicolon` (2) | FMT-BREAK-041 (other side) | line-break assertion + `AssertSecondPassIsStable` | pass |
| `…ContinuationJoinTests.JoinAppliesInANodeLevelRoot` | FMT-BREAK-041 | `FormatNode` | fails |
| `Regression/LineBreaks/PatternChainLayoutTests` — `SpecExampleEveryCombinatorStartsItsLine`, `SpecExampleAChainWrappedAtOneCombinatorIsWrappedAtEveryCombinator`, `CombinatorsOfNestedBinaryPatternsBelongToOneChain`, `ParenthesizedPatternStartsAChainOfItsOwn`, `ChainInsideAnArgumentStartsEachCombinatorOnItsLine`, `ChainWhoseOnlyBreakIsNextToAMultiLinePropertyPatternIsWrappedAtEveryCombinator` (lead decision 8) | FMT-BREAK-017 | `AssertRuleResult` | fail |
| `…PatternChainLayoutTests.SpecExampleAChainThatIsNotWrappedKeepsItsLayout` | FMT-BREAK-017 | `AssertRuleResult(input)` | passes |
| `…PatternChainLayoutTests.CombinatorInFrontOfAMultiLineParenthesizedPatternStartsItsLine` (05 finding 2) | FMT-BREAK-017, FMT-INDENT-026 | `AssertRuleResult` | fails: content in column 8 |
| `…PatternChainLayoutTests.SpecExampleContentOfAParenthesizedPatternIsIndentedFourColumnsRightOfTheParenthesis` | FMT-INDENT-026 | `AssertRuleResult` | fails |
| `Regression/Indentation/ContinuationLineIndentationTests` — `SpecExampleValueKeptOnItsOwnLineByACommentIsIndentedOneLevel`, `SpecExampleContinuationInsideACollectionElementIsIndentedFromTheElementsLine`, `SpecExampleLinesInsideTheHoleOfAVerbatimInterpolatedStringAreIndentedFromTheStringsLine`, `LinesInsideTheHoleOfAVerbatimInterpolatedStringInASwitchArmAreIndentedFromTheArmsLine` (lead decision 7), `SpecExampleContinuationOfAnAssignmentInsideALogicalExpressionIsIndentedFromTheStatementsLine`, `ContinuationOfAnAssignmentInsideAConditionIsIndentedFromTheStatementsLine`, `ContinuationOfAContinuationLineIsIndentedOneMoreLevel`, `SemicolonBehindAConditionalGroupIsAContinuationLine` | FMT-INDENT-034 | `AssertRuleResult` | fail |
| `…ContinuationLineIndentationTests.SpecExampleLinesPlacedByBlockIndentationAreNoContinuationLines`, `OtherLinesPlacedByBlockIndentationAreNoContinuationLines` | FMT-INDENT-034 (other side) | `AssertRuleResult(input)` | pass |
| `Unit/Precheck/UnsupportedJoinedGapCommentTests` — 3 spec examples (refused, with line/column; the conditional-modifier one in both builds; the `return⏎#if` one in both builds), each gap of the examples alone, every table row (36 rows, one line and across lines), the declaration rows (10), directives / disabled text / documentation comments in 3 documentation modes, conditional modifiers per build, the whole-member group (other side), the other-side rows (17 + 2 + 2), the gaps of FMT-UNSUP-011/012/014 keeping their reason, the overlap with FMT-UNSUP-004/010, the gap in a list element / hole / lambda / arm / local function, disabled text per build, `FormatDocumentAsync`, every node-level entry point, FMT-UNSUP-003 (expression behind `return`, pattern behind `is`, base list without its type), `FindUnsupportedSyntax` on a built node, closure of 7 joins and of a converted expression body | FMT-UNSUP-017 (+ FMT-UNSUP-003, -004, -005, -010 where the table of the test names them) | `AssertUnsupportedFor`, `AssertSupported`, entry points | refusals fail (not refused); other sides, closures, and the X-undefined builds pass |
| `Unit/Precheck/UnsupportedOmittedTypeArgumentCommentTests` — spec examples (location 4:63), every place in the list in 3 documentation modes (11 rows), other side (4), disabled text, overlap with FMT-UNSUP-009/004, node holding the list, closure | FMT-UNSUP-018 (+ -003, -004, -005, -009) | as above | refusals fail; other sides pass |
| `Unit/Precheck/UnsupportedAssignmentInArmCommentTests` — spec example (location 6:29), spec other side, every assignment operator in an arm / a member / its `when` / nested argument, array, lambda (10), outside the shape (6, incl. a statement in a lambda block inside an arm), the member's own `=` (FMT-UNSUP-017), disabled text, FMT-UNSUP-003 (assignment without its arm), closure | FMT-UNSUP-019 (+ -003, -005, -017) | as above | refusals fail; other sides pass |

New analyzer tests (`src/Reihitsu.Analyzer.Test/`):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Formatting/RH5302…AnalyzerTests.VerifyFixIndentsAContinuationOnTheRootsFirstLineFromTheDocumentsLineStart` (LF, CRLF) | FMT-INDENT-034, FMT-PIPE-024 (FC-028) | `ApplyCodeFixAsync` | fails: `3) > 0` in column 16, not 12 |
| `Formatting/RH5308…AnalyzerTests.VerifyFixIndentsAContinuationOnTheRootsFirstLineFromTheDocumentsLineStart` (LF, CRLF) | FMT-INDENT-034, FMT-PIPE-024 (FC-028) | `ApplyCodeFixAsync` | fails |
| `Formatting/RH5305…AnalyzerTests.VerifyFixJoinsTheBodyOfALambdaElementBehindItsArrow` | FMT-BREAK-041 | `ApplyCodeFixAsync` | fails: `1,` on its own line |
| `Formatting/RH7101DoNotCombineFieldsAnalyzerTests.FixJoinsALineBreakBehindTheLastModifierInEveryField` | FMT-STRUCT-021, FMT-BREAK-041 | `Verify` | fails |
| `Formatting/RH7101DoNotCombineFieldsCommentAndDirectiveTests.VerifyNoFixIsOfferedForAShapeOfTheJoinedGapsInTheType` (4 rows) | RH7101-007, SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| `Design/RH2001…TriviaTests.VerifyNoFixIsOfferedForACommentBehindNewInTheInitializerValue`, `VerifyNoFixIsOfferedForACommentBehindAnAssignmentInsideAnAnonymousObjectMember` | RH2001-007 | `VerifyNoCodeFixIsOffered` | fail: fix offered |

Existing tests changed to the specified output (each fails on `main` now):

- Refused now (FMT-UNSUP-017), rewritten to `AssertUnsupportedFor` with the row's reason: `WithExpressionSpacingTests.CommentInFrontOfWithIsUnsupported` and `DocumentationCommentInFrontOfWithIsUnsupported`; `ThrowOperandPrecedenceExpressionBodyTests.CommentAfterThrowKeywordIsUnsupported` and `NextLineOperandWithCommentIsUnsupported`; `DelimitedDocumentationInExpressionTests.DelimitedDocumentationBehindLambdaArrowIsUnsupported`; `ExpressionBodiedAccessorTests.CommentAfterReturnKeywordIsUnsupported` and `CommentBeforeReturnedExpressionIsUnsupported`; `RegionFormattingFullPipelineTests.RegionBehindTheArrowOfALambdaIsUnsupported`; `ParameterContinuationLineAlignmentTests.LineCommentBehindAParameterModifierIsUnsupported`; `ListElementInteriorJoinTests.ParameterInteriorWrapHeldByLineCommentBehindAModifierIsUnsupported`; `CommentLedFirstTokenAnchorTests.ListPatternBehindCommentThatLeadsTheLineBehindIsIsUnsupported`.
- Precheck other sides that moved to FMT-UNSUP-017: `UnsupportedBinaryOperatorCommentTests` (spec example's `or 3` and `/* c7 */ is 5`; pattern-match and combinator rows now `CommentBehindIsOfAPatternMatchOrACombinatorGivesTheReasonOfTheJoinedGap`); `UnsupportedExpressionBodyArrowCommentTests` (spec example `(/* c5 */ 1)`; lambda rows now `CommentBehindTheArrowOfALambdaGivesTheReasonOfTheLambda`; closure rows G and H removed); `UnsupportedExpressionBodyArrowLeadingCommentTests` (spec example `1 => "one", // c6`; arm row now `CommentInFrontOfTheArrowOfASwitchArmGivesTheReasonOfTheArm`); `UnsupportedAttachedPartCommentTests` (`new /* c */ []` and `o is /* c */ [1]` now `CommentInFrontOfABracketBehindNewOrIsGivesTheReasonOfTheJoinedGap`); `UnsupportedFieldDeclarationDirectiveTests` (the conditional-modifier and between-modifiers rows now `DirectiveBehindAModifierOfAFieldWithOneDeclaratorGivesTheReasonOfTheModifier`, FMT-UNSUP-017's conditional-modifier rule).
- FMT-UNSUP-018: `ZeroWidthTokenLineStartTests` (four omitted-type-argument tests now refusals; the SHARED-BLANK-008 example now the rank specifier `int[, // c⏎]` with `]);` in column 12; the six rank-specifier tests' continuation lines +4, tagged FMT-INDENT-034); `UnsupportedListSeparatorCommentTests.CommentBehindTheCommaOfAnOmittedTypeArgumentGivesTheReasonOfTheOmittedTypeArgumentList`.
- FMT-INDENT-034 (`;` and the comment glued to it +4): `DocumentationCommentRequirementTests` (spec example, retagged), `StrandedDocumentationLineStartTests` (4 `;`, the lambda `=> x;` and the assignment `= 1;`, retagged FMT-INDENT-034), `StrandedDocumentationTokenSpacingTests` (10), `ExpressionBodyDirectiveGuardTests` (3, incl. the FMT-STRUCT-007 example), `ExpressionBodiedAccessorTests` (3), `GetOnlyMemberExpressionBodyTests` (3), `ExpressionBodiedMethodTests`, `ExpressionBodyNextLineExpressionTests.DirectiveAfterNextLineExpressionConvertsInOnePass`, `ThrowOperandPrecedenceExpressionBodyTests` (1), `LineBreakRequirementTests` (2), `UsingDirectiveCommentTextTests` (3: SHARED-USING-009 example and two siblings), `CommentBeforeBraceBlankLineTests` (2, the FMT-BLANK-020 example), `HorizontalSpacingRewriterTests` (FMT-SPACE-001 example; phase-only, passes either way), analyzer `RH3202`/`RH3203`/`RH3205`/`RH3206`/`RH3207`/`RH3208` directive-before-`;` fixes and the SHARED-BODY-001 RH3203 example, `RH5113…FormatterTests` (2), `RH6003…FormatterTests`, `RH5025…FormatterTests`.
- `CommentLedFirstTokenAnchorTests` (12 tests): the comment-led line stood behind `return`, now a refused gap; `return` became `_ =`, so the comment-led line is a continuation line and every anchored line moves +4, keeping each test's FMT-INDENT-030 anchor. `RH5112…AnalyzerTests.VerifyFixOfChainBehindLineLeadingCommentMatchesTheFormatter` got the same rewrite (its whole-document `Format` would be refused otherwise).
- Joins: `FieldDeclarationSplitTests` (spec example of FMT-STRUCT-021, and the two modifier tests renamed `…IsJoinedInEveryField`, tagged FMT-BREAK-041), `RH7101DoNotCombineFieldsAnalyzerTests.FixRepeatsLineBreaksInsideAndBehindTheHeadInEveryField`, `RH7101DoNotCombineFieldsFormatterTests.VerifyFormatterRepeatsLineBreaksInsideAndBehindTheHeadInEveryField`, `JoinSeparatorTests.BinaryOperatorsMovedInFrontOfAFusingOperandAreSpaced` (`var e = o is int;`), `LineBreaksConsolidationStabilityTests.ListPatternDesignationStaysOnCloseBracketLine` (now asserts `value is [` and `] items)` only, because no requirement fixes the list pattern's columns once `[` joins), `ParenthesizedLambdaLineStartTests` (two tests now `return (x) => x;`).

### Not covered, and why

- RH5306 fix behind `v is // note`: pinned by `RH5306…Tests.VerifyFixLaysOutListPatternBehindTrailingCommentOfPrecedingToken` (+CRLF); the root holds no `is` (FMT-UNSUP-003), so no new test.
- RH3209 `get { return /* c */ _value; }`: pinned by `RH3209…AnalyzerTests` (~line 1284), unchanged (kept Core guard).
- Query clauses inside a list element: pinned by `ArgumentContinuationLineAlignmentTests.QueryClausesOfNamedArgumentAreAlignedWithTheQueryStart`.
- RH5111 context example (`b` keeps its column): pinned by the existing RH5111 context tests; the fix writes no new column.
- CLI: no CLI test added; the CLI reports `result.Unsupported.Reason` generically, and every reason is pinned through `FormatSyntaxTree` and `FormatDocumentAsync`.
- `return⏎from x in a⏎select x` (05): `QueryClauseAlignmentTests.QueryBehindReturnJoinsAndItsClausesAlignWithFrom` (#1896 section).
- Failure classes: FC-002/FC-003 (both sides of every gap, the type-test `is`, `return⏎;`/`throw⏎;`, the parenthesized/property-pattern exception) covered; FC-010/FC-017 by the second pass inside every `AssertRuleResult`/`AssertSupported`; line endings by LF and CRLF in every helper; FC-020/FC-021 (directives per build) covered for the conditional modifiers, `return⏎#if`, the omitted list, the arm, and the `#endif` declarators; FC-027 (nesting) by the list-element, hole, lambda, arm rows; FC-028 by the RH5302/RH5308 fix tests; FC-038 to FC-043 (declared shapes) by the three precheck classes (examples, both sides, every entry point, `FindUnsupportedSyntax`, node-level parts, closure). FC-013 (analyzer/formatter parity) does not apply: no analyzer reports a joined gap (05).

### Findings for the lead (not changed by me)

1. **RH5419-004 still names "a line break … between a modifier, the type" as one the formatter keeps.** FMT-BREAK-041 now joins it. `RH5419…AnalyzerTests.VerifyFixIsWithheldForALineBreakBetweenAModifierAndTheType` (`public⏎int this[int a,⏎int b] => a;`) pins the withheld fix and was left unchanged; it fails or holds depending on how the fix predicts the formatter. A spec question for the spec author.
2. **Function-pointer parameter modifiers.** FMT-UNSUP-017's modifier row says "a parameter"; whether `delegate*<ref // keep⏎int, void>` (a `FunctionPointerParameterSyntax`) counts is not stated. `ParameterContinuationLineAlignmentTests.FunctionPointerParameterTypeAfterModifierAndLineCommentStaysAligned` was left unchanged.
3. **RH5201 tests become vacuous.** Twelve `RH5201…AnalyzerTests.VerifyNoDiagnosticForChain…LaidOutAsTheFormatter` tests (lines ~3330–3600) hold `return⏎/* c */ s.Trim()` and call `AssertFormattingChangesNothing`; the formatter now refuses the input and returns it unchanged, so that half passes without checking layout (FC-017). Left unchanged; the `_ =` rewrite used for `CommentLedFirstTokenAnchorTests` would restore them.

### Follow-up candidates

_None_ beyond the findings above.

## thoenissen/Reihitsu#1852

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Regression/Structural/StructuralTransformRequirementTests.ConvertedBodyKeepsCommentBeforeSemicolonAndJoinsNextLineExpression` (spec example with `Third`) | FMT-STRUCT-006 | `AssertRuleResult` | fails: `return⏎3;` |
| `Regression/Structural/ExpressionBodyNextLineExpressionTests.BlankLineBeforeExpressionJoinsTheCreatedReturn` (one blank line, one holding whitespace, two) — replaces `BlankLineBeforeExpressionKeepsExpressionOnOwnLine` | FMT-STRUCT-006 | `AssertRuleResult` | fails |
| `…ExpressionBodyNextLineExpressionTests.BlankLineBeforeExpressionJoinsTheCreatedReturnOfEveryConvertedMember` (operator, conversion operator, local function) | FMT-STRUCT-006 | `AssertRuleResult` | fails |
| `…ExpressionBodyNextLineExpressionTests.BlankLineBeforeExpressionOfAVoidMethodDisappears` | FMT-STRUCT-006 | `AssertRuleResult` | passes on `main` (already the specified output) |
| `RH3202`, `RH3205`, `RH3206`, `RH3208` `…AnalyzerTests.VerifyExpressionBehindABlankLineIsFixedToSingleSpacedReturn` (LF, CRLF) | FMT-STRUCT-006, SHARED-BODY-003 | `Verify` | fail: `return⏎42;` |
| `ContinuationJoinTests.ReturnValueOnTheNextLineJoinsTheReturn`, `SpecExampleBlankLineInTheGapGoesWithTheJoin` | FMT-BREAK-041 | `AssertRuleResult` | fail (the issue's `return⏎2;`) |
| `UnsupportedJoinedGapCommentTests` return rows and `SpecExampleDirectiveBetweenReturnAndItsValueIsUnsupportedInEveryBuild` | FMT-UNSUP-017 | `AssertUnsupportedFor` | fail |

### Not covered, and why

The no-blank-line side is pinned by the existing `…OnNextLineConvertsInOnePass` tests in `ExpressionBodyNextLineExpressionTests` and the four `VerifyExpressionOnNextLineIsFixedToSingleSpacedReturn` analyzer tests. A comment behind the arrow stays FMT-UNSUP-012's (existing tests).

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1896

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Regression/Indentation/QueryClauseAlignmentTests.SpecExampleQueryClausesAlignWithTheQuerysFirstToken` | FMT-INDENT-035 | `AssertRuleResult` | fails |
| `…SelectClauseOfAQueryInALocalDeclarationAlignsWithFrom` (the issue's input) | FMT-INDENT-035 | `AssertRuleResult` | fails |
| `…QueryBehindReturnJoinsAndItsClausesAlignWithFrom` | FMT-INDENT-035, FMT-BREAK-041 | `AssertRuleResult` | fails |
| `…IntoThatStartsItsLineAlignsWithFrom`, `ClausesOfANestedQueryAlignWithTheNestedFrom` | FMT-INDENT-035 | `AssertRuleResult` | fail |
| `…LinesInsideAClauseAreContinuationLinesOfTheClause` (`orderby x,⏎-x`, `group x⏎by x`) | FMT-INDENT-035, FMT-INDENT-034 | `AssertRuleResult` | fails |
| `…AlignedClausesAreLeftUnchanged` | FMT-INDENT-035 | `AssertRuleResult(input)` | fails: `main` moves aligned clauses outside a list element to column 8 |
| `ContinuationJoinTests.SpecExampleEqualsOfAnAnonymousObjectMemberAndOfALetClauseJoinsBothSides` (the `let` part) | FMT-BREAK-016 | `AssertRuleResult` | fails |

### Not covered, and why

The query inside an argument is pinned by `ArgumentContinuationLineAlignmentTests.QueryClausesOfNamedArgumentAreAlignedWithTheQueryStart`.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1818

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ContinuationLineIndentationTests.DeclaratorBehindEndifInEventFieldAndLocalIsIndentedOneLevel` (the issue's input as the fixed point, and `main`'s output formatted to it) | FMT-INDENT-034 | `AssertRuleResult` | fails: `D;` in column 4, `c;` in column 8 |
| `…SpecExampleLaterDeclaratorsAreIndentedOneLevel` | FMT-INDENT-034 | `AssertRuleResult` | fails |
| `…DeclaratorsInsideAndBehindAnActiveConditionalGroupAreIndentedOneLevel` (`X` defined) | FMT-INDENT-034 (FC-020) | `AssertRuleResult` | fails |
| `…DeclaratorInTheActiveBranchOfAnElseOrElifGroupIsIndentedOneLevel` (`#else`, `#elif`) | FMT-INDENT-034 (FC-020) | `AssertRuleResult` | fails |
| `…FirstDeclaratorBehindEndifDirectlyBehindTheTypeIsIndentedOneLevel` (both builds) | FMT-INDENT-034 (FC-020) | `AssertRuleResult` | fails |

### Not covered, and why

A field with several declarators and a directive is refused (FMT-UNSUP-010, existing tests); disabled declarators keep their columns inside every test above (FMT-INDENT-011).

### Follow-up candidates

_None._

## Whole run

- Build: `scripts/build.sh` passed; `scripts/format.sh --force` over every changed test file formatted nothing further.
- `scripts/test.sh --project formatter`: 268 failed, 6803 passed; every failure is in a file listed above, and the failures sampled show the spec's difference (a kept gap, a block column, a missing refusal).
- `scripts/test.sh --project analyzer`: 40 failed, 11862 passed; every failure is a test listed above.
- `scripts/test.sh --project architecture`: 198 passed (`EveryRequirementOfAnEnforcedSpecIsCovered` now passes: FMT-INDENT-034, FMT-INDENT-035, FMT-BREAK-041, FMT-UNSUP-017, FMT-UNSUP-018, FMT-UNSUP-019 are tagged).
- Residual risk: existing tests that pin an output the spec diff changes were found by searching the test projects for lines ending in `return`, `throw`, `await`, `new`, ` is`, ` not`, ` or`, ` and`, a `foreach … in`, a bare `;` in a statement's column, a modifier alone on its line, lines starting with `when`, `=>`, `with`, `or`, `and`, comments behind the keywords and modifiers of the FMT-UNSUP-017 table, and `Dictionary<,`/`List<>` with a comment. A continuation line held by something else, in a test I did not find, will surface as an implementer failure; it goes back through the lead.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~ContinuationLineIndentationTests|FullyQualifiedName~QueryClauseAlignmentTests|FullyQualifiedName~ContinuationJoinTests|FullyQualifiedName~PatternChainLayoutTests"
scripts/test.sh --project formatter --filter "FullyQualifiedName~UnsupportedJoinedGapCommentTests|FullyQualifiedName~UnsupportedOmittedTypeArgumentCommentTests|FullyQualifiedName~UnsupportedAssignmentInArmCommentTests"
scripts/test.sh --project formatter --filter "FullyQualifiedName~ExpressionBodyNextLineExpressionTests|FullyQualifiedName~StructuralTransformRequirementTests|FullyQualifiedName~ZeroWidthTokenLineStartTests|FullyQualifiedName~CommentLedFirstTokenAnchorTests|FullyQualifiedName~StrandedDocumentation|FullyQualifiedName~FieldDeclarationSplitTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~VerifyExpressionBehindABlankLineIsFixedToSingleSpacedReturn|FullyQualifiedName~VerifyFixIndentsAContinuationOnTheRootsFirstLine|FullyQualifiedName~VerifyFixJoinsTheBodyOfALambdaElement|FullyQualifiedName~RH7101|FullyQualifiedName~RH2001PrivateAutoPropertiesShouldNotBeUsedTriviaTests|FullyQualifiedName~RH5112"
scripts/test.sh --project formatter
scripts/test.sh --project analyzer
scripts/test.sh --project architecture
```

## Follow-up candidates

_None_ beyond #953's findings 1 to 3, which are for the lead to route.
