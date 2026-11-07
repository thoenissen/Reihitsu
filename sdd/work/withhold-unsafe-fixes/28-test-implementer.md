Role: test implementer
Commit: 9c326aa4
Verdict: #1257: nothing to do; #1279: reproduced (row 41); #1281: reproduced (row 40), summaries corrected (row 42); #1282: nothing to do; #1417: nothing to do; #1422: nothing to do

Architecture changes after audit 4, lead rows 40–42 (`00-lead.md`), auditor findings 14–16 (`26-auditor.md`). The audit's fixture files under the scratchpad's `audit4/` were gone; I used the shapes and outputs recorded in `26-auditor.md` and `audit4/rh3004.log`. Expected values come from RH3106-003 (first bullet, maintainer answer 2) and RH3004-006's stays list ("a line that starts inside a verbatim string literal begun on an earlier line … stays") with SHARED-FIX-003, never from the code. The spec author rewords RH3004-006 in parallel; I did not touch `sdd/specs`. My prompt carried no intended fix, suspected cause, or hoped-for verdict. No output questions raised (standing instruction).

"Fails" means the test fails on 9c326aa4 for the reason the requirement states; I read every failure message and diff. Class names drop the `AnalyzerTests` suffix.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH3106UnnecessaryDelegateParenthesesShouldBeRemoved.AnonymousMethodUnderVarInAForInitializerWithAUseInTheConditionAndIncrementorIsNotReported` (`for (var a = delegate () { }; a != null; a())`, method body) | RH3106-003 | Verify (analyzer) | fails: reported at `()` (5,31) |
| `….AnonymousMethodUnderVarInAForInitializerInALambdaBodyWithAUseInTheConditionAndIncrementorIsNotReported` | RH3106-003 | Verify (analyzer) | fails: reported |
| `….AnonymousMethodInAnImplicitlyTypedArrayInAForInitializerWithAUseInTheConditionAndIncrementorIsNotReported` (`new[] { delegate () { } }`, `arr[0]()`) | RH3106-003 | Verify (analyzer) | fails: reported |
| `….AnonymousMethodUnderVarInAForInitializerWithAUseOnlyInTheBodyIsNotReported` (holding: use only in the body) | RH3106-003 | Verify (analyzer) | passes |
| `RewriteBindingUtilitiesTests.BindsAsBeforeReturnsTrueForProbeWhoseNaturalTypeBreaksForIncrementor` | FC-003, FC-027 (non-path child operation) | direct call | fails: `BindsAsBefore` returns false |
| `RewriteBindingUtilitiesTests.BindsAsBeforeReturnsTrueForProbeWhoseNaturalTypeBreaksForCondition` (`for (var f = delegate () { return false; }; f();)`) | FC-003, FC-027 | direct call | fails: returns false |
| `RewriteBindingUtilitiesTests.BindsAsBeforeReturnsFalseForRemovedListThatLeavesLambdaReturnTypeUninferredInForInitializer` (holding: an error on the path still counts beside an incrementor) | FC-003 | direct call | passes |
| `RH3004UseLambdaSyntax.FixKeepsAContinuationLineOfAnInterpolatedVerbatimStringThatStartsWithAHoleInItsColumns` (`$@"Hello⏎{name}⏎done"`) | RH3004-006 | Verify | fails: `{name}` moved to column 19; expected column 0 |
| `….FixKeepsAContinuationLineOfAnInterpolatedVerbatimStringThatStartsWithAHoleInItsColumnsWithCarriageReturnLineFeed` | RH3004-006 | ApplyCodeFixAsync (CRLF) | fails: same difference, CRLF kept |
| `….FixKeepsTheClosingQuoteLineOfAnInterpolatedVerbatimStringInItsColumns` (`$@"Hello {name}⏎"`) | RH3004-006 | Verify | fails: `";` moved to column 19; expected column 0 |
| `….FixKeepsAContinuationLineOfAnInterpolatedVerbatimStringAfterASharedOpeningBraceLineInItsColumns` (`{ var s = $@"Hello⏎{name}"; Console.WriteLine(s); };`) | RH3004-006 | Verify | fails: `{name}"; Console.WriteLine(s);` moved to column 31; expected column 0 |
| `….FixKeepsTheContinuationLinesOfAnInterpolatedVerbatimStringInDisabledTextInTheirColumns` (same lines inside `#if NEVER`) | RH3004-006 | Verify | passes (see Scenarios) |
| `….FixKeepsTheClosingQuoteLineOfAVerbatimStringInItsColumns` (holding: plain `@"Hello⏎";`) | RH3004-006 | Verify | passes |
| `….FixKeepsContinuationLinesOfAnInterpolatedVerbatimStringThatStartWithTextInTheirColumns` (holding: `dear {name}`, `done";` lines) | RH3004-006 | Verify | passes |
| `….FixRewritesASingleReturnOfAnInterpolatedVerbatimStringAsAnExpressionBodyAndKeepsItsValue` (holding: expression-body path) | RH3004-003 | Verify | passes |

Row 42 / finding 16: the summaries of `BindsAsBeforeReturnsTrueForProbeWhoseNaturalTypeBreaksSiblingStatementInBlock` and `BindsAsBeforeReturnsTrueForProbeReturnedByLambdaWhoseResultBreaksSiblingStatement` now say the local's declaration statement is the binding root, so the sibling statement lies outside the bound node and is not bound at all (checked against `GetBindingRoot`: innermost non-block statement around the outermost enclosing anonymous function). Comment-only; assertions unchanged.

The Core tests carry no requirement tag, as in rows 13, 35 and 36: they check the shared helper, not an RH3106 requirement.

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `for` initializer under `var`, use in condition/incrementor: method body, lambda body, implicitly typed array | LF | analyzer | reproduced (3 of 3) | reported at `()`; expected not reported |
| Same, `BindsAsBefore` with the probe (incrementor, condition) | – | Core | reproduced (2 of 2) | false; expected true |
| Interpolated verbatim string, continuation line starting with `{` / closing `"` in column 0; shared-brace form | LF, CRLF (hole case) | code fix | reproduced (4 of 4) | line moved right (19 or 31 columns), string value changed; expected kept columns |
| Same lines (hole and closing quote in column 0) in disabled text of `#if NEVER` | LF | code fix | not reproduced | columns kept as specified. The audit traced this as failing ("`ParseTokens` splits them the same way"); observed it holds on this head. Kept as a characterization of the required outcome, so the architecture change must keep it. |

## Not covered, and why

- A line that starts inside a hole begun on an earlier line (`{name⏎}` in a verbatim interpolated string): no value change, and the spec author's rewording of RH3004-006 was not available to me; left to the spec author's wording and the next audit.
- RH3106 CRLF and Fix All: the decision is semantic (no text or layout), and each new case has one diagnostic that is not reported.
- RH3106-002 and the Fix All interacting-instance tests (finding 14's "owner checks they hold"): existing tests, unchanged; they run in the command below and pass on this head.
- RH3004 CRLF for the closing-quote and shared-brace shapes: the hole case under CRLF covers the line-ending dimension of the same decision; one CRLF variant, as the audit suggested.
- Failure classes: FC-003 (both sides: off-path child vs on-path error; string token vs string text), FC-022 (literal value), FC-027 (decision scope), FC-016 (sibling shapes: lambda body, array, shared brace line, disabled text), FC-031 (summaries, row 42), FC-033 (CRLF) apply and are covered. No formatter phase, rename, token gap, or Fix All provider changes: FC-008, FC-010–FC-014, FC-018/036, FC-029 cannot reach it.

## Commands

```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH3004UseLambdaSyntaxAnalyzerTests|FullyQualifiedName~RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzerTests"
scripts/test.sh --configuration Debug --project core --filter "FullyQualifiedName~RewriteBindingUtilitiesTests"
```

Result on 9c326aa4: analyzer 7 failed / 146 passed; core 2 failed / 26 passed — exactly the tests marked "fails" above.

## Amendment — spec aedc8b5f (`27-spec-author.md`)

The coordinator asked me to cover the spec author's row 41 and row 40 examples. Expected values are copied verbatim from the spec examples in aedc8b5f. Results below are from the working tree on e8f8475c (aedc8b5f plus my first commit). No production code was touched.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH3004UseLambdaSyntax.FixKeepsEveryLineThatStartsInsideAStringBegunOnAnEarlierLineInItsColumns` (spec example verbatim: `s` and `t`) | RH3004-006 | Verify | fails: `{name}` and `";` at 19, `name} done";` at 35; expected 0, 0, 16 |
| `….FixKeepsALineInsideAHoleThatSpansLinesInItsColumns` (the `t` string alone) | RH3004-006 | Verify | fails: `name} done";` at 35; expected 16 |
| `….FixMovesTheLineOnWhichAVerbatimStringStartsAndKeepsItsContinuationLine` (other side: `@"a` moves to 27, `b");` stays) | RH3004-006 | Verify | passes |
| `….FixKeepsALineInsideAStringAfterASharedOpeningBraceLineAndDoesNotCountItForTheLeftmostLine` (spec example verbatim) | RH3004-006 | Verify | fails: `{name}";` at 31 and `Console.WriteLine(s);` at 43; expected 0 and 31 |
| `….FixKeepsALineInsideAStringOfDisabledTextThatClosesBeforeTheEndingDirectiveInItsColumns` (spec `DEBUG` example, `DEBUG` undefined; other side of RH3004-008) | RH3004-006, RH3004-008 | Verify | passes |
| `….FixKeepsALineInsideAStringOfAnActiveBranchInItsColumnsWithDebugAndCarriageReturnLineFeed` (same example, `DEBUG` defined, CRLF) | RH3004-006 | ApplyCodeFixAsync (CRLF, `DEBUG`) | fails: `{name}");` at 19; expected 0 |
| `….FixWithheldForAStringThatDisabledTextLeavesOpen` (spec `#if A` / `#elif B` example, no symbols) | RH3004-008 | VerifyNoCodeFixIsOffered | fails: one code fix offered |
| `….FixWithheldForAStringThatDisabledTextLeavesOpenWithTheFirstBranchActive` (same, `A` defined: the string is active and spans `#elif B`) | RH3004-008 | VerifyNoCodeFixIsOffered (symbol `A`) | fails: one code fix offered |
| `RH3106UnnecessaryDelegateParenthesesShouldBeRemoved.AnonymousMethodsUnderVarInForInitializersOfTheSpecificationExampleAreNotReported` (spec example verbatim; one-line `{ }` bodies; the three shapes in one method) | RH3106-003 | Verify (analyzer) | fails: reported |

RH3106-003's new example uses the same three inputs as my first-round tests. The only differences are the one-line `{ }` bodies and having all three in one method, so I added it as one more test verbatim and kept the per-shape tests.

The spec's own-line example already contains the audit's (a) and (b) inputs; my first-round tests for them stay. My first-round shared-brace test (`{name}"; Console.WriteLine(s);` on one line) and the spec's shared-brace example (`Console.WriteLine(s);` on its own line) are different inputs, so both stay.

`EveryRequirementOfAnEnforcedSpecIsCovered` now passes: `scripts/test.sh --configuration Debug --project architecture` gives 115 passed.

Result: the analyzer filter above gives 14 failed and 148 passed. Seven of the failures are from the first round and seven are new in this amendment, all for the reasons above. The core filter is unchanged.
