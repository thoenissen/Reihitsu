Role: spec author
Commit: 0363c1b
Verdict: spec written

Text-only repair for finding #4 of `15-auditor.md` and hint 1, decisions 19 and 21 in `00-lead.md`: the wider refusal the code implements is accepted, and the text is aligned with it. The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decisions.

## Changed requirements

- RH3001-009 — A negation that touches a word in front of or behind it gets no fix — clarified (`sdd/specs/analyzer/RH3001.md`). The letter already withheld the fix on the gap in front of `!` alone; it now says so: "whatever the operand starts with, an opening parenthesis or a comment between `!` and the operand included (`return!(a)`, `case!(true):`, `when!(y)`, `where!(w)`, `return!/* c */a`)". The Why no longer claims that every withheld member would merge: merging holds where the operand starts with a word and on the trailing side; for an operand starting with `(` or a kept comment the rewrite would be valid, and the front side is decided on the gap alone because telling those operands apart is a second check for the same rare shape (and `return(a)` is what RH6001 reports). Meaning unchanged; no new ID.
- Change doc `sdd/changes/2026-11/rh3001-fix-scope.md`:
  - *What* and *Why*: the RH3001-009 bullet and sentence state the front-side scope and the corrected reason.
  - *Visible output changes*: a new block against `main` — `return!(a)` (before `return(a) == false`), plus `yield return!(a)`, `case!(true):`, `_ when!(y) =>`, `where!(w) select w`, `return!/* c */a` (before `return/* c */a == false`), each now reported without a fix. `main` outputs for `return!(a)` and `return!/* c */a` probed with `scripts/apply-fix.sh RH3001` on a detached worktree at `cf6d26b` (removed afterwards); the others are the auditor's `main` column.
  - *Decisions*: the lead's acceptance of the wider front-side refusal (decision 19) is recorded.
  - *Alternatives considered*: the "insert a space" rejection no longer says the fix would decide spacing in a gap the author left empty (RH3002 and the formatter do exactly that, and `main` wrote `return a == false`); it now states the two answers to the merge question as a decision. New rejected alternative: withhold only where a token would merge (`WouldMergeIntoOneToken`).
- Rule page `documentation/rules/RH3001.md`: not changed (withheld fix for a rare class).
- `TouchesWord`'s summary in the provider still gives the merge reason; per `00-lead.md` *State* that is the cleaner's (17), not mine.

## Examples for the tests

None new. The withheld members named in the letter (`return!(a)`, `return!/* c */a`) have no test pinning them (auditor, *Assertion adequacy*, "Missing"); the lead decided no test changes in this round. `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"`: 51 passed.

## Output questions

_None._ Withholding a fix for a rare class (decision 19).

## Internal choices

- Change doc kept `Accepted`: the added visible changes are withheld fixes for a rare class, decided by the lead. Flip to re-acceptance if you read `sdd/README.md` otherwise.
- RH6001 cited in the Why only for `return(a)`; it does not cover `case(`, `when(`, or query clauses, so the Why rests mainly on "a second check for the same rare shape". Alternative: no RH6001 mention.

## Follow-up candidates

_None._
