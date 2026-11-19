Role: skeptic
Commit: 3b3cd890
Verdict: #1865: output question (the spec author's question 1 stands, with the cost and caveat added below; everything else proceeds); #1866: proceed

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## thoenissen/Reihitsu#1865

### Premise

Real, as amended by the maintainer's binding scope (`00-lead.md`). Checked at 3b3cd890:

- `Debug.ruleset:98-99` and `Release.ruleset:113,124` set RH2008 and RH2111 to `None`; they carry out decision 11A of `sdd/changes/2026-11/line-count-rules.md`, which this change reverses.
- `Directory.Build.props` loads `binaries/*.dll` as `Analyzer` items; the only `CompilerVisibleProperty` items for `IsTestProject`/`IsTestingPlatformApplication` are in `src/Reihitsu.Analyzer.Package/buildTransitive/Reihitsu.Analyzer.targets`, which only a package consumer imports. Without a change, every project of this repository is production code under SHARED-LINES-003 — exactly what that requirement's `Why:` says for a project that loads the analyzer without the package's build files.
- Every one of the eight test projects (`Analyzer.Test`, `ArchitectureTests`, `Cli.Test`, `Core.Test`, `Formatter.Test`, `Playground.Test`, `SelfHosting.Test`, `Tooling.Test`) references `Microsoft.NET.Test.Sdk` 18.12.0, whose `build/*/Microsoft.NET.Test.Sdk.props:17` sets `<IsTestProject>true</IsTestProject>` (read in `~/.nuget/packages/microsoft.net.test.sdk/18.12.0`). `MSTest.TestAdapter` 4.5.3 sets `IsTestingPlatformApplication` to `$(EnableMSTestRunner)`, which no project sets, so no project is test code through the second property. No non-test project references the test SDK.
- **The ruleset change is inert in this run.** `binaries/Reihitsu.Analyzer.dll` (last changed by 8d5fcaca, the 1.4.0 bump) contains no `RH2008`, `RH2111`, or `LineCountAnalyzerBase` string; `RH8402` is found by the same search, so the method works. The rules arrived in e91afdf7, after the bump. Consequences: the issue's "the build reports no new diagnostics" is met trivially, and **nothing in this run's build exercises the new `CompilerVisibleProperty` items**; they first take effect when `/bump-version` rebuilds `binaries/`. If they were wrong then, the Release build (`TreatWarningsAsErrors`, ruleset `Error`) would fail in every test project with RH2008 at 60 lines (the issue counted 171 test methods over 60). The only check this run has of the repository's state is the self-hosting scan, which does not run MSBuild (below).
- The issue's "self-hosting scans under `scripts/`" is stale: the scans are `src/Reihitsu.SelfHosting.Test` (`AnalyzerSelfHostingTests._analyzersLeftOut`, `AnalyzerViolationReportTests._analyzersLeftOutOfTheScan` and the test at line 95-106 that asserts both rules have no row). The standalone apps under `scripts/` (`apply-fix`, `proof`, `trace`, `benchmark`, `lane-check`) each have a `Directory.Build.props` that stops the root one, so they load no analyzer and are not involved.
- The scan runs `compilation.WithAnalyzers(...)` without `AnalyzerOptions` (`AnalyzerViolationReport.Collect`), so every scanned compilation reads no `build_property.*` and would be production code there too.
- Measurement: I did not rerun the spec author's temporary harness. Its production figures are corroborated by two independent measurements of earlier trees — the issue (19 methods over 60, maximum 132; 5 types over 500, maximum 812; 28 test types over 2000) and `line-count-rules.md` *Why* (19 methods over 60; 5 of 873 types over 500). The difference of one production method (18 now) is not material; the scan, once enabled, is the list the implementer works from (FC-032: the counts were taken before any refactor).

Caller reachability (`AGENTS.md`, *Supported inputs*) does not apply in its usual sense: the change is to the repository's own build and scan, not to an input a code fix, the CLI, or whole-document formatting receives. No shipped surface changes unless question 1 changes the default.

### Mechanism and candidates

_N/A_ — not a bug. The candidate set of the change is the set of projects, each judged as test or production code by two independent paths:

| Project | SDK | Test SDK | Build (after bump) | Self-hosting scan |
|---|---|---|---|---|
| Analyzer, Analyzer.CodeFixes, Core, Formatter, Cli, Tooling | `Microsoft.NET.Sdk` | no | production | scanned, production |
| Analyzer.Package | `Microsoft.NET.Sdk` | no | production, no source | not scanned (no source files) |
| Playground | `BlazorWebAssembly` | no | production | not scanned (`IsSelfHostable` admits only `Microsoft.NET.Sdk`) |
| Analyzer.Test, ArchitectureTests, Cli.Test, Core.Test, Formatter.Test, SelfHosting.Test, Tooling.Test | `Microsoft.NET.Sdk` | yes | test | scanned, must be test |
| Playground.Test | `Microsoft.NET.Sdk` | yes | test | not scanned (references Playground) |

Playground and Playground.Test are judged only by the build after the bump. Their largest files have 373 (`HomePageTests.cs`), 153, and 86 physical lines, and the largest gap between member starts in them is about 36 lines, so they can hold no type over 500 and no method over 60; neither will report.

### Breaking inputs

- **Build plumbing untested (no FC; the change's own gap).** Axis: test project / production project × build / scan. The scan cells can be tested in this run; the build cells cannot run the rules until the bump. What can be checked in this run is that the build passes the properties: after a build, a test project's `obj/<Configuration>/<tfm>/<Project>.GeneratedMSBuildEditorConfig.editorconfig` must hold `build_property.IsTestProject = true` (and the key for `IsTestingPlatformApplication`), and a production project's must not hold `true`. I did not build to observe this; the test implementer must not assume it.
- **The issue's check** — one over-limit test method and one over-limit production method, each judged by its own limit — cannot be run through the build in this run (rules absent from `binaries/`). It can be run through the scan's path: the same analyzer, the same options the scan derives for a test and for a production compilation. Cover both directions: a 61–100-line method is reported in production and not in test; a 501–1000-line type likewise (or the answered test default).
- **FC-044** (a result another build computes is modelled): the scan harness parses `.csproj` XML and `obj/project.assets.json` instead of running MSBuild, so whatever it uses to decide test code is a model of MSBuild's `IsTestProject`. See *Internal choices* and *Architecture findings*.
- **FC-036** (a rename reaches `nameof`/reflection): applies if the restructuring of `AnalyzerResources` (704 lines of code, over 500) renames anything. Its properties are consumed by `nameof` in 384 places, and `AnalyzerResourcesTests` reflects over `typeof(AnalyzerResources).GetProperties(...)`; `DiagnosticAnalyzerBase` passes `typeof(AnalyzerResources)` and the manifest name `"Reihitsu.Analyzer.AnalyzerResources"`. Splitting the type into partial parts keeps all of these; changing its name or namespace breaks resource lookup at run time, not at compile time.
- **FC-031** (stale summaries): every extracted method and every moved block of the 23 production refactors; the extracted helpers get documentation comments (the repository's documentation rules apply to them).
- Every production refactor touches code that a shipped surface runs (`ReihitsuFormatter.FormatSplitFieldInContext`, `PrecheckPhase`, `SwitchCaseBraceRewriter`, `UsingDirectiveOrderingRewriter`, `DeclarationBraceLineBreakRewriter`, the code-fix bases, `Program.ParseArguments`, `FormatCommandHandler`). Behavior preservation rests on the existing tests; no visible output is in question as long as no expected value changes.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `Debug.ruleset`, `Release.ruleset` | must change (RH2008, RH2111 entries) |
| `Directory.Build.props` (root) | must change (the two properties made compiler-visible) |
| `src/Reihitsu.Analyzer.Package/buildTransitive/Reihitsu.Analyzer.targets` | must stay (package behavior); see *Architecture findings* |
| `src/Reihitsu.SelfHosting.Test` (scan, report tests, harness options) | must change |
| Production code with violations (18 methods, 5 types) | must change, behavior-preserving |
| Test code with violations (20 methods; 94, 28, or 11 types by question 1) | must change |
| `.claude/commands/add-resource-texts.md` | must change if `AnalyzerResources.cs` is split: it names that one file as where accessors are added (lines 18, 71) |
| RH2008, RH2111 analyzers, SHARED-LINES spec | must stay |
| RH2111 spec, rule page, analyzer constant, tests | must stay, unless question 1 is answered B or C |
| RH2008 rule page and spec | must stay |
| Formatter, code fixes, CLI behavior | must stay (refactors only) |
| `binaries/` | must stay (`/bump-version`'s job) |
| `scripts/` standalone apps | not involved |

Conflicts: none with a spec or `shared/` policy. SHARED-DIAG-010 does not apply: RH2008/RH2111 judge size, and SHARED-LINES-001's `Why:` accepts that formatting can raise a count. In this repository the formatter scans already hold the code formatter-clean, so the count is taken on formatted code. P11 is not involved.

### Refuse option

- **Refuse** = leave both rules off (status quo, decision 11A). Cost: none; contradicts the maintainer's binding scope, so not available.
- **Narrower: refactor only production code, keep test code unjudged.** Not available either: the binding scope turns both rules on for the whole repository with the default limits.
- **Excluding single files or folders** (`.editorconfig` severity) — ruled out by the binding scope (no repository-specific limit), and already rejected in the change doc.
- No unsupported shape is involved; declaring one is not an option for this change.
- **Smaller option for the outlier `AnalyzerResources`:** it is not "a type that reads poorly" but an accessor list of two one-line properties per rule (RH5: 260, RH8: 126, RH7: 92, RH4: 76, RH3: 48, RH6: 48, RH2: 38, RH1: 6, RH0: 4). It grows by 2 lines of code per rule, so as one type it exceeds 500 today and every later rule widens the gap. A split into partial parts by category (RH2111-004 judges each part on its own; RH4001 accepts `Type.Part.cs`, RH4001-002) keeps every name and needs no new type; the largest part, Layout, would hold about 260 lines of code. That is the cheapest restructuring; I name it as the size floor, not as the implementation. `CodeFixResources` (231 properties) stays under 500.

### Output questions

The spec author's question 1 (RH2111 default test-type limit) is a legitimate output question and must be asked before the test implementer starts: the test classes to split, and the RH2111 tests and rule page, depend on it. I add two facts the lead should put into it, because they change the trade-off:

1. **Cost of each option in files, not only classes.** RH2111-004 makes splitting a class into partial parts a full answer, so each class over the limit needs at least `ceil(lines / limit) − 1` new part files. From the spec author's thresholds (94 over 1000, 28 over 2000, 11 over 3000, 6 over 4000, 3 over 5000, 1 over 6000): option A needs at least 94+28+11+6+3+1 = **143** new files, C (2000) at least 28+6+1 = **35**, B (3000) at least 11+1 = **12**. These are lower bounds; splitting at member boundaries needs more.
2. **The evidence is one code base, and an atypical one for test code.** This repository's analyzer and formatter tests write their input and their expected output as raw strings in each test, and SHARED-LINES-001 counts every literal line (decision 10A). A typical application's test class holds no source text. Raising the default to this repository's 99th percentile (2899) sets it for every user of the package, most of whose test classes are far smaller. The other three defaults were derived from this same repository, so B is consistent with how the defaults were set; but 14B rejected 2000 only for its test-method half ("marks almost no outlier" fits 5 test methods over 150, not 28 test types over 2000), so C was never rejected on its merits.

My recommendation stays **B**, with the caveat above stated in the question so the maintainer weighs it; if the maintainer gives the atypicality more weight, **C** is the compromise: it cuts the 94 classes to 28 and keeps the default below this repository's 99th percentile.

Format note for the lead: the question as the spec author wrote it already follows *Output questions*; the "Before" block marks the span on the name, which matches RH2111-006.

### Internal choices

1. **Release.ruleset action.** `Action="Error"` like the 331 other enabled Reihitsu entries, versus deleting the two lines (the default severity `Warning` plus `TreatWarningsAsErrors` gives the same result). Recommend `Error`: the file's convention is to list every enabled rule. In `Debug.ruleset`, which lists Reihitsu rules only to switch them off, delete the two lines. No visible output either way.
2. **How the scan decides test code.** (a) From the project's restored package graph or `PackageReference` to `Microsoft.NET.Test.Sdk`/the property in the project file — consistent with how the harness models every other project fact, but a model of MSBuild (FC-044); (b) from the build's generated `.GeneratedMSBuildEditorConfig.editorconfig` — reads MSBuild's own result and would also verify the root `Directory.Build.props` items, but `scripts/test.sh --project selfhosting` runs `dotnet test` on `SelfHosting.Test` alone, which builds only it and its three references (Analyzer, Formatter, Formatter.Test), so the file would be missing for most projects in a focused run, and its path depends on the configuration. Recommend (a), together with a cheap check that the build actually passes the properties (see *Architecture findings*), because (b) makes the scan depend on a full prior build it does not depend on today (it already depends on a prior restore: `SelfHostingSolution.cs:280` fails without `project.assets.json`).
3. **Where the issue's two-method check lives.** In the self-hosting tests (through the scan's own option path), not in an analyzer test: RH2008/RH2111 analyzer tests already cover the rule's test/production split (SHARED-LINES-003 examples); what this run adds is the repository's recognition. Recommend the self-hosting tests.
4. **`AnalyzerResources` restructuring** — see *Refuse option*; recommend the partial split by category and updating `.claude/commands/add-resource-texts.md` in the same change (tooling path; the lane admits it).

### Issue goals

- "The repository's own limits … set in its `reihitsu.json`" — dropped by the maintainer's binding scope (already in `00-lead.md`); no visible output.
- "the self-hosting scans under `scripts/`" — stale: change to `src/Reihitsu.SelfHosting.Test`; the `scripts/` apps load no analyzer. No visible output.
- "the build reports no new diagnostics" — met trivially, since the committed `binaries/` lack both rules; as a goal it proves nothing. Replace it by: the self-hosting scan judges both rules with no violation, and the build passes both properties to the analyzers (observable in the generated MSBuild editorconfig). No visible output.
- "check with one over-limit test method and one over-limit production method" — keep, but through the scan's path (internal choice 3); it cannot run through the build in this run.
- The issue's figures "171 test methods over 60" and "28 test types over 2000" are measured against limits the rules do not use (test defaults are 100 and 1000); not a goal, only context.
- The maintainer expected "only a few changes". The production work is 23 declarations; the test work is 20 methods plus 94/28/11 classes by question 1. The production defaults are not poorly chosen by the rule's own measure (0.7 % of methods, 0.6 % of types), so only the test-type default is in question; the lead may want to give the maintainer these totals with question 1.

### Architecture findings

**One rule stated in two places: which MSBuild properties the analyzers see.** The change adds a second statement of "make `IsTestProject` and `IsTestingPlatformApplication` compiler-visible" — the package's `buildTransitive/Reihitsu.Analyzer.targets` and the root `Directory.Build.props` (or a new `Directory.Build.targets`) — and a third, a model of it, in the scan harness. The statements can drift: a property added to the package's list later (SHARED-LINES-003 names exactly these two today) would reach users but not this repository's build, which is the point of #1865 ("judged as every project that installs the package is"). Since nothing in this run's build runs the rules, a drift or a typo in the new items would only show at the next `/bump-version`, as a broken Release build. Shapes that avoid the drift without new machinery: one statement that both the package and the repository use, or the copy guarded by a cheap test that compares the two item lists and checks the generated editorconfig of one test project. This is within the components the run changes; it adds no type and a few lines. The lead decides.

### Follow-up candidates

_None._ (Observation, not a candidate: 15 enabled rules — RH5113, RH5308, RH5417, RH6023, RH6024, RH7110, RH7305A, RH7308–RH7311, RH7409–RH7412 — have no `Release.ruleset` entry. All report at `Warning`, which `TreatWarningsAsErrors` makes an error in Release, and RH7305A is off by default by design, so the Release build enforces them all the same; nothing to file.)

## thoenissen/Reihitsu#1866

### Premise

Real. `Debug.ruleset:101` holds `<Rule Id="RH08402" Action="None" />`; `Release.ruleset:433` holds `RH8402`. I compared every `Rule Id` in both rulesets with the 349 `DiagnosticId = "RH…"` constants in `src/Reihitsu.Analyzer`: `RH08402` is the only ID no rule has (Debug has 4 entries, Release 334). The rule meant is RH8402, which Release switches off, so correcting the ID makes the two rulesets agree.

RH8402 checks nothing without a `reihitsu.json` whose `copyright` section sets a text (RH8402-001), and no `reihitsu.json` is tracked in the repository (`git ls-files`), nor is one added as `AdditionalFiles` by any project file. So RH8402 reports nothing in either configuration before or after the correction; the entry hid nothing. Severity: cosmetic.

### Mechanism and candidates

_N/A._

### Breaking inputs

None. The correction has no observable diagnostic effect; a test of it would assert nothing a build shows. A ruleset-ID check (every `Rule Id` under `Reihitsu.Analyzer` names a shipped rule) would keep the class from recurring; whether that is worth a test is the lead's call (it is cheap, and it would also have caught this entry).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `Debug.ruleset` | must change (one ID) |
| `Release.ruleset` | must stay (already correct) |
| RH8402 analyzer, spec, rule page | not involved |

### Refuse option

Drop the entry instead of correcting it: same effect today (RH8402 reports nothing without configuration), but the two rulesets would then differ in how they treat RH8402. Recommend correcting, as the spec author did. No unsupported shape involved.

### Output questions

_None._

### Internal choices

Correct versus drop the entry — see *Refuse option*; recommend correct. No visible output either way.

### Issue goals

- "after checking whether the build then reports RH8402 anywhere" — answered: it reports nothing, there is no `reihitsu.json`. Keep the goal as satisfied.
- "the Debug and Release builds report the same RH8402 diagnostics" — holds before and after (none). No change.

### Architecture findings

_None._

### Follow-up candidates

_None._
