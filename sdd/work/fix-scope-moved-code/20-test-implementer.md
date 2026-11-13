Role: test implementer
Commit: 38d080b
Verdict: tests written

Repair round 2: tests for auditor 18 findings 1–3 and the spec examples of 0669c17 (lead decisions 34–37, 40). The prompt named the inputs and the test areas; it carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the spec text and examples of 0669c17, not from the code. 43 new test methods in five files: 24 fail on this head, each on "a code fix is offered" (`No code fix may be offered for the diagnostic at …`), so the failure is the specified difference and not markup or unrelated layout; 19 pass and pin the other side of each new condition.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH7004 `NoCodeFixForAnInterpolatedVerbatimStringWhoseSecondLineStartsAtAHole` (spec example; auditor `interp`) | RH7004-007 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| RH7004 `NoCodeFixForAnInterpolatedVerbatimStringWhoseSecondLineStartsAtTheClosingQuote` (auditor `f2`) | RH7004-007 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| RH7004 `NoCodeFixForAnAtDollarInterpolatedStringWhoseSecondLineStartsAtAHole` (`@$"`, auditor `f3c`) | RH7004-007 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| RH7004 `NoCodeFixForAnInterpolatedStringWhoseHoleSpansLines` (spec example; auditor `f4`) | RH7004-007 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| RH7004 `NoCodeFixForAnInterpolatedRawStringWhoseContentLineStartsAtAHole` (`$"""`, auditor `f3b`) | RH7004-007 | `VerifyNoCodeFixIsOffered` | passes (member that holds) |
| RH7004 `InterpolatedStringOnOneLineKeepsTheFix` (LF, CRLF) | RH7004-007 | `VerifyCodeFixWithEndOfLine` | passes (other side) |
| RH7004 `NoCodeFixForATabInFrontOfAMovedLine` (spec example) | RH7004-007 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| RH7004 `NoCodeFixForATabInFrontOfTheLineOfTheUsingDeclaration` (spaces then a tab; moved lines with spaces) | RH7004-007 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| RH7004 `TabBetweenTwoTokensOfAMovedLineKeepsTheFix` (LF, CRLF) | RH7004-007 | `VerifyCodeFixWithEndOfLine` | passes (other side; tab kept) |
| RH7004 `TabInsideAStringOnAMovedLineKeepsTheFix` | RH7004-007 | `Verify` | passes (other side) |
| RH7004 `TabOnABlankLineAmongTheMovedStatementsKeepsTheFix` (LF, CRLF; line written empty) | RH7004-007, SHARED-FIX-007 | `VerifyCodeFixWithEndOfLine` | passes (other side) |
| RH7004 `TabInFrontOfADirectiveLineAmongTheMovedStatementsKeepsTheFix` (line keeps its column and its tab) | RH7004-007, SHARED-FIX-007 | `Verify` | passes (other side) |
| RH7004 `NoCodeFixWhenTheMovedStatementsHoldALineBreakOtherThanTheDocumentsLineEnding` (CRLF with one LF; auditor `f12`) | SHARED-FIX-008 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| RH7004 `NoCodeFixWhenTheMovedStatementsHoldLoneCarriageReturnLineBreaks` (all `\r`; auditor `f10`) | SHARED-FIX-008 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| RH7004 `LineBreakOtherThanTheDocumentsLineEndingOutsideTheChangedCodeKeepsTheFix` (LF behind `using System.IO;` in a CRLF document; result CRLF, that LF kept) | SHARED-FIX-008 | `ApplyCodeFixAsync` + `Assert.AreEqual` (byte exact) | passes (other side) |
| RH7109 `NoCodeFixWhenARightShiftedFieldHoldsAnInterpolatedVerbatimStringWhoseSecondLineStartsAtAHole` (spec example; auditor `f5b`) | SHARED-ORDER-010, RH7109-006 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| RH7109 `NoCodeFixWhenARightShiftedFieldHoldsAnInterpolatedVerbatimStringWhoseSecondLineStartsAtTheClosingQuote` | SHARED-ORDER-010 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| RH7109 `NoCodeFixWhenARightShiftedFieldHoldsAnInterpolatedStringWhoseHoleSpansLines` | SHARED-ORDER-010 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| RH7109 `NoCodeFixWhenALeftShiftedFieldHoldsAnInterpolatedVerbatimStringOverTwoLines` | SHARED-ORDER-010 | `VerifyNoCodeFixIsOffered` | passes (member that holds: left shift) |
| RH7109 `FieldIndentedAsItsSlotMovesWithAnInterpolatedVerbatimStringOverTwoLines` (LF, CRLF; shift 0) | SHARED-ORDER-010 | `VerifyCodeFixWithEndOfLine` | passes (other side) |
| RH7109 `NoCodeFixWhenTheMovedFieldHoldsALineBreakOtherThanTheDocumentsLineEnding` (spec example) | SHARED-FIX-008 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| RH7109 `MovedFieldThatHoldsALineBreakMovesWhenEveryLineBreakIsAlike` (LF, CRLF; spec example's second sentence) | SHARED-FIX-008 | `VerifyCodeFixWithEndOfLine` | passes (other side) |
| RH7109 `NoCodeFixWhenTheMovedFieldHoldsALoneCarriageReturnLineBreak` (auditor `f9`) | SHARED-FIX-008 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| RH7109 `LineBreakOtherThanTheDocumentsLineEndingOutsideTheMovedCodeKeepsTheFix` (LF behind `class C`) | SHARED-FIX-008 | `ApplyCodeFixAsync` + `Assert.AreEqual` | passes (other side) |
| RH7107 `NoCodeFixWhenACommentStandsBetweenTheAccessorsOfAListOnOneLine` (spec example; auditor `f6`) | SHARED-ORDER-009, RH7107-005 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| RH7107 `NoCodeFixWhenACommentStandsBehindTheLastAccessorOfAListOnOneLine` (auditor `f11`) | SHARED-ORDER-009, RH7107-005 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| RH7107 `NoCodeFixWhenACommentStandsInFrontOfTheFirstAccessorOfAListOnOneLine` (auditor `f7`, decision 40) | SHARED-ORDER-009, RH7107-005 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| RH7107 `CommentInsideAnAccessorTravelsWithItWhenTheAccessorsSwapWithinTheLine` (LF, CRLF; spec example) | SHARED-ORDER-009, RH7107-004 | `VerifyCodeFixWithEndOfLine` | passes (other side) |
| RH7107 `CommentInsideTheExpressionBodyOfTheMovedAccessorTravelsWithIt` | SHARED-ORDER-009, RH7107-004 | `Verify` | passes (other side) |
| RH7107 `CommentBehindTheClosingBraceOfAListOnOneLineKeepsTheSwap` (comment outside the list) | SHARED-ORDER-009, RH7107-004 | `Verify` | passes (boundary of "in the list") |
| RH7107 `AccessorsOnOneLineSwapInADocumentWhoseLineBreaksAreLoneCarriageReturns` (every `\r` kept) | SHARED-FIX-008, SHARED-ORDER-009 | `ApplyCodeFixAsync` + `Assert.AreEqual` | passes (swap stays offered) |
| RH7107 `NoCodeFixWhenTheCrossedAccessorHoldsALineBreakOtherThanTheDocumentsLineEnding` (multi-line list; auditor `f13`) | SHARED-FIX-008 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| RH7107 `NoCodeFixForAnAccessorListOverSeveralLinesWhoseLineBreaksAreLoneCarriageReturns` | SHARED-FIX-008 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| RH7107 `NoCodeFixWhenARightShiftedAccessorHoldsAnInterpolatedVerbatimStringWhoseSecondLineStartsAtAHole` (auditor "accessor move traced") | SHARED-ORDER-010, RH7107-005 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| RH7108 `NoCodeFixWhenACommentStandsBetweenTheEventAccessorsOfAListOnOneLine` | SHARED-ORDER-009, RH7108-005 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| RH7108 `NoCodeFixWhenACommentStandsBehindTheLastEventAccessorOfAListOnOneLine` | SHARED-ORDER-009, RH7108-005 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| RH7108 `NoCodeFixWhenACommentStandsInFrontOfTheFirstEventAccessorOfAListOnOneLine` | SHARED-ORDER-009, RH7108-005 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| RH7108 `CommentInsideAnEventAccessorTravelsWithItWhenTheAccessorsSwapWithinTheLine` (LF, CRLF) | SHARED-ORDER-009, RH7108-004 | `VerifyCodeFixWithEndOfLine` | passes (other side) |
| RH3004 `FixWithheldForAnAtDollarInterpolatedStringWhoseContinuationLineStartsWithAHole` | RH3004-008 | `VerifyNoCodeFixIsOffered` | passes (node-based check holds; must keep holding when the check moves to Core) |
| RH3004 `FixWithheldForARegularInterpolatedStringWhoseHoleSpansLines` (`$"{x⏎+ 1}"`; existing tests cover only `$@"`) | RH3004-008 | `VerifyNoCodeFixIsOffered` | passes (same) |
| RH3004 `FixWithheldWhenTheBlockHoldsALineBreakOtherThanTheDocumentsLineEnding` (auditor `f14`, decision 39) | SHARED-FIX-008 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| RH3004 `FixWithheldWhenTheBlockHoldsLoneCarriageReturnLineBreaks` | SHARED-FIX-008 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| RH3004 `LineBreakOtherThanTheDocumentsLineEndingOutsideTheChangedCodeKeepsTheFix` (LF behind `using System;`) | SHARED-FIX-008 | `ApplyCodeFixAsync` + `Assert.AreEqual` | passes (other side) |

Mixed documents are built from LF text with `NormalizeToCarriageReturnLineFeed` and a `{LF}` marker at a line end replaced by a lone `\n`; lone-`\r` documents replace every `\n` with `\r`. The byte-exact assertions go through `ApplyCodeFixAsync`, which does not normalize line endings.

## Not covered, and why
- RH7108 lone-`\r` one-line swap and RH7108 multi-line mixed endings: RH7107 and RH7108 both reach the swap and the whole-line move through `AccessorOrderingUtilities.TryCreateMove` → `OrderingMoveSafety.TryCreateSwapWithinLine` / `TryCreateWholeLineMove` (`rg -n "TryCreateMove" src/Reihitsu.Analyzer.CodeFixes`), so one rule carries the line-ending arms. The comment arms are tested on both rules, as asked.
- RH7102/RH7103/RH7104–RH7106 member moves with interpolated strings or mixed endings: same shared `OrderingMoveSafety.TryCreateWholeLineMove` as RH7109 (auditor *Policy ownership*); RH7109 carries the arms.
- RH3004 with an interpolated string on the expression-body path (single `return`): existing `FixRewritesASingleReturnOfAnInterpolatedVerbatimStringAsAnExpressionBodyAndKeepsItsValue` covers it; nothing in this round changes it.
- RH3004 one-line block in a lone-`\r` document: the spec names the block-body path's refusal for moved code that holds a line break; a block on one line moves none, and no example states its output. Left out rather than guessed.
- RH7004 "new line breaks written as the detected ending, not the declaration line's": the declaration's own line break is inside the rewritten span, so any document that would tell the two apart is withheld by the mixed-ending refusal; no observable test separates them.
- Fix All: no new co-occurrence; the refusals are per diagnostic, and the batch bases already run each rule's scenario.
- FC walk: FC-003 (both sides of each new condition) covered above; FC-017 (assert exact bytes) by `ApplyCodeFixAsync`; FC-033 (LF/CRLF/sibling shapes) by the DataRows and the `@$"`, `$"""`, closing-quote and hole-spanning siblings; FC-034 (simplified cousin) answered by the auditor's own failing members. Formatter, rename, unsupported-shape, and directive-gap classes (FC-010–FC-014, FC-018, FC-023, FC-036, FC-038–FC-043) do not reach these fixes: no formatter phase, rename, or declared shape changes.

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7004UsingDeclarationsShouldNotBeUsedAnalyzerTests|FullyQualifiedName~RH7107PropertyAccessorsMustFollowOrderAnalyzerTests|FullyQualifiedName~RH7108EventAccessorsMustFollowOrderAnalyzerTests|FullyQualifiedName~RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests|FullyQualifiedName~RH3004UseLambdaSyntaxAnalyzerTests"
```
