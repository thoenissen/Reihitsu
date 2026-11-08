Role: test implementer
Commit: f574f97
Verdict: tests written

Lane: behavior change. Expected values come from the spec examples and requirements (FMT-BLANK-004/-006/-016, FMT-SWITCH-004/-006, FMT-REGION-002/-004/-007 at f574f97) and the maintainer answers 1A and 2A in `00-lead.md`; none were taken from a patched copy of the code. The prompt carried no intended fix or hoped-for verdict. Every test below was run at f574f97 with production code unchanged; "Expected now" is that observation, and every red test was checked to fail on the specified difference only (the assertion's `expected near` / `actual near` shows nothing but the misplaced blank line, the added braces, or the gap after `#`). The whole formatter project at f574f97 plus these tests: 34 failed, 4699 passed; the 34 are exactly the red tests listed here.

`AssertRuleResult` runs the full pipeline under LF and CRLF, checks a second pass and phase stability, so every full-pipeline test below covers CRLF and idempotency (FC-010).

## thoenissen/Reihitsu#1385

### Tests

New file `src/Reihitsu.Formatter.Test/Regression/BlankLines/RequiredBlankLineBehindDirectiveTests.cs` (class `RequiredBlankLineBehindDirectiveTests`), helper `FormatterTestsBase.AssertRuleResult` unless stated.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `BlankLineBeforeIfGoesBehindEndIfOfDisabledGroup` | FMT-BLANK-016, FMT-BLANK-001 (spec example; skeptic 01) | AssertRuleResult | fail: blank line in front of `#if NEVER` |
| `BlankLineBeforeIfGoesBehindEndIfOfActiveGroup` | FMT-BLANK-016, FMT-BLANK-001 (skeptic 02) | AssertRuleResult | fail: blank line in front of `#endif` |
| `BlankLineBeforeReturnGoesBehindPragmaInFrontOfComment` | FMT-BLANK-016, FMT-BLANK-001 (skeptic 03) | AssertRuleResult | fail: blank in front of `#pragma` |
| `BlankLineBeforeLocalDeclarationGoesBehindPragmaInFrontOfComment` | FMT-BLANK-016, FMT-BLANK-002 (skeptic 04) | AssertRuleResult | fail: same |
| `BlankLineBehindLocalDeclarationGoesBehindPragmaInFrontOfComment` | FMT-BLANK-016, FMT-BLANK-002 (skeptic 05) | AssertRuleResult | fail: same |
| `BlankLineBeforeYieldGoesBehindPragmaInFrontOfComment` | FMT-BLANK-016, FMT-BLANK-003 (skeptic 06) | AssertRuleResult | fail: same |
| `BlankLineBehindBreakGoesBehindPragmaInFrontOfComment` | FMT-BLANK-016, FMT-BLANK-004 (skeptic 07) | AssertRuleResult | fail: same |
| `BlankLineBeforeBreakGoesBehindPragmaInFrontOfComment` | FMT-BLANK-016, FMT-BLANK-004 (skeptic 08) | AssertRuleResult | fail: same |
| `BlankLinesBeforeDeclarationAndBehindBreakGoBehindIfDirective` | FMT-BLANK-016, FMT-BLANK-002, FMT-BLANK-004, FMT-BLANK-006 (spec example; skeptic 23) | AssertRuleResult | fail: blank in front of the first `#if !NEVER` |
| `BlankLineBeforeIfGoesBehindIfDirectiveBehindTrailingComment` | FMT-BLANK-016, FMT-BLANK-001 (skeptic 16) | AssertRuleResult | fail: blank in front of `#if` |
| `BlankLineBeforeIfGoesBehindLastOfTwoDirectivesWithComments` | FMT-BLANK-016, FMT-BLANK-001 (skeptic 21) | AssertRuleResult | fail: blank in front of `#if` |
| `AuthorBlankLineInFrontOfDirectiveIsKeptAndNotDoubled` | FMT-BLANK-001 (skeptic 20) | AssertRuleResult (unchanged) | pass |
| `BlankLineInFrontOfLeadingCommentIsNotDoubledBehindDirective` | FMT-BLANK-011, FMT-BLANK-001 (skeptic 15) | AssertRuleResult | pass |
| `DirectiveDirectlyBeforeIfBehindCommentGetsNoStatementBlankLine` | FMT-BLANK-006, FMT-BLANK-011 (skeptic 22) | AssertRuleResult | pass |
| `BlockStatementDirectlyBehindIfDirectiveBehindBreakGetsNoBlankLine` | FMT-BLANK-006, FMT-BLANK-004 (skeptic 30) | AssertRuleResult (unchanged) | pass |
| `BlankLineBehindLastDirectiveIsLeftUnchanged` | FMT-BLANK-016 (skeptic 31/32; FC-010 fixed point) | AssertRuleResult (unchanged) | pass |
| `DirectivesDirectlyInFrontOfSwitchSectionsLeaveTheSwitchUnchanged` | FMT-BLANK-006, FMT-BLANK-004 (spec example; change doc) | AssertRuleResult (unchanged) | fail: blank in front of `#pragma` |
| `PragmaDirectlyInFrontOfSwitchSectionGetsNoBlankLine` | FMT-BLANK-006, FMT-BLANK-004 (change doc input 1; skeptic 10) | AssertRuleResult (unchanged) | fail: same |
| `DisabledConditionalGroupDirectlyInFrontOfSwitchSectionGetsNoBlankLine` | FMT-BLANK-006, FMT-BLANK-004 (change doc input 2; skeptic 12) | AssertRuleResult (unchanged) | fail: blank in front of `#if NEVER` |
| `ActiveIfDirectiveDirectlyInFrontOfSwitchSectionGetsNoBlankLine` | FMT-BLANK-006, FMT-BLANK-004 | AssertRuleResult (unchanged) | fail: blank in front of `#if !NEVER` |
| `SwitchSectionBlankLineGoesBehindPragmaInFrontOfComment` | FMT-BLANK-006, FMT-BLANK-016, FMT-BLANK-004 (spec example; skeptic 11) | AssertRuleResult | fail: blank in front of `#pragma` |
| `SwitchSectionBlankLineGoesBehindEndIfOfDisabledGroupInFrontOfComment` | FMT-BLANK-016, FMT-BLANK-004 (skeptic 25) | AssertRuleResult | fail: blank in front of `#if NEVER` |
| `SwitchSectionBehindReturnSectionWithDirectiveAndCommentGetsNoBlankLine` | FMT-BLANK-004 (skeptic 24) | AssertRuleResult (unchanged) | pass |
| `SwitchSectionWithoutDirectiveBehindBreakSectionGetsBlankLine` | FMT-BLANK-004 (FC-003: no directive in the gap) | AssertRuleResult | pass |

Changed existing tests in `src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterNodeBlankLineTests.cs` (they pinned the output the spec now forbids; the skeptic and spec author named both):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `InsertsNoBlankLineAboveSwitchSectionDirectlyPrecededByDirectiveAfterBreakSection` (was `InsertsBlankLineAboveDirectiveOfSwitchSectionAfterBreakSection`; expected is now the input, no blank line above `#if !DEBUG`) | FMT-BLANK-006 | `AssertFormatsTarget` (`FormatNodeInDocumentAsync`, target = last switch section) | fail: blank in front of `#if !DEBUG` |
| `InsertsBlankLineBehindDirectiveOfCommentedBlockStatementAfterBreak` (was `InsertsBlankLineAboveDirectiveOfCommentedBlockStatementAfterBreak`; blank line moved from above `#if !DEBUG` to between it and `// Scope`) | FMT-BLANK-016 | `AssertFormatsTarget` (target = last block) | fail: blank in front of `#if !DEBUG` |

New analyzer-parity test in `src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH5011BreakStatementsShouldBeFollowedByABlankLineFormatterTests.cs`:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyFormatterKeepsSwitchSectionDirectlyPrecededByDirectiveWithoutBlankLine` | FMT-BLANK-006 (FC-013 parity with RH5011-002) | `FormatterTestsBase<RH5011…>.VerifyFormatter(source)` | fail: blank in front of `#pragma` |

### Not covered, and why
- FMT-BLANK-005 with directive and comment (skeptic 09) — already covered by `BlankLineRequirementTests.BlankLineAfterClosingBraceGoesBetweenDirectiveAndComment` and `…GoesBehindLastDirectiveOfConditionalGroup` (both pass); not duplicated.
- `#region` in the gap (skeptic 13/14) — FMT-BLANK-013, out of the run's scope; existing `BlankLineRegionTests`/`RegionDirectiveLineAboveTests` cover it.
- FC-028 (detached vs document node formatting) — skeptic found no caller that formats a statement whose own leading trivia carries a directive with `FormatNode`; the node-level document path is covered by the two rewritten `ReihitsuFormatterNodeBlankLineTests`.
- FC-007/008/009/018/030 (code fixes) — the RH5002/RH5008 fix part moved to #1447; no code fix is in scope.
- FC-004/FC-005/FC-021 (relocated trivia) — the change only moves an inserted blank line; every test asserts exact output, so a dropped or moved directive or comment fails them.
- FC-022–FC-026, FC-029, FC-036 — no literal, identifier, member-kind, token re-resolution, key/value layout, or rename is involved.

## thoenissen/Reihitsu#1398

### Tests

`src/Reihitsu.Formatter.Test/Regression/SwitchCaseBraces/SwitchCaseBraceRequirementTests.cs` (full pipeline) and `src/Reihitsu.Formatter.Test/Unit/SwitchCaseBraces/SwitchCaseBraceRewriterTests.cs` (phase only):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `BreakOnlySectionWithMultiLinePropertyPatternDoesNotMakeTheSwitchMultiLine` | FMT-SWITCH-004, FMT-SWITCH-006 (spec example) | AssertRuleResult (unchanged) | fail: `B();` wrapped in `{ }` |
| `BreakOnlySectionWithMultiLinePropertyPatternStaysBareWhenAnotherSectionBracesTheSwitch` | FMT-SWITCH-004, FMT-SWITCH-006 (spec example; FC-003 other side) | AssertRuleResult | pass |
| `BreakOnlySectionWithMultiLineListPatternDoesNotMakeTheSwitchMultiLine` | FMT-SWITCH-004, FMT-SWITCH-006 (skeptic matrix: list) | AssertRuleResult (unchanged) | fail: braces |
| `BreakOnlySectionWithMultiLinePatternInWhenClauseDoesNotMakeTheSwitchMultiLine` | FMT-SWITCH-004, FMT-SWITCH-006 (skeptic: `when` clause, not run by the skeptic) | AssertRuleResult (unchanged) | fail: braces |
| `BreakOnlySectionWithStackedLabelsAndMultiLinePatternDoesNotMakeTheSwitchMultiLine` | FMT-SWITCH-004, FMT-SWITCH-006 (FC-027 stacked labels) | AssertRuleResult (unchanged) | fail: braces |
| `SectionOfTwoBreaksWithMultiLinePatternDoesNotMakeTheSwitchMultiLine` | FMT-SWITCH-004, FMT-SWITCH-006 (FC-003 `break` count 2) | AssertRuleResult (unchanged) | fail: braces |
| `BreakOnlySectionWithMultiLineParenthesizedPatternDoesNotAddBraces` | FMT-SWITCH-004, FMT-SWITCH-006 (parenthesized) | `SwitchCaseBracePhase` alone, `Assert.AreEqual(input, actual)` | fail: braces around `B();` |
| `SectionWithBodyAndMultiLineParenthesizedPatternAddsBraces` | FMT-SWITCH-004 (FC-003 other side) | phase alone; syntax check: every section is block + `break` | pass |

Existing tests that stay valid: `MultiLinePropertyPatternInLabelMakesTheSectionMultiLine` (section with a body, braces the switch), `BreakOnlySectionDoesNotMakeTheSwitchMultiLine`, `WrappedWhenClauseDoesNotMakeTheSectionMultiLine`.

Label layout note: the list-pattern and `when`-clause inputs are written in the column layout the formatter produces today for those labels (FMT-BREAK/indentation requirements, not in question); the tests assert that layout unchanged together with the absence of braces. The parenthesized case is tested at phase level only, because its full-pipeline label layout is the pre-existing defect the skeptic listed as follow-up 1 (`case⏎(⏎Point …`); a full-pipeline test would pin that broken layout.

### Not covered, and why
- `return`-/`throw`-only section with a multi-line label — FMT-SWITCH-006 exempts `break` only; spec-decided today and listed as follow-up 2 in `00-lead.md`; not pinned so the follow-up stays open.
- FC-020 directives in a section — guarded by `SectionCarriesDirectives`, unchanged by the run; existing tests cover it.
- Code-fix, naming, literal classes — not involved.

## thoenissen/Reihitsu#1239

### Tests

`src/Reihitsu.Formatter.Test/Regression/Regions/RegionLineFormTests.cs`, new region `Whitespace between the number sign and the keyword`:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `WhitespaceAfterNumberSignOfDescribedRegionPairIsRemoved` | FMT-REGION-007, FMT-REGION-002 (spec example) | AssertRuleResult | fail: `# region Fields` / `#  endregion // Fields` |
| `WhitespaceAfterNumberSignIsRemovedWhenNothingElseChanges` | FMT-REGION-007, FMT-REGION-002 (FC-003: line canonical except the gap) | AssertRuleResult | fail: gap kept on both lines |
| `TabAfterNumberSignOfRegionLineIsRemoved` | FMT-REGION-007 (spec example) | AssertRuleResult | fail: `#    region Fields` |
| `TabAfterNumberSignOfEndRegionLineIsRemoved` | FMT-REGION-002 | AssertRuleResult | fail: `#    endregion // Fields` |
| `WhitespaceAfterNumberSignOfRegionPairInDisabledBranchIsRemoved` | FMT-REGION-007, FMT-REGION-002 (spec example; FC-020) | AssertRuleResult | fail: gap kept |
| `WhitespaceAfterNumberSignOfUndescribedRegionPairWithEndRegionTextIsRemoved` | FMT-REGION-004 (spec example) | AssertRuleResult | fail: `# region` / `#  endregion note` |
| `WhitespaceAfterNumberSignOfBareRegionPairIsRemoved` | FMT-REGION-004 | AssertRuleResult | fail: gap kept |
| `WhitespaceAfterNumberSignOfUndescribedRegionPairInDisabledBranchIsRemoved` | FMT-REGION-004 (FC-020) | AssertRuleResult | fail: gap kept |
| `WhitespaceAfterNumberSignOfNestedRegionPairsIsRemoved` | FMT-REGION-007, FMT-REGION-002 (FC-027 nesting) | AssertRuleResult | fail: gap kept |

Characterization for Decision 2 (RH7301 fix output unchanged), in `src/Reihitsu.Analyzer.Test/Formatting/RH7301RegionsShouldMatchAnalyzerTests.cs`:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RegionFixKeepsWhitespaceBetweenNumberSignAndEndRegionKeyword` (`#  endregion // Wrong` → `#  endregion // Methods`) | none (characterization of `main`'s fix output; guards the shared `RegionDirectiveUtilities.CreateCanonicalEndRegionTrivia`) | `Verify` on `BatchCodeFixTestsBase` | pass |

Existing tests that stay valid: `ReihitsuFormatterNodeRegionTests.FormatNodeInDocumentAsyncReplacesTabBetweenHashAndRegionKeyword` (node-level skips the region phase), and every canonical-line test in `RegionLineFormTests` (the other side of the condition: no gap, line untouched).

### Not covered, and why
- Other directives (`# pragma`, `# if`) — out of scope (follow-up candidate in `00-lead.md`); not pinned so the follow-up stays open.
- Unpaired region with a gap — an unpaired `#region` is a syntax error, and the formatter leaves syntax-invalid files unchanged (FMT-PIPE-005); existing coverage, not duplicated.
- RH7302 — tracked in #1444, not involved.

## Whole run

### Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~RequiredBlankLineBehindDirectiveTests|FullyQualifiedName~ReihitsuFormatterNodeBlankLineTests|FullyQualifiedName~SwitchCaseBraceRequirementTests|FullyQualifiedName~SwitchCaseBraceRewriterTests|FullyQualifiedName~RegionLineFormTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5011BreakStatementsShouldBeFollowedByABlankLineFormatterTests|FullyQualifiedName~RH7301RegionsShouldMatchAnalyzerTests"
```

Results at f574f97: formatter filter 34 failed / 183 passed; analyzer RH5011 formatter tests 1 failed / 2 passed; RH7301 25 passed. Whole formatter project: 34 failed (all listed above as `fail`), 4699 passed.

All test files were formatted with `scripts/format.sh`.
