Role: cleaner
Commit: see the commit that adds this file
Verdict: clean

## Formatted
_None._ (`scripts/format.sh`: Formatted 0 of 2 file(s).)

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| Audit finding #2 (comment text) | `src/Reihitsu.Core/OrderingDeclarationUtilities.cs` public `ChangesMemberOrder(..., Func<int,int>)` summary | Anchor stated as "in front of the last instance field of the type's layout in the code the reorder moves or moves code across" (SHARED-ORDER-008) |
| Audit finding #2 (comment text) | same file, move overload summary and `<returns>` | "in front of the spans" renamed to "in front of the moved fields" |
| Audit finding #2 (comment text) | `tests/Reihitsu.Analyzer.Test/Formatting/RH7309RegionsShouldFollowCategoryOrderTriviaTests.cs` three test summaries (the older one and the two new ones) | "in front of the spans" renamed to "in front of the moved fields" |

## Unresolved
_None._

## Checks
`scripts/verify-text-only.sh --base 0a5c616 --head worktree`: `TEXT-ONLY PROOF: PASS — base=0a5c616 head=worktree — 6 changed file(s) (4 non-compiled, 2 C# comment/layout-only); xml-doc impact: 2 file(s)`
No build or tests run: the proof shows only documentation trivia changed.

## Follow-up candidates
_None._ (The move overload summary and its `<returns>` also said "in front of the spans" for the layout anchor and now say "in front of the moved fields".)
