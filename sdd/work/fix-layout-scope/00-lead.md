# Lead — fix-layout-scope

- Issues (prompt order): thoenissen/Reihitsu#1140 (agent-filed), thoenissen/Reihitsu#1687 (agent-filed), thoenissen/Reihitsu#1748 (agent-filed). All goals are proposals.
- Slug: `fix-layout-scope`
- Branch: `claude/magical-edison-h81uj2` (the session environment only permits pushing to this branch; used instead of `claude/issues-1140-1687-1748-<slug>`).
- Draft PR: thoenissen/Reihitsu#1760
- Lane: **behavior-change** (widened from bug-fix, D12)
  - #1140: code fixes that format more than the code their diagnostic is about, against SHARED-FIX-006 and the rules' fix requirements — observed vs. expected; no new behavior.
  - #1687: fixes that format a member leave a sibling member on the member's line, against SHARED-FIX-007 (shared line) — observed vs. expected.
  - #1748: RH5403/RH5404 fixes insert a blank line above a comment that leads the target, against SHARED-FIX-006 — observed vs. expected.
- Roles: spec author (clarification: rule requirements that restate a wider scope than SHARED-FIX-006, per #1140) → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. Issue manager before publishing if follow-ups exist.

## Maintainer clarifications (from the issues, summarized)

- #1140: principle P9 (maintainer, #912 round 5): a code fix changes only the code its diagnostic is about; layout around it stays. Round 7a (2026-10-20): also P14a — a fix inserts no blank line above the changed node. Triage: bug-fix lane, no change doc; RH5111-004 and RH5401-003 aligned to SHARED-FIX-006 as a clarification. The issue records that an entry point laying out only changed code would be >100 lines and a new type (design question, escalation 9 territory); until decided, instances are fixed narrowly with a cheap guard or stay as on main. Two output questions are still open on the issue (RH5112 chain in `if` header; RH5101 operator spacing inside an element) — not yet answered.
- #1748: maintainer answered **A** (2026-11-15): RH5403 and RH5404 fixes insert no blank line above a comment that leads their target; leading trivia, blank lines included, stays as written.
- #1687: no maintainer answer; the issue allows treating the class as rare (input already violates RH5417) with the gap recorded in the rule's spec — the reproduction decides.

## Maintainer answers

| # | Question | Answer |
|---|---|---|
| M1 | #1748 RH5403/RH5404 blank line above a leading comment | A — no blank line inserted; leading trivia kept as written (given on the issue, 2026-11-15) |
| M2 | Q1 RH5112 chain in `if` header (#1140) | C — only the chain is joined; rest of the header (`x==1`) and the body stay as written. Applies to the SHARED-CHAIN-009 fallback (RH5112, RH5201) |
| M3 | Q2 RH5101 spacing inside a list element (#1140) | A — only the list's line breaks change; element interiors (`1+2`) stay. Same for RH5102 and the other list-layout rules (RH5303–RH5308, RH5417-003 interior where the spec author found the same sentence) |
| M4 | Q3 RH5206 switch expression in `if` header (#1140) | A — only the switch expression is laid out; the `if` body stays |
| M6 | Q6 (escalation 9) scope of the M3/M5 write-back | C — not in this run: RH5101, RH5102, RH5303–RH5308, RH5403, RH5404, RH5417 keep `main`'s output inside the construct; M3/M5 (and the spanning-part question, recommended A: part moves as one piece) go into a follow-up that builds the write-back step |
| M7 | Q7 RH5303–RH5308 code around the construct | B — only the construct is laid out, from the column its first token keeps; rest of the statement stays (interior follows M6: formatted as on main) |
| M8 | Q8 change 7: refused shape elsewhere in the statement (RH5303–RH5308) | A — the fix is offered; it formats only the reported construct |
| M9 | Q9 RH5308 conditional in `[assembly:]`/`[module:]` attribute argument | A — the fix is offered; RH5308-006 retired |
| M5 | Q4 RH5404/RH5403 contents rewritten by other rules (#1140) | A — the declaration/statement is broken onto lines; members/statements inside keep their own shape (no expression-body-to-block conversion, no attribute split, no blank line inserted) |

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| D1 | Claim | Branch name | Use `claude/magical-edison-h81uj2` | Session environment forbids pushing to other branches | Internal | ed5cd61 |
| D2 | Claim | Lane | bug-fix for all three | Each reports observed vs. expected against an unchanged shared requirement | Internal | ed5cd61 |
| D3 | Spec author | Change doc needed for the eight rule-requirement rewrites? | No; accepted as clarifications | They remove text contradicting unchanged SHARED-FIX-006 or apply M1; triage on #1140 already called this a clarification | Behavior demanded by unchanged SHARED-FIX-006 / M1 | d1884a0 |
| D4 | Spec author | Keep a rewritten requirement whose fix this run may not change? | Decide per rule after the implementer: any requirement whose fix stays as on main is reverted and its instance left on #1140 | Avoid spec/code contradiction in the PR | Spec text only | pending |
| D5 | Skeptic | Arch finding: two owners of a fix target's leading trivia (FMT-PIPE-010/024 vs SHARED-FIX-006) | Change the structure in this run: `FormatNodeInDocumentAsync` keeps the target's leading trivia (as FMT-PIPE-018 does for the context entry point); FMT-PIPE-010/024 amended; escalation 9 if it exceeds ~100 production lines or a new type | One owner; code fixes are the entry point's only callers; class is common so refusing is no option | Output demanded by unchanged SHARED-FIX-006 and M1 | pending |
| D6 | Skeptic | Arch finding: no entry point lays out only changed code whose first token moves (RH5110; RH5403 header/other branches; RH5404 header) | Narrow: these stay as on `main`; revert those sentences of RH5110-003, RH5403-003, RH5404-003 (keep leading-trivia/M1 parts); skeptic question 4 dropped; #1140 linked `Refs` | Needs a prediction of the indentation phase or a >100-line mechanism; issue allows "stays as on main" | Outputs stay as on `main` | pending |
| D7 | Skeptic | "Leading trivia of the formatted node stays" in eight rule specs or once? | Once, in SHARED-FIX-006; rule specs keep `Uses`, removal of contradicting text, examples | One statement of a rule | Spec wording | pending |
| D8 | Skeptic | Arch finding #1687: shared-line decision has two owners (`FormatMemberOnLinesOfItsOwnAsync`, private branch in `TargetAttributePlacementCodeFixProviderBase`) | Route the attribute fixes (RH5513/RH5517) and the measured failing fixes (expression-body base RH3203/RH3205/RH3207, RH5401, RH5408, RH5113, RH5404 nested type) through the existing helper; withhold only where it does not fit (rare) | One owner, existing helper, no new type | Output demanded by unchanged SHARED-FIX-007 | pending |
| D9 | Skeptic | #1687 RH3208 statement case; RH5417 rarity premise | Drop both | SHARED-FIX-007 covers members only; RH5417 does not report two members on one line | Nothing changes | n/a |
| D10 | Skeptic | RH5111 node choice; RH5113 semicolon only | In scope as narrow fixes | Cheap, local | Demanded by clarified RH5111-004/RH5113-003 + SHARED-FIX-006 | pending |
| D11 | Skeptic | #1748 wider class (no-comment `if`, switch section, misindented comment) | In scope; covered by M1 and RH5403-003 | Same mechanism (D5) | Covered by M1 | pending |
| D12 | Spec author 03 | Change doc required for D5/M2/M4 (changes FMT-PIPE-010/017/018/024, SHARED-CHAIN-009, RH5206-003) | Accept; widen lane bug-fix → behavior-change (lane check confirmed); run the skeptic on the change doc and spec diff before the test implementer | `sdd/README.md` requires a change doc for changed requirements with new behavior | Visible changes of the change doc go to the maintainer (batch 2) | 8349028 |
| D13 | Spec author 03 | Remove the preceding-token and list-position facts of FMT-PIPE-024 | Accept | No consumer remains once leading trivia is kept | Internal | 8349028 |
| D14 | Batch 2 | RH5303–RH5308 narrowing (M7) joins the run | Accept as scope growth | Maintainer chose B; cheap node-choice change like M4 | Decided by M7 | pending |
| D15 | Skeptic 04 | SHARED-FIX-006 *Leading trivia* vs SHARED-FIX-007 *Shared line* wording | Add explicit exception for the shared-line break | Two requirements must not disagree on their face | Spec wording | pending |
| D16 | Skeptic 04 | Change-doc completeness (RH2001, RH3004/RH5205/RH5301/RH5302 targets, blank lines behind `{`/two blank lines stay), D6 rejection rewrite, pipeline.md code owners row, nested-construct Fix All | Accept all; Fix All tests for nested constructs required where the run changes the fix | Demanded by unchanged requirements / M1 | Internal or already decided | pending |
| D17 | Test impl 06 | RH5308 nested stair: inner `?` column 28 (document formatting, RH5308-002) vs 30 (literal reading of RH5308-004) | Keep 28 | Unchanged RH5308-002 demands it; RH5308-004 is read consistently with it | Decided by an unchanged requirement | df2ea58 |
| D18 | Test impl 06 | Comment between two members sharing a line (#1687) | Not pinned; rare class (two members on one line plus a comment) — fix may withhold | AGENTS.md *Code-fix coverage* | Rare class refusal | n/a |
| D19 | Test impl 06 | Coverage via shared code for RH3004, RH5205, RH5301, RH5302, RH5413–RH5416, RH3206; dropped nested Fix All cases (RH5306, RH5304, direct RH5308 branch) | Accept | Same call site / not reachable per spec | Internal | df2ea58 |
| D20 | Implementer 07 | Two RH5201 tests contradict changed SHARED-CHAIN-009 (`VerifyFixMovesBreakInsideOperatorOfChainInConditionalExpression`, `VerifyFixAlignsInnerChainInsideWrappedOuterChain`) | Test is wrong: route to test implementer to rewrite to chain-only output | Decided by M2 / change 5 | Covered by M2 | pending |
| D21 | Implementer 07 | RH5206, RH5303–RH5308 switch to `DocumentOrderFixAllProvider` | Accept | One Fix All pass (SHARED-FIX-002) after narrowing | Required by unchanged SHARED-FIX-002 | a356c2e |
| D22 | Implementer 07 | RH5113 keeps formatter-based withholding (syntax errors, refused shapes) though it now edits text only | Accept | Removing would be a visible change no answer covers | Keeps main | a356c2e |
| D23 | Implementer 07 | Arch finding: break in front of a shared-line member has two owners (helper vs attribute base private branch; RH5511 tests pin the base) | Fix narrowly (helper only for break behind member); structural unification is a follow-up (FU5) | Unifying changes RH5511 outputs (visible), outside the run's issues | Keeps main for the before-break | a356c2e |
| D24 | Implementer 07 | FC: `BatchFixer` on fixes that now separate shared-line members | Dropped | Only serves a rare class (two reported members on one line), unmeasured | n/a | n/a |

## Follow-up candidates

- FU1 (01-spec-author): SHARED-FIX-007 *Shared line* read literally makes a gap-only fix (e.g. RH6024 on `int _a = 1+2; int _b;`) split two members on one line; recommend reading A, only the gaps change. Outside the run's issues; output question carried in the follow-up.

- FU4 (M6): shared write-back step so parts inside a reported construct keep their own code (RH5101, RH5102, RH5303–RH5308, RH5403, RH5404, RH5417) — carries maintainer answers M3, M5 (=A) and the open spanning-part question (options from 04-skeptic Q1, recommended A). Related #1605, #1140 FU3.
- FU5 (07-implementer, D23): one owner for the line break in front of a member that shares its line (`FormatMemberOnLinesOfItsOwnAsync` vs private branch in `TargetAttributePlacementCodeFixProviderBase`); they disagree on column (tab vs spaces) — e.g. `→private int _pad; [First] internal Example() { }`. Visible for RH5511/RH5513/RH5517: carries an output question.
- FU6 (07-implementer): `PrecedingTokenFacts`/`TriviaFact` now only wrap a tree token; replace by direct reads (internal cleanup).

## Worklist / next step

- FU2 (02-skeptic): RH5303–RH5308, RH3201 scope instances unmeasured; stay on #1140 (no new issue; #1140 stays open via `Refs`).
- FU3 (02-skeptic, D6): entry point that writes back only changed code whose first token moves to a new line (RH5110, RH5403/RH5404 header) — design question already recorded on #1140; extend #1140 rather than a new issue.

## Worklist / next step

- Asked maintainer (batch 1, before test implementer): Q1 RH5112 chain in `if` header (options A whole header line / B main / C chain only); Q2 RH5101 list element interior; Q3 RH5206 switch expr in `if` header; Q4 RH5403/RH5404 interior members rewritten by other rules. ANSWERED 1C, 2A, 3A, 4A (M2–M5).
- Done: spec author 03 (8349028) — change doc `sdd/changes/2026-11/fix-layout-scope.md`; lane now behavior-change.
- Done: skeptic 04 (ac7d832). Asked maintainer batch 2: Q6 escalation 9 — scope of M3/M5 write-back (A build / B one-line parts only / C defer to follow-up, recommended C); Q7 RH5303–RH5308 code around construct (A main / B only the construct, recommended). ANSWERED 1C, 2B (M6, M7).
- Next: spec author (fresh) amendment for M6 (revert *Inside* bullet and its references; interior as main), M7, D15, D16 → 05-spec-author.md; then accept change doc; then test implementer.
- Done: spec author 05 (2d1b537). Asked batch 3: Q8 change 7 (fix now offered where a refused shape stands elsewhere in the statement; rec A offer), Q9 RH5308 conditional in [assembly:]/[module:] attribute (rec A offer, retires RH5308-006). ANSWERED 1A, 2A (M8, M9).
- Done: spec author 05 addendum (3948f75); change doc Accepted. Note: three RH5308 tests tagged RH5308-006 must be rewritten (test implementer).
- Done: test implementer 06 (df2ea58): 74 formatter + 104 analyzer rows failing for the right reason; architecture green.
- Done: implementer 07 (a356c2e): +281/−532, no new type; 2 disputed RH5201 tests.
- Next: test implementer (fresh, 08) rewrites the two RH5201 tests (D20); then cleaner.
- Pending internal after answers: SHARED-FIX-006 Leading-trivia exception for SHARED-FIX-007 shared line (accept skeptic rec); rewrite D6 bullet in change doc Alternatives; change-doc completeness items (04-skeptic Internal choices 3); pipeline.md code owners row; Fix All tests for nested constructs.
- (old) Next: skeptic (fresh) on change doc + spec diff → 04-skeptic.md; then batch 2 output questions (change-doc visible changes not covered by M1–M5; spec author's open: argument spanning lines in RH5101/5102; tab-indented target) → then test implementer.
- (old) After answers: spec author (fresh) for D6 reverts, D7 consolidation, D5 FMT-PIPE-010/024 amendment, answers; then test implementer.
