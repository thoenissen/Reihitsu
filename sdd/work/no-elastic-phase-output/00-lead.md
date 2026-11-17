# Lead — no-elastic-phase-output

- Issue: thoenissen/Reihitsu#1516 — "FMT-PIPE: no phase output carries elastic trivia" — labels `enhancement`, `formatter`, `maintenance`, `agent-filed` (goals are proposals).
- Slug: `no-elastic-phase-output`
- Branch: `claude/trusting-johnson-d4cgsd` (session-designated)
- Draft PR: thoenissen/Reihitsu#1822
- Lane: **behavior change** — the issue requests a new pipeline requirement (FMT-PIPE) and phase changes to meet it, i.e. new specified behavior, not an observed-versus-expected difference against an existing requirement.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator (all run; none skipped).
- Maintainer clarifications: none.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Spec author | Requirement wording: "no phase adds elastic trivia" vs the issue's "no phase output carries" | Take "adds" (FMT-PIPE-034), with a non-goal for elastic trivia a node-level caller passes in | Identical for document/tree entry points; "carries" would force phases to strip trivia from code-fix input and change code-fix spacing no one asked about | No formatter output changes (spec author ran format on the example; suite outputs unchanged); code-fix outputs untouched | 05b79d7 |
| 2 | Spec author | Where the rule lives | Stated once in pipeline spec; FMT-STRUCT-026 points to it | One statement of a rule | Spec text only | 05b79d7 |

## Follow-up candidates

(none yet)

## Audited SHA

(none yet)

## Open worklist / next step

- Done: spec author (01), commit 05b79d7; lane check confirmed behavior-change. Change doc Draft, no visible output changes, no output questions.
- Next: skeptic (02).
