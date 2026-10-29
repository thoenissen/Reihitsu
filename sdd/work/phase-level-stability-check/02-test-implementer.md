Role: test implementer
Commit: 4f56d0d
Verdict: tests written

My prompt named the issue, slug, lane, branch, base, and the escalation-12 answer. It carried no intended fix, suspected cause, or hoped-for verdict.

The issue's whole deliverable is test-side: the helper extension and the exception list. I wrote both, so nothing is left for the implementer, and no production file changed. The production phase list stays `private`. The check reads it through reflection (`FormattingPipeline.CreatePhases`), not through a restated copy (FC-015).

## What the check does

- `AssertRuleResult` runs the check under LF and CRLF, both with and without an expectation. `AssertSecondPassIsStable` does the same. Each runs it once the text assertions hold, on the final text. Every production phase runs alone through `FormattingPipeline.ExecutePhases`. The text is reparsed with the fixture's `parseOptions` and the same `FormattingContext` (end of line, `LanguageVersionResolver.Resolve(tree.Options)`), as a second pass would see it (FC-033).
- **Exception key.** The key is the test method, the fixture fingerprint, the phase, and the line endings. The test method is found on the call stack: the innermost method with `[TestMethod]`, or its async state machine. The fingerprint is the first 16 hex digits of SHA-256 over the LF-normalized final output, so one entry covers both endings and tells several helper calls in one method apart (FC-017). The phase is written as `nameof(...)` and the test as `typeof(...)` / `nameof(...)`, so a rename breaks the build. Line endings are `Lf`, `Crlf`, or `Both` (FC-010).
- **Failures.**
  - Unlisted change: names the phase, fingerprint, qualified test, and line ending, plus the first differing line.
  - Stale at the call: the fixture's entry names a phase that no longer changes it under that ending.
  - Stale at cleanup: an entry of a test method that ran the check was never reached under an ending it covers. This happens when the fingerprint drifted or the fixture went away. `[TestCleanup]` on `FormatterTestsBase` reports it.
  - Per-test state is an `AsyncLocal` dictionary that `[TestInitialize]` creates. I confirmed that it flows into the test and into its cleanup under MSTest 4.5.1: every check found its recorder, and the cleanup failures below fired.
- **No `TestContext` on the base.** Twenty derived classes already declare `TestContext`, and MSTEST0036 forbids a second one on the base. That is why the test identity comes from the stack.
- **The enum location.** It lives in `Helpers/Enumerations/` because `ShouldBeInEnumerationsNamespace` (architecture tests) requires it.

## The exception list

`src/Reihitsu.Formatter.Test/Helpers/PhaseReversalExceptions.cs` lists the fixtures that reverse today. I found them by running the check over the whole formatter suite (3002 tests) with every violation collected rather than asserted; that collection code is gone. Ten fixtures reverse, all under both LF and CRLF, and every one belongs to one of the issue's three causes. `scripts/trace.sh <final output> --passes 2` shows the same pair of phases for each.

| Test | Fingerprint | Phase | Cause |
|---|---|---|---|
| `DelimitedDocumentationBlankLineConvergenceTests.SeparatesDelimitedDocumentationFromPrecedingUsingDirectiveInOnePass` | c258de221e30fadf | UsingDirectiveOrderingPhase | 1, blank line above comment-led using of same group |
| `UsingDirectiveOrderingTests.SameGroupDirectiveBehindMultiLineDelimitedDocumentationCommentKeepsTheCommentWithIt` | 855e76f82f59bced | UsingDirectiveOrderingPhase | 1 |
| `CommentLedFirstTokenAnchorTests.CreationInitializerBraceBehindCommentThatLeadsTheLineMovesNoOtherLine` | d98468a6e06d824b | BlankLinePhase | 2, blank line next to `{` |
| `CommentLedFirstTokenAnchorTests.NestedInitializerAlignsToItsBraceBehindCommentThatLeadsTheLine` | 79b71c9b01485d74 | BlankLinePhase | 2 |
| `WrappedAngleBracketListTests.UnboundGenericWithZeroWidthAnchorIsNotAlignedToADegenerateColumn` | 0bd97eed9cf654ec | LineBreakPhase | 3, line-start indentation (`Dictionary<,⏎>`) |
| `ArgumentFormattingFullPipelineTests.FormatsMethodChainWithArguments` | c328bbe3d627fc17 | LineBreakPhase | 3 (`(a, b) =>` argument line) |
| `IndentationTests.LambdaBlockBodyInArgumentListIndentsCorrectly` | bcc307cbab39d7a4 | LineBreakPhase | 3 (`(a, b) =>`) |
| `IndentationTests.SwitchExpressionInsideLambdaBreaksAndAligns` | c3d9f70128b65ace | LineBreakPhase | 3 (`(total, current) =>`) |
| `ListElementAttributeLayoutSinglePassTests.MinimalApiAttributedLambdaSettlesInOnePass` | d14ac64d5396f53c | LineBreakPhase | 3 (`() => "x"` line behind `[Authorize]`) |
| `ParameterContinuationLineAlignmentTests.LambdaParameterInWrappedArgumentStaysAligned` | b8e2a139534a68aa | LineBreakPhase | 3 (`(int first,` line) |

Where the issue's text and the observations differ:

- **`ExpressionBodyArrowCommentTests`.** The issue names fixtures in this class under cause 2, but none of them reverses. This matches the skeptic's finding that `CommentsAroundTheArrowLeadTheCreatedStatement` is stable.
- **Cause 2's block statement after `}`.** It reverses (`BlankLinePhase`), but no existing fixture formats that shape. It is covered by a characterization test, not by an entry.
- **`MinimalApiAttributedLambdaSettlesInOnePass`.** The stripped line is the lambda's parameter list behind an attribute line, not the lambda's first line. It is still "the line-break phase strips the indentation of a line that the indentation phase writes back", so it goes under cause 3.

## Tests

All tests below are in `src/Reihitsu.Formatter.Test/Unit/Pipeline/PhaseStabilityCheckTests.cs`. None carries a requirement tag: no requirement specifies the per-phase property yet (see `01-skeptic.md`, FMT-PIPE-004, which a later sibling issue adds).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| CheckRunsTheProductionPhasesInPipelineOrder | FC-015 | `CreateProductionPhases` vs `ObservedPipeline.Run` | pass |
| CheckFindsNoChangeInFormattedSource | stable valid code | `FindChangingPhases`, LF+CRLF | pass |
| CheckFindsThePhaseThatChangesTheSource | other side of the condition | `FindChangingPhases`, LF+CRLF | pass |
| CheckParsesTheTextWithTheFixtureParseOptions | FC-033 | `FindChangingPhases` with and without `#if` symbol | pass |
| CheckCatchesUsingPhaseRemovingBlankLineAboveDelimitedDocumentationLedUsingDirective | cause 1, `/** */` | `FindChangingPhases`, LF+CRLF | pass (exactly `UsingDirectiveOrderingPhase`) |
| CheckCatchesUsingPhaseRemovingBlankLineAboveCommentLedUsingDirective | cause 1, `//` | same | pass |
| CheckFindsNoChangeAboveCommentLedUsingDirectiveOfAnotherGroup | stable neighbour of cause 1 | same | pass (no phase) |
| CheckCatchesBlankLinePhaseInsertingBlankLineBeforeBlockAfterClosingBrace | cause 2, block after `}` | same | pass (exactly `BlankLinePhase`) |
| CheckCatchesBlankLinePhaseInsertingBlankLineAboveCommentGluedToOpeningBrace | cause 2, `/* o */ {` | same | pass |
| CheckCatchesLineBreakPhaseStrippingIndentationOfClosingAngleBracketBehindOmittedTypeArgument | cause 3, `Dictionary<,⏎>` | same | pass (exactly `LineBreakPhase`) |
| CheckCatchesLineBreakPhaseStrippingIndentationOfParenthesizedLambdaArgument | cause 3, `Run(1,⏎(x) => x)` | same | pass |
| FingerprintIgnoresLineEndingsAndTellsFixturesApart | FC-010, FC-017 | `ComputeFingerprint` | pass |
| UnlistedChangeFailsNamingFixturePhaseAndLineEnding | candidate 3 | `PhaseStabilityRecorder.Check`, LF+CRLF | pass |
| ListedChangePasses | listed side | recorder | pass |
| EntryWhosePhaseNoLongerChangesTheFixtureFailsAsStale | candidate 2 (at the call) | recorder, LF+CRLF | pass |
| EntryForAnotherPhaseDoesNotHideTheChange | FC-017 (an entry hides only its own phase) | recorder | pass |
| EntryCoversOnlyTheLineEndingsItNames | FC-010 | recorder | pass |
| EntryNoFixtureReachedFailsAsStale | candidate 2 (fingerprint drift) | `FindUnreachedExceptions` | pass |
| EntryReachedUnderOneLineEndingFailsAsStaleForTheOther | FC-010 | `FindUnreachedExceptions` | pass |
| EntryOfAnotherTestIsIgnored | FC-017 (key per test) | recorder | pass |
| EntryOfOneFixtureDoesNotCoverAnotherFixtureOfTheSameTest | FC-017 (key per call) | recorder | pass |
| AssertRuleResultWithoutExpectationRunsTheCheck | candidate 5 | `AssertRuleResult(input)` + `GetCheckedFixtures` | pass |
| AssertRuleResultWithExpectationRunsTheCheck | candidate 5 | `AssertRuleResult(input, expected)` | pass |
| AssertSecondPassIsStableRunsTheCheck | candidate 5 | `AssertSecondPassIsStable` | pass |
| HelpersRunTheCheckWithTheFixtureParseOptions | candidate 5, FC-033 | both helpers with `parseOptions` | pass |
| EveryExceptionNamesATestMethodAndAKnownCause | list integrity | reflection over `PhaseReversalExceptions.Entries` | pass |

The ten listed fixtures in their own test classes are the end-to-end cases. Each fails today with the unlisted message once its entry is removed: the first run had an empty list, and all ten failed naming their phase under LF.

### Failures watched

- **Empty list.** The 10 fixtures above fail, each naming its fixture, phase, and line ending.
- **Temporary entries (removed again).** I added three:
  - an entry with a fingerprint no fixture has, for `AssertRuleResultWithoutExpectationRunsTheCheck`, and the same for `AssertSecondPassIsStableRunsTheCheck`;
  - a one-digit change to the fingerprint `c258de221e30fadf`.

  All three tests failed in cleanup with `Stale phase reversal exception ... under LF` and `... under CRLF`. The changed fingerprint also failed at the call, as an unlisted `UsingDirectiveOrderingPhase` change. `EveryExceptionNamesATestMethodAndAKnownCause` failed on the string-named entry.
- **Final state.** Formatter suite 3028/3028, architecture 89/89, analyzer, core, and CLI green (`scripts/test.sh`). `scripts/lane-check.sh tooling --head worktree`: confirmed.

## Not covered, and why

- **The `CheckCatches*` tests characterize today's defects.** The structural packages under thoenissen/Reihitsu#1482 that remove a cause must delete those tests together with the cause's entries. The tests fail loudly when that happens, by design.
- **An entry whose test method no longer calls either helper at all is not reported.** Cleanup only knows the test methods that ran the check: no `TestContext` on the base, see above. A rename breaks the build through `nameof`. A deleted call with the method kept is the one gap. Closing it needs a `TestContext` on `FormatterTestsBase`, which means removing the property from the twenty derived classes. That is a follow-up candidate, not done here.
- **Formatter tests that check idempotency without these two helpers are out of scope.** Examples are `FormatterPhaseTestsBase`, the node-level entry points, and the public-API `ReihitsuFormatter*Tests` (see `01-skeptic.md`, Surfaces).
- **The reversal inside one phase is out of reach.** Its own steps undo each other, the fourth reversal named in the sibling issue thoenissen/Reihitsu#1499. Running one phase alone cannot see it.
- **Failure classes outside the list.** Analyzer, code-fix, Fix All, naming, and token-gap failure classes cannot reach this change: no shipped source changes. Second-pass idempotency and CRLF are the property under test itself.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.Pipeline.PhaseStabilityCheckTests"
scripts/test.sh --project formatter
scripts/test.sh --project architecture
```
