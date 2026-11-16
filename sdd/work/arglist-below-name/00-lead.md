# Lead — arglist-below-name

- Issue: thoenissen/Reihitsu#1786 — `agent-filed` (goals are proposals). Labels: enhancement, analyzer, formatter.
- Slug: `arglist-below-name`
- Branch: `claude/eloquent-dijkstra-d4xuqf` (session-designated); draft PR #1813.
- Lane: **behavior change** — the issue requests changed formatter output (an argument list whose `(` stands on the line after the invoked name, including the collection-expression `with(...)` element), posed as an output question; it is not an observed-versus-expected report against a spec.
- Roles: spec author → skeptic → (output questions) → test implementer → implementer → cleaner → spec checker → auditor → validator. None skipped.
- Maintainer clarifications: none beyond the issue.

## Maintainer answers

- OQ batch 1 (questions as in 03-spec-author.md): 1A (join; multi-line collection keeps one element per line), 2A (join constructor initializer and primary-constructor base type), 3A (conditional judged after the join → one line), 4A (dictionary element initializer keeps one element per line).

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | skeptic | `(f)⏎(1)` example is a cast and does not compile | Replace with `(f ?? g)⏎(1)` in spec, change doc, Q1 | Skeptic compiled it (CS0118) | Example text only; the output question is asked afterwards | 69fcd5f |
| 2 | skeptic | FMT-BREAK-039's second paragraph names only collection expressions; reason for Q1 A overstated | Cover every construct FMT-BREAK-022 decides (incl. dictionary element initializers); narrow the reason to collection-like constructs | Rewriter order decides these as written | Outputs asked as output questions | 69fcd5f |
| 3 | skeptic | Architecture finding: argument-list `(` gap demanded by FMT-BREAK-039, -020, -015 | FMT-BREAK-039 owns the gap; FMT-BREAK-020 and -015 point to it instead of restating it; the implementation has one owner of that gap | One statement of a rule (README, Architecture findings); within the component the run changes | Same outputs for every input; spec wording only | 69fcd5f |
| 4 | skeptic | F4 cast operand below the cast (`(int)⏎(x + 1)`) | Dropped, not a follow-up | Rare input class (AGENTS.md, Code-fix coverage) | No change made | — |
| 5 | implementer | New type `ArgumentListLineStartRewriter` as the one owner of the gap | Accept | It implements the issue's own fix and decision 3; not a structural change under escalation 9 | Internal structure | 993f700 |
| 6 | implementer | `AGENTS.md` phase-table edit made by the implementer | Keep; the spec checker verifies it | The table must name the new owner; one-line text change | Repository instructions only | 993f700 |

## Follow-up candidates

- F1 (01-spec-author): element access outside a chain (`arr⏎[0]`) not joined.
- F2 (01-spec-author): continuation line no indentation requirement covers gets different columns from CLI vs. code fix (`[() =>⏎1, () => 2]`); related #953.
- F3 (01-spec-author): candidate new analyzer rule for an argument list's `(` below its name (P11).

## State

- Last step: spec author amendment (03, 69fcd5f); change doc accepted by lead pending answers; OQ batch 1–4 (03-spec-author.md) asked to maintainer.
- Answers received: 1A 2A 3A 4A.
- Last step: spec author 04 (185f42c), change doc Accepted.
- Last step: test implementer 05 (856fbf9): 29 formatter + 12 code-fix failures expected.
- Implementer 06 (993f700, 1e1f7bb): all tests pass (Debug full run green).
- Next: cleaner (07).
