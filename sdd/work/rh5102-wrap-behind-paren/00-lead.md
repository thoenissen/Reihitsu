# Lead — rh5102-wrap-behind-paren

- Issue: thoenissen/Reihitsu#1755 — `agent-filed` (goals are proposals). RH5102 reports an argument list wrapped only behind `(` whose arguments share one line; the formatter joins it.
- Branch: `claude/pensive-davinci-jjfo8i` (the session's designated push branch, used instead of `claude/issue-1755-…`). Draft PR: #1778.
- Lane: **behavior change** — the maintainer's answer removes a diagnostic RH5102-001 currently requires, so a visible output changes and a change doc is required.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist.

## Maintainer answers

- #1755, answered on the issue before the run (2026-11-15): **option A**. RH5102 does not report an argument list whose arguments all stand on one line, even when that line is not the line of the opening `(` — consistent with the RH5109 decision for parameter lists (#1736). Example `M(⏎    1, 2);` is not reported. The issue's "Done when": the answer is recorded in the RH5102 spec (change doc since visible output changes), and both shapes `M(⏎1, 2)` (not reported) and `M(1,⏎2)` (reported) have a covering test.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use `claude/pensive-davinci-jjfo8i` instead of `claude/issue-1755-<slug>` | The session environment only permits pushing to that branch | Branch names are internal | c59c687 |

## Follow-up candidates

(none yet)

## State

- Last step: claim, `prepare.sh` green (SDK 11.0.100).
- Next: spawn spec author.
