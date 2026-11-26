Role: auditor
Commit: e4c8eb9
Verdict: #1924: pass; #1949: pass

Read: `squad/roles/auditor.md`, `squad/README.md`, `00-lead.md`, `08-auditor.md`, `09-validator.md`, `10-test-implementer.md`, `11-cleaner.md`, `12-spec-checker.md`, `git diff ec68841 e4c8eb9 -- . ':(exclude)sdd/work'`, the whole of `tests/Reihitsu.SelfHosting.Test/Utilities/SelfHostingSolution.cs`, `SelfHostingProject.cs`, every `*.csproj`, and the counterparts named below. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## thoenissen/Reihitsu#1924

Nothing to audit: the repair touches no file of this issue (`PipelineTraceCommand.cs` and its tests are byte-identical to ec68841). `08-auditor.md` stands.

## thoenissen/Reihitsu#1949

The repair is in the self-hosting harness, which this issue's `CodeFixRunCommand.cs` exposed; it is audited once under *Whole run*. The issue's production and tooling files are byte-identical to ec68841, so `08-auditor.md` stands for them.

## Whole run

### Scope

- Head: `e4c8eb9e44167f7039aca4a0235021e65f9b1663` (checked out, clean). Base: `origin/main` = `68f236ea94fdcbcf4aa14f98ff759d2c06708f32`, equal to `git ls-remote origin refs/heads/main` and an ancestor of the head.
- Repair delta against `ec68841` (the tree of `08-auditor.md`; the validator's commit `7a6163d` differs from it only under `sdd/work/`). Outside `sdd/work/`, the delta is `tests/Reihitsu.SelfHosting.Test/Utilities/SelfHostingSolution.cs` only, as `10-test-implementer.md` and `12-spec-checker.md` describe. Scope did not grow: no production, tooling, spec, or change-doc path changed.
- `scripts/lane-check.sh behavior-change` → `LANE CHECK: confirmed — claimed behavior-change, diff needs behavior-change`.
- No test re-run: no finding turned on one. The test implementer's and cleaner's runs (line-count rows 18/18, self-hosting default rows 6253/6254 with only the expected `WorkingDocumentsDirectoryContainsNoFiles` failure, Release build 0 warnings) stand.

### Previous findings

`08-auditor.md` had no findings; there is nothing to close, keep open, or supersede.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_N/A — no finding._

What the repair establishes, checked against the build it models:
- **Same reference set as MSBuild.** MSBuild passes the outputs of transitive project references to the compiler unless a `ProjectReference` sets `PrivateAssets`, `ReferenceOutputAssembly="false"`, or the project sets `DisableTransitiveProjectReferences`. `rg` over every `*.csproj`, `*.props`, `*.targets` finds none of these on a `ProjectReference` (the `PrivateAssets` hits are all `PackageReference`s). So the closure the walk computes is exactly what the real build references; the harness is not more permissive than the build. The new references per project: `Reihitsu.Tooling` gains `Reihitsu.Formatter` and `Reihitsu.Core` (the case the validator hit; `InternalsVisibleTo("Reihitsu.Tooling")` applies because the compilation's assembly name is `Reihitsu.Tooling`), `Reihitsu.Tooling.Test` gains `Analyzer`, `CodeFixes`, `Formatter`, `Core`, and `Cli.Test`/`SelfHosting.Test` gain `Core` — each also what the build passes.
- **Termination and cycles.** The walk keys `visited` on the canonical `ProjectFilePath` of the dictionary entry (reference paths are `Path.GetFullPath`ed in `SelfHostingProject.Load`, the dictionary is case-insensitive like the set), so each project is enqueued once and the loop ends on any graph. The start project is not pre-marked: on a cycle `A → B → A` the walk returns `A` as its own reference, and `CreateCompilation(A)` then fails with the existing "cyclic project-reference graph" message through `compilationStack` — the same outcome as before the repair.
- **Unknown references.** A reference outside `projectsByFilePath` still fails with "references undiscovered project", now naming the project that holds the reference (`currentProject`). Every project the walk reaches beyond the direct ones is itself a discovered project whose own compilation already requires all its direct references to be discovered, so the walk fails in no graph where the old code passed. The unhostable project `Reihitsu.Playground` (`Microsoft.NET.Sdk.BlazorWebAssembly`) is referenced only by `Reihitsu.Playground.Test`, which `DiscoverProjects` skips, and no project references `Playground.Test`.
- **Caching.** Each reached project's compilation comes from `compilationCache`, so the wider set costs no second compilation.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which projects a self-hosted compilation references | the `ProjectReference` closure from the project's file, through discovered projects | MSBuild's transitive project references (no `PrivateAssets`/`ReferenceOutputAssembly`/`DisableTransitiveProjectReferences` in the repository) | same closure | holds |
| Which projects are skipped as unhostable (`DiscoverProjects`, unchanged) | direct references only | the compilation's reference closure (now transitive) | differs in span, but no project reaches an unhostable one other than directly, and a project behind a skipped project already failed with "undiscovered project" before the repair; the repair widens no failing case. Decision 12 dropped it as unreachable | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Transitive project-reference walk | `rg "ProjectReferencePaths\|ProjectReference\b" tests src --glob '*.cs'`: `SelfHostingSolution.GetTransitiveProjectReferences` (compilation references) and `PhaseOwnershipDocumentationTests.CollectFormatterProjectDirectories` (`tests/Reihitsu.ArchitectureTests/Formatter/PhaseOwnershipDocumentationTests.cs:770`, the directories of the formatter and the projects it reaches, for the guard-name lookup) | two walks in two test assemblies that share no utility project, deciding different things (compilations vs. directories to search); not a duplicated policy. Not a finding |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `LineCountSelfHostingTests.AnalyzerReportsNoViolationsOnOwnSourceCode` rows RH2008, RH2111 (unchanged) and every other self-hosting row | every self-hostable project compiles without errors with the references its build has, then the analyzer reports nothing | a compilation error (as at 7a6163d: CS0234/CS0103 in `CodeFixRunCommand.cs`), or a violation | `SelfHostingSolution.AssertNoCompilationErrors` fails on any error-severity diagnostic | adequate: the Tooling compilation is a live transitive case, so the walk is observed by the rows that failed before; a walk that missed a level would bring the same errors back. No dedicated harness unit test is needed for that (`10-test-implementer.md`, *Not covered*) |

### Code quality and comments

- `GetTransitiveProjectReferences`'s summary, parameters, and return describe the new code; `CreateCompilation`'s summary ("including transitive project references") is now true, and `GetCompilationReferences`'s summary still holds (FC-031). No `TODO`, no commented-out code, no issue or PR number. `scripts/format.sh` left the file unchanged (`11-cleaner.md`).
- The method is `private static`, BFS with a `Queue` and an `OrdinalIgnoreCase` set, consistent with the file's other path sets.

### Failure classes dismissed
- FC-001, FC-002, FC-044: the only predicate is the reference closure; see *Guard scope*.
- FC-003: the boundary direct/transitive is exercised by the Tooling compilation (transitive) and every other project (direct).
- FC-015, FC-016: see *Policy ownership*; the one closure feeds every compilation.
- FC-017: see *Assertion adequacy*.
- FC-031: see *Code quality and comments*.
- FC-032: no project result is reused across a compiled change; the compilation cache lives for one test run.
- FC-004 to FC-014, FC-018 to FC-030, FC-033 to FC-043: formatter, analyzer, code-fix, trivia, precheck, and expected-value classes; the repair changes only how a test harness assembles compilation references, and no expected value of any test changed.

### Visible output is decided

The repair changes no formatter, analyzer, code-fix, rule-page, or CLI expected value; the harness is test infrastructure. Nothing undecided.

### Architecture findings
_None._ The repair removes a gap between the harness and the build it models (`CreateCompilation`'s documented contract) rather than adding a second statement of a rule.

### Hints
- The `InternalsVisibleTo("Reihitsu.Tooling")` hint of `08-auditor.md` stands unchanged.

### Follow-up candidates
_None._ (`DiscoverProjects`' direct-only unhostable check was dropped by decision 12 as unreachable; nothing in this repair changes that.)
