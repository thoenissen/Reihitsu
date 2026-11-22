Role: test implementer
Commit: 4022a0c
Verdict: tests written

Repair round for decision 2 in `00-lead.md`. Read: `00-lead.md` (decision 2), `04-test-implementer.md`, `05-implementer.md` (#971 section and *Disputed tests or scope*). The prompt carried a suggested test shape (an additional non-colliding field), which matches the lead's recorded decision 2; it carried no suspected cause or hoped-for verdict. Production code untouched.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH4106PrivateFieldCasingAnalyzerTests.FixAllSkipsAFieldWithALeadingAcronymThatCollidesWithAnotherField` (reshaped) | SHARED-CASING-010, RH4106-007 | `Verify(source, fixedSource, …)` with `BatchFixedState` | Passes on `bc565c2`; fails on the pre-implementation `src/` (`93c54a8`) in the iterative single fix with `_hTTPClient` instead of `_httpClient` |

What changed in the test:
- Input gains a third reported field `Count`, whose derived name `_count` collides with nothing, and a reference to it.
- Single-fix expected output (`FixedState`): unchanged for the colliding pair (`HTTPClient` → `_httpClient`, `HttpClient` kept with its diagnostic); `Count` → `_count` added.
- Fix All expected output (`BatchFixedState`): a new `batchFixedCode` instead of the unchanged input. Both colliding fields `HTTPClient` and `HttpClient` stay with their diagnostics (the asserted skip is unchanged); only `Count` → `_count`.
- `NumberOfFixAllIterations = -2` with the comment the file and RH4107 already use for a skipped group: the remaining diagnostics keep the framework's Fix All loop running for a second pass that changes nothing. An exact `1` fails with "Expected '1' iterations but found '2'"; an exact `2` passes with the same final state; `-2` follows the existing convention (`RH4106…:1315`, `RH4107…:268`, `RH4109…:308`).
- Summary sentence updated to name the non-colliding field.

No assertion was weakened: the skipped pair and the single fix's outputs are asserted exactly as before.

## Not covered, and why
Nothing new in scope; the matrix of `04-test-implementer.md` stands. This round only repairs the test shape the framework rejects.

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4106PrivateFieldCasingAnalyzerTests"
```
Result on `4022a0c`: 47 passed, 0 failed.

## Follow-up candidates
_None._
