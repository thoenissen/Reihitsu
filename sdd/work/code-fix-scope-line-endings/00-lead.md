# Lead — code-fix-scope-line-endings

- Issues: thoenissen/Reihitsu#1283, #1284, #1285 — all three carry `agent-filed` (goals are proposals).
- Slug: `code-fix-scope-line-endings`
- Branch: `claude/issues-1283-1284-1285-code-fix-scope-line-endings`
- Draft PR: thoenissen/Reihitsu#1699
- Lane: **behavior change** (widened from bug fix, decision 2), for every issue.
  - #1283: RH3006 code fix writes an LF into a CRLF document; reported against SHARED-FIX-008 (exists on `main` since #1357). Observed-versus-expected difference in shipped code → bug fix.
  - #1284: expression-body fixes (RH3202, RH3203, RH3205–RH3208) format more than the replaced body; reported against SHARED-FIX-006/-007 (P14d). Bug fix; the issue and its triage comment ask that SHARED-BODY-003 and the RH3202 layout example of SHARED-FIX-003 be reworded as a clarification (spec author runs for that).
  - #1285: RH3001 fix withheld by accident for argument / list positions, and should be withheld for constant patterns (P23). Bug fix. A comment on #1285 adds the parentheses instance (#1245 Q3, round 7b option A: parentheses only where binding needs them) — it is part of the issue.
- Blocker #1138 is closed (merged via #1357): SHARED-FIX-006/-007/-008 exist.
- Roles: spec author (clarification only: SHARED-BODY-003, SHARED-FIX-003 example, RH3202 spec, any RH3001/RH3006 spec text the instances need) → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. Issue manager before publishing if follow-up candidates exist.

## Maintainer answers

- Batch 1, Q1 — RH3001, negation as an `orderby` key without a direction (`orderby !a`): **A** — offer the fix, output `orderby a == false` (as recommended).

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch slug has five words (`code-fix-scope-line-endings`) instead of at most four | Keep it | Already pushed with the claim; renaming costs a new claim | Internal naming | 114ae37 |
| 2 | Spec author | #1285 changes RH3001-004 and adds RH3001-008, so it needs a change doc | Widen the run's lane from bug fix to behavior change (lane check confirmed) | A change doc under `sdd/changes/` requires the behavior-change lane; the roles are the same | Lane is internal; visible changes are those of the change doc | cd2ddbc |
| 3 | Spec author | RH3001-008 keyed on the constant-pattern node, so `case !true:` keeps its fix | Take it | The maintainer saw `case !true:` fixed as the contrast case in #1245 Q2; withholding it would change visible output nobody asked to change | Keeps today's output | cd2ddbc |
| 4 | Spec author | #1284 triage comment's "coverage sentence" | SHARED-BODY-003 scope plus RH3202 intro, no new requirement per rule spec | Smallest reading that meets the comment | Spec wording only | cd2ddbc |
| 5 | Spec author | #1283's "CRLF check in the fix test base" goal | Left to the test implementer to propose; lead decides on its handover | Test infrastructure is internal | Tests only | — |
| 6 | Skeptic | #1283 test tags | Tag the RH3006 CRLF test SHARED-FIX-006 + RH3006-003; SHARED-FIX-008 only for the blank-line case | The stray LF lies outside the replaced span; SHARED-FIX-008 excludes it, SHARED-FIX-006 governs | Tags are internal | — |
| 7 | Skeptic | #1283 goal "add a CRLF check to the fix test base" | Drop; `VerifyCodeFixWithEndOfLine` already exists | Goal already met | Test infrastructure | — |
| 8 | Skeptic | Withhold the RH3006 fix in CRLF documents? | No; repair it | Fix works in LF and the input is common enough | Restores SHARED-FIX-006 output | — |
| 9 | Skeptic | RH3003 shows the same LF-in-CRLF defect (architecture finding) | Fix narrowly in this run for RH3006; RH3003 becomes a follow-up, unless the implementer's repair lands in a shared component both fixes use, in which case it is reported and the lead decides an adjacent fix | Outside the run's issues; not yet traced whether the mechanism is shared | No visible output of the run changes | — |
| 10 | Skeptic | RH3001 trailing space when `!` ends a line before a comment | Dropped, not a follow-up | Comment inside a binary operand on its own line is a rare input class | — | — |
| 11 | Skeptic | SHARED-BODY-003 wording ("last token in front of `=>`", name initializer and constraint clauses), RH3001-004 / change doc to name the five extra argument positions, #1283 example tags | Route to the spec author (continued) as one amendment | Small corrections in the area it wrote | Five positions are plain arguments, covered by #1245 Q2 | — |
| 12 | Skeptic | #1284 goal "reword the SHARED-FIX-003 example" | Already met; drop | Example already says "the node the fix replaces" | — | — |
| 13 | Test implementer | RH3202 on a tab-indented member: keep the signature line's tabs and write the new block in spaces at the formatter's column, or copy the tabs into the block, or withhold | Keep the tabs on the untouched signature line, write the new block lines in spaces (tests as written) | SHARED-FIX-006 keeps the unedited line; SHARED-FIX-007 lays out the new lines as the formatter does, and the formatter writes no tab after its preparation phase, so copying tabs would contradict SHARED-FIX-007 and need machinery. Tab indentation violates RH5601, so it is a rare class; withholding is not needed because no special case is required | Only one reading is consistent with the unchanged SHARED-FIX-006/-007; rare class (AGENTS.md, Code-fix coverage) | 4d0bec2 |

## Follow-up candidates

- RH3003 code fix writes an LF into a CRLF document (`return a +` / `/* x */ "";`), same symptom as #1283 — 02-skeptic.md.

## State

- Audited SHA: —
- Last step finished: spec author (01-spec-author.md, change doc `sdd/changes/2026-11/rh3001-fix-scope.md` Draft).
- Skeptic done (02-skeptic.md): all reproduced and reachable; one output question (RH3001 `orderby !a` without direction).
- Spec author amendment done (ff3315d).
- Maintainer answered batch 1 (A).
- Change doc Accepted (b20b8a7).
- Test implementer done (03-test-implementer.md, 4d0bec2): #1283 and #1284 reproduced; #1285 tests red as expected.
- Next step: implementer (04-implementer.md).
