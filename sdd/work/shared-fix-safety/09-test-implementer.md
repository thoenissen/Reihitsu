Role: test implementer
Commit: 0f775ef (base of this repair round)
Verdict: tests written

My prompt carried the repair task: finding 1 of `08-spec-checker.md`. It also carried constraints: no production change, and every test passes on today's code. It named no intended fix, suspected cause, or hoped-for verdict. The checker's "revert it" was given to me as a suggestion only. I re-derived what the test needs, below.

Repair round for finding 1 of `08-spec-checker.md`. No production code changed.

## What the test needs

`RH7103…AnalyzerTests.CodeFixIsOfferedWhenCrossedMemberBodyContainsActiveBalancedConditional` carries three tags:
- RH7103-006: the fix moves the member in front of the first instance member.
- RH7103-007: the fix is withheld only for the listed guards.
- SHARED-ORDER-006: a balanced directive pair keeps the move available.

On `origin/main` it asserted only `Assert.HasCount(1, actions)`. In 5ffd719 that became `Assert.IsNotEmpty(actions)`. No requirement of this run needed that change, so it was an unexplained weakening (FC-017). `HasCount(1)` does hold today:
- `TypeMemberOrderingCodeFixProviderBase.RegisterCodeFixesAsync` registers one action per diagnostic (one `context.RegisterCodeFix` call).
- `Create` is reported once.

Restoring `HasCount(1)` alone still leaves the RH7103-006 tag without a check of what the action does. The test now also applies the single action and compares the result to the full text of the move. The expected text is the move as RH7103-006 describes it: `Create` in front of `Run`, with the `#if DEBUG` pair intact inside `Run`. It is the same text as the after-block of the sibling test `CodeFixIsOfferedWhenCrossedMemberBodyContainsBalancedConditional`, where `DEBUG` is undefined. That matches the test's own summary: the guard decides on the directive trivia, not on whether the enclosed code is compiled.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests.CodeFixIsOfferedWhenCrossedMemberBodyContainsActiveBalancedConditional` (assertion restored to `HasCount(1)`; applied result asserted) | RH7103-006, RH7103-007, SHARED-ORDER-006; FC-017 | `GetCodeFixActionsAsync(..., "DEBUG")` + `ApplyChangesOperation` text | pass |

Watch-fail: I temporarily changed the expected text (`Create` to `Make`) and the test failed (1 failed). Then I restored the file byte for byte from a backup and ran it green again.

## Scenarios (bug fix)

_N/A._

## Not covered, and why

- No CRLF variant. This test is about whether the move is offered and what it moves under an active directive. Line endings are not part of it, and RH7103's existing CRLF tests stay in place.
- `sdd/specs/shared/code-fixes.md` had changes in the working tree while I worked. They were not mine and I did not stage them. The spec author committed them in 76f76e3, alongside my commit.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Ordering.RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests|TestCategory=SHARED-FIX-001|TestCategory=SHARED-FIX-003|TestCategory=SHARED-FIX-004|TestCategory=SHARED-FIX-005|TestCategory=SHARED-ORDER-006"
```

Results:
- That run: 283 passed, 0 failed.
- RH7103 class alone: 34 passed.
- `scripts/format.sh` on the changed file: formatted 0 of 1 files.
