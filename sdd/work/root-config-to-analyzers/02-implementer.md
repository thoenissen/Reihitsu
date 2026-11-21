Role: implementer
Commit: e629114 (changes in this commit; see `git log` for "Pass root reihitsu.json to the analyzers for #1880")
Verdict: tests pass

No intended fix, suspected cause, or hoped-for verdict was carried in the prompt.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `Directory.Build.props` | New unconditional `<ItemGroup>` with `<AdditionalFiles Include="$(MSBuildThisFileDirectory)reihitsu.json" />` after the `binaries/` analyzer items; its comment says the file is passed in every project and configuration. The sentence above the package-targets import that said the file is not passed (and why) is removed. | none (tooling); `BuildConfigurationTests.RepositoryBuildPassesRootConfigurationToTheAnalyzers` |
| `scripts/README.md` (*The line-count rows*, first paragraph) | Now states that the Release build (`scripts/build.sh`, and the default run, which builds in Release) fails on RH2008/RH2111 with the root limits, as the self-hosting scan does, and that the build runs the analyzers of the last version bump, so a rule change reaches the build only with the next rebuild of `binaries/`. The sentence "A change that adds lines to a test file can therefore pass both locally and fail CI" is removed. The rows' definition and command are unchanged. | none |

## Policy owners
- `rg -n "AdditionalFiles|reihitsu.json" -g '*.csproj' -g '*.props' -g '*.targets'`: the new item in `Directory.Build.props` is the only owner of the root file; the only other `AdditionalFiles` item is `@(RazorComponent)` in `src/Reihitsu.Analyzer.Package/buildTransitive/Reihitsu.Analyzer.targets`. No project adds a `reihitsu.json` of its own (the root file is the only one in the tree), so no duplicate item.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: empty (no production `src` change). Tooling: `Directory.Build.props` +8/-2, `scripts/README.md` 1 line. No new type.

## Deleted special cases
_N/A._

## Tests run
- Before the change: `scripts/test.sh --configuration Debug --project selfhosting --filter "FullyQualifiedName~Reihitsu.SelfHosting.Test.Repository.BuildConfigurationTests"` → `Failed: 1, Passed: 5` (`expected count: 1, actual count: 0`), as the handover says.
- After: same command → `Passed! - Failed: 0, Passed: 6`.
- `scripts/build.sh` (Release) → `Build succeeded. 0 Warning(s) 0 Error(s)`; `dotnet build Reihitsu.sln -c Release --no-incremental` output holds no `RH0001`, `RH2111`, or `RH2008` line.
- `scripts/build.sh --configuration Debug` → 0 warnings, 0 errors.
- `scripts/test.sh --no-build --project selfhosting --filter "FullyQualifiedName~Reihitsu.SelfHosting.Test.Repository"` → `Failed: 1, Passed: 32`; the failure is `WorkingDocumentsDirectoryContainsNoFiles`, which fails while `sdd/work/` exists during a run and clears when it is deleted before publishing. The full self-hosting scan (excluded from the default run) was not run.

## Disputed tests or scope
- `squad/roles/validator.md` (lines 19, 26, 49) still runs the line-count rows as an exception when a run adds lines to a test file. Its reason — the build did not judge the limits — is gone for every limit the 1.5.0 `binaries/` enforce; the rows still catch a change to RH2008/RH2111 themselves before the next bump. Left unchanged: whether to keep or drop the exception is a squad-process decision for the lead.

## Architecture findings
_None._

## Follow-up candidates
_None._
