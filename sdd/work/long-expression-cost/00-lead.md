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
| 1 | Skeptic | Issue goals (agent-filed) | Drop the chain and concatenation rows as already met by #1862 (now linear); restate "slowest phase" per defect; widen the array row to every long sibling list (initializers, collection expressions, argument lists, enum members, class members, block statements); keep "output unchanged on the repository's own sources" | 01-skeptic.md measurements | Output stays byte-identical; a cost change only | 72f3885 |
| 2 | Skeptic | Scope of defect B (per-token previous/next-token navigation across five phases) | Fix defect A (nested conditional in `TernaryLineBreakRewriter`) and every implicated site of defect B, since fixing only the largest sites leaves the growth quadratic. It is the issue's own fix, so not escalation 9; a site that cannot be made linear with a cheap change is left as is and its remaining cost recorded per phase with the reason | Issue asks for linear cost where a cheap change allows; FC-016 | Output unchanged | — |
| 3 | Skeptic | Form of the reproduction | Scaling-ratio tests (time at N versus 2N, or an operation count), never an absolute wall-clock budget | CI contention would make a budget flaky | Tests only | — |
| 4 | Test implementer | FMT-BREAK-018 is silent on whether a parenthesized nested conditional counts as a nested conditional in a branch; `ParenthesizedChainOnOneLineStaysOnOneLine` pins today's output | No action in this run: the run keeps output byte-identical, and the characterization pins today's behavior; not a follow-up (no output in question, no defect) | 02-test-implementer.md | Output unchanged | 5304634 |
| 5 | Implementer | Scope growth: new public type `SyntaxTokenNavigationUtilities` in `Reihitsu.Core`, 47 production files, including Core helpers the analyzers share | Keep: it is the issue's own fix (not escalation 9 per `squad/README.md` item 9), every default-form navigation call on the formatter path must change or the growth stays quadratic (FC-016), and the shared Core helpers are on the formatter's path; their results are unchanged and the analyzer suite passes | 03-implementer.md | Output unchanged (6019-file before/after diff empty) | 3e94566 |

## Follow-up candidates

- (01-skeptic.md) CLI: a very deep spine (6400-link chain, 16 000 concatenation terms) overflows the stack in `LayoutComputer.ComputeBlockIndentation`, killing the CLI process (exit 134, later files unprocessed, no summary), while a deeper input (32 000 terms) is reported per file with exit 2. Visible CLI behavior, outside the run's issue: file as a follow-up carrying the output question in 01-skeptic.md (recommended there: report the file as an error and continue).

## Open worklist / next step

- Last step: cleaner (04-cleaner.md, clean, no changes), 0abf970.
- Next: spec checker (05-spec-checker.md), then auditor, validator.
