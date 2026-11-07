Role: test implementer
Commit: 7188f1e
Verdict: tests written

This is repair round 3, for lead decisions 12 and 13. It covers audit 2's findings #3 and #4 and the test list in `20-spec-author.md`. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

Changed files:

- `src/Reihitsu.Analyzer.Test/Formatting/RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzerTests.cs` gains 20 tests. It also gains a private helper, `VerifyOuterLiteralFixIsWithheldAndNestedLiteralFixIsOffered`, under a new `#region Methods`. The helper does three things. It verifies exactly the marked diagnostics. It asserts through `GetCodeFixActionsAsync` that no action is registered for the outer literal, which is the first multi-line raw string in document order. It asserts that exactly one action is registered for the nested literal, which is the last one.
- `src/Reihitsu.Core.Test/RawStringLiteralUtilitiesTests.cs` fixes finding #4. Rows 25, 26, 27, and 30 now write ` ` and ` ` as escapes. The values are unchanged and all 11 rows pass.

At 7188f1e, the RH5202 class has 89 tests: 77 pass and 12 fail. Every withheld-side test and both Fix All tests are red. Every offered-side test is green.

## Tests

In every input the outer literal shifts 8 columns to the left (25 → 17), unless the row says otherwise. `k` is the width of the whitespace in front of the nested closing quotes.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| VerifyFixIsWithheldWhenShiftingLeftWouldChangeValueOfRawStringNestedInHole (`k=4`, `x` at 34; RH5202-006 first example) | RH5202-006; FC-003 | `VerifyOuterLiteralFixIsWithheldAndNestedLiteralFixIsOffered` | red: outer action registered ("Expected collection to be empty") |
| …WithCrLf | RH5202-006; FC-033 | same + CRLF | red: same |
| VerifyFixIsWithheldWhenShiftingLeftWouldChangeValueOfInterpolatedRawStringNestedInHole (nested `$"""`, `x{n}`) | RH5202-006 | same | red: same |
| …WithCrLf | RH5202-006 | same + CRLF | red: same |
| VerifyFixIsWithheldWhenShiftingLeftAndClosingQuotesOfRawStringNestedInHoleStartInColumnZero (`k=0`) | RH5202-006 | same | red: same |
| …WithCrLf | RH5202-006 | same + CRLF | red: same |
| VerifyFixIsWithheldWhenShiftingLeftWouldChangeValueOfUtf8RawStringNestedInHole (`"""u8.Length`, `k=4`) | RH5202-006 | same | red: same |
| VerifyFixIsWithheldWhenShiftingLeftWouldChangeValueOfRawStringNestedInHoleOfNestedInterpolatedString (raw string in a hole of a regular `$"a{…}b"` in the hole, `k=4`) | RH5202-006 | same | red: same |
| VerifyFixAllFixesRawStringNestedInHoleFirstAndThenOuterLiteralKeepingNestedValue. The input is the first example. Expected: nested quotes in column 24, `x` in column 54, nested closing in column 24, relative 30. Exactly 2 incremental and 2 Fix All iterations (document, project, solution) | RH5202-006, SHARED-FIX-002 | `Verify(source, fixed, onConfigure, …)` | red: iterative application writes `x` in column 50, relative 26. This is the audit's value change |
| …WithCrLf | RH5202-006, SHARED-FIX-002 | same + CRLF | red: same |
| VerifyFixShiftsLeftWhenClosingWhitespaceOfRawStringNestedInHoleIsAsWideAsShift (`k=d=8`; second example, spec output) | RH5202-004, RH5202-006 | `Verify` + `ApplyCodeFixAsync` + re-`Verify` (nested stays reported) | green |
| …WithCrLf | RH5202-004, RH5202-006 | same + CRLF | green |
| VerifyFixShiftsLeftWhenNoContentLineOfRawStringNestedInHoleIsWiderThanItsClosingLine (`k=4`, `x` at 4; third example, spec output) | RH5202-003, RH5202-006 | same | green |
| …WithCrLf | RH5202-003, RH5202-006 | same + CRLF | green |
| VerifyFixIsWithheldWhenShiftingLeftWouldShortenWhitespaceOnlyContentLineOfRawStringNestedInHole (6-space line under `k=4`; fourth example) | RH5202-006 | withheld helper, `ExpandWhitespaceMarkers` | red: same as the first row |
| …WithCrLf | RH5202-006 | same + CRLF | red: same |
| VerifyFixShiftsLeftWhenWhitespaceOnlyContentLineOfRawStringNestedInHoleIsNoWiderThanItsClosingLine (4-space line under `k=4`; prose variant of the fourth example). Expected: the line is empty, and `x` and the nested closing quotes are in column 0 | RH5202-003, RH5202-006 | `Verify` + `ApplyCodeFixAsync` + re-`Verify` | green |
| …WithCrLf | RH5202-003, RH5202-006 | same + CRLF | green |
| VerifyFixShiftsRightWhenContentLineOfRawStringNestedInHoleIsWiderThanItsClosingLine. Shifts 13 columns to the right with `k=4` and `x` at 34. Expected: every line +13, `x` in column 47, nested closing in column 17, nested still reported. This is the "to the left" condition's other side; no existing test had a content line wider than the nested closing line on a right shift | RH5202-004, RH5202-006 | `Verify` + `ApplyCodeFixAsync` + re-`Verify` | green |
| …WithCrLf | RH5202-004, RH5202-006 | same + CRLF | green |
| `RawStringLiteralUtilitiesTests.GetLeadingWhitespaceLengthCountsEveryLeadingWhitespaceCharacter`, rows 25/26/27/30 (finding #4, escapes only) | RH5202-001 (unchanged tag) | MSTest `DataRow` | green |

Every red test was checked to fail for the expected reason. The diagnostics step of each withheld test passes: the input compiles and both literals are reported. Each one then fails at the assertion on the outer literal's actions. The Fix All tests fail on the `x` line only, and the diff shows the audit's 30 → 26.

## Scenarios (bug fix)

Not applicable. This round adds tests for a new requirement, RH5202-006, and repairs test data. It is not a reproduction.

## Not covered, and why

- **Existing `VerifyFixLeavesEmptyContentLineOfRawStringNestedInHoleEmptyWhenShiftingLeft` (+CRLF)** is not retagged with RH5202-006. It is on the offered side with `k = 29 ≥ d`, so it touches the requirement only incidentally. The `k=d` test covers that boundary exactly.
- **CRLF variants for the `u8` and depth-2 withheld tests.** I left these out. CRLF reaches the condition through the line split and the per-line whitespace measurement, which do not depend on the literal kind. The four CRLF withheld tests exercise that path (plain, interpolated, `k=0`, whitespace-only). The `u8` and depth arms differ only in which node is nested, not in line endings.
- **`$$"""` nested.** The spec names "any number of `$`". The interpolated `$"""` test covers the interpolated-start-token arm, and the dollar count changes only the hole delimiters, not the lines measured. If the implementer's guard branches on the dollar count, the lead should ask for a test.
- **Formatter parity.** None is added. Whole-document formatting of the first example keeps the value, as audit 2 found, and RH5202-006 is a code-fix decision with no formatter counterpart.
- **Comment or directive in the gap.** Not applicable. RH5202-006 measures whitespace at line starts inside a nested literal, where neither can occur. Comments in hole code are RH5202-004's existing tests.
- **Fix All across several outer literals in one document.** The batch base's `GetFixAllScenario` already covers that. The new Fix All tests cover the withheld-then-released interaction.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzerTests"
scripts/test.sh --project core --filter "FullyQualifiedName~Reihitsu.Core.Test.RawStringLiteralUtilitiesTests"
scripts/test.sh --project architecture
```

Results at 7188f1e with these tests: analyzer RH5202 77 of 89 pass (the 12 red tests are listed above), Core 11 of 11, architecture 115 of 115. `EveryRequirementOfAnEnforcedSpecIsCovered` is now green for RH5202-006.
