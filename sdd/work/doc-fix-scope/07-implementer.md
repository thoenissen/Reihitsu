Role: implementer
Commit: see `git log -1` on the branch (the commit that adds this file)
Verdict: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Work follows `00-lead.md` decisions 19–23 (decision 22 is the structural change), FMT-PIPE-032 at `ffbb0bb`, and `06-test-implementer.md`. No test file was changed.

## thoenissen/Reihitsu#1269

_No work in this round._ The two RH8201 event tests (decision 19) pass on the corrected inputs from `fa68bde`; no production change was needed.

## thoenissen/Reihitsu#1342

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/ReihitsuFormatter.cs` | New public `FormatDocumentationElementInDocumentAsync(Document, XmlElementSyntax, CancellationToken)`: detects the document's line ending, hands the element to the documentation phase's `LayOutElement`, and returns the document with the new text (or the unchanged document). No syntax-error, generated-code, or unsupported-syntax check | FMT-PIPE-032, FMT-PIPE-009, FMT-UNSUP-002 |
| `src/Reihitsu.Formatter/Pipeline/DocumentationComments/DocumentationCommentFormattingPhase.cs` | New `internal static LayOutElement(SyntaxTree, XmlElementSyntax, string, CancellationToken)`: rebuilds the outermost normalization candidates among the element and its descendants in the document's own tree, round after round (incremental `WithChangedText`, element re-found by its start position) until none is left, bounded by the phase's existing `MaximumNormalizationRounds`. The candidate selection and `BuildReplacement` call, formerly inline in `NormalizeOutermostElements`, moved to a shared private `BuildOutermostReplacements` that both the phase and `LayOutElement` use, so the element layout has one implementation. `GetElementStartingAt` helper; class summary names the new member | FMT-PIPE-032, FMT-DOC-008…011 |
| `src/Reihitsu.Analyzer.CodeFixes/Core/DocumentationCommentCodeFixUtilities.cs` | `LayOutElementAsync` now finds the element and calls `ReihitsuFormatter.FormatDocumentationElementInDocumentAsync`. Removed: the detached-copy formatting through `FormatNode`, the element mapping by ordinal, the re-indent of the element's further lines with the reported line's indentation (the copy of the formatter's `///` prefix decision), and the private `RemoveLineIndentation`. `CanLayOutElement` (registration checks, RH8305-005, RH8307-005, RH8307-006) unchanged, per decision 23 | RH8305-004, RH8307-004, FMT-PIPE-032 (decision 22) |

The column of the lines an element gains comes from `DocCommentElementNormalizer.BuildReplacement` → `DocumentationCommentUtilities.GetContinuationPrefix` on the start-tag line, which already strips a byte order mark; laying out in the document's own tree is what makes it the start-tag line's indentation, as FMT-PIPE-032 requires. No new decision was written for it.

### Disputed tests or scope

_None._ The new public member is the one FMT-PIPE-032 and decision 22 name.

## thoenissen/Reihitsu#1343

_No work in this round._

## thoenissen/Reihitsu#1459

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Core/DocumentationAnalysisUtilities.cs` | `IsPrimaryConstructorRun` (internal) moved in front of the private methods; body unchanged. Removes the RH7110 warning 05-spec-author.md saw | — (member order) |

The RH8305/RH8307 compilation-unit fixes go through the new entry point (see #1342).

### Disputed tests or scope

_None._

## Whole run

### Policy owners

- Element layout (FMT-DOC-008…011): `rg -n "BuildReplacement|BuildOutermostReplacements|LayOutElement\b|GetContinuationPrefix" src --glob '!*Test*'` — `DocCommentElementNormalizer.BuildReplacement` (one owner), called only from `DocumentationCommentFormattingPhase.BuildOutermostReplacements`, which both the phase's comment normalization and `LayOutElement` use. `ReihitsuFormatter.FormatDocumentationElementInDocumentAsync` is the only caller of `LayOutElement`; the code fixes reach it only through `ReihitsuFormatter` (FMT-PIPE-009; architecture tests pass). `GetContinuationPrefix` (Core) remains used by `BuildReplacement` and by `MoveContentToNextDocumentationLineAsync` (RH8304, unchanged, not a layout copy of the element).
- No code-fix code keeps its own element layout: `rg -n "FormatNode\(|TrimStart\(' ', '\\\\t'\)" src/Reihitsu.Analyzer.CodeFixes` finds only RH5401's unrelated `FormatNode` call.

### Production diff size

This round (`git diff --shortstat HEAD~1 -- src`): 4 files changed, 135 insertions(+), 84 deletions(-). No new type. New members: `ReihitsuFormatter.FormatDocumentationElementInDocumentAsync` (public), `DocumentationCommentFormattingPhase.LayOutElement` (internal), and the private `BuildOutermostReplacements` and `GetElementStartingAt`. Run total (`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`): 16 files changed, 480 insertions(+), 226 deletions(-).

### Deleted special cases

_N/A._ No shape is declared unsupported.

### Tests run

```shell
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~ReihitsuFormatterDocumentationElementTests"
# Passed: 8, Total: 8
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Documentation.RH83|FullyQualifiedName~Documentation.RH82|FullyQualifiedName~Documentation.RH84"
# Passed: 462, Total: 462
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~DocumentationComment|FullyQualifiedName~ReihitsuFormatterDocumentationElementTests"
# Passed: 373, Total: 373
scripts/test.sh --configuration Debug
# Analyzer 8092, Formatter 4837, Core 660, Cli 270, Architecture 117, Tooling 126, Playground 19 — all passed, 0 failed
```

`scripts/format.sh` over the four production files changed nothing; `scripts/build.sh` (Release) and `scripts/build.sh --configuration Debug` build without warnings (the RH7110 warning is gone).

### Architecture findings

_None._ Decision 22's finding is resolved: the copy of the formatter's prefix decision in `LayOutElementAsync` is deleted, and the element layout has one implementation in the documentation phase.

### Follow-up candidates

_None new._
