# Lead — interp-hole-csharp10

- Issue: thoenissen/Reihitsu#1948 — `agent-filed` (goals are proposals). Labels: bug, analyzer. Milestone 1.6.0.
- Slug: `interp-hole-csharp10`
- Branch: `claude/squad-1948-336ja3` (session-designated)
- Draft PR: #1976
- Lane: **bug-fix** — the issue reports observed diagnostics (RH5301, RH5303, RH5206, RH5103, RH5008, RH5513, RH5104 inside a hole of a non-verbatim interpolated string under C# 10) against an existing shared requirement (SHARED-DIAG-011). No new behavior beyond the spec's existing principle.
- Roles: skeptic (premise + reachability) → spec author (clarification only: the rules' specs state the position, if the skeptic confirms) → test implementer (reproduction) → implementer → cleaner → spec checker → auditor → validator. Issue manager before publish if follow-up candidates exist.
- Maintainer clarifications: none.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Lane | Order of spec author and skeptic | Skeptic first, then spec author for the clarification | In the bug-fix lane the spec author runs only for a clarification; whether one is needed (and for which rules) follows from the premise check | Ordering only | — |

## Follow-up candidates

(none yet)

## State

- Last step: claim, prepare done.
- Next: skeptic.
