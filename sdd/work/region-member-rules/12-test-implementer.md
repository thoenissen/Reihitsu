Role: test implementer (repair round 1, audit findings #1 and #3)
Commit: 66c018f (tests written on top of it)
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from RH7107-001/RH7107-004, RH7108-001/RH7108-004, SHARED-ORDER-006 (finding #1) and RH7105-001/RH7106-001, SHARED-MODORDER-002/-003, RH7105-004/RH7106-004 (finding #3). Whether the RH7107/RH7108 fix is offered follows from SHARED-ORDER-006: the conditional pair is balanced inside the moved or the crossed accessor, no other directive kind is present, and the moved accessor keeps its column (SHARED-ORDER-010 does not apply). `main` (`8a26419`, `scripts/apply-fix.sh` in a temporary worktree, LF and CRLF) reports and fixes every RH7107/RH7108 input below with exactly the expected text of the tests.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH7107…ExcludedCodeAtTheEndOfTheExpressionBodyOfTheSetAccessorDoesNotKeepTheGetAccessorFromBeingReported` (`#if DEBUG` / `+ 1` / `#endif` in front of the crossed `set`'s `;`; LF and CRLF) | RH7107-001, RH7107-004, SHARED-ORDER-006 | `VerifyCodeFixWithEndOfLine` | **fails**: no diagnostic (expected 1) |
| `RH7107…ConditionalDirectiveClosingAtTheEndOfTheExpressionBodyOfTheSetAccessorDoesNotKeepTheGetAccessorFromBeingReported` (`#if !NEVER`: only `#endif` lies in the `;`'s trivia) | RH7107-001, RH7107-004 | `Verify(source, fixed)` | **fails**: no diagnostic |
| `RH7107…ExcludedCodeAtTheEndOfTheExpressionBodyOfTheGetAccessorDoesNotKeepItFromBeingReported` (pair at the end of the moved `get`'s expression body; travels with it) | RH7107-001, RH7107-004, SHARED-ORDER-006 | `Verify(source, fixed)` | **fails**: no diagnostic |
| `RH7108…ExcludedCodeAtTheEndOfTheExpressionBodyOfTheRemoveAccessorDoesNotKeepTheAddAccessorFromBeingReported` (auditor's `?? throw` shape; LF and CRLF) | RH7108-001, RH7108-004, SHARED-ORDER-006 | `VerifyCodeFixWithEndOfLine` | **fails**: no diagnostic |
| `RH7108…ExcludedCodeAtTheEndOfTheExpressionBodyOfTheAddAccessorDoesNotKeepItFromBeingReported` (pair at the end of the moved `add`) | RH7108-001, RH7108-004, SHARED-ORDER-006 | `Verify(source, fixed)` | **fails**: no diagnostic |
| `RH7105…ConditionalDirectiveInFrontOfTheFirstModifierDoesNotKeepTheDeclarationFromBeingReportedOrFixed` (`#if DEBUG void Log() { } #endif` then `static public int A;`; without and with `DEBUG`) | RH7105-001, RH7105-004, SHARED-MODORDER-002 | `Verify(source, fixed)` twice, second with `DefineDebug` | passes |
| `RH7106…ConditionalDirectiveInFrontOfTheFirstModifierDoesNotKeepTheDeclarationFromBeingReportedOrFixed` (same shape, `internal protected int A;`) | RH7106-001, RH7106-004, SHARED-MODORDER-002 | `Verify(source, fixed)` twice, second with `DefineDebug` | passes |

All five failing tests fail on the diagnostic count (`expected "1" actual "0"`), the auditor's observed-versus-expected difference; none fails on layout or compilation. The RH7105/RH7106 tests pin the lower boundary of the judged span: without `DEBUG` the excluded member is disabled text in the first modifier's leading trivia, with `DEBUG` only the `#endif` is.

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| RH7107, excluded code before the crossed `set`'s `;` (auditor's shape) | LF, CRLF | analyzer + fix (`apply-fix.sh`, head `66c018f`) | reproduced | no diagnostic vs reported and fixed (as on `main`) |
| RH7107, same with `#if !NEVER` | LF, CRLF | analyzer + fix | reproduced | no diagnostic vs reported and fixed |
| RH7107, excluded code before the moved `get`'s `;` | LF, CRLF | analyzer + fix | reproduced | no diagnostic vs reported and fixed |
| RH7108, excluded `?? throw` before the crossed `remove`'s `;` (auditor's shape) | LF, CRLF | analyzer + fix | reproduced | no diagnostic vs reported and fixed |
| RH7108, same before the moved `add`'s `;` | LF, CRLF | analyzer + fix | reproduced | no diagnostic vs reported and fixed |
| RH7105/RH7106, `#endif` of a preceding conditional member in front of the first modifier | LF, CRLF | analyzer + fix | holds | reported and fixed, as specified |

## Not covered, and why

- A directive inside an expression body away from its end (`N(value, #if … #endif)`): the auditor observed it reported and fixed on this head; the excluded span already covers the arrow clause, so only the `;` gap is new.
- Formatter counterpart: RH7107/RH7108 are ordering rules with no formatter parity; the fix's formatter path is unchanged.
- Fix All: the new inputs add no co-occurrence the existing `GetFixAllScenario` does not exercise.

## Commands

```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~ExpressionBodyOfThe|FullyQualifiedName~ConditionalDirectiveInFrontOfTheFirstModifier"
```
