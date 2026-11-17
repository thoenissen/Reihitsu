# Lead — arglist-below-name

- Issue: thoenissen/Reihitsu#1786 — `agent-filed` (goals are proposals). Labels: enhancement, analyzer, formatter.
- Slug: `arglist-below-name`
- Branch: `claude/eloquent-dijkstra-d4xuqf` (session-designated); draft PR #1813.
- Lane: **behavior change** — the issue requests changed formatter output (an argument list whose `(` stands on the line after the invoked name, including the collection-expression `with(...)` element), posed as an output question; it is not an observed-versus-expected report against a spec.
- Roles: spec author → skeptic → (output questions) → test implementer → implementer → cleaner → spec checker → auditor → validator. None skipped.
- Maintainer clarifications: none beyond the issue.

## Maintainer answers

- OQ batch 1 (questions as in 03-spec-author.md): 1A (join; multi-line collection keeps one element per line), 2A (join constructor initializer and primary-constructor base type), 3A (conditional judged after the join → one line), 4A (dictionary element initializer keeps one element per line).
- OQ 5 (audit 11 finding 1): 5A — an argument list whose only line break was an argument-list gap inside an interpolation hole, later query clause, or array/stackalloc initializer is laid out as it reads after the join (one line), for chain and non-chain members alike.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | skeptic | `(f)⏎(1)` example is a cast and does not compile | Replace with `(f ?? g)⏎(1)` in spec, change doc, Q1 | Skeptic compiled it (CS0118) | Example text only; the output question is asked afterwards | 69fcd5f |
| 2 | skeptic | FMT-BREAK-039's second paragraph names only collection expressions; reason for Q1 A overstated | Cover every construct FMT-BREAK-022 decides (incl. dictionary element initializers); narrow the reason to collection-like constructs | Rewriter order decides these as written | Outputs asked as output questions | 69fcd5f |
| 3 | skeptic | Architecture finding: argument-list `(` gap demanded by FMT-BREAK-039, -020, -015 | FMT-BREAK-039 owns the gap; FMT-BREAK-020 and -015 point to it instead of restating it; the implementation has one owner of that gap | One statement of a rule (README, Architecture findings); within the component the run changes | Same outputs for every input; spec wording only | 69fcd5f |
| 4 | skeptic | F4 cast operand below the cast (`(int)⏎(x + 1)`) | Dropped, not a follow-up | Rare input class (AGENTS.md, Code-fix coverage) | No change made | — |
| 5 | implementer | New type `ArgumentListLineStartRewriter` as the one owner of the gap | Accept | It implements the issue's own fix and decision 3; not a structural change under escalation 9 | Internal structure | 993f700 |
| 6 | implementer | `AGENTS.md` phase-table edit made by the implementer | Keep; the spec checker verifies it | The table must name the new owner; one-line text change | Repository instructions only | 993f700 |
| 7 | spec checker | Repair round 1: `ArgumentListLineStartRewriter` missing from code owners (line-breaks.md header, specs/README.md) | Route to spec author; skip the cleaner for this text-only repair, re-run spec checker | Same requirement, PR-introduced | Spec metadata only | ba51961 |
| 8 | audit repair | Other FMT-BREAK-020 joins inside nested constructs (`Baz($"{x.⏎ToString()}", 2)`) still run after the list layout (12-spec-author) | Dropped, not a follow-up | Rare input class (line break inside a member access inside an interpolation hole inside an argument) | Not changed by the run; main's output kept | — |

## Follow-up candidates

- F1 (01-spec-author): element access outside a chain (`arr⏎[0]`) not joined.
- F2 (01-spec-author): continuation line no indentation requirement covers gets different columns from CLI vs. code fix (`[() =>⏎1, () => 2]`); related #953.
- F5 (07-cleaner): `scripts/format.sh` build prints S1125 warnings from `src/Reihitsu.Cli/FormatCommandHandler.cs` (pre-existing on main; Release build clean).
- F3 (01-spec-author): candidate new analyzer rule for an argument list's `(` below its name (P11).

## State

- Last step: spec author amendment (03, 69fcd5f); change doc accepted by lead pending answers; OQ batch 1–4 (03-spec-author.md) asked to maintainer.
- Answers received: 1A 2A 3A 4A.
- Last step: spec author 04 (185f42c), change doc Accepted.
- Last step: test implementer 05 (856fbf9): 29 formatter + 12 code-fix failures expected.
- Implementer 06 (993f700, 1e1f7bb): all tests pass (Debug full run green).
- Cleaner 07 (8154f04): clean.
- Spec checker 08 (b71bd82): 1 finding (code owners).
- Spec author 09 (ba51961): code owners listed.
- Spec checker 10 (a4e0856): consistent. main up to date; Release build green.
- Auditor 11 spawned on head a4e0856, main eb89003.
- Auditor 11 (86552ab): 1 finding, undecided output (list collapsing to one line when the only break was an argument-list gap inside an interpolation hole / query clause / array or stackalloc initializer). Asked as OQ 5 (A one line = branch, B one argument per line = main for chain member).
- Answer: 5A. Repair round 1 (audit): spec author 12 (d8c5fb6), test implementer 13 (e23344a, tests pass, no implementer needed). Merged origin/main 1102aa5 (773d38a; test conflicts resolved by test implementer 14).
- Cleaner 15 clean, spec checker 16 consistent; Release build green; auditor 17 spawned on head 1287283, main 1102aa5.
- Auditor 17 (3439a34): pass, text cleanup (finding 2: FMT-BREAK-039 Why wording; "query clause after the first" in change doc). Audited SHA 1287283.
- Next: spec author 18 text fix, verify-text-only vs 1287283; then validator, issue manager (F1, F2, F3, F5), note, publish.
