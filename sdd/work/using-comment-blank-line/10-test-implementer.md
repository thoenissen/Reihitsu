Role: test implementer
Commit: a790245 (base); this handover's commit carries the tests
Verdict: reproduced

Scope: repair round 1 of thoenissen/Reihitsu#1497 — audit finding 1 (with the other members of its class), lead decisions 5 and 6, and audit finding 2. Read: `00-lead.md` (decisions 4–6, *Repair round 1*), `04-test-implementer.md`, `06-implementer.md`, `09-auditor.md`, the change doc `sdd/changes/2026-11/using-comment-blank-line.md`, FMT-USING-002/-003/-005/-006/-008/-010, SHARED-USING-008/-011, SHARED-FIX-003, RH7207-004. The prompt carried no intended fix or hoped-for verdict. Expected values come from FMT-USING-010 (blank line kept, exactly one), FMT-USING-008 (the comment stays directly above its directive), FMT-USING-005 (no line break added behind a spanning trailing comment for a directive without a comment), and RH7207-004 (the fix produces the phase's output); none comes from patched code. No production code and no existing test changed.

The auditor's inputs carry a space between `b */` and the next directive. That space shows up as trailing whitespace in the output, which is decision 6, not finding 1 or decision 5. So the reproductions use the same input with no space (`b */using System;`), and the auditor's verbatim inputs are separate decision-6 tests that also assert no trailing whitespace. If the lead moves decision 6 to a follow-up, only the tests named `NoTrailingWhitespace…` / `CodeFixLeavesNoTrailingWhitespace…` are affected.

## Tests

Formatter, using phase alone — `src/Reihitsu.Formatter.Test/Unit/UsingDirectives/UsingDirectivePhaseCommentBlankLineTests.cs` (`AssertResult`: LF and CRLF, plus a second run on its own output):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| HeaderBlankLineCountsForTheCommentThatMovesBehindATrailingCommentSpanningLines — finding 1, member (d) | FMT-USING-010 | phase alone | **fails**: no blank line above `// About Linq` |
| HeaderBlankLineAndWhitespaceOnlyLineBecomeOneBlankLineBehindATrailingCommentSpanningLines — member (e) | FMT-USING-010 | phase alone | passes |
| BlankLineAboveCommentIsKeptBehindATrailingCommentSpanningLines — member (b) | FMT-USING-010 | phase alone | passes |
| CommentOnALineOfItsOwnStaysThereBehindATrailingCommentSpanningLines — decision 5 | FMT-USING-008, FMT-USING-010 | phase alone | **fails**: `b */// About Linq` |
| DirectiveWithoutCommentGetsNoLineBreakBehindATrailingCommentSpanningLines — the other side of decision 5's condition | FMT-USING-005 | phase alone | passes: `b */using System.Linq;` |
| NoTrailingWhitespaceIsLeftWhenTheHeaderBlankLineIsKeptBehindATrailingCommentSpanningLines — decision 6, auditor's (d) input verbatim | FMT-USING-010 | phase alone | **fails**: `b */ ⏎// About Linq` (blank line lost and trailing space) |
| NoTrailingWhitespaceIsLeftWhenTheBlankLineAboveACommentIsKeptBehindATrailingCommentSpanningLines — decision 6, auditor's `f1` shape | FMT-USING-010 | phase alone | **fails**: only the trailing space (`b */ ⏎⏎// About Text`) |
| NoTrailingWhitespaceIsLeftWhenACommentOnALineOfItsOwnIsPlacedBehindATrailingCommentSpanningLines — decision 6, auditor's `k` input verbatim | FMT-USING-008 | phase alone | **fails**: `b */ // About Linq` (decision-5 relocation) |

Members (a) and (c) of the auditor's class are covered by existing tests: (a) by `BlankLineAboveCommentStaysWhen…` and the other tests in this file, and (c) by `HeaderBlankLineCountsForTheCommentThatMovesAwayFromTheTop`.

RH7207 code fix — `src/Reihitsu.Analyzer.Test/Formatting/RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzerTests.cs`. `Verify` checks the diagnostic, the fixed source, Fix All, and convergence. The CRLF variants use `ApplyCodeFixAsync`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| CodeFixKeepsHeaderBlankLineAboveCommentThatMovesBehindATrailingCommentSpanningLines — finding 1 (d), LF | RH7207-004 | `Verify(source, fixed)` | **fails**: the blank line is missing |
| …UnderCarriageReturnLineFeed — finding 1 (d), CRLF | RH7207-004 | `ApplyCodeFixAsync` | **fails**: `b */\r\n// About Linq` |
| CodeFixWritesOneBlankLineForHeaderBlankLineAndWhitespaceOnlyLineBehindATrailingCommentSpanningLines — (e) | RH7207-004 | `Verify` | passes |
| CodeFixKeepsBlankLineAboveCommentBehindATrailingCommentSpanningLines — (b) | RH7207-004 | `Verify` | passes |
| CodeFixKeepsCommentOnALineOfItsOwnBehindATrailingCommentSpanningLines — decision 5, LF | RH7207-004 | `Verify` | **fails**: `b */// About Linq` |
| …UnderCarriageReturnLineFeed — decision 5, CRLF | RH7207-004 | `ApplyCodeFixAsync` | **fails**: same |
| CodeFixLeavesNoTrailingWhitespaceWhenItKeepsTheHeaderBlankLineBehindATrailingCommentSpanningLines — decision 6 | RH7207-004 | `Verify` | **fails**: trailing space and the missing blank line |
| CodeFixLeavesNoTrailingWhitespaceWhenItKeepsTheBlankLineAboveACommentBehindATrailingCommentSpanningLines — decision 6 | RH7207-004 | `Verify` | **fails**: trailing space only |
| CodeFixLeavesNoTrailingWhitespaceWhenACommentOnALineOfItsOwnIsPlacedBehindATrailingCommentSpanningLines — decision 6 | RH7207-004 | `Verify` | **fails**: `b */ // About Linq` |

Sibling fix (the shared base, decision 5) — `src/Reihitsu.Analyzer.Test/Ordering/RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzerTests.cs`:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| CodeFixKeepsCommentOnALineOfItsOwnBehindATrailingCommentSpanningLines | RH7203-004 | `Verify(source, fixed, diag)` | **fails**: `b */// About Linq` |

Core, finding 2 — `src/Reihitsu.Core.Test/UsingDirectiveOrderingUtilitiesTests.cs`. These tests cover existing behavior, so all of them pass:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| IsLedByCommentIsTrueForSingleLineComment / …DelimitedComment / …SingleLineDocumentationComment / …DelimitedDocumentationComment / …CommentOnTheDirectivesLine | SHARED-USING-011 | direct call on parsed source | pass |
| IsLedByCommentIsFalseForWhitespaceAndLineBreaksOnly / …WithoutLeadingTrivia / …ForCommentTrailingThePredecessor | SHARED-USING-011 | direct | pass |
| HasBlankLineAboveIsFalseForOneLineBreakFromThePredecessor — 1 line break | SHARED-USING-008 | direct | pass |
| HasBlankLineAboveIsTrueForOneLineBreakFromThePredecessorAndOneFromTheDirective — 2 line breaks | SHARED-USING-008 | direct | pass |
| HasBlankLineAboveIsTrueForWhitespaceOnlyLine | SHARED-USING-008 | direct | pass |
| HasBlankLineAboveIsFalseForDirectiveSharingItsPredecessorsLine | SHARED-USING-008 | direct | pass |
| HasBlankLineAboveIsFalseBehindATrailingCommentSpanningLines (a blank line inside the comment does not count) | SHARED-USING-008 | direct | pass |
| HasBlankLineAboveCountsTheWholeLeadingTriviaWithoutSignificantTrivia | SHARED-USING-008 | direct | pass |
| HasBlankLineAboveIsTrueForBlankLineAboveTheLeadingComment / …IsFalseWithoutBlankLineAboveTheLeadingComment | SHARED-USING-011 | direct | pass |
| HasBlankLineAboveIgnoresBlankLineBelowTheLeadingComment / …IgnoresBlankLineBetweenTwoLeadingComments | SHARED-USING-011 | direct | pass |
| HasBlankLineAboveMeasuresCarriageReturnLineFeedLineBreaks (both sides of 1 vs 2) | SHARED-USING-011 | direct | pass |
| HasBlankLineAboveIsFalseForOneLeadingLineBreakBehindAPredecessorWhoseLineDoesNotEnd — the pipeline-only shape, built with `SyntaxFactory` | — (mechanism, untagged) | direct | pass |
| HasBlankLineAboveIsTrueForTwoLeadingLineBreaksBehindAPredecessorWhoseLineDoesNotEnd | — (mechanism, untagged) | direct | pass |

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Finding 1 (d): `// Header⏎⏎// About Linq⏎using System.Linq;⏎using System.Collections; /* a⏎   b */using System;` | LF, CRLF | phase alone (LF), RH7207 fix (LF, CRLF) | reproduced | observed `b */⏎// About Linq`; expected `b */⏎⏎// About Linq` |
| Decision 5: `using System.Collections; /* a⏎   b */using System;⏎// About Linq⏎using System.Linq;` | LF, CRLF | phase alone (LF), RH7207 fix (LF, CRLF), RH7203 fix (LF) | reproduced | observed `b */// About Linq`; expected `b */⏎// About Linq` |
| Decision 6, auditor's `f1` shape (space before the next directive) | LF | phase alone, RH7207 fix | reproduced | observed `b */ ⏎`; expected `b */⏎` |
| Members (b) and (e) of the finding-1 class | LF, CRLF (phase) | phase alone, RH7207 fix | not reproduced (they hold) | — |

The phase's `AssertResult` stops at the first line ending that fails, so for the failing phase tests only LF is observed. The CRLF failure is shown on the RH7207 fix path, which runs the same phase.

## Not covered, and why

- **RH7201, RH7202, RH7204, RH7205, RH7206 fixes.** The static proof from `04-test-implementer.md` still holds: every member of `UsingDirectiveOrderingCodeFixProviderBase` is `sealed override`, the six providers override nothing, and all of them call `UsingDirectiveCodeFixUtilities.OrganizeScopeUsingsAsync`, which runs `UsingDirectiveOrderingPhase`. RH7203 is the arm through the base, and RH7207 is the direct caller.
- **Whole-document formatting for these shapes.** The auditor found that `scripts/format.sh` already produces the correct layout, because later phases restore it. Finding 1 and decision 5 are code-fix-path defects. The phase-alone tests pin the phase output that the fixes return.
- **Finding 1 under the `Unchanged` arm** (a header comment with no blank line above it). That arm cannot occur: by construction the header ends with a blank line (`GetHeaderSplitIndex`).
- **A doubled line break in the predecessor's trailing trivia (`HasBlankLineAbove`).** Parsed source never produces it, and no pipeline phase that runs before the using phase writes it, so no caller reaches it (*Supported inputs*). I wrote a test for it and then removed it.
- **FC-005/FC-019/FC-020** (directives in the gap): any directive makes the block unsafe (SHARED-USING-009), and existing tests cover that. **FC-007/FC-008**: covered by `Verify`. **FC-010**: covered by the phase's second run. The other failure classes are dismissed as in `04-test-implementer.md`, because these tests add no new construct.

## Spec question for the lead

Decision 5's expected output, with the comment kept on its own line, follows the lead's decision. No requirement states it plainly:

- **SHARED-FIX-003 does not cover it.** Its *Layout does not count* bullet exempts "the whitespace around a comment". Turning the line break before `// About Linq` into nothing is such a whitespace change, so SHARED-FIX-003 read literally does not forbid main's output.
- **FMT-USING-005 permits not breaking.** It says a spanning block comment that trails the first of two new neighbors "counts" as their separation. `DirectiveWithoutCommentGetsNoLineBreakBehindATrailingCommentSpanningLines` pins that reading for a directive without a comment (`b */using System.Linq;`).
- **FMT-USING-008 is the nearest requirement.** It says the comment must "stay directly above" its directive, and main's output still has the comment on the line above the directive.

I tagged the decision-5 tests FMT-USING-008 (phase) and RH7203-004/RH7207-004 (fixes). I recommend that the spec author add one sentence to FMT-USING-008: a leading comment that starts its line in the source still starts its line after the rebuild. That makes the expected output derivable, and the spec checker can then confirm the tag.

Decision 6 has a similar gap. FMT-USING-003 ("replacing the whitespace that separated the two") is the closest requirement, but it speaks of a directive that shares its predecessor's line, not of whitespace that is left behind by a reorder. So I tagged the decision-6 tests with the requirement of the shape they extend, and none for the whitespace itself.

## Follow-up candidates

None.

## Architecture findings

None beyond decision 4.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~UsingDirectivePhaseCommentBlankLineTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzerTests|FullyQualifiedName~RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzerTests"
scripts/test.sh --project core --filter "FullyQualifiedName~UsingDirectiveOrderingUtilitiesTests"
scripts/test.sh --project architecture
```

Results on `a790245` with these tests:

- formatter filter: 5 failed, 23 passed.
- analyzer filter: 8 failed, 97 passed.
- core filter: 40/40 passed.
- architecture: 92/92 passed.
