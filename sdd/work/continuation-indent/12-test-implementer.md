Role: test implementer
Commit: bfc3e5e
Verdict: tests written

Follow-up round for the lead's decision rows 16, 17, 19, and 20 (`00-lead.md`), from `10-implementer.md` (disputed rows, the 54 failing existing tests and the clause each contradicts) and `11-spec-author.md` (changed expected values, new tests). The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond those rows. No production code was touched. Each expected value below was checked against the clause named, not copied from the head's output; where the head's output and the clause agree the test now asserts it.

On the head (bfc3e5e), Debug: formatter 7086 passed, 0 failed; analyzer 11912 passed, 0 failed; architecture 198 passed, 0 failed.

## Tests

### Row 16 — joins reshaped by unchanged rules (`Regression/LineBreaks/ContinuationJoinTests.cs`)

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `EachGapIsJoined`, row *With expression* | FMT-BREAK-041 | `AssertRuleResult` | `var w = r with` / `{` / `};`, both braces in column 18 (FMT-BREAK-041's reshaping example, FMT-BREAK-021) |
| `EachGapIsJoined`, row *Modifier of an anonymous method* | FMT-BREAK-041 | `AssertRuleResult` | `Func<int> g = static delegate` / `{` / `return 1;` / `};` (FMT-BREAK-002, FMT-BREAK-006); summary updated |
| `ModifierGapsOfOtherDeclarationsAreJoined` | FMT-BREAK-041 | `AssertRuleResult` | `public class N;` (FMT-STRUCT-017) |
| `SpecExampleJoinedConstructsAreLaidOutAsWhenWrittenOnOneLine` (new) | FMT-BREAK-041 | `AssertRuleResult` | the reshaping example verbatim (`public class N;`, the anonymous method's block, the `with` initializer) |

### Row 17 — existing tests whose output or input the accepted spec diff changes

Continuation lines, FMT-INDENT-034 (the line's first token stands inside a statement, member, or using directive begun on an earlier line, begins no scope part, and no other requirement places it; column = line of the nearest construct begun on an earlier line + 4):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Regression/Structural/FieldDeclarationSplitTests`: `LineBreakBehindAGenericTypeIsRepeatedInEveryField`, `LineBreakBehindTheTypeIsRepeatedAndLineBreakBehindACommaIsNot`, `LineBreakBehindTheTypeIsRepeatedForDeclaratorsWithInitializers`, `LineBreakBehindTheTypeIsRepeatedInEveryField`, `WhitespaceOnlyTriviaBeforeFirstDeclaratorIsUnchanged` | FMT-STRUCT-021 | `AssertRuleResult` | declarator behind the type line break in column 8 (FMT-STRUCT-021's second example) |
| `…FieldDeclarationSplitTests.SingleDeclaratorWithLineBreakBehindTheTypeIsLaidOutAsASplitField` | — | `AssertRuleResult` | `_x;` in column 8 |
| `Formatting/RH7101DoNotCombineFieldsAnalyzerTests.FixRepeatsALineBreakBehindTheTypeInEveryField` (LF, CRLF), `…VerifyFixLaysOutAFirstDeclaratorWithoutACommentAsTheFormatterLaysItOut` | FMT-STRUCT-021, RH7101-004 / RH7101-004 | `VerifyCodeFixWithEndOfLine` / `Verify` | declarators in column 8 (FMT-STRUCT-021 example states the RH7101 fix gives the same output) |
| `Design/RH2001PrivateAutoPropertiesShouldNotBeUsedTriviaTests.VerifyFixIsOfferedForANameOnItsOwnLineWithoutCommentOrDirective` (+ `…CarriageReturnLineFeed`) | RH2001-004, RH2001-007, SHARED-FIX-007 | `Verify` | `_value;` in column 8 (the fix lays the field out as the formatter does; shared constant `NameOnItsOwnLineFixedSource`) |
| `Regression/Indentation/CommentLedFirstTokenAnchorTests.ArrayInitializerAlignsToItsBraceBehindCommentThatLeadsTheLine` | FMT-INDENT-023, FMT-INDENT-030 | `AssertRuleResult` | `/* c */ {` in column 8, elements 20, `}` 16 (brace column behind the comment, FMT-INDENT-023) |
| `…CommentLedFirstTokenAnchorTests.ArrayInitializerBelowCommentOnTheLineAboveKeepsItsBraceAtTheLineColumn` | FMT-INDENT-023, FMT-INDENT-030 | `AssertRuleResult` | `// c`, `{`, `};` in column 8, elements 12 |
| `Regression/BlankLines/GluedCommentBlankLineTests.DecidesBlankLinesAboveGluedCommentAsDirectlyBeforeItsToken`, `…InsertsNoBlankLineAboveCommentGluedToArrayInitializerBraceOrArgumentListParenthesis` | FMT-BLANK-011, FMT-BLANK-018 | `AssertRuleResult` | `/* b */ { 1, 2 };` in column 8 (see *Spec findings*, item 1) |
| `Regression/BlankLines/MultiLineGluedCommentBlankLineTests.DecidesAuthorBlankLinesAboveMultiLineCommentsGluedToArrayBraceMemberStatementOrElse`, `…InsertsNoBlankLineAboveMultiLineCommentsGluedToArrayBraceMemberStatementOrElse` | FMT-BLANK-011, FMT-BLANK-018 / FMT-BLANK-018 | `AssertRuleResult` | `/* g` in column 8; the comment's second line ` */ { 1, 2 };` keeps its text (FMT-INDENT-011: only lines that start with a token, a comment, or a directive are re-indented) |
| `Regression/BlankLines/CommentBeforeBraceBlankLineTests.RemovesBlankLineBetweenCommentAndOpeningBraceOfPropertyArrayInitializer` | FMT-BLANK-020 | `AssertRuleResult` | `// c`, `{`, `};` in column 8, `1` in 12 |
| `Regression/LineBreaks/AutoPropertyTrailingCommentTests.CommentBeforeOwnLineInitializerIsNotCrossed` | — | `AssertRuleResult` | `= 1;` in column 8 |
| `Regression/FullPipeline/DelimitedDocumentationInExpressionTests.MovesDelimitedDocumentationBehindAssignmentOperator` | — | `AssertRuleResult` | `/** d */ a;` in column 12 |
| `Regression/Indentation/ArgumentContinuationLineAlignmentTests.HeldLineInsideObjectInitializerArgumentIsLeftToTheInitializer`, `…HeldLineInsideSwitchExpressionArgumentIsLeftToTheSwitchExpression` | FMT-INDENT-013, FMT-INDENT-034 (tag added) | `AssertRuleResult` | `value,` / `"one",` in column 18, four right of the member's / arm's line (14); FMT-INDENT-013 leaves them to the construct, FMT-UNSUP-019 does not refuse them (no assignment behind the comment / an arm's `=>`) |
| `Regression/LineBreaks/UnionDeclarationLayoutTests.EndOfLineCommentBetweenUnionNameAndCaseTypeListKeepsItsLineBreak` | — | `AssertRuleResult` | `(int, string);` in column 8 |
| `Regression/Spacing/HorizontalSpacingTests.BlockCommentBeforeGenericBracketOnContinuationLineKeepsGapUntouched` | FMT-SPACE-001 | `AssertRuleResult` | `/* Keep. */ <int> M()` in column 8; summary no longer calls the column a re-flow |
| `Regression/Spacing/StrandedDocumentationTokenSpacingTests.KeepsGapBetweenBlockCommentAndTerminatorBelowStrayDocumentationComment` | FMT-SPACE-001 | `AssertRuleResult` | `/* b */ ;` in column 12 (FMT-INDENT-034 names a `;` behind a documentation comment written behind code) |
| `Regression/LineBreaks/ArgumentListLineStartTests.TypeofOperandBelowTheKeywordKeepsItsLineBreak` | — | `AssertRuleResult` | `(int);` in column 12 |
| `Regression/Spacing/InterpolationAlignmentSpacingReproTests.AlignmentCommaSeparatedFromAlignmentByLineBreakStaysUnchanged` | — | `AssertRuleResult` | `,-10}";` in column 12 (hole of a regular interpolated string; the nearest construct begun earlier is the interpolation) |
| `Regression/Structural/AttributeListStructuralFinalOutputTests.LambdaAttributeListKeepsItsOutput`, `…LambdaAttributeListWithALineBreakBehindTheOpeningBracketSettlesInOnePass` | FMT-INDENT-034 (tag added) | `AssertFirstPassAndSecondPass` | `[B]` and `(int x) => x;` in column 12 (lambda attribute lists are not in FMT-INDENT-034's list of scope parts) |
| `Regression/Structural/StructuralTransformRequirementTests.AccessorWithAttributedStatementKeepsBlock` | FMT-STRUCT-010 | `AssertRuleResult` | `return _value;` behind `[Obsolete]` in column 16 (only attribute lists of a member, accessor, or local function are scope parts); input unchanged, expected added (see *Spec findings*, item 3) |
| `Unit/Indentation/LayoutComputerTests.ComputeHandlesLinqExpression` | FMT-INDENT-034 (tag added) | `LayoutComputer.Compute` | `.Where` and `.Select` column 12: `.Where` is the chain's first link and its own anchor (SHARED-CHAIN-007), which no alignment places |
| `Unit/ReihitsuFormatterDetachedRootTests.ChainPrefixTargetKeepsTheLayoutOfTheLongerChain` → renamed `ChainPrefixTargetIndentsItsLinksAsContinuationLinesOfTheRootsFirstLine` | FMT-PIPE-023, FMT-PIPE-024, FMT-INDENT-034 (tag added) | `AssertFormatsTarget` | `.Where`/`.Select` in column 12: the target is not the chain's outermost node, so SHARED-CHAIN-001 does not lay it out as a chain, and FMT-INDENT-034's root clause measures from the line-start column (8) |
| `Formatter/Formatting/RH5105…FormatterTests.VerifyFormatterGenericMethodCommentInGapIsStable` | as tagged | `VerifyFormatter(source)` | `// why` and `(TValue value)` in column 8; RH5105 does not report it |
| `Formatter/Formatting/RH6001…FormatterTests.VerifyFormatterOutputWithStrayDocumentationCommentBehindKeywordIsNotReported`, `…WithBlockCommentAndStrayDocumentationCommentBehindKeywordIsNotReported` | as tagged | `VerifyFormatter(source)` | `(c)` in column 12; RH6001 does not report it |
| `Formatter/Formatting/RH6005…FormatterTests.VerifyFormatterOutputWithStrayDocumentationCommentBehindOperatorKeywordIsNotReported` | as tagged | `VerifyFormatter(source)` | `+(C a, C b)` in column 8; not reported |
| `Formatter/Formatting/RH6011…FormatterTests.VerifyContinuationLineOpeningGenericBracketStaysAnalyzerClean` | as tagged | `VerifyFormatter(source)` | `<int> Method()` in column 8; not reported |
| `Formatter/Formatting/RH6014…FormatterTests.VerifyContinuationLineClosingAttributeBracketStaysAnalyzerClean` | as tagged | `VerifyFormatter(source)` | `System.Obsolete` and `]` in column 4 (lines inside the attribute list continue it); not reported |
| `Formatter/Formatting/RH6021…FormatterTests.VerifyFormatterOutputWithStrayDocumentationCommentBehindColonIsNotReported` | as tagged | `VerifyFormatter(source)` | `base(1)` in column 12, four right of the initializer's `:` line; not reported |

The open question of 10 item 6, whether the analyzers of these "stays analyzer clean" tests report the new columns, is answered: none does (`VerifyFormatter` asserts it, all pass).

Inputs now refused (FMT-UNSUP-017); each test is rewritten so that it still checks its own requirement on an input a caller reaches, or turned into the refusal under the shape that now owns the gap:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Unit/Precheck/UnsupportedExpressionBodyArrowCommentTests.CommentOutsideTheGapIsSupported` | FMT-UNSUP-012 | `AssertSupported` | rows *Behind a throw* and *Behind the arrow of a lambda that is the expression* removed |
| `…UnsupportedExpressionBodyArrowCommentTests.CommentBehindTheFirstTokenInAGapOfAnotherShapeIsRefusedForThatShape` (new, the two rows) | FMT-UNSUP-012, FMT-UNSUP-017 | `AssertUnsupportedFor` | refused with `KeywordOperandCommentReason` / `LambdaArrowCommentReason`, not FMT-UNSUP-012's reason |
| `Unit/Precheck/UnsupportedVariableDeclarationTests.CommentBehindTheFirstSemicolonOfAForStatementIsSupported` → `…IsRefusedForTheForHeader` | FMT-UNSUP-004, FMT-UNSUP-017 | `AssertUnsupportedFor` | refused with `ForHeaderCommentReason`, reported at `/* c */` (not FMT-UNSUP-004's shape) |
| `Unit/Precheck/UnsupportedSyntaxClosureTests.OutputOfAFormattedInputIsNotUnsupported`, the `using`/`for` row | FMT-UNSUP-005 | `AssertFormattedOutputIsNotUnsupported` | `for (int i = 0; i < 1; i++) // d` instead of a comment behind the header's `;` |
| `Regression/Spacing/ArrowSpacingTests.CommentBehindTheArrowKeepsItsPlace` | FMT-SPACE-017, FMT-SPACE-013 | `AssertRuleResult` | a switch arm `1 =>/* a */1,` → `1 =>/* a */ 1,` (the arm's `=>` gap is neither joined nor refused) |
| `Regression/Spacing/PrefixKeywordSpacingTests.CommentBehindTheKeywordKeepsItsPlace` | FMT-SPACE-024, FMT-SPACE-013 | `AssertRuleResult` | `var/* a */(u, w) = t;` → `var/* a */ (u, w) = t;` (`var` of a designation, a FMT-SPACE-024 keyword outside FMT-UNSUP-017) |
| `Regression/LineBreaks/ListElementInteriorJoinTests.ParameterInteriorWrapHeldByDisabledTextIsAlignedInsteadOfJoined` | FMT-BREAK-001, FMT-BREAK-015 | `AssertRuleResult` | disabled text between a parameter's type and name: `M(int⏎#if NEVER⏎extra⏎#endif⏎value)`, `value)` aligned under `int` (column 20) |
| `Design/RH2001…TriviaTests.VerifyFixKeepsADirectiveBetweenModifiersThatKeepTheirOrder` (LF, CRLF) → `VerifyNoFixIsOfferedForADirectiveBetweenModifiersThatKeepTheirOrder` | RH2001-009, SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | reported, no fix: the field the fix builds holds `#pragma` behind `private` (FMT-UNSUP-017; maintainer answers 1d A and 3 A). See *Spec findings*, item 2 |

Direct contradictions of FMT-BREAK-041, and row 18:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Regression/LineBreaks/ParenthesizedLambdaLineStartTests.BlankLineInFrontOfLambdaBehindExpressionLambdaArrowIsKept` → `…GoesWithTheJoin` | FMT-BREAK-041, FMT-BREAK-030 | `AssertRuleResult` | `Func<int, Func<int, int>> h = x => (y) => y;` (blank line goes with the join, as the `return` sibling) |
| `Formatting/RH5111…AnalyzerTests.VerifyCodeFixConvergesForMultilinePropertyInitializer` | RH5111-001, RH5111-004 | `Verify` | `public string Property { get; set; } = "test";` (change doc section 4, lead row 18) |

### Row 19

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Unit/LineBreaks/JoinSeparatorPhaseTests.DirectiveInTheGapStillRefusesEveryJoin` → `DirectiveInTheGapStillRefusesTheJoinOfACaseGuard` | FMT-BREAK-031 | `ExecuteLineBreakPhase` (LF, CRLF) | the accessor-modifier half removed (refused before the phase, no caller passes it); the reachable `case (1, 2)⏎#pragma⏎when b:` half kept, the directive still on its own line. Checked through the CLI that the precheck does not refuse that case-guard input |

### Row 20 — FMT-SPACE-010 (`Regression/Spacing/HorizontalSpacingRequirementTests.cs`, new)

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `SpecExampleOneSpaceAroundPatternOperatorsAndTheEqualsOfALetClause` | FMT-SPACE-010 | `AssertRuleResult` | the requirement's second example verbatim |
| `OneSpaceAroundEachPatternOperatorAndTheEqualsOfALetClause`, 8 rows: `is(`, `is[`, `or(` in a list pattern, `(o)is"s"`, `)and(`, `)or(`, `let y=x`, and *Spaced already* (`o is (1) or (2)` unchanged) | FMT-SPACE-010 | `AssertRuleResult` | one space on both sides; the spaced row stays |
| `OneSpaceAroundACombinatorInASwitchArmAndInACaseLabel` | FMT-SPACE-010 | `AssertRuleResult` | `case(1) or (2):` (the gap behind `case` keeps what it has, change doc section 5) and `(1) or (2) => 1,` |

Watched against `origin/main`'s code in a temporary worktree (this file only): every new FMT-SPACE-010 test fails there on the missing space (`o is(1)`, `(1)and(2)`, `let y=x`, `case(1)or(2)`), except *Spaced already*, which passes on both, as the stable side should. The row-16 spec-example test and the rewritten tests assert behavior the head already has; they pass on the head.

## Spec findings

1. **FMT-BLANK-018's example is stale under FMT-INDENT-034.** `sdd/specs/formatter/blank-lines.md:1991-1996`, the output of "Above a comment glued to an array initializer's brace…": `int[] G { get; } =` ⏎ `/* b */ { 1, 2 };` shows the comment line in column 4. FMT-INDENT-034 (and FMT-INDENT-002's sentence that a continuation line takes its column) give it column 8, which the head produces and `GluedCommentBlankLineTests.DecidesBlankLinesAboveGluedCommentAsDirectlyBeforeItsToken`, the test of that example, now asserts. The example needs `/* b */ { 1, 2 };` four columns deeper; the blank-line decision it illustrates is unchanged. Route to the spec author.
2. **RH2001-009 still says the fix is offered for a directive between modifiers that keep their order.** `sdd/specs/analyzer/RH2001.md:634`: "A directive other than `#if`, `#elif`, `#else`, and `#endif` between two modifiers that keep their order … does not withhold the fix, and it keeps its place between them." The field the fix builds holds that directive behind a modifier, which FMT-UNSUP-017 refuses (maintainer answers 1d A and 3 A: such code is refused and fixes formatting it are withheld), so SHARED-FIX-010 withholds the fix, and the head withholds it. I changed the test to the withheld fix on those answers; the sentence (and RH2001-009's title, "a directive that a reordered modifier would cross") needs the spec author. 10 attributed this to RH2001-007; RH2001-007's amended paragraph covers only comments inside the initializer value, so it does not decide this input.
3. **A statement's attribute list is not a scope part.** `StructuralTransformRequirementTests.AccessorWithAttributedStatementKeepsBlock` (`[Obsolete]` ⏎ `return _value;`, a semantic error the parser accepts) now expects `return` four columns right of `[Obsolete]`, because FMT-INDENT-034's closed list names only the attribute lists of a member, an accessor, or a local function. The spec decides it as written; whether a statement's attribute list should join that list is the spec author's call (10, item 8). Not a blocker.

## Not covered, and why

- No test was left unchanged whose expected value no spec or maintainer answer decides: every one of 10's 54 failing tests is decided by FMT-INDENT-034, FMT-BREAK-041, FMT-UNSUP-017, FMT-STRUCT-017/021, or SHARED-FIX-010 as listed above.
- FMT-INDENT-034's node-level root clause (row 23): `VerifyFixIndentsAContinuationOnTheRootsFirstLineFromTheDocumentsLineStart` of RH5302 and RH5308 already carry the FMT-INDENT-034 tag; nothing added.
- CRLF: every `AssertRuleResult` and `VerifyFormatter` call runs LF and CRLF itself; the analyzer code-fix tests keep their existing LF/CRLF rows; the phase test runs both.
- Second-pass idempotency: `AssertRuleResult` and `AssertFirstPassAndSecondPass` check it.
- Comment or directive in the gap for the new spacing rule: a comment beside a pattern combinator or the `is` of a pattern is refused (FMT-UNSUP-011, FMT-UNSUP-017), covered by `UnsupportedJoinedGapCommentTests`; FMT-SPACE-013's comment handling is covered by the rewritten `CommentBehindTheArrowKeepsItsPlace` and `CommentBehindTheKeywordKeepsItsPlace`.
- Failure classes: the changes are expected-value updates and spacing tests; no new condition was added to production code in this round, so no FC-003 boundary test beyond the *Spaced already* row and the `case` gap that stays.

## Commands

```shell
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~ContinuationJoinTests|FullyQualifiedName~HorizontalSpacingRequirementTests|FullyQualifiedName~FieldDeclarationSplitTests|FullyQualifiedName~CommentLedFirstTokenAnchorTests|FullyQualifiedName~GluedCommentBlankLineTests|FullyQualifiedName~CommentBeforeBraceBlankLineTests|FullyQualifiedName~ArgumentContinuationLineAlignmentTests|FullyQualifiedName~AttributeListStructuralFinalOutputTests|FullyQualifiedName~StructuralTransformRequirementTests|FullyQualifiedName~LayoutComputerTests|FullyQualifiedName~ReihitsuFormatterDetachedRootTests|FullyQualifiedName~JoinSeparatorPhaseTests|FullyQualifiedName~UnsupportedExpressionBodyArrowCommentTests|FullyQualifiedName~UnsupportedVariableDeclarationTests|FullyQualifiedName~UnsupportedSyntaxClosureTests|FullyQualifiedName~ArrowSpacingTests|FullyQualifiedName~PrefixKeywordSpacingTests|FullyQualifiedName~ListElementInteriorJoinTests|FullyQualifiedName~ParenthesizedLambdaLineStartTests"
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH2001PrivateAutoPropertiesShouldNotBeUsedTriviaTests|FullyQualifiedName~RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests|FullyQualifiedName~RH7101DoNotCombineFieldsAnalyzerTests|FullyQualifiedName~RH5105OpeningParenthesisMustBeOnDeclarationLineFormatterTests|FullyQualifiedName~RH6001KeywordsMustBeSpacedCorrectlyFormatterTests|FullyQualifiedName~RH6005OperatorKeywordMustBeFollowedBySpaceFormatterTests|FullyQualifiedName~RH6011OpeningGenericBracketsMustBeSpacedCorrectlyFormatterTests|FullyQualifiedName~RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyFormatterTests|FullyQualifiedName~RH6021ColonsMustBeSpacedCorrectlyFormatterTests"
scripts/test.sh --configuration Debug --project architecture
```

## Follow-up candidates

_None_ beyond the spec findings above, which belong to this run.
