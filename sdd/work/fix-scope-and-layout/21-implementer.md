Role: implementer
Commit: (this commit; base 1144034)
Verdict: tests pass

Repair round 3 (last), lead decision 33 (auditor 19 finding 8). The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the decision itself.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Core/BlankLineCodeFixUtilities.cs` | `FormatMemberOnLinesOfItsOwnAsync`: the line begins with a member only when its first token starts the line behind whitespace and comments whose text starts on that line (the existing `StartsLineBehindComments`, the statement counterpart's check) and that token is the first token of a member of the same list (`AncestorsAndSelf().OfType<MemberDeclarationSyntax>().TakeWhile(first token equal).Any(same parent)`, the shape of `TryGetListIndentation`'s `startsListStatement`). Otherwise no gap is broken and the member is formatted as on `main`. Summary rewritten (FC-031) | RH5417-003, RH3202-002, SHARED-FIX-007 |
| same file | The copied line-start lookup (`FindToken(line.Start)` + `GetNextToken`) is now one private helper `GetLineStartToken`, used by the member path and `TryGetIndentation` (auditor 19 *Policy ownership*: the copy). No behavior change on the statement path | SHARED-STMT-004 (unchanged) |

Refused shapes now: (a) continuation of a sibling's initializer (first token `2` is not a member's first token), (b) a statement of a sibling's body, (c) the closing line of a sibling's verbatim string (the literal reaches into the line, so `StartsLineBehindComments` fails), (d) the tail of a comment that starts on an earlier line (the comment does not start on the line). The separated side (a comment that starts on the member's line in front of it) still separates.

### RH5203 `startsLine` — kept, not dead

`ApplyCodeFixAsync` initializes `startsLine` from `firstToken.GetPreviousToken().TrailingTrivia.Any(EndOfLineTrivia)`. Behind the registration guard (`SyntaxTokenPositionUtilities.StartsLine`) it is `false` exactly when the previous token is zero-width: `StartsLine` judges against the token with text in front (SHARED-BLANK-008), while `GetPreviousToken()` returns the missing token, whose trailing trivia is empty. Example: a missing comma, `M(a⏎    b)` — the parser inserts a missing `,` in front of `b`, the analyzer (which runs on erroneous code; `IsFirstOnLine` holds) reports `b`, and the fix registers. So the value is reachable and the branch is not provably unreachable; per the lead's condition I left it unchanged. On that input the current code keeps `b`'s whitespace and appends the alignment, so `b` lands too far right; initializing `startsLine = true` would be correct for every input the guard admits. Not built: syntax-error input only (see *Follow-up candidates*).

## Policy owners
- `rg -n "StartsLineBehindComments|GetLineStartToken" src --glob '!*Test*'`: one owner each, `BlankLineCodeFixUtilities` (statement path lines 268/303/312, member path 146/147). The member predicate now uses the same start-of-line check and the same first-token-equality shape as the statement counterpart.
- `rg -n "FindToken\((memberLine|anchorLine|line)\.Start\)" src --glob '!*Test*'`: `BlankLineCodeFixUtilities.GetLineStartToken` and `RawStringLineShifts.cs:214` (formatter, a different decision: raw-string content lines). The in-file copy is gone.
- Column of code separated from a sibling: still `FormatMemberOnLinesOfItsOwnAsync` (line indentation) and `TargetAttributePlacementCodeFixProviderBase` (depth). On the lines of finding 8 the helper now refuses, so the two no longer produce different columns there; RH5513 still separates shape (a) at the depth column (see *Architecture findings*).

## Production diff size
- Round 3 delta: `git diff --shortstat 1144034 -- src`: 1 file changed, 29 insertions(+), 17 deletions(-) (12 of the insertions are the new private helper with its summary; the copy it replaces was 12 lines). No new type, no new public member.
- `git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` before this commit: 14 files changed, 348 insertions(+), 246 deletions(-).

## Deleted special cases
_N/A._

## Tests run
- `scripts/build.sh --configuration Debug`: 0 Warning(s), 0 Error(s).
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzerTests|FullyQualifiedName~RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests"`: Passed! Failed 0, Passed 101 (the 8 red tests of handover 20 are green).
- `scripts/test.sh --configuration Debug --project analyzer`: Passed! Failed 0, Passed 8333.
- `scripts/test.sh --configuration Debug --project architecture`: Passed! Failed 0, Passed 117.
- `scripts/test.sh --configuration Debug --project core`: Passed! Failed 0, Passed 655.
- `scripts/format.sh` on the changed file: formatted 0 of 1.

## Disputed tests or scope
_None._ No test assertion changed. The spec files `sdd/specs/analyzer/RH5417.md` and `RH3202.md` (spec author, in parallel) are not staged in this commit.

## Architecture findings
The two-owner column (helper vs. `TargetAttributePlacementCodeFixProviderBase`, audit 13 / audit 19) is unchanged in structure; this round only narrows the helper to the lines where it reads a real member column, as decision 33 prescribes. RH5513 still separates shape (a) at the depth column; decision 33 leaves it out of scope.

## Follow-up candidates
- RH5203 fix on a zero-width previous token (missing comma, syntax-error input): `startsLine` starts `false`, so the argument's whitespace is kept and the alignment whitespace appended. Small (one initializer), syntax-error input only; whether any caller counts that as reached is the lead's call.

## Whole run
`git diff --shortstat origin/main -- src ':(exclude)*Test*'` (after this commit): 14 files changed, 366 insertions(+), 252 deletions(-)
