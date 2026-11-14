Role: test implementer
Commit: 1907dbd
Verdict: tests written

Follow-up spawn for Decision 7 (disputed pre-existing RH7203 test, `05-implementer.md` *Disputed test*). No intended fix, suspected cause, or hoped-for verdict was in the prompt beyond the lead's recorded decision.

## thoenissen/Reihitsu#1105

Nothing to do in this spawn.

## thoenissen/Reihitsu#1266

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzerTests.MicrosoftUsingsAreFixedWithoutMovingSortedSystemGroup` (pre-existing, corrected) | RH7203-001, RH7203-004 (tags unchanged) | `Verify(source, fixedSource, Diagnostics(..., 3))` on `BatchCodeFixTestsBase` | pass (232 of 232 RH720 tests) |

Correction: the block
```csharp
using Microsoft.CodeAnalysis.CSharp;
using {|#0:Microsoft.CodeAnalysis|};
using {|#1:Microsoft.CodeAnalysis.CodeActions|};
using {|#2:Microsoft.CodeAnalysis.CodeFixes|};
```
now marks `…CodeActions` and `…CodeFixes` and expects three diagnostics instead of one. Both sort before the earlier `Microsoft.CodeAnalysis.CSharp` of their group (`Co` < `CS`, case-insensitive ordinal), which RH7203-001 and SHARED-USING-007 ("every earlier directive of its group") demand to be reported — the same shape as the RH7203-001 example `Microsoft.C; {|RH7203:Microsoft.A|}; {|RH7203:Microsoft.B|}`. The `fixedCode` is unchanged (RH7203-004 organizes the whole block). A comment above the `Verify` call states why the two extra directives are reported. On `main` this test now fails (1 reported, 3 expected), which is the adjacent-only behavior #1266 reports.

### Audit of other tests for adjacent-only assertions
- `rg -ln "RH7203|RH7204|RH7206" src --glob '*.cs'` outside production: only the three analyzer test classes and `src/Reihitsu.Formatter.Test/Regression/UsingDirectives/UsingDirectiveOrderingTests.cs` (a comment naming RH7204; it asserts formatter output, not diagnostics).
- Every `Verify` in the three analyzer test classes asserts the exact diagnostic set, so any other test that encoded adjacent-only comparison would fail against the implementer's every-earlier comparison; the RH720 run is green after this correction, so no other test asserts it.
- `rg -n -i "adjacent|predecessor|preceding|previous|directly before|right before|neighbo"` over the three classes: `EveryDirectiveSortedBeforeItsNeighborIsReportedOnItsName` (RH7203), `EveryAliasSortedBeforeItsNeighborIsReportedOnItsAliasName` (RH7204), `EveryStaticDirectiveSortedBeforeItsNeighborIsReportedOnItsTypeName` (RH7206). Each holds at most two directives per group, so its assertion is correct under both comparisons and its name/summary ("directly above it") describes its input truthfully; left unchanged. The every-earlier behavior is pinned by the reproduction tests of `04-test-implementer.md` and the corrected test above.

### Scenarios
_N/A._ Correction of an existing test, not a new reproduction.

## thoenissen/Reihitsu#1267

Nothing to do in this spawn.

## Not covered, and why
- No new tests: the decision asked only for the correction and the audit; the every-earlier behavior already has its reproductions (`04-test-implementer.md`).
- LF/CRLF, formatter parity: analyzer-only diagnostic count; the fixed code and the formatter path are unchanged.

## Commands
```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH720"
```
Result: Passed 232, Failed 0, Total 232.
