Role: spec author
Commit: 58b304d
Verdict: spec written

Repair round 1: fixes both findings of `05-spec-checker.md`. The prompt carried no intended fix, cause, or hoped-for verdict beyond the findings.

## Changed requirements

- No requirement text changed. Header rows only:
  - `sdd/specs/analyzer/RH3002.md`, `Uses`: `—` → `SHARED-DIAG-008` (referenced by RH3002-009 and RH3002-010) — finding 1.
  - `sdd/specs/shared/diagnostics.md`, `Uses`: `RH3002-010` inserted at its sorted position between `RH2109-004` and `RH5308-005` (referenced by SHARED-DIAG-008) — finding 2.

`scripts/test.sh --project architecture` -> "Passed!  - Failed:     0, Passed:   118, Skipped:     0, Total:   118".

## Examples for the tests

Unchanged: RH3002-010 (see `01-spec-author.md`).

## Output questions

_None._

## Internal choices

_None._

## Follow-up candidates

_None._
