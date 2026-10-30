Role: auditor
Commit: 9d31d36e3bcdf656ce5076093477661037abc438
Verdict: findings

My prompt named the repair round, the head, the base, the earlier audit, the new handovers, and the decisions entry. It carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

- Head `9d31d36`, which is the checkout, with a clean tree. `origin/main` is `735d0db`, the SHA in my prompt, and the branch contains it.
- This audits the repair delta of round 1 against `5dbd7f9` (my first audit was at `cb2ffd6`).
- The delta matches `06`–`09`:
  - helper rework under `src/Reihitsu.Formatter.Test/Helpers/`;
  - `Unit/Pipeline/PhaseStabilityCheckTests.cs` and the new `TextChangingPhase.cs`;
  - removal of the duplicate `TestContext` from 14 derived classes, which the maintainer chose (option A, `squad/decisions/2026-10-30-phase-level-stability-check-test-identity.md`). Each of those 14 diffs removes only the 9-line `#region Properties` block.
- Scope grew by those 14 files, inside the tests and tooling surface and as the maintainer decided. It did not grow into another surface, so this is a delta audit.
- `scripts/lane-check.sh tooling`: confirmed.

How I verified:

- **Static tracing** of `PhaseStabilityCheck`, `PhaseStabilitySession`, `PhaseStabilityRecorder`, `PhaseReversalEntry`, `PhaseChange`, `PhaseReversalCause(s)`, `PhaseReversalExceptionList`, `SecondPassInput`, `FormatterTestsBase`, and the new tests.
- **Experiments:** all on a `git archive` copy of the head in the scratchpad, except the first, which ran on the unmodified tree. The repository was not edited.
  1. `DOTNET_TieredCompilation=0 scripts/test.sh --project formatter --filter` over `IndentationTests`, `PhaseStabilityCheckTests`, `DelimitedDocumentationBlankLineConvergenceTests`, and `WrappedAngleBracketListTests` (the filter that failed 115/143 in the first audit): 155/155 passed.
  2. The first audit's mutation, with both `Assert.Fail` calls of `Verify` and `EndTest` replaced by no-ops. 6 of 38 check tests now fail:
     - `AssertRuleResultWithoutExpectationFailsOnAnUnlistedChange`
     - `AssertRuleResultWithExpectationFailsOnAnUnlistedChange`
     - `AssertSecondPassIsStableFailsOnAnUnlistedChange`
     - `HelperFailsOnAnEntryWhosePhaseNoLongerChangesTheFixture`
     - `TestCleanupFailsOnAnEntryNoFixtureReached`
     - `TestCleanupFailsOnAnEntryOfATestThatNeverRunsTheCheck`

     Before the repair, the same mutation left 29/29 green.
  3. A data-row probe with array arguments, described under #10.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | The stack walk is gone. `FormatterTestsBase.BeginPhaseStabilityCheck` keys the session from `TestContext`, and experiment 1 passes with tiered compilation off. |
| 2 | closed, with a new member in #10 | The members #2 listed now hold. Data rows with distinct scalar arguments are keyed apart (`DataRowEntryCoversOnlyItsOwnRow (True)`/`(False)`). Inherited methods are keyed by the executing class (`FullyQualifiedTestClassName`), which is the class the entry's `typeof` names; this one was traced, since no inherited test method exists. A member #2 did not enumerate fails: #10. |
| 3 | closed | `PhaseStabilityRecorder.IsCoveredBy` requires the phase **and** `PhaseOutputFingerprint`. Both members, an added change and a replaced change, produce another output fingerprint and therefore read as unlisted plus stale. `EntryForTheSamePhaseWithAnotherOutputDoesNotHideTheChange` pins this under LF and CRLF. |
| 4 | closed | Experiment 2. |
| 5 | closed | `ApplyRule`, `FindChangingPhases`, and `AssertIsFinalOutput` share `SecondPassInput.Create`. `ObservedPipeline.Run` is the pre-existing fourth copy and was not part of the suggested change. #12 covers the summary that says otherwise. |
| 6 | closed | Each `CheckCatches*` test passes the phase's full expected output to `AssertOnlyPhaseChanges`, under LF and CRLF. |
| 7 | closed | `PhaseReversalEntry.Phase => Cause.Phase`, so a cause cannot be paired with another phase. `EveryCauseNamesAProductionPhase` ties each cause to the production list. |
| 8 | closed | Both public summaries and the private `AssertRuleResult` summary now state the phase-level check. |
| 9 | closed | The types are renamed (`PhaseReversalEntry`, `PhaseReversalExceptionList`), the unused constructor is removed, the null `TestClass`/`TestMethod` is documented, and `Assert.IsNotNull(entry.TestClass, …)` guards the integrity test. One leftover word is covered by #12. |

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 10 | low | `Helpers/PhaseStabilityCheck.cs` `FormatDataRow`/`FormatArgument`; key built in `FormatterTestsBase.BeginPhaseStabilityCheck` | FC-001 | #2 | PR-introduced | **Problem.** The key of a data-row execution is the text `FormatArgument` writes. For any argument that is not a scalar or a string, that text is the type name, so different rows get the same key. This is #2's decision (which execution an entry belongs to, between the recorder's key and MSTest's execution) failing on a member #2's enumeration did not name.<br>**Probe.** `[DataRow(new[] { 1 })] [DataRow(new[] { 2 })] ProbeArrayRow(int[] values)`, with one entry for row `[1]` (`TextChangingPhase` for row 1, production phases for row 2):<br>• `ProbeArrayRow ([1])` passed;<br>• `ProbeArrayRow ([2])` failed with `Stale phase reversal entry (…ProbeArrayRow(System.Int32[]), …) … no longer changes …` and `… no fixture of the test has the final output … under CRLF`. This is the failure #2 reported.<br>**Members.**<br>• `DataRow` with array arguments: **fails** (constructed).<br>• `DynamicData` rows whose arguments are objects without an invariant `ToString` (tuples, records of non-primitives, custom types): **fails** (traced; `Convert.ToString` returns the type name).<br>• String arguments that themselves contain `", "` with quotes, e.g. `DataRow("a\", \"b")` against `DataRow("a", "b")`, which both give `"a", "b"`: **fails** (traced; contrived).<br>• `null`, `bool`, numbers, enums, plain strings, and distinct scalar rows: hold (`DataRowEntryCoversOnlyItsOwnRow`, `DataRowIsFormattedInInvariantCultureWithQuotedStrings`).<br>`EveryEntryNamesATestMethodItsDataRowAndAKnownCause` accepts such an entry, because the colliding text is one of the method's formatted rows.<br>**Impact.** No helper caller is data-driven today, so the failing members are latent. They fail loudly; none passes falsely. **Under `README.md`, *Architecture findings*, a finding that repeats an earlier audit finding's class is escalation 12, not another repair round.** | Make the row text injective, or refuse rows it cannot represent: format arrays element by element, escape quotes in strings, and fail in `BeginTest` with a clear message when an argument is neither a scalar, a string, nor an array of those. Pin each with a test. |
| 11 | low | `Helpers/FormatterTestsBase.cs` `BeginPhaseStabilityCheck` (`$"{TestContext.FullyQualifiedTestClassName}.{TestContext.TestName}"`) and `Helpers/PhaseStabilityCheck.cs` `FormatTestName` (`$"{testClass.FullName}.{testMethod}"`) | FC-015 | – | PR-introduced | The format of the qualified test name is written twice. The runtime key and the entry key match only while the two interpolations agree and MSTest's class name equals `Type.FullName`. They agree today, and the formatter suite has no nested or generic test classes. Nothing ties them, and a change to `FormatTestName` would not reach the base: every entry would then be unlisted plus stale. That failure is loud. | Have the base call one formatter that takes class name and method name (for example a string overload of `FormatTestName`), so a single statement owns the format. |
| 12 | low | `Helpers/SecondPassInput.cs` (type summary); `Helpers/Enumerations/PhaseReversalLineEndings.cs:4` | FC-031 | – | PR-introduced | `SecondPassInput`'s summary says "Every formatter pass of the test helpers … take their input from here". `ObservedPipeline.Run` and `FormatterPhaseTestsBase` in `Helpers/` still parse on their own. `PhaseReversalLineEndings` still says "a phase reversal exception covers" after the rename to `PhaseReversalEntry`. Both are comment text only and reach no compiled behavior. | Narrow the summary to the callers that use it (`ApplyRule`, the phase-level check, the check's fixed-point assertion). Change "exception" to "entry". |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which test execution is running | `TestContext.FullyQualifiedTestClassName` + `TestName` + `FormatDataRow(TestData)` | MSTest's execution (class, method, row) | Agree for scalar rows. Disagree for rows whose argument text collides | #10 |
| Entry belongs to the execution | `entry.TestKey == session key` (`FormatTestKey(FormatTestName(type, method), dataRow)`) | runtime key from the base's own interpolation | Agree today, stated twice | #11 |
| Entry covers a change | phase name **and** LF-normalized fingerprint of the phase's output | the documented reversal: that phase's specific change | Agree | ok (closes #3) |
| Stale at cleanup | the execution's entries against the execution's reached set, including entries of an execution that never ran the check | entry scope = one execution | Agree, except where keys collide (#10) | ok / #10 |
| What the phase-alone run parses | `SecondPassInput.Create`, once per check | `ApplyRule` through `SecondPassInput.Create` | Agree, one statement | ok (closes #5) |
| Override of entries and phases | `OverrideForCurrentTest` asserts that no fixture was checked yet | `Verify` uses `session.Recorder` | Agree | ok |
| A second `EndTest` | `_currentSession.Value = null` plus `session.IsEnded` | the base `[TestCleanup]` after a direct call | Agree, both paths return early | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Second-pass parse and context | `SecondPassInput.Create`, used by `FormatterTestsBase.ApplyRule`, `PhaseStabilityCheck.FindChangingPhases`, and `PhaseStabilityCheckTests.AssertIsFinalOutput`. `ObservedPipeline.Run:31-32` and `FormatterPhaseTestsBase:33` parse on their own and are pre-existing | One owner for the check's path. The follow-up candidate for `ObservedPipeline` is already named in `06`. #12 for the summary |
| Qualified test-name format | `FormatterTestsBase.BeginPhaseStabilityCheck` (inline) and `PhaseStabilityCheck.FormatTestName` | Two owners: #11 |
| Data-row text | `PhaseStabilityCheck.FormatDataRow`, used by the base and by the integrity test | One owner. It does not identify rows uniquely: #10 |
| Cause-to-phase link | `PhaseReversalCause.Phase`; `PhaseReversalEntry.Phase` derives from it | One owner |
| `TestContext` property | `FormatterTestsBase` for every derived class. The remaining declarations (`FormatterPhaseTestsBase`, `Integration/*`, `CommentJoinTests`, `CleanupPhaseTrailingDirectiveAtEndOfFileTests`) are in classes outside that hierarchy; the build has no MSTEST0036 | ok |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| AssertRuleResultWithoutExpectationFailsOnAnUnlistedChange, AssertRuleResultWithExpectationFailsOnAnUnlistedChange, AssertSecondPassIsStableFailsOnAnUnlistedChange | Each helper path fails on an unlisted change and names the phase, fixture, test, and `under LF` | No throw, or a message without those parts | `OverrideForCurrentTest` + `TextChangingPhase` | adequate (experiment 2) |
| HelperFailsOnAnEntryWhosePhaseNoLongerChangesTheFixture | Stale at the call fails through the helper | No throw | override, production phases | adequate |
| TestCleanupFailsOnAnEntryNoFixtureReached, TestCleanupFailsOnAnEntryOfATestThatNeverRunsTheCheck | `EndTest` fails on unreached entries under both endings, including for a test that never runs the check | No throw, or one ending missing | direct `EndTest` | adequate |
| HelpersPassOnAListedChange | A listed change passes both helpers and cleanup | A throw | override | adequate (the other side of the condition) |
| EntryForTheSamePhaseWithAnotherOutputDoesNotHideTheChange | Same phase, other output reads as unlisted plus stale (LF and CRLF) | Fewer than two failures | recorder | adequate |
| CheckCatches* (6) | The named phase alone produces exactly the named output | Another phase list or another output | `AssertOnlyPhaseChanges` | adequate |
| DataRowEntryCoversOnlyItsOwnRow (True/False) | One row's entry neither covers nor goes stale in another row | A stale or unlisted failure in row `False` | override | adequate for scalar rows. The colliding-row member is untested: #10 |
| DataRowIsFormattedInInvariantCultureWithQuotedStrings | The row text for `null`, string, double, and bool, and the key format | Other text | `FormatDataRow`, `FormatTestKey` | has no array or object member, which is where uniqueness breaks: #10 |
| EveryEntryNamesATestMethodItsDataRowAndAKnownCause | Entries name a real test, a matching row or none, a known cause, and no duplicate key | A bad entry accepted | reflection | accepts an entry whose row text collides: #10 |
| EveryCauseNamesAProductionPhase | Each cause's phase is a production phase | An unknown phase | reflection | adequate |

## Failure classes dismissed

- FC-002 and FC-003: the repair's new conditions are tested on both sides:
  - same output and other output;
  - listed row and unlisted row;
  - check run and check never run;
  - LF and CRLF.
- FC-004 to FC-009, FC-013, FC-014, and FC-018 to FC-030: no shipped source, rewrite, analyzer, or code fix changed.
- FC-010 and FC-012: the ten entries still cover `Both`. One LF-normalized output fingerprint serves both endings, and `06` reports the suite green under both.
- FC-011: no loop.
- FC-016: the sibling shapes are still characterized, now with full outputs.
- FC-032: the results come from this head.
- FC-033: the phase-alone run and `ApplyRule` share `SecondPassInput.Create`, and parse options still reach the check (`HelpersRunTheCheckWithTheFixtureParseOptions`).
- FC-034: the shapes are unchanged since the first audit.
- FC-035: the output fingerprints were read from unlisted messages of the unpatched head (`06`).
- FC-036: the renames are type renames that the compiler follows; `"CreatePhases"` is unchanged and guarded.
- FC-037: no text-only claim.

## Architecture findings
_None._ The repair is local to the test helpers. #11 states the same rule twice, but nothing behaves differently, so it is not a defect with a structural cause. #10 is not an architecture finding. It **repeats the class of #2**, which makes it escalation 12 under `README.md`, *Architecture findings*: the same decision, failing between the same components, on a member #2's enumeration did not name.

## Hints
- `TextChangingPhase` reparses its output without the context's parse options. That is fine for its fixtures, which have no `#if`. A future override test with symbols would see disabled text in the reparsed root.
