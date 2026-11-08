Role: spec author
Commit: e8b2a713
Verdict: spec written

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond decision 36 itself, which is the lead's recorded decision and the input of this step.

Measured with `scripts/apply-fix.sh RH2001` on temporary worktrees (removed afterwards), fixture `if_static` from `34-skeptic.md`:

- `main` (`57acf0d`): `private` / `readonly` on its own line / `#if DEBUG` / `static` / `#endif` / `int _value;`. Under CRLF, main also reports `line-ending-drift` (one LF in the output); head does not.
- head (`e8b2a713`): `private` / `#if DEBUG` / `static` / `#endif` / `readonly int _value;`, LF and CRLF alike. This is the output decision 36 adopts.

## Changed requirements

- RH2001-009 — The fix is withheld for a comment or directive that a reordered modifier would cross — changed: the sentence "A directive at that place is not covered by this requirement" is replaced by a paragraph. When the place of `readonly` lies directly behind a conditional group between the modifiers whose branches hold only modifiers that RH7105-001 puts in front of `readonly`, and every modifier behind the group is one RH7105-001 puts behind `readonly`, `readonly` is written on the line after `#endif`, in front of those modifiers or the type; the group stays in place. Any other directive at that place (for example a group holding a modifier RH7105-001 puts behind `readonly`) stays uncovered. Added a `Why:` sentence and the example "a conditional group at the place of `readonly`" (input and output as measured on head).
- Change doc `sdd/changes/2026-11/rh2001-modifier-reorder-trivia.md`: a *What* bullet, a *Visible output changes* entry (main before, head after), a *Decisions* entry citing decision 36 and the standing answer, and two rejected alternatives (in front of the group, as on main; inside the branch). The status stays `Accepted`, as the lead set it. Decision 36 is covered by the standing answer, so I did not reset it to `Draft`. The lead can change that.

No requirement ID was added. No rule page changed: this is a corner case, not the normal case.

## Examples for the tests

- RH2001-009, example "a conditional group at the place of `readonly`". The test implementer pins this output in `VerifyFixIsOfferedForAConditionalModifierBetweenTheModifiersAndTheType` in place of the count-only assertion. The spec's example includes `Get()` so that the rename of the use is shown; the change doc omits it.

## Output questions

_None._ Decision 36 is covered by the standing answer.

## Internal choices

- The covered shape is narrow: a group of modifiers that all precede `readonly`, with only modifiers after `readonly` behind it. The alternative was to state a place for any group, and I did not take it. A group holding `unsafe`, for example, has no single place that is correct in every build without reordering across the directive, and neither the decision nor any test covers it.
- The column of `readonly` (the indentation of the type, as measured) is not stated. It is layout, not placement, and the example shows it.

## Follow-up candidates

- `main`'s RH2001 fix on `if_static` reports `line-ending-drift` under CRLF. Head does not, so this run already fixes it. Nothing to file unless the auditor finds it again.
