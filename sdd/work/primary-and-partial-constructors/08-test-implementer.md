Role: test implementer
Commit: 4d5df93 (head the tests were run on; the test commit follows it)
Verdict: #1680 repair round 1: reproduced (finding 1), tests written (finding 2)

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the unchanged "one diagnostic per …" requirements (RH3002-007, RH3005-003, RH3006-002, RH7302-003) and lead decisions 9 and 10 (00-lead.md); none from a patched copy of the code.

## Tests

Paths are under `src/Reihitsu.Analyzer.Test/`. All use `Verify(source, diagnostics)` (analyzer verifier, `{|#0:…|}` markup, one expected diagnostic), so a second diagnostic on the span fails the count.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Clarity/RH3006….AndInsideOrInIfConditionInFrontOfClosingBraceOfClassWithPrimaryConstructorIsReportedOnce` | RH3006-001, RH3006-002 | `Verify` | fail (1 expected, 2 actual) |
| `RH3006….AndInsideOrInElifConditionInFrontOfClosingBraceOfClassWithPrimaryConstructorIsReportedOnce` | RH3006-001, RH3006-002 | `Verify` | fail (2) |
| `RH3006….AndInsideOrInIfConditionInFrontOfClassWithPrimaryConstructorIsReportedOnce` | RH3006-001, RH3006-002 | `Verify` | fail (2) |
| `Clarity/RH3005….ConstantOnLeftInIfConditionInFrontOfClosingBraceOfClassWithPrimaryConstructorIsReportedOnce` | RH3005-001, RH3005-003 | `Verify` | fail (2) |
| `RH3005….ConstantOnLeftInIfConditionInFrontOfClassWithPrimaryConstructorIsReportedOnce` | RH3005-001, RH3005-003 | `Verify` | fail (2) |
| `Clarity/RH3002….ParenthesesInIfConditionInFrontOfClosingBraceOfClassWithPrimaryConstructorAreReportedOnce` | RH3002-007 | `Verify` | fail (2) |
| `RH3002….ParenthesesInIfConditionInFrontOfClassWithPrimaryConstructorAreReportedOnce` | RH3002-007 | `Verify` | fail (2) |
| `Formatting/RH7302….VerifyLowercaseRegionInFrontOfBaseTypeArgumentListOfClassWithPrimaryConstructorIsReportedOnce` | RH7302-001, RH7302-003 | `Verify` | pass (guard edge a) |
| `RH7302….VerifyLowercaseRegionInFrontOfParameterListOfClassWithPrimaryConstructorIsReportedOnce` | RH7302-001, RH7302-003 | `Verify` | pass (guard edge b) |

Inputs (finding 1): `#if A || {|#0:B && C|}`, `#if A` / `#elif A || {|#0:B && C|}`, `#if true {|#0:==|} A`, `#if ({|#0:(A)|})`, each followed by `#endif`, placed either in front of `}` of `public class Service(int value) { public int Value => value; }` or in front of its `public`. RH3002 marks the inner pair, as the auditor's probe showed.

Inputs (finding 2): (a) `public class Derived(int value) : Base` / `#region lower` / `#endregion` / `(value);` — the directive is in the leading trivia of the base argument list's `(`, so it exercises both the base-argument-list exemption and its `FullSpan`; (b) `public class Service` / `#region lower` / `#endregion` / `(int value)` / `{ … }` — the directive is in the leading trivia of the parameter list's `(`, outside its `Span` but inside its `FullSpan`.

Watched: the seven finding-1 tests fail with `Mismatch between number of diagnostics returned, expected "1" actual "2"`. Control: with `Service(int value)` replaced by a plain `Service` (temporary copy, restored; not committed) all seven pass, so the marked spans are the ones reported and the only difference is the count. The two finding-2 tests pass on head, as they must (the code is right); their falsifying observation — 0 diagnostics once the base-argument-list term is deleted or `FullSpan` becomes `Span` — is the auditor's probe (07-auditor.md, finding 2). I did not mutate production code to re-watch it.

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| RH3006 `#if` condition in front of `}` of a primary-constructor class | LF | analyzer | reproduced | 2 vs 1 |
| RH3006 `#elif` condition, same position | LF | analyzer | reproduced | 2 vs 1 |
| RH3006 `#if` condition in front of the type's first token | LF | analyzer | reproduced | 2 vs 1 |
| RH3005 `#if true == A`, both positions | LF | analyzer | reproduced | 2 vs 1 |
| RH3002 `#if ((A))`, both positions | LF | analyzer | reproduced | 2 vs 1 |
| same seven inputs in a class without a parameter list | LF | analyzer | 1 each | control |

## Not covered, and why

- CRLF: the count is a property of which nodes the driver visits, not of line endings; no layout is involved. The existing round-0 tests made the same choice.
- Code-fix path for RH3002/RH3005/RH3006 on directive conditions: the worklist and finding 1 concern the report count only; whether those fixes rewrite inside a directive condition is unchanged behavior not in the run's scope.
- Records and structs for finding 1: the auditor shows the guard is position-based and shared by every type declaration with a parameter list (the round-0 tests cover records and structs for the trivia kinds); the class shape is the auditor's counterexample.
- RH3001, RH5302, and the cref rules: the auditor found 0 reports in both runs, so no duplicate to reproduce.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~ReportedOnce&(FullyQualifiedName~RH3002|FullyQualifiedName~RH3005|FullyQualifiedName~RH3006|FullyQualifiedName~RH7302)"
```
