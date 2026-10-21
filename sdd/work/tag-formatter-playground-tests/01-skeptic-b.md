Role: skeptic
Commit: 8f78a04
Verdict: proceed

Spec group B: `formatter/line-breaks.md` (FMT-BREAK-001 to -028), `formatter/switch-case-braces.md` (FMT-SWITCH-001 to -013), `formatter/horizontal-spacing.md` (FMT-SPACE-001 to -015). No spec diff exists. The subject is the existing specs against the code at 8f78a04; production code under `src/Reihitsu.Formatter` and `src/Reihitsu.Core` is unchanged since 8e0f35d. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Premise

Lane premise holds. Every spec example of group B runs as written.

- Every `csharp` block of the three specs was extracted into a fixture (59 fixtures: an input with the block after it, or a block the text calls "left unchanged"). Each fixture was formatted with `scripts/format.sh <dir> --force`, under LF and again converted to CRLF. Each output equals the spec's expected block, apart from the final line break FMT-CLEAN removes. A second pass changed 0 of 59 files under CRLF.
- About 120 further probe fixtures (scratchpad, deleted afterwards) were formatted twice in the same way. None of them was non-idempotent. The findings below quote their real output.
- For the outputs the questions dispute, analyzer agreement (FC-013) was checked with `scripts/apply-fix.sh`: RH5402, RH5404, RH5417, and RH5103 give `no-diagnostic` on the LB-Q1 and LB-Q2 outputs. The RH5408-002/-003 exemptions cover the LB-Q3 shapes. RH5106/RH5107 cover only parenthesized parameter lists (LB-Q5). RH6011/RH6012, RH6021-002, RH6023, and RH6024-002 do not report the SP-Q2 pairs. I found no new analyzer/formatter disagreement.

Where a requirement is wrong about current behavior, or the case looks wrong, it is listed under *Open questions to file*. None blocks tagging; a test for a requirement avoids the disputed input.

## Mechanism and candidates

_N/A._ Not a bug. Dispatch facts the test implementer can rely on:

- `LineBreakPhase.CreateRewriters` runs 16 rewriters in this order: block, initializer, contained block, assignment, declaration semicolon, list-element interior join, list, property layout, generic constraint, declaration brace, member line start, attribute target, binary operator, chain, ternary, `case … when`.
- Brace placement: `DeclarationBraceLineBreakRewriter.Visit` handles class, struct, interface, record, enum, namespace, constructor, method, operator, conversion operator, destructor, indexer, event, local function, and delegate. It has no `ExtensionBlockDeclarationSyntax` case, while `MemberLineStartRewriter` and `LineBreakListRewriter` both visit extension blocks (LB-Q1). `LineBreakContainedBlockRewriter.TryGetContainedBlock` lists for, foreach, foreach-variable, while, do, using, lock, fixed, unsafe, labeled, checked, try, catch, finally, accessor, both lambda forms, and anonymous method. `LineBreakBlockRewriter` handles blocks, accessor lists, switch statements, switch expressions, and switch sections.
- Members: `MemberLineStartRewriter` covers compilation units (top-level statements included), namespaces, file-scoped namespaces, class, struct, interface, record, and extension blocks. It does not cover enums (a non-goal).
- Lists: `LineBreakListRewriter` lays out `ArgumentList`, `AttributeArgumentList`, and `ParameterList` one element per line. It joins `BracketedArgumentList`, `TypeArgumentList`, `TypeParameterList`, and `FunctionPointerParameterList`. It does not visit `BracketedParameterList`, the indexer parameter list (LB-Q5).
- Conditionals: `TernaryLineBreakRewriter.NormalizeTernaryOperatorPosition` counts a conditional as nested only when `WhenTrue` or `WhenFalse` is itself a `ConditionalExpressionSyntax` (LB-Q6).
- Auto-property collapse: `AccessorListLayout.CanCollapseAutoAccessorListToSingleLine` requires no comment or directive inside the list, none between the token before `{` and `{`, and a signature (first modifier or type to the token before `{`) on one line. Otherwise the property falls through to the multi-line layout (LB-Q3).
- Switch braces: `SwitchCaseBraceRewriter.IsMultiLineSection` checks labels first (`RecursivePattern`, `ListPattern`, or `ParenthesizedPattern` spanning lines, anywhere in the label, a `when` clause included). It then counts statements other than `break`, `return`, and `throw`. `IsEmptySection` (only `break`s) is consulted only when building, not when deciding (SW-Q1). `AddBraces` rebuilds label, first-statement, last-statement, and `break` trivia from comments only (SW-Q2). `SectionCarriesDirectives` checks the leading and trailing trivia of the labels and of the directly owned statements.
- Spacing: `SpacingPolicy` order is attribute-close-bracket, no-space, comma, single-space, operator, for-semicolon, keyword. `HorizontalSpacingRewriter.VisitToken` returns early when `AreSeparatedByEndOfLine` holds (EOL trivia only, so a line break inside a `/* */` comment does not count: SP-Q1). It also returns early when the next token's leading trivia holds any comment (in practice a `/** */` or `///`). `TrimGapBeforeToken` removes whitespace after such a comment only when the desired spacing is 0 and the comment is not `///`.
- Arrow spacing outside FMT-SPACE: `PropertyLayoutLineBreakRewriter.CollapseExpressionBody` rewrites the arrow of an expression-bodied property or indexer to single spaces on both sides (LB-Q8).

## Requirements against the code

"Holds" means the requirement states today's behavior and its example reproduces. Tests are starting points found by name and by spot-reading, not a verified final set. `AssertRuleResult` (`Helpers/FormatterTestsBase.cs`) runs the whole `FormattingPipeline`, exact output, LF and CRLF, two passes. Phase-level unit tests (`ApplyPhase`, `ExecuteLineBreakPhase`, `AssertHorizontalSpacing`) run one phase on raw input. Several assert only counts or `Contains` (FC-017). Paths are relative to `src/Reihitsu.Formatter.Test/`.

### formatter/line-breaks.md

| ID | Against the code | Existing tests (file: method) |
|---|---|---|
| FMT-BREAK-001 | Holds. Second sentence checked: `} /* c */ else`, `C() /* c */⏎    : this(1)`, `class C<T> /* c */⏎    where`, `[A] /* c */⏎[B]` keep the comment on the earlier line | `Regression/LineBreaks/CommentJoinTests.cs` (all 13); `Regression/LineBreaks/DirectiveJoinTests.cs` (all 9); `Regression/LineBreaks/BracketedArgumentTrailingCommentTests.cs`: `CommentBetweenTheBracketsStillBlocksTheCollapse`, `CommentBeforeTheClosingBracketStillBlocksTheCollapse`, `DirectiveBetweenTheBracketsStillBlocksTheCollapse`; `Regression/LineBreaks/ChainSplitMemberNameRejoinTests.cs`: `CommentBetweenDotAndMemberNameKeepsSplit`, `BlockCommentBetweenDotAndMemberNameKeepsSplit`, `OwnLineCommentBeforeMemberNameKeepsSplit`, `DirectiveBetweenDotAndMemberNameKeepsSplit`; `Regression/LineBreaks/ListElementInteriorJoinTests.cs`: `InteriorWrapHeldBy*` (5), `ParameterInteriorWrapHeldBy*` (2); `Regression/LineBreaks/CaseWhenClauseLayoutTests.cs`: `GuardSeparatedByCommentIsNotCollapsed`; second sentence: `Regression/LineBreaks/ChainedContinuationKeywordTests.cs`: `GluedElseWithBlockCommentOnBraceLineKeepsCommentOnBraceLine`, `Regression/LineBreaks/AccessorListAccessorLineBreakTests.cs`: `SameLineBlockCommentStaysWithPreviousAccessor`. The spec example as one test: none found |
| FMT-BREAK-002 | Holds, except C# 14 extension blocks (LB-Q1). Probed: enum, namespace (empty too), static constructor, finalizer, local function, `checked`/`unchecked`/`unsafe`/`fixed`/`lock`/labeled/bare blocks, anonymous method, empty lambda | `Regression/Indentation/BracePlacementTests.cs`: `BlockScopedNamespaceBracesAreMovedToNewLines`, `TypeAndMethodBracesAreMovedToNewLines`, `ConstructorBracesAreMovedToNewLines`, `IfElseBracesAreMovedToNewLines`, `ForLoopBracesAreMovedToNewLines`, `ForeachLoopBracesAreMovedToNewLines`, `WhileAndDoWhileBracesAreMovedToNewLines`, `TryCatchFinallyBracesAreMovedToNewLines`, `LambdaBlockBracesAreMovedToNewLines`, `FullyMinimizedSingleLineInputBracesAreMovedToNewLines`; `Regression/LineBreaks/ContainedBlockBraceTests.cs` (3); `Regression/LineBreaks/SingleLineTypeBraceWithLeadingUsingTests.cs` (3); `Integration/LineBreakPhaseIntegrationTests.cs`: `ConvertsKAndRBracesToAllmanStyle`, `PreservesAllmanStyleBraces`; `Unit/LineBreaks/LineBreakRewriterTests.cs`: `PlacesOpenBraceOnNewLine`, `PlacesCloseBraceOnNewLine`, `PlacesConversionOperatorBraceOnNewLine`, `PlacesDestructorBraceOnNewLine`, `HandlesEmptyBlock`, `HandlesNestedBraces` |
| FMT-BREAK-003 | Holds (`// c` after `{` too) | `Regression/LineBreaks/ChainedContinuationKeywordTests.cs`: `GluedElseWithBlockCommentOnBraceLineKeepsCommentOnBraceLine`; `Regression/LineBreaks/AccessorListDeclarationBraceTests.cs`: `CommentBeforeAccessorListOpenBraceIsPreserved`. `void M() /* c */ {` and `{ /* c */ }`: none found |
| FMT-BREAK-004 | Holds | `Regression/LineBreaks/ChainedContinuationKeywordTests.cs` (all 14); `Unit/LineBreaks/ContainedBlockRewriterTests.cs` (both) |
| FMT-BREAK-005 | Wrong for three inputs (LB-Q4) | `Regression/LineBreaks/ElseIfConditionCommentTests.cs`: `TrailingCommentOnPlainIfConditionMovesToOwnLine`, `TrailingCommentOnElseIfConditionDoesNotSplitElseIf` |
| FMT-BREAK-006 | Holds, except a multi-line block comment that ends mid-line (LB-Q2). `Foo(); ;` comes out as `Foo();;` (line breaks kept, FMT-SPACE-004 removes the space) | `Regression/LineBreaks/LineBreaksConsolidationStabilityTests.cs`: `TwoStatementsOnOneLineInBlockAreSplit`, `StatementAfterBlockCloseBraceStillMovesToOwnLine`; `Unit/LineBreaks/LineBreakBlockRewriterTests.cs`: `VisitBlockSplitsTwoStatementsOnOneLine`, `VisitSwitchSectionSplitsTwoStatementsOnOneLine`. Trailing comment kept, empty statement kept: none found |
| FMT-BREAK-007 | Holds, except LB-Q2 and the closing brace of an extension block (LB-Q1). Top-level statements are split (compilation-unit members) | `Regression/FullPipeline/OneLineClassBodyMembersFullPipelineTests.cs` (all 13; `KeepsEnumMembersOnSharedLine` is the non-goal, `KeepsMemberAfterMultiLineCommentEndingMidLine` is the LB-Q2 shape). Compilation unit, namespace, file-scoped namespace: none found |
| FMT-BREAK-008 | Holds. Empty switch expression `x switch { }` expands too. Arms on one line: decided change thoenissen/Reihitsu#956 | `Unit/LineBreaks/LineBreakRewriterTests.cs`: `PlacesSwitchExpressionBracesOnOwnLines`, `PlacesSwitchSectionBlockBraceOnOwnLine`; `Regression/Indentation/BracePlacementTests.cs`: `SwitchStatementBracesAreMovedToNewLines` |
| FMT-BREAK-009 | Holds for the four kinds. Not covered and kept: the `;` of an abstract, interface, or partial method, and of an expression-bodied property (RH5113-001 exempts the latter) | None found |
| FMT-BREAK-010 | Holds. Also after a wrapped parameter list (`C(int a,⏎  int b) : base(a)` → `: base(a)` on its own line). Colon at line end: thoenissen/Reihitsu#955 | `Unit/LineBreaks/LineBreakRewriterTests.cs`: `EnsuresConstructorInitializerOnNewLine`; `Integration/LineBreakPhaseIntegrationTests.cs`: `PlacesConstructorInitializerOnNewLine` |
| FMT-BREAK-011 | Holds for parenthesized lists (conversion operator, finalizer, local function, lambda, anonymous method, record, class primary constructor, extension block probed). Wrong for indexer lists (LB-Q5) | `Unit/LineBreaks/LineBreakRewriterTests.cs`: `CollapsesOpeningParenthesisToDeclarationLine`, `CollapsesConversionOperatorParameterListOpener`, `CollapsesSourceDerivedParameterListOpeningGapsAndIsIdempotent`, `PreservesProtectedSourceDerivedParameterListOpeningGaps`, `CollapsesParameterListClosingGapsForEveryDirectParentAndIsIdempotent`, `CollapsesMisplacedParameterCommaButKeepsListMultiline`; `Regression/LineBreaks/SeparatedListCommaBlockCommentSpacingTests.cs`: `LeadingCommaFollowedByBlockCommentKeepsSpaceWhenMovedToPreviousParameterLine`, `LeadingCommaFollowedByWhitespaceOnlyDropsWhitespaceWhenMovedToPreviousParameterLine` |
| FMT-BREAK-012 | Holds, with known gaps. A wrap only after `(` is joined (`M(⏎1, 2)` → `M(1, 2)`); the decided clarification is thoenissen/Reihitsu#1131 (from #1111 Q1). Indexer lists: LB-Q5. Attribute argument lists keep a leading comma and a `)` on its own line (further instance of #956, below) | `Unit/LineBreaks/LineBreakRewriterTests.cs`: `PreservesSingleLineArgumentList`, `PreservesArgumentsAlreadyOnSeparateLines`, `SplitsMixedLineArguments`, `SplitsMixedLineArgumentsUsingConfiguredLineFeed`, `CollapsesFirstArgumentToSameLine`, `PreservesSingleLineParameterList`, `SplitsMixedLineParameters`; `Regression/FullPipeline/ArgumentFormattingFullPipelineTests.cs`: `FormatsSimpleMethodCallArguments`, `FormatsNestedCallArguments`, `FormatsConstructorCallArguments`, `FormatsMultiLineArguments`; attribute argument list: `Regression/LineBreaks/SeparatedListCommaBlockCommentSpacingTests.cs`: `CommaFollowedByBlockCommentKeepsSpaceWhenAttributeArgumentListIsSplit` |
| FMT-BREAK-013 | Holds (raw string, verbatim string, implicit array, switch expression, block lambda as last argument probed) | `Unit/LineBreaks/LineBreakRewriterTests.cs`: `SplitsOuterArgumentsWhen*` (4); `Regression/FullPipeline/ArgumentFormattingFullPipelineTests.cs`: `FormatsArgumentsWhenNestedAnonymousObjectBecomesMultiLine`, `FormatsOuterArgumentsWhenNestedInvocationBecomesMultiLine`, `FormatsArgumentsWhenLambdaContainsMultiMemberAnonymousObject`, `FormatsArgumentsWhenLambdaContainsMultiMemberObjectInitializer`, `FormatsArgumentsWhenLambdaBodyInvocationBecomesMultiLine`, `FormatsStatementLambdaAsSecondArgumentAndSplitsOuterArguments` |
| FMT-BREAK-014 | Holds | `Regression/LineBreaks/WrappedAngleBracketListTests.cs` (all 27); `Regression/LineBreaks/BracketedArgumentTrailingCommentTests.cs` (all 6); `Regression/LineBreaks/DocumentationCommentCollapseGuardTests.cs`: `DocumentedBracketedArgumentListIsNotCollapsedToSingleLine`, `OrdinaryCommentedBracketedArgumentListIsNotCollapsedToSingleLine`; `Regression/LineBreaks/ListElementAttributeLayoutSinglePassTests.cs`: `Indexer*`, `NestedIndexers*` |
| FMT-BREAK-015 | Holds. The text names `is`-pattern operator wraps as owned by FMT-BREAK-016 to -020, but none of those requirements handles an `is` pattern: `Foo(o is⏎string s)` is kept as written | `Regression/LineBreaks/ListElementInteriorJoinTests.cs` (all 52) |
| FMT-BREAK-016 | Holds for `=`. Compound assignments are kept (LB-Q9). Enum members: decided change thoenissen/Reihitsu#1149. Parameter defaults and named attribute arguments are joined; an anonymous-object member `A⏎= 1` is kept | `Unit/LineBreaks/LineBreakRewriterTests.cs`: `CollapsesVariableValueToEqualsLine`, `CollapsesVariableEqualsToTargetLine`, `CollapsesVariableEqualsAndValueToTargetLine`, `CollapsesAssignmentValueToEqualsLine`, `CollapsesPropertyAssignmentEqualsToTargetLine`, `CollapsesFieldValueToEqualsLine`, `CollapsesMemberInitializationValueToEqualsLine`, `CollapsesIndexAssignmentEqualsToTargetLine`, `PreservesCorrectSingleLineAssignment`, `PreservesMultilineValueStartingOnEqualsLine`, `CollapsesRawMultilineStringAssignmentToEqualsLine`, `CollapsesInterpolatedRawMultilineStringAssignmentToEqualsLine`, `PreservesRawMultilineStringStartingOnEqualsLine`; `Regression/FullPipeline/AssignmentLineJoinSpacingFullPipelineTests.cs`: `JoinedAssignmentOperatorDoesNotDoubleSpace`; `Regression/LineBreaks/AutoPropertyTrailingCommentTests.cs`: `OwnLineInitializerWithoutTriviaIsJoined`; `Regression/Indentation/CollectionExpressionAlignmentTests.cs`: `CollectionExpressionStartMovesToAssignmentOperatorLine*` (2) |
| FMT-BREAK-017 | Holds (`is`/`as` with a type, `??`, combinator before and after a multi-line pattern probed). A combinator at the end of a line is kept (author's layout) | `Unit/LineBreaks/LineBreakRewriterTests.cs`: `NormalizesBinaryOperatorPosition`; `Integration/LineBreakPhaseIntegrationTests.cs`: `MovesBinaryOperatorToNextLine`; `Regression/FullPipeline/LogicalExpressionAlignmentFullPipelineTests.cs`: `FormatsLogicalExpressionLayout`; `Regression/Indentation/RecursivePatternAlignmentTests.cs`: `AndOrChainRecursivePatternsPlaceEachCombinatorOnOwnLine`, `OrPatternRecursivePatternsAlignToIsKeyword`. "Never wraps a one-line expression": none found |
| FMT-BREAK-018 | Wrong for a parenthesized nested conditional and for one inside a lambda (LB-Q6). `(a ? b : c) ? 1 : 2` (nested in the condition) stays on one line, as written | `Regression/FullPipeline/ConditionalExpressionFullPipelineTests.cs` (all 17); `Regression/Indentation/IndentationTests.cs`: `ConditionalExpressionMovesQuestionTokenToNextLine`, `NestedTernaryContinuationRemainsAligned`, `NestedTernaryWithMalformattedCondition`; `Regression/LineBreaks/LeadingTriviaMultiLineDetectionTests.cs`: `TernaryWithLeadingCommentLineStaysSingleLine` |
| FMT-BREAK-019 | Holds (spot checks; the behavior is SHARED-CHAIN's) | `Regression/LineBreaks/ChainLinkLayoutTests.cs`, `ConditionalAccessWrapBetweenOperatorTokensTests.cs`, `ChainInsideConditionalAccessArgumentTests.cs`; `Regression/FullPipeline/MethodChainAlignmentFullPipelineTests.cs` (3); `Unit/LineBreaks/LineBreakRewriterTests.cs`: `BreaksLongMethodChain`, `HandlesConditionalAccessChain` |
| FMT-BREAK-020 | Holds. Without a chain link the break before `(` or `[` is kept (LB-Q10; asserted by `ChainLinkLayoutTests`: `ElementAccessWithoutMemberAccessKeepsItsBreak`, `ArgumentListWithoutMemberAccessKeepsItsBreak`) | `Regression/LineBreaks/ChainSplitMemberNameRejoinTests.cs` (all 13); `Regression/LineBreaks/ChainLinkLayoutTests.cs`: `ArgumentListJoinsItsElement`, `ElementAccessJoinsItsElement`, `ConditionalElementAccessJoinsItsElement`, `ArgumentListOfRootJoinsInMemberChain`, `MemberNameSplitAfterConditionalAccessIsRejoinedAndIsNoWrap`; `Regression/FullPipeline/ConditionalExpressionFullPipelineTests.cs`: `SplitMemberAccessIsRejoined` |
| FMT-BREAK-021 | Holds. Also expands an empty initializer (`new Foo { }` and `new { }` become two-line braces) and a `with` initializer (`r with { A = 2 }`, not named in the spec). `stackalloc int[] { 1, 2 }` keeps its layout like other array initializers. The nested `L = { 1, 2 }` wording is the decided clarification thoenissen/Reihitsu#1291 | `Regression/Indentation/BracePlacementTests.cs`: `ObjectInitializerBracesAreMovedToNewLines`, `AnonymousObjectInitializerBracesAreMovedToNewLines`, `CollectionInitializerInMethodChainBracesAreMovedToNewLines`; `Regression/Indentation/ObjectInitializerAlignmentTests.cs`: `EmptyInitializerBracesAlignToNewKeyword`, `CollectionInitializerInsideObjectInitializerIndentsCorrectly`; `Regression/FullPipeline/ObjectInitializerAlignmentFullPipelineTests.cs`: `FormatsObjectInitializerLayout`. Array initializer kept as written: none found |
| FMT-BREAK-022 | Holds for collection expressions, list patterns, property patterns (switch-expression arm: brace on its own line; `case`: brace on the label line), and complex element initializers. Parenthesized patterns: LB-Q7. A positional pattern clause `o is (1,⏎2)` is not in the list and is kept | `Regression/Indentation/RecursivePatternAlignmentTests.cs`: `CasePatternRecursivePatternKeepsBraceOnCaseLineAndBracesBody`, `ParenthesizedRecursivePatternBreaksParenthesisOntoOwnLine`, `SingleLineRecursivePatternRemainsUnchanged`, `SwitchExpressionArmRecursivePatternAligns`; `Regression/Indentation/CollectionExpressionAlignmentTests.cs` (several); `Regression/LineBreaks/ComplexElementInitializerLineBreakTests.cs` (2); `Regression/LineBreaks/LineBreaksConsolidationStabilityTests.cs`: `RecursivePatternDesignationStaysOnCloseBraceLine`, `ListPatternDesignationStaysOnCloseBracketLine`; `Idempotency/IdempotencyTests.cs`: `RecursivePatternLayoutIsIdempotent`, `ComplexElementInitializerLayoutIsIdempotent` |
| FMT-BREAK-023 | Holds (type, method, local function, delegate, record, struct, bodiless partial method probed). Wrap inside a clause: thoenissen/Reihitsu#955 | `Unit/LineBreaks/LineBreakRewriterTests.cs`: `EnsuresGenericConstraintsOnNewLines`. Delegate, local function, member without a body: none found |
| FMT-BREAK-024 | Wrong for a wrapped signature and for a comment or directive between signature and list (LB-Q3) | `Unit/LineBreaks/LineBreakRewriterTests.cs`: `CollapsesGetOnlyAutoPropertyToSingleLine`, `CollapsesGetSetAutoPropertyToSingleLine`, `KeepsAutoPropertyAccessorListInlineWhenInitializerWraps`, `CollapsesPropertyAttributedAutoPropertyToSingleLine`, `LeavesAccessorAttributedAutoPropertyAsWritten`, `KeepsSingleLineAccessorAttributedAutoPropertyOnOneLine`, `CollapsesAutoPropertyWithJoinableWrappedSignature`; `Regression/LineBreaks/AutoAccessorListLayoutTests.cs`: `SingleLine*RemainsUnchanged` (6), `MultiLineAutoIndexerCollapses`, `AutoIndexerWithClosingBraceOnAccessorLineCollapses`, `MultiLineAbstractAutoIndexerWithAccessorModifierCollapses`; `Regression/LineBreaks/AutoPropertyTrailingCommentTests.cs`: `*Collapses*`, `*DoesNotSplitTheProperty`; `Regression/LineBreaks/AutoPropertyAccessorListCrlfTests.cs`; `Regression/LineBreaks/AccessorListDeclarationBraceTests.cs`: `MultiLineAutoAccessorIndexerCollapses`, `SingleLineAutoAccessorIndexerRemainsUnchanged`; `Regression/Indentation/BracePlacementTests.cs`: `AutoPropertyBracesRemainOnSingleLine` |
| FMT-BREAK-025 | Holds, except an accessor after a multi-line comment that ends mid-line (LB-Q2) | `Regression/LineBreaks/AccessorListAccessorLineBreakTests.cs` (all 24); `Regression/LineBreaks/AccessorListDeclarationBraceTests.cs` (rest); `Regression/LineBreaks/AutoAccessorListLayoutTests.cs`: `*IsLaidOut`, `*StartOwnLines`; `Regression/LineBreaks/AutoPropertyTrailingCommentTests.cs`: `CommentInsideAccessorListStillSplitsTheProperty`, `DirectiveInsideAccessorListStillSplitsTheProperty`; `Unit/LineBreaks/LineBreakRewriterTests.cs`: `DoesNotCollapsePropertyWithAccessorBody`, `DoesNotCollapseAutoPropertyWithCommentTrivia`, `HandlesPropertyAccessors` |
| FMT-BREAK-026 | Holds for the line breaks. The phase also sets the arrow's spaces (LB-Q8) | `Unit/LineBreaks/LineBreakRewriterTests.cs`: `CollapsesExpressionBodiedProperty`; `Integration/LineBreakPhaseIntegrationTests.cs`: `HandlesExpressionBodiedPropertyAcrossLines`; `Regression/LineBreaks/CommentJoinTests.cs`: `ExpressionBodiedPropertyIsNotCollapsedIntoTrailingComment`; `Regression/LineBreaks/DirectiveJoinTests.cs`: `ExpressionBodiedPropertyIsNotCollapsedAcrossDirective`. Indexer: none found |
| FMT-BREAK-027 | Holds. Probed: assembly lists split into one list each, enum-member and local-function lists split, parameter lists merged (`[A] [B] int a` → `[A, B] int a`), a list followed by a comment keeps it (`[A] /* c */⏎[B]`). Known: `param:`/`typevar:` thoenissen/Reihitsu#1093; lambda attribute lists thoenissen/Reihitsu#1131. Multi-line comment ending mid-line: LB-Q2 | `Regression/LineBreaks/ParameterAttributeTargetLayoutTests.cs` (all 28); `Regression/LineBreaks/TypeParameterSplitAttributeListSinglePassTests.cs` (19); `Regression/LineBreaks/DocumentationCommentCollapseGuardTests.cs`: `Documented*Attribute*`, `DirectiveWrappedAttributeLists*`; `Regression/LineBreaks/ListElementAttributeLayoutSinglePassTests.cs`: `ArgumentLambda*`, `MinimalApiAttributedLambdaSettlesInOnePass`; `Regression/FullPipeline/OneLineClassBodyMembersFullPipelineTests.cs`: `PlacesAttributedMemberOnItsOwnLine` |
| FMT-BREAK-028 | Holds. Not covered and kept: a wrap after the `when` keyword (`case int j when⏎j < 0:`), and a `when` in a switch-expression arm (the spec says `case` label) | `Regression/LineBreaks/CaseWhenClauseLayoutTests.cs` (all 5) |

### formatter/switch-case-braces.md

| ID | Against the code | Existing tests (file: method) |
|---|---|---|
| FMT-SWITCH-001 | Holds, apart from the tracked FMT-PIPE-003 violation thoenissen/Reihitsu#948 | `Regression/Structural/SwitchCaseBracesTests.cs`: `TypePatternCaseWithMultipleStatementsAddsBracesCorrectly`, `StatementFollowedByReturnAddsBracesAndIsIdempotent`; unbraced case: `Regression/Indentation/RecursivePatternAlignmentTests.cs`: `SingleLineCasePatternRemainsInlineWithoutBodyBraces`; `Unit/SwitchCaseBraces/SwitchCaseBraceRewriterTests.cs`: `AddsBracesToMultiLineSwitchSection`, `DoesNotAddBracesToSingleLineSwitchSection`, `HandlesDefaultSection` (brace counts only) |
| FMT-SWITCH-002 | Holds (`continue`, `goto case`, `yield break` go inside the block) | `Regression/Structural/SwitchCaseBracesTests.cs`: `BreakStatementIsPlacedOutsideBracesBlock`, `ReturnStatementRemainsInsideBracesBlock`, `TypePatternCaseWithMultipleStatementsAddsBracesCorrectly` (`continue`); `Unit/SwitchCaseBraces/SwitchCaseBraceRewriterTests.cs`: `AddsBracesKeepsBreakOutsideBlock`, `AddsBracesKeepsReturnInsideBlock` |
| FMT-SWITCH-003 | Holds. `A(); goto case 2;` and `A(); continue;` count as two statements, like the spec's wording | `Regression/Structural/SwitchCaseBracesTests.cs`: `StatementFollowedByReturnAddsBracesAndIsIdempotent`, `StatementFollowedByThrowAddsBracesAndIsIdempotent`; `Unit/SwitchCaseBraces/SwitchCaseBraceRewriterTests.cs`: `AddsBracesWhenSingleNonTerminalStatementIsFollowedByReturn`, `DoesNotAddBracesToSingleMultiLineReturnStatement`. One statement spanning two lines: none found |
| FMT-SWITCH-004 | Holds, including a property pattern inside a `when` clause (`case 1 when o is string { … }` braces the switch). A wrapped `or` chain does not count. Conflicts with FMT-SWITCH-006 for a `break`-only section (SW-Q1) | `Regression/Indentation/RecursivePatternAlignmentTests.cs`: `CasePatternRecursivePatternKeepsBraceOnCaseLineAndBracesBody`. Wrapped `when` and `or` left alone: none found |
| FMT-SWITCH-005 | Holds. Because FMT-BREAK-008 always expands a switch expression, any switch expression in a section's statements braces the switch (`return x switch { 1 => 1, _ => 0 };` written on one line too). A multi-line switch expression in a `when` clause does not count | `Regression/Structural/SwitchCaseBracesTests.cs`: `CaseContainingMultiLineSwitchExpressionAddsBraces` |
| FMT-SWITCH-006 | Wrong for a `break`-only section with a multi-line pattern label (SW-Q1) | `Regression/Structural/SwitchCaseBracesTests.cs`: `TypePatternCaseWithMultipleStatementsAddsBracesCorrectly` (stacked labels), `EmptyCasedShouldNotBeChanged` (`break`-only section stays bare) |
| FMT-SWITCH-007 | Holds for the three named positions. The trivia around them is rebuilt (SW-Q2). Comment before the terminal `break`: thoenissen/Reihitsu#949 | `Regression/Structural/SwitchCaseBracesTests.cs`: `LabelAndLeadingCommentsArePreservedWhenAddingBraces`, `TrailingStatementCommentIsPreservedWhenAddingBraces`; `Unit/SwitchCaseBraces/SwitchCaseBraceRewriterTests.cs`: `AddBracesPreserve*` (3, `Contains` only) |
| FMT-SWITCH-008 | Holds. The removal path is dead code (decided thoenissen/Reihitsu#950) | `Regression/Structural/SwitchCaseBracesTests.cs`: `EmptyCasedShouldNotBeChanged`; `Regression/Indentation/IndentationTests.cs`: `SwitchCaseBlockBracesRemainUnchanged`, `SwitchOptionParsingCaseBlocksRemainUnchanged`; `Unit/SwitchCaseBraces/SwitchCaseBraceRewriterTests.cs`: `PreservesExistingBraces`, `RemovesBracesFromSingleLineSection` (asserts the braces are kept). An existing block bracing the other sections: none found |
| FMT-SWITCH-009 | Holds | `Regression/BlankLines/BracedSwitchSectionBlankLineTests.cs`: `BreakInsideExplicitCaseBlockIsLeftToRH7501`; `Unit/SwitchCaseBraces/SwitchCaseBraceRewriterTests.cs`: `PreservesExistingBraces`. Statements after the block left in place: none found |
| FMT-SWITCH-010 | Holds; `#region` and `#pragma` in section-level trivia block the section too | `Regression/Structural/DirectivePreservationTests.cs`: `SwitchCaseBraceInsertionKeepsDirectivesInSection` (one section only). Other sections still braced: none found |
| FMT-SWITCH-011 | Holds | `Unit/SwitchCaseBraces/SwitchCaseBraceRewriterTests.cs`: `DoesNotAddBracesWhenGotoTargetsLabelInAnotherSection`, `DoesNotAddBracesWhenNestedSwitchGotoTargetsLabelInAnotherSection` (phase level, exact). `goto case`/`goto default` not blocking: none found |
| FMT-SWITCH-012 | Holds | `Unit/SwitchCaseBraces/SwitchCaseBraceRewriterTests.cs`: `AddBracesIgnoresGotoTargetsInsideLocalFunctions`, `AddBracesIgnoresGotoTargetsInsideLambdas` (structural asserts) |
| FMT-SWITCH-013 | Holds | None found |

### formatter/horizontal-spacing.md

| ID | Against the code | Existing tests (file: method) |
|---|---|---|
| FMT-SPACE-001 | Wrong for tokens separated only by a multi-line `/* */` comment (SP-Q1). Directive interiors (`#if  A&&B`) are not visited and stay | `Regression/Spacing/HorizontalSpacingTests.cs`: `MultiLineExpressionNoSpacingChange`, `BlockCommentBeforeGenericBracketOnContinuationLineKeepsGapUntouched`. Trailing `//` column kept: none found |
| FMT-SPACE-002 | Holds; tab runs collapse | `Regression/Spacing/TabGapSpacingTests.cs` (all 6); `Regression/Spacing/HorizontalSpacingTests.cs`: `MultipleSpacesNormalizedToSingle`; `Unit/HorizontalSpacing/HorizontalSpacingRewriterTests.cs`: `RemovesDoubleSpaces`; `Integration/HorizontalSpacingPhaseIntegrationTests.cs`: `CollapsesMultipleSpaces`; `Unit/HorizontalSpacing/SpacingPolicyTests.cs`: `NoRuleAppliesBetweenTypeAndIdentifier`, `NoSpaceRuleTakesPrecedenceOverKeywordForImplicitArray` (rule order). Cast space kept: none found |
| FMT-SPACE-003 | Holds | `Unit/HorizontalSpacing/SpacingPolicyTests.cs`: `AttributeListCloseBracketRequiresSingleSpace` (policy only). Through the phase or pipeline: none found |
| FMT-SPACE-004 | Holds | `Regression/Spacing/HorizontalSpacingTests.cs`: `OpenParenRemovesSpaceAfter`, `CloseParenRemovesSpaceBefore`, `SemicolonOutsideForNoChange`; `Unit/HorizontalSpacing/HorizontalSpacingRewriterTests.cs`: `RemovesSpaceInsideParentheses`, `RemovesSpaceBeforeComma`, `RemovesSpaceBeforeSemicolon`; `Integration/HorizontalSpacingPhaseIntegrationTests.cs`: `RemovesSpacesInsideParentheses`; `Unit/HorizontalSpacing/SpacingPolicyTests.cs`: `OpenParenthesisRequiresNoSpace`. Brackets: none found |
| FMT-SPACE-005 | Holds. A function-pointer `<`/`>` is not a type argument list and is only collapsed (SP-Q2) | `Unit/HorizontalSpacing/HorizontalSpacingRewriterTests.cs`: `HandlesGenericTypeArguments`; `Regression/Spacing/MemberAccessSymbolSpacingTests.cs`: `NullableTypeQuestionMarkStaysAttached`; `Regression/Spacing/HorizontalSpacingTests.cs`: `BlockCommentBeforeGenericBracketOnSameLineIsGluedToBracket` |
| FMT-SPACE-006 | Holds. Explicit-interface dot: decided change thoenissen/Reihitsu#958 | `Regression/Spacing/MemberAccessSymbolSpacingTests.cs` (all 10) |
| FMT-SPACE-007 | Holds (`^ 1`, `* p`, `& a`, `~ a`, `- - -a`, `a -  - a` probed) | `Regression/Spacing/HorizontalSpacingTests.cs`: `NestedUnaryMinusKeepsSpace`, `NestedUnaryPlusKeepsSpace`, `NestedAddressOfKeepsSpace`; `Unit/HorizontalSpacing/HorizontalSpacingRewriterTests.cs`: `HandlesUnaryOperators` (input already tight); `src/Reihitsu.Core.Test/UnaryOperatorSpacingUtilitiesTests.cs` (Core). Tightening `- a`, `! b`, `o !`: none found |
| FMT-SPACE-008 | Holds | `Regression/Spacing/InterpolationAlignmentSpacingReproTests.cs` (all 19); `Regression/Spacing/HorizontalSpacingTests.cs`: `CommaTokenEnsuresSpaceAfter`, `MultidimensionalArrayCreationEnsuresSpaceAfterComma`; `Unit/HorizontalSpacing/HorizontalSpacingRewriterTests.cs`: `AddsSpaceAfterComma`, `HandlesCommaSpacingInMultidimensionalArrays`, `KeepsUnboundGenericCommasCompact`; `Unit/HorizontalSpacing/SpacingPolicyTests.cs`: `CommaRequiresSingleSpace`, `RankOnlyArrayCommaRequiresNoSpace`; `Integration/HorizontalSpacingPhaseIntegrationTests.cs`: `InsertsSpaceAfterComma`; `src/Reihitsu.Core.Test/CommaSpacingUtilitiesTests.cs` (Core) |
| FMT-SPACE-009 | Holds. `{1, 2 }` (space before `}` only) is the decided change thoenissen/Reihitsu#958 | Interpolation braces: `Unit/HorizontalSpacing/HorizontalSpacingRewriterTests.cs`: `HandlesStringInterpolation`. Space before a brace, base-list colon, constructor-initializer colon: none found |
| FMT-SPACE-010 | Holds. Enum-member `=` (an `EqualsValueClause`) is spaced although not named; the `=` of a query `let` clause is not (SP-Q2) | `Regression/Spacing/HorizontalSpacingTests.cs`: `BinaryOperatorPlusEnsuresSingleSpace`, `BinaryOperatorMinusEnsuresSingleSpace`, `AssignmentOperatorEnsuresSingleSpace`, `CompoundAssignmentEnsuresSingleSpace`, `EqualsValueClauseEnsuresSingleSpace`, `AnonymousType*EnsuresSpacing` (4, anonymous-object name-equals); `Regression/Spacing/TabGapSpacingTests.cs`: `TabBeforeBinaryOperatorIsNormalizedToSingleSpace`; `Unit/HorizontalSpacing/HorizontalSpacingRewriterTests.cs`: `AddsSpaceAroundBinaryOperator`, `AddsSpaceAroundAssignment`; `Unit/HorizontalSpacing/SpacingPolicyTests.cs`: `BinaryOperatorRequiresSingleSpace`; `Integration/HorizontalSpacingPhaseIntegrationTests.cs`: `InsertsSpacesAroundBinaryOperators`. Using alias, named attribute argument: none found |
| FMT-SPACE-011 | Holds | `Regression/Spacing/HorizontalSpacingTests.cs`: `SemicolonInForEnsuresSpaceAfter`; `Unit/HorizontalSpacing/HorizontalSpacingRewriterTests.cs`: `AddsSpaceAfterSemicolonInForLoop`; `Unit/HorizontalSpacing/SpacingPolicyTests.cs`: `ForLoopSemicolonRequiresSingleSpace`. `for (;;)`: none found |
| FMT-SPACE-012 | Holds. `fixed(`: decided bug thoenissen/Reihitsu#957. `when(` of a catch filter is not in the list (SP-Q2) | `Regression/Spacing/HorizontalSpacingTests.cs`: `IfKeywordEnsuresSpaceAfter`, `ForKeywordEnsuresSpaceAfter`, `ForEachKeywordEnsuresSpaceAfter`, `WhileKeywordEnsuresSpaceAfter`, `ReturnKeywordEnsuresSpaceAfter`, `ReturnKeywordBeforeSemicolonNoSpace`, `ThrowKeywordBeforeSemicolonNoSpace`, `NewKeywordDoesNotAddSpaceBeforeParenInTargetTypedNew`, `NewKeywordEnsuresSpaceBeforeTypeName`, `NewConstraintSpaceRemoved`; `Unit/HorizontalSpacing/HorizontalSpacingRewriterTests.cs`: `AddsSpaceAfterKeyword`, `RemovesSpaceInNewConstraint`; `Unit/HorizontalSpacing/SpacingPolicyTests.cs`: `SpacedKeywordRequiresSingleSpace`, `ReturnBeforeSemicolonRequiresNoSpace`, `NoSpaceRuleTakesPrecedenceOverKeywordForImplicitArray`; `Integration/HorizontalSpacingPhaseIntegrationTests.cs`: `InsertsSpaceAfterKeyword`. `switch`, `catch`, `using`, `lock`, `operator`, `[return: A]`: none found |
| FMT-SPACE-013 | Holds | `Regression/Spacing/InterpolationAlignmentSpacingReproTests.cs`: `CommentBeforeAlignmentCommaStaysUnchanged`, `SpaceAfterCommentFollowingAlignmentCommaIsRemoved`; `Regression/Spacing/HorizontalSpacingTests.cs`: `BlockCommentBeforeGenericBracketOnSameLineIsGluedToBracket`; `Unit/HorizontalSpacing/TrailingWhitespaceWriterTests.cs`: `SetTrailingWhitespacePreservesInlineComment`; `Regression/LineBreaks/SeparatedListCommaBlockCommentSpacingTests.cs` (comma plus block comment after a list split) |
| FMT-SPACE-014 | Holds as written. P6 says a `/** */` that documents nothing behaves like `/* */`, and the spec's own premise conflicts with it (see *Decided by principle*). Double space in front: decided bug thoenissen/Reihitsu#957 | `Unit/HorizontalSpacing/HorizontalSpacingRewriterTests.cs`: `RemovesGapBetweenDelimitedDocumentationCommentAndFollowingToken`; `Regression/Spacing/StrandedDocumentationTokenSpacingTests.cs`: `RemovesGapBetweenDelimitedDocumentationCommentAndTerminatorOnSameLine` |
| FMT-SPACE-015 | Holds | `Unit/HorizontalSpacing/HorizontalSpacingRewriterTests.cs`: `KeepsIndentationOfTokenFollowingSingleLineDocumentationComment`; `Regression/Spacing/StrandedDocumentationTokenSpacingTests.cs`: `Keeps*` (13) |

## Open questions to file

No open-questions issue for these three specs is open (#918, #1031, #1111, #919, #920, #1051 are closed), so each group below is a new issue. Notation: `⏎` line break. The phase lists come from `scripts/trace.sh` or the probe outputs, and every output below is idempotent.

### `SDD open questions: sdd/specs/formatter/line-breaks.md`

**LB-Q1: The braces of a C# 14 extension block are not placed on their own lines.** Touches FMT-BREAK-002, FMT-BREAK-007.

Input:

```csharp
static class E
{
    extension(int x) { public int A => x; public int B => x; }
}
```

Real output:

```csharp
static class E
{
    extension(int x) {
        public int A => x;
        public int B => x; }
}
```

Why it looks wrong: the members are split (FMT-BREAK-007), but the `{` stays on the header line and the `}` is glued behind the last member, against the Allman rule of FMT-BREAK-002. `extension(int x) {` with members already on their own lines also keeps the brace. `DeclarationBraceLineBreakRewriter.Visit` has no extension-block case, while `MemberLineStartRewriter` and `LineBreakListRewriter` handle extension blocks. FMT-BREAK-002 lists "type" but not extension blocks. No analyzer reports the output (RH5402, RH5404, RH5417: no-diagnostic).

**LB-Q2: A multi-line block comment that ends mid-line keeps the next statement, member, accessor, or declaration on its line.** Touches FMT-BREAK-006, FMT-BREAK-007, FMT-BREAK-025, FMT-BREAK-027.

Input:

```csharp
class C
{
    int a; /* first
    second */ int b;

    void M()
    {
        Foo(); /* first
        second */ Bar();
    }

    [System.Obsolete] /* a
    b */ void N()
    {
    }
}
```

Real output: unchanged. `get => _x; /* first⏎second */ set => _x = value;` is also unchanged. Tests assert this: `Regression/FullPipeline/OneLineClassBodyMembersFullPipelineTests.cs`: `KeepsMemberAfterMultiLineCommentEndingMidLine`, and `Regression/LineBreaks/AccessorListAccessorLineBreakTests.cs`: `AccessorAfterMultiLineCommentEndingMidLineRemainsUnchanged`.

Why it is a question: each requirement says the element MUST start its own line, with no exception. Breaking in front of `int b` would keep the comment at the end of the previous line, which FMT-BREAK-001's second sentence allows. Behind an opening brace the phase does break (`{ /* p⏎q */ Foo(); }` → `{ /* p⏎q */⏎Foo();`). Reading A: intended, and the four requirements gain the exception. Reading B: break in front of the element as everywhere else.

**LB-Q3: An auto-property accessor list stays multi-line, or is expanded, when the signature spans lines or a comment or directive sits between the signature and `{`.** Touches FMT-BREAK-024, FMT-BREAK-025.

Input:

```csharp
class C
{
    int Q /* q */ { get; set; }

    public
        int B { get; }
}
```

Real output:

```csharp
class C
{
    int Q /* q */
    {
        get;
        set;
    }

    public
    int B
    {
        get;
    }
}
```

Tests assert the comment case: `Regression/LineBreaks/AutoPropertyTrailingCommentTests.cs`: `CommentInSignatureGapIsNotCrossed`, `Regression/LineBreaks/AutoAccessorListLayoutTests.cs`: `AutoIndexerWithSignatureCommentIsLaidOut`, `Unit/LineBreaks/LineBreakRewriterTests.cs`: `DoesNotCollapseAutoPropertyWithRefusedJoinSignature`. A signature that another rule joins (`Dictionary<string,⏎string> Prop`) collapses: `CollapsesAutoPropertyWithJoinableWrappedSignature`.

Why it is a question: FMT-BREAK-024 says such an accessor list MUST be on one line with the declaration. That excludes only comments and directives *inside* the list. `int Q /* q */ { get; set; }` is already on one line and is expanded to four. RH5408-002/-003 exempt both shapes, so the analyzer agrees with either reading. The `int B` column is an indentation matter (thoenissen/Reihitsu#953). Reading A: intended, so FMT-BREAK-024 gains the qualifier and FMT-BREAK-025 names these lists. Reading B: a list that cannot be collapsed is left as written, and one on a single line stays there.

**LB-Q4: The predicate of FMT-BREAK-005 differs from the text in three ways.** Touches FMT-BREAK-005, FMT-BREAK-001, and the non-goal for unbraced embedded statements.

Input:

```csharp
class C
{
    void M(bool a, int[] xs)
    {
        if (a) /* c */ {
            Foo();
        }

        if (a) /* x */ // y
        {
            Foo();
        }

        while (a) if (a) // w
            {
                Foo();
            }

        lbl: if (a) // l
        {
            Foo();
        }
    }
}
```

Real output (relevant lines):

```csharp
        /* c */
        if (a)
        {
            Foo();
        }

        if (a) /* x */ // y
        {
            Foo();
        }

        while (a) // w
            if (a)
            {
                Foo();
            }

        lbl: // l
        if (a)
```

`foreach (var x in xs) if (a) // f` behaves like the `while` case.

Why it looks wrong:

- (a) The text moves only a comment "with nothing but whitespace and line breaks behind it", yet `/* c */` with `{` behind it on the same line moves.
- (b) `// y` has nothing behind it, but neither comment moves. `MoveTrailingConditionCommentToOwnLine` takes the first comment in the trivia only.
- (c) When the `if` does not start its line, its leading trivia lies behind the enclosing `while (…)`, `foreach (…)`, or label. The comment then ends up trailing a different construct, and a line break is inserted into an unbraced embedded statement, which the non-goal says keeps the author's line breaks. FMT-BREAK-001 gives the reason this matters: moving a comment behind another token changes what the comment is about.

The maintainer decides each of (a), (b), and (c).

**LB-Q5: Indexer parameter lists `[…]` are not laid out.** Touches FMT-BREAK-011, FMT-BREAK-012.

Input:

```csharp
class C
{
    int this[int a
             , int b] => a;

    int this[string a, string b,
             string c] => 0;
}
```

Real output: unchanged. `LineBreakListRewriter` visits `ParameterListSyntax` but not `BracketedParameterListSyntax`.

Why it is a question: FMT-BREAK-011 says "This holds for every parameter list", and FMT-BREAK-012 covers "a parameter list". The leading comma and the partly wrapped list are the shapes both requirements correct elsewhere. RH5106/RH5107 cover parenthesized lists only, but RH5035 treats the `[ ]` of an indexer as a parameter list. Reading A: the requirements mean parenthesized lists and say so. Reading B: indexer lists are laid out like the others.

**LB-Q6: "contains a nested conditional in a branch" is narrower in the code.** Touches FMT-BREAK-018. The wording planned by thoenissen/Reihitsu#1291 ("contains a nested one") depends on the answer.

Input:

```csharp
class C
{
    int M(bool a, bool b)
    {
        var x = a ? (b ? 1 : 2) : 3;
        System.Func<int> f = a ? () => b ? 1 : 2 : null;

        return a ? 1 : b ? 2 : 3;
    }
}
```

Real output: the first two stay on one line. Only the `return` is spread (`a⏎? 1⏎: b⏎? 2⏎: 3`). `TernaryLineBreakRewriter` counts a branch only when it *is* a `ConditionalExpressionSyntax`, so parentheses or a lambda around it hide it.

Reading A: "nested" means a branch that is itself a conditional, and the text is narrowed. Reading B: any conditional inside a branch, parenthesized or not, spreads the outer one.

**LB-Q7: The `(` of a wrapped parenthesized pattern is moved onto its own line, also directly after `case`.** Touches FMT-BREAK-022, FMT-SWITCH-004.

Input:

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case (1 or
                  2):
                A();
                break;

            case 3:
                B();
                break;
        }
    }
}
```

Real output (switch section only):

```csharp
            case
            (
            1 or
            2
            ):
                {
                    A();
                }
                break;
```

`return x is not (1 or⏎2);` becomes `return x is not⏎(⏎1 or⏎2⏎);`, with `(` and `);` at column 17 and `1 or` and `2` at column 8. In a switch-expression arm, `(1 or⏎2) => 1` also puts `(` on its own line.

Why it looks wrong: FMT-BREAK-022 says only that the opening *brace of a property pattern* starts its own line, and it exempts a pattern introduced directly by a `case` label. `LineBreakInitializerRewriter.VisitParenthesizedPattern` moves the `(` unconditionally. `case⏎(` separates the label keyword from its pattern. The `(` on its own line inside an `is` pattern is asserted by `Regression/Indentation/RecursivePatternAlignmentTests.cs`: `ParenthesizedRecursivePatternBreaksParenthesisOntoOwnLine` and shown in FMT-INDENT-026's example, so that part looks intended. The open points are the missing FMT-BREAK text and the missing `case` exception. The pattern content at the statement column is the indentation matter thoenissen/Reihitsu#953 lists.

**LB-Q8: The line-break phase sets the spaces around `=>` of expression-bodied properties and indexers, but no other arrows.** Touches FMT-BREAK-026, the scope sentence of `line-breaks.md` ("The phase only adds and removes line breaks"), and the FMT-SPACE non-goal.

Input:

```csharp
class C
{
    int _p;

    int P =>5;
    int Q=>  6;
    int this[int i]=>i;

    int R
    {
        get =>_p;
        set=>  _p = value;
    }
}
```

Real output: `int P => 5;`, `int Q => 6;`, `int this[int i] => i;`, while `get =>_p;` and `set=> _p = value;` are only collapsed. Lambda arrows and switch-expression-arm arrows are also only collapsed. `scripts/trace.sh` shows the property change in `LineBreakPhase`: `PropertyLayoutLineBreakRewriter.CollapseExpressionBody` replaces the arrow's trivia with one space on each side, also when the declaration is already on one line.

Why it is a question: no requirement states this spacing. The phase's own scope sentence excludes it, and FMT-SPACE treats the arrow as a pair without a rule (thoenissen/Reihitsu#958 adds a rule for lambda arrows only). Reading A: intended for properties and indexers, recorded in FMT-BREAK-026. Reading B: arrow spacing belongs to FMT-SPACE for every arrow kind.

**LB-Q9: Compound assignments keep their wraps.** Touches FMT-BREAK-016.

Input:

```csharp
class C
{
    void M(int? x)
    {
        x
            += 1;
        x ??=
            2;
    }
}
```

Real output: both wraps are kept. `+= 1;` and `2;` drop to the statement column, which is the #953 part.

Why it is a question: FMT-BREAK-016 speaks of "the `=` of an assignment". `+=` and `??=` are assignment operators, and the code joins only `=`. RH5111-001 checks simple assignments only, so there is no disagreement with the analyzer. Reading A: only `=` is meant, and the text says so. Reading B: every assignment operator stays on the target's line with its value.

**LB-Q10: An argument list or element access on the line after its target is kept unless it is part of a chain.** Touches FMT-BREAK-020 and the argument-list part of FMT-BREAK-011/-012.

Input:

```csharp
class B(int a, int b);

class D(int a) : B
    (a, 2);

class C
{
    void M()
    {
        Foo
            (a);
        var x = arr
            [0];
        var y = new Bar
            (a);
    }
}
```

Real output: every break is kept. `(a);`, `[0];`, and `(a);` drop to column 8, and the primary-constructor base arguments `(a, 2);` drop to column 0. This is asserted by `Regression/LineBreaks/ChainLinkLayoutTests.cs`: `ElementAccessWithoutMemberAccessKeepsItsBreak`, `ArgumentListWithoutMemberAccessKeepsItsBreak`.

Why it is a question: FMT-BREAK-020 joins an attached part only "in a chain with at least one link", so this is specified by omission. The output reads unformatted, especially the column-0 base arguments. #956 decided that argument lists are laid out like parameter lists, and FMT-BREAK-011 keeps a parameter list's `(` on the declaration line. That decision does not mention the opening parenthesis. Reading A: intended. Reading B: the `(` or `[` joins its target as for a chain; it is decided together with #956.

### `SDD open questions: sdd/specs/formatter/switch-case-braces.md`

**SW-Q1: A `break`-only section with a multi-line pattern label makes the switch braced.** Touches FMT-SWITCH-004, FMT-SWITCH-006.

Input:

```csharp
class C
{
    void M(object o)
    {
        switch (o)
        {
            case Point { X: 1,
                         Y: 2 }:
                break;

            case string s:
                A();
                break;
        }
    }
}
```

Real output (sections):

```csharp
            case Point {
                     X: 1,
                     Y: 2
                 }:
                break;

            case string s:
                {
                    A();
                }
                break;
```

Why it is a question: FMT-SWITCH-006 says a section whose only statements are `break`s "MUST NOT make the switch multi-line". FMT-SWITCH-004 says a multi-line pattern in a label makes the section multi-line. `IsMultiLineSection` checks the labels first and consults `IsEmptySection` only when building. Reading A: FMT-SWITCH-004 wins, as today, and FMT-SWITCH-006 gains the exception. Reading B: FMT-SWITCH-006 wins, and the switch above stays unbraced.

**SW-Q2: Brace insertion rebuilds the trivia it touches from comments only.** Touches FMT-SWITCH-007 (and FMT-PIPE-002, which allows normalizing whitespace around comments).

Input:

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1: /* a */ // b
                /* c */ A();
                B(); /* x */ // y
                break; /* z */ // w

            case 2:
                // first

                // second
                C();
                D();
                break;
        }
    }
}
```

Real output (sections):

```csharp
            case 1: /* a */// b
                {
                    /* c */
                    A();
                    B(); /* x */// y
                }
                break; /* z */// w

            case 2:
                {
                    // first
                    // second
                    C();
                    D();
                }
                break;
```

The same comments in an unbraced section or in a block keep their layout: `/* c */ A();` stays on one line, the blank line between `// first` and `// second` stays, and the space between two comments stays.

Why it looks wrong: three layout changes happen only because braces were inserted. (a) The space between two adjacent comments is dropped, which glues them (`/* a */// b`). (b) A comment that shared its line with the first statement moves to its own line. (c) A blank line between two leading comments is dropped. FMT-SWITCH-007's `Why:` says comments keep their position relative to the code they describe. Reading A: allowed by FMT-PIPE-002, and FMT-SWITCH-007 states it. Reading B: the rebuilt trivia keeps spaces, same-line placement, and blank lines.

### `SDD open questions: sdd/specs/formatter/horizontal-spacing.md`

**SP-Q1: Tokens separated only by a multi-line `/* */` comment get their gap normalized.** Touches FMT-SPACE-001, FMT-SPACE-013.

Input:

```csharp
class C
{
    void M(int a)
    {
        var x = a /* one
            two */    + 1;
        var y = a   /* three
            four */;
    }
}
```

Real output: `two */ + 1;` and `var y = a /* three`. The run in front of the comment is collapsed, and the gap after it gets the rule's decision.

Why it is a question: FMT-SPACE-001 says the phase changes "only the whitespace between two tokens that are on the same line" and leaves the whole gap alone when the trivia "contains a line break". Here the tokens are on different lines, but the only line break is inside the comment's text, which `AreSeparatedByEndOfLine` does not see. Reading A: intended, so a line break inside a comment does not count and the text says so. Reading B: such a gap is left unchanged.

**SP-Q2: More token pairs without a rule, beyond the list decided in #920 Q2 (thoenissen/Reihitsu#958).** Touches FMT-SPACE-002, the non-goal, FMT-SPACE-005, -009, -010, -012.

Input (one statement per pair):

```csharp
using System;
using System.Linq;

[assembly : CLSCompliant(true)]

unsafe class C<T>
    where T:class
{
    delegate* < int, void > _f;
    int [] _a;
    int * _p;

    [method : Obsolete]
    [return:Obsolete]
    int M(int x, int[] arr)
    {
        var a = x is >5;
        var b = arr[1 .. 2];
        int[] c = [..  arr];
        var d = default (int);
        var e = sizeof (int);
        var q = from v in arr
                let w=v
                select w;
        var g = x switch
        {
            1=>2,
            _ => 3
        };

        try
        {
        }
        catch (Exception ex) when(ex != null)
        {
        }

        return 0;
    }
}
```

Real output: every listed gap is kept apart from collapsing runs to one space (`[..  arr]` → `[.. arr]`). `[return : Obsolete]` would become `[return: Obsolete]`, but `[return:Obsolete]` keeps no space after the colon, and `[assembly : …]` and `[method : …]` keep their spaces.

Why it is a question: the spec states this through FMT-SPACE-002 and the non-goal "such as …", so it describes today's behavior. Several results read unformatted (`where T:class`, `x is >5`, `1=>2`, `let w=v`, `delegate* < int, void >`), and the attribute-target colon is spaced by target. #958 covers lambda arrows, conditionals, element-access `[` and invocation `(` (with `typeof`), `::`, named-argument colons, and the inside of initializer braces. None of the pairs above is in it. No analyzer reports them (RH6011/RH6012 type argument and parameter lists only, RH6021-002 other colons not reported, RH6023 no `let`, RH6024-002 no patterns), so there is no analyzer/formatter disagreement. Question: keep these collapse-only, or add them to #958?

## Decided by principle

- **Decided by principle P6 (thoenissen/Reihitsu#912)**: a `/** */` comment that documents nothing is an ordinary block comment. Follow-up: thoenissen/Reihitsu#1077. These are further instances, so the lead adds a one-line comment there, not a new question. They are bugs against SHARED-level P6 and conflict with the premise of FMT-SPACE-014 (and its interplay with FMT-SPACE-009, -010, -013):
  - `class C:/** x */B` → `class C :⏎⏎/** x */B` (moved onto its own line after a blank line). The `/* */` twin gives `class C :/* x */ B`.
  - `int _c = 1 /** w */+ 2;` is unchanged. The twin gives `1 /* y */ + 2`.
  - `int _d  /** v */;` keeps two spaces. The twin gives `_e /* u */;` (the double space is already #957).

## Further instances of existing follow-ups (one-line comments, not new questions)

- thoenissen/Reihitsu#948 (bracing must not change which declarations are in scope). The decision lists local declarations, pattern variables, and `out var`. A **local function** declared in one section and called from another breaks the same way:

  ```csharp
  switch (x)
  {
      case 1:
          int L() => 1;
          A();
          return L();

      case 2:
          return L();
  }
  ```

  The output braces both sections, so `L` is scoped to the first block and `return L();` in `case 2` no longer resolves. By the C# scope rule, a local function declared in a switch section is in scope in the whole switch block. This was not compiled here.
- thoenissen/Reihitsu#956 (argument lists laid out like parameter lists). Attribute argument lists show the same shapes and are not normalized: `[A(1⏎, 2)]` and `[A(1, 2⏎)]` are kept as written. The second contradicts FMT-BREAK-012's first sentence ("on one line or one element per line"). There is no analyzer rule for attribute argument lists yet (thoenissen/Reihitsu#1159). If the lead does not read #956's "argument lists" as including attribute argument lists, this becomes a line-breaks question instead.

Already tracked, no action: #955 (colon at line end, constraint wrap), #954, #956 (arms; comma and `)` of argument lists), #953 (continuation columns), #948, #949, #950, #957, #958, #1077, #1093, #1131 (FMT-BREAK-012 boundary wrap; FMT-BREAK-027 lambda lists), #1149, #1243 and #1273 (operator alone on its line), #1291, #1098.

## Inputs the code decides that the spec does not mention (no question)

Facts for the test implementer. These are consistent with the requirements' intent, but no requirement names them:

- FMT-BREAK-007: top-level statements count as compilation-unit members and are split.
- FMT-BREAK-009: method `;` (abstract, interface, partial) and expression-bodied property `;` on their own line are kept.
- FMT-BREAK-016: a parameter default (`int a⏎= 5`) and a named attribute argument (`N⏎= 1`) are joined. An anonymous-object member (`A⏎= 1`) is kept.
- FMT-BREAK-021: `with` initializers and empty initializers are expanded. `stackalloc int[] { … }` keeps its layout.
- FMT-BREAK-022: a positional pattern clause `(1,⏎2)` is kept. An empty wrapped collection expression `[⏎]` keeps `]` on its own line.
- FMT-BREAK-027: separate parameter attribute lists of the same target are merged into one.
- FMT-BREAK-028: the wrap after `when` (`when⏎j < 0`) is kept, and a switch-expression arm's `when` is not handled.
- FMT-SWITCH-003/-005: any switch expression in a section's statements braces the switch, because FMT-BREAK-008 always expands it. `continue`, `goto case`, and `yield break` count as statements.
- FMT-SWITCH-010: `#region` and `#pragma` block a section like `#if`.
- FMT-SPACE-010: an enum-member `=` is spaced.
- FMT-SPACE-001: preprocessor-directive interiors (`#if  A&&B`) are not touched.

## Breaking inputs

These are the inputs a tagged or new test must include, or must avoid because the behavior is disputed:

- FC-019/FC-020/FC-021: every join rule with a comment and with a directive in the gap. Joins already have broad tests (`CommentJoinTests`, `DirectiveJoinTests`). New focused tests for FMT-BREAK-003, -009, -023 (delegate, local function, bodiless member), and FMT-SWITCH-010 ("other sections are still braced") need a comment variant and a directive variant.
- FC-003: both sides of each boundary. FMT-SWITCH-003: one single-line statement vs one spanning two lines. FMT-BREAK-018: one line vs wrapped. FMT-BREAK-024: one-line attributed list vs multi-line attributed list. FMT-SPACE-007: `- -a` vs `- +a`.
- FC-010/FC-017: tag tests that use `AssertRuleResult` (exact, LF and CRLF, two passes). Phase-level unit tests are weaker. `Unit/SwitchCaseBraces/SwitchCaseBraceRewriterTests.cs` runs the brace phase on raw input without `LineBreakPhase`, so it cannot show FMT-SWITCH-008's pipeline behavior (FC-033). Several of its tests assert brace counts or `Contains` only.
- FC-027: nesting. Nested switches (FMT-SWITCH-013, none found), nested conditionals in either branch (FMT-BREAK-018), nested initializers (FMT-BREAK-021).
- FC-022: raw and verbatim multi-line literals for FMT-BREAK-013 and -016.
- Avoid the disputed inputs LB-Q1 to LB-Q10, SW-Q1, SW-Q2, SP-Q1, SP-Q2, the P6 instances, and the tracked shapes (#948 scoping, #949 comment before `break`, #955, #956 arms and comma/`)`, #957, #958 pairs, #1093 `param:`/`typevar:`, #1149 enum members, #1243 lone operator). A test pinned to such an input either flips when the decision lands or encodes behavior the maintainer has not confirmed.
- FMT-BREAK-012: a test of the boundary-only wrap pins today's join (`M(⏎1, 2)` → `M(1, 2)`), as decided in #1111 Q1. The spec text still says otherwise until #1131 lands.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `Reihitsu.Formatter` / `Reihitsu.Core` production code | Must stay (lane: tests only) |
| `src/Reihitsu.Formatter.Test` | Must change: tags, and focused tests where "none found" above |
| `src/Reihitsu.Core.Test` | Not involved by the lane. `CommaSpacingUtilitiesTests` and `UnaryOperatorSpacingUtilitiesTests` cover FMT-SPACE-007/-008 mechanics at Core level, if cross-project tags are wanted |
| Analyzer, code fixes, rule docs | Not involved. No new analyzer/formatter disagreement was found for group B |
| Specs under `sdd/specs/` | Must stay (no spec edits). The questions go to the issues |
| `RequirementCoverageTests.EnforcedSpecPaths` | Must change at the end (per `00-lead.md`) |

## Simpler option

_None._

## Questions for the maintainer

_None blocking._ The items under *Open questions to file* go to the per-spec issues, which the lead files.

## Out of scope, for the lead to route

- Group A (`formatter/indentation.md`, #953 family). Continuation lines that fall to the statement column or to column 0 in group B probes:
  - query clauses `let`/`select` at column 8 under `var q = from v in arr`;
  - a positional pattern `o is (1,⏎2)`, with `2);` at column 8;
  - a switch-expression arm `int i⏎when i > 0` with `when` at column 8;
  - an anonymous-object member `A⏎= 1`, with `= 1` at column 8;
  - `x⏎+= 1` and `x ??=⏎2`, at column 8;
  - `public⏎int B` at column 4;
  - the primary-constructor base `B⏎(a, 2)`, with `(a, 2);` at column 0;
  - `[Obsolete]⏎(x) => x` (lambda after a split attribute), with `(x)` at column 8;
  - `new[] { 1,⏎2 }` inside a split argument list, with `2` at column 16.
- Group A routed to group B "`new int[] {1,2}` → `{1, 2 }`": that is the decided change thoenissen/Reihitsu#958 (#920 Q3), and it is consistent with FMT-SPACE-009 as written.
- Stale test comment (FC-031): `Unit/SwitchCaseBraces/SwitchCaseBraceRewriterTests.cs`: `AddsBracesToTypePatternCaseWithMultipleStatements` says "second should not (1 statement)". The code braces every section once one is multi-line, and the assertion only checks a brace count.
- The phase-level `RemoveBraces*` unit tests reach the dead removal path (#950 removes it). Do not tag them as FMT-SWITCH requirement tests, except `RemovesBracesFromSingleLineSection`, which asserts that braces are kept.
- Stale non-goal wording (no edit in this run): `line-breaks.md` still lists switch-expression arms and the comma and `)` of argument lists as non-goals (decided change #956). `horizontal-spacing.md` lists the lambda arrow, the conditional operator, named-argument colons, and the space before an argument list (decided change #958).
