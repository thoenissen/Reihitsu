Role: spec author
Commit: 498bb0b7
Verdict: spec written

Scope: decision 53, audit 7 finding #23 only — RH2001-009 reworded so that a conditional directive in the leading trivia of the type withholds the fix also when the property has no modifier. No other requirement, change doc, or rule page touched. The prompt carried no intended fix beyond the decision.

## Changed requirements
- RH2001-009 — The fix is withheld for a comment or directive that a reordered modifier would cross, or for a conditional directive between the modifiers — clarified (narrowing round): the conditional-directive paragraph now names the leading trivia of a modifier other than the first, or of the type; for a property without a modifier that is in front of its type, also on the lines above the property, whatever the group holds (whole members included). The carve-out to RH2001-004 now covers only the first modifier of a property that has one. Title unchanged.
- New example (one block, both sides of "has no modifier"): `#if DEBUG extern #endif int First { get; }` and a whole-member group `#if DEBUG private int _trace; #endif` above `int Second { get; }` — both reported, no code fix offered. The existing counterpart (`static private int Value` below a group, fixed) is kept; its intro now says "a property that has a modifier".

## Examples for the tests
RH2001-009 (new example): `First` matches the tests in `54-test-implementer.md` (r3/r4). `Second` (audit r6) is spec'd as withheld, matching the guard without `Modifiers.Count > 0` and `main`; no test pins it yet — the lead decides whether one is wanted.

## Output questions
_None._ The behavior is `main`'s output for these inputs (audit 7 #23), decided by the lead in decision 53.

## Internal choices
- Kept the requirement title unchanged rather than adding "or in front of the type"; the alternative is a retitle, which would ripple into the change doc's heading.
- Stated r6 (a whole-member group above a property without modifiers) explicitly as withheld rather than leaving it unspecified, because the guard withholds it and FC-002 wants both sides of the qualifier shown.

## Follow-up candidates
- `sdd/changes/2026-11/rh2001-modifier-reorder-trivia.md:14` still says "A conditional directive on the lines above the property keeps the fix (RH2001-004)" without the "has a modifier" qualifier. Outside this prompt's file scope; the lead may have it aligned.
