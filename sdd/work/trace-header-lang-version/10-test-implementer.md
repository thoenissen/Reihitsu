Role: test implementer
Commit: 6de5ba1 (repair on top; this commit carries it)
Verdict: tests written (repair round 1: harness repaired, line-count rows pass)

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Cause
`src/Reihitsu.Tooling/Reihitsu.Tooling.csproj` references `Reihitsu.Analyzer` and `Reihitsu.Analyzer.CodeFixes` only; it reaches `Reihitsu.Formatter` through `Reihitsu.Analyzer.CodeFixes` (a transitive project reference). MSBuild passes transitive project outputs to the compiler, so the real build compiles (the validator's `scripts/build.sh` had 0 errors). The self-hosting harness, `SelfHostingSolution.GetCompilationReferences`, added only the **direct** `ProjectReference`s as compilation references (and removes every loaded assembly named like a repository project, so the loaded `Reihitsu.Formatter.dll` was not a fallback), although the doc of `CreateCompilation` already says "including transitive project references". Before this run no Tooling source used a formatter type; `CodeFixRunCommand.cs` now uses `Reihitsu.Formatter` / `LanguageVersionResolver` (decision 5, `InternalsVisibleTo("Reihitsu.Tooling")`), which exposed the gap. The defect is in the test harness, not in production or tooling code: the production project graph is valid as MSBuild builds it.

## Repair
`tests/Reihitsu.SelfHosting.Test/Utilities/SelfHostingSolution.cs`: new `GetTransitiveProjectReferences` walks the project-reference graph breadth-first (each project once, undiscovered reference still fails with the same message); `GetCompilationReferences` adds a compilation reference for every project in that closure. Cycle detection stays in `CreateCompilation`. `InternalsVisibleTo` is honored because the Tooling compilation's assembly name is `Reihitsu.Tooling`.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `LineCountSelfHostingTests` / `AnalyzerReportsNoViolationsOnOwnSourceCode` rows RH2008, RH2111 (existing, unchanged) | — (harness) | `SelfHostingSolution` | pass (18/18) |

The failure before the repair is the validator's run at 7a6163d (`09-validator.md`, 4 compilation errors in `Reihitsu.Tooling`); the same rows pass after it.

## Scenarios (bug fix)
_N/A (repair of the harness)._

## Not covered, and why
- No dedicated unit test for the transitive walk: every self-hosting row builds the compilations through it, and the Tooling project is a live transitive case, so the line-count rows and the default rows exercise both direct and transitive references.
- `DiscoverProjects` still skips a project only when a **direct** reference is unhostable; no repository project reaches an unhostable project transitively today, so it was left as is (follow-up candidate below).

## Commands
- `scripts/build.sh` → 0 warnings, 0 errors.
- `scripts/test.sh --no-build --project selfhosting --filter "Name~RH2008|Name~RH2111"` → Passed 18, Failed 0.
- `scripts/test.sh --no-build --project selfhosting` (default rows) → Passed 6253, Failed 1 of 6254. The one failure is `WorkingDocumentsTests.WorkingDocumentsDirectoryContainsNoFiles`, which lists the files under `sdd/work/trace-header-lang-version/`; it is expected while the run's working docs exist and clears when the lead deletes them before the PR is ready. No analyzer or formatter self-hosting row failed.
- `scripts/format.sh tests/Reihitsu.SelfHosting.Test/Utilities/SelfHostingSolution.cs` → `Formatted 0 of 1 file(s).`

## Follow-up candidates
- (small) `SelfHostingSolution.DiscoverProjects` checks only direct references for unhostable projects; with transitive references now compiled, a project that reaches an unhostable project through another one would fail with "references undiscovered project" instead of being skipped. Not reachable in the repository today.
