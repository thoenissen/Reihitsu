# Run rh3002-conditional-directive — lead state

- Issue: thoenissen/Reihitsu#1756 — `agent-filed` (goals are proposals)
- Slug: `rh3002-conditional-directive`
- Branch: `claude/upbeat-tesla-8yqfoe` (the session's designated branch; see Decisions #1)
- Draft PR: #1769
- Lane: **docs** — the maintainer already decided the output (option B: RH3002 keeps reporting, no fix, RH3002-009 unchanged) and scoped the issue to aligning spec text only, so no shipped behavior changes. Widens to tests if a covering test for the issue's input is missing and has to be added.
- Roles: spec author (writes the text), spec checker. Skipped: skeptic, test implementer, implementer, cleaner, auditor (docs lane); validator only if a compiled file changes.

## Maintainer answers

1. Output question of #1756 (issue comments, 2026-11-15): **B** — RH3002 keeps reporting the parentheses around an expression with a conditional directive inside, and no fix is offered (RH3002-009 as it stands). General principle stated: analyzers judge the code as the preprocessor symbols of the current build are set.
2. Scope (issue comment, 2026-11-15, answer 3B): in this issue only the spec text is aligned, so that RH3002-009 reads as the decided behavior rather than a contradiction of SHARED-DIAG-008. No other rule changes. The general reversal of SHARED-DIAG-008 is thoenissen/Reihitsu#1759 and is out of scope.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's designated branch `claude/upbeat-tesla-8yqfoe` instead of `claude/issue-1756-<slug>` | The environment allows pushing only to that branch | Branch name is internal | claim commit |
| 2 | Lane | Lane | Docs | Maintainer answers 1 and 2 leave only spec text to change | No output changes | — |

## Follow-up candidates

(none yet)

## Worklist / next step

- Next: spec author (01-spec-author.md).
