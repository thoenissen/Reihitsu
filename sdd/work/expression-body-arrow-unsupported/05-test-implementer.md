Role: test implementer
Commit: 752469f
Verdict: tests written

Inputs: thoenissen/Reihitsu#1851, `00-lead.md` (batch-1 answers 1A, 2A; decisions 1–6), `01`–`04` handovers, the accepted change doc `sdd/changes/2026-11/expression-body-arrow-leading-comment-unsupported.md`, and the spec diff (FMT-UNSUP-013, FMT-UNSUP-012, FMT-STRUCT-006/007/015/026, SHARED-BODY-001/003, RH5401-004, RH7101-007). My prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the spec and change-doc examples; production code is unchanged (`main`'s behavior), so every refusal test fails today and every "supported" or closure test passes today.

How existing tests were found: a scratch Roslyn scanner (scratchpad, not committed) parsed every string literal in every test project, also wrapped in a class, under `DocumentationMode.Parse` with and without `DEBUG`, and listed every literal that holds a comment, directive, or disabled text in the trailing trivia of the token in front of an expression body's `=>` or in the `=>`'s leading trivia (lambdas and switch arms excluded). It also scanned every repository `.cs` file as code: none holds the shape, so `FormatterSelfHostingTests` is not affected.

## Tests

New file `src/Reihitsu.Formatter.Test/Unit/Precheck/UnsupportedExpressionBodyArrowLeadingCommentTests.cs`; new constant `ExpressionBodyArrowLeadingCommentReason` in `src/Reihitsu.Formatter.Test/Helpers/FormatterTestsBase.cs`. Every `AssertUnsupportedFor`/`AssertSupported` call runs LF and CRLF; `AssertSupported` also asserts the second pass.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `UnsupportedExpressionBodyArrowLeadingCommentTests.SpecExampleCommentsInFrontOfTheArrowsOfExpressionBodiesAreUnsupported` (location `(5,11)`) | FMT-UNSUP-013 | `AssertUnsupportedFor` + `FormatSyntaxTree` | fail |
| `…EachExpressionBodyOfTheSpecExampleIsUnsupported` (6 rows) | FMT-UNSUP-013 | `AssertUnsupportedFor` | fail |
| `…SpecExampleDirectiveInFrontOfTheArrowIsUnsupportedInEveryBuild` (DEBUG on/off, `(4,1)`) | FMT-UNSUP-013 | `AssertUnsupportedFor` | fail |
| `…SpecExampleCommentsOutsideBothGapsAreSupported` | FMT-UNSUP-013, FMT-UNSUP-012 | `AssertSupported` | pass |
| `…CommentInFrontOfTheArrowOfEveryExpressionBodyIsUnsupported` (27 owners: method, async, three constraint kinds, ctor, ctor with initializer, static ctor, finalizer, binary/unary operator, implicit/explicit conversion, property, static property, indexer, explicit interface, get/set/init, indexer getter, add/remove, local function, async/static/generic-constrained local function) | FMT-UNSUP-013; FC-025 | `AssertUnsupportedFor` | fail |
| `…CommentInFrontOfTheArrowOfALambdaOrASwitchArmIsSupported` (6 rows) | FMT-UNSUP-013 | `AssertSupported` | pass |
| `…EveryCommentPositionInTheGapIsUnsupported` (15 rows: trailing, own line, blank line before/after, at the arrow, column 0, one line, no whitespace, tab, two comments, multi-line block) | FMT-UNSUP-013; FC-021, FC-027 | `AssertUnsupportedFor` | fail |
| `…DocumentationCommentInFrontOfTheArrowIsUnsupportedInEveryDocumentationMode` (14 rows, Parse/None/Diagnose, `///` and `/** */`, trailing and own line, getter) | FMT-UNSUP-013; FC-040 | `AssertUnsupportedFor` | fail |
| `…EveryDirectiveInFrontOfTheArrowIsUnsupported` (10 rows: pragma, region, nullable, line, fully inactive `#if false`, comment before directive, property, indexer, getter, local function) | FMT-UNSUP-013; FC-005, FC-020 | `AssertUnsupportedFor` | fail |
| `…ConditionalGroupInFrontOfTheArrowIsUnsupportedInEveryBuild` (group choosing a method's arrow; group with disabled text; X on/off) | FMT-UNSUP-013; FC-020 | `AssertUnsupportedFor` | fail |
| `…ConditionalGroupsClosingInFrontOfASharedArrowAreUnsupported` (change 12 file, `(9,1)`) | FMT-UNSUP-013 | `AssertUnsupportedFor` + `FormatSyntaxTree` | fail |
| `…ConditionalGroupClosingInFrontOfASharedArrowIsUnsupported` (signature / constraint / property type, symbol on/off: `#endif`/`#else`, `#if`/`#endif`) | FMT-UNSUP-013; FC-020 | `AssertUnsupportedFor` | fail |
| `…ConditionalGroupAroundTheWholeMemberIsSupported` (symbol on/off) | FMT-UNSUP-013; FC-003 | `AssertSupported` | pass |
| `…CommentOutsideTheGapIsSupported` (12 rows: above member, doc comment above, directive above, inside parameter list at both ends, inside indexer brackets, inside constraint, behind expression/semicolon, lambda that is the expression, comment in front of `get`) | FMT-UNSUP-013; FC-003 | `AssertSupported` | pass |
| `…EveryNestedExpressionBodyIsCheckedWithItsOwnArrow` (8 rows, incl. field with one and several declarators, accessor body, nested type, local in local, interface, record) | FMT-UNSUP-013; FC-039 | `AssertUnsupportedFor` | fail |
| `…ExpressionBodyInDisabledTextIsNotChecked` (X off: supported; X on: refused) | FMT-UNSUP-013 | both | X off pass, X on fail |
| `…DirectiveInFrontOfTheArrowInAFieldWithSeveralDeclaratorsKeepsTheReasonOfTheFieldDeclaration` (X on/off) | FMT-UNSUP-010, FMT-UNSUP-013 | `AssertUnsupportedFor` | fail (FMT-UNSUP-010 half passes today; the one-declarator half fails) |
| `…FirstShapeInTheTextIsReported` (both gaps of one arrow → this reason at the earlier comment; ordering against FMT-UNSUP-012, -011, -004) | FMT-UNSUP-001, FMT-UNSUP-013 | `AssertUnsupportedFor` | fail |
| `…FormatDocumentAsyncRefusesTheShape` | FMT-UNSUP-013 | `FormatDocumentAsync` | fail |
| `…NodeLevelEntryPointsRefuseATargetThatHoldsTheShape` (`FormatNode`, `FormatNodeInDocumentAsync` on member and type, `…WithContextAsync`, method beside formatted) | FMT-UNSUP-013; FC-028 | entry points | fail |
| `…ArrowClauseAloneDoesNotHoldTheShape` (FindUnsupportedSyntax on member/root report; on arrow clause, `FormatNode`, `FormatNodeInDocumentAsync` do not) | FMT-UNSUP-013, FMT-UNSUP-003; FC-041 | entry points | fail (member/root half) |
| `…FindUnsupportedSyntaxAnswersForAnAccessor` | FMT-UNSUP-013, FMT-UNSUP-003; FC-041 | `FindUnsupportedSyntax` | fail |
| `…FindUnsupportedSyntaxAnswersForABuiltNode` | FMT-UNSUP-003, FMT-UNSUP-013; FC-043 | `FindUnsupportedSyntax` | fail |
| `…FindUnsupportedSyntaxGivesTheAnswerOfThePrecheck` (5 rows) | FMT-UNSUP-013 | `FindUnsupportedSyntax` vs `FormatSyntaxTree` | pass (both agree today) |
| `…OutputOfFormattedExpressionBodiesIsNotUnsupported` (21 rows: skeptic's FMT-UNSUP-005 probe V1–V12, plus doc comments around the accessor list, a directive between `get` and its block, joined property/indexer arrows, comment above a converted method) | FMT-UNSUP-005, FMT-UNSUP-013; FC-038, FC-010 | `FormatSyntaxTree` twice, LF/CRLF | pass |
| `ExpressionBodyArrowCommentTests.CommentsInFrontOfTheArrowOfAConvertedMemberAreUnsupported` (15 rows: every former FMT-STRUCT-006 own-line/at-arrow test input, rewritten to the refusal) | FMT-UNSUP-013 | `AssertUnsupportedFor` | fail |
| `ExpressionBodyArrowCommentTests.CommentAboveTheMemberStaysAboveTheConvertedMember` (change 5's alternative, exact output) | FMT-UNSUP-013 | `AssertRuleResult` | pass |
| `ExpressionBodiedConstructorTests.CommentBeforeArrowIsUnsupported`, `ExpressionBodiedMethodTests.CommentBeforeArrowIsUnsupported` (were `PreservesCommentBeforeArrow`, FMT-STRUCT-006) | FMT-UNSUP-013 | `AssertUnsupportedFor` | fail |
| `ExpressionBodiedIndexerTests.LineCommentAfterClosingBracketIsUnsupported`, `.DirectiveBeforeArrowIsUnsupported` (were `…KeepsArrowOnOwnLine`) | FMT-UNSUP-013 | `AssertUnsupportedFor` | fail |
| `ExpressionBodyDirectiveGuardTests.MethodWithRegionBeforeArrowIsUnsupported` (was `…IsNotConverted`, FMT-STRUCT-007) | FMT-UNSUP-013 | `AssertUnsupportedFor` | fail |
| `GetOnlyMemberExpressionBodyTests.CommentAfterGetKeywordIsUnsupported`, `.CommentBeforeGetterArrowIsUnsupported`, `.DirectiveBeforeGetterArrowIsUnsupported` (were `…KeepsAccessorList`, FMT-STRUCT-015) | FMT-UNSUP-013 | `AssertUnsupportedFor` | fail |
| `CliHiddenCodeEndToEndTests.CommentOrDirectiveInFrontOfTheArrowOfAnExpressionBodyIsReportedAsUnsupported` (11 rows: change doc 1–9, region, 12; exact error line, exit code 2, file unchanged) | FMT-UNSUP-013 | CLI `Program.Main` | fail |
| `RH3202…VerifyNoFixIsOfferedForACommentBeforeTheArrow` (was `VerifyCommentBeforeArrowLeadsTheCreatedStatement`; change 10, LF/CRLF) | SHARED-BODY-001, SHARED-BODY-003 | `VerifyNoCodeFixIsOffered` | fail |
| `RH3202…VerifyNoFixIsOfferedForABlockCommentBehindTheSignatureOnTheArrowsLine`, `…ForALineCommentBehindTheSignatureWhenTheArrowStartsTheNextLine` (were `FixKeeps…`) | SHARED-BODY-003 | `VerifyNoCodeFixIsOffered` | fail |
| `RH3202…SpecExampleConditionalGroupAroundTheSignatureIsReportedWithoutAFix` (LF/CRLF), `…WhereTheSymbolIsDefined`, `…ConditionalGroupAroundAConstraintClauseIsReportedWithoutAFix`, `…ConditionalGroupThatChoosesTheArrowIsReportedWithoutAFix` | SHARED-BODY-001, SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | fail (reported today, fix offered) |
| `RH3202…SpecExampleConditionalGroupAroundTheWholeMemberIsFixed` | SHARED-BODY-001 | `Verify` | pass |
| `RH3202…VerifyMethodWithARegionDirectiveInFrontOfTheArrowIsNotReported` | SHARED-BODY-001 | `Verify` | pass |
| `RH3202…VerifyNoFixIsOfferedForADirectiveInFrontOfTheArrow` | SHARED-BODY-003 | `VerifyNoCodeFixIsOffered` | fail |
| `RH3202…VerifyFixIsOfferedForAMethodBesideAMethodWithACommentInFrontOfItsArrow` (LF/CRLF; withheld one, applied one, re-verify) | SHARED-FIX-010; FC-043 | `GetCodeFixActionsAsync`, `TryApplyCodeFixAtAsync`, `Verify` | fail |
| `RH3203/RH3205/RH3206/RH3207/RH3208…VerifyNoFixIsOfferedForACommentInFrontOfTheArrow` (LF/CRLF each) | SHARED-BODY-003, SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | fail |
| `RH5110…VerifyNoFixIsOfferedForACommentInFrontOfTheArrow` (change 11, LF/CRLF) | SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | fail |
| `RH5401…VerifyFixIsWithheldForACommentOrDirectiveInFrontOfTheArrow` (line comment, block comment at the arrow, comment line, pragma; LF/CRLF) | RH5401-004 | `VerifyNoCodeFixIsOffered` | pass (withheld today for another reason; visible result unchanged, as RH5401-004 states) |
| `RH7101…CommentInFrontOfTheArrowOfAnExpressionBodyWithholdsTheFix` (change 11, LF/CRLF), `…ElsewhereInTheTypeWithholdsTheFix` (4 rows) | RH7101-007 | `VerifyNoCodeFixIsOffered` | fail |

Existing tests changed because their input now holds the shape (one per output the change doc changes):

| Test | Change |
|---|---|
| `UnsupportedExpressionBodyArrowCommentTests.SpecExampleCommentsOutsideTheGapAreSupported` | input rewritten to FMT-UNSUP-012's changed example (c1–c6) |
| `UnsupportedExpressionBodyArrowCommentTests.CommentOutsideTheGapIsSupported` | 6 rows with a comment/directive in front of the arrow removed (now refused; covered in the new file) |
| `UnsupportedExpressionBodyArrowCommentTests.OutputOfFormattedExpressionBodiesIsNotUnsupported` | 12 rows whose input holds the new shape removed (covered as refused in the new file's documentation-mode and position tests) |
| `ExpressionBodyArrowCommentTests` | the 13 FMT-STRUCT-006 own-line/at-arrow tests and their three private assertion helpers deleted, their inputs kept as rows of the refusal data test; the 4 rows of the FMT-UNSUP-012 data test that hold both gaps removed (they now report the earlier comment with FMT-UNSUP-013's reason); two of them (`Both`, `M() /* c */ =>⏎// a`) are asserted in `FirstShapeInTheTextIsReported` |
| `StructuralTransformSeparatorFinalOutputTests.CommentsBehindTheArrowOfTheRemovedExamplesAreUnsupported` | row `Get3` (both gaps) removed, now in `FirstShapeInTheTextIsReported` |
| `StructuralTransformSeparatorTests.OwnLineCommentMovedInFrontOfTheCreatedReturnKeepsLeadingIt` | deleted (tests the deleted comment move; its input is refused) |
| `StructuralTransformSeparatorTests.TokensCreatedForConvertedExpressionBodiesCarryNoElasticTrivia` | member `Other` with the moved comment removed |
| `JoinSeparatorPhaseTests.CommentInTheGapStillRefusesEveryJoin`, `.DirectiveInTheGapStillRefusesEveryJoin` | the property `P` with a comment/directive in front of its arrow removed: the line-break phase no longer meets it |
| `CommentJoinTests.ExpressionBodiedPropertyArrowIsNotCollapsedIntoTrailingComment`, `DirectiveJoinTests.ExpressionBodiedPropertyArrowIsNotCollapsedAcrossDirective` | deleted: phase-level tests of the deleted join refusal; the same inputs are refused in the new file (`Line comment behind the name of a property`, `Pragma directive in front of the arrow of a property`) |
| `AccessorListAccessorLineBreakTests.CommentInsideFollowingAccessorIsPreserved` | comment moved from `set /* c */ =>` to `_x = /* c */ value` (output checked on `main`) |
| `IdempotencyTests.ExpressionBodiedCommentTestData` | member `LeadingArrowComment` replaced by a comment above the member |
| `ExpressionBodiedConstructorTests`, `ExpressionBodiedMethodTests`, `ExpressionBodiedIndexerTests`, `ExpressionBodyDirectiveGuardTests`, `GetOnlyMemberExpressionBodyTests` | rewritten to the refusal (rows in *Tests* above) |

Left unchanged although their input holds the shape: `RH5040…AnalyzerTests` (8 literals; analyzer only, no code fix, analyzers do not consult the precheck), `RH3202…VerifyNoFixIsOfferedWhenTheArrowLineCarriesAComment` (still no fix), `RH5401…VerifyFixIsWithheldWhenPropertyStaysOnSeveralLines` (still no fix), `UnsupportedExpressionBodyArrowCommentTests.FindUnsupportedSyntaxGivesTheAnswerOfThePrecheck` row 4 (compares precheck with `FindUnsupportedSyntax`, holds either way).

Run results on this tree (production code = `main`): formatter project 126 failed / 5747 passed — every failure is an `AssertUnsupportedFor` refusal or an entry-point refusal assertion of the tests above ("must return a refused input unchanged", "must report the input as unsupported", reason mismatch); analyzer filter (RH3202–RH3208, RH5040, RH5110, RH5401, RH7101) 33 failed / 496 passed, every failure "No code fix may be offered"; CLI `CliHiddenCodeEndToEndTests` 11 failed / 26 passed, every failure exit code 0 instead of 2; architecture 198 passed (FMT-UNSUP-013 now covered; FMT-STRUCT-006, -007, -015 still covered). Release build warning-free.

## Not covered, and why

- FMT-UNSUP-006 phase throw sites: FMT-UNSUP-013 names no phase that refuses the shape, so no `SyntaxNotSupportedException` throw site exists to test.
- FC-007/FC-008 (convergence, Fix All): the changed fixes are withheld, not changed; `RH3202…VerifyFixIsOfferedForAMethodBeside…` verifies the fixable neighbor converges and the withheld one stays, and the existing Fix All scenarios hold no shape (scanner).
- FC-018 (Renamer), FC-022/FC-023/FC-026/FC-029/FC-036: no rename, literal, identifier, or key/value layout is involved.
- FC-012/FC-013: no phase is added; RH5401's and RH3202's reporting is unchanged (decision 1), asserted by the RH5401 and RH3202 tests.
- FC-006: the refusal changes no code.
- The spec author's documentation-mode axis for `///` behind `get` on its own line is covered in Parse mode only (one row); the trailing and own-line `///` rows of members cover all three modes.
- RH3205/RH3206/RH3207 one-line block comment and directive variants: one test per rule (comment behind the signature); they share `ExpressionBodyToBlockCodeFixProviderBase.GetFormattingInput` → `ExpressionBodyCodeFixUtilities.ConvertBodyAsync`, which RH3202 covers with all variants (`rg -n "ConvertBodyAsync" src/Reihitsu.Analyzer.CodeFixes`).

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~UnsupportedExpressionBodyArrowLeadingCommentTests|FullyQualifiedName~UnsupportedExpressionBodyArrowCommentTests|FullyQualifiedName~ExpressionBodyArrowCommentTests|FullyQualifiedName~ExpressionBodiedConstructorTests|FullyQualifiedName~ExpressionBodiedMethodTests|FullyQualifiedName~ExpressionBodiedIndexerTests|FullyQualifiedName~ExpressionBodyDirectiveGuardTests|FullyQualifiedName~GetOnlyMemberExpressionBodyTests|FullyQualifiedName~StructuralTransformSeparator|FullyQualifiedName~JoinSeparatorPhaseTests|FullyQualifiedName~CommentJoinTests|FullyQualifiedName~DirectiveJoinTests|FullyQualifiedName~AccessorListAccessorLineBreakTests|FullyQualifiedName~IdempotencyTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3202|FullyQualifiedName~RH3203|FullyQualifiedName~RH3205|FullyQualifiedName~RH3206|FullyQualifiedName~RH3207|FullyQualifiedName~RH3208|FullyQualifiedName~RH5110|FullyQualifiedName~RH5401|FullyQualifiedName~RH7101"
scripts/test.sh --project cli --filter "FullyQualifiedName~CliHiddenCodeEndToEndTests"
scripts/test.sh --project architecture
```

## Follow-up candidates

_None._
