Role: auditor
Commit: b08b083
Verdict: pass, text cleanup

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit. Head `b08b083116b9fb1a0ce49d455db5485a8dca47ca` (checked out, clean tree). Base `origin/main` = `dc6d942bd6200274b5a23b74e8b661f1491f11d5` (`git ls-remote` agrees), an ancestor of the head. Diff against the base: `scripts/test.sh`, `scripts/README.md`, `.github/workflows/build.yml`, `.github/workflows/SonarCloud.yml`, and this run's `sdd/work/` files; it matches `01-implementer.md`. `scripts/lane-check.sh tooling` → `confirmed — claimed tooling, diff needs tooling`.

Issue thoenissen/Reihitsu#1850 (`agent-filed`) "Done when": a test that blocks forever makes `scripts/test.sh` and the CI test job fail within a stated limit, and the limit is documented in `scripts/README.md`.
- `scripts/test.sh`: every `dotnet test` call (both branches, filter or not) gets `--blame-hang-timeout 10min --blame-hang-dump-type none`, before the caller's arguments and the run-settings `--`.
- CI: `build.yml` tests only through `scripts/test.sh`; `SonarCloud.yml` passes the same two arguments in both test steps (coverage loop and "without Sonar"). Job backstops `timeout-minutes: 30` and `120`. Recent completed SonarCloud runs (#1905–#1918) took 27–43 minutes, so 120 does not cut a normal run.
- Documented: `scripts/README.md`, "Hang timeout".

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | low | `scripts/README.md:86`, "Hang timeout", second sentence | – | – | PR-introduced | "A test that blocks forever therefore fails the run about 10 minutes after the last test before it finished" holds only for a host that runs tests one at a time. Core, Architecture, Playground, and SelfHosting run method-level parallel (`[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]`), so other tests keep starting and finishing after the blocked test started, each resetting the timer; the host aborts 10 minutes after the last other test in the host finished. Shown: a Core test host with `BlocksForever` (`Thread.Sleep(Timeout.Infinite)`) and `QuickOne` (`Thread.Sleep(3000)`) started together under `--blame-hang-timeout 20s` aborted about 20 s after `QuickOne` finished (24.6 s wall), not 20 s after a test "before" the blocked one. Text only: a sentence in a Markdown file, which no build, test, or diagnostic reads. | Say "about 10 minutes after the last other test in its test host finished". |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| End a hung test host | VSTest blame collector inactivity: time since the last test start or end in one test host, from session start | `[Timeout(..., CooperativeCancellation = true)]` per test (stops only token-observing code); job `timeout-minutes` (whole job, covers hangs outside a test host: build, `dotnet-coverage merge`, Sonar scanner) | Complementary: the blame timer covers the test host whatever the code checks; the job limit covers the rest. Both arguments reach every `dotnet test` call in `scripts/test.sh` and both `SonarCloud.yml` test steps; `rg "dotnet test"` finds no other caller in `scripts/` or `.github/` | ok |
| Failure propagates | `dotnet test` exit 1 on abort | `set -euo pipefail` in `test.sh`; Actions `run:` under `bash -e`; `dotnet-coverage collect` propagates the exit code (implementer, 603 s run) | Agree; my run reproduced exit 1 and `The active Test Run was aborted …` / `The test running when the crash occurred: … BlocksForever` | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Hang limit and dump type | `rg -n "blame-hang"`: `scripts/test.sh:137` (`hang_arguments`), `.github/workflows/SonarCloud.yml:62`, `:74`; prose in `scripts/README.md:86` | Three owners, justified: a workflow cannot read a Bash variable, and the coverage step calls `dotnet test` inside `dotnet-coverage collect` rather than `test.sh`. Parity holds today (identical strings), and each workflow comment points to the README section. Accepted (FC-015 justified and proven) |
| Job time limit | `timeout-minutes` only in `build.yml:18`, `SonarCloud.yml:18` | One owner each; lead decision 3 drops the other workflows, which run no tests |

## Assertion adequacy
_N/A — the diff adds or changes no test. The "Done when" demonstration was a temporary, uncommitted test; the implementer's handover records the commands and outputs, and I reproduced the mechanism independently (below)._

## Verification performed
- Temporary worktree outside the repository (`git worktree`, removed afterwards), temporary `TemporaryHangTests.cs` in `Reihitsu.Core.Test` with one blocking and one 3-second test, Debug build, then `dotnet test … --no-build --verbosity minimal --blame-hang-timeout 20s --blame-hang-dump-type none --filter FullyQualifiedName~TemporaryHangTests` → exit 1 after 24.6 s; output contains `The active test run was aborted. Reason: Test host process crashed`, `Data collector 'Blame' message: The specified inactivity time of 20 seconds has elapsed.`, `Test Run Aborted.`, `The active Test Run was aborted because the host process exited unexpectedly.`, `The test running when the crash occurred: Reihitsu.Core.Test.TemporaryHangTests.BlocksForever`. The README's quoted message and "naming the test that was running" hold. No dump written.
- `bash -n scripts/test.sh` ok; both workflow files parse as YAML.
- Repository-wide file enumerations in tests (`RoslynVersionOwnership.Check`, `SpecCatalog`, `SelfHostingSolution`, `FormatterSourceCatalog`, `SelfReferentialTrackerReferenceTests`) filter by `*.cs`, `*.md`, `*.csproj`, or build-file extensions, so the blame collector's `TestResults/` output (git-ignored, `.gitignore:44`; empty GUID directories after a passing run, a `Sequence_*.xml` after a hang) cannot reach them.

## Failure classes dismissed
- FC-001, FC-002, FC-003 — the one guard is a command-line option, not a predicate over syntax; its span is traced above.
- FC-016 — no sibling `dotnet test` caller lacks the arguments (`rg` above); `release.yml`, `deploy-pages.yml`, `pr-labels.yml` run no tests.
- FC-017, FC-033, FC-034, FC-035 — no test added or changed.
- FC-031 — the changed `test.sh` header, the `hang_arguments` comment, and both workflow comments describe the new code.
- FC-032 — the implementer's passing runs (all projects, self-hosting) were taken with the guard in place; the merge of `dc6d942` touched none of these files.
- FC-037 — no C# file changed.
- FC-004 to FC-014, FC-018 to FC-030, FC-036, FC-038 to FC-044 — analyzer, code-fix, formatter, and precheck classes; the diff changes no compiled code.

## Architecture findings
_None._

## Hints
- The PR's own CI is the first run of the arguments on GitHub Actions and under `dotnet-coverage` instrumentation for the whole suite. A legitimate step that is silent for 10 minutes inside a test host (for example one-time solution loading in `Reihitsu.SelfHosting.Test` under coverage) would abort there; the implementer's local self-hosting run under the guard did not, and the longest local test was 33.4 s.
- Under method-level parallelization the summary line of an aborted project can still read `Passed!` (seen in my run) before `Test Run Aborted.`; the exit code is 1, so nothing is masked.

## Follow-up candidates
_None._
