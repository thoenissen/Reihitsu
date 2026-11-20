Role: implementer
Commit: (this commit; see `git log` for the subject "Share the line-ending rewrite of RH5604 and the formatter for #1874 [skip ci]")
Verdict: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the location fixed in `00-lead.md`, decision 1, which I followed.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/LineEndingUtilities.cs` | New `public static SyntaxNode NormalizeLineEndings(SyntaxNode node, string endOfLine)`: the two passes both copies ran (XML newline tokens, then `EndOfLineTrivia` with `descendIntoTrivia: true`), with no formatter guard; returns `node` itself when nothing differs. Class summary names the new member | FMT-EOL-001..003, RH5604-005, FMT-PIPE-009 (Core is callable from fixes) |
| `src/Reihitsu.Formatter/Pipeline/LineEndings/LineEndingNormalizationPhase.cs` | `Execute` delegates to `LineEndingUtilities.NormalizeLineEndings(root, context.EndOfLine)`; summary names the shared owner (identity promise unchanged and still true) | FMT-PIPE-019, FC-031 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5604CodeMustNotContainMixedLineEndingsCodeFixProvider.cs` | `ApplyCodeFixAsync` calls `LineEndingUtilities.DetectEndOfLine` and `NormalizeLineEndings`; returns the original `Document` when the reference is unchanged, otherwise `WithSyntaxRoot`. The fix now calls Core's `DetectEndOfLine` directly instead of the formatter's `ReihitsuFormatterHelpers.DetectEndOfLine` wrapper (which only forwards to it), so the `Reihitsu.Formatter.Utilities` and `Microsoft.CodeAnalysis.CSharp` usings are gone. Summary describes the new body | RH5604-005, FC-031 |

The fix's former extra early return ("tokens replaced, no trivia") is gone; it returned the same text as the general path.

## Policy owners
- `rg -n "XmlTextLiteralNewLineToken" src -g '!*Test*'`: `LineEndingUtilities.DetectEndOfLine` (count), `LineEndingUtilities.NormalizeLineEndings` (rewrite, the one owner now), `RH5604CodeMustNotContainMixedLineEndingsAnalyzer.OnSyntaxTree` (count/report), `RH8308...Analyzer` (unrelated). The rewrite has one owner; the two counting statements remain, as decision 2 keeps them for the follow-up.

## Production diff size
Working tree before commit: `3 files changed, 48 insertions(+), 52 deletions(-)` under `src` (no test files changed). No new type; one new public member on an existing public Core class.

## Deleted special cases
_N/A._

## Tests run
- `scripts/build.sh --configuration Debug`: Build succeeded, 0 warnings.
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5604"`: Passed 47/47.
- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~LineEnding"`: Passed 59/59.
- `scripts/test.sh --configuration Debug --project architecture`: Passed 198/198.
- `scripts/test.sh --configuration Debug --project formatter`: Passed 6120/6120.
- `scripts/test.sh --configuration Debug --project core`: Passed 738/738.
- `scripts/test.sh --configuration Debug --project analyzer`: Passed 11298/11298.
- `scripts/format.sh` over the three files: 0 of 3 changed.

## Disputed tests or scope
- Scope: a new public member on `Reihitsu.Core.LineEndingUtilities` (`NormalizeLineEndings`), as decision 1 prescribes. No unit test of it in `Reihitsu.Core.Test`; the test implementer deliberately left that out (both callers are pinned). The lead may want one now that the signature exists.
- Spec `Code owners` lines (`sdd/specs/formatter/line-endings.md`, `sdd/specs/analyzer/RH5604.md`) not touched: routed to the spec author per decision 4.

## Architecture findings
_None_ beyond the skeptic's FC-016 sibling statements (`DetectEndOfLine` and the RH5604 analyzer counting the same carrier set), already recorded as a follow-up candidate in `00-lead.md`.

## Follow-up candidates
_None_ new.
