# Lead — fix-scope-and-layout

- Branch: `claude/squad-tickets-review-eiuqp5` (the session's required branch, instead of `claude/batch-1140-...`)
- Draft PR: thoenissen/Reihitsu#1686
- Lane: **bug fix**
- Base at claim: `d6a01ee`

## Issues (prompt order)

| Issue | agent-filed | Lane reason |
|---|---|---|
| thoenissen/Reihitsu#1140 | yes | Code fixes (RH5101/5102/5110/5111/5112+5201/5113/5204/5206, plus comment instances RH5412/RH5417 and round-7a instances) format more than the code their diagnostic is about; observed vs. expected against SHARED-FIX-006 (P9). Bug fix. |
| thoenissen/Reihitsu#1141 | yes | RH6023/RH6024 fixes format more than the two gaps next to the operator; against SHARED-FIX-006, SHARED-OPSPACE-004. Bug fix. |
| thoenissen/Reihitsu#1144 | yes | Fix output not laid out as the formatter lays it out (RH5101/5102 raw string, RH5205, RH5203, plus P14b instances RH5417/RH3202 and round-7a instances); against SHARED-FIX-007 (P10). Bug fix. |
| thoenissen/Reihitsu#1145 | yes | RH5103 fix withheld behind embedded/labeled statements (guard scope) and wrong indentation into one-line blocks; against RH5103-003/004, SHARED-FIX-007. Bug fix. |
| thoenissen/Reihitsu#1094 | yes | RH5513 fix: break before attribute list, indentation in continuation lambda, tab-indented code; against SHARED-ATTR-004/008/009, SHARED-FIX-007. Bug fix. |
| thoenissen/Reihitsu#1393 | yes | RH5104 fix inserts a blank line before moved comment (RH5417 half moved to #1443 by triage, not part of this run); likely resolved on main by #1457. Bug fix. |
| thoenissen/Reihitsu#1511 | yes | RH5204 fix makes no progress on a first type behind a leading doc comment/directive; against SHARED-FIX-001. Bug fix. |
| thoenissen/Reihitsu#1561 | yes | RH5204 fix makes no progress on a doc comment in front of a switch label; against SHARED-FIX-002. Bug fix. |

All goals are proposals (all `agent-filed`).

Maintainer clarifications recorded on the issues (summarized):
- #1140: triage says bug-fix lane, no change doc; RH5111-004 and RH5401-003 are aligned to SHARED-FIX-006 as a clarification in the same PR. Further P9/P14a instances in comments (round 7a list; RH5412/RH5417 region instances of 2026-11-07).
- #1144: further instances in comments (round 7a list; RH5417 `int _x; void M() { }`; RH3202 `=> _value; private int _x;`; RH5405 `else if` brace column). The "misindented target keeps its column" instance is now specified behavior (FMT-PIPE-017) — not a defect.
- #1393: triage moved the RH5417 half to #1443; this issue keeps only the RH5104 part; likely resolved on main by #1457 — run adds the regression test and closes, or `Refs` if a defect remains.

## Roles

Bug-fix lane: skeptic (premise) → spec author (only for clarifications) → test implementer (reproductions) → implementer → cleaner → spec checker → auditor → validator. Issue manager before publishing if follow-ups exist.

## Maintainer answers

1. RH5204 scope (#1140, #1511) — **A**: the fix moves only the reported line; RH5204-006 changes accordingly (the whole-scope formatting is dropped).
2. RH5203 comment line above an argument (#1144) — **A**: the comment line is aligned with the argument; RH5203-003 changes accordingly.
3. Escalation 4, #1393 not reproduced — **A**: add a characterization test with the issue's input (LF, CRLF) and link with `Closes #1393`, noting it was resolved before the run.
4. Escalation 10, token behind a multi-line comment's closing line — **A**: not declared; RH5204's analyzer stops reporting a token that does not start its line (SHARED-BLANK-008).
The maintainer raised no objection to the narrowing of #1140 and the withheld rare classes (decisions 3–9).

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's required branch `claude/squad-tickets-review-eiuqp5` instead of `claude/batch-1140-<slug>` | The session's environment permits pushing only to that branch | Branch names are internal | 6976d68 |
| 2 | Start | Order of skeptic and spec author in the bug-fix lane | Skeptic first (premise + reachability + reproduction status on main), spec author afterwards only for the clarifications the confirmed issues need | Several issues may already be resolved on main (#1393, #1144 RH5101/5102); clarifications for cases that do not reproduce would be wasted | Order of roles is internal | — |
| 3 | Skeptic | #1140 spans ~20 code fixes and ~15 rule requirements (architecture finding: scope chosen by node; fixes predicting the indentation phase) | Fix narrowly, no new formatter entry point. #1140 is narrowed to the instances on fixes this run changes anyway: RH5204 (with #1511/#1561), RH6023/RH6024 (#1141), RH5405–RH5407/RH5403 (with #1144 `else if`), RH5417/RH3202 (with #1144 shared line). Its other instances (RH5101/5102 content, RH5110, RH5111, RH5112/RH5201 fallback, RH5113, RH5206, RH5303–RH5308, RH5401, RH5404, RH5408, RH5412–RH5416, RH3201 Q2) stay on #1140, which is linked `Refs`; the structural cause becomes a follow-up candidate | An entry point that lays out only changed code is >100 lines/new type; the batch is already eight issues; narrowing leaves those outputs as on `main` | Narrowing keeps `main`'s output; nothing new is written | — |
| 4 | Skeptic | #1140 output questions 2 (RH5112) and 3 (RH5101 `1+2`) | Not asked: they belong to the instances left on #1140 (decision 3); carried into the follow-up | Outside the narrowed run | Not changed by the run | — |
| 5 | Skeptic | #1141 `/* c */` instance | Drop as defect; characterization test | Does not reproduce | Unchanged output | — |
| 6 | Skeptic | #1144 RH5101/RH5102 raw-string, #1238 Q2, misindented-target instance, FMT-PIPE-010 sentence | Drop; characterization tests for the raw-string and #1238 Q2 inputs; no FMT-PIPE-010 change (FMT-PIPE-017 already states it) | Do not reproduce / already specified | Unchanged output | — |
| 7 | Skeptic | #1144 RH5205 lambda with attribute list | Withhold the RH5205 fix when the lambda carries an attribute list (rare class, cheap check); record in RH5205 spec | Avoids changing FMT-PIPE-017's implementation for a rare shape | Withholding for a rare class (README) | — |
| 8 | Skeptic | #1145 Q4 (preceding statement does not start its line inside a block) | Withhold the RH5103 fix for that class; record in RH5103 spec. Q2: narrow the guard | Both Q4 inputs already violate RH5417 (rare); computing the column would predict the indentation phase | Withholding for a rare class | — |
| 9 | Skeptic | #1094 Q3 (attribute list in a block not at its depth column) / Q4 | Q3: withhold the fix when the innermost enclosing brace is not at four spaces per scope (SHARED-ATTR-008's own boundary). Q4: drop, characterization test (output decided in #1030 Q4). Q2: fix | Rare class; Q4 already decided output | Withholding for a rare class; Q4 unchanged | — |
| 10 | Skeptic | #1144 `else if` brace column vs #1416 | Fixed here under SHARED-FIX-007; #1416 (SHARED-BRACE-002 wording) stays its own issue | SHARED-FIX-007 decides the column already | Unchanged requirement decides it | — |
| 11 | Skeptic | #1561 | Analyzer fix: RH5204 uses SHARED-BLANK-008 (`StartsLine`); no spec clarification | RH5204-001 + SHARED-BLANK-008 already decide "not reported" | Bug fix back to unchanged requirement | — |
| 12 | Spec author | RH5204-006: a wrapped continuation line below the reported line keeps its column | Accept: covered by answer 1A ("only the reported line moves"); RH5203 reports the continuation separately | Literal reading of the maintainer's answer | Covered by maintainer answer 1 | b92e71f |
| 13 | Spec author | Change doc for the rewritten RH5204-006 and RH5203-003 | None; lane stays bug fix | Both rewrites align a rule requirement to unchanged shared requirements the maintainer already decided (SHARED-FIX-006/P9 for RH5204; SHARED-FIX-007/P10 and #1123 Q1 for RH5203), as triage J2 on #1140 prescribes; the answers are recorded in the specs and here | Covered by maintainer answers 1 and 2 | b92e71f |
| 14 | Spec author | RH5103-005 defined by brace position; SHARED-ATTR-021 measures the brace's line | Accept | Keeps common inputs (`M(); M(); M();`, `void M() {`) fixable | Narrower withholding than decided; rare class only | b92e71f |
| 15 | Test impl. | Three SHARED-FIX-010 "no fix offered" tests (RH5405, RH5403, RH6024) depend on the new fix scope | Leave as written; if the narrowed fix no longer formats the refused node, the test implementer rewrites them to "fix offered" in the repair worklist | SHARED-FIX-010 (unchanged) withholds only when the formatted node holds the shape; scope change covered by answer 1A/P9 | Follows from unchanged SHARED-FIX-010 and answer 1 | — |
| 16 | Test impl. | Does RH5204-006 "second line of a multi-line comment keeps its column" cover a run of `///` lines? | No: it covers delimited comments; a `///` run moves as one comment (today's output, existing test unchanged). Spec author adds the wording | Keeps `main`'s output; the sentence was written for delimited comment text | Output unchanged from `main` | — |
| 17 | Test impl. | RH5204 `GetFixAllScenario` exact `NumberOfFixAllIterations = 2` | May be relaxed to at most two if it trips (test implementer, in repair) | Iteration count is internal | Internal | — |
| 18 | Test impl. | RH5403 fix moves `while` of `do` to the next line | Accept | Formatter's layout (SHARED-FIX-007) | Unchanged requirement decides it | abe294f |
| 19 | Test impl. | #1141 Fix All tests expect one pass | Keep; the implementer aims for one pass. If not reachable inside the narrowed fix, decide then (the issue routes multi-pass to the Fix All issue) | Issue's done-when | Internal (iteration count) | — |

## Follow-up candidates

- Structural cause of #1140 (skeptic 01, *Architecture findings*): fixes choose scope by choosing a node and `FormatNodeInDocumentAsync` formats everything inside it; fixes that need less compute columns themselves; ~15 rule requirements restate a wider scope than SHARED-FIX-006. Carries #1140's remaining instances and output questions 2 (RH5112) and 3 (RH5101). (01-skeptic)
- Other analyzers calling `IsFirstOnLine` where their spec says SHARED-BLANK-008 (01-skeptic, #1561)

## Worklist / next step

- Skeptic done (01-skeptic.md, 2402f9b). Answers received (1A 2A 3A 4A). Was waiting for maintainer: output questions RH5204 scope, RH5203 comment line; escalation 4 (#1393); escalation 10 (#1561 shape); confirmation of narrowing #1140 (shown for visibility).
- Spec author done (02-spec-author.md, b92e71f); lane check confirmed bug-fix.
- Test implementer done (03, abe294f): all reproduced except #1393 (characterization, answer 3).
- Next: implementer; spec author amendment for decision 16 in parallel. (old: spec author (clarifications RH5204-006, RH5203-003 per answer, RH5103-003 withheld class, RH5205 withheld class, RH5513/SHARED-ATTR withheld class), then test implementer.
