Role: auditor
Commit: 31a764778adac1cb4882018d39ebf274c216369c
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore.

## Scope

Full audit. Head 31a7647 (checked out, clean tree). `origin/main` and `git ls-remote origin refs/heads/main` are both d63eab4bee09632526c02bfb4a46f54aabdd8001, and it is an ancestor of the head. The diff against `origin/main` (33 files) matches what handovers 01–09 describe: the change doc, the process text in `squad/`, `scripts/README.md`, `CLAUDE.md`, `.claude/agents/`, `sdd/templates/follow-up-issue.md`, the classifier and command in `src/Reihitsu.Tooling`, their tests, `scripts/lane-check.sh`, and the text-only proof with its wrapper.

I read thoenissen/Reihitsu#1906 and its one comment, the maintainer's triage decisions. The five rules in `00-lead.md` summarize that comment correctly. All five are written into the files the comment names, and the change doc states them with `Affected requirements: none — process change`.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Tooling.Test/Unit/LaneCheckCommandTests.cs:29` (constant `WordingChangesWithChangeDoc`) | FC-032 (the roles ran only the tooling project; nobody ran the architecture project on this tree) | – | PR-introduced | The fixture line `"A\tsquad/notes/2026-11-21-rh5201-member-access-wording.md\n"` names a squad note by its repository path. `SquadNoteReferenceTests.RepositoryFilesReferenceNoSquadNote` scans `.cs` files too and fails on it. I ran `scripts/test.sh --project architecture --filter "FullyQualifiedName~SquadNoteReferenceTests"` and got `Failed RepositoryFilesReferenceNoSquadNote`, finding `src/Reihitsu.Tooling.Test/Unit/LaneCheckCommandTests.cs:29: notes/2026-11-21-rh5201-member-access-wording.md`, with 1 failed and 7 passed. Class members: every line the diff adds outside `sdd/work/` and `squad/notes/` that names a note. That scan covers the whole repository and finds only this one. The change doc, the role files, `scripts/README.md`, and the other test file hold. | Write the fixture's text path so the guard does not match it, for example `squad/notes/README.md`, which the guard allows. The line is only classified as `Text`, so no assertion depends on the name. |
| 2 | medium | `squad/lead.md` §4, row "Spec author: needs a test (wording lane)"; change doc, *No test path in the wording lane*, sentence "A diff with a test path needs a wider lane: `tests` without a change doc, `behavior-change` with one" | FC-015 (one rule stated in two places, drifted from `scripts/README.md`, *The lane check*) | – | PR-introduced | The row tells the lead to widen to `tests` when the run has no change doc. That is wrong when the wording diff also edits a C# comment outside the test projects, which `spec-author.md` *Wording* explicitly includes ("a comment that quotes the title"), as the RH5201 run did. A changed test literal fails the text-only proof, so the comment edit is classified by its path as production, and rule 2 then requires `bug-fix`. Counterexample, run through the shim with `--text-only fail` (what the proof returns once a test literal changes) on `M AnalyzerResources.resx`, `M documentation/rules/RH5201.md`, `M src/Reihitsu.Analyzer/Rules/Layout/RH5201MethodChainsShouldBeAlignedAnalyzer.cs` (comment edit), `M src/Reihitsu.Analyzer.Test/Layout/RH5201Tests.cs`: `LANE CHECK: mismatch — claimed tests, diff needs bug-fix`. The lead learns this only at §8 step 2, after the validator, and must then add the bug-fix roles (implementer, auditor). Class over change doc × C# comment edit outside test projects: without change doc and without comment edit → `tests`, holds; with change doc, with or without comment edit → `behavior-change`, holds; without change doc and with comment edit → `bug-fix`, fails. | State the widening target by the lane-check rules instead of restating two of their outcomes. A changed test fails the proof, so every C# comment edit outside the test projects then counts as production: `bug-fix` without a change doc and `behavior-change` with one. The `tests` lane applies only when no such edit exists. Make the same correction to the change-doc sentence. |
| 3 | low | `squad/roles/spec-author.md`, *Wording* bullet, "Quote the `TEXT-ONLY PROOF` line for a C# comment edit as in the docs lane" | FC-037 (adjacent: which command is the proof) | – | PR-introduced | The docs-lane bullet points to `scripts/verify-text-only.sh --head worktree` and says to stop when the proof fails. In the wording lane that command, without `--exclude`, always fails on the resource file: `BEHAVIORAL src/Reihitsu.Analyzer/AnalyzerResources.resx`, as 05-implementer demonstration 2 shows. A spec author who follows "as in the docs lane" literally stops and reports a widening that is not needed. The line that proves the comment edit is the one `scripts/lane-check.sh wording --head worktree` prints, which carries the exclusions. | Name the command: quote the `TEXT-ONLY PROOF` line that `scripts/lane-check.sh wording --head worktree` prints. |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| A path is wording | `LaneClassifier._wordingFiles`: the two exact paths, `OrdinalIgnoreCase`, checked first in `ClassifySourcePath`, after the change-doc, text, and tooling prefixes, which none of the two paths match | `LaneCheckCommand --wording-paths` calls the same `ClassifyPath(path, false)`. The `isTextOnlyProven` flag cannot change the answer, because the wording check runs before the `.cs` check | Same predicate, one owner | ok |
| Leave a changed file out of the proof | `verify-text-only.cs`: `excludedPaths.Contains(Path)` (exact, `OrdinalIgnoreCase`), and for status `R` also `Contains(OldPath)`, over the proof's own `--find-renames` diff | The lane check's `--no-renames` diff, which supplies the exclusion list (wording paths only) | A rename between a wording path and another file arrives as D+A in the lane check, so only the wording side is excluded. The proof keeps the `R` entry, which is behavioral, so the proof fails safely. A copy (`C`) is excluded on its new path alone, which equals excluding an added file, since the copy source is unchanged | ok |
| Narrowest lane | `TryGetRequiredLane`, rules in order: unknown → production → tooling (with wording: change doc ? behavior-change : none) → wording (test ? (change doc ? behavior-change : tests) : wording) → change doc / test / docs | `scripts/README.md`, *The lane check*, rules 1–5; the change doc's *What* | Matches rule by rule, including that test paths and wording next to production change nothing | ok (see finding 2 for a restatement elsewhere) |
| Claim covers requirement | `Covers`: `required == Wording && claimed is (Tests or BugFix)`; `required == Tests && claimed is not (Docs or Wording)` | README widening orders docs → wording → tests → bug fix → behavior change and docs → tests → tooling → behavior change; "tooling does not cover wording, wording covers only docs" | I checked all 6 × 6 pairs against both orders, and they agree. A `behavior-change` claim still needs a change doc (`Decide`); a `wording` claim has no extra condition | ok |
| `--wording-paths` mode exclusivity | `TryParseArguments`: rejects `--lane` or `--text-only` next to it; an unknown argument is rejected by the existing default case; `--help` wins | The usage text and the `scripts/README.md` statement "takes no other argument" | Agree | ok |
| Validator runs the line-count rows | `validator.md`: `git diff --numstat origin/main...HEAD`, a non-zero added count on a C# file of a test project | The lane-check *Test* category (`src/*.Test/**`, `src/Reihitsu.ArchitectureTests/**`) | Same set, stated in prose | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Which paths are wording | `rg -l "AnalyzerResources\.resx\|CodeFixResources\.resx"`: `LaneClassifier._wordingFiles` (code), the `scripts/README.md` table (the declared one statement), the change doc (record), and the two test files. `scripts/lane-check.sh` holds no list | one owner, ok |
| Lane rules and widening | `LaneClassifier.TryGetRequiredLane`/`Covers`; `scripts/README.md` *The lane check* (one statement); `squad/README.md` *Lanes* "In short" (a summary that points there); `squad/lead.md` §4 row (a restatement of the widening outcome) | drift in the lead.md row, finding 2 |
| Line-count filter | `scripts/README.md` (two lines: the example list and *The line-count rows*); the change doc quotes it as a record; `validator.md` points to the README | ok |
| Push points | `squad/lead.md` *Pushes*; `squad/README.md` points to it; change doc | ok |
| CI-only subscription | `squad/lead.md` §9; `CLAUDE.md` restates it in one sentence and points to §9, consistently; `rg -i unsubscrib` finds nothing else | ok |
| Coverage first | `squad/README.md` *Coverage first*; `skeptic.md` and `test-implementer.md` point to it | ok |
| Lane names in usage and errors | `scripts/lane-check.sh:4,67`, `LaneCheckCommand.PrintUsage` and its `--lane` error, `LaneClassifier.TryParseLane`/`GetName`, `sdd/templates/follow-up-issue.md:19` | all list `wording`; line 104 (variant *Internal*) intentionally does not (03-spec-author) |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `LaneClassifierTests.ClassifyPathAssignsTheCategoryOfEachKindOfPath` (new resx rows), `ClassifyPathTreatsCSharpAsTextOnlyWhenProven` (resx rows), `ClassifyPathTreatsTheDiagnosticAndCodeFixResourcesAsWording` | Only the two exact paths, in any case and with or without the proof, are wording. Look-alike resx files keep their folder's category | A prefix, suffix, or `*.resx` match would turn a look-alike row into `Wording` | direct call, `AreEqual` | ok |
| `TryGetRequiredLaneReturnsTheNarrowestAdmittingLane` (new rows), `…FindsNoLaneForWordingAndToolingWithoutChangeDoc`, `…ProductionWordingAndTooling…`, `…UnknownPathNextToWording` | Each of the five rules on both sides of its change-doc and test conditions | A reordered rule changes a row's lane | direct call, `AreEqual`; the reason is checked by the words the README states | ok |
| `CoversFollowsTheWideningOrders` (11 rows), `LaneNamesRoundTrip`, `WordingLaneIsNamedWording` | Wording's place in both orders; `wording` spelling | A wrong `Covers` edge or name | direct call | ok |
| `LaneCheckCommandTests.Execute*Wording*` verdict tests (10 methods) | Verdict lines and exit codes for wording claims, wider claims, narrower claims, behavior-change without a change doc, and no lane next to tooling | Wrong exit code or verdict text | `Run`, verdict line by `Contains` of the full line | ok. `ExecuteRejectsAWordingClaimForAWordingChangeWithAChangedTest` reaches `behavior-change` through rule 2: under `--text-only fail` the analyzer `.cs` is production. It does not reach it through rule 4's test branch, which only the classifier row `Wording, Test, ChangeDoc → BehaviorChange` pins. The expected value is right |
| `ExecuteWritesOnlyTheWordingPathsOfADiff`, `…NoWordingPath…`, `…InItsOwnSpellingWithForwardSlashes`, `…RejectsTheWordingPathsQuery…` (×2), `…MalformedChangeLineInTheWordingPathsQuery` | The query writes exactly the wording paths, nothing else, and nothing on error | Extra lines, a verdict line, partial output on error | `Run`, stdout `AreEqual` in full | ok |
| `--exclude` of the proof and the `lane-check.sh` hand-off | Exact, case-insensitive exclusion; the suffix only when n > 0; renames need both paths | — | none (no harness, decision 19) | demonstrated in 05-implementer rows 1–10. I traced the code against each row and it agrees |

## Failure classes dismissed
- FC-004 to FC-014, FC-018 to FC-030, FC-033, FC-034, FC-036, FC-038 to FC-043: formatter phases, analyzer rules, code fixes, the precheck, and reproduction harnesses. The diff touches none of them; it changes a path classifier over name-status lines, a proof option, and process text.
- FC-001/FC-002: no qualifier of the rules is lost. *Guard scope* shows each predicate and its counterpart agree.
- FC-003: each new condition (change doc, test path, tooling, production next to wording; claim edges) is tested on both sides (04 table, checked above).
- FC-016: the sibling of the two resource files, any other `.resx`, is deliberately production and pinned by six rows.
- FC-017: the verdict lines and the query output are asserted in full; see *Assertion adequacy*.
- FC-031: the summaries of `Execute`, `ClassifyPath`, `TryGetRequiredLane`, `Covers`, `ClassifySourcePath`, the enum members, the `lane-check.sh` header, and the proof header describe the new behavior.
- FC-035: the tests were written against undocumented enum stubs, before the behavior existed (04). The query tests were mutation-checked (06).
- FC-044: nothing computed elsewhere is modelled.
- FC-032: applies, finding 1.
- FC-037: the lane check uses the proof, not a grep. Finding 3 is about which proof invocation the spec author quotes.

Also checked:
- No `TODO`, and no tracker number in the changed source.
- No visible output: no rule page or test of a visible surface changed, so check 10 has nothing to cover.
- No special case: check 11 is not applicable.
- Scope: every changed path serves one of the five rules or a skeptic finding.

## Architecture findings
Finding 2 is a rule stated in more than one place. `scripts/README.md`, *The lane check*, declares itself the one statement of the lane rules. `squad/lead.md` §4 restates the outcome of rules 2 and 4 for a widened wording run as "`tests` without a change doc, `behavior-change` with one", and the change doc repeats it. The restatement omits rule 2, which decides first once the proof fails, as it does for a C# comment edit outside the test projects. The input is the counterexample in finding 2.

## Hints
- `squad/roles/spec-author.md` (*Every lane …*) and the change doc say that a new rule's entries "with their wrapper properties" stay with the implementer. Only code-fix entries have wrapper properties: `AnalyzerResourcesHoldsNoPropertyPerRule` pins that analyzer texts have none. `implementer.md` step 5 already used the same loose wording before this run.
- `lane-check.sh` turns every stdout line of the `--wording-paths` call into an `--exclude`. A stray line from `dotnet run` would become an exclusion that names no changed file, and per the README that excludes nothing. Harmless.

## Follow-up candidates
_None._ The two in `00-lead.md` stand.
