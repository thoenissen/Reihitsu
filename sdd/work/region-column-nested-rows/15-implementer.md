Role: implementer
Commit: b8f1801
Verdict: tests pass

I worked from these sources:
- `00-lead.md`: the *State* worklist A1–A3, and decisions 8 and 9;
- `13-auditor.md`: findings 1 and 2, and hint 1;
- `14-test-implementer.md`: the six failing tests;
- my handover `08`;
- FMT-BREAK-046.

My prompt carried no intended fix, suspected cause, or hoped-for verdict. I changed no test assertion, input, or expected output. Before the change the six tests 14 names failed as described: 6 failed and 89 passed in the two classes.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/LineBreakPhase.cs` | A1: `ArrayInitializerBraceLineBreakRewriter` now runs **last** in `CreateRewriters`, after `SwitchCaseWhenLineBreakRewriter`. Before, it ran in front of `LineBreakListRewriter`. Each initializer is now judged on the lines that every other rewriter of the phase leaves. Nothing predicts a later join (FC-044). | FMT-BREAK-046 *On one line* |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/ArrayInitializerBraceLineBreakRewriter.cs` | A2: `VisitInitializerExpression` calls `MoveTokenBelowItsLine` on `}` after `NormalizeGapBeforeOwnedToken`. This is the same cheap move the opening side uses. A `}` behind a comment that starts its line, or behind the tail of a multi-line comment, now starts the line below it. The class `<remarks>` now describe the new position and the closing-brace move (FC-031). | FMT-BREAK-046 *Spanning several lines*, comment paragraph |
| `src/Reihitsu.Formatter/Pipeline/Precheck/PrecheckPhase.RegionInsideMember.cs` | Hint 1: `FindRegionDirectiveInsideMember` returns at once when `node.ContainsDirectives` is false. `StandsBetweenMembers` (the ancestor walk) now runs only for a token whose leading trivia holds an active region directive. Behavior is unchanged: an excepted token is still skipped as a whole. The summary says so. | FMT-UNSUP-023 (unchanged) |

### Why the new position holds (A1)
08 put the rewriter before `LineBreakListRewriter`. The reason was an argument whose array spans several lines only through a string literal: the list has to become one argument per line. I checked that case after the move:
- `N(1, new[] { @"x⏎y" });` and `N(1, new[] { 1,⏎2 });` still get one argument per line. `LineBreakListRewriter` already counts the line break inside the literal, so it does not depend on the brace rewriter's line breaks. The output is stable on the second pass.

The rewriter only adds line breaks inside an initializer that already spans several lines. So no construct around it goes from one line to several, and no rewriter that runs before it would decide differently.

I ran the CLI twice over a scratch fixture with every member auditor 13 lists for findings 1 and 2. Results:
- every member is laid out as the spec says;
- the output is stable;
- the members that hold still hold. For example, a chain with two wrapped links still gets its braces on lines of their own.

I found no conflicting requirement or test.

## Policy owners
- `rg -n "RemoveTrailingWhitespace\(.*LeadingTrivia\)" src` finds `ChainLineBreakRewriter.EnsureLineBreak` and `ArrayInitializerBraceLineBreakRewriter.MoveTokenBelowItsLine`, unchanged from 08. The closing brace reuses the existing `MoveTokenBelowItsLine` and adds no copy. Hint 3 (shared helper) was not taken (decision 9).
- `rg -n "ArrayInitializerExpression" src/Reihitsu.Formatter/Pipeline/LineBreaks`: the only owner of array-brace placement is still `ArrayInitializerBraceLineBreakRewriter`.
- The `AGENTS.md` `LineBreakPhase` row already says "judged … on the lines the other line-break rules leave". The new order makes that statement true, so the row needs no change.

## Production diff size
- `git diff --shortstat origin/main...HEAD -- src`: 6 files changed, 354 insertions(+), 7 deletions(-).
- This round: 3 files changed, 24 insertions(+), 10 deletions(-).
- No new type.

## Deleted special cases
_N/A._ This round declares no shape.

## Tests run
- Before the change, `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.LineBreaks.ArrayInitializerBraceLineTests|FullyQualifiedName~Reihitsu.Formatter.Test.Regression.LineBreaks.ListElementInteriorJoinTests"`: Failed 6, Passed 89. These are exactly the six tests from 14.
- `scripts/test.sh --configuration Debug --project formatter`: Passed 7669/7669.
- `scripts/test.sh --configuration Debug --project analyzer`: Passed 12503/12503.
- `scripts/test.sh --configuration Debug --project cli`: Passed 473, Skipped 1 (474).
- `scripts/test.sh --configuration Debug --project architecture`: Passed 247/247.
- `scripts/build.sh --configuration Debug`: 0 warnings, 0 errors.
- `scripts/format.sh` over the three changed files: 0 of 3 reformatted.
- `scripts/format.sh src tests --check`: 0 of 2098 need formatting. The new order reformats no repository file.

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None._
