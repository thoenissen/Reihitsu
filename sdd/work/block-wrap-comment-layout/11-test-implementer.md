Role: test implementer
Commit: efcf8a0
Verdict: tests written

Repair round 2, decision 10 of `00-lead.md`: tests for maintainer answer 3A (09-auditor finding #1) at both places it names, the first wrapped statement and the terminal `break`. Also tests for the new FMT-SWITCH-007 example and the members of the auditor's class enumeration. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

Expected values come from FMT-SWITCH-007 as amended in `efcf8a0` (first and second paragraph, the new last example) and the two requirements it cites for "as the wrap finds it", FMT-BLANK-014 and FMT-BLANK-010. No existing test was changed. The new tests were appended to `src/Reihitsu.Formatter.Test/Regression/SwitchCaseBraces/BracedSwitchSectionCommentLayoutTests.cs` (the diff is insertions only). All of them use `FormatterTestsBase.AssertRuleResult(input, expected)`, which runs the full pipeline under LF and CRLF, checks the second pass and per-phase stability, and does the phase-tree round trip.

Every test has a second, multi-line section (`case 2:` / `C();`, or `case 1:` / `A();` / `B();` in the single-statement test) so that the switch is braced.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `SpecificationExampleKeepsTheBlankLineBehindTheLastLeadingCommentAtTheFirstStatementAndTheTerminalBreak` (FMT-SWITCH-007's new last example, verbatim) | FMT-SWITCH-007 | `AssertRuleResult` | passes |
| `BlankLineBetweenTheLastLineCommentAndTheFirstStatementIsKept` (site: first statement, `//`, one blank line) | FMT-SWITCH-007 | `AssertRuleResult` | passes |
| `BlankLineBetweenTheLastLineCommentAndTheTerminalBreakIsKept` (site: `break`, `//`, one) | FMT-SWITCH-007 | `AssertRuleResult` | passes |
| `BlankLineBetweenTheLastBlockCommentAndTheFirstStatementIsKept` (first statement, `/* */`, one) | FMT-SWITCH-007 | `AssertRuleResult` | passes |
| `BlankLineBetweenTheLastBlockCommentAndTheTerminalBreakIsKept` (`break`, `/* */`, one) | FMT-SWITCH-007 | `AssertRuleResult` | passes |
| `SeveralBlankLinesBetweenTheLastCommentAndTheFirstStatementAreCarriedAsOne` (first statement, `//`, three, giving one) | FMT-SWITCH-007, FMT-BLANK-014 | `AssertRuleResult` | passes |
| `SeveralBlankLinesBetweenTheLastCommentAndTheTerminalBreakAreCarriedAsOne` (`break`, `//`, three, giving one) | FMT-SWITCH-007, FMT-BLANK-014 | `AssertRuleResult` | passes |
| `BlankLinesBetweenAndBehindTheCommentsLeadingTheFirstStatementAreKept` (`// a` blank `// b` blank `A();`: the "last comment" qualifier, FC-002) | FMT-SWITCH-007 | `AssertRuleResult` | passes |
| `BlankLinesBetweenAndBehindTheCommentsLeadingTheTerminalBreakAreKept` (`// b1` blank `// b2` blank `break;`) | FMT-SWITCH-007 | `AssertRuleResult` | passes |
| `BlankLineBehindADocumentationCommentLeadingTheFirstStatementIsNotCarried` (first statement, `///`, one, giving none) | FMT-SWITCH-007, FMT-BLANK-010 | `AssertRuleResult` | passes |
| `BlankLineBehindADocumentationCommentLeadingTheTerminalBreakIsNotCarried` (`break`, `///`, one, giving none) | FMT-SWITCH-007, FMT-BLANK-010 | `AssertRuleResult` | passes |
| `BlankLineBehindTheCommentLeadingTheOnlyStatementOfASectionIsKept` (the auditor's `case 3:` / `// only comment` / blank / `A();    /* x */`) | FMT-SWITCH-007 | `AssertRuleResult` | passes |

### Result on this head and on `main`

On this head (`efcf8a0`), all 36 tests of the class pass, together with the neighbouring classes: 80 tests, 0 failing (command below).

To show that the new tests detect the change, I copied the test file into an extracted tree of `main` (`fbf5b6f`) and ran the class there. I checked that the switch-phase sources there match `fbf5b6f` and that `BlockWrapTriviaUtilities` is absent. Results:

- **Fail on `main`, as the auditor observed:** every `//` and `/* */` test at both sites, for one and for several blank lines, plus the spec example, the comment pairs, and the single-statement section. Each fails first on the blank line behind the last leading comment, which is missing. In the single-statement test, the second difference is the gap `A();    /* x */`, which `main` collapses to one space (answer 1A).
- **`BlankLineBehindADocumentationCommentLeadingTheTerminalBreakIsNotCarried`:** passes on `main`. The auditor marked this case "holds".
- **`BlankLineBehindADocumentationCommentLeadingTheFirstStatementIsNotCarried`:** fails on `main` because the pass that braces the section emits a blank line behind `/// doc`. This is the FMT-PIPE-004 defect that the existing `SingleLineDocumentationCommentLeadingTheFirstStatementIsStableInThePassThatBracesTheSection` already reproduces (03). It differs from the auditor's "holds", probably because the auditor compared the converged two-pass output. That output has no blank line on `main` either.

## Not covered, and why

- **The other side of the condition (no blank line between the last leading comment and its statement or `break`):** already covered by existing tests that stay unchanged, for example the first FMT-SWITCH-007 example, `CommentInFrontOfTheTerminalBreakStaysOnTheBreaksLine`, and `TerminalBreakCommentTests` (FC-003).
- **Label site and trailing site:** these sites cannot hold a blank line between a comment and its statement (09-auditor, class enumeration).
- **CRLF:** `AssertRuleResult` runs every test under LF and CRLF.
- **Idempotency (FC-010, FC-011, FC-012):** `AssertRuleResult` runs a second pass and the per-phase stability check.
- **Code fix, analyzer parity, node-level entry points (FC-007 to FC-009, FC-013, FC-014, FC-018, FC-028, FC-030):** unchanged since 03. No code fix or rule reaches the braced-section trivia, and the auditor dismissed these classes for the same reasons.
- **Directive in the gap:** the guard is unchanged, and `SectionWithADirectiveKeepsItsCommentsUnbracedWhileTheOtherSectionIsBracedWithItsLayout` pins it.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~BracedSwitchSectionCommentLayoutTests"
scripts/test.sh --project formatter --filter "FullyQualifiedName~BracedSwitchSectionCommentLayoutTests|FullyQualifiedName~BracedElseBodyCommentLayoutTests|FullyQualifiedName~TerminalBreakCommentTests|FullyQualifiedName~SwitchCaseBraceRequirementTests|FullyQualifiedName~PipelineRequirementTests"
```
