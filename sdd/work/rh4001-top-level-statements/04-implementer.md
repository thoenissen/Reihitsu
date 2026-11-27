Role: implementer
Commit: 9db50e8
Verdict: tests pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing was ignored.

Before the change, the handed-over run failed as the test implementer's handover says: `FileWithTopLevelStatementsIsNotReported` and `FileWithTopLevelLocalFunctionAndNamespacedTypeIsNotReported` each got 1 diagnostic where 0 were expected.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Naming/RH4001TypeNameShouldMatchFileNameAnalyzer.cs` | New private `HasTopLevelStatements(root)`: true when the compilation unit's `Members` hold a `GlobalStatementSyntax`. `OnSyntaxTreeAction` returns early for such a file, before it looks for the first type. The XML summary of `OnSyntaxTreeAction` now states the exemption. | RH4001-007 |

The code fix and the helper are unchanged. The fix is offered only on a reported diagnostic.

## Policy owners
`rg -n "GlobalStatementSyntax" src` found:
- `Formatter/.../MemberLineStartRewriter.cs:56`: an empty global statement, which is a different decision;
- `Analyzer/Core/CommentPositionUtilities.cs:112`: a member-kind list used for comment placement, which is a different decision.

Nothing else decides whether a file holds top-level statements, so the new predicate is the only owner of that decision.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src`: 1 file changed, 19 insertions(+), 1 deletion(-). No new type and no new public member.

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4001TypeNameShouldMatchFileNameAnalyzerTests"` → Passed: 49, Failed: 0
- `scripts/test.sh --configuration Debug --project analyzer` → Passed: 12147, Failed: 0
- `scripts/test.sh --configuration Debug --project architecture` → Passed: 198, Failed: 0

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None._
