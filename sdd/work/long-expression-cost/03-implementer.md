Role: implementer
Commit: d7f4213
Verdict: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict. The skeptic's mechanism notes were treated as hypotheses; both were confirmed by the timings below before and after the change.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/TernaryLineBreakRewriter.cs` | Defect A. `BreakOperatorsOntoOwnLines` descends only into a nested conditional that holds no nested conditional of its own: the rewriter visits bottom-up, so a nested conditional with a nested conditional was already broken by its own visit (same method), and re-running it is a no-op. `NormalizeTernaryOperatorPosition` checks for a nested conditional before `LineBreakDetection.IsMultiLine`, which measured the rewritten node's text at every level. New private `HasNestedConditional`. Doc comments rewritten (FC-031) | FMT-BREAK-018 (output unchanged); issue cost goal |
| `src/Reihitsu.Core/SyntaxTokenNavigationUtilities.cs` (new public static class) | Defect B. `GetPreviousToken(SyntaxToken)` / `GetNextToken(SyntaxToken)` return what Roslyn's default-form `token.GetPreviousToken()` / `GetNextToken()` return, but climb to the nearest ancestor whose full span reaches past the token and descend with `SyntaxNode.FindToken`, which picks children by position. Roslyn's navigation scans the parent's (and each climbed ancestor's) children one by one, which is linear in the sibling count per call and quadratic over a long list. Falls back to Roslyn's own call for a zero-width candidate that carries trivia, and when no ancestor reaches past the token (first/last token of a tree or of a structured trivia) | issue cost goal |
| 44 call sites in `src/Reihitsu.Formatter` (all phases incl. precheck, structural transforms, doc comments, line breaks, switch-case braces, blank lines, horizontal spacing, indentation, cleanup, `ReihitsuFormatter`) and `src/Reihitsu.Core` (`SyntaxTokenPositionUtilities`, `TokenGapAnalysis`, `AdjacentTokenSpacingUtilities`, `FormattingTextAnalysisUtilities`, `RegionDirectiveBlankLineUtilities`, `FluentChain`, `AttributeTargetUtilities`, `SyntaxNodeUtilities`, `DeclarationModifierUtilities`, `OrderingMoveSafety`, `ConditionalGroupUnitUtilities`, `EmptyTypeDeclarationSemicolonAnalysisUtilities`, `GetOnlyMemberExpressionBodyRewriteUtilities`) | Every default-form `x.GetPreviousToken()` / `x.GetNextToken()` (and the one explicit `GetNextToken(includeZeroWidth: false)` in `AttributeTargetUtilities.TryGetTokenAfterAttributeList`) replaced by the helper. Mechanical; no other logic changed | – |
| `src/Reihitsu.Formatter/Pipeline/Cleanup/CleanupPhase.cs` (`CleanTrailingTrivia`) | The per-token `GetNextToken(includeZeroWidth: true)` became the helper, with the zero-width-inclusive Roslyn call only when the helper finds no token with text (only then can `EndOfFileToken` follow). A zero-width token in front of a token with text (omitted array size) takes neither branch either way, so the outcome is identical. Comment updated | – |

Sites deliberately left on Roslyn's navigation (not per token of a long list, or need zero-width tokens): `TokenLocator.TryGetPreviousToken` (`includeZeroWidth: true`, walks back over missing tokens), `DocumentationCommentFormattingPhase` lines 292/296 (per documentation comment, zero-width), `ControlFlowBraceTransform` lines 122/142 (zero-width). None appeared in the timings after the change.

## Policy owners

- `rg -n "\.(GetPreviousToken|GetNextToken)\(" src/Reihitsu.Formatter src/Reihitsu.Core` — after the change only the zero-width-inclusive calls listed above, the `CleanupPhase` fallback, and the helper's own fallbacks remain.
- `rg -n "FindToken\(" src/Reihitsu.Formatter src/Reihitsu.Core` — position lookups of lines (`RawStringLineShifts`, `RawStringLineUtilities`, `RegionDirectiveBlankLineUtilities`, `SyntaxIndentationUtilities`, `OrderingDeclarationUtilities`, `DocumentationCommentFormattingPhase`, `TokenLocator.GetCurrentToken`); none is a token-neighbour helper, so no private copy of the new policy exists.
- `rg -n "class .*Navigat" src -g '*.cs'` — only the new class.
- Ternary breaking has one owner, `TernaryLineBreakRewriter`; unchanged.

## Production diff size

Before committing, `git diff --shortstat -- src ':(exclude,glob)src/*Test*/**'`: 47 files changed, 197 insertions(+), 87 deletions(-) (about 75 of the insertions are the new class with its doc comments; most others are one-line call swaps plus `using Reihitsu.Core;`). New type: `Reihitsu.Core.SyntaxTokenNavigationUtilities` (public static, two methods).

## Deleted special cases

_N/A._

## Tests run

- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.FullPipeline.LongExpression"` — Passed 21 of 21 (cost tests 12, layout tests 9; before the change 11 cost tests failed).
- `scripts/test.sh --configuration Debug` (default run, before the last three Core call swaps): Analyzer 11279, Formatter 6136, Core 738, Cli 403, Architecture 198, Tooling 136, Playground 23 — all passed.
- After the last swaps: `--project core` 738 passed, `--project formatter` 6136 passed, `--project analyzer` 11279 passed. Release build warnings are the cleaner's; the Debug build has 0 warnings, 0 errors after `scripts/format.sh`.
- Output identity: the CLI built at d7f4213 and the changed CLI each formatted (`--force`) copies of the repository's `src` tree as is, converted to CRLF, and with all leading indentation stripped, plus the skeptic's long-list fixtures: 6019 files, 3978 rewritten; `diff -r` of the two outputs is empty.
- Timings, Debug CLI `--check`, wall clock incl. start-up, before → after: array 4000/8000 3.22/10.48 s → 0.51/0.70 s; ternary 1600/3200 2.84/9.62 s → 0.85/1.41 s; enums 8000 → 0.84 s, fields 4000/8000 1.06/1.78 s, statements 4000/8000 0.84/1.36 s, args 4000/8000 0.53/0.80 s (all linear).
- `Reihitsu.SelfHosting.Test` not run locally (not in the default run); CI runs it.

## Disputed tests or scope

- **Scope growth.** The change adds a public member set (the new public static class in `Reihitsu.Core`, a shared helper with consumers in the analyzers' Core utilities too) and touches 47 production files, well over double the dozen sites the handovers name. Decision 2 asks for every implicated site; I swapped every default-form navigation in the formatter and in the Core helpers it calls rather than only the sampled ones, because any per-element call in a long list is quadratic and a partial swap leaves the hazard for the next sampled site (FC-016). The new class is the issue's own fix (escalation 9 exempts it), at about 75 lines. The Core swaps in analyzer-shared files (`ConditionalGroupUnitUtilities`, `EmptyTypeDeclarationSemicolonAnalysisUtilities`, `DeclarationModifierUtilities`, `OrderingMoveSafety`, `SyntaxNodeUtilities`) change no result, which the analyzer suite confirms; the lead may narrow them back to the formatter-hot sites if a smaller diff is preferred.
- No test disputed.

## Architecture findings

_None._

## Follow-up candidates

_None_ beyond the deep-spine stack overflow already recorded in 00-lead.md.
