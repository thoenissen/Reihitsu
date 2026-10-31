Role: spec author
Commit: 1dc0ff8
Verdict: spec written

Repair round 1, finding 1 of `06-spec-checker.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict. I re-derived the change myself instead of copying the suggestion.

## Changed requirements

None. Only the code-owner listing changed: `BlockWrapTriviaUtilities` was added to the `Code owners` row of `sdd/specs/formatter/pipeline.md` and to the matching row in `sdd/specs/README.md`.

I derived the change from `sdd/README.md`, *Code owners*. A type is a code owner of at most one spec, and the behavior it implements decides which spec, not the code that calls it. The type's own documentation comment says what it implements: carrying the comments of wrapped code across new braces with the author's layout. That is FMT-PIPE-013. So the owning spec is FMT-PIPE. The checker suggested `structural-transforms.md` and `switch-case-braces.md`, but listing the type in both would break the one-spec rule. Listing it in only one would assign shared behavior to one of its two callers. Both caller specs already name FMT-PIPE-013 under `Uses`, so their headers stay unchanged.

`scripts/test.sh --project architecture --filter Sdd`: passed, 51/51.

## Examples for the tests

None. No requirement or example changed.

## Output questions

_None._

## Internal choices

1. The owner is `pipeline.md` (FMT-PIPE-013) rather than the two caller specs the checker named. The rulebook allows one owning spec per type, and it decides by the behavior implemented.

## Follow-up candidates

_None._
