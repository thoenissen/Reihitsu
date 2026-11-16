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

## Follow-up candidates

None yet.

## Worklist / next step

- First implementer spawn lost: the container restarted while it was measuring, before it committed anything (cause not established; a memory-exhausted container is one possibility).
- Next: implementer, fresh spawn (`01-implementer.md`).
