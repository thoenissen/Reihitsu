# Lead — constraint-clause-wrap

- Issues: thoenissen/Reihitsu#1631 — `agent-filed` (goals are proposals; labels: bug, formatter, agent-filed)
- Slug: `constraint-clause-wrap`
- Branch: `claude/dreamy-curie-x5c135` (session-designated branch; see decision 1)
- Draft PR: thoenissen/Reihitsu#1632
- Lane: **behavior change**. The observed column of wrapped constraints is a defect, but the expected layout (all-or-none wrapping of a clause's constraints, alignment under the first constraint, the rule when the first constraint itself is wrapped) is not stated by any spec today (FMT-BREAK-023 and FMT-INDENT-017 cover only the clause's own line). Stating it is new specified behavior that changes formatter output, so a change doc is expected.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. None skipped.
- Maintainer clarifications: none so far.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name `claude/issue-1631-<slug>` per `lead.md` vs. the session's designated branch | Use `claude/dreamy-curie-x5c135` | The session environment permits pushing only to its designated branch | Branch names are internal | claim commit |

## Follow-up candidates

(none yet)

## Open worklist / next step

- Next: spawn spec author (01).
