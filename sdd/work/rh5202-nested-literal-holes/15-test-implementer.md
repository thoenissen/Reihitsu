Role: test implementer
Commit: d2196f6
Verdict: tests written

This is repair round 2. The tests follow lead decision 10 (every empty line after the opening line stays empty, hole code included), the tag list in `14-spec-author.md`, and finding 2 of `12-spec-checker.md` (the tags). The message carried no intended fix, suspected cause, or hoped-for verdict. The only file changed is `src/Reihitsu.Analyzer.Test/Formatting/RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzerTests.cs`. At d2196f6 the RH5202 class passes 65 of 69 tests. The 4 red tests fail for one reason only: the fix writes 13 spaces onto the empty hole-code line where an empty line is expected. No other line differs.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| VerifyFixLeavesEmptyLineInsideHoleEmptyWhenShiftingRight (renamed from VerifyFixShiftsEmptyLineInsideHole). The expected output is now an empty line, per decision 10 and the last example of RH5202-005 | RH5202-003, RH5202-005 | Analyzer `Verify` | red: 13 spaces written onto the empty line |
| VerifyFixLeavesEmptyLineInsideHoleEmptyWhenShiftingRightWithCrLf (renamed from VerifyFixShiftsEmptyLineInsideHoleWithCrLf), same change | RH5202-003, RH5202-005; FC-033 | Analyzer `Verify` + CRLF | red: same |
| VerifyFixLeavesEmptyLineInsideHoleOfNestedInterpolatedStringEmptyWhenShiftingRight (new). An empty line of code in a multi-line hole of a regular `$"x{…}"` nested in the outer literal's hole: "at every depth", sibling shape | RH5202-003, RH5202-005; FC-033 | Analyzer `Verify` | red: same |
| …WithCrLf (new) | RH5202-003, RH5202-005; FC-033 | Analyzer `Verify` + CRLF | red: same |
| VerifyFixLeavesEmptyLineInsideHoleEmptyWhenShiftingLeft (unchanged, tags already RH5202-003 and RH5202-005) | RH5202-003, RH5202-005; FC-003 (other direction) | Analyzer `Verify` | green |
| VerifyFixLeavesEmptyContentLineOfRawStringNestedInHoleEmptyWhenShiftingRight / …WithCrLf / …WhenShiftingLeft / …WhenShiftingLeftWithCrLf (RH5202-005 tag added) | RH5202-004, RH5202-005, SHARED-FIX-007 | `Verify` + `ApplyCodeFixAsync` | green |
| VerifyFixShiftsWhitespaceOnlyContentLine (RH5202-003 tag added). This is the second example of RH5202-005 (6 spaces become 18) | RH5202-003, RH5202-005 | Analyzer `Verify` | green |
| VerifyFixShiftsWhitespaceOnlyLineInsideHole (unchanged, RH5202-003 only, as 14 says; decision 11 keeps `main`'s shift) | RH5202-003 | Analyzer `Verify` | green |
| VerifyFixLeavesEmptyContentLineEmpty(/WithCrLf), …OfInterpolatedLiteralEmpty(/WithCrLf), …EmptyWhenShiftingLeft, VerifyFixMatchesFormatterForEmptyContentLine (unchanged) | RH5202-005 | Analyzer `Verify` / `VerifyCodeFixMatchesFormatter` | green |

Coverage of the RH5202-005 examples, one row per example in the spec:

| Example | Test |
|---|---|
| plain, 12 to the right | VerifyFixLeavesEmptyContentLineEmpty |
| whitespace-only content line | VerifyFixShiftsWhitespaceOnlyContentLine |
| interpolated | VerifyFixLeavesEmptyContentLineOfInterpolatedLiteralEmpty |
| nested raw string, 13 to the right, still reported | VerifyFixLeavesEmptyContentLineOfRawStringNestedInHoleEmptyWhenShiftingRight (the same input and output as the example) |
| empty hole-code line, 13 to the right | VerifyFixLeavesEmptyLineInsideHoleEmptyWhenShiftingRight (the same input and output as the example) |

All five were already covered, so no new test was needed for an example. The two new tests cover the "hole code at any depth" clause of RH5202-005's normative sentence.

## Scenarios (bug fix)
Not applicable. These are tests for an amended requirement, not a reproduction.

## Not covered, and why
- **Formatter parity for an empty hole-code line:** not applicable. The formatter removes the line, and RH5202-005 cites SHARED-FIX-006 for why the fix leaves it empty instead. The outputs differ by design.
- **A whitespace-only hole-code line written empty:** out of the run (decision 11). It is a follow-up candidate. The existing test keeps `main`'s shift.
- **Empty hole-code line shifting left in a nested hole:** the line has no leading whitespace to remove, so it stays empty whatever the guard decides. The direct-hole left test already covers that arm.
- **Comment or directive in the gap:** an empty line holds neither. The comment cases are under RH5202-004's existing tests.

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzerTests"
scripts/test.sh --project architecture
```
Architecture project at d2196f6 plus these tests: 115 of 115 pass.
