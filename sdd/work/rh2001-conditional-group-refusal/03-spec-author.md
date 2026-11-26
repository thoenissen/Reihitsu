Role: spec author
Commit: a6f4308 (statements derived from it)
Verdict: change doc written

My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's decisions 1 and 3 in `00-lead.md`, which this handover follows.

## Changed requirements

- RH2001-010 — The fix is withheld for a conditional directive in front of the first modifier or behind a property without an initializer — clarified. Its examples did not cover a group that wraps the property itself. The first boundary bullet now names that shape: its `#if` stands in front of the first modifier. A new third example shows it in one block, as class `C` (`#if NET` / `private int Value { get; }` / `#endif`, without an initializer) and class `D` (`#if NET` / `private static int Value { get; } = 5;` / `#endif`, with an initializer). Both are reported and no fix is offered. The normative sentence is unchanged.
- Change doc `sdd/changes/2026-11/rh2001-conditional-group-refusal.md`:
  - New *Visible output changes* section 3, "A conditional group that wraps the property itself". It gives input, `main` output, and after, both without and with an initializer. It notes that the no-modifier form is already withheld (RH2001-009) and that answer A covers the case.
  - `Status: Accepted`, as the lead decided (decision 3).

## Examples for the tests

The new RH2001-010 example covers the skeptic's cells 7 and 8 (f4 and f5). Pin both classes as separate tests, one for no initializer and one for an initializer.

I ran both inputs through `scripts/apply-fix.sh RH2001` on the current head. The code is still unchanged, so this shows `main`: each input is fixed under LF and CRLF, with exactly the *Before* output in the change doc. The combined `C`/`D` spec block compiles and is fixed the same way. Architecture tests pass, 198 of 198.

## Output questions

_None._ The lead decided that answer A covers this case (decision 1).

## Internal choices

- I put both forms in one spec example, as `C` and `D`, following the existing `C`/`D` example for the behind-the-property case. The alternative was two separate examples.
- I placed section 3 as its own section of the change doc rather than as a bullet under section 1. This shape is no group of whole members next to the property, and the before output differs.

## Follow-up candidates

_None._
