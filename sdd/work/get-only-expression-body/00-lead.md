# Lead — get-only-expression-body

- Issues: thoenissen/Reihitsu#849 (`agent-filed`, goals are proposals), thoenissen/Reihitsu#850 (`agent-filed`, goals are proposals)
- Slug: `get-only-expression-body`
- Branch: `claude/awesome-dijkstra-fyihwi` (designated by the session)
- PR: thoenissen/Reihitsu#1868 (draft claim)
- Lane: behavior change / new rule
  - #849: requests a new Clarity analyzer rule (and code fix) reporting get-only properties that the formatter collapses to an expression body (formatter behavior from #848, merged in thoenissen/Reihitsu#854).
  - #850: the same for indexers, as a separate rule. Same mechanism and component as #849, so one run.
- Roles run: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist. None skipped.
- Maintainer clarifications: none beyond the prompt.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Start | Batch #849 and #850 into one run? | Yes | They share one mechanism (the formatter's get-only collapse) and one lane; two rules | Internal | b3e1523 |

## Follow-up candidates

(none yet)

## Worklist / next step

- Next: spawn spec author (01).
