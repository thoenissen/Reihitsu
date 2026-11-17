# Lead — issue 1805

- Issue: thoenissen/Reihitsu#1805 — FMT-PIPE-027 per-phase stability check in the analyzer and CLI full-run idempotency tests. Label `agent-filed`: its goals are proposals.
- Slug: `phase-check-shared`
- Branch: `claude/intelligent-hopper-li5bmz` (session-designated); draft PR thoenissen/Reihitsu#1824.
- Lane: **tests** — the issue adds a check to tests of existing behavior (FMT-PIPE-027); no production behavior changes. The issue's own wiring needs a `.csproj` change (a reference or a shared project), which the lane check classifies as tooling; expect a widening to `tooling` once that path is in the diff.
- Roles (tests lane): skeptic (premise, reachability, and the reference-vs-shared-project structure), test implementer, cleaner, spec checker, validator. Spec author skipped: the run adds no spec unless the skeptic finds a spec gap. Implementer and auditor skipped in the tests lane; a widening to tooling adds them.
- Maintainer clarifications: none.

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

None yet.

## Worklist / next step

- Claimed (commit "Claim issue #1805"), prepare.sh green.
- Next: skeptic.
