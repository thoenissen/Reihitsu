Role: test implementer
Commit: see the commit that adds this file (on top of 583bee0)
Verdict: tests written

CI repair round 1, decision 19: a test that reaches `BlankLineEditor.cs:98`, the whitespace skip in `IsMissingRequiredBlankLineBeforeRegionDirective`. The prompt carried no intended fix, no suspected cause, and no hoped-for verdict. The expected outputs come from SHARED-BLANK-001 (a line that holds only whitespace is a blank line), SHARED-BLANK-005 / FMT-BLANK-013 (the line directly above a region directive must be blank), and FMT-CLEAN (the whitespace on that line is stripped). They are confirmed equal to `main`'s output: all four tests pass on `origin/main` 99ae644, copied into a detached worktree under a different class name.

## Tests
All tests are in `src/Reihitsu.Formatter.Test/Regression/BlankLines/RegionDirectiveLineAboveTests.cs`. Each input holds a line of four spaces (`{WHITESPACE}` placeholder, replaced at run time so that no trailing whitespace sits in the source file). Each expected output is the input with that line emptied, and no second blank line is inserted.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `WhitespaceOnlyLineAboveRegionCountsAsBlankLine` (the implementer's probe input) | SHARED-BLANK-001, SHARED-BLANK-005, FMT-BLANK-013 | `AssertRuleResult` (LF and CRLF, phase stability) | pass |
| `WhitespaceOnlyLineAboveEndRegionCountsAsBlankLine` (`#endregion` followed by a member) | SHARED-BLANK-001, SHARED-BLANK-005, FMT-BLANK-013 | `AssertRuleResult` | pass |
| `WhitespaceOnlyLineBetweenCommentAndRegionCountsAsBlankLine` (the skip ends on trivia in the list, `triviaIndex >= 0`) | SHARED-BLANK-001, SHARED-BLANK-005, FMT-BLANK-013 | `AssertRuleResult` | pass |
| `WhitespaceOnlyLineAboveEndRegionBeforeClosingBraceCountsAsBlankLine` (`#endregion` directly before the class's `}`) | SHARED-BLANK-001, SHARED-BLANK-005, FMT-BLANK-013 | `AssertRuleResult` | pass |

Reach proof: I temporarily replaced line 98 (`triviaIndex--;`) with `throw new InvalidOperationException("PROBE-LINE-98")`, ran the class, and restored the file byte for byte from a scratch copy (`git status` shows only the test file). Under the probe, the first three tests fail with `PROBE-LINE-98` from `BlankLinePhase`, which shows they execute line 98. Both sides of the loop's exit are covered: the first two exit with `triviaIndex < 0`, the third with `triviaIndex >= 0`. The fourth test does **not** reach line 98 (it passed under the probe). It is an observation, not a diagnosis: an `#endregion` directly before a closing brace with a whitespace-only line above it is decided elsewhere in the blank-line phase. I kept it as a characterization of that sibling shape.

## Scenarios (bug fix)
| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Whitespace-only line above `#region` / `#endregion` / below a comment / above `#endregion` before `}` | LF, CRLF (helper runs both) | formatter, full pipeline | pass on head and on `main` | observed equals expected |

## Not covered, and why
- Analyzer side (RH5031 on a whitespace-only line above a region): the change under test is the formatter reader. RH5031 reads the source text through the unchanged `SourceText` overload, and its whitespace handling is not part of decision 19.
- Tab-only line: the preparation phase replaces tabs with spaces before the blank-line phase (FMT-PIPE-019), so a tab line reaches line 98 as spaces. That is the same arm as the four-space line.
- Comment or directive in the gap: covered by the existing tests of this class (`SecondRegionBelowCommentInSameGapGetsBlankLine`, `…PragmaInSameGap…`, `…IfDirectiveInSameGap…`).
- Code-fix convergence, Fix All, Renamer: there is no code fix or rename in the changed code.

## Commands
```shell
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.BlankLines.RegionDirectiveLineAboveTests"
```
Result on this head: `Passed! - Failed: 0, Passed: 23`. `scripts/format.sh` on the test file: `Formatted 0 of 1 file(s)`.
