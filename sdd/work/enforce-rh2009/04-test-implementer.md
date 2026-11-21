Role: test implementer
Commit: 3ebf2d4
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore. The issue is a roll-out, not a bug, so there is no reproduction; the tests below pin repository configuration, and no spec requirement changes (change doc: `Affected requirements: none`), so no test carries a `[Requirement]` tag.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RulesetTests.ReleaseRulesetReportsRuleAsError("RH2009")` (new data row; summary names RH2009) | — (repository config; decision 7) | `LoadReihitsuEntries` | **fails**: `Release.ruleset` has `None` for RH2009, the test expects `Error` |
| `RulesetTests.DebugRulesetHasNoEntryForComplexityRule` (new) | — (decision 3, 7) | `LoadReihitsuEntries` | passes (Debug has no RH2009 entry today and must keep none) |
| `RulesetTests.ReleaseRulesetSwitchesOffComplexityRule` | — | — | **deleted** (asserted `None`; decision 7) |
| `AnalyzerViolationReportTests.ReportLeavesOutComplexityViolations` | — | — | **deleted** (asserted the scan drops RH2009; decision 4) |
| `AnalyzerSelfHostingTests.AnalyzerReportsNoViolationsOnOwnSourceCode("RH2009MethodsShouldNotExceedTheComplexityLimitAnalyzer")` (existing row; now effective) | — | self-hosting scan | **fails** with exactly the 19 production members of `01-spec-author.md` (list below); none of the 8 test members is reported any more |

To make the scan judge RH2009, I removed `_ignoredDiagnosticIds` (the field, its `Fields` region, and its filter in `Collect`) from `src/Reihitsu.SelfHosting.Test/Utilities/AnalyzerViolationReport.cs` (decision 4). The scan now ignores no rule. That class is test infrastructure, and the removal is what the self-hosting assertion checks, so it falls to me and not to the implementer. The implementer's remaining items are `Release.ruleset` (`RH2009` → `Error`) and the 19 production members.

The RH2009 row reports these 19 members at `3ebf2d4`, each at the score and line from the spec author's table: TargetAttributePlacementCodeFixProviderBase.cs:108 (16) and :208 (18); RH8201…CodeFixProvider.cs:112 (17); RH8402…CodeFixProvider.cs:119 (22); NonOverrideMembers…AnalyzerBase.cs:106 (17); RH3002TypeArgumentListUtilities.cs:148 (20), :292 (18), :457 (16); RH8203…Analyzer.cs:62 (17); RH8303…Analyzer.cs:171 (17); RH5204…Analyzer.cs:74 (19); DeclarationModifierUtilities.cs:215 (22); RegionDirectiveBlankLineUtilities.cs:29 (29); BlankLineTokenCleanupRewriter.cs:145 (19), :560 (17); IndentationRewriter.cs:67 (22); RawStringLineShifts.cs:298 (16); AttributeListShapeUtilities.cs:139 (18); LaneClassifier.cs:95 (23).

### The 8 test members (decision 6): restructured, results unchanged

| Member | Restructuring | How I established that the result is unchanged |
|---|---|---|
| `SelfHostingSolution.AddPackageReferencesFromAssets` (27) | Moved the per-package loop over compile assets into `AddCompileAssets`. The inner loop "first package folder whose file exists, skip if the assembly name is present, else add" is now `Select(...).FirstOrDefault(File.Exists)` followed by one combined condition. | Equivalent by construction: the old loop broke at the first existing path in both branches. Every self-hosting row builds its compilations through this method, and the 55 self-hosting rows I ran compiled every project with no compile error. |
| `SpecCatalog.ReadRequirements` (17) | Moved the per-spec body into the private `AddRequirements` without changing it. | Pure extraction. The 198 architecture tests pass, and the spec checks among them read every real spec. |
| `SpecCatalog.ReadMentionedRequirementIds` (23) | Moved the per-spec body into the private `ReadMentionedRequirementIdsOfSpec`. Replaced `if (isInHeaderTable) { hasPassedHeaderTable = true; }` with `hasPassedHeaderTable = isInHeaderTable;`. | This sits inside `if (hasPassedHeaderTable == false)`, so the flag is false when assigned, and the two forms are equal. The architecture tests pass. |
| `ObservedPipeline.FindTabsInCodeAndDirectiveWhitespace` (23) | Moved the directive branch into `AddDirectiveWhitespaceTabs(directive, findings)`. It appends to the same list in the same order. | Pure extraction. The `PhaseColumnOwnershipTests` that use it pass. |
| `PhaseLineBreakOwnershipTests.GetTokenLineShape` (19) | Moved the blank-line count loop into `CountBlankLinesBetween`. | Pure extraction. `PhaseLineBreakOwnershipTests` pass. |
| `PhaseOwnershipDocumentationTests.CollectFormatterDeclarations` (17) | Moved the per-file declaration collection into `AddDeclarations(root, declarations)`. Added `using Microsoft.CodeAnalysis;`. | Pure extraction. The architecture tests pass. |
| `CliDeepNestingEndToEndTests.CreateSource` (17) | Replaced the switch statement with eight loops by a switch expression over a new `Repeat(firstIndex, endIndex, createPart)` helper. The case strings are interpolated, because RH1003 rejects concatenation. | **Byte comparison**: a throwaway program ran the old and the new `CreateSource` side by side. It covered all 8 shapes, the depths 0, 1, 2, 3, 7, 50, 2000, 6400, and 16000, and LF and CRLF: 144 comparisons, 0 differences. An unknown shape still throws `ArgumentOutOfRangeException`. `CliDeepNestingEndToEndTests` pass (14). |
| `RoslynVersionOwnership.Check` (16) | Moved the XML element loop into `ReadBuildFile`. The "root props → `null`" choice of the recorded file is now computed once per build file in `Check` and passed in. | `Path.GetFullPath` is pure, so the only change is when it is evaluated. The architecture tests pass, including the Roslyn-version ownership tests. |

Every new private method sits in the private part of its `Methods` region and has full documentation comments. `scripts/format.sh` changed none of the 10 files. The Release build has 0 warnings.

## Scenarios (bug fix)

_N/A_ (roll-out, not a bug).

## Not covered, and why

- **No new tests for the 19 production refactors.** The skeptic's *Breaking inputs* table names the existing tests that pin each member (`README.md`, *Coverage first*). The RH2009 self-hosting row is the check that they get restructured.
- **Root `reihitsu.json` sets no complexity limit.** This is already pinned by `BuildConfigurationTests.RootConfigurationSetsOnlyTheProductionTypeLimitOfFiveHundred`.
- **RH2009 has a scan row.** This is already pinned by `AnalyzerViolationReportTests.ComplexityAnalyzerHasARow`, unchanged.
- **The ignore list stays empty.** No test was added. Deleting `ReportLeavesOutComplexityViolations` together with the field leaves nothing to assert, and the existing `ReportNamesFileLineRuleAndAnalyzerOfAViolation` and the ordering tests cover `Collect`'s filter on source diagnostics.
- **`scripts/README.md` line-count paragraph.** It is unchanged and still correct. `DebugRulesetHasNoEntryForLineCountRule` was not renamed, and the new RH2009 row and test carry no `RH2008`/`RH2111` in their names, so the `Name~RH2008|Name~RH2111` filter still selects the same 18 rows.
- **Failure classes.** FC-017/FC-035: no assertion was weakened, and the expected values come from the change doc's decisions, not from patched code. FC-036: none of the names in the phase table was renamed, and the architecture tests pass. FC-031: every extracted method has a summary describing its own shape, and the summaries of the original methods still hold. FC-032: after the restructuring I re-ran the RH2008, RH2111, and RH2009 rows and the full analyzer scan (see *Commands*). The formatter, code-fix, and naming classes do not apply: no output-producing code changed in my stage.

## Commands

```bash
scripts/build.sh
scripts/test.sh --no-build --project architecture
scripts/test.sh --no-build --project formatter --filter "FullyQualifiedName~PhaseColumnOwnershipTests|FullyQualifiedName~PhaseLineBreakOwnershipTests"
scripts/test.sh --no-build --project cli --filter "FullyQualifiedName~CliDeepNestingEndToEndTests"
scripts/test.sh --no-build --project selfhosting --filter "FullyQualifiedName~RulesetTests|FullyQualifiedName~AnalyzerViolationReportTests|Name~RH2008|Name~RH2111|Name~RH2009"
scripts/test.sh --no-build --project selfhosting --filter "FullyQualifiedName~AnalyzerReportsNoViolationsOnOwnSourceCode"
```

Results at `3ebf2d4`:
- architecture: 198/198 passed.
- formatter: 32/32 passed.
- cli: 14/14 passed.
- Self-hosting focused run: 53 passed, 2 failed as expected (`ReleaseRulesetReportsRuleAsError("RH2009")`, and the RH2009 analyzer row with the 19 production members). The RH2008 and RH2111 rows passed.
- Full analyzer scan (19 min): 352 of 353 analyzer rows passed. Only the RH2009 row failed, with the same 19 production members, so no other analyzer reports anything in the 10 changed test files.

## Follow-up candidates

_None._
