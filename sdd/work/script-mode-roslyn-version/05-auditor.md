Role: auditor
Commit: 2d4c8635ceb1d9c438b881d71c2b361508870122
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope
Full audit. Head `2d4c863` (checked out, clean tree); `origin/main` and `git ls-remote origin refs/heads/main` are both `eb89003`, an ancestor of the head. The diff matches the handovers: the mode of `scripts/trace.sh` (content unchanged), `RoslynVersion` in the root `Directory.Build.props`, 12 references in 11 `src/` project files switched to `$(RoslynVersion)`, and four new files in `src/Reihitsu.ArchitectureTests/General/`. `scripts/lane-check.sh tooling`: confirmed. `scripts/trace.sh --help` runs without a `bash` prefix. `dotnet msbuild -getItem:PackageReference` on every `src/` project evaluates every non-Testing `Microsoft.CodeAnalysis*` reference to `5.12.0`, and neither the SDK nor any restored package defines a property named `RoslynVersion` that could override it.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | medium | `src/Reihitsu.ArchitectureTests/General/RoslynPackageReferences.cs:58-91`, `:150-212`; `RoslynPackageVersionTests.EveryProjectReferencesOneRoslynVersion` | FC-044, FC-017, FC-001 | – | new mechanism | The #1780 guard models MSBuild evaluation instead of reading it, and the model passes three shapes in which two projects resolve different Roslyn versions. Constructed against a copy of the reader (MSBuild applies all three): **(a) `Update` item** — `B.csproj` holds `<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="$(RoslynVersion)" />` and `<PackageReference Update="Microsoft.CodeAnalysis.CSharp" Version="5.6.0" />`; the reader skips the item without `Include` and reports both projects at `5.12.0`, no mismatch. **(b) Item in a props file** — the root `Directory.Build.props` adds `<PackageReference Include="Microsoft.CodeAnalysis.Workspaces.Common" Version="5.6.0" />` (this repository already adds `SonarAnalyzer.CSharp` there); the reader reads only properties and `PackageVersion` from props files, so the reference is never collected, no mismatch. **(c) Conditioned property** — `<RoslynVersion Condition="'$(MSBuildProjectName)'=='B'">5.6.0</RoslynVersion>` followed by `<RoslynVersion Condition="'$(RoslynVersion)'==''">5.12.0</RoslynVersion>`; MSBuild gives B `5.6.0`, the reader ignores `Condition`, takes the last definition, and reports both projects at `5.12.0`, no mismatch. Members that hold: a literal version in a project file, a nested `Directory.Build.props` redefining the property, an undefined property (`<unresolved>`). `01-test-implementer.md` states that an approximation error "cannot pass silently"; (a)-(c) pass silently. Separately, after decision 5 the reader's central-package-management, `VersionOverride`, and props-chain resolution, with `CentralPackageVersionIsResolved` and `VersionPropertyFromDirectoryBuildPropsIsResolved`'s chain case, serve options the run did not take and no file of the repository uses. | Stop modelling MSBuild. Either check the chosen mechanism with a cheap syntactic check — every non-Testing `Microsoft.CodeAnalysis*` `PackageReference`/`PackageVersion` item, `Include` or `Update`, in every tracked `*.csproj`/`*.props`/`*.targets` that reaches `src/` names exactly `Version="$(RoslynVersion)"` (no literal, no `VersionOverride`), and `RoslynVersion` is defined exactly once, without `Condition`, in the root `Directory.Build.props` — or read the computed result (`dotnet msbuild -getItem:PackageReference`, or the restored `obj/project.assets.json`). Drop the resolution paths and fixture tests that only serve options not taken. |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which Roslyn version a project uses (#1780 guard) | `PackageReference` elements with `Include` in each `src/**/*.csproj`; `PropertyGroup` children and `PackageVersion` items of every `Directory.Build.props`/`Directory.Packages.props` from the repository root down; `Condition` ignored | MSBuild evaluation: `Include` and `Update` items from the project and every import (props and targets), conditions applied, only the nearest `Directory.*.props` imported unless it chains | Disagree on `Update` items, items from imports, and `Condition` | Finding #1 |
| Which Roslyn packages are in scope | `Microsoft.CodeAnalysis` or `Microsoft.CodeAnalysis.*`, not ending in `.Testing`, case-insensitive | Issue #1780 wording ("every non-Testing `Microsoft.CodeAnalysis*` reference"); `RoslynVersion` comment in `Directory.Build.props` | Agree (includes `Microsoft.CodeAnalysis.Analyzers`, decision 3) | Holds |
| Which files are shell scripts (#898 guard) | `git ls-files --stage -- '*.sh'` (pathspec `*` crosses `/`): 10 entries under `scripts/` and `scripts/lib/` | Issue #898 ("every tracked `scripts/*.sh`"); tracked files with a `#!` line | Agree: the guard's set is a superset of the issue's; no tracked non-`.sh` file starts with `#!` | Holds |
| What "executable" means | Index mode `100755` | What a checkout reproduces on Linux/macOS | Agree; the index, not the working-tree bit, is what is committed | Holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Roslyn version of the `src/` projects | `Directory.Build.props:28` `RoslynVersion`; all 12 non-Testing references in `src/**/*.csproj` use `$(RoslynVersion)` (`rg '5\.12\.0|RoslynVersion'`); `scripts/trace/trace.cs`, `scripts/lane-check/lane-check.cs`, `scripts/apply-fix/*.cs` reach Roslyn only through `#:project` into `src/`, which evaluates in the project's own directory and inherits the property (lane-check built and ran) | One owner. `scripts/proof/verify-text-only.cs:1` `#:package Microsoft.CodeAnalysis.CSharp@5.6.0` is a second, standalone owner already recorded as a follow-up candidate (decision 4) |
| Mode of tracked shell scripts | git index; guard `ShellScriptExecutableModeTests` | One owner |
| Repository-root lookup, `FindExecutable`, `RunAsync` in architecture tests | `FindRepositoryRoot` privately in 10 test files; `FindExecutable`/`RunAsync` in `ApplyFixWrapperPositionalArgumentTests` and now `ShellScriptExecutableModeTests` | Existing private-copy convention of the test project; hint, not a finding |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `ShellScriptExecutableModeTests.EveryTrackedShellScriptIsExecutable` | Every tracked `*.sh` is `100755` in the index | `scripts/trace.sh` at `100644` (observed failing before the fix); vacuity blocked by asserting `scripts/trace.sh` is listed | `git ls-files --stage`; `Inconclusive` without git (decision 2; CI runs on `ubuntu-latest` with `actions/checkout`, so it runs there) | Holds |
| `RoslynPackageVersionTests.EveryProjectReferencesOneRoslynVersion` | Every non-Testing Roslyn reference of `src/` resolves to one version | Two projects resolving different versions | `RoslynPackageReferences.Collect`/`DescribeMismatch` | Weaker than the guarantee: passes (a)-(c) of finding #1 |
| `…DifferentLiteralVersionsAreReportedAsMismatch` / `…SameLiteralVersionsAreNotReported` | Mismatch on both sides of the boundary (FC-003) | Mismatch not reported / agreement reported | fixture directory | Holds |
| `…VersionPropertyFromDirectoryBuildPropsIsResolved` | `$(Property)` from a props file resolves | Property left unresolved | fixture directory | Holds for what it checks; part of the modelling finding #1 |
| `…CentralPackageVersionIsResolved` | Central `PackageVersion` and `VersionOverride` resolve | Wrong or missing version | fixture directory | Holds for what it checks; serves an option not taken (finding #1) |
| `…TestingPackagesAreNotCollected` | `*.Testing` excluded | Testing package collected | fixture directory | Holds |
| `…UnresolvableVersionIsReported` | An unresolved property fails the guard | `<unresolved>` passes | fixture directory | Holds |

## Failure classes dismissed
- FC-002, FC-003: FC-003 is covered on both sides by the fixture tests; FC-002 — no qualifier of a requirement; the issue scopes are matched (guard scope table).
- FC-015, FC-016: one owner per policy (policy table); the only sibling shape for #898, other tracked scripts, was enumerated (none at `100644`, no non-`.sh` script).
- FC-004, FC-005, FC-019-FC-021: no trivia, comment, or directive is edited by any shipped code.
- FC-006, FC-022-FC-027, FC-029, FC-036: no rewrite of C# source; the change is a file mode and an MSBuild property.
- FC-007-FC-009, FC-018, FC-030, FC-043: no code fix changed.
- FC-010-FC-014, FC-028, FC-038-FC-042: no formatter phase, precheck, or analyzer changed; the resolved Roslyn version stays `5.12.0`, so no compiled behavior moves.
- FC-031: every summary in the four new files describes its code; the `RoslynVersion` comment describes the property. No `TODO`, no tracker number (`rg '#[0-9]+|TODO'` empty).
- FC-032: the cleaner's build and the implementer's architecture run are on the final project files (round 2); the resolved versions are unchanged, so no earlier result is invalidated.
- FC-033-FC-035: the #898 reproduction failed on the real index entry; the #1780 tests use hand-written fixtures, not output of the code under test.
- FC-037: no comment-only change claimed.

## Architecture findings
_None._ Finding #1 is FC-044 inside one test helper, not a decision shared between components.

## Hints
- `Microsoft.CodeAnalysis.Analyzers` now moves with `RoslynVersion` (decision 3). It ships today on the same `5.12.0` line; a bump where that package has no matching release fails restore and shows itself at once, so it is not a silent risk.
- `FindExecutable`/`RunAsync` are copied from `ApplyFixWrapperPositionalArgumentTests`, and `FindRepositoryRoot` now has 11 private copies in the architecture tests; this follows the test project's existing convention.

## Follow-up candidates
- Already recorded in `00-lead.md`: the `scripts/proof/verify-text-only.cs` Roslyn pin, the S1125 warnings. Nothing new.
