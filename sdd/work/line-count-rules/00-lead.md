# Lead — line-count-rules

- Issues: thoenissen/Reihitsu#353 (agent-filed) — report methods whose line count exceeds a limit; thoenissen/Reihitsu#354 (agent-filed) — report types whose line count exceeds a limit. Both carry `agent-filed`: goals, target behavior, and prescribed solutions are proposals.
- Slug: `line-count-rules`
- Branch: `claude/squad-353-354-pq7nz1` (designated by the session)
- PR: thoenissen/Reihitsu#1860 (draft claim)
- Lane: behavior change / new rule — both issues request a new analyzer rule (new diagnostic, new configuration option, new rule page); no existing behavior is reported as wrong. They share a mechanism (counting the lines of a declaration's body against a configured limit) and both sit in the Design (`RH2###`) range, so one run.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing when follow-up candidates exist. None skipped.
- Maintainer clarifications: none beyond the prompt (`/squad #353 #354`).

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

(none yet)

## Audited SHA

(none yet)

## Open worklist / next step

- Next: spawn the spec author (handover `01-spec-author.md`).
