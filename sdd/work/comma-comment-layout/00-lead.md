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
| 2 | Spec author | One change doc per issue or one shared | One per issue | Causes differ (fix vs. formatter layout) | Doc structure is internal | 81bfa87 |
| 3 | Spec author | #1738 scope: object initializers only or the whole FMT-BREAK-021/022 family | Whole family, pending skeptic | Same mechanism; one class, not instances | The family's outputs are asked as output questions | 81bfa87 |
| 4 | Spec author | New requirement IDs vs. extending existing ones | New requirements (FMT-BREAK-038, FMT-INDENT-033) | Keeps existing requirements stable | Wording/IDs internal | pending |

## Follow-up candidates

(none yet)

## Audited SHA

(none yet)

## Open worklist / next step

- Spec author done (01, open questions Q1–Q3, two Draft change docs). Next: skeptic (02), then ask Q1–Q3 (+ any skeptic questions) before the test implementer.
