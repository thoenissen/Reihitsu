Role: spec author
Commit: 8228cb3
Verdict: spec written

This round applies `squad/decisions/2026-10-20-997-shared-prefix-collision.md`. It renames only this run's IDs; `main`'s SHARED-EMPTY and SHARED-BRACE requirements are untouched. No test file was touched.

## Changed requirements

- `shared/empty-parentheses.md`: the prefix SHARED-EMPTY becomes **SHARED-PARENS**.
  - SHARED-EMPTY-001 becomes **SHARED-PARENS-001** (the diagnostic covers the empty list).
  - SHARED-EMPTY-002 becomes **SHARED-PARENS-002** (the fix removes the empty list).
- `shared/blank-line-after-brace-fixes.md`: the prefix SHARED-BRACE becomes **SHARED-AFTERBRACE**.
  - SHARED-BRACE-001 becomes **SHARED-AFTERBRACE-001** (the blank line goes directly before the next statement).
- Wording and behavior are unchanged.
- References updated:
  - RH3106 and RH3107: `Uses` and intro;
  - RH3208: `Uses` and RH3208-002;
  - RH5030: `Uses`, intro, and the retirement line of RH5030-003.

  No other spec of this run referenced the old IDs.
- Index: the two rows now show the new prefixes.
- Ledger: SHARED-AFTERBRACE-001, SHARED-PARENS-001, and SHARED-PARENS-002 are inserted in sorted order. SHARED-EMPTY-001/-002 and SHARED-BRACE-001 stay, as `main`'s lines. No line was removed.

## Checks

`scripts/test.sh --project architecture`: 71/72. The duplicate-ID failure is gone. The one failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, for SHARED-AFTERBRACE-001, SHARED-PARENS-001, and SHARED-PARENS-002, until the tests are re-tagged.

No check flags the old tags below: they now name `main`'s active requirements of other specs, so they are wrong but pass. Every one of them must change.

## Tags to change (test implementer)

In `src/Reihitsu.Analyzer.Test/`:

- `Design/RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzerTests.cs`
  - `VerifyDiagnosticAndCodeFix`: SHARED-EMPTY-001 → SHARED-PARENS-001, SHARED-EMPTY-002 → SHARED-PARENS-002
  - `EmptyParameterListsOfAnonymousMethodsAreReportedOverTheParentheses`: SHARED-EMPTY-001 → SHARED-PARENS-001
  - `FixRemovesTheEmptyParameterList`: SHARED-EMPTY-002 → SHARED-PARENS-002
- `Design/RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemovedAnalyzerTests.cs`
  - `VerifyDiagnosticsAndCodeFix`: SHARED-EMPTY-001 → SHARED-PARENS-001, SHARED-EMPTY-002 → SHARED-PARENS-002
  - `EmptyArgumentListsOfAttributesAreReportedOverTheParentheses`: SHARED-EMPTY-001 → SHARED-PARENS-001
  - `FixRemovesTheEmptyArgumentListAndKeepsACommentInFrontOfIt`: SHARED-EMPTY-002 → SHARED-PARENS-002
- `Formatting/RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzerTests.cs`, SHARED-BRACE-001 → SHARED-AFTERBRACE-001 in:
  - `VerifyStatementDirectlyAfterLocalFunctionIsSeparatedByBlankLine`
  - `VerifyBlankLineIsInsertedAboveCommentLeadingNextStatement`
  - `VerifyBlankLineIsInsertedAfterDirectiveLeadingNextStatement`
  - `FixInsertsTheBlankLineAboveACommentAndBehindADirective`
- `Formatting/RH5030BlankLineAfterClosingBraceAnalyzerTests.cs`, SHARED-BRACE-001 → SHARED-AFTERBRACE-001 in:
  - `VerifyDiagnosticWhenStatementFollowsIfBlockWithoutBlankLine`
  - `VerifyDiagnosticWhenStatementFollowsWhileBlockWithoutBlankLine`
  - `VerifyDiagnosticWhenStatementFollowsTryBlockWithoutBlankLine`
  - `VerifyDiagnosticWhenStatementInSwitchSectionFollowsBlockWithoutBlankLine`
  - `VerifyDiagnosticWhenStatementFollowsEndRegionDirectiveWithoutBlankLine`
  - `VerifyDiagnosticAndFixForBreakAfterClosingBraceInsideBracedSwitchSection`
  - `VerifyDiagnosticWhenCommentLineSeparatesClosingBraceFromNextStatement`
  - `VerifyDiagnosticAndFixForBreakStatementAfterClosingBraceOutsideSwitchSection`
  - `VerifyDiagnosticAndFixForNonTerminalDirectBreakAfterClosingBrace`
  - `VerifyFixInsertsBlankLineDirectlyBeforeNextStatement`
  - `VerifyFixInsertsCarriageReturnLineFeedBlankLineBeforeNextStatement`

Do not touch the SHARED-EMPTY-00x tags in the RH5412–RH5416 tests or the SHARED-BRACE-00x tags in the RH5405–RH5407 tests. They belong to `main`.

## Open questions

No new questions. Filed issues and earlier handovers still cite the old IDs:

- RH3106 Q1 and the `shared/empty-parentheses.md` issue (Q1, Q2) cite SHARED-EMPTY-002, which is now SHARED-PARENS-002.
- Handovers 02–13 cite SHARED-BRACE-001 and SHARED-EMPTY-00x under their old names.
