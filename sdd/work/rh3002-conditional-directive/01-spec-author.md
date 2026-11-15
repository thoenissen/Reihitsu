Role: spec author
Commit: 9f69e84
Verdict: spec written

Issue #1756 (`agent-filed`). Worked from the issue, its two maintainer comments, and `00-lead.md`; the maintainer's answer (B: RH3002 keeps reporting, no fix; spec text aligned only, SHARED-DIAG-008's general reversal is #1759) overrides the issue's recommended option A. The prompt carried no intended fix beyond that answer.

No intended behavior changes: the new requirement writes down what the analyzer does today (the issue's "Today's output"), and SHARED-DIAG-008 gains only a sentence naming that exception. So no change doc (`sdd/README.md`, *When a change doc is required*; a clarification adds the current behavior as a requirement), and nothing visible changes.

## Changed requirements

- RH3002-010 — A conditional directive inside the parentheses does not keep them from being reported — added (clarification of current behavior, decided by the maintainer as answer B; the example is the issue's input).
- RH3002-009 — A comment or a directive inside the parentheses withholds the fix — clarified: "The diagnostic is still reported, also for a conditional directive (RH3002-010)."
- SHARED-DIAG-008 — A conditional directive in the judged span — clarified: names RH3002 as the one exception, pointing at RH3002-010. Its normative rule for every other rule is unchanged.
- `sdd/specs/requirement-ids.txt` — RH3002-010 inserted after RH3002-009.

## Examples for the tests

- RH3002-010 — its example is exactly the issue's input (`DEBUG` undefined, so `+ 1` is excluded code): reported from `(` to `)`, no code fix offered.

**Covering test:** none exists for the issue's exact input. The closest is `VerifyDirectiveInsideThrowExpressionParenthesesWithholdsTheCodeFix` in `src/Reihitsu.Analyzer.Test/Clarity/RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzerTests.cs` (tagged RH3002-009): an empty `#if DEBUG`/`#endif` group inside the parentheses of a throw expression, with no excluded code and not in a `return` statement. RH3002-010 is a new active requirement of an enforced spec, so `RequirementCoverageTests` needs a test tagged `[Requirement("RH3002-010")]` in this PR: the lane widens to tests (as `00-lead.md` foresees). The test should assert the diagnostic and that no code fix action is offered, like the throw-expression test above.

## Output questions

_None._ The maintainer already answered the run's output question (B).

## Internal choices

- A new requirement RH3002-010 for the report, rather than folding the statement into RH3002-009: RH3002-009 is about the fix, and the report under a conditional directive is a separate decision that departs from a shared requirement, so it gets its own ID and its own test. Alternative: amend RH3002-009 only, which avoids a new test tag but leaves the decision without its own covering test.
- SHARED-DIAG-008 names RH3002 as a specific exception rather than gaining a general permission ("a rule's own requirement may report …"), so no other rule reads it as license to depart; the general reversal stays with #1759.
- No change doc: behavior and visible output are unchanged; the decision is recorded in the specs.
- Rule page `documentation/rules/RH3002.md` unchanged: what is reported and how to fix it do not change for the user, and `documentation/rules/general-notes.md` ("Code excluded by `#if`": only the code active for the current build is analyzed) already states the decided behavior.

## Follow-up candidates

_None._ (The general reversal of SHARED-DIAG-008 is already #1759.)
