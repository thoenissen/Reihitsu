Role: spec author
Commit: 8f91745
Verdict: text written

Text cleanup after the passing audit, finding 9 of `31-auditor.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond naming the finding. Lane bug fix: text only, no change doc, no new requirement ID, `requirement-ids.txt`, `sdd/specs/README.md`, and rule pages unchanged.

## Changed requirements
- SHARED-CASING-009 — clarified (Why only). The clause "A constructor, finalizer, operator, or local function belongs to no interface mapping" is wrong for operators since C# 11 (`static abstract` operators and conversion operators in interfaces). It now reads: "A constructor, finalizer, or local function belongs to no interface mapping, and an operator or conversion operator implements an interface operator by its operator token or its conversion types, which a rename does not change". The conclusion is unchanged: operators and conversion operators stay in the not-checked list of the normative text, which is untouched. The meaning of the requirement does not change.

## Examples for the tests
_None._ No example changed.

## Output questions
_None._

## Internal choices
- Conversion operators are named next to operators, because they can also implement a `static abstract` interface member and the normative text lists them separately; alternative: the auditor's suggested wording with "an operator" alone, which would leave conversion operators uncovered by the Why.
- The auditor's hint on the paragraph's opening ("a method, property, or event" before the later narrowing to ordinary methods) is not applied: it is a hint, not a finding, the paragraph is consistent as written, and the change asked was the operator clause only. Alternative: say "an ordinary method" in the opening sentence.

## Follow-up candidates
_None._
