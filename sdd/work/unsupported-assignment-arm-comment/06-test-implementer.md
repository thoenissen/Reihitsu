Role: test implementer
Commit: 4f0282c (tests); this handover is the commit after it
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. I worked from `00-lead.md` (decision 4 sets the test goals: one precheck test per input, one test per affected fix plus the must-stay-offered cases), `01`–`05`, the accepted change doc, and `git diff origin/main HEAD -- sdd AGENTS.md`.

Every expected value comes from a spec example, a requirement sentence, or the change doc. To find the existing tests the change breaks, I ran the whole formatter, analyzer, and CLI suites against a throwaway prototype of the precheck (the three shapes, the five deleted join refusals) in a scratch worktree outside the repository. That prototype is not committed. It decided no expected value: it listed the breaking tests, and it served as a sanity run of the new tests (all 7374 formatter tests, the 1082 tests of the changed analyzer classes, and the 441 CLI tests pass against it). Supported inputs whose layout this change does not touch (the retargeted carriers, the still-offered fixes) were written from the spec and then confirmed green on this branch, which has no production change.

## Tests

"Fails" means: fails now, for the reason the change removes (the input is formatted or the fix is offered instead of refused or withheld). "Passes" means: passes now and must keep passing.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| **New: `tests/Reihitsu.Formatter.Test/Unit/Precheck/UnsupportedAssignmentCommentTests.cs`** (replaces `UnsupportedAssignmentInArmCommentTests.cs`, FMT-UNSUP-019 retired) | | | |
| `SpecExampleStatementAndPropertyInitializerAreUnsupported` (3,24) | FMT-UNSUP-020 | `AssertUnsupportedFor` + location | Fails |
| `SpecExampleInitializerMembersAndACommentOnOneLineAreUnsupported` (12,13) | FMT-UNSUP-020 | same | Fails |
| `SpecExampleCompoundOperatorAndDeconstructionAreUnsupported` (5,14) | FMT-UNSUP-020 | same | Fails |
| `SpecExampleAssignmentInsideAnArmOrAMemberIsUnsupported` (7,30, new reason) | FMT-UNSUP-020 | same | Fails (old reason) |
| `SpecExampleEqualsOfEnumMemberPropertyParameterAndDeclaratorAreUnsupported` (3,9) | FMT-UNSUP-020 | same | Fails |
| `SpecExampleDirectiveBehindAnAssignmentIsUnsupported` (6,1; with and without `DEBUG`) | FMT-UNSUP-020, FC-005, FC-020 | same | Fails |
| `SpecExampleCommentsOutsideTheShapeAreSupported` | FMT-UNSUP-020 | `AssertSupported` + `AssertRuleResult` (fixed point) | Passes |
| `EveryAssignmentOperatorIsUnsupported` (all 13 operators, one line and across lines) | FMT-UNSUP-020, FC-003 | `AssertUnsupportedFor` | Fails |
| `AssignmentIsUnsupportedWhereverItStands` (argument, lambda, block lambda, `for` incrementor, chains, ref assignment, three deconstructions, object/`with`/index members in a declaration's initializer value, own-line member comment, arm, `when`, nested arm, anonymous member, lambda in member, collection element, interpolation hole) | FMT-UNSUP-020, FC-039, FC-027 | same | Fails |
| `AssignmentInsideTheInitializerValueOfAMemberIsUnsupported` (field, multi-declarator field, property) | FMT-UNSUP-020, FC-039 | same | Fails |
| `EveryKindOfTriviaBehindTheOperatorIsUnsupported` (own-line `//`, leading `/* */`, `/** */`, `#pragma`, `#region`, `#if` behind `+=`) | FMT-UNSUP-020, FC-005, FC-020 | same | Fails |
| `ConditionalGroupBehindTheOperatorIsUnsupportedInEveryBuild` | FMT-UNSUP-020, FC-020 | same | Fails |
| `DocumentationCommentBehindTheOperatorIsUnsupportedInEveryDocumentationMode` (`/** */`, `/// d`, own-line `///` × Parse/None/Diagnose) | FMT-UNSUP-020, FC-040 | same | Fails |
| `DocumentationCommentBehindTheEqualsOfADeclarationIsUnsupportedInEveryDocumentationMode` (property, enum member) | FMT-UNSUP-020, FC-040 | same | Fails |
| `EqualsOfEveryOwnerOfAnEqualsValueClauseIsUnsupported` (property ×3, enum ×2, parameter of method/local function/lambda/indexer/constructor/record, directive behind local/field/event/const/`using`/`fixed`/`for` declarator) | FMT-UNSUP-020 | same | Fails |
| `CommentOutsideTheGapIsSupported` (in front of `=`/`+=`, inside the value, behind `ref`, behind/above the statement, in front of a member's `=`, line break alone) | FMT-UNSUP-020, FC-003 | `AssertSupported` | Passes |
| `CommentOutsideTheGapOfAnEqualsValueClauseIsSupported` | FMT-UNSUP-020, FC-003 | same | Passes |
| `EqualsOfOtherShapesKeepsItsReason` (anonymous member, `let`, attribute argument, using alias, `==`, `<=`) | FMT-UNSUP-020 | `AssertUnsupportedFor` | Passes (017, 011 rows), fails (022 rows) |
| `OverlapWithTheVariableDeclarationShapesGivesTheirReason` (004 for a declarator comment, 010 for a multi-declarator field directive) | FMT-UNSUP-020 | same | Passes |
| `AssignmentInDisabledTextIsNotChecked` | FMT-UNSUP-020 | both | Fails (X defined, old reason), passes (X undefined) |
| `FirstOfTwoCommentsIsReported` | FMT-UNSUP-020 | `AssertUnsupportedFor` | Fails |
| `FormatDocumentAsyncRefusesTheShape` | FMT-UNSUP-020 (FMT-UNSUP-002 entry point) | direct | Fails |
| `NodeLevelEntryPointsRefuseATargetThatHoldsTheShape` (`FormatNode`, `FormatNodeInDocumentAsync`, context entry point, method beside formatted) | FMT-UNSUP-020 | direct | Fails |
| `NodeThatHoldsTheOperatorHoldsTheShape` | FMT-UNSUP-020 | `FindUnsupportedSyntax`, `FormatNode` | Fails |
| `EqualsValueClauseWithoutItsOwnerHoldsTheShape` (four clause targets; statement keeps 004) | FMT-UNSUP-003, FMT-UNSUP-020, FC-041 | same | Fails |
| `NodeThatHoldsOnlyTheValueDoesNotHoldTheShape` (object creation behind `= // c` and behind own-line comment; property value) | FMT-UNSUP-003, FMT-UNSUP-020, FC-041 | same | Fails (the parent assertions) |
| `FindUnsupportedSyntaxAnswersForABuiltNode` | FMT-UNSUP-020 | same | Fails |
| `OutputOfJoinsAroundTheOperatorIsNotUnsupported` (joins behind `=`, `+=` across a blank line, collection expression, declarator, arm and member; `/** */` in front of `=` and behind the value) | FMT-UNSUP-005, FMT-UNSUP-020, FC-038 | format twice | Passes |
| `OutputOfJoinsBehindTheEqualsOfADeclarationIsNotUnsupported` (property array/collection, enum member, parameter, `int P { get; } /** c */ = 1;`) | FMT-UNSUP-005, FMT-UNSUP-020, FC-038 | format twice | Passes |
| **New: `.../Unit/Precheck/UnsupportedSwitchArmExpressionCommentTests.cs`** | | | |
| `SpecExampleEachArmIsUnsupported` (7,25) | FMT-UNSUP-021 | `AssertUnsupportedFor` + location | Fails |
| `SpecExampleCommentsOutsideTheShapeAreSupported` | FMT-UNSUP-021 | `AssertSupported` + `AssertRuleResult` | Passes |
| `DirectiveBehindTheArrowIsUnsupportedInEveryBuild` (change doc, 8,1) | FMT-UNSUP-021, FC-020 | same | Fails |
| `EveryKindOfTriviaBehindTheArrowIsUnsupported` (one line, trailing `//`, own-line `//`, leading `/* */`, `/** */`, `#pragma`, arm with `when`, assignment arm) | FMT-UNSUP-021, FC-005 | same | Fails |
| `DocumentationCommentBehindTheArrowIsUnsupportedInEveryDocumentationMode` | FMT-UNSUP-021, FC-040 | same | Fails |
| `EveryArmIsUnsupportedWhereverItStands` (default arm, nested switch, inside a lambda, property initializer) | FMT-UNSUP-021, FC-039 | same | Fails |
| `CommentOutsideTheGapIsSupported` | FMT-UNSUP-021, FC-003 | `AssertSupported` | Passes |
| `ArrowsOfOtherShapesKeepTheirReason` (017 arm `=>` front, 017 lambda, 012, 020 in the arm) | FMT-UNSUP-021 | `AssertUnsupportedFor` | Passes except the 020 row |
| `ArmInDisabledTextIsNotChecked` | FMT-UNSUP-021 | both | Fails (X defined) / passes |
| `NodeThatHoldsTheArrowHoldsTheShape` (arm and switch refused, expression with leading comment formatted) | FMT-UNSUP-003, FMT-UNSUP-021, FC-041 | direct | Fails |
| `FindUnsupportedSyntaxAnswersForABuiltNode` | FMT-UNSUP-021 | direct | Fails |
| `OutputOfASwitchExpressionIsNotUnsupported` | FMT-UNSUP-005, FMT-UNSUP-021, FC-038 | format twice | Passes |
| **New: `.../Unit/Precheck/UnsupportedNameEqualsCommentTests.cs`** | | | |
| `SpecExampleUsingAliasAndAttributeArgumentAreUnsupported` (2,14) | FMT-UNSUP-022 | `AssertUnsupportedFor` + location | Fails |
| `SpecExampleAttributeArgumentAloneIsUnsupported` (10,15) | FMT-UNSUP-022 | same | Fails |
| `SpecExampleCommentsOutsideTheShapeAreSupported` | FMT-UNSUP-022 | `AssertSupported` + `AssertRuleResult` | Passes |
| `UsingAliasIsUnsupported` (one line, own line, `/** */`, `#if`, `#pragma`, `global`, block and file-scoped namespace, tuple alias) | FMT-UNSUP-022, FC-005 | same | Fails |
| `AttributeArgumentIsUnsupported` (method, own line, `/** */`, `#if`, `#pragma`, parameter, property, accessor, `return:`, behind a positional argument) | FMT-UNSUP-022 | same | Fails |
| `AttributeOnATypeOrTheAssemblyIsUnsupported` | FMT-UNSUP-022 | same | Fails |
| `DocumentationCommentBehindTheEqualsIsUnsupportedInEveryDocumentationMode` | FMT-UNSUP-022, FC-040 | same | Fails |
| `CommentOutsideTheGapIsSupported` | FMT-UNSUP-022, FC-003 | `AssertSupported` | Passes |
| `OtherShapesKeepTheirReason` (004 on a field's attribute, 010 directive in a multi-declarator field's attribute, 017 anonymous member, 020 property) | FMT-UNSUP-022 | `AssertUnsupportedFor` | Passes except the 020 row |
| `ShapeInDisabledTextIsNotChecked` | FMT-UNSUP-022 | both | Fails (X defined) / passes |
| `NodeThatHoldsTheEqualsHoldsTheShape` (argument, attribute, list, method, using directive refused; value and alias target formatted) | FMT-UNSUP-003, FMT-UNSUP-022, FC-041 | direct | Fails |
| `FindUnsupportedSyntaxAnswersForABuiltNode` | FMT-UNSUP-022 | direct | Fails |
| `OutputOfTheFormatterIsNotUnsupported` (FMT-BREAK-015 join behind the `=`, attribute list split, using ordering) | FMT-UNSUP-005, FMT-UNSUP-022, FC-038 | format twice | Passes |
| **Changed existing formatter tests** | | | |
| `UnsupportedSyntaxEntryPointTests.SpecExampleTargetBelowTheDeclarationIsFormattedWhileTheDeclarationIsRefused` (new FMT-UNSUP-003 example: object creation formatted, clause refused with 020, statement and document with 004) | FMT-UNSUP-003 | entry points | Fails (clause) |
| `UnsupportedSyntaxEntryPointTests.FindUnsupportedSyntaxAnswersForTheNodeAlone` (clause now 020, value null) | FMT-UNSUP-003, FMT-UNSUP-007 | direct | Fails |
| `UnsupportedSyntaxEntryPointTests.FindUnsupportedSyntaxGivesTheAnswerOfThePrecheck` (+3 rows: enum `=`, arm, using alias) | FMT-UNSUP-007 | direct | Passes |
| `UnsupportedVariableDeclarationTests.CommentIn{AParameterDefault,APropertyInitializer,AnEnumMemberValue}GivesTheReasonOfTheEqualsOfADeclaration` (were `...IsSupported`) | FMT-UNSUP-004 | `AssertUnsupportedFor` (020 reason) | Fails |
| `UnsupportedBinaryOperatorCommentTests.SpecExampleCommentsInFrontOfTheOperatorsAreSupported` (`n += /* c6 */ 1;` dropped, as the spec) | FMT-UNSUP-011 | `AssertSupported` | Passes |
| `UnsupportedBinaryOperatorCommentTests.CommentOutsideTheShapeIsSupported` (three assignment rows moved out) | FMT-UNSUP-011 | same | Passes |
| `UnsupportedBinaryOperatorCommentTests.CommentBehindAnAssignmentOperatorKeepsTheReasonOfItsOwnShape` (the three moved rows) | FMT-UNSUP-011 | `AssertUnsupportedFor` (020) | Fails |
| `UnsupportedExpressionBodyArrowCommentTests.SpecExampleCommentsOutsideTheGapAreSupported` (switch block dropped, as the spec) | FMT-UNSUP-012 | `AssertSupported` | Passes |
| `UnsupportedExpressionBodyArrowCommentTests.CommentBehindTheArrowOfASwitchArmGivesTheReasonOfTheArm` (was `...IsSupported`) | FMT-UNSUP-012, FMT-UNSUP-021 | `AssertUnsupportedFor` | Fails |
| `UnsupportedJoinedGapCommentTests.SpecExampleCommentsOutsideTheGapsAreSupported` (switch block dropped, as the spec) | FMT-UNSUP-017 | `AssertSupported` | Passes |
| `UnsupportedJoinedGapCommentTests.CommentOutsideTheGapsIsSupported` (arm row moved out) / `GapsOfOtherRequirementsKeepTheirReason` (+ arm row, 021) | FMT-UNSUP-017 | both | Passes / fails (new row) |
| `ReihitsuFormatterNodeColumnTests.ObjectCreationTargetBehindTheLastLineOfAMultiLineCommentIsLaidOutFromNewWhereItStands` (was `EqualsValueTarget...`; new FMT-PIPE-017 example, clause refused) | FMT-PIPE-017, FMT-INDENT-030, FMT-UNSUP-003 | `AssertFormatsTarget` | Fails (clause assertion only) |
| `ReihitsuFormatterNodeColumnTests.ChainTargetWithContext{BehindTheLastLineOfAMultiLineComment,HoldingAForeignLineBreak}AlignsTheWrappedLink` (context retargeted from the clause to its value) | FMT-PIPE-017, FMT-INDENT-029 | context entry point | Passes |
| `ContinuationLineIndentationTests.SpecExampleOperandAndArmExpressionKeptOnTheirOwnLineAreIndentedOneLevel`, `SpecExampleContinuationInsideACollectionElementIsIndentedFromTheElementsLine`, `SpecExampleContinuationOfAParenthesizedExpressionInsideALogicalExpressionIsIndentedFromTheStatementsLine`, `ContinuationOfAParenthesizedExpressionInsideAConditionIsIndentedFromTheStatementsLine`, `ContinuationOfAContinuationLineIsIndentedOneMoreLevel` (new FMT-INDENT-034 examples) | FMT-INDENT-034 | `AssertRuleResult` | Passes |
| `CommentLedFirstTokenAnchorTests.ComplexElementOfAFieldInitializerAlignsToItsBraceBehindCommentThatLeadsTheLine` (new FMT-INDENT-023 example) | FMT-INDENT-023, FMT-INDENT-030 | `AssertRuleResult` | Passes |
| `CommentLedFirstTokenAnchorTests.{NestedInitializerBehindCommentThatLeadsTheLine,NestedInitializerBelowCommentOnTheLineAbove,ArrayInitializerOfAPropertyBehindCommentThatLeadsTheLine,ArrayInitializerOfAPropertyBelowCommentOnTheLineAbove}IsUnsupported` | FMT-UNSUP-020 | `AssertUnsupportedFor` | Fails |
| `CommentLedFirstTokenAnchorTests` — the eleven `_ =⏎/* c */ X` anchor tests (collection expression ×2, object/collection creation, anonymous object, switch, lambda, binary, conditional, chain, pattern combinator), carrier now `_ = (⏎/* c */ X…)` | unchanged tags (FMT-INDENT-018..030) | `AssertRuleResult` | Passes |
| `GluedCommentBlankLineTests.RemovesAuthorBlankLineAboveCommentGluedToInitializerBrace`, `KeepsCommentsGluedToBracesWithoutBlankLine`, `DecidesBlankLinesAboveGluedCommentAsDirectlyBeforeItsToken` (new FMT-BLANK-018 examples) | FMT-BLANK-018 (FMT-BLANK-011) | `AssertRuleResult` | Passes |
| `GluedCommentBlankLineTests.InsertsNoBlankLineAboveCommentGluedToInitializerBrace`, `InsertsNoBlankLineAboveCommentGluedToArrayInitializerBraceOrArgumentListParenthesis`, `MovesBlankLineAboveOwnLineCommentInFrontOfCommentGluedToOpeningBrace` (carrier `new P⏎/* c */ {`, `return new int[]⏎/* b */ {`) | FMT-BLANK-018 (FMT-BLANK-011) | `AssertRuleResult` | Passes |
| `MultiLineGluedCommentBlankLineTests.{InsertsNoBlankLineAboveMultiLineCommentsGluedTo,DecidesAuthorBlankLinesAboveMultiLineCommentsGluedTo}ArrayBraceMemberStatementOrElse` (array carrier now `return new int[]`) | FMT-BLANK-018 (FMT-BLANK-011) | `AssertRuleResult` | Passes |
| `MultiLineGluedCommentBlankLineTests.MultiLineCommentBehindAssignment{,AndBlankLine}GluedToCollectionInitializerBraceIsUnsupported` | FMT-UNSUP-020 | `AssertUnsupportedFor` | Fails |
| `CommentBeforeBraceBlankLineTests.RemovesBlankLineBehindCommentLineAndBeforeOneLineComplexElement` (new FMT-BLANK-020 example) | FMT-BLANK-020 | `AssertRuleResult` | Passes |
| `CommentBeforeBraceBlankLineTests.{CommentBehindPropertyInitializerEqualsInFrontOfArrayInitializerBrace,CommentAndBlankLineBehindAssignmentInFrontOfCollectionInitializerBrace,CommentBehindAssignmentInFrontOfCollectionInitializerBrace}...Unsupported` | FMT-UNSUP-020 | `AssertUnsupportedFor` | Fails |
| `BlankLineBehindDirectiveBeforeTokenTests.DirectiveBehind...CollectionInitializerBrace...IsUnsupported` ×3 | FMT-UNSUP-020 | `AssertUnsupportedFor` | Fails |
| `DelimitedDocumentationInExpressionTests.DelimitedDocumentationBehindAssignmentOperatorIsUnsupported` (was `Moves...`) | FMT-UNSUP-020, FC-040 | `AssertUnsupportedFor` | Fails |
| `ParameterContinuationLineAlignmentTests.DefaultValueBehindEqualsAndLineCommentIsUnsupported` | FMT-UNSUP-020 | `AssertUnsupportedFor` | Fails |
| `ArrowSpacingTests.CommentBehindTheArrowOfASwitchArmIsUnsupported` (was `CommentBehindTheArrowKeepsItsPlace`) | FMT-UNSUP-021 | `AssertUnsupportedFor` | Fails |
| `CommentJoinTests.CommentBehindAssignmentOperatorIsRefusedBeforeTheJoin` / `CommentBehindVariableInitializerEqualsIsRefusedBeforeTheJoin` (deleted special case, rewritten to the refusal) | FMT-UNSUP-020 | `FormatSyntaxTree` | Fails / passes (004 reason) |
| `DirectiveJoinTests.DirectiveBehind{AssignmentOperator,VariableInitializerEquals}IsRefusedBeforeTheJoin` (deleted special case) | FMT-UNSUP-020 | `FormatSyntaxTree` | Fails |
| `JoinSeparatorPhaseTests.CommentInTheGapStillRefusesEveryJoin` (`var x = // x` case dropped: its join refusal is deleted) | FMT-BREAK-031 | phase alone | Passes |
| `LeadingTriviaMultiLineDetectionTests.TernaryWithLeadingCommentLineStaysSingleLine` (carrier `var x = (⏎// note⏎cond ? a : b);`) | — | phase alone | Passes |
| `AccessorListAccessorLineBreakTests.CommentInsideFollowingAccessorIsPreserved` (comment moved to `value /* c */;`) | FMT-BREAK-025 | `AssertRuleResult` | Passes |
| `ArgumentContinuationLineAlignmentTests.HeldLineInside{ObjectInitializer,SwitchExpression}Argument...` (carrier `( // keep⏎value)`) | FMT-INDENT-013, FMT-INDENT-034 | `AssertRuleResult` | Passes |
| **Code fixes (`tests/Reihitsu.Analyzer.Test`)** | | | |
| `RH7101DoNotCombineFieldsCommentAndDirectiveTests.VerifyNoFixIsOfferedForAShapeTheFormatterRefusesInTheType` (was `...OfTheJoinedGapsInTheType`; + property `=` LF/CRLF, parameter default, `+=`, local declarator directive, nested enum, arm, attribute argument LF/CRLF) | RH7101-007, SHARED-FIX-010, FC-043 | `VerifyNoCodeFixIsOffered` | New rows fail |
| `RH2001...TriviaTests.VerifyNoFixIsOfferedForACommentBehindAnObjectInitializerMembersEquals`, `...BehindTheArrowOfASwitchArm`, `...ForADirectiveBehindTheInitializersEquals` | RH2001-007 | same | Fail |
| `RH3202...Tests.VerifyNoFixIsOfferedForACommentBehindAnAssignmentOperatorInTheExpressionBody` (`=`, `+=`), `...BehindTheEqualsOfAnAttributeArgumentOfTheMethod` | SHARED-FIX-010, FMT-UNSUP-020 / FMT-UNSUP-022 | same | Fail |
| `RH3203...Tests.VerifyNoFixIsOfferedForACommentBehindAnAssignmentOperatorInTheExpressionBody` (two constructors, `=` and `+=`) | SHARED-FIX-010, FMT-UNSUP-020 | `VerifyNoCodeFixIsOffered` | Fails |
| `RH5206...Tests.VerifyNoFixIsOfferedForACommentBehindTheArrowOfAnArm` | SHARED-FIX-010, FMT-UNSUP-021 | same | Fails |
| `RH5301...AnalyzerTests.VerifyNoFixIsOfferedForACommentBehindTheEqualsOfAMember` | SHARED-FIX-010, FMT-UNSUP-020 | same | Fails |
| `RH5304...Tests.VerifyNoFixIsOfferedFor{A,ALine}CommentBehindTheEqualsOfTheMember` (fixed on `main`; the `// c` one is FC-009 `no-progress` on `main`) | SHARED-FIX-010, FMT-UNSUP-020, FC-009 | same | Fail |
| `RH5111...AnalyzerTests.VerifyNoFixIsOfferedForAnEnumMemberBesideACommentBehindASiblingsEquals` | SHARED-FIX-010, FMT-UNSUP-020 | same | Fails |
| `RH5201MethodChainsShouldBeAlignedFirstCallTests.NoFixIsOfferedForACommentBehindTheAssignmentOperatorOfTheStatement` (chain helper formats the statement) | SHARED-FIX-010, FMT-UNSUP-020, FC-043 | same | Fails |
| `RH5301...AnalyzerTests.VerifyFixIsOfferedForAnObjectCreationBehindACommentedAssignmentOperator` | SHARED-FIX-010, FMT-UNSUP-003, FC-041 | `VerifyCodeFixWithEndOfLine` | Passes |
| `RH5303...Tests.VerifyFixIsOfferedForACollectionCreationBehindACommented{PropertyInitializerEquals,AssignmentOperator}` | SHARED-FIX-010, FMT-UNSUP-003, FC-041 | same | Passes |
| `RH5305...Tests.VerifyFixIsOfferedForACollectionExpressionBehindACommentedPropertyInitializerEquals` (change doc example) | SHARED-FIX-010, FMT-UNSUP-003, FC-041 | same | Passes |
| `RH5201MethodChainsShouldBeAlignedAnalyzerTests` — four chain-behind-comment fix tests, `RH5112...Tests.VerifyFixOfChainBehindLineLeadingCommentMatchesTheFormatter`, `RH5301/RH5303/RH5305 ...FormatterTests.VerifyFormatterAligns...BehindCommentThatLeadsTheLine` (carrier `_ = (`) | unchanged tags | `VerifyCodeFixWithEndOfLine` / `VerifyFormatter` | Pass |
| `RH5302/RH5308 ...Tests.VerifyFixIndentsAContinuationOnTheRootsFirstLineFromTheDocumentsLineStart` (new FMT-INDENT-034 RH5302/RH5308 examples) | FMT-INDENT-034, FMT-PIPE-024 | `ApplyCodeFixAsync` | Pass |

## Not covered, and why

- **CLI per input** (issue goal): decision 4. CLI-019's end-to-end tests (`tests/Reihitsu.Cli.Test/EndToEnd/CliUnsupportedSyntaxEndToEndTests.cs`) pin the CLI path for every reason; the CLI suite passes unchanged against the prototype. The reason strings are pinned by the precheck tests.
- **A code-fix test per input** (issue goal): decision 4. One test per fix whose result changes, plus the still-offered RH5301, RH5303, RH5305 cases.
- **Using alias and code fixes**: no fix formats a node holding a using directive except through `OrganizeUsingDirectivesInDocumentAsync`, which runs no precheck (change doc, FMT-PIPE-030). Not tested as withheld.
- **Throw sites of `SyntaxNotSupportedException`**: the change adds no phase-level refusal (FMT-UNSUP-006); none to test.
- **FMT-UNSUP-019**: retired; its tests are folded into `UnsupportedAssignmentCommentTests` with the new reason.
- **Fix All / one-pass convergence (FC-007, FC-008)**: the new code-fix tests are no-fix tests; the still-offered ones go through `Verify`, which runs one pass, re-analysis, and the class's Fix All scenario.
- **FC-006, FC-018, FC-022–FC-026, FC-029, FC-036**: no rewrite, rename, literal, or member-kind logic is added; the change is a refusal plus deleted joins.
- **FC-010–FC-012**: covered by the closure tests (format twice) and `AssertRuleResult`'s phase stability check on every supported example.
- **FC-013, FC-014**: no analyzer changes; RH5111 keeps reporting (pinned by the RH5111 no-fix test, which marks both diagnostics).
- **FC-044**: the shapes are syntactic; nothing models a computed result.
- **`LineBreakPhaseBlankLineTests.KeepsBlankLinesAboveBlockStatementAndGluedComment`** (`Child =⏎⏎/* c */ {`, phase alone): left unchanged. It pins the FMT-BREAK-030 example, which the spec still shows (see *Follow-up candidates*); it passes against the prototype.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~UnsupportedAssignmentCommentTests|FullyQualifiedName~UnsupportedSwitchArmExpressionCommentTests|FullyQualifiedName~UnsupportedNameEqualsCommentTests|FullyQualifiedName~UnsupportedSyntaxEntryPointTests|FullyQualifiedName~UnsupportedVariableDeclarationTests|FullyQualifiedName~UnsupportedBinaryOperatorCommentTests|FullyQualifiedName~UnsupportedExpressionBodyArrowCommentTests|FullyQualifiedName~UnsupportedJoinedGapCommentTests"
scripts/test.sh --project formatter --filter "FullyQualifiedName~CommentLedFirstTokenAnchorTests|FullyQualifiedName~ContinuationLineIndentationTests|FullyQualifiedName~GluedCommentBlankLineTests|FullyQualifiedName~MultiLineGluedCommentBlankLineTests|FullyQualifiedName~CommentBeforeBraceBlankLineTests|FullyQualifiedName~BlankLineBehindDirectiveBeforeTokenTests|FullyQualifiedName~ReihitsuFormatterNodeColumnTests|FullyQualifiedName~CommentJoinTests|FullyQualifiedName~DirectiveJoinTests|FullyQualifiedName~JoinSeparatorPhaseTests|FullyQualifiedName~LeadingTriviaMultiLineDetectionTests|FullyQualifiedName~AccessorListAccessorLineBreakTests|FullyQualifiedName~ArrowSpacingTests|FullyQualifiedName~ArgumentContinuationLineAlignmentTests|FullyQualifiedName~ParameterContinuationLineAlignmentTests|FullyQualifiedName~DelimitedDocumentationInExpressionTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7101DoNotCombineFieldsCommentAndDirectiveTests|FullyQualifiedName~RH2001PrivateAutoPropertiesShouldNotBeUsedTriviaTests|FullyQualifiedName~RH3202|FullyQualifiedName~RH3203|FullyQualifiedName~RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests|FullyQualifiedName~RH5112|FullyQualifiedName~RH5201|FullyQualifiedName~RH5206|FullyQualifiedName~RH5301|FullyQualifiedName~RH5302|FullyQualifiedName~RH5303|FullyQualifiedName~RH5304|FullyQualifiedName~RH5305|FullyQualifiedName~RH5308"
scripts/test.sh --project architecture
```

Self-hosting against the prototype: every formatter self-hosting test passes, so no repository source, the new test files included, holds a declared shape; the two failures were `WorkingDocumentsDirectoryContainsNoFiles` (this run's `sdd/work/`) and RH2111 on the prototype's own `PrecheckPhase` (516 lines), neither from these tests. The implementer should expect RH2111 on `PrecheckPhase` if the new checks go into that type.

On this commit: formatter 181 failing (every one a refusal or a withheld fix listed above as "fails"), the changed analyzer classes 33 failing (all `Expected collection to be empty`: a fix is still offered), architecture 198 passed.

## Follow-up candidates

- **Spec examples that still show refused input** (FC-042; for the spec author, found by formatting every `csharp` block of `sdd/specs` with the prototype):
  - `sdd/specs/formatter/line-breaks.md`, FMT-BREAK-030 example (`Child =⏎⏎/* c */ {`, around line 2225): a phase-alone example, still true for the phase alone, but its closing sentence ("In the whole pipeline, FMT-BLANK keeps the first of these blank lines and removes the second") no longer holds — the whole pipeline refuses the input. FMT-BLANK-018 replaced the same carrier with `new P⏎⏎/* c */ {`; the same replacement fits here. `LineBreakPhaseBlankLineTests.KeepsBlankLinesAboveBlockStatementAndGluedComment` pins the example and would follow it.
  - `sdd/specs/formatter/indentation.md`, FMT-INDENT-030: "the opening brace of a complex element initializer or of a nested initializer (FMT-INDENT-023)" — the nested-initializer case now has no whole input that reaches it (FMT-INDENT-023 says so itself).
- `sdd/specs/analyzer/RH5042.md` RH5042-002 example: refused by the formatter (now with FMT-UNSUP-022's reason for its first comment); analyzer-only example, no action needed (P11), noted only because the scan flagged it.
- The `int P { get; } /** c */ = 1;` relocation candidate stays as `00-lead.md` records it; `OutputOfJoinsBehindTheEqualsOfADeclarationIsNotUnsupported` pins that its output is not refused.
