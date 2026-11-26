# Run: rh5202-tab-aligned-raw-strings

- Issue: thoenissen/Reihitsu#1925 — RH5202: a multi-line raw string literal aligned in characters behind tabs is reported and fixed. Label `agent-filed` (goals are proposals; the maintainer's answer below is binding).
- Branch: `claude/hopeful-bell-fsyqxb` (session-designated). Draft PR: thoenissen/Reihitsu#1953.
- Lane: behavior-change — the issue requests changed analyzer behavior (a new reported case and fix result for RH5202) and the maintainer answered the output question.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if there are follow-up candidates. None skipped.

## Maintainer answers

- Issue comment 2026-11-22 (before the run): option A. RH5202 reports a multi-line raw string literal whose content and closing lines are aligned in characters behind tabs, and its fix writes the layout the formatter writes (FMT-RAW-010): content and closing lines indented with spaces, one per character of the stripped indentation. The maintainer left it to the run to decide whether the diagnostic message needs a wording adjustment for the new case.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

(none yet)

## Worklist / next step

- Next: spawn spec author.
