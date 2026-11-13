Role: implementer
Commit: 4b94245a644c1785b3ace786ca59d2530903b3f3 (production changes; this handover line amended in the next commit)
Verdict: test disputed

Repair round 2: decisions 17 to 20 of `00-lead.md` (auditor 14 findings 1, 2, 3, 6, 7), against the 24 red tests of `16-test-implementer.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict. 22 of the 24 red tests pass; the CRLF DataRows of two end-of-file tests are disputed below.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Base/StatementBracesCodeFixProviderBase.cs` | `TryCreateClosingChange` became `CreateClosingChange`: a body that ends the document without a line break behind it gets `⏎<indentation>}` inserted at the end of its last token's full span (whitespace and comments behind the body stay on its line, nothing behind `}`) instead of withholding the fix (finding 1) | SHARED-BRACE-002 |
| same | Private constant `IndentationLevel = "    "` removed; the body moved off the header line is indented by `new string(' ', SyntaxIndentationUtilities.IndentSize)` (finding 6) | FC-015 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/TargetAttributeListShapeCodeFixProviderBase.cs` | `StartsOnTheLineOfTheAccessorListBrace` became `StartsOnTheLineOfTheHoldingBrace`: the `{` of the body directly holding the list's owner — accessor list, `BaseTypeDeclarationSyntax` (class/struct/record/interface/enum), block-bodied namespace, or a block holding a local function — compared by line with the list (finding 2) | SHARED-ATTR-022 |
| same | New `HoldsLineBreakTheSplitCannotWrite`, checked at registration: a target specifier whose first attribute starts on a later line than `[`, or `LineEndingUtilities.HoldsOtherLineBreak` over the list's span against the detected line ending (finding 3) | SHARED-ATTR-023, SHARED-FIX-008 |
| same | Dead `SeparateLine`/`string.Empty` separator branch removed, with the `placementMode` parameter of `ApplySplitCodeFixAsync`, the unused `ResolvePlacementMode` virtual, and the abstract `DefaultPlacementMode` (finding 7) | – |
| 16 `RH55xx…AttributeListsMustFollowShapeRulesCodeFixProvider.cs` under `Rules/Layout` | Their `DefaultPlacementMode` overrides deleted (3 lines each); nothing read them once the branch was gone, and in RH5524/RH5529 (merge rules) nothing ever read them | – |

## Policy owners
- Indentation unit: `rg -n 'IndentSize|"    "' src/Reihitsu.Analyzer.CodeFixes src/Reihitsu.Core` → `SyntaxIndentationUtilities.IndentSize` is the owner; the brace base now uses it. Pre-existing private copies in RH3004 and RH7004 (auditor 14) are untouched.
- Foreign line break in changed text: `LineEndingUtilities.HoldsOtherLineBreak` (Core), reused by the split fix; owners unchanged otherwise (`OrderingMoveSafety`, RH3004, RH7004, brace base, RH7309).
- Detected line ending: `ReihitsuFormatterHelpers.DetectEndOfLine` → `LineEndingUtilities.DetectEndOfLine`, already used in the file.
- "List on the line of the `{` holding its owner": `rg -n "OpenBraceToken" src/Reihitsu.Analyzer.CodeFixes src/Reihitsu.Core | rg -i line` finds only other decisions (`AccessorOrderingUtilities`, RH3004, RH5103, RH5408, region blank lines); one owner.

## Production diff size
- Run (before this round): `git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` → `5 files changed, 329 insertions(+), 131 deletions(-)`.
- This round: `18 files changed, 60 insertions(+), 109 deletions(-)`. No new type, no new public member.

## Deleted special cases
_N/A._ No shape is declared unsupported.

## Tests run
```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5405BracesMustNotBeOmitted|FullyQualifiedName~RH5406Braces|FullyQualifiedName~RH5407UseBraces|FullyQualifiedName~RH7309Regions|FullyQualifiedName~RH5506Class|FullyQualifiedName~RH5514Method|FullyQualifiedName~RH5518Field|FullyQualifiedName~RH5526Delegate"
```
`Failed: 2, Passed: 398, Total: 400` (before: 24 failed).

```shell
scripts/test.sh --configuration Debug --project analyzer
scripts/test.sh --configuration Debug --project architecture
```
Analyzer: `Failed: 2, Passed: 8789, Total: 8791` — the same two DataRows. Architecture: `Passed: 117`. `scripts/format.sh` on the 18 changed files: formatted 0. Debug build: no warning in `Reihitsu.Analyzer.CodeFixes`.

## Disputed tests or scope
1. **`RH5405BracesMustNotBeOmittedAnalyzerTests.FixWrapsABodyOnTheLineOfItsIfThatEndsTheDocument("\r\n")` and `FixKeepsTrailingWhitespaceOfABodyThatEndsTheDocumentOnTheBodyLine("\r\n")`.** Each input is one line with no line break at all (`if (args.Length > 0) System.Console.WriteLine();`, with `···` in the second), so the LF and CRLF DataRows feed the identical document. The tests assert CRLF in the output for the second row. SHARED-FIX-008 makes the fix write the line ending FMT-PIPE-007 detects, and FMT-PIPE-007 (`sdd/specs/formatter/pipeline.md:165`) falls back to `Environment.NewLine` when the input holds no line break; `LineEndingUtilities.DetectEndOfLine` implements exactly that. On Linux the fix writes `\n` (actual: `if (args.Length > 0)<LF>{<LF>    System.Console.WriteLine();<LF>}`), and the CRLF row fails; on Windows the LF row would fail instead. No implementation meets both rows and the spec. The LF row and the two below-the-`if` tests (whose input holds a line break) pass. Suggested repair for the test implementer: drop the CRLF row of these two one-line tests, or give the input a line break above the `if` (e.g. a leading statement) so the document has a detectable ending.
2. **Scope (finding 7).** Removing the dead branch left `ResolvePlacementMode` and the abstract `DefaultPlacementMode` without readers, so I deleted them and the 16 provider overrides (pure deletions, 3 lines each). This touches 16 files beyond the two bases the handovers name. The alternative — keep the branch and state why — leaves a member every split provider must implement and nothing reads. The lead may ask to revert to the two-file change.

## Architecture findings
_None._

## Follow-up candidates
_None._
