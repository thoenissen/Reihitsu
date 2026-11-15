# Lead — comma-comment-layout

- Issues: thoenissen/Reihitsu#1737 (`agent-filed`), thoenissen/Reihitsu#1738 (`agent-filed`). Goals of both are proposals.
- Slug: `comma-comment-layout`
- Branch: `claude/adoring-shannon-yomd27` (the session's designated branch; the harness allows pushes only there, so the `claude/issues-1737-1738-...` name of `lead.md` is not used)
- PR: thoenissen/Reihitsu#1749 (draft claim)
- Lane: **behavior change**
  - #1737: the fix and the formatter disagree on where a comment behind a leading comma goes; one of them changes visible output and the spec has to state the placement → behavior change (change doc).
  - #1738: no spec requirement names a comma starting a line in an object initializer; deciding and specifying its layout is new specified behavior → behavior change.
- Roles: spec author → skeptic → (output questions) → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist. None skipped.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's designated branch | Harness restricts pushes to it | Internal | claim commit |

## Follow-up candidates

(none yet)

## Audited SHA

(none yet)

## Open worklist / next step

- Next: spawn spec author.
