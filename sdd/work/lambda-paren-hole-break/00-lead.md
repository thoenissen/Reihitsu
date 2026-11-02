Role: lead
Commit: ae55f3e
Verdict: run started

# Run

- Issue: thoenissen/Reihitsu#1494 — `agent-filed` (goals are proposals). Package C8 of thoenissen/Reihitsu#1482; its blocker thoenissen/Reihitsu#1488 is closed (merged in #1513).
- Slug: `lambda-paren-hole-break`
- Branch: `claude/ecstatic-shannon-j9cmkw` (session-designated branch; see Decisions taken #1)
- Draft PR: thoenissen/Reihitsu#1565
- Lane: **behavior change** — the issue asks for changed formatter behavior (a hole that stays on one line, a collapse that no longer touches parenthesized lambdas) and requires a change doc under `sdd/changes/`.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing when follow-up candidates exist. None skipped.
- The issue was written against `main` at `735d0dba`; every statement about today's code must be checked against current `main` first (issue section "Before the run"). Statements already made true or moot are noted and dropped.

## Maintainer answers

1. Anonymous method in a hole of a multi-line raw string literal — **B**: break it and align it (braces under the anonymous method's first token, as a block lambda in a hole), lines move with the literal; not kept on one line. No analyzer changes.
2. Blank line in front of a parenthesized lambda that starts its line (after `,` in an argument list, between array-initializer elements, between collection-expression elements, after an expression lambda's `=>`, after `return`) — **A**: kept; columns unchanged.
3. Lambda starting a hole line no alignment places — **A**: keeps its column (FMT-RAW-007).
4. `static` anonymous method in such a hole — **B**: the block aligns under `delegate` (not under `static`), consistent with a `static` block lambda aligning at its `(`.
5. Lambda elements in collection initializers and wrapped collection expressions (change doc entry 7) — **A**: laid out like any other element (one per line, as FMT-BREAK-021/022 give for `(1)`).
6. `async` anonymous method in such a hole — **B**: the block aligns under `async`, as an `async` block lambda does (FMT-INDENT-028). Corrects the lead's earlier routing note, which had extended answer 4 to `async` without asking.
- General clarification from the maintainer: inside interpolation holes the same formatting applies as everywhere else; a hole is not a place with its own layout rules.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 14 | Spec author (23) | Is change-doc entry 9 (first-pass block placement on unmoved hole lines) an output question? | No: recorded as a visible change, not asked | `main`'s output there is not idempotent; the new first pass equals `main`'s second pass, which unchanged FMT-INDENT-028/030/031 demand | A bug fix back to unchanged requirements puts nothing in question | 5a5f594 |
| 13 | Auditor (18) | Architecture finding: hole-line columns are stored before the raw-string move and floored at 0 in three places, two before the move | Change the structure in this run: the 0 floor applies once, to the column after the move; `LayoutModel`, `IndentationRewriter`, `RawStringLineShifts` are the run's component and its counterparts | A narrow anonymous-method-only fix would keep the pre-move floor and leave block lambdas broken | Outputs are the pass-2 fixed points FMT-INDENT-028/031 already demand | — |
| 12 | Auditor (18) | Finding #2: anchor on a hole line that does not move (end line of a `/* */` comment, verbatim continuation line) with column < shift is not idempotent (safety rail: idempotency) | Repair round 2: fix the mechanism for anonymous methods and block lambdas alike; tests for both members, moved and aligned literal, anchor column equal to the shift and one less | Same requirement (FMT-INDENT-031: anchor measured after the move); the run exposes it and must not ship an idempotency break | Block-lambda output becomes the one FMT-INDENT-028 already demands (unchanged requirement) | — |
| 11 | Auditor (12) | Architecture finding: the FMT-INDENT-031 guard re-derives from syntax which raw hole a line belongs to, while `LayoutComputer`/`LayoutModel` hole-line registration decides it with a different span | Change the structure in this run: the guard and the registration share one statement of "the multi-line raw hole that owns this line" | The narrow fix would keep a second statement of the rule; the change stays within components the run changes | Same output as finding #1's repair | — |
| 10 | Auditor (12) | Finding #1: an anonymous method in a regular/verbatim/single-line raw string nested in a multi-line raw hole is not aligned (column depends on where the literal stood) | Repair round 1, option (a): align it like its block-lambda counterpart (which is already aligned there); narrow the spec's single-line sentence to non-nested single-line raw literals | Same requirement; the output is covered by maintainer answer 1 and the clarification that holes format like everywhere else, and the block-lambda counterpart already aligns there (answers 4/6 rule) | Covered by maintainer answers 1, 4, 6 and the clarification | — |
| 9 | Spec author | Change doc status `Draft` → `Accepted` | Confirmed | Every visible output change has a maintainer answer (1–6) | Status field only | 59c1536 |
| 8 | Implementer / test implementer | Unused `PhaseReversalCauses.LineStartIndentation` after the exception list emptied | Revised: keep the cause unchanged (the class must keep a member to build — S2094; deleting the class empties the known-cause checks, and a suppression is not wanted) | Removal costs more than it saves | Test helper only | 4586769 |
| 7 | Implementer | `ConstraintClauseInHoleOf{Aligned,Misaligned}LiteralIsIndentedFromItsDeclaration*` expect the anonymous method's block at column 17 | Re-derive expectations to the FMT-INDENT-031 layout (brace under `delegate`); test implementer edits, change doc lists both tests as changing | Asserted output is decided by maintainer answer 1 and FMT-INDENT-031; the tests' own point (the `where` clause indentation) stays | Covered by maintainer answer 1 | — |
| 6 | Test implementer | Columns behind `=>`/`return` (entry 5) | Assert only the blank line; column left to follow-up candidate 3 | No requirement decides that column | No column asserted or changed | c269f57 |
| 5 | Test implementer | Who removes stale `PhaseReversalExceptionList` entries | The implementer, once its change makes them stale (the list's own test reports them) | Removal is required by the list's design and the issue's Done-when, weakens no assertion | Test helper data only | — |
| 4 | Spec author | Accept the change doc `sdd/changes/2026-11/lambda-paren-hole-break.md` | Accepted after skeptic and amendments for answers 1–6 | Visible output changes are all answered | All changes covered by maintainer answers | 8bcb6de |
| 3 | Spec author | Anchor of `static async delegate` | Under `async`, like its block-lambda counterpart | Follows the rule of answers 4 and 6 (an anonymous method aligns like its block-lambda counterpart) | Covered by the maintainer's answers 4/6 rule | 8bcb6de |
| 2 | Skeptic | Phase-stability check test loses its lambda example | Replace with an injected (synthetic) phase, not another formatter defect | The check's own test must not depend on a defect surviving | Test-only | — |
| 1 | Claim | Branch name `claude/issue-1494-<slug>` per `squad/lead.md` vs. the session's designated branch | Use `claude/ecstatic-shannon-j9cmkw` | The session environment pins development and pushes to that branch | Branch names are internal | ae55f3e |

## Follow-up candidates

1. Anonymous method in a hole of a regular interpolated string below C# 11 produces CS8967 (safety rail; caller path unverified) — 01-spec-author.md, 02-skeptic.md.
2. Parenthesized lambda behind a modifier or return type on the next line (`static⏎(int x) => x`) lands in the statement's column — 01-spec-author.md, 02-skeptic.md.
4. Switch-expression arm after a multi-line arm stays on the brace line (`}, _ => null`) — pre-existing — 12-auditor.md.
5. Tab in a raw literal's content prefix may count as one column for hole alignment — uncertain, verify first — 12-auditor.md.
3. Value continued on the line after `return` or a lambda's `=>` starts in the statement's column — 02-skeptic.md (file only if no FMT-INDENT requirement decides it); same class per 12-auditor.md: a value continued after the `(` of a parenthesized expression or after a switch arm's `=>`.
