Role: spec author
Commit: 1d212ae
Verdict: change doc written

The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore; it carried the lead's decisions 5–7, which this amendment records.

Amended `sdd/changes/2026-11/enforce-complexity-rule.md`:

- `Status: Accepted`.
- *Visible output changes*: the paragraph saying the default is asked as an output question in this run is removed; the section says none.
- *Decisions — The default of 15 stays*: the pending wording is replaced by the decision and its evidence: the maintainer's triage condition (propose a new default only if 15 is poorly chosen) is not met; 0.73 % of production members over 15 (19 of 2609) against 0.72 % in the RH2008 roll-out (18 of 2514), production p99 14, test 0.05 % (8 of 16909), highest 29 and no cluster, and the skeptic's independent re-run found the same 27 members.
- *Decisions — Members are restructured*: names the 19 production members and 8 test helpers (no test method) and states that a test helper keeps its results for every argument the tests pass (decision 6). The doc names no role; who restructures what stays in `00-lead.md`.
- *Decisions — Debug build*: the Debug absence is checked by a ruleset test of its own, and the Release `Error` by the same test as RH2008/RH2111 (decision 7), without naming test methods.
- *Alternatives — A default of 20*: "Rejected on the recommendation" becomes "Rejected".

## Changed requirements

_None._ RH2009-001 is unchanged.

## Examples for the tests

No spec example. The ruleset and scan tests follow `00-lead.md` decisions 3, 4, and 7.

## Output questions

_None._ Output question 1 of `01-spec-author.md` is withdrawn by the lead's decision 5.

## Internal choices

- Test names and role assignments are left out of the change doc (it holds no task list or implementation plan); it states only what is checked. Alternative: name `ReleaseRulesetReportsRuleAsError` and `DebugRulesetHasNoEntryForComplexityRule`.

## Follow-up candidates

_None._
