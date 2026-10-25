Role: test implementer
Commit: ed3aa9d
Verdict: tests written

Repair round 1, second worklist item: escalation 10 (`00-lead.md`, *Escalations* and *Repair round 1*; `squad/decisions/2026-10-25-lane-check-contributing-md-unclassified.md`, answer B). The maintainer decided that the lane check classifies the repository's root `CONTRIBUTING.md` as text. The prompt carried no intended fix, suspected cause, or hoped-for verdict. No production code under `src/Reihitsu.Tooling/` was touched.

## thoenissen/Reihitsu#1467

### Observed on the current head (before the tests)

`scripts/lane-check.sh tooling --no-install` on `f2db279` printed `Unknown    CONTRIBUTING.md` and ended with `LANE CHECK: no lane — claimed tooling, but the diff contains paths no lane rule classifies`. Every other path of the diff was classified.

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Reihitsu.Tooling.Test.Unit.LaneClassifierTests.ClassifyPathTreatsTheRootContributingGuideAsText(False)` | tool: maintainer decision, escalation 10 | `LaneClassifier.ClassifyPath` | fail: expected `Text`, actual `Unknown` |
| `Reihitsu.Tooling.Test.Unit.LaneClassifierTests.ClassifyPathTreatsTheRootContributingGuideAsText(True)` | tool: the text-only proof does not change it; FC-003 | `LaneClassifier.ClassifyPath` | fail: expected `Text`, actual `Unknown` |
| `Reihitsu.Tooling.Test.Unit.LaneClassifierTests.ClassifyPathAssignsTheCategoryOfEachKindOfPath("CLAUDE.md", Tooling)` (new data row) | tool: a neighboring root Markdown file stays tooling; FC-003 guard against widening the rule to every root `.md` | `LaneClassifier.ClassifyPath` | pass |
| `Reihitsu.Tooling.Test.Unit.LaneCheckCommandTests.ExecuteConfirmsTheDocsLaneForTheRootContributingGuide` | tool: a diff of only `CONTRIBUTING.md` lists it as `Text` and fits the docs lane | `LaneCheckCommand.Execute` via `Run` | fail: exit code expected `0`, actual `3` (no lane) |
| `Reihitsu.Tooling.Test.Unit.LaneCheckCommandTests.ExecuteConfirmsTheToolingLaneForTheRootContributingGuideNextToTooling` | tool: this run's shape (`AGENTS.md`, `CONTRIBUTING.md`, `scripts/test.sh`) confirms the tooling lane | `LaneCheckCommand.Execute` via `Run` | fail: exit code expected `0`, actual `3` (no lane) |

No test carries a `[Requirement]` tag: the lane check is a tool with no spec under `sdd/specs/`.

Run on `ed3aa9d`: the four new cases fail as listed, the other 71 tests of `LaneClassifierTests` and `LaneCheckCommandTests` pass (75 total), including the existing `AGENTS.md` → `Tooling` and `notes.txt` → `Unknown` rows.

### Not covered, and why

- **Other root Markdown files** (for example a hypothetical `CHANGELOG.md` or `notes.md`): the decision names `CONTRIBUTING.md` only. I pin neither "text" nor "unknown" for other root `.md` files, so the implementer may scope the change to the one file or to root Markdown generally. The only boundary pinned is that `AGENTS.md` and `CLAUDE.md` stay tooling; a general "root `*.md` is text" rule would have to keep them tooling, because the text check runs before the tooling check in `ClassifyPath`.
- **Case variants** (`contributing.md`): every list in `LaneClassifier` compares with `OrdinalIgnoreCase`; the decision says nothing about case, so I did not pin it.
- **`CONTRIBUTING.md` below a prefix** (`scripts/CONTRIBUTING.md`, `src/.../CONTRIBUTING.md`): already decided by the existing prefix and `src/` `.md` rules, unaffected by the decision.
- **Failure classes**: FC-003 is covered by both `isTextOnlyProven` arms and the `CLAUDE.md` guard; FC-017 by asserting the exact category, exit code, listing line, and verdict line; FC-035 by taking expected values from the decision and `squad/README.md` *Lanes* ("Markdown" is docs), not from a patched classifier. Formatter, code-fix, line-ending, and naming classes cannot reach a path classifier.

## Whole run

`scripts/format.sh` over both test files: 0 of 2 files changed.

## Commands

```shell
scripts/test.sh --project tooling --filter "FullyQualifiedName~ContributingGuide"
scripts/test.sh --project tooling --filter "FullyQualifiedName~Reihitsu.Tooling.Test.Unit.LaneClassifierTests|FullyQualifiedName~Reihitsu.Tooling.Test.Unit.LaneCheckCommandTests"
scripts/lane-check.sh tooling
```
