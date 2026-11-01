Role: test implementer
Commit: see the commit that adds this file (subject "Test implementer: FMT-BLANK-011 line behind = and glued terminal-break comment follow the corrected spec [skip ci]")
Verdict: tests written

Repair round 2, follow-up to the spec corrections. I worked from `17-spec-author.md` ("Examples for the tests", items 1–7), `00-lead.md` rows 24–26, and the spec on e985d4d: FMT-BLANK-020's corrected third example and FMT-SWITCH-007's new sentence and corrected example. The prompt named the tests to adjust. It carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the spec examples and the requirement text. No assertion is weakened. Each changed expectation adds or removes exactly the one blank line that the corrected spec decides, and nothing else in the test changes. Production code is untouched.

Run on 8043a07 plus these tests: the two focused classes give 16 failed / 43 passed. Ten of the failures were already listed in `16-test-implementer.md` and are unchanged (finding 2, entry 18, and the four parenthesized-pattern `)` tests that row 26 keeps). The six that concern this round are below. Each fails on its one blank line under LF, the first line ending the helper checks.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `CommentBeforeBraceBlankLineTests.RemovesBlankLineBehindCommentLineBeforeAssignedCollectionInitializerAndOneLineComplexElement` (item 1, spec example: blank line added between `L =` and `// c`) | FMT-BLANK-020 | `AssertRuleResult` | fails now: no blank line above `// c` (line-break phase removes it); also, as before, the blank lines behind `// e` and `/* a */ /* b */` are kept |
| `CommentBeforeBraceBlankLineTests.RemovesBlankLineBetweenCommentAndOpeningBraceOfAssignedCollectionInitializer` (item 2, entry 13: blank line added between `L =` and `// c`; summary names the FMT-BLANK-011 line) | FMT-BLANK-020 | `AssertRuleResult` | fails now: `L =⏎// c⏎{` instead of `L =⏎⏎// c⏎{` |
| `CommentBeforeBraceBlankLineTests.InsertsBlankLineAboveCommentBetweenAssignmentAndOpeningBraceOfAssignedCollectionInitializer` (item 3, new, entry 13 without the author's blank line) | FMT-BLANK-011, FMT-BREAK-030 | `AssertRuleResult` | fails now: `L =⏎// c⏎{` instead of `L =⏎⏎// c⏎{` |
| `BracedSwitchSectionCommentLayoutTests.TwoCommentsInFrontOfTheTerminalBreakStayOnTheBreaksLine` (item 4: blank line between `}` and `/* b1 */ /* b2 */ break;` removed; summary loses "and its blank line") | FMT-SWITCH-007 | `AssertRuleResult` | fails now: blank line inserted above `/* b1 */ /* b2 */ break;` |
| `BracedSwitchSectionCommentLayoutTests.CommentInFrontOfTheTerminalBreakStaysOnTheBreaksLine` (item 5, entry 19's input: blank line removed; summary changed the same way) | FMT-SWITCH-007 | `AssertRuleResult` | fails now: blank line inserted above `/* b2 */ break;` |
| `BracedSwitchSectionCommentLayoutTests.SpecificationExampleKeepsEveryCommentLayoutOfTheBracedSections` (item 6, FMT-SWITCH-007 spec example: blank line removed) | FMT-SWITCH-007 | `AssertRuleResult` | fails now: blank line inserted above `/* b1 */ /* b2 */ break; /* x */ // y` |
| `BracedSwitchSectionCommentLayoutTests.AuthorsBlankLineAboveCommentInFrontOfTheTerminalBreakIsKept` (item 7, new: `B();⏎⏎/* b */ break;` gives `}⏎⏎/* b */ break;`) | FMT-SWITCH-007 | `AssertRuleResult` | passes now (holds; `main` keeps the author's line too). It guards the other side of the condition the implementer adds for items 4–6 (FC-003) |

`AssertRuleResult` checks the pass that inserts the braces, the second pass, and each phase run alone, under LF and CRLF. So items 4–6 also cover FMT-SWITCH-007's "already in the output of the pass that inserts the braces", and items 1–3 cover the phase reversal from finding A, because the line-break phase run alone must keep the line.

Unchanged, as `17-spec-author.md` says: the four `RemovesBlankLineBehind…BeforeParenthesizedPatternParenthesis` tests. They still expect `main`'s output (row 26) and still fail on the branch for the multi-comment shapes (auditor finding 2).

## Not covered, and why

- Own-line `// before break` above the terminal `break`: it keeps its FMT-BLANK-011 line. The existing FMT-SWITCH-007 spec-example tests already cover this (second example), and the corrected spec does not change it.
- The other `=` shapes in row 24's table (object initializer, array initializer, scalar, local, assignment expression, `new List<int>`): their output does not change in this round. `MovesBlankLineAboveOwnLineCommentInFrontOfCommentGluedToOpeningBrace` already pins the object-initializer case. The others are not part of the requested list. They already agree with FMT-BLANK-011 on `main` and on the branch.
- A directive in the gap behind `=`: FMT-BLANK-009 and FMT-BLANK-011 decide it, as they did before. This round changes no condition about directives.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.BlankLines.CommentBeforeBraceBlankLineTests|FullyQualifiedName~Reihitsu.Formatter.Test.Regression.SwitchCaseBraces.BracedSwitchSectionCommentLayoutTests"
```
