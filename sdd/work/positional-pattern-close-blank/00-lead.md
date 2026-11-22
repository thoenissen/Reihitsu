# Lead — positional-pattern-close-blank

- Issue: thoenissen/Reihitsu#1935 (`agent-filed`; labels enhancement, formatter; milestone 1.6.0)
- Slug: `positional-pattern-close-blank`
- Branch: `claude/squad-1935-ptmco4` (session-designated); draft PR #1946
- Lane: **behavior change** — the issue requests changed formatter output (a blank line in front of a positional pattern's closing `)` is removed); FMT-BLANK-019 has to change, so a change doc is required.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager only if follow-up candidates exist.

## Maintainer answers

1. Issue #1935 output question (recorded on the issue, comment of 2026-11-22): **option A** — the blank line in front of a positional pattern's closing `)` is removed, like the one in front of a list pattern's `]`. The work touches `BraceBlankLinePolicy`, FMT-BLANK-019, and the requirements that refer to it.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | spec author | Does the maintainer's answer (option A) cover change-doc item 2 (comment/directive gaps in front of the positional `)`, FMT-BLANK-009/018/020)? | Covered; not asked again | The answer names FMT-BLANK-019 "and the requirements that refer to it" and states the rule as "like a list pattern's `]`"; each item-2 After equals main's output for the list-pattern form | The visible change is covered by the maintainer's answer | fdca76f |

## Follow-up candidates

(none yet)

## Audited SHA

(none yet)

## Worklist / next step

- Done: spec author (01, fdca76f), lane check confirmed.
- Next: skeptic (02) running; then output-question batch if anything new, then accept change doc + test implementer.
