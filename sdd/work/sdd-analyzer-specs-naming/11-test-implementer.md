Role: test implementer
Commit: 561d0bbdee60106d46851f2a5b128a0d88e75ce2
Verdict: tests written

Repair round for finding 1 of `09-spec-checker.md`. The coordinator's message named the files to read and the task. It carried no intended fix or hoped-for verdict.

I read the new example text in `sdd/specs/analyzer/RH4008.md` and `RH4009.md`, as `10-spec-author.md` describes it (`a569638`). Each test that covers one of the two examples now runs the new text word for word. No production code, spec, or other test changed.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH4008 `EachBlockScopedNamespaceSegmentThatIsNotPascalCaseIsReported` | RH4008-001 — its first declaration is now `namespace Company.{|RH4008:product|}.Data.{|RH4008:access|}` | `Verify(source)` | pass |
| RH4009 `NamingSectionWithoutAllowListIsInactive` | RH4009-001 (`{ "naming": { } }`) — `namespace Company.Product` | `reihitsu.json` additional file | pass |
| RH4009 `AllowListWithoutStringEntryIsInactive` | RH4009-001 (`[ 1 ]`) — same declaration | same | pass |
| RH4009 `EntriesThatAreNotStringsAreIgnoredAndStringEntriesStayInEffect` | RH4009-001 (`[ "A", 1 ]`) — `namespace {|RH4009:Company.Product|}` | same | pass |
| RH4009 `NoConfiguration` (existing) | RH4009-001 (no configuration file) | `Verify(source)`, now with a local `testCode` that holds the example text | pass |
| RH4009 `EmptyConfiguration` (existing) | RH4009-001 (`[]`) | same local `testCode` | pass |
| RH4009 `InvalidConfiguration` (existing) | RH4009-001 (malformed JSON) | same local `testCode` | pass |

Notes on the existing tests:

- `NoConfiguration`, `EmptyConfiguration` and `InvalidConfiguration` predate this run. I tagged them in `07-test-implementer.md` as the tests that cover the configurations in the RH4009-001 example prose. So each now declares the example text in a local `testCode` instead of using the shared `TestCode` constant.
- The malformed JSON in `InvalidConfiguration` keeps its entry `"TestNameSpace"`. That entry now differs from the declared `Company.Product`. If the JSON were parsed, the namespace would be reported, so the test can now fail where before it could not.
- The `TestCode` constant (`using System; namespace TestNameSpace { }`) is unchanged. It is still used by `InvalidNamespace` and `ValidNamespace`, which predate this run and cover RH4009-002, not the example.

## Not covered, and why

- `10-spec-author.md` raises the RH4001 examples as a follow-up candidate for the lead: `Test0.cs`, `TestClass`, and `TestComponent`. Their specs did not change, so their tests did not either.

## Commands

```shell
scripts/format.sh src/Reihitsu.Analyzer.Test/Naming/RH4008NamespaceCasingAnalyzerTests.cs src/Reihitsu.Analyzer.Test/Naming/RH4009NamespaceNotAllowedAnalyzerTests.cs
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH4008NamespaceCasingAnalyzerTests|FullyQualifiedName~RH4009NamespaceNotAllowedAnalyzerTests"
scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"
```

Results on this head:

- `scripts/format.sh`: `Formatted 0 of 2 file(s)`.
- RH4008 and RH4009 tests: 14 passed.
- `Sdd` architecture tests: 39 passed.
