Role: test implementer
Commit: b8b330fa (tests added on top of this head)
Verdict: tests written

Scope: decision 53, audit 7 finding #23 only (r3 and r4). No intended fix or cause was carried in the prompt beyond the decision's scope.

## Tests
File: `src/Reihitsu.Analyzer.Test/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests.cs`

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyNoCodeFixIsOfferedForAConditionalModifierInFrontOfTheTypeOfAGetOnlyPropertyWithoutModifiers` (`\n`, `\r\n`) | RH2001-009 | `VerifyNoCodeFixIsOffered` | red on head: an action is offered (`Assert.IsEmpty(actions)`), both line endings |
| `VerifyNoCodeFixIsOfferedForAConditionalModifierInFrontOfTheTypeOfAReadWritePropertyWithoutModifiers` (`\n`, `\r\n`) | RH2001-009 | `VerifyNoCodeFixIsOffered` | red on head: an action is offered, both line endings |

Input of both: `#if DEBUG` / `extern` / `#endif` / `int Value { get; }` (resp. `{ get; set; }`) in a class, with a reading method. The tag relies on RH2001-009 being reworded per decision 53 to cover the type of a property without modifiers; the current text ("of the type when a modifier stands in front of it") does not yet cover it.

## Not covered, and why
- r5, r6 (#23) and #24/#25: outside decision 53 (the lead's scope is r3/r4; r6 becomes withheld as on main, not asked to be pinned).
- Fix All / convergence: no-fix tests; nothing to apply.

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests.VerifyNoCodeFixIsOfferedForAConditionalModifierInFrontOfTheType"
```
