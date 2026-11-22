Role: implementer
Commit: e2d55dd
Verdict: #953: test disputed; #1852: tests pass; #1896: tests pass; #1818: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Every handed-over test of 07 and 09 passes except the three rows of `ContinuationJoinTests` listed under *Disputed tests or scope*; 54 further existing tests that neither handover touched now fail because they pin the output the spec diff changes, or an input the spec diff now refuses. They are listed with the clause that contradicts each, and no test was changed.

## thoenissen/Reihitsu#953

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/Core/Enumerations/JoinedGap.cs` (new) | The gaps the line-break phase joins and the precheck refuses, one value per table row, plus the type-test `is` (joined, refused by FMT-UNSUP-011) and the parameter modifier (joined by FMT-BREAK-015) | FMT-BREAK-041, FMT-UNSUP-017 |
| `src/Reihitsu.Formatter/Pipeline/Core/Utilities/JoinedGapUtilities.cs` (new) | The one statement of those gaps, by the roles of the two tokens around the gap (`GetGap`), and `IsModifier` for every modifier list of the tree; shared by the precheck and the line-break phase, like `AttachedPartGapUtilities` | FMT-BREAK-016, FMT-BREAK-017, FMT-BREAK-041, FMT-UNSUP-017 |
| `src/Reihitsu.Formatter/Pipeline/Precheck/PrecheckPhase.cs` | Reasons and finders for FMT-UNSUP-017 (one walk over the tokens; the gap's token in front must lie in the node, FMT-UNSUP-003), FMT-UNSUP-018 (lists whose every argument is omitted), FMT-UNSUP-019 (walk up to the nearest arm, member declarator, statement, or member declaration, inside the node); listed after every other shape so FMT-UNSUP-004/009/010 keep their reasons. `FindUnsupportedSyntax` split into `AddLastCandidates` and one helper for 018/019 (RH2008) | FMT-UNSUP-017, -018, -019 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/ContinuationJoinRewriter.cs` (new) | Joins the FMT-BREAK-041 gaps through `LineBreakTriviaUtilities.JoinGaps`; keeps the pattern behind `is` or `not` below it where FMT-BREAK-022 starts its first token on its own line. Runs right after `AttachedPartLineStartRewriter`, so the initializer rewriter decides a collection written over several lines as written (the RH5305 lambda-element fix test needs that order) | FMT-BREAK-041 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Utilities/PatternLineStartUtilities.cs` (new) | `StartsOwnLine`: the `{` of a property pattern without a type and the `(` of a parenthesized pattern, each spanning several lines | FMT-BREAK-017, FMT-BREAK-022, FMT-BREAK-041 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/LineBreakPhase.cs` | Registers the rewriter; class summary | FMT-BREAK-041 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/BinaryOperatorLineBreakRewriter.cs` | The chain layout replaces `NormalizePatternOperatorPosition`: at the outermost binary pattern of a chain (not through parentheses), a wrapped chain gets every combinator at a line start and its right pattern joined; the type-test `is` no longer moves | FMT-BREAK-017 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/LineBreakAssignmentRewriter.cs` | `VisitAnonymousObjectMemberDeclarator` and `VisitLetClause`: the `=` joins the name, the value joins the `=` | FMT-BREAK-016 |
| `src/Reihitsu.Formatter/Pipeline/HorizontalSpacing/Rules/OperatorSpacingRule.cs` | One space around the combinator of a binary pattern, the `is` of a pattern match, and the `=` of a `let` clause (see *Disputed tests or scope*, item S1) | FMT-SPACE-010 (gap) |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Contributors/ContinuationLineContributor.cs` (new) | Runs once per sweep at the root, first of all contributors; for every line-starting token that does not begin a scope part (statement, member, accessor, switch section or label, `else`/`catch`/`finally`, using directive, member/accessor/local-function attribute list and the token behind it, scope braces, the `while` of a `do`), finds the nearest node it does not begin, checks that a statement, member, or using directive (or a node-level root) holds it, and gives the line the column of that node's line + 4; a line that begins inside earlier text is measured from where that text starts; the root's first line uses the line-start column where the entry point passes one. Linear: container answers are cached, construct lines read from `SpanStart` | FMT-INDENT-034 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/LayoutModel.cs` | `SetContinuation`: writes only while no other contributor placed the line through `Set` or `SetUnlessPlaced`, also in later sweeps | FMT-INDENT-034 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Contributors/ParenthesizedPatternContributor.cs` | The content of a multi-line parenthesized pattern at `(` + 4, unless it is a multi-line property pattern's `{`, which `RecursivePatternContributor` already places there | FMT-INDENT-026 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/LayoutComputer.cs` | Registers the two new contributors | FMT-INDENT-034, -035 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/ListElementAligner.cs` | Deleted the zero-width anchor guard and the empty-first-token skip (see *Deleted special cases*); query bodies inside a list element are left to the query clause contributor and to continuation lines | FMT-INDENT-013, -015, FMT-UNSUP-018 |
| `src/Reihitsu.Formatter/Data/FormattingContext.cs`, `ReihitsuFormatter.cs`, `Utilities/ReihitsuFormatterHelpers.cs` | `RootLineStartColumn` fact; `ComputeDocumentLineStartColumn`; passed by `FormatNodeInDocumentAsync` (target) and `FormatContext` (context); XML docs of both entry points updated | FMT-PIPE-024, FMT-INDENT-034 |

### Tests

All 07/09 tests of #953 pass except `ContinuationJoinTests.EachGapIsJoined` rows *With expression* and *Modifier of an anonymous method*, and `ContinuationJoinTests.ModifierGapsOfOtherDeclarationsAreJoined` (disputed below). The RH5302/RH5308 `VerifyFixIndentsAContinuationOnTheRootsFirstLineFromTheDocumentsLineStart`, RH5305 `VerifyFixJoinsTheBodyOfALambdaElementBehindItsArrow`, RH7101, RH2001, RH5419, RH5401, RH5201, RH5112, RH5301/5303/5305 formatter tests of 07/09 pass.

## thoenissen/Reihitsu#1852

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Utilities/ExpressionBodyToBlockConverter.cs` | `CreateReturnStatement` no longer decides by the expression's leading trivia; the created `return` takes the arrow's trailing line breaks (not its whitespace), and the expression keeps its leading trivia, so the `return` join of FMT-BREAK-041 removes every line break and blank line (see *Deleted special cases*) | FMT-STRUCT-006, FMT-BREAK-041 |

The `return` join itself is #953's `ContinuationJoinRewriter`.

### Tests

`ExpressionBodyNextLineExpressionTests`, `StructuralTransformRequirementTests.ConvertedBodyKeepsCommentBeforeSemicolonAndJoinsNextLineExpression`, the RH3202/RH3205/RH3206/RH3208 `VerifyExpressionBehindABlankLineIsFixedToSingleSpacedReturn` tests, and the `return` rows of `UnsupportedJoinedGapCommentTests` pass.

## thoenissen/Reihitsu#1896

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/Indentation/Contributors/QueryClauseContributor.cs` (new) | Every clause, `select`/`group`, and `into` that starts a line aligns with the first token of its query expression, through every continuation; lines inside a clause are continuation lines (FMT-INDENT-034) | FMT-INDENT-035 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/ListElementAligner.cs` | No longer aligns query lines inside a list element itself (the clause contributor owns them everywhere) | FMT-INDENT-013 |

### Tests

`QueryClauseAlignmentTests` (all), `ArgumentContinuationLineAlignmentTests.QueryClausesOfNamedArgumentAreAlignedWithTheQueryStart`, and the `let` part of `SpecExampleEqualsOfAnAnonymousObjectMemberAndOfALetClauseJoinsBothSides` pass.

## thoenissen/Reihitsu#1818

### Changes

No change of its own: the later declarators behind `#endif` are continuation lines of their declaration (`ContinuationLineContributor`, #953).

### Tests

The five `ContinuationLineIndentationTests` declarator tests (both builds, `#else`/`#elif`) pass.

## Whole run

### Policy owners

- `rg -n "JoinedGapUtilities\.|PatternLineStartUtilities\.|SetContinuation\(" src --glob '!*Test*'`: `PrecheckPhase.FindTriviaInJoinedGap` and `ContinuationJoinRewriter.IsJoinedGap` read the gaps from `JoinedGapUtilities.GetGap` (one statement); `ContinuationJoinRewriter` and `BinaryOperatorLineBreakRewriter.NormalizePatternChain` read `PatternLineStartUtilities.StartsOwnLine`; `ContinuationLineContributor` is the only caller of `SetContinuation`.
- `rg -ln "QueryExpressionSyntax|QueryBodySyntax" src/Reihitsu.Formatter`: `QueryClauseContributor` (query clause column), `ListElementInteriorUtilities` (owning construct, unchanged), `SwitchCaseBraceRewriter` (unrelated).
- `rg -ln "IsPatternExpressionSyntax" src/Reihitsu.Formatter/Pipeline/LineBreaks src/Reihitsu.Formatter/Pipeline/HorizontalSpacing`: `ListElementInteriorJoinRewriter.IsOperatorWrap` (kept, the change doc's *Checked and kept*), `OperatorSpacingRule`.
- Modifier gaps: `JoinedGapUtilities.IsModifier` is new; `DeclarationModifierUtilities` (Core) serves accessibility ordering and has no modifier-gap policy.

### Production diff size

`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 19 files changed, 1378 insertions(+), 76 deletions(-) (13 modified, 6 new files).

New types: `JoinedGap` (enum), `JoinedGapUtilities`, `PatternLineStartUtilities`, `ContinuationJoinRewriter`, `ContinuationLineContributor`, `QueryClauseContributor`. New members: `FormattingContext.RootLineStartColumn` (constructor parameter), `LayoutModel.SetContinuation`, `ReihitsuFormatterHelpers.ComputeDocumentLineStartColumn`. All internal.

### Deleted special cases

- **`ExpressionBodyToBlockConverter.CreateReturnStatement`'s drop/keep of the expression's leading trivia** (continuation-join.md, *Deleted*): deleted. Not deletable without a replacement: for `=>⏎    42` the line break belongs to the arrow, which the conversion discards, so no line break is left for the join and `return         42` stayed (11 `…OnNextLineConvertsInOnePass` tests). The created `return` now carries the arrow's trailing line breaks (whitespace dropped, so `=> "s"` still gives `return"s"`, FMT-STRUCT-026), and the join removes them. One trivia filter replaces the old condition; the `return42` merge check stays.
- **`ListElementAligner.AlignToFirstElement`'s zero-width anchor guard and `AlignContinuations`' empty-first-token skip** (omitted-type-argument-comment-unsupported.md, *Deleted*): deleted. The guard also protected lists on one line (`typeof(Dictionary<,>)`): `GetFirstToken()` of an omitted argument returns no token, and `GetAdjustedColumn` threw (`WrappedUnboundGenericIsJoined` and 17 sibling tests). The anchor is now the element's own token, `GetFirstToken(includeZeroWidth: true)`, which needs no case: such a list never reaches the phase wrapped.
- No further special case found.

### Tests run

```text
scripts/test.sh --configuration Debug --project formatter      Failed: 39, Passed: 7036 (all 39 listed below)
scripts/test.sh --configuration Debug --project analyzer       Failed: 15, Passed: 11897 (all 15 listed below)
scripts/test.sh --configuration Debug --project architecture   Passed: 198
scripts/test.sh --configuration Debug --project cli            Passed: 425
scripts/test.sh --configuration Debug --project core           Passed: 812
scripts/build.sh (Release)                                     Build succeeded, 0 warnings
scripts/format.sh --check src                                  0 of 2043 file(s) need formatting
```

The handover filters of 07 (`ContinuationLineIndentationTests|QueryClauseAlignmentTests|ContinuationJoinTests|PatternChainLayoutTests|Unsupported…|ExpressionBodyNextLineExpressionTests|…|FieldDeclarationSplitTests|ParameterContinuationLineAlignmentTests`): 462 passed, 13 failed, each listed below. The performance tests (`ConditionalChain…GrowsLinearly`) pass. Self-hosting was not run locally.

### Disputed tests or scope

**Handed-over tests (07) that contradict an unchanged requirement** — the join happens, but a later, unchanged requirement lays the construct out differently:

1. `ContinuationJoinTests.EachGapIsJoined`, row *With expression* (`var w = r⏎    with { };` expected `r with { };`): the `with` joins; FMT-BREAK-021 then starts the initializer on its own line (`r with⏎{⏎}`), as `main` does for `var w = r with { };` written on one line.
2. Same test, row *Modifier of an anonymous method* (`static⏎    delegate { return 1; };` expected one line): the modifier joins; the block of the anonymous method is then laid out over several lines, as `main` does for the same statement written on one line.
3. `ContinuationJoinTests.ModifierGapsOfOtherDeclarationsAreJoined`: everything joins, but `public class N⏎{⏎}` becomes `public class N;` (the empty-type semicolon transform, unchanged); the expected value keeps the braces.

**Existing tests the handovers did not touch, whose pinned output FMT-INDENT-034 changes** (the line is a continuation line no other requirement places; each now stands 4 columns right of its construct's line):

4. `Regression/Structural/FieldDeclarationSplitTests`: `LineBreakBehindAGenericTypeIsRepeatedInEveryField`, `LineBreakBehindTheTypeIsRepeatedAndLineBreakBehindACommaIsNot`, `LineBreakBehindTheTypeIsRepeatedForDeclaratorsWithInitializers`, `LineBreakBehindTheTypeIsRepeatedInEveryField`, `SingleDeclaratorWithLineBreakBehindTheTypeIsLaidOutAsASplitField`, `WhitespaceOnlyTriviaBeforeFirstDeclaratorIsUnchanged` (declarator behind a type line break; FMT-STRUCT-021's amended example). Same cause in the analyzer project: `RH7101DoNotCombineFieldsAnalyzerTests.FixRepeatsALineBreakBehindTheTypeInEveryField` (LF, CRLF), `…VerifyFixLaysOutAFirstDeclaratorWithoutACommentAsTheFormatterLaysItOut`, `RH2001PrivateAutoPropertiesShouldNotBeUsedTriviaTests.VerifyFixIsOfferedForANameOnItsOwnLineWithoutCommentOrDirective` (+`…CarriageReturnLineFeed`).
5. Comment-led or comment-kept lines behind a property initializer's or an assignment's `=`: `Regression/Indentation/CommentLedFirstTokenAnchorTests.ArrayInitializerAlignsToItsBraceBehindCommentThatLeadsTheLine`, `…ArrayInitializerBelowCommentOnTheLineAboveKeepsItsBraceAtTheLineColumn`; `Regression/BlankLines/GluedCommentBlankLineTests.DecidesBlankLinesAboveGluedCommentAsDirectlyBeforeItsToken`, `…InsertsNoBlankLineAboveCommentGluedToArrayInitializerBraceOrArgumentListParenthesis`; `Regression/BlankLines/MultiLineGluedCommentBlankLineTests.DecidesAuthorBlankLinesAboveMultiLineCommentsGluedToArrayBraceMemberStatementOrElse`, `…InsertsNoBlankLineAboveMultiLineCommentsGluedToArrayBraceMemberStatementOrElse`; `Regression/BlankLines/CommentBeforeBraceBlankLineTests.RemovesBlankLineBetweenCommentAndOpeningBraceOfPropertyArrayInitializer`; `Regression/LineBreaks/AutoPropertyTrailingCommentTests.CommentBeforeOwnLineInitializerIsNotCrossed`; `Regression/FullPipeline/DelimitedDocumentationInExpressionTests.MovesDelimitedDocumentationBehindAssignmentOperator`.
6. Lines kept behind a comment elsewhere: `Regression/Indentation/ArgumentContinuationLineAlignmentTests.HeldLineInsideObjectInitializerArgumentIsLeftToTheInitializer` (`A = // keep⏎value`, from the `A` line) and `…HeldLineInsideSwitchExpressionArgumentIsLeftToTheSwitchExpression` (`1 => // keep⏎"one"`, from the arm's line; the gap behind an arm's `=>` is neither joined nor refused); `Regression/LineBreaks/UnionDeclarationLayoutTests.EndOfLineCommentBetweenUnionNameAndCaseTypeListKeepsItsLineBreak`; `Regression/Spacing/HorizontalSpacingTests.BlockCommentBeforeGenericBracketOnContinuationLineKeepsGapUntouched`; `Regression/Spacing/StrandedDocumentationTokenSpacingTests.KeepsGapBetweenBlockCommentAndTerminatorBelowStrayDocumentationComment`; `Regression/LineBreaks/ArgumentListLineStartTests.TypeofOperandBelowTheKeywordKeepsItsLineBreak`; `Regression/Spacing/InterpolationAlignmentSpacingReproTests.AlignmentCommaSeparatedFromAlignmentByLineBreakStaysUnchanged` (a line inside a hole of a regular interpolated string). Analyzer project: `RH6001…FormatterTests.VerifyFormatterOutputWithStrayDocumentationCommentBehindKeywordIsNotReported`, `…WithBlockCommentAndStrayDocumentationCommentBehindKeywordIsNotReported`, `RH6005…FormatterTests.VerifyFormatterOutputWithStrayDocumentationCommentBehindOperatorKeywordIsNotReported`, `RH6021…FormatterTests.VerifyFormatterOutputWithStrayDocumentationCommentBehindColonIsNotReported`, `RH5105…FormatterTests.VerifyFormatterGenericMethodCommentInGapIsStable`, `RH6011…FormatterTests.VerifyContinuationLineOpeningGenericBracketStaysAnalyzerClean`, `RH6014…FormatterTests.VerifyContinuationLineClosingAttributeBracketStaysAnalyzerClean` (lines inside a member's attribute list). These "stays analyzer clean" tests also assert that their analyzer does not report the formatter's output; whether it reports the new column was not checked.
7. Lambda attribute lists, which FMT-INDENT-034's closed list of scope parts does not name: `Regression/Structural/AttributeListStructuralFinalOutputTests.LambdaAttributeListKeepsItsOutput`, `…LambdaAttributeListWithALineBreakBehindTheOpeningBracketSettlesInOnePass`.
8. An attribute list on a statement other than a local function (`[Obsolete]⏎return _value;`, a semantic error the parser accepts): `Regression/Structural/StructuralTransformRequirementTests.AccessorWithAttributedStatementKeepsBlock`. The spec's list names the attribute lists of a member, an accessor, or a local function only, so `return` is a continuation line. Either the spec adds statement attribute lists (one more `AttributeLists` read) or the test changes; I followed the spec.
9. `Unit/Indentation/LayoutComputerTests.ComputeHandlesLinqExpression`: `.Where` is the chain's first link and its own anchor (SHARED-CHAIN-007), placed by nothing else, so it is a continuation line (12, not 8); `.Select` aligns to it.
10. `Unit/ReihitsuFormatterDetachedRootTests.ChainPrefixTargetKeepsTheLayoutOfTheLongerChain`: the target `source.Where(…).Select(…)` is a prefix of a longer chain, so the chain rule leaves its links; `.Where` and `.Select` are continuation lines of the root's first line and take the line-start column + 4 (12), which whole-document formatting gives the whole chain as well. `main`'s 16 came from shifting block indentation to the root's first-token column.

**Existing tests whose input the spec diff now refuses (FMT-UNSUP-017)**:

11. `Unit/Precheck/UnsupportedExpressionBodyArrowCommentTests.CommentOutsideTheGapIsSupported`, rows *Behind a throw* (`=> throw /* c */ new Exception()`, a throw expression) and *Behind the arrow of a lambda that is the expression* (`() => /* c */ 1`).
12. `Unit/Precheck/UnsupportedVariableDeclarationTests.CommentBehindTheFirstSemicolonOfAForStatementIsSupported` and the `for (int i = 0; /* c */ i < 1; i++)` row of `Unit/Precheck/UnsupportedSyntaxClosureTests.OutputOfAFormattedInputIsNotUnsupported` (gap behind a `for` header's `;`).
13. `Regression/Spacing/ArrowSpacingTests.CommentBehindTheArrowKeepsItsPlace` (lambda arrow), `Regression/Spacing/PrefixKeywordSpacingTests.CommentBehindTheKeywordKeepsItsPlace` (keyword operand), `Regression/LineBreaks/ListElementInteriorJoinTests.ParameterInteriorWrapHeldByDisabledTextIsAlignedInsteadOfJoined` (disabled text behind the parameter modifier `ref`).
14. `RH2001PrivateAutoPropertiesShouldNotBeUsedTriviaTests.VerifyFixKeepsADirectiveBetweenModifiersThatKeepTheirOrder` (LF, CRLF): `private⏎#pragma …⏎static` puts a directive behind a modifier of the field the fix builds; RH2001-007 now withholds the fix.
15. `RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests.VerifyCodeFixConvergesForMultilinePropertyInitializer`: the fix formats the property, the `public⏎string` gap joins (FMT-BREAK-041), and the property then collapses to one line; the expected value keeps `public` alone on its line.

**Tests that contradict FMT-BREAK-041 directly**:

16. `Regression/LineBreaks/ParenthesizedLambdaLineStartTests.BlankLineInFrontOfLambdaBehindExpressionLambdaArrowIsKept` (`x =>⏎⏎(y) => y`): the lambda arrow's gap joins, blank line included.
17. `Unit/LineBreaks/JoinSeparatorPhaseTests.DirectiveInTheGapStillRefusesEveryJoin`: runs `LineBreakPhase` alone on `protected⏎#pragma warning disable CS0002⏎internal set;`, an input the precheck refuses (FMT-UNSUP-017); the new join carries no refusal, as the change doc states, so the directive is joined onto `protected`'s line. No caller reaches this input (decision 15's reasoning); keeping the test would need FMT-BREAK-001's check in the join, which the change doc's *Special cases* rules out.

**Scope and spec gaps**:

- S1. **Spacing of joined pattern operators and the `let` `=`.** A join writes the minimal separator, and no FMT-SPACE rule covered `or (`, `is (`, or `let y = x` (FMT-SPACE-010 names binary, assignment, initializer, and name-equals operators only), so `x is 1 or⏎(2 or 3)` became `or(2 or 3)` and `let y⏎= x` `let y= x`, while `PatternChainLayoutTests.ParenthesizedPatternStartsAChainOfItsOwn` and the FMT-BREAK-016 example expect one space. `OperatorSpacingRule` now also covers a binary pattern's combinator, the `is` of a pattern match, and a `let` clause's `=`. This changes `x is(1)` on one line to `x is (1)`. The spec author should add them to FMT-SPACE-010.
- S2. **Code owners.** `ContinuationJoinRewriter` and `PatternLineStartUtilities` (FMT-BREAK), `ContinuationLineContributor` and `QueryClauseContributor` (FMT-INDENT), and `JoinedGapUtilities` (FMT-UNSUP, FMT-BREAK) are in no spec's *Code owners* row; the AGENTS.md phase table's line-break and precheck rows name no owner for the new joins and shapes.
- S3. **Size.** Six new types and about 1,370 production lines; the handovers name no production paths. The continuation rule is one contributor, so no contributor was added per construct.
- S4. **A node-level root counts as a container.** FMT-INDENT-034 asks for a token "inside a statement, a member declaration, or a using directive"; a node-level root such as RH5302's `&&` expression holds none, so the contributor treats the root itself as one (it stands in one in its document). Without this, `VerifyFixIndentsAContinuationOnTheRootsFirstLine…` stays at 16. The spec text could say so.
- S5. **Declare-unsupported option, not taken.** The comment-kept lines of items 5–6 (behind a property initializer's `=`, an assignment's `=` outside arms, an arm's `=>`, a union name) stay continuation lines; declaring them is a maintainer question, not raised by me.

### Architecture findings

_None._ The gap list is stated once (`JoinedGapUtilities`) and read by the precheck and the line-break phase; the continuation contributor yields to every other contributor through the layout model's ownership records rather than predicting which construct places a line.

### Follow-up candidates

- Items 4–15 above need test updates (test implementer), and S1/S2/S4 spec text (spec author).
