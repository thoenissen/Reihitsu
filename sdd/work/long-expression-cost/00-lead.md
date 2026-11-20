# Lead — long-expression-cost

- Issue: thoenissen/Reihitsu#1803 — "Formatter: a very long expression or list takes time that grows much faster than its length". Labels: `formatter`, `maintenance`, `agent-filed` (goals are proposals).
- Branch: `claude/gifted-fermi-xa6rlf` (designated by the session). Draft PR: thoenissen/Reihitsu#1883.
- Slug: `long-expression-cost`.
- Lane: **bug fix**. Reason: the issue reports an observed difference (formatting time grows super-linearly with the length of a chain, `?:` chain, concatenation, or list) against the expected behavior (time roughly linear in the input); the fix is expected in production code under `src/Reihitsu.Formatter/` with formatter output unchanged, so no change doc.
- Roles run: skeptic (premise and reachability), test implementer (reproduction), implementer, cleaner, spec checker, auditor, validator. Spec author: skipped unless a clarification is needed (bug-fix lane).
- Context from the issue comments (summarized): a later comment measured the single-line array initializer as quadratic through the CLI; another attributed a whole-tree token replacement in the indentation and cleanup phases, filed and fixed separately as #1862 (merged on `main` as a8ea6f3), and asked this issue to re-measure after that fix and attribute only what remains; a third comment lists unattributed candidates (per-token previous/next-token navigation in several phases, `LayoutComputer.Compute`, a replacement set in the line-break list rewriter) and asks that measurement precede any change. Output must not change.
- Maintainer clarifications: none yet.

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

None yet.

## Open worklist / next step

- Last step: claim (draft PR #1883), toolchain prepared.
- Next: skeptic on the premise (01-skeptic.md).
