Role: implementer
Commit: 1f3f7b0
Verdict: tests pass

No test-implementer handover exists for this run (tooling lane, no test project covers `scripts/test.sh` or the workflows). The "Done when" demonstration was performed here with a temporary, uncommitted blocking test; its file is deleted and is not in any commit.

## Premise check

- Issue claim "no workflow sets `timeout-minutes`": confirmed for `build.yml` and `SonarCloud.yml` on `db991a7`.
- Issue claim "not verified whether `--blame-hang-timeout` works with this MSTest runner configuration": the test projects run MSTest 4.5.3 through VSTest (`Microsoft.NET.Test.Sdk` 18.12.0, no Microsoft.Testing.Platform opt-in), so `dotnet test` uses the VSTest blame data collector. Verified empirically (below): `--blame-hang-timeout` with `--blame-hang-dump-type none` aborts a test host whose test blocks forever, names the test, and makes `dotnet test` exit 1.
- Correction to the issue's framing: in `build.yml` the CI test step is the architecture tests only, run through `scripts/test.sh`; the job that runs every test project is `SonarCloud.yml`, which calls `dotnet test` (wrapped in `dotnet-coverage collect`) directly, so it needs the arguments itself.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `scripts/test.sh` | Every `dotnet test` call passes `--blame-hang-timeout 10min --blame-hang-dump-type none` (before the run-settings `--`, so caller run settings are unaffected); header and inline comments state why | Issue #1850 "Done when" |
| `.github/workflows/SonarCloud.yml` | Both test steps (coverage loop and "without Sonar") pass the same two arguments; job `timeout-minutes: 120` as a backstop (normal job ~30-42 min observed on recent runs) | Issue #1850 |
| `.github/workflows/build.yml` | Job `timeout-minutes: 30` as a backstop (normal job ~3 min); its test step already goes through `scripts/test.sh` | Issue #1850 |
| `scripts/README.md` | New section "Hang timeout" stating the 10-minute limit, what it measures (inactivity of a test host, not run length), the failure message, that CI applies the same limit, and the `timeout-minutes` backstops; table row for `test.sh` links it | Issue #1850 "the limit is documented" |

Mechanism chosen: both — the hang timeout fails the hung test host with a message naming the test; `timeout-minutes` only catches a hang outside a test host.

Limit choice: the longest single test in a full local Release run (default set plus self-hosting, 4 cores) was 33.4 s (`FormatterIsIdempotentOnSourceFile ("src/Reihitsu.Cli.Test/Unit/FormatCommandHandlerTests.cs")`; next 32.1 s `AnalyzerReportsNoViolationsOnOwnSourceCode ("RH1001…")`). Analyzer tests max 2.0 s, formatter tests max 7.8 s. Because the blame timer is inactivity since the last test start/end, the gap it measures never exceeds the longest single test; 10 minutes is ~18x that, leaving room for coverage instrumentation and slow CI runners.

## Policy owners
The limit is stated in three places, which cannot share a variable: `rg -n "blame-hang" scripts .github` →
- `scripts/test.sh:137` (`hang_arguments`)
- `.github/workflows/SonarCloud.yml:62`, `:74`
`scripts/README.md`, "Hang timeout", is the one prose statement, and each workflow comment points to it.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` → empty (no `src` change). No new type.

## Deleted special cases
_N/A._

## Tests run

Demonstration (temporary file `src/Reihitsu.Core.Test/TemporaryHangTests.cs`, one `[TestMethod]` calling `Thread.Sleep(Timeout.Infinite)`; built in Debug; deleted afterwards):

- Mechanism check, 30 s limit: `dotnet test src/Reihitsu.Core.Test/Reihitsu.Core.Test.csproj -c Debug --no-build --filter "FullyQualifiedName~TemporaryHangTests" --blame-hang-timeout 30s --blame-hang-dump-type none` → `Data collector 'Blame' message: The specified inactivity time of 30 seconds has elapsed.` / `Test Run Aborted.` / `The test running when the crash occurred: Reihitsu.Core.Test.TemporaryHangTests.BlocksForever`; exit 1 after 32 s.
- `scripts/test.sh` (the `build.yml` path): `scripts/test.sh --configuration Debug --no-build --project core --filter "FullyQualifiedName~TemporaryHangTests"` → `The specified inactivity time of 10 minutes has elapsed.` / `Test Run Aborted.`, names `BlocksForever`; exit 1 after 602 s.
- SonarCloud test-step command line, under `bash -e` as Actions runs `run:` blocks: `dotnet-coverage collect "dotnet test …/Reihitsu.Core.Test.csproj --no-build --configuration Debug -m --blame-hang-timeout 10min --blame-hang-dump-type none --filter FullyQualifiedName~TemporaryHangTests" -f xml -o coverage-demo.xml` (dotnet-coverage 18.12.0) → same abort message; `dotnet-coverage` propagates exit 1, and the following command in the script was not reached; 603 s.
- A hung run leaves only a 261-byte `Sequence_*.xml` under the project's `TestResults/` (git-ignored by `.gitignore:44`); no dump.
- Without the guard, the issue already records the hang (killed by an outer `timeout 240`); not re-run.

No legitimate test trips the guard (Release, with the guard in place):
- `scripts/test.sh --no-build --project all` (temporary file present, filtered out): Analyzer `Passed: 10639`, Formatter `Passed: 5738`; Core could not run because the temporary file failed the Release build.
- After deleting the temporary file and rebuilding Release (`0 Warning(s)`, `0 Error(s)`): core `Passed: 738`, cli `Passed: 385`, architecture `Passed: 198`, tooling `Passed: 133`, playground `Passed: 23`.
- `scripts/test.sh --no-build --project selfhosting`: `Failed: 3, Passed: 5840` in 18 m 9 s, no abort. The three failures are not from this change: two rows report the temporary file (`TemporaryHangTests.cs:21 [RH5603]`, `:11 [RH7305]`), and `WorkingDocumentsDirectoryContainsNoFiles` reports `sdd/work/` (this run's and `precheck-comma-linear-scan/`, see lead decision 2), which is expected until the run's working docs are deleted.

Other checks: `bash -n scripts/test.sh` ok; both workflow files parse as YAML; `scripts/lane-check.sh tooling --head worktree` → `LANE CHECK: confirmed — claimed tooling, diff needs tooling`.

Not verified: the workflows themselves on GitHub Actions (their own CI run on the PR shows whether the arguments are accepted there; a hang cannot be shown there without committing the blocking test).

## Disputed tests or scope
_None._ Scope is the four files the issue names.

## Architecture findings
_None._

## Follow-up candidates
- `release.yml`, `deploy-pages.yml`, and `pr-labels.yml` also set no `timeout-minutes`; outside this issue's files and run no tests. Small; the lead decides whether it is worth filing.
- A caller that passes its own `--blame-hang-timeout` as an extra argument to `scripts/test.sh` now gives `dotnet test` the option twice; not needed by any caller today, so not handled.
