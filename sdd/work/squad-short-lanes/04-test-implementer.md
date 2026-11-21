Role: test implementer
Commit: 6b5b185 (base); my commit follows
Verdict: tests written

Tooling change, no requirement: no test carries a `[Requirement]` tag. Expected values come from `scripts/README.md`, *The lane check* and *The text-only proof*, and the change doc `sdd/changes/2026-11/squad-wording-lane-and-ci-only-watch.md`. My prompt carried no intended fix or verdict to ignore.

**The test project does not compile until the implementer adds `Lane.Wording` and `PathCategory.Wording`.** I assumed those member names because `scripts/README.md` names the category *Wording* and the lane `wording`, and the command prints `{category,-10}`. To watch the tests fail, I added the two enum members locally as undocumented stubs, without changing any behavior. I ran the tests and then reverted the stub; it is not committed. Result on the stub: 32 failed, 113 passed, 145 total. Every failure was an assertion, with no exception.

## Tests

All tests are in `src/Reihitsu.Tooling.Test/Unit/`. Rows added to an existing data-driven method are listed under that method. Sonar S4144 forbids a second method with an identical body, so the rows go into the existing methods.

| Test | Requirement / failure class | Helper | Expected now (stub, no behavior) |
|---|---|---|---|
| `LaneClassifierTests.ClassifyPathAssignsTheCategoryOfEachKindOfPath`: existing row `AnalyzerResources.resx` changed from `Production` to `Wording` | — (tool) | direct call | fails |
| same method, new rows: other `.resx` → `Production` (`Cli/CliResources.resx`, `Formatter/FormatterResources.resx`, `Analyzer/Rules/AnalyzerResources.resx`, `Analyzer/CodeFixResources.resx`, `Analyzer.CodeFixes/AnalyzerResources.resx`, `Analyzer/AnalyzerResources.de.resx`); `.resx` in a test project → `Test`; `.resx` in `Reihitsu.Tooling` → `Tooling` | — | direct call | passes (guards exact-path matching) |
| `ClassifyPathTreatsCSharpAsTextOnlyWhenProven`: existing row `AnalyzerResources.resx` changed to `Wording`; new row `Cli/CliResources.resx` → `Production` | — | direct call | first fails, second passes |
| `ClassifyPathTreatsTheDiagnosticAndCodeFixResourcesAsWording` (new): both paths × proven/unproven, plus two mixed-case spellings | — | direct call | fails (6 rows) |
| `TryGetRequiredLaneReturnsTheNarrowestAdmittingLane`, new rows: wording, +text, +change doc, +text+change doc → `Wording`; wording+test (±text) → `Tests`; wording+test+change doc → `BehaviorChange`; wording+tooling+change doc (±test) → `BehaviorChange`; wording+production (±test) → `BugFix`; wording+production+change doc (±tooling) → `BehaviorChange`; tooling+text+change doc → `Tooling` | — | direct call | 6 fail (the `Wording` results and wording+tooling+change doc); the rest pass as ordering guards |
| `TryGetRequiredLaneFindsNoLaneForWordingAndToolingWithoutChangeDoc` (new): wording+tooling (±text, ±test); reason contains `wording`, `tooling`, `without a change doc` | — | direct call | fails |
| `TryGetRequiredLaneFindsNoLaneForProductionWordingAndToolingWithoutChangeDoc` (new) | — | direct call | passes (rule 2 decides first) |
| `TryGetRequiredLaneFindsNoLaneForAnUnknownPathNextToWording` (new) | — | direct call | passes (rule 1 decides first) |
| `CoversFollowsTheWideningOrders`, 11 new rows: `wording` required is covered by wording, tests, bug-fix, behavior-change, not by docs or tooling; a `wording` claim covers docs and wording only | — | direct call | 3 fail (tests and bug-fix covering wording; wording not covering tests) |
| `LaneNamesRoundTrip`: new row `Wording` | — | direct call | fails |
| `WordingLaneIsNamedWording` (new): `TryParseLane("wording")`, `GetName(Lane.Wording) == "wording"` | — | direct call | fails |
| `LaneCheckCommandTests.ExecuteConfirmsTheWordingLaneForTheResourceFilesAndText` (new): both resx files, rule page, package README; checks the `Wording    <path>` lines | — | `Run` | fails |
| `ExecuteConfirmsAWordingClaimWithoutChangeDoc` (new) | — | `Run` | fails |
| `ExecuteConfirmsTheWordingLaneForAWordingChangeWithChangeDocAndProvenCommentEdit` (new): the RH5201 diff (3caebda) without its test file, proof `pass` → `wording` | — | `Run` | fails |
| `ExecuteRejectsAWordingClaimForAWordingChangeWithAChangedTest` (new): the same diff with its test file, proof `fail` → mismatch, needs `behavior-change` | — | `Run` | fails |
| `ExecuteRejectsAWordingClaimForWordingWithATestPath` (new): wording + test without change doc → mismatch, needs `tests` | — | `Run` | fails |
| `ExecuteRejectsAClaimThatDoesNotCoverTheWordingLane` (new, `docs`, `tooling`) → mismatch, needs `wording` | — | `Run` | fails |
| `ExecuteConfirmsAWiderClaimForAWordingDiffWithChangeDoc` (new, `tests`, `bug-fix`) → confirmed, "needs only wording" | — | `Run` | fails |
| `ExecuteRejectsABehaviorChangeClaimForAWordingDiffWithoutChangeDoc` (new) → "no change doc …; it needs wording" | — | `Run` | fails |
| `ExecuteReportsNoLaneForWordingAndToolingWithoutChangeDoc` (new) → exit `NoLane` | — | `Run` | fails |
| `ExecuteConfirmsTheBehaviorChangeLaneForWordingAndToolingWithChangeDoc` (new) | — | `Run` | fails |

Each of the five ordered rules and each changed `Covers` edge is tested on both sides. The wording/tooling condition is tested with and without a change doc. The wording/test condition is tested with and without a change doc. Production next to wording is tested with and without a change doc. A `wording` claim is tested with and without a change doc, beside the `behavior-change` claim's change-doc condition, which stays.

## Coverage first

- Existing tests that assert a changed output: `LaneClassifierTests.cs`, the two `AnalyzerResources.resx → Production` rows. Both were updated, not duplicated. An `rg` over `src/**/*.cs` outside `Reihitsu.Tooling*` for `AnalyzerResources.resx`, lane names, and `Lane.` finds no other test.
- Values already pinned get no new row. Examples: change doc + text → `BehaviorChange`, production + tooling without change doc → no lane, and unknown → no lane, all in the existing rows and methods. Also tooling + change doc → `Tooling`, the "without wording" side of rule 3.
- `ExecuteRejectsInvalidArguments` already pins an unknown lane name (`hotfix`) as a tool error. `wording` being accepted is pinned by the new `Execute…` tests.

## Not covered, and why

- **`verify-text-only --exclude` and its `; excluded: <n> path(s)` suffix.** The proof is a file-based app outside `Reihitsu.sln` (`scripts/proof/verify-text-only.cs`). No test project covers it, so `rg -l "verify-text-only" src` finds only a Roslyn-version guard. Building a harness for it would be new test infrastructure. The implementer demonstrates it by running it, as the skeptic noted. Cases to show:
  1. A diff of a resx file plus a C# comment edit, with `--exclude` on the resx: `PASS`, with suffix `excluded: 1 path(s)`.
  2. The same without `--exclude`: not proven.
  3. `--exclude` in a different letter case: still excluded.
  4. `--exclude` naming a path not in the diff: no suffix, verdict unchanged.
- **`lane-check.sh` getting the wording paths from the classifier.** `scripts/README.md` and decision 14 leave the interface open, so a test against `LaneCheckCommand` would pick the API for the implementer. The classifier's `ClassifyPath` → `Wording` rows pin the source of truth. If the implementer adds a query mode to `LaneCheckCommand`, it can be covered in a repair round, with the lead's approval. The demonstration: a diff of a resx file plus a comment edit gives `wording` through `scripts/lane-check.sh wording --head worktree`.
- **Usage strings listing `wording`.** These are help text only. The existing invalid-argument test checks only `usage: lane-check`.
- **Failure classes.** Formatter, analyzer, code-fix, line-ending, and unsupported-shape classes (FC-033, FC-038 to FC-043, and others) cannot reach a path classifier over name-status lines. FC-003 (both sides of each condition) is applied above. FC-017 (weak assertions): the verdict lines are asserted in full, and the reason strings only by the words the README states.

## Commands

```shell
scripts/test.sh --project tooling --filter "FullyQualifiedName~Reihitsu.Tooling.Test.Unit.LaneClassifierTests|FullyQualifiedName~Reihitsu.Tooling.Test.Unit.LaneCheckCommandTests"
```

## Follow-up candidates

_None._
