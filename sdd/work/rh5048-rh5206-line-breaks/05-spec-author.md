Role: spec author
Commit: ae76d7c (test implementer's handover; analyzer and code-fix sources unchanged since 72b08b0)
Verdict: spec written

Repair round for findings 1 and 2 of `04-test-implementer.md`. The prompt asked for these two items only. It carried no intended fix or verdict.

## How the statements were established

- I re-read `GetIndentation` and `GetPreviousStatement` of `RH5103CodeMustNotContainMultipleStatementsOnOneLineCodeFixProvider`, and the existing RH5103 switch-section tests:
  - `SwitchSectionChainInsideObjectInitializerFixAllKeepsLabelRelativeIndentation`
  - `SwitchSectionChainWithoutAnchorScopeUsesCanonicalIndentationRegardlessOfLabelsOwnColumn`
  - `VerifySplitStatementKeepsSiblingColumnWhenPrecedingLineStartsWithComment`
  - `VerifySplitStatementDoesNotCompensateWhenSwitchLabelIsNotFirstOnItsLine`
  - `VerifySplitStatementCompensatesWhenLineStartsWithAnotherSectionsLabel`
  - `VerifySplitStatementCompensatesWhenCommentPrecedesTheLabelOnItsLine`
  - `VerifySplitStatementDoesNotCompensateWhenLineStartsWithAnEarlierSiblingSectionsStatement`
  - `VerifySplitStatementOverIndentsWhenLabelSpansMultipleLinesAndStatementSharesItsContinuationLine`
- I read the `GetFixAllScenario` overrides of RH5104, RH5110 and RH5204.
- All RH5103 and RH5104 spec examples were run again through the scratch harness (marked spans, fix applied until the rule reports nothing, fix-offered claims). Result: 0 mismatches.
- I probed one more RH5104 input to check the whitespace wording: `N(a /* c */);` becomes `/* c */` / `N(a );`, and `N(a); /* d */  ` keeps the trailing spaces.
- `scripts/test.sh --project architecture`: 72 passed, 0 failed.

## Changed requirements

**RH5103-003 — changed (finding 2).**
- The old sentence said: "shares the line of a `case` label → the section's statement indentation, one level deeper than the label." It was false for a label that spans several lines (thoenissen/Reihitsu#1096 Q1). It was also false for a misindented label: outside an anchor scope the column is the canonical one, inside it the column follows the label.
- New qualifier: in a switch section, when the preceding statement does not start its line and that line is indented as its scope depth requires, the moved statement is indented to the section's statement column (FMT-INDENT-003). This also holds when the line starts with a comment, with another section's label, or with another section's statement.
- The requirement explicitly does not cover a label that spans several lines (#1096, a decided bug, not filed again), a label on the line of the switch's opening brace, or a line indented differently from its scope depth.
- The example now shows each covered shape: label line, own-line predecessor, `/* three */ case 3:`, `case 4: break; case 5:`, and `break; case 7:`.
- `Uses` now lists FMT-INDENT-003.
- The excluded side has no output example, because that behavior is the #1096 bug or open question RH5103 Q4 (#1116).

**RH5104-004 — clarified.**
- The RH5104 Fix All scenario (`GetFixAllScenario`) produces `if (left == right )`. That contradicted "removes the comment together with the whitespace that separates it from the code".
- Corrected to the code's behavior: when code follows the comment on its last line, the spaces and tabs behind it are removed and those in front are kept; otherwise the spaces and tabs in front of it are removed and those behind are kept.
- New example: `P(a /* value */);` becomes `/* value */` / `P(a );`.

**Not changed:**
- No other requirement and no merged shared requirement (SHARED-FIX-002 untouched).
- The RH5103 rule page already documents the limitation in one sentence: "After applying the fix inside a `switch` that is written on a single line, or whose `case` label spans several lines, check the indentation of the moved statement." It stays until #1096 is fixed.

## Examples for the tests

- **Two tags now outside RH5103-003's scope.** These tests are tagged `RH5103-003`, but their input has a misindented label, so they no longer verify RH5103-003. Their other tags may stay. The test implementer should remove the `RH5103-003` tag from:
  - `SwitchSectionChainInsideObjectInitializerFixAllKeepsLabelRelativeIndentation`
  - `SwitchSectionChainWithoutAnchorScopeUsesCanonicalIndentationRegardlessOfLabelsOwnColumn`
- These remaining `RH5103-003` tags stay valid: `VerifyMultipleStatementsOnOneLineAreDetectedAndFixed`, the CRLF test, `ThreeStatementChainFixAllKeepsBlockIndentation`, `TabIndentedBlockChainFixAllPreservesExactIndentation`, both `…KeepsSiblingColumnWhenPrecedingLineStartsWithComment` tests, `VerifySplitStatementCompensatesWhenLineStartsWithAnotherSectionsLabel`, `VerifySplitStatementCompensatesWhenCommentPrecedesTheLabelOnItsLine`, `VerifySplitStatementDoesNotCompensateWhenLineStartsWithAnEarlierSiblingSectionsStatement`, and `VerifyStatementBehindStatementOnCaseLabelLineIsFixedToSectionIndentation`.
- **RH5104-004:** a test with the comment directly in front of `)` would pin the newly worded half; the RH5104 Fix All scenario already shows it.

## Finding 1 — SHARED-FIX-002 and Fix All in two iterations

- **No spec of this run claims a one-pass Fix All of its own.** The rule specs state no Fix All requirement. "Fix All corrects everything in one application" comes only from the merged SHARED-FIX-002, which applies to every rule whose header says `Fix All`. The headers of RH5104, RH5110 and RH5204 say `Fix All`, and that matches their providers (BatchFixer).
- **The two Why sentences about "one application" are about one chain, not Fix All.** SHARED-CHAIN-008 and RH5201-005 say the chain fix "clears every diagnostic the chain had in one application" / "at once". Both concern a single fix of a single chain, and the RH5112 and RH5201 Fix All scenarios do not need a second iteration.
- **What the code does:**
  - RH5104 (`GetFixAllScenario`, `NumberOfFixAllIterations = 2`): two comments relocate to the same insertion point at the start of the `if` line; the batch fixer applies one insertion per pass.
  - RH5110: the outer method's fix formats the whole declaration, which contains the local function the inner fix rewrites; the batch fixer discards the overlapping change in its first pass.
  - RH5204: six diagnostics whose fixes format overlapping scopes need two passes. The test implementer also observed this for a misindented field plus a misindented comment in one class.
- **So these three rules do not meet the merged SHARED-FIX-002** ("in one Fix All application"). The same `NumberOfFixAllIterations = 2` appears outside this batch: RH3001, RH7102, RH5518.
- This is behavior on `main` against a merged shared requirement. I did not change SHARED-FIX-002, and no rule spec of this run states anything about it. I recommend filing it as an open question in the `SDD open questions: sdd/specs/shared/code-fixes.md` issue; it is not a P7 case, because P7 covers SHARED-FIX-001/004/005. Entry below.

## Open questions to file

New, in addition to the complete list in `03-spec-author.md`, which still stands:

### `SDD open questions: sdd/specs/shared/code-fixes.md` (same issue as 03's fix-scope question)

**Q2 — Fix All needs two applications for rules whose fixes overlap or insert at the same position.**
- Inputs: the `GetFixAllScenario` inputs of `RH5104CommentsMustBeOnTheirOwnLineAnalyzerTests` (`if (left /* c1 */ == right /* c2 */)`), `RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzerTests` (an outer method and its local function, both with `where` on the signature line), and `RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzerTests` (six misindented lines in one class).
- Real output: the expected result is reached only after two Fix All iterations (`NumberOfFixAllIterations = 2`). One iteration leaves a diagnostic. The same setting is used for RH3001, RH7102 and RH5518.
- Why it looks wrong: SHARED-FIX-002 requires every reported diagnostic to be corrected "in one Fix All application", and `FixAllCorrectsEveryDiagnostic` carries that requirement.
- Decision needed: bug against SHARED-FIX-002, or a clarification of SHARED-FIX-002 for overlapping or colliding fixes.
- Touches: SHARED-FIX-002 and the `Fix All` header rows of RH5104, RH5110 and RH5204.

## Follow-up comments

Unchanged from `03-spec-author.md` (#956, #953, #1097). Nothing new for #1096: the multi-line label case is its Q1 and is only excluded from RH5103-003.
