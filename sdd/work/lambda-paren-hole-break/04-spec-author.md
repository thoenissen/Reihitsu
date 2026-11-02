Role: spec author
Commit: 37d04a5
Verdict: change doc written

This amendment applies maintainer answers 4 and 5 from `00-lead.md`. No intended fix was carried beyond them.

## Changed requirements

- FMT-INDENT-031 — An anonymous method in a raw-string hole aligns to its `delegate` keyword — **changed** (renamed). The anchor is now the `delegate` keyword, also when a `static` or `async` modifier stands in front of it (answer 4).
  - The `static delegate` example now has its brace in column 29, under `delegate`, and its body in column 33.
  - The example's "after" block was verified as a fixed point of `main`'s formatter.
- Change doc:
  - The title, What, and the decision "The anchor is the `delegate` keyword" now say `delegate`.
  - Answers 4 and 5 are recorded under *Maintainer answers*, and answer 1 notes the refinement.
  - The "everywhere" alternative now says `delegate` keyword.
- No other spec, ledger, or code-owner change.

## Examples for the tests

- FMT-INDENT-031's second example covers `static delegate` under `delegate`. Add an `async delegate` case to it: its block also aligns under `delegate`.

## Output questions

_None._ The amendment adds no visible output change beyond answers 4 and 5. One point is recorded in the change doc: an `async` anonymous method aligns under `delegate`, while an `async` block lambda aligns at `async` (FMT-INDENT-028). The maintainer stated the answer applies to `async` as well.

## Internal choices

_None._

## Follow-up candidates

Unchanged from `03-spec-author.md`.
