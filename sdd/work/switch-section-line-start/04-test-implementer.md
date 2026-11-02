Role: test implementer
Commit: 2bb5511
Verdict: tests written

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Behavior-change lane: tests derived from the spec examples (FMT-BREAK-033/034/035, FMT-BLANK-004), the change doc's visible output changes 1 to 6, the skeptic's candidates and class matrix, and lead decisions 5, 6, and 8. Expected values come from the spec and change-doc examples; where a shape's layout outside the changed lines had to be known (braced sections, a block as section body, a lambda's column), it was taken from `main`'s output for the same code with every section, label, and first statement already on its own line, which this change does not touch.

All new tests run on 2bb5511 (production code identical to `origin/main`): 32 fail, 8 pass. Every failure was inspected and fails on the difference the spec decides (details per test below). No production code touched.

## Tests

Files:
- `src/Reihitsu.Formatter.Test/Regression/LineBreaks/SwitchSectionLineStartTests.cs` (new, `FormatterTestsBase.AssertRuleResult`: exact output, LF and CRLF, second pass, per-phase stability check)
- `src/Reihitsu.Formatter.Test/Unit/LineBreaks/LineBreakPhaseSwitchSectionTests.cs` (new, `LineBreakPhase` run alone, LF and CRLF)
- `src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterNodeSwitchSectionTests.cs` (new, `FormatNodeInDocumentAsync`, LF and CRLF, second pass)
- `src/Reihitsu.Analyzer.Test/Formatting/RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzerTests.cs`, `src/Reihitsu.Analyzer.Test/Formatting/RH5403StatementMustNotBeOnSingleLineAnalyzerTests.cs` (one test each appended, `Verify` under LF and CRLF: one pass, re-analysis)

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `SwitchSectionLineStartTests.SectionsOnOneLineBehindBreakGetTheirLinesAndBlankLineInOneRun` (issue input 1, change-doc entry 2) | FMT-BREAK-033, FMT-BREAK-035, FMT-BLANK-004; FC-010, FC-017 | AssertRuleResult | Fails: `case 1: A();` / `case 2: B();` stay on label lines, no blank line in pass 1 |
| `...SectionsOnOneLineBehindReturnStartTheirOwnLines` (issue input 2, entry 1) | FMT-BREAK-033, FMT-BREAK-035 | AssertRuleResult | Fails: `case 1: return 1; case 2: return 2;` on one line |
| `...SectionsBehindReturnGotoCaseAndThrowStartTheirOwnLines` (FMT-BREAK-033 example 1) | FMT-BREAK-033; FC-021 | AssertRuleResult | Fails: `return 1; case 2:` kept |
| `...SectionBehindMultiLineCommentTailMovesAndSectionBehindLineCommentStays` (example 2) | FMT-BREAK-033; FC-003, FC-021 | AssertRuleResult | Fails: `b */ case 2:` kept |
| `...DelimitedDocumentationCommentInFrontOfSectionStaysInFrontOfTheLabel` (example 3) | FMT-BREAK-033 | AssertRuleResult | Passes (output unchanged by the change) |
| `...SectionsBehindContinueAndGotoDefaultStartTheirOwnLines` | FMT-BREAK-033, FMT-BREAK-035; FC-016 | AssertRuleResult | Fails: all sections on one line |
| `...SectionBehindBreakAndDelimitedCommentGetsBlankLineInOneRun` | FMT-BREAK-033, FMT-BLANK-004; FC-019, FC-021 | AssertRuleResult | Fails: no blank line in pass 1 |
| `...SectionBehindBreakAndMultiLineCommentTailGetsBlankLineInOneRun` | FMT-BREAK-033, FMT-BLANK-004; FC-003, FC-021 | AssertRuleResult | Fails: `case 2:` on its own line but no blank line in pass 1 |
| `...SectionOnLineOfBreakGetsBlankLineInSameRun` (FMT-BLANK-004 new example) | FMT-BLANK-004; FC-010 | AssertRuleResult | Fails: no blank line in pass 1 |
| `...SectionBehindBreakWithLineCommentKeepsItsLineAndGetsBlankLine` | FMT-BREAK-033, FMT-BLANK-004; FC-003 | AssertRuleResult | Passes (other side of the condition) |
| `...SectionBehindDirectiveKeepsItsLine` | FMT-BREAK-033, FMT-BREAK-035; FC-019, FC-020 | AssertRuleResult | Fails: `return 1;`/`return 2;` stay on label lines |
| `...LabelsSharingALineAreStacked` (FMT-BREAK-034 example) | FMT-BREAK-034; FC-021 | AssertRuleResult | Fails: labels stay on one line |
| `...DefaultLabelFollowedByCaseLabelIsStacked` | FMT-BREAK-034, FMT-BREAK-035; FC-016 | AssertRuleResult | Fails: unchanged |
| `...LabelBehindMultiLineCommentTailIsMoved` | FMT-BREAK-034; FC-003, FC-021 | AssertRuleResult | Fails: `b */ case 4:` kept |
| `...LabelBehindLineCommentKeepsItsLine` | FMT-BREAK-034; FC-003 | AssertRuleResult | Passes |
| `...FirstStatementsStartTheirOwnLines` (FMT-BREAK-035 main example) | FMT-BREAK-035 | AssertRuleResult | Fails: first statements stay on label lines |
| `...FirstStatementsMoveBehindPlainAndGuardedLabels` (entry 3) | FMT-BREAK-035; FC-021 | AssertRuleResult | Fails: unchanged |
| `...DelimitedDocumentationCommentMovesWithFirstStatementAndLabel` (entry 6) | FMT-BREAK-034, FMT-BREAK-035 | AssertRuleResult | Fails: unchanged |
| `...DelimitedDocumentationCommentStartingItsLineIsKept` (entry 6 target shape as input) | FMT-BREAK-033, FMT-BREAK-034, FMT-BREAK-035; FC-003 | AssertRuleResult | Passes |
| `...FirstStatementBehindGuardOnItsOwnLineStartsItsOwnLine` | FMT-BREAK-035; FC-027 | AssertRuleResult | Fails: `when n > 0: A();` kept |
| `...LocalDeclarationAndLabeledStatementAsFirstStatementStartTheirOwnLines` | FMT-BREAK-035; FC-016 | AssertRuleResult | Fails: unchanged |
| `...BlockAsFirstStatementStartsItsOwnLine` | FMT-BREAK-035 | AssertRuleResult | Passes (FMT-BREAK-008 already places it) |
| `...SectionBehindBlockBodyStartsItsOwnLineWithoutBlankLine` | FMT-BREAK-033, FMT-BLANK-004 | AssertRuleResult | Fails: `} case 2:` kept |
| `...EmptyFirstStatementBehindStackedLabelsStaysOnTheLabelsLine` | FMT-BREAK-034, FMT-BREAK-035; FC-003 | AssertRuleResult | Fails: `case 2: case 3:; break;` kept on one line (the `case 3:;` part is expected and kept, per maintainer answer 5) |
| `...BracedSectionEndingInBreakGetsOneBlankLineInOneRun` (skeptic cell, decisions 5/8) | FMT-BREAK-033, FMT-BREAK-035, FMT-BLANK-004; FC-010, FC-012 | AssertRuleResult | Fails: blank line in front of the switch's `}` in pass 1 (the blank line before `case 2:` is already right on `main`) |
| `...BracedSectionEndingInReturnGetsNoBlankLine` (skeptic cell + `}` shape, decisions 5/6/8) | FMT-BREAK-033, FMT-BREAK-035, FMT-BLANK-004; FC-010, FC-012 | AssertRuleResult | Fails: blank line in front of the switch's `}` in pass 1 |
| `...BracedSectionsOnLineOfClosingBraceGetNoBlankLineBeforeIt` (decision 6 shape) | FMT-BREAK-034, FMT-BLANK-004; FC-010, FC-012 | AssertRuleResult | Fails: `case 1: case 2:` kept on one line (and, on `main`, the blank line before `}`) |
| `...NestedSwitchSectionsStartTheirOwnLines` (braced section without `break` followed by a section on its line) | FMT-BREAK-033, FMT-BREAK-035; FC-012, FC-027 | AssertRuleResult | Fails: inner and outer sections on one line |
| `...SwitchSectionsInsideLambdaStartTheirOwnLines` | FMT-BREAK-033, FMT-BREAK-035; FC-027 | AssertRuleResult | Fails: unchanged |
| `...SwitchSectionsInsideLocalFunctionStartTheirOwnLines` | FMT-BREAK-033, FMT-BREAK-035; FC-027 | AssertRuleResult | Fails: unchanged |
| `...SwitchWhoseSectionsAlreadyStartTheirLinesIsKept` | FMT-BREAK-033, FMT-BREAK-034, FMT-BREAK-035 | AssertRuleResult (no change) | Passes |
| `LineBreakPhaseSwitchSectionTests.EmptyFirstStatementIsLeftAsWritten` (FMT-BREAK-035 phase-alone example) | FMT-BREAK-035 | LineBreakPhase alone | Passes |
| `...SectionBehindReturnStartsItsLineWithoutBlankLine` | FMT-BREAK-033 | LineBreakPhase alone | Fails: no line starts with `case 2:` |
| `...SectionBehindBreakStartsItsLineWithoutBlankLine` (the phase inserts no blank line, FMT-BREAK-030 via FMT-BREAK-033) | FMT-BREAK-033 | LineBreakPhase alone | Fails: no line starts with `case 2:` |
| `...LabelBehindLabelStartsItsLineWithoutBlankLine` | FMT-BREAK-034 | LineBreakPhase alone | Fails: no line starts with `case 2:` |
| `...FirstStatementStartsItsLineWithoutBlankLine` | FMT-BREAK-035 | LineBreakPhase alone | Fails: no line starts with `A();` |
| `ReihitsuFormatterNodeSwitchSectionTests.SwitchStatementTargetGetsItsSectionsOnLinesOfTheirOwn` | FMT-BREAK-033, FMT-BREAK-035; FC-028 | FormatNodeInDocumentAsync | Fails: sections on one line |
| `...FirstStatementTargetOnItsLabelsLineKeepsTheGapInFrontOfIt` (node-level sentence of FMT-BREAK-033, FMT-PIPE-010) | FMT-BREAK-033; FC-003 | FormatNodeInDocumentAsync | Passes |
| `RH5417...Tests.VerifyFixOfABodyHoldingASwitchPutsEverySectionOnItsOwnLine` (entry 5) | FMT-BREAK-033, FMT-BREAK-035; FC-007, FC-030, FC-032 | Verify, LF + CRLF | Fails: fix writes `case 1: return 1; case 2: return 2;` |
| `RH5403...Tests.VerifyFixOfABodyHoldingASwitchPutsEverySectionOnItsOwnLine` (entry 5) | FMT-BREAK-033, FMT-BREAK-035; FC-007, FC-030, FC-032 | Verify, LF + CRLF | Fails: fix writes `case 1: continue; case 2: return;` |

`scripts/test.sh --project architecture`: 92 passed — `RequirementCoverageTests` now finds covering tests for FMT-BREAK-033/034/035, and the naming/tracker conventions hold.

## Scenarios (bug fix)

_N/A_ — behavior change. The issue's two inputs are the first two tests above; both fail on 2bb5511 exactly as the issue's "today's output" (input 1 additionally lacks the blank line in pass 1).

## Not covered, and why

- **RH3208** (`FormatNodeInDocumentAsync` / `FormatNodeInDocumentWithContextAsync`, `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedCodeFixProvider.cs:106,109`): a converted expression-bodied local function holds a switch statement only through a block lambda inside its arrow expression. The switch layout inside is decided by the same pipeline phases the RH5417/RH5403 tests and the node-level test reach; the with-context entry differs only in writing back the target's leading trivia (FMT-PIPE-017), which holds no switch section. One fix test on each of the other two entry calls is kept.
- **Fix All (FC-008)**: the switch layout is not a diagnostic; RH5417/RH5403 Fix All is unchanged by this run and covered by their existing `GetFixAllScenario`.
- **FC-004/FC-005/FC-020 directive variants beyond one `#if`**: the line break is inserted only where the token does not start its line (SHARED-BLANK-008); a directive always ends its line, so every directive kind puts the next token at a line start through the same `StartsLine` fact. One `#if` test covers the arm.
- **`///` behind a label** (`case 3: /// e`): already moved by FMT-DOC-001 on `main` (`case 3:` / `/// e` / `A();`), and the token behind it starts its line; no change is specified there. Not added.
- **FC-006, FC-022–FC-026, FC-029, FC-036, FC-018**: no rewrite of tokens, literals, identifiers, members, or names; only line breaks in front of section/label/statement tokens.
- **FC-011**: no loop-until-stable is specified by the change.
- **FC-013/FC-014**: no rule changes; analyzer-clean code changed by the formatter is intended under P11 (change doc, *Analyzers are unchanged*).
- **FC-031, FC-037**: concern the implementer's comments and text-only proofs, not tests.
- **FMT-BREAK-006** wording change: scope narrowing with an unchanged example; existing `LineBreaksConsolidationStabilityTests` (tagged FMT-BREAK-006) still covers statements behind statements. No new test.
- **FC-032**: the full analyzer project was not rerun here (no compiled change on this head). RH5029/RH5103 fixed codes with `case 0: Method(1);` must be confirmed by the validator after the implementer's change, as the skeptic noted.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~SwitchSectionLineStartTests|FullyQualifiedName~LineBreakPhaseSwitchSectionTests|FullyQualifiedName~ReihitsuFormatterNodeSwitchSectionTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~VerifyFixOfABodyHoldingASwitchPutsEverySectionOnItsOwnLine"
scripts/test.sh --project architecture
```
