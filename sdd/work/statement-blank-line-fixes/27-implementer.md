Role: implementer
Commit: (this commit; parent 30237c4)
Verdict: test disputed

Scope: architecture decisions #28, #29, #31 and #32 (`00-lead.md`). The specs are SHARED-STMT-004 and SHARED-AFTERBRACE-002 at 9c87631, and the tests are 25-test-implementer. My prompt named the change asked for, which is the lead's decisions themselves. It carried no suspected cause and no hoped-for verdict.

Result: all 12 failing tests of 25 now pass. The four RH5029 tests from 25's table pass, and so do the holding tests. One test from before the run, `VerifyDiagnosticDoesNotCompensateWhenSwitchLabelItselfIsNotFirstOnItsLine`, now fails because the committed spec refuses its input. See *Disputed tests or scope*.

## thoenissen/Reihitsu#946, #972, #1096, #1076, #1626

Nothing to do: these decisions concern none of these issues. The owner change below belongs to the #1626 owner. Its callers outside an initializer keep their outputs.

## thoenissen/Reihitsu#1625

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Core/BlankLineCodeFixUtilities.cs` | Anchor-scope branch of `TryGetIndentation`: it finds the first token that starts on the anchor line (`root.FindToken(line.Start)`, or the next token when that one starts earlier). The new `TryGetListIndentation` classifies that token and replaces `TryGetSwitchSectionIndentation` and `IsWithinLabelContinuation`. (1) The token must start its line under the comment-tolerant definition (`StartsLineBehindComments`), otherwise the method refuses. (2) A token inside a label of the same switch that is not the label's first token makes the line a continuation line: the fallback is a statement of the own section, other than the target, that starts its line under the same definition. Without one, the method refuses. (3) The first token of a list statement (the target's block, or any section of the same switch) or of a label of the same switch gives a column through the existing `SyntaxIndentationUtilities.ComputeAnchorDerivedIndentation`, which adds one level for a label. (4) Anything else refuses. New private helpers: `StartsLineBehindComments` (no earlier token reaches into the line, and in front of the token only whitespace, plus comments whose span starts on that line) and `GetLineIndentation`. Summaries of `CanInsertBlankLineBefore` and `TryGetIndentation` updated | SHARED-STMT-004, SHARED-AFTERBRACE-002 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/ExpressionBodyToBlockCodeFixProviderBase{TNode}.cs` | Inline comment above the registration condition. It now covers all three refusals: async preservation, unsupported syntax, and the derived provider's `CanRegisterCodeFix` | decision #29 |

Both callers go through the owner's `CanInsertBlankLineBefore`. The statement base and RH3208 call it at registration, so both withhold their fix in the refused class without any change of their own. RH5030 never reaches the rebuild path (23-auditor).

Comment-tolerant "starts its line", as spec 26 defines it: a comment counts when its whole span starts on the line. The tail `b */` of a comment that starts earlier crosses the line start, so it refuses. A multi-line token reaching into the line (`previousToken.Span.End > lineStart`) also refuses. Any other trivia in front of the token on that line refuses too, but none can stand there except whitespace or comments.

### Policy owners
- `rg -n "CanInsertBlankLineBefore\(|TryInsertBlankLineBefore\(" src --glob '!*Test*/**'`: one owner, `BlankLineCodeFixUtilities`. Callers: the statement base, RH3208, and RH5030. Unchanged.
- `rg -n "ComputeAnchorDerivedIndentation" src --glob '!*Test*/**'`: owner `SyntaxIndentationUtilities` (Core). Callers: this owner and RH5103. It is still called, not copied, for the label column one level deeper.
- `rg -n "StartsLine|IsCommentTrivia" src --glob '!*Test*/**'`: `SyntaxTokenPositionUtilities.StartsLine` is the strict definition (SHARED-BLANK-008, no comment allowed), and `AttributeTargetUtilities.StartsLine` is a private copy for another purpose. No existing owner of SHARED-STMT-004's comment-tolerant definition exists, so `StartsLineBehindComments` is its only statement. It calls `SyntaxTriviaUtilities.IsCommentTrivia`.

### Production diff size
- This round: `git diff --shortstat -- src`: 2 files changed, 84 insertions(+), 57 deletions(-). That is under 100 production lines. No new type and no new non-private member.
- Run: `git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` (before this commit): 25 files changed, 568 insertions(+), 291 deletions(-).

### Deleted special cases
_N/A._

### Disputed tests or scope
- `RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzerTests.VerifyDiagnosticDoesNotCompensateWhenSwitchLabelItselfIsNotFirstOnItsLine` (from `main`, not in 25's table) now fails.
  - What the test asserts: the RH5029 fix applies to the input below and puts `var y = 1;` in the `switch` line's column. Its own summary says "No requirement decides this shape; the test characterizes today's output".

    ```csharp
    Handler = value =>
    {
        switch (value) { case 0: Method(1); var y = 1; break; }
    }
    ```

  - What the spec says: SHARED-STMT-004 at 9c87631 says the anchor line, where `Method(1);` starts, is started by `switch`. That token is neither the first token of a statement of the section's list nor a label: "A statement counts here only when it is a statement of the block's or section's own list". The spec therefore says "MUST NOT be offered". No reading of bullets 1–3 admits the line, so the spec and the test cannot both hold.
  - Size: the class is rare, since the input already violates RH5103. The 01-spec-author follow-up candidate ("a switch written on one line inside an initializer is undecided") is now decided by the committed wording.
  - Options: (a) the test implementer changes the test to `VerifyNoCodeFixIsOffered`. This is consistent with decisions #28 and #30, which withdraw the fix from rare cells offered today, and nothing in the code changes. (b) The spec author adds the enclosing `switch` statement's first token to the line starters, and I add one condition. That adds a column the test summary itself calls a guess ("no whitespace run equals the label's column"). I recommend (a). I did not change the test.

### Architecture findings
_None._ The owner now keys on what starts the anchor line, which closes the axis that #1, #4 and #5 shared as a whole.

### Follow-up candidates
_None_ new.

## Whole run

### Tests run
- `scripts/build.sh --configuration Debug`: 0 Warning(s), 0 Error(s).
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH3208|FullyQualifiedName~RH5029|FullyQualifiedName~RH5002|FullyQualifiedName~PrecededByABlankLine|FullyQualifiedName~RH5030|FullyQualifiedName~RH5011"`: Failed: 1, Passed: 491 (the disputed test).
- `scripts/test.sh --configuration Debug --project analyzer`: Failed: 1, Passed: 8233 (the same disputed test only).
- `scripts/format.sh` over both changed files: Formatted 0 of 2 file(s), after the first format pass on the owner.
