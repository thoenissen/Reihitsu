Role: test implementer
Commit: d0c6e803
Verdict: #1865: tests written; #1866: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## thoenissen/Reihitsu#1865

### Tests

Analyzer tests (`src/Reihitsu.Analyzer.Test/Design/`), existing tests whose asserted defaults change (1A):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH2008…AnalyzerTests.DefaultTestLimitIsOneHundredTwenty` (was `…OneHundred`; 120 not reported, 121 reported "…limit of 120.") | RH2008-001, SHARED-LINES-002 | `VerifyLines` (`Verify`) | fails: 120 reported against 100 |
| `RH2008…AnalyzerTests.ConfiguredProductionSettingLeavesTestDefault` (70 not reported, 121 reported against 120) | RH2008-001, SHARED-LINES-002 | same | fails |
| `RH2008…AnalyzerTests.InvalidTestSettingKeepsDefaultTestLimit` (3 rows; 120/121) | SHARED-LINES-002, RH2008-001 | same | fails |
| `RH2008…AnalyzerTests.ProductionSettingDoesNotApplyToTestCode` (summary text only: default 120) | SHARED-LINES-002, RH2008-001 | same | passes |
| `RH2111…AnalyzerTests.DefaultProductionLimitIsFifteenHundred` (was `…FiveHundred`; 1500/1501) | RH2111-001, SHARED-LINES-002 | same | fails: 1500 reported against 500 |
| `RH2111…AnalyzerTests.DefaultTestLimitIsThreeThousand` (was `…OneThousand`; 3000/3001) | RH2111-001, SHARED-LINES-002 | same | fails |
| `RH2111…AnalyzerTests.ConfiguredProductionSettingLeavesTestDefault` (700 not reported, 3001 reported against 3000) | RH2111-001, SHARED-LINES-002 | same | fails |
| `RH2111…AnalyzerTests.ConfiguredTestSettingLeavesProductionDefault` (1500/1501) | RH2111-001, SHARED-LINES-002 | same | fails |
| `RH2111…AnalyzerTests.InvalidSettingKeepsDefaultProductionLimit` (7 rows, incl. `maximumTypeLines: 0`; 1500/1501) | RH2111-001, SHARED-LINES-002 | same | fails |
| `RH2111…AnalyzerTests.InvalidTestSettingKeepsDefaultTestLimit` (3000/3001) | RH2111-001, SHARED-LINES-002 | same | fails |

Observed on cad07133: 17 failed, 155 passed in the two classes; every failure is "expected 1 diagnostic, actual 2" with the old limit in the message (e.g. "This member has 120 lines of code, which exceeds the limit of 100.").

Self-hosting tests (`src/Reihitsu.SelfHosting.Test/`), no requirement behind them (repository build and scan, not a specified surface), so untagged:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Analyzers/LineCountSelfHostingTests.ProjectModelMarksExactlyTheTestProjectsAsTestProjects` — `SelfHostingProject.Load(...).IsTestProject` is true for exactly the 8 test projects (explicit list) | FC-044 (scan models MSBuild) | — | does not compile (API missing) |
| `…ScanJudgesMethodInTestProjectByTheTestMethodLimit` — synthetic file added to the `Reihitsu.Core.Test` compilation, scan path; 120 not reported, 121 reported "…limit of 120." | FC-003 | `AnalyzerViolationReport.Collect` with options | does not compile |
| `…ScanJudgesMethodInProductionProjectByTheProductionMethodLimit` — same in `Reihitsu.Core`; 60/61 | FC-003 | same | does not compile |
| `…ScanJudgesTypeInTestProjectByTheTestTypeLimit` — test project, 3000/3001 against 3000 (root `reihitsu.json` does not lower test types) | FC-003 | same | does not compile |
| `…ScanJudgesTypeInProductionProjectByTheRepositoryTypeLimit` — production project, 500/501 against 500 (from the root `reihitsu.json`) | FC-003 | same | does not compile |
| `…RepositoryConfigurationIsPassedAndValid` — the scan's options hold exactly the root `reihitsu.json` as additional file; RH0001 over `Reihitsu.Core` with them reports nothing (also non-source locations, which `Collect` filters out) | — | direct `WithAnalyzers` | does not compile |
| `Analyzers/AnalyzerSelfHostingTests.AnalyzerReportsNoViolationsOnOwnSourceCode` — now has rows for RH2008 and RH2111 and passes the scan's options per project | — | `Collect` with options | does not compile; once it does, the RH2008/RH2111 rows fail until the violations are fixed |
| `Analyzers/AnalyzerViolationReportTests.LineCountAnalyzersHaveARow` (was `…HaveNoRow`) | — | — | passes (row list is test code) |
| `Analyzers/AnalyzerViolationReportTests.AnalyzerRowsMatchTheAnalyzersTheCompilerLoadsFromTheAnalyzerAssembly` — no analyzer is left out any more | — | — | passes |
| `Repository/BuildConfigurationTests.RootConfigurationSetsOnlyTheProductionTypeLimitOfFiveHundred` — root `reihitsu.json` has exactly `design.maximumTypeLines = 500` (2A) | — | — | fails: no root `reihitsu.json` |
| `…RootConfigurationSetsNoLimitAtOrAboveTheShippedDefault` — every `design` setting is one of the four limits and strictly below 60/120/1500/3000 (decision 10) | — | — | fails: no file |
| `…RepositoryBuildPassesRootConfigurationToTheAnalyzers` — root `Directory.Build.props`/`.targets` hold one `AdditionalFiles` item ending in `/reihitsu.json` | — | — | fails |
| `…RepositoryBuildImportsThePackageBuildFile` — root build files import `src/Reihitsu.Analyzer.Package/buildTransitive/Reihitsu.Analyzer.targets` once (decision 2) | FC-015 (one statement) | — | fails |
| `…BuildMarksTestProjectAsTestProjectForTheAnalyzers` — MSBuild's generated `obj/<Config>/<tfm>/Reihitsu.SelfHosting.Test.GeneratedMSBuildEditorConfig.editorconfig` holds `build_property.IsTestProject = true` and the `IsTestingPlatformApplication` key | FC-044 | — | fails: key absent |
| `…BuildDoesNotMarkProductionProjectAsTestProjectForTheAnalyzers` — same file of `Reihitsu.Analyzer` holds both keys, neither `true` | FC-003 | — | fails: keys absent |

Analyzer-resource tests (3C), `src/Reihitsu.Analyzer.Test/Resources/AnalyzerResourcesTests.cs` (rewritten; the old reflection test is gone):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `EveryAnalyzerPublishesTitleAndMessageFormat` | FC-036 | — | does not compile (API missing) |
| `EveryAnalyzerPublishesTheResourcesItsDiagnosticIdNames` — `Title`/`MessageFormat` equal the resources `<ID>Title`/`<ID>MessageFormat`, and equal the descriptor's title and message | FC-036 | — | does not compile |
| `EveryRuleResourceBelongsToAShippedAnalyzer` — every key of the resource set is `<shipped ID>Title`/`MessageFormat` | FC-036 | — | does not compile |
| `AnalyzerResourcesHoldsNoPropertyPerRule` — `AnalyzerResources` keeps `ResourceManager`, no static property named `RH…` | — | — | does not compile; with the API stubbed: fails (698 properties) |

### API the tests pin

The tests compile against these members, none of which exists yet:

- `DiagnosticAnalyzerBase`: public instance `string Title` and `string MessageFormat` (the rule's texts, current UI culture); constructors `(string diagnosticId, DiagnosticCategory category)` and, by the change doc, one with `isEnabledByDefault` — the two test probes `DiagnosticAnalyzerBaseProbe` and `ParenthesisRemovalProbeAnalyzer` call `base(id, category)`.
- `AnalyzerTestsBase<TAnalyzer>.MessageFormat` (test code, written here): `protected static`, reads `new TAnalyzer()` as `DiagnosticAnalyzerBase`. All 3330 `AnalyzerResources.RH####MessageFormat` call sites in 498 test files now read `MessageFormat`. I checked by script that in every file the replaced ID was the ID of the class's own `TAnalyzer` (the probe analyzer's ID is RH3002 and it used RH3002), so no expected message changes.
- `SelfHostingProject.IsTestProject` (`internal bool`), decided by the implementer from the project data the harness reads; the expected set is the explicit list of 8 projects.
- `SelfHostingSolution.GetAnalyzerOptions(string assemblyName)` → `AnalyzerOptions`: the options the build gives that project — `build_property.IsTestProject = true` for a test project, and the root `reihitsu.json` as additional file. Keyed by assembly name, because the tests pass a compilation with a synthetic tree added.
- `AnalyzerViolationReport.Collect(DiagnosticAnalyzer, IEnumerable<CSharpCompilation>, Func<CSharpCompilation, AnalyzerOptions>, string solutionRoot, CancellationToken)`. The existing 4-argument overload stays: the synthetic report tests call it, without options (production code).

Verification of the pinned API without touching the shared tree: in a scratch worktree (discarded) I added throwaway stubs — the two properties and the `(id, category)` constructor on `DiagnosticAnalyzerBase`, and stubs of the three harness members. With them, the whole `Reihitsu.Analyzer.Test` project ran in Debug with 18 failed and 11227 passed — exactly the 17 default-limit failures above plus `AnalyzerResourcesHoldsNoPropertyPerRule` (the stub keeps the properties), so the 3330 rewritten message references change no result — and the Release build of `Reihitsu.Analyzer.Test` was warning-free (the only Release errors were the missing harness members in `Reihitsu.SelfHosting.Test`); the `LineCountSelfHostingTests` failed for the right reasons: no project marked as test (expected 8, actual 0), test-project samples reported against production limits ("121 … limit of 60", "3001 … limit of 500"), options without `reihitsu.json`. Nothing of the stubs is committed.

Note: `ScanJudgesMethodInProductionProjectByTheProductionMethodLimit` passes on any path today, and `ScanJudgesTypeInProductionProjectByTheRepositoryTypeLimit` passes today only because the old default is 500; once the default is 1500 it tests that the scan reads the root `reihitsu.json`.

### Work the tests leave to the implementer

- The `AnalyzerSelfHostingTests` rows RH2008 and RH2111 pass only after every violation is fixed: 18 production methods, 9 test methods over 120, 5 production types over 500, 11 test types over 3000 (`01-spec-author.md`, `03-spec-author.md`). The test-code restructuring (splitting test classes and methods) changes no assertion, input, or expected output; I did not do it. The lead decides who does (it is test code; moving tests between files is not an assertion change).
- `scripts/test.sh --project selfhosting` runs `WorkingDocumentsTests`, which fails while `sdd/work/` exists — not related.

### Not covered, and why

- No formatter, code-fix, CLI, or naming behavior changes: FC-004 to FC-014, FC-018 to FC-030, FC-038 to FC-043 cannot reach the change. The production refactors keep behavior; the existing tests of the refactored code are the check (no expected value changes).
- Line endings: the defaults are compared with a count that existing tests already check per line ending (`RH2008…CountIsTheSameForEveryLineEnding`); a default value has no line-ending dimension.
- Build of the rules through `binaries/`: the committed binaries lack both rules (`02-skeptic.md`), so no test can run them through MSBuild in this run; `BuildMarks…`/`BuildDoesNotMark…` read MSBuild's own generated configuration instead, which is what the rules will read after the bump.
- `AdditionalFiles` is checked in the build files' XML, not in an MSBuild result: the generated editorconfig does not list additional files, and running MSBuild from a test is not a cheap check.
- FC-031 (stale summaries) and FC-032 (counts taken before the refactor) concern the implementer's work; the scan rows are the current list.
- Test-default examples below a boundary (e.g. the change doc's 110-line test method) are covered by the boundary pair 120/121; 1000-line production type by 1500/1501.

### Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH2008MethodsShouldNotExceedTheLineLimitAnalyzerTests|FullyQualifiedName~RH2111TypesShouldNotExceedTheLineLimitAnalyzerTests|FullyQualifiedName~AnalyzerResourcesTests|FullyQualifiedName~DiagnosticAnalyzerBaseTests"
scripts/test.sh --project selfhosting --filter "FullyQualifiedName~LineCountSelfHostingTests|FullyQualifiedName~BuildConfigurationTests|FullyQualifiedName~RulesetTests|FullyQualifiedName~AnalyzerViolationReportTests"
scripts/test.sh --project selfhosting --filter "FullyQualifiedName~AnalyzerSelfHostingTests"   # every analyzer row, RH2008 and RH2111 included
```

The whole `Reihitsu.Analyzer.Test` project does not compile until the `DiagnosticAnalyzerBase` API exists; the same holds for `Reihitsu.SelfHosting.Test` and the harness API.

### Follow-up candidates

_None._ (Observation: `src/Reihitsu.Analyzer.Test/Resources/CodeFixResourcesTests.cs:19` says "AnalyzerResources" in a summary about `CodeFixResources` — a pre-existing copy-paste slip, one comment line; the implementer may fix it in passing.)

## thoenissen/Reihitsu#1866

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `src/Reihitsu.SelfHosting.Test/Repository/RulesetTests.EveryReihitsuEntryNamesAShippedRule` (Debug, Release) — every `Rule Id` under `AnalyzerId="Reihitsu.Analyzer"` is a `SupportedDiagnostics` ID of a shipped analyzer (decision 6) | — | — | Debug fails: "names rules that no analyzer ships: RH08402"; Release passes |
| `RulesetTests.RulesetSwitchesOffRH8402` (Debug, Release) | — | — | Debug fails (no RH8402 entry); Release passes |
| `RulesetTests.ReleaseRulesetReportsLineCountRuleAsError` (RH2008, RH2111; #1865, decision 5) | — | — | fails: `None` |
| `RulesetTests.DebugRulesetHasNoEntryForLineCountRule` (RH2008, RH2111; #1865, decision 5) | — | — | fails: listed with `None` |

### Not covered, and why

The correction has no diagnostic effect (no `copyright` section in the root `reihitsu.json`, RH8402-001), so no analyzer test applies; the ruleset tests are the check.

### Commands

```shell
scripts/test.sh --project selfhosting --filter "FullyQualifiedName~RulesetTests"
```

### Follow-up candidates

_None._
