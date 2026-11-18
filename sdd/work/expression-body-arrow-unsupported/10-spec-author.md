Role: spec author
Commit: 1b4e84387808e362771c67c60f76e91774b60ed8
Verdict: text written

Repair round after the passing audit, finding #1 of `09-auditor.md` only. My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond naming the finding.

Re-derived from the current tree: change 12's *After* in `sdd/changes/2026-11/expression-body-arrow-leading-comment-unsupported.md` claimed the location `(9,1)` was derived, not observed, because the precheck did not exist. The precheck exists on this branch, and `CliHiddenCodeEndToEndTests` row "Conditional groups that close in front of a shared arrow" observes `(9,1)`; the single-member statements are observed by `UnsupportedExpressionBodyArrowLeadingCommentTests` ("Group around a constraint clause, symbol not defined" gives `#if NET7_0_OR_GREATER`). The parenthetical is dropped; the rest of the sentence is unchanged and true. Only the change doc is touched: Markdown, no compiled behavior, no requirement ID, `Uses` row, or code owner changed, so no architecture run was needed.

## Changed requirements
_None._ (change doc wording only)

## Examples for the tests
_None._

## Output questions
_None._

## Internal choices
- Dropped the parenthetical as the auditor suggested, rather than replacing it with a note naming the observing test; the alternative adds a test reference the change doc does not otherwise carry.

## Follow-up candidates
_None._
