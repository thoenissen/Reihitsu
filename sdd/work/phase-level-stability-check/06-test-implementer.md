Role: test implementer
Commit: 8577744
Verdict: blocked

This is repair round 1, working against `05-auditor.md` (commit 5dbd7f9).

- Findings 3 to 9 are repaired, and finding 2 has an interim guard.
- Finding 1, and with it the complete fix of finding 2, needs a choice the maintainer has not made. It is written out under **Open choice** and is not decided here.
- My prompt carried no intended fix, suspected cause, or hoped-for verdict. The auditor's suggested changes were treated as suggestions.

## Findings

| # | What I did | Proof |
|---|---|---|
| 1 | **Not repaired; open choice below.** Test identity still comes from the call stack. | Reproduced on 8577744: with `DOTNET_TieredCompilation=0`, the filter over `PhaseStabilityCheckTests` and `WrappedAngleBracketListTests` gives 30 failed, 31 passed, with "The phase-level stability check found no test method on the call stack." The default run is green. |
| 2 | **Interim guard.** `EveryEntryNamesATestMethodThatIsNotDataDrivenAndAKnownCause` refuses an entry whose method carries `DataRow`/`DynamicData`; its summary states why: each row is its own execution, so an entry of one row is stale in every other. No helper caller is data-driven today, so nothing is narrowed in practice. The complete fix, keying by the data row, depends on the choice for #1. | Failing first: I added a temporary `[DataRow(1)] ProbeDataRow` method with an entry for it. The test failed with "... names a data-driven test method." Both were then reverted. |
| 3 | **Repaired.** An entry now also carries `PhaseOutputFingerprint`, the LF-normalized fingerprint of the text the phase returns, and an entry covers a change only when phase **and** output match (`PhaseStabilityRecorder.IsCoveredBy`). A different or additional change by the same phase now reads as unlisted plus stale. The ten real entries carry their output fingerprints, read from the unlisted messages of a run with placeholder values. One fingerprint serves LF and CRLF: the suite is green under both. New test: `EntryForTheSamePhaseWithAnotherOutputDoesNotHideTheChange` (LF and CRLF). | Mutation: matching on the phase only fails exactly that test (1 of 34). |
| 4 | **Repaired.** `PhaseStabilityCheck.OverrideForCurrentTest(entries, createPhases)` is internal and replaces the entries and phases of the running test's session. `TextChangingPhase` (new, `Unit/Pipeline/`) changes every text, so the helpers can be driven into failure without a real reversal. New tests: `AssertRuleResultWithoutExpectationFailsOnAnUnlistedChange`, `AssertRuleResultWithExpectationFailsOnAnUnlistedChange`, `AssertSecondPassIsStableFailsOnAnUnlistedChange` (each asserts the phase, fixture fingerprint, test name, and `under LF`), `HelperFailsOnAnEntryWhosePhaseNoLongerChangesTheFixture`, `TestCleanupFailsOnAnEntryNoFixtureReached` (drives `EndTest` into its stale failure under LF and CRLF), and `HelpersPassOnAListedChange` (the other side: a listed change passes the helpers and `EndTest`). `EndTest` now clears the session's recorders, so a test that calls it directly is not reported twice by its cleanup. | Mutation: the auditor's own, both `Assert.Fail` calls in `Verify`/`EndTest` replaced by no-ops. Five of the new tests fail (34 run, 5 failed). Before this round, the same mutation passed 29/29. |
| 5 | **Repaired.** `SecondPassInput.Create(text, endOfLine, parseOptions)` (new, internal) is the one statement of "parse with the fixture's options, context from line ending and resolved language version". `FormatterTestsBase.ApplyRule`, `PhaseStabilityCheck.FindChangingPhases`, and the test-side fixed-point assertion (`AssertIsFinalOutput`, formerly `AssertSecondPassIsStableWithoutCheck`) all use it. `FindChangingPhases` now parses once per call instead of once per phase, as the auditor hinted. | The formatter suite is green (3036/3036), with parse options still reaching the check (`HelpersRunTheCheckWithTheFixtureParseOptions`, `CheckParsesTheTextWithTheFixtureParseOptions`). |
| 6 | **Repaired.** `FindChangingPhases` returns `PhaseChange` (phase, output, output fingerprint, first difference). The six `CheckCatches*` tests assert the phase's full output under LF and CRLF, besides the phase list. The stable cases use `AssertNoPhaseChanges`. | I wrote each expected phase output from the traced change in its name before the first run, and all six matched on that first run. |
| 7 | **Repaired by construction.** A cause is now a `PhaseReversalCause` object (phase plus description). An entry takes its phase from its cause (`PhaseReversalEntry.Phase => Cause.Phase`), so an entry cannot pair a cause with another phase. `EveryCauseNamesAProductionPhase` ties each cause's phase to the production list. | The mismatch the auditor describes can no longer be written. |
| 8 | **Repaired.** `FormatterTestsBase`: both public summaries and the private `AssertRuleResult` summary now state the phase-level check as something the method verifies, "apart from the reversals `PhaseReversalExceptionList` names". The cleanup summary names entries. | Text only, formatted. |
| 9 | **Repaired.** `PhaseReversalException` is renamed `PhaseReversalEntry`, and the list is renamed `PhaseReversalExceptionList`, so no type carries an `Exception` suffix. The unused `PhaseStabilityRecorder(Type, string, …)` constructor is removed; the recorder has a public production-phase constructor and an internal one that takes a phase factory. The name-only `PhaseReversalEntry` constructor documents that `TestClass` and `TestMethod` stay null and that the list does not accept such an entry. The integrity test asserts `TestClass != null` with a message before using it. Messages say "Stale phase reversal entry". | The architecture suite is green (89/89). |

New and changed files, all under `/home/user/Reihitsu/src/Reihitsu.Formatter.Test/`:
- `Helpers/`: `PhaseChange.cs`, `PhaseReversalCause.cs`, `PhaseReversalCauses.cs`, `PhaseReversalEntry.cs`, `PhaseReversalExceptionList.cs`, `PhaseStabilityCheck.cs`, `PhaseStabilityRecorder.cs`, `PhaseStabilitySession.cs` (moved out of the check, RH2101), `SecondPassInput.cs`, `FormatterTestsBase.cs`.
- `Unit/Pipeline/`: `PhaseStabilityCheckTests.cs` (34 tests), `TextChangingPhase.cs`.

## Open choice

**Context.** Issue thoenissen/Reihitsu#1483, repair round 1, auditor finding 1. The check finds the running test by walking the call stack for a `[TestMethod]` frame (`Helpers/PhaseStabilityCheck.cs`, `FindCallingTestName`). Under fully optimized JIT code, a test whose last statement calls a helper becomes a tail call and its frame disappears:

    public void X() { ...; AssertRuleResult(input, expected); }   // frame removed under DOTNET_TieredCompilation=0

Reproduced on 8577744: 30 of 61 tests fail with "found no test method on the call stack". The default run and the CI coverage run pass, because tier-0 code makes no tail calls. That is runtime behavior nothing guarantees. Taking the identity from MSTest instead would also let the key include the data row, which completes finding 2. The obvious source, a `TestContext` property on `FormatterTestsBase`, collides with the 14 derived classes that declare their own (MSTEST0036 fails the build). Ruled out: marking the 146 derived classes' test methods `[MethodImpl(NoOptimization)]`, which patches the symptom and has to be repeated in every new test.

**Question.** Where should the check get the running test's identity?

- **A** — Put `TestContext` on `FormatterTestsBase` and delete the property from the 14 derived classes. They keep using the inherited one; each loses 5 lines, and every access stays `TestContext.…`. `BeginTest` keys the session by `FullyQualifiedTestClassName`, `TestName`, and the data row, and the stack walk is deleted. This closes #1 and the full #2, and it also covers the gap in `02-test-implementer.md` where an entry outlives every call. Cost: 14 more test files touched in this run, within the tests lane.
- **B** — Use MSTest's static `TestContext.Current` in `BeginTest`. No derived class changes. The API is marked experimental in MSTest 4.5.1 ("may change, break, or be removed at any time"), which needs a suppression and can break on an MSTest upgrade.
- **C** — Keep the stack walk and record the limitation: it depends on tiered compilation, and the default and CI runs are the supported configurations. No code change. A future runtime or configuration change turns the helpers red across the suite: a loud failure, never a false pass.

**Recommendation: A.** It takes the identity from MSTest's own execution without an experimental API. The edit is mechanical, and it is the only option that also fixes data rows and outliving entries. B becomes preferable only if touching the 14 classes is out of the run's scope, and C only if the maintainer accepts JIT-dependent behavior in test infrastructure.

## Architecture findings

_None._ Every repair is local to the test helpers. The open choice is about where the test helpers get the identity of the running test; it is not a structural defect of a shipped component. The pre-existing fourth copy of the parse-and-context rule in `ObservedPipeline.Run` is untouched: it is outside the findings, and it is a follow-up candidate to switch it to `SecondPassInput.Create`.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.Pipeline.PhaseStabilityCheckTests"
scripts/test.sh --project formatter
scripts/test.sh --project architecture
DOTNET_TieredCompilation=0 scripts/test.sh --project formatter --filter "FullyQualifiedName~PhaseStabilityCheckTests|FullyQualifiedName~WrappedAngleBracketListTests"   # finding 1 reproduction
```

Results on 8577744:
- Formatter 3036/3036 and architecture 89/89.
- `scripts/lane-check.sh tooling --head worktree`: confirmed.
- `scripts/format.sh` over `Helpers/` and `Unit/Pipeline/`: no changes left.
