Role: auditor
Commit: e3d15260f78280b9985ed1f046665ef33de00d2a
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit. Head e3d15260 (checked out, clean tree); base `origin/main` f597ab5343a2d292af69fb8fe448faed0f187404, confirmed by `git fetch` and merged (`git merge-base --is-ancestor`). The diff `f597ab53..HEAD` (960 files) is what handovers 01–12 describe: rulesets, `Directory.Build.props`, root `reihitsu.json`, defaults and specs and rule pages of RH2008/RH2111, the per-rule resource properties dropped (349 analyzers, 22 bases), 18 production methods and 6 production types restructured (five new internal types), the self-hosting harness and its new tests, the 11 test classes split by topic into 29, the 9 test methods shortened, `LaneClassifier` and `scripts/README.md`, `AGENTS.md` and `.claude/commands/add-resource-texts.md`. No source, build or ruleset file changed after b3175d4e, the tree the 09 test runs and the cleaner's Release build (0 warnings) were taken on.

How the refactors were checked: a script over `git diff -U0` matched every added line of the production refactors against the removed lines (whitespace-insensitive) and listed the genuinely new lines per file; each was traced against the code it replaced. The formatter (`PrecheckPhase`, `DeclarationBraceLineBreakRewriter`, `SwitchCaseBraceRewriter`, `UsingDirectiveOrderingRewriter`, `ReihitsuFormatter`/`TargetWriteBackUtilities`), Core (`AttributeTargetUtilities`, `FluentChain`, `ModifierOrderingUtilities`, `SyntaxTriviaUtilities`/`DirectivePairSplitScan`, `OrderingDeclarationUtilities`/`ConditionalGroupUnitUtilities`), CLI (`Program`, `FormatCommandHandler`) and code-fix (`StatementBracesCodeFixProviderBase`, `CasingCodeFixProviderBase{T}`/`CasingRenameSafetyUtilities`) changes are behavior-preserving: same branch order, same defaults (`target = default` ⇔ `GetValueOrDefault()`), same short-circuit (`Any(scan.Splits)` stops at the first `true` as the old `return true` did), same precheck candidate order, same `lineIndentation` condition, `originalFirstToken`/`originalLastToken` re-derived as `members[targetIndex].GetFirstToken()/GetLastToken()` (identical to `targetField`'s), `files.Count` ⇔ `results.Length`, the CLI's `ParseResult` positional order and first-error semantics unchanged. One MSBuild evaluation was run (`dotnet msbuild src/Reihitsu.Playground/Reihitsu.Playground.csproj -getItem:…`): `IsTestProject` and `IsTestingPlatformApplication` are `CompilerVisibleProperty` items through the new import.

## thoenissen/Reihitsu#1865

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | low | `src/Reihitsu.SelfHosting.Test/Utilities/SelfHostingSolution.cs:26,41,86,130`; `…/Utilities/SelfHostingAdditionalText.cs:11`; `…/Repository/BuildConfigurationTests.cs:16`; `…/Analyzers/AnalyzerSelfHostingTests.cs:14`; `…/Analyzers/LineCountSelfHostingTests.cs:19-21` | FC-031 | – | PR-introduced | Written in 261a706b, when the build still passed the root `reihitsu.json` as `AdditionalFiles`; decision 13 (be29e3c8) removed that item, and these comments still say the build passes it: `ConfigurationFileName` "which the build passes to the analyzers of every project"; `GetAnalyzerOptions` "the analyzer options the repository's build gives a scanned project: … and the repository's root `reihitsu.json` as additional file"; `CreateAnalyzerOptions` and `_analyzerOptions` "the analyzer options the (repository's) build gives a/each project" (they include the file); `SelfHostingAdditionalText` "A file of the repository that the build passes to the analyzers as an `AdditionalFiles` item" (its only use is `reihitsu.json`); `BuildConfigurationTests` class summary "its root `reihitsu.json`, which it passes to every project", contradicting its own test `RepositoryBuildDoesNotPassRootConfigurationToTheBinaryAnalyzers` (line 99); `AnalyzerSelfHostingTests` "Each project is analyzed with the analyzer options the repository's build gives it"; `LineCountSelfHostingTests` "as the repository's build does: …, with the limits of the repository's root `reihitsu.json`". The comments that speak only of the `build_property.*` entries (`SelfHostingAnalyzerConfigOptions`, `SelfHostingAnalyzerConfigOptionsProvider`) hold. Comment-only; test code; no compiled behavior | Reword these summaries: the scan gives each project the `build_property.*` entries the build gives it, plus the root `reihitsu.json`, which the build does not pass until `binaries/` is rebuilt |
| 2 | low | `src/Reihitsu.Analyzer/Core/UnnecessaryParenthesesTypeArgumentListUtilities.cs:7,101,128,426,556`; `src/Reihitsu.Analyzer/Rules/Clarity/RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.cs` (`ShouldReport`, `GetTokensWithoutParentheses`, `GetUnparenthesizedExpression` made `internal`) | – (code quality: coupling) | – | PR-introduced | The type extracted from RH3002 to meet the type limit is placed among the shared analyzer utilities (`Reihitsu.Analyzer.Core`) but serves only RH3002 and calls back into the rule's analyzer (`using Reihitsu.Analyzer.Rules.Clarity`; mutual recursion through `ShouldReport`). It is the only type in `Analyzer/Core` that depends on a rule (`rg -l "using Reihitsu.Analyzer.Rules" src/Reihitsu.Analyzer/Core src/Reihitsu.Analyzer/Base` finds only this file), so the split makes one rule's decision look like a shared policy and inverts the Core → Rules direction. No behavior is affected; the analyzer tests pass | Keep the helper beside its rule: move it to `Rules/Clarity` (namespace `Reihitsu.Analyzer.Rules.Clarity`) under an RH3002-specific name; no logic change |

### Requirements

- RH2008-001 (test default 120), RH2111-001 (1500/3000), SHARED-LINES-002 example: hold in `RH2008…Analyzer.DefaultTestLimit = 120`, `RH2111…Analyzer.DefaultProductionLimit = 1500`, `DefaultTestLimit = 3000`; the production method default stays 60. Specs, rule pages, and the change doc's three visible changes agree; `rg` finds no stale 100/500/1000 default in `sdd/specs` or `documentation/rules` (RH0001's own examples are configured values, not defaults).
- Maintainer answer 2A: root `reihitsu.json` is `{ "design": { "maximumTypeLines": 500 } }` only; guarded by `RootConfigurationSetsOnlyTheProductionTypeLimitOfFiveHundred` and `RootConfigurationSetsNoLimitAtOrAboveTheShippedDefault`.
- Maintainer answer 3C: `AnalyzerResources` holds only `ResourceManager`; one owner of the name derivation (`DiagnosticAnalyzerBase`, the only `LocalizableResourceString(`/`new DiagnosticDescriptor(` in production); `Title`/`MessageFormat` published; `AnalyzerResourcesTests` checks existence, ID-derived identity, no orphan resource, no per-rule property. No `nameof(AnalyzerResources.…)`, `AnalyzerResources.RH…`, `titleResourceName` or `messageFormatResourceName` remains anywhere outside `sdd/`. RH7305A (suffix letter) resolves through the same derivation and is covered by the identity test.
- Rulesets: `Release.ruleset` RH2008/RH2111 `Error`, `Debug.ruleset` entries removed (decision 5); `RulesetTests` guard both.
- Build import (decision 2): `Directory.Build.props` imports `src/Reihitsu.Analyzer.Package/buildTransitive/Reihitsu.Analyzer.targets`; MSBuild evaluation of Playground lists both properties as `CompilerVisibleProperty`; `BuildMarksTestProjectAsTestProjectForTheAnalyzers` / `BuildDoesNotMarkProductionProjectAsTestProjectForTheAnalyzers` read the generated editorconfig. The `scripts/*` apps stop the props search and are not affected.
- Decision 13: no root build file passes `reihitsu.json`; guarded by `RepositoryBuildDoesNotPassRootConfigurationToTheBinaryAnalyzers`; follow-up is with the lead.
- The issue's check "one over-limit test method and one over-limit production method" (decision 3/4): `LineCountSelfHostingTests` covers both rules on both sides of each limit (60/61, 120/121, 500/501, 3000/3001) through the scan's own option path.
- Every reported declaration fixed: the full `AnalyzerSelfHostingTests` run of 09 (361 passed, both line-count rows included) at b3175d4e, which equals the head's source.

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Harness: is a project test code (`SelfHostingProject.IsMarkedAsTestProject`) | the project file's `IsTestProject` property, else its `PackageReference Include="Microsoft.NET.Test.Sdk"` | MSBuild `IsTestProject` (set by `Microsoft.NET.Test.Sdk.props`) and `IsTestingPlatformApplication`, read by `LineCountAnalyzerBase.IsTestCode` | Agree for every project of the repository: `ProjectModelMarksExactlyTheTestProjectsAsTestProjects` pins the 8 test projects; the editorconfig tests compare the model's inputs with MSBuild's output for one test and one production project. `IsTestingPlatformApplication` is modelled as always empty (no project sets `EnableMSTestRunner`) | Holds; FC-044 model accepted by decision 3 |
| `DeclarationBraceLineBreakRewriter.IsBracedTypeDeclaration` + switch arms | class/struct/interface/record/union with a present `{`; enum separately | the removed `switch` cases | Same kinds, same missing-brace pass-through, enum reaches its own arm (not in the kind list) | Holds |
| `AttributeTargetUtilities.TryResolveTarget` | `IsNullOrWhiteSpace(target) \|\| target == "type"` → implicit | removed `switch` | Same partition; unknown target → `default`, `false` | Holds |
| `PrecheckPhase` candidate list | head of three by `isDeclaratorComma`, common tail of five | removed two literal lists | Identical order in both branches | Holds |
| `UsingDirectiveOrderingRewriter` line indentation | `beginsLine ? (startsItsLine ? null : scope) : null` | removed `beginsLine && !startsItsLine ? scope : null` | Equal | Holds |
| `LaneClassifier` root `reihitsu.json` | exact root path, case-insensitive | `scripts/README.md` classification table | Agree; `samples/reihitsu.json` stays Unknown, a test-project copy stays Test (both tested) | Holds |
| `RepositoryBuildDoesNotPassRootConfigurationToTheBinaryAnalyzers` | `AdditionalFiles` items of root `Directory.Build.props`/`.targets` ending in `reihitsu.json` | where the build could add the file | Project files are not inspected; none adds it today | Holds for the decision's scope |
| `RootConfigurationSetsNoLimitAtOrAboveTheShippedDefault` | every `design` setting of the root file, against a copy of the four shipped defaults | the analyzers' private default constants | Agree today; see Hints | Holds |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Resource names of a rule's title and message | `rg -n "LocalizableResourceString\(\|new DiagnosticDescriptor\(" src` → only `DiagnosticAnalyzerBase.cs:50-53` in production; tests read the same names in `AnalyzerResourcesTests` and the pre-existing `AnalyzerPackageMetadataTests` | One owner |
| Which MSBuild properties the analyzers see | `src/Reihitsu.Analyzer.Package/buildTransitive/Reihitsu.Analyzer.targets` (imported by the root props); the scan harness models them in `SelfHostingSolution.CreateAnalyzerOptions` | One statement for the build; the harness model is test code, accepted (decision 3) |
| Test-code recognition | `LineCountAnalyzerBase.IsTestCode` (production, `build_property.IsTestProject`); `SelfHostingProject.IsMarkedAsTestProject` (harness model) | One production owner |
| Moved helpers (`EndTargetLineAsFormatted`, `StartsLineInLayout`, `TrimLeadingWhitespace`, `TryGetSplitFields`, the casing rename checks, the conditional-group unit helpers, the directive-pair scan, the type-argument-list guards) | `rg` per name: one definition each, now in `TargetWriteBackUtilities`, `CasingRenameSafetyUtilities`, `ConditionalGroupUnitUtilities`, `DirectivePairSplitScan`, `UnnecessaryParenthesesTypeArgumentListUtilities`; no leftover private copy | One owner each |
| Root configuration file name | `ConfigurationManager.ConfigurationFileName` (production); `SelfHostingSolution`, `BuildConfigurationTests`, `LineCountSelfHostingTests` (test literals) | Production has one owner |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `AnalyzerResourcesTests.EveryAnalyzerPublishesTheResourcesItsDiagnosticIdNames` | each analyzer's descriptor texts are `<ID>Title`/`<ID>MessageFormat` | an analyzer whose resource is missing (`GetString` → null ≠ "") or misnamed | reflection discovery | Adequate |
| `AnalyzerResourcesTests.EveryRuleResourceBelongsToAShippedAnalyzer` | no orphan rule resource | a leftover `RH…Title` for no analyzer | invariant resource set | Adequate |
| `AnalyzerResourcesTests.AnalyzerResourcesHoldsNoPropertyPerRule` | 3C | a re-added `RH…` property | reflection | Adequate |
| Analyzer tests using `MessageFormat` (337 replacements) | the reported message equals the rule's message text | a wrong message; the expected text is the analyzer's own descriptor, but the identity test above pins that to the ID's resource, so the pair is as strong as the old `AnalyzerResources.RH…MessageFormat` | `AnalyzerTestsBase.MessageFormat` | Adequate; the uses outside a rule's own analyzer-test file (`GeneratedCodeTests`, `CodeFixWarningWaveCheckBaseTests`, `RemoveWhitespaceRunCodeFixProviderBaseTests`, and the `…FormatterTests` files of RH5102, RH5111, RH5501, RH5502, RH5530, RH5531, RH6023) compare against the `TAnalyzer` of their class, and they pass, so the texts match |
| RH2008/RH2111 default tests | 120/121, 1500/1501, 3000/3001 with exact messages | a wrong default | `VerifyLines` with exact `LineLimit` | Adequate; output covered by 1A |
| `LineCountSelfHostingTests.Scan…` (4) | the scan judges each side of each limit with the right limit | a test project judged as production, or the repository limit not applied | `ScanSample` through `AnalyzerViolationReport.Collect` with the scan's options; exact message, `HasCount(1)` | Adequate (both boundary sides in one sample) |
| `LineCountSelfHostingTests.RepositoryConfigurationIsPassedAndValid` | the scan passes exactly the root file, RH0001 is silent | a missing or extra configuration file, or a `design` error | exact path sequence; empty diagnostics | Adequate |
| `BuildConfigurationTests` (6) | repository config, import, absence of the build item, generated properties | see Guard scope | XML/JSON/editorconfig reads | Adequate |
| `RulesetTests` (4 methods, 8 rows) | every Reihitsu entry names a shipped rule; RH2008/RH2111 actions; RH8402 `None` in both | `RH08402` returning; a `None` re-added | `XDocument` | Adequate; the shipped-ID check would have caught #1866 |
| The 1586 moved test methods (09) and the 9 shortened ones (07) | unchanged tests | a changed member text or literal | member-text SHA-256 and literal-value multisets, TRX outcome multisets | Adequate; corroborated: `[TestMethod]` count across analyzer and formatter tests 14830 → 14833 (+3 = `AnalyzerResourcesTests`, 1 → 4 methods) |
| 17 copied `GetFixAllScenario` overrides | Fix All on two diagnostics | — | `BatchCodeFixTestsBase` | Redundant, not weaker (decision 21) |

### Visible output is decided

Changed expected values of visible surfaces: the RH2008/RH2111 default tests and both rule pages — covered by answer 1A (change doc *Decisions*, `00-lead.md` *Maintainer answers*). Every other analyzer test changed only where its expected message comes from (same resource text); the split and shortened tests change no expected value (09/07 proofs). Nothing undecided.

### Cost of special cases

_N/A — the production changes are behavior-preserving restructurings; no guard, special case, or unsupported shape is added or deleted._

### Failure classes dismissed

- FC-001, FC-002, FC-003, FC-016: no guard was narrowed or added in production; the restructured predicates partition their input as before (Guard scope). FC-003 for the new `LaneClassifier` entry is covered on both sides.
- FC-004, FC-005, FC-019, FC-020, FC-021, FC-022, FC-023, FC-024, FC-025, FC-026, FC-027, FC-029: no rewrite, gap handling, or trivia decision changed; the formatter and code-fix refactors move code verbatim or replace a `switch` by an equivalent expression (traced above), and the formatter (6115), analyzer (11245), core (738) and CLI (403) suites passed on the refactored production code (05/06), unchanged since.
- FC-006, FC-007, FC-008, FC-009, FC-018, FC-030, FC-043: no code fix changed what it writes, when it is offered, or which node it formats; `CasingRenameSafetyUtilities.ChangesProjectsUnsafelyAsync` holds the removed tail of `RenameWithoutConflictAsync` in the same order (hiding-carrier check, then compile errors, then positional properties).
- FC-010, FC-011, FC-012, FC-013, FC-014, FC-028: no phase changed behavior; `FormatSplitFieldInContext` writes back the same trivia (FMT-PIPE-010/017/018/029) through `TargetWriteBackUtilities`.
- FC-038–FC-042: no unsupported shape declared or touched; the precheck change keeps candidate order.
- FC-036: checked — the resource manifest name (`Reihitsu.Analyzer.AnalyzerResources`) and the `ResourceManager` are unchanged; every removed property's consumers (`nameof`, reflection in `AnalyzerResourcesTests`) are gone or rewritten; `AnalyzerPackageMetadataTests` already read by `GetString($"{id}Title")`. New public `Title`/`MessageFormat` on `DiagnosticAnalyzerBase` collide with no member (warning-free Release build).
- FC-015, FC-044: see Policy ownership and Guard scope; the harness model is decision 3.
- FC-017: see Assertion adequacy.
- FC-031: finding #1; the production summaries of every changed or extracted member (`Main`, `ParseArguments`, `HandleArgumentsWithoutFormattingAsync`, `FinishRun`, `GetSkipResult`, `TryResolveTarget`, `CollectMemberAccess`, `TryGetGapBehindModifier`, `SplitsDirectivePair`, `DirectivePairSplitScan`, `NormalizeDeclaration`, `GetDirectlyDeclaredLocalNamesOfStatement`, `GetBlockStatements`, `CreateLeadingTriviaOfLaterDirective`, `GetSourceLayout`, `DetachHeaderOfOriginalFirst`, `TargetWriteBackUtilities.*`, `TryGetOwnerTokens`, `DiagnosticAnalyzerBase` constructors) describe their bodies; no prose (non-cref) reference to a moved member's old location remains.
- FC-032: the counts the restructuring worked from were re-measured after the refactors (06) and the scan rows pass on the final source (09).
- FC-033, FC-034, FC-035: no reproduction; the new expected values come from the spec examples and maintainer answers, not from the code.
- FC-037: no comment-only claim relies on a `grep` proof; the cleaner did not claim a text-only change.

### Architecture findings

_None._ (Finding #2 is a placement of one rule's helper, not two owners of one decision: the type-argument-list decision has one owner.)

## thoenissen/Reihitsu#1866

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._ `Debug.ruleset` now reads `RH8402` (`Release.ruleset` already did); `RulesetTests.EveryReihitsuEntryNamesAShippedRule` (both rulesets) and `RulesetSwitchesOffRH8402` (both) guard it. RH8402 reports nothing without a `copyright` section, which the new root `reihitsu.json` does not have, so no output changes.

### Guard scope

_N/A — no predicate; the ruleset ID check is under #1865._

### Policy ownership

_N/A — one ruleset entry._

### Assertion adequacy

See `RulesetTests` under #1865.

### Failure classes dismissed

All classes: a ruleset ID correction that reaches no compiled behavior and no output.

### Architecture findings

_None._

## Whole run

### Hints

- `BuildConfigurationTests._shippedDefaultLimits` restates the four shipped defaults; a later change of a default must edit it as well as the RH2008/RH2111 tests, or the "never above the default" guard checks against a stale value. Not a finding: tests restating specified values is the repository's pattern.
- `AnalyzerTestsBase<TAnalyzer>.MessageFormat` constructs a new analyzer on every access; negligible in test code.
- The imported package targets also add `@(RazorComponent)` as `AdditionalFiles`; in the repository build that item list is empty at evaluation (Playground: 0 `RazorComponent`, 0 `AdditionalFiles` from `dotnet msbuild -getItem`), so the Razor branch is inert here. I did not check whether it is inert for package consumers too; outside the run.
- `sdd/work/` must be deleted before publishing (`WorkingDocumentsTests` fails while it exists, as 06/07 recorded).

### Follow-up candidates

_None_ beyond the one in `00-lead.md` (pass the root `reihitsu.json` to the build once `binaries/` is rebuilt).
