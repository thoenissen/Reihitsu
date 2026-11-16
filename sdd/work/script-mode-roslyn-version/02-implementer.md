Role: implementer
Commit: 65b3a94 (round 1 on e6d02a3; round 2 is committed on top of 65b3a94)
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
Round 2, on the lead's decision to take the issue's first option as well (one owner of the version) in addition to the guard:

| File | What changed | Requirement |
|---|---|---|
| `Directory.Build.props` | New property `RoslynVersion` = `5.12.0`, in its own `PropertyGroup` with a comment naming what uses it | — (tooling invariant; guard `RoslynPackageVersionTests`) |
| 11 `*.csproj` under `src/` (Core, Core.Test, Analyzer, Analyzer.CodeFixes, Analyzer.Test, Formatter, Formatter.Test, Cli.Test, SelfHosting.Test, Tooling, ArchitectureTests) | Each non-Testing `Microsoft.CodeAnalysis*` reference (12 references, including `Microsoft.CodeAnalysis.Analyzers`) uses `Version="$(RoslynVersion)"` instead of the literal `5.12.0`; the `*.Testing` references (`1.1.4`) are unchanged | same |

A Roslyn bump is now one edit in `Directory.Build.props`. No `src/` project has a nested `Directory.Build.props` (the only nested ones are under `scripts/`), so every one of them inherits the property. `dotnet list package` on `Reihitsu.Formatter.csproj` still resolves `Microsoft.CodeAnalysis.CSharp.Workspaces` `5.12.0`. `RoslynPackageVersionTests` is unchanged and resolves `$(RoslynVersion)` from `Directory.Build.props`.

### Policy owners
`rg -n 'Microsoft\.CodeAnalysis' -g '*.csproj' -g '*.props' src` and the repository root: 11 project files under `src/` name the non-Testing version (all `5.12.0`); no `Directory.Build.props` or `Directory.Packages.props` property owned it before this change; after it, `RoslynVersion` in the root `Directory.Build.props` is the single owner. `scripts/proof/verify-text-only.cs` pins `5.6.0` via `#:package` (already a follow-up candidate in `00-lead.md`).

## Whole run

### Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: `5 files changed, 6 insertions(+), 6 deletions(-)` (the project files of Core, Analyzer, Analyzer.CodeFixes, Formatter and Tooling; no C# source). Outside `src/`: `Directory.Build.props` (+5) and the mode of `scripts/trace.sh`. No new type. Round 2 delta: the property and the 12 references.

### Deleted special cases
_N/A._

### Tests run
- `scripts/test.sh --configuration Debug --project architecture --filter "FullyQualifiedName~ShellScriptExecutableModeTests|FullyQualifiedName~RoslynPackageVersionTests"` (round 1): `Passed! - Failed: 0, Passed: 8, Skipped: 0, Total: 8`
- `scripts/build.sh --configuration Debug` (round 2): `0 Warning(s)`, `0 Error(s)`
- `scripts/test.sh --configuration Debug --project architecture` (round 2): `Passed! - Failed: 0, Passed: 161, Skipped: 0, Total: 161`

### Disputed tests or scope
_None._ (The round-1 item, the issue's first option, was decided by the lead and is implemented above.)

### Architecture findings
_None._

### Follow-up candidates
_None_ beyond the one already in `00-lead.md` (`scripts/proof/verify-text-only.cs` Roslyn pin).
