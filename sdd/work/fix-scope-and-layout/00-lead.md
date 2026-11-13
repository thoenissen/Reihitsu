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
| 20 | Implementer | Change grew to ~408 net production lines and a new type `LineLayoutCodeFixUtilities` (294 lines) that computes brace/line columns (architecture finding confirmed: fixes predicting the indentation phase) | Narrow instead of escalation 9: the brace fixes RH5405–RH5407 and RH5403 go back to `main` (production, tests, RH5403-003 and SHARED-BRACE-002 text); their #1140 instances stay on #1140 and the `else if` column of #1144 is left to #1416 (reverses decision 10). The shared-line member helper stays but moves into an existing type; no new type; target net growth about 100 lines, otherwise the implementer reports and it becomes escalation 9 | Removes the bulk of the column prediction and the new type; narrowing keeps `main`'s output | Narrowing keeps `main`'s output for the brace fixes | 1e9a5f7, d979e84, 0b12f75 |
| 21 | Implementer | Red tests that pin outputs the changed or unchanged requirements forbid: RH6024 SHARED-FIX-010 test (decision 15), RH5204 incremental-iteration count (LF/CRLF), RH5204 Fix All count (decision 17), RH5204 blank-line removal behind stray doc comment (RH5204-006/answer 1), two RH6023 wrapped-link tests (SHARED-OPSPACE-004/P9), five RH5511 tests keeping the list on the previous member's line (SHARED-FIX-007 shared line), RH3102/RH3201 tests pinning the fixed defects | Test implementer rewrites them | Each assertion contradicts a requirement covered by an answer or unchanged | Covered by answers 1 or unchanged SHARED-FIX-006/007 | — |
| 22 | Implementer | RH5203 test: `AssertFormattingChangesNothing` fails because the formatter inserts a blank line before a comment line inside an argument list | Drop that assertion for this input; output stays answer 2A. Not a follow-up: a comment inside an argument list already violates RH5038 (rare class) | Rare class; maintainer chose the output | Output is answer 2A | — |
| 23 | Implementer | Trailing-comma fix turns `B , // c` into `B  // c` (PR-introduced suspect, untested) | Test implementer adds a test with the formatter's output; implementer fixes if red | Within the changed helper | SHARED-FIX-007 decides | — |
| 24 | Spec checker | No change doc although RH5204-006/RH5203-003 change and three withheld-fix requirements are added | Keep decision 13; withheld rare classes are recorded in the rule spec as AGENTS.md *Code-fix coverage* prescribes; the PR's Review notes state the reason | Covered by maintainer answers 1–2 and the rare-class policy | Answers 1–2; rare classes | — |
| 25 | Spec checker | Repair round 1: stale FMT-PIPE-024 tags on six RH6023 tests; optional SHARED-FIX-007 tag on the RH5513 tab test | Test implementer fixes both; it runs `scripts/format.sh` on the file itself, so the cleaner is not re-run (tags only); spec checker re-runs on the repair | Tag-only change | Internal | — |
| 26 | Test impl. (repair 1) | SHARED-ATTR-021 does not say how a tab counts in "indented four spaces for each such brace" | A tab counts as four columns (the code's behavior); spec author states it. Keeps `main`'s output for tab-indented code (fix offered, #1030 Q4) | Counting one would withhold a fix `main` offers for common tab-indented code | Keeps `main`'s output | — |
| 27 | Auditor 13 (repair round 2) | F1 high: RH5203 fix does not converge when a delimited comment stands in front of the argument on its line | Refuse the class: withhold the fix when anything but whitespace precedes the argument on its line; RH5203-003 records it | Rare class (comment in argument list, RH5038); safety rail repaired in scope | Withholding for a rare class | — |
| 28 | Auditor 13 | Architecture finding / F2: shared-line split column decided by two components; wrong on a type written on one line | Refuse the class: `FormatMemberOnLinesOfItsOwnAsync` separates only when the member's line starts with a sibling member of the same list; otherwise the fix behaves as on `main` | Rare (RH5404 reports a one-line type); refusing removes the disagreement | Rare class keeps `main`'s output | — |
| 29 | Auditor 13 | F3: attribute base does not split a sibling behind the member (`[Obsolete] int _a; int _b;`) | Follow-up (pre-existing on `main`, not a regression) | Keeps the run narrow | Unchanged output | — |
| 30 | Auditor 13 | F4 tests (comment in gap per caller path), F5 RH5203-003 comment kinds (spec narrowed to the kinds the code moves; doc comments inside an argument list are a rare class), F6 SHARED-ATTR-021 wording vs SHARED-ATTR-008 (spec text), F7 stale comment (cleaner) | Route to owners | Same requirement / text | F5 rare class; others internal | — |
| 31 | Auditor 13 | #1144 `else if` instance left to #1416 | `Closes #1144`; Review notes name the instance as tracked by #1416 | Issue's main instances met | — | — |
| 32 | Implementer 16 | `FixLeavesTheMemberInFrontOnTheLineOfAMemberOfATypeWrittenOnOneLine` CRLF half: input has no line break, so CRLF conversion is a no-op while the expectation is CRLF | Test wrong: test implementer gives the input a line break (or drops the CRLF half) | Test-harness artifact, `main` gives the same output | Output equals `main`'s | — |

## Follow-up candidates

- Structural cause of #1140 (skeptic 01, *Architecture findings*): fixes choose scope by choosing a node and `FormatNodeInDocumentAsync` formats everything inside it; fixes that need less compute columns themselves; ~15 rule requirements restate a wider scope than SHARED-FIX-006. Carries #1140's remaining instances and output questions 2 (RH5112) and 3 (RH5101). (01-skeptic)
- Other analyzers calling `IsFirstOnLine` where their spec says SHARED-BLANK-008 (01-skeptic, #1561)
- Other fixes that format a whole member (expression-body base behind RH3203 etc.) may leave a sibling member on a shared line (04-implementer)
- #1140's brace-fix instances (RH5405–RH5407, RH5403: scope, blank lines) and #1144's `else if` brace column (with #1416) — narrowed out by decision 20 (04-implementer)
- Attribute-placement fix leaves a sibling member behind the member on its line (`[Obsolete] int _a; int _b;`, RH5517), SHARED-FIX-007; pre-existing (13-auditor F3)

## Worklist / next step

- Skeptic done (01-skeptic.md, 2402f9b). Answers received (1A 2A 3A 4A). Was waiting for maintainer: output questions RH5204 scope, RH5203 comment line; escalation 4 (#1393); escalation 10 (#1561 shape); confirmation of narrowing #1140 (shown for visibility).
- Spec author done (02-spec-author.md, b92e71f); lane check confirmed bug-fix.
- Test implementer done (03, abe294f): all reproduced except #1393 (characterization, answer 3).
- Spec amendment f85c954. Implementer done (04, ba83087/f2875e4): 18 red tests, growth 408 lines + new type.
- Spec author 05 (1e9a5f7) restored RH5403/statement-braces specs; test implementer 06 (d979e84): 7 red brace tests await production revert; decision 23 no defect; RH3102 duplicate characterization test kept (harmless).
- Implementer 07 (0b12f75): all tests pass; no new type; net production +87 lines (330+/243-). Decision 20 carried out.
- Cleaner 08 clean (43087b1). Spec checker 09 (f36127b): findings → repair round 1 (decision 25): test implementer 10 (beb8ec6) done; spec author 11 (d50e9f8), tag amendment eb50771, spec checker 12 (d3c17e1) consistent. Build green. Auditor 13 on head 261ba8f (audited SHA), base main d6a01ee: findings → repair round 2 (decisions 27–30): spec author 14 (e846caa) + test implementer 15 (d85b4d6), implementer 16 (fa0083d, 1 disputed CRLF test → decision 32, test implementer amendment), cleaner 17, spec checker 18, auditor 19 (repair only), validator.
- Repair (not a repair round; narrowing + test disputes): spec author 05 and test implementer 06 in parallel, then implementer 07.
- (old) Next: implementer; spec author amendment for decision 16 in parallel. (old: spec author (clarifications RH5204-006, RH5203-003 per answer, RH5103-003 withheld class, RH5205 withheld class, RH5513/SHARED-ATTR withheld class), then test implementer.
