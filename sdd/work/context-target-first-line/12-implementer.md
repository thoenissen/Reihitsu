Role: implementer
Commit: see the commit that adds this handover (subject "Measure the root offset without the target fact [skip ci]")
Verdict: tests pass

Repair round 1, auditor finding #1 (`10-auditor.md`, decision 10 of `00-lead.md`). My prompt passed on the auditor's suggested change as a suggestion only, and carried no suspected cause or hoped-for verdict. I worked out the repair from the code. I did not use either variant the auditor suggested; the variant I chose is close to the second one, "have the shift read the depth-placed column directly".

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/LayoutComputer.cs` | Split the placed-column part of `GetColumnBeforeRawStringShift` into a new private method, `GetPlacedColumnBeforeRawStringShift`. The public method still answers through the target fact first and then falls back to the new method. `ShiftToRootFirstTokenColumn` now measures the root's first token through `GetPlacedColumnBeforeRawStringShift`. This brings back `main`'s offset when the root's first token is the target's first token, which is what the fluent-chain fixes pass. The summary of `ShiftToRootFirstTokenColumn` now says the offset is measured from block indentation alone, so the fact never cancels it. That also fixes the FC-031 sentence the auditor named. | FMT-PIPE-017, FMT-PIPE-024 |

Why this repair and not moving `UseTargetPosition` after the shift:
- Block indentation (`ComputeBlockIndentation`) already stores its layouts through `LayoutModel.Set`. `Set` converts each column with `GetRawStringShift`, which is computed lazily and measures the opening quotes through `GetColumnBeforeRawStringShift`.
- If the fact were installed only after block indentation, a raw string opening on the target's first line would have its content lines stored with one shift and read back with another.
- With the split, the fact is present from the start, as before, and only the root-offset measurement ignores it. After the shift, both measurements give the same column for the target's first token whenever it is the root's first token. The fact column and the shifted placed column both equal `RootFirstTokenColumn`, so the raw-string shift agrees whichever way it is measured.

Production diff of this round: one file, +18/−2, mostly the new method's summary.

## Raw-string shift check
I ran `scripts/apply-fix.sh` on head with this change and on a temporary worktree of `origin/main` (`21c2ff4`, since removed), under LF and CRLF. All fixtures had the statement off its depth, two blank lines above it, and ` ;`:
- RH5201 r1: a raw string opening on the chain's first line.
- RH5201 r2: a raw string plus an anonymous-method argument.
- RH5201 r3: an interpolated raw string with a chain in its hole.
- RH5112 g1: `b` / `.A("""…""") ;`.

Every fixture was fixed in one iteration, and every output was byte-identical to `main`'s. The fixtures are in the scratchpad, outside the repository.

## Policy owners
- `rg -n "GetColumnBeforeRawStringShift|GetPlacedColumnBeforeRawStringShift|TryGetTargetLineColumn" src/Reihitsu.Formatter`
  - The target override still has one owner, `TryGetTargetLineColumn`.
  - The placed-column measurement now has one owner, `GetPlacedColumnBeforeRawStringShift`.
  - Every contributor still reads through `GetColumnBeforeRawStringShift`/`GetAdjustedColumn`.
  - The only caller of the new method besides that one is `ShiftToRootFirstTokenColumn`, which is the root offset's single owner.
  - No private copy of the measurement exists.

## Tests run
- `scripts/build.sh --configuration Debug`: 0 errors. One warning, S1125 in `src/Reihitsu.Cli/Diff/EditScriptBuilder.cs`, a file this branch does not touch.
- `scripts/format.sh` on the changed file: formatted 0 of 1.
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~ChainOfStatement"`: Passed 7, Failed 0. This includes the 6 analyzer tests from `11-test-implementer.md` that failed before.
- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~ReihitsuFormatterNodeColumnTests"`: Passed 47, Failed 0. This includes the 2 handed-over formatter failures.
- `scripts/test.sh --configuration Debug` (every project except self-hosting), all with 0 failed:

  | Project | Passed |
  |---|---|
  | Analyzer | 6455 |
  | Formatter | 4428 |
  | Core | 607 |
  | Cli | 251 |
  | Architecture | 92 |
  | Tooling | 125 |
  | Playground | 17 |

  These cover the earlier round's tests and the RH5111, RH3208, RH5112, and RH5201 suites.

No test, assertion, input, or expected output was changed.

## Disputed tests or scope
_None._

## Architecture findings
_None._ The defect was a guard-scope error within `LayoutComputer`: the target override reached a measurement it does not own. The repair narrows the override to the measurements that do own it.

## Follow-up candidates
_None._ The raw-string probe r2 gives an argument layout on `main` that looks odd: the `delegate` block is at the depth, under the wrapped `delegate`. Head gives the identical output, so this run does not change it. I have not shown that it is a defect.
