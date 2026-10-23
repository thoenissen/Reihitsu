Role: test implementer
Commit: 118743a
Verdict: tests written

Repair round (worklist in `00-lead.md`, "Repair round"): the test side of item 1 and item 3, plus a covering test for the new FMT-PIPE-007 example. Specs and tests for existing behavior: every test pins a conforming shape only and passes on the unchanged code. No production code was touched. My prompt carried no intended fix or verdict.

`␍` is a lone `\r` and `⏎` is `\n`.

Blockers: none. No test I expected to pass failed; none was adjusted to the code.

## Item 1: SHARED-FIX-008, narrowed

- **Existing tagged test checked against the narrowed text.** `RH5405BracesMustNotBeOmittedAnalyzerTests.FixWritesLineFeedInADocumentWhoseLineBreaksAreLoneCarriageReturns` did not contradict the narrowed text: its three created breaks are `\n`. But its exact-string assertion also pinned `}␍`, the kept break behind the closing brace. The narrowed requirement no longer states that, and it is open question Q-1380-1 on thoenissen/Reihitsu#1408. The summary and inline comment also described the removed clause.
- **Changed:**
  - The test now asserts the layout with every line break read as `\n`.
  - It asserts that the three created breaks are `\n` (`if (a)⏎{⏎M(a, b);⏎}`).
  - It no longer asserts the line ending of a kept break.
  - The summary and inline comment now claim only the created breaks.
  - A private helper, `NormalizeEveryLineBreakToLineFeed`, was added to the class's `Methods` region.
  - The assertion is exactly as strong as the requirement (FC-017). It fails if a created break is `\r` or `\r\n`, or if the layout changes.
- **Conforming shapes the narrowed text covers, added:**
  - RH5417, the issue's P35 example. The auditor verified it and it was untested. Created breaks: behind `void M()` and behind `{`.
  - RH5406 with a body that spans two lines. This is the formatter-delegating shape that led to the narrowing. Its created breaks are asserted: behind `if (a)`, behind `{`, and behind `b);`. The break inside the body (`M(a,` / `b);`) is carried over from the input, so it is not asserted.
- **Not tagged:**
  - RH5405 with the body on the next line. It is unclear which of its output breaks are created and which are kept.
  - RH5011 and RH8304 all-`␍` shapes. They are non-conforming (Q-1380-2).

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5405BracesMustNotBeOmittedAnalyzerTests.FixWritesLineFeedInADocumentWhoseLineBreaksAreLoneCarriageReturns` (rewritten assertion, summary, comment) | SHARED-FIX-008 (narrowed); FC-017, FC-033 | `ApplyCodeFixAsync` (no normalization) | pass |
| `RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzerTests.FixWritesLineFeedInADocumentWhoseLineBreaksAreLoneCarriageReturns` (`void M() { }␍` → created `void M()⏎{⏎}`) | SHARED-FIX-008; FC-033 | `ApplyCodeFixAsync` | pass |
| `RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzerTests.FixWritesLineFeedInADocumentWhoseLineBreaksAreLoneCarriageReturns` (`if (a) M(a,␍ b);␍`) | SHARED-FIX-008; FC-033 | `ApplyCodeFixAsync` | pass |
| `RH5201MethodChainsShouldBeAlignedAnalyzerTests.VerifyLinkInAnchorColumnBehindMisplacedBlockCommentIsDetectedWithCrLf` | SHARED-COMMENT-011, RH5201-002 | analyzer `Verify`, CRLF | pass |
| `RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzerTests.VerifyMemberInItsColumnBehindMisplacedBlockCommentIsDetectedWithCrLf` | SHARED-COMMENT-011, RH5301-002 | analyzer `Verify`, CRLF | pass |
| `RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzerTests.VerifyElementInItsColumnBehindMisplacedBlockCommentIsDetectedWithCrLf` | SHARED-COMMENT-011, RH5303-002 | analyzer `Verify`, CRLF | pass |
| `RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzerTests.VerifyElementInItsColumnBehindMisplacedBlockCommentIsDetected` (`/* c */` in column 0, element `1` in its column 37; diagnostic on the whole assignment) | SHARED-COMMENT-011, RH5304-002, RH5304-003 | analyzer `Verify` | pass |
| `RH5304…Tests.VerifyElementInItsColumnBehindMisplacedBlockCommentIsDetectedWithCrLf` | same | analyzer `Verify`, CRLF | pass |
| `ReihitsuFormatterTests.FormatSyntaxTreeWritesLineFeedWhenLoneCarriageReturnsOutnumberCarriageReturnLineFeed` (FMT-PIPE-007 example 2: `"class C\r{\rvoid M(){}\r\n}"` → `\n` throughout) | FMT-PIPE-007; FC-003, FC-033 | public `FormatSyntaxTree`; the string is built in the test | pass |
| `ReihitsuFormatterTests.FormatSyntaxTreeWritesCarriageReturnLineFeedWhenLoneCarriageReturnsTieWithCarriageReturnLineFeed` (the example's tie sentence: a second `\r\n` behind `}` → `\r\n` throughout, no final break) | FMT-PIPE-007; FC-003 (other side of the count) | public `FormatSyntaxTree` | pass |

Notes on the tests:
- **Expected values.** They come from the spec text and examples. The FMT-PIPE-007 layout is the example's output. The missing final break in the tie case follows the first example's note that FMT-CLEAN removes it.
- **Example 2 discriminates.** If a lone `\r` were not counted, the count would be 0:1, and the result would be `\r\n`.
- **The tie test** asserts `\r\n` on a 2:2 count. If the tie rule flipped, it would fail.
- **The other side of SHARED-COMMENT-011 is already covered.** For each rule, a test with the comment in the column the line belongs in is not reported: RH5201 `VerifyNoDiagnosticForLinkBehindBlockCommentStartingTheLineInTheAnchorColumn` (LF and CRLF), and the RH5301, RH5303 and RH5304 `VerifyNoDiagnosticForCommentPrefixed…` tests.

Files:
- `src/Reihitsu.Analyzer.Test/Formatting/RH5405BracesMustNotBeOmittedAnalyzerTests.cs`
- `src/Reihitsu.Analyzer.Test/Formatting/RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzerTests.cs`
- `src/Reihitsu.Analyzer.Test/Formatting/RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzerTests.cs`
- `src/Reihitsu.Analyzer.Test/Formatting/RH5201MethodChainsShouldBeAlignedAnalyzerTests.cs`
- `src/Reihitsu.Analyzer.Test/Formatting/RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzerTests.cs`
- `src/Reihitsu.Analyzer.Test/Formatting/RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzerTests.cs`
- `src/Reihitsu.Analyzer.Test/Formatting/RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzerTests.cs`
- `src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterTests.cs`

## Not covered, and why

- **SHARED-FIX-008, kept breaks in an all-`␍` document.** The narrowed requirement no longer states them, and they are open question Q-1380-1. No test pins them in either direction.
- **SHARED-FIX-008, RH5011 and RH8304 all-`␍` shapes.** They are non-conforming (Q-1380-2, decided by P35), so they are not tagged.
- **SHARED-FIX-008, RH5405 with the body on the next line.** Which of its output breaks are created is not determined by the spec text, so pinning any of them would assert more than the requirement says.
- **RH5304 fix result on the misplaced-comment shape.** It is not asserted. SHARED-COMMENT-011 covers the report; RH5304-004 owns the fix. The same holds for the RH5301 and RH5303 CRLF variants.
- **FC-008 (Fix All), and the renaming, literal and identifier classes.** No fix or rewrite changed in this run.

## Commands

```shell
scripts/test.sh --project analyzer --filter "Name~FixWritesLineFeedInADocumentWhoseLineBreaksAreLoneCarriageReturns|Name~BehindMisplacedBlockCommentIsDetected"
scripts/test.sh --project formatter --filter "TestCategory=FMT-PIPE-007"
scripts/test.sh --project architecture --filter "FullyQualifiedName~RequirementCoverageTests|FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"
```

Results on the working tree:
- **Analyzer, by name:** 13 passed. That is the 11 new or rewritten tests, plus the two RH5204 tests that match the name filter.
- **Analyzer, whole classes:** the seven touched classes passed, 200 tests.
- **Formatter, `FMT-PIPE-007`:** 19 passed, which equals the tag count in the project.
- **Architecture, the three named classes:** 39 passed. The whole architecture project: 72 passed.
- **Formatting:** `scripts/format.sh` was run on the eight changed test files: "Formatted 0 of 8 file(s)".
