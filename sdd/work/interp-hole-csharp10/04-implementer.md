Role: implementer
Commit: 9afcc6b
Verdict: test disputed

My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decisions.

42 of the 44 handed-over tests pass now. The remaining two (`FixAllLeaves…`) fail because of a harness setting in the tests, not because of the production code; see *Disputed tests or scope*.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/StringInterpolationUtilities.cs` | New overload `IsInsideSingleLineInterpolation(SyntaxNode)` that reads the language version from the node's `SyntaxTree.Options`, so analyzers and fixes ask one owner instead of each repeating the parse-options read | SHARED-DIAG-011 (position) |
| `src/Reihitsu.Analyzer/Base/StatementShouldBePrecededByABlankLineAnalyzerBase{TStatement}.cs` | `OnStatement` skips a statement inside such a hole (covers RH5001–RH5010, RH5012–RH5019, RH5021, RH5029) | SHARED-STMT-005 |
| `src/Reihitsu.Analyzer/Base/StatementShouldBeFollowedByABlankLineAnalyzerBase{TStatement}.cs` | same in the "followed by" base (only user: RH5011) | RH5011-009 |
| `src/Reihitsu.Analyzer/Base/TargetAttributePlacementAnalyzerBase.cs` | A separate-line violation inside such a hole is not a violation (single-line mode unchanged) | SHARED-ATTR-024 (RH5513, RH5527) |
| `src/Reihitsu.Analyzer/Rules/Layout/RH5103…Analyzer.cs` | `AnalyzeStatements` returns early for a block/switch section inside the hole | RH5103-006 |
| `src/Reihitsu.Analyzer/Rules/Layout/RH5110…Analyzer.cs` | `AnalyzeConstraintClauses` returns early for a declaration inside the hole | RH5110-004 |
| `src/Reihitsu.Analyzer/Rules/Layout/RH5206…Analyzer.cs` | `OnSwitchExpression` returns early | RH5206-004 |
| `src/Reihitsu.Analyzer/Rules/Layout/RH5301…Analyzer.cs`, `RH5303…Analyzer.cs` | `On…InitializerExpression` returns early | SHARED-INIT-003 |
| `src/Reihitsu.Analyzer/Rules/Layout/RH5403…Analyzer.cs` | `OnBlock` returns early | RH5403-004 |
| `src/Reihitsu.Analyzer/Rules/Layout/RH5417…Analyzer.cs` | Private copy `IsInsideSingleLineInterpolation(BlockSyntax)` deleted; calls the Core overload (FC-015) | RH5417-005 (unchanged behavior) |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3004UseLambdaSyntaxCodeFixProvider.cs` | `WithholdsFixForLines`: on the block-body path only, withholds when the anonymous method is inside the hole; expression path untouched | RH3004-010 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3208…CodeFixProvider.cs` | `CanRegisterCodeFix` withholds when the local function is inside the hole; the shared `ExpressionBodyToBlockCodeFixProviderBase` is untouched, because its other users act on members that cannot stand in a hole | RH3208-004 |

XML summaries of every changed method were updated to name the new case (FC-031).

## Policy owners

`rg -n "IsInsideSingleLineInterpolation|CanInterpolationsHoldLineBreaks|InterpolatedStringStartToken\)" src --glob '*.cs'`:
- Owner: `Reihitsu.Core.StringInterpolationUtilities.IsInsideSingleLineInterpolation(SyntaxNode, LanguageVersion)` and the new `(SyntaxNode)` overload that delegates to it.
- Callers: the formatter (`ControlFlowBraceTransform`, `SingleLineInterpolationUtilities`, `DocumentationCommentFormattingPhase`, `SwitchCaseBraceRewriter`, with the context's language version, unchanged), and the analyzer/fix call sites listed above. There is no private copy left; RH5417's copy was the only one and is removed.

## Production diff size

`git diff --shortstat origin/main...HEAD -- src`: 13 files changed, 79 insertions(+), 30 deletions(-). No new type. One new public member: the `StringInterpolationUtilities.IsInsideSingleLineInterpolation(SyntaxNode)` overload in `Reihitsu.Core` (a shared helper with other consumers). It replaces the parse-options read that each of the 12 call sites would otherwise repeat, as RH5417 did privately. Reported here per *Scope*; the alternative is an inline `SyntaxTree.Options is CSharpParseOptions` read at each site.

## Deleted special cases

_N/A._

## Tests run

```shell
scripts/build.sh --configuration Debug                       # Build succeeded, 0 warnings
scripts/test.sh --configuration Debug --project analyzer --filter "<the handover's focused filter>"
# Failed: 2, Passed: 153, Total: 155  (the two FixAllLeaves… tests, see below)
scripts/test.sh --configuration Debug --project analyzer     # Failed: 2, Passed: 12252, Total: 12254 (same two)
scripts/test.sh --configuration Debug --project core         # Passed: 901
scripts/test.sh --configuration Debug --project architecture # Passed: 198
```

## Disputed tests or scope

**Test disputed: `RH3004UseLambdaSyntaxAnalyzerTests.FixAllLeavesTheBlockBodyInsideANonVerbatimInterpolationBeforeCSharp11` and `RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzerTests.FixAllLeavesTheLocalFunctionInsideANonVerbatimInterpolationBeforeCSharp11`.**

- *What they assert*: the fixed source keeps the hole diagnostic, marked as `{|RH3004:delegate|}` / `{|RH3208:…|}` markup in `fixedCode`. Method `N` is fixed. This is what RH3004-010 / RH3208-004 state.
- *Why they fail*: the verifier processes the fixed and batch-fixed states with the default `MarkupHandling`, which ignores markup for a fixable diagnostic. The expected count in the fixed state is therefore 0, the actual is 1 (the hole diagnostic, which the spec says must stay): `Context: Diagnostics of fixed state — Mismatch between number of diagnostics returned, expected "0" actual "1"`. With the production change the fixed text is as expected; only the count check fails.
- *Evidence*: the repository's existing pattern for a withheld diagnostic that stays in the fixed source is `test.FixedState.MarkupHandling = MarkupMode.Allow; test.BatchFixedState.MarkupHandling = MarkupMode.Allow;` (e.g. `RH8204DoNotUsePlaceholderElementsAnalyzerTests.cs:454-455`, `RH8309…Tests.cs:1288`). I added exactly those two lines to the `onConfigure` lambda of both tests in a temporary copy, and the focused run passed (30/30). I then restored the test files unchanged; they are not part of my commit.
- *Note for the test implementer*: their handover lists these tests as "Fails now (repro): the hole is rewritten over lines". On `main` they would fail for that reason too, but they can never pass as written.
- *Proposed repair* (test implementer's file, not mine): add the two `MarkupHandling = MarkupMode.Allow` lines next to the language-version transform in both tests. No assertion, input, or expected output changes.

**Scope**: the one new public Core overload, above.

## Architecture findings

_None_ beyond the skeptic's FC-028 finding (node-level entry points detach the target), which lead decision 4 settles by refusing in the fix. My change follows that decision and does not touch the entry points.

## Follow-up candidates

_None._
