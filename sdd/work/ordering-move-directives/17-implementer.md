Role: implementer
Commit: 81d6e73
Verdict: tests pass

Repair round 1, finding #1 of `14-auditor.md` (#1668), on the spec of db9eed8 (decision 13: SHARED-ORDER-008's layout clause anchored at the last instance field of the spans) and the tests of `16-test-implementer.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/OrderingDeclarationUtilities.cs` | `ChangesMemberOrder(TypeDeclarationSyntax, bool, Func<int, int>)`: the layout clause's anchor was the first crossed position (`Array.IndexOf(isCrossed, true)`); it is now the start of the last member that counts as an instance field of the layout (`IsLayoutField`) and stands at a crossed position (`-1` when none, so nothing counts). A conditional directive outside the spans counts when it stands in front of that member. Directives inside the spans already refuse before this point. Its summary follows ("in front of the last instance field of the type's layout that stands at such a position; one behind it is not read") | SHARED-ORDER-008 (third place), SHARED-ORDER-007, RH7309-005 |

For a member move (RH7102, RH7103, RH7109) the spans are one stretch, so every position between the first crossed position and that field is crossed and refuses already: same result as before, as the spec author derived. Only RH7309 with several stretches changes.

## Policy owners
- `rg -n "IsLayoutField|GetCrossedPositions" src --glob '*.cs'`: both private to `OrderingDeclarationUtilities`; the layout clause has one owner, `ChangesMemberOrder`, used by the member moves and RH7309 alike (decision 1). No copy elsewhere.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src` (before this commit): 6 files changed, 277 insertions(+), 388 deletions(-). This round: 1 file, 10 insertions(+), 2 deletions(-). No new type or member.

## Deleted special cases
_N/A._ No shape is declared unsupported (decision 2).

## Tests run
- `scripts/format.sh src/Reihitsu.Core/OrderingDeclarationUtilities.cs`: Formatted 0 of 1 file(s).
- `scripts/build.sh --configuration Debug`: 0 Warning(s), 0 Error(s).
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH7309RegionsShouldFollowCategoryOrderTriviaTests"`: Passed 51 of 51 (the two red tests `NoCodeFixWhenAGroupBetweenTwoReorderedRunsPrecedesAReorderOfInstanceFieldsThatOpensAStruct` and `NoCodeFixWhenAGroupOfMembersBetweenTwoReorderedRunsPrecedesAReorderOfInstanceFields` now pass; the two guards stay green).
- `... --filter "FullyQualifiedName~RH7102|...RH7103|...RH7107|...RH7108|...RH7109|...RH7309"`: Passed 621 of 621.
- `scripts/test.sh --configuration Debug --project analyzer`: Passed 11885 of 11885.
- `scripts/test.sh --configuration Debug --project core`: Passed 887 of 887.
- `scripts/test.sh --configuration Debug --project architecture`: Passed 198 of 198.
- `scripts/lane-check.sh behavior-change`: confirmed.

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None._
