Role: implementer
Commit: ed9ad75
Verdict: #1505: tests pass (nothing to change); #1577: tests pass (nothing to change); #1576: tests pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict; I worked from `00-lead.md` and handovers 01 and 02.

## thoenissen/Reihitsu#1505

Nothing to do: the test implementer's helper change (`ObservedPipeline.Run` through `SecondPassInput.Create`) is complete and its tests are green. No production or project change is needed.

## thoenissen/Reihitsu#1577

Nothing to do: the seven call sites and `PhaseStabilityCheck.AssertNoPhaseChanges` are test code and green. No production or project change is needed.

## thoenissen/Reihitsu#1576

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.SelfHosting.Test/Reihitsu.SelfHosting.Test.csproj` | `ProjectReference` to `..\Reihitsu.Formatter.Test\Reihitsu.Formatter.Test.csproj` (decision 6), so the scan reaches `PhaseStabilityCheck` | FMT-PIPE-027 (covering test) |
| `scripts/README.md` | *The self-hosting scans* listed one row per file "for formatter idempotency and for formatter stability"; it now also names the per-phase stability check (FC-031: the description of what the scan runs went stale with the new method) | — |

### Confirmations the test implementer asked for
- `scripts/build.sh` (Release): `Build succeeded. 0 Warning(s) 0 Error(s)`. `RequirementAttribute` is not ambiguous: the build reports no CS0104.
- `scripts/test.sh --project selfhosting` discovers only `Reihitsu.SelfHosting.Test.dll`. The output has a single result line for that assembly, and the total (5783) does not include the 5250 formatter tests.
- The new method passes on every row: 1810 rows in 2 m 22 s on this sandbox, which is the added CI time of the scan. The full self-hosting run took 24 m 9 s.

### Policy owners
No policy touched (project wiring only).

### Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: empty. No new type.

### Deleted special cases
_N/A._

### Tests run
- `scripts/test.sh --project selfhosting --filter "FullyQualifiedName~NoPhaseChangesTheFormattedOutputOfSourceFile"` → `Passed! - Failed: 0, Passed: 1810, Skipped: 0, Total: 1810`
- `scripts/test.sh --project selfhosting` → `Failed! - Failed: 1, Passed: 5782, Total: 5783`. The only failure is `WorkingDocumentsTests.WorkingDocumentsDirectoryContainsNoFiles` ("sdd/work/ must be deleted before a pull request is ready"). It is expected while this run's `sdd/work/phase-check-helpers/` exists and clears once the lead deletes it.
- `scripts/test.sh --project architecture` → `Passed! - Failed: 0, Passed: 118`
- `scripts/test.sh --project formatter --filter "FullyQualifiedName~PhaseStabilityAssertionTests|FullyQualifiedName~ObservedPipelineTests|FullyQualifiedName~PhaseStabilityCheckTests"` → `Passed! - Failed: 0, Passed: 24`

### Disputed tests or scope
- `scripts/README.md` is an edit beyond the `.csproj` the handover named: one sentence, so the scan's documented contents match the new method. Tooling lane; `scripts/lane-check.sh tooling --head worktree` → `LANE CHECK: confirmed`.
- CI side effect: `Reihitsu.Formatter.Test.dll` is now copied into the self-hosting output. The SonarCloud workflow runs `dotnet test` per project, so tests are not run twice. The test assembly may appear in that project's coverage collection, as test code; I did not verify this.

### Architecture findings
_None._

### Follow-up candidates
_None_ beyond those in `00-lead.md`.

## Whole run

- Build: Release, clean (above).
- Lane check: `tooling`, confirmed.
