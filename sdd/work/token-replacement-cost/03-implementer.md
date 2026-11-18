Role: implementer
Commit: 06d658a
Verdict: tests pass

My prompt named the issue, the slug, the lane, the branch, the handovers, and the probe. It carried no intended fix, suspected cause, or hoped-for verdict.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/Core/Rewriter/TokenRewriter.cs` (new) | `internal sealed class TokenRewriter : CSharpSyntaxRewriter` (`visitIntoStructuredTrivia: false`) with `static SyntaxNode RewriteTokens(SyntaxNode root, Func<SyntaxToken, SyntaxToken> rewrite)`; `VisitToken` hands each rewrite the token of the input tree. One tree walk, so the cost grows with the token count. It visits the same tokens as `ReplaceTokens(root.DescendantTokens(), …)`: no tokens inside structured trivia, and missing tokens too | #1862 (decision rows 1, 3) |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/IndentationRewriter.cs` | `Apply` uses `TokenRewriter.RewriteTokens` instead of the whole-tree `ReplaceTokens`; `ApplyIndentation` takes one token instead of `original`/`rewritten` (in the replaced call both were the same token: with no trivia in the replacement set and no structured trivia visited, Roslyn's replacer passes the unvisited token as `rewritten`); summary and parameter docs updated (FC-031) | #1862 |
| `src/Reihitsu.Formatter/Pipeline/Cleanup/CleanupPhase.cs` | `Execute` uses `TokenRewriter.RewriteTokens`; `CleanToken` takes one token; summary and parameter docs updated | #1862 |
| `src/Reihitsu.Formatter/Pipeline/TabReplacement/TabReplacementPhase.cs` | `:39` sibling site (decision row 3): the filtered `ReplaceTokens(tokens, …)` became `TokenRewriter.RewriteTokens` with the same `HasWhitespaceTab` filter in the callback; the "no tab → input node returned" shortcut is kept (`DescendantTokens().Any(HasWhitespaceTab)`). Summary updated | #1862, FMT-PIPE-019 unchanged |

The rewriter is in a `Rewriter` namespace because `NamespaceTests.RewriterShouldBeInRewriterNamespace` requires every `CSharpSyntaxRewriter` there.

Sibling sites not changed (decision row 3, the helper does not fit at no extra cost):

- `LineEndingNormalizationPhase.cs:31` replaces `XmlTextLiteralNewLineToken`s inside documentation-comment structured trivia, and `:45` replaces trivia (also inside structured trivia). The helper would need `visitIntoStructuredTrivia: true` and a trivia visitor, a second mode of the type. Cost per the skeptic's `mixed` variant (40 % foreign line endings): 31.9 → 124.1 ms N = 800 → 1600, about 4 % of the total before this fix; it needs mixed line endings to grow at all, and at most half the lines.
- `DocumentationCommentFormattingPhase.cs:183`/`:553` replace a dictionary-keyed set of relocated tokens and normalized trivia; the replacement values are computed beforehand from the original tree, a different shape. Skeptic's `docnospace` variant: 16.2 → 46.3 ms N = 800 → 1600, share of the replacement not separated.
- `TabReplacementPhase.cs:137`/`:150`: per-directive sets, bounded.

## Measurements (probe from `01-skeptic.md`, Release, best of 3, ms)

Before = a worktree at fe3d38f, after = 06d658a, same machine, same session. `plain` variant:

| N | Tokens | Indentation before → after | Cleanup before → after | `LayoutComputer.Compute` alone | Total before → after |
|---|---|---|---|---|---|
| 400 | 2409 | 115.3 → 40.6 | 57.1 → 6.9 | 18.0 / 20.8 | 285.4 → 170.8 |
| 800 | 4809 | 283.9–287.6 → 73.6–77.2 | 211.0–218.7 → 9.6–10.1 | 35–37 / 32.5–34.5 | 643.0–674.7 → 217.3–228.0 |
| 1600 | 9609 | 1196.9–1207.1 → 241.9–245.2 | 928.4–941.6 → 28.9–29.2 | 125.5–126.3 / 113.3–116.5 | 2609.8–2629.7 → 690.6–709.8 |

Cleanup now grows about 1.5 to 3× per doubling (6.9 → 10 → 29 ms; the last step is noisy at this size). Indentation grows about 3.2× per doubling: the remainder is `LayoutComputer.Compute` (113–127 ms at N = 1600) plus the per-token callback, the follow-up already recorded in `00-lead.md`.

`tabs` variant, tab-replacement sub-phase: 4.8 / 8.0–9.2 / 26.6–27.2 ms before → 3.0 / 2.7–3.8 / 5.1–5.3 ms after (N = 400 / 800 / 1600). Totals 2752.6–2811.7 → 754.2–765.4 ms at N = 1600.

Command: `dotnet run probe.cs -- <repo>/src/Reihitsu.Cli/bin/Release/net11.0 <plain|tabs>` after `scripts/build.sh`; raw output in the session scratchpad (`probe-results.txt`), not committed.

## Policy owners

- `rg -n "\.ReplaceTokens\(|\.ReplaceTrivia\(" src/Reihitsu.Formatter src/Reihitsu.Core` — the full list is the skeptic's table; after the change no call passes a root's whole `DescendantTokens()` except `TabReplacementPhase.cs:150`, whose root is one directive's structure (bounded).
- `rg -n ": CSharpSyntaxRewriter" src/Reihitsu.Formatter` — no existing rewriter that rewrites every token through a callback; `TokenRewriter` is the only one.

## Production diff size

`git diff --shortstat fe3d38f..06d658a -- src ':(exclude,glob)src/*Test*/**'`: 4 files changed, 100 insertions(+), 35 deletions(-). One new type, `TokenRewriter` (64 lines, mostly docs and region markers).

## Deleted special cases

_N/A._

## Tests run

```shell
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~CleanupPhaseWholeFileTests|FullyQualifiedName~IndentationRewriterWholeFileTests|FullyQualifiedName~LargeFileFullPipelineTests"
# Passed!  - Failed: 0, Passed: 16, Skipped: 0, Total: 16
scripts/test.sh --configuration Debug --project formatter
# Passed!  - Failed: 0, Passed: 5924, Skipped: 0, Total: 5924
scripts/test.sh --configuration Debug --project analyzer
# Passed!  - Failed: 0, Passed: 10932, Skipped: 0, Total: 10932
scripts/test.sh --configuration Debug --project cli
# Passed!  - Failed: 0, Passed: 396, Skipped: 0, Total: 396
scripts/test.sh --configuration Debug --project architecture
# Passed!  - Failed: 0, Passed: 198, Skipped: 0, Total: 198 (after moving TokenRewriter into the Rewriter namespace; the first run failed RewriterShouldBeInRewriterNamespace)
```

`scripts/build.sh` (Release) succeeded with no CS or RH warnings in the grep of its output; `scripts/format.sh` over the four files reports 0 of 4 changed.

## Disputed tests or scope

- **New type and the tab site.** `TokenRewriter` is a new internal type, shared by three consumers. It is the issue's own fix (README, escalation 9: not a structural change). The tab-replacement site `:39` joined under decision row 3, since it uses the helper with no extra machinery. The test implementer's handover says the tab site keeps its existing tests (`Regression/TabReplacement`) and asks to be sent back for whole-file tests if it changes; it changed, so the lead decides whether to route that. The existing formatter tests pass.

## Architecture findings

_None._

## Follow-up candidates

_None_ beyond those in `00-lead.md`. The measurement confirms them: after this change, at N = 1600, the largest remaining costs are indentation (≈ 245 ms, of which `LayoutComputer.Compute` ≈ 115), blank lines (≈ 187), and line breaks (≈ 122), each still growing about 3× or more per doubling.
