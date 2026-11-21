# Lead — spec-wording-indent-diag

- Issue: thoenissen/Reihitsu#1901 — `agent-filed` (goals are proposals)
- Slug: `spec-wording-indent-diag`
- Branch: `claude/squad-1901-0qwkbz` (session-designated); draft PR #1904
- Lane: **docs** — the issue asks for two requirement texts to be reworded to match existing behavior; no output change, no code or test change is requested.
- Roles run: spec author (writes the text), spec checker. Validator only if a compiled file changes.
- Roles skipped: skeptic, test implementer, implementer, cleaner, auditor — not in the docs lane.
- Maintainer clarifications: none.

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Spec author | Issue goal 1 asks FMT-INDENT-012 to name every bracketed argument list | Name the three parents and exempt the indexer-initializer key (FMT-INDENT-014) | The issue's general wording would contradict FMT-INDENT-014; the code sends that case to `AlignDictionaryKey` | Spec text only; behavior unchanged | bc47ce6 |
| 2 | Spec author | Tag existing tests with FMT-INDENT-012 for the new example | Not in this run | Requirement is not new; tagging changes test files outside the docs lane | No output involved | bc47ce6 |

## Follow-up candidates

None yet.

## Worklist / next step

- Done: spec author (01, bc47ce6, lane check confirmed docs).
- Next: spec checker (02).
