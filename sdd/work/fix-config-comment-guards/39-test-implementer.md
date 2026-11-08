Role: test implementer
Commit: 036a4713 (base; this handover's commit adds the test change)
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict; it named decision 36 and the RH2001-009 example from `38-spec-author.md`, which are this step's inputs.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests.VerifyFixIsOfferedForAConditionalModifierBetweenTheModifiersAndTheType` (`"\n"`, `"\r\n"`) | RH2001-009, SHARED-FIX-009 | `Verify(source, fixedSource)` on `BatchCodeFixTestsBase` (diagnostic, one-pass fix, re-analysis, Fix All) | green on head |

Change: the count-only assertion (`GetCodeFixActionsAsync` + `Assert.HasCount(1, actions)`) is replaced by the full output of the RH2001-009 example "a conditional group at the place of `readonly`", copied from `sdd/specs/analyzer/RH2001.md`: `private` / `#if DEBUG` / `static` / `#endif` / `readonly int _value;` (at the type's column, 4 spaces), with the use in `Get()` renamed to `_value`. The test gained `[Requirement("RH2001-009")]`; `SHARED-FIX-009` stays, since the fix is still offered for the directive. A pinned output is stronger than the old count (the count is implied: `Verify` fails when no fix applies).

Watched it fail for the right reason: with the expected output temporarily set to main's placement (`readonly` on its own line in front of `#if DEBUG`, type at 8 spaces), both data rows failed on the output comparison; the file was restored afterwards.

## Scenarios (bug fix)
_Not applicable_: this step pins a spec example of a behavior change.

## Not covered, and why
- Groups holding a modifier that RH7105-001 puts behind `readonly` (for example `unsafe`, `new` behind the group): RH2001-009 leaves them uncovered by design (`38-spec-author.md`, *Internal choices*), so no output is specified to pin.
- Formatter parity: RH2001 is a code-fix rule (property to field); the formatter does not perform this rewrite, so there is no counterpart surface to compare.
- Fix All with several co-occurring instances: covered by the class's `GetFixAllScenario`; `Verify` already runs the batch fix on this input.

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests.VerifyFixIsOfferedForAConditionalModifierBetweenTheModifiersAndTheType"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests"
```
Whole RH2001 class on this head: 99 passed, 0 failed.
