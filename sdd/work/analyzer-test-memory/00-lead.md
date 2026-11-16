# Lead — analyzer-test-memory

- Issue: thoenissen/Reihitsu#1791 — `scripts/test.sh` is OOM-killed in the cloud sandbox while running `Reihitsu.Analyzer.Test`. Labels: `maintenance`, `agent-filed` (goals are proposals).
- Branch: `claude/dazzling-maxwell-ftisho` (session-designated). Draft PR: #1814.
- Slug: `analyzer-test-memory`
- Lane: **tooling** — the change is to repository tooling (`scripts/`, test-project parallelization settings, role files); no production code, no visible output. A test-project `AssemblyInfo.cs` classifies as test, which the tooling lane admits.
- Roles run: implementer (measures the cause, then changes the tooling), cleaner, spec checker, auditor, validator.
- Roles skipped: spec author, skeptic (not in the tooling lane); test implementer — deferred: `scripts/test.sh` is a Bash script whose only covering test is `TestScriptProjectSelectionTests` (project selection); the lead routes to the test implementer after the implementer if the change adds behavior a test can cover.
- Environment: cloud sandbox, 4 cores, 15 GiB RAM, no swap.

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Run the test implementer before the implementer? | Not up front; implementer measures first, test implementer runs afterwards only if the change has testable behavior | The mechanism is unknown until measured; a test written before would be guesswork | Tooling only | — |
| 2 | Implementer | Issue goal "bound the worker count" | Dropped; the change sets glibc malloc tunables for the test host instead | Measured: one worker grows as fast per test as four; retained freed native memory is the cause (01-implementer.md) | Tooling only | 9b401f7 |
| 3 | Implementer | Add a test for test.sh's argument assembly? | No; test implementer skipped | Bash glue exercised by every run; an architecture test could only grep the script text | Tooling only | — |
| 4 | Implementer | Follow-up: give SonarCloud.yml the setting | Dropped, no candidate filed | No defect today (CI runners fit the peak); speculative | Tooling only | — |
| 5 | Cleaner | Run the cleaner? | Skipped | Diff changes no C# or compiled file; nothing to format, Release build unaffected | Tooling only | — |
| 6 | Spec checker / auditor | Order | Run in parallel on the same commit | Neither changes the tree; findings form one worklist | Tooling only | — |

## Follow-up candidates

None yet.

## Worklist / next step

- First implementer spawn lost: the container restarted while it was measuring, before it committed anything (cause not established; a memory-exhausted container is one possibility).
- Implementer done: 01-implementer.md, verdict tests pass, commit 9b401f7.
- Spec checker 02: consistent. Auditor 03: pass, text cleanup (audited SHA 579cae86; finding #1: README:86 and test.sh:107 comment say only the test host gets GLIBC_TUNABLES, but its child processes inherit it).
- Repair round 1: cleaner 04 (test.sh comment, 91d16e16), spec author 05 (README, 0ad136d0). Text-only proof base 579cae86: FAIL (scripts/** is behavioral), so a re-audit is due.
- Next: spec checker (06) and auditor (07, repair only) in parallel on 0ad136d0, then validator (08).
