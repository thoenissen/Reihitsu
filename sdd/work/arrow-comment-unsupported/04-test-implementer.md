Role: test implementer
Commit: 425afc1 (tests derived from the specs and the change doc at this commit; the test commit follows it)
Verdict: tests written

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

Every test that asserts a refusal, a withheld fix, or a CLI error fails on 425afc1 for the expected reason: the input is formatted or returned without a report ("must return a refused input unchanged" / "must report the input as unsupported"), the fix is offered ("No code fix may be offered"), or the CLI exits 0. Every test of behavior that stays, the "not refused" and closure cases included, passes on 425afc1. Results on 425afc1, Release: formatter 138 failed / 5597 passed, CLI 8 failed / 377 passed, analyzer (filter `RH320|RH5401|RH7101`) 23 failed / 465 passed, architecture 197 passed (FMT-UNSUP-012 is now covered).

Method note. To find every existing test whose input holds the shape, I ran the suites in a throwaway worktree (`git worktree` under the scratchpad, never committed, deleted afterwards). In that worktree the precheck threw on the shape, so every test whose input reaches `FindUnsupportedSyntax` with the shape failed. Tests that call a phase directly were found with `rg` instead. I also ran the new tests against a minimal precheck-only refusal in that worktree, to check fixture syntax and locations; all passed. The expected values come only from the spec examples, the change doc, and the requirements (FC-035).

## Tests

New file `src/Reihitsu.Formatter.Test/Unit/Precheck/UnsupportedExpressionBodyArrowCommentTests.cs` (`UnsupportedExpressionBodyArrowCommentTests`), and the constant `ExpressionBodyArrowCommentReason` in `src/Reihitsu.Formatter.Test/Helpers/FormatterTestsBase.cs`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `SpecExampleCommentsBehindTheArrowsOfExpressionBodiesAreUnsupported` | FMT-UNSUP-012 | `AssertUnsupportedFor` + location (5,14) | fail |
| `EachExpressionBodyOfTheSpecExampleIsUnsupported` (6 rows) | FMT-UNSUP-012 | `AssertUnsupportedFor` | fail |
| `SpecExampleDirectiveBehindTheArrowIsUnsupportedInEveryBuild` (DEBUG on/off) | FMT-UNSUP-012; FC-005, FC-020 | `AssertUnsupportedFor` + location (4,1) | fail |
| `SpecExampleCommentsOutsideTheGapAreSupported` | FMT-UNSUP-012; FC-003 | `AssertSupported` | pass |
| `CommentBehindTheArrowOfEveryExpressionBodyIsUnsupported` (24 rows: methods incl. `async` Task/Task<T>/ValueTask<T>, ctor, static ctor, finalizer, binary/unary operator, implicit/explicit conversion, property, static property, indexer, explicit interface impl., get/set/init/indexer-get/add/remove accessors, local function incl. `async`/`static`) | FMT-UNSUP-012; FC-025, FC-024 | `AssertUnsupportedFor` | fail |
| `CommentBehindTheArrowOfALambdaOrASwitchArmIsSupported` (5 rows) | FMT-UNSUP-012; FC-003 | `AssertSupported` | pass |
| `EveryCommentPositionInTheGapIsUnsupported` (11 rows: same line `//`, `/* */` at line end, own line, behind blank line, column 0, two comments, multi-line block, no whitespace, tab) | FMT-UNSUP-012; FC-021 | `AssertUnsupportedFor` | fail |
| `DocumentationCommentBehindTheArrowIsUnsupportedInEveryDocumentationMode` (12 rows, None/Parse/Diagnose) | FMT-UNSUP-012; FC-040 | `AssertUnsupportedFor` | fail |
| `EveryDirectiveBehindTheArrowIsUnsupported` (9 rows: pragma, region, nullable, line, `#if false`, if/elif/else, comment in front of directive, property, getter) | FMT-UNSUP-012; FC-005, FC-020 | `AssertUnsupportedFor` | fail |
| `DisabledTextBehindTheArrowIsUnsupportedInEveryBuild` (X on/off) | FMT-UNSUP-012; FC-020 | `AssertUnsupportedFor` | fail |
| `CommentInFrontOfTheExpressionsFirstTokenIsUnsupported` (8 rows: ref, throw, paren, unary, new, await, lambda, binary) | FMT-UNSUP-012; FC-003 | `AssertUnsupportedFor` | fail |
| `CommentOutsideTheGapIsSupported` (15 rows: above member, behind name, own line / same line in front of arrow, directive above arrow, behind ref/throw/unary, inside parens/arguments, before/after `;`, lambda-as-expression arrow, behind `get`) | FMT-UNSUP-012; FC-003 | `AssertSupported` | pass |
| `EveryNestedExpressionBodyIsCheckedWithItsOwnArrow` (7 rows: local fn in lambda in property body / field with one and with several declarators, nested type accessor, local fn in local fn, nested interface, nested record) | FMT-UNSUP-012; FC-039 | `AssertUnsupportedFor` | fail |
| `ExpressionBodyInDisabledTextIsNotChecked` (X on: refused / X off: supported) | FMT-UNSUP-012; FC-020 | both | on: fail, off: pass |
| `DirectiveBehindTheArrowInAFieldWithSeveralDeclaratorsKeepsTheReasonOfTheFieldDeclaration` (X on/off) | FMT-UNSUP-010, FMT-UNSUP-012; FC-039 | `AssertUnsupportedFor` | fail |
| `FirstShapeInTheTextIsReported` | FMT-UNSUP-001, FMT-UNSUP-012 | `AssertUnsupportedFor` | fail |
| `FormatDocumentAsyncRefusesTheShape` | FMT-UNSUP-012 | direct, LF/CRLF | fail |
| `NodeLevelEntryPointsRefuseATargetThatHoldsTheShape` (`FormatNode`, `FormatNodeInDocumentAsync` on member and on arrow clause, context entry point; neighbour formatted) | FMT-UNSUP-012; FC-028 | direct, LF/CRLF | fail |
| `FindUnsupportedSyntaxAnswersForTheArrowClauseAndNotForTheExpressionAlone` (arrow clause, member, root reported; expression not, as `FindUnsupportedSyntax`, `FormatNode`, and target) | FMT-UNSUP-012, FMT-UNSUP-003; FC-041 | direct, LF/CRLF | fail |
| `FindUnsupportedSyntaxAnswersForABuiltNode` | FMT-UNSUP-003, FMT-UNSUP-012; FC-041, FC-043 | direct | fail |
| `FindUnsupportedSyntaxGivesTheAnswerOfThePrecheck` (5 rows) | FMT-UNSUP-012 | direct | pass (agreement holds today, and must keep holding) |
| `OutputOfFormattedExpressionBodiesIsNotUnsupported` (20 rows incl. doc-comment modes; skeptic's getters A–H, setter block, doc comments in front of arrows, blank line behind arrow, own-line comment above method arrow) | FMT-UNSUP-005, FMT-UNSUP-012; FC-038, FC-040, FC-010 | two passes, LF/CRLF | pass |

Formatter tests changed (each existing test whose input holds the shape; found by the throwing probe and `rg`):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ExpressionBodyArrowCommentTests.CommentsInFrontOfTheArrowLeadTheCreatedStatement` (was `CommentsAroundTheArrowLeadTheCreatedStatement`; the new FMT-STRUCT-006 example) | FMT-STRUCT-006 | `AssertRuleResult` | pass |
| `ExpressionBodyArrowCommentTests.CommentsBehindTheArrowOfAConvertedMemberAreUnsupported` (13 rows; replaces the 11 removed tests listed under *Not covered*, plus two rows) | FMT-UNSUP-012 | `AssertUnsupportedFor` | fail |
| `ExpressionBodyArrowCommentTests.BlankLineBetweenTwoCommentsBeforeArrowIsKept` (was `...AfterArrowIsKept`; moved in front of the arrow to keep the FMT-PIPE-013 case) | FMT-STRUCT-006, FMT-PIPE-013 | `AssertRuleResult` | pass |
| `ExpressionBodyDirectiveGuardTests.DirectiveBetweenTheArrowAndTheExpressionIsUnsupported` (11 rows, DEBUG on and off; replaces the 11 `...WithDirectiveBeforeExpressionIsNotConverted`/pragma/nullable/line/region tests) | FMT-UNSUP-012 | `AssertUnsupportedFor` | fail |
| `ExpressionBodyDirectiveGuardTests.MethodWithInlineCommentBehindTheArrowIsUnsupported` (was `MethodWithInlineCommentIsStillConverted`, which asserted `{/* inline */`) | FMT-UNSUP-012 | `AssertUnsupportedFor` | fail |
| `ExpressionBodyConversionCommentPlacementTests.CommentBehindABlankLineBehindTheArrowIsUnsupported` (3 rows; replaces `PlacesCommentOnOwnLineInConverted{Method,Constructor,LocalFunction}Body`) | FMT-UNSUP-012 | `AssertUnsupportedFor` | fail |
| `ExpressionBodiedIndexerTests.InlineCommentAfterArrowIsUnsupported`, `.DirectiveAfterArrowIsUnsupported` (were `InlineCommentAfterArrowRemainsInPlace`, `DirectiveAfterArrowKeepsExpressionOnOwnLine`) | FMT-UNSUP-012 | `AssertUnsupportedFor` | fail |
| `ExpressionBodiedMethodTests.CommentAfterArrowIsUnsupported` (was `PreservesCommentAfterArrow`, `{// why`) | FMT-UNSUP-012 | `AssertUnsupportedFor` | fail |
| `ExpressionBodyNextLineExpressionTests.{LineCommentTrailingArrow…,BlockCommentTrailingArrow…}WithExpressionOnNextLineIsUnsupported`, `.BlockCommentLeadingNextLineExpressionIsUnsupported`, `.LineCommentBeforeNextLineExpressionIsUnsupported` (4; were `…ConvertsInOnePass`, `…JoinsReturnKeyword`, `…KeepsOwnLine`) | FMT-UNSUP-012 | `AssertUnsupportedFor` | fail |
| `GetOnlyMemberExpressionBodyTests.CommentAfterGetterArrowIsUnsupported`, `.CommentBeforeGetterExpressionIsUnsupported`, `.DirectiveBeforeGetterExpressionIsUnsupported` (were `…KeepsAccessorList`, tagged FMT-STRUCT-015) | FMT-UNSUP-012 | `AssertUnsupportedFor` | fail |
| `StructuralTransformSeparatorFinalOutputTests.CommentsBehindTheArrowOfTheRemovedExamplesAreUnsupported` (10 rows; replaces `IndentedCommentMovedBehindReturnEndsUpOneSpaceBehindIt`, `CommentAtColumnZeroMovedBehindReturnFollowsItWithoutASpace`, `MembersWithACommentAtTheArrowReachAStableOutput`, and carries the inputs of the deleted unit separator tests) | FMT-UNSUP-012 | `AssertUnsupportedFor` | fail |
| `StructuralTransformSeparatorTests.OwnLineCommentMovedInFrontOfTheCreatedReturnKeepsLeadingIt` (comment moved in front of the arrow), `.TokensCreatedForConvertedExpressionBodiesCarryNoElasticTrivia` (third member's comment moved in front of the arrow) | FMT-STRUCT-026 | phase observer | pass |
| `StructuralTransformRequirementTests.ExpressionBodyWhoseConversionWouldSplitAConditionalGroupIsKept` (DEBUG on/off; the new FMT-STRUCT-024 example; replaces `ExpressionBodyWhoseConversionWouldNotCompileIsLeftUnchanged`) | FMT-STRUCT-024 | `FormatSyntaxTree` + `AssertSecondPassIsStable` | pass |
| `CommentJoinTests.ExpressionBodiedPropertyArrowIsNotCollapsedIntoTrailingComment`, `DirectiveJoinTests.ExpressionBodiedPropertyArrowIsNotCollapsedAcrossDirective` (were the behind-the-arrow cases; now the kept in-front join refusal) | FMT-BREAK-001; FC-003 | line-break phase alone | pass |
| `IdempotencyTests.ExpressionBodiedCommentIsIdempotent` (fixture member `TrailingArrowComment() => /* keep me */ 42;` replaced by `LeadingArrowComment()` with the comment on a line of its own in front of the arrow; otherwise the whole fixture would be refused and the other members untested) | — (FMT-PIPE-004 tag unchanged) | `AssertIdempotentUnderBothEndings` | pass |

CLI and analyzer tests:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `CliHiddenCodeEndToEndTests.CommentOrDirectiveBehindTheArrowOfAnExpressionBodyIsReportedAsUnsupported` (8 rows = change-doc changes 1–8, with their line/column) | FMT-UNSUP-012 | `Program.Main`, exit code, stderr line, file unchanged | fail |
| `RH3202…Tests.VerifyNoFixIsOfferedForACommentBehindTheArrow` (change 9, LF/CRLF) | SHARED-BODY-001, SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | fail |
| `RH3202…Tests.VerifyNoFixIsOfferedForABlockCommentBehindTheArrowOnOneLine` | SHARED-BODY-001, SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | fail |
| `RH3202…Tests.VerifyFixIsOfferedForAMethodBesideAMethodWithACommentBehindItsArrow` (two methods on one line: A offered, B not) | SHARED-FIX-010; FC-043, FC-030 | `GetCodeFixActionsAsync` | fail (B is offered today) |
| `RH3202…Tests.VerifyNoFixIsOfferedForACommentLineBehindTheArrow` (was `VerifyCommentAfterArrowLeadsTheCreatedStatement`, SHARED-BODY-003) | SHARED-BODY-001, SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | fail |
| `RH3202…Tests.VerifyNoFixIsOfferedWhenTheArrowLineCarriesAComment` (LF/CRLF; was `VerifyFixKeepsCommentOrderWhenTheArrowLineCarriesAComment`, SHARED-BODY-003) | SHARED-BODY-001, SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | fail |
| `RH3203…Tests.VerifyNoFixIsOfferedForACommentTrailingTheArrow` (change 9's RH3203 input, LF/CRLF) | SHARED-BODY-001, SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | fail |
| `RH3205/RH3206/RH3207/RH3208…Tests.VerifyNoFixIsOfferedForACommentTrailingTheArrow` (LF/CRLF each) | SHARED-BODY-001, SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | fail |
| `RH3205/RH3206/RH3207/RH3208…Tests.VerifyCommentTrailingTheSemicolonSurvivesFix` (the kept half of the removed `VerifyCommentsAroundExpressionBodySurviveFix`, which asserted `{// why`) | — (untagged, as before) | `Verify` | pass |
| `RH5401…Tests.VerifyFixIsWithheldWhenPropertyStaysOnSeveralLines` (spec example; new member `int D => // note⏎1;`) | RH5401-004 | `VerifyNoCodeFixIsOffered` | pass (withheld today by the format-and-compare; see below) |
| `RH7101…Tests.SpecExampleCommentBehindTheArrowOfAnExpressionBodyWithholdsTheFix` (LF/CRLF) | RH7101-007 | `VerifyNoCodeFixIsOffered` | fail |
| `RH7101…Tests.CommentBehindTheArrowOfAnExpressionBodyElsewhereInTheTypeWithholdsTheFix` (4 rows: method on one line, getter, local fn in the initializer value, directive in nested type) | RH7101-007; FC-039 | `VerifyNoCodeFixIsOffered` | fail |

On the RH5401 row: on 425afc1 the RH5401 test fails only on its own fixture. The spec example names the new member `C` inside `class C`, which is CS0542 (`'C': member names cannot be the same as their enclosing type`), so the test uses `D`. With `D`, RH5401 already withholds the fix on 425afc1 (format-and-compare), so the test passes there. It covers the requirement's new sentence, not a change.

Unchanged and checked: the SHARED-BODY-001 "directive between `=>` and the expression is not reported" tests of RH3202, RH3203, RH3205–RH3208 stay as they are and pass on 425afc1 (analyzers keep `BlocksRewrite`). `RH7107…Tests.CommentInsideTheExpressionBodyOfTheMovedAccessorTravelsWithIt` (`get => /* one */ 1`) is untouched: RH7107's fix does not format through the formatter, so FMT-UNSUP-012 does not withhold it, and the throwing probe did not reach it.

## Not covered, and why

- **Deleted without a replacement row, because they call a phase directly with an input the precheck now refuses.** No caller passes such an input to a phase, and each deleted special case's input stays tested as a refusal in a final-output test:
  - `ExpressionBodiedTransformTests.PreservesCommentTrailingArrowIn{Method,Constructor,Operator,Conversion,Finalizer}`
  - `ExpressionBodiedLocalFunctionTransformTests.PreservesCommentTrailingArrowInLocalFunction`
  - `StructuralTransformSeparatorTests.CreatedReturnInFrontOfACommentAtColumnZeroGetsNoSpace`, `.CreatedReturnInFrontOfAnIndentedCommentKeepsTheAuthorsWhitespace`, the six `…BehindAnAtArrowComment…`/`…BehindACommentInFrontOfTheArrow…` tests, `.ExpressionStatementBehindAnAtArrowCommentKeepsItsLeadingComment`, and the now-unused helper `AssertLineBreakBehindCreatedReturn`.
- **Removed from `ExpressionBodyArrowCommentTests`, and kept as rows of `CommentsBehindTheArrowOfAConvertedMemberAreUnsupported`:** `CommentAfterArrowLeadsTheCreated{Return,Expression,Throw}Statement`, `CommentInFrontOfExpressionOnItsLineJoinsTheReturnKeyword`, `CommentsBeforeAndAfterArrowLeadTheStatementInSourceOrder`, `CommentAfterArrowOfLocalFunctionLeadsTheCreatedStatement`, `OwnLineCommentStaysOutOfTheBlockWhen{TheArrowLineCarriesALineComment,ADelimitedCommentIsGluedBehindTheArrow}`, `CommentBehind{TheExpression,TheSemicolon}DoesNotStopTheMoveOfACommentAfterTheArrow`, `CommentBehindTheTokenInFrontOfTheArrowDoesNotStopTheMove`.
- **Left unchanged:** `ExpressionBodiedTransformTests.KeepsCommentTrailingArrowInIndexer`. It is phase-level, holds the shape, and asserts that the indexer is left as is. No transform touches an indexer's arrow, so it passes before and after the change, and it tests no deleted special case.
- **FC-038 phases covered:** the closure test covers the getter conversion, the get-only lift, the method conversion, the documentation-comment relocation, and the property join. No other phase writes an arrow or moves trivia next to one (skeptic, FC-038).
- **No throw site.** FMT-UNSUP-012 names no phase that throws `SyntaxNotSupportedException`, so there is no throw-site test.
- **FC-007 / FC-008 (convergence, Fix All).** The shape only withholds fixes. Each refused member is tested with `VerifyNoCodeFixIsOffered`, which checks every reported diagnostic in the document, and the neighbour test shows the fix stays offered for a clean member. No new fix output exists that could fail to converge.
- **FC-018, FC-036 (naming), FC-022/FC-023 (literals, identifiers):** cannot reach this change. The check reads only trivia lists, and no rename or literal is involved.
- **FC-026 (identical tokens on one line):** covered by the RH3202 two-methods-on-one-line test and by `Two block comments`.
- **FC-012, FC-011:** no phase is added or changed by the declaration itself. The deletions the implementer makes are covered by the existing suites and by the closure test.
- **SHARED-BODY-003:** two RH3202 tests that were tagged with it (`VerifyCommentAfterArrowLeadsTheCreatedStatement`, `VerifyFixKeepsCommentOrderWhenTheArrowLineCarriesAComment`) now test the withheld fix and carry SHARED-BODY-001 / SHARED-FIX-010. SHARED-BODY-003 stays covered by the remaining tests, for example `VerifyCommentBeforeArrowLeadsTheCreatedStatement`; the architecture coverage test passes.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~UnsupportedExpressionBodyArrowCommentTests"
scripts/test.sh --project formatter --filter "FullyQualifiedName~ExpressionBodyArrowCommentTests|FullyQualifiedName~ExpressionBodyDirectiveGuardTests|FullyQualifiedName~ExpressionBodyConversionCommentPlacementTests|FullyQualifiedName~ExpressionBodiedIndexerTests|FullyQualifiedName~ExpressionBodiedMethodTests|FullyQualifiedName~ExpressionBodyNextLineExpressionTests|FullyQualifiedName~GetOnlyMemberExpressionBodyTests|FullyQualifiedName~StructuralTransformSeparator|FullyQualifiedName~StructuralTransformRequirementTests|FullyQualifiedName~CommentJoinTests|FullyQualifiedName~DirectiveJoinTests|FullyQualifiedName~IdempotencyTests|FullyQualifiedName~ExpressionBodiedTransformTests|FullyQualifiedName~ExpressionBodiedLocalFunctionTransformTests"
scripts/test.sh --project cli --filter "FullyQualifiedName~CliHiddenCodeEndToEndTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH320|FullyQualifiedName~RH5401|FullyQualifiedName~RH7101"
scripts/test.sh --project architecture
```

## Follow-up candidates

- **Spec defect, inside the run (for the spec author, not a follow-up issue):** the RH5401-004 example in `sdd/specs/analyzer/RH5401.md` adds `{|RH5401:int C => // note⏎1;|}` inside `class C`, which does not compile (CS0542). The test uses `int D`. The member should be renamed in the spec, for example to `D`.
