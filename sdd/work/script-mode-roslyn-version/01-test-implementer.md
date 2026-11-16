Role: test implementer
Commit: 5ebd5f9 (head the tests were written and run on; the tests and this handover are committed on top of it)
Verdict: #898 reproduced; #1780 tests written

No spec author or skeptic ran (tooling lane), so the tests come from the issues' acceptance criteria and the repository state; no requirement exists for either invariant, so no test carries a `[Requirement]` tag. My prompt carried no intended fix or hoped-for verdict.

## #898 — `scripts/trace.sh` is committed without the executable bit

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Reihitsu.ArchitectureTests.General.ShellScriptExecutableModeTests.EveryTrackedShellScriptIsExecutable` | — (tooling invariant; FC-017: also asserts `scripts/trace.sh` is among the entries, so the guard cannot pass vacuously) | `git ls-files --stage -- '*.sh'` from the repository root | **Fails**: `scripts/trace.sh (100644)` is the only entry without `100755` |

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `scripts/trace.sh --help` without a `bash` prefix, Linux sandbox | n/a | shell | reproduced | Observed `/bin/bash: line 1: scripts/trace.sh: Permission denied`; expected the script to run |
| `git ls-files -s` over every tracked `*.sh` | n/a | git index | reproduced | `scripts/trace.sh` is `100644`; the other nine (`apply-fix.sh`, `build.sh`, `format.sh`, `lane-check.sh`, `lib/dotnet-env.sh`, `prepare.sh`, `test.sh`, `usage.sh`, `verify-text-only.sh`) are `100755` |

Answer to the issue's first question: no other tracked `*.sh` lacks the bit. The other files under `scripts/` that are `100644` are `.cs` file-based programs, their `Directory.Build.props`, and `README.md`, which are not run directly.

The guard covers every tracked `*.sh` in the repository (git pathspec `*.sh` matches across directories), not only `scripts/*.sh`; today both sets are the same. It reads the git index rather than the working-tree mode, because the index is what a checkout reproduces on every platform. It reports `Inconclusive` when `git` is not on the `PATH` or the command fails (for example a non-work-tree copy or git's "dubious ownership" refusal), matching `ApplyFixWrapperPositionalArgumentTests`' handling of a missing `bash`.

## #1780 — one Roslyn package version for every project

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Reihitsu.ArchitectureTests.General.RoslynPackageVersionTests.EveryProjectReferencesOneRoslynVersion` | — (tooling invariant; FC-017: asserts that Core, Analyzer, CodeFixes and Formatter were found, so it cannot pass vacuously) | `RoslynPackageReferences.Collect` over `src/` + `DescribeMismatch` | Passes (every reference resolves to `5.12.0`) |
| `…RoslynPackageVersionTests.DifferentLiteralVersionsAreReportedAsMismatch` | FC-003 (mismatch side) | fixture directory | Passes |
| `…RoslynPackageVersionTests.SameLiteralVersionsAreNotReported` | FC-003 (agreement side) | fixture directory | Passes |
| `…RoslynPackageVersionTests.VersionPropertyFromDirectoryBuildPropsIsResolved` | — (the "one version property" option of the issue) | fixture directory | Passes |
| `…RoslynPackageVersionTests.CentralPackageVersionIsResolved` | — (the `Directory.Packages.props` option, `VersionOverride` included) | fixture directory | Passes |
| `…RoslynPackageVersionTests.TestingPackagesAreNotCollected` | — (the issue's exclusion of `*.Testing`) | fixture directory | Passes |
| `…RoslynPackageVersionTests.UnresolvableVersionIsReported` | FC-017 (a version the reader cannot resolve fails the guard instead of passing unchecked) | fixture directory | Passes |

Supporting files: `RoslynPackageReferences.cs` (the reader and the mismatch predicate the repository test and the fixture tests share) and `RoslynVersionFixtureDirectory.cs` (temporary fixture tree).

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `rg 'Microsoft\.CodeAnalysis' -g '*.csproj' src`, non-`.Testing` references | n/a | project files | premise confirmed | 11 project files name the version separately, all `5.12.0`; no central property, no `Directory.Packages.props`, no test |
| Temporarily set `Reihitsu.SelfHosting.Test.csproj` to `Microsoft.CodeAnalysis.CSharp` `5.6.0` and run the repository test (reverted afterwards) | n/a | guard | guard fails | `EveryProjectReferencesOneRoslynVersion` fails on the mismatch, as intended |

The issue reports a missing guard, not a defect, so there is nothing to reproduce as a failure: the repository test passes on this head and the fixture tests prove it fails on a mismatch. The reader accepts every form the issue's two options can take — literal `Version`, `$(Property)` from the project or any `Directory.Build.props`/`Directory.Packages.props` between the project and the repository root, a central `PackageVersion`, and `VersionOverride` — so the implementer may choose either option without changing the test. The reader is a deliberate approximation of MSBuild evaluation (it ignores `Condition` attributes and imports every `Directory.*.props` up to the root, not only the nearest); anything it cannot resolve shows up as `<unresolved>` and fails the guard, so an approximation error cannot pass silently.

Scope follows the issue's wording: every `Microsoft.CodeAnalysis` and `Microsoft.CodeAnalysis.*` reference except `*.Testing`. That includes `Microsoft.CodeAnalysis.Analyzers` in `Reihitsu.Analyzer.csproj` (today `5.12.0`, so it holds). The lead may narrow the filter if the Analyzers package should be allowed its own version line.

## Not covered, and why

- FC-001 to FC-044 except FC-003 and FC-017: the change touches no analyzer, code fix, formatter phase, trivia, or rewrite; the formatter, code-fix, unsupported-shape and stability classes cannot reach a file mode or a package version.
- Line endings: no layout is involved.
- Running `scripts/trace.sh` itself from a test: the mode in the index is the issue's acceptance criterion, and running the script would build and possibly install the SDK inside a test.

## Commands

```shell
scripts/test.sh --project architecture --filter "FullyQualifiedName~ShellScriptExecutableModeTests"
scripts/test.sh --project architecture --filter "FullyQualifiedName~RoslynPackageVersionTests"
```

The whole architecture suite on this head: 160 passed, 1 failed (`EveryTrackedShellScriptIsExecutable`, the #898 reproduction).

## Follow-up candidates

- `scripts/proof/verify-text-only.cs` line 1 pins `#:package Microsoft.CodeAnalysis.CSharp@5.6.0`, a different Roslyn version from the `5.12.0` every project uses. It is a file-based program outside `src/`, not linked with the formatter, so it is outside #1780's stated scope and the guard does not read `#:package` directives. Whether "one Roslyn version" should cover it (bump it, or extend the guard to `#:package` lines) is the lead's call.
