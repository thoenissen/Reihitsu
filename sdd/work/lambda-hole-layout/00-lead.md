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
| 2 | Spec author | Change docs: one per issue; FMT-INDENT-031 changed in place; FMT-BREAK-045 new; #1698 rule keyed on kind of interpolated string | Accept the spec author's choices | Issues share area, not cause; in-place change keeps test IDs valid | Internal spec structure | b1406f8 |

## Follow-up candidates

- If #1568 answer A: RH5101/RH5102 fix column differs from whole-file formatting (01-spec-author).
- If #1568 answer B: anonymous-method counterpart of RH5205 as a new rule under P11 (01-spec-author).
- If #1567 answer A: RH5105 does not report a lambda's `(` behind a return type — gap or excluded under P11 (01-spec-author).

## Worklist / next step

- Done: claim, lane, prepare.
- Done: spec author (01, b1406f8). #1567 premise narrowed: only explicit return type reproduces.
- Next: skeptic, then output-question batch.
