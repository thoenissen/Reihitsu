Role: spec author
Commit: 935d8d1
Verdict: text written

## Changed requirements

None. No intended behavior changes, so no change doc and no spec diff (`sdd/README.md`, *When a change doc is required*).

Text changes:

- `src/Reihitsu.Formatter/ReihitsuFormatter.cs` — the `<summary>` of `ReihitsuFormatter.FormatNodeInDocumentWithContextAsync` no longer calls the context "positional context". It now says the phases format the containing node so the target is laid out in the code around it, and that the containing node decides no column of the target's first token, which keeps its line and column in the document (FMT-PIPE-017, FMT-PIPE-018). This agrees with the method's `<remarks>` and with FMT-PIPE-023/024 (the context is the root the phases format and supplies the depth fact; the target-position fact supplies the first-token column).
- `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5111AssignmentsMustHaveProperLineBreaksCodeFixProvider.cs` — the `<summary>` of `GetFormattingContextNode` said the context's "tokens provide positional context while formatting"; it now says the node is handed to the formatter as the code around the formatting node. The rest of that summary (the chain decides the wrapped links' column, RH5111-006) is unchanged.

Text-only proof (`scripts/verify-text-only.sh --head worktree`):

```
TEXT-ONLY PROOF: PASS — base=31d2931 head=worktree — 3 changed file(s) (1 non-compiled, 2 C# comment/layout-only); xml-doc impact: 2 file(s)
```

Both C# files: tokens identical, directives and disabled text identical, changed trivia: documentation. `scripts/format.sh` on both files changed nothing.

## Examples for the tests

None — docs lane, no behavior.

## Output questions

_None._

## Internal choices

- The issue says the `ReihitsuFormatter` summary is "the remaining occurrence" of the stale "positional context" wording. It is not: `RH5111AssignmentsMustHaveProperLineBreaksCodeFixProvider.GetFormattingContextNode` carries the same wording for the same cause (the context passed to `FormatNodeInDocumentWithContextAsync`). I reworded it in this run as well, since it is the same text-only defect and keeps the wording consistent with the #1871 change to `FluentChainCodeFixHelper`. Alternative: leave it and file a follow-up. Other hits of "positional" in `src/` (`TokenLocator`, `NameColonSpacingRule`, `CasingRenameSafetyUtilities`, `CasingCodeFixProviderBase{T}`) mean positional subpatterns, record parameters, or token guesses, not the formatting context, and stay.
- No rule page changes: nothing a user sees changes.

## Follow-up candidates

_None._
