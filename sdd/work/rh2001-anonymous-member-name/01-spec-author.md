Role: spec author
Commit: 1f01049b2cdc88089d5f1838d06421715da37151
Verdict: change doc written

The prompt carried no intended fix or verdict beyond the maintainer's binding answer (option B) recorded in `00-lead.md` and the issue comment.

Change doc: `sdd/changes/2026-11/rh2001-anonymous-member-name.md` (Status: Draft), four visible output changes, all RH2001 code-fix results; no formatter output and no rule page change.

## Changed requirements

- RH2001-012 — The fix keeps the name of an anonymous-object member — added (refers to SHARED-CASING-015; RH2001 `Uses` gains SHARED-CASING-015).
- RH2001-011 — The fix is withheld when the field name is already bound at a reference — changed: an anonymous-object member no longer withholds the fix; tuple paragraphs unchanged; the anonymous `new { Value }` and `ToString()` negative examples removed (now positive in RH2001-012).
- RH2001-004 — The fix replaces the property with a private field — clarified: the coverage sentence names only tuple elements under RH2001-011 and points at RH2001-012 for anonymous-object members.
- RH2001 non-goal added: writing out a tuple element's inferred name.
- `sdd/specs/shared/casing.md` and `sdd/specs/README.md`: `CasingAnonymousMemberUtilities` added as a SHARED-CASING code owner (it implements SHARED-CASING-015 and was listed nowhere).

## Examples for the tests

RH2001-012 (six examples: bare reference; `this.`/`?.` access read by name; Fix All over two properties with `ToString()`; `this.`-qualified rename next to a local `_value`; `Value.ToString().Length` only renamed; anonymous member next to a natural-type tuple, no fix offered). RH2001-011's remaining tuple examples are unchanged, and its explicit-name example (`new { Value = Value }`) still holds.

Checked with `scripts/apply-fix.sh RH2001` on `main` (1f01049): the first four examples and the tuple-mix example are `no-fix-offered` today; the `Value.ToString().Length` example is fixed today exactly as written in the spec. The four "after" outputs compile (built as a net11.0 class library). `scripts/test.sh --project architecture`: 220 pass, 1 fails — `EveryRequirementOfAnEnforcedSpecIsCovered` reports RH2001-012 without a tagged test, which is the test implementer's job.

## Output questions

_None._ The maintainer's answer B settles the issue's question.

## Internal choices

- A new requirement RH2001-012 that references SHARED-CASING-015, instead of rewriting RH2001-011 in place to state the explicit-name rewrite there; alternative: fold it into RH2001-011, whose title is about withholding.
- The conflict is resolved by RH2001 now conforming to SHARED-CASING-015's scope sentence ("every code fix that renames a reported name as SHARED-CASING-007 states"), which therefore stays unchanged; alternative: name RH2001 in that sentence, which would add an RH2001 row to the casing spec's `Uses`.
- `CasingAnonymousMemberUtilities` added as a casing code owner; alternative: leave owner lists alone (the type then stays unowned while RH2001 is expected to reuse it).
- Every other withholding reason still applies in the same rename, so an anonymous member next to a tuple in a withheld position stays without a fix (stated in RH2001-012 with an example); this matches today's behavior for that input.

## Follow-up candidates

_None._
