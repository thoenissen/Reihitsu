# Lead — conditional-element-access-list

- Issues: thoenissen/Reihitsu#1877 (`agent-filed`), thoenissen/Reihitsu#1878 (`agent-filed`). Both goals are proposals.
- Slug: `conditional-element-access-list`
- Branch: `claude/pensive-carson-bh605t` (session-designated). Draft PR: #1897.
- Lane: **behavior change** for the run.
  - #1877: requested changed formatter output (join the wrapped argument list of a conditional element access `c?[1,⏎2]`), with a spec clarification of FMT-BREAK-014 / FMT-BREAK-040; carries an output question.
  - #1878: requested changed analyzer behavior (RH5307 reports and fixes `c?[...]` like `c[...]`), removes a stated exclusion from spec RH5307-001 and the rule page.
  - Shared mechanism: the bracketed argument list of an element binding expression (`?[`); the RH5307 fix formats through the formatter, so #1878's fix depends on #1877's join (maintainer's triage note on #1877).
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist. None skipped.
- Maintainer clarifications (summarized): during triage the maintainer placed both issues in one run, because the RH5307 fix formats the list through the formatter and needs the join; earlier direction (on #1869) — `?` and `!` in front of index brackets must not behave differently from a plain element access. The output question stated in #1877 is to be asked in the run.

## Maintainer answers

- Q1 (formatter, wrapped list of `c?[...]`): **A** — join it (`c?[1, 2]`); code fixes formatting such a node join it too; the existing regression test's expected output becomes `a?[b[1, 2], 3]`.
- Q2 (RH5307 on `c?[...]`): **A** — reported on the brackets (from `[` to `]`), fixed to `c?[1, 2]`; `e?[0,⏎1][1,⏎2]` gets two diagnostics and fixes to `e?[0, 1][1, 2]`; rule page drops the exclusion sentence.
- Maintainer's words: "As recommended" (to both).

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Skeptic | Architecture finding: which bracketed lists hold an index is stated twice (formatter `LineBreakListRewriter.IsElementAccessArgumentList`, analyzer `RH5307...IsIndexerArgumentList`), both with the same omission | Change the structure in this run: one shared predicate in an existing `Reihitsu.Core` utility class, used by both; no new type | Narrow fix would keep a second statement of the rule that already drifted; few lines, within the components the run changes and Core | Same outputs as the narrow fix | — |
| 2 | Skeptic | RH5307-001 restates FMT-BREAK-014's list of parent kinds | Have RH5307-001 refer to FMT-BREAK-014 instead (spec author, with the Accepted status follow-up) | One statement of the rule | Spec wording only | — |
| 3 | Skeptic | Characterization test for fixed-size buffer `fixed int b[⏎3];` | Test implementer adds it | Pins the excluded fourth parent | Characterizes `main` output, unchanged | — |
| 5 | Test implementer | `FormatNode` on a bracketed list inside a tree misplaces the column even for `d[...]` | Dropped, not a follow-up | No caller formats such a list through `FormatNode` (RH5307 fix uses `FormatNodeInDocumentAsync`) — AGENTS.md *Supported inputs* | No caller reaches it | — |
| 6 | Implementer | Two RH5307 tests use `VerifyCodeFixWithEndOfLine` whose single-application assertion contradicts RH5307-004/SHARED-FIX-006 on multi-diagnostic sources | Test is wrong; test implementer switches them to the multi-diagnostic verify form, final text unchanged | Unchanged requirements decide it | Expected fixed text unchanged | — |
| 7 | Implementer | `VerifyFixAndIndexAccessFixConvergeInEitherOrder` expects RH5115 on `[`; RH5115 reports on `?` (existing, untouched behavior) | Test is wrong; marker moves to the `?` | Unchanged RH5115 behavior on `main` decides it | RH5115 output unchanged by the run | — |
| 8 | Implementer | New public `SyntaxNodeUtilities.IsIndexArgumentList` has no direct Core test | Test implementer adds a small Core.Test test (four parent kinds incl. fixed buffer = false) | Core tests cover the shared utility assembly directly | Internal | — |
| 4 | Skeptic | #1877's P11 "RH5307 stays silent" rationale is stale | Drop it; PR states the difference | Superseded by #1878 | Rationale only | — |

## Follow-up candidates

(none yet)

## State

- Last step: spec author 01 (commit 196829de, change doc Draft; both premises reproduce; output questions 1 and 2 drafted, spec written on 1A/2A). Lane check confirmed.
- Skeptic 02 (7f8e64fa): both premises real, agrees with 1A/2A, no new output questions, no follow-ups.
- Output questions answered 1A, 2A (spec already written on 1A/2A).
- Spec author 03 (96c005be): answers recorded, change doc Accepted, decision 2 done.
- Test implementer 04 (44d72441): 18 formatter + 34 analyzer rows fail as expected.
- Implementer 05 (938ad38c code, 6f877378 handover): formatter green; 6 analyzer rows disputed (decisions 6, 7); decision 1 done.
- Test implementer 06 (10dce053): decisions 6, 7, 8 done; RH5307 84 passed, Core 5 passed.
- Cleaner 07 (9155c6cb): clean, Release 0 warnings.
- Spec checker 08 (d7f0d6e2): one finding (change doc line ~142 still calls the location question open) → repair round 1, owner spec author.
- Now: spec author (09) repair; then spec checker (10), auditor (11), validator.
