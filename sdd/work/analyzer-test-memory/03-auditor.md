Role: auditor
Commit: 579cae86
Verdict: pass, text cleanup

## Scope
Head `579cae86d2283c2912ada650e130827b41a7e462` (checked out; the spec checker's `6981319a` landed on top during the audit and adds only `02-spec-checker.md`). `origin/main` = `eb89003591b8371d91e4a14b8590d764bdf6d605`, confirmed by `git fetch` and merged into the head (`git merge-base --is-ancestor`). Full audit. Diff against `origin/main`: `scripts/test.sh` (+24), `scripts/README.md` (+6), and the run's work files; it matches `01-implementer.md`, *Changes*. `scripts/lane-check.sh tooling --no-install`: `confirmed — claimed tooling, diff needs tooling`.

No test was re-run: no finding turns on a test result. The implementer's raw measurements in its scratch area (`.../scratchpad/m/*.summary`, `*.out`, `*.samples`, `bounded.sh`) were read and agree with the handover's figures (w4 9410 MiB, w4_mmap_arena2 4452 MiB, full_after 3841 MiB rc 0, w1 aborted after 3806 tests at the 10 GiB cap). The analyzer test project runs under VSTest (`Microsoft.NET.Test.Sdk` + `MSTest.TestAdapter`, no Microsoft.Testing.Platform switch, no `global.json`), so `-- RunConfiguration.EnvironmentVariables.<name>=<value>` is the right channel; `envprobe` shows the value in the test host's `/proc/<pid>/environ`.

Issue thoenissen/Reihitsu#1791 "Done when":
1. Cause measured — held: worker counts 1 and 4, managed vs. native split, `malloc_info` and `malloc_trim`.
2. Whole analyzer project runs in the sandbox — held: full default run peaked at 3841 MiB under a 12 GiB cap, no OOM kill. Achieved by fixing what grows, an option the issue names; the worker-count goal is dropped on evidence (lead decision 2), which is legitimate for an `agent-filed` issue and puts no visible output in question.
3. No noticeably longer — held on the measurements: analyzer durations overlap with and without the setting; CI reaches `test.sh` only for ArchitectureTests (`.github/workflows/build.yml:44`), unchanged at 21 s; `SonarCloud.yml` uses plain `dotnet test` and is not reached; macOS is skipped by the `uname` check.
4. No role file names a workaround — held: `rg -n -i "memory|--project analyzer|per project|split|chunk|OOM" squad/roles squad/lead.md squad/README.md .claude/agents AGENTS.md CLAUDE.md` finds none; `squad/roles/validator.md` runs plain `scripts/test.sh --no-build`.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | low | `scripts/README.md:86` ("so neither the build nor any other process sees it"); `scripts/test.sh:107` ("The setting reaches the test host only") | – | – | PR-introduced | Both texts claim only the test host receives `GLIBC_TUNABLES`. Every process the test host starts inherits it: `ApplyFixWrapperPositionalArgumentTests` (`src/Reihitsu.ArchitectureTests/General/ApplyFixWrapperPositionalArgumentTests.cs:161`, `ProcessStartInfo` with the inherited environment) runs `bash scripts/apply-fix.sh`, which builds and runs a file-based app through `dotnet`, all with the setting. Harmless in effect (the full run with the setting passed ArchitectureTests 153), but the statement is false. Text-only: a Markdown sentence and a Bash comment, neither reaches compiled behavior, diagnostics, or rule metadata | Say that the setting reaches the test host and the processes it starts, not the build or `dotnet test` itself |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Apply the setting at all | `uname -s` equals `Linux` and `GLIBC_TUNABLES` is unset (`${GLIBC_TUNABLES+set}`, so an empty value counts as set) | `scripts/README.md`, *Test-host memory*: "On Linux … A caller that sets `GLIBC_TUNABLES` itself keeps its own value, and other platforms are not affected" | Agree: set-but-empty is "set by the caller" and keeps the caller's (empty) value; exercised on both sides by the implementer (`GLIBC_TUNABLES= bash -x …` and unset). A musl Linux would get a tunable glibc reads and musl ignores: harmless | holds |
| Add the `--` separator | every element of `test_arguments` (all caller arguments test.sh does not consume), compared with `--` exactly | `dotnet test`'s own parsing: the first `--` starts inline run settings | Agree: a caller `--` anywhere means the setting appended at the end lands after it, among run settings; no caller `--` means one is added. Exercised both sides (`-- MSTest.Parallelize.Workers=1` and none). A `--` given as an option's value is not a shape any repository caller passes | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| glibc malloc tunables for test hosts | `rg -n "dotnet test\|GLIBC\|MALLOC\|DOTNET_GC\|export DOTNET" scripts squad .claude .github`: `scripts/test.sh:110-124` (only owner), described in `scripts/README.md:84-88`. `SonarCloud.yml:58,70` run `dotnet test` without it; no defect there today (lead decision 4) | one owner |

## Assertion adequacy
_N/A — the run adds and changes no test. Lead decision 3 skips a test for the argument composition; the behavior it would guard either fails loudly on every run (a broken `--`) or only shows as memory growth, which no repository test observes. `TestScriptProjectSelectionTests` (project selection) is unaffected: the `all)` branch is unchanged._

## Failure classes dismissed
- FC-001, FC-002, FC-003 — traced above for the two Bash guards; both sides of each boundary were exercised.
- FC-015 — one owner (above).
- FC-016 — the sibling path, `SonarCloud.yml`'s plain `dotnet test`, runs on runners whose memory fits the measured 9.4 GiB peak; no instance of the hazard today, and the lead dropped it on that ground (decision 4).
- FC-017, FC-033, FC-034, FC-035 — no test added or changed.
- FC-031 — the new comment in `scripts/test.sh` describes the new code; its one inaccuracy is finding #1. The script's header usage comment needs no change (no new option).
- FC-032 — no compiled change; the full run with the setting (`full_after`) was taken on `9b401f7`, the commit that carries the change, and nothing compiled changed since.
- FC-037 — no C# file changed.
- FC-004 to FC-014, FC-018 to FC-030, FC-036, FC-038 to FC-044 — formatter, analyzer, code-fix, and precheck classes; the diff touches no source under `src/` and no formatter, analyzer, or code-fix path.

## Architecture findings
_None._

## Hints
- `01-implementer.md` omits one run present in its raw data: `w1t` (`Workers: 1, Scope: ClassLevel`, no setting) passed all 10419 tests in 7.3 min with a peak test-host RSS of 9164 MiB under the 10 GiB cap. It supports the stated conclusion (the peak does not scale with the worker count) and changes nothing shipped; it also shows a single worker is about 40 % slower than the default, which further argues against the dropped worker-count goal.
- The source comment and README carry measured figures ("about 9 GiB", "0.7 GiB", "about 4.5 GiB"); they will age with the test suite. Not a defect.

## Follow-up candidates
_None._
