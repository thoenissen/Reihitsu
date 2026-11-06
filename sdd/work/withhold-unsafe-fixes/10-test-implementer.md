Role: test implementer
Commit: 2c806fb3
Verdict: #1257: tests written; #1279: tests written; #1281: tests written; #1282: nothing to do; #1417: nothing to do; #1422: nothing to do

Repair round 1, for audit findings 1–3 (`08-auditor.md`) and lead decisions 15–17 (`00-lead.md`), against the spec diff of 82129fca and maintainer answer 6 (A). My prompt restated the lead's decided rows. It carried no intended fix or verdict beyond them, so I had nothing to set aside. Expected values come from the amended spec examples (RH3004-006, RH3005-008, RH3101-005, RH3105-005, RH3106-002/-003, SHARED-FIX-002), never from the code.

"Fails" means the test fails on 2c806fb3 for the reason the requirement states. I inspected every failure diff. In the Fix All tests, the single and iterative passes already match. Only the `Fix all in document` pass differs, and it differs on the instance that must stay reported.

Class names below drop the `AnalyzerTests` suffix.

## thoenissen/Reihitsu#1257

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH3005UseReadableConditions.FixAllLeavesTheComparisonThatWouldCompleteAGenericReadingReported` | RH3005-008, SHARED-FIX-002 | Verify (single, iterative, Fix All; `MarkupMode.Allow`) | fails: Fix All writes `M(G < Max, B > (7))` |
| `RH3005UseReadableConditions.FixAllSwapsComparisonsThatTogetherStillReadAsComparisons` | RH3005-008, SHARED-FIX-002 | Verify | passes (holding: `M(G < Max, B > 7)`) |
| `RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExists.FixAllLeavesTheQualifierThatWouldCompleteAGenericReadingReported` | RH3101-005, SHARED-FIX-002 | Verify (`MarkupMode.Allow`) | fails: Fix All writes `M(A < B, C2 > (D))` |
| `RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExists.FixAllRemovesQualifiersThatTogetherStillReadAsComparisons` | RH3101-005, SHARED-FIX-002 | Verify | passes (holding: `M(A < B, C2 > D)`) |
| `RH3105DoNotPrefixLocalMembersWithThis.FixAllLeavesTheQualifierThatWouldCompleteAGenericReadingReported` | RH3105-005, SHARED-FIX-002 | Verify (`MarkupMode.Allow`) | fails: Fix All writes `M(A < B, C2 > (D))` |
| `RH3105DoNotPrefixLocalMembersWithThis.FixAllRemovesQualifiersThatTogetherStillReadAsComparisons` | RH3105-005, SHARED-FIX-002 | Verify | passes (holding) |

The inputs and the expected outputs are the spec's Fix All examples. Each holding test uses the same shape with `> D` / `> 7` in place of `> (…)`, which is the other side of the "followed by `(`" condition (FC-003). The verifier compiles every fixed state, so the holding outputs are proven to compile.

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| RH3005 `M(Max > G, (7) < B)` | LF | Fix All | reproduced | observed `M(G < Max, B > (7))`; expected `M(G < Max, (7) < B)`, second still reported |
| RH3101 `M(base.A < base.B, base.C2 > (base.D))` | LF | Fix All | reproduced | observed `M(A < B, C2 > (D))`; expected `M(A < B, base.C2 > (D))` |
| RH3105 the same with `this.` | LF | Fix All | reproduced | observed `M(A < B, C2 > (D))`; expected `M(A < B, this.C2 > (D))` |

Line endings play no part here: the rewrite is single-line, and the decision changes no layout.

## thoenissen/Reihitsu#1279

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH3004UseLambdaSyntax.FixKeepsTheLayoutOfTheStatementsItMoves` | RH3004-003, RH3004-006 | Verify | fails: `x + 1`, `if (x > 1)` with braces, blank lines collapsed |
| `RH3004UseLambdaSyntax.FixKeepsTheLayoutOfTheStatementsItMovesWithCarriageReturnLineFeed` | RH3004-006 | ApplyCodeFixAsync | fails (same difference, CRLF) |
| `RH3004UseLambdaSyntax.FixMovesTheStatementsOfAOneLineBlockOntoOneLine` | RH3004-006 | Verify | fails: statements split onto separate lines with a blank line, `x + 1` |
| `RH3004UseLambdaSyntax.FixLeavesStatementsTheFormatterWouldRewriteAsWritten` | RH3004-003, RH3004-006 | Verify | fails: expression-bodied local function → block body, embedded-statement braces, switch-section braces, anonymous object wrapped, three blank lines → one, token spacing |
| `RH3004UseLambdaSyntax.FixMovesAMultiLineDocumentationCommentWithTheStatements` | RH3004-006 | Verify | fails: `/// Text that continues` and `/// </summary>` left at column 12, and the local function's expression body rewritten |

The first and third tests are the two halves of the RH3004-006 example: `b`, and `c` per answer 6 (A). The fourth and fifth are the auditor's failing members of finding 3.

**Existing RH3004 tests against RH3004-006.** None contradicts it, and none was changed. I checked every RH3004 test that expects a block body: `AnonymousMethodWithBlockBodyIsConverted`, `CommentInsideSingleReturnBlockIsPreserved`, `DocumentationCommentedAnonymousMethodKeepsItsBody`, `FixWritesAnExpressionBodyOnlyForASingleReturnStatement`, `FixLaysOutABlockOnItsOwnLinesAsTheFormatter` (and its CRLF variant), and `FixOfAOneLineBlockMatchesTheFormatter`.
- Their blocks hold only statements the formatter leaves unchanged.
- Their expected statements keep their own text.
- Their lines move by one delta, so the first line stands one level inside braces at the anchor column.
- Statements from a one-line block stand on one line between the braces.

All of them pass on 2c806fb3. `FixOfAOneLineBlockMatchesTheFormatter` asserts that formatting changes nothing. It still holds for its formatter-clean block (`await Task.Delay(1);`), but it would not hold for a block like `c`. That follows from answer 6 (A) and is not a contradiction.

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| RH3004-006 example `b` | LF, CRLF | code fix | reproduced | statements reformatted; expected own layout, shifted +16 columns |
| RH3004-006 example `c` (answer 6 A) | LF | code fix | reproduced | `int y = x + 1;` / blank / `Console.WriteLine(y);`; expected `int y = x+1; Console.WriteLine(y);` on one line |
| local function, `if(…)`, unbraced switch section, anonymous object, 3 blank lines | LF | code fix | reproduced | see the test row |
| multi-line `///` in a moved block | LF | code fix | reproduced | continuation lines stay at their old column |

## thoenissen/Reihitsu#1281

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH3106UnnecessaryDelegateParenthesesShouldBeRemoved.AnonymousMethodsThatTakeTheirNaturalTypeAreNotReported` (spec example, amended) | RH3106-003 | Verify (analyzer) | fails: 4 reports (`(…)` under `var`, `new[]`, both lambda returns) |
| `….ParenthesizedAnonymousMethodInImplicitlyTypedVariableIsNotReported` | RH3106-003 | Verify (analyzer) | fails: reported |
| `….AnonymousMethodInAnImplicitlyTypedArrayCreationIsNotReported` | RH3106-003 | Verify (analyzer) | fails: reported |
| `….AnonymousMethodReturnedByTheExpressionBodyOfALambdaWithoutATargetIsNotReported` | RH3106-003 | Verify (analyzer) | fails: reported |
| `….AnonymousMethodReturnedByAReturnStatementOfALambdaWithoutATargetIsNotReported` | RH3106-003 | Verify (analyzer) | fails: reported |
| `….AnonymousMethodInAConditionalOrASwitchArmWithAnObjectTargetIsNotReported` | RH3106-003 | Verify (analyzer) | passes (holding) |
| `….AnonymousMethodWithADelegateTargetThroughParenthesesAnArrayOrALambdaIsReported` | RH3106-001, RH3106-003 | Verify (code fix) | passes (counter-examples: `Action e = (…)`, `new Action[] {…}`, `Func<Action> f = () => …`) |
| `….AnonymousMethodAsTheRightOperandOfCompetingUserDefinedOperatorsIsReportedWithoutACodeFix` | RH3106-002 | VerifyNoCodeFixIsOffered | fails: fix offered |
| `….AnonymousMethodAsTheLeftOperandOfCompetingUserDefinedOperatorsIsReportedWithoutACodeFix` | RH3106-002 | VerifyNoCodeFixIsOffered | fails: fix offered (sibling shape, FC-016) |
| `….AnonymousMethodAsAnOperandOfASingleUserDefinedOperatorIsFixed` | RH3106-002 | Verify (code fix) | passes (holding) |
| `….FixAllLeavesTheArgumentThatWouldMakeTheCallAmbiguousReported` | RH3106-002, SHARED-FIX-002 | Verify (`MarkupMode.Allow`) | fails: Fix All writes `Run(delegate { }, delegate { })` |
| `….FixAllRemovesTheListsOfArgumentsThatTogetherKeepTheBinding` | RH3106-002, SHARED-FIX-002 | Verify | passes (holding: only `Run(Action, Action)`) |

**Spec defect, for the spec author.** The RH3106-003 example in 82129fca has the line `var g = b ? delegate () { } : delegate () { };`, and that line does not compile: CS0173, "no implicit conversion between 'anonymous method' and 'anonymous method'". A switch expression with anonymous-method arms under `var` does not compile either: CS8506. So the example asserts a position that the test input cannot hold.
- I dropped that one line from the spec-example test. `AnonymousMethodsThatTakeTheirNaturalTypeAreNotReported` is otherwise the amended example verbatim.
- The conditional and switch-arm sentence of RH3106-003 is covered instead with an `object` target (`object g = b ? … : …;` and a returned `object` switch expression), which compiles.
- The auditor listed "`b ? … : …` and switch arms under `var`" as holding. Those inputs do not compile.
- Suggested spec edit: replace the line with `object g = b ? delegate () { } : delegate () { };`, or drop it.

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `var e = (delegate () { });`, `new[] { delegate () { } }`, `var f = () => delegate () { };`, `return delegate () { };` in a lambda without a target | LF | analyzer | reproduced | reported; expected not reported |
| `c + delegate () { }` / `delegate () { } + c` with two user-defined operators | LF | code fix | reproduced | fix offered; expected none |
| `Run(delegate () { }, delegate () { })`, overloads `(Action, Action)` / `(Action<int>, Action<int>)` | LF | Fix All | reproduced | `Run(delegate { }, delegate { })` (CS0121); expected the second list kept and reported |

## thoenissen/Reihitsu#1282

Nothing to do. No repair finding concerns it.

## thoenissen/Reihitsu#1417

Nothing to do. No repair finding concerns it.

## thoenissen/Reihitsu#1422

Nothing to do. No repair finding concerns it.

## Whole run

### Not covered, and why

- Fix All tests for RH3001, RH3004, RH3104, RH3107, and RH3201: the audit found their rewrites do not interact, and RH3201-006 already has its adjacent-comment Fix All test. The lead's row 15 names only RH3005, RH3101, RH3105, and RH3106.
- Audit finding 4 (`ParsesAsWritten` cost): this is performance only. No behavior changes, so no unit test asserts it, and a timing test would be flaky under CI.
- RH3004 CRLF: covered for example `b`. The other layout tests share the one fix path, `RH3004UseLambdaSyntaxCodeFixProvider.ApplyCodeFixAsync`, and differ only in the statements moved.
- RH3004 anchor `async` with moved statements: the column is the one `FixWritesAnExpressionBodyOnlyForASingleReturnStatement` already checks, and the moved-statement path does not depend on the anchor.
- `Run(() => delegate () { })` with `Run<T>(Func<T>)`: the spec leaves it undecided (09-spec-author follow-up).
- Failure classes: FC-008 (Fix All interaction), FC-003 (both sides of each changed condition), FC-016 (sibling operand side), FC-030 (fix formats only what it creates), and FC-033 (CRLF) apply and are covered above. No formatter phase, rename, or literal is touched, so FC-010–FC-014, FC-018/036, and FC-022 cannot reach this repair.

### Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3004UseLambdaSyntaxAnalyzerTests|FullyQualifiedName~RH3005UseReadableConditionsAnalyzerTests|FullyQualifiedName~RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsAnalyzerTests|FullyQualifiedName~RH3105DoNotPrefixLocalMembersWithThisAnalyzerTests|FullyQualifiedName~RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzerTests"
```

On 2c806fb3: 179 tests, 16 failed (RH3004 5, RH3005 1, RH3101 1, RH3105 1, RH3106 8). All 16 are the tests marked "fails" above, and every pre-existing test of these classes passes except the amended spec-example test.

## Amendment — rows 21–24

Commit: 38b5c74d. Verdict: tests written. Expected values come from the spec diff of d7b72d0b and fc43e244 (RH3106-003, RH3004-006 and its two new examples) and maintainer answer 7, never from the code. My prompt named the decided rows and the standing instruction and carried no intended fix or hoped-for verdict to set aside. "Fails" means the test fails on 38b5c74d for the reason the requirement states; I read every failure diff.

### thoenissen/Reihitsu#1281 (RH3106, rows 21 and 23)

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH3106UnnecessaryDelegateParenthesesShouldBeRemoved.AnonymousMethodsWithADelegateTypedOtherBranchArmOrElementUnderVarAreReported` (spec example, verbatim) | RH3106-003 | Verify (analyzer) | fails: 0 of 3 reported |
| `….AnonymousMethodInAConditionalWithADelegateTypedOtherBranchUnderVarIsReportedAndFixed` | RH3106-001, RH3106-003 | Verify (code fix) | fails: not reported |
| `….AnonymousMethodInASwitchArmWithADelegateTypedOtherArmUnderVarIsReportedAndFixed` | RH3106-001, RH3106-003 | Verify (code fix) | fails: not reported |
| `….AnonymousMethodInAnImplicitlyTypedArrayWithADelegateTypedOtherElementUnderVarIsReportedAndFixed` | RH3106-001, RH3106-003 | Verify (code fix) | fails: not reported |
| `….AnonymousMethodWhoseOtherBranchArmOrElementIsNotOfADelegateTypeIsNotReported` | RH3106-003 | Verify (analyzer) | passes (holding: the other branch, arm, or element is `new object()`; FC-003 other side of "whose type is a delegate type") |

The fixed outputs (`delegate { }` in each position) are the RH3106-001 fix; the verifier compiles them, so they are proven to compile. The holding members of the earlier round (`var a = delegate () { };`, `(…)` under `var`, `new[] { delegate () { } }`, lambda returns, `object g = b ? … : …`, object switch) are unchanged and still pass.

### thoenissen/Reihitsu#1279 (RH3004, rows 22 and 24, answer 7)

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH3004UseLambdaSyntax.FixKeepsConditionalDirectivesInColumnZeroAndShiftsTheStatements` (first spec example, `DEBUG` defined) | RH3004-006 | Verify with `DefineDebug` | fails: `#if`/`#else`/`#endif` at column 28, statements at column 40 |
| `….FixKeepsConditionalDirectivesInColumnZeroWhenTheFirstBranchIsDisabled` (same example, no symbol: the first statement is the `#else` one) | RH3004-006 | Verify | fails: same difference |
| `….FixKeepsConditionalDirectivesInColumnZeroWithCarriageReturnLineFeed` | RH3004-006 | ApplyCodeFixAsync (`DEBUG`) | fails: same difference, CRLF |
| `….FixMovesRegionDirectivesAndIndentedDirectivesWithTheStatements` (second spec example, `DEBUG` defined) | RH3004-006 | Verify with `DefineDebug` | fails: `#region`/`#endregion` at 28 and `#if`/statement at 40; expected 16 and 28 |
| `….FixKeepsANullableDirectiveInColumnZero` | RH3004-006 | Verify | fails: `#nullable` lines at column 31, statements at 43; expected 0 and 31 |
| `….FixMeasuresTheShiftOnTheFirstStatementAndNotOnACommentLineInFrontOfIt` (comment at column 4 in front of a statement at column 12) | RH3004-006 | Verify | fails: comment at 28, statement at 36; expected 20 and 28 |
| `….FixMeasuresTheShiftOfABlockWithoutAStatementOnItsFirstLine` (block holding only a comment) | RH3004-006 | Verify | passes (the other side of "in a block without a statement") |

Existing RH3004 tests: none holds a directive or a comment line in front of the first statement in another column, so none contradicts the amended RH3004-006; none was changed. The test class gained a private `DefineDebug` solution transform (the pattern of `RH7004UsingDeclarationsShouldNotBeUsedAnalyzerTests`).

### Findings

- RH3004-006 second example depends on `DEBUG`. Without the symbol, `Console.WriteLine(x);` is disabled text, the block has no statement, and the requirement as written measures on the block's first line, the `#region` line in column 0. That gives a shift of 28 columns, not the 16 the example shows. The spec does not say whether a statement in disabled text counts. I ran the example with `DEBUG` defined, where it is unambiguous, and wrote no test for the symbol-less reading. For the spec author.

### Decided on recommendation

- RH3004 `#nullable` test: answer 7 names conditional directives; the spec (row 24) extends column 0 to every directive the formatter keeps there. I tested one such directive, `#nullable`, as the representative of that list; `#pragma`, `#define`, and the others share the same condition in the requirement. No output question raised (standing instruction).
- RH3004 comment-line test: the spec says a comment line in front of the first statement does not decide the shift and moves with the statements. I asserted that the comment keeps its offset (−8) to the statement. No output question raised.

### Not covered, and why

- RH3106 Fix All across the three new positions: they do not interact (each list is in a separate statement), and the RH3106 Fix All scenario already covers several diagnostics in one document.
- RH3106 conditional under `var` with `null` or a lambda as the other branch: no natural type, CS0173; not an input.
- RH3004 directives other than `#if`/`#else`/`#endif`, `#region`/`#endregion`, and `#nullable`: they reach the same condition of RH3004-006 (one rule: column-0 directive other than region); one representative tested. Whether the implementation shares one call site is unknown before the repair, so this is a judgment, not a static proof.
- RH3004 CRLF: tested for the first example; the other directive tests share the line-moving path and differ only in which lines are column 0.

### Commands

```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH3004UseLambdaSyntaxAnalyzerTests|FullyQualifiedName~RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzerTests"
```

Result on 38b5c74d with these tests: Failed 10, Passed 74, Total 84 — exactly the ten tests marked "fails" above.
