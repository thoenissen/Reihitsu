Role: cleaner
Commit: HEAD (see git log)
Verdict: clean

## Formatted
_None._ (formatter left the four files unchanged)

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| S2583 | DocumentOrderFixAllProvider.cs `GetFixedDocumentAsync` | Registered action is stored in a `List<CodeAction>`; the callback checks `Count == 0`, the caller checks `Count == 0` and uses `[0]` |
| S3776 | RewriteBindingUtilities.cs `GetBindingRoot` | Extracted `GetDecidingNode` and `IsBindingRootCandidate` |
| S3776 | RH3004UseLambdaSyntaxCodeFixProvider.cs `MoveStatementsAsync` | Extracted `GetColumnDeltas` (delta selection) and `AppendLaterLines` (line loop) |
| S3776 | EmptyParenthesesCodeFixProviderBase{TNode}.cs `BuildSeparatingTrivia` | Extracted static `AppendSeparatingTrivia` |
| S6605 | RH3004 `GetFirstStatementLinePosition` | `elements.Any(...)` became `elements.Exists(...)` |
| IDE0028 | RewriteBindingUtilities.cs `BindsAsBefore` | `new HashSet<SyntaxNode>(x)` became `[.. x]` |

## Unresolved
_None._

## Checks
scripts/build.sh: 0 Warning(s), 0 Error(s). Analyzer tests: Passed 7005, Failed 0. Core tests: Passed 649, Failed 0.
