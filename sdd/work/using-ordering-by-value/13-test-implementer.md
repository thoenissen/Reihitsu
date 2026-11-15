Role: test implementer
Commit: b4be5bb
Verdict: tests written

Repair round 2. My prompt said the production code is expected to already satisfy these tests. I did not take that as a verdict: every test below was run on b4be5bb, and the result is reported as observed. Expected values come from the examples in `12-spec-author.md` (SHARED-USING-007, RH7203-001/-002, RH7206-001) and the Q2 answer in `00-lead.md`. I wrote no test from the code.

The tests discriminate against `main`'s key, which kept `global::`. `11-auditor.md` Finding 1 records what `main` does on these inputs: the formatter writes `global::System.IO; System; System.Collections`, and RH7203 reports nothing on `global::System.IO; System.Collections`. On `main`, `global::…` sorts before `System…` ("g" < "s", case-insensitive), so each negative test below would also get a report there. That makes both sides of the strip pinned (FC-003).

Literal Unicode escapes: `git grep -c 'u0053' -- src` gives the same per-file counts before and after this change (RH7207 analyzer tests 2, RH7201 1, RH7203 1, `UsingDirectiveNameValueTests` 2). The diff only adds lines. The insertions were made with a Python script so that no editing tool decoded an escape.

## thoenissen/Reihitsu#1105

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH7203 `GlobalQualifierIsNotPartOfTheSortKey` (spec example) | RH7203-001 | `Verify(source)` | pass (observed) |
| RH7203 `CodeFixPlacesADirectiveBeforeAGlobalQualifiedDirectiveThatSortsAfterIt` | RH7203-001, RH7203-004 | `Verify(source, fixed, diagnostic)` (re-analysis = convergence, FC-007) | pass (observed) |
| RH7203 `GlobalQualifiedDirectiveThatSortsAfterByItsValueIsNotReported` (spec example) | RH7203-002 | `Verify(source)` | pass (observed) |
| RH7206 `GlobalQualifierIsNotPartOfTheSortKey` (spec example) | RH7206-001 | `Verify(source)` | pass (observed) |
| RH7206 `GlobalQualifiedStaticDirectiveThatSortsAfterByItsValueIsNotReported` (other side, FC-003) | RH7206-002 | `Verify(source)` | pass (observed) |
| Formatter `GlobalQualifierIsNotPartOfTheSortKey` (spec example) | SHARED-USING-007 | `AssertRuleResult(input, expected)` (2 passes, LF+CRLF, phase stability) | pass (observed) |
| Formatter `GlobalQualifiedDirectiveInOrderByItsValueIsUnchanged` (stable valid code) | SHARED-USING-007 | `AssertRuleResult(input)` | pass (observed) |

Files:
- `src/Reihitsu.Analyzer.Test/Ordering/RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzerTests.cs`
- `src/Reihitsu.Analyzer.Test/Ordering/RH7206UsingStaticDirectivesMustBeOrderedAlphabeticallyAnalyzerTests.cs`
- `src/Reihitsu.Formatter.Test/Regression/UsingDirectives/UsingDirectiveNameValueTests.cs`

### Scenarios (bug fix)
| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `global::System.IO; System.Collections` | LF | RH7203 analyzer + fix | as specified | 1 diagnostic on `System.Collections`; fix gives `System.Collections; global::System.IO` |
| `System.Collections; global::System.IO` | LF | RH7203 | as specified | 0 diagnostics |
| `static global::System.Math; static System.Console` | LF | RH7206 | as specified | 1 diagnostic on `System.Console` |
| `static System.Console; static global::System.Math` | LF | RH7206 | as specified | 0 diagnostics |
| `System; global::System.IO; System.Collections` | LF, CRLF | formatter | as specified | `System; System.Collections; global::System.IO`, spelling kept, stable on pass 2 |

### Not covered, and why
- Code-fix CRLF variant for RH7203: the fix goes through the formatter's using phase, and the formatter test covers CRLF for the same reorder. The ordering decision does not depend on the line ending.
- RH7206 code-fix test: the spec author listed only the RH7206-001 diagnostic example. The fix runs the same Core `ComputeCanonicalOrder` that the formatter test pins.
- Global section (`global using global::…`) and the extern-alias qualifier (`Lib::X`): the spec author deliberately wrote no example (internal choices in `12-spec-author.md`). SHARED-USING-004 covers every section through the same `GetSortKey`.
- Fix All: there is one diagnostic per block in these shapes. The existing `GetFixAllScenario` overrides cover Fix All for RH7203/RH7206.
- FC catalog: unchanged from `04`/`06` and the auditor's dismissals. This round adds no production change, only pins the existing sort key.

## thoenissen/Reihitsu#1266
Nothing to do in this round.

## thoenissen/Reihitsu#1267
Nothing to do in this round.

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~GlobalQualifierIsNotPartOfTheSortKey|FullyQualifiedName~CodeFixPlacesADirectiveBeforeAGlobalQualifiedDirectiveThatSortsAfterIt|FullyQualifiedName~GlobalQualifiedDirectiveThatSortsAfterByItsValueIsNotReported|FullyQualifiedName~GlobalQualifiedStaticDirectiveThatSortsAfterByItsValueIsNotReported"
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.UsingDirectives.UsingDirectiveNameValueTests"
```
Observed: analyzer 5/5 passed, formatter 21/21 passed (19 existing + 2 new).
