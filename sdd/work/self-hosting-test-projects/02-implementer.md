Role: implementer
Commit: f25f4d4 (changes made on top of it; see the commit that adds this file)
Verdict: tests pass

The prompt carried no intended fix, suspected cause, or verdict.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `Reihitsu.sln` | Lists `src/Reihitsu.SelfHosting.Test/Reihitsu.SelfHosting.Test.csproj` (new project GUID `{4CB7F7DF-CD2F-4899-A5A0-3538368A13A0}`), with the six Debug/Release × Any CPU/x64/x86 configuration pairs and nested under the `Tests` solution folder, laid out exactly like `Reihitsu.Playground.Test`. Edited by hand, not with `dotnet sln add`, so no other line moved | tool: solution build and whole-solution `dotnet test` include the scans (`SolutionListsTheSelfHostingTestProject`) |
| `scripts/test.sh` | New `selfhosting)` single-project branch; `all` unchanged, so the default run leaves the scans out; usage line, both `--project` error messages, a header paragraph explaining why `all` skips `selfhosting`, and a `--project selfhosting` example | tool: default run skips, alias selects (`ShellAllIncludesEverySolutionTestProjectExceptSelfHosting`, `NamedSelfHostingSelectionSelectsOnlyTheSelfHostingProject`, `ShellAllLeavesOutTheSelfHostingProject`) |
| `.github/workflows/SonarCloud.yml` | `Reihitsu.SelfHosting.Test` appended to the `for project in ...` test loop. The fork-PR branch `Test (without Sonar)` runs `dotnet test` over the whole solution and picks the project up through `Reihitsu.sln` | tool: CI runs the scans on every PR (`SonarCloudTestLoopRunsTheSelfHostingProject`, `SonarCloudTestLoopRunsEverySolutionTestProjectExceptArchitecture`) |
| `scripts/README.md` | `test.sh` table row, example, new section *The self-hosting scans*; dropped the clause "so local full validation covers every solution test project", which is no longer true | documentation of the above |
| `squad/roles/validator.md` | The default run covers every project except `Reihitsu.SelfHosting.Test`; CI runs it, and the validator does not add it. The rule against narrowing with `--project` is unchanged | issue's file list |
| `squad/roles/implementer.md` | Step 6: the self-hosting scans are not part of a local run; CI runs them | issue's file list |
| `squad/roles/test-implementer.md` | Helper table: `SelfHostingTests` → `FormatterSelfHostingTests` in `Reihitsu.SelfHosting.Test`, run in CI (the test implementer's follow-up note) | follow-up from `01-test-implementer.md` |
| `AGENTS.md` | Comment on `scripts/test.sh` in the build block: "all test projects" → "all test projects except the self-hosting scans, which CI runs (--project selfhosting)"; not in the issue's file list, but the old comment became false with this change | consequence of the default-run change |
| `sdd/README.md` | Line 80, the list of test projects that declare `RequirementAttribute`, now names `Reihitsu.SelfHosting.Test` (it declares one for its `FMT-PIPE-004` tags); not in the issue's file list, but the list became incomplete with this run. Text only; the spec checker may want to confirm it | consequence of the new project |

No C# file changed. No public API, no dependency, no shared helper.

## Policy owners

The test-project selection has these owners; every one was checked and updated or confirmed unaffected:

- `rg -n "Reihitsu\.(Playground|Tooling)\.Test" --glob '!sdd/work/**' --glob '!**/bin/**' --glob '!**/obj/**' -l` → `scripts/test.sh` (updated), `scripts/README.md` (updated), `sdd/README.md` (updated, attribute list), `Reihitsu.sln` (updated), `src/Reihitsu.ArchitectureTests/Properties/Assemblies.cs` and `General/AssembliesTests.cs` (assembly loading for enumeration rules; left out deliberately, as the test implementer's handover argues), `src/Reihitsu.ArchitectureTests/General/TestScriptProjectSelectionTests.cs` (the guarding tests). `.github/workflows/SonarCloud.yml` names projects without the `.csproj` path and was found by reading the workflows: `build.yml` runs only `--project architecture`; `release.yml` and `deploy-pages.yml` run no tests.
- `rg -n -i "self-?hosting|test\.sh\b" squad/ .claude/ AGENTS.md CLAUDE.md scripts/README.md sdd/README.md README.MD` → the hits changed above; the rest (`cleaner.md`, `auditor.md`, `skeptic.md`, `spec-checker.md`, `.claude/commands/*`) name focused `--project` runs and stay correct.

## Tests run

Before the change (architecture tests read the files at runtime, so the stashed tree was checked without a rebuild):

```text
scripts/test.sh --no-install --no-build --configuration Debug --project architecture --filter "FullyQualifiedName~TestScriptProjectSelectionTests"
  Failed NamedSelfHostingSelectionSelectsOnlyTheSelfHostingProject
  Failed ShellAllIncludesEverySolutionTestProjectExceptSelfHosting
  Failed SolutionListsTheSelfHostingTestProject
  Failed SonarCloudTestLoopRunsTheSelfHostingProject
Failed!  - Failed:     4, Passed:     3, Skipped:     0, Total:     7
```

After the change:

```text
scripts/build.sh --configuration Debug --no-install
Build succeeded. 0 Warning(s) 0 Error(s)

scripts/test.sh --no-install --no-build --configuration Debug --project architecture
Passed!  - Failed:     0, Passed:    89, Skipped:     0, Total:    89 - Reihitsu.ArchitectureTests.dll (net10.0)

scripts/test.sh --no-install --no-build --configuration Debug --project selfhosting --filter "FullyQualifiedName~AnalyzerViolationReportTests|FullyQualifiedName~SourceFileRowsCover"
Passed!  - Failed:     0, Passed:     6, Skipped:     0, Total:     6 - Reihitsu.SelfHosting.Test.dll (net10.0)

scripts/test.sh --project bogus
test.sh: unknown --project value 'bogus' (use analyzer, formatter, core, cli, architecture, tooling, playground, selfhosting, or all).

scripts/lane-check.sh tooling --head worktree --no-install
LANE CHECK: confirmed — claimed tooling, diff needs tooling
```

Whole self-hosting project, wired through the new alias (Debug, `--no-build`, 4-core sandbox; full log outside the repository):

```text
scripts/test.sh --no-install --no-build --configuration Debug --project selfhosting
Passed!  - Failed:     0, Passed:  3588, Skipped:     0, Total:  3588, Duration: 17 m 39 s - Reihitsu.SelfHosting.Test.dll (net10.0)
```

3588 = 340 analyzer rows + 2 × 1621 formatter rows + 6 tool tests. The validator's default run will not execute this project; CI's SonarCloud loop is the next run that does.

## Disputed tests or scope

_None._

## Follow-up candidates

_None._
