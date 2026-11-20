Role: implementer
Commit: 7ee7c97
Verdict: tests pass

Inputs: thoenissen/Reihitsu#851 (`agent-filed`), `00-lead.md`, `02-skeptic.md`, `03-spec-author.md`, `04-test-implementer.md`, change doc `sdd/changes/2026-11/rh5419-indexer-arrow-single-line.md`, spec `sdd/specs/analyzer/RH5419.md`. The prompt carried no intended fix or verdict. The tests failed as the handover said: the analyzer test project did not compile, with CS0246 for the two missing types.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Layout/RH5419ExpressionStyleGetOnlyIndexersShouldBeSingleLinedAnalyzer.cs` (new) | Registers `SyntaxKind.IndexerDeclaration`. It reports an indexer with an expression body when its `[` and `]` start on different lines, or when its `=>` or the first token of its expression does not start on the line of the `]`. The diagnostic is on the whole declaration. | RH5419-001, RH5419-002 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5419ExpressionStyleGetOnlyIndexersShouldBeSingleLinedCodeFixProvider.cs` (new) | Fix and Fix All (`BatchFixer`). See *How the fix works* below. | RH5419-003, RH5419-004, SHARED-FIX-007, SHARED-FIX-010 |
| `src/Reihitsu.Analyzer.CodeFixes/Core/AttributeListCodeFixUtilities.cs` (new, internal static) | The two attribute-list guards of RH5401-004, moved out of the RH5401 provider unchanged in logic and generalized from `PropertyDeclarationSyntax` to `MemberDeclarationSyntax`: `AnyHoldsBlankLineDirectiveDisabledTextOrOwnLineCommentBetweenBrackets` and `KeepsBlankLinesBehindAttributeLists`. The second now returns `true` for a member without attribute lists, which the RH5401 caller checked before. | RH5401-004, RH5419-004 (FC-015) |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedCodeFixProvider.cs` | Calls the shared guards instead of its private copies, which are deleted. RH5401's behavior is unchanged. | RH5401-004 |
| `src/Reihitsu.Analyzer/AnalyzerResources.resx` | `RH5419Title` and `RH5419MessageFormat`, both "Expression style get-only indexers should be single lined." | RH5419 title (maintainer, batch 1, Q1) |
| `src/Reihitsu.Analyzer.CodeFixes/CodeFixResources.resx`, `CodeFixResources.cs` | Code-action title `RH5419Title` = "Collapse indexer to single line" (mirrors RH5401's "Collapse property to single line"), with its wrapper property. | — |
| `Reihitsu.sln` | `documentation\rules\RH5419.md` in the Documentation folder. | — |

The rule page `documentation/rules/RH5419.md` and the row in `src/Reihitsu.Analyzer.Package/README.MD` already existed (spec author), so I left them unchanged.

### How the fix works

The provider offers the fix only when all of these hold:

1. `ReihitsuFormatter.FindUnsupportedSyntax(indexer)` is null. This covers the comment and directive cases around the arrow (FMT-UNSUP-012/013, SHARED-FIX-010).
2. When the parameter list spans several lines, no token gap from the `[` through the `]` holds a comment, a directive, or disabled text. The leading trivia of `[` and the trailing trivia of `]` are outside the list. The check uses `SyntaxTriviaUtilities.WouldJoinAcrossUnjoinableTrivia`, the existing Core predicate.
3. No attribute list holds a blank line, a directive, disabled text, or an own-line comment between its brackets (shared guard).
4. The formatted joined indexer passes two checks. The indexer is joined in its tree, then formatted with `ReihitsuFormatter.FormatNode`, as RH5401 formats its property.
   - The text from its `[` through its `;` holds no line break. This catches a multi-line literal in the list, or an expression the formatter keeps wrapped.
   - The blank lines behind every attribute list are unchanged (shared guard).

Joining the list: in every gap from `[` through `]` that holds an `EndOfLineTrivia`, the fix removes the trailing trivia of the token in front, and replaces the leading trivia of the token behind with `LineBreakTriviaUtilities.CreateJoinSeparator`. That is the formatter's own minimal join separator (FMT-BREAK-031). Its type is one that `CodeFixFormatterAccessTests.BannedTypesExcludeHelpersAndUtilityClasses` explicitly allows code fixes to use (FMT-PIPE-009).

Applying the fix: the fix replaces the indexer with the joined one and then calls `BlankLineCodeFixUtilities.FormatMemberOnLinesOfItsOwnAsync` on it. That call handles SHARED-FIX-007 and formats the indexer in place through `ReihitsuFormatter.FormatNodeInDocumentAsync` (FC-030).

## Policy owners

- Attribute-list guards of RH5401-004 / RH5419-004: `rg -n "HoldsBlankLineDirectiveDisabledTextOrOwnLineCommentBetweenBrackets|KeepsBlankLinesBehindAttributeLists" src --glob '!*Test*'`. The only owner is `AttributeListCodeFixUtilities`, and its callers are the RH5401 and RH5419 providers. No private copy is left.
- Join separator: `rg -n "WouldMergeIntoOneToken\(" src --glob '!*Test*'`. The owner is `LineBreakTriviaUtilities.CreateJoinSeparator` (formatter), which RH5419 calls. The other hits are `TokenSeparationUtilities` itself (Core), `ExpressionBodyToBlockConverter`, and RH3002's fix. Each of those is a separate, pre-existing use for a different decision, and RH5419 adds no copy.
- Unjoinable trivia in a gap: `SyntaxTriviaUtilities.WouldJoinAcrossUnjoinableTrivia` (Core), reused.
- Unsupported shapes: `ReihitsuFormatter.FindUnsupportedSyntax`, reused.

## Production diff size

`git diff --cached --shortstat origin/main...HEAD`-equivalent over the staged production paths: `7 files changed, 455 insertions(+), 89 deletions(-)`. Two new types are the issue's own rule: the analyzer and the code-fix provider. One new internal static helper type, `AttributeListCodeFixUtilities`, takes the moved RH5401 guards (about 120 lines with docs, roughly 89 of them moved).

## Deleted special cases

_N/A._ No shape is declared unsupported.

## Tests run

- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5419|FullyQualifiedName~RH5401"`: `Passed! - Failed: 0, Passed: 212, Skipped: 0, Total: 212` (final tree). The RH5419 tests alone gave 86/86.
- `scripts/test.sh --configuration Debug --project analyzer`: `Passed! - Failed: 0, Passed: 11365, Skipped: 0, Total: 11365`. This run was before the last two edits, which touched only the RH5419 provider: removing a nested ternary (S3358) and switching to `CreateJoinSeparator`. The focused run above covers both.
- `scripts/test.sh --configuration Debug --project architecture`: `Passed! - Failed: 0, Passed: 198, Skipped: 0, Total: 198` (final tree).

## Disputed tests or scope

- **Scope: one new shared helper type with two consumers.** `AttributeListCodeFixUtilities` exists so the RH5401-004 attribute-list guards are shared rather than copied. The skeptic flagged this as an FC-015 risk, and the test implementer relies on it: RH5401's tests are the guard's tests. RH5401's provider changed only by delegating to it.
- No disputed tests.

## Architecture findings

_None._

## Follow-up candidates

_None_ beyond the lead's existing formatter candidate (joining a wrapped indexer parameter list in the formatter).
