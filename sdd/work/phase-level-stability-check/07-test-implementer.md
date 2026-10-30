Role: test implementer
Commit: 7d0657b
Verdict: tests written

This completes repair round 1. The maintainer chose option A for finding 1, recorded in `squad/decisions/2026-10-30-phase-level-stability-check-test-identity.md`. Findings 3 to 9 stay as `06-test-implementer.md` describes them, and the commit 8577744 is unchanged. Two things change here:

- **Finding 1.** The check now identifies the running test through MSTest's `TestContext`, not through the call stack.
- **Finding 2.** It now has a full repair, which replaces the interim guard.

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## What changed

- **One `TestContext` for every formatter test.**
  - `FormatterTestsBase` declares `TestContext` once.
  - I deleted the duplicate `#region Properties` block, which held only `TestContext`, from the 14 derived classes. They now use the inherited property, with no other edit:
    - `Unit/`: `ReihitsuFormatterNodeColumnTests`, `ReihitsuFormatterEntryPointTests`, `ReihitsuFormatterNodeBlankLineTests`, `BlankLines/BlankLineRegionDirectivePhaseTests`, `Pipeline/PipelineInvariantTests`, `ReihitsuFormatterLanguageVersionTests`, `ReihitsuFormatterTests`, `StructuralTransforms/NodeLevelStructuralTransformTests`, `RawStringAlignment/RawStringAlignmentPipelineTests`;
    - `Regression/`: `Indentation/CommentLedFirstTokenAnchorTests`, `TabReplacement/TabReplacementTests`, `FullPipeline/ArgumentFormattingFullPipelineTests`, `FullPipeline/RawStringAlignmentFullPipelineTests`;
    - `Idempotency/IdempotencyTests`.
- **Each execution is keyed by class, method, and data row.**
  - `[TestInitialize]` calls `PhaseStabilityCheck.BeginTest(testName, dataRow)` with `TestContext.FullyQualifiedTestClassName`, `TestContext.TestName`, and `PhaseStabilityCheck.FormatDataRow(TestContext.TestData)`.
  - `FormatDataRow` writes each argument in invariant culture and quotes strings; a test without data gets `null`.
  - `FormatTestKey` builds the key as `<name>` for a test without data, or `<name>(<row>)` for a data row.
- **One session per execution.**
  - `PhaseStabilitySession` holds the key and exactly one `PhaseStabilityRecorder`.
  - `Verify`, `GetCheckedFixtures`, and `EndTest` use that recorder. `FindCallingTestName` and the whole stack walk are deleted.
  - `EndTest` marks the session as ended, so a test that calls it directly is not reported a second time by its cleanup.
- **Entries carry the data row.**
  - `PhaseReversalEntry` gains an optional `DataRow` and a `TestKey`, and the recorder filters entries by `TestKey`.
  - The ten real entries have no data row and stay unchanged.
- **A side effect: the outliving-entry gap is closed.** It was named in `02-test-implementer.md`. The session now knows its test before any helper call, so cleanup reports an entry of a test that never runs the check.
- **Finding 2's interim guard is replaced.** `EveryEntryNamesATestMethodThatIsNotDataDrivenAndAKnownCause` refused data-driven methods. Its replacement, `EveryEntryNamesATestMethodItsDataRowAndAKnownCause`, accepts them on three conditions:
  - an entry of a `DataRow` method names one of that method's rows;
  - an entry of a `DynamicData` method names some row;
  - an entry of any other method names no row.

  The duplicate-key check now uses `TestKey`.

## New tests (`Unit/Pipeline/PhaseStabilityCheckTests.cs`, now 38)

| Test | Covers | Expected now |
|---|---|---|
| `DataRowEntryCoversOnlyItsOwnRow (True)` / `(False)` | finding 2: an entry of row `True` covers its row's change (`TextChangingPhase`). Row `False` formats the same fixture with the production phases, and the entry neither covers it nor is reported stale. Both rows also assert their own formatted row. | pass |
| `DataRowIsFormattedInInvariantCultureWithQuotedStrings` | row and key format | pass |
| `TestCleanupFailsOnAnEntryOfATestThatNeverRunsTheCheck` | outliving entry: cleanup fails under LF and CRLF | pass |
| `EveryEntryNamesATestMethodItsDataRowAndAKnownCause` | list integrity with data rows (replaces the interim guard) | pass |

## Evidence

- **Finding 1, the run that failed before.**
  - `DOTNET_TieredCompilation=0 scripts/test.sh --project formatter` gives 3040/3040, the whole project.
  - On 8577744, the filter over `PhaseStabilityCheckTests` and `WrappedAngleBracketListTests` failed 30 of 61 with "found no test method on the call stack". That message no longer exists.
  - The default run also gives 3040/3040.
- **Finding 2, the data-row probe.**
  - `DataRowEntryCoversOnlyItsOwnRow` is the auditor's probe in permanent form: two rows, one entry for one row. Both rows pass.
  - Failing first: with `FormatTestKey` mutated to drop the row, `DataRowEntryCoversOnlyItsOwnRow (False)` failed with `Stale phase reversal entry … TextChangingPhase no longer changes …` and `… no fixture of the test has the final output …`. That is the auditor's observed failure. `DataRowIsFormattedInInvariantCultureWithQuotedStrings` failed as well. The mutation was then reverted.
- **Other checks.**
  - Architecture tests: 89/89.
  - `scripts/format.sh --force` over `Helpers/`, `Unit/Pipeline/`, and the 14 classes: 0 of 35 files changed. `--force` is needed because the script prompts above its file-count threshold.
  - `scripts/lane-check.sh tooling --head worktree`: confirmed.

## Not covered, and why

- **Inherited test methods.** The key now uses the executing class (`FullyQualifiedTestClassName`), so an entry for an inherited method names the derived class. No test method in the formatter suite is inherited today, so I added no test.
- **`DynamicData` rows.** The integrity test cannot enumerate them statically, so it only requires that such an entry names some row. No helper caller uses `DynamicData` today.
- **The `dotnet-coverage` CI configuration.** It is not available here. With the stack walk removed, the JIT has nothing left to affect.

## Architecture findings

_None._

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.Pipeline.PhaseStabilityCheckTests"
scripts/test.sh --project formatter
DOTNET_TieredCompilation=0 scripts/test.sh --project formatter
scripts/test.sh --project architecture
```
