Role: test implementer
Commit: 305ef4c
Verdict: tests written (repair round 3, audit finding #12)

Inputs: `00-lead.md` (decision 16), `19-test-implementer.md`, `24-auditor.md` (finding #12), SHARED-ORDER-007, SHARED-ORDER-002, RH7103-006/-007, RH7109-001/-005/-006. The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the worklist. I treated the audit's fixtures as suggestions. The expected outputs come from the specs. SHARED-ORDER-007 says "a move of a static member therefore reorders instance fields when excluded code in its leading trivia declares one **and the move crosses an instance field**", so without a crossed instance field the fix is offered. The output is then the plain move of RH7103-006 or RH7109-005, performed per SHARED-ORDER-002: the excluded code travels with the declaration, and the blank-line separator stays in its slot.

The implementer's concurrent edit of `src/Reihitsu.Core/OrderingDeclarationUtilities.cs` is uncommitted in the shared tree and is not part of this commit. I ran every test in a detached worktree of `fd4f4a8` with only these two test files copied in.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH7109 `FixCarriesAnInstanceFieldOfExcludedCodeInFrontOfTheMovedStaticFieldOfAStructAcrossOnlyAStaticField` (audit n1: `struct S { _x; _s; #if DEBUG _y #endif static readonly _r }` → `_r` with the `#if` block moves in front of `_s`, and `_x` stays first) | SHARED-ORDER-007, RH7109-006 | `Verify` | passes |
| RH7103 `FixCarriesAnInstanceFieldOfExcludedCodeInFrontOfTheMovedStaticMethodOfAStructAcrossOnlyAMethod` (audit m1, block-bodied: `struct S { M(); ⏎ #if DEBUG _y #endif static N() }` → `#if` block and `N` move in front of `M`, and the blank line stays between the two methods) | SHARED-ORDER-007, RH7103-007 | `Verify` | passes |

Each test flips the third condition of the layout counting (a crossed instance field) and keeps the other two (an excluded instance field in the moved trivia, and a layout type: a plain struct). The existing withheld tests and their class and no-excluded-field counterparts cover the other sides.

Checked against a mutation: I temporarily changed the worktree's layout clause to "withhold when any layout field moves" (`members.Any(member => IsLayoutField(member.Member) && getNewRank(member.Position) == 1)`), ignoring what the move crosses. Exactly these two tests failed (2 of 106 RH7103/RH7109 tests), and every earlier test passed. That confirms the gap finding #12 describes, and that these tests close it. The worktree was then removed.

## Scenarios (bug fix)
Not applicable: the tests pin behavior the code already has.

## Not covered, and why
- CRLF variants: the guard reads member kinds, initializers, and the type symbol, not line structure, and the move itself (`MoveMemberBefore`) is unchanged and already covered. This is the same reasoning as `19-test-implementer.md`.
- Audit n2 (the same shape with `_s` in front of `_x`): this flips no other condition than n1 does, so n1 covers it.
- Fix All: these fixtures have one diagnostic each. Each `Verify` runs the base's iterations and Fix All.

## Tests or specs found wrong
None.

## Output questions
_None._ Both outputs are the plain RH7103-006/RH7109-005 move, which `main` also produces (`24-auditor.md`).

## Architecture findings
_None._

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7103Static|FullyQualifiedName~RH7109"
```
Result on `fd4f4a8` plus these tests: 106 passed, 0 failed. `scripts/format.sh` over both test files: 0 of 2 changed.
