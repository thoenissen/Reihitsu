Role: spec author
Commit: 480be98
Verdict: text written

Text cleanup after the passing audit `21-auditor.md`, finding #5. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Changed requirements

- RH5419-004 — The fix is withheld where the indexer cannot be put on one line — clarified (wording only). The contrast behind the bare line break between `this` and the `[` now reads "unlike one the formatter keeps in front of `this`" instead of "unlike a line break in front of `this`", so it no longer reads as if every line break in front of `this` withholds the fix. A line break in front of `this` that the formatter joins, such as one inside the return type (auditor C4, `List<⏎ int> this[...]`), keeps the fix, as the governing clause already states. No behavior, requirement ID, `Uses` row, or code owner changed.
- Change doc `sdd/changes/2026-11/rh5419-indexer-arrow-single-line.md`: checked, not changed. Its *What* already says "a line break in front of `this` that the formatter keeps", and its decision "A line break the formatter keeps in front of `this` withholds the fix" is qualified in its heading and names only positions the formatter keeps.

Only `sdd/specs/analyzer/RH5419.md` and this handover are staged; the concurrent comment edit in `src/` is the cleaner's. No build or test run, per the prompt.

## Examples for the tests

_None._ No example changed; C4 follows the governing clause and is not given as an example.

## Output questions

_None._

## Internal choices

_None._

## Follow-up candidates

_None._
