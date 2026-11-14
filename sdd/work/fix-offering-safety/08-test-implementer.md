Role: test implementer
Commit: 3c1336e (tests corrected on top of d125d9a)
Verdict: #1600: tests written (setup of two tests corrected); #1150, #1274, #1143, #1673, #1678: nothing to do

My prompt named the dispute and the decision (00-lead.md decision 18). It carried no intended fix for production code. I checked the dispute myself before changing anything.

- thoenissen/Reihitsu#1150: nothing to do; no test of this issue is disputed.
- thoenissen/Reihitsu#1274: nothing to do; no test of this issue is disputed.
- thoenissen/Reihitsu#1143: nothing to do; no test of this issue is disputed.
- thoenissen/Reihitsu#1673: nothing to do; no test of this issue is disputed.
- thoenissen/Reihitsu#1678: nothing to do; no behavior change (Q5 A), and no test is disputed.

## thoenissen/Reihitsu#1600

The two SHARED-FIX-012 Fix All tests had the wrong setup. The implementer's dispute holds. On 3c1336e, both fail with `Mismatch between number of diagnostics returned, expected "0" actual "1"`. The framework's default `FixedState.MarkupHandling` drops the fixable-diagnostic markup (`{|RH4103:useTotal|}`, `{|RH6024:-|}`) in the expected carrier document. So it expects no diagnostic, while SHARED-FIX-012 requires that the diagnostic stays.

The correction adds `test.FixedState.MarkupHandling = MarkupMode.Allow;` and `test.BatchFixedState.MarkupHandling = MarkupMode.Allow;`. This is the pattern RH8204, RH8309, and RH8107 already use for a diagnostic that stays. RH6024 also gains `using Microsoft.CodeAnalysis.Testing;`. The sources, the expected sources, the iteration counts, and the requirement tags are unchanged. The tests still assert the same things: the carrier document is unchanged after Fix All, and its diagnostic is still reported.

Right-reason check: I applied the correction on 53f6660 (my tests, before the fix) in a scratch worktree. There, both tests fail with `content of '/0/Test1.cs' did not match`, because the carrier is rewritten (for example, `useTotal` becomes `UseTotal`). That is the defect SHARED-FIX-012 forbids. On 3c1336e, both tests pass.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH4103MethodNameCasingAnalyzerTests.FixAllLeavesADocumentWithAHidingCarrierUnchanged` | SHARED-FIX-012 (tags unchanged) | `Verify` with markup allowed in the fixed and batch-fixed state | passes on 3c1336e |
| `RH6024BinaryOperatorsMustBeSpacedCorrectlyAnalyzerTests.FixAllLeavesADocumentWithAHidingCarrierUnchanged` | SHARED-FIX-012 (tags unchanged) | `Verify` with markup allowed in the fixed and batch-fixed state | passes on 3c1336e |

## Not covered, and why
Nothing new. This is a setup correction only (decision 18).

## Commands
scripts/test.sh --project analyzer --filter "FullyQualifiedName~FixAllLeavesADocumentWithAHidingCarrierUnchanged"
