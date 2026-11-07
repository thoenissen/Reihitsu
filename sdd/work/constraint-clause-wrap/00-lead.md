# Lead — constraint-clause-wrap

- Issues: thoenissen/Reihitsu#1631 — `agent-filed` (goals are proposals; labels: bug, formatter, agent-filed)
- Slug: `constraint-clause-wrap`
- Branch: `claude/dreamy-curie-x5c135` (session-designated branch; see decision 1)
- Draft PR: thoenissen/Reihitsu#1632
- Lane: **behavior change**. The observed column of wrapped constraints is a defect, but the expected layout (all-or-none wrapping of a clause's constraints, alignment under the first constraint, the rule when the first constraint itself is wrapped) is not stated by any spec today (FMT-BREAK-023 and FMT-INDENT-017 cover only the clause's own line). Stating it is new specified behavior that changes formatter output, so a change doc is expected.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. None skipped.
- Maintainer clarifications: none so far.

## Maintainer answers

Batch 1 (before test implementer), all recommendations chosen:
- Q1 → A: a wrapped clause puts one constraint per line, aligned under the first constraint (column behind `where T : `).
- Q2 → A: a first constraint wrapped below the colon is joined onto the colon's line; the clause is then laid out by Q1.
- Q3 → A: where a comment or directive blocks that join, the constraints take the column the first constraint would have behind the colon.
- Q4 → A: a leading comma moves to the end of the previous line; where a comment blocks that, the comma stays first on its line and is aligned to the constraint column.
- Q5 → A: a constraint a comment keeps over two lines wraps the whole clause (one per line); its inner line aligns inside the constraint.
- Q6 → A (skeptic S1): wraps inside a constraint that no comment/directive holds are joined first; the clause is judged after the join.
- Q7 → A (skeptic S2): `where T⏎ : …` — the colon joins the type parameter's line; where a comment blocks the join, the colon keeps its own line one level deeper than `where` and the constraints align under the first.
- Q9 → A: when a comment/directive blocks a FMT-BREAK-037 join, RH5110's fix is still offered and yields the formatter's fallback layout (Q3A/Q4A/Q7A); Q8 covers only inputs where a comment, directive, or disabled text would actually have to move.
- Q10 → A: the constraint colon always gets one space on each side, like a base-list colon (FMT-SPACE-009 widens); also one-line `where T:class` → `where T : class`.
- Q11 → A: a colon a comment/directive keeps on its own line wraps the clause — constraints one per line, aligned under the first (`: class,` / `  new()`).
- Q8 → A: RH5110's fix is withheld for inputs where the layout could only be reached by moving a comment, directive, or literal.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 2 | Skeptic | Does the `where T⏎ : …` gap (01 follow-up 1, skeptic S2) join the run? | Yes, as output question S2 (options A/B only) | The clause's constraints would still sit left of `where` after the change — the issue's own symptom; same component | Its output is asked as S2 | — |
| 3 | Skeptic | Skeptic's spec amendments 1–5 (directives/disabled text named, "left as it is" vs. FMT-BREAK-014 join, judged before/after joins per S1, change-doc multi-line claim, extension blocks covered) | Accept all; route to spec author with the maintainer's answers | Wording precision; no output beyond the asked questions | Each is decided by an asked question or changes no output | — |
| 4 | Skeptic | Extension blocks: `where` not on its own line (FMT-BREAK-023 / RH5110 skip them) | Follow-up, not this run; FMT-BREAK-037/FMT-INDENT-032 still apply to extension-block clauses (amendment 5) | Placement of `where` itself is FMT-BREAK-023, outside #1631 | Not changed by this run | — |
| 5 | Spec | RH5110-004 dropped after Q9; ledger line removed rather than retired | Accept | The ID was only ever issued on this branch; SHARED-FIX-003 alone governs | Q9 answered the visible output | 6ebc723 |
| 6 | Spec | Accept change doc `sdd/changes/2026-11/constraint-clause-wrap.md` | Accepted | Skeptic ran; amendments and Q1–Q9 carried in | Its visible output changes were all asked | 6ebc723 |
| 7 | Implementer | D2: `LocalFunctionInLambdaClauseAligns` expects lambda block at column 8; existing lambda layout (unchanged on main) gives 26 | Test expectation is wrong; test implementer fixes it to the existing layout | The lambda block's column is decided by unchanged requirements, not this run | Unchanged requirement decides it | — |
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

From `07-spec-author.md`:
8. New-rule candidate (P11): spacing of the constraint colon (`where T:class`), which the formatter rewrites after Q10 but no analyzer reports (RH6021 names only base-list and constructor-initializer colons).

## Open worklist / next step

- 01 spec author done (44509cd): change doc `sdd/changes/2026-11/constraint-clause-wrap.md` (Draft), FMT-BREAK-037, FMT-INDENT-032; 5 output questions in its handover.
- 02 skeptic done (38c6f09): premise confirmed; verdict output question; S1, S2 added.
- Batch 1 answered (all A).
- 03 spec author done (c716d42): change doc Accepted; FMT-BREAK-037, FMT-INDENT-032 rewritten; RH5110-004 added (fix withheld when a comment/directive stands in a gap FMT-BREAK-037 joins).
- RH5110-004 reading of Q8 is ambiguous (Q3A/Q4A/Q7A give safe fallback layouts for exactly those inputs, so the fix could be offered with them). Q9 answered A.
- 04 spec author done (6ebc723): RH5110-004 dropped; RH5110-003 changed (fix offered, fallback layout).
- 05 test implementer done (9a1da1d, 7c0d665): 46 formatter + 6 RH5110 tests; 43+6 fail on unchanged code as expected, 4 guard tests pass.
- 06 implementer (da0e710): verdict test disputed; 43 new tests pass; D1 (no space after joined colon, 7 tests), D2 (decision 7), D3 (colon-only wrap) open.
- Q10 A, Q11 A answered.
- 07 spec author done (922ed04): FMT-SPACE-009 covers constraint colon; FMT-BREAK-037 colon-held wrap; RH6021 non-goal reworded; change doc entries 14, 15. Rejected options recorded match those shown.
- Next: test implementer (08) for D2 + Q10/Q11 tests → implementer (09).
