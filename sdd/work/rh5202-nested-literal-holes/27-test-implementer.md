Role: test implementer
Commit: 866aa44
Verdict: tests written

Final narrowing round, for lead decision 15 (audit 3, finding #5). My prompt carried no intended fix, suspected cause, or hoped-for verdict.

Changed file: `src/Reihitsu.Analyzer.Test/Formatting/RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzerTests.cs` gains two tests at the end of `#region Tests`. No production code changed.

Input (finding #5's shape, inside `void M(int n)`): the outer `$"""` shifts 8 columns to the left (closing 25 → 17). The nested `$"""` opens behind `Hello {` and closes with `k = 4`. Its line `{n` starts in its text with `w = 4 = k`; its line `+ 1}` starts in its own hole with 14 > 4 whitespace characters. Expected (RH5202-003/004, unchanged rewrite): every line loses `min(8, w)` — `Hello` line at 17, `{n` at 0, `+ 1}` at 6, nested closing at 0, outer closing at 17; the nested value `{n + 1}` keeps no leading whitespace, and the nested literal is still reported.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| VerifyFixShiftsLeftWhenWiderLineOfInterpolatedRawStringNestedInHoleStartsInItsOwnHole | RH5202-004, RH5202-006; FC-003, FC-017 | `Verify` (2 diagnostics) + `ApplyCodeFixAsync` exact text + re-`Verify` (nested still reported) | green |
| …WithCrLf | RH5202-004, RH5202-006; FC-033 | same + `NormalizeToCarriageReturnLineFeed` | green |

**Can fail.** With a temporary local mutation that makes the hole check constant `false` in `ChangesValueOfNestedRawString` (`if ((StartsInOwnHole(nested, line.Start) && lineNumber < 0) == false`, needed because a plain `true` trips S1144 on the then-unused method), both tests fail with `InvalidOperationException: No code fix action was registered for the diagnostic` at `ApplyCodeFixAsync`, and the other 89 RH5202 tests pass. The mutation was reverted (`git checkout`); the production file is unchanged.

**At head 866aa44** (with the two tests): RH5202 class 91 of 91 green; architecture 115 of 115 green.

## Scenarios (bug fix)

Not applicable: a missing offered-side test for an existing guard branch, not a reproduction.

## Not covered, and why

- **Withheld counterpart with a hole line.** A nested `$"""` whose hole line is wider than `k` *and* a text line wider than `k` is withheld by the text line alone; the existing withheld tests already pin that side. The new test is the side that only the hole qualifier decides.
- **Deeper hole (line inside a hole of a literal nested in the nested literal's hole).** Audit 3 (*Guard scope*) shows such a line lies inside the nested literal's own hole span too, so the same `StartsInOwnHole` call decides it; no separate arm.
- **`$$"""` nested with a multi-line hole.** The hole spans come from `InterpolationSyntax` regardless of the dollar count; one call site.
- **Fix All / second pass.** The outer rewrite is unchanged and its Fix All interaction is pinned by the existing Fix All pair; this test pins registration only, plus the single-fix output.
- **Comment or directive in the gap.** A comment in hole code is RH5202-004's existing tests; a directive in a hole is a parse error.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzerTests.VerifyFixShiftsLeftWhenWiderLineOfInterpolatedRawStringNestedInHoleStartsInItsOwnHole"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzerTests"
scripts/test.sh --project architecture
```
