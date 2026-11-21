Role: auditor
Commit: 3298297
Verdict: pass, text cleanup

No intended fix, suspected cause, or hoped-for verdict was carried in the prompt.

## Scope
Full audit. Head 329829721dd33694ee50f4872754b324afba6096 (checked out); base `origin/main` 5af53bd1b2c825c9ea292fb5b11d9963bb4ac9c1, fetched and an ancestor of the head. The diff outside `sdd/work/` is `Directory.Build.props`, `scripts/README.md` (*The line-count rows*), and `src/Reihitsu.SelfHosting.Test/Repository/BuildConfigurationTests.cs`, as handovers 01 and 02 describe; every commit after `a691cc8` touches only `sdd/work/`, so the cleaner's Release build and the focused test results are on the audited tree.

Issue #1880 (`agent-filed`) "Done when": (1) the Release build passes the root `reihitsu.json` with no RH0001 report — met: unconditional `AdditionalFiles` item, and the implementer's `--no-incremental` Release build shows no RH0001 with the 1.5.0 `binaries/`; (2) a type over 500 lines fails the build — met as a property of the Release build (`Release.ruleset` sets RH2111 to Error, `TreatWarningsAsErrors`), shown by the test implementer's probe with the limit lowered to 20 (74 × RH2111); not pinned by a committed test, which I accept: building a project from a test would duplicate CI; (3) the build-configuration test asserts the item is passed — met.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | low | `src/Reihitsu.SelfHosting.Test/Utilities/SelfHostingSolution.cs:26-27` and `:86-90` | FC-031 | – | PR-introduced | Two doc comments state the behavior this run removes. The `ConfigurationFileName` summary says "the build does not pass it until `binaries/` is rebuilt"; the `GetAnalyzerOptions` summary says the root `reihitsu.json` is an additional file "which the build does not pass". Both are false after the new `AdditionalFiles` item. Outside the diff, so the implementer's owner search (build files only) and the spec checker's check 5 missed them. Confined to XML doc comments in a test-project utility: no compiled behavior, diagnostic, or rule metadata can change. | Reword both to say the scan passes the root file as the build does (e.g. "which the build passes too"); prove with `scripts/verify-text-only.sh`. |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which items the test counts as "the root configuration item" | `AdditionalFiles` elements of the root `Directory.Build.props` and `Directory.Build.targets`, `Include` resolved with `$(MSBuildThisFileDirectory)` expanded to the build file's directory | `ConfigurationManager.GetConfiguration` (`src/Reihitsu.Analyzer/Data/ConfigurationManager.cs:58`): first additional file whose file name is `reihitsu.json`, case-insensitive | The test is stricter (exact full path of the root file); a different `reihitsu.json` would satisfy the analyzer but not the test, which is the intended direction | ok |
| Whether the item applies in every project and configuration | `Condition` on the item and every ancestor (`ItemGroup`, `Choose`/`When`, `Project`) | MSBuild evaluation of the root props in every project under `src/`; no project-level `Directory.Build.props` under `src/`, no `AdditionalFiles Remove` or `Analyzer Remove` in any `*.csproj` (`git grep`). The five `scripts/*/Directory.Build.props` stop the search deliberately for file-based apps outside `Reihitsu.sln`, which also receive no `binaries/` analyzers | agree | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Passing the root `reihitsu.json` to the analyzers of the build | `Directory.Build.props:64` only (`git grep -E "AdditionalFiles|reihitsu\.json" -- '*.csproj' '*.props' '*.targets' '*.nuspec'`); the package's `buildTransitive/Reihitsu.Analyzer.targets` adds only `@(RazorComponent)`. The tracked tree holds one `reihitsu.json`, at the root | one owner, ok |
| Passing the root `reihitsu.json` in the self-hosting scan | `SelfHostingSolution.Create` (`SelfHostingAdditionalText` over the root file) | separate mechanism by design (the scan builds compilations itself and never reads MSBuild items), so no double pass; its comments are finding #1 |
| Statements of whether the build enforces the root limits | `scripts/README.md:103` (updated), `Directory.Build.props:59-62` (updated), `BuildConfigurationTests` summaries (updated), `SelfHostingSolution.cs:26-27, 86-90` (stale, #1); `squad/notes/2026-11-19-enable-line-count-rules.md` and `sdd/changes/` are history and stay | #1 |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `BuildConfigurationTests.RepositoryBuildPassesRootConfigurationToTheAnalyzers` | The root build files pass exactly the root `reihitsu.json` as one `AdditionalFiles` item that no condition restricts | Item absent, pointing at another file, duplicated in the root files, or conditioned on itself or any enclosing element — the test implementer showed absent and Release-conditioned both turn it red | `ResolveBuildFilePath` (expands only `$(MSBuildThisFileDirectory)`; any other property leaves the path unresolved and the test red — the safe direction) | adequate. It does not see a `*.csproj` that removes the item or a new nested `Directory.Build.props` that stops the import; neither exists, and either would also drop the `binaries/` analyzers the class already relies on, so no finding |

## Failure classes dismissed
- FC-004, FC-005, FC-019–FC-021 (trivia and directives), FC-006, FC-022–FC-029, FC-036 (rewrites), FC-007–FC-009, FC-018, FC-030 (code fixes), FC-010–FC-014 (formatter stability), FC-038–FC-043 (unsupported shapes), FC-044: the change is an MSBuild item, a test of the build files, and a README paragraph; no analyzer, code fix, formatter phase, or precheck is touched.
- FC-001, FC-002: the one predicate (the test's item selection and condition check) is traced above; the qualifiers "every project" and "every configuration" are explicit conditions (no `Condition` on the item or an ancestor).
- FC-003: both sides covered (present unconditioned vs. absent, and vs. conditioned), per handover 01.
- FC-015, FC-016: one owner of the build item; no sibling build file adds or removes it.
- FC-017: see *Assertion adequacy*.
- FC-031: finding #1.
- FC-032: no compiled change after `a691cc8`; the cleaner's Release build and the focused test run are on the audited tree.
- FC-033, FC-034: not a bug-fix reproduction.
- FC-035: the expected value is the root file's path, computed independently of the patched build file.
- FC-037: no comment-only change is claimed proven.

Visible output (check 10): no test of an end-user surface changes; the effect is on the repository's own build only. Special cases (check 11): none added.

## Architecture findings
_None._

## Hints
- Any later addition of a new configuration section to the root `reihitsu.json` fails the Release build with RH0001 until `binaries/` know the section — the same constraint that blocked this issue. `RootConfigurationSetsOnlyTheProductionTypeLimitOfFiveHundred` pins the file's content, so such a change is always deliberate; `scripts/README.md` names the rule-change lag but not this one. Not a defect of this run.
- In Visual Studio, an `AdditionalFiles` item outside a project's directory shows as a linked `reihitsu.json` node in every project of Solution Explorer; `Visible="false"` would hide it. Cosmetic, the maintainer's taste.

## Follow-up candidates
_None._
