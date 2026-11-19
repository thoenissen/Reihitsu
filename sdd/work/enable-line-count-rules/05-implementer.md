Role: implementer
Commit: 261a706b
Verdict: #1865: blocked (work in progress, stopped by the lead; one build blocker below); #1866: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## thoenissen/Reihitsu#1865

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Base/DiagnosticAnalyzerBase.cs` | Constructors `(id, category)` and `(id, category, isEnabledByDefault)`; resources `<ID>Title`/`<ID>MessageFormat` derived from the ID; public `Title`, `MessageFormat` (current UI culture) | 3C, FC-036 |
| `src/Reihitsu.Analyzer/AnalyzerResources.cs` | Only `ResourceManager` left (698 properties and `GetString` removed) | 3C |
| 22 bases under `src/Reihitsu.Analyzer/Base/`, 349 analyzers under `src/Reihitsu.Analyzer/Rules/` | Mechanical: the `titleResourceName`/`messageFormatResourceName` parameters, their `<param>` docs, and the `nameof(AnalyzerResources.…)` arguments removed (script checked every analyzer named exactly `<its DiagnosticId>Title`/`MessageFormat`) | 3C |
| `RH2008…Analyzer.cs`, `RH2111…Analyzer.cs` | Defaults 120 (test methods); 1500 / 3000 (types) | RH2008-001, RH2111-001 |
| `reihitsu.json` (new, root) | `{ "design": { "maximumTypeLines": 500 } }` | 2A |
| `Directory.Build.props` | Imports `src/Reihitsu.Analyzer.Package/buildTransitive/Reihitsu.Analyzer.targets` (decision 2); `AdditionalFiles` of the root `reihitsu.json`, written as `$([System.IO.Path]::GetDirectoryName('$(MSBuildThisFileFullPath)'))\reihitsu.json` because `BuildConfigurationTests` requires the literal Include to end in `/reihitsu.json` (`$(MSBuildThisFileDirectory)reihitsu.json` does not). Put in `.props`, not a new root `Directory.Build.targets`, because the `scripts/*` apps stop only the `.props` search and would inherit a root `.targets` | 2A, decision 2 |
| `Debug.ruleset`, `Release.ruleset` | Debug: RH2008/RH2111 lines deleted; Release: both `Error` (decision 5) | — |
| `src/Reihitsu.SelfHosting.Test/Utilities/SelfHostingProject.cs` | `IsTestProject`: an `IsTestProject` property in the project file, else a `PackageReference` to `Microsoft.NET.Test.Sdk` | decision 3 |
| `…/Utilities/SelfHostingSolution.cs` | `GetAnalyzerOptions(assemblyName)`: `build_property.IsTestProject` (`true` or empty), `build_property.IsTestingPlatformApplication` (empty), root `reihitsu.json` as additional file | decision 3 |
| `…/Utilities/AnalyzerViolationReport.cs` | 5-argument `Collect` with an options function; the 4-argument one delegates with no options | — |
| `…/Utilities/SelfHostingAdditionalText.cs`, `SelfHostingAnalyzerConfigOptions.cs`, `SelfHostingAnalyzerConfigOptionsProvider.cs` (new, test harness) | Minimal Roslyn option types for the above | — |

Production refactors (behavior-preserving; no expected value changed):

| Violation (as measured before) | Change |
|---|---|
| `Cli/Program.cs:39` Main (72), `:165` ParseArguments (132) | `HandleArgumentsWithoutFormattingAsync`, `RunFormatCommandAsync`; parsing via `ParseResult with {…}`: `ApplyFlag`, `ApplyOptionValue`, `WithArgumentError` |
| `Cli/FormatCommandHandler.cs:272` (88), `:414` (63) | `FinishRun` (flush on cancel + aggregation), `GetSkipResult` |
| `Core/AttributeTargetUtilities.cs:100` (62), `:473` (85) | Switch expressions in `TryResolveTarget`, `TryResolveImplicitTarget` |
| `Core/FluentChain.cs:356` (61) | `CollectMemberAccess` |
| `Core/ModifierOrderingUtilities.cs:91` (67) | `TryGetGapBehindModifier` |
| `Core/SyntaxTriviaUtilities.cs:504` (121) and type (505) | New internal type `DirectivePairSplitScan`; `SplitsDirectivePair` keeps its public doc and delegates |
| `Core/OrderingDeclarationUtilities.cs` type (544) | New internal static `ConditionalGroupUnitUtilities` (11 helpers moved verbatim, made internal) |
| `CodeFixes/Base/CasingCodeFixProviderBase{T}.cs` type (812), `:721` (64) | New internal static `CasingRenameSafetyUtilities` (18 T-independent rename checks moved, plus new `ChangesProjectsUnsafelyAsync` from the tail of `RenameWithoutConflictAsync`) |
| `CodeFixes/Base/StatementBracesCodeFixProviderBase.cs:86` (64) | `TryGetOwnerTokens` |
| `Analyzer/Rules/Clarity/RH3002…Analyzer.cs` type (693), `:795` (67) | New internal static `Analyzer/Core/UnnecessaryParenthesesTypeArgumentListUtilities` (type scanner and type-argument-list guards moved); `GetArguments`, `ClosesPrecedingTypeArgumentList` split from `WouldJoinTypeArgumentListAcrossArguments`; `ShouldReport`, `GetTokensWithoutParentheses`, `GetUnparenthesizedExpression` made internal on the analyzer (mutual recursion between the two classes) |
| `Formatter/ReihitsuFormatter.cs:705` (98) | `WriteBackSplitFields`, `ReplaceTargetByFields` |
| `Formatter/Pipeline/Precheck/PrecheckPhase.cs:78` (75) | Candidate list built as head + common tail (same order); `CreateFirstUnsupportedSyntax` |
| `Formatter/…/SwitchCaseBraceRewriter.cs:335` (84), `:663` (64) | `GetDirectlyDeclaredLocalNamesOfStatement` (switch expression); `GetBlockStatements` |
| `Formatter/…/DeclarationBraceLineBreakRewriter.cs:151` (69) | `NormalizeDeclaration` (switch expression), `IsBracedTypeDeclaration` |
| `Formatter/…/UsingDirectiveOrderingRewriter.cs:63` (87) | `GetSourceLayout`, `DetachHeaderOfOriginalFirst` (ref parameters) |

All changed files were run through `scripts/format.sh --force`; the Debug build is warning-free apart from RH0001 (below) and `MSTEST0049` at `LineCountSelfHostingTests.cs:173` (test code, cleaner).

### What is left

1. **Not re-measured after the refactors.** The last scan of the RH2008/RH2111 rows ran before the refactors and listed the 22 production violations in the table above plus 20 test violations (9 test methods > 120, 11 test types > 3000, the lists in `01-spec-author.md`). Each refactor was sized by an estimate of lines of code, not by the analyzer; a full self-hosting run was in progress when I was stopped. Re-run the two rows and fix any production declaration still listed:

   ```shell
   scripts/test.sh --configuration Debug --project selfhosting --filter "FullyQualifiedName~AnalyzerSelfHostingTests&DisplayName~RH2008"
   scripts/test.sh --configuration Debug --project selfhosting --filter "FullyQualifiedName~AnalyzerSelfHostingTests&DisplayName~RH2111"
   ```

   (`DisplayName~` on the DynamicData rows is untried; without it the class runs all 349 rows, about 18 minutes.) Test-code violations are for the later test-implementer stage (decision 12).
2. **The other 347 analyzer rows and the formatter self-hosting rows** have not run over the refactored code; before the refactors (with the base/resource change) all 347 other rows passed. Run `scripts/test.sh --configuration Debug --project selfhosting` (full; `WorkingDocumentsTests` fails while `sdd/work/` exists).
3. **Build blocker — Release build fails (needs the lead).** The committed `binaries/Reihitsu.Analyzer.dll` (1.4.0, 8d5fcaca) predates the `design` section (added in e91afdf7), so its RH0001 reports the root `reihitsu.json` in every project: `reihitsu.json(2,4): warning RH0001: … Unknown configuration section 'design'.` (observed in the Debug build of every project). `Release.ruleset` sets RH0001 to `Error` and Release has `TreatWarningsAsErrors`, so the Release build and CI fail until `binaries/` is rebuilt. I did not verify it with a Release build. Resolving it is outside my scope: rebuilding `binaries/` (`/bump-version`), not passing the file to the build until then (breaks `BuildConfigurationTests.RepositoryBuildPassesRootConfigurationToTheAnalyzers`), or a ruleset/suppression change (escalation 6).
4. Release build and FC-031 re-read of the summaries of every moved/extracted member (done while writing; not re-checked after `format.sh`).

### Policy owners

- `rg -n "LocalizableResourceString\(" src -g '*.cs'` — one owner: `DiagnosticAnalyzerBase` (the resource-name derivation).
- `rg -n '"reihitsu.json"' src -g '*.cs'` (production: `ConfigurationManager`); the harness has its own constant in `SelfHostingSolution` beside the test constants — test code.
- `rg -n IsTestProject src` — the analyzer reads it in `LineCountAnalyzerBase`; the harness models MSBuild's value in `SelfHostingProject.IsMarkedAsTestProject` (FC-044 model, accepted by decision 3).

### Production diff size

Working tree against bcce9adf (before this commit), production paths: 391 files, +2986 / −6412; without `AnalyzerResources.cs` and `Rules/**`: 41 files, +2648 / −1955 (moved code counts on both sides). New production types: `DirectivePairSplitScan`, `ConditionalGroupUnitUtilities` (Core), `CasingRenameSafetyUtilities` (CodeFixes), `UnnecessaryParenthesesTypeArgumentListUtilities` (Analyzer). All four are the issue's own fix (splitting types over 500), not structural changes in the escalation-9 sense; the lead decides.

### Deleted special cases

_N/A._

### Tests run (Debug, at the refactored tree, before the commit)

- `--project analyzer`: Passed 11245, Failed 0.
- `--project formatter`: Passed 6115; `--project core`: Passed 738; `--project cli`: Passed 403; `--project architecture`: Passed 198.
- Earlier (after the API, defaults, harness, and build changes, before the refactors): `--project analyzer --filter "…RH2008…Tests|…RH2111…Tests|…AnalyzerResourcesTests|…DiagnosticAnalyzerBaseTests"` Passed 180; `--project selfhosting --filter "…LineCountSelfHostingTests|…BuildConfigurationTests|…RulesetTests|…AnalyzerViolationReportTests"` Passed 27; full `AnalyzerSelfHostingTests` Failed 2 (RH2008, RH2111 rows), Passed 347.

### Disputed tests or scope

- `BuildConfigurationTests.RepositoryBuildPassesRootConfigurationToTheAnalyzers` demands an `AdditionalFiles` Include that literally ends in `/reihitsu.json`; met with a property function (above). Not disputed, but it shapes the build file.
- Item 3 of *What is left*.
- Four new production types (above).

### Architecture findings

_None._ (The RH3002 analyzer and its new helper call each other because `ShouldReport` is recursive through nested parentheses; one decision, one owner, no copy.)

### Follow-up candidates

- `src/Reihitsu.Analyzer.Test/Resources/CodeFixResourcesTests.cs:19` names `AnalyzerResources` in a summary about `CodeFixResources` (test code; one line).

## thoenissen/Reihitsu#1866

### Changes

| File | What changed | Requirement |
|---|---|---|
| `Debug.ruleset` | `RH08402` → `RH8402` | decision 6 |

### Tests run

`--project selfhosting --filter "FullyQualifiedName~RulesetTests"` (within the 27 above): passed.

### Disputed tests or scope

_None._

## Whole run

Commit 261a706b holds all production, build, configuration, and harness changes. Lane check not run.
