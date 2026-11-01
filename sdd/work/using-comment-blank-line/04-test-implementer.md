Role: test implementer
Commit: edcf520
Verdict: tests written

Scope read: thoenissen/Reihitsu#1497, `00-lead.md` (with *Maintainer answers* 1–7), `01-spec-author.md`, `02-skeptic.md`, `03-spec-author.md`, the accepted change doc `sdd/changes/2026-11/using-comment-blank-line.md`, and the spec diff `db61ed3..edcf520`. The prompt carried no intended fix or hoped-for verdict.

All expected values come from the spec examples, the change doc's visible output changes 1–6 and *Unchanged*, and the skeptic's candidate shapes. None comes from patched code.

Full runs on `edcf520` with these tests:

- Formatter: 33 failed, 3711 passed.
- Analyzer: 29 failed, 6354 passed.

Every failure is one of the new or rewritten tests below. Each one fails on the missing blank line above the comment, either as a phase-output or fixed-code diff, as "using phase changes the final output", or as an unexpected RH7207 diagnostic. No other test fails. `scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec|FullyQualifiedName~Requirement"`: 51/51 pass, so FMT-USING-010 and SHARED-USING-011 are now covered.

## Tests

Test-side helper changes (part of the matrix):

- `PhaseReversalExceptionList`: both `BlankLineAboveCommentLedUsingDirective` entries are removed, for `DelimitedDocumentationBlankLineConvergenceTests.SeparatesDelimitedDocumentationFromPrecedingUsingDirectiveInOnePass` and `UsingDirectiveOrderingTests.SameGroupDirectiveBehindMultiLineDelimitedDocumentationCommentKeepsTheCommentWithIt`.
- `PhaseReversalCauses.BlankLineAboveCommentLedUsingDirective` is removed. This implements the issue's "the matching phase-level idempotency exceptions are removed".
- Both existing tests now fail until the using phase keeps the blank line. Their assertions are unchanged.

`PhaseStabilityCheckTests.CheckCatchesUsingPhaseRemovingBlankLineAbove{DelimitedDocumentation,Comment}LedUsingDirective` asserted the old reversal. They are rewritten as `CheckFindsNoChangeAbove{DelimitedDocumentation,Comment}LedUsingDirectiveOfTheSameGroup`, using the same sources with `AssertNoPhaseChanges`.

Two existing RH7207 tests gain a `SHARED-USING-011` tag because they are that requirement's examples:

- `NoDiagnosticWhenCommentSeparatesSameGroupUsings`: no blank line above the comment, organized.
- `BlankLineInsideAGroupIsReported`: no comment, not organized.

Formatter, using phase alone: `src/Reihitsu.Formatter.Test/Unit/UsingDirectives/UsingDirectivePhaseCommentBlankLineTests.cs` (new). Each test runs the phase under LF and CRLF, and runs it again on its own output (FC-010).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| BlankLineAboveCommentStaysWhenTheBlockIsReorderedAroundIt | FMT-USING-010 | phase alone, LF+CRLF, 2nd run | fails: blank line removed |
| BlankLineAboveCommentStaysWhenItsDirectiveMoves | FMT-USING-010 | same | fails: blank line removed |
| BlankLineAboveCommentOnTheDirectivesLineStaysThroughAReorder | FMT-USING-010 | same | fails: blank line removed |
| SeveralBlankLinesAboveCommentBecomeOne | FMT-USING-010 | same | fails: none written |
| SeveralBlankLinesAboveCommentBecomeOneWithoutReorder | FMT-USING-010 | same | fails: none written |
| HeaderBlankLineCountsForTheCommentThatMovesAwayFromTheTop | FMT-USING-010 | same | fails: none written |
| CommentWithoutBlankLineAboveGetsNoneWhenItsDirectiveMovesIntoTheGroup | FMT-USING-010 (other side of the condition) | same | passes |
| CommentWithoutBlankLineAboveInAnOrderedBlockGetsNone | FMT-USING-010 (other side) | same | passes |
| BlankLineAboveSingleLineCommentInAnOrderedBlockIsKept | FMT-USING-010 | same | fails |
| BlankLineAboveDelimitedCommentInAnOrderedBlockIsKept | FMT-USING-010 | same | fails |
| BlankLineAboveSingleLineDocumentationCommentInAnOrderedBlockIsKept | FMT-USING-010 | same | fails |
| BlankLineAboveDelimitedDocumentationCommentInAnOrderedBlockIsKept | FMT-USING-010 | same | fails |
| BlankLineAboveCommentOnTheDirectivesLineInAnOrderedBlockIsKept | FMT-USING-010 | same | fails |
| BlankLineAboveMultiLineDocumentationCommentOnTheDirectivesLineIsKept | FMT-USING-010 | same | fails |
| BlankLinesAboveAndBelowTheCommentAreKept | FMT-USING-010 | same | fails (blank line above removed) |
| BlankLineBetweenTwoCommentsIsKept | FMT-USING-010 | same | fails on its second case (blank line above `// A`); first case passes |
| BlankLineAboveCommentBehindATrailingCommentIsKept | FMT-USING-010 | same | fails |
| BlankLineAboveCommentIsKeptInsideABlockScopedNamespace | FMT-USING-010 | same | fails |
| BlankLineAboveCommentIsKeptInsideAFileScopedNamespace | FMT-USING-010 | same | fails |
| BlankLineAboveDirectiveWithoutCommentIsRemoved | FMT-USING-002 | same | passes |

Formatter, full pipeline: `src/Reihitsu.Formatter.Test/Regression/UsingDirectives/UsingDirectiveCommentBlankLineTests.cs` (new). Each test asserts the output and a stable second pass under LF and CRLF, plus the phase-level stability check on the final output.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| BlankLineAboveSingleLineCommentIsKept (change doc 1) | FMT-BLANK-011, FMT-USING-010 | `AssertRuleResult` | fails: using phase alone changes the output (FC-012) |
| BlankLineAboveDelimitedDocumentationCommentIsKept (change doc 1) | FMT-BLANK-011, FMT-USING-010 | `AssertRuleResult` | fails: same |
| CommentLeadingDirectiveOfTheSameGroupGetsOneBlankLine (FMT-BLANK-011 new example) | FMT-BLANK-011 | `AssertRuleResult` | fails: same |
| SeveralBlankLinesAboveCommentLeadingDirectiveBecomeOne | FMT-BLANK-011 | `AssertRuleResult` | fails: same |
| SingleLineDocumentationCommentLeadingDirectiveGetsOneBlankLine | FMT-BLANK-011 | `AssertRuleResult` | fails: same |
| BlankLineAboveOrdinaryCommentOnTheDirectivesLineIsKept (change doc 6) | FMT-USING-010 | `AssertRuleResult` | fails: output has no blank line above `/* c */` |
| OrdinaryCommentOnTheDirectivesLineWithoutBlankLineGetsNone (change doc 6, last sentence) | FMT-USING-010 (other side) | `AssertRuleResult` | passes |
| BlankLinesAboveAndBelowTheCommentAreKept | FMT-USING-010 | `AssertRuleResult` (unchanged input) | fails: output mismatch |
| TwoCommentParagraphsLeadingDirectiveGetOneBlankLineAboveTheFirst | FMT-BLANK-011 | `AssertRuleResult` | fails: phase stability |
| CommentBehindPredecessorWithTrailingCommentGetsOneBlankLine | FMT-BLANK-011 | `AssertRuleResult` | fails: phase stability |
| BlankLineAboveCommentIsKeptThroughAReorder (change doc 2 input) | FMT-USING-010 | `AssertRuleResult` | fails: phase stability |
| BlankLineAboveCommentIsKeptInsideABlockScopedNamespace | FMT-USING-010 | `AssertRuleResult` | fails: phase stability |
| BlankLineAboveCommentIsKeptInsideAFileScopedNamespace | FMT-USING-010 | `AssertRuleResult` | fails: phase stability |
| (existing) DelimitedDocumentationBlankLineConvergenceTests.SeparatesDelimitedDocumentationFromPrecedingUsingDirectiveInOnePass | — (exception entry removed) | `AssertRuleResult` | fails: phase stability |
| (existing) UsingDirectiveOrderingTests.SameGroupDirectiveBehindMultiLineDelimitedDocumentationCommentKeepsTheCommentWithIt | — (exception entry removed) | `AssertRuleResult` | fails: phase stability |
| PhaseStabilityCheckTests.CheckFindsNoChangeAboveDelimitedDocumentationLedUsingDirectiveOfTheSameGroup | FMT-USING-010 | `AssertNoPhaseChanges` | fails: the using phase changes it |
| PhaseStabilityCheckTests.CheckFindsNoChangeAboveCommentLedUsingDirectiveOfTheSameGroup | FMT-USING-010 | `AssertNoPhaseChanges` | fails: same |

Analyzer RH7207: `src/Reihitsu.Analyzer.Test/Formatting/RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzerTests.cs`. Its Fix All is covered because `Verify` with a fixed source runs Fix All, and the verifier's iteration check covers one-pass convergence (FC-007/FC-008).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| BlankLineAboveSingleLineCommentLeadingDirectiveOfTheSameGroupIsNotReported (change doc 1) | RH7207-002, SHARED-USING-011 | `Verify(source)` | fails: RH7207 reported |
| BlankLineAboveDelimitedDocumentationCommentLeadingDirectiveOfTheSameGroupIsNotReported (change doc 1) | RH7207-002, SHARED-USING-011 | `Verify(source)` | fails |
| BlankLinesAboveSeveralCommentsLeadingDirectivesOfTheSameGroupAreNotReported (RH7207-002 example) | RH7207-002 | `Verify(source)` | fails |
| BlankLineAboveSingleLineDocumentationCommentLeadingDirectiveOfTheSameGroupIsNotReported | SHARED-USING-011 | `Verify(source)` | fails |
| BlankLineAboveDelimitedCommentLeadingDirectiveOfTheSameGroupIsNotReported | SHARED-USING-011 | `Verify(source)` | fails |
| SeveralBlankLinesAboveCommentLeadingDirectiveOfTheSameGroupAreNotReported | RH7207-002, SHARED-USING-011 | `Verify(source)` | fails |
| BlankLineAboveOrdinaryCommentOnTheDirectivesLineIsNotReported (change doc 6; RH5020 fix output) | RH7207-002, SHARED-USING-011 | `Verify(source)` | fails |
| BlankLineAboveMultiLineDocumentationCommentOnTheDirectivesLineIsNotReported | SHARED-USING-011 | `Verify(source)` | fails |
| BlankLineBelowCommentLeadingDirectiveOfTheSameGroupIsNotReported | RH7207-002, SHARED-USING-011 | `Verify(source)` ×2 | fails on the case with the blank line above; the other case passes by analysis (`HasBlankLineBefore` sees no line break before `//`) |
| BlankLineBetweenTwoCommentsLeadingDirectiveOfTheSameGroupIsNotReported | SHARED-USING-011 | `Verify(source)` ×2 | fails on the second case (blank line above `// A`); the first case passes |
| BlankLineAboveCommentBehindPredecessorWithTrailingCommentIsNotReported | SHARED-USING-011 | `Verify(source)` | fails |
| BlankLineAboveCommentInsideBlockScopedNamespaceIsNotReported | SHARED-USING-011 | `Verify(source)` | fails |
| BlankLineAboveCommentInsideFileScopedNamespaceIsNotReported | SHARED-USING-011 | `Verify(source)` | fails |
| BlankLineAboveDirectiveWithoutCommentNextToCommentLedDirectiveIsReported | RH7207-001, SHARED-USING-011 | `Verify(source, fixed)` | fails: the fix also removes the blank line above `// About Linq` (the diagnostic itself is reported) |
| CodeFixKeepsBlankLineAboveCommentOfMovedDirective (change doc 2; RH7207-004 example) | RH7207-004 | `Verify(source, fixed)` | fails: blank line removed |
| CodeFixKeepsBlankLineAboveCommentOfDirectiveThatStaysInPlace (change doc 3) | RH7207-004 | `Verify(source, fixed)` | fails |
| CodeFixWritesOneBlankLineWhereSeveralStoodAboveComment (change doc 4) | RH7207-004 | `Verify(source, fixed)` | fails |
| CodeFixKeepsHeaderBlankLineAboveCommentThatMovesAwayFromTheTop (change doc 5) | RH7207-004 | `Verify(source, fixed)` | fails |
| CodeFixKeepsBlankLineAboveOrdinaryCommentOnTheDirectivesLine | RH7207-004 | `Verify(source, fixed)` | fails |
| CodeFixWritesNoBlankLineAboveCommentThatHadNone (change doc *Unchanged*, answer 6) | RH7207-004 | `Verify(source, fixed)` | passes |
| CodeFixKeepsBlankLineAboveCommentOfMovedDirectiveUnderCarriageReturnLineFeed | RH7207-004 | `ApplyCodeFixAsync`, CRLF | fails |
| CodeFixWritesOneBlankLineWhereSeveralStoodAboveCommentUnderCarriageReturnLineFeed | RH7207-004 | `ApplyCodeFixAsync`, CRLF | fails |
| FixAllKeepsBlankLineAboveCommentInEveryBlock (compilation unit + block namespace, `//` and glued `/* c */`) | RH7207-004 | `Verify(source, fixed)`, 2 diagnostics | fails |

Analyzer ↔ formatter parity: `src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH7207UsingDirectivesShouldBeOrganizedIntoGroupsFormatterTests.cs` (FC-013)

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| FormattedBlankLineAboveSingleLineCommentIsNotReported | RH7207-002 | `VerifyFormatter(source)` LF+CRLF | fails: RH7207 reported |
| FormattedBlankLineAboveDelimitedDocumentationCommentIsNotReported | RH7207-002 | `VerifyFormatter(source)` | fails |
| FormattedBlankLineAboveOrdinaryCommentOnTheDirectivesLineIsNotReported | RH7207-002 | `VerifyFormatter(source)` | fails |
| FormatterOutputWithBlankLineAddedAboveCommentIsNotReported | RH7207-002 | `VerifyFormatter(source, fixed)` | fails: RH7207 reports the formatter's output |
| FormatterOutputOfReorderWithBlankLineAboveCommentIsNotReported | RH7207-002 | `VerifyFormatter(source, fixed, diag)` | fails: same |

Sibling fixes and RH5020 (FC-016, SHARED-FIX-004 sequence):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH7201…AnalyzerTests.CodeFixKeepsBlankLineAboveCommentLeadingDirectiveOfTheSameGroup | RH7201-004 | `Verify(source, fixed)`, 2 diagnostics | fails: blank line removed |
| RH7203…AnalyzerTests.CodeFixWritesOneBlankLineWhereSeveralStoodAboveCommentLeadingDirectiveOfTheSameGroup | RH7203-004 | `Verify(source, fixed)` | fails: none written |
| RH5020…AnalyzerTests.VerifyDiagnosticForCommentLeadingUsingDirectiveOfTheSameGroup | RH5020-001, RH5020-005 | `Verify(source, fixed)` | passes (RH5020 unchanged); its fixed output is the input of the RH7207 not-reported test for `//` |
| RH5020…AnalyzerTests.VerifyDiagnosticForCommentOnTheLineOfUsingDirectiveOfTheSameGroup | RH5020-001, RH5020-005 | `Verify(source, fixed)` | passes; its fixed output is exactly the input of `BlankLineAboveOrdinaryCommentOnTheDirectivesLineIsNotReported` |

## Not covered, and why

- **RH7202, RH7204, RH7205, RH7206 fixes.** These have no test of their own. A static proof replaces them:
  - `rg -n "override|abstract|virtual" src/Reihitsu.Analyzer.CodeFixes/Base/UsingDirectiveOrderingCodeFixProviderBase.cs src/Reihitsu.Analyzer.CodeFixes/Rules/Organization/RH720[1-6]*.cs` shows that every member of `UsingDirectiveOrderingCodeFixProviderBase` is `sealed override` and that the six providers override nothing.
  - The base calls `UsingDirectiveCodeFixUtilities.OrganizeScopeUsingsAsync` (`UsingDirectiveOrderingCodeFixProviderBase.cs:59`), which runs `new UsingDirectiveOrderingPhase().Execute` (`UsingDirectiveCodeFixUtilities.cs:44`).
  - RH7207's provider calls the same utility directly.
  - RH7201 and RH7203 are tested as two arms of the base, and RH7207 as the separate caller.
- **Directives (FC-005, FC-019, FC-020).** A block with any directive trivia is not reordered (SHARED-USING-009). Both the phase and RH7207 leave it alone, and existing tests cover this (`BlockWithPreprocessorDirectiveIsNotReported`, `ConditionalDirectiveSkipsReordering`, `PragmaDirectiveSkipsReordering`). The gap-with-a-comment case is the subject of this run and is covered throughout.
- **FC-004 / FC-021.** Comments that move with a reordered directive are covered by the moved, in-place, header, glued, and namespace cases.
- **FC-006, FC-018, FC-022–FC-029, FC-036.** No semantics, literals, identifiers, renames, or member kinds are touched.
- **FC-009.** No new code action is registered.
- **FC-011.** There is no loop.
- **FC-030.** The existing `CodeFixDoesNotFormatFollowingType` covers this.
- **FC-031, FC-037.** These apply to the implementer and the auditor.
- **FC-032.** The analyzer and formatter results above were both gathered on `edcf520`, after the test changes and before any production change.
- **A comment-led directive with a blank line above it that becomes the block's first directive through the reorder.** Example: `using System.Linq;⏎⏎// About System⏎using System;`. FMT-USING-010 covers only a directive that continues its predecessor's group, and FMT-USING-009 covers only the original first directive. No spec example decides whether that blank line survives at the top, so I wrote no test for it (see the spec note below).
- **A whitespace-only line counted as blank above the comment (SHARED-BLANK-001).** No requirement of this change names it, so it is not tested.

## Spec and test notes

- **Possible spec gap for the lead.** Neither FMT-USING-009 nor FMT-USING-010 decides the reorder case listed above, where a comment-led directive with a blank line above it becomes the first directive. It is unclear whether that is in scope. I wrote no test for it.
- `BlankLineBelowCommentLeadingDirectiveOfTheSameGroupIsNotReported` and `BlankLineBetweenTwoCommentsLeadingDirectiveOfTheSameGroupIsNotReported` each verify two sources in sequence. Only the source with a blank line above the first comment fails today.

## Follow-up candidates

None.

## Architecture findings

None.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~UsingDirectivePhaseCommentBlankLineTests|FullyQualifiedName~UsingDirectiveCommentBlankLineTests|FullyQualifiedName~PhaseStabilityCheckTests|FullyQualifiedName~SameGroupDirectiveBehindMultiLineDelimitedDocumentationCommentKeepsTheCommentWithIt|FullyQualifiedName~SeparatesDelimitedDocumentationFromPrecedingUsingDirectiveInOnePass"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7207|FullyQualifiedName~RH5020CommentsShouldBePrecededByABlankLineAnalyzerTests|FullyQualifiedName~RH7201|FullyQualifiedName~RH7203"
scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec|FullyQualifiedName~Requirement"
```
