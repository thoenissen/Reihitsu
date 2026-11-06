Role: test implementer
Commit: 57e119d2
Verdict: #1257: tests written; #1279: tests written; #1281: tests written; #1282: nothing to do; #1417: nothing to do; #1422: nothing to do

Repair round 2, for audit findings 5–9 (`14-auditor.md`) and lead decisions 26–33 (`00-lead.md`). Expected values come from the spec diff `09a5cd28..57e119d2` (RH3004-003/-006/-007, RH3106-002/-003, SHARED-FIX-002) and `15-spec-author.md` with its amendments, never from the code. My prompt restated the lead's decided rows and carried no intended fix, suspected cause, or hoped-for verdict, so I had nothing to set aside.

"Fails" means the test fails on 57e119d2 for the reason the requirement states. I read every failure message and diff. Class names drop the `AnalyzerTests` suffix.

## thoenissen/Reihitsu#1257

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH3005UseReadableConditions.FixAllSwapsAComparisonNestedInTheOperandOfAnotherInTheSameApplication` | SHARED-FIX-002 | Verify (incremental 2, Fix All 1 iteration) | fails: Fix All needs 2 iterations (the inner `1 < x` stays reported after the first) |
| `RH3005UseReadableConditions.FixAllSwapsAComparisonNestedInTheOperandOfAnEqualityInTheSameApplication` | SHARED-FIX-002 | Verify (same) | passes (holding: `2 == (3 > x ? 4 : 5)` → `(x < 3 ? 4 : 5) == 2`, the auditor's holding shape) |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `var b = 1 < (1 < x ? 2 : 3);` | LF | Fix All | reproduced | one application leaves `1 < x` reported; expected `(x > 1 ? 2 : 3) > 1` in one application |

## thoenissen/Reihitsu#1279

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH3004UseLambdaSyntax.FixAllRewritesAnAnonymousMethodNestedInTheBlockOfAnotherInTheSameApplication` (SHARED-FIX-002 example, verbatim) | RH3004-006, SHARED-FIX-002 | Verify (incremental 2, Fix All 1 iteration) | fails: Fix All needs 2 iterations |
| `RH3004UseLambdaSyntax.FixAllCorrectsEveryDiagnostic` (base test; scenario changed from `NumberOfFixAllIterations = 2` to `1`) | SHARED-FIX-001, SHARED-FIX-002 (base tags) | FixAllScenario | fails: 2 iterations. The curried `return delegate (int x) { return delegate (int y) … }` is a nested instance SHARED-FIX-002 now covers in one application |
| `….FixWithheldForABlockHoldingAMultiLineRawStringLiteral` (spec example) | RH3004-007 | VerifyNoCodeFixIsOffered | fails: fix offered |
| `….FixWithheldForABlockHoldingAMultiLineRawStringLiteralWithAWhitespaceOnlyLine` (finding 7 input, 22-space line, closing quotes at 16; `·` markers) | RH3004-007 | VerifyNoCodeFixIsOffered | fails: fix offered |
| `….FixWithheldForABlockHoldingAMultiLineInterpolatedRawStringLiteral` (`$"""`) | RH3004-007 | VerifyNoCodeFixIsOffered | fails: fix offered |
| `….FixWithheldForABlockHoldingAMultiLineUtf8RawStringLiteral` (`"""u8`) | RH3004-007 | VerifyNoCodeFixIsOffered | fails: fix offered |
| `….FixWithheldForASingleReturnOfAMultiLineRawStringLiteral` (expression-body path) | RH3004-007 | VerifyNoCodeFixIsOffered | fails: fix offered |
| `….FixRewritesABlockHoldingARawStringLiteralOnOneLine` (spec example) | RH3004-006, RH3004-007 | Verify | passes (other side: one-line raw string) |
| `….FixKeepsTheContinuationLineOfAMultiLineVerbatimStringInItsColumns` | RH3004-006, RH3004-007 | Verify | passes (other side: multi-line verbatim string, continuation line kept in column 0) |
| `….FixMovesTheLinesAfterASharedOpeningBraceLineSoTheLeftmostStandsInTheStatementColumn` (spec `Run` example) | RH3004-006 | Verify | fails: `if` at column 0, `Foo(x + 1);` at 1 |
| `….FixMovesTheLinesAfterASharedOpeningBraceLineSoTheLeftmostStandsInTheStatementColumnWithCarriageReturnLineFeed` | RH3004-006 | ApplyCodeFixAsync (CRLF) | fails: same difference, CRLF |
| `….FixKeepsTheLinesAfterASharedOpeningBraceLineRelativeToTheFirstStatement` (spec `Foo(x,` continuation example) | RH3004-006 | Verify | passes (other side: no later line left of the first statement) |
| `….FixMovesALaterStatementInFrontOfAClosingBraceOnItsLineIntoTheStatementColumn` (finding 8 second input) | RH3004-006 | Verify | fails: `Foo(x + 1);` at column 0; expected 28 |
| `….FixKeepsAColumnZeroCommentLineAfterASharedOpeningBraceLineInColumnZero` | RH3004-006 | Verify | fails: `Foo(x + 1);` at column 0; expected 16 (comment at 0 does not count for the leftmost line) |
| `….FixKeepsAColumnZeroCommentLineInColumnZero` (spec, row 33, comment at 0) | RH3004-006 | Verify | passes (the clamp happens to give the specified output) |
| `….FixMovesACommentLineInAnotherColumnByTheMeasuredShift` (spec, comment at 8) | RH3004-006 | Verify | passes (other side: no line crosses column 0) |
| `….FixMovesTheLinesOfABlockSoTheLeftmostStandsInTheStatementColumnWhenTheShiftWouldCrossColumnZero` (spec, comment at 2) | RH3004-006 | Verify | fails: comment clamped at 0 and `Foo();` at 16; expected comment 16, `Foo();` 34 |
| `….FixKeepsAColumnZeroDirectiveLineInColumnZeroWhenTheShiftMovesTheLinesLeft` (`#if DEBUG`/`#endif` at 0, shift −4, `DEBUG` defined) | RH3004-006 | Verify with `DefineDebug` | passes (column-0 directive neither moves nor counts) |
| `….FixMovesADirectiveLineInAnotherColumnIntoTheStatementColumnWhenTheShiftWouldCrossColumnZero` (`#region`/`#endregion` at 2, shift −4) | RH3004-006 | Verify | fails: directives clamped at 0, `Foo();` at 16; expected 16 and 34 |
| `….FixMeasuresTheShiftOnAnActiveStatementInFrontOfAStatementInAnotherColumn` (spec disabled-before-active example, `DEBUG` defined; finding 9 b) | RH3004-006 | Verify with `DefineDebug` | passes |
| `….FixMeasuresTheShiftOnADisabledStatementInFrontOfAStatementInAnotherColumn` (same example, no symbol, verbatim) | RH3004-006 | Verify | passes |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| nested `Action<int>` anonymous methods (SHARED-FIX-002 example) | LF | Fix All | reproduced | inner stays reported after one application |
| block with a multi-line raw string (plain, whitespace-only line, `$`, `u8`, `return`) | LF | code fix | reproduced | fix offered; expected none |
| first statement on the `{` line, later lines shallower | LF, CRLF | code fix | reproduced | later lines clamped to columns 0–1 |
| first statement on its own line, shift −4, comment or `#region` at column 2 | LF | code fix | reproduced | those lines clamped at 0 |

## thoenissen/Reihitsu#1281

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH3106UnnecessaryDelegateParenthesesShouldBeRemoved.CompoundAssignmentWithCompetingStaticOperatorsIsReportedWithoutACodeFix` (finding 5 a) | RH3106-002 | VerifyNoCodeFixIsOffered | fails: fix offered |
| `….CompoundAssignmentWithASingleStaticOperatorIsFixed` | RH3106-002 | Verify | passes (other side) |
| `….CompoundAssignmentWithCompetingInstanceCompoundAssignmentOperatorsIsReportedWithoutACodeFix` (finding 5 b, C# 14) | RH3106-002 | VerifyNoCodeFixIsOffered | fails: fix offered |
| `….CompoundAssignmentWithASingleInstanceCompoundAssignmentOperatorIsFixed` | RH3106-002 | Verify | passes (other side) |
| `….CollectionInitializerElementWithCompetingAddMethodsIsReportedWithoutACodeFix` (finding 5 c) | RH3106-002 | VerifyNoCodeFixIsOffered | fails: fix offered |
| `….CollectionInitializerElementWithASingleAddMethodIsFixed` | RH3106-002 | Verify | passes (other side) |
| `….CompoundAssignmentsAndACollectionInitializerWithCompetingCandidatesAreReportedWithoutACodeFix` (spec example, verbatim) | RH3106-002 | VerifyNoCodeFixIsOffered | fails: fix offered |
| `….AnonymousMethodReturnedByALambdaPassedToASingleNonGenericOverloadIsFixed` (`Run(Func<Action>)`) | RH3106-002 | Verify | passes |
| `….AnonymousMethodReturnedByAnAnonymousMethodOrInACollectionExpressionOfObjectElementsIsNotReported` (spec example, verbatim; finding 5 d and e) | RH3106-003 | Verify (analyzer) | fails: both reported |
| `….AnonymousMethodReturnedByOneOfSeveralReturnsOfAnAnonymousMethodUnderVarIsNotReported` (finding 5 e, second input) | RH3106-003 | Verify (analyzer) | fails: reported |
| `….AnonymousMethodReturnedByAnAnonymousMethodWithADelegateTargetIsReportedAndFixed` (`Func<Action> n = …`) | RH3106-001, RH3106-003 | Verify | passes (other side) |
| `….AnonymousMethodInACollectionExpressionOfDelegateElementsIsReportedAndFixed` (`List<Action> l = [...]`) | RH3106-001, RH3106-003 | Verify | passes (other side) |
| `….AnonymousMethodsWithADelegateTargetAlsoFromADeclaredLambdaReturnTypeAreReportedAndFixed` (spec example, verbatim, with fix) | RH3106-001, RH3106-003 | Verify | fails: 4 of 6 reported (`var g`, `var k` missing) |
| `….AnonymousMethodReturnedByTheExpressionBodyOfALambdaWithADeclaredReturnTypeIsReportedAndFixed` (finding 5 f) | RH3106-001, RH3106-003 | Verify | fails: not reported |
| `….AnonymousMethodReturnedByALambdaWithADeclaredReturnTypeUnderObjectIsReportedAndFixed` (finding 5 f, traced member) | RH3106-001, RH3106-003 | Verify | fails: not reported |
| `….AnonymousMethodInAConditionalWithADelegateTypedOtherBranchUnderObjectIsReportedAndFixed` (finding 9 a) | RH3106-001, RH3106-003 | Verify | passes |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `c += delegate () { };`, two static operators | LF | code fix | reproduced | fix offered |
| `d += delegate () { };`, two instance `operator +=` | LF | code fix | reproduced | fix offered |
| `new Coll { delegate () { } }`, two `Add` | LF | code fix | reproduced | fix offered |
| `Coll e = [delegate () { }];`, `var n = delegate () { return delegate () { }; };` | LF | analyzer | reproduced | reported; expected not reported |
| `var f = Action () => delegate () { };`, `var k = Action () => { return … };`, `object o = Action () => …` | LF | analyzer | reproduced | not reported; expected reported |

### Finding: row 32's input does not compile

`Run(() => delegate () { });` with only `static T Run<T>(Func<T> f)` is CS0411 on the input itself (the verifier reported `CS0411 The type arguments for method 'C.Run<T>(Func<T>)' cannot be inferred` on the test source). The anonymous method's natural type does not reach the lambda's inferred return type for generic inference. So it is not an input of the rule (the case cannot reach a fix in compiling code), and I wrote no test for it. 15-spec-author's internal choice ("now decided by RH3106-002's wording") and row 32 rest on an input that does not exist. For the lead / spec author: nothing in RH3106-002 depends on it, so no spec edit is required unless an example names it (none does).

## Whole run

### Decided on recommendation

- RH3004 nested Fix All base scenario: `GetFixAllScenario` now expects one Fix All iteration instead of two. SHARED-FIX-002's nested sentence demands one application; the previous `2` asserted the behavior finding 6 calls a defect. No output changes, only the iteration count.
- RH3004 column-2 `#region` test: row 31 says every moved line counts for the leftmost line, including a directive line in front of the first statement. I applied that to a `#region` in column 2 (not in column 0, so it moves). Expected `#region`/`#endregion` at 16 and `Foo();` at 34.
- RH3004 shared-brace-line with a column-0 comment: rows 29 and 33 together give `Foo(x + 1);` at the statement column (16), the comment at 0.
- No output questions raised (standing instruction).

### Not covered, and why

- `Run<T>(Func<T>)` (row 32): not an input; see the finding above.
- RH3106 Fix All across the new positions: every new case is one diagnostic per statement and they do not interact; RH3106 already has interacting Fix All tests.
- RH3004-007 CRLF: the withholding decision does not depend on the line ending (it inspects tokens, no layout).
- RH3004 CRLF for the column-0 / leftmost-line examples: one CRLF test for the shared-brace example covers the new line-moving branch under CRLF; the other shapes differ only in which lines are counted, not in how line breaks are written.
- RH3005 nested in the other order (inner comparison first in the document): only the document-order case is a nesting; the inner diagnostic is always behind the outer operator.
- Failure classes: FC-008 (Fix All, nested), FC-003 (both sides of each changed condition), FC-002 (declared vs inferred lambda return type), FC-022 (literal values: raw string), FC-016 (sibling shapes: `$`, `u8`, `return` path, collection expression vs initializer), FC-033 (CRLF) apply and are covered. No formatter phase, rename, or token gap is touched: FC-010–FC-014, FC-018/036, FC-029 cannot reach this round.

### Commands

```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH3004UseLambdaSyntaxAnalyzerTests|FullyQualifiedName~RH3005UseReadableConditionsAnalyzerTests|FullyQualifiedName~RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzerTests"
scripts/test.sh --configuration Debug --project architecture
```

On 57e119d2 with these tests: analyzer 164 total, 23 failed, 141 passed — exactly the 23 tests marked "fails" above (RH3004 13 including the base `FixAllCorrectsEveryDiagnostic`, RH3005 1, RH3106 9). Architecture: 115 passed (RH3004-007 is now covered).
