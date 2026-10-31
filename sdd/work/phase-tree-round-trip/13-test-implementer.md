Role: test implementer
Commit: 21ab3a3
Verdict: tests written (repair round 1: audit findings #1–#6 and the two structural decisions)

Inputs:
- `11-auditor.md`, findings #1–#7, A1, and A2;
- `12-spec-author.md`;
- FMT-PIPE-021, FMT-STRUCT-026, and FMT-BREAK-002 as amended at 6316862;
- `sdd/changes/2026-10/phase-tree-round-trip.md`;
- `squad/decisions/2026-10-31-phase-tree-round-trip-{would-merge-owner,member-brace-owner}.md`.

My prompt carried no intended fix to ignore. I took expected values from the spec text and examples, not from the auditor's suggestions. The node-level brace outputs come from `origin/main`, as the member-brace-owner decision requires. I got them by running `FormatNodeInDocumentAsync` and `FormatNode` from a scratch harness against a `git archive` of `origin/main`. Finding #7 is text only and has no test.

## Spec defect found

`sdd/specs/formatter/structural-transforms.md` at 6316862 has lost the backslash of `\u0061` in the `return` example: "`int B() => a;` gives `return a;`" is identical to A and should read `int B() => \u0061;` gives `return \u0061;`. The first separator bullet is intact (`a`, then an identifier that starts with a Unicode escape, `\u0061`).

I tested the shape the bullet names, `\u0061`. The spec author should restore the escape. When writing it, note that a `\u` sequence typed in a tool call can be unescaped on the way into the file. My own first draft lost them the same way, and I wrote them with `chr(92)` instead.

## Tests

Every test below fails at 21ab3a3 unless marked *pass*. Each fails on the requirement's own difference.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| Reparse hook `PhaseTreeRoundTrip.FindDifference` now compares token values: `ValueText`, the value's type, and value equality, element-wise for arrays | FMT-PIPE-021 (untagged helper; reaches every `FormatterTestsBase` fixture) | — | no existing test newly fails |
| `PhaseTreeRoundTripTests.MovedInterpolatedTextTokenStillStandsForItsContent` | FMT-PIPE-021 | `ObservedPipeline.GetPhaseOutput("IndentationPhase")` | fail: `ValueText` is `"                 a "`, expected `"a "` |
| `PhaseTreeRoundTripTests.ColumnZeroInterpolatedLiteralMovesUnderItsOpeningQuotes` | FMT-PIPE-021 | `AssertRuleResult` | fail: the hook reports the value of the IndentationPhase text token |
| `PhaseTreeRoundTripTests.MovedDoubleDollarInterpolatedLiteralAtColumnZeroKeepsItsTokenValues` | FMT-PIPE-021 | `ExecuteAndCompare` | fail: same class, `{a} ` |
| `PhaseTreeRoundTripTests.MovedInterpolatedLiteralWithIndentedClosingLineKeepsItsTokenValues` | FMT-PIPE-021 | `ExecuteAndCompare` | pass |
| `PhaseTreeRoundTripTests.MovedPlainRawLiteralAtColumnZeroKeepsItsValue` | FMT-PIPE-021 | `ExecuteAndCompare` | pass |
| `PhaseTreeRoundTripTests.MovedUtf8RawLiteralAtColumnZeroKeepsItsValue` | FMT-PIPE-021 | `ExecuteAndCompare` | pass |
| `PhaseTreeRoundTripTests.InterpolatedLiteralMovedToTheLeftKeepsItsTokenValues` | FMT-PIPE-021 | `ExecuteAndCompare` | pass |
| `StructuralTransformSeparatorTests.NestedInsertedCloseBracesInFrontOfACommentBreakOnlyBehindTheOuterBrace` | FMT-STRUCT-026 | phase output, gap | fail: line break between the inner and the outer `}` |
| `StructuralTransformSeparatorTests.NestedInsertedCloseBracesInFrontOfADirectiveBreakOnlyBehindTheOuterBrace` (without and with `DEBUG`) | FMT-STRUCT-026 | phase output, gap | fail: same |
| `StructuralTransformSeparatorTests.NestedElseBodyCloseBraceInFrontOfTheOuterInsertedBraceGetsNoLineBreak` | FMT-STRUCT-026 | phase output, gap | fail: same |
| `StructuralTransformSeparatorTests.ThreeNestedInsertedCloseBracesGetNoLineBreak` | FMT-STRUCT-026 | phase output, gap | pass |
| `StructuralTransformSeparatorTests.CreatedReturnInFrontOfAnIdentifierGetsOneSpace` | FMT-STRUCT-026 | phase output | pass |
| `StructuralTransformSeparatorTests.CreatedReturnInFrontOfAUnicodeEscapeIdentifierGetsOneSpace` | FMT-STRUCT-026 | phase output | fail: `return\u0061;` |
| `StructuralTransformSeparatorTests.CreatedReturnInFrontOfAVerbatimIdentifierGetsNoSpace` | FMT-STRUCT-026 | phase output | fail: `return @a;` |
| `StructuralTransformSeparatorTests.CreatedReturnInFrontOfAVerbatimStringGetsNoSpace` | FMT-STRUCT-026 | phase output | fail: `return @"s";` |
| `StructuralTransformSeparatorTests.CreatedReturnInFrontOfAParenthesizedIdentifierGetsNoSpace` | FMT-STRUCT-026 | phase output | pass |
| `StructuralTransformSeparatorTests.CreatedReturnInFrontOfAStringGetsNoSpace` | FMT-STRUCT-026 | phase output | pass |
| `StructuralTransformSeparatorFinalOutputTests.EveryCreatedReturnEndsUpOneSpaceInFrontOfItsExpression` | FMT-STRUCT-026 | `AssertRuleResult` | fail: the reparse hook reads `return\u0061` as an expression statement |
| `StructuralTransformSeparatorTests.BlockCommentLeadingTheExpressionBehindAnAtArrowCommentKeepsLeadingIt` | FMT-STRUCT-026 | phase output, gap | fail: no line break in front of `/* d */` |
| `StructuralTransformSeparatorTests.OwnLineCommentLeadingTheExpressionBehindAnAtArrowCommentKeepsLeadingIt` | FMT-STRUCT-026 | phase output, gap | fail: same for `// d` |
| `StructuralTransformSeparatorTests.BlockCommentLeadingTheExpressionBehindACommentInFrontOfTheArrowKeepsLeadingIt` | FMT-STRUCT-026 | phase output, gap | fail: same |
| `StructuralTransformSeparatorTests.LocalFunctionCommentLeadingTheExpressionBehindAnAtArrowCommentKeepsLeadingIt` | FMT-STRUCT-026 | phase output, gap | fail: same |
| `StructuralTransformSeparatorTests.OperatorCommentLeadingTheExpressionBehindAnAtArrowCommentKeepsLeadingIt` | FMT-STRUCT-026 | phase output, gap | fail: same |
| `StructuralTransformSeparatorTests.ConversionCommentLeadingTheExpressionBehindAnAtArrowCommentKeepsLeadingIt` | FMT-STRUCT-026 | phase output, gap | fail: same |
| `StructuralTransformSeparatorTests.ExpressionStatementBehindAnAtArrowCommentKeepsItsLeadingComment` | FMT-STRUCT-026 | phase output, `FindDifference` | pass |
| `StructuralTransformSeparatorFinalOutputTests.MembersWithACommentAtTheArrowReachAStableOutput` | FMT-STRUCT-026 | `AssertSecondPassIsStable`; the final layout is unspecified (FMT-STRUCT-006) | fail: the reparse hook reports `/* d */` trailing `return` |
| `StructuralTransformSeparatorTests.ParenthesesAroundAThrowOperandOfAnAccessorCarryNoElasticTrivia` | FMT-STRUCT-026 | elastic scan | fail: `(` trailing, `)` leading |
| `StructuralTransformSeparatorTests.ParenthesesAroundAThrowOperandOfAGetOnlyPropertyCarryNoElasticTrivia` | FMT-STRUCT-026 | elastic scan | fail: same |
| `StructuralTransformSeparatorTests.ParenthesesAroundAThrowOperandOfAGetOnlyIndexerCarryNoElasticTrivia` (assignment operand) | FMT-STRUCT-026 | elastic scan | fail: same |
| `StructuralTransformSeparatorTests.SemicolonCreatedForAnEmptyTypeCarriesNoElasticTrivia` | FMT-STRUCT-026 | elastic scan | pass |
| `StructuralTransformSeparatorTests.TokensCreatedForAConvertedThrowBodyCarryNoElasticTrivia` | FMT-STRUCT-026 | elastic scan | pass |
| `ReihitsuFormatterNodeBraceTests.MethodBodyTargetGetsItsBracesPlaced` (the FMT-BREAK-002 example) | FMT-BREAK-002 | `FormatNodeInDocumentAsync` (LF and CRLF, second pass) and `FormatNode` on the attached target | fail: `{ Foo(); }` kept |
| `ReihitsuFormatterNodeBraceTests.MethodBodyTargetGetsItsClosingBracePlaced` | FMT-BREAK-002 | same | fail: `Foo(); }` kept |
| `ReihitsuFormatterNodeBraceTests.ConstructorBodyTargetGetsItsBracesPlaced` | FMT-BREAK-002 | same | fail |
| `ReihitsuFormatterNodeBraceTests.FinalizerBodyTargetGetsItsBracesPlaced` | FMT-BREAK-002 | same | fail |
| `ReihitsuFormatterNodeBraceTests.OperatorBodyTargetGetsItsBracesPlaced` | FMT-BREAK-002 | same | fail |
| `ReihitsuFormatterNodeBraceTests.LocalFunctionBodyTargetGetsItsBracesPlaced` | FMT-BREAK-002 | same | fail |
| `ReihitsuFormatterNodeBraceTests.EventAccessorListTargetGetsItsBracesPlaced` | FMT-BREAK-002 | same | fail: `{ add` / `} remove` / `} }` |
| `ReihitsuFormatterNodeBraceTests.AccessorBodyTargetGetsItsBracesPlaced` | FMT-BREAK-002 | same | pass |
| `ReihitsuFormatterNodeBraceTests.StatementBlockTargetGetsItsBracesPlaced` | FMT-BREAK-002 | same | pass |
| `RH3002…Tests.ReturnWithoutSpaceBeforeParenthesesAroundANumberGetsOneSpace` | RH3002-008 | `Verify` | pass |
| `RH3002…Tests.ReturnWithoutSpaceBeforeParenthesesAroundAUnicodeEscapeIdentifierGetsOneSpace` | RH3002-008 | `Verify` | pass |
| `RH3002…Tests.ReturnWithoutSpaceBeforeParenthesesAroundAVerbatimIdentifierGetsNoSpace` | RH3002-008 | `Verify` | pass |
| `RH3002…Tests.ReturnWithoutSpaceBeforeParenthesesAroundAVerbatimStringGetsNoSpace` | RH3002-008 | `Verify` | pass |
| `RH3002…Tests.ReturnWithoutSpaceBeforeParenthesesAroundAStringGetsNoSpace` | RH3002-008 | `Verify` | pass |

The full class name behind `RH3002…Tests` is `RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzerTests`.

## Each finding's class

- **#1 (comment leading the expression when a comment stands at the arrow).**
  - Failing shapes: every member kind through `CreateReturnStatement` (method, local function, operator, conversion) and every comment position the spec example names (`/* d */` behind `=> // c`, own-line `// d`, `/* c */ =>` on its own line).
  - Holding shapes, as the other side: the expression-statement form, and FMT-STRUCT-006's moved-comment path, which the existing `ExpressionBodyArrowCommentTests` cover.
  - The final layout of these members is left unspecified (open question 1 in `12-spec-author.md`). Its test therefore asserts stability and the round trip only.
- **#2 (nested inserted `}`).**
  - Failing shapes: a comment behind the outer brace; a directive behind it, with and without `DEBUG`; an inner `else` body.
  - Holding shape: three levels with no comment.
  - FC-026 covers the nesting itself.
- **#3 and A1 (would-merge).**
  - The spec names `42` and `a` (existing tests), `\u0061`, `(`, a string, `@a`, `@"s"`, and a comment (existing test). Each now has a phase-output test, and the final-output test covers the five examples A to E.
  - Agreement with RH3002's fix is covered on the same shapes as `return(…)` written tight. All five pass today, because RH3002 already asks the lexer. Once one owner serves both, these tests and the formatter's separator tests pin the same answers.
  - The list join, the third consumer, has no shape named in any spec. It is covered only by the existing `ListElementInteriorJoin…` tests and the reparse hook; I added no test for it.
- **#4 and A2 (node-level member-body braces).**
  - Failing targets: the body of a method (two shapes), a constructor, a finalizer, an operator, and a local function, and an event accessor list. Each is checked through `FormatNodeInDocumentAsync`, with a second pass, and through `FormatNode` on the attached node (FC-028).
  - Holding targets: a property accessor body and a statement block.
  - Not added:
    - A conversion-operator body: it is the same `IsMemberBody` path as the operator.
    - `FormatNodeInDocumentWithContextAsync`: the auditor reports it holds because the context is detached.
- **#5 (elastic trivia).**
  - The only remaining created-node call is `SyntaxFactory.ParenthesizedExpression` in `AccessorExpressionBodyTransform`. `rg -n "SyntaxFactory\.(Token|Block|ParenthesizedExpression|ReturnStatement|ExpressionStatement|ThrowStatement|ArrowExpressionClause)\(" src/Reihitsu.Formatter/Pipeline/StructuralTransforms` finds it alongside explicit-trivia tokens.
  - Covered: an accessor, a get-only property, and a get-only indexer, with a conditional and an assignment operand. Lambda and query operands reach the same single call site, so they need no test of their own.
  - Added for the class: the empty-type `;` and a converted `throw` body. Both pass.
- **#6 (moved raw-string token value).** The hook now compares values for every token of every fixture.
  - Failing: interpolated literals with the closing line in column 0 (`$` and `$$`).
  - Holding: indented closing, plain and UTF-8 literals in column 0, and a literal moved to the left.

## Failing at 21ab3a3, with the reason

The formatter project has 3372 tests, 27 of which fail. All 27 are tests added in this round, each failing for the reason in the table:

- 3 in the token-value class (#6);
- 3 nested `if` (#2);
- 4 `return` separator, 3 in the phase and 1 final (#3);
- 7 at-arrow, 6 in the phase and 1 stability (#1);
- 3 elastic parentheses (#5);
- 7 node-level braces (#4).

Other results at 21ab3a3:

- No existing test newly fails because the hook now compares token values.
- RH3002 filter in the analyzer project: 239/239 pass.
- Architecture project: 89/89 pass.

## Not covered, and why

- **Finding #7.** It is a change-doc text item.
- **The final layout of at-arrow members.** It is unspecified; see open question 1 in `12-spec-author.md`.
- **List-join shapes for the shared would-merge owner.** No spec names one. The existing join tests and the reparse hook will catch a regression.
- **The FMT-BREAK-029 reach.** It is unchanged by this round.

## Commands

```shell
scripts/test.sh --project formatter
scripts/test.sh --project formatter --filter "FullyQualifiedName~PhaseTreeRoundTripTests|FullyQualifiedName~StructuralTransformSeparator|FullyQualifiedName~ReihitsuFormatterNodeBraceTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3002"
scripts/test.sh --project architecture
```
