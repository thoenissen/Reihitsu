# Lead — conditional-element-access-list

- Issues: thoenissen/Reihitsu#1877 (`agent-filed`), thoenissen/Reihitsu#1878 (`agent-filed`). Both goals are proposals.
- Slug: `conditional-element-access-list`
- Branch: `claude/pensive-carson-bh605t` (session-designated). Draft PR: #1897.
- Lane: **behavior change** for the run.
  - #1877: requested changed formatter output (join the wrapped argument list of a conditional element access `c?[1,⏎2]`), with a spec clarification of FMT-BREAK-014 / FMT-BREAK-040; carries an output question.
  - #1878: requested changed analyzer behavior (RH5307 reports and fixes `c?[...]` like `c[...]`), removes a stated exclusion from spec RH5307-001 and the rule page.
  - Shared mechanism: the bracketed argument list of an element binding expression (`?[`); the RH5307 fix formats through the formatter, so #1878's fix depends on #1877's join (maintainer's triage note on #1877).
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist. None skipped.
- Maintainer clarifications (summarized): during triage the maintainer placed both issues in one run, because the RH5307 fix formats the list through the formatter and needs the join; earlier direction (on #1869) — `?` and `!` in front of index brackets must not behave differently from a plain element access. The output question stated in #1877 is to be asked in the run.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

(none yet)

## State

- Last step: spec author 01 (commit 196829de, change doc Draft; both premises reproduce; output questions 1 and 2 drafted, spec written on 1A/2A). Lane check confirmed.
- Next: skeptic (02), then output-question batch to the maintainer before the test implementer.
