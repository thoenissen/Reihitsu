Role: test implementer
Commit: 52df787b (base; the test commit follows it)
Verdict: tests written

Scope: decision 55, audit 8 #27, input a1 only. One no-fix test, LF and CRLF, asserting the head's current output. No production change; spec files left to the parallel spec author.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests.VerifyNoCodeFixIsOfferedForAConditionalWholeMemberGroupInFrontOfTheTypeOfAReadWritePropertyWithoutModifiers` (`\n`, `\r\n`) | RH2001-009 | `VerifyNoCodeFixIsOffered` | pass (2/2 on the head) |

Input (a1): `#if DEBUG` / `private int _trace;` / `#endif` / `int Value { get; set; }` inside `public class C`, with a method reading `Value`. RH2001 is reported on `Value`; no code action is offered. Per audit 8, `main` offers a fix here, so the test pins the decision-55 refusal against a revert of the guard to `main`'s behavior (not run on `main` in this step).

## Scenarios (bug fix)
Not applicable: the test asserts decided current behavior, not a reproduction.

## Not covered, and why
- a9 (property inside an enclosing `#if !DEBUG` group): the lead's task named a1 only (decision 55: "a no-fix test for a1").
- Get-only counterpart (a3, the spec's `Second`): existing behavior, unchanged by decision 55; not requested.
- All other failure classes: as dismissed in `57-auditor.md`; this round adds no production change.

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests.VerifyNoCodeFixIsOfferedForAConditionalWholeMemberGroup"
```
