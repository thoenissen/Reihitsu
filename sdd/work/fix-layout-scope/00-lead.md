# Lead — fix-layout-scope

- Issues (prompt order): thoenissen/Reihitsu#1140 (agent-filed), thoenissen/Reihitsu#1687 (agent-filed), thoenissen/Reihitsu#1748 (agent-filed). All goals are proposals.
- Slug: `fix-layout-scope`
- Branch: `claude/magical-edison-h81uj2` (the session environment only permits pushing to this branch; used instead of `claude/issues-1140-1687-1748-<slug>`).
- Draft PR: thoenissen/Reihitsu#1760
- Lane: **bug-fix**
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

## Follow-up candidates

- FU1 (01-spec-author): SHARED-FIX-007 *Shared line* read literally makes a gap-only fix (e.g. RH6024 on `int _a = 1+2; int _b;`) split two members on one line; recommend reading A, only the gaps change. Outside the run's issues; output question carried in the follow-up.

## Worklist / next step

- FU2 (02-skeptic): RH5303–RH5308, RH3201 scope instances unmeasured; stay on #1140 (no new issue; #1140 stays open via `Refs`).
- FU3 (02-skeptic, D6): entry point that writes back only changed code whose first token moves to a new line (RH5110, RH5403/RH5404 header) — design question already recorded on #1140; extend #1140 rather than a new issue.

## Worklist / next step

- Asked maintainer (batch 1, before test implementer): Q1 RH5112 chain in `if` header (options A whole header line / B main / C chain only); Q2 RH5101 list element interior; Q3 RH5206 switch expr in `if` header; Q4 RH5403/RH5404 interior members rewritten by other rules. ANSWERED 1C, 2A, 3A, 4A (M2–M5).
- After answers: spec author (fresh) for D6 reverts, D7 consolidation, D5 FMT-PIPE-010/024 amendment, answers; then test implementer.
