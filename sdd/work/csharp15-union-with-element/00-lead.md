# Lead — csharp15-union-with-element

- Issues (prompt order): thoenissen/Reihitsu#1762 (`agent-filed`), thoenissen/Reihitsu#1763 (`agent-filed`). Both goals are proposals.
- Slug: `csharp15-union-with-element`
- Branch: `claude/gifted-brahmagupta-pew7ub` (the session's designated branch; the environment permits pushing only there, so the playbook's `claude/issues-...` name is not used)
- Draft PR: #1782
- Lane: **behavior change**.
  - #1762: new formatter behavior for C# 15 union declarations (braces, members, constraints, empty body, field split, case-type list wrap) — requested changed output with open output questions → behavior change.
  - #1763: new horizontal-spacing behavior for the C# 15 `with(...)` collection-expression element — requested changed output with an open output question → behavior change.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-ups exist. None skipped.
- Maintainer clarifications: none beyond the prompt.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's designated branch `claude/gifted-brahmagupta-pew7ub` | Environment allows pushes only there | Internal | claim commit |

## Follow-up candidates

(none yet)

## Worklist / next step

- Next: spawn spec author.

## Log
- 01-spec-author done (2272e74): two change docs; Q1 (wrapped case-type list), Q2 (empty union body), Q3 (accept class-like union layout incl. RH7101 fix offered), Q4 (`with(` spacing) open for batch 1.
- Follow-up candidates: (a) attribute lists on unions not split — SHARED-ATTR-001 has no union target (01-spec-author); (b) argument list on the line after its name lands at the statement column (01-spec-author).
- Next: skeptic.
