# Phase 5b — triage 3 (issues #1676–#1690 without milestone, main c2866ea, 2026-11-13)

Triage only (triage.md sections 1–3); nothing applied. Open milestones: 1.2.1, 1.2.2, 1.3.0 (empty), 1.4.0, 1.5.0, vNext, Maintenance. Collected: exactly the 10 named issues are open without a milestone (plus PR #1669, skipped). None reproduced; severity is judged from the reported behavior.

## Milestones per group

| Group | Milestone |
|---|---|
| Critical bug | 1.2.1 |
| Minor bug | 1.2.1 when it fits a pending 1.2.1 run, else 1.2.2 |
| Extension | vNext |
| Maintenance | Maintenance |

## Issues

| # | Title | Group | Reason | Labels to add | Milestone | Run |
|---|---|---|---|---|---|---|
| [#1678](https://github.com/thoenissen/Reihitsu/issues/1678) | RH7109/RH7103/RH7309: partial part with a layout attribute only in excluded code (output question) | Critical bug | Reordering instance fields changes the type's layout in another build (program meaning, no frequency exception); output question A (accept recorded gap) vs B (withhold for every partial type = refusal on common code, maintainer's). | — | 1.2.1 | P11 (other-build safety; follow-up of #1667, closed in P14) |
| [#1676](https://github.com/thoenissen/Reihitsu/issues/1676) | Casing fixes rewrite call sites to keep binding (output question) | Minor bug | Rewrite preserves behavior and compiles; only SHARED-FIX-006 scope (edit beyond the diagnostic) for a rare collision class. | — | 1.2.2 ? | Q5 (naming analyzers/casing fixes) |
| [#1680](https://github.com/thoenissen/Reihitsu/issues/1680) | Documentation analyzers report positional record / primary-ctor type twice (unconfirmed) | Minor bug | Duplicate diagnostic of a real violation, not a false positive on correct code; derived from reading code (only RH8305/RH8307 use `IsPrimaryConstructorRun`). | — | 1.2.2 ? | Q4 (documentation analyzers) |
| [#1687](https://github.com/thoenissen/Reihitsu/issues/1687) | Fixes that format a member leave a sibling on the shared line | Minor bug | Layout outcome only; input already violates RH5417, so it is rare and may end as a recorded gap. | — | 1.2.1 ? | P5 (attribute-placement fix `TargetAttributePlacementCodeFixProviderBase`; fallback 1.2.2 new) |
| [#1681](https://github.com/thoenissen/Reihitsu/issues/1681) | RH8307: element in `/** */` reported with no fix (output question) | Extension | Rule would report less; RH8307-001/-006 leave `/** */` reporting explicitly undecided, so not a spec defect. | — | vNext | — (could ride Q4 if the maintainer answers B early) |
| [#1685](https://github.com/thoenissen/Reihitsu/issues/1685) | Wrapped `when` condition indented one level deeper than `case` (output question) | Extension | Changes formatter output for a case FMT-INDENT-021 does not cover; no code damaged. | — | vNext | — |
| [#1689](https://github.com/thoenissen/Reihitsu/issues/1689) | RH7004 fix for moved statements with multi-line literal / `/* */` | Extension | Offers a fix where it is deliberately withheld today (maintainer-decided deferral). | — | vNext | — |
| [#1682](https://github.com/thoenissen/Reihitsu/issues/1682) | RH8307 analyzer and documentation phase share one predicate | Maintenance | Refactor with no behavior change; both copies agree today. | — | Maintenance | — |
| [#1684](https://github.com/thoenissen/Reihitsu/issues/1684) | Statement blank-line fixes/RH5103: column from the formatter, not predicted | Maintenance | Internal design/refactor; may close if impossible without #1605's layout-only mode (vNext). | — | Maintenance | — |
| [#1690](https://github.com/thoenissen/Reihitsu/issues/1690) | SHARED-FIX-003 vs FMT-INDENT-007: later lines of `/* */` (clarification) | Maintenance | Spec wording only; no output depends on it today. | — | Maintenance | — |

Labels: every issue already carries its group label and surface label (`bug`/`enhancement`/`maintenance` + `analyzer`/`formatter`/`documentation`); nothing to add, nothing removed. No issue is unreachable: all cases arise through an analyzer, a code fix, or the CLI. No proposal to declare a shape unsupported is among them (#1678 option B is a refusal on common code, flagged as the maintainer's output decision; it is not a declare-unsupported shape, because the precheck is local to one input and cannot see other parts' excluded text).

Answer format: `ok` · `ok, minor bugs → 1.2.1` · `ok, #1687 → 1.2.2, #1681 Q4`.

## #1140 remainder

State: PR #1686 (P6, merged) fixed RH5204, RH6023/RH6024, RH5417/RH3202 shared line; #1140 stays in 1.2.1 with RH5101/RH5102 list content, RH5110, RH5111, RH5112/RH5201 chain fallback, RH5113, RH5206, RH5303–RH5308, RH5401, RH5403–RH5408, RH5412–RH5416, RH3201, ~15 rule requirements to align, the 2026-11-07 region blank-line instances, and two open output questions (RH5112 chain in `if`, RH5101 `1+2`). Its own "Structural cause": a fix scopes only by node choice; anything narrower needs column prediction (reverted in P6) or a new layout-only entry point (>100 lines, new type = escalation 9; #1605 vNext, #1521 vNext, #1684).

Proposal:
- **A (recommended)** — move #1140 to **vNext**, blocked by #1605 (layout-only node entry point), with its two output questions asked when #1605 is decided; the brace-scope instances RH5405–RH5407 stay with #1416 (P5), RH5403 added there as a note. Reason: every remaining instance is a layout outcome (minor, no code damage), and no pending run can fix it without the machinery *Special cases do not grow the source* forbids; keeping it in 1.2.1 blocks the release on a design question.
- B — move to 1.2.2 as its own run limited to cheap node-narrowing (format a smaller node where that node alone suffices, e.g. RH5206 arms, RH5110 clause) plus requirement alignment; the rest stays as on `main`. Pick B if the maintainer wants the per-rule requirement texts aligned before vNext.
- Not recommended: keep in 1.2.1 as its own run (P6 already showed the narrow route needs prediction).

## P4 escalation-10 note (multi-line `/* */` tail)

- Plan (`phase5b-unsupported-impact.md` §4.2, decided 2026-11-09 "P4 raises escalation 10 itself"): P4 should ask whether "a token behind the closing line of a multi-line delimited comment" is declared unsupported.
- Evidence it was raised: **none found.** PR #1683 body, issue comments, review comments (0) and reviews (0) do not mention it; the squash merge dropped `sdd/work/<slug>/`; no change doc in `sdd/changes/2026-11`; `unsupported-syntax.md` still declares only FMT-UNSUP-004; `state.md` records no answer. (Chat-only answers are invisible to me — confirm with the maintainer.)
- What happened instead: P4 handled the shape locally — RH3208 moves `L();` below the comment; statement fixes withhold where the target line starts with a comment tail (SHARED-STMT-004). P6 (#1686) handled #1561 in the RH5204 analyzer (token behind the tail does not start its line). Both apply maintainer principle **P33** (round 9, 2026-10-22: token behind a multi-line comment's tail does not start its line; formatter breaks before it, rules report), now in FMT-BREAK-006/-033 and SHARED-COMMENT-012. The shape is thus specified as supported; declaring it unsupported would reverse P33, which is likely why it was not raised.
- **#1690 is not that follow-up**: it reconciles whether the *later lines* of a `/* */` comment are layout or text (SHARED-FIX-003 vs FMT-INDENT-007), not the token behind the tail; Maintenance/docs.
- **Q7 (#1642) inherits**: no escalation answer; treat as an ordinary horizontal-spacing bug under P33/SHARED-COMMENT-012 (the gap behind `*/` is on one line, so FMT-SPACE keeps one space as behind a single-line comment); a cheap fix in `HorizontalSpacingPhase` (`AreSeparatedByEndOfLine` treating the comment's inner line break as a gap line break). Its inputs (comment inside a qualified name/type argument/parameter list) are rare, so if the fix needs more than that cheap check the lead leaves it as written; escalation 10 stays available but is not inherited as decided. P5 (#1388 RH5405 `/* one⏎ two */`) inherits the same.

## Doubts / duplicates

- #1678 is the residual of #1667 (closed in P14); its option A only records an already-written gap (SHARED-ORDER-008), so if the maintainer picks A it is a docs-only close inside P11. Related: #1668 (vNext, refuse any conditional block).
- #1681 and #1682 both touch RH8307 and #1680 the documentation analyzers; all three could ride Q4 if the maintainer prefers one documentation run (only #1680 is proposed there).
- #1684 overlaps #1140's structural cause and #1605; if #1140 goes to vNext (A), #1684 likely closes with the same reason.
- #1687 overlaps #1140 (P9 shared-line family); P6 fixed RH5417/RH3202, #1687 is the attribute-placement/whole-member remainder. If P5's lead finds it outside its scope, 1.2.2 as a new single-issue run.
- #1676: unreached by any other run; Q5 contains #1071 #1268 #1289 #974, casing-fix base fits.
