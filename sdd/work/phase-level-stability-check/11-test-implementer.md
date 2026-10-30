Role: test implementer
Commit: aba7bdb7
Verdict: tests written

This is repair round 2, working against `10-auditor.md` (f22a678). For finding 10 the maintainer chose the narrow option, recorded in `squad/decisions/2026-10-30-phase-level-stability-check-data-row-key.md`. Findings 11 and 12 are repaired in the same round. The auditor's suggestions were treated as suggestions. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Finding 10: the data-row key is unique, and anything else is refused

`DataRowKey.Format` (new, `Helpers/DataRowKey.cs`) is now the only owner of the row text. It replaces `PhaseStabilityCheck.FormatDataRow`. The base class, `PhaseReversalEntry`, and the integrity test call it.

**Why the text is unique.** Every argument becomes a self-delimiting token, and arguments are joined with `", "`:

| Argument | Token | Why it cannot collide |
|---|---|---|
| `null` | `null` | A string becomes `"null"` (quoted) |
| string | `"…"` with `\` and `"` escaped | It ends at the first unescaped `"`, so quotes and separators inside it are inert |
| char | `'…'` with `\` and `'` escaped | Its quote differs from a string's |
| bool | `true` / `false` | A string becomes `"true"` |
| int, uint, long, ulong | `1`, `1u`, `1L`, `1UL` | The suffix names the type |
| short, ushort, byte, sbyte | `(short)1` and so on | The prefix names the type |
| float, double | round-trip `R` plus `f` / `d` | Full precision, so `0.1+0.2` ≠ `0.3`; `-0d` ≠ `0d` |
| decimal | invariant plus `m` | Scale is kept, so `1.0m` ≠ `1.00m` |
| enum | `Full.Type.Name(<underlying>)` | Flag combinations and undefined values hold no separator |
| `Type` | `typeof("<FullName>")`, quoted | The name is escaped like a string |
| one-dimensional array, including jagged | `<element type>[e1, e2]`, recursive; a jagged element type is written `T[]` | Brackets enclose the elements. The element type keeps `int[]{1}`, `object[]{1}`, and `long[]{1}` apart. The last bracket pair always holds the elements, so `[]` suffixes cannot be misread |

A type full name written without quotes (enum type, array element type) must contain none of `"',[]()`. That excludes generic types.

**Refused, with a message naming the argument's position and type, at test start.** `FormatterTestsBase.BeginPhaseStabilityCheck` calls `DataRowKey.Format(TestContext.TestData)` in `[TestInitialize]`, so these fail before the test body runs:

- value tuples, records, and class or struct objects (the `DynamicData` objects the auditor names);
- `DateTime`, `Guid`, and any other type not in the table;
- multi-dimensional arrays;
- arrays with a generic or otherwise unquotable element type;
- an array holding a refused element. Its position is named, for example `argument 1, element 2`.

**Same full name in two assemblies.** I found this member myself; it was not in the audit. Two different types with the same full name in two loaded assemblies would share one token. The key refuses such a type. `IsUniquelyNamed` looks the name up in every loaded assembly. A type forwarded to the type itself, such as `System.Runtime` → `System.Private.CoreLib` for `System.Int32`, counts as the same type. I found that case when the first version refused `System.Int32`.

**Members I enumerated beyond the audit**, each pinned by a test:
- char against string;
- numbers of different types;
- full precision, NaN and infinity, and decimal scale;
- enums: defined, flag combination, undefined, and an enum against a string of its name;
- `Type` against a string of its name;
- an array against the same values as separate arguments;
- arrays of different element types;
- nested and jagged arrays, including empty jagged arrays at several depths;
- the same values moved between arguments;
- the multi-dimensional, generic-element, and unsupported-element refusals.

## Finding 11: one test-name format

- `PhaseStabilityCheck.FormatTestName(string testClassName, string testMethod)` is the only statement of the format. The `Type` overload calls it.
- The base calls it with `TestContext.FullyQualifiedTestClassName` and `TestContext.TestName`.
- `RuntimeTestNameMatchesTheEntryTestName` pins that the runtime key equals the key an entry builds from `typeof` and `nameof`. `TestKeyAppendsTheDataRowInParentheses` pins that the two overloads agree.

## Finding 12: comment text

- The `SecondPassInput` summary now names only the callers that use it: the formatter passes of `FormatterTestsBase`, the phases the check runs alone, and the fixed-point assertion of the check's tests.
- In `PhaseReversalLineEndings`, "exception" now reads "entry".
- From the auditor's hint: `TextChangingPhase` now reparses its output with the input tree's parse options.

## Tests

| Test | Covers | Expected now |
|---|---|---|
| `DataRowKeyTests` (20 tests: a row without arguments, scalar tokens, the distinctness members above, six refusals) | #10, each member | pass |
| `PhaseStabilityCheckTests.ArrayDataRowEntryCoversOnlyItsOwnRow ([1])` / `([2])` | #10, the auditor's array probe made permanent: an entry for row `System.Int32[1]` covers row 1; row 2 neither fails on it nor reports it stale | pass |
| `PhaseStabilityCheckTests.StringDataRowEntryCoversOnlyItsOwnRow` (`"a\", \"b","c"` / `"a","b\", \"c"`) | #10, string rows that collided before (both were `"a", "b", "c"`) | pass |
| `PhaseStabilityCheckTests.TestKeyAppendsTheDataRowInParentheses` | key format, #11 | pass |
| `PhaseStabilityCheckTests.RuntimeTestNameMatchesTheEntryTestName` | #11 | pass |
| `DataRowEntryCoversOnlyItsOwnRow` | expected text updated from `True` to `true` | pass |

`DataRowIsFormattedInInvariantCultureWithQuotedStrings` is replaced by `DataRowKeyTests.ScalarsAreWrittenAsTheirTokens` and `PhaseStabilityCheckTests.TestKeyAppendsTheDataRowInParentheses`.

## Evidence

**Failing first.** I temporarily routed `DataRowKey.Format` through the old per-argument formatting: `null`, quoted unescaped string, `Convert.ToString`. Over `DataRowKeyTests` and `PhaseStabilityCheckTests`, 19 of 63 failed. Among them:

- `ArraysWithDifferentElementsGetDifferentKeys`, `NestedArraysAreKeyedElementByElement`, `StringsWithQuotesBackslashesAndSeparatorsGetDifferentKeys`, `CharactersAreEscapedAndDifferFromStrings`, `NumbersOfDifferentTypesGetDifferentKeys`, `NumbersKeepTheirFullPrecision`, and `EnumsAreKeyedByTypeAndUnderlyingValue`: two rows shared a key. Both arrays became `System.Int32[]`.
- All six refusal tests: nothing was refused.
- `ArrayDataRowEntryCoversOnlyItsOwnRow ([1])` / `([2])` and `StringDataRowEntryCoversOnlyItsOwnRow ("a\", \"b","c")`.

I then reverted the change.

**Refusal at test start.** I added a temporary `DynamicData` class deriving `FormatterTestsBase` with rows `[new object()]` and `[new[] { 1 }]`. The result:

- `ProbeDynamicRow (System.Object)` failed with "The phase-level stability check cannot key this data row: argument 1 is a System.Object. …";
- the array row passed.

I then deleted the class.

**Final state (aba7bdb7):**
- formatter 3065/3065;
- `DOTNET_TieredCompilation=0` formatter 3065/3065;
- architecture 89/89;
- `scripts/format.sh` over `Helpers/` and `Unit/Pipeline/`: no changes;
- `scripts/lane-check.sh tooling --head worktree`: confirmed.

## Not covered, and why

**Two sources of the same row.** `DataRowAttribute.Data`, which the integrity test reads, and `TestContext.TestData`, which the runtime reads, are assumed to hold the same arguments. For `DataRow(new[] { 1 })` they agree: `ArrayDataRowEntryCoversOnlyItsOwnRow` passes at runtime with the key the attribute yields. If MSTest ever expanded them differently, for example a `string[]` passed as the only argument, which is covariant to `object[]`, an entry would read as unlisted plus stale. That failure is loud. No helper caller is data-driven today.

## Architecture findings

_None._ Every change stays inside the test helpers.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~DataRowKeyTests|FullyQualifiedName~PhaseStabilityCheckTests"
scripts/test.sh --project formatter
DOTNET_TieredCompilation=0 scripts/test.sh --project formatter
scripts/test.sh --project architecture
```
