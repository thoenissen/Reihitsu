Role: implementer
Commit: 37c9930
Verdict: tests pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing to ignore.

Before the change, the handed-over tests failed as `03-test-implementer.md` says: RH5102 was reported where RH5102-001 now says MUST NOT.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Layout/RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer.cs` | `OnArgumentList` no longer compares the lines of `(` and `)`. The arguments alone decide whether the list is multi-line: two arguments start on different lines, or one argument spans lines. The list is reported only when it is multi-line and two arguments start on the same line. The conditional-directive guard (RH5102-004) and the diagnostic location (RH5102-002) are unchanged. The inline comment states the new reading. The method's XML summary still holds. | RH5102-001 |

The code fix is unchanged (RH5102-003).

## Policy owners
`rg -n "isMultiLine \|=|sharesLine|startLines" src --glob '!*Test*'`:
- `RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer.OnArgumentList`: this change.
- `RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer.OnParameterList`: the same reading, element by element, for parameter lists (RH5109-001, the earlier decision in #1736).

`Reihitsu.Core` has no shared helper for this reading. The formatter's `LineBreakDetection.IsMultiLine(SyntaxNode)` judges a single node, not a separated list, so it is a different policy. The two analyzers now hold the same 12-line loop over different list types. See *Disputed tests or scope*.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 1 file changed, 12 insertions(+), 14 deletions(-). No new type and no new member.

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5102ArgumentsShouldBeOnSingleOrSeparateLines"`: Passed: 76, Failed: 0.
- `scripts/test.sh --configuration Debug --project analyzer`: Passed: 10136, Failed: 0.

## Disputed tests or scope
- **Option, not taken: share the element-line reading.** RH5102 and RH5109 now hold the same loop: start lines, one element spanning lines, and a shared start line. It could move to a `Reihitsu.Core` helper over `SeparatedSyntaxList<TNode>`. That would add a new public member and touch RH5109, so I kept the narrow change. The lead decides.

## Architecture findings
_None._

## Follow-up candidates
_None._
