# Lead — per-phase-idempotency

- Issue: thoenissen/Reihitsu#1500 — "FMT-PIPE-004: no single phase changes the formatter's output". Label `agent-filed`: its goals are proposals.
- Parent: thoenissen/Reihitsu#1482 (package D3). Blockers thoenissen/Reihitsu#1483, #1499, #1497, #1494 are closed (merged); #1487 and #1496 are reached through #1499.
- Slug: `per-phase-idempotency`
- Branch: `claude/kind-bohr-ib25uy` (session-designated branch, see Decisions taken #1)
- PR: thoenissen/Reihitsu#1575 (draft claim)
- Base: `origin/main` at `fe24dcc`

## Lane

Behavior change. The issue requests changed behavior of a formatter requirement (FMT-PIPE-004 extended to a per-phase property) and asks for a change doc under `sdd/changes/`.

Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. Issue manager before publishing when follow-up candidates exist. None skipped.

## Notes for the roles

- The issue was written against `main` at `735d0dba`; every statement about today's code (the exception list in the formatter test helpers, which fixtures it holds) is to be checked against current `main`. A statement an earlier package already made true or moot is noted in the handover and dropped.
- The issue's comment (2026-10-28) states that thoenissen/Reihitsu#1497 keeps the blank line above a comment that leads a same-group using, so the example's expected output keeps it.
- The change doc has to decide whether the per-phase property also covers the node-level entry points, and which tests cover the extension.

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | `claude/kind-bohr-ib25uy` instead of `claude/issue-1500-per-phase-idempotency` | The session's instructions fix the branch to push to | Branch names are internal | 7737a43 |

## Follow-up candidates

None yet.
