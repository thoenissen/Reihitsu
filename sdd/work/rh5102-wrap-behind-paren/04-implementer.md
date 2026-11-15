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

## Amendment

Lead decision 8: one shared helper states the check that a list is multi-line by its elements, and both rules call it.

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/SyntaxNodeUtilities.cs` | New `public static bool IsPartlyWrapped<TNode>(IEnumerable<TNode> elements) where TNode : SyntaxNode`, next to `AreAllSingleLine`. It returns true when the list is multi-line (two elements start on different lines, or one element spans lines) and two elements start on the same line. The private `CoversSingleLine` decides whether an element spans lines. This is the existing class, not a new type. | RH5102-001, RH5109-001 |
| `src/Reihitsu.Analyzer/Rules/Layout/RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer.cs` | The loop is replaced by `SyntaxNodeUtilities.IsPartlyWrapped(argumentList.Arguments)`, and the unused `System.Collections.Generic` using is removed. | RH5102-001 |
| `src/Reihitsu.Analyzer/Rules/Layout/RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer.cs` | The same loop is replaced by `SyntaxNodeUtilities.IsPartlyWrapped(parameterList.Parameters)`, and the unused using is removed. The logic is the same as before; only where it lives changed. | RH5109-001 |

- Commit: 5c8c36c.
- Policy owners after the amendment: `rg -n "isMultiLine \|=|sharesLine|startLines" src --glob '!*Test*'` finds no copy outside `SyntaxNodeUtilities.IsPartlyWrapped`.
- New public member: `SyntaxNodeUtilities.IsPartlyWrapped`. No new type.
- Diff:
  - Round delta (`git diff --shortstat HEAD~1 -- src`): 3 files changed, 37 insertions(+), 38 deletions(-).
  - Total production diff against `origin/main`: 3 files changed, 38 insertions(+), 41 deletions(-).
- Not added: a direct `Reihitsu.Core.Test` test for `IsPartlyWrapped`, because writing tests is not the implementer's job. Both rules' tests cover it. The lead can route a direct test to the test implementer if wanted.

Tests run (Debug):
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5102ArgumentsShouldBeOnSingleOrSeparateLines|FullyQualifiedName~RH5109ParametersMustBeOnSameLineOrSeparateLines"`: Passed: 121, Failed: 0.
- `scripts/test.sh --configuration Debug --project core`: Passed: 682, Failed: 0.
- `scripts/test.sh --configuration Debug --project architecture`: Passed: 117, Failed: 1. The failure is `UsesListsExactlyTheReferencedRequirements`, and it comes from the spec texts, not the code:
  - `analyzer/RH5102.md` references SHARED-DIAG-010 but does not list it in its Uses row.
  - `formatter/line-breaks.md` lists SHARED-DIAG-010 in Uses but no longer references it outside its non-goals.

  The amended RH5102-001 Why and the reworded FMT-BREAK-012 Why cause it. It belongs to the spec author's files, so I left it.
