# Lead — message-format-test-literals

- Issue: thoenissen/Reihitsu#1912 — `agent-filed` (goals are proposals)
- Slug: `message-format-test-literals`
- Branch: `claude/cool-mendel-cmd1vb` (session-designated); draft PR #1967
- Lane: **tests** — the issue changes only test code so it reads diagnostic messages from the analyzer instead of quoting literals; no production code, no visible output.
- Roles run: skeptic (premise and scope against the code), test implementer (performs the test refactoring), cleaner, spec checker, validator. Skipped: spec author (the run adds no spec), implementer and auditor (not in the tests lane).
- Note: the issue names `src/*.Test/`; the test projects live under `tests/`. Its `rg` command finds 23 files under `tests/`, some of them in `Reihitsu.Tooling.Test` and `Reihitsu.ArchitectureTests`, which may not quote analyzer messages at all.

## Maintainer answers

None.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

None.

## Worklist / next step

- Claimed (commit on branch). Next: skeptic.
