Role: implementer
Commit: cb63799
Verdict: tests pass

My prompt named the issue, the handovers, and the decisions to read; it carried no intended fix or suspected cause.

The 21 tests named in 04-test-implementer.md failed on cb63799 as described: 14 in `DelimitedDocumentationRelocationPhaseTests`, 7 in `BlankLinePhaseDocumentationCommentTests` (`Failed: 21, Passed: 14`). They pass now. No test was changed.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/DocumentationComments/DocumentationCommentFormattingPhase.cs` | `RegisterRelocation` accepts both documentation-comment kinds (`SyntaxTriviaUtilities.IsDocumentationCommentTrivia`). A delimited comment must also pass the new `IsMovableDelimitedDocumentation`: it is the first comment of its token's leading trivia, and the preceding token, resolved through `PrecedingTokenFacts.Resolve` (so `RootPrecedingToken` at node level), is neither `{` nor a switch-label colon (`BlankLineEditor.IsFirstInBlock`). The existing written-behind, `DocumentsFollowingCode`, and relocation mechanics (line break on the previous token, or in the owning token's leading trivia) are reused unchanged. The XML normalization pass still acts on `///` only. Class and method summaries updated. | FMT-DOC-001, FMT-DOC-003, FMT-DOC-012 |
| `src/Reihitsu.Formatter/Pipeline/BlankLines/Rewriter/BlankLineTriviaBoundaryRewriter.cs` | `IsStrandedInlineDocumentation` became `IsInlineDocumentation`: the boundary is skipped for every documentation comment that opens the leading trivia and does not start its line, no longer only for one that documents nothing. The phase therefore inserts no line break in front of a documentation comment of either form; a comment that starts its line still gets FMT-BLANK-011's blank line. The private `FindFirstCommentIndex` was removed in favor of the shared helper. Remarks of `EnsureBlankLineBeforeFirstComment` updated. | FMT-BLANK-012 |
| `src/Reihitsu.Formatter/Pipeline/Core/Utilities/TriviaListUtilities.cs` | New internal `IndexOfFirstComment(SyntaxTriviaList)`, moved out of the blank-line rewriter so both phases read the "first comment of the leading trivia" fact from one owner (decision #8). | FMT-DOC-001, FMT-BLANK-012 |
| `AGENTS.md` | Phase table guard names: blank-line row `IsStrandedInlineDocumentation` → `IsInlineDocumentation`; documentation row gains `IsMovableDelimitedDocumentation`. | — |

## Policy owners

- "First comment of the leading trivia": `rg -n "FirstComment|IndexOfFirst" src --type cs -g '!*Test*'` before the change found one owner, `BlankLineTriviaBoundaryRewriter.FindFirstCommentIndex` (private). Now one owner, `TriviaListUtilities.IndexOfFirstComment`, used by the blank-line rewriter (twice) and the documentation phase. RH5020's `IsFirstCommentInBlock` is a different fact (block start) on the analyzer side.
- "Preceding token is `{` or a switch-label colon": `rg -n "IsFirstInBlock|IsSwitchLabelColon" src/Reihitsu.Formatter --type cs` found one formatter owner, `BlankLineEditor.IsFirstInBlock(PrecedingTokenFacts)` over `PrecedingTokenFacts.IsSwitchLabelColon`. The documentation phase calls it; no copy added. RH8303's private `IsFirstInBlock` is a pre-existing analyzer-side copy (not touched).
- "Documentation comment kind": `SyntaxTriviaUtilities.IsDocumentationCommentTrivia` (Core), reused.
- "Written behind code" and "documents following code": `DocumentationCommentUtilities.IsWrittenBehindSource` / `DocumentsFollowingCode`, and `IsWrittenBehindRootPrecedingToken` for node level, all reused unchanged.

## Tests run

All in Debug on the working tree before commit.

```shell
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~DelimitedDocumentationRelocationPhaseTests|FullyQualifiedName~BlankLinePhaseDocumentationCommentTests"
```
Before the change: `Failed: 21, Passed: 14, Total: 35`.

```shell
scripts/test.sh --configuration Debug
```
- Analyzer: `Passed: 6330, Failed: 0`
- Formatter: `Passed: 3494, Failed: 0`
- Core: `Passed: 535, Failed: 0`
- CLI: `Passed: 251, Failed: 0`
- Architecture: `Passed: 92, Failed: 0` (rerun after the `AGENTS.md` edit: 92/92)
- Tooling: `Passed: 125, Failed: 0`
- Playground: `Passed: 17, Failed: 0`

`scripts/format.sh` over the three changed source files: `Formatted 0 of 3 file(s)`. `scripts/build.sh --configuration Debug`: 0 warnings, 0 errors.

## Disputed tests or scope

_None._ Three production files, as the handovers name (documentation phase, blank-line rewriter) plus the shared helper the decision #8 single-owner requirement needed. The helper is internal and has two consumers, both changed by this run.

## Architecture findings

_None new._ Decision #8's duplicate ownership is resolved: the blank-line phase no longer decides a line break in front of any documentation comment, and the documentation phase reads the first-comment and `{`/switch-label facts from their existing owners. Brace placement keeps the `{ /** d */ code` case (decision #5); no change there.

## Follow-up candidates

_None._ (RH8303's private `IsFirstInBlock` predates this run and is on the analyzer surface; it is not a defect this change depends on.)
