Role: spec author
Commit: 46e33c5
Verdict: spec written

Repair round 1, finding 1 of `07-spec-checker.md`. I re-derived the change from the specs. My prompt carried no intended fix or verdict.

## Changed requirements

- No requirement changed. `sdd/specs/analyzer/RH5409.md`, `RH5410.md`, `RH5411.md`:
  - The `Uses` row gains SHARED-TRAIL-004.
  - The intro sentence now reads "... offers a code fix that removes it as SHARED-TRAIL-003 specifies, also when the only conditional directives behind it are `#elif`, `#else`, and `#endif` (SHARED-TRAIL-004)."
  - The wording follows SHARED-TRAIL-004's condition: no `#if` in the gap.
- No rule page is touched. The normal case is unchanged.
- `scripts/test.sh --project architecture`: Passed 198, Failed 0.

## Examples for the tests

_None new._ SHARED-TRAIL-004 is already tagged (07, check 7).

## Output questions

_None._

## Internal choices

- I named SHARED-TRAIL-004 in the intro sentence as well as in `Uses`. The alternative was the `Uses` row only. I rejected it because the intro sentence otherwise reads as if SHARED-TRAIL-003 alone decides when the fix is offered.

## Follow-up candidates

_None._
