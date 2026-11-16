# Lead — arglist-below-name

- Issue: thoenissen/Reihitsu#1786 — `agent-filed` (goals are proposals). Labels: enhancement, analyzer, formatter.
- Slug: `arglist-below-name`
- Branch: `claude/eloquent-dijkstra-d4xuqf` (session-designated); draft PR #1813.
- Lane: **behavior change** — the issue requests changed formatter output (an argument list whose `(` stands on the line after the invoked name, including the collection-expression `with(...)` element), posed as an output question; it is not an observed-versus-expected report against a spec.
- Roles: spec author → skeptic → (output questions) → test implementer → implementer → cleaner → spec checker → auditor → validator. None skipped.
- Maintainer clarifications: none beyond the issue.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

- F1 (01-spec-author): element access outside a chain (`arr⏎[0]`) not joined.
- F2 (01-spec-author): continuation line no indentation requirement covers gets different columns from CLI vs. code fix (`[() =>⏎1, () => 2]`); related #953.
- F3 (01-spec-author): candidate new analyzer rule for an argument list's `(` below its name (P11).

## State

- Last step: spec author (01, aecc867), change doc Draft.
- Next: skeptic, then output-question batch.
