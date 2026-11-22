Role: implementer
Commit: (this commit; parent a92a8ca)
Verdict: tests pass

Architecture repair, decision 17 in `00-lead.md` (supersedes decision 16), read with decisions 19 and 20, auditor finding #3 (`17-auditor.md`), and the amended FMT-BREAK-044 (`18-spec-author.md`). The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond decision 17; nothing was ignored. The summaries finding #4 names (`DocumentationCommentFormattingPhase`) are untouched; they are the cleaner's.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Rewriter/ControlFlowBraceTransform.cs` | The round-1 per-brace guard is removed: `WrapWithBraces` loses `forbidsLineBreak` and the separator-based early return, and its summary loses the FMT-BREAK-044 sentence. New private static `IsInSingleLineInterpolationWithDocumentationComment(ifStatement, languageVersion)`: true when the `if` stands in a non-verbatim interpolation below C# 11 (`IsInsideSingleLineInterpolation`) and some interpolation ancestor that is, or stands inside, such an interpolation holds a documentation comment anywhere in its descendant trivia (`SyntaxTriviaUtilities.IsDocumentationCommentTrivia`). `VisitIfStatement` reads it on the original node and, when true, returns the visited node with neither body braced, so every nested `if` in the same interpolation stays unbraced too | FMT-BREAK-044 (structural part, decisions 17, 19, 20), FMT-STRUCT-001/026 |

Why the ancestor test is the spec's "any non-verbatim interpolation that contains the body" (decision 19): an interpolation that holds the comment and stands inside a `$"` interpolation passes the check, and that `$"` interpolation holds the same comment; conversely a `$"` interpolation itself passes `IsInsideSingleLineInterpolation`. A `$@"` interpolation outside every `$"` one does not pass, so a comment there refuses nothing. A comment in a sibling interpolation of the same string is in no ancestor of the body (decision 20, example `w`). The check is cheap: the predicate already walks the ancestors, and the trivia scan runs only for an `if` that is inside a `$"` hole below C# 11.

`GetTokenBehindInsertedCloseBrace`/`GetTokenBehindIfStatement` are unchanged and their summaries still hold: inside a refused interpolation no brace is inserted, so their prediction of an outer inserted `}` is never read there; outside it, nothing changed.

## Policy owners
- `rg -n "IsInsideSingleLineInterpolation|CanInterpolationsHoldLineBreaks" src --type cs`: the C# 11 boundary still only in `StringInterpolationUtilities.CanInterpolationsHoldLineBreaks`, the `$"` hole predicate only in `IsInsideSingleLineInterpolation`; the new check calls it, no copy of the verbatim test.
- `rg -n "IsDocumentationComment" src --type cs`: the doc-comment kind test reuses Core `SyntaxTriviaUtilities.IsDocumentationCommentTrivia`; `CommentPositionUtilities.IsDocumentationComment` (analyzer) and `DocumentationCommentFormattingPhase.IsMovableDelimitedDocumentation` are pre-existing and untouched.
- FMT-STRUCT-026 separators: decided only in `WrapWithBraces`, now without any refusal branch.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src` before this commit: 9 files changed, 192 insertions(+), 37 deletions(-). Delta of this commit: 1 file changed, 31 insertions(+), 16 deletions(-). No new type, no new public member.

## Deleted special cases
_N/A._ (No shape declared unsupported. The round-1 per-brace refusal in `WrapWithBraces` is deleted, as decision 17 asks.)

## Tests run
- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~NonVerbatimInterpolationLineBreakTests"`: before, 11 failed as `19-test-implementer.md` says; after, Passed 79 of 79.
- `scripts/test.sh --configuration Debug --project formatter`: Passed 7267 of 7267.
- `scripts/test.sh --configuration Debug --project architecture`: Passed 198 of 198.
- `scripts/test.sh --configuration Debug --project analyzer`: Passed 11886 of 11886.
- `scripts/test.sh --configuration Debug --project cli`: Passed 441, Skipped 1.
- `scripts/format.sh` on the file: 0 of 1 changed; no build warning on it (a first draft as an instance method drew RH7103, so the method is static).

## Disputed tests or scope
_None._

## Architecture findings
_None._ Decision 17 removes the prediction-dependent per-brace refusal the auditor described; the refusal no longer depends on what the transform does to an outer body.

## Follow-up candidates
_None_ beyond those already in `00-lead.md`.
