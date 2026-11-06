Role: spec author
Commit: a21fd95
Verdict: change doc written

Repair round 1, worklist: spec checker finding 1 (`16-spec-checker.md`), lead decision 11. The prompt carried no intended fix or hoped-for verdict beyond decision 11 itself, which I checked against the spec text and the tests.

## Changed requirements

No requirement changed. `sdd/changes/2026-10/withhold-unsafe-fixes.md` now matches the repaired SHARED-ORDER-007:

- *Affected requirements* adds SHARED-ORDER-007 and RH7103-007.
- The table under *What* gains an RH7103 row: withheld when the move crosses a static member with a runtime initializer that excluded code declares; excluded code in front of the moved member moves with it. The RH7109 row adds the same case for instance members and lists SHARED-ORDER-007.
- *Visible output changes* gains case 10, added at the end so cases 1 to 9 keep their numbers. It gives the inputs of `NoCodeFixWhenTheMoveCrossesAnInitializedFieldOfExcludedCode` (RH7109) and `NoCodeFixWhenTheMoveCrossesAnInitializedStaticFieldOfExcludedCode` (RH7103) verbatim, without the markup. Before: `main`'s fix output, measured with `scripts/apply-fix.sh RH7109`/`RH7103` on a worktree of `a59a003` (the doc's stated source), LF and CRLF alike, "fixed (1 action(s), 1 iteration(s))". In both, the moved field goes to the top of the type, in front of the `#if` pair, and `Log` is rewritten to a block body. After: no code fix is offered. It also names the other side of the condition: an excluded member without an initializer, and an `#if` pair directly in front of the moved member (`FixCrossesAFieldOfExcludedCodeWithoutInitializer`, `FixMovesExcludedCodeInFrontOfTheMovedFieldWithIt`, `FixMovesExcludedCodeInFrontOfTheMovedStaticFieldWithIt`).
- *Decisions* gains "Excluded code counts in the initializer guard". It records that case 10 was added after the maintainer's approval, and that the approved SHARED-FIX-003 *Build configurations* condition covers it, so no new question was asked (lead decision 11).

The status stays `Accepted`. I edited nothing else. `scripts/test.sh --project architecture --filter Sdd`: "Passed!  - Failed: 0, Passed: 51, Skipped: 0, Total: 51".

## Examples for the tests

None new. Case 10 mirrors the existing tests named above.

## Output questions

_None._ Decision 11 already settles it.

## Internal choices

- I appended case 10 instead of inserting it after case 5 (RH7109). Inserting it would renumber cases 6 to 9, which earlier handovers may cite.
- I left the wording of "The maintainer's answers" ("approved the visible output changes … listed above") as it is. The new decision just above it says that case 10 is not part of that approval and why it is covered. The alternative was to reword the approval sentence, which would alter a record of the maintainer's answer.

## Follow-up candidates

_None._

## Architecture findings

_None._
