Role: implementer
Commit: (this commit; see `git log`)
Verdict: tests pass

Structural round after audit 25, for lead decision 15 (auditor finding #3 and its architecture finding). Fresh agent. The prompt named the change through decision 15 and carried no suspected cause or hoped-for verdict beyond it. I worked from 00-lead.md, 20, 25, 26 and 27.

On 8b3963e, `HoleAnchorInStatementColumnAlignmentTests` and `HoleAnchorOnUnmovedLineAlignmentTests` give Failed 72, Passed 26 (Total 98), as 27-test-implementer.md says. All 98 pass now. No test was changed.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/LayoutModel.cs` | `ShiftRange` no longer returns early on a delta of 0. Every accepted line in the range is placed (`RecordPlacement`). Only a non-zero delta rewrites the layout, so a delta of 0 keeps the column and the source label (`LayoutModelTests.ShiftRangeWithZeroDeltaDoesNotModify` still holds). For the root shift (`ShiftToRootFirstTokenColumn`), `Contributor` is null, so `RecordPlacement` records nothing and the result is unchanged. | FMT-INDENT-028, FMT-INDENT-031, FMT-RAW-007 bullet 1 |
| same file | `CaptureColumns`/`MatchesColumns` (the convergence check of `LayoutComputer.RunAlignmentSweeps`) capture the column that contributors read, through the new private `GetCapturedColumn`. For a hole line that is not placed, this is its registered column; otherwise it is the layout column. A placement that leaves the layout column unchanged therefore still counts as a change when it changes what contributors read. See *Disputed tests or scope*. | FMT-INDENT-030, idempotency |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Contributors/LambdaAlignmentContributor.cs` | `Contribute` no longer returns on `delta == 0`. It always hands the block's range to `ShiftRange`, which places the lines. | FMT-INDENT-028, FMT-INDENT-031 |

**Decision 15.**
- Before: the contributor's "already under the anchor, nothing to move" decision never reached the model, so the model released the block's lines.
- Now: placement is recorded for every line the alignment covers, whether or not it writes a different column.
- In finding #3's counterexample, the block-indentation layouts that the shift confirms are kept: `{`/`}` in column 8 and the body in 12. They are no longer released to input whitespace.

## Policy owners

- Writes that place a line: `rg -n "RecordPlacement\(|\.Set\(|SetUnlessPlaced\(|ShiftRange\(" src/Reihitsu.Formatter --glob '!**/obj/**'`.
  - Placement is recorded only by `LayoutModel.RecordPlacement`, called from `Set`, `SetUnlessPlaced` and `ShiftRange`.
  - The writers are `LayoutComputer.SetIfFirstOnLine` (used by every Set-based contributor), `LayoutComputer.SetTokenIndentation`/directives (block pass, `Contributor` null), `CommentIndentationContributor` (after the sweeps, `Contributor` null), `ListElementAligner.AlignContinuations` (`SetUnlessPlaced`), `LambdaAlignmentContributor` (`ShiftRange`), and `LayoutComputer.ShiftToRootFirstTokenColumn` (`ShiftRange`, `Contributor` null).
- Zero-delta skips, as the prompt asked me to check: `rg -n "delta|== current|!= current|Column ==|Column !=" src/Reihitsu.Formatter/Pipeline/Indentation/Contributors src/Reihitsu.Formatter/Pipeline/Indentation/Utilities`.
  - The only skips of a write on an unchanged column were the two this change removes: `LambdaAlignmentContributor.Contribute` and `LayoutModel.ShiftRange`.
  - Every other alignment contributor writes through `SetIfFirstOnLine` → `Set`, or through `SetUnlessPlaced`, without comparing against the current column. So they already place a line whose column does not change.
  - Their early returns (`ConstructorInitializerContributor`, `GenericConstraintContributor`, `CommentIndentationContributor` on `TryGetPlacedColumn == false`; `CommentIndentationContributor` on `commentLine != tokenLine`) depend on whether an anchor line exists or on where a comment stands, not on a delta.
  - `ShiftToRootFirstTokenColumn` also reaches `ShiftRange` with a delta of 0, but it places nothing, because `Contributor` is null.
  - Result: no other contributor has this defect.
- Convergence check: `rg -n "CaptureColumns|MatchesColumns" src --glob '!**/obj/**'` finds one production caller, `LayoutComputer.RunAlignmentSweeps`, plus `LayoutModelTests`. The snapshot keeps its type, `Dictionary<int, int>`.

## Tests run

```text
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~HoleAnchorInStatementColumnAlignmentTests|FullyQualifiedName~HoleAnchorOnUnmovedLineAlignmentTests"
  before (8b3963e): Failed: 72, Passed: 26, Total: 98
  after:            Failed: 0, Passed: 98, Total: 98
scripts/test.sh --configuration Debug --project formatter   Failed: 0, Passed: 4261, Total: 4261
scripts/test.sh --configuration Debug --project analyzer    Failed: 0, Passed: 6397, Total: 6397
scripts/test.sh --configuration Debug --project cli         Failed: 0, Passed: 251, Total: 251
scripts/build.sh --configuration Debug                      0 Warning(s), 0 Error(s)
```

`scripts/format.sh` over both changed files: `Formatted 0 of 2 file(s)`.

Idempotency probe, run with the Debug CLI twice per fixture:
- **Corpus.** 408 fixtures: anonymous method and `() =>` × literal moved right, aligned, or moved left × input column 0–16 of the `})` line × input column 0/8/12/16 of the next line. The anchor is in the statement's column behind a `/* */` end line. An outer chain `M(…).A()⏎.B()` is anchored on the block's closing line.
- **Result.** All 408 are stable, and they all give the same block and chain lines.

## Disputed tests or scope

**The convergence-check change has no covering test.**
- The 98 handed-over tests already passed with the two `delta == 0` removals alone.
- With only those removals, the probe above gave 16 fixtures that were not idempotent. They are the anonymous method and the lambda, each with an aligned literal and the `})` line at input column 6, or a left-moving literal and that line at 14. The column of the next line made no difference.
- Example (aligned literal, input `      }).A()` ⏎ `.B()}`): pass 1 gives `.B()` in column 8, pass 2 in column 10.
- Mechanism:
  1. The chain is an ancestor of the lambda, so it runs earlier in the pre-order sweep. It reads `.A` on the still-unplaced `}` line at its registered column 6.
  2. The lambda then places the block with a delta of 0.
  3. The chain's column for `.B()` happens to equal its block-indentation stand-in, so no layout column changed. `MatchesColumns` ended the sweeps before the chain could read the placed `}` line.
- The snapshot change closes this. It is in `LayoutModel`, a component the decision names, and it adds no public member.
- The same gap existed before for a `Set` that places a hole line at the column that block indentation had written. That is the same mechanism, with placement inferred from a column change.
- The test implementer should add a test. Input: an anonymous method in the statement's column behind a `/* */` end line, aligned literal, `})` line at input column 6, followed by `.A()` ⏎ `.B()`. Expected: `}).A()` in column 8 with `.B()` under `.A` (column 10). Add the block-lambda counterpart and the left-moving literal (`})` at input column 14).

Scope: two production files, both named by decision 15. No new public member and no new dependency.

## Architecture findings

_None_ beyond decision 15. Two facts are relevant to that decision:
- Placement is now recorded independently of a column change in every writer.
- The sweep's convergence check now sees placement through the column contributors read, so it no longer infers placement from a write.

## Follow-up candidates

_None._
