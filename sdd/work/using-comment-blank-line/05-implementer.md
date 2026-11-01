Role: implementer
Commit: 8c4600a
Verdict: tests pass

Scope read: `00-lead.md` (Maintainer answers 1–7, Decisions 1–2), `01-spec-author.md`, `02-skeptic.md`, `03-spec-author.md`, `04-test-implementer.md`, the accepted change doc `sdd/changes/2026-11/using-comment-blank-line.md`, and the spec diff it names (SHARED-USING-008/011, FMT-USING-002/010, RH7207-001/002/004). The prompt carried no intended fix, suspected cause, or hoped-for verdict.

Baseline on `8c4600a` before any production change: the focused formatter filter failed 33 of 75, the focused analyzer filter 29 of 154, as `04-test-implementer.md` states.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/UsingDirectiveOrderingUtilities.cs` | New `IsLedByComment(UsingDirectiveSyntax)`: the first non-whitespace, non-line-break trivia of the directive's leading trivia is a comment (`SyntaxTriviaUtilities.IsCommentTrivia`, so every comment kind). The one predicate RH7207 and the using phase share | SHARED-USING-011 |
| `src/Reihitsu.Analyzer/Rules/Organization/RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.cs` | `IsOrganized` accepts a same-group directive with a blank line in front of its first comment when `IsLedByComment` holds; summary updated | RH7207-001, RH7207-002, SHARED-USING-011 |
| `src/Reihitsu.Formatter/Pipeline/UsingDirectives/Utilities/UsingLeadingTriviaBuilder.cs` | New `HasBlankLineAboveLeadingComment(predecessorTrailingTrivia, current)`: the directive is led by a comment and the line breaks in front of that comment (one for the source predecessor's trailing trivia when it ends the line, plus those in the directive's own leading layout) are two or more | FMT-USING-010 |
| `src/Reihitsu.Formatter/Pipeline/UsingDirectives/Rewriter/UsingDirectiveOrderingRewriter.cs` | `OrganizeUsingDirectives` records, from the source order, every directive with a blank line above its leading comment, and the detached original first directive when its remaining trivia is led by a comment (the header's blank line counts). For such a directive that continues its group, the rebuilt leading trivia gets exactly one extra line break in front; summary updated | FMT-USING-002, FMT-USING-010 |

The RH7201–RH7207 code fixes needed no change: they run `UsingDirectiveOrderingPhase` through `UsingDirectiveCodeFixUtilities.OrganizeScopeUsingsAsync`. The block that cannot be reordered (`SeparateDirectivesSharingALine`) already keeps every blank line and is untouched. A comment-led directive that becomes the block's first through the reorder keeps `main`'s output (lead decision 2): the new line break is only written in the non-first branch.

The line breaks are counted across the predecessor's trailing trivia, not only the leading trivia, because inside the pipeline the documentation-comment phase can leave a predecessor whose line does not end, with the line break in the successor's leading trivia (the case the existing `HasLineBreakBeforeSignificantTrivia` branch handles); one line break there ends a line and is not a blank line.

## Policy owners

- `rg -n "IsLedByComment|HasBlankLineAboveLeadingComment|HasBlankLineBefore\(|HasLineBreakBeforeSignificantTrivia\(" src --glob '!*Test*'`
  - "a comment leads the directive": `UsingDirectiveOrderingUtilities.IsLedByComment` only (Core), called by RH7207 and the using phase.
  - "blank line above the directive / its first comment": `RH7207…Analyzer.HasBlankLineBefore` (pre-existing, analyzer, counts line breaks in the leading layout) and `UsingLeadingTriviaBuilder.HasBlankLineAboveLeadingComment` (new, formatter, also counts the predecessor's line end). See *Architecture findings*.
- `rg -ln "UsingDirectiveSyntax" src --glob '!*Test*' | xargs rg -ln "EndOfLineTrivia"`: `UsingTrailingTriviaBuilder`, `UsingLeadingTriviaBuilder`, the RH7207 analyzer — no other copy.

## Tests run

```shell
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~UsingDirectivePhaseCommentBlankLineTests|FullyQualifiedName~UsingDirectiveCommentBlankLineTests|FullyQualifiedName~PhaseStabilityCheckTests|FullyQualifiedName~SameGroupDirectiveBehindMultiLineDelimitedDocumentationCommentKeepsTheCommentWithIt|FullyQualifiedName~SeparatesDelimitedDocumentationFromPrecedingUsingDirectiveInOnePass"
```
Passed! - Failed: 0, Passed: 75, Total: 75

```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH7207|FullyQualifiedName~RH5020CommentsShouldBePrecededByABlankLineAnalyzerTests|FullyQualifiedName~RH7201|FullyQualifiedName~RH7203"
```
Passed! - Failed: 0, Passed: 154, Total: 154

Full projects (Debug): formatter 3744/3744, analyzer 6383/6383, core 544/544, cli 251/251, architecture 92/92, tooling 125/125. `scripts/build.sh --configuration Debug`: 0 warnings, 0 errors. `scripts/format.sh` on the four changed files: 0 of 4 reformatted.

## Disputed tests or scope

New public member: `UsingDirectiveOrderingUtilities.IsLedByComment` in `Reihitsu.Core`, shared by the analyzer and the formatter so the decision has one owner. `HasBlankLineAboveLeadingComment` is public on an `internal` class. No new dependency; four production files, the paths the handovers name.

## Architecture findings

None that meets the bar. Observation only: the "blank line before a directive" count has two implementations, RH7207's `HasBlankLineBefore` (leading layout only) and the formatter's new `HasBlankLineAboveLeadingComment` (leading layout plus the predecessor's line end). On parsed source they agree — the parser always puts a line's end in the predecessor's trailing trivia — so RH7207 never sees the pipeline-only shape where they differ. Left as is.

## Follow-up candidates

None.
