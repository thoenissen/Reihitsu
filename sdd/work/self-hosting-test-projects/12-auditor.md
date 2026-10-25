Role: auditor
Commit: 5459c5ddfb031e381d7901b83f61e60c853843bd
Verdict: pass, text cleanup

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

Finding #4 repeats the class of finding #2, and this handover lists an architecture finding. Under `squad/README.md`, *Architecture findings*, both go to escalation 12 before anything else happens; the text cleanup waits for that answer.

## Scope

Head `5459c5d`, base `origin/main` = `0b162f1` (the prompt's `git ls-remote` SHA). Checked: the checkout is the head SHA with a clean working tree, and `git merge-base --is-ancestor 0b162f1 HEAD` confirms the base is merged. This is a repair-round delta against the first audit at `11bc505` (`05-auditor.md`), covering `git diff 11bc505..5459c5d`. The diff is what handovers `06`–`11` and the lead's three decision records describe, and nothing more:

- three corrected default-run lines (`CONTRIBUTING.md:22`, `squad/README.md:19`, `.claude/agents/validator.md:3`);
- `AnalyzerViolationReportTests` and the new `Utilities/LoadedAnalyzerAssemblyLoader.cs`;
- `CONTRIBUTING.md` added to `LaneClassifier._textFiles`, plus the matching row in `scripts/README.md:112`;
- four new lane-check test cases and the `CLAUDE.md` data row;
- the decision records and the handovers.

The scope grew, by the maintainer's decision under escalation 10, into `Reihitsu.Tooling` (the lane classifier), a surface the first audit did not cover. I audited that surface in full: every rule of `ClassifyPath` that can classify a root file, its single caller (`LaneCheckCommand.Execute`), and every prose statement of the lane rules. Everything else in the self-hosting project, the scripts, and the workflows is byte-identical to the first audit, and that audit's evidence stands.

The cleaner's Release build and focused runs were taken at `542f8ef`. `git diff --stat 542f8ef 5459c5d` shows only `10-cleaner.md` and `11-spec-checker.md`, so no compiled file changed after those results (FC-032). `scripts/lane-check.sh tooling --base 0b162f1 --no-install` on the head gives `LANE CHECK: confirmed — claimed tooling, diff needs tooling`.

### Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | superseded | The maintainer accepted the analyzer scan's unchanged wall-clock and narrowed the issue's performance goal to the default local run (`squad/decisions/2026-10-25-self-hosting-analyzer-scan-wall-clock.md`). The repair changed no code there, so the measurement (15 m 13 s before, 16 m 01 s after) still stands. See the hint on the PR description |
| 2 | closed | The three members the finding listed now read "except the self-hosting scans, which CI runs" or equivalent, which matches `scripts/test.sh` `all)`. Another place the enumeration did not name still fails: finding #4 |
| 3 | closed | The tautological `HasCount` is gone. Completeness is now checked against Roslyn's metadata-based discovery (`AnalyzerFileReference.GetAnalyzers`), which does not use `AnalyzerCatalog`. `07` shows the test failing when analyzers are dropped and when a non-analyzer row is added. The method's summary was left behind: finding #5 |

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 4 | low | `squad/roles/auditor.md:36` | FC-015, FC-016 | #2 | PR-introduced | "Never run the full suite — the validator does." This change made the second clause false. The validator now runs the default set, which leaves out `Reihitsu.SelfHosting.Test`, and only CI runs that project. The clause says the same thing as the pre-repair `squad/README.md:19` ("runs all tests"), which finding #2 counted as stale and the maintainer had corrected. It is a ninth restatement of the same decision, and neither #2's enumeration nor the repair's `rg` (`06`: "all tests", "every test project", "all test projects", "runs all") found it. Enumeration of the class (every text statement of what the default run, or the validator, covers): **fails**: `squad/roles/auditor.md:36`. **holds**: `scripts/test.sh:12-15`; `scripts/README.md:13,78`; `AGENTS.md:14`; `CONTRIBUTING.md:22` and `:36` (the second only says "run the tests" and claims no coverage); `squad/README.md:19`; `squad/README.md:110` ("a full test run" names a kind of whole-tree result and assigns no coverage to anyone); `.claude/agents/validator.md:3`; `.claude/agents/README.md` validator row ("Runs three scripts"); `squad/roles/validator.md:3,19`; `squad/roles/implementer.md:28`; `squad/roles/test-implementer.md:53`; `sdd/changes/2026-10/introduce-sdd.md:170` (a historical change doc that records the state at its time). Text only: a Markdown role file that nothing compiles and no test reads (`rg squad src --glob '*.cs'` finds only the classifier's path prefix) | Make the clause match the default run, e.g. "Never run the full suite: the validator runs the default set, and CI runs the self-hosting scans." Whether this member gets the narrow fix or joins the consolidation follow-up from the escalation-12 answer A is the maintainer's call |
| 5 | low | `src/Reihitsu.SelfHosting.Test/Analyzers/AnalyzerViolationReportTests.cs:44` | FC-031 | – | PR-introduced | The repair changed `AnalyzerRowsNameEachAnalyzerTypeExactlyOnce` and kept its summary: "Verifies that there is one row per analyzer, each named by the analyzer type…". The method now checks only that `RH4106` has a row and that no two rows share a name. It cannot observe a missing analyzer. That guarantee now belongs to the new `AnalyzerRowsMatchTheAnalyzersTheCompilerLoadsFromTheAnalyzerAssembly`, so a reader who trusts the summary looks in the wrong method. This is not a repeat of #3: #3's guarantee (completeness) is now actually tested, and only the comment says where it is tested. Text only: an XML comment on a test method in an unshipped test assembly, so no IL, diagnostic, public API documentation, or rule metadata changes | Reword the summary to what the method asserts (row names are distinct, and a known analyzer has a row). The method name can stay: renaming it would be a compiled change, and next to the completeness test, "exactly once" reads as the uniqueness this method checks |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| The root `CONTRIBUTING.md` is text (`LaneClassifier._textFiles`) | The whole repository-relative path, exact match, `OrdinalIgnoreCase`. Checked after the change-doc prefix and before every tooling rule | `scripts/README.md:112` Text row; `squad/README.md:39` ("Docs — … Markdown …", which is stated as what the lane check derives); `ClassifySourcePath`'s `.md` rule under `src/` | `scripts/README.md` now names the same four files as `_textFiles`. `squad/README.md` agrees for `CONTRIBUTING.md` but not for the root `CODE_OF_CONDUCT.md` and `SECURITY.md`: the prose makes them docs, while the classifier returns `Unknown`, i.e. no lane. I constructed this in a scratch clone at the head by appending a space to each of the three files: `scripts/lane-check.sh docs --base HEAD --head worktree` lists `Unknown CODE_OF_CONDUCT.md`, `Text CONTRIBUTING.md`, `Unknown SECURITY.md` and ends `LANE CHECK: no lane`. Both files are pre-existing and outside the run's diff | holds for the change. The pre-existing members are under *Architecture findings* and *Hints* |
| The text rule takes precedence over the tooling rule for the new entry | `_textFiles` is tested before `_toolingPrefixes`, `_toolingRootFiles`, and `_toolingExtensions` | `_toolingRootFiles` (`AGENTS.md`, `CLAUDE.md`, …) | `CONTRIBUTING.md` is in no tooling list, so the precedence decides nothing new. A wider "root `*.md` is text" rule would have taken `AGENTS.md`/`CLAUDE.md` away from tooling; it was not chosen, and both are pinned as tooling by data rows | holds |
| Which analyzers the rows must cover (the completeness oracle) | `AnalyzerFileReference.GetAnalyzers(LanguageNames.CSharp)`: Roslyn reads `[DiagnosticAnalyzer]` types from the DLL's metadata and instantiates them | `AnalyzerCatalog.DiscoverAnalyzerTypes`: reflection, concrete, not an interface, assignable to `DiagnosticAnalyzer`, attribute found through inheritance | The sets are equal today (the test is green, per `07` and `10`). The ways the two predicates can diverge all fail an assertion instead of passing silently: a concrete subclass that inherits but does not declare the attribute, a VB-only attribute, or an abstract attributed type. Each shows up as a row-set difference or an `AnalyzerLoadFailed` entry | holds |
| `LoadedAnalyzerAssemblyLoader.LoadFromPath` accepts only the analyzer assembly | Ordinal equality of `Path.GetFullPath` of the requested path and of `Assembly.Location` | Roslyn passes back the path the `AnalyzerFileReference` was created with, which is that same `Location` | agree. Any other path throws, and Roslyn reports that as a load failure, which the test asserts empty | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| What the default local test run covers | `scripts/test.sh` `all)` (executable), plus the text restatements listed in finding #4 | finding #4 (repeat of #2). The consolidation is already a follow-up under the escalation-12 answer A |
| Which lane a changed path belongs to (here: which root files are text) | `LaneClassifier._textFiles`, `_toolingRootFiles`, `ClassifyPath` (the only code owner: `rg '_textFiles\|ClassifyPath'`; its only caller is `LaneCheckCommand.cs:51`); `scripts/README.md:109-116` (category table); `squad/README.md:37-43` (*What the lane check derives from the paths*) | One code owner and two prose restatements. One restatement disagrees for two existing files. See *Architecture findings* |
| Which types are shipped analyzers | `AnalyzerCatalog.DiscoverAnalyzerTypes` (SelfHosting.Test) and `AnalyzerMetadataDiscovery.DiscoverAnalyzers` (Analyzer.Test), both unchanged since the first audit. Roslyn's `AnalyzerFileReference` serves as an oracle in one test | The oracle is not a copy of the policy. It is the compiler's own discovery, used to check the catalog. Not a finding |
| Loading the analyzer assembly for discovery | `LoadedAnalyzerAssemblyLoader`, the only `IAnalyzerAssemblyLoader` in the repository (`rg IAnalyzerAssemblyLoader`) | one owner |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `AnalyzerRowsNameEachAnalyzerTypeExactlyOnce` (changed) | Row names are distinct, and `RH4106` has a row | Two rows with one name; `RH4106` missing | – | adequate for what it asserts; its summary claims more (finding #5) |
| `AnalyzerRowsMatchTheAnalyzersTheCompilerLoadsFromTheAnalyzerAssembly` (new) | Row names equal, as a set, the C# analyzers Roslyn loads from the analyzer DLL; nothing fails to load; the discovery is not empty | An analyzer dropped from the catalog, and a non-analyzer row (both shown in `07`); a load failure; an empty discovery | `AnalyzerFileReference` with `LoadedAnalyzerAssemblyLoader` | adequate. It compares simple names, and the sibling test plus `CreateAnalyzer`'s `Single` rule out name collisions |
| `ClassifyPathTreatsTheRootContributingGuideAsText(False/True)` (new) | `CONTRIBUTING.md` is `Text` whether or not the diff is proven text-only | `Unknown`, or a dependence on the proof (failed before the fix, `08`) | `LaneClassifier.ClassifyPath` | adequate |
| `ClassifyPathAssignsTheCategoryOfEachKindOfPath("CLAUDE.md", Tooling)` (new row) | The neighboring agent-instruction file stays tooling | A widening to "every root `.md` is text" | same | adequate |
| `ExecuteConfirmsTheDocsLaneForTheRootContributingGuide` (new) | A `CONTRIBUTING.md`-only diff exits 0, lists it as `Text`, and confirms the docs lane | Exit 3 (no lane), a different category in the listing, or a wider lane (failed before the fix, `08`) | `LaneCheckCommand.Execute` | adequate. It pins the exit code, the listing line, and the verdict line |
| `ExecuteConfirmsTheToolingLaneForTheRootContributingGuideNextToTooling` (new) | This run's shape (`AGENTS.md`, `CONTRIBUTING.md`, `scripts/test.sh`) confirms the tooling lane | Exit 3 (failed before the fix, `08`) | same | adequate |

## Failure classes dismissed
- FC-001, FC-002, FC-003: traced in *Guard scope*. The new list entry is an exact-path match with no qualifier left implicit. Both proof arms are tested, and so is the neighboring tooling file.
- FC-004 to FC-014, FC-018 to FC-030, FC-036, and the tab-width class matrix: the repair changes no analyzer, code fix, formatter, or Core code. It adds one string to a path list, adds tests, and edits text.
- FC-015, FC-016: not dismissed. See finding #4, *Policy ownership*, and *Architecture findings*.
- FC-017: see *Assertion adequacy*; finding #3 is closed.
- FC-031: finding #5. The other changed or new members have accurate summaries: `_textFiles`, `LoadedAnalyzerAssemblyLoader` and its members, and the four new tests. Added source lines have no `TODO`, no commented-out code, and no tracker number (`git diff 11bc505..5459c5d -- 'src/**' | grep '^+' | grep -E '#[0-9]+|TODO|issue|PR '` is empty).
- FC-032: no compiled change after the cleaner's build at `542f8ef` (see *Scope*).
- FC-033, FC-034: the lane tests use the exact path and the diff shape that produced "no lane" in this run, run through the real classifier, and fail before the fix (`08`).
- FC-035: the expected values come from the decision record and `squad/README.md` *Lanes*, not from the patched classifier. The tests were red before `LaneClassifier.cs` changed (`08`, `09`).
- FC-037: no text-only proof is claimed for a C# change. The three text corrections are in non-compiled files.

## Architecture findings

One rule is stated in more than one place, and the statements have drifted apart: which lane a changed path belongs to.

- **Components.** `LaneClassifier.ClassifyPath` (`src/Reihitsu.Tooling/LaneClassifier.cs`) executes the rule. `scripts/README.md:109-116` restates it as a category table, and `squad/README.md:37-43` restates it as *What the lane check derives from the paths*. The prose says "Docs — `sdd/`, `documentation/`, Markdown, issue and PR templates…". The classifier makes a root Markdown file text only if it is listed by name (`README.MD`, `CONTRIBUTING.md`); every other root `.md` that is not an agent-instruction file is `Unknown`, which the lane check reports as no lane.
- **How it reached the run.** The repair's `CONTRIBUTING.md` edit produced "no lane" (escalation 10). The maintainer chose to classify that one file as text in this run (`squad/decisions/2026-10-25-lane-check-contributing-md-unclassified.md`, answer B). That escalation offered no structural option. The implementer listed the consolidation as a follow-up candidate (`09`) and decided it was not an architecture finding, but that choice belongs to the maintainer (`squad/README.md`, *Architecture findings*).
- **Places that still disagree.** The root `CODE_OF_CONDUCT.md` and `SECURITY.md` are both tracked and both linked from `README.MD:117`. The prose makes them docs. The classifier makes them `Unknown`, as constructed in *Guard scope*: `LANE CHECK: no lane`. The next docs-lane run that edits either file will stop at escalation 10, as this run did. Both are pre-existing and outside this run's diff, so they are not blocking findings. The prose also leaves open the overlap between "Markdown" (docs) and `scripts/`, `.claude/`, `squad/`, `.github/` (tooling). The classifier resolves that overlap by tooling taking precedence, but no statement says so.

## Hints
- The PR description should say plainly that the analyzer self-hosting scan's wall-clock did not drop (issue thoenissen/Reihitsu#1467, *Done when* item 3) and that the maintainer accepted this (`squad/decisions/2026-10-25-self-hosting-analyzer-scan-wall-clock.md`), as `AGENTS.md`, *Issue intake*, requires when the shipped change differs from the request.
- `CODE_OF_CONDUCT.md` and `SECURITY.md` are classified `Unknown` by the lane check (pre-existing; structural cause in *Architecture findings*). Follow-up candidate, together with the one in `09`.
- The first audit's hints are unchanged and still hold, because the code they name is byte-identical: `_ignoredDiagnosticIds`, the formatter scan's directory list, `IsInBuildOutputPath` on absolute paths, green formatter rows for syntax-invalid files, the duplicated `all`-branch regex, the wording of *The self-hosting scans*, and the `dotnet-coverage` exit code.
