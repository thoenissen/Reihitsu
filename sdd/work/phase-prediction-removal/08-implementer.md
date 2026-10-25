Role: implementer
Commit: b275428
Verdict: scope stop

The prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for. This round implements the second amendment: `06-spec-author.md`, approved in `squad/decisions/2026-10-25-phase-prediction-removal-second-amendment.md`.
- Every test in `07-test-implementer.md` passes now, except four. All four are the same failure: a complex element whose brace stands behind the last line of a multi-line comment, formatted at node level on a target that starts in the middle of a line.
- Making them pass needs a change to the node-level column steps, which the change doc lists as "Not changed here". So I stopped and report it below (escalation 5). This defect is also an architecture finding (escalation 12).

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Core/InitializerLayoutAnalysisUtilities.cs` | `FindFirstComplexElementMisalignment` still checks the opening brace's line at the expected column (SHARED-INIT-001). It now measures the closing brace, the expressions, and a nested element from the column the opening brace stands in. If a comment ends on the brace's line in front of it, that column is behind the comment. Otherwise it equals the expected column, because the check above passed. Updated the summary. | SHARED-INIT-002, RH5303-003, RH5304-002 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Contributors/ObjectInitializerContributor.cs` | A nested initializer assigned to a member (`Child =` / `{`) still puts a brace that starts its line in the member's column. Its members and closing brace now follow the brace's own column after that placement (`GetAdjustedColumn(OpenBraceToken)`), not the member's column. Without a leading comment the two columns are the same. With `/* c */ {` the brace is measured behind the comment. | FMT-INDENT-023, FMT-INDENT-030 |

The RH5303 and RH5304 code fixes delegate to `ReihitsuFormatter.FormatNodeInDocumentAsync`, so they are unchanged. The change touches the shared helper `InitializerLayoutAnalysisUtilities` in its complex-element method only. RH5303 and RH5304 are that method's only callers (`rg -n "FindFirstComplexElementMisalignment" src`), and SHARED-INIT-002 names both. `IsAlignedAt`, which RH5301 also uses, is unchanged.

## Policy owners

- Complex-element check: `rg -n "FindFirstComplexElementMisalignment|IsAlignedAt" src --glob '*.cs'`.
  - `InitializerLayoutAnalysisUtilities` is the only owner.
  - Callers: RH5303 (lines 92, 93, 123, 135), RH5304 (127, 146, 155), RH5301 (90, 91, 113; `IsAlignedAt` only).
  - No private copy exists.
- Formatter anchor column: `LayoutComputer.GetAdjustedColumn` / `GetColumnBeforeRawStringShift` (fa7fb0f). `ObjectInitializerContributor` now reads the brace through it, like the complex-element and equals-value branches of the same contributor already do.

## Tests run

Every count below was observed on b275428.

| Command | Result |
|---|---|
| `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~CommentLedFirstTokenAnchorTests\|FullyQualifiedName~CommentLedLineAlignmentTests"` | `Failed: 2, Passed: 27, Total: 29` (failures listed below) |
| `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5301\|FullyQualifiedName~RH5303\|FullyQualifiedName~RH5304\|FullyQualifiedName~RH5305"` | `Failed: 2, Passed: 138, Total: 140`. On e4ab9df the test implementer saw 23 failures. |
| `scripts/test.sh --configuration Debug --project formatter` | `Failed: 2, Passed: 2971, Total: 2973` |
| `scripts/test.sh --configuration Debug --project analyzer` | `Failed: 2, Passed: 6200, Total: 6202` |
| `scripts/test.sh --configuration Debug --project architecture` | `Passed: 84` |
| `scripts/test.sh --configuration Debug --project core` | `Passed: 533` |
| `scripts/test.sh --configuration Debug --project cli` | `Passed: 250` |
| `scripts/test.sh --configuration Debug --project tooling` | `Passed: 120` |
| `scripts/test.sh --configuration Debug --project playground` | `Passed: 17` |

The four failing tests:
- `Reihitsu.Formatter.Test.Regression.Indentation.CommentLedFirstTokenAnchorTests.FormatNodeInDocumentAlignsComplexElementToItsBraceBehindTheLastLineOfAMultiLineComment`
- `…CommentLedFirstTokenAnchorTests.FormatNodeInDocumentAlignsComplexElementToItsBraceBehindTheLastLineOfAMultiLineCommentWhenTheInitializerBraceMoves`
- `Reihitsu.Analyzer.Test.Formatting.RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzerTests.VerifyComplexElementBehindTheLastLineOfAMultiLineCommentMeasuredFromTheCommentIsDetectedAndFixed`
- `Reihitsu.Analyzer.Test.Formatting.RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzerTests.VerifyComplexElementBehindTheLastLineOfAMultiLineCommentMeasuredFromTheCommentIsDetectedAndFixed`

All four are off by the same 6 columns. The two formatter tests and the RH5303 test put the expressions in 38 and `}` in 34, where 32 and 28 are expected. The RH5304 test puts them in 46 and 42, where 40 and 36 are expected. Document-level formatting of the same inputs gives the expected output; the `AssertRuleResult` half of the second formatter test passes. `scripts/format.sh --force` reports no change on either changed file, and the Debug build shows no warning.

## Disputed tests or scope

**Scope stop (escalation 5): one remaining failure needs a change to the node-level column steps.**

Input: the fixture of `FormatNodeInDocumentAlignsComplexElementToItsBraceBehindTheLastLineOfAMultiLineComment`.

```
        var d = new Dictionary<int, int>
                {
                    /* a
                       b */ {
                    1,
                    2
                }
                };
```

The four tests fail only when the target is the `EqualsValueClauseSyntax`. That is the node the RH5303 and RH5304 fixes format for a declaration (`TryGetFormatTarget`). The statement target passes.

The steps that produce 38/34, read from `ReihitsuFormatter.FormatNodeInDocumentAsync` and `ReihitsuFormatterHelpers`. The numbers match the observed output; I did not trace each phase.
1. The target `= new Dictionary…` starts in the middle of its line, at document column 14. The pipeline formats it as a tree of its own, in which `=` starts line 0. Block indentation puts line 0 at the base column 8 (`ComputeBaseIndentLevel` gives 2).
2. After the pipeline, `AdjustNodeIndentation` moves the start of every line by `originalColumn - formattedColumn` = 14 − 8 = **6**. That is a column move after the phases, from `ComputeTokenColumn` and `AdjustNodeIndentation`.
3. `AdjustNodeIndentation` rewrites the leading whitespace of a token that starts a line. The line `   b */ {` starts inside the comment, so it has no such whitespace and does not move. Its brace stays in document column 28, and the formatted tree shows that column too.
4. Inside the indentation phase, every other line is laid out 6 columns left of where it will end up, while the brace's line is already at its document column. The layout now measures the brace where it stands, 28, as FMT-INDENT-030 requires. It puts the expressions at 32 and `}` at 28 in the detached tree. Step 2 then moves those lines to 38 and 34.

With a one-line comment (`/* e */ {`), the brace's line starts with whitespace and moves by 6 like the others. That variant passes.

Before this run, the layout measured a token on a comment-led line at the line's layout column. That column is shifted by the same offset as the other lines, so the mismatch did not show.

I see no fix inside the scope the change doc names:
- The indentation phase cannot know the offset that `AdjustNodeIndentation` applies after the pipeline. Using that offset would be a prediction of a later step, which FMT-PIPE-001 rules out.
- `AdjustNodeIndentation` cannot move a line that starts inside a comment without changing the comment's text.
- What remains is to change the node-level column steps, for example by formatting the target at its document column so that no move follows. The change doc's inventory lists exactly those steps (`ReihitsuFormatterHelpers.ComputeTokenColumn` and `AdjustNodeIndentation`, used by `FormatNodeInDocumentAsync` and `FormatNodeInDocumentWithContextAsync`) as "Not changed here". So this is the maintainer's choice.

The four tests assert what RH5303-005 and SHARED-INIT-002 require: the fix lays the element out as the formatter does, and the fix converges. I do not dispute them. The test implementer's handover notes that thoenissen/Reihitsu#1382 will later make the formatter break before a token behind a multi-line comment's last line (SHARED-COMMENT-012). That would remove this input shape.

## Architecture findings

**The node-level column move and the indentation phase decide the column of a line that starts inside a comment differently.**
- **Components:**
  - `ReihitsuFormatterHelpers.ComputeTokenColumn` / `AdjustNodeIndentation`: the step after the pipeline in `FormatNodeInDocumentAsync` and `FormatNodeInDocumentWithContextAsync`.
  - The indentation phase: `LayoutComputer.GetColumnBeforeRawStringShift` and `TryGetLineTextColumn`.
- **Shared decision:** the final column of a token on a line that starts inside text that began on an earlier line.
  - The indentation phase takes the token's column in its input as final, because it does not place that line.
  - The node-level step moves every other line afterwards, by the difference between the target's document column and its formatted column, but not this one.
  - So the phase's columns are final only when that difference is zero.
- **Input that shows it:** the fixture above, on the `EqualsValueClauseSyntax` target. Output 38/34; document level gives 32/28.
- **Why it reaches the run:** it is a pre-existing structural defect that the approved FMT-INDENT-030 change newly exposes. The skeptic's follow-ups 1 and 2 in `02-skeptic.md` name the same step: it counts a tab as one column, and it does not move a raw string's content lines. This is a third member of that class.

## Follow-up candidates

- The skeptic's follow-ups 1 and 2 (`02-skeptic.md`) and the finding above all trace back to the column move after the pipeline in the node-level entry points.
- From `05-implementer.md`, still open:
  - A node-level target with a tab in its own whitespace is detached by the first phase.
  - No committed test runs the indentation phase on an attached node that holds a raw string.
- RH5201 against a chain whose first token stands behind a comment that leads its line (change doc row 10) is unchecked (`07-test-implementer.md`, follow-up 2).
