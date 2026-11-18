# Lead — token-replacement-cost

- Issue: thoenissen/Reihitsu#1862 — `agent-filed` (goals are proposals). Milestone 1.5.0.
- Slug: `token-replacement-cost`
- Branch: `claude/compassionate-brown-quwcgr` (session-designated); draft PR thoenissen/Reihitsu#1872.
- Lane: **bug fix**. The issue reports an observed cost (whole-file token replacement in the indentation and cleanup phases grows with the square of the token count) against the expectation that formatting time grows about linearly, with output unchanged. The change touches production formatter code only, no visible output and no change doc.
- Roles run: skeptic (premise, reachability) → test implementer (reproduction) → implementer → cleaner → spec checker → auditor → validator. Spec author skipped: bug-fix lane, only for a clarification.
- Maintainer clarifications: the triage comment on the issue places it in 1.5.0 as the hot spot of the fixed-corpus benchmark; another issue's measurement is re-taken after this run merges. Nothing else.

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

None yet.

## Audited SHA

—

## Open worklist / next step

- Next: skeptic (premise check).
