Role: test implementer
Commit: see the commit that adds this handover (tests and handover in one commit)
Verdict: tests written

Carries out maintainer answer 2 (`00-lead.md`, decisions 37 and 38) from `24-spec-author.md` and `25-spec-author.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict. No production code touched. The decided behavior is `main`'s and head's, so every test is expected to pass on head; all of them do (see *Commands*). Expected values are taken from the spec examples of FMT-BREAK-006 and FMT-BREAK-033 and from the outputs the spec-author handovers list, not from a run of the code.

## Tests

All in `src/Reihitsu.Formatter.Test/Regression/LineBreaks/`. Helper for every row: `FormatterTestsBase.AssertRuleResult` (LF and CRLF, second pass, `PhaseStabilityCheck`); rows marked "unchanged" call it without an expected value.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `StatementBehindMultiLineCommentTests.StatementsBehindCommentTailFollowTheExampleOfOneStatementPerLine` — FMT-BREAK-006's multi-line comment example (`M`, `N`, `O`) verbatim | FMT-BREAK-006, SHARED-COMMENT-012 | `AssertRuleResult` | pass |
| `StatementBehindMultiLineCommentTests.StatementBehindCommentTailWithoutBlankLineRequirementStaysBehindTheComment` — `A(); /* x⏎ */ B();` | FMT-BREAK-006, SHARED-COMMENT-012 | `AssertRuleResult` (unchanged) | pass |
| `StatementBehindMultiLineCommentTests.ReturnBehindTailOfCommentOnItsOwnLineStaysAndGetsTheBlankLineAboveTheComment` — `a++;⏎/* z⏎ */ return a;` | FMT-BREAK-006, SHARED-COMMENT-012 | `AssertRuleResult` | pass |
| `StatementBehindMultiLineCommentTests.FirstStatementBehindTailOfCommentBehindOpeningBraceStartsTheLineBelow` — `{ /* f⏎ */ A();` in a method block | FMT-BREAK-006, SHARED-COMMENT-012 | `AssertRuleResult` | pass |
| `StatementBehindMultiLineCommentTests.FirstStatementBehindTailOfCommentOnItsOwnLineStaysBehindTheComment` — `{⏎/* f⏎ */ A();` | FMT-BREAK-006, SHARED-COMMENT-012 | `AssertRuleResult` (unchanged) | pass |
| `StatementBehindMultiLineCommentTests.FirstStatementBehindTailOfCommentBehindLabelStartsTheLineBelow` — `case 1: /* p⏎ */ A();` | FMT-BREAK-006, SHARED-COMMENT-012 | `AssertRuleResult` | pass |
| `StatementBehindMultiLineCommentTests.FirstStatementOfSectionBehindTailOfCommentOnItsOwnLineStaysBehindTheComment` — `case 2:⏎/* q⏎ */ B();` | FMT-BREAK-006, SHARED-COMMENT-012 | `AssertRuleResult` (unchanged) | pass |
| `SwitchSectionBehindMultiLineCommentTests.SectionsAndLabelsBehindCommentTailsFollowTheExampleOfOneSectionPerLine` — FMT-BREAK-033's new example (`case 1:` to `case 5:`) verbatim; covers 25-spec-author items 1, 3, 6, 8, 9 | FMT-BREAK-033, FMT-BREAK-034, SHARED-COMMENT-012 | `AssertRuleResult` | pass |
| `SwitchSectionBehindMultiLineCommentTests.SectionBehindTailOfCommentOnItsOwnLineAfterReturnStaysBehindTheComment` — item 1 | FMT-BREAK-033, SHARED-COMMENT-012 | `AssertRuleResult` (unchanged) | pass |
| `SwitchSectionBehindMultiLineCommentTests.SectionBehindTailOfCommentOnItsOwnLineAfterBreakStaysAndGetsTheBlankLineAboveTheComment` — item 2 | FMT-BREAK-033, FMT-BLANK-004, SHARED-COMMENT-012 | `AssertRuleResult` | pass |
| `SwitchSectionBehindMultiLineCommentTests.LabelBehindTailOfCommentOnItsOwnLineStaysBehindTheComment` — item 3 | FMT-BREAK-034, SHARED-COMMENT-012 | `AssertRuleResult` (unchanged) | pass |
| `SwitchSectionBehindMultiLineCommentTests.FirstSectionBehindTailOfCommentOnItsOwnLineStaysBehindTheComment` — item 4 | FMT-BREAK-033, SHARED-COMMENT-012 | `AssertRuleResult` (unchanged) | pass |
| `SwitchSectionBehindMultiLineCommentTests.FirstSectionBehindTailOfCommentBehindOpeningBraceStartsTheLineBelow` — item 9 alone | FMT-BREAK-033, SHARED-COMMENT-012 | `AssertRuleResult` | pass |
| `SwitchSectionBehindMultiLineCommentTests.LabelBehindTailOfDocumentationCommentBehindLabelStaysBehindTheComment` — item 5, `case 1: /** l⏎ */ case 2:` | FMT-BREAK-034, SHARED-COMMENT-012 | `AssertRuleResult` | pass |
| existing `SwitchSectionLineStartTests.SectionBehindMultiLineCommentTailMovesAndSectionBehindLineCommentStays` (item 6) — tag added | FMT-BREAK-033, SHARED-COMMENT-012 | `AssertRuleResult` | pass, assertion unchanged |
| existing `SwitchSectionLineStartTests.SectionBehindBreakAndMultiLineCommentTailGetsBlankLineInOneRun` (item 7) — tag added | FMT-BREAK-033, FMT-BLANK-004, SHARED-COMMENT-012 | `AssertRuleResult` | pass, assertion unchanged |
| existing `SwitchSectionLineStartTests.LabelBehindMultiLineCommentTailIsMoved` (item 8) — tag added | FMT-BREAK-034, SHARED-COMMENT-012 | `AssertRuleResult` | pass, assertion unchanged |

The seven existing `StatementBehindMultiLineCommentTests` tests (the split side with a blank-line requirement) were already tagged FMT-BREAK-006 and SHARED-COMMENT-012 and are reused unchanged; only the class summary is widened to name the "stays" side. The FMT-BREAK-002 and FMT-BREAK-035 clarifications only point to FMT-BREAK-006/033 and carry no tag of their own here.

Each condition of the decided behavior has a test on each side (FC-003): where the comment starts (behind the previous token vs. on its own line) for statements, first statements of a block, first statements of a section, later sections, the first section, and labels; and, for statements behind a comment that starts behind code, with vs. without a blank-line requirement.

## Scenarios (bug fix)

Not a bug-fix lane.

## Not covered, and why

- **Watching tests fail.** None fail on head: the decided output is `main`'s and head's (answer 2 and decision 38 change no output). They guard that the run's later changes keep it.
- **Directive in the gap.** The requirements decide a gap behind a multi-line comment; a directive ends its line, so a token behind it starts its line (SHARED-BLANK-008) and the case is the existing `SectionBehindDirectiveKeepsItsLine`, not this condition.
- **CRLF and second pass.** Not separate tests: `AssertRuleResult` runs every row under LF and CRLF and asserts the second pass.
- **Class members behind a comment tail** (decision 39). Out of the run; no test, since a test would decide the follow-up's output.
- **Analyzer counterpart.** SHARED-COMMENT-012's analyzer clause is unchanged by answer 2; no analyzer test touched.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~StatementBehindMultiLineCommentTests|FullyQualifiedName~SwitchSectionBehindMultiLineCommentTests|FullyQualifiedName~SwitchSectionLineStartTests"
scripts/test.sh --project architecture
```

On 44d31af plus these changes: formatter filter 61 passed, 0 failed; architecture 92 passed. `scripts/format.sh` over the three test files formatted 1 of 3 (the new file).
