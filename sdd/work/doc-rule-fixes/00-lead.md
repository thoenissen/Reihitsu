# Lead — doc-rule-fixes

- Branch: `claude/squad-tickets-review-y2yb3m` (session-designated branch; replaces the playbook's naming)
- Draft PR: thoenissen/Reihitsu#1700
- Base at claim: cf6d26b03fe1917f193d412c8540916f93a00dc4
- Lane: **bug fix**

## Issues (prompt order)

| Issue | agent-filed | Lane | Why |
|---|---|---|---|
| thoenissen/Reihitsu#1345 | yes | bug fix | Triage comment: behavior already required on main by SHARED-DOC-004; RH8303-001/-005 aligned as a clarification; the body's change-doc line no longer applies |
| thoenissen/Reihitsu#1346 | yes | bug fix | Triage comment: already required by SHARED-DOC-008; rule spec text aligned as clarification |
| thoenissen/Reihitsu#1347 | yes | bug fix | Triage comment: already required by SHARED-DIAG-010; rule spec text aligned as clarification |
| thoenissen/Reihitsu#1348 | yes | bug fix | Two fixes undo each other (SHARED-FIX-004); comments add the formatter side (FMT-BLANK-010 text should name the region-directive exception; no formatter output changes) |
| thoenissen/Reihitsu#1270 | yes | bug fix | P20 / SHARED-DIAG-006: every element of its kind (RH8106, RH8203; comment adds RH8305, RH8306 instances) |
| thoenissen/Reihitsu#1092 | yes | bug fix | Triage split: only Q3b stays (partial type group from accessibility across parts, SHARED-DIAG-004); Q4 moved to #1448, out of this run |
| thoenissen/Reihitsu#1153 | yes | bug fix | Triage split: only the bug stays — no RH8032 diagnostic without a source location (`extension(long)`, SHARED-DIAG-007); P8b/P8c moved to #1449, out of this run |
| thoenissen/Reihitsu#1617 | yes | bug fix | Unconfirmed, found by reading code; must be reproduced first (SHARED-DIAG-004) |

All issues carry `agent-filed`: goals are proposals.

## Roles

Bug-fix lane: spec author (clarifications only: RH8303, RH8401/RH8402, RH8307, RH8302/FMT-BLANK-010, RH8002 coverage, as the issues' triage notes name) → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. Issue manager before publishing if follow-up candidates exist.

## Maintainer clarifications (summarized)

- Triage comments moved #1345, #1346, #1347 and #1092 to the bug-fix lane without a change doc; the rules' own spec text is aligned as a clarification in the same PR.
- #1092 keeps only Q3b; #1153 keeps only the missing-location bug; the split parts belong to #1448 and #1449.
- #1617: the run reproduces before building anything.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session-designated branch `claude/squad-tickets-review-y2yb3m` instead of `claude/batch-1345-<slug>` | The session environment permits pushing only to that branch | Internal | 065c044 |
| 2 | Spec author | Clarify RH8301/RH8302 too for #1345, not only RH8303 | Accept | SHARED-DOC-004 names all three rules | Required by an unchanged shared requirement | b148498 |
| 3 | Spec author | Mode `None` defined as "same result as mode `Parse`" (RH8401-006, RH8402-007) | Accept | Shortest statement of SHARED-DOC-008 | Required by SHARED-DOC-008 | b148498 |
| 4 | Spec author | Retire RH8307-006 instead of rewording | Accept | No diagnostic, nothing to withhold | Follows SHARED-DIAG-010 | b148498 |
| 5 | Spec author | #1092 clarification placed in SHARED-DOC-002 | Accept | The group is decided there for all private-group rules | Required by SHARED-DIAG-004 | b148498 |
| 6 | Spec author | Drop rule-page goals of #1345 and #1347 | Accept | Rule pages list no exceptions (AGENTS.md) | Rule pages unchanged | b148498 |
| 8 | Skeptic | #1092: fixing the group alone only renames RH8002 to RH8001 on the issue's input, contradicting #1037 Q3a (#1091) | Option C: no code change; `Refs #1092`; characterization test kept; SHARED-DOC-002 clarification reverted | agent-filed goal; A would put visible output in question, B pulls in vNext behavior change | No output changes | pending |
| 9 | Skeptic | #1348 architecture finding: `BlankLineTokenCleanupRewriter` and `BlankLineRegionDirectiveRewriter` both decide the gap; correct only by order | Fix narrowly (analyzer side); spec author words FMT-BLANK-010 as the required output, not as a single-owner claim; structural single owner becomes a follow-up | Formatter output already correct; structure change outside the components the run changes | No formatter output changes | pending |
| 7 | Spec author | #1153: `extension(long)` not reported, from the recorded P8b answer | Accept | The maintainer's P8b answer names it ("An unnamed parameter is not reported") | Covered by a maintainer answer | b148498 |

## Follow-up candidates

- Non-private documentation rules judge a partial type part by part (reported once per undocumented part; a part counts undocumented though another part is documented; SHARED-DOC-001, SHARED-DIAG-004). Possibly tracked by #1091 / #1037 Q3a. — 01-spec-author.md
- RH4105 reports unnamed extension receivers (`extension(long)`) with no source location (SHARED-DIAG-007). — 02-skeptic.md
- RH8031's verdict depends on the order of several summaries (extend #1449). — 02-skeptic.md
- RH8107 and RH8202 mark only the first of several elements (same question as output Q2; carry its answer). — 02-skeptic.md
- Link #1092 to #1091: the group fix only makes sense together with Q3a for partial types. — 02-skeptic.md
- Blank-line phase: give the gap above a region directive behind a documentation comment one owner (cleanup rewriter vs region rewriter). — 02-skeptic.md (decision 9)

## Worklist / next step

- Done: 01 spec author (b148498). Open output questions from 01: Q1 RH8307 wrapped inline element; Q2 RH8106/RH8306 several reported elements.
- Done: 02 skeptic (8bb05b0).
- Output batch 1 asked (Q1 RH8307 inline element, Q2 RH8106/RH8306 several elements, Q3 escalation 10 behind-code `///`, Q4 RH8201/RH8203 partial member documented on both parts). Waiting.
- After answers: spec author repair (revert SHARED-DOC-002 for #1092; FMT-BLANK-010 wording; RH8201-001/RH8203-001 per Q4; Q1/Q2 answers; retag notes), then test implementer.
