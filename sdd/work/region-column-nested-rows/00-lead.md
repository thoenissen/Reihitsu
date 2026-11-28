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

- F1 (01-spec-author): blank line inserted after an opening `[`/`(` before a `#region` and before a closing `]`/`)` after an `#endregion`, but not for `{`/`}` — blank-line phase.
- F2 (01-spec-author): a region inside a wrapped method chain, binary expression, or query takes the statement column; spec leaves it unspecified.

## State

- Last step finished: spec author (01, c9404e4; change doc Draft; open output question 1 + several change-doc entries needing answers).
- Next: skeptic (02), then output-question batch.
