Role: spec author
Commit: f6fdc85
Verdict: spec written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. This round carries out `00-lead.md` decisions 18 and 19 (points 2 and 3 of the spec findings in `04-test-implementer.md`). No visible output changes; no test, code, rule page, or change doc is touched.

Files: `AGENTS.md`, `sdd/specs/formatter/switch-case-braces.md`.

## Changed requirements

- FMT-SWITCH-015 — The inserted braces are placed as the block of a section — clarified (decision 19). Each of the four placements now reads "starts the next line with text below …" instead of "starts the line below …", so a blank line the phase carries into the gap (FMT-SWITCH-016) no longer contradicts it. A sentence states the qualifier on both sides: the line directly below when no blank line is carried (FMT-SWITCH-015's own example), a carried blank line staying between the two otherwise (FMT-SWITCH-016's example). The terminal-`break` placement also names "or the first comment on a line of its own that leads it", matching the first-statement placement; FMT-SWITCH-016's example (`// b` in front of the `break`) already showed that shape.
- `AGENTS.md`, `BlankLinePhase` ownership row — the stale FMT-BLANK-021 clause ("it writes a line break that ends a line only for a statement the line-break phase leaves on its predecessor's line next to an empty statement") is replaced by "it adds and removes only blank lines, between lines the earlier phases have already broken", the wording of FMT-PIPE-025 (decision 18; follows decisions 6, 7, 12).

## Examples for the tests

None new. The existing FMT-SWITCH-015 phase-alone tests (input without blank lines) and FMT-SWITCH-016 tests (blank-line counts and text lines) already cover both sides of the clarified qualifier, as `04-test-implementer.md` point 3 notes.

## Output questions

_None._

## Internal choices

- The `AGENTS.md` row borrows FMT-PIPE-025's sentence rather than a negative ("writes no line break that ends a line"); alternative was the negative form, rejected because it restates the old exception's shape instead of the rule.
- FMT-SWITCH-015 adds the "first comment that leads the `break`" alternative to the terminal-`break` placement; alternative was leaving that clause as it was, rejected because with `// b` in front of the `break` the `break` is not the next line with text below the closing brace.

## Follow-up candidates

_None._
