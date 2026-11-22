Role: spec author
Commit: bdc13c3
Verdict: text written

The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore.

Finding #3 of `20-auditor.md`: the sentence behind the new RH7309-005 example in `sdd/specs/analyzer/RH7309.md` gave a branch-content reason ("in a build with `DEBUG`, the group could close `C` …") that the example's `#if DEBUG` / `Trace()` / `#endif` group contradicts. It now gives the positional reason, as SHARED-ORDER-008's *Why* puts it:

> is reported, and no code fix is offered: the fix does not read the group, and a group in that place can close `C` and open a struct whose fields `_d` and `_b` become.

Spec prose only: no requirement ID, `Uses` row, or code owner changed, so no architecture test run is needed; the rule page is unaffected. Nothing was built or tested (another role edits C# comments in the same tree).

## Changed requirements
RH7309-005 — example explanation — clarified (wording only, no behavior change)

## Examples for the tests
_None._ The example's code and its verdict are unchanged.

## Output questions
_None._

## Internal choices
_None._

## Follow-up candidates
_None._
