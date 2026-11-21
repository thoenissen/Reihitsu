Role: implementer
Commit: a125a33 (production); this handover follows in its own commit
Verdict: tests pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore. Tooling change, so no requirement IDs; the behavior is the one `scripts/README.md`, *The lane check* and *The text-only proof*, states.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Tooling/Enumerations/Lane.cs` | `Lane.Wording` (after `Docs`) | README, *The lane check* |
| `src/Reihitsu.Tooling/Enumerations/PathCategory.cs` | `PathCategory.Wording` (after `ChangeDoc`) | README, table row *Wording* |
| `src/Reihitsu.Tooling/LaneClassifier.cs` | `_wordingFiles` (the two resource paths, exact, case-insensitive) checked first in `ClassifySourcePath`; `TryGetRequiredLane` follows the five ordered rules (tooling + wording: behavior change with a change doc, no lane without; wording: tests lane next to test paths, behavior change with a change doc, else wording); `Covers` adds the wording edges; `TryParseLane`/`GetName` know `wording`; summaries updated | README, rules 1–5 and the widening orders |
| `src/Reihitsu.Tooling/LaneCheckCommand.cs` | New mode `--wording-paths` (no other argument): reads the same name-status lines and writes each path `ClassifyPath` counts as wording, one per line; usage and the `--lane` error list `wording` | README, "asks the classifier … holds no path list of its own" (decision 14) |
| `scripts/lane-check.sh` | Pipes the diff into `lane-check.cs --wording-paths`, passes one `--exclude` per returned path to `verify-text-only.sh`; header comment and the missing-lane message list `wording` | README, *The lane check*, first two paragraphs |
| `scripts/proof/verify-text-only.cs` | `--exclude <path>`, repeatable, into an `OrdinalIgnoreCase` set; matching entries are removed from the changed files before any verdict; every `TEXT-ONLY PROOF:` line (pass, fail, no changed files) ends with `; excluded: <n> path(s)` when n > 0; usage and header comment | README, *The text-only proof*, `--exclude` paragraph |
| `scripts/verify-text-only.sh` | Header usage names `--exclude` and `--no-install` (arguments already pass through) | same |

One choice the README does not spell out: the proof diffs with `--find-renames`, so a rename is one entry. It is excluded only when both its old and new path are excluded; a rename onto a wording path from another file still counts the other file. Lane-check classifies with `--no-renames`, so both paths of such a rename reach the classifier anyway.

## Policy owners
- Which paths are wording: `rg -n "AnalyzerResources\.resx|CodeFixResources\.resx" scripts src --glob '!*Test*/**'` → only `LaneClassifier._wordingFiles` and the README table. `lane-check.sh` holds no list; it asks the command.
- Lane names in usage/errors: `rg -n "bug-fix, behavior-change" scripts src .claude squad` → `scripts/lane-check.sh:67`, `LaneCheckCommand.cs:254`, both updated.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 4 files changed, 114 insertions(+), 16 deletions(-). Scripts (outside `src`): 3 files, 47 insertions, 12 deletions. No new type; two enum members.

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --configuration Debug --project tooling` → `Passed!  - Failed: 0, Passed: 195, Skipped: 0, Total: 195`
- `scripts/build.sh --configuration Debug` → no warning or error in `Reihitsu.Tooling`.

Demonstrations for the parts no test covers (04, *Not covered*), run on commit a125a33 with two temporary uncommitted edits, reverted afterwards: the `RH5201MessageFormat` value in `src/Reihitsu.Analyzer/AnalyzerResources.resx` and a `///` line in `src/Reihitsu.Analyzer/Rules/Layout/RH5201MethodChainsShouldBeAlignedAnalyzer.cs`.

| # | Command | Result |
|---|---|---|
| 1 | `scripts/verify-text-only.sh --base HEAD --head worktree --exclude src/Reihitsu.Analyzer/AnalyzerResources.resx` | exit 0, `TEXT-ONLY PROOF: PASS — … 1 changed file(s) (0 non-compiled, 1 C# comment/layout-only); xml-doc impact: 1 file(s); excluded: 1 path(s)` |
| 2 | same without `--exclude` | exit 1, `BEHAVIORAL src/Reihitsu.Analyzer/AnalyzerResources.resx`, `TEXT-ONLY PROOF: FAIL — … 1 of 2 changed file(s) carry compiled behavior; xml-doc impact: 1 file(s)` (no suffix) |
| 3 | `--exclude SRC/reihitsu.analyzer/ANALYZERRESOURCES.resx` | exit 0, same PASS line as 1, with `; excluded: 1 path(s)` |
| 4 | `--exclude src/Reihitsu.Analyzer.CodeFixes/CodeFixResources.resx` (not in the diff) | exit 1, FAIL line as in 2, no suffix |
| 5 | `scripts/lane-check.sh wording --base HEAD --head worktree` | exit 0: the PASS line of 1, `Wording    src/Reihitsu.Analyzer/AnalyzerResources.resx`, `Text       src/…/RH5201MethodChainsShouldBeAlignedAnalyzer.cs`, `LANE CHECK: confirmed — claimed wording, diff needs wording` |
| 6 | same with `docs`, then `tooling` | exit 1 each, `LANE CHECK: mismatch — claimed docs, diff needs wording` / `… claimed tooling, diff needs wording` |
| 7 | resx edit reverted, comment edit kept, `lane-check.sh docs …` | exit 0, PASS line without suffix (empty exclude list), `LANE CHECK: confirmed — claimed docs, diff needs docs` |
| 8 | `printf` of four name-status lines (both resx, `Cli/CliResources.resx`, a rule page) into `dotnet run scripts/lane-check/lane-check.cs --verbosity quiet -- --wording-paths` | exit 0, prints exactly the two wording paths; with only the rule page: exit 0, no output |
| 9 | `--wording-paths --lane docs` | exit 2, `lane-check: --wording-paths takes no other argument.` plus usage |
| 10 | `scripts/lane-check.sh` with no lane; `scripts/lane-check.sh hotfix …` | exit 2, `expected a lane (docs, wording, tests, bug-fix, behavior-change, or tooling).`; exit 2, `--lane expects docs, wording, …` and the usage naming `wording` and `--wording-paths` |

## Disputed tests or scope
- The query mode `--wording-paths` of `LaneCheckCommand` is a new argument of the command, not a new public member; `scripts/README.md` leaves the interface open (decision 14) and does not name it. No test covers it; 04 offers a test in a repair round with the lead's approval. Whether the README should name the flag is the spec author's call.
- `lane-check.sh` now runs the file-based shim twice (query, then verdict); the second run reuses the build, so it costs a launch, not a build.

## Architecture findings
_None._

## Follow-up candidates
_None._
