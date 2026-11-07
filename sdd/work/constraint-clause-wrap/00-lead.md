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
| 2 | Skeptic | Does the `where T⏎ : …` gap (01 follow-up 1, skeptic S2) join the run? | Yes, as output question S2 (options A/B only) | The clause's constraints would still sit left of `where` after the change — the issue's own symptom; same component | Its output is asked as S2 | — |
| 3 | Skeptic | Skeptic's spec amendments 1–5 (directives/disabled text named, "left as it is" vs. FMT-BREAK-014 join, judged before/after joins per S1, change-doc multi-line claim, extension blocks covered) | Accept all; route to spec author with the maintainer's answers | Wording precision; no output beyond the asked questions | Each is decided by an asked question or changes no output | — |
| 4 | Skeptic | Extension blocks: `where` not on its own line (FMT-BREAK-023 / RH5110 skip them) | Follow-up, not this run; FMT-BREAK-037/FMT-INDENT-032 still apply to extension-block clauses (amendment 5) | Placement of `where` itself is FMT-BREAK-023, outside #1631 | Not changed by this run | — |
| 1 | Claim | Branch name `claude/issue-1631-<slug>` per `lead.md` vs. the session's designated branch | Use `claude/dreamy-curie-x5c135` | The session environment permits pushing only to its designated branch | Branch names are internal | claim commit |

## Follow-up candidates

From `01-spec-author.md` (each reproduced via the CLI):
1. `where T⏎ : class` — the `: class` line lands one level left of `where`.
2. Base list wrapped behind its colon (`class B :⏎ IDisposable`) — base types land at column 0.
3. Angle-bracket list whose first element a comment pushes onto the next line — `T>` lands at column 4; FMT-INDENT-015 does not cover it.
4. `Foo(1⏎ , 2, 3)` is not split to one argument per line — looks like a bug against FMT-BREAK-012.

From `02-skeptic.md`:
5. Extension blocks are skipped by FMT-BREAK-023 and RH5110 (`where` stays on the declaration line); carries an output question (A: own line one level deeper than `extension`, recommended).
6. New-rule candidate (P11): layout of constraints inside a wrapped clause (counterpart of RH5101/RH5102/RH5107).
7. A line inside a constraint that a comment keeps wrapped lands at the block column (`where T : System. // c⏎ IDisposable`); merge with 3 if S1 is answered A.
(Candidate 1 joined the run as S2, decision 2.)

## Open worklist / next step

- 01 spec author done (44509cd): change doc `sdd/changes/2026-11/constraint-clause-wrap.md` (Draft), FMT-BREAK-037, FMT-INDENT-032; 5 output questions in its handover.
- 02 skeptic done (38c6f09): premise confirmed; verdict output question; S1, S2 added.
- **Waiting on maintainer**: output-question batch 1 posted (Q1–Q5 from 01, Q6 = S1, Q7 = S2, Q8 = RH5110 fix withholding). Next after answers: spec author amends spec/change doc (answers + decision 3), then test implementer.
