Role: implementer
Commit: (this commit; base d85b4d6)
Verdict: test disputed

Repair round 2, decisions 27 and 28 (auditor 13 findings 1, 2), against spec author 14 and test implementer 15. The prompt carried no intended fix, suspected cause, or hoped-for verdict. Debug build, 0 errors.

## thoenissen/Reihitsu#1140

Covered through #1144 (the shared-line helper, decision 28).

## thoenissen/Reihitsu#1141

Nothing to do in this round.

## thoenissen/Reihitsu#1144

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5203MultiLineArgumentsShouldBeAlignedCodeFixProvider.cs` | `RegisterCodeFixesAsync` registers the fix only when `SyntaxTokenPositionUtilities.StartsLine(argument.GetFirstToken())`: nothing but whitespace in front of the argument on its line (a comment in front of it, or the closing line of a multi-line comment, withholds it) | RH5203-003 (decision 27) |
| `src/Reihitsu.Analyzer.CodeFixes/Core/BlankLineCodeFixUtilities.cs` | `FormatMemberOnLinesOfItsOwnAsync` breaks a shared-line gap only when the first token on the member's first line belongs to a member whose parent is the member's parent (the member itself or a sibling in front of it); otherwise it breaks nothing and only formats the member, as `main` did. XML summary updated | RH5417-003, RH3202-002 (decision 28) |

### Policy owners

- Token starts its line (SHARED-BLANK-008): `rg -n "static bool StartsLine|static bool IsFirstOnLine" src` — `SyntaxTokenPositionUtilities.StartsLine` (reused, no copy) and `IsFirstOnLine` (the RH5203 analyzer's report condition, which ignores comments; `StartsLine` is the stricter one RH5203-003 now states for the fix).
- First token on a line: `rg -n "lineStartToken = root.FindToken" src --glob '!*Test*'` — the new use in `FormatMemberOnLinesOfItsOwnAsync` and the existing one in `TryGetIndentation` of the same file (two lines each, same file; not extracted).
- Shared-line column: still `FormatMemberOnLinesOfItsOwnAsync` and `TargetAttributePlacementCodeFixProviderBase`; the class where they disagreed (line begins with a type header) is now refused by the helper (decision 28).

### Tests run

- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5203MultiLineArgumentsShouldBeAlignedAnalyzerTests|FullyQualifiedName~RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzerTests|FullyQualifiedName~RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests|FullyQualifiedName~RH5513MethodAttributesMustFollowPlacementRulesAnalyzerTests"`: Failed 1, Passed 145, Total 146.
- `scripts/test.sh --configuration Debug --project analyzer`: Failed 1, Passed 8322, Total 8323 (the disputed test below).
- `scripts/test.sh --configuration Debug --project architecture`: Passed 117/117.
- `scripts/test.sh --configuration Debug --project core`: Passed 655/655.

Of the 8 red tests, 7 pass: the five RH5203 `NoFixIsOffered…` tests, `RH5417…FixLeavesTheMembersOnTheLinesOfAMemberOfATypeWrittenOnOneLine`, `RH3202…FixLeavesTheMembersOnTheLinesOfAMethodOfATypeWrittenOnOneLine`.

### Disputed tests or scope

- `RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzerTests.FixLeavesTheMemberInFrontOnTheLineOfAMemberOfATypeWrittenOnOneLine` — the LF half passes; the CRLF half fails. Its input `class C { int _x; void M() {|RH5417:{|} } }` holds no line break, so `NormalizeToCarriageReturnLineFeed` leaves it identical to the LF input, while the expected output carries CRLF. The line ending the fix writes comes from the formatter's `LineEndingUtilities.DetectEndOfLine`, which falls back to `Environment.NewLine` (LF on Linux) when the input has none. For this input the helper now does exactly what `main`'s RH5417 fix did (`FormatNodeInDocumentAsync(document, member)` on the unchanged document), so `main` produces the same LF output: the test's claim that `main` matches under CRLF cannot hold for a one-line input (an `apply-fix.sh` run over a CRLF copy of a one-line file has no CRLF to show). Observed: `class C { int _x; void M()<LF>                  {<LF>                  } }`. Suggested route: the test implementer drops the CRLF half (or gives the input a line break); no spec states the line ending of a document without one, and changing the formatter's fallback is outside this lane.

### Architecture findings

_None_ beyond 13's two-owner finding, whose disagreeing class decision 28 now refuses.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1145

Nothing to do in this round.

## thoenissen/Reihitsu#1094

Nothing to do in this round (the new comment-gap test passes unchanged).

## thoenissen/Reihitsu#1393

Nothing to do in this round.

## thoenissen/Reihitsu#1511

Nothing to do in this round.

## thoenissen/Reihitsu#1561

Nothing to do in this round.

## Whole run

`git diff --shortstat origin/main -- src ':(exclude)*Test*'` (working tree before commit): `14 files changed, 347 insertions(+), 244 deletions(-)`. Delta of this round: `2 files changed, 21 insertions(+), 5 deletions(-)`. No new type, no new public member.

Deleted special cases: _N/A._
