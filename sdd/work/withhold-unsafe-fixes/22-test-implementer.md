Role: test implementer
Commit: 7a955713
Verdict: #1257: nothing to do; #1279: tests written; #1281: tests written; #1282: nothing to do; #1417: nothing to do; #1422: nothing to do

Repair round 3, for lead rows 36–39 (`00-lead.md`), auditor findings 10–13 (`20-auditor.md`), and the spec diff of 7a955713 (RH3004-006, RH3004-007) with `21-spec-author.md`. Expected values come from the spec examples and RH3106-003's first bullet, never from the code. My prompt restated the lead's decided rows and carried no intended fix, suspected cause, or hoped-for verdict, so I had nothing to set aside. No output questions raised (standing instruction).

"Fails" means the test fails on 7a955713 for the reason the requirement states; I read every failure message and diff. Class names drop the `AnalyzerTests` suffix.

## thoenissen/Reihitsu#1281 (rows 36, 37; finding 10)

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH3106UnnecessaryDelegateParenthesesShouldBeRemoved.AnonymousMethodUnderVarInAnIfBlockWithALaterCallOfTheLocalIsNotReported` | RH3106-003 | Verify (analyzer) | fails: reported at `()` |
| `….AnonymousMethodUnderVarInAUsingBlockWithALaterUseOfTheLocalIsNotReported` (`Action b = a;`) | RH3106-003 | Verify (analyzer) | fails: reported |
| `….AnonymousMethodInAnImplicitlyTypedArrayInAForeachBlockWithALaterCallOfAnElementIsNotReported` | RH3106-003 | Verify (analyzer) | fails: reported |
| `….AnonymousMethodReturnedByALambdaUnderVarInAnIfBlockWithALaterCallIsNotReported` (`g()();`) | RH3106-003 | Verify (analyzer) | fails: reported |
| `….AnonymousMethodUnderVarInALambdaBodyWithALaterCallOfTheLocalIsNotReported` | RH3106-003 | Verify (analyzer) | fails: reported |
| `….AnonymousMethodUnderVarInAnAnonymousMethodBodyWithALaterCallOfTheLocalIsNotReported` (outer `delegate { … }` without list) | RH3106-003 | Verify (analyzer) | fails: reported |
| `….AnonymousMethodUnderVarInALocalFunctionBodyWithALaterCallOfTheLocalIsNotReported` | RH3106-003 | Verify (analyzer) | fails: reported |
| `….AnonymousMethodUnderVarInAnIfBlockOfTopLevelStatementsWithALaterCallOfTheLocalIsNotReported` (`OutputKind.ConsoleApplication`) | RH3106-003 | Verify (analyzer) | fails: reported |
| `….AnonymousMethodUnderVarInAnIfBlockOfAnAsyncMethodWithALaterCallOfTheLocalIsNotReported` | RH3106-003 | Verify (analyzer) | fails: reported |
| `….AnonymousMethodUnderVarInAnIfBlockOfAnIteratorMethodWithALaterCallOfTheLocalIsNotReported` | RH3106-003 | Verify (analyzer) | fails: reported |
| `….AnonymousMethodUnderVarInALabeledBlockOfAGotoMethodWithALaterCallOfTheLocalIsNotReported` | RH3106-003 | Verify (analyzer) | fails: reported |
| `….AnonymousMethodUnderVarInAnIfBlockWithoutALaterUseIsNotReported` (holding: unused local) | RH3106-003 | Verify (analyzer) | passes |
| `….AnonymousMethodUnderObjectInAnIfBlockWithALaterCastAndCallIsNotReported` (holding: `object o …; ((Action)o)();`) | RH3106-003 | Verify (analyzer) | passes |
| `….AnonymousMethodWithADelegateTargetInAnIfBlockWithALaterCallOfTheLocalIsReportedAndFixed` (other side: `Action a = …; a();` in an `if`) | RH3106-001, RH3106-003 | Verify (fix) | passes |
| `RewriteBindingUtilitiesTests.BindsAsBeforeReturnsTrueForProbeWhoseNaturalTypeBreaksSiblingStatementInBlock` (probe, `if` block, `a();`) | FC-003, FC-027 (sibling statement off the path) | direct call | fails: `BindsAsBefore` returns false |
| `RewriteBindingUtilitiesTests.BindsAsBeforeReturnsTrueForProbeWhoseNaturalTypeBreaksSiblingStatementInLambdaBody` | FC-003, FC-027 | direct call | fails: returns false |
| `RewriteBindingUtilitiesTests.BindsAsBeforeReturnsTrueForProbeReturnedByLambdaWhoseResultBreaksSiblingStatement` (`var g = () => …; g()();`) | FC-003, FC-027 | direct call | fails: returns false |
| `RewriteBindingUtilitiesTests.BindsAsBeforeReturnsFalseForRemovedListThatLeavesLambdaReturnTypeUninferredInBlock` (holding: an error on the path, the lambda's declaration, still counts beside a sibling statement) | FC-003 | direct call | passes |

The Core tests carry no requirement tag, as in rows 13 and 35: they check the shared helper, not an RH3106 requirement. The holding member "same declaration directly in the method body" is already pinned by `AnonymousMethodInImplicitlyTypedVariableIsNotReported` (it has the later `a();`).

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `var a = delegate () { }; a();` inside `if`, `using`, `foreach`, lambda body, anonymous-method body, local function, top-level `if`, async/iterator/labeled-`goto` methods; `var g = () => delegate () { }; g()();` in an `if` | LF | analyzer | reproduced (11 of 11) | RH3106 reported at `()`; expected not reported |

### Finding: two of the audit's holding members do not compile

`var t = (delegate () { }, 1);` is CS0815 and `new { A = delegate () { } }` is CS0828 (the verifier reported both, plus CS1955 for `n.A()`). An anonymous method has no type a tuple element or an anonymous-type member under `var` can take. They are not inputs of the rule, so I wrote no test for them; `20-auditor.md` (#10 holding, and *Checked and holding*) lists them as compiling inputs. Nothing in RH3106-003 names them, so no spec edit follows.

## thoenissen/Reihitsu#1279 (rows 38, 39; findings 12, 13)

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH3004UseLambdaSyntax.FixRewritesASingleReturnOfAMultiLineRawStringLiteralAsAnExpressionBody` (renamed from `FixWithheldForASingleReturnOfAMultiLineRawStringLiteral`; spec example verbatim, content and closing quotes at column 31) | RH3004-007 | Verify | fails: no fix offered (input left unchanged) |
| `….FixWithheldForABlockHoldingAMultiLineRawStringLiteral`, `…WithAWhitespaceOnlyLine`, `…InterpolatedRawStringLiteral`, `…Utf8RawStringLiteral` (kept, block-body path) | RH3004-007 | VerifyNoCodeFixIsOffered | pass (unchanged) |
| `….FixMovesAFirstStatementLineThatStartsWithAColumnZeroCommentAndMeasuresTheShiftFromTheComment` (spec example verbatim) | RH3004-006 | Verify | fails: `/* note */ Console.WriteLine(x);` stays at column 0; expected 28 (second line at 40 already) |
| `….FixMovesAFirstStatementLineThatStartsWithAColumnZeroCommentWithCarriageReturnLineFeed` | RH3004-006 | ApplyCodeFixAsync (CRLF) | fails: same difference, CRLF kept |
| `….FixKeepsBothLinesOfABlockCommentThatStartsInColumnZeroInTheirColumns` (spec example, first comment) | RH3004-006 | Verify | fails: continuation line moved to column 19; expected 3 |
| `….FixMovesBothLinesOfABlockCommentThatStartsInAnotherColumnByTheShift` (spec example, second comment) | RH3004-006 | Verify | passes (other side: 28 and 31) |
| `….FixKeepsOrMovesEveryLineOfABlockCommentWithTheLineOnWhichTheCommentStarts` (spec example verbatim, both comments) | RH3004-006 | Verify | fails: the column-0 comment's continuation line at 19; expected 3 |

The other side of "holds code" is the existing `FixKeepsAColumnZeroCommentLineInColumnZero` (a code-free `// note` line stays), unchanged and passing.

### Existing tests the rewritten rule contradicts

Only `FixWithheldForASingleReturnOfAMultiLineRawStringLiteral`, replaced as above (row 38). For RH3004-006 I checked every RH3004 test input line in column 0 (`rg` for raw-string lines at the string's base indentation): they are directives, the `// Foo(x + 2);` and `// note` code-free comment lines (stay under both rules), and a verbatim continuation `b");` (stays). No existing test has a column-0 comment in front of code or a multi-line comment starting in column 0, so no other test changes.

## Not covered, and why

- Row 37 (finding 11, binding-root extent / cost): internal; the auditor left a pinning test to the owner. The sibling-statement Core tests fix the decision's scope; a root-extent or timing test would assert on internals or wall-clock time, so I wrote none.
- RH3106 CRLF and Fix All for the new positions: the decision is semantic (no text or layout), and every new case has at most one diagnostic, which is not reported.
- RH3004-007 CRLF: the expression-body layout is the formatter's (FMT-RAW-001/002, covered there); the withholding decision inspects tokens only.
- Disabled-text variant of the column-0 statement line (`/*` at 0 in an inactive branch): the spec's rule is textual and the same branch decides it as for active code (the auditor traced `IsDisabledStatementLine` and `StaysInColumnZero`); not added this round to keep the last round small. The implementer's code must still satisfy it; candidate for the auditor.
- Failure classes: FC-003 (both sides of each condition), FC-027 (decision scope), FC-016 (sibling shapes: `using`, `foreach`, function bodies, top level, async/iterator/goto), FC-022 (raw string value), FC-033 (CRLF for the RH3004 layout case) apply and are covered. No formatter phase, rename, token gap, or Fix All provider changes this round: FC-008, FC-010–FC-014, FC-018/036, FC-029 cannot reach it.

## Commands

```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH3004UseLambdaSyntaxAnalyzerTests|FullyQualifiedName~RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzerTests"
scripts/test.sh --configuration Debug --project core --filter "FullyQualifiedName~RewriteBindingUtilitiesTests"
scripts/test.sh --configuration Debug --project architecture
```

On 7a955713 with these tests: analyzer 141 total, 16 failed (RH3106 11, RH3004 5), 125 passed; Core `RewriteBindingUtilitiesTests` 25 total, 3 failed, 22 passed — exactly the tests marked "fails" above. Architecture 115 passed. `scripts/format.sh` changed nothing.
