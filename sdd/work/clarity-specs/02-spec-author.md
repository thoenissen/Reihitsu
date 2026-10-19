Role: spec author
Commit: db09acc
Verdict: spec written

Revision after the maintainer's answers (`squad/decisions/2026-10-19-997-*.md`, summarized at the end of `00-lead.md`). `01-spec-author.md` stays as it was; this file lists only the delta.

## Changed requirements

- RH3202-002 — A method the formatter cannot convert is not reported — retired (replaced by SHARED-BODY-001).
- RH3202-003 — The diagnostic covers the expression body — retired (replaced by SHARED-BODY-002).
- RH3202-004 — The fix converts the method as the formatter does — retired (replaced by SHARED-BODY-003).
- RH3202-001 — clarified: its exception now reads "unless SHARED-BODY-001 applies" instead of naming the retired RH3202-002; same behavior. RH3202-005 is unchanged (decision 4).
- RH3202 header and intro: `Uses` gains SHARED-BODY-001..003; the intro names them. The existing FMT entries stay, because the retired requirements' text still references them.
- SHARED-BODY (`shared/expression-body-conversion.md`): its scope now includes RH3202 (methods); the non-goal that excluded methods is gone. No requirement text changed.
- SHARED-BRACE-001 — The blank line goes directly before the next statement — added, in the new `sdd/specs/shared/blank-line-after-brace-fixes.md`. Its code owner is `BlankLineCodeFixUtilities` and it uses SHARED-BLANK-003 and FMT-BLANK-005. The text is RH5030-003's text, unchanged; there is one RH5030 example (RH5030-003's) and one RH3208 example (the comment and directive cases).
- RH5030-003 — The fix inserts the blank line directly before the next statement — retired (replaced by SHARED-BRACE-001). RH5030's `Uses` gains SHARED-BRACE-001, and its intro names it.
- RH3208-002 — changed (new in this PR, so it is not retired): it now says the blank line goes "where SHARED-BRACE-001 puts it" instead of restating the placement. Its example keeps the plain next-statement, terminal `break`, and labeled cases; the comment and directive cases moved to SHARED-BRACE-001. `Uses` gains SHARED-BRACE-001.
- RH3201: unchanged (decision 3).
- Ledger: SHARED-BRACE-001 added in sorted order. Index: new row for `shared/blank-line-after-brace-fixes.md`.

Verification: the examples of SHARED-BRACE-001, RH3208, and SHARED-BODY were re-run through `scripts/apply-fix.sh` with the extractor from 01: 9 cases, 0 mismatches. `SpecConsistencyTests` and `SpecStructureTests` pass. As expected, two checks fail until the tests are re-tagged:

- `EveryRequirementOfAnEnforcedSpecIsCovered` (the 78 new IDs);
- `EveryRequirementReferenceNamesAnActiveRequirement` (the tags of the retired IDs listed below).

## Examples for the tests — re-tagging

Retired IDs that tests carry today, all in `src/Reihitsu.Analyzer.Test/Formatting/`:

- `RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests` (other tags in brackets):
  - `VerifyExpressionBodiedMethodsAreDetectedAndFixed` [RH3202-001] — RH3202-003, RH3202-004 → SHARED-BODY-002, SHARED-BODY-003
  - `VerifyThrowExpressionBodiedMethodIsFixedToThrowStatement` — RH3202-004 → SHARED-BODY-003
  - `VerifyAsyncValueTaskExpressionBodiedMethodIsFixed` [RH3202-001] — RH3202-004 → SHARED-BODY-003
  - `VerifyExpressionBodyWithDirectiveBeforeExpressionIsNotReported` — RH3202-002 → SHARED-BODY-001
  - `VerifyExpressionBodyWithDirectiveAfterExpressionIsFixed` — RH3202-002, RH3202-004 → SHARED-BODY-001, SHARED-BODY-003
  - `VerifyExpressionOnNextLineIsFixedToSingleSpacedReturn` — RH3202-004 → SHARED-BODY-003
- `RH5030BlankLineAfterClosingBraceAnalyzerTests`, tagged RH5030-003 → SHARED-BRACE-001:
  - `VerifyDiagnosticWhenStatementFollowsIfBlockWithoutBlankLine`, `VerifyDiagnosticWhenStatementFollowsWhileBlockWithoutBlankLine`, `VerifyDiagnosticWhenStatementFollowsTryBlockWithoutBlankLine`, `VerifyDiagnosticWhenStatementInSwitchSectionFollowsBlockWithoutBlankLine`, `VerifyDiagnosticWhenStatementFollowsEndRegionDirectiveWithoutBlankLine`, `VerifyDiagnosticAndFixForBreakAfterClosingBraceInsideBracedSwitchSection`, `VerifyDiagnosticWhenCommentLineSeparatesClosingBraceFromNextStatement`, `VerifyDiagnosticAndFixForBreakStatementAfterClosingBraceOutsideSwitchSection`, `VerifyDiagnosticAndFixForNonTerminalDirectBreakAfterClosingBrace`, `VerifyFixInsertsBlankLineDirectlyBeforeNextStatement`, `VerifyFixInsertsCarriageReturnLineFeedBlankLineBeforeNextStatement`.
- The corresponding RH3208 tests in `RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzerTests`, none tagged today, exercise the same placement:
  - `VerifyBlankLineIsInsertedAboveCommentLeadingNextStatement` and `VerifyBlankLineIsInsertedAfterDirectiveLeadingNextStatement` → SHARED-BRACE-001 (and RH3208-002);
  - `VerifyStatementDirectlyAfterLocalFunctionIsSeparatedByBlankLine`, `VerifyStatementDirectlyAfterLocalFunctionInSwitchSectionIsSeparatedByBlankLine`, `VerifyStatementDirectlyAfterLabeledLocalFunctionIsSeparatedByBlankLine`, `VerifyStatementDirectlyAfterNestedLabeledLocalFunctionIsSeparatedByBlankLine` → RH3208-002, with SHARED-BRACE-001 where the test checks where the line goes.

## Open questions to file

In addition to the 14 questions in `01-spec-author.md`:

### `SDD open questions: sdd/specs/analyzer/RH3202.md` (new)

**Q1.** No principle marking: P9 and P10 point in opposite directions here (see below). Input:

```csharp
class C
{
    private int _value;


    public int GetValue() => _value;
}
```

Real output of the RH3202 fix (`scripts/apply-fix.sh RH3202`, LF and CRLF alike):

```csharp
class C
{
    private int _value;

    public int GetValue()
    {
        return _value;
    }
}
```

Why it looks wrong: RH3202-005 says the fix keeps the blank lines above and below the method, but the two blank lines above it collapse to one. The fix formats the method node, and the formatter collapses the excess blank line in the method's leading trivia. Competing readings:

- (a) A bug against RH3202-005 (and P9: the layout around the fixed code stays). The fix should keep both blank lines.
- (b) Intended. The blank lines are the formatted member's own leading trivia (P10: fix output laid out as the formatter does). RH3202-005 is clarified to "keeps the separation", and the result stays as shown.

The same holds for the other SHARED-BODY rules (verified for RH3203). Touches RH3202-005 and SHARED-BODY-003.

## Decisions needed

_None._
