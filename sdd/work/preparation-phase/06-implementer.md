Role: implementer
Commit: 056c1fc
Verdict: tests pass

This is the repair round after `05-test-implementer.md`, under the 2026-10-30 disputed-tests decision (1A, 2A). The message that started the round carried no intended fix or suspected cause. It stated the required signature, which the test project fixes. No test file was changed.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/DocumentationComments/Utilities/DocCommentElementNormalizer.cs` | Signature is now `BuildReplacement(XmlElementSyntax element, SourceText sourceText, string endOfLine)`. `BuildExpandedElement` writes the given line ending between the lines of an expanded element. `GetLineBreak` and its `Environment.NewLine` fallback are removed. It used to copy the start tag's line break, and after the preparation phase that copy already was the detected ending; so output changes only where the fallback was reached | FMT-PIPE-019, FMT-PIPE-004 |
| `src/Reihitsu.Formatter/Pipeline/DocumentationComments/DocumentationCommentFormattingPhase.cs` | `NormalizeDocumentationComment`, `NormalizeElementsUntilStable`, and `NormalizeOutermostElements` take `endOfLine`; `Execute` passes `context.EndOfLine`. XML docs updated (FC-031) | FMT-PIPE-019 |

Dispute 1 needed no production change. `PhaseStabilityCheck.GetReportedName` runs a phase through `FormattingPipeline.ExecutePhases` and gets its name from `PreparationPhase.GetSubPhaseName`, both from `a61f166`.

## Policy owners

`rg -n "Environment\.NewLine|CarriageReturnLineFeed" src/Reihitsu.Formatter --type cs` returns only the XML comment of `ReihitsuFormatterHelpers.DetectEndOfLine` (`:65`). That is the detection fallback of FMT-PIPE-007, whose owner is `LineEndingUtilities`. No formatter phase writes a fixed or environment line ending any more.

## Tests run

```shell
scripts/build.sh --configuration Debug                          # Build succeeded.
scripts/test.sh --configuration Debug --project formatter       # Failed: 0, Passed: 3148, Total: 3148
scripts/test.sh --configuration Debug --project cli             # Failed: 0, Passed: 251, Total: 251
scripts/test.sh --configuration Debug --project architecture    # Failed: 0, Passed: 89, Total: 89
scripts/test.sh --configuration Debug --project analyzer        # Failed: 0, Passed: 6242, Total: 6242
```

The probe from dispute 2 was a CRLF file `class C\r\n{\r\n}\r\n/// <summary>a</summary>` with no final line break. Run through `bash scripts/trace.sh <file> --no-install`, it now reports `Stable after pass 2 (0 phase changes observed)` and no `PreparationPhase/LineEndings` step in pass 2.

## Disputed tests or scope

_None._

## Architecture findings

_None._

## Follow-up candidates

_None._
