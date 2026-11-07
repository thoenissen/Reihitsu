Role: spec author
Commit: 7eadddb
Verdict: spec written

Correction for decision 10 in `00-lead.md` (finding of 04-test-implementer). The FMT-INDENT-013 enum example's output and the after block of visible output change 2 in `sdd/changes/2026-11/raw-string-tab-one-column.md` now show `A` without the trailing comma, which is what whole-document formatting produces under the unchanged FMT-STRUCT-019. The inputs keep `A,`. `Int32` stays at column 9 (Q1). Nothing else changed.

## Changed requirements
FMT-INDENT-013 — A line inside a list element aligns with the element — example corrected (output only; normative text unchanged)

## Examples for the tests
FMT-INDENT-013 (enum example: input `A,`, output `A`)

## Output questions
_None._

## Internal choices
- The change doc's **before** block (today's output on `main`) still shows `A,`. FMT-STRUCT-019 already removes the comma on `main`, so that block is inaccurate in the same way; it was left as is because the task limited the edit to the after block. Recommendation: the lead lets a later round change line 82 of the change doc to `A`, too.

## Follow-up candidates
_None._
