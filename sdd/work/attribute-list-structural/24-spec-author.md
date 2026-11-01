Role: spec author
Commit: fbe30bb3b128aef57cb4bccdf931085398ee258a
Verdict: text written

My prompt named the three passages to change under decision 24 and asked that the spec introduction and change doc refer to FMT-STRUCT-029 rather than restate its owner set; that matches decision 24 and finding 9 / the second hint of `22-auditor.md`, which I worked from. No other intended fix or verdict was carried.

Text repairs (no requirement text changed):

- `sdd/specs/formatter/structural-transforms.md`, introduction: "one attribute per list on declarations, and one list per target on parameters and type parameters" becomes "one attribute per list on declarations (FMT-STRUCT-028), and the merge of attribute lists that name one target (FMT-STRUCT-029)". The owners of the merge are no longer restated (finding 9).
- `sdd/changes/2026-11/attribute-list-structural.md`, *FMT-BREAK-027 is split into shape and placement*: "FMT-STRUCT-028 (split on declarations) and FMT-STRUCT-029 (merge on parameters and type parameters), each with its guards" becomes "FMT-STRUCT-028 (the split) and FMT-STRUCT-029 (the merge), each with its guards and the owners it names" (finding 9).
- `sdd/specs/formatter/line-breaks.md`, introduction: "The phase only adds and removes line breaks …" becomes "The phase adds and removes line breaks and keeps the blank lines it finds (FMT-BREAK-030), and it changes tokens only where it shapes an accessor's attribute lists (FMT-BREAK-032)." This matches the change doc's statement that the accessor-shape rewriter is the one rewriter of the phase that changes tokens (auditor's second hint).

The C# comments of finding 9 are the cleaner's; I did not touch or stage them.

## Changed requirements

_None._ (Spec introductions and change-doc rationale only.)

## Examples for the tests

_None._

## Output questions

_None._

## Internal choices

- Named the split by FMT-STRUCT-028 alongside the merge in both texts, so neither restates an owner set; the alternative, extending the old wording with "and of `param:`/`typevar:` lists on any non-accessor owner" as finding 9 suggested, keeps a copy that can drift again (decision 24).

## Follow-up candidates

_None._
