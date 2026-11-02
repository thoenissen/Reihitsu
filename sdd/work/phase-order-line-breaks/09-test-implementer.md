Role: test implementer
Commit: 06ebdaa (tests); handover recorded in the next commit
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. This round carries out `00-lead.md` decision 26: cover FMT-SWITCH-016's second example (comment-free blank lines in front of the first wrapped statement and the terminal `break` are not carried). Expected values come from that example in `sdd/specs/formatter/switch-case-braces.md`. No production code touched.

**No existing test covered the example.** `rg -n "FMT-SWITCH-016" src --type cs` finds four tagged tests, in `Regression/SwitchCaseBraces/SwitchBraceLayoutOwnerTests.cs` (`BlankLinesAroundInsertedBlocksAreThoseOfAnyBlock`, `BlankLineInFrontOfFirstStatementIsRemovedBehindInsertedBrace`) and `Unit/SwitchCaseBraces/SwitchCaseBracePhaseLayoutTests.cs` (`BraceInsertionWritesAndRemovesNoBlankLine`, `BraceInsertionKeepsBlankLineInFrontOfFirstStatementComment`). Every one of them puts a comment in front of each blank line it checks, so all exercise the comment-carrying side of the requirement only. The two tests below add the comment-free side.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Reihitsu.Formatter.Test.Regression.SwitchCaseBraces.SwitchBraceLayoutOwnerTests.BlankLinesWithoutCommentAroundWrappedStatementsAreNotKept` | FMT-SWITCH-016 | `AssertRuleResult(input, expected)` (full pipeline, LF and CRLF, second pass and phase stability) | pass |
| `Reihitsu.Formatter.Test.Unit.SwitchCaseBraces.SwitchCaseBracePhaseLayoutTests.BraceInsertionDoesNotCarryBlankLinesWithoutComment` | FMT-SWITCH-016 | local `AssertLineTexts` (switch-brace phase alone, LF and CRLF, every output line's text in order) | pass |

The full-pipeline test uses the spec example's input and output verbatim. The phase-alone test checks the requirement's own sentence ("where it holds no comment, the phase MUST NOT carry it") at the phase that owns it: on the example input, the phase output holds no blank line behind `case 1:` or above the terminal `break`, and keeps the blank line in front of `case 2:`, which crosses no new brace. Without that second test, a phase that carried the comment-free blank lines and a blank-line phase that removed them afterwards would pass the pipeline test too.

Both pass on 2d88922, as decision 26 expects (output as on `main`). Neither is vacuous: the pipeline expected differs from the input (braces inserted, two blank lines gone), and the phase-alone assertion compares every line, so a carried blank line adds an `""` entry and fails the sequence comparison.

I also corrected the class summary of `SwitchCaseBracePhaseLayoutTests` ("writes and removes no blank line" → "writes no blank line, and carries only the blank lines that lead a comment"), which `08-spec-author.md` flagged as no longer matching FMT-SWITCH-016. The method name `BraceInsertionWritesAndRemovesNoBlankLine` is left as is: it is true for its own input (every blank line there leads a comment), and renaming a test the implementer worked against goes through the lead.

## Scenarios (bug fix)

Not a bug-fix lane.

## Not covered, and why

- **Only one of the two comment-free positions.** The example has a blank line in both positions at once; the tests do not split them into "only behind the label" and "only above `break`". Both positions go through `BlockWrapTriviaUtilities.GetCarriedLeadingTrivia` (`rg -n "GetCarriedLeadingTrivia" src/Reihitsu.Formatter/Pipeline/SwitchCaseBraces` → the two calls in `SwitchCaseBraceRewriter.AddBraces`, both with `keepBlankLinesAboveFirstComment: true`), and the phase-alone test asserts each position's line separately, so a failure in either shows.
- **Comment and directive in the gap.** The comment side is FMT-SWITCH-016's first example and the existing tests above; a directive in a switch section blocks brace insertion (`SectionCarriesDirectives`), so it does not reach the wrap.
- **CRLF and idempotency.** Covered inside both helpers.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~SwitchBraceLayoutOwnerTests|FullyQualifiedName~SwitchCaseBracePhaseLayoutTests"
scripts/test.sh --project architecture
```

On 2d88922 plus these tests: formatter filter 12 passed, 0 failed; architecture 92 passed (includes `RequirementCoverageTests`). `scripts/format.sh` over both test files formatted 0 of 2.
