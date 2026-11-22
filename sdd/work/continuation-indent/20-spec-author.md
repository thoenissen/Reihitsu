Role: spec author
Commit: 9f1eb7d423de2ea0ee13f2db7bc9d1abe3db7986
Verdict: text written

Merge of `origin/main` into `claude/squad-issues-953-1852-439khw` (merge commit `9f1eb7d`). Two conflicts, both resolved by keeping both sides' behavior; no requirement text was rewritten.

## Conflict resolution

- `AGENTS.md`, phase-ownership row `IndentationPhase`: kept this run's text (query-clause alignment, the continuation indent of a line no alignment places, owners `ContinuationLineContributor` with `LayoutModel.SetContinuation` and `QueryClauseContributor`) and added main's raw-string clause ("with the indentation the compiler strips from a tab-indented raw string written as spaces, one per character", FMT-RAW-010) in the position main gave it.
- `sdd/specs/formatter/indentation.md`, header table: kept this run's code owners and its sorted `Uses` row, and added main's `FMT-RAW-010` after `FMT-RAW-007`. Main's other change to this spec (FMT-INDENT-011 naming FMT-RAW-010 as the only rewritten raw-string whitespace) auto-merged and does not touch this run's requirements FMT-INDENT-034 and FMT-INDENT-035.

## Requirement IDs

`sdd/specs/requirement-ids.txt` auto-merged: main adds CLI-024, FMT-RAW-010, RH5202-007, RH5202-008; this run adds FMT-BREAK-041, FMT-INDENT-034, FMT-INDENT-035, FMT-UNSUP-017 to FMT-UNSUP-019. No duplicates, ordinal (C-locale) order holds. `sort -V` flags `RH7305A-001`, which is the same on `origin/main` and not from this merge.

`scripts/test.sh --project architecture`: 198 passed, 0 failed.

## Changed requirements
_None._ (merge resolution only)

## Examples for the tests
_None._

## Output questions
_None._

## Internal choices
_None._

## Follow-up candidates
_None._
