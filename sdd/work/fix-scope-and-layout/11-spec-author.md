Role: spec author
Commit: aba01c6
Verdict: spec written

Repair round 1, decision 26. Only `sdd/specs/shared/attribute-targets.md` changed.

Code check: `TargetAttributePlacementCodeFixProviderBase.HasUnusableEnclosingScope` sums the leading whitespace of the innermost enclosing brace's line with `obj == '\t' ? IndentSize : 1` and compares it with `ComputeBaseIndentLevel(ancestor) * IndentSize`. A tab counts as four columns and every other whitespace character as one; the spec now says exactly that. No difference to report.

## Changed requirements
SHARED-ATTR-021 — The separate-line fix is withheld when the enclosing brace is off its depth column — clarified (tab counts as four spaces, every other whitespace character as one; Why extended; tab-indented example added, with the two-space variant as the other side of the condition)

## Examples for the tests
SHARED-ATTR-021 (tab-indented example). The existing RH5513 test `FixInTabIndentedCodeIndentsTheDeclarationWithSpacesToTheAttributeListsColumn` matches it and may now carry the SHARED-ATTR-021 tag.

## Per issue
- #1140: nothing to do in this round.
- #1141: nothing to do in this round.
- #1144: nothing to do in this round.
- #1145: nothing to do in this round.
- #1094: nothing to do in this round.
- #1393: nothing to do in this round.
- #1511: nothing to do in this round.
- #1561: nothing to do in this round (SHARED-ATTR-021 serves the RH5513 placement work of the run; no issue-specific change).

## Output questions
_None._ The clarification writes down the current behavior and keeps `main`'s output.

## Internal choices
- The tab example uses a `text` block with `→` for a tab rather than literal tab characters, so the spec stays readable and survives editors that convert tabs; alternative: literal tabs in a `csharp` block.

## Follow-up candidates
_None._
