# Run: rh4120-dictated-record-param

- Issue: thoenissen/Reihitsu#1939 — `agent-filed` (goals are proposals; the visible output was decided by the maintainer, see below)
- Branch: `claude/squad-1939-81sm3x`; draft PR #1947
- Lane: behavior-change — the issue asks for changed analyzer behavior (RH4120 stops reporting a positional record parameter whose generated property implements a property declared in a referenced assembly), which needs a spec change and production code.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist. None skipped.

## Maintainer answers

1. #1939 output question (answered on the issue, comment 2026-11-22): **option A** — RH4120 does not report a positional record parameter whose generated property implements a property of another (referenced) assembly; that name is dictated, as for properties under SHARED-CASING-014. Example: `public record Person(string name) : INamed;` with `INamed.name` declared in a referenced assembly is not reported.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

(none yet)

## Audited SHA

(none yet)

## Open worklist / next step

- Next: spec author (01).
