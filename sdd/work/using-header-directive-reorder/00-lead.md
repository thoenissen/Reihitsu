# Lead — using-header-directive-reorder

- Issue: thoenissen/Reihitsu#1971 — `agent-filed` (goals are proposals).
- Slug: `using-header-directive-reorder`
- Branch: `claude/intelligent-babbage-pxnhds` (session-designated); PR thoenissen/Reihitsu#1996 (draft claim).
- Lane: `behavior-change` — the issue requests changed code-fix and formatter behavior (RH7201–RH7207 fixes, Fix All, and the formatter's using phase reorder a block whose every directive stands before its first using); a change doc is required.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist. None skipped.
- Prerequisite: the maintainer ordered this run after #1972 (merged as #1991, `13b0ed2`); satisfied at claim.

## Maintainer answers

1. Output question in the issue (answered in the issue comment before the run): option A. When every preprocessor directive of a using block stands in front of its first using (e.g. a `#nullable enable` header), the RH7201–RH7207 fixes, Fix All, and the formatter reorder the block and the directive stays on top. A block with a directive between or after its usings stays reported without a fix. The narrowed guard must stay a cheap syntactic check. SHARED-USING-009 lives in `sdd/specs/shared/using-directive-ordering.md`, not `code-fixes.md`.

2. Batch 1 (questions 1–6, in the chat): 1A — RH7207 reports a header-led sorted block without its group blank line, fix and formatter add it. 2A — comment header, blank line, `#nullable enable`: fix and formatter reorder, header and directive stay on top. 5B — a conditional group (`#if`…`#endif`, disabled text) in front of the block: stays reported without a fix, the formatter keeps the block ("don't make it complicated"). 6A — a directive inside a using block is not declared unsupported. Questions 3 and 4 not yet decided: for 3 the maintainer suggested a blank line above the comment so case 3 becomes case 4, and asked whether header-like comments then stay on top; asked back (3/4 follow-up).

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | skeptic | The answer's phrase "between or after its usings": does a directive behind the last using now block the fix? | Read it as "after the first using"; a directive behind the block keeps its fix (SHARED-USING-009 unchanged there) | The literal reading would remove an existing fix that no one asked to remove; the issue is about narrowing the guard | Today's output for that input stays as it is | 616e3b6 |
| 2 | skeptic | Case 3 (#region around the block) needs asking? | No: it is case 1 with the directive on top, and region framing is unchanged FMT-BLANK-013 | Single reading under answer 1 | Covered by answer 1 and unchanged requirements | 616e3b6 |
| 3 | skeptic | Internal wording suggestions ("that using directive" in FMT-USING-012, "among its directives" in RHxxxx-001, an FMT-USING-012 example for directive + blank line) | Take all three; route to spec author with the answers | Clarity, no output effect | Spec wording only | pending |

## Follow-up candidates

(none yet)

## Worklist / next step

- Spec author 01 done (616e3b6), skeptic 02 done (d50eb50): verdict output question.
- Batch 1 answered except 3/4; asked back.
- Was: waiting for the maintainer on batch 1 (questions 1–6: cases 2, 4, 5, 6, 7 and escalation 10 on declaring directives inside a using block unsupported).
- Next: route answers + decision 3 to the spec author (continue, a86743313ec3ecb5c, or fresh), then status Accepted, then test implementer.
