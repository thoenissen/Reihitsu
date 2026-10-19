Role: auditor
Commit: af6b731c31db33568ce3e46d9ce2d864d4cb7940
Verdict: pass

My prompt carried the issue, the slug, the SHAs, and the handovers and decision to read. It asked me to confirm that findings 1 and 2 of `14-auditor.md` are closed. I treated that as a question to check, not as a verdict to reach. It carried no intended fix or suspected cause.

## Scope

Repair delta against `894256a`, the commit of `14-auditor.md`. The repair round audits only what the repair moved, and the scope did not grow.

State checks:

- The checkout is `af6b731`, and the working tree is clean.
- `git ls-remote origin refs/heads/main` returns `d65c7827442dedd681a33286da71e5db7257b74b`. That commit is an ancestor of HEAD through merge `1b21a0d` (parents `206af00` and `d65c782`).
- `git show --cc 1b21a0d` shows one resolved hunk, in `sdd/specs/requirement-ids.txt`:
  - the result passes `LC_ALL=C sort -c` and has no duplicate IDs;
  - it has 941 lines, which is main's 938 plus `SHARED-FIX-003` to `-005`;
  - it does not differ from main in any other line.
- I compared `git diff d65c782 HEAD` with the branch diff that the first audit covered (`git diff 3b113db 894256a`). Only five paths differ: the ledger, plus the four repair files that `15` to `18` describe.
  - `sdd/specs/shared/code-fixes.md`: the RH7103 example of SHARED-FIX-003 only.
  - `squad/decisions/2026-10-19-audit-repair-doc-layout-followups.md`.
  - `RH4106PrivateFieldCasingAnalyzerTests.cs`.
  - `RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests.cs`.

  Main changed none of these four paths, and every other file of the run is byte-identical to `894256a`.
- `scripts/lane-check.sh tooling` prints "confirmed — claimed tooling, diff needs tooling".
- FC-032:
  - Main brought no production change (`git diff 3b113db d65c782 -- src` touches test projects only).
  - It changed the test bases additively: `VerifyNoCodeFixIsOffered` and `ExpandWhitespaceMarkers`, with no change to `Verify` or `GetCodeFixActionsAsync`.
  - `19-spec-author.md` re-ran the SHARED-FIX-001/-003/-004/-005 categories after the merge: 253 passed.

  I trusted those results and did not re-run the tests.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | **Spec.** The RH7103 example now has an input with no member that its diagnostics are not about (`_a = 1`, no `Compute`). A result block follows it, and the prose names it as the result of the fix for `_a`, with `_b` still reported (`sdd/README.md`, Spec format). The reasoning holds: `_b` moved in front of `_a` would read `0`.<br><br>**Test.** `VerifyTheActionForTheFieldThatCrossesOnlyAnInstanceField` asserts:<br>- `HasCount(1)` at `_a`;<br>- `AreEqual` with the result block, so any other edit in the type fails, including the #969 reformat that `16` watched fail under LF and CRLF;<br>- an analyzer `Verify` of the result with only `_b` marked.<br><br>It runs under LF and CRLF. `GetCodeFixActionsAsync` builds the document with `SourceText.From(source)` and does not normalize line endings. `NoCodeActionIsRegisteredWhenTheMoveWouldChangeTheRunTimeValueOfAField` now runs on the example input, with both fields marked and `IsEmpty` at `_b`. The old `Compute` input stays covered by `NoCodeFixWhenMoveChangesStaticInitializerExecutionOrder`, unchanged. |
| 2 | closed | The duplicate was removed. `NoFixIsOfferedWhenTheNewNameIsTaken` now carries SHARED-CASING-009, RH4106-006 and SHARED-FIX-003, its summary names the compile error, and its input is the SHARED-FIX-003 example verbatim. `Verify(testCode, testCode)` still proves the absence of a fix. I re-read the Microsoft.CodeAnalysis.Testing 1.1.4 XML documentation of `NumberOfIncrementalIterations`: when the expected output equals the input, the default is `0`, and "If the number of iterations is zero, the code fix test will validate that no code fixes are offered". The usings of the RH4106 file are still all used. |

## Findings

_N/A — no confirmed finding in the repair delta._

## Guard scope

The repair adds or changes no predicate. The one bound that the changed example exercises:

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Run time: initializer order (SHARED-FIX-003, the RH7103 example) | the moved static field and the members it crosses up to its target | `OrderingDeclarationUtilities.ChangesInitializerExecutionOrder` via `RH7103…CodeFixProvider.IsMoveSafe`, evaluated per diagnostic in `TypeMemberOrderingCodeFixProviderBase.RegisterCodeFixesAsync` on the current document | They agree on the example: `_a` has one action and `_b` has none. I confirmed this in a scratch harness outside the repository, which calls `RegisterCodeFixesAsync` per analyzer diagnostic (LF and CRLF). After `_a` has moved, `_b` has an action that crosses only `_instance`, which is correct under SHARED-FIX-003. | ok. Fix All on this input: see Hints. |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Asserting that no code fix is offered | `Verify(source, source)` (zero iterations); `GetCodeFixActionsAsync` plus `Assert.IsEmpty`; `VerifyNoCodeFixIsOffered`, new from main (`AnalyzerTestsBase{TAnalyzer,TCodeFix}.cs`) | These are test helpers, not a shipped policy. `VerifyNoCodeFixIsOffered` requires that no reported diagnostic has an action. It cannot replace the `_b` assertion, because `_a` has one there. For RH4106 it is equivalent to `Verify(t, t)`. No finding. |
| Initializer order on a move | unchanged since `14-auditor.md`: SHARED-ORDER-007 and `ChangesInitializerExecutionOrder`; RH7309 is now thoenissen/Reihitsu#1135 | consistent |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH7103 `CodeActionIsRegisteredWhenTheMoveCrossesOnlyAnInstanceField` | One action for `_a`, whose result is exactly the spec's result block. In that result `_b` is reported and `_a` is not. | A second action, any other edit in the type, or `_a` reported (or `_b` not) | `GetCodeFixActionsAsync`, `HasCount(1)`, `AreEqual`, analyzer `Verify` | adequate |
| RH7103 `…WithCarriageReturnLineFeed` (new) | The same, under CRLF, with every line ending kept | An LF in the result, or any of the above | the same helper; inputs via `NormalizeToCarriageReturnLineFeed`; the document is not normalized | adequate |
| RH7103 `NoCodeActionIsRegisteredWhenTheMoveWouldChangeTheRunTimeValueOfAField` (input changed) | Both fields are reported on the example input; `_b` has no action | Either field unreported, or an action for `_b` | analyzer `Verify` plus `GetCodeFixActionsAsync` and `IsEmpty` | adequate |
| RH4106 `NoFixIsOfferedWhenTheNewNameIsTaken` (tagged, `D` renamed to `C`) | The collision is reported and no fix is offered | Any registered action that changes the document | `Verify(t, t)`: zero iterations, framework-validated | adequate |

## Failure classes dismissed

- **Guards: FC-001, FC-002, FC-015, FC-016.** The repair changes no code guard.
- **FC-003.** Both sides of the initializer-order bound are tested on the same input: `_a` gets an action and `_b` gets none.
- **Trivia: FC-004, FC-005, FC-019, FC-020, FC-021.** No rewrite was added. The changed example is about run time and holds no comment or directive. The trivia cases are unchanged since `14-auditor.md`.
- **Rewrites: FC-006, FC-022 to FC-027, FC-029, FC-036.** No production change.
- **FC-007, FC-008.** The single fixes converge (`15`: 2 iterations, one per diagnostic, as SHARED-FIX-001 allows). Fix All on the example input needs two applications. That is pre-existing behavior that the repair does not depend on, and it is filed as a hint.
- **FC-009.** Covered by `IsEmpty` at `_b` and by the zero-iteration RH4106 test.
- **FC-018.** N/A.
- **FC-030.** The exact-text assertion now catches the type-wide reformat (thoenissen/Reihitsu#969) on this example. `16` falsified it with the old input.
- **Formatter: FC-010 to FC-014, FC-028.** No formatter or code-fix change.
- **FC-031.** Every changed summary describes its test or helper. The `_b` test's summary still fits its new input. The added source lines contain no `TODO` and no tracker number.
- **FC-032.** See Scope.
- **FC-033.** The CRLF variant goes through an unnormalized `SourceText`.
- **FC-034.** No reproduction in this run.
- **FC-035.** The expected text is the spec's result block. The spec author derived it from runs of the shipped code and checked it against the prose: separators stay in place (SHARED-ORDER-002) and no blank line is inserted. This is a characterization of a requirement that holds today. No code was patched.
- **FC-037.** No text-only claim.

## Hints

Follow-up candidates. They do not block.

- **Fix All on the new RH7103 example needs two applications (SHARED-FIX-002).** I ran a scratch harness outside the repository. It applies the provider's `BatchFixer` with `FixAllScope.Document` to the example input, under LF and CRLF.
  - Pass 1 moves `_a` only. Its result is the spec's result block, and in it `_b` is reported and now has an action.
  - Pass 2 moves `_b` in front of `_instance`.

  The single fixes applied one after another reach pass 2's text, so one Fix All application does not equal them.

  The cause differs from the overlapping-edit case of thoenissen/Reihitsu#1110 Q1, which already names RH7103 (`NumberOfFixAllIterations = 2`). `BatchFixer` computes every action on the original document, so a fix that SHARED-FIX-003 withholds until another fix of the same rule has run is never part of the first pass. The same can reach every guard-gated batch Fix All, such as RH7109's `IsMoveSafe`.

  The repair does not cause this, and no statement of the example depends on it. The spec says only that `_b` is still reported in the result. Suggested routing: add this input and mechanism to thoenissen/Reihitsu#1110 Q1, and name it in the PR.
- **Two examples of one scenario now differ only in the type name.** The SHARED-CASING-009 example in `sdd/specs/shared/casing.md` spells `public class D`. The SHARED-FIX-003 example and the one remaining test spell `public class C`. The behavior is identical. No rule requires a verbatim copy.
- **`VerifyNoCodeFixIsOffered` (from main)** would fit future SHARED-FIX-003 or -005 tests whose every diagnostic is withheld. The run's tests do not need it.
- **PR description.** In addition to the items listed in `14-auditor.md`, link thoenissen/Reihitsu#1134 to #1137.
