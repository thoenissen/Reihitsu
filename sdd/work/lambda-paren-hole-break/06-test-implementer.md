Role: test implementer
Commit: 07461be
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the spec examples, the change doc's "after" outputs, and the requirements' normative sentences. Where a test needed the line shape that unchanged requirements give outside a hole (two anonymous-method arguments, an empty anonymous method, a block comment behind the brace), I took that shape from `main`'s output for the same code outside a hole, which the maintainer's general clarification makes binding for holes. Only the columns come from FMT-INDENT-031.

## Tests

"Expected now" means on 07461be. Every failing test was checked to fail on the difference the change targets and on nothing else: a removed blank line, a strip-and-repaint caught by the phase check, an undone FMT-BREAK-021/022 break, or hole lines in column 0 or 13 where the spec puts them under the anchor.

`src/Reihitsu.Formatter.Test/Regression/LineBreaks/ParenthesizedLambdaLineStartTests.cs` (new)

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| SpecExampleAnonymousMethodParenthesisJoinsWhileLambdaArgumentKeepsItsLine | FMT-BREAK-011 | `AssertRuleResult` | fail: blank line removed |
| SpecExampleLambdaElementsOfInitializerAndCollectionExpressionStartTheirOwnLines | FMT-BREAK-011 | `AssertRuleResult` | fail: `{ (x) => x, (x) => x`, `[(x) => x,` |
| LambdaArgumentOnItsOwnLineIsLeftUnchanged | FMT-BREAK-011; FC-012/FC-010 | `AssertRuleResult` (phase check) | fail: `LineBreakPhase` changes the final output |
| BlankLineInFrontOfLambdaArgumentIsKept | FMT-BREAK-011, FMT-BREAK-030 | `AssertRuleResult` | fail: blank line removed |
| BlankLineBetweenLambdaElementsOfArrayInitializerIsKept | FMT-BREAK-011, FMT-BREAK-030 | `AssertRuleResult` | fail: blank line removed |
| BlankLineInFrontOfLambdaBehindExpressionLambdaArrowIsKept | FMT-BREAK-011, FMT-BREAK-030 | `AssertBlankLinesAbove` | fail: blank line removed |
| BlankLineInFrontOfLambdaBehindReturnIsKept | FMT-BREAK-011, FMT-BREAK-030 | `AssertBlankLinesAbove` | fail: blank line removed |
| LambdaBehindReturnWithoutBlankLineKeepsItsLineBreak | FMT-BREAK-011, FMT-BREAK-030; FC-003 | `AssertBlankLinesAbove` | fail: phase check (`LineBreakPhase`) |
| LineBreaksInFrontOfLambdaElementsOfCollectionsAreKept | FMT-BREAK-011 (change-doc entry 7, incl. the collection-expression blank-line row of entry 5) | `AssertRuleResult` | fail: entry 7's "before" |
| LambdaBehindAssignmentIsJoinedOntoTheAssignmentLine | FMT-BREAK-011; FC-003 | `AssertRuleResult` | pass |
| LambdaAsFirstArgumentIsJoinedOntoTheOpeningParenthesisLine | FMT-BREAK-011; FC-003 | `AssertRuleResult` | pass |
| LambdaBehindNamedArgumentColonIsJoinedOntoTheNameLine | FMT-BREAK-011; FC-003 | `AssertRuleResult` | pass |
| LambdaParenthesisBehindStaticModifierInArgumentIsJoinedOntoTheModifierLine | FMT-BREAK-011; FC-003 | `AssertRuleResult` | pass |
| CommentLineInFrontOfLambdaArgumentIsLeftUnchanged | FC-019, FC-021 (no tag) | `AssertRuleResult` | pass |
| DirectiveInFrontOfLambdaArgumentIsLeftUnchanged | FC-019, FC-020 (no tag) | `AssertRuleResult` | pass |

`src/Reihitsu.Formatter.Test/Regression/Indentation/AnonymousMethodHoleAlignmentTests.cs` (new)

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| AnonymousMethodInHoleOfAlignedLiteralIsAlignedUnderDelegate | FMT-INDENT-031 (change-doc entry 2) | `AssertRuleResult` | fail: column 0 |
| SpecExampleStatementsModifiersAndExpressionLambdaBodyInAlignedLiteral | FMT-INDENT-031 | `AssertRuleResult` | fail: column 0 |
| StatementsOfAnonymousMethodInHoleAreSeparatedInsideTheAlignedBlock | FMT-INDENT-031 (entry 3) | `AssertRuleResult` | fail: column 0 |
| StaticAnonymousMethodInHoleIsAlignedUnderDelegate | FMT-INDENT-031 (answer 4) | `AssertRuleResult` | fail: column 0 |
| AsyncAnonymousMethodInHoleIsAlignedUnderAsync | FMT-INDENT-031 (answer 6) | `AssertRuleResult` | fail: column 0 |
| StaticAsyncAnonymousMethodInHoleIsAlignedUnderAsync | FMT-INDENT-031 (lead decision 3) | `AssertRuleResult` | fail: column 0 |
| AnonymousMethodWithParameterListInHoleIsAlignedUnderDelegate | FMT-INDENT-031; FC-027 | `AssertRuleResult` | fail: column 0 |
| AnonymousMethodInExpressionLambdaBodyInHoleIsAlignedUnderItsDelegate | FMT-INDENT-031; FC-002/FC-003 (expression body) | `AssertRuleResult` | fail: column 0 |
| SpecExampleBlockLambdaInsideAnonymousMethodFollowsTheAlignedBlock | FMT-INDENT-031, FMT-INDENT-028 (entry 4) | `AssertRuleResult` | fail: column 0 |
| SpecExampleAnonymousMethodInsideBlockLambdaBodyOfTheSameHoleAndOutsideHoleKeepScopeDepth | FMT-INDENT-031; FC-003 (block body, outside hole) | `AssertRuleResult` | pass |
| AnonymousMethodInsideAnonymousMethodOfTheSameHoleTakesScopeDepthOfTheOuterBody | FMT-INDENT-031; FC-002 ("or of another anonymous method") | `AssertRuleResult` | fail: column 0 |
| AnonymousMethodInNestedLiteralInsideBlockLambdaOfOuterHoleIsAligned | FMT-INDENT-031; FC-002 ("innermost hole") | `AssertRuleResult` | fail: column 0 |
| AnonymousMethodInHoleOfSingleLineLiteralTakesScopeDepth | FMT-INDENT-031; FC-003 ("multi-line") | `AssertRuleResult` | pass |
| SeveralAnonymousMethodsInOneHoleAreEachAlignedUnderTheirDelegate | FMT-INDENT-031; FC-027 | `AssertRuleResult` | fail: column 0 |
| EmptyAnonymousMethodInHoleIsAlignedUnderDelegate | FMT-INDENT-031; FC-027 | `AssertRuleResult` | fail: column 0 |
| AnonymousMethodInHoleOfDollarDollarLiteralIsAlignedUnderDelegate | FMT-INDENT-031; FC-022 | `AssertRuleResult` | fail: column 0 |
| AuthorWrittenAnonymousMethodWithCommentsInMovedLiteralIsAlignedAndKeepsItsComments | FMT-INDENT-031; FC-021 | `AssertRuleResult` | fail: column 17 (moved by the literal's shift only) |
| BlockCommentBehindTheBraceOfAnonymousMethodInHoleStaysWithTheAlignedBrace | FMT-INDENT-031; FC-019/FC-021 | `AssertRuleResult` | fail: column 0 |
| AnonymousMethodInHoleOfLiteralMovedLeftIsAlignedUnderDelegate | FMT-INDENT-031; FC-003 (shift direction) | `AssertRuleResult` | fail: column 0 |

`src/Reihitsu.Formatter.Test/Unit/RawStringAlignment/RawStringInterpolationHoleTests.cs` (changed)

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| AnonymousMethodBrokenInsideHoleIsStableOnTheSecondPass: was `AssertSecondPassIsStable`, now asserts entry 1's "after" exactly; name kept because the change doc names it | FMT-PIPE-004, FMT-INDENT-031 (entry 1) | `AssertRuleResult` | fail: column 13 |
| ParenthesizedLambdaStartingAnUnplacedHoleLineKeepsItsColumn (new) | FMT-RAW-007, FMT-BREAK-011 (entry 6) | `AssertRuleResult` | fail: `(x) => x` in column 0 |

`src/Reihitsu.Formatter.Test/Unit/Pipeline/PhaseStabilityCheckTests.cs` (changed), with `IndentationStrippingPhase.cs` (new, a synthetic test phase)

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| CheckCatchesAPhaseWhoseChangeALaterPhaseWritesBack: replaces `CheckCatchesLineBreakPhaseStrippingIndentationOfParenthesizedLambdaArgument` (lead decision 2). The source is a fixed point of [strip, `IndentationPhase`], and the check reports only the strip phase, under LF and CRLF. The unused `AssertOnlyPhaseChanges` helper and the `LineBreaks` using went with the old test | none (tooling) | `FindChangingPhases` with injected phases | pass |

`src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH5417FunctionAndAccessorBodyBracesMustNotShareLineFormatterTests.cs` (changed)

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| VerifyFormatterFixesAnonymousMethodBodyInRawStringHole | FC-013 (no tag) | `VerifyFormatter(source, fixedSource, …)` | fail: formatter output column 0. The diagnostic is reported as marked |

Totals on 07461be:
- Formatter project: 4142 tests, of which exactly the 28 listed as failing above fail.
- Analyzer filter: 1 of 6 tests fails.
- Architecture project: green, so FMT-INDENT-031 is now covered.

## Not covered, and why

- **`PhaseReversalExceptionList` entries.** I did not change any. Five `LineStartIndentation` entries name fixtures with lambdas. An entry whose reversal disappears fails its own test as "Stale phase reversal entry", and which entries go can only be found by running after the change (02-skeptic.md). Deleting a stale entry is required by the list's design and the issue's "Done when"; it does not weaken an assertion. The lead decides whether the implementer deletes them or routes them back to me.
- **Lambda behind a modifier or return type on the next line, outside a list element** (`f = static⏎(int x) => x`). This is follow-up candidate 2 and unspecified. A test would lock today's statement-column output in. The list-element case is covered (FMT-BREAK-015).
- **Columns of a lambda continued after `return` or after an expression lambda's `=>`.** This is follow-up candidate 3, and no FMT-INDENT requirement decides that column. Those two tests therefore assert only the blank line (`AssertBlankLinesAbove`), so the column is not locked in.
- **A lambda in a conditional branch, or behind a binary operator.** The operator-wrap requirements own these gaps (FMT-BREAK-017 to FMT-BREAK-020). In their canonical shape the operator starts the line, so the lambda's `(` does not start it. No condition of this change reaches it.
- **Regular `$"…"` and verbatim `$@"…"` holes** (FC-022). FMT-INDENT-031 covers only multi-line raw literals and leaves these unspecified (CS8967 follow-up candidate 1). No test locks either in.
- **A directive inside a hole** (FC-020). A preprocessor directive cannot stand inside an interpolation hole, so no input exists. The directive case is covered on the lambda side.
- **An anonymous method in an object initializer inside a hole** (FC-027). Not written: the initializer's own layout inside a hole is not decided by this change, and I found no spec example to derive it from. The argument, nested-call, expression-lambda, and block-lambda enclosures are covered.
- **Node-level entry points (FC-028).** Spec author's note: a detached root cannot see the hole (FMT-PIPE-023), and this behavior is left unspecified there.
- **Code-fix convergence and Fix All (FC-007, FC-008).** No code fix changes. The RH5417 parity test covers the formatter-backed output in a hole.
- **FC-004, FC-005, FC-006, FC-011, FC-018, FC-023 to FC-026, FC-029, FC-036.** No join of user tokens, rename, or rewrite of syntax shape is added; the change only stops a strip and adds an alignment.
- **FC-031 to FC-035.** These apply to the implementer's and auditor's work, or to evidence handling. Expected values here were not taken from patched code (FC-035).
- **Tab variants of the raw-string matrix.** Not changed by this run: the anchor column is measured by the existing machinery FMT-RAW-007 uses, and none of the inputs carries a tab. Left-shift and no-shift cells are covered; right shift is covered by entry 1.

## Findings

- **Change-doc entry 5, columns behind `=>` and `return`.** The entry says "columns unchanged", that is, the statement's column. I assert only the blank line there, because no requirement decides that column (follow-up candidate 3). Nothing for the spec to change.
- No wrong spec example found. Every spec "after" block I used is consistent with the requirement text. The failures show only the targeted difference.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~ParenthesizedLambdaLineStartTests|FullyQualifiedName~AnonymousMethodHoleAlignmentTests|FullyQualifiedName~RawStringInterpolationHoleTests|FullyQualifiedName~PhaseStabilityCheckTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5417FunctionAndAccessorBodyBracesMustNotShareLineFormatterTests"
scripts/test.sh --project formatter --filter "FullyQualifiedName~PhaseReversal|FullyQualifiedName~ArgumentFormattingFullPipelineTests.FormatsMethodChainWithArguments|FullyQualifiedName~IndentationTests.LambdaBlockBodyInArgumentListIndentsCorrectly|FullyQualifiedName~IndentationTests.SwitchExpressionInsideLambdaBreaksAndAligns|FullyQualifiedName~MinimalApiAttributedLambdaSettlesInOnePass|FullyQualifiedName~LambdaParameterInWrappedArgumentStaysAligned"
scripts/test.sh --project architecture
```
