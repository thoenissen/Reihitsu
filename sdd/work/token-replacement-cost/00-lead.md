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

## Follow-up candidates

- Per-token `GetPreviousToken()` calls grow ~4× per doubling (4.1 → 14.4 → 55.0 ms); several phases call it per token or per comment; may explain the remaining superlinear growth of other phases and of `LayoutComputer.Compute`. Must measure and attribute first. (01-skeptic.md)
- Remaining superlinear growth of `LayoutComputer.Compute` (35–40 → 124–146 ms N=800→1600) and of the other phases at large N, incl. blank-line/line-break/horizontal-spacing on a large array initializer (`LineBreakListRewriter.cs:526`); cause not established. (01-skeptic.md) — likely to be merged with the item above by the issue manager.

## Audited SHA

—

## Open worklist / next step

- Skeptic done (f65198d): proceed narrower, no output questions, no architecture findings.
- Test implementer done (f75732b): reproduced by measurement (probe in scratchpad), 16 equivalence tests green on unchanged code.
- Next: implementer.
