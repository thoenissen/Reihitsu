Role: spec author
Commit: c55f330
Verdict: spec written

Repair round 2, finding #1 of `09-auditor.md`, answered 3A by the maintainer (`00-lead.md`). The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Changed requirements

- FMT-SWITCH-007 — Comments survive brace insertion — clarified. Answer 3A is now recorded next to answers 1A and 2A:
  - First paragraph: a blank line between the last comment that leads the first statement and that statement MUST be carried into the block as the wrap finds it; brace insertion MUST NOT remove it.
  - Second paragraph: the list of layouts kept around the terminal `break` now includes the blank line between the last comment that leads the `break` and the `break`. One sentence states what "as the wrap finds it" means: a run of blank lines has already collapsed to one (FMT-BLANK-014), and a blank line behind a documentation comment has already been removed (FMT-BLANK-010). Both match what the auditor observed (several blank lines give one; `///` keeps none).
  - `Why:` gets one sentence: a plain block and a braced `if`/`else` body keep this blank line, so the braced section keeps it too.
  - New example: `// a` / blank / `A();` at the first statement and `// b` / blank / `break;` at the terminal `break`, input and output. I formatted the input on the head (`c55f330`) with `scripts/format.sh`: the output equals the example and is stable in a second pass.
  - `FMT-BLANK-010` and `FMT-BLANK-014` were added to the spec's `Uses` header.

FMT-PIPE-013 is unchanged, for the same reason as decision 7 of `00-lead.md`. No IDs were added. `scripts/test.sh --project architecture --filter Sdd` passes (51/51).

## Examples for the tests

- FMT-SWITCH-007, the new last example: one test per site (first statement, terminal `break`), as decision 10 asks. The members the auditor listed are useful variants: `/* */` instead of `//`, several blank lines (collapsed to one), and a single-statement section.

## Output questions

_None._

## Internal choices

1. The FC-002 qualifier "last comment" is shown on both sides by the existing example (a blank line *between* two comments in front of the `break`) and the new one (a blank line *behind* the last comment). I did not add a separate `///` example: FMT-BLANK-010 decides that case before the wrap runs, and the requirement cites it.

## Follow-up candidates

_None._
