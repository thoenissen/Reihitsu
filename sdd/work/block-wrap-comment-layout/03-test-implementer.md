Role: test implementer
Commit: 3d03766
Verdict: reproduced

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from FMT-PIPE-013, FMT-SWITCH-007 as amended on this branch (`a245f0c`), FMT-SWITCH-010, and FMT-PIPE-004, together with the maintainer answers in `00-lead.md`. Where a spec states a layout but not every column, the reference is what the same comments get today in a braced `if` body. I probed that with `scripts/format.sh`: `/** doc */ A();`, `/* multi` / `line */ A();`, `///` above the statement, and `B();// none` all stay as written and are stable over two passes.

**Characterization first.** I searched the test projects (`rg` over `Regression/SwitchCaseBraces`, `Unit/SwitchCaseBraces`, `Regression/Structural/SwitchCaseBracesTests.cs`, `PipelineRequirementTests.cs`). No existing test asserts the issue's observed switch output. The reproduction is therefore a new test.

All new tests use `FormatterTestsBase.AssertRuleResult(input, expected)`. It checks the full pipeline under LF and CRLF, a second pass, the per-phase stability check, and the phase-tree round trip (FMT-PIPE-021).

## Tests

New file `src/Reihitsu.Formatter.Test/Regression/SwitchCaseBraces/BracedSwitchSectionCommentLayoutTests.cs`:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `SectionsOfTheReportedSwitchKeepTheirCommentLayoutWhenBraced` (reproduction, the issue's switch input verbatim) | FMT-PIPE-013, FMT-SWITCH-007; FC-034 | `AssertRuleResult` | fails |
| `BlankLineBetweenCommentsLeadingTheFirstStatementIsKept` | FMT-PIPE-013, FMT-SWITCH-007 | `AssertRuleResult` | fails |
| `CommentInFrontOfTheFirstStatementStaysOnTheStatementsLine` | FMT-PIPE-013, FMT-SWITCH-007; FC-021 | `AssertRuleResult` | fails |
| `TwoCommentsOnTheirOwnLineAboveTheFirstStatementKeepTheSpaceBetweenThem` | FMT-PIPE-013, FMT-SWITCH-007 | `AssertRuleResult` | fails |
| `DocumentationBlockCommentInFrontOfTheFirstStatementStaysOnTheStatementsLine` | FMT-PIPE-013, FMT-SWITCH-007 | `AssertRuleResult` | fails |
| `MultiLineCommentEndingInFrontOfTheFirstStatementStaysOnTheStatementsLine` | FMT-PIPE-013, FMT-SWITCH-007 | `AssertRuleResult` | fails |
| `TwoCommentsBehindTheLastStatementKeepTheSpaceBetweenThem` | FMT-PIPE-013, FMT-SWITCH-007 | `AssertRuleResult` | fails |
| `GapInFrontOfTheCommentBehindTheLastStatementIsCarried` | FMT-SWITCH-007 | `AssertRuleResult` | fails |
| `MissingGapInFrontOfTheCommentBehindTheLastStatementStaysMissing` | FMT-SWITCH-007; FC-003 (zero gap next to four) | `AssertRuleResult` | fails |
| `TwoCommentsBehindACaseLabelKeepTheSpaceBetweenThem` | FMT-PIPE-013, FMT-SWITCH-007 | `AssertRuleResult` | fails |
| `TwoCommentsBehindADefaultLabelKeepTheSpaceBetweenThem` | FMT-PIPE-013, FMT-SWITCH-007 | `AssertRuleResult` | fails |
| `TwoCommentsBehindAPatternLabelKeepTheSpaceBetweenThem` | FMT-PIPE-013, FMT-SWITCH-007 | `AssertRuleResult` | fails |
| `CommentsBehindTheLastOfTwoLabelsKeepTheSpaceBetweenThem` | FMT-PIPE-013, FMT-SWITCH-007 | `AssertRuleResult` | fails |
| `GapInFrontOfTheCommentBehindTheLabelIsCarried` | FMT-SWITCH-007 | `AssertRuleResult` | fails |
| `SingleLineDocumentationCommentLeadingTheFirstStatementIsStableInThePassThatBracesTheSection` | FMT-PIPE-004, FMT-PIPE-013; FC-010 | `AssertRuleResult` | fails |
| `TwoLineDocumentationCommentLeadingTheFirstStatementIsStableInThePassThatBracesTheSection` | FMT-PIPE-004, FMT-PIPE-013; FC-010 | `AssertRuleResult` | fails |
| `TwoCommentsInFrontOfTheTerminalBreakStayOnTheBreaksLine` | FMT-SWITCH-007 | `AssertRuleResult` | fails |
| `CommentInFrontOfTheTerminalBreakStaysOnTheBreaksLine` | FMT-SWITCH-007 | `AssertRuleResult` | fails |
| `TwoCommentsBehindTheTerminalBreakKeepTheSpaceBetweenThem` | FMT-SWITCH-007 | `AssertRuleResult` | fails |
| `GapInFrontOfTheCommentBehindTheTerminalBreakIsCarried` | FMT-SWITCH-007 | `AssertRuleResult` | fails |
| `SpecificationExampleKeepsEveryCommentLayoutOfTheBracedSections` (FMT-SWITCH-007's last example, verbatim) | FMT-SWITCH-007 | `AssertRuleResult` | fails |
| `SwitchInAnUnbracedIfBodyKeepsTheCommentLayoutOfItsSections` | FMT-PIPE-013, FMT-SWITCH-007; FC-026, FC-027 | `AssertRuleResult` | fails |
| `AlreadyBracedSectionsWithTheseCommentLayoutsStayUnchanged` (stable valid code) | FMT-SWITCH-007 | `AssertRuleResult(input)` | passes |
| `SectionWithADirectiveKeepsItsCommentsUnbracedWhileTheOtherSectionIsBracedWithItsLayout` | FMT-SWITCH-010, FMT-SWITCH-007; FC-005, FC-019, FC-020 | `AssertRuleResult` | fails (only on the braced section; the directive-bearing section already matches) |

New file `src/Reihitsu.Formatter.Test/Regression/Structural/BracedElseBodyCommentLayoutTests.cs` (characterization of the `else` half; the `if` half is already covered by `StructuralTransformSeparatorFinalOutputTests`):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `BlankLineBetweenCommentsLeadingAnElseBodyIsKept` | FMT-PIPE-013 | `AssertRuleResult` | passes |
| `CommentInFrontOfAnElseBodyStaysOnTheStatementsLine` | FMT-PIPE-013 | `AssertRuleResult` | passes |
| `TwoCommentsOnTheirOwnLineAboveAnElseBodyKeepTheSpaceBetweenThem` | FMT-PIPE-013 | `AssertRuleResult` | passes |

I checked every failing test's first mismatch (LF; CRLF runs after LF in the same assertion). Each fails on the layout it names and on nothing else:

- blank line between leading comments: dropped;
- `/* lead */ A();`, `/** doc */ A();`, and `line */ A();`: the statement moves to its own line;
- `/* one */ /* two */`: split onto two lines;
- trailing and label comments: `/* x */// y` and `/* p *//* q */`, the space between the two comments is lost;
- `    // four`, `// none`, `    // label`, `    // gap`: each becomes one space;
- `/* b1 */ /* b2 */ break;` and `/* b2 */ break;`: split onto separate lines;
- `///`: an extra blank line after the comment in pass 1.

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Issue switch input, `case 1` (`// a` blank `// b`) | LF (CRLF in the same assertion, after LF) | formatter, full pipeline | reproduced | observed: no blank line between `// a` and `// b`; expected: the blank line is kept |
| Issue switch input, `case 2` (`/* lead */ C();`) | LF / CRLF | formatter, full pipeline | reproduced | observed: `/* lead */` and `C();` on separate lines; expected: `/* lead */ C();` |
| Issue `if` input | LF / CRLF | formatter | not reproduced (01-skeptic; 00-lead decision 4) | already matches the issue's required output; no new `if` test, existing `StructuralTransformSeparatorFinalOutputTests` cover it |
| Same three layouts as an `else` body | LF / CRLF | formatter | holds | characterization tests pass |
| Nearest sibling: switch one level deeper (inside an unbraced `if` body) | LF / CRLF | formatter | reproduced | label gap collapsed, `/* one */ /* two */` split, `/* x */// y`, `/* lead */` split |
| Counterpart surface (analyzer) | n/a | n/a | not run | no analyzer reports this layout (01-skeptic: RH5104 is outside SHARED-DIAG-010; no code-fix path involved) |

## Not covered, and why

- **Code fix / Fix All (FC-007, FC-008, FC-009, FC-018, FC-030):** the issue involves no code fix, and RH7501's fix works on a block that already exists (01-skeptic, Surfaces).
- **Analyzer parity (FC-013, FC-014):** no rule reports the braced-section comment layout (01-skeptic).
- **`RemoveBraces`:** unreachable. No shipped behavior removes braces (switch-case-braces.md non-goal).
- **Expression-body conversion and the `if` body:** not changed by this run, and already covered (`PipelineRequirementTests`, `ExpressionBodyArrowCommentTests`, `StructuralTransformSeparatorFinalOutputTests`).
- **Terminal `break` with comments on their own lines, with or without a blank line between them:** already covered by `TerminalBreakCommentTests`, which stays unchanged and is the other side of the "comment on the `break`'s line" condition (FC-003).
- **Sections whose comments are already in the rebuild's canonical form:** a single own-line comment, and one space before a single trailing comment. Already covered by `PipelineRequirementTests.BracedSwitchSectionKeepsOwnLineAndTrailingCommentsOnTheirLines` and `SwitchCaseBracesTests`. These are the other side of every layout condition above (FC-003).
- **Interior lines of the multi-line comment:** `MultiLineCommentEndingInFrontOfTheFirstStatementStaysOnTheStatementsLine` expects the comment's second line at its written column (19), not shifted with the block. That is what today's switch output gives for the same comment on its own line, and what the `if` body gives. No requirement places the interior lines of a delimited comment, so the test asserts that column only as today's behavior.
- **Label trivia other than the last label's:** not covered, because the skeptic's dispatch names only the last label's trailing trivia as a rebuild site (`BuildLabelTrailingTrivia`). I covered the last of two labels.
- **`///` behind a label:** not a plausible shape (01-skeptic matrix).
- **FC-022 to FC-025, FC-028, FC-029, FC-036:** no literals, identifiers, type modifiers, expression bodies, node-level entry points, key/value layout, or renames reach the switch phase's trivia carry.
- **FC-011 and FC-012:** the per-phase stability check and the second pass in `AssertRuleResult` fail any phase that undoes another, or any non-converging result.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~BracedSwitchSectionCommentLayoutTests|FullyQualifiedName~BracedElseBodyCommentLayoutTests"
scripts/test.sh --project formatter --filter "FullyQualifiedName~TerminalBreakCommentTests|FullyQualifiedName~SwitchCaseBraceRequirementTests|FullyQualifiedName~PipelineRequirementTests"
```

Result on this head: 27 tests, 23 failing, 4 passing (3 `else` characterization tests, plus `AlreadyBracedSectionsWithTheseCommentLayoutsStayUnchanged`).
