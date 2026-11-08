Role: test implementer
Commit: 8afda2c (test change committed on top of it)
Verdict: tests written — #963: disputed test corrected; #1384: no change

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond decision 10 of `00-lead.md`; the dispute was checked on its own evidence.

## thoenissen/Reihitsu#963

### The dispute

`05-implementer.md` is right. `RH7101DoNotCombineFieldsAnalyzerTests.FixAllSplitsOnlyTheDeclarationWithoutACommentInThePartEveryFieldRepeats` marks the diagnostic RH7101-007 keeps (`{|RH7101:_b|}`) in its expected source but called `Verify(source, fixedSource)` without `MarkupMode.Allow` on the fixed states, so the harness did not read that markup as an expected diagnostic and failed with `Diagnostics of fixed state … expected "0" actual "1"` on the very diagnostic RH7101-007 requires to stay. That is a defect of my test (04), not of the code.

Correction: the test now passes an `onConfigure` that sets `FixedState.MarkupHandling` and `BatchFixedState.MarkupHandling` to `MarkupMode.Allow`, the repository's pattern for a diagnostic that remains after a fix (`RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzerTests`). Source, fixed source, and requirement tags are unchanged. The test now asserts both halves of RH7101-007 on the iterative and the Fix All path: `_c, _d` and `_e, _f` are split, and `private int /* c */ _a, _b;` stays whole with its diagnostic reported and no fix applied.

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH7101DoNotCombineFieldsAnalyzerTests.FixAllSplitsOnlyTheDeclarationWithoutACommentInThePartEveryFieldRepeats` (changed: harness configuration only) | RH7101-007, RH7101-004 | `Verify(source, fixedSource, onConfigure)` | pass |

### Scenarios (bug fix)
| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Corrected test on HEAD production (1ac2927) | LF | code fix, iterative + Fix All | pass | as expected |
| Corrected test with the three production files of `1ac2927^` checked out temporarily (restored afterwards) | LF | code fix, iterative + Fix All | fail, for the issue's difference | `Iterative code fix application` diff: expected `private int /* c */ _a, _b;`, observed `private int /* c */ _a;` + `private int /* c */ _b;` — the comment copied onto every field |

### Not covered, and why
Nothing new; CRLF and the single-fix path for this input stay covered by the tests listed in `04-test-implementer.md`.

## thoenissen/Reihitsu#1384

No change; its tests pass on HEAD (`05-implementer.md`).

## Commands

```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH7101DoNotCombineFieldsAnalyzerTests"
```
`Passed! - Failed: 0, Passed: 104, Total: 104`
