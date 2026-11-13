Role: implementer
Commit: ba83087
Verdict: test disputed

Every one of the 76 tests the test implementer handed over passes, except two that fail only on a check outside the fix's own output (RH5203 `AssertFormattingChangesNothing`, and the RH5204 one-pass iteration count). 18 analyzer tests are red: 3 are the SHARED-FIX-010 tests that decision 15 expected, 2 are the RH5204 iteration counts decision 17 expected, and 13 are tests that predate the run (or are characterization tests the test stage wrote) and pin the old scope or the reproduced defect. All 18 are listed under *Disputed tests or scope*. No test was changed. The prompt carried no intended fix, cause, or hoped-for verdict beyond the lead's recorded decisions. The change is larger than the handovers name: a new internal type and about 400 net production lines. See *Whole run*.

## thoenissen/Reihitsu#1140

Narrowed per decision 3.

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5204IndentationMustUseFourSpacesPerScopeLevelCodeFixProvider.cs` | The fix no longer formats a scope. It replaces only the whitespace in front of the reported line's first element with the indentation the analyzer's `BuildExpectedIndentationMap` expects. It does this for tokens, comments, and directives alike, so the token path now uses the same path as directives. A `///` run re-indents all of its lines (decision 16). `TryGetFormattingScope`, `IsScopeRefused`, `ApplyIndentationFormattingAsync`, `ApplyDirectiveIndentationFormattingAsync`, `TryGetDirectiveTrivia`, and `GetStartLine` are deleted. The fix no longer calls the formatter, so SHARED-FIX-010 no longer withholds it. | RH5204-006, SHARED-FIX-006 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/StatementBracesCodeFixProviderBase.cs` (RH5405–RH5407) | The fix wraps only the reported body in a new block and hands it to `LineLayoutCodeFixUtilities.ReplaceWithBlockOnOwnLinesAsync`. It no longer formats the owning `if` statement. The SHARED-FIX-010 check now looks at the body. | SHARED-BRACE-002, SHARED-FIX-006, SHARED-FIX-007 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5403StatementMustNotBeOnSingleLineCodeFixProvider.cs` | The fix lays out only the reported block, through the same helper. The SHARED-FIX-010 check now looks at the block. | RH5403-003 |
| `src/Reihitsu.Analyzer.CodeFixes/Core/LineLayoutCodeFixUtilities.cs` (new, internal) | `ReplaceWithBlockOnOwnLinesAsync` puts a block's braces on lines of their own, in the column of the line on which the block's owner starts. The gap in front of `{` loses only its whitespace at the end of the line. A following `else`, `catch`, `finally`, or `do … while` on the same line moves to the next line in that column. `FormatNodeInDocumentAsync` then lays out the block itself. | SHARED-FIX-006, SHARED-FIX-007 |

## thoenissen/Reihitsu#1141

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Base/OperatorSpacingCodeFixProviderBase.cs` | The fix no longer formats the node around the operator. It replaces each gap next to the operator that the analyzer's gap policy reports with exactly one space. A gap with a line break or a comment stays as written. The SHARED-FIX-010 check is gone because the fix no longer calls the formatter. Fix All corrects `a+b+c`, `a==b&&b>0`, and `d[k=1]=2` in one pass (decision 19). | SHARED-OPSPACE-004, SHARED-FIX-006 |
| `src/Reihitsu.Core/FormattingTextAnalysisUtilities.cs` | `IsOperatorGapViolation` changes from private to public, with a fuller summary, so the analyzer's `HasOperatorSpacingViolation` and the fix share one gap policy (FC-015). | SHARED-OPSPACE-004 |

## thoenissen/Reihitsu#1144

### Changes
| File | What changed | Requirement |
|---|---|---|
| `RH5205StatementLambdaOpeningBraceShouldBeAlignedCodeFixProvider.cs` | The fix is withheld when the lambda has an attribute list. | RH5205-004 |
| `RH5203MultiLineArgumentsShouldBeAlignedCodeFixProvider.cs` | A comment that starts its own line in front of the argument moves into the alignment column. The whitespace of blank lines, directive lines, and the lines behind a `///` comment stays as written. Previously every whitespace trivia was dropped. | RH5203-003 |
| `StatementBracesCodeFixProviderBase.cs`, `RH5403…CodeFixProvider.cs` | The braces of an `else if` body stand in the column of `else`, through the helper above. | SHARED-FIX-007 |
| `RH5417…CodeFixProvider.cs`, `RH3202…CodeFixProvider.cs` | Both fixes first separate the member from a sibling member that shares its first or last line, but only when the gap holds nothing but whitespace (`LineLayoutCodeFixUtilities.FormatMemberOnLinesOfItsOwnAsync`), and then format the member as before. Fix All for both changes from `BatchFixer` to `DocumentOrderFixAllProvider`. Under BatchFixer, two adjacent members each break the shared gap, and the merged text left a whitespace-only line (`FixPutsTwoMethodsOnOneLineOnLinesOfTheirOwn`). | SHARED-FIX-007 (shared line), SHARED-FIX-002 |
| `Core/TrailingCommaCodeFixHelper.cs` (RH5409 and the other users of the trailing-comma base) | A comma at the end of its line takes the spaces in front of it along. A comma that starts its line in front of `}` leaves `}` in the indentation of the line that holds the matching `{`. | SHARED-FIX-007 |
| `RH3102CodeMustNotContainEmptyStatementsCodeFixProvider.cs` | An empty statement that ends a line behind other code first hands its line break to the token in front of it, whose trailing whitespace is dropped. | SHARED-FIX-007 |
| `RH3201CommentsMustContainTextCodeFixProvider.cs` | A comment that stands alone on its lines is removed together with all of them. Before, only the first line of a multi-line comment was checked. | SHARED-FIX-007 |

The RH5101, RH5102, RH3106, and RH3107 characterization tests stay green.

## thoenissen/Reihitsu#1145

### Changes
| File | What changed | Requirement |
|---|---|---|
| `RH5103CodeMustNotContainMultipleStatementsOnOneLineCodeFixProvider.cs` | `HasEditableLeadingGap`: the token in front of the statement only has to be the preceding statement's last token. Its nearest statement ancestor no longer has to be that statement, which covers embedded statements, blocks, labeled statements, and local functions (Q2). The fix is withheld when the block's `{` stands on the line on which the preceding statement starts (Q4). | RH5103-003, RH5103-005 |

## thoenissen/Reihitsu#1094

### Changes
| File | What changed | Requirement |
|---|---|---|
| `Base/TargetAttributePlacementCodeFixProviderBase.cs` | New guard `IsInnermostEnclosingBraceOffItsDepthColumn` for the separate-line mode outside every anchor scope. It withholds the fix when the line of the innermost enclosing `{` (block, type, namespace, enum, switch, or accessor list) is not indented `ComputeBaseIndentLevel(owner) × 4`. A tab counts as four, so the tab characterization test (Q4) stays green. For Q2, a list that starts a member behind a sibling member on the same line also gets a line break in front of it, in the declaration's column. | SHARED-ATTR-021, SHARED-FIX-007 |

## thoenissen/Reihitsu#1393

No production change. The characterization test passes.

## thoenissen/Reihitsu#1511

Covered by the RH5204 fix change under #1140. The fix replaces the reported line's whitespace directly, so the root-child leading-trivia rule of FMT-PIPE-010 no longer applies. All eight tests pass.

## thoenissen/Reihitsu#1561

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Layout/RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.cs` | `AlignCommentIndentation` drops the token's own line from the map when a `/* */` or `/** */` comment in its leading trivia starts on an earlier line and ends on the token's line (`EndsMultiLineCommentOnLine`). The token's expected column still aligns the comment, so a misindented multi-line comment is still reported, on the comment. | RH5204-001, RH5204-002, SHARED-BLANK-008 |

Switching `SetTokenIndentation` from `IsFirstOnLine` to `StartsLine` was tried first and reverted. `StartsLine` also excludes a token behind a one-line `/**/` at the start of its line, which `VerifyMemberInScopeColumnBehindMisplacedBlockCommentIsDetected` (SHARED-COMMENT-011) requires to stay reported.

## Whole run

### Policy owners
- Operator gap policy: `rg "IsOperatorGapViolation|HasOperatorSpacingViolation" src` finds one owner, `FormattingTextAnalysisUtilities.IsOperatorGapViolation`, now called by the analyzer path and the fix. The fix has no copy.
- Line indentation and whitespace runs: `rg "GetLeadingWhitespace|GetLeadingWhitespaceRunStart" src/Reihitsu.Core` finds `FormattingTextAnalysisUtilities`, which `LineLayoutCodeFixUtilities.GetLineIndentation` and `TrailingCommaCodeFixHelper` reuse. `TrailingCommaCodeFixHelper.SkipSpacesForward` is a private forward scan; no forward counterpart exists in Core.
- RH5204 expected indentation: `RH5204…Analyzer.BuildExpectedIndentationMap` stays the one owner, and the fix reads it for every diagnostic.
- Attribute depth model: `SyntaxIndentationUtilities.ComputeBaseIndentLevel`, reused by the new guard. The guard's brace-kind switch mirrors `IsIndentingAncestor` minus initializers and anonymous objects, as the existing `HasIncompleteEnclosingScope` already does in the same file.
- Trailing-trivia trimming: `rg "EndWithLineBreak|TrimTrailingWhitespace" src` finds no earlier owner in CodeFixes; the new `LineLayoutCodeFixUtilities.EndWithLineBreak` is shared by its callers.

### Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 16 files changed, 676 insertions(+), 268 deletions(-). That is about 408 net lines, 294 of them in the new file.
New type: `Reihitsu.Analyzer.CodeFixes.Core.LineLayoutCodeFixUtilities` (internal static).
New public member: `FormattingTextAnalysisUtilities.IsOperatorGapViolation`, which was private before.

### Deleted special cases
_N/A._ No shape is declared unsupported.

### Tests run
- `scripts/build.sh --configuration Debug`: 0 warnings, 0 errors.
- `scripts/test.sh --configuration Debug --project analyzer`: Failed 18, Passed 8296, Total 8314. Each failure is listed below.
- `scripts/test.sh --configuration Debug --project architecture`: Passed 117/117.
- `scripts/test.sh --configuration Debug --project core`: Passed 655/655.
- Focused runs during the work: `--filter "FullyQualifiedName~RH5204"`, `~RH6023|~RH6024`, `~RH5405|~RH5406|~RH5407`, `~RH5403`, `~RH5417|~RH3202`, `~RH5103`, `~RH5203|~RH5205`, `~RH5409|~RH5410|~RH5411`, `~RH3102`, `~RH3201`, `~RH55`. Only the tests listed below fail in them.
- The formatter and CLI projects were not run: neither project changed.

### Disputed tests or scope

Expected by the lead's decisions (the test implementer rewrites them):
1. `RH5405…Tests.NoFixIsOfferedWhenTheIfStatementHoldsACommentTheFormatterRefuses`, `RH5403…Tests.NoFixIsOfferedWhenTheOwningStatementHoldsACommentTheFormatterRefuses`, `RH6024…Tests.NoFixIsOfferedWhenTheBinaryExpressionHoldsACommentTheFormatterRefuses` (decision 15). The refused comment is in another body, block, or operand, which the narrowed fixes no longer format, so the fix is offered.
2. `RH5204…Tests.FixAllCorrectsEveryDiagnostic` (`GetFixAllScenario`, decision 17). The scenario asserts `NumberOfFixAllIterations = 2`, but Fix All now finishes in 1 iteration.

Tests that assert the old scope or the reproduced defect, against the specs as changed in this run:

3. `RH5204…Tests.VerifyEmbeddedIfBehindMultiLineCommentIsFixedInOnePass` and `…WithCarriageReturnLineFeed`. The output now matches the rewritten expectation, but the tests set `NumberOfIncrementalIterations = 1` with two diagnostics (`{` and `}`). Under RH5204-006 each fix moves only its own line, so two applications are needed. SHARED-FIX-001 allows one application per diagnostic. The count should be 2, or the setting dropped.
4. `RH5204…Tests.VerifyClosingBraceBehindStrayDocumentationCommentAndBlankLineIsDetectedAndFixed` (pre-existing, tags RH5204-001). The expected output removes the blank line in front of `}`. RH5204-006 now says the fix MUST NOT change the blank lines around the line. Actual: `internal int A; /// x⏎⏎}`.
5. `RH6023…Tests.VerifyFixKeepsTheWrappedLinkOfACompoundAssignedMemberBehindAMemberBinding` and `…OfAnAssignedMemberInANestedConditionalAccess` (pre-existing, tags RH6023-002 and FMT-PIPE-024). They expect the fix to move the wrapped `.C` link from column 9 to 10, and from 12 to 13. SHARED-OPSPACE-004 now says only the operator gaps change, so the link keeps its column. The test implementer rewrote a sibling of these (`…WhoseValueIsWrapped`) but not these two.
6. `RH5511…Tests.VerifyCodeFixIndentsAtNestingDepthWhenPrecededByOtherSourceOnSameLine`, `…UsesSpacesRegardlessOfSurroundingTabIndentation`, `…IndentsCorrectlyWhenLineBeginsInsideMultiLineVerbatimString`, `…IndentsCorrectlyWhenAttributeListFollowsMultiLineParameterList`, `…IndentsCorrectlyWhenAttributeListFollowsMultiLineInitializer` (pre-existing). Each has `<member>; [First]⏎    internal Example()` and expects the list to stay behind the previous member. That is the #1094 Q2 shape, which the new `RH5513…FixPutsTheAttributeListBehindAFieldOnALineOfItsOwn` (SHARED-FIX-007, shared line) requires on its own line. Actual: `private int _pad;⏎    [First]⏎    internal Example() { }`. With tab indentation the inserted line uses four spaces, as the declaration line already did.
7. `RH3102…Tests.FixOfEmptyStatementBehindAStatementJoinsTheNextLine` (characterization). It pins `a++;         a++;`, the exact defect that `FixKeepsTheNextStatementOnItsLine` now fixes.
8. `RH3201…Tests.EmptyMultiLineCommentWithNewlinesIsReportedAndFixed` (pre-existing, tags RH3201-001 and RH3201-004) and `FixOfBlockCommentSpanningSeveralLinesLeavesAnEmptyLine` (characterization). Both expect the empty line that a multi-line comment leaves behind `{`, which `FixLeavesNoEmptyLineBehindTheOpeningBraceWhereAMultiLineCommentStood` (SHARED-FIX-007, #1235 Q3) now forbids.
9. `RH5203…Tests.FixMovesACommentLineAboveTheArgumentIntoTheAlignmentColumn`. Both `Verify` calls pass. Its `AssertFormattingChangesNothing(fixedCode)` fails because the formatter inserts a blank line in front of `// two` inside the argument list (`M(1,⏎⏎          // two⏎          2);`). That is formatter output on the expected code, not fix output. Either the formatter's blank-line policy for a comment line inside an argument list is a formatter defect, or the assertion does not belong in this test. No formatter phase is in this run's scope. A comment inside an argument list is RH5038's rare class.

Scope (`README.md`, *Scope*):

10. New internal type `LineLayoutCodeFixUtilities`. It is a shared helper with four consumers: StatementBraces (RH5405–RH5407), RH5403, RH5417, and RH3202. `EndWithLineBreak` and `SharesLineWithPreviousSiblingMember` are also used by `TargetAttributePlacementCodeFixProviderBase`. Without it, the brace and RH5403 fixes would each carry the same block layout code.
11. A private Core method became public: `FormattingTextAnalysisUtilities.IsOperatorGapViolation`.
12. Fix All provider change: RH5417 and RH3202 now use `DocumentOrderFixAllProvider` instead of `BatchFixer` (see #1144). This was needed for `FixPutsTwoMethodsOnOneLineOnLinesOfTheirOwn`.
13. The production paths are wider than the handovers name: about 408 net lines, against the prompt's limit of about 100. The fixes for RH5409–RH5411, RH3102, RH3201, RH5203, RH5103, RH5205, and the attribute base are small and local. The largest parts are the new file (294 lines, most of it XML documentation) and the RH5204 fix rewrite, which is a net deletion of 110 lines.
14. Mechanism note for the brace and RH5403 fixes. The block is formatted with `FormatNodeInDocumentAsync`, after the fix itself has placed `{` on its own line in the column of the owner's line. That the formatter keeps that column comes from the tests, not from a reading of FMT-PIPE-010/017. The `else if` and nested tests pass with `AssertFormattingChangesNothing`. A comment in the body's leading trivia (`if (a)⏎    // c⏎    M();`) moves into the block with the body, where the formatter indents it. No test covers this case.

### Architecture findings

The #1140 finding (skeptic 01) is confirmed. Fixes that need less than a node must place braces and lines themselves. `LineLayoutCodeFixUtilities` decides the column of the braces from the indentation of the owner's line, and `TrailingCommaCodeFixHelper` decides the `}` column from the indentation of the line that holds `{`. The tab-width-4 measure in the new SHARED-ATTR-021 guard also restates the indentation phase's model. These are the narrowest changes the tests need. An entry point that lays out only the changed code would remove these predictions; that is the follow-up already recorded in `00-lead.md`.

### Follow-up candidates
- Formatter: a blank line is inserted in front of a comment line inside an argument list (`M(1,⏎ // two⏎ 2);`), so formatting is not idempotent with the RH5203-003 output (item 9).
- RH3202 and other fixes that format a member through `FormatNodeInDocumentAsync`, for example `ExpressionBodyToBlockCodeFixProviderBase` (RH3203 and the other expression-body kinds, FC-025), may still leave a sibling member on the shared line. Only RH3202 and RH5417 got the separation in this run.
- `TrailingCommaCodeFixHelper` leaves `B , // c` as `B  // c` (two spaces). It is untested and is not the shape the run covers.
