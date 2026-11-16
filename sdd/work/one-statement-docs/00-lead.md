# Lead — one-statement-docs

- Branch: `claude/awesome-cerf-nc79lq` (session-designated)
- PR: thoenissen/Reihitsu#1793 (draft)
- Slug: `one-statement-docs`
- Lane: **tooling**

## Issues

| Issue | agent-filed | Lane on its own | Why |
|---|---|---|---|
| thoenissen/Reihitsu#1470 — default test run's coverage stated in one place | yes | tooling | Text in `scripts/`, `squad/`, `.claude/`, `AGENTS.md`, `CONTRIBUTING.md`: tooling paths for the lane check (Markdown under `scripts/`, `squad/`, `.claude/` and `AGENTS.md` are tooling) |
| thoenissen/Reihitsu#1471 — lane classification has one statement the lane check executes | yes | tooling | Changes `src/Reihitsu.Tooling/LaneClassifier.cs` and its tests, plus `scripts/README.md` and `squad/README.md` |
| thoenissen/Reihitsu#1479 — tab-width matrix in `squad/failure-classes.md` | yes | tooling | `squad/` text |
| thoenissen/Reihitsu#1544 — phase-ownership table rows in `AGENTS.md` | yes | tooling | `AGENTS.md`; architecture tests check the table |

All four issues are `agent-filed`: their goals and "Done when" items are proposals.

Issue comment on #1544 (maintainer account, 2026-11-16): the `LineBreakPhase` row already says the phase keeps every blank line; only the missing `BraceBlankLinePolicy` in the `BlankLinePhase` row remains.

## Roles

Tooling lane: test implementer (for #1471, the lane classifier has tests; the other issues have tests only where an architecture test checks the text), implementer (writes the code and every text change, since the spec author does not run in this lane), cleaner, spec checker, auditor, validator. Skipped: spec author and skeptic (the tooling lane does not run them).

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Start | Batch of four issues with partly different mechanisms — split? | Keep one run | All four restate a rule in documents that drift from its one executed or specified statement; all text-sized, no code fix or rule; the maintainer named them together | Repository tooling and agent docs only | 5de9bc5 |

## Follow-up candidates

None yet.

## State

- Last step: claim, draft PR, prepare.sh done.
- Next: test implementer.
