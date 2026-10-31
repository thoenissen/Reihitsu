Role: implementer
Commit: 449e70c
Verdict: tests pass

This is repair round 1. It works from `12-auditor.md`, `13-test-implementer.md`, `00-lead.md`, and `squad/decisions/2026-10-30-line-start-fact-audit-1-copies.md`. The auditor's suggested changes were treated as suggestions; each repair below is re-derived from the code and the requirement. The prompt carried no other intended fix.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Utilities/LineBreakTriviaUtilities.cs` | `CollapseTokenToSameLine` now redirects whenever the directly preceding token has no text, no longer only when the gap to that token holds no line break. Tokens without text carry no trivia, so the gap reaches back to the token with text: that token's trailing trivia and the joined token's leading trivia. The join removes the line breaks in both, wherever they stand. Under the old condition, a line break in the joined token's own leading trivia skipped the redirect. That covered a blank line, a whitespace-only line, or a plain line break followed by a blank line, in front of `>` or of any `,`. The trailing line break of the token with text then survived pass 1. Every member `13-test-implementer.md` enumerates goes through this one decision. Summary updated | FMT-BREAK-014 (finding 1) |
| `src/Reihitsu.Core/AdjacentTokenSpacingUtilities.cs` | `DetermineLineBreakTolerantSpacing`, shared by the RH6021 analyzer and its fix, decides both sides with `SyntaxTriviaUtilities.AreSeparatedByEndOfLine`. The private `EndOfLineTrivia` checks are gone, so the line break of a `///` comment counts. Without a previous or next token, the fact reads the token's own trivia, as the old code did. Summary updated | RH6021-001, RH6021-002, RH6021-004 (finding 2) |
| `src/Reihitsu.Analyzer/Rules/Spacing/RH6005OperatorKeywordMustBeFollowedBySpaceAnalyzer.cs` | The keyword is not reported when its trailing trivia holds whitespace, or when `AreSeparatedByEndOfLine(keyword, nextToken)` is true. That covers the line break that ends a `///` comment behind it. The `EndOfLineTrivia`-only check is replaced | RH6005-001 (finding 2) |
| `src/Reihitsu.Formatter/Utilities/ReihitsuFormatterHelpers.cs` | `ComputeTokenColumn` deleted. The class summary already names only entry-point guards and computations | issue *Done when* (finding 3) |
| `src/Reihitsu.Analyzer.CodeFixes/Base/TargetAttributePlacementCodeFixProviderBase.cs` | Its one caller now uses `SyntaxTokenPositionUtilities.GetColumn` from `Reihitsu.Core`. That method reads the same value, the start column of the token's line span in its tree | finding 3 |

## Policy owners

- **A line break separates two tokens.** I searched for the copies with `rg -n "IsKind\(SyntaxKind.EndOfLineTrivia\)\)|Any\(SyntaxKind.EndOfLineTrivia\)" src --glob '*.cs' --glob '!**/obj/**' --glob '!*.Test/**'`.
  - The owner is `SyntaxTriviaUtilities.AreSeparatedByEndOfLine`.
  - The RH6021 helper and RH6005 now call it.
  - Remaining hits are outside decision `…-audit-1-copies`:
    - `AttributeTargetUtilities.StartsLine`, deferred by decision 3A;
    - the trailing-trivia check of `RH6001…Analyzer.cs:68`. The auditor found it unreachable on formatter output, and the decision does not name it;
    - trailing-trivia "ends its line" checks in `AccessorExpressionBodyTransform` and `EndOfLineWhitespace`. They read only a token's trailing trivia, which never holds a `///` or a directive.
- **Joining across tokens without text.** The only place is `CollapseTokenToSameLine`.

## Tests run

All on 449e70c, Debug (`scripts/build.sh --configuration Debug`: 0 warnings, 0 errors).

| Project | Passed | Failed |
|---|---|---|
| `scripts/test.sh --configuration Debug --project formatter` | 3260 | 0 |
| `… --project analyzer` | 6279 | 0 |
| `… --project core` | 535 | 0 |
| `… --project cli` | 251 | 0 |
| `… --project architecture` | 89 | 0 |
| `… --project tooling` | 125 | 0 |

The formatter run includes the seven blank-line join rows. The analyzer run includes the nine RH6021/RH6005 rows; the RH6021 fix row turned green through the shared helper, with no change to the fix provider.

`scripts/format.sh --check src`: `0 of 1753 file(s) need formatting`.

## Disputed tests or scope

_None._

## Architecture findings

_None._ Finding 2 was a member of the class the run already fixes structurally (decision `…-audit-1-copies`, option B). The repair routes both copies through the shared fact and adds no new structure.

## Follow-up candidates

- `RH6001…Analyzer.cs:68` still checks the line break behind `nameof` by `EndOfLineTrivia` kind. It cannot be reached on formatter output, because the documentation-comment phase relocates the comment. Its own analyzer report on hand-written code still disagrees with `///`.
- The earlier candidates stand: `AttributeTargetUtilities.StartsLine` and the analyzer-side terminator sets (decision 3A), and `Data/RootListPosition` → `TokenGapNormalizer.HasOwnLinePlacementOwner`.
