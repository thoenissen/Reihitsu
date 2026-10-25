Role: test implementer
Commit: d2ec189 (tests written on top of it; see the commit that adds this file)
Verdict: tests written

The prompt carried no intended fix or verdict.

## What exists now

New test project `src/Reihitsu.SelfHosting.Test/Reihitsu.SelfHosting.Test.csproj` (plain `Microsoft.NET.Sdk`, MSTest 4.5.1, `Parallelize(Workers = 0, Scope = MethodLevel)`, references `Reihitsu.Analyzer` and `Reihitsu.Formatter`). It is **not** in `Reihitsu.sln`, not in `scripts/test.sh`, and not in `.github/workflows/SonarCloud.yml` — that wiring is the implementer's, and the architecture tests below fail until it exists. The project name ends in `.Test`, so `RequirementReferenceCatalog` (ArchitectureTests) picks up its `[Requirement]` tags.

- `Analyzers/AnalyzerSelfHostingTests.cs` — the analyzer scan, moved from `src/Reihitsu.Analyzer.Test/SelfHosting/SelfHostingTests.cs` (deleted). One `[DynamicData]` row per analyzer, the row value being the analyzer type's simple name, so a row shows as `AnalyzerReportsNoViolationsOnOwnSourceCode ("RH4106PrivateFieldCasingAnalyzer")`. Each row runs only its own analyzer and fails with `path:line [ID] (Analyzer) message` lines.
- `Utilities/SelfHostingSolution.cs` — project discovery and compilation building, moved unchanged in logic from the old test; the compilations are built once per test process in a `Lazy<T>` with `ExecutionAndPublication` and shared by every row (a build failure is rethrown to every row). `Utilities/SelfHostingProject.cs` is moved by `git mv` (namespace changed only).
- `Utilities/AnalyzerCatalog.cs`, `Utilities/AnalyzerViolationReport.cs` — row source and the per-analyzer run and report.
- `Formatting/FormatterSelfHostingTests.cs` — the two formatter scans, moved from `src/Reihitsu.Formatter.Test/Idempotency/SelfHostingTests.cs` (deleted), split into one row per source file (`FormatterIsIdempotentOnSourceFile`, `FormatterProducesNoChangesOnSourceFile`), each row checking LF and CRLF. Both keep their `[Requirement("FMT-PIPE-004")]` tag.
- `Utilities/FormatterSourceCatalog.cs` — the scanned directories: the old list plus `src/Reihitsu.SelfHosting.Test`, so the moved code stays covered as it was while it lived in `Analyzer.Test`/`Formatter.Test`. Generated-file skipping is now shared with the analyzer scan and also skips `.g.i.cs` (the old formatter list skipped `.Designer.cs` and `.g.cs` only; no `.g.i.cs` file exists in the scanned directories).
- `src/Reihitsu.Analyzer.Test/SelfHosting/Utilities/AnalyzerMetadataDiscovery.cs` — anchored its assembly lookup on `typeof(AnalyzerMetadataDiscovery)` instead of the deleted `SelfHostingTests`; same assembly, no behavior change. The fast convention tests under `Analyzer.Test/SelfHosting/` stay where they are.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Reihitsu.ArchitectureTests.General.TestScriptProjectSelectionTests.ShellAllIncludesEverySolutionTestProjectExceptSelfHosting` (renamed from `ShellAllIncludesEverySolutionTestProject`) | tool: default run = solution test projects minus self-hosting | file parse of `scripts/test.sh`, `Reihitsu.sln` | fail (solution lacks the project) |
| `...TestScriptProjectSelectionTests.ShellAllLeavesOutTheSelfHostingProject` | tool: default run skips the scans | `scripts/test.sh` parse | pass |
| `...TestScriptProjectSelectionTests.NamedSelfHostingSelectionSelectsOnlyTheSelfHostingProject` | tool: `--project selfhosting` selects exactly `src/Reihitsu.SelfHosting.Test/Reihitsu.SelfHosting.Test.csproj` | `scripts/test.sh` parse | fail (no alias) |
| `...TestScriptProjectSelectionTests.SolutionListsTheSelfHostingTestProject` | tool: solution build and whole-solution `dotnet test` include it | `Reihitsu.sln` parse | fail |
| `...TestScriptProjectSelectionTests.SonarCloudTestLoopRunsTheSelfHostingProject` | tool: CI runs the scans on every PR | `SonarCloud.yml` `for project in ...; do` loop | fail |
| `...TestScriptProjectSelectionTests.SonarCloudTestLoopRunsEverySolutionTestProjectExceptArchitecture` | tool: no solution test project is missing from the CI loop (architecture runs in `build.yml`) | `SonarCloud.yml` + `Reihitsu.sln` | pass now; fails once the solution lists the project and the loop does not |
| `...TestScriptProjectSelectionTests.NamedArchitectureAndToolingSelectionsRemainNarrow` | unchanged | | pass |
| `Reihitsu.SelfHosting.Test.Analyzers.AnalyzerSelfHostingTests.AnalyzerReportsNoViolationsOnOwnSourceCode` (340 rows) | moved scan | `SelfHostingSolution`, `AnalyzerViolationReport` | pass |
| `Reihitsu.SelfHosting.Test.Analyzers.AnalyzerViolationReportTests.AnalyzerRowsNameEachAnalyzerTypeExactlyOnce` | tool: one uniquely named row per analyzer | | pass |
| `...AnalyzerViolationReportTests.AnalyzerRowNameCreatesThatAnalyzer` | tool: row name resolves to its analyzer | | pass |
| `...AnalyzerViolationReportTests.ReportNamesFileLineRuleAndAnalyzerOfAViolation` | tool: a violation names file, line, rule (issue's done-when) | synthetic compilation + RH4106 | pass |
| `...AnalyzerViolationReportTests.ReportIsEmptyForCodeWithoutViolations` | tool: other side of the condition | synthetic compilation + RH4106 | pass |
| `...AnalyzerViolationReportTests.ReportListsViolationsOfEveryCompilationOrderedByFileAndLine` | tool: every compilation reported, ordered by file then position | two synthetic compilations | pass |
| `Reihitsu.SelfHosting.Test.Formatting.FormatterSelfHostingTests.FormatterIsIdempotentOnSourceFile` (1621 rows) | FMT-PIPE-004 | `ReihitsuFormatter.FormatSyntaxTree`, LF and CRLF | pass |
| `...FormatterSelfHostingTests.FormatterProducesNoChangesOnSourceFile` (1621 rows) | FMT-PIPE-004 | `ReihitsuFormatter.FormatSyntaxTree`, LF and CRLF | pass |
| `...FormatterSelfHostingTests.SourceFileRowsCoverEveryScannedDirectoryAndSkipBuildOutput` | tool: rows unique, forward-slash, cover every directory, no bin/obj/generated | | pass |

Evidence that the tests decide the right thing:

- The four failing architecture tests fail on their own assertions (missing `selfhosting)` branch, solution set, CI loop), not on compile or parse errors. With a temporary local wiring (a `selfhosting)` line in `scripts/test.sh`, `dotnet sln add`, then the name appended to the SonarCloud loop — all reverted, nothing of it committed) all seven pass; with the solution entry but without the SonarCloud entry, both SonarCloud tests fail.
- Mutating `AnalyzerViolationReport` (zero-based line number, no ordering; reverted) fails `ReportNamesFileLineRuleAndAnalyzerOfAViolation` and `ReportListsViolationsOfEveryCompilationOrderedByFileAndLine`.
- A deliberate violation `src/Reihitsu.Core/DeliberateViolation.cs` (a private field `Value`; removed again) failed the row `AnalyzerReportsNoViolationsOnOwnSourceCode ("RH4106PrivateFieldCasingAnalyzer")` with `src/Reihitsu.Core/DeliberateViolation.cs:11 [RH4106] (RH4106PrivateFieldCasingAnalyzer) Private field names should be in _camelCase`, and the row `FormatterProducesNoChangesOnSourceFile ("src/Reihitsu.Core/DeliberateViolation.cs")` with a per-line diff.

Measured on this 4-core sandbox (Release, `--no-build`, with the temporary wiring): analyzer rows 340 passed in 16 m 17 s wall-clock; formatter rows 3243 passed in 2 m 15 s (7 m 54 s CPU). The old single-method analyzer scan was not run here for comparison; the issue's "drops measurably on a 4-core runner" is a CI observation.

## Not covered, and why

- FC-001 to FC-030, FC-036: no analyzer, code-fix, or formatter code changes; the lane is tooling.
- FC-017 / FC-033 (weaker or normalized evidence): the formatter rows keep both line endings and the second-pass comparison of the old scans; the analyzer rows keep the old compilation and reference model unchanged.
- `scripts/test.sh` usage text, error messages, and `scripts/README.md`: documentation, owned by the implementer; no test parses them.
- `Assemblies.All` in ArchitectureTests does not load the new assembly: its only rules are about enumerations, the new project declares none, and loading it would make ArchitectureTests depend on building the scans.
- No test asserts that `Analyzer.Test`/`Formatter.Test` no longer contain a scan; the scans are deleted there, and `ShellAllLeavesOutTheSelfHostingProject` plus the solution-set test guard where they live.

## Follow-up candidates (outside my files)

- `squad/roles/test-implementer.md:53` names `SelfHostingTests` as the whole-repository formatter-stability helper; it is now `FormatterSelfHostingTests` in `Reihitsu.SelfHosting.Test`, run in CI only. The implementer owns `squad/roles/`.

## Commands

```shell
scripts/test.sh --project architecture --filter "FullyQualifiedName~TestScriptProjectSelectionTests"
# after the implementer adds the selfhosting alias:
scripts/test.sh --project selfhosting --filter "FullyQualifiedName~AnalyzerViolationReportTests|FullyQualifiedName~SourceFileRowsCover"
scripts/test.sh --project selfhosting --filter "FullyQualifiedName~AnalyzerSelfHostingTests"
scripts/test.sh --project selfhosting --filter "FullyQualifiedName~FormatterSelfHostingTests"
# one analyzer row:
scripts/test.sh --project selfhosting --filter "Name~RH4106PrivateFieldCasingAnalyzer&FullyQualifiedName~AnalyzerSelfHostingTests"
```
