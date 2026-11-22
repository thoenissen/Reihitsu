Role: test implementer
Commit: b4d741a
Verdict: #1633: tests written; #1634: tests written; #1647: tests written; #1685: tests written; #1542: tests written; #1929: tests written

My prompt carried no intended fix, suspected cause, or hoped-for verdict. The issue texts and handovers still carry one (#1929's `PatternAnchor` fallback), and I did not use it. Every expected value comes from the spec examples, the change docs' *After* blocks, or the requirement text (marked *derived* below). For inputs the run leaves unchanged, the expected value is today's output, which `scripts/format.sh` gives on this head (src equals `origin/main`) and which the specs keep. No value comes from patched code (FC-035).

Each formatter test uses `FormatterTestsBase.AssertRuleResult`. It formats under LF and CRLF, asserts a second pass is identical, and runs `PhaseStabilityCheck` (FC-010, FC-012, FC-017). Each code-fix test uses `Verify(source, fixedSource, …)`. It runs the fix to convergence and re-analyzes (FC-007).

Every test that should fail before the change does, and only on the changed columns or joins. I checked this with a unified diff of expected against actual for all 35 formatter failures and both analyzer failures. Every test for unchanged behavior passes.

## thoenissen/Reihitsu#1633

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `BaseListColonJoinTests.SpecExampleFirstBaseTypeWrappedBehindTheColonJoinsTheColonsLine` | FMT-BREAK-042 | `AssertRuleResult` | fail (base types stay below the colon) |
| `BaseListColonJoinTests.SpecExampleCommentBehindTheColonKeepsTheFirstBaseTypeOnItsOwnLine` | FMT-BREAK-042; FC-019, FC-021 | `AssertRuleResult` (unchanged) | pass |
| `BaseListColonJoinTests.BlockCommentBehindTheColonKeepsTheFirstBaseTypeOnItsOwnLine` | FMT-BREAK-042; FC-021 | `AssertRuleResult` (unchanged) | pass |
| `BaseListColonJoinTests.CommentOnItsOwnLineBetweenTheColonAndTheFirstBaseTypeBlocksTheJoin` | FMT-BREAK-042; FC-019 | `AssertRuleResult` (unchanged) | pass |
| `BaseListColonJoinTests.DirectiveBetweenTheColonAndTheFirstBaseTypeBlocksTheJoin` | FMT-BREAK-042; FC-020 | `AssertRuleResult` (unchanged) | pass |
| `BaseListColonJoinTests.ColonOnItsOwnLineAndBlankLinesBehindTheColonAreJoined` | FMT-BREAK-042; FC-027 | `AssertRuleResult` | fail |
| `BaseListColonJoinTests.FirstBaseTypeInFrontOfAConstraintClauseAndAPrimaryConstructorBaseTypeJoinTheColonsLine` | FMT-BREAK-042; FC-027 | `AssertRuleResult` | fail |
| `BaseListColonJoinTests.FirstBaseTypeOfNestedInterfaceAndRecordStructJoinsTheColonsLine` | FMT-BREAK-042; FC-027 | `AssertRuleResult` | fail |
| `BaseListColonJoinTests.BaseListBehindItsColonStaysAsWritten` | FMT-BREAK-042; FC-003 (valid code stays) | `AssertRuleResult` (unchanged) | pass |
| `BaseListColonJoinTests.SpecExampleCommaThatStartsALineInABaseListEndsTheBaseTypesLine` | FMT-BREAK-038 | `AssertRuleResult` | fail |
| `BaseListColonJoinTests.SeveralCommasThatStartALineInABaseListMoveAlsoBehindAJoinedFirstBaseType` | FMT-BREAK-038, FMT-BREAK-042 | `AssertRuleResult` | fail |
| `BaseListColonJoinTests.BlockCommentBehindACommaThatStartsALineInABaseListMovesWithTheComma` | FMT-BREAK-038; FC-021 | `AssertRuleResult` | fail |
| `RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzerTests.VerifyFixOfClassWithBaseListWrappedBehindItsColonJoinsTheFirstBaseType` | RH5412-001, FMT-BREAK-042; FC-028 | `Verify` | fail (`class Example :⏎    I1;`) |

Derived (not in a spec example): the constraint clause keeps its own line at column 4 behind a joined base type (`class G<T> : I1⏎    where T : class`). The comment behind a leading comma follows FMT-BREAK-038's block-comment sentence (`class U : I1, /* c */⏎          I2`).

### Existing tests changed or left
- `ContinuationJoinTests.LineBreakBehindTheColonOfABaseListIsKept` (FMT-BREAK-041) is deleted. It asserted the line break behind a base list's `:` stays, which FMT-BREAK-042 now joins. Its other side, a comment behind the colon that keeps the line break, is in `BaseListColonJoinTests`. The doc comment of `GapsOutsideTheListKeepTheirLineBreak` no longer names the base list.
- Left unchanged: `RH6021ColonsMustBeSpacedCorrectlyAnalyzerTests` (`class E :⏎    B`, `class F :⏎    B`, `TestClass :`). RH6021's fix does not go through the formatter (skeptic). `RH5043CommentsMustNotBePlacedInsideBaseListAnalyzerTests` runs the analyzer only. No other test project holds a base list wrapped behind its colon or a comma-first base list. I searched `tests/` for a declaration line ending in `:` and for a line starting with `, ` below a declaration.

### Not covered, and why
- Fix All: RH5412 already has `GetFixAllScenario`, and the base-list join adds no new interaction between diagnostics.
- RH5413–RH5416 and RH5418: they reach the same formatter call as RH5412 for a type declaration. One fix is enough to show the node-level path. Each fix has its own provider, so this is no single-call-site proof. I record it as a deliberate narrowing, not as proven.

## thoenissen/Reihitsu#1634

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ContinuationLineIndentationTests.SpecExampleLinesACommentKeepsInsideABaseListATypeArgumentListOrAQualifiedNameAreContinuationLines` | FMT-INDENT-034 | `AssertRuleResult` | pass (characterization; no output changes) |
| `ContinuationLineIndentationTests.FirstElementACommentKeepsBelowTheOpeningBracketOfOtherAngleBracketListsIsAContinuationLine` | FMT-INDENT-034; FC-027 | `AssertRuleResult` | pass |

The second test covers a type parameter list (`class D< // c⏎    T>`), which takes the continuation column. It also covers a function-pointer parameter list inside a parameter, which keeps FMT-INDENT-013's list-element column (column 18), as FMT-INDENT-034's clarification says. Both are today's output.

### Not covered, and why
- The retired FMT-INDENT-036, -037, and -038 are not tagged.
- The no-comment qualified-name wraps (casts, `typeof`, using aliases) are out of scope after decision 3. They are a follow-up candidate already recorded in `00-lead.md`.

## thoenissen/Reihitsu#1647

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `PatternClosingDelimiterCommentTests.SpecExampleCommentBeforeClosingDelimiterOfAPatternIsIndentedIntoItsContent` | FMT-INDENT-006 | `AssertRuleResult` | fail (list, positional, parenthesized) |
| `PatternClosingDelimiterCommentTests.CommentBeforeClosingBracketOfListPatternAndClosingParenthesisOfParenthesizedPatternIsIndented` | FMT-INDENT-006 | `AssertRuleResult` | fail (change doc input) |
| `PatternClosingDelimiterCommentTests.LineAndDelimitedCommentBeforeClosingBracketOfListPatternAreIndented` | FMT-INDENT-006; FC-021 | `AssertRuleResult` | fail |
| `PatternClosingDelimiterCommentTests.CommentBeforeClosingBracketOfNestedListPatternIsIndented` | FMT-INDENT-006; FC-027 | `AssertRuleResult` | fail |
| `PatternClosingDelimiterCommentTests.CommentBeforeClosingBracketOfListPatternInCaseLabelAndSwitchArmIsIndented` | FMT-INDENT-006; FC-027 | `AssertRuleResult` | fail |
| `PatternClosingDelimiterCommentTests.CommentBeforeSliceThatEndsListPatternStaysInTheElementsColumn` | FMT-INDENT-006; FC-003 | `AssertRuleResult` (unchanged) | pass |
| `PatternClosingDelimiterCommentTests.CommentBeforeClosingParenthesisOfArgumentListKeepsTheParenthesisColumn` | FMT-INDENT-006; FC-003 (other side of the cause) | `AssertRuleResult` (unchanged) | pass |

### Existing tests changed or left
- `CommentBeforeBraceBlankLineTests.RemovesBlankLineBetweenCommentAndEveryNamedBraceOrBracket` (FMT-BLANK-020): in the expected output, `// end pattern` in front of a list pattern's `]` moves from the `]` column to the elements' column.
- Left unchanged, because they assert only blank lines and the second pass: `FirstCommentBehindOpeningTokenTests.AuthorsBlankLineBehindOpeningBracketOfEmptyListPatternIsKept`, and the parenthesized-pattern fixtures in `CommentBeforeBraceBlankLineTests` (`PlaceCommentLines`), `GluedCommentBlankLineTests`, `MultiLineGluedCommentBlankLineTests`, `ClosingBracketBlankLineTests`, and `DirectiveBeforeEndRegionBlankLineTests`.

### Not covered, and why
- RH5306's fix: RH5306 does not report a list pattern for this comment alone (change doc, observed by the spec author). The fix reaches the comment only through another finding.

## thoenissen/Reihitsu#1685

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ContinuationLineIndentationTests.SpecExampleWhenConditionWrappedBehindWhenIsIndentedOneLevelDeeperThanCase` | FMT-INDENT-034 | `AssertRuleResult` | pass (characterization; `main` already gives the output) |
| `ContinuationLineIndentationTests.WhenConditionWrappedBehindWhenTakesTheContinuationColumnWhereverItIsWritten` | FMT-INDENT-034; FC-027 | `AssertRuleResult` | pass |

The second test covers a condition written in column 0, one already indented, and one behind `> 4 and < 10`.

### Not covered, and why
- The code-fix path is already pinned by `RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzerTests` (skeptic), so there is no new test for it.

## thoenissen/Reihitsu#1542

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `PositionalPatternLayoutTests.SpecExampleWrappedPositionalPatternGetsOneSubpatternPerLine` | FMT-BREAK-022 | `AssertRuleResult` | fail |
| `PositionalPatternLayoutTests.SpecExamplePositionalPatternIndentsFromItsOpeningParenthesis` | FMT-INDENT-025 | `AssertRuleResult` | fail |
| `PositionalPatternLayoutTests.WrappedPositionalPatternsAreLaidOutLikeListPatternsInEveryPlaceAPatternStands` | FMT-BREAK-022, FMT-INDENT-025 | `AssertRuleResult` | fail (change doc / question 7 input: `is`, type, comma-first, arm, `case`) |
| `PositionalPatternLayoutTests.SpecExamplePositionalPatternClauseJoinsTheLineOfItsType` | FMT-BREAK-043 | `AssertRuleResult` | fail |
| `PositionalPatternLayoutTests.BlankLinesBetweenTheTypeAndThePositionalClauseGoWithTheJoin` | FMT-BREAK-043; FC-027 | `AssertRuleResult` | fail |
| `PositionalPatternLayoutTests.CommentBetweenTheTypeAndThePositionalClauseBlocksTheJoin` | FMT-BREAK-043; FC-019, FC-021 | `AssertRuleResult` (unchanged) | pass |
| `PositionalPatternLayoutTests.DirectiveBetweenTheTypeAndThePositionalClauseBlocksTheJoin` | FMT-BREAK-043; FC-020 | `AssertRuleResult` (unchanged) | pass |
| `PositionalPatternLayoutTests.SpecExampleCommaThatStartsALineInAPositionalPatternEndsTheSubpatternsLine` | FMT-BREAK-038 | `AssertRuleResult` | fail |
| `PositionalPatternLayoutTests.BlockCommentBehindACommaThatStartsALineInAPositionalPatternMovesWithTheComma` | FMT-BREAK-038; FC-021 | `AssertRuleResult` | fail |
| `PositionalPatternLayoutTests.SpecExampleMultiLinePositionalPatternNextToACombinatorWrapsTheChain` | FMT-BREAK-017 | `AssertRuleResult` | fail |
| `PositionalPatternLayoutTests.SingleLinePositionalPatternsNextToACombinatorKeepTheChainOnOneLine` | FMT-BREAK-017; FC-003 | `AssertRuleResult` (unchanged) | pass |
| `PositionalPatternLayoutTests.WrappedPositionalPatternBehindNotKeepsItsOpeningParenthesisOnTheNotLine` | FMT-BREAK-022, FMT-INDENT-025; FC-027 | `AssertRuleResult` | fail |
| `PositionalPatternLayoutTests.WrappedPositionalPatternWithDesignationNamesOrPropertyClauseGetsOneSubpatternPerLine` | FMT-BREAK-022, FMT-INDENT-025; FC-027 | `AssertRuleResult` | fail |
| `PositionalPatternLayoutTests.NestedWrappedPositionalPatternsIndentFromTheirOwnOpeningParenthesis` | FMT-BREAK-022, FMT-INDENT-025; FC-027 | `AssertRuleResult` | fail |
| `PositionalPatternLayoutTests.WrappedParenthesizedVariableDesignationIsNotLaidOutAsAPositionalPattern` | FMT-BREAK-022; FC-003 (other side of the construct) | `AssertRuleResult` (unchanged) | pass |
| `PositionalPatternLayoutTests.CommentBeforeClosingParenthesisOfWrappedPositionalPatternIsIndentedIntoTheSubpatterns` | FMT-INDENT-006 | `AssertRuleResult` | fail (change doc `var e`) |

Derived from the requirement text, not from a spec example:
- Behind `not`, the `(` stays on the `not` line (FMT-BREAK-041 join; FMT-BREAK-022).
- A designation (`) p;`) and a one-line property clause (`) { X: 1 };`) stay behind the `)`. Only the positional clause spans several lines, and a property pattern clause written on one line stays on one line (FMT-BREAK-022).
- A nested positional pattern and a list-pattern element `(` start their own line as first elements, and each indents from its own `(` (FMT-INDENT-025).
- Named subpatterns are elements like any other.

### Existing tests changed or left
- `RecursivePatternAlignmentTests.RecursivePatternWithPositionalClauseAlignsBracesToIsKeyword` pins a one-line positional clause in front of a wrapped property clause. It stays (skeptic), and I did not change it.
- Left unchanged, because they assert only blank lines and the second pass: `FirstCommentBehindOpeningTokenTests` (`p is (⏎// positional⏎1, 2)`, `t is (⏎⏎// text⏎{ Length: 0 }, 1)`, `t is (⏎⏎{ Length: 0 }, 1)`). They pass today. After the change, a wrapped positional clause gets one subpattern per line. The asserted blank lines are FMT-BLANK's and should hold, but the implementer must re-run them, since their fixtures now change layout.
- Left unchanged, analyzer only: `RH5020…Tests` and `RH5038…Tests.VerifyNoDiagnosticForPositionalPatternElement`.
- `UnsupportedListSeparatorCommentTests` already refuses a comment in front of a positional pattern's comma (FMT-UNSUP-009, `DisplayName = "Positional pattern"`), so I added no new test for it.

### Not covered, and why
- A code-fix path (FC-028): no fix formats a positional pattern on its own (change doc). The RH5306 fix reaches a positional pattern only inside a reported list pattern. The whole-document list-element case covers the same layout.
- `case (1,⏎2) when …`: the `when` clause is FMT-INDENT-034's, and the positional part is the same call as the `case` test.

## thoenissen/Reihitsu#1929

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `PatternAnchorBehindWrapperTests.SpecExampleBracesBehindAndOrNotStandUnderTheOutermostPatternsFirstToken` | FMT-INDENT-026 | `AssertRuleResult` | fail (arm, list element, three `case` labels) |
| `PatternAnchorBehindWrapperTests.BracesBehindOrInASwitchArmStandUnderTheArmsFirstToken` | FMT-INDENT-026; FC-027 | `AssertRuleResult` | fail |
| `PatternAnchorBehindWrapperTests.ParenthesizedPatternBehindNotInASwitchArmStandsUnderTheNot` | FMT-INDENT-026; FC-027 | `AssertRuleResult` | fail (change doc member) |
| `PatternAnchorBehindWrapperTests.BracesBehindNotInASliceStandUnderTheNot` | FMT-INDENT-026; FC-027 | `AssertRuleResult` | fail (change doc member; `{` under `not`, column 27) |
| `PatternAnchorBehindWrapperTests.BracesBehindNotInAListPatternElementOfACaseLabelStandUnderTheNot` | FMT-INDENT-026; FC-027 | `AssertRuleResult` | fail |
| `PatternAnchorBehindWrapperTests.PropertyPatternsWithoutAWrapperKeepTheirAnchors` | FMT-INDENT-026; FC-003 | `AssertRuleResult` (unchanged) | pass (`case P {`, `case (P and {`, `P {` and `{` at an arm's start) |
| `RH5206SwitchExpressionBracesShouldBeAnchoredAnalyzerTests.VerifyFixPutsBracesOfPropertyPatternBehindAndOrNotUnderTheArmsFirstToken` | RH5206-003, FMT-INDENT-026; FC-028 | `Verify` | fail (braces at column 16) |

The RH5206 fixture switches on a `P` rather than on an `object`. That keeps `not { A: 3, … }` compilable for the verifier, and the layout does not depend on it.

### Existing tests changed or left
The skeptic named some of these tests; my search found the rest. All were updated to option 8A: the content of a `case`-label property pattern behind `and`, `or`, or `not` goes four columns right of the outermost pattern's first token, and its `}` goes under that token.
- `LineBreakRequirementTests.SpecExampleCaseLabelIntroducesItsPatternThroughCombinatorsAndNot` (FMT-BREAK-022 example).
- `ContinuationJoinTests.SpecExampleBraceOfAPropertyPatternBehindNotInACaseLabelJoinsTheNot` and `SpecExampleBraceWrittenBehindNotGivesTheSameOutput` (FMT-BREAK-041 example).
- `PatternChainLayoutTests.SpecExampleBraceOfAPropertyPatternInACaseLabelJoinsItsCombinator` and `SpecExampleBraceWrittenBehindItsCombinatorGivesTheSameOutput` (FMT-BREAK-017 example; the skeptic did not name these).

Each now matches its spec example's output block. Searched with `rg -U "case [^\n]*(and|or|not) \{\n"` and `rg -U "^\s+(and|or|…) \{\n"` over `tests/`. Wrapper-then-brace lines in `PatternChainLayoutTests` and `RecursivePatternAlignmentTests` are `is` expressions, anchored to `is`, and do not change. No analyzer or CLI test holds the shape.

### Not covered, and why
- Arm × `and not` and arm × `or not`: the shared condition is the wrappers climbed to the arm parent. The spec example covers `and`, `not`, and `or`, and the `case` labels cover `and not` and `or not`. Both run through the same anchor reading, but I give no `rg` proof of a single call site. A deliberate narrowing.

## Whole run

- `scripts/test.sh --project architecture`: 198 passed. `EveryRequirementOfAnEnforcedSpecIsCovered` passes, since FMT-BREAK-042 and FMT-BREAK-043 are now tagged.
- `scripts/test.sh --project formatter`: 35 failed, 7139 passed. The 35 are exactly the tests marked "fail" above plus the 5 updated `case`-label tests and the updated blank-line test. Each failure differs from expected only in the joins or columns under change.
- `scripts/test.sh --project analyzer` (default run): 2 failed (the RH5412 and RH5206 tests above), 11935 passed.
- `scripts/test.sh --project cli`: 440 passed, 1 skipped. `--project core`: 812 passed.
- Failure classes walked and not applicable:
  - FC-004/005: no move of comments beyond FMT-BREAK-038's existing carry, which is tested.
  - FC-006, FC-022 to FC-026, FC-036: no rewrite of literals, names, or members.
  - FC-008/009: no new code-fix action.
  - FC-011: no new loop.
  - FC-013/014: no rule reports these layouts. RH5206 is covered by its fix test, and RH5306 is unchanged.
  - FC-018: no rename.
  - FC-029: no key/value layout.
  - FC-038 to FC-043: no shape is declared unsupported in this run.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~BaseListColonJoinTests|FullyQualifiedName~PositionalPatternLayoutTests|FullyQualifiedName~PatternAnchorBehindWrapperTests|FullyQualifiedName~PatternClosingDelimiterCommentTests|FullyQualifiedName~ContinuationLineIndentationTests|FullyQualifiedName~ContinuationJoinTests|FullyQualifiedName~LineBreakRequirementTests|FullyQualifiedName~PatternChainLayoutTests|FullyQualifiedName~CommentBeforeBraceBlankLineTests|FullyQualifiedName~FirstCommentBehindOpeningTokenTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzerTests|FullyQualifiedName~RH5206SwitchExpressionBracesShouldBeAnchoredAnalyzerTests"
scripts/test.sh --project architecture --filter "FullyQualifiedName~RequirementCoverageTests"
```

## Follow-up candidates

_None._
