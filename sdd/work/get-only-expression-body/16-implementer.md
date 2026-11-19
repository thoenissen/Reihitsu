Role: implementer
Commit: af77361
Verdict: tests pass

Repair round 1, decision 12 (with 14, 15), audit finding 1 and its architecture finding. My prompt carried no intended fix or hoped-for verdict beyond restating decision 12. The structural option of decision 12 was taken; the narrow fallback was not needed.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Core/ExpressionBodyCodeFixUtilities.cs` | The splice ("keep the member as written, take the formatter's body and the gap in front of it") is now one public method, `ReplaceBodyAsync(document, member, formatterInput, getBodyStartToken, withBodyOf, formatAsync, ct)`, which works in both directions. `ConvertBodyAsync` (RH3202/RH3203/RH3208) is now a call of it; its result is unchanged. Class summary updated | SHARED-BODY-003, SHARED-GETONLY-005 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/GetOnlyMemberExpressionBodyCodeFixProviderBase.cs` | Its private copy of the splice is gone. `ApplyCodeFixAsync` now routes through `BlankLineCodeFixUtilities.FormatMemberOnLinesOfItsOwnAsync` (the RH3202 family's shared-line owner, including its line-start exception), with a callback `ConvertAsync` that hands the pre-converted member to `ReplaceBodyAsync`. `CanConvert` also withholds the fix when `SharesLineWithSiblingMemberAcrossComment` holds. Fix All is now `DocumentOrderFixAllProvider` (as RH3202 and RH5417): with `BatchFixer`, two fixes on one line both broke the same gap and left a whitespace-only line. The constructor takes the analyzer for that | SHARED-FIX-007, SHARED-GETONLY-005, -008, SHARED-FIX-002 |
| `src/Reihitsu.Analyzer.CodeFixes/Core/BlankLineCodeFixUtilities.cs` | `SharesLineWithSiblingMember`'s sibling test is factored into the private `IsGapToSiblingMember`. The new public `SharesLineWithSiblingMemberAcrossComment(member)` uses it: a gap to a sibling, in front of the first token or behind the last, that holds a comment and no line break (no trivia whose text contains `\n`) | SHARED-GETONLY-008 paragraph 2 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3209…CodeFixProvider.cs`, `RH3210…CodeFixProvider.cs` | Pass their analyzer to the base | SHARED-FIX-002 |

## Policy owners
- Splice: `rg -n "GetPreviousToken\(\).*TrailingTrivia|ReplaceToken\(lastSignatureToken" src/Reihitsu.Analyzer.CodeFixes` finds one owner, `ExpressionBodyCodeFixUtilities.ReplaceBodyAsync`.
- Shared line: `rg -n "SharesLineWithSiblingMember|FormatMemberOnLinesOfItsOwnAsync" src/Reihitsu.Analyzer.CodeFixes` finds the owner `BlankLineCodeFixUtilities` and its callers (`ExpressionBodyToBlockCodeFixProviderBase`, RH3202, RH5401, RH5417, the get-only base). No private copy of the line-start test was written; the one-line-type tests pass through the shared helper (decision 15).
- Fix All order: `DocumentOrderFixAllProvider`, reused.

## Production diff size
- `git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` before this round: 14 files changed, 1051 insertions(+), 497 deletions(-).
- This round (af77361): 5 files changed, 177 insertions(+), 64 deletions(-), net +113. Of these, about 75 are XML documentation; the code delta is about 40 lines. Within decision 12's bound of about 100 production lines, so the structural option was carried out. No new type; new members: `ExpressionBodyCodeFixUtilities.ReplaceBodyAsync` (internal class, public method) and `BlankLineCodeFixUtilities.SharesLineWithSiblingMemberAcrossComment`.

## Deleted special cases
_N/A._ No shape is declared unsupported.

## Tests run
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH3209GetOnlyPropertiesShouldUseAnExpressionBodyAnalyzerTests|FullyQualifiedName~RH3210GetOnlyIndexersShouldUseAnExpressionBodyAnalyzerTests"`: Passed 114 of 114. Before this round, 26 failed.
- `scripts/test.sh --configuration Debug --project analyzer`: Passed 11052 of 11052. This includes RH3202/RH3203/RH3208, whose splice now runs through `ReplaceBodyAsync`.
- `scripts/test.sh --configuration Debug --project architecture`: Passed 198 of 198.
- `scripts/build.sh --configuration Debug`: 0 warnings, 0 errors. `scripts/format.sh` on the five files: formatted 0 of 5.
- Formatter, Core and CLI: not run. No file in those projects changed.

## Disputed tests or scope
- Fix All provider changed from `BatchFixer` to `DocumentOrderFixAllProvider`. This was not named in decision 12. The Fix All shared-line tests need it, and it follows the RH3202 and RH5417 precedent. The test classes stay on `BatchCodeFixTestsBase`, because the provider still returns a Fix All provider.
- `SharesLineWithSiblingMemberAcrossComment` treats a multi-line delimited comment in the gap as a line break. Such a gap is not a shared line, so the fix stays offered and nothing is separated. No test covers this case.

## Architecture findings
_None._ The auditor's finding (two copies of the splice) is resolved: one owner serves both directions, and both families go through the same shared-line wrapper.

## Follow-up candidates
_None new._ The RH3202 family still keeps a comment-gap shared line on one line. This candidate is already in `00-lead.md`. The get-only fix withholds itself for that class, as decision 14 states.
