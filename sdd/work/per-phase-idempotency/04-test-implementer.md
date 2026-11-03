Role: test implementer
Commit: ffc661d (tests); this line amended in the follow-up commit
Verdict: tests written

My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the decisions recorded in `00-lead.md`.

Specs and tests for existing behavior: every test passes on the unchanged formatter (`scripts/test.sh --project formatter`: 4368 passed, 0 failed; `--project architecture`: 92 passed). `scripts/lane-check.sh tests --head worktree`: confirmed.

## Tests

All in `src/Reihitsu.Formatter.Test/Unit/Pipeline/PhaseStabilityCheckTests.cs`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `CheckFindsNoChangeAboveCommentLedUsingDirectiveOfTheSameGroup` (source changed to FMT-PIPE-027 example 1 verbatim, `class C;` included) | FMT-USING-010, FMT-PIPE-027 | `AssertNoPhaseChanges` (LF, CRLF; `FormatSyntaxTree` leg, then every phase alone) | pass |
| `CheckFindsNoChangeInArgumentAlignedUnderTheFirstArgument` (new; example 2) | FMT-PIPE-027 | `AssertNoPhaseChanges` | pass |
| `CheckFindsNoChangeInTheOutputOfAnInputTheFormatterFormats` (new; first qualifier's formatted case `class C{void M(){int x=1;}}`, plus a variant with an LF and a CRLF line break in the input so the detected line ending is exercised both ways) | FMT-PIPE-027 | `AssertNoPhaseChangesTheOutputOf` (formats through `FormatSyntaxTree`, asserts the output differs from the input, runs every phase alone with the tree's parse options and the line ending `DetectEndOfLine` finds in the output) | pass |
| `CheckFindsNoChangeInFormattedSource` (tag added) | FMT-PIPE-027 | `AssertNoPhaseChanges` | pass |
| `CheckRunsTheProductionPhasesInPipelineOrder` (tag added) | FMT-PIPE-027 | `ObservedPipeline` vs `CreateProductionPhases` | pass |
| `CheckFindsThePhaseThatChangesTheSource` (fingerprint assertion dropped) | — (helper) | `FindChangingPhases` | pass |
| `CheckParsesTheTextWithTheFixtureParseOptions`, `CheckCatchesAPhaseWhoseChangeALaterPhaseWritesBack`, `CheckFindsNoChangeAboveDelimitedDocumentationLedUsingDirectiveOfTheSameGroup` (FMT-USING-010 tag unchanged), `CheckFindsNoChangeAboveCommentLedUsingDirectiveOfAnotherGroup` | unchanged | unchanged | pass |
| `AssertRuleResultWithoutExpectationRunsTheCheck`, `AssertRuleResultWithExpectationRunsTheCheck`, `AssertSecondPassIsStableRunsTheCheck`, `HelpersRunTheCheckWithTheFixtureParseOptions` (checked fixtures now recorded as output text + end-of-line sequence instead of fingerprint + enum) | — (helper) | `GetCheckedFixtures` | pass |
| `AssertRuleResultWithoutExpectationFailsOnAPhaseThatChangesTheOutput`, `AssertRuleResultWithExpectationFailsOnAPhaseThatChangesTheOutput`, `AssertSecondPassIsStableFailsOnAPhaseThatChangesTheOutput` (renamed from `…FailsOnAnUnlistedChange`; assert phase name, `LF`, and the first changed line in the message) | — (helper) | `OverrideForCurrentTest(CreateTextChangingPhases)` | pass |

Removed tests (Decision #7): `FingerprintIgnoresLineEndingsAndTellsFixturesApart`, `UnlistedChangeFailsNamingFixturePhaseOutputAndLineEnding`, `ListedChangePasses`, the eight `Entry…` recorder tests, `HelpersPassOnAListedChange`, `HelperFailsOnAnEntryWhosePhaseNoLongerChangesTheFixture`, both `TestCleanupFails…` tests, `EveryCauseNamesAProductionPhase`, the three `…DataRowEntryCoversOnlyItsOwnRow` tests, `TestKeyAppendsTheDataRowInParentheses`, `RuntimeTestNameMatchesTheEntryTestName`, `EveryEntryNamesATestMethodItsDataRowAndAKnownCause`, and all of `DataRowKeyTests`.

## Helper changes (Decisions #6, #7)

- Deleted: `PhaseReversalExceptionList`, `PhaseReversalCauses`, `PhaseReversalCause`, `PhaseReversalEntry`, `Enumerations/PhaseReversalLineEndings` (the folder is now empty), `PhaseStabilityRecorder`, `DataRowKey`.
- `PhaseStabilityCheck`: removed `ComputeFingerprint`, `FormatTestName` (both overloads), `FormatTestKey`, unreached-entry reporting; `BeginTest()` takes no test identity; `EndTest()` only clears the session; `OverrideForCurrentTest` takes only the phase factory; `GetCheckedFixtures` returns `(FinalOutput, EndOfLine, ParseOptions)`. The class summary names FMT-PIPE-027 and states that the helpers also check outputs of inputs with syntax errors, which the requirement does not cover (Decision #6).
- `PhaseStabilitySession`: now holds only the phase factory and the checked fixtures, and runs the check itself (the recorder's role without entries). Failure message: `<phase> changes the final output under <LF|CRLF> line endings when it runs alone (FMT-PIPE-027). <first difference>`.
- `PhaseChange`: `OutputFingerprint` removed.
- `FormatterTestsBase`: `[TestInitialize]`/`[TestCleanup]` kept (the session still carries the per-test override and the checked-fixture record the helper tests read); their summaries, the `TestContext` summary, and the three `<see cref="PhaseReversalExceptionList"/>` mentions updated. `TestContext` stays: many derived classes use `TestContext.CancellationToken`.
- `SecondPassInput` summary unchanged: it still describes the check correctly.
- The "is formatter output" leg of the tagged example tests now goes through `ReihitsuFormatter.FormatSyntaxTree` (Decision #5); this also applies to the other `AssertNoPhaseChanges` users in the class (same results).

## Watched failing

- Mutating example 2 (`2);` moved two columns left) makes `CheckFindsNoChangeInArgumentAlignedUnderTheFirstArgument` fail at the "is formatter output" assertion, as intended; restored.
- The three `…FailsOnAPhaseThatChangesTheOutput` tests prove the check fails a helper when a phase run alone changes the output; `CheckCatchesAPhaseWhoseChangeALaterPhaseWritesBack` proves a write-back pair is caught.

## Not covered, and why

- FMT-PIPE-027 node-level exclusion and the "Text, not tree" qualifier: no test asserts behavior the requirement leaves out; the check compares text (`FindChangingPhases`, `output != text`), which is what the qualifier states.
- Syntax-error qualifier (`class C{void M(){int x=1}}`): the requirement says nothing about it, so no assertion; the stricter helper behavior is documented in `PhaseStabilityCheck`'s summary.
- FC-010/FC-012: covered by the check running every production phase under LF and CRLF on every `FormatterTestsBase` fixture (4000+ executions, green).
- Seven idempotency classes outside `FormatterTestsBase` and the self-hosting scans: follow-up candidates in `00-lead.md` (Decision #8), not part of this run.
- Comment/directive-in-gap, code-fix, Fix All, naming: no rule, code fix, or token gap changes.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.Pipeline.PhaseStabilityCheckTests"
scripts/test.sh --project formatter
```
