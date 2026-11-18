# Lead — token-replacement-cost

- Issue: thoenissen/Reihitsu#1862 — `agent-filed` (goals are proposals). Milestone 1.5.0.
- Slug: `token-replacement-cost`
- Branch: `claude/compassionate-brown-quwcgr` (session-designated); draft PR thoenissen/Reihitsu#1872.
- Lane: **bug fix**. The issue reports an observed cost (whole-file token replacement in the indentation and cleanup phases grows with the square of the token count) against the expectation that formatting time grows about linearly, with output unchanged. The change touches production formatter code only, no visible output and no change doc.
- Roles run: skeptic (premise, reachability) → test implementer (reproduction) → implementer → cleaner → spec checker → auditor → validator. Spec author skipped: bug-fix lane, only for a clarification.
- Maintainer clarifications: the triage comment on the issue places it in 1.5.0 as the hot spot of the fixed-corpus benchmark; another issue's measurement is re-taken after this run merges. Nothing else.

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Skeptic | Goal "indentation and cleanup phases grow linearly" cannot be met by the named mechanism (`LayoutComputer.Compute` alone grows ~3.1–3.7× per doubling) | Narrow the goal: the whole-tree token replacement in both phases becomes linear, and both phases lose its share; the remaining superlinear growth of the layout computation is a follow-up. PR links `Closes #1862` (purpose met: the measured hot spot removed) and states the difference | Skeptic measurement (01-skeptic.md); `agent-filed` goals are proposals | Performance only; formatter output stays byte-identical | — |
| 2 | Skeptic | How to test a performance fix | No wall-clock or ratio assertion in tests (flaky; a whole-phase ratio would fail after a correct fix). Tests pin byte-identical output of both phases on inputs where they actually rewrite (the input list in 01-skeptic.md); the before/after timing is measured and reported in handovers and the PR | Skeptic recommendation 1 | Tests assert today's output unchanged | — |
| 3 | Skeptic | Sibling sites (tab replacement, line endings, doc-comment phase) | Make them linear only if the hot-site fix yields a shared helper at no extra cost; otherwise the PR states each site's measured cost and why it stays | Skeptic recommendation 2; issue's own wording | No output change | — |
| 4 | Implementer | New internal type `TokenRewriter` (one-walk token rewrite, three consumers) | Accept; it is the issue's own fix, not a structural change under escalation 9 | README escalation 9: code implementing the issue's own fix is not escalated | Internal type; output unchanged | 06d658a |
| 5 | Implementer | Tab-replacement site (`TabReplacementPhase.cs:39`) changed under row 3 | Accept, and send the test implementer back (fresh agent) for whole-file tab-replacement equivalence tests, as its handover asked | Row 3; 02-test-implementer.md | Output unchanged | 06d658a |

## Follow-up candidates

- Per-token `GetPreviousToken()` calls grow ~4× per doubling (4.1 → 14.4 → 55.0 ms); several phases call it per token or per comment; may explain the remaining superlinear growth of other phases and of `LayoutComputer.Compute`. Must measure and attribute first. (01-skeptic.md)
- Remaining superlinear growth of `LayoutComputer.Compute` (35–40 → 124–146 ms N=800→1600) and of the other phases at large N, incl. blank-line/line-break/horizontal-spacing on a large array initializer (`LineBreakListRewriter.cs:526`); cause not established. (01-skeptic.md) — likely to be merged with the item above by the issue manager.
- `RH5604CodeMustNotContainMixedLineEndingsCodeFixProvider.ApplyCodeFixAsync` (`src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5604CodeMustNotContainMixedLineEndingsCodeFixProvider.cs:41-66`) is a private copy of `LineEndingNormalizationPhase.Execute`: one rule stated in two places (FC-015, pre-existing, untouched by this run). (07-auditor.md)

## Audited SHA

6db7bcc (base origin/main 6d574e1)

## Open worklist / next step

- Skeptic done (f65198d): proceed narrower, no output questions, no architecture findings.
- Test implementer done (f75732b): reproduced by measurement (probe in scratchpad), 16 equivalence tests green on unchanged code.
- Implementer done (06d658a, handover 76d7748): tests pass; indentation ≈1200→243 ms, cleanup ≈935→29 ms at N=1600; line-ending and doc-comment sites unchanged with reasons.
- Tab tests done (0f331bc, 04-test-implementer.md): 11 tests, green before and after.
- Cleaner done (8fa0e97): clean, no changes, Release 0 warnings.
- Spec checker done (6e06438): consistent, no findings.
- Auditor done (c437b5d): pass, text cleanup — finding #1 comment at `TabReplacementPhase.cs:24-26` (FC-031). Output-equivalence across 11736 perturbed files confirmed.
- PR text hints: line-ending site is ~14 % (≈119 ms of ≈830 ms at N=1600, mixed variant) after the fix and stays because mixed line endings are rare (RH5604 reports them); `TabReplacementPhase.cs:150` set is bounded to one directive.
- Next: cleaner fixes the comment, proven by verify-text-only vs 6db7bcc; issue manager in parallel; then validator, publish.
