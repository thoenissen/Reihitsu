Role: spec author
Commit: d283edd
Verdict: text written

Narrowing round (lead decision 26; `27-auditor.md` #9, #10, and the hint on the inferred-name bullet). Text only; no code or test touched. The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's decision.

## Changed requirements

- RH2001-011 — The fix is withheld when the field name is already bound at a reference — clarified (#9):
  - Offered bullet 2: a nullable tuple type is not a tuple type, also when it names the element, so `(int Value, int Other)? pair = (Value, 1);` keeps the fix.
  - Withheld bullet 3 limited to "a tuple converted to a tuple type that supplies the name itself", with a pointer that a nullable tuple type is not one.
- Change doc `sdd/changes/2026-11/rh2001-rename-binding.md` (status stays `Accepted`):
  - #10: every `sdd/work/rh2001-rename-safety/…` pointer removed (lines 32, 136, 184, 222, 240, 248 at d283edd); the measured commit SHAs (`962d0c4`, `52ae88c`, `0696846`, `4838e9a`) are kept. The two decision pointers (19, 23) are dropped; the surrounding sentences already state the reason. The work-folder fixture labels `a01` and `a02` were dropped with them, since they name nothing outside `sdd/work/`.
  - Hint: the inferred-name *What* bullet now says "Everywhere else means three groups of tuple positions", each a sentence of its own; the governing tuple of a `switch` sits in the first group (names no code reads) with "which the maintainer decided". The third group carries the same tuple-type/nullable boundary as RH2001-011.

## Examples for the tests

RH2001-011: `(int Value, int Other)? pair = (Value, 1);` is offered with `main`'s output `(_value, 1)` (n1 in `27-auditor.md`). Pinning it is optional; #8's tests cover `object` and `(int, int)?`.

## Output questions

_None._ No output changes; the wording matches what head and `main` already do.

## Internal choices

- #9 resolved by both suggested edits (state the nullable side in bullet 2 and restrict bullet 3 to tuple types) rather than one, so neither bullet can be read alone the wrong way. Alternative: only one of the two.
- The change doc's third group got the same tuple-type restriction, to keep it consistent with the spec after #9. Alternative: leave the change doc's "a type that supplies the name" as written.

## Follow-up candidates

_None._
