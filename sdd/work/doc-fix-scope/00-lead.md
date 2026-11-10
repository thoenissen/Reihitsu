# Lead — doc-fix-scope

- Issues (prompt order): thoenissen/Reihitsu#1269, #1342, #1343, #1459 — all four carry `agent-filed` (goals are proposals).
- Slug: `doc-fix-scope`
- Branch: `claude/squad-tickets-1269-1342-gxh523` (the session's designated branch; overrides the playbook's `claude/batch-1269-...` name)
- Draft PR: #1679
- Base at claim: `8ac5705` (origin/main)

## Lane

**Bug fix**, for every issue. Each reports an observed-versus-expected difference in documentation code fixes against existing requirements:

- #1269 — RH8201/RH8202/RH8204 fixes change more than the reported markup (layout, line endings, re-serialized content) or need two applications (SHARED-FIX-001/006/008).
- #1342 — RH8305/RH8307/RH8309/RH8402 fixes reformat beyond the reported comment, move a behind-code `///`, drop a blank line (SHARED-FIX-006/007, SHARED-DOC-004).
- #1343 — RH8308/RH8309/RH8401 fixes need two applications (SHARED-FIX-001/005); RH8401 rule page text to be aligned with behavior (docs path, admitted by the bug-fix lane).
- #1459 — RH8305 fix makes no progress on the first type of a file without a namespace (RH8305-004). Same instance also commented on #1343.

Roles: skeptic → spec author (only for clarifications and the RH8401 rule page) → test implementer → implementer → cleaner → spec checker → auditor → validator. Issue manager before publishing if follow-up candidates exist.

## Maintainer clarifications

None in the prompt.

## Maintainer answers

- Q1 (#1342; RH8305/RH8307/RH8309, documentation comment written behind code on the same line as code): **A** — withhold the fix for that shape; the diagnostic stays. The #1342 comment's expected in-place layout is dropped.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's designated branch `claude/squad-tickets-1269-1342-gxh523` | Harness requires it | Internal | a200b98 |
| 2 | Lane | Spec author before skeptic? | Skeptic first; spec author only for clarifications and the RH8401 rule page | Bug-fix lane runs the spec author only for a clarification | Internal | — |
| 3 | Skeptic | #1269 RH8202: report each `<value>` or fix all in one application | Keep one diagnostic; the fix removes every `<value>` the rule would report | SHARED-FIX-001 already demands it | Diagnostics unchanged; fix result demanded by an unchanged requirement | — |
| 4 | Skeptic | #1269 RH8201 member written behind `*/` | Member goes on the line after `/// <inheritdoc/>` at the column the `/**` had (RH8201-005 reading) | Same as today for standard indentation | RH8201-005 decides it | — |
| 5 | Skeptic | #1269 goal "`/** */` and member joined" | Dropped | A `///` line cannot share its line with code | No change against today | — |
| 6 | Skeptic | #1343 RH8309 diagnostic in a later of several doc comments | Withhold the fix when the declaration's leading trivia holds more than one documentation comment | Rare class, cheap check; issue's Done-when admits withholding | Withholding for a rare class puts nothing in question | — |
| 7 | Skeptic | #1343 RH8401 architecture finding (analyzer vs `DocumentationCommentUtilities` on U+FEFF/U+001A before `///`) | Refuse the class first: withhold the RH8401 fix for a comment where a line has U+FEFF or U+001A before `///`; no change to the Core helper | README *Architecture findings*: rare class → refuse first; avoids touching a shared helper with other callers | Rare-class withholding; issue's Done-when admits it | — |
| 8 | Skeptic | #1343 `RH8401.md` | Remove the *Code fix behavior* corner-case text (spec author) | AGENTS.md: rule pages list no exceptions or corner cases; withheld fixes are covered by general-notes.md | Demanded by the repository's rule-page convention and the issue's goal | — |
| 9 | Skeptic | #1459 scope | Widen to every declaration directly in the compilation unit, RH8305 and RH8307; formatter's FMT-PIPE-010 stays | Same class, same cause as #1342 | Output demanded by RH8305-004/RH8307-004 and SHARED-FIX-006 | — |
| 10 | Skeptic | #1343 RH8308 span | Span runs to the end of the line through references (RH8308-003) | Bug against unchanged requirement | RH8308-003 already demands it | — |
| 11 | Skeptic | Follow-up: RH8202 `<value>` in a second doc comment | Dropped | Two documentation comments on one declaration is a rare class | — | — |

## Follow-up candidates

- RH8309 fix makes no progress on an accessor's documentation comment (`get => 0;` with `<remarks>` before `<summary>`): SHARED-FIX-005 — 01-skeptic.md, #1342 section.

## Worklist / next step

- Done: skeptic (01-skeptic.md, commit 67d5ea7).
- Q1 answered: A.
- Next: spec author (coverage widening, RH8402 replace path, RH8401.md, withheld cases RH8309/RH8401, the answer), then test implementer.
