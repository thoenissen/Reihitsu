Role: auditor
Commit: a0f4e7f41d6b07cbd7aff3ce3ee70c03ea74b2f2
Verdict: pass

## Scope

State:
- The checkout is the head `a0f4e7f`, and the working tree was clean before I wrote this file.
- `origin/main` and `git ls-remote origin refs/heads/main` both give `e84b26c9854b4014c96c30f8b3a549998a5b7396`, which is an ancestor of the head.
- `scripts/lane-check.sh tooling` gives "LANE CHECK: confirmed — claimed tooling, diff needs tooling".

What I audited:
- This is a repair delta against my previous audit, `27-auditor.md` at `820eba6`. Its handover commit is `40547c9`.
- The delta commits are `1ab8ba0`, `29036e5`, and `a0f4e7f`. Outside `sdd/work/`, `git diff 40547c9 HEAD` changes exactly two files:
  - the decisions entry `squad/decisions/2026-10-20-996-test-only-audit-findings.md`;
  - `src/Reihitsu.Analyzer.Test/Performance/RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzerTests.cs`. It gets one new test, plus a rename and a reworded summary on an existing test.
- Nothing changed in `src/Reihitsu.Analyzer`, `src/Reihitsu.Core`, `src/Reihitsu.Analyzer.CodeFixes`, `src/Reihitsu.Formatter`, `sdd/specs`, or `documentation` (`git diff --stat` is empty). All the spec and code evidence of `27-auditor.md` still applies.
- The decisions entry describes my two findings correctly. Its answer matches what the delta does.
- My prompt asked only for this delta and named no hoped-for verdict.

Status of the previous findings and hints:

| Item of 27 | Status | Evidence |
|---|---|---|
| Finding 1: null-conditional receivers of the first item have no tagged test | closed | New `[Requirement("SHARED-EQUAL-002")]` test: `VerifyNullConditionalAccessOnNullConstantReceiverIsNotCustomComparer`. It marks `NullHolder?.Comparer` (a `const` field), `isConst?.Comparer` (a `const` local), `default(Holder)?.Comparer`, and `default(VH?)?.Comparer`. It leaves the contrast `notConst?.Comparer` unmarked (a non-`const` local holding `null`) and asserts exactly 4 diagnostics with their message. These are the inputs I ran in the scratch build for 27, and there they behaved as the test asserts: the four constant receivers were reported and the non-`const` local was not. Together with the existing `((Holder)null)?.Comparer`, every receiver form the item names now has a tagged test on the reported side. 28 recorded two failing mutations, one per side: expected 3 / actual 4, and expected 5 / actual 4. |
| Finding 2: test name and summary phrase | closed | The method is renamed `VerifyComparerArgumentCountsAsCustomByItsForm`. The new name states the form-based rule and does not collide with `VerifyFormOfArgumentDecidesWhetherComparerIsCustom`. The phrase now reads "a variable passed as a named argument or positionally after a non-trailing named argument", which is accurate for `Enumerable.Distinct(source: items, comparer)`. The tags, input, and assertion are unchanged; the only non-comment change in that hunk is the signature line. `rg` finds the old name nowhere outside `sdd/work/`. |
| Hints 1–3 of 27 | unchanged | Not part of the decided worklist and not blocking. |

## Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|

_None._

## Guard scope

_N/A — the delta changes no predicate, guard, or spec statement; only a test and a test name or summary._

## Policy ownership

_N/A — no policy changed. The new test pins `EqualityComparerArgumentUtilities.HasExplicitEqualityComparerArgument`, which `27-auditor.md` established as the only owner._

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH1002 · `VerifyNullConditionalAccessOnNullConstantReceiverIsNotCustomComparer` (`SHARED-EQUAL-002`) | a null-conditional access whose receiver is a `const` field, a `const` local, `default` of a reference type, or `default` of a nullable value type is not custom; one on a non-`const` local holding `null` is custom | one of the four constant-receiver calls not reported, or the `notConst` call reported. Both sides failed under mutation in 28 | `Verify` + `Diagnostics(…, 4)`: an exact set, markup locations, and the message | adequate |
| RH1002 · `VerifyComparerArgumentCountsAsCustomByItsForm` (renamed) | unchanged from 27 | unchanged | exact, 10 | adequate; the name and summary now match the spec and the input |

## Failure classes dismissed

- FC-001, FC-002, FC-005, FC-015, FC-016: no guard, predicate, or policy changed.
- FC-003: the new test covers both sides of its item.
- FC-004, FC-006 to FC-014, FC-018 to FC-030, FC-036: no code fix, rewrite, or formatter change.
- FC-017: see Assertion adequacy.
- FC-031: the changed summary describes the test's input.
- FC-032, FC-035: no compiled production change. The expected values come from the unchanged analyzer.
- FC-033: the diagnostics are located by markup, and no line ending or language version is involved.
- FC-034: the test uses the exact inputs that finding 1 named.
- FC-037: no claim that a `.cs` change is text-only.

## Hints

_None new. Hints 1–3 of `27-auditor.md` stand as written there._
