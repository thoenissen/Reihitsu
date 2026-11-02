Role: spec author
Commit: e02b08f
Verdict: change doc written

This amendment applies maintainer answer 6 from `00-lead.md`. It also removes my earlier statement, in `04-spec-author.md` and in the change doc, that the maintainer applied answer 4 to `async`. That came from the coordinator's own note, not from the maintainer.

## Changed requirements

- **FMT-INDENT-031 is changed and renamed** to "An anonymous method in a raw-string hole aligns like a block lambda".
  - The anchor is the `async` keyword of an async anonymous method, and otherwise `delegate`, also behind `static`. This is the anchor FMT-INDENT-028 takes for a block lambda in the same position.
  - The second example gains `T {M(async delegate { return 1; })}`. Its brace is in column 22, under `async`, and its body in column 26.
  - `static delegate` stays under `delegate`, in column 29.
  - The example's "after" block was verified as a fixed point of `main`'s formatter.
- **Change doc:**
  - The title and What now describe alignment like a block lambda.
  - The anchor decision is now headed "The anchor follows the block-lambda counterpart".
  - Answer 4 now covers `static` only, and answer 6 is added.
  - Answer 1 notes that answers 4 and 6 refined it.
  - The "everywhere" alternative is reworded to match.
- No other spec, requirement-ID, or code-owner change.

## Examples for the tests

The second example of FMT-INDENT-031 now covers `static delegate` (under `delegate`) and `async delegate` (under `async`).

## Output questions

_None._ No visible output change beyond answer 6.

## Internal choices

- `static async delegate` anchors at `async`, as an async lambda anchors at `async` whatever stands in front of it. This follows answer 6's rule, and no example covers it.

## Follow-up candidates

Unchanged from `03-spec-author.md`.
