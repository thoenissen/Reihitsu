Role: auditor
Commit: 11bc50574621a2a9e342b1de129e18cd248bcb34
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit of head `11bc505` against base `origin/main` = `0b162f1` (confirmed with `git ls-remote origin refs/heads/main`; `git merge-base --is-ancestor` confirms it is merged; the merge commit brings in only `squad/` text from #1466). Checkout is the head SHA, working tree clean. The diff matches the handovers `00`–`04`: new project `src/Reihitsu.SelfHosting.Test`, deleted scans in `Analyzer.Test`/`Formatter.Test`, the anchor change in `AnalyzerMetadataDiscovery`, `TestScriptProjectSelectionTests`, `Reihitsu.sln`, `scripts/test.sh`, `scripts/README.md`, `SonarCloud.yml`, `AGENTS.md`, `sdd/README.md`, and three `squad/roles/` files. `scripts/lane-check.sh tooling --base 0b162f1`: `LANE CHECK: confirmed — claimed tooling, diff needs tooling`.

Issue thoenissen/Reihitsu#1467, *Done when*:

| Item | Status | Evidence |
|---|---|---|
| Default `scripts/test.sh` no longer runs the scans; `--project <new>` runs them | holds | `all)` branch lists the seven other projects; `selfhosting)` selects `src/Reihitsu.SelfHosting.Test/Reihitsu.SelfHosting.Test.csproj`; no scan is left in `Analyzer.Test` or `Formatter.Test` |
| CI runs them on every pull request | holds | `SonarCloud.yml` runs on `pull_request` to `main`; the in-repo path loops over the project, the fork path runs `dotnet test` over `Reihitsu.sln`, which lists it |
| One test result per analyzer, and wall-clock on a 4-core runner drops measurably compared with the single test | **fails** (second half) | finding #1 |
| A deliberate violation still fails CI, naming file, line, and rule | holds | report format pinned by `ReportNamesFileLineRuleAndAnalyzerOfAViolation`; row failure shown by the test implementer (`01`); the CI loop's failure path is unchanged from the old scan, which ran in the same loop |

The issue's bullets (rows via `[DynamicData]` named by type, each row only its own diagnostics, compilations built once behind a thread-safe cache, formatter scans split per file, convention tests left in `Analyzer.Test/SelfHosting/`) hold in the code.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.SelfHosting.Test/Utilities/AnalyzerViolationReport.cs:51`; `src/Reihitsu.SelfHosting.Test/Utilities/SelfHostingSolution.cs:28` | – (issue requirement, *Done when* item 3) | – | same requirement | The analyzer scan's wall-clock does not drop. Measured on this 4-core, 16 GB sandbox, Release, one run after the other with nothing else running: **old single test 15 m 13 s** (`scripts/test.sh --no-install --no-build --project analyzer --filter "FullyQualifiedName=Reihitsu.Analyzer.Test.SelfHosting.SelfHostingTests.AnalyzersReportNoViolationsOnOwnSourceCode"` on a `git archive` of `0b162f1` built in Release) against **new 340 rows 16 m 01 s** (`scripts/test.sh --no-install --no-build --project selfhosting --filter "FullyQualifiedName~Reihitsu.SelfHosting.Test.Analyzers.AnalyzerSelfHostingTests"` at head). The test implementer's independent Release measurement (16 m 17 s, `01`) agrees; its handover deferred the comparison to CI instead of making it. Why: each row still calls `compilation.WithAnalyzers(...)` for every one of the 13 compilations, and `CompilationWithAnalyzers` analyzes its own copy of the compilation, so every row binds every project again — 340 × 13 full bindings, the same total work as the old nested loop. The `Lazy` cache shares only parsing and reference resolution. The old test already used Roslyn's concurrent analysis (2.1–2.5 cores busy in `top` snapshots); the new rows averaged about 2.4 cores (38 m 34 s user CPU over 16 m 03 s) — parallel rows add contention, not throughput. Members of the class: analyzer rows fail; formatter idempotency and no-change rows hold (old two tests 4 m 41 s → new 3243 rows 2 m 29 s, same conditions), because those rows split work that was strictly sequential before. Not measured: the CI runner itself; its 4 cores and the `dotnet-coverage` instrumentation that wraps both the old and the new run give no reason to expect a different ratio | Make the rows share the binding instead of only the parse: for example, one `CompilationWithAnalyzers` per compilation holding every analyzer, built once behind the same thread-safe cache, from which each row requests only its own analyzer's diagnostics; or compute every analyzer's diagnostics per compilation once and let each row select its own. Keep "each row reports only its own diagnostics". Re-measure against the 15 m 13 s baseline under the same conditions and record both numbers |
| 2 | low | `CONTRIBUTING.md:22`; `squad/README.md:19`; `.claude/agents/validator.md:3` | FC-015, FC-016 | – | PR-introduced | The change made three statements of the default test run false: `CONTRIBUTING.md` says `scripts/test.sh # run all test projects`, the squad roles table says the validator "Builds the solution and runs all tests", and the validator wrapper's description says it "runs every test project". The run updated the identical `AGENTS.md:14` comment and `squad/roles/validator.md:3` but not these copies (the implementer's `rg` scope in `02` left out `CONTRIBUTING.md` and the phrases "all tests" / "every test project"). Text only: none of these files is compiled, read by a test, or rule metadata | Say "all test projects except the self-hosting scans, which CI runs" (or equivalent) in the three places |
| 3 | low | `src/Reihitsu.SelfHosting.Test/Analyzers/AnalyzerViolationReportTests.cs:52` | FC-017 | – | PR-introduced | `AnalyzerRowsNameEachAnalyzerTypeExactlyOnce` asserts `HasCount(AnalyzerCatalog.DiscoverAnalyzerTypes().Count(), names)`, but `names` is `DiscoverAnalyzerTypes().Select(type => type.Name)`: the assertion compares the list with itself and cannot fail. The test's name promises a row for *each* analyzer type; an analyzer the discovery drops is not observable by it. The distinctness and `RH4106` assertions are real | Drop the tautological count, or check completeness against an enumeration that does not go through `DiscoverAnalyzerTypes` |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Default local run leaves the scans out | `all)` branch body of `scripts/test.sh` | test projects of `Reihitsu.sln` (by `Microsoft.NET.Test.Sdk`), `_expectedTestProjects`, the `SonarCloud.yml` loop | `all` = solution − selfhosting (exact order asserted); loop = solution − architecture (architecture runs in `build.yml`); fork path = whole solution | holds |
| `--project selfhosting` selects one project | `selfhosting)` line | `NamedSelfHostingSelectionSelectsOnlyTheSelfHostingProject` regex | single-line `projects=("…")` form, same as the other aliases | holds |
| Which files are generated | `RepositoryRoot.IsGeneratedSourceFile` (suffix, `OrdinalIgnoreCase`), now used by both scans | `Reihitsu.Cli.GeneratedFileUtilities.IsGeneratedFile` | identical suffix set and comparison; the formatter scan widened from `.Designer.cs`/`.g.cs` to include `.g.i.cs`; no tracked file has any of these suffixes | holds |
| Which paths are build output | `RepositoryRoot.IsInBuildOutputPath` (substring `/bin/`, `/obj/`, `OrdinalIgnoreCase`) on solution-relative file paths; on absolute paths for `.csproj` discovery | old formatter scan (same substrings, `Ordinal`); CLI `IsInBuildOutputDirectory` (per segment, platform comparer) | the formatter scan now also skips case variants (`Bin/`); no tracked path is under any case variant of `bin`/`obj` | holds; absolute-path case is a pre-existing hint |
| Which files the formatter scan covers | `FormatterSourceCatalog.SourceDirectories` (9 directories) | projects the analyzer scan discovers (every plain-SDK project with sources) | the PR adds `src/Reihitsu.SelfHosting.Test`, so the moved code stays covered; the pre-existing gap to the analyzer scan is unchanged | holds for the change; hint |
| One build of the compilations for all rows | `Lazy<SelfHostingSolution>` with `ExecutionAndPublication` | old per-test `compilationCache` | same discovery, references, and error assertion; a build failure is cached and rethrown to every row; cancellation is no longer honored during the build (it runs once, outside any one row) | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| What the default local test run covers | executable owner `scripts/test.sh` `all)`; restated in `scripts/test.sh` header, `scripts/README.md` (table, *The self-hosting scans*, tooling paragraph), `AGENTS.md:14`, `squad/roles/validator.md:3,19`, `squad/roles/implementer.md:28`, and — stale — `CONTRIBUTING.md:22`, `squad/README.md:19`, `.claude/agents/validator.md:3` | finding #2; architecture finding below |
| Which projects CI tests | `SonarCloud.yml` loop; guarded by `SonarCloudTestLoopRunsEverySolutionTestProjectExceptArchitecture` | one owner, guarded |
| Generated-file suffixes | `RepositoryRoot.IsGeneratedSourceFile` (test), `GeneratedFileUtilities.IsGeneratedFile` (CLI), `SelfReferentialTrackerReferenceTests._generatedFileSuffixes` (architecture) | identical sets in separate assemblies; the PR merged two test copies into one; not a finding |
| Which types are shipped analyzers | `AnalyzerCatalog.DiscoverAnalyzerTypes` (SelfHosting.Test), `AnalyzerMetadataDiscovery.DiscoverAnalyzers` (Analyzer.Test) | byte-identical predicate; the duplication existed before (the old scan carried its own copy in the same assembly); not a finding |
| Line-ending normalization for the formatter scans | `FormatterSelfHostingTests.NormalizeLineEndings`/`DescribeLineEnding`, `FormatterTestsBase` (Formatter.Test), `RegionDirectiveUtilitiesTests` (Core.Test) | byte-identical bodies; the moved scan lost its base class because a reference to `Formatter.Test` would couple the test projects; not a finding |
| `RequirementAttribute` | one per test project, by design (`sdd/README.md:80`) | identical to the other copies except the BOM |
| Self-hosting compilation model | `SelfHostingSolution`, `SelfHostingProject` only; the `Analyzer.Test` copy is deleted | one owner |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `ShellAllIncludesEverySolutionTestProjectExceptSelfHosting` | `all` = the seven projects in order; solution test projects = those seven + selfhosting | selfhosting added to `all`; a solution test project missing from `all` | file parse | adequate |
| `ShellAllLeavesOutTheSelfHostingProject` | `all` omits selfhosting | selfhosting in `all` | file parse | adequate; subsumed by the test above |
| `NamedSelfHostingSelectionSelectsOnlyTheSelfHostingProject` | alias selects exactly the project | missing alias, other path | regex | adequate |
| `SolutionListsTheSelfHostingTestProject` | the solution lists the project | project line removed | `Reihitsu.sln` parse | adequate (configuration lines unchecked; a missing `Build.0` would fail CI's `--no-build` run visibly) |
| `SonarCloudTestLoopRunsEverySolutionTestProjectExceptArchitecture` | loop = solution − architecture | a solution test project missing from the loop | workflow regex | adequate |
| `SonarCloudTestLoopRunsTheSelfHostingProject` | loop names the project | removed from the loop | workflow regex | adequate; subsumed by the test above |
| `AnalyzerReportsNoViolationsOnOwnSourceCode` (340 rows) | no diagnostic of this analyzer on own source | any source diagnostic | `SelfHostingSolution`, `AnalyzerViolationReport` | adequate; same compilation model and filtering as the old scan |
| `AnalyzerRowsNameEachAnalyzerTypeExactlyOnce` | one uniquely named row per analyzer | duplicate names (caught); a dropped analyzer (not caught) | – | weak: finding #3 |
| `AnalyzerRowNameCreatesThatAnalyzer` | a row name resolves to its type | wrong type | – | adequate |
| `ReportNamesFileLineRuleAndAnalyzerOfAViolation` | `relative/path:line [ID] (Analyzer) ` with a one-based line | zero-based line, absolute path, missing ID | synthetic compilation + RH4106 | adequate (mutation shown in `01`) |
| `ReportIsEmptyForCodeWithoutViolations` | no report for clean code | spurious line | synthetic compilation | adequate |
| `ReportListsViolationsOfEveryCompilationOrderedByFileAndLine` | every compilation reported, ordered by file then position | a compilation dropped; compilation order leaking into the report | two compilations passed later-first | adequate for file order; within-file order matches Roslyn's own order, so `ThenBy` is not independently pinned (hint) |
| `FormatterIsIdempotentOnSourceFile` / `FormatterProducesNoChangesOnSourceFile` (1621 rows each) | FMT-PIPE-004 per file under LF and CRLF; second pass equals first; output equals input | any non-idempotent or changed file | `ReihitsuFormatter.FormatSyntaxTree` | adequate; same strength as the old scans (both line endings, two passes); the no-change comparison now reads the input back from the parsed tree, which Roslyn round-trips exactly |
| `SourceFileRowsCoverEveryScannedDirectoryAndSkipBuildOutput` | rows unique, forward slashes, every directory represented, no `bin`/`obj`/generated file | a broken build-output filter would admit `obj/**/*.AssemblyInfo.cs` | – | adequate |

## Failure classes dismissed
- FC-001, FC-002, FC-003: traced in *Guard scope*; every changed boundary is tested on both sides (default run omits / alias selects; violation / no violation).
- FC-004 to FC-014, FC-018 to FC-030, FC-036: no analyzer, code-fix, formatter, or Core code changes; the formatter and analyzers are only exercised.
- FC-005, FC-019 to FC-021: no trivia or directive handling in the change.
- FC-031: every changed or moved method's summary describes the new code (`SelfHostingSolution`, `AnalyzerViolationReport`, `AnalyzerCatalog`, `FormatterSourceCatalog`, `RepositoryRoot`, the `test.sh` header). No `TODO`, no commented-out code, no tracker numbers in added lines.
- FC-032: the cleaner's Release build and the implementer's full self-hosting run were taken at `69038da`; no compiled file changed after it (the later commits are handovers and the `squad/` merge). My timing runs were on head after an incremental Release build.
- FC-033: the formatter rows keep both line endings; the analyzer rows keep the old compilation, reference, and filtering model unchanged.
- FC-034, FC-035: no reproduction test; expected values in the report tests are hand-written, not taken from patched code.
- FC-037: no text-only proof is claimed for a C# change.

## Architecture findings

One rule stated in more than one place, drifted apart by this change (finding #2). The decision is what the default local test run covers — equivalently, which tests the validator runs. Its executable owner is the `all)` branch of `scripts/test.sh`; it is restated in eight text places, of which the run updated five (`scripts/test.sh` header, `scripts/README.md`, `AGENTS.md:14`, `squad/roles/validator.md`, `squad/roles/implementer.md`) and left three false: `CONTRIBUTING.md:22` ("run all test projects"), `squad/README.md:19` (validator "runs all tests"), `.claude/agents/validator.md:3` (validator "runs every test project"). The defect is text only and reaches no compiled behavior.

## Hints
- `AnalyzerViolationReport._ignoredDiagnosticIds` is an always-empty set described as "temporarily excluded" IDs; carried over unchanged from the old scan (pre-existing dead code).
- `FormatterSourceCatalog.SourceDirectories` omits five self-hostable projects the analyzer scan covers (`Reihitsu.ArchitectureTests`, `Reihitsu.Core.Test`, `Reihitsu.Tooling`, `Reihitsu.Tooling.Test`, `Reihitsu.Playground.Test`), although `squad/roles/test-implementer.md:53` says the formatter scan "covers every file"; pre-existing, structural cause: a hard-coded list next to a discovered one.
- `SelfHostingSolution.DiscoverProjects` applies `IsInBuildOutputPath` to absolute `.csproj` paths, so a clone under any directory named `bin` or `obj` would discover no project and fail every row with "No C# projects"; pre-existing (the old scan did the same).
- A formatter row passes without checking anything when both line-ending variants of its file have syntax errors; pre-existing (the old loop skipped such files silently), now visible as a green row per file.
- The `all`-branch regex literal appears twice in `TestScriptProjectSelectionTests` (lines 61 and 208).
- `scripts/README.md`, *The self-hosting scans*: "names the file, the line, and the rule" — a formatter row names the file and lines, but no rule.
- CI failure propagation of the SonarCloud loop relies on `dotnet-coverage collect` returning the test run's exit code; unchanged by this run (the old scan ran in the same loop), not verified here.

Timing logs (outside the repository): `/tmp/claude-0/-home-user-Reihitsu/4c43f5d8-f42d-5c02-bb86-0b7e84f6b696/scratchpad/base-old-scan.log`, `/tmp/claude-0/-home-user-Reihitsu/4c43f5d8-f42d-5c02-bb86-0b7e84f6b696/scratchpad/head-new-scan.log`, `/tmp/claude-0/-home-user-Reihitsu/4c43f5d8-f42d-5c02-bb86-0b7e84f6b696/scratchpad/base-old-fmt.log`, `/tmp/claude-0/-home-user-Reihitsu/4c43f5d8-f42d-5c02-bb86-0b7e84f6b696/scratchpad/head-new-fmt.log`.
