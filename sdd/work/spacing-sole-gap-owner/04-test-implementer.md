Role: test implementer
Commit: 7ad4b30
Verdict: tests written

The prompt carried the run's inputs and the coverage the lead asks for (every member of the skeptic's classes A–E and of the spec author's coverage table, joined cases next to their one-line counterparts, the accessor-modifier fusion through whole-document formatting and RH5408 under LF and CRLF). It carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the spec examples, the requirements, the change doc, and the maintainer's answers (1B–13B, 6C, 7C); layout that none of them decides (line breaks and columns of a conditional, a `with` initializer, a binary-operator wrap) was taken from the unchanged line-break and indentation behavior, run on this commit, and only the gap a requirement decides differs from it.

Every new or changed requirement is covered: `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"` passes 51 of 51 with these tests, `EveryRequirementOfAnEnforcedSpecIsCovered` included.

## Tests

Helper `AssertRuleResult` is `FormatterTestsBase.AssertRuleResult`: full pipeline under LF and CRLF, second pass, phase stability, and the per-phase round trip (FMT-PIPE-021). "Expected now" is the result on 7ad4b30. A data-row test is red when at least one row is red.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `JoinSeparatorTests.RequirementExampleIsSpacedByTheSpacingPhase` | FMT-BREAK-031 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `JoinSeparatorTests.ProtectedInternalOnTwoLinesOfAnAccessorListStaysTwoKeywords` | FMT-BREAK-031 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `JoinSeparatorTests.InternalProtectedOnTwoLinesOfAnAccessorListStaysTwoKeywords` | FMT-BREAK-031 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `JoinSeparatorTests.PrivateProtectedOnTwoLinesOfAnAccessorListStaysTwoKeywords` | FMT-BREAK-031 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `JoinSeparatorTests.ProtectedPrivateOnTwoLinesOfAnAccessorListStaysTwoKeywords` | FMT-BREAK-031 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `JoinSeparatorTests.AccessorModifiersInFrontOfTheFirstAccessorOfAnIndexerStayTwoKeywords` | FMT-BREAK-031 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `JoinSeparatorTests.BinaryOperatorsMovedInFrontOfAFusingOperandAreSpaced` | FMT-BREAK-031 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `JoinSeparatorTests.JoinedPairWithoutRuleKeepsTheMinimalSeparator` | FMT-SPACE-002, FMT-BREAK-031 | AssertRuleResult (LF, CRLF, second pass, phase stability) | green |
| `AccessorListSpacingTests.TightAccessorListsGetOneSpaceBehindTheBraceAndBetweenAccessors` | FMT-SPACE-022 | AssertRuleResult (LF, CRLF, second pass, phase stability) | green |
| `AccessorListSpacingTests.WhitespaceRunsInsideAOneLineAccessorListAreCollapsed` | FMT-SPACE-022 | AssertRuleResult (LF, CRLF, second pass, phase stability) | green |
| `AccessorListSpacingTests.OneLineAccessorListsWithAttributedAccessorsAreSpaced` | FMT-SPACE-022 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `AccessorListSpacingTests.CollapsedAccessorListIsSpacedLikeTheSameListOnOneLine` | FMT-SPACE-022, FMT-BREAK-031 | AssertRuleResult (LF, CRLF, second pass, phase stability) | green |
| `AccessorListSpacingTests.AccessorListInFrontOfAnInitializerIsSpaced` | FMT-SPACE-022 | AssertRuleResult (LF, CRLF, second pass, phase stability) | green |
| `AccessorListSpacingTests.InitializerBracesAreNotAnAccessorList` | FMT-SPACE-022 | AssertRuleResult (LF, CRLF, second pass, phase stability) | green |
| `ArrowSpacingTests.LambdaArrowsGetOneSpaceOnEachSide` | FMT-SPACE-017 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `ArrowSpacingTests.ArrowsOfLambdasWithModifiersAttributesAndReturnTypesGetOneSpaceOnEachSide` | FMT-SPACE-017 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `ArrowSpacingTests.SwitchExpressionArmArrowsGetOneSpaceOnEachSide` | FMT-SPACE-017 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `ArrowSpacingTests.PropertyAndIndexerArrowsGetOneSpaceOnEachSide` | FMT-SPACE-017 | AssertRuleResult (LF, CRLF, second pass, phase stability) | green |
| `ArrowSpacingTests.HandWrittenAccessorArrowsGetOneSpaceOnEachSide` | FMT-SPACE-017 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `ArrowSpacingTests.ArrowsCreatedByStructuralTransformsGetOneSpaceOnEachSide` | FMT-SPACE-017, FMT-STRUCT-026 | AssertRuleResult (LF, CRLF, second pass, phase stability) | green |
| `ArrowSpacingTests.ArrowsOfKeptMethodAndLocalFunctionExpressionBodiesGetOneSpaceOnEachSide` | FMT-SPACE-017 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `ArrowSpacingTests.JoinedLambdaArrowIsSpacedLikeTheSameArrowOnOneLine` | FMT-SPACE-017, FMT-BREAK-031 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `ArrowSpacingTests.JoinedPropertyArrowIsSpacedLikeTheSameArrowOnOneLine` | FMT-SPACE-017, FMT-BREAK-026 | AssertRuleResult (LF, CRLF, second pass, phase stability) | green |
| `ArrowSpacingTests.NestedArrowsGetOneSpaceOnEachSide` | FMT-SPACE-017 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `ArrowSpacingTests.ArrowsAlreadySpacedStayUnchanged` | FMT-SPACE-017 | AssertRuleResult (LF, CRLF, second pass, phase stability) | green |
| `ArrowSpacingTests.CommentInFrontOfTheArrowKeepsItsPlace` | FMT-SPACE-017, FMT-SPACE-013 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `ArrowSpacingTests.CommentBehindTheArrowKeepsItsPlace` | FMT-SPACE-017, FMT-SPACE-013 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `AttributeTargetColonSpacingTests.KeywordTargetsGetNoSpaceBeforeAndOneSpaceBehindTheColon` | FMT-SPACE-016 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `AttributeTargetColonSpacingTests.IdentifierTargetsGetNoSpaceBeforeAndOneSpaceBehindTheColon` | FMT-SPACE-016 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `AttributeTargetColonSpacingTests.WhitespaceRunsAroundTheColonAreNormalized` | FMT-SPACE-016 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `AttributeTargetColonSpacingTests.AttributeTargetsAlreadySpacedStayUnchanged` | FMT-SPACE-016 | AssertRuleResult (LF, CRLF, second pass, phase stability) | green |
| `AttributeTargetColonSpacingTests.AccessorAttributeTargetInsideAOneLineAccessorListIsSpaced` | FMT-SPACE-016 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `AttributeTargetColonSpacingTests.AttributeTargetsOfALocalFunctionAndItsParameterAreSpaced` | FMT-SPACE-016 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `AttributeTargetColonSpacingTests.JoinedAttributeTargetIsSpacedLikeTheSameTargetOnOneLine` | FMT-SPACE-016, FMT-BREAK-031 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `AttributeTargetColonSpacingTests.NamedAttributeArgumentColonBehindATargetIsANameColon` | FMT-SPACE-016, FMT-SPACE-019 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `AttributeTargetColonSpacingTests.DocumentationCommentInFrontOfTheColonKeepsItsPlace` | FMT-SPACE-016, FMT-SPACE-014 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `AttributeTargetColonSpacingTests.CommentTheTargetCarriesInFrontOfTheColonKeepsItsPlace` | FMT-SPACE-016, FMT-SPACE-013 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `AttributeTargetColonSpacingTests.CommentTheColonCarriesBehindItKeepsItsPlace` | FMT-SPACE-016, FMT-SPACE-013 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `ConditionalOperatorSpacingTests.TightConditionalGetsOneSpaceOnEachSideOfBothOperators` | FMT-SPACE-018 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `ConditionalOperatorSpacingTests.UnevenlySpacedConditionalsAreNormalized` | FMT-SPACE-018 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `ConditionalOperatorSpacingTests.NestedConditionalsAreSpaced` | FMT-SPACE-018 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `ConditionalOperatorSpacingTests.NestedConditionalsInArgumentsInterpolationsAndInitializersAreSpaced` | FMT-SPACE-018 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `ConditionalOperatorSpacingTests.NullableTypesConditionalAccessAndCoalescingAreNotConditionalOperators` | FMT-SPACE-018 | AssertRuleResult (LF, CRLF, second pass, phase stability) | green |
| `ConditionalOperatorSpacingTests.JoinedBranchesBehindTightOperatorsAreSpacedLikeSpacedOperators` | FMT-SPACE-018, FMT-BREAK-031 | AssertRuleResult (LF, CRLF, second pass, phase stability) | green |
| `ConditionalOperatorSpacingTests.CommentInFrontOfTheQuestionMarkKeepsItsPlace` | FMT-SPACE-018, FMT-SPACE-013 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `ConditionalOperatorSpacingTests.CommentBehindTheColonKeepsItsPlace` | FMT-SPACE-018, FMT-SPACE-013 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `DeclaredNameSpacingTests.ParameterNamesGetOneSpaceBehindTheirType` | FMT-SPACE-021 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `DeclaredNameSpacingTests.PointerParameterNameGetsOneSpaceBehindItsType` | FMT-SPACE-021 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `DeclaredNameSpacingTests.TupleElementNamesGetOneSpaceBehindTheirType` | FMT-SPACE-021 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `DeclaredNameSpacingTests.LocalVariableNamesGetOneSpaceBehindTheirType` | FMT-SPACE-021 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `DeclaredNameSpacingTests.DeclarationExpressionNamesGetOneSpaceBehindTheirType` | FMT-SPACE-021 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `DeclaredNameSpacingTests.DeclarationPatternDesignationsGetOneSpaceBehindTheirType` | FMT-SPACE-021 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `DeclaredNameSpacingTests.RecursiveAndListPatternDesignationsGetOneSpaceBehindThePattern` | FMT-SPACE-021 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `DeclaredNameSpacingTests.ForeachVariableGetsOneSpaceBehindItsType` | FMT-SPACE-021 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `DeclaredNameSpacingTests.CatchVariableGetsOneSpaceBehindItsType` | FMT-SPACE-021 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `DeclaredNameSpacingTests.QueryRangeVariablesGetOneSpaceBehindTheirType` | FMT-SPACE-021 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `DeclaredNameSpacingTests.GapInsideTheTypeIsNotDecidedByTheRule` | FMT-SPACE-021 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `DeclaredNameSpacingTests.JoinedParameterIsSpacedLikeTheSameParameterOnOneLine` | FMT-SPACE-021, FMT-BREAK-031 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `DeclaredNameSpacingTests.JoinedRangeVariableIsSpacedLikeTheSameRangeVariableOnOneLine` | FMT-SPACE-021, FMT-BREAK-031 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `DeclaredNameSpacingTests.CommentBehindTheTypeKeepsItsPlace` | FMT-SPACE-021, FMT-SPACE-013 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `DeclaredNameSpacingTests.DocumentationCommentInFrontOfTheNameKeepsItsPlace` | FMT-SPACE-021, FMT-SPACE-014 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `FunctionPointerCallingConventionSpacingTests.CallingConventionGetsOneSpaceBehindTheAsterisk` | FMT-SPACE-027 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `FunctionPointerCallingConventionSpacingTests.FunctionPointerWithoutCallingConventionKeepsItsGap` | FMT-SPACE-027 | AssertRuleResult (LF, CRLF, second pass, phase stability) | green |
| `FunctionPointerCallingConventionSpacingTests.JoinedCallingConventionIsSpacedLikeTheSameCallingConventionOnOneLine` | FMT-SPACE-027, FMT-BREAK-031 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `FunctionPointerCallingConventionSpacingTests.CommentBehindTheAsteriskKeepsItsPlace` | FMT-SPACE-027, FMT-SPACE-013 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `LambdaReturnTypeSpacingTests.KeywordArrayAndGenericReturnTypesGetOneSpace` | FMT-SPACE-026, FMT-BREAK-031 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `LambdaReturnTypeSpacingTests.NullableAndTupleReturnTypesGetOneSpace` | FMT-SPACE-026, FMT-BREAK-031 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `LambdaReturnTypeSpacingTests.ReturnTypeBehindAModifierGetsOneSpace` | FMT-SPACE-026 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `LambdaReturnTypeSpacingTests.ReturnTypeOfAnAssignedLambdaGetsOneSpace` | FMT-SPACE-026 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `LambdaReturnTypeSpacingTests.CommentBehindTheReturnTypeKeepsItsPlace` | FMT-SPACE-026, FMT-SPACE-013 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `NameColonSpacingTests.NamedArgumentColonsAreNormalized` | FMT-SPACE-019 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `NameColonSpacingTests.NamedAttributeArgumentColonsAreNormalized` | FMT-SPACE-019 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `NameColonSpacingTests.TupleElementNameColonsAreNormalized` | FMT-SPACE-019 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `NameColonSpacingTests.SubpatternNameColonsAreNormalized` | FMT-SPACE-019 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `NameColonSpacingTests.NestedNameColonsAreNormalized` | FMT-SPACE-019 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `NameColonSpacingTests.ColonsThatAreNotNameColonsKeepTheirSpacing` | FMT-SPACE-019 | AssertRuleResult (LF, CRLF, second pass, phase stability) | green |
| `NameColonSpacingTests.JoinedNamedArgumentIsSpacedLikeTheSameArgumentOnOneLine` | FMT-SPACE-019, FMT-BREAK-031 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `NameColonSpacingTests.JoinedTupleElementNameIsSpacedLikeTheSameNameOnOneLine` | FMT-SPACE-019, FMT-BREAK-031 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `NameColonSpacingTests.DocumentationCommentInFrontOfTheColonKeepsItsPlace` | FMT-SPACE-019, FMT-SPACE-014 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `NameColonSpacingTests.CommentBehindTheColonKeepsItsPlace` | FMT-SPACE-019, FMT-SPACE-013 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `NameColonSpacingTests.CommentInFrontOfTheColonKeepsItsPlace` | FMT-SPACE-019, FMT-SPACE-013 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `PrefixKeywordSpacingTests.LambdaModifiersGetOneSpaceInFrontOfAParameterList` | FMT-SPACE-024, FMT-BREAK-031 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `PrefixKeywordSpacingTests.LambdaModifiersGetOneSpaceInFrontOfAVerbatimParameter` | FMT-SPACE-024, FMT-BREAK-031 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `PrefixKeywordSpacingTests.ArgumentModifiersGetOneSpaceInFrontOfAParenthesizedOperand` | FMT-SPACE-024, FMT-BREAK-031 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `PrefixKeywordSpacingTests.ArgumentModifiersGetOneSpaceInFrontOfAVerbatimIdentifier` | FMT-SPACE-024, FMT-BREAK-031 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `PrefixKeywordSpacingTests.ArgumentModifiersGetOneSpaceInFrontOfAPointerIndirection` | FMT-SPACE-024, FMT-BREAK-031 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `PrefixKeywordSpacingTests.ThisAndParamsGetOneSpaceInFrontOfATupleType` | FMT-SPACE-024, FMT-BREAK-031 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `PrefixKeywordSpacingTests.RefInAndReadonlyGetOneSpaceInFrontOfATupleType` | FMT-SPACE-024, FMT-BREAK-031 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `PrefixKeywordSpacingTests.ScopedGetsOneSpaceInFrontOfItsOperand` | FMT-SPACE-024 | AssertRuleResult (LF, CRLF, second pass, phase stability) | green |
| `PrefixKeywordSpacingTests.AwaitGetsOneSpaceInFrontOfItsOperand` | FMT-SPACE-024, FMT-BREAK-031 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `PrefixKeywordSpacingTests.StackallocGetsOneSpaceInFrontOfItsBracket` | FMT-SPACE-024, FMT-BREAK-031 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `PrefixKeywordSpacingTests.VarGetsOneSpaceInFrontOfAParenthesizedDesignation` | FMT-SPACE-024, FMT-BREAK-031 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `PrefixKeywordSpacingTests.PatternNotGetsOneSpaceInFrontOfAConstant` | FMT-SPACE-024, FMT-BREAK-031 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `PrefixKeywordSpacingTests.PatternNotGetsOneSpaceInFrontOfARelationalPattern` | FMT-SPACE-024, FMT-BREAK-031 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `PrefixKeywordSpacingTests.PatternNotGetsOneSpaceInFrontOfADelimitedPattern` | FMT-SPACE-024 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `PrefixKeywordSpacingTests.ForeachInGetsOneSpaceInFrontOfItsCollection` | FMT-SPACE-024 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `PrefixKeywordSpacingTests.FromInGetsOneSpaceInFrontOfItsCollection` | FMT-SPACE-024, FMT-BREAK-031 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `PrefixKeywordSpacingTests.JoinInGetsOneSpaceInFrontOfItsCollection` | FMT-SPACE-024 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `PrefixKeywordSpacingTests.NestedKeywordsGetOneSpaceInFrontOfTheirOperand` | FMT-SPACE-024 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `PrefixKeywordSpacingTests.KeywordsWithTheirOwnParenthesisKeepTheirGap` | FMT-SPACE-024 | AssertRuleResult (LF, CRLF, second pass, phase stability) | green |
| `PrefixKeywordSpacingTests.MemberModifiersInFrontOfATupleTypeKeepTheirGap` | FMT-SPACE-024 | AssertRuleResult (LF, CRLF, second pass, phase stability) | green |
| `PrefixKeywordSpacingTests.CommentBehindTheKeywordKeepsItsPlace` | FMT-SPACE-024, FMT-SPACE-013 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `RelationalPatternSpacingTests.EveryRelationalOperatorGetsOneSpaceInFrontOfItsOperand` | FMT-SPACE-023 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `RelationalPatternSpacingTests.RelationalPatternsInEveryPatternContextAreSpaced` | FMT-SPACE-023 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `RelationalPatternSpacingTests.JoinedRelationalPatternIsSpacedLikeTheSamePatternOnOneLine` | FMT-SPACE-023, FMT-BREAK-031 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `RelationalPatternSpacingTests.BinaryComparisonsAreNotRelationalPatterns` | FMT-SPACE-023 | AssertRuleResult (LF, CRLF, second pass, phase stability) | green |
| `RelationalPatternSpacingTests.CommentBehindTheOperatorKeepsItsPlace` | FMT-SPACE-023, FMT-SPACE-013 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `RelationalPatternSpacingTests.DocumentationCommentInFrontOfTheOperandKeepsItsPlace` | FMT-SPACE-023, FMT-SPACE-014 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `WhenSpacingTests.CaseGuardBehindATokenThatIsNotAWordGetsOneSpace` | FMT-SPACE-020 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `WhenSpacingTests.CaseGuardBehindAnIdentifierGetsOneSpace` | FMT-SPACE-020 | AssertRuleResult (LF, CRLF, second pass, phase stability) | green |
| `WhenSpacingTests.SwitchExpressionArmGuardGetsOneSpace` | FMT-SPACE-020 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `WhenSpacingTests.CatchFilterGetsOneSpace` | FMT-SPACE-020 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `WhenSpacingTests.SpaceBehindWhenStaysAsWritten` | FMT-SPACE-020 | AssertRuleResult (LF, CRLF, second pass, phase stability) | green |
| `WhenSpacingTests.JoinedCaseGuardIsSpacedLikeTheSameGuardOnOneLine` | FMT-SPACE-020, FMT-BREAK-031 | AssertRuleResult (LF, CRLF, second pass, phase stability) | green |
| `WhenSpacingTests.CommentInFrontOfWhenKeepsItsPlace` | FMT-SPACE-020, FMT-SPACE-013 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `WithExpressionSpacingTests.WithBehindAnInvocationGetsOneSpaceInFrontOfIt` | FMT-SPACE-025 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `WithExpressionSpacingTests.WithBehindOtherNonWordTokensGetsOneSpaceInFrontOfIt` | FMT-SPACE-025 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `WithExpressionSpacingTests.WhitespaceRunsAroundWithAreCollapsed` | FMT-SPACE-025 | AssertRuleResult (LF, CRLF, second pass, phase stability) | green |
| `WithExpressionSpacingTests.WithInFrontOfANonEmptyInitializerGetsOneSpaceInFrontOfIt` | FMT-SPACE-025 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `WithExpressionSpacingTests.JoinedWithIsSpacedLikeTheSameWithOnOneLine` | FMT-SPACE-025, FMT-BREAK-031 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `WithExpressionSpacingTests.JoinedWithBehindAnElementAccessIsSpacedLikeTheSameWithOnOneLine` | FMT-SPACE-025, FMT-BREAK-031 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `WithExpressionSpacingTests.CommentInFrontOfWithKeepsItsPlace` | FMT-SPACE-025, FMT-SPACE-013 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `WithExpressionSpacingTests.DocumentationCommentInFrontOfWithKeepsItsPlace` | FMT-SPACE-025, FMT-SPACE-014 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `SameLineGapRulePhaseTests.ArrowsOfEveryExpressionBodiedMemberKindGetOneSpaceOnEachSide` | FMT-SPACE-017 | HorizontalSpacingPhase alone | red |
| `SameLineGapRulePhaseTests.OneLineEventAccessorListIsSpaced` | FMT-SPACE-022 | HorizontalSpacingPhase alone | red |
| `SameLineGapRulePhaseTests.OneLineIndexerAccessorListIsSpaced` | FMT-SPACE-022 | HorizontalSpacingPhase alone | red |
| `SameLineGapRulePhaseTests.AccessorListOverSeveralLinesIsNotSpaced` | FMT-SPACE-022 | HorizontalSpacingPhase alone | green |
| `SameLineGapRulePhaseTests.CommentsBehindTheBraceAndBetweenAccessorsKeepTheirPlace` | FMT-SPACE-022, FMT-SPACE-013 | HorizontalSpacingPhase alone | red |
| `SameLineGapRulePhaseTests.DocumentationCommentsInFrontOfAccessorsKeepTheirPlace` | FMT-SPACE-022, FMT-SPACE-014 | HorizontalSpacingPhase alone | red |
| `SameLineGapRulePhaseTests.DocumentationCommentBehindTheArrowKeepsItsPlace` | FMT-SPACE-017, FMT-SPACE-014 | HorizontalSpacingPhase alone | red |
| `SameLineGapRulePhaseTests.DocumentationCommentsAroundTheQuestionMarkKeepTheirPlace` | FMT-SPACE-018, FMT-SPACE-014 | HorizontalSpacingPhase alone | red |
| `SameLineGapRulePhaseTests.DocumentationCommentInFrontOfWhenKeepsItsPlace` | FMT-SPACE-020, FMT-SPACE-014 | HorizontalSpacingPhase alone | red |
| `SameLineGapRulePhaseTests.DocumentationCommentInFrontOfTheOperandOfAwaitKeepsItsPlace` | FMT-SPACE-024, FMT-SPACE-014 | HorizontalSpacingPhase alone | red |
| `SameLineGapRulePhaseTests.DocumentationCommentInFrontOfTheLambdaParameterListKeepsItsPlace` | FMT-SPACE-026, FMT-SPACE-014 | HorizontalSpacingPhase alone | red |
| `SameLineGapRulePhaseTests.DocumentationCommentInFrontOfTheCallingConventionKeepsItsPlace` | FMT-SPACE-027, FMT-SPACE-014 | HorizontalSpacingPhase alone | red |
| `SameLineGapRulePhaseTests.WithRequirementExampleGetsOneSpaceOnEachSide` | FMT-SPACE-025 | HorizontalSpacingPhase alone | red |
| `SameLineGapRulePhaseTests.WithBehindAnObjectInitializerGetsOneSpaceInFrontOfIt` | FMT-SPACE-025 | HorizontalSpacingPhase alone | red |
| `SameLineGapRulePhaseTests.ArrowOfAnAttributedLambdaGetsOneSpaceOnEachSide` | FMT-SPACE-017 | HorizontalSpacingPhase alone | red |
| `JoinSeparatorPhaseTests.RequirementExampleHoldsTheMinimalSeparatorAtEveryJoin` | FMT-BREAK-031 | LineBreakPhase alone | red |
| `JoinSeparatorPhaseTests.AccessorModifiersJoinedInACollapsedListAreSeparatedByOneSpace` | FMT-BREAK-031 | LineBreakPhase alone | red |
| `JoinSeparatorPhaseTests.BinaryOperatorMovedToTheNextLineIsSeparatedFromAFusingOperand` | FMT-BREAK-031 | LineBreakPhase alone | red |
| `JoinSeparatorPhaseTests.AssignmentJoinsWriteNoWhitespace` | FMT-BREAK-031 | LineBreakPhase alone | red |
| `JoinSeparatorPhaseTests.CaseGuardJoinWritesTheMinimalSeparator` | FMT-BREAK-031 | LineBreakPhase alone | red |
| `JoinSeparatorPhaseTests.ListElementJoinsWriteTheMinimalSeparator` | FMT-BREAK-031 | LineBreakPhase alone | red |
| `JoinSeparatorPhaseTests.PropertyArrowJoinsWriteNoWhitespace` | FMT-BREAK-031, FMT-BREAK-026 | LineBreakPhase alone | red |
| `JoinSeparatorPhaseTests.GapsTheJoinDoesNotTouchKeepTheirWhitespace` | FMT-BREAK-031, FMT-BREAK-026, FMT-BREAK-024 | LineBreakPhase alone | red |
| `JoinSeparatorPhaseTests.CollapsedAccessorListIsJoinedWithoutWhitespace` | FMT-BREAK-031, FMT-BREAK-024 | LineBreakPhase alone | red |
| `JoinSeparatorPhaseTests.JoinsOfTokensThatStayApartWriteNoWhitespace` | FMT-BREAK-031 | LineBreakPhase alone | green |
| `JoinSeparatorPhaseTests.CommentInTheGapStillRefusesEveryJoin` | FMT-BREAK-031 | LineBreakPhase alone | green |
| `JoinSeparatorPhaseTests.DirectiveInTheGapStillRefusesEveryJoin` | FMT-BREAK-031 | LineBreakPhase alone | green |
| `ReihitsuFormatterSameLineGapTests.FormatNodeSpacesThePropertyArrowAndTheAccessorList` | FMT-SPACE-017, FMT-SPACE-022 | FormatNode / FormatNodeInDocumentAsync | red |
| `ReihitsuFormatterSameLineGapTests.FormatNodeKeepsJoinedAccessorModifiersApart` | FMT-BREAK-031 | FormatNode / FormatNodeInDocumentAsync | red |
| `ReihitsuFormatterSameLineGapTests.FormatNodeSpacesANamedLambdaArgument` | FMT-SPACE-019, FMT-SPACE-017 | FormatNode / FormatNodeInDocumentAsync | red |
| `ReihitsuFormatterSameLineGapTests.FormatNodeInDocumentAsyncSpacesThePropertyArrow` | FMT-SPACE-017 | FormatNode / FormatNodeInDocumentAsync | green |
| `ReihitsuFormatterSameLineGapTests.FormatNodeInDocumentAsyncSpacesTheAccessorList` | FMT-SPACE-022 | FormatNode / FormatNodeInDocumentAsync | green |
| `ReihitsuFormatterSameLineGapTests.FormatNodeInDocumentAsyncKeepsJoinedAccessorModifiersApart` | FMT-BREAK-031 | FormatNode / FormatNodeInDocumentAsync | red |
| `RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzerTests.VerifyFixSpacesATightArrow` | FMT-SPACE-017 | Verify (code fix, re-analysis, Fix All) | green |
| `RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzerTests.VerifyFixSpacesATightArrowWithCarriageReturnLineFeed` | FMT-SPACE-017 | Verify (code fix, re-analysis, Fix All) | green |
| `RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzerTests.VerifyFixKeepsProtectedInternalAccessorModifiersOnTwoLinesApart` | FMT-BREAK-031 | Verify (code fix, re-analysis, Fix All) | red |
| `RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzerTests.VerifyFixKeepsProtectedInternalAccessorModifiersOnTwoLinesApartWithCarriageReturnLineFeed` | FMT-BREAK-031 | Verify (code fix, re-analysis, Fix All) | red |
| `RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzerTests.VerifyFixKeepsInternalProtectedAccessorModifiersOnTwoLinesApart` | FMT-BREAK-031 | Verify (code fix, re-analysis, Fix All) | red |
| `RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzerTests.VerifyFixKeepsInternalProtectedAccessorModifiersOnTwoLinesApartWithCarriageReturnLineFeed` | FMT-BREAK-031 | Verify (code fix, re-analysis, Fix All) | red |
| `RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzerTests.VerifyFixKeepsPrivateProtectedAccessorModifiersOnTwoLinesApart` | FMT-BREAK-031 | Verify (code fix, re-analysis, Fix All) | red |
| `RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzerTests.VerifyFixKeepsPrivateProtectedAccessorModifiersOnTwoLinesApartWithCarriageReturnLineFeed` | FMT-BREAK-031 | Verify (code fix, re-analysis, Fix All) | red |
| `RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzerTests.VerifyFixKeepsProtectedPrivateAccessorModifiersOnTwoLinesApart` | FMT-BREAK-031 | Verify (code fix, re-analysis, Fix All) | red |
| `RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzerTests.VerifyFixKeepsProtectedPrivateAccessorModifiersOnTwoLinesApartWithCarriageReturnLineFeed` | FMT-BREAK-031 | Verify (code fix, re-analysis, Fix All) | red |
| `RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzerTests.VerifyFixKeepsSplitAccessorModifiersApartInSeveralProperties` | FMT-BREAK-031 | Verify (code fix, re-analysis, Fix All) | red |
| `HorizontalSpacingRequirementTests.AttributeTargetColonsGetNoSpaceBeforeAndOneSpaceBehind` | FMT-SPACE-016 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `HorizontalSpacingRequirementTests.EveryArrowGetsOneSpaceOnEachSide` | FMT-SPACE-017 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `HorizontalSpacingRequirementTests.ConditionalOperatorGetsOneSpaceOnEachSide` | FMT-SPACE-018 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `HorizontalSpacingRequirementTests.NameColonsGetNoSpaceBeforeAndOneSpaceBehind` | FMT-SPACE-019 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `HorizontalSpacingRequirementTests.WhenGetsOneSpaceInFrontOfIt` | FMT-SPACE-020 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `HorizontalSpacingRequirementTests.DeclaredNamesGetOneSpaceBehindTheirTypeOrPattern` | FMT-SPACE-021 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `HorizontalSpacingRequirementTests.OneLineAccessorListsGetOneSpaceInside` | FMT-SPACE-022 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `HorizontalSpacingRequirementTests.RelationalPatternOperatorsGetOneSpaceBehind` | FMT-SPACE-023 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `HorizontalSpacingRequirementTests.ModifiersAndPrefixKeywordsGetOneSpaceBehind` | FMT-SPACE-024 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `HorizontalSpacingRequirementTests.LambdaReturnTypesGetOneSpaceInFrontOfTheParameterList` | FMT-SPACE-026 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `HorizontalSpacingRequirementTests.FunctionPointerCallingConventionsGetOneSpaceBehindTheAsterisk` | FMT-SPACE-027 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `HorizontalSpacingRequirementTests.AttributeTargetRuleDecidesBeforeTheKeywordRule` | FMT-SPACE-012, FMT-SPACE-016, FMT-SPACE-002 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `StructuralTransformSeparatorFinalOutputTests.HandWrittenArrowAndConvertedAccessorArrowAreSpacedAlike` (updated, renamed from `HandWrittenArrowKeepsItsGapsWhileAConvertedAccessorGetsSpacesAroundItsArrow`) | FMT-STRUCT-026, FMT-SPACE-017 | AssertRuleResult (LF, CRLF, second pass, phase stability) | red |
| `LineBreakRewriterTests.CollapsesGetOnlyAutoPropertyToSingleLine` (updated) | FMT-BREAK-031 | LineBreakPhase alone | red |
| `LineBreakRewriterTests.CollapsesGetSetAutoPropertyToSingleLine` (updated) | FMT-BREAK-031 | LineBreakPhase alone | red |
| `LineBreakRewriterTests.CollapsesPropertyAttributedAutoPropertyToSingleLine` (updated) | FMT-BREAK-031 | LineBreakPhase alone | red |
| `LineBreakRewriterTests.CollapsesAutoPropertyWithJoinableWrappedSignature` (updated) | FMT-BREAK-031 | LineBreakPhase alone | red |
| `LineBreakRewriterTests.CollapsesExpressionBodiedProperty` (updated) | FMT-BREAK-031 | LineBreakPhase alone | red |
| `StructuralTransformSeparatorTests.AccessorTransformWritesNoWhitespaceAroundTheCreatedArrow` (updated, renamed from `AccessorTransformWritesOneSpaceOnEachSideOfTheCreatedArrow`) | FMT-STRUCT-026 | StructuralTransformPhase alone | red |
| `StructuralTransformSeparatorTests.GetOnlyMemberTransformWritesNoWhitespaceAroundTheCreatedArrow` (updated, renamed from `GetOnlyMemberTransformWritesOneSpaceOnEachSideOfTheCreatedArrow`) | FMT-STRUCT-026 | StructuralTransformPhase alone | red |
| `StructuralTransformSeparatorTests.GetOnlyMemberTransformWritesNoWhitespaceAroundTheArrowBehindAnIndexer` (updated, renamed from `GetOnlyMemberTransformWritesOneSpaceOnEachSideOfTheArrowBehindAnIndexer`) | FMT-STRUCT-026 | StructuralTransformPhase alone | red |

Totals: 188 test methods new or updated (157 red, 31 green on this commit). Files:

- new: `src/Reihitsu.Formatter.Test/Regression/Spacing/{AttributeTargetColon,Arrow,ConditionalOperator,NameColon,When,DeclaredName,AccessorList,RelationalPattern,PrefixKeyword,WithExpression,LambdaReturnType,FunctionPointerCallingConvention}SpacingTests.cs`, `src/Reihitsu.Formatter.Test/Regression/LineBreaks/JoinSeparatorTests.cs`, `src/Reihitsu.Formatter.Test/Unit/HorizontalSpacing/SameLineGapRulePhaseTests.cs`, `src/Reihitsu.Formatter.Test/Unit/LineBreaks/JoinSeparatorPhaseTests.cs`, `src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterSameLineGapTests.cs`;
- extended: `HorizontalSpacingRequirementTests.cs` (spec examples of FMT-SPACE-016 to -024, -026, -027, and the FMT-SPACE-002/-012/-016 precedence case), `RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzerTests.cs`, `RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzerTests.cs`;
- updated, the ones the handovers name: `LineBreakRewriterTests` (five collapse tests, phase text now the minimal separator), `StructuralTransformSeparatorTests` (three created-arrow tests, now no whitespace), `StructuralTransformSeparatorFinalOutputTests` (`get=>"";` now `get => "";`). No other existing test was changed. `ListElementInteriorJoinTests.LambdaParameterListWrappedAfterStaticModifierKeepsItsSpace`, which 01-spec-author listed under option A of the old Q10, keeps its expectation: answer 10B keeps `static (int value)`.

### Why the red tests fail on this commit

Observations only, grouped by what the assertion meets:

- **One-line pair spaced as written** (FMT-SPACE-016 to -027, every "on one line" line of a joined/one-line pair, every spec example, every comment test of class E): the phase keeps the author's gap, e.g. `[field :NonSerialized]`, `y=>y`, `b?1:2`, `M(b:b)`, `case (1, 2)when b:`, `int?a`, `{[Obsolete] get;set; }`, `x is >5`, `await(t)`, `not"x"`, `p.Q()with`, `int(x) => x`, `delegate*managed<int>`.
- **Accessor modifiers fused** (class A): whole-document formatting, `FormatNode`, `FormatNodeInDocumentAsync`, and the RH5408 fix give `protectedinternal`, `internalprotected`, `privateprotected`, `protectedprivate` under LF and CRLF; the full-pipeline tests stop at the round trip ("The output of LineBreakPhase does not parse back to its tree … CS1014").
- **Binary operator fused in the phase's text** (class A): `JoinSeparatorPhaseTests.BinaryOperatorMovedToTheNextLineIsSeparatedFromAFusingOperand` sees `--a`, `++a`, `+++a`, `---a`, `&&a`, `/*p` (a comment start), `asstring`, `isint`; `JoinSeparatorTests.BinaryOperatorsMovedInFrontOfAFusingOperandAreSpaced` and the FMT-BREAK-031 example stop at the round trip of the line-break phase.
- **The line-break phase writes a space at a join or into a gap it does not join** (FMT-BREAK-031, -026, -024, FMT-STRUCT-026): phase text `Value => 42;`, `Prop { get; }`, `P { get; set; }`, `var x = 1;`, `var y = 2;`, `case (1, 2) when b:`, `N(name: "x")`, `N(static (int i) => i)`, `N(x is > 1)`, `N(z => z)`, `int? a`, `? 1`; `public int R =>1;` becomes `=> 1`, `{get;set;}` becomes `{ get; set; }`, `T  =>  1` becomes `=> 1`; the structural transforms write `" "` on both sides of a created `=>`.
- `HandWrittenArrowAndConvertedAccessorArrowAreSpacedAlike`: `get=>"";` stays.

### Green tests and what they hold

Joined outputs that must not change (`JoinedBranchesBehindTightOperators…`, `JoinedCaseGuard…`, `JoinedPropertyArrow…`, `CollapsedAccessorListIsSpacedLikeTheSameListOnOneLine`, `JoinedPairWithoutRuleKeepsTheMinimalSeparator`, `JoinsOfTokensThatStayApartWriteNoWhitespace`), counter-sides of each rule (`…AreNotConditionalOperators`, `ColonsThatAreNotNameColons…`, `SpaceBehindWhenStaysAsWritten`, `InitializerBracesAreNotAnAccessorList`, `AccessorListOverSeveralLinesIsNotSpaced`, `BinaryComparisonsAreNotRelationalPatterns`, `KeywordsWithTheirOwnParenthesis…`, `MemberModifiersInFrontOfATupleType…`, `FunctionPointerWithoutCallingConvention…`), already-formatted inputs, created arrows in the final output, the join refusals with a comment and with a directive, RH5401 on a tight arrow (LF, CRLF), and `FormatNodeInDocumentAsync` on a property arrow and an accessor list (today's property layout already spaces both; they guard the move of that spacing into the spacing phase).

## Scenarios (bug fix)

Class A, change doc entry 10 (not an output question; FMT-PIPE-003):

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `get;⏎protected⏎internal set;` (and `internal⏎protected`, `private⏎protected`, `protected⏎private`) | LF, CRLF | whole document (`JoinSeparatorTests`) | reproduced | `{ get; protectedinternal set; }` (etc.) vs `{ get; protected internal set; }` |
| same, in front of the first accessor of an indexer | LF, CRLF | whole document | reproduced | `{ protectedinternal get; set; }` vs `{ protected internal get; set; }` |
| same four pairs | LF, CRLF | RH5408 code fix (`Verify`, one pass and Fix All over two properties) | reproduced | fixed code `protectedinternal set;` (etc.), does not compile |
| same four pairs | LF, CRLF | `FormatNode` on the property | reproduced | `public int P { get; internalprotected set; }` |
| `protected⏎internal` | LF, CRLF | `FormatNodeInDocumentAsync` on the property | reproduced | `protectedinternal` |
| same four pairs | LF | line-break phase alone | reproduced | `P{get;protectedinternal set;}`-like text vs `P{get;protected internal set;}` |
| `a -⏎-a`, `a +⏎+a`, `a +⏎++a`, `a -⏎--a`, `a &⏎&a`, `a /⏎*p`, `o as⏎string`, `o is⏎int` | LF (CRLF for `-`, `/` via the example) | line-break phase alone | reproduced | fused or comment-opening text; final output of `-`, `+`, `as`, `is` repaired by FMT-SPACE-010 but rejected by the round trip |

## Not covered, and why

- **Failure classes walked.** FC-002/FC-003 (counter-sides, both sides of each rule), FC-006/FC-022/FC-023 (fusing pairs, literals, verbatim identifiers), FC-010 (LF/CRLF and second pass through `AssertRuleResult`), FC-017 (exact output everywhere; phase-text tests assert the joined text plus that the text reads back as the input's tokens), FC-019/FC-021 (a comment on both sides of every rule's gap; a comment and a `#pragma` in the gap of every joiner), FC-025 (every arrow kind), FC-027 (nesting in arguments, nested argument lists, interpolation holes, initializers, subpatterns, list patterns), FC-028 (`FormatNode`, `FormatNodeInDocumentAsync`, an argument root), FC-007/FC-008 (RH5408 one pass and Fix All), FC-033 (RH5408 path, CRLF, `FormatNode`). Not applicable: FC-018/FC-036 (no rename), FC-024 (no partial or interface construct is spaced differently), FC-026 (no token re-resolution in a test-side edit), FC-029 (no key/value layout), FC-011/FC-012 (no loop or neighbor-phase insert/collapse added; the phase-stability check of `AssertRuleResult` would see a reversal), FC-013/FC-014 (the skeptic checked RH6xxx rules; no rule reports the pairs). FC-032 (the full analyzer project, because every code fix formats through the formatter) is a run after the implementation, not a test to write; on this commit the RH5401 and RH5408 classes give 92 passed, 9 failed (the new RH5408 tests).
- **A delimited documentation comment `/** */` on the right side of a gap inside an expression or a parameter list** (arrow, conditional, `when`, `await`, lambda return type, function-pointer calling convention): the documentation-comment phase moves it onto a line of its own in the final output, so its spacing is tested with `HorizontalSpacingPhase` alone (`SameLineGapRulePhaseTests`). In an attribute list, behind a name colon, behind a type, behind a relational operator, and in front of `with`, it stays on the line and is tested through the full pipeline.
- **Arrows of operators, conversion operators, constructors, finalizers, and methods or local functions with a `Task` return type**: the structural transforms convert them to blocks in every caller, so they are tested with `HorizontalSpacingPhase` alone; the kept expression bodies (custom task type) go through the full pipeline.
- **The spec example of FMT-SPACE-025** is tested with `HorizontalSpacingPhase` alone (`SameLineGapRulePhaseTests.WithRequirementExampleGetsOneSpaceOnEachSide`): the full pipeline lays out every `with` initializer, an empty one included, over several lines (`p.Q() with⏎{⏎X = 1⏎};`), so the example's one-line output is not the formatter's output. Change doc entry 11 shows the same one-line After; output question 11 as asked showed the multi-line layout. Finding for the spec author, not a test question.
- **One-line event and indexer accessor lists** (FMT-SPACE-022) are tested with `HorizontalSpacingPhase` alone: the line-break phase lays a list with accessor bodies out over several lines. A one-line list with a comment is expanded by the line-break phase too, so the comment cases of FMT-SPACE-022 are phase-level as well.
- **Class C members that are not members.** `x!⏎with { }` is not joined on this commit (the null-forgiving operator's gap is owned): `N(p!⏎with { })` keeps the line break and puts `with` and the initializer below it. Its one-line form is covered (`p!with` → `p! with`). `new P { }⏎with { }` is joined, but the object initializer is laid out over several lines first; its spacing is covered at phase level (`new P { }with { }` → `new P { } with { }`).
- **A member's own modifier in front of a tuple type** (`public(int, int) G;`, `static(int, int) H()`) and a member's return type in front of its name are left unspecified by FMT-SPACE-024 and FMT-SPACE-021; `MemberModifiersInFrontOfATupleTypeKeepTheirGap` pins only that the rule does not touch them. A `ref` in front of a local's tuple type (`ref(int, int) r = ref t;`) is not tested: FMT-SPACE-024 names "a modifier of a type", which covers it, but also excludes "a modifier of a local declaration", which may cover it; the spec author decides.
- **`scoped`** cannot stand directly in front of a token that does not need a space, so only the collapse of a run is tested.
- **The attribute-list merge exception of FMT-BREAK-031** is output-neutral by its own statement; the existing `LineBreakRewriterTests.KeepsSingleLineAutoPropertyOnOneLineWhenMergingAccessorAttributeLists` and the FMT-BREAK-027 tests cover it unchanged.
- **Lambda with an attribute in the full pipeline**: `Func<int, int> f = [return:Obsolete] (int x) => x;` is broken after the attribute list today (`[return:Obsolete]⏎(int x) => x;`, the lambda at the statement's column), so the attributed lambda's arrow and target colon are tested at phase level only. Follow-up candidate, outside the run (line breaks, FMT-BREAK-027): reachable through whole-document formatting.
- `scripts/test.sh --project architecture` fails `SquadNoteReferenceTests.RepositoryFilesReferenceNoSquadNote` on `sdd/work/spacing-sole-gap-owner/00-lead.md:24` (it names two `squad/notes/` files). Not a test file of this role; `sdd/work/` is deleted before publishing.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~AttributeTargetColonSpacingTests|FullyQualifiedName~ArrowSpacingTests|FullyQualifiedName~ConditionalOperatorSpacingTests|FullyQualifiedName~NameColonSpacingTests|FullyQualifiedName~WhenSpacingTests|FullyQualifiedName~DeclaredNameSpacingTests|FullyQualifiedName~AccessorListSpacingTests|FullyQualifiedName~RelationalPatternSpacingTests|FullyQualifiedName~PrefixKeywordSpacingTests|FullyQualifiedName~WithExpressionSpacingTests|FullyQualifiedName~LambdaReturnTypeSpacingTests|FullyQualifiedName~FunctionPointerCallingConventionSpacingTests|FullyQualifiedName~HorizontalSpacingRequirementTests|FullyQualifiedName~SameLineGapRulePhaseTests|FullyQualifiedName~JoinSeparatorPhaseTests|FullyQualifiedName~JoinSeparatorTests|FullyQualifiedName~ReihitsuFormatterSameLineGapTests|FullyQualifiedName~LineBreakRewriterTests|FullyQualifiedName~StructuralTransformSeparator"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5408SimpleAutoProperties|FullyQualifiedName~RH5401ExpressionStyleGetOnly"
scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"
```

On 7ad4b30: formatter 161 failed, 175 passed of 336 (data rows counted; every failure is in a new or updated test, and the whole formatter project shows no other failure: 158 failed, 3768 passed before the last four tests were added); analyzer 9 failed, 92 passed; architecture `Sdd` 51 passed.
