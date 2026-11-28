# Lead — using-header-directive-reorder

- Issue: thoenissen/Reihitsu#1971 — `agent-filed` (goals are proposals).
- Slug: `using-header-directive-reorder`
- Branch: `claude/intelligent-babbage-pxnhds` (session-designated); PR thoenissen/Reihitsu#1996 (draft claim).
- Lane: `behavior-change` — the issue requests changed code-fix and formatter behavior (RH7201–RH7207 fixes, Fix All, and the formatter's using phase reorder a block whose every directive stands before its first using); a change doc is required.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist. None skipped.
- Prerequisite: the maintainer ordered this run after #1972 (merged as #1991, `13b0ed2`); satisfied at claim.

## Maintainer answers

1. Output question in the issue (answered in the issue comment before the run): option A. When every preprocessor directive of a using block stands in front of its first using (e.g. a `#nullable enable` header), the RH7201–RH7207 fixes, Fix All, and the formatter reorder the block and the directive stays on top. A block with a directive between or after its usings stays reported without a fix. The narrowed guard must stay a cheap syntactic check. SHARED-USING-009 lives in `sdd/specs/shared/using-directive-ordering.md`, not `code-fixes.md`.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

(none yet)

## Worklist / next step

- Next: spawn spec author (change doc + spec diff).
