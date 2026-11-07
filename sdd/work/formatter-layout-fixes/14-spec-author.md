Role: spec author
Commit: eeba52ae3a3316a692531ac5a84ddc2f25ccaa43
Verdict: text written

Repair round after the passing audit: 13-auditor finding 1, as lead decision 19 states. Spec text only; no change doc, test, code, or rule page changes, and no output changes. My prompt carried no intended fix beyond decision 19.

## Changed requirements
- FMT-BREAK-015 — Wraps inside a list element are joined — clarified: the rule joins only the gap in front of a parameter's default `=`; the gap behind it is FMT-BREAK-016's.
- FMT-BREAK-016 — Assignments keep target, operator, and value together — clarified: it defers only the gap in front of a parameter's default `=` to FMT-BREAK-015 and places the value behind it, as for every equals-value clause (`LineBreakAssignmentRewriter.NormalizeEqualsValuePlacement`), so a multi-line literal default starts on the `=` line (`string s = """`, observed at `eeba52a`). The lead-in of its second example names both owners.

## Examples for the tests
_None._ The examples are unchanged; the edit corrects ownership statements only.

## Output questions
_None._

## Internal choices
- Also corrected the attribution in FMT-BREAK-016's second example lead-in ("(FMT-BREAK-015)" → "(FMT-BREAK-015 in front of each `=`, this requirement behind it)"), since it named FMT-BREAK-015 as the only joiner; alternative was to leave it.

## Follow-up candidates
- Already recorded as follow-up 15 in `00-lead.md`: `ListElementInteriorJoinRewriter.IsOwnedGap` also joins the gap behind a parameter's default `=` where `NormalizeEqualsValuePlacement` did not, so two components decide that gap (pre-existing on `main`). The spec now names FMT-BREAK-016 as its owner; the structural repair is out of scope.
