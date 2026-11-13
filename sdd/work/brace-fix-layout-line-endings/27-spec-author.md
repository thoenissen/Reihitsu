Role: spec author
Commit: b640f0e72991e90f8b7061dbe5116549c8b196c4
Verdict: spec written

Repair of spec checker 26, finding 1 (decision 24). Text only: no requirement ID added, retired, or renumbered, and no normative sentence changed.

## Changed requirements
- RH5502-004, RH5504-004, RH5506-004, RH5508-004, RH5510-004, RH5512-004, RH5514-005, RH5516-004, RH5518-004, RH5520-004, RH5522-004, RH5526-004, RH5528-004, RH5531-006 — clarified. Each requirement changes in two places:
  - **Heading:** "on the line of the brace that opens the body holding the declaration" becomes "behind a brace on its line that is still open there".
  - **`Why:`:** the old sentence becomes "Behind a `{` on the list's line that is still open there, a further list would start at the indentation of the code that owns that `{` or of code further out, in front of the rest of that body."
  - Both now match the restated SHARED-ATTR-022, and the normative sentence still defers to it by ID.

`rg "body holding"` over `sdd/specs` finds nothing more. The one remaining hit is in the change doc's *Decisions*, where it records the earlier widening as history and stays.

## Examples for the tests
_None new._ The examples are unchanged, and no test changes.

## Output questions
_None._

## Internal choices
_None._

## Follow-up candidates
_None._
