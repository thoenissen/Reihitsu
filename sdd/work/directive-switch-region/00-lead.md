# Lead — directive-switch-region

- Issues (prompt order): thoenissen/Reihitsu#1385 (agent-filed), thoenissen/Reihitsu#1398 (agent-filed), thoenissen/Reihitsu#1239 (agent-filed). All goals are proposals.
- Slug: `directive-switch-region`
- Branch: `claude/nifty-hypatia-urch1j` (session-mandated branch, see Decisions #1)
- Draft PR: thoenissen/Reihitsu#1657
- Lane: **bug fix**
  - #1385: formatter part only (triage comment split the RH5002/RH5008 code-fix part into #1447). Bug against FMT-BLANK-016, which already requires the blank line after directives; triage says bug-fix lane, no change doc, coverage sentence widened as a clarification.
  - #1398: bug against FMT-SWITCH-006 (break-only section never makes the switch multi-line); triage: bug-fix lane, FMT-SWITCH-004 gains the precedence of -006 as a clarification.
  - #1239: bug against FMT-REGION-007 (a `#region` line is `#region`, one space, description) if that requirement covers whitespace between `#` and `region`. The issue asks the maintainer to confirm that reading; it is a two-readings output question for the batch before the test implementer. A "general directive policy" answer would widen the run to behavior change.
- Roles: spec author (clarifications only) → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. Issue manager before publishing when follow-ups exist.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's designated branch `claude/nifty-hypatia-urch1j` instead of `claude/issues-1385-1398-1239-<slug>` | The session environment forbids pushing to any other branch | Branch names are internal | 32e14d4 |

## Follow-up candidates

(none yet)

## Worklist / next step

- Audited SHA: —
- Next: spawn spec author.
