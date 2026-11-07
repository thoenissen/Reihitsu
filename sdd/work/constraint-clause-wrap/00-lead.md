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

From `01-spec-author.md` (each reproduced via the CLI):
1. `where T⏎ : class` — the `: class` line lands one level left of `where`.
2. Base list wrapped behind its colon (`class B :⏎ IDisposable`) — base types land at column 0.
3. Angle-bracket list whose first element a comment pushes onto the next line — `T>` lands at column 4; FMT-INDENT-015 does not cover it.
4. `Foo(1⏎ , 2, 3)` is not split to one argument per line — looks like a bug against FMT-BREAK-012.

## Open worklist / next step

- 01 spec author done (44509cd): change doc `sdd/changes/2026-11/constraint-clause-wrap.md` (Draft), FMT-BREAK-037, FMT-INDENT-032; 5 output questions in its handover.
- Next: skeptic (02), then the output-question batch to the maintainer before the test implementer.
