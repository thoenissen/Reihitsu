# Lead — rh4118-analyzer-rename

- Issue: thoenissen/Reihitsu#1090 — `agent-filed` (goals are proposals). Rename `RH4118TupleElementCasingAnalyzer` to a name that matches what it checks (tuple argument names), plus its code-fix provider and test class where they carry the same mismatch; update the code owners in `sdd/specs/analyzer/RH4118.md` and `sdd/specs/README.md`. No behavior change.
- Branch: `claude/epic-tesla-69cpc7` (session-designated). PR: #1975 (draft).
- Slug: `rh4118-analyzer-rename`.
- Lane: `bug-fix` — the change renames production files under `src/` and needs no change doc (no behavior change); the lane check classifies production without a change doc as `bug-fix`.
- Roles run: spec author (spec code-owner names only), skeptic (premise: does the mismatch exist, which types carry it, any caller-visible effect), implementer (renames production types and the test class; no assertion changes), cleaner, spec checker, auditor, validator.
- Roles skipped: test implementer — a rename has no behavior to reproduce; the existing RH4118 tests characterize the unchanged behavior and move with the rename.
- Maintainer clarifications: none beyond the issue.

## Maintainer answers

None.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Lane | Which roles run for a pure rename | Skip the test implementer; implementer renames the test class together with the production type | Nothing to reproduce; the type rename and the test reference must change atomically | Type names are internal; diagnostic ID, texts, and help link are unchanged | claim |

## Follow-up candidates

None.

## Worklist

- Next: spec author (01).
