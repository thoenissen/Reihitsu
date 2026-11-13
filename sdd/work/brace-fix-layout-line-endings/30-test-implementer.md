Role: test implementer
Commit: c2de574099d61d9280ed2aa93cef8e8bbfe9144d
Verdict: tests written

Scope: decision 25 of `00-lead.md` (auditor 29 finding 9), SHARED-ATTR-022. The prompt named the tests finding 9 calls for; it carried no intended fix, suspected cause, or hoped-for verdict.

## Tests
All in `src/Reihitsu.Analyzer.Test/Formatting/RH5514MethodAttributeListsMustFollowShapeRulesAnalyzerTests.cs`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `SplitFixIsOfferedBehindANestedClosedBodyOnTheLineOfTheList` (LF, CRLF) | SHARED-ATTR-022, SHARED-ATTR-017 | `VerifySplitFix` (verifier + raw text, line endings) | green |
| `SplitFixIsWithheldBehindANestedClosedBodyWhileTheBraceOfTheClassIsStillOpenOnTheLineOfTheList` | SHARED-ATTR-022 | `VerifyNoCodeFixIsOffered` | green |

Inputs and expected values:

- Offered: `    void M() { if (true) { } } [A, B] void N() { }` inside `class C` becomes `    void M() { if (true) { } } [A]` / `    [B] void N() { }` — the SHARED-ATTR-017 output for a list that shares its line with other code; every `{` in front is closed by its `}` in front, so SHARED-ATTR-022 does not withhold.
- Withheld: `class C3 { void M() { if (true) { } } [A, B] void N() { } }` — the class's `{` is still open in front of the list on its line.

Both tests are green on the head, as expected: the auditor measured the current scan as right, and these tests pin the behavior (FC-017, FC-027). To check they are not vacuous, I temporarily replaced the counter in `TargetAttributeListShapeCodeFixProviderBase.StartsBehindAnOpenBraceOnItsLine` by a flag (`waitingCloseBraces = 1` / `= 0`), ran the RH5514 class, and reverted the file (`git checkout`; no production change is committed): exactly the two new offered rows failed (`Assert.IsEmpty`-side: no action registered), 62 others passed. So before this round no test distinguished a count from a flag, as finding 9 states; now one does. The withheld test does not catch the flag mutation (a flag also withholds there); it guards the other side — a scan that treats a nested closed body as cancelling every `{` in front.

## Scenarios (bug fix)
_Not a bug fix round._

## Not covered, and why
- CRLF for the withheld case: a withheld fix writes no layout, so the line ending cannot change the outcome (as in 23).
- Other split rules and RH5528: every split provider reaches the one scan (`rg -n "SHARED-ATTR-022" src/Reihitsu.Analyzer.CodeFixes` → `Base/TargetAttributeListShapeCodeFixProviderBase.cs` only, `StartsBehindAnOpenBraceOnItsLine`), so the pairing is tested once.
- Deeper nesting (three levels) or sibling closed pairs: a counter that passes two levels and the flag mutation it falsifies are the dimension finding 9 names; further levels add no new branch.
- Comment/directive in the gap, idempotency, Fix All: unchanged by this round; covered by the tests listed in 23.

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5514MethodAttributeListsMustFollowShapeRulesAnalyzerTests|FullyQualifiedName~RH5528ReturnValueAttributeListsMustFollowShapeRulesAnalyzerTests"
```
Result on c2de574099d61d9280ed2aa93cef8e8bbfe9144d plus these tests: 87 passed, 0 failed.
