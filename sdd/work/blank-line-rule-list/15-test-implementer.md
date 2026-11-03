Role: test implementer
Commit: see the commit that adds this file (on top of fe6b684)
Verdict: tests written

Repair round 2, `14-auditor.md` finding 1. The four pipeline tests for a whitespace-only line above a region directive could not observe the whitespace skip in `BlankLineEditor.IsMissingRequiredBlankLineBeforeRegionDirective`: if the reader ignores the skip, it inserts a second blank line, and `BlankLineCollapser` removes that line later in the same phase. My prompt carried no intended fix, no suspected cause, and no hoped-for verdict. I treated the auditor's suggested change as a suggestion and re-derived the repair.

The repair has two parts:
- **Rewriter-level tests pin the reader** where the collapser cannot hide an insertion. I chose the two real callers of `EnsureBlankLineBeforeRegionDirectives`, run alone, over a direct call of the static reader. The callers compute the reader's facts (`PrecedingTokenFacts`, line start index), so these tests also check that wiring.
- **The four pipeline tests stay as characterizations of the final output.** I renamed them and rewrote their summaries so they claim only what the output shows. Each summary names the rewriter-level class that pins the reader.

Expected values come from three unchanged requirements:
- SHARED-BLANK-001: a line that holds only whitespace is a blank line.
- SHARED-BLANK-005 / FMT-BLANK-013: the line directly above a region directive must be blank.
- The rewriter alone performs no cleanup, so for a satisfied directive its output is the input unchanged, with the whitespace still on the line.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Unit.BlankLines.BlankLineRegionDirectiveRewriterTests.WhitespaceOnlyLineAboveRegionIsTakenForRequiredBlankLine` (new; the skip runs off the start of the list, `triviaIndex < 0`) | SHARED-BLANK-001, SHARED-BLANK-005, FMT-BLANK-013 | `ApplyRewriter` (rewriter alone, exact text) | pass |
| `Unit.BlankLines.BlankLineRegionDirectiveRewriterTests.WhitespaceOnlyLineBetweenCommentAndRegionIsTakenForRequiredBlankLine` (new; the skip ends on trivia in the list, `triviaIndex >= 0`) | SHARED-BLANK-001, SHARED-BLANK-005, FMT-BLANK-013 | `ApplyRewriter` | pass |
| `Unit.BlankLines.BlankLineTriviaBoundaryRewriterTests.WhitespaceOnlyLineAboveEndRegionIsTakenForRequiredBlankLine` (new class; `#endregion` followed by a member) | SHARED-BLANK-001, SHARED-BLANK-005, FMT-BLANK-013 | `ApplyRewriter` (new, same shape as the region class) | pass |
| `Unit.BlankLines.BlankLineTriviaBoundaryRewriterTests.WhitespaceOnlyLineAboveEndRegionBeforeClosingBraceIsTakenForRequiredBlankLine` (new; `#endregion` before the type's `}`) | SHARED-BLANK-001, SHARED-BLANK-005, FMT-BLANK-013 | `ApplyRewriter` | pass |
| `Unit.BlankLines.BlankLineTriviaBoundaryRewriterTests.MemberLineAboveEndRegionGetsBlankLine` (new; the insertion side, so that a rewriter that never inserts cannot pass the two tests above) | SHARED-BLANK-005, FMT-BLANK-013 | `ApplyRewriter` | pass |
| `Regression.BlankLines.RegionDirectiveLineAboveTests.WhitespaceOnlyLineAboveRegionIsKeptAsOneEmptyLine` (renamed from `…CountsAsBlankLine`; summary narrowed) | SHARED-BLANK-001, SHARED-BLANK-005, FMT-BLANK-013 | `AssertRuleResult` | pass |
| `Regression.BlankLines.RegionDirectiveLineAboveTests.WhitespaceOnlyLineAboveEndRegionIsKeptAsOneEmptyLine` (renamed; summary narrowed) | SHARED-BLANK-001, SHARED-BLANK-005, FMT-BLANK-013 | `AssertRuleResult` | pass |
| `Regression.BlankLines.RegionDirectiveLineAboveTests.WhitespaceOnlyLineBetweenCommentAndRegionIsKeptAsOneEmptyLine` (renamed; summary narrowed) | SHARED-BLANK-001, SHARED-BLANK-005, FMT-BLANK-013 | `AssertRuleResult` | pass |
| `Regression.BlankLines.RegionDirectiveLineAboveTests.WhitespaceOnlyLineAboveEndRegionBeforeClosingBraceIsKeptAsOneEmptyLine` (renamed; summary narrowed) | SHARED-BLANK-001, SHARED-BLANK-005, FMT-BLANK-013 | `AssertRuleResult` | pass |

The tag sets stay unchanged. The pipeline tests still assert output those requirements demand; they no longer claim anything about the reader.

**Mutation check.** I used the auditor's mutation: I appended `&& trivia.Count < 0` to the whitespace-skip `while` condition in `BlankLineEditor.cs`, then restored the file byte for byte from a scratchpad copy (`cmp` and `git diff --quiet -- src/Reihitsu.Formatter` both clean).
- All four new `…IsTakenForRequiredBlankLine` tests fail. Each fails on `Assert.AreEqual(input, actual)` with one extra character at the line above the directive, which is the inserted line break.
- The other 25 tests of the three classes pass.

**Observation on the closing-brace shape.** At the rewriter level, `#endregion` before `}` does reach the skip: its test fails under the mutation. In the full pipeline it did not reach the skip (`11-test-implementer.md`), so another rewriter earlier in the phase decides that line in the pipeline. This is an observation, not a diagnosis.

## Scenarios (bug fix)
| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Whitespace-only line above `#region`, below a comment above `#region`, above `#endregion` with a member below, above `#endregion` before `}` | `Environment.NewLine` (rewriter tests); LF and CRLF (pipeline tests) | the rewriter alone; the full pipeline | pass on head | observed equals expected |

## Not covered, and why
- **CRLF at the rewriter level.** The reader tests trivia kinds (`EndOfLineTrivia`, `WhitespaceTrivia`), not newline text, and the pipeline tests run both line endings. The rewriter helper follows the existing class and uses `Environment.NewLine`.
- **A direct unit test of `IsMissingRequiredBlankLineBeforeRegionDirective`.** The rewriter tests cover both loop exits for both directive kinds through the real fact computation, so a direct test would add no failing mutant.
- **Analyzer side, tabs, and comments or directives in the gap.** These are unchanged from `11-test-implementer.md`.
- **Code fix, Fix All, and Renamer.** The changed code has no code fix and no rename.

## Pre-existing failure outside my files
`Reihitsu.ArchitectureTests.RepositoryFilesReferenceNoSquadNote` fails on this head. `sdd/work/blank-line-rule-list/14-auditor.md:18` names the run note's path. That file is not mine and I did not change it. It goes to the lead.

## Commands
```shell
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.BlankLines.BlankLineTriviaBoundaryRewriterTests|FullyQualifiedName~Reihitsu.Formatter.Test.Unit.BlankLines.BlankLineRegionDirectiveRewriterTests|FullyQualifiedName~Reihitsu.Formatter.Test.Regression.BlankLines.RegionDirectiveLineAboveTests"
```
Results on this head:
- The focused run: `Passed: 29`.
- The full formatter suite: `Passed: 4411`.
- `scripts/format.sh` on the three test files: `Formatted 1 of 3 file(s)` (the new class).
