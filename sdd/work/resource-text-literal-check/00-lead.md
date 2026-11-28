# Lead — resource-text-literal-check

- Issue: thoenissen/Reihitsu#1970 — `agent-filed` (goals are proposals). Labels: analyzer, maintenance, agent-filed. Milestone 1.6.0.
- Slug: `resource-text-literal-check`
- Branch: `claude/dreamy-cerf-dvesr3` (designated by the session)
- PR: thoenissen/Reihitsu#1992 (draft claim)
- Lane: `tests` — the requested change is a new architecture test under `tests/Reihitsu.ArchitectureTests/`, plus replacing any literals it finds in other test files; no production, no change doc, no visible output.
- Roles run: skeptic (premise and design points against the code), test implementer (writes the architecture test and replaces offending literals), cleaner, spec checker, validator.
- Roles skipped: spec author — no spec exists for architecture tests and the run adds none; implementer and auditor — not part of the tests lane.
- Maintainer clarifications: none.

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Spec author in the tests lane | Skipped | No spec under `sdd/specs/` covers architecture tests; the run adds none | Internal test tooling only | — |

## Follow-up candidates

None yet.

## State

- Last step: claim pushed, draft PR opened, `scripts/prepare.sh` green.
- Next: skeptic.
