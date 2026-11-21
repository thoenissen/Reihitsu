Role: spec author
Commit: 0021f75
Verdict: text written

## Changed requirements

- FMT-INDENT-012 — Wrapped argument and parameter lists align to the opening delimiter — clarified. "element-access arguments" became "bracketed arguments"; a new paragraph states the cause in syntax-tree terms: a `BracketedArgumentListSyntax` aligns behind its `[` whatever its parent is (`ElementAccessExpressionSyntax`, the `ElementBindingExpressionSyntax` of a conditional element access, the `VariableDeclaratorSyntax` of a fixed-size buffer), except the key of an indexer initializer (`ImplicitElementAccessSyntax`), which FMT-INDENT-014 owns. A second example shows all three parents. The issue's "every bracketed argument list" was wrong in that one case: `ArgumentAlignmentContributor.Contribute` sends `BracketedArgumentListSyntax { Parent: ImplicitElementAccessSyntax }` to `AlignDictionaryKey` (block indentation, test `ArgumentAlignmentContributorTests.AlignsMultiLineDictionaryIndexerKey`, tagged FMT-INDENT-014).
- SHARED-DIAG-010 — A layout rule does not report a shape the formatter keeps — clarified. "its fix is withheld (SHARED-FIX-010)" became "its fix is withheld where the node the fix formats holds the shape (SHARED-FIX-010)", followed by one sentence with the RH5307 case where only the rest of the document holds the shape and the fix is still offered. No new requirement-ID reference (an `FMT-UNSUP-014` reference was tried and dropped: it would need a `Uses` row; the sentence describes the refusal in plain words instead).

How established:
- FMT-INDENT-012: `scripts/format.sh` on the new example's input (scratchpad fixture) gives exactly the example's output, and a second run changes nothing. Existing tests agree: `ConditionalElementAccessArgumentListTests.MultiLineArgumentKeepsConditionalElementAccessListWrapped`, `.BlockCommentBetweenBracketsKeepsConditionalElementAccessListAsWritten`, `.FixedSizeBufferSizeListStaysWrapped`, `ArgumentAlignmentContributorTests.AlignsBracketedArgumentList`.
- SHARED-DIAG-010: `scripts/apply-fix.sh RH5307` over `d /* c */ [1,⏎2]`, `c /* c */ ?[1,⏎2]`, and `c?⏎// c⏎[1,⏎2]`: 6 fixed under LF and CRLF, each joining the list and leaving the comment in place. `scripts/format.sh` on the first refuses the file ("unsupported syntax: a comment or directive in front of an argument list, an element access, or a null-forgiving operator", file unchanged). `RH5307IndexerBracketedArgumentsShouldBeSingleLinedCodeFixProvider.RegisterCodeFixesCoreAsync` asks `FindUnsupportedSyntax` about the bracketed list only. Existing tests: `RH5307...AnalyzerTests.VerifyFixJoinsConditionalElementAccessListWithCommentInFrontOfQuestionMark`, `...WithCommentBehindQuestionMark`, `...BehindCommentLine`.
- `scripts/test.sh --project architecture`: 198 passed.

No output changes; no change doc required. No rule page touched (no user-visible behavior changes).

## Examples for the tests

None required: docs lane, no output change. The new FMT-INDENT-012 example matches behavior the tests above already cover (they are not tagged FMT-INDENT-012; tagging them is test-file work outside this lane).

## Output questions

_None._

## Internal choices

- FMT-INDENT-012: named the three parents and the `ImplicitElementAccessSyntax` exception instead of the issue's "bracketed argument lists in general"; alternative was the bare general wording, which would contradict FMT-INDENT-014.
- FMT-INDENT-012: added a second example with the three parents (FC-002, one example per side: the exception is shown by reference to FMT-INDENT-014's own example rather than a new block); alternative was text only.
- SHARED-DIAG-010: added a one-sentence RH5307 illustration besides the issue's wording; alternative was the wording change alone.

## Follow-up candidates

_None._

## Whole run

Lane check: `LANE CHECK: confirmed — claimed docs, diff needs docs` (`scripts/lane-check.sh docs --head worktree`). No C# file changed, so `verify-text-only.sh` has nothing to prove.
