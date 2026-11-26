Role: implementer
Commit: the commit "Drop dead directive null check and point summaries at the owner [skip ci]" (production changes and this handover)
Verdict: tests pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict. It scoped me to auditor finding #1 and the two summary hints (lead decisions 4 and 5); I re-derived each change from the code.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/OrderingDeclarationUtilities.cs` (`DeclarationHoldsConditionalDirective`) | Auditor finding #1. The trivia are filtered with `SyntaxTriviaUtilities.IsConditionalDirective` first, and a trivia of kind `#if`/`#elif`/`#else`/`#endif` always has a `DirectiveTriviaSyntax` structure, so the `as` cast cannot yield `null`. The `as` became a direct cast and the `entry.Directive != null` arm is gone; the remaining condition (`entry.Token != firstToken \|\| WrapsWholeDeclaration(...) == false`) is unchanged. The summary describes no null case, so it stays | SHARED-ORDER-008 (unchanged); FC-031 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/ListElementInteriorJoinRewriter.cs` (class remarks, comment only) | Hint 1. The sentence now names the owner `SyntaxIndentationUtilities.IsOwningConstruct` instead of `FindOwningConstruct`, and the prose list gains the two arms it omitted (anonymous objects, accessor lists), so it matches the owner's arms | FC-031 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/ListElementAligner.cs` (method summary, comment only, plus `using Reihitsu.Core;` for the cref) | Hint 1. "as `FindOwningConstruct` names them" became "as `SyntaxIndentationUtilities.IsOwningConstruct` decides". The using is needed only to resolve the cref; the build has 0 warnings | FC-031 |
| `src/Reihitsu.Core/SyntaxIndentationUtilities.cs` (class summary, comment only) | Hint 2. "for indentation-sensitive formatting and code fixes" became "for indentation-sensitive formatting, code fixes, and analyzers"; analyzers call `IsIndentingScope` and now `IsOwningConstruct` (e.g. `CommentPositionUtilities`, RH5204, RH5301) | FC-031 |

The auditor's third hint (the `token` parameter documented as "Token" while `LiesInListElement` passes `default`) was not in my worklist; the inline comment at `ListElementInteriorUtilities.cs:57-58` already explains it, so I left it.

## Policy owners
No policy changed owner. `rg -n "IsConditionalDirective\b" src` and `rg -n "FindOwningConstruct|IsOwningConstruct" src` give the same owners and callers as `06-auditor.md` (*Policy ownership*); the summaries now name the owner directly.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src` before this round: 7 files changed, 56 insertions(+), 91 deletions(-). This round: 4 files changed, 10 insertions(+), 10 deletions(-) (one production line in `OrderingDeclarationUtilities`, one `using`, the rest comments). No new type or member.

## Deleted special cases
_N/A._

## Tests run
```shell
scripts/format.sh <the 4 changed files>                       # Formatted 0 of 4 file(s)
scripts/build.sh --configuration Debug                       # Build succeeded, 0 Warning(s), 0 Error(s)
scripts/test.sh --configuration Debug --project core         # Passed! - Failed: 0, Passed: 901
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH70|FullyQualifiedName~RH71|FullyQualifiedName~RH72"
#   Passed! - Failed: 0, Passed: 1315
scripts/test.sh --configuration Debug --project analyzer     # Passed! - Failed: 0, Passed: 12064
scripts/test.sh --configuration Debug --project formatter    # Passed! - Failed: 0, Passed: 7493
scripts/test.sh --configuration Debug --project architecture # Passed! - Failed: 0, Passed: 198
```
The analyzer total is 15 lower than in `03-implementer.md` because `07-test-implementer.md` deleted the duplicate 15-row test.

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None._
