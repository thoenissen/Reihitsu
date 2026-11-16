Role: spec author
Commit: 8da4e05
Verdict: text written

Repair round 2, auditor finding #2 (lead decision 18). The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Changed requirements

_None._ RH5106-001 already names the case-type list of a union (`sdd/specs/analyzer/RH5106.md`). Only its derived rule page drifted.

- `documentation/rules/RH5106.md` — the *Description* list of parameter-list owners now names "the case-type lists of unions", as `RH5105.md` does. Q1 A covers the visible change. The rest of the page (examples, fix text) is unchanged: the normal case does not change for users.

## Examples for the tests

_None._ The page change is text only; the RH5106 union tests already derive from RH5106-001's union example.

## Output questions

_None._

## Internal choices

- Placed the union entry after "primary constructors" and before "extension blocks", the same position it has in the RH5105 list; the alternative was appending it at the end of the list.

## Follow-up candidates

_None._
