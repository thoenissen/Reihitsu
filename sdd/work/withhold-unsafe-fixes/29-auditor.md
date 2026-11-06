Role: auditor
Commit: 169692d
Verdict: pass (#1136: pass; #1134: pass; #1135: pass; #1262: pass; #1332: pass; #1336: pass; #1340: pass)

Inputs: the seven issues as named in `00-lead.md`, `00-lead.md` (decision 16), the earlier audits `10-auditor.md`, `18-auditor.md`, `24-auditor.md`, handovers `25`–`28`, SHARED-ORDER-007 (`sdd/specs/shared/ordering-move-safety.md`), RH7103-006/-007, RH7109-005/-006, RH7309-006, and the diff `69687d9..169692d`. The prompt carried no intended fix, suspected cause, or hoped-for verdict. No test was re-run and no fixture was executed: no finding turns on a result, and the cleaner's run on `c55f130` (RH7103/RH7109 106 passed, Core 607 passed; `169692d` adds only a handover) and the test implementer's mutation run are the evidence for the tests.

## Scope

Repair delta of round 3 against `69687d9`. Head `169692d` (checked out, clean). `origin/main` = `a59a003f6a9f85126892eac7646fc8945a4b9073`, an ancestor of the head. The diff matches handovers `25`–`28`: two tests (one in each of the RH7103 and RH7109 test classes), `OrderingDeclarationUtilities` (`IsLayoutField` and `HasInitializer` made `private` and moved behind `FieldOrderSetsLayout`), `00-lead.md` decision 16 and its follow-up list, and the handovers. No spec, change doc, rule page, or other production file changed; scope did not grow. `scripts/lane-check.sh behavior-change`: confirmed. Evidence of `24-auditor.md` that the round did not touch is reused.

### Status of the earlier findings

| # | Status | Evidence |
|---|---|---|
| 12 | closed | Both suggested members are pinned on the offered side with full output: RH7109 n1 (`FixCarriesAnInstanceFieldOfExcludedCodeInFrontOfTheMovedStaticFieldOfAStructAcrossOnlyAStaticField`) and RH7103 m1 (`FixCarriesAnInstanceFieldOfExcludedCodeInFrontOfTheMovedStaticMethodOfAStructAcrossOnlyAMethod`). The RH7109 test flips only the crossed member (static field instead of instance field) against the withheld `NoCodeFixWhenExcludedCode…InstanceFieldsOfAStruct`; the test implementer's mutation (withhold whenever a layout field has rank 1, ignoring what is crossed) failed exactly these two of 106 tests. Class walked again: rule axis RH7103, RH7109 (pinned now), RH7309 (no moved-trivia rank; excluded members are ranked by segment through the same rank function as initializers, pinned by `NoCodeFixWhenAnInitializerInDisabledTextWouldChangeOrder`, and the layout counting of excluded members is the owner's, pinned by the RH7103/RH7109 tests; no RH7309-only code decides it). Condition axis: excluded field in moved trivia, layout type, crossed instance field — all three flipped by a test on both rules. n2 is the same flip as n1 |
| 13 | closed | `rg -n "HasInitializer\b\|IsLayoutField" src`: both are `private`, called only by the private `ChangesMemberOrder(…, bool fieldOrderSetsLayout, Func<int, int>)` (lines 400–402); the remaining hit is an RH7109 test method name. Moved bodies and summaries are textually identical to the removed ones (line-multiset comparison of the hunk: no line differs except the two modifiers). The public surface of the member-order policy is now the two symbol overloads of `ChangesMemberOrder` and the move helpers |

No new finding repeats an earlier class; the round adds no finding.

## thoenissen/Reihitsu#1136

Pass. No change in the round.

## thoenissen/Reihitsu#1134

Pass. #12 is closed on the RH7109 side (n1 tested; expected output is the RH7109-005 move per SHARED-ORDER-002: the `#if` block travels with `_r` in front of `_s`, `_x` stays first). #13 closed.

## thoenissen/Reihitsu#1135

Pass. #12 is closed on the RH7103 side (m1 tested; expected output is the RH7103-006 move: the `#if` block and `N` move in front of `M`, the blank line stays between the two methods). #13 closed.

## thoenissen/Reihitsu#1262

Pass. No change in the round; `RegionBlockLayout` still reaches only the public symbol overload, which the visibility change does not touch.

## thoenissen/Reihitsu#1332

Pass. No change in the round.

## thoenissen/Reihitsu#1336

Pass. No change in the round.

## thoenissen/Reihitsu#1340

Pass. No change in the round.

## Whole run

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Layout counting of excluded code in the moved trivia (unchanged code) | `ChangesOrder(IsLayoutField)` over ranks: moved full span → 1, crossed span → 2 | SHARED-ORDER-007 "… declares one and the move crosses an instance field" | yes; now pinned on both sides of all three conditions | ok (#12 closed) |

The round adds or changes no predicate: `IsLayoutField` and `HasInitializer` changed visibility only.

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Member order (initializers, layout fields), counting excluded members | `OrderingDeclarationUtilities.ChangesMemberOrder` (public symbol overloads → private flag overloads); building blocks `IsLayoutField`, `HasInitializer`, `FieldOrderSetsLayout` all private; `ChangesInitializerExecutionOrder` internal, Core tests only; consumers RH7103, RH7109, `RegionBlockLayout` → RH7309 | one owner, no building block reachable from another assembly (#13 closed) |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH7109 `FixCarriesAnInstanceFieldOfExcludedCodeInFrontOfTheMovedStaticFieldOfAStructAcrossOnlyAStaticField` | excluded instance fields that move with the declaration withhold only when an instance field is crossed | no action, or another output | `Verify` (iterations and Fix All of the batch base) | ok; differs from the withheld counterpart only in the crossed member; mutation-checked (25-test-implementer) |
| RH7103 `FixCarriesAnInstanceFieldOfExcludedCodeInFrontOfTheMovedStaticMethodOfAStructAcrossOnlyAMethod` | as above, for RH7103 | no action, or another output | `Verify` | ok; mutation-checked. It also changes the moved kind (method instead of field) against the withheld RH7103 counterpart, so in isolation it pins "no instance field crossed" together with "moved member is a method"; the RH7109 test isolates the crossing condition on the shared owner, which is enough |

### Visible output is decided

Both new expected values are the plain moves RH7103-006 and RH7109-005 demand, performed per SHARED-ORDER-002, and are `main`'s output: on `a59a003` both providers asked `ChangesInitializerExecutionOrder` (no initializer in either fixture → offered), the base `IsMoveSafe` directive check (`OrderingMoveSafety`, unchanged in the run; the `#if` pair lies wholly in the moved trivia) passes, and `MoveMemberBefore` is unchanged by the run. Traced, not executed. No rule page changed. No undecided output.

### Failure classes dismissed

- FC-010, FC-011, FC-012, FC-013, FC-014, FC-028, FC-032: no formatter file changed.
- FC-001, FC-002, FC-006, FC-016: no predicate or guard added or changed; the two members changed visibility only.
- FC-003, FC-017: walked; the three conditions of the layout counting are now flipped by tests on both rules (#12 closed).
- FC-015, FC-018, FC-036: one owner; no other spelling or copy of `IsLayoutField`/`HasInitializer` (`rg`).
- FC-004, FC-005, FC-019–FC-023, FC-025, FC-026, FC-029: no trivia, literal, identifier, expression-body, or repeated-token handling changed.
- FC-007, FC-008, FC-009: both new tests use `Verify` of `BatchCodeFixTestsBase`, which runs iterations and Fix All; no action that changes nothing.
- FC-024: no partial-type logic changed.
- FC-030: no whole-type formatting changed; the RH7103 fixture uses block-bodied methods.
- FC-031: the moved summaries describe the unchanged bodies; the two new test summaries describe their fixtures (crosses only a method / only a static field).
- FC-033, FC-034: the guard reads member kinds and the symbol, not line structure; CRLF variants add nothing (25-test-implementer, same reasoning as `19`).
- FC-035: the mutation run shows each new test can fail.
- FC-037: no text-only claim.

### Architecture findings

_None._ The member-order decision has one owner, and with #13 none of its building blocks is reachable outside `OrderingDeclarationUtilities`. #12 was decided under decision 16 as a test gap without structural cause; the round closes it by tests, and no member of its class fails.

### Hints

_None new._ The hints of `24-auditor.md` stand (RH7004 CRLF drift on `main`, now in the follow-up list; `ChangesInitializerExecutionOrder` used only by Core tests; RH7102 without the owner call; `InternalsVisibleTo` style).
