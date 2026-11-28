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
| 2 | Skeptic | Goal "passes after the literals are replaced" | Narrowed to "passes on the repository as it stands": no test file quotes a resource text any more (01-skeptic) | Literals were already removed by an earlier clean-up | Test tooling only | — |
| 3 | Skeptic | Equality or containment | Containment of the decoded value (longest fixed fragment for placeholder values) in the decoded literal or interpolated text part | Equality misses embedded messages that occurred; containment had no false positive on either tree (01-skeptic) | Test tooling only | — |
| 4 | Skeptic | Minimum length / exemptions | Neither: no threshold, no exempt file | Shortest real values are two-word titles; resource tests quote no values (01-skeptic) | Test tooling only | — |

## Follow-up candidates

- Give `FindRepositoryRoot` (eight private copies in `tests/Reihitsu.ArchitectureTests`) and the near-identical fixture-directory types one owner — `01-skeptic.md`.

## State

- Last step: skeptic, `proceed narrower`, no output questions.
- Next: test implementer.
