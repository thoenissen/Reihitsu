# Lead — region-column-nested-rows

- Issue: thoenissen/Reihitsu#1399 (`agent-filed`: goals are proposals)
- Slug: region-column-nested-rows
- Branch: claude/epic-rubin-2swis1 (session-designated); draft PR thoenissen/Reihitsu#1995
- Lane: behavior change — the issue requests changed formatter output (region directive column inside initializers, collection expressions, switch expressions, argument lists, patterns; nested array-initializer rows laid out like complex element initializers) that FMT-INDENT-010 / FMT-INDENT-023 do not state today, so a change doc is required.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist. None skipped.
- Issue context (summarized): the issue says the two decisions were taken in thoenissen/Reihitsu#912 (round 9 items 20, 21, both option b). Two later comments add instances: list patterns and parenthesized patterns wrapped over several lines (item 20 shape), and a region inside a switch *section* where `#endregion` stands four columns left of `#region` (not checked whether FMT-INDENT-010 decides it).

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

(none yet)

## State

- Last step finished: claim, lane.
- Next: spec author (01).
