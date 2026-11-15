Role: implementer
Commit: see the commit that adds this file (parent 4e60d8d)
Verdict: tests pass

Repair round 1. The prompt carried the lead's worklist (decisions 14, 15, 17, 18) and the auditor's hint for decision 17; it carried no intended fix or hoped-for verdict beyond those references, and I worked from the handovers and specs.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Utilities/TrailingCommaRemovalUtilities.cs` | Restored the leading-trivia guard `ContainsNonFormattingTrivia(separator.LeadingTrivia)` that the run had deleted (decision 14). It is not `main`'s code verbatim: `main` appended the comma's leading trivia to the last element's **trailing** trivia, which puts a documentation comment where no parse of the text puts it. `AssertRuleResult`'s FMT-PIPE-021 check failed on that (`trailing trivia of 'Large' differ: [/** largest */] in the tree, [] in the reparsed text`). When the comma's leading trivia holds more than whitespace and line breaks, the comma's leading and trailing trivia now lead the closing brace, in their order. The text is the same as on `main`, and the trivia sits where a reparse puts it. `GetTriviaToPreserve` is gone: its two branches are now the early return and the existing trailing-trivia path. Summary rewritten (FC-031). | FMT-STRUCT-019, FMT-STRUCT-020, FMT-UNSUP-009 `Why:`, FMT-PIPE-021 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5107CommaMustBeOnSameLineAsPreviousParameterCodeFixProvider.cs` | The guard `token.TrailingTrivia.Any(IsCommentTrivia)` became `HasCommentBehindCommaOnItsLine(token)`. It returns true when the comma's trailing trivia holds a comment before a line break. When that trivia has no line break, it also scans the next token's leading trivia up to the first `EndOfLineTrivia`: under `Parse`/`Diagnose`, `/** */` and `///` written behind the comma lead the next token. Cheap trivia check only. Class summary updated. | RH5107-004 (decision 15) |

Decision 18: no `src` change; the characterization rows stay green.

Decision 17 (zero-width loop in `PrecheckPhase.EnumerateGapTrivia`): **kept, not proven unreachable — disproved.** I ran a scratch Roslyn 5.9.0 program (`scratchpad/zw/Program.cs`) over all 1894 `.cs` files under `src`, in `Parse` and `None`, plus about 11k random mutations: token deleted, `/* c */⏎` inserted, a token replaced by ` // c⏎ , `. Results:
- In trees without syntax errors, no zero-width token other than end-of-file ever carried trivia (86 zero-width tokens in comma gaps, 0 with trivia), so the auditor's premise holds for valid code.
- In trees with syntax errors, missing tokens carry trivia, and 208 of them sat between a comma and its previous token.

The pipeline never sees such a tree: every entry point returns early on `HasSyntaxErrors`. But `ReihitsuFormatter.FindUnsupportedSyntax` is called by about 25 code fixes at registration (SHARED-FIX-010), and they can run on a document with syntax errors in the IDE. Deleting the loop would change whether those fixes are offered on broken code, so the loop stays.

## Policy owners
- Trivia of a removed trailing comma: `rg -n "separator\.LeadingTrivia|comma\w*\.LeadingTrivia" src --glob '!*Test*'` finds only `TrailingCommaRemovalUtilities.PreserveSeparatorTrivia`. Its callers are `EnumTrailingCommaRemovalTransform` and `InitializerTrailingCommaRemovalTransform` (13-auditor policy table).
- "A comment behind the comma on its line" for RH5107: `rg -n "TrailingTrivia.Any\(SyntaxTriviaUtilities.IsCommentTrivia\)" src/Reihitsu.Analyzer.CodeFixes` finds no other site. The formatter's collapse (`LineBreakTriviaUtilities.CollapseSeparatorsToPreviousElementLine`) carries only the comma's trailing trivia. That is now intentional under decision 18: the documentation-comment phase places doc comments. No new shared helper.

## Production diff size
- `git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` (before this commit): 8 files changed, 217 insertions(+), 120 deletions(-).
- Round delta: 2 files, +73 / −9 (`git diff --shortstat -- src`). No new type and no new public member (one private method in the fix provider; one private method kept in the utility).

## Deleted special cases
None deleted in this round. One deletion was reverted: the run's *Deleted* entry for the comma-leading-trivia branch. Decision 14 moved it to *Checked and kept*.

## Tests run
```
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~TrailingCommaDocumentationCommentTests|FullyQualifiedName~LeadingCommaDocumentationCommentTests|FullyQualifiedName~TrailingComma|FullyQualifiedName~UnsupportedListSeparatorCommentTests"
  Passed!  - Failed: 0, Passed: 145, Total: 145
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5107"
  Passed!  - Failed: 0, Passed: 55, Total: 55
scripts/test.sh --configuration Debug --project formatter
  Passed!  - Failed: 0, Passed: 5173, Total: 5173
scripts/test.sh --configuration Debug --project analyzer
  Passed!  - Failed: 0, Passed: 9575, Total: 9575
scripts/test.sh --configuration Debug --project architecture
  Failed!  - Failed: 1, Passed: 117, Total: 118  (SpecStructureTests.UsesListsExactlyTheReferencedRequirements; fails identically on 4e60d8d without my change, see below)
```
All 18 rows that 15-test-implementer listed as failing now pass. No assertion, input, or expected value was changed.

## Disputed tests or scope
- **Spec bookkeeping (spec author, not mine):** `SpecStructureTests.UsesListsExactlyTheReferencedRequirements` fails on HEAD 4e60d8d before any change of mine: `formatter/unsupported-syntax.md: FMT-STRUCT-019 is referenced but missing from Uses`. Repair round 1's new FMT-UNSUP-009 `Why:` sentence introduced the reference. The Uses row needs FMT-STRUCT-019.
- **Change doc wording (spec author):** the *Checked and kept* entry in `sdd/changes/2026-11/comma-behind-comment-unsupported.md` names `TrailingCommaRemovalUtilities.GetTriviaToPreserve`'s branch, and that symbol no longer exists. The guard is now `ContainsNonFormattingTrivia(separator.LeadingTrivia)` in `PreserveSeparatorTrivia`. The trivia now leads the closing brace instead of trailing the last element, because a doc comment cannot trail a token (FMT-PIPE-021). The visible text is unchanged. The entry and FMT-UNSUP-009's "carries it onto the last element" read correctly as text, but the symbol name should be updated.
- **Decision 17:** the auditor's hint is refuted for trees with syntax errors (see Changes). The lead may want the spec author's line-count wording change only (14-spec-author did it); no code change follows from the hint.

## Architecture findings
_None._

## Follow-up candidates
- `main` has the same FMT-PIPE-021 defect: it appends a documentation comment from the comma's leading trivia to the last element's trailing trivia, a tree that does not reparse to itself. This round fixes it as part of decision 14. Nothing remains open.
