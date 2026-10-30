Role: auditor
Commit: 5f159ae39541ee130e0cf0427ad0a5b96580f254
Verdict: pass

My prompt named the repair round, the head, the base, the previous audit, the new handovers, and the decisions entry. It carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

- Head `5f159ae`, the checkout, with a clean tree. `origin/main` is `735d0db`, the SHA in my prompt, and the branch contains it.
- This is a repair-delta audit of round 2 against `f22a678`. My previous audit was at `9d31d36`.
- The delta matches `11`–`13`:
  - new `Helpers/DataRowKey.cs` and `Unit/Pipeline/DataRowKeyTests.cs`;
  - `FormatterTestsBase.BeginPhaseStabilityCheck` and the `FormatTestName(string, string)` overload;
  - removal of `PhaseStabilityCheck.FormatDataRow`;
  - comment fixes in `SecondPassInput` and `PhaseReversalLineEndings`;
  - parse options in `TextChangingPhase`;
  - new tests in `PhaseStabilityCheckTests`.
- Scope did not grow beyond the maintainer's answer (`squad/decisions/2026-10-30-phase-level-stability-check-data-row-key.md`, option A).
- `scripts/lane-check.sh tooling`: confirmed.

How I verified:

- I traced every token of `DataRowKey` for injectivity.
- On a `git archive` copy of the head in the scratchpad (the repository is untouched), I added probe rows for the one member the implementer left open: the two sources of a row (`DataRowAttribute.Data`, which the integrity test reads, and `TestContext.TestData`, which the runtime reads). The probes were `[DataRow(new[] { "a" })]` and `[DataRow(new[] { "a", "b" })]` on a `string[]` parameter (the covariant case that C# expands into `params object[]`), and `[DataRow(new object[] { 1, "x" })]` on an `object[]` parameter. Each row's runtime key was among the keys built from the attribute: 3/3 passed. In the same run, `DataRowKeyTests` and `PhaseStabilityCheckTests` passed together, 66/66 including the probes.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1–9 | closed | Untouched by this round, except that `FormatDataRow` moved into `DataRowKey`. The evidence of `10-auditor.md` stands. |
| 10 | closed | `DataRowKey.Format` writes every supported argument as a self-delimiting token, and the tokens are distinct by construction:<br>- strings `"…"` and chars `'…'`, both escaped;<br>- `null`, `true`, `false`;<br>- type-suffixed or type-prefixed numbers, with `R` round-trip for float and double and scale kept for decimal;<br>- enums `Full.Name(<underlying>)`;<br>- `typeof("…")`;<br>- arrays `<elementType>[…]`, recursive, where the last bracket pair holds the elements.<br>No token contains `", "` outside quotes or brackets. Every argument of another shape is refused in `[TestInitialize]` with its position named.<br>All members of #10 now hold: arrays (`ArrayDataRowEntryCoversOnlyItsOwnRow ([1])`/`([2])`), objects and tuples (refused, `ValueTupleArgumentIsRefused`, `ClassObjectArgumentIsRefused`), and strings with quoted separators (`StringDataRowEntryCoversOnlyItsOwnRow`). The members of #2 still hold. The implementer's mutation back to the old formatting failed 19 of 63 (`11`). |
| 11 | closed | The base calls `PhaseStabilityCheck.FormatTestName(string, string)`, the only statement of the format; the `Type` overload delegates to it. `RuntimeTestNameMatchesTheEntryTestName` pins the runtime key against an entry's key. |
| 12 | closed | `SecondPassInput` names only the callers that use it. `PhaseReversalLineEndings` reads "entry". |

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| _None._ | | | | | | | |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which data-driven execution is running | `DataRowKey.Format(TestContext.TestData)` | `DataRowKey.Format(DataRowAttribute.Data)`, used by the integrity test and the entries | Agree. Verified for scalar, `int[]`, covariant `string[]`, and `object[]` rows | ok |
| Which argument shapes can be keyed | the shapes the token table names; everything else is refused | uniqueness of the resulting text | Agree for the supported shapes. The refusal is wider than the entries need: see the first hint | ok (the maintainer's decision) |
| A type name is unique | `IsUniquelyNamed`: `Assembly.GetType(FullName)` over the loaded assemblies, cached per type | uniqueness over the run's lifetime | Agree for the assemblies loaded at first use. See the second hint | ok |
| Qualified test name | `FormatTestName(string, string)` only | the entry's key through `FormatTestName(Type, string)` | Agree, one statement | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Data-row text | `DataRowKey.Format`, called by `FormatterTestsBase.BeginPhaseStabilityCheck`, `PhaseReversalEntry` (through `dataRow`), and `EveryEntryNamesATestMethodItsDataRowAndAKnownCause`. `rg "FormatDataRow"` returns nothing | One owner |
| Qualified test-name format | `PhaseStabilityCheck.FormatTestName(string, string)`; the `Type` overload delegates | One owner |
| Second-pass parse and context | unchanged since `10-auditor.md` | One owner for the check's path |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `DataRowKeyTests` distinctness tests (14), e.g. `ArraysWithDifferentElementsGetDifferentKeys`, `ArrayAndItsElementsAsArgumentsGetDifferentKeys`, `StringsWithQuotesBackslashesAndSeparatorsGetDifferentKeys`, `NumbersKeepTheirFullPrecision`, `EnumsAreKeyedByTypeAndUnderlyingValue`, `RowsWithTheSameValuesInOtherArgumentsGetDifferentKeys` | Different argument lists give different text | Two rows with one key | `DataRowKey.Format` | adequate. 19 of 63 fail under the old formatting (`11`) |
| `DataRowKeyTests` refusal tests (6) | An unsupported argument fails and its position is named | No failure | `DataRowKey.Format` | adequate |
| `ArrayDataRowEntryCoversOnlyItsOwnRow`, `StringDataRowEntryCoversOnlyItsOwnRow` | An entry of one row neither covers nor turns stale in another row, end to end through `[TestInitialize]` | A stale or unlisted failure in the other row | override, `TextChangingPhase` | adequate. These are the auditor's probes |
| `RuntimeTestNameMatchesTheEntryTestName`, `TestKeyAppendsTheDataRowInParentheses` | The runtime key equals the entry key, and the two name overloads agree | Unequal keys | `TestContext`, `FormatTestName` | adequate |

## Failure classes dismissed
- FC-002 and FC-003: the new conditions are tested on both sides: supported and refused, distinct and colliding shapes, listed row and unlisted row.
- FC-004 to FC-009, FC-013, FC-014, and FC-018 to FC-030: no shipped source, rewrite, analyzer, or code fix changed.
- FC-010 and FC-012: the entries and fingerprints are unchanged since `10-auditor.md`.
- FC-011: no loop.
- FC-015: one owner per policy (table above).
- FC-016: the sibling shapes of #10 are enumerated and pinned in `DataRowKeyTests`.
- FC-017: the table above.
- FC-031: the summaries match the code.
- FC-032: the results come from this head.
- FC-033: unchanged.
- FC-034 and FC-035: no fixture or fingerprint changed.
- FC-036: `FormatDataRow` was removed and its callers were moved. The build follows, and `rg` finds no leftover reference.
- FC-037: no text-only claim.

## Architecture findings
_None._ The repair is local to the test helpers. No member of the #2/#10 class fails, so no finding repeats an earlier audit's class.

## Hints
- **The refusal is wider than the check needs, by the maintainer's choice of failing at test start.** `[TestInitialize]` keys every data-driven test of every `FormatterTestsBase` class. A row with an unsupported argument (`DateTime`, a tuple, an object) therefore fails even when the test has no entry and never calls the helpers. No such test exists today: the suite is 3065/3065. A key computed only when an entry with a data row exists for the test name, or when `Verify` first runs, would refuse only rows that need a key.
- **`IsUniquelyNamed` caches its answer at first use.** An assembly loaded later that defines the same full name is not seen, and `Assembly.GetType(name, false)` can still throw `FileNotFoundException` or `FileLoadException` for a forwarder whose target cannot be loaded. Both are contrived for this suite's argument types.
- **The uniqueness claim in `DataRowKey`'s summary has two contrived limits.** NaN values with different payloads share `NaNd`, and an enum with an underlying type that C# cannot declare (`char`, `bool`) gives an empty `Name()` token.
