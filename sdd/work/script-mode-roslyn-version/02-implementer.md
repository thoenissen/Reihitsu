Role: implementer
Commit: e6d02a3 (head the change was made on; the change and this handover are committed on top of it)
Verdict: tests pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## #898 — `scripts/trace.sh` is committed without the executable bit

### Changes
| File | What changed | Requirement |
|---|---|---|
| `scripts/trace.sh` | Mode `100644` → `100755` in the index (`git update-index --chmod=+x`) and in the working tree; content unchanged | — (tooling invariant; guard `ShellScriptExecutableModeTests.EveryTrackedShellScriptIsExecutable`) |

`scripts/trace.sh --help` now runs without a `bash` prefix and prints its usage.

### Policy owners
None touched: a file mode carries no policy.

## #1780 — one Roslyn package version for every project

### Changes
None. The issue names two options and says "either is enough"; its *Done when* is "Changing the Roslyn version needs one edit, **or** a test fails when two projects name different versions". The test implementer's `RoslynPackageVersionTests.EveryProjectReferencesOneRoslynVersion` is the second option and already passes on this head, with the fixture tests proving it fails on a mismatch. Adding a central version property as well would edit 11 project files beyond what the issue requires, so I made no production change. The reader accepts a `$(Property)` from `Directory.Build.props` and `Directory.Packages.props`, so a later move to one owner needs no test change; whether to do it in this run is the lead's call (see *Disputed tests or scope*).

### Policy owners
`rg -n 'Microsoft\.CodeAnalysis' -g '*.csproj' -g '*.props' src` and the repository root: 11 project files under `src/` name the non-Testing version (all `5.12.0`); no `Directory.Build.props` or `Directory.Packages.props` property owns it. `scripts/proof/verify-text-only.cs` pins `5.6.0` via `#:package` (already a follow-up candidate in `00-lead.md`).

## Whole run

### Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: empty (no production source under `src/` changed). The only non-test change is the mode of `scripts/trace.sh`. No new type.

### Deleted special cases
_N/A._

### Tests run
- `scripts/test.sh --configuration Debug --project architecture --filter "FullyQualifiedName~ShellScriptExecutableModeTests|FullyQualifiedName~RoslynPackageVersionTests"`: `Passed! - Failed: 0, Passed: 8, Skipped: 0, Total: 8`
- `scripts/test.sh --configuration Debug --project architecture`: `Passed! - Failed: 0, Passed: 161, Skipped: 0, Total: 161`

### Disputed tests or scope
- #1780: the issue's first option (one version property, e.g. `RoslynVersion` in `Directory.Build.props`, referenced by the 11 project files) is not implemented; the guard alone satisfies *Done when*. If the lead wants a single edit point as well, it is a mechanical 12-file change the existing guard already accepts.

### Architecture findings
_None._

### Follow-up candidates
_None_ beyond the one already in `00-lead.md` (`scripts/proof/verify-text-only.cs` Roslyn pin).
