# Run: rh5307-rh5401-fix-guards

- Issue: thoenissen/Reihitsu#1277 — "Fix guards: RH5307 resolves the reported list, RH5401 judges the property's own lines". Labels: enhancement, analyzer, **agent-filed** (goals are proposals).
- Branch: `claude/magical-ramanujan-irv0z2` (session-designated). Draft PR: thoenissen/Reihitsu#1869.
- Lane: **behavior change** (widened from bug fix, decision 1).
- Original lane claim: bug fix. Reason: the issue reports code fixes that are withheld for inputs the requirements RH5307-001/-004 and RH5401-002/-003/-004 name (observed vs. expected), plus a spec clarification for `?[ ]`. Widening to behavior change if the spec author finds a change doc is required.
- Roles: spec author (clarification only) → skeptic (premise, reachability) → test implementer (reproductions) → implementer → cleaner → spec checker → auditor → validator. Issue manager before publishing if follow-up candidates exist.
- Maintainer clarifications: none (no issue comments).

## Maintainer answers

Batch 1 (questions in 03-spec-author.md, Q1–Q4). Maintainer's reply, verbatim: "1a 2b 3 der fix sollte keine leerzeile einfügen, das gehört nicht zum scope 4a"
- Q1 (RH5401, attribute on the property's line): A — fix offered, `[Obsolete]⏎int E => 1;`.
- Q2 (RH5401 rule-page sentence): B — "The code fix is offered when the property, apart from its attributes, fits on one line."
- Q3 (RH5401, comment between attribute and property): neither A nor B. In English: the fix must not insert a blank line; that is not part of its scope. Read as: the fix is offered and gives `[Obsolete]⏎// note⏎int E => 1;` (no blank line added).
- Q4 (RH5307 further receivers/containers): A — each fix joins the list it reports.

Question 5 (04-spec-author.md, several blank lines between attributes and property; also without comment and between two attribute lists). Maintainer's reply, verbatim: "5a, dont touch" — A: the fix keeps the blank lines as written.

Escalation 9 (question 6, after the implementer: ~200-line blank-line restore in the RH5401 fix). Maintainer's reply, verbatim: "B is ok" — B: withhold the RH5401 fix when formatting would insert or remove a blank line behind an attribute list (cheap blank-line-count comparison); the Q3 input and the Q5 input get no fix; `[Obsolete]⏎⏎// note⏎int E⏎    => 1;` still gets `[Obsolete]⏎⏎// note⏎int E => 1;`; `[Obsolete] int E⏎    => 1;` still gets `[Obsolete]⏎int E => 1;`. Supersedes the offered-fix part of answers 3 and 5.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | spec author | Lane | Widen bug fix → behavior change | RH5307-004 and RH5401-003/-004 limited the fix to today's inputs; offering it changes requirements, so a change doc is required. Lane check confirms behavior-change | The visible changes are the maintainer's decisions on #1217 Q1/Q3 and #1219 Q1 (2026-10-20) | 3501371 |
| 2 | spec author | RH5307 rule page mentions `?[ ]` despite rule-page convention | Keep the sentence | The maintainer's decision #1217 Q1 names the page | Maintainer decision covers it | 3501371 |
| 3 | skeptic | RH5307 architecture finding: the fix decides the reported list by a different rule than the analyzer and keeps a private copy of the analyzer's guard | Change the structure in this run: the fix resolves the list at the diagnostic's location and reuses the analyzer's guard instead of its private copy, where the analyzer exposes it; otherwise the copy becomes a follow-up | The narrow fix is the structural fix; stays in the analyzer and its code fix (counterparts), small | Which list is fixed is the maintainer's #1217 Q3 decision; the new members are asked as output questions | |
| 4 | skeptic | RH5307 class wider than the change doc (c?[0][…], c?[m[…]], initializer `[m[…]] = 1`, a[0].P[0][…]/this[0][…], j[0,⏎1][1,⏎2]) | Widen the run to the whole class; members not named by #1217 Q3 go into the output-question batch | Same cause (target resolved from the receiver's first token) | Asked | |
| 5 | skeptic | Spec wording: RH5401-003 "as FMT-BREAK-027 places them"; RH5307-004 example gains `c?[0][1,⏎2]`; change doc names the whole class incl. no-progress members | Accept all three | Prevent misreadings; no output change | Wording only | |

## Follow-up candidates

- FMT-BREAK-014 literally covers `c?[1,⏎2]` but the formatter leaves it wrapped (reachable through whole-document formatting/CLI). Carries an output question: A join it like `c[1,⏎2]` (recommended, RH5307 stays silent under P11), B clarify FMT-BREAK-014 to exclude element-binding lists. Source: 01-spec-author.md.

## State

- Last step: spec author (01, commit 3501371), change doc Draft. Open output question: 01 Q3 (attribute on the property's line).
- Skeptic done (02). Spec author amendment done (03, dba3ee9), change doc Draft.
- Batch 1 carried out (04, 36627a0), change doc Accepted. RH5401-003 got a rule-local exception: no blank line inserted.
- Q5 carried out (04 follow-up, f0437cc). Test implementer done (05, b1dc664): 41 new, 3 changed tests, 60 failing as expected. Implementer done (06, b64257f/a6ab82f): all tests pass; RH5401 fix grew ~200 lines restoring blank lines after formatting (Q3/Q5), and reports an architecture finding (fix vs. blank-line phase own the same gaps).
- Escalation 9 answered: B. Spec author done (07, 398d780). Test implementer done (08, dfa6179): 14 RH5401 rows fail as expected. Next: implementer (09), then cleaner.
