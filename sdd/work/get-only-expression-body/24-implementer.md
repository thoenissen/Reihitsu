Role: implementer
Commit: the commit that adds this handover
Verdict: tests pass

Decision 18 (`00-lead.md`), audit 21 findings 4 and 5, SHARED-GETONLY-008 paragraph 2 as restated in 22. My prompt restated decision 18 as the worklist and carried no other intended fix or hoped-for verdict.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Core/BlankLineCodeFixUtilities.cs` | `IsCommentGapToSiblingMember` now decides by cause: the gap to a sibling member holds any trivia other than whitespace and `SyntaxTriviaUtilities.AreSeparatedByEndOfLine(left, right)` is false. The enumerated comment kinds (`SingleLineCommentTrivia`, `MultiLineCommentTrivia`) and the private `IndexOf('\n')` line-break test are gone, so `/** c */` (documentation comment in Parse/Diagnose mode) and a multi-line comment whose line break lies inside its text both withhold the fix. XML summaries of it and of `SharesLineWithSiblingMemberAcrossComment` updated | SHARED-GETONLY-008 |

The call site (`GetOnlyMemberExpressionBodyCodeFixProviderBase.CanConvert`) is unchanged; its summary ("where a comment separates the member from a sibling member on one line") still holds, since only a delimited comment can stand in a gap no line break separates.

## Policy owners
- Line break between two tokens: `rg -n "AreSeparatedByEndOfLine|EndsLine" src --glob '!*Test*'` — owner `SyntaxTriviaUtilities` in `Reihitsu.Core`; the guard now calls it. No private line-break test is left in the get-only path (`rg -n "IndexOf\('\\\\n'\)" src/Reihitsu.Analyzer.CodeFixes` finds only RH5401's check of formatted text, which is a different decision: whether a formatted member fits on one line).
- Sibling gap: `IsGapToSiblingMember`, shared with `SharesLineWithSiblingMember`, unchanged.

## Production diff size
- `git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` after this round: 16 files changed, 1188 insertions(+), 521 deletions(-).
- This round: 1 file changed, 9 insertions(+), 7 deletions(-). No new type or member.

## Deleted special cases
_N/A._ No shape is declared unsupported.

## Tests run
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH3209GetOnlyPropertiesShouldUseAnExpressionBodyAnalyzerTests|FullyQualifiedName~RH3210GetOnlyIndexersShouldUseAnExpressionBodyAnalyzerTests"`: Passed 148 of 148 (23 reported 24 failing before the change).
- `scripts/test.sh --configuration Debug --project analyzer`: Passed 11107 of 11107.
- `scripts/test.sh --configuration Debug --project architecture`: Passed 198 of 198.
- `scripts/build.sh --configuration Debug`: 0 warnings, 0 errors. `scripts/format.sh` on the changed file: formatted 0 of 1.
- Formatter, Core, CLI: not run; no file in those projects changed.

## Disputed tests or scope
_None._ The 16 handover's open point (a multi-line comment gap was treated as a line break, fix offered) is resolved by decision 18: that gap now withholds the fix, and 23's tests cover both directions.

## Architecture findings
_None._ Finding 5's private line-break test is replaced by the Core owner; finding 4's missed kind cannot recur because no kind is enumerated.

## Follow-up candidates
_None new._ The RH3202 family's comment-gap behavior stays in `00-lead.md`.
