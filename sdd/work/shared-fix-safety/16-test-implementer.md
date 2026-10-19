Role: test implementer
Commit: ee02c5f94f56b0e6f0fcc495ca15f2599a6807ef
Verdict: tests written

Repair round for auditor findings 1 and 2 (`14-auditor.md`), test side only, as allowed by `squad/decisions/2026-10-19-audit-repair-doc-layout-followups.md` answer 1. My prompt carried no intended fix, suspected cause, or hoped-for verdict. It noted that the auditor's suggested change is only a suggestion; I re-derived each test from the spec. No production file changed.

## Tests

All in `src/Reihitsu.Analyzer.Test`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH7103 `CodeActionIsRegisteredWhenTheMoveCrossesOnlyAnInstanceField` (rewritten) | SHARED-FIX-003, RH7103 example; FC-017 | `GetCodeFixActionsAsync` at `_a`, `Assert.HasCount(1)`, exact text of the spec's result block, then analyzer `Verify` of that result with only `_b` marked | pass |
| RH7103 `CodeActionIsRegisteredWhenTheMoveCrossesOnlyAnInstanceFieldWithCarriageReturnLineFeed` (new) | SHARED-FIX-003, RH7103 example; FC-033 (line endings) | same helper, both texts passed through `NormalizeToCarriageReturnLineFeed` | pass |
| RH7103 `NoCodeActionIsRegisteredWhenTheMoveWouldChangeTheRunTimeValueOfAField` (input changed) | SHARED-FIX-003, RH7103 example | analyzer `Verify` with `_a` and `_b` marked, plus `GetCodeFixActionsAsync` at `_b` and `Assert.IsEmpty` | pass |
| RH4106 `NoFixIsOfferedWhenTheNewNameIsTaken` (tagged; class `D` renamed `C`) | SHARED-FIX-003 RH4106 compile-error example; SHARED-CASING-009; RH4106-006 | `Verify(testCode, testCode)` | pass |
| RH4106 `NoCodeActionIsRegisteredWhenTheRenameWouldCauseACompileError` | — | — | removed (duplicate) |

### Finding 1 (RH7103)

- The example's input and result block come verbatim from `sdd/specs/shared/code-fixes.md` (SHARED-FIX-003, "Example — another run-time change, for RH7103"), as fixed by `15-spec-author.md`. They live in two class constants under a new `Constants` region, shared by the LF and CRLF tests. The assertions live in one private method under a new `Methods` region, `VerifyTheActionForTheFieldThatCrossesOnlyAnInstanceField`.
- What the method asserts:
  - exactly one action is registered for `_a`, since the provider registers one per diagnostic;
  - the action's text equals the result block exactly, so any edit to anything else in the type fails;
  - in that result, `_b` is still reported and `_a` is not, which is the spec's second-block markup. This uses analyzer-only `Verify`, because `_b` has a fix once `_a` has moved (`15-spec-author.md`), so a `Verify(result, result)` would be wrong.
- I did not use `Verify(source, fixed)` for the action on `_a`. Over the input, it runs two iterations (`_a`, then `_b`), so it would not pin this one application.
- The `_b` test's input was the old example (`_a = Compute()` with a `Compute` method). I moved it to the new example input so that both claims of the example (both reported; no action for `_b`) are tested on the example's own text. The old input stays covered, unchanged and without markup, by `NoCodeFixWhenMoveChangesStaticInitializerExecutionOrder`. That removes the overlap the auditor noted.
- **Watched failing for the right reason.** I put the audited input back into both constants temporarily: `private static int Compute() => 1;` in the input, and the same line in the expected result. Both new tests then failed on `Assert.AreEqual`. LF and CRLF both showed the actual text rewriting `Compute` into a block body (thoenissen/Reihitsu#969), the rewrite the old substring assertions let through. I restored the file and confirmed the diff, then ran it green.

### Finding 2 (RH4106)

- I removed `NoCodeActionIsRegisteredWhenTheRenameWouldCauseACompileError`. I tagged the existing `NoFixIsOfferedWhenTheNewNameIsTaken` with `SHARED-FIX-003` and renamed its class `D` to `C`, so its input is the spec example verbatim. Its summary now names the compile error.
- The remaining assertion is `Verify(testCode, testCode)`. As the auditor quotes, with zero incremental iterations Microsoft.CodeAnalysis.Testing validates that no code fix is offered. A registered rename would change the document there, so it would fail. I did not falsify this myself (that would need a production change); the claim rests on the framework's documented behavior.
- `NoCodeActionIsRegisteredWhenTheFixWouldNotChangeTheDocument` (`_1st`, SHARED-FIX-005) stays as it is, as the auditor recommends: its `GetCodeFixActionsAsync` assertion also catches an action that changes nothing.

## Not covered, and why

- **The fix for `_b` after `_a` has moved** (the second iteration). The example states only that `_b` is still reported in the result, and the test asserts exactly that. The safety of that later move is not part of this example.
- **A CRLF variant of the `_b` no-action test.** It registers no action, so no layout is produced to compare. Line endings matter only where a rewrite happens.
- **No other failure class is touched by this repair.** No guard, rewrite, or formatter path changed, and the auditor's dismissals in `14-auditor.md` still hold.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Ordering.RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests|FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4106PrivateFieldCasingAnalyzerTests"
scripts/test.sh --project architecture
```

Results on this head:

- the focused analyzer run: 59 passed, 0 failed;
- architecture: 72 passed, 0 failed, including `RequirementCoverageTests`;
- `scripts/format.sh` on both test files: 0 of 2 files changed.
