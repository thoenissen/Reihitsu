# Lead — arch-tests-shared-helpers

- Issue: thoenissen/Reihitsu#1994 — `agent-filed` (goals are proposals), label `maintenance`, milestone 1.6.0.
- Slug: `arch-tests-shared-helpers`
- Branch: `claude/optimistic-babbage-ir30vs` (session-designated)
- PR: thoenissen/Reihitsu#1998 (draft claim)
- Lane: `tests` — the issue changes only test code in `tests/Reihitsu.ArchitectureTests` (deduplicating helpers, no behavior change); the issue itself names this lane.
- Roles run: skeptic, test implementer, cleaner, spec checker, validator.
- Roles skipped: spec author (the run adds no spec), implementer and auditor (not in the tests lane).
- Maintainer clarifications: an issue comment (scope note) says main has 11 identical `FindRepositoryRoot` copies, 7 bin/obj skip sites with differing case comparison and dot-directory rules (the run picks one rule), and 4 fixture-directory types.

## Maintainer answers

(none)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

(none)

## State

- Last step: claim pushed, prepare.sh green.
- Next: skeptic.
