Role: test implementer
Commit: 38f04e2
Verdict: tests written

This is the third repair round. I covered the sentences that `38f04e2` adds to RH7105-001 and RH7106-001: each part of a `partial` type or member is judged by the modifiers that part carries (SHARED-DIAG-004). My prompt carried no intended fix or hoped-for verdict. I worked out coverage from the specs and the test classes, not from the suggestions in `21-spec-author.md`. The tests lane applies, so every new test pins existing, conforming behavior and passes on the unchanged code. No existing test or assertion changed; the diff only adds lines.

## Coverage, worked out again

- RH7106-001, partial member: `PartOfAPartialPropertyIsReportedAndFixedOnItsOwn` already covers it, with the tags `RH7106-001`, `RH7106-004` and `SHARED-DIAG-004`. One part writes `internal protected` and the other part writes `protected internal`. The test reports exactly the misordered part, and the fix touches only that part. No new test was needed for the member arm.
- RH7106-001, partial type: no test covered it. The sentence names "type or member" and the analyzer registers the two arms separately, as type and member declaration kinds. A new test is added.
- RH7105-001: no test had two parts where the other part is in canonical order. The `partial` tests that exist (`MisorderedModifiersWithPartialAreReportedAndFixed`, `AsyncBeforePartialMethodIsNotReported`, …) have one part, or no misordered part. New tests are added for a partial method and a partial type.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH7105DeclarationKeywordsMustFollowOrderAnalyzerTests.PartOfAPartialMethodIsReportedAndFixedOnItsOwn` | RH7105-001, RH7105-004, SHARED-DIAG-004 | `Verify(source, fixedSource)` (batch base) | pass: `static public partial void M();` is reported on `static`; the implementing part `public static partial void M() { }` is not reported and is left unchanged by the fix |
| `RH7105DeclarationKeywordsMustFollowOrderAnalyzerTests.PartOfAPartialTypeIsReportedAndFixedOnItsOwn` | RH7105-001, RH7105-004, SHARED-DIAG-004 | `Verify(source, fixedSource)` | pass: the second part `static public partial class C` is reported on that part; the first part `public static partial class C` is not reported and is left unchanged |
| `RH7106ProtectedMustComeBeforeInternalAnalyzerTests.PartOfAPartialTypeIsReportedAndFixedOnItsOwn` | RH7106-001, RH7106-004, SHARED-DIAG-004 | `Verify(source, fixedSource)` | pass: the second part of a nested `partial class Inner` that writes `internal protected` is reported on `protected`; the first part, which writes `protected internal`, is not reported and is left unchanged |

I checked that the tests can fail. I removed the expected-diagnostic markup from the reported part of each new test, and each test then failed with `expected "0" actual "1"`. So each test sees exactly one diagnostic, on the reported part. I restored the markup afterwards.

Observation, not a diagnosis: in the first draft of the two partial-type tests, both parts had empty bodies `{ }`. The code fix's output then wrote the reported part as `public static partial class C;` (and `protected internal partial class Inner;`). That is the empty-type form that `sdd/specs/shared/empty-type-declarations.md` describes. It has nothing to do with the sentence under test, so each part now carries one `public const` member and the fixed source stays a pure modifier change.

## Not covered, and why
- Both parts of one partial declaration misordered (two diagnostics, Fix All across parts): the sentence is about the part that conforms, and a single test can pin it. Fix All across several diagnostics in one document is already covered by each class's `GetFixAllScenario`.
- LF/CRLF, comment and directive in the gap: these tests involve no layout or trivia decision. The modifier rewrite and its trivia are covered by the SHARED-MODORDER-002/003 tests in both classes, and they do not change.
- Formatter parity: RH7105/RH7106 are not about formatter layout. The run changes no production code.
- Failure classes: the run is in the tests lane and changes no production code, so no condition moves (FC-003 needs no extra arm). FC-017: each test pins both the location of the report and the unchanged other part, and a bare "a diagnostic exists" would be weaker. FC-035: the expected values come from the spec sentences and the SHARED-DIAG-004 example, not from patched code.

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Ordering.RH7105DeclarationKeywordsMustFollowOrderAnalyzerTests|FullyQualifiedName~Reihitsu.Analyzer.Test.Ordering.RH7106ProtectedMustComeBeforeInternalAnalyzerTests"
scripts/test.sh --project architecture
```

Results on this head: analyzer `Passed: 45, Failed: 0`; architecture `Passed: 72, Failed: 0`. `scripts/format.sh` on both test files: `Formatted 0 of 2 file(s).`
