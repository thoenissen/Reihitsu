Role: spec author
Commit: 503cc91
Verdict: spec written

Repair round 1, auditor finding #1, spec side only. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Changed requirements

- FMT-BLANK-011 — A comment on its own line is preceded by exactly one blank line — clarified (example only). The `void O<⏎    // first⏎    T>()` member is removed from both blocks of the "first behind an opening parenthesis, bracket, or `<`" example. Its output pinned the column of the first type parameter behind `<`, which no requirement decides (FMT-INDENT-015 aligns only the later elements; open follow-up candidate in `00-lead.md`). The lead-in now reads "First behind an opening parenthesis or bracket — of a record's parameter list, a union's case-type list, a method's parameter list, an argument list, a collection expression, and a list pattern". Every remaining member's column is decided by FMT-INDENT-012 or FMT-INDENT-025. The normative sentence is unchanged and still names the generic `<`; it needs no example that states a column.
- No other spec example pins an undecided column: I checked FMT-BLANK-011, FMT-BLANK-019, RH5020-002, and RH8303-002 for `<` and array-size `[` cells. RH5020-002 (`void O<`) and RH8303-002 (`void M<`, `List<`) state only reported / not reported, so they stay, as the auditor said.
- No requirement ID, `Uses` row, or code owner changed, so the architecture tests were not rerun. The remaining members are unchanged from the version verified in `01-spec-author.md`/`03-spec-author.md`, and removing an independent member does not change the others' output.

## Examples for the tests

FMT-BLANK-011: the parenthesis/bracket example no longer has a `<` member. Tests that keep the `<` or `new int[` cells should assert only the blank lines there (`AssertBlankLinesAbove`), per finding #1. That is the test implementer's repair; I touched no test.

## Output questions

_None._

## Internal choices

- The change doc `sdd/changes/2026-11/first-comment-behind-opening-token.md`, entry 3, still shows `void M<` and `new int[` with their real columns in *before* and *after*. I left it. A change doc records the visible output as it is on `main` and after the change, and it is not a spec. `sdd/README.md` keeps undecided cases out of specs, not out of a change record of real output. The alternative was to trim entry 3 as well. I rejected it because entry 3 is the accepted record of what the maintainer decided in 1C, and its *after* is the real output.

## Follow-up candidates

_None_ beyond `00-lead.md`.
