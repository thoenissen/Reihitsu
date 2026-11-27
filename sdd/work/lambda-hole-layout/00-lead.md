# Lead — lambda-hole-layout

- Issues (prompt order): thoenissen/Reihitsu#1568, thoenissen/Reihitsu#1698, thoenissen/Reihitsu#1567 — all three carry `agent-filed` (goals are proposals).
- Branch: `claude/beautiful-lovelace-w4kwiy` (session-designated). Draft PR: #1985.
- Slug: `lambda-hole-layout`.
- Lane: `behavior-change`, for every issue.
  - #1568: requested change of the formatter's indentation of an anonymous method's block outside raw-string holes (output question in the issue) — new behavior, change doc.
  - #1698: requested change of the column of a comment in front of a regular interpolated string hole's closing `}` (and the blank line in front of it) — new behavior, change doc.
  - #1567: requested join of a parenthesized lambda's `(` onto its modifier / return type on the previous line — new behavior, change doc.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist. None skipped.
- Maintainer clarifications: none beyond the prompt.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Start | Do the three issues share a mechanism, or should the batch be split (lead.md 1.4)? | Keep one run | All three are formatter layout of the same code area: #1568 and #1698 are line columns the indentation phase owns, #1567 and #1568 are the layout of a lambda / anonymous function; three issues, one lane, no code fix or rule named; the maintainer named them together | Process only | claim |

## Follow-up candidates

(none yet)

## Worklist / next step

- Done: claim, lane, prepare.
- Next: spawn spec author.
