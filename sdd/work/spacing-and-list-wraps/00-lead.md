# Lead — spacing-and-list-wraps

- Issues (prompt order): thoenissen/Reihitsu#1642 (`agent-filed`), thoenissen/Reihitsu#1635 (`agent-filed`). Goals of both are proposals.
- Slug: `spacing-and-list-wraps`
- Branch: `claude/keen-goldberg-e84ipm` (session-mandated branch, see decision 1)
- Draft PR: thoenissen/Reihitsu#1733
- Lane: **bug fix**
  - #1642: reported observed-versus-expected difference in the formatter's horizontal spacing (space behind a multi-line `*/` removed before some tokens).
  - #1635: reported difference against FMT-BREAK-012 in the line-break phase (argument list wrapped in front of a comma is not laid out one element per line).
- Roles run: skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. Spec author: only for a clarification (skipped unless a role reports a spec gap).

## Maintainer clarifications

- #1635 triage comment (maintainer): stays a bug against FMT-BREAK-012 for 1.2.2; related to the vNext issue about a leading comma in an argument list (thoenissen/Reihitsu#956) but not folded into it.
- #1635 second comment (agent-written): two related observations — a list wrapped only after `(` is joined to one line (`M(⏎a, b)` → `M(a, b)`), and a trailing line comment that pins a leading comma keeps two elements on one line. Both proposed to be settled together with the issue's input; proposals, not binding.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's mandated branch `claude/keen-goldberg-e84ipm` instead of `claude/issues-1642-1635-<slug>` | The environment allows pushing only to that branch | Branch name is internal | claim commit |

## Follow-up candidates

(none yet)

## Worklist / next step

- Next: skeptic (01).
