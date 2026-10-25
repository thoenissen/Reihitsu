Role: test implementer
Commit: 48e5417 (tests changed on top of it; see the commit that adds this file)
Verdict: tests written

Repair round 1, worklist: auditor finding 3 (`05-auditor.md`, FC-017) only. The prompt carried no intended fix or hoped-for verdict. Auditor finding 1 was accepted by the maintainer (`squad/decisions/2026-10-25-self-hosting-analyzer-scan-wall-clock.md`) and is not mine; finding 2 is text owned by the spec author.

## What changed

- `src/Reihitsu.SelfHosting.Test/Analyzers/AnalyzerViolationReportTests.cs`
  - `AnalyzerRowsNameEachAnalyzerTypeExactlyOnce`: the tautological `Assert.HasCount(AnalyzerCatalog.DiscoverAnalyzerTypes().Count(), names)` is removed. The test keeps its two real assertions (RH4106 has a row; no two rows share a name).
  - New `AnalyzerRowsMatchTheAnalyzersTheCompilerLoadsFromTheAnalyzerAssembly`: checks the row names against an enumeration that does not go through `AnalyzerCatalog.DiscoverAnalyzerTypes`: Roslyn's own discovery, `AnalyzerFileReference.GetAnalyzers(LanguageNames.CSharp)` over the `Reihitsu.Analyzer` assembly. That is how the compiler finds analyzers in the package: it reads `[DiagnosticAnalyzer]` attributes from metadata. The test checks set equality both ways (no analyzer the compiler loads lacks a row, and no row names a type the compiler would not load), and fails on any `AnalyzerLoadFailed` event and on an empty discovery. The failure messages list the names on each side of the difference.
- `src/Reihitsu.SelfHosting.Test/Utilities/LoadedAnalyzerAssemblyLoader.cs` (new): an `IAnalyzerAssemblyLoader` that returns the analyzer assembly already loaded in the test process, and throws for any other path. Roslyn has no public default loader. Returning the loaded assembly keeps discovery and rows on the same assembly.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Reihitsu.SelfHosting.Test.Analyzers.AnalyzerViolationReportTests.AnalyzerRowsNameEachAnalyzerTypeExactlyOnce` | tool: row names are unique, RH4106 has a row | – | pass |
| `Reihitsu.SelfHosting.Test.Analyzers.AnalyzerViolationReportTests.AnalyzerRowsMatchTheAnalyzersTheCompilerLoadsFromTheAnalyzerAssembly` | tool: one row for each analyzer; FC-017 repair | `AnalyzerFileReference` + `LoadedAnalyzerAssemblyLoader` | pass |

Neither test carries a `[Requirement]` tag: they check a tool, not a spec requirement.

Evidence that the new test can fail, and fails for the right reason. Each mutation was a temporary edit to `Utilities/AnalyzerCatalog.cs`, reverted byte for byte from a backup. Nothing of it is committed:

- Dropping analyzers from discovery (`&& type.Name.StartsWith("RH10", …) is false`) fails the new test with `Analyzers the compiler loads but no row runs: RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer, RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer, RH1003UseStringInterpolationInsteadOfStringConcatenationAnalyzer`. The other 5 tests of the class pass. Before this repair, `AnalyzerRowsNameEachAnalyzerTypeExactlyOnce` passed under the same mutation, which is the auditor's observation.
- Adding a non-analyzer row (`.Append(typeof(DiagnosticAnalyzerBase))`) fails the new test with `Rows for types the compiler does not load as analyzers` (count 1).
- On the unchanged catalog, all 6 tests of the class pass, so Roslyn's discovery and the row set are equal today.

## Not covered, and why

- Failure classes FC-001 to FC-016 and FC-018 to FC-037: no analyzer, code-fix, formatter, or Core code changes, so they cannot reach these tests (the same scope as `01` and `05`). FC-017 is the class this repair addresses.
- The 340 analyzer rows were not re-run: neither the rows nor `AnalyzerCatalog` changed. The two changed files are covered by the build, where analyzer violations are errors and the Release build was clean, and by their formatter self-hosting rows (run below, green).

## Commands

```shell
scripts/test.sh --project selfhosting --filter "FullyQualifiedName~AnalyzerViolationReportTests"
scripts/test.sh --project selfhosting --filter "(FullyQualifiedName~FormatterSelfHostingTests&(Name~LoadedAnalyzerAssemblyLoader|Name~AnalyzerViolationReportTests))|Name~SourceFileRowsCover"
```
