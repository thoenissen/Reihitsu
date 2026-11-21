Role: test implementer
Commit: a7d7a9e (base; this handover is committed with the repair)
Verdict: tests written

Repair round 1 (CI), finding `10-ci.md`: RH2111 reported `RH5201MethodChainsShouldBeAlignedAnalyzerTests` at 3015 lines of code, over the test limit of 3000. The prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing ignored.

## Repair

The three tests this run added to `RH5201MethodChainsShouldBeAlignedAnalyzerTests` (`04-test-implementer.md`) move unchanged — names, summaries, inputs, and assertions verbatim — into a new class `RH5201MemberAccessChainWordingTests` in the same folder. `RH5201MethodChainsShouldBeAlignedAnalyzerTests.cs` is back to its content before this run (`git diff 67c71cf^ -- <file>` is empty), the usings the run had added included, so its count is the one `main` already passed with.

The new class tests the code fix and the provider supports Fix All, so it derives from `BatchCodeFixTestsBase` (`AGENTS.md`), which requires a `GetFixAllScenario`: two misaligned links of a chain without any call (`.City`, `.Name` behind `order.Customer.Address`), both aligned under `.Address`'s dot in one Fix All iteration, asserted with `MessageFormat`. The scenario is new, not moved; it uses the same shape as the moved tests. This follows the precedent of `RH5111AssignmentsMustHaveProperLineBreaksConditionalAccessTests`, a second batch class of one rule with its own scenario. RH2111-004 judges partial parts separately, but the repository has no partial test class, so a separate class was chosen over a partial one.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5201MemberAccessChainWordingTests.VerifyTitleNamesMemberAccessChain` (moved) | — (wording) | descriptor `Title` | passes |
| `RH5201MemberAccessChainWordingTests.VerifyMisalignedLinkOfCallLessChainIsReportedWithMemberAccessChainMessage` (moved) | — (wording) | `Verify(source, fixedSource, Diagnostics(id, literal message))` | passes |
| `RH5201MemberAccessChainWordingTests.VerifyCodeFixTitleNamesMemberAccessChain` (moved) | — (wording) | `GetCodeFixActionsAsync` | passes |
| `RH5201MemberAccessChainWordingTests` Fix All test of the base (new scenario) | — | `BatchCodeFixTestsBase.GetFixAllScenario` | passes |

No assertion changed. No test carries a requirement tag, as before.

## Scenarios (bug fix)

_N/A — behavior change._

## Not covered, and why

Unchanged from `04-test-implementer.md`.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5201"
scripts/test.sh --project selfhosting --filter "FullyQualifiedName~AnalyzerReportsNoViolationsOnOwnSourceCode"
```

The first: 147 passed. The second: 352 passed, 0 failed (RH2111 row included).

## Follow-up candidates

_None._
