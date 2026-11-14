Role: implementer
Commit: 3c1336e
Verdict: test disputed

My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the maintainer's approved new base type (Q3 B), which is recorded in `00-lead.md`.

Every handed-over test passes except two SHARED-FIX-012 Fix All tests. Both fail on test configuration, not on behavior: see *Whole run*, *Disputed tests or scope*.

## thoenissen/Reihitsu#1600

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Utilities/ReihitsuFormatterHelpers.cs` | New `HoldsHidingCarrier(SyntaxNode root)`, the one definition of a hiding carrier. Quick exit without directives. It walks the outermost `#if`…`#endif` spans (active and inactive directives, unclosed groups left out). Tokens and trivia starting in a span are checked: verbatim/raw/UTF-8 string tokens, the whole `InterpolatedStringExpressionSyntax` behind a `$@"`/raw start token, and `/* */`/`/** */` trivia. A carrier counts if a line after its first starts with whitespace, `#`, whitespace, then the word `elif`, `else`, or `endif` | FMT-PIPE-033 |
| `src/Reihitsu.Formatter/ReihitsuFormatter.cs` | `FormatDocumentAsync` and `FormatSyntaxTree` return their input unchanged with `Unsupported` null when the tree holds a carrier. Order: syntax error → generated → carrier → pipeline (precheck), so a carrier beside an unsupported shape is not reported. New public `HoldsHidingCarrier(SyntaxTree, CancellationToken)` | FMT-PIPE-033, SHARED-FIX-012, CLI-021, PLAY-009 |
| `src/Reihitsu.Cli/FormatCommandHandler.cs`, `FileProcessResult.cs` | After the syntax-error and generated checks, a carrier file returns `SkippedHiddenCode`. It prints `Skipped (code hidden in another build configuration): <path>` only with `--verbose`. The summary gets `Skipped <n> file(s) with code hidden in another build configuration.` after the syntax-error line. The file is not written (also with `--utf8-bom`) and does not affect the exit code | CLI-021, CLI-014, CLI-015 |
| `src/Reihitsu.Playground/PlaygroundFormatter.cs`, `PlaygroundFormatResult.cs`, `Pages/Home.razor.cs` | `PlaygroundFormatResult` gains `HoldsHiddenCode`. The page shows "The input contains code hidden in another build configuration and was left unchanged." | PLAY-009 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/ReihitsuCodeFixProviderBase.cs` (new) | Common base of every provider. Its sealed `RegisterCodeFixesAsync` returns without registering when the diagnostic's tree holds a carrier, then calls `protected abstract RegisterCodeFixesCoreAsync`. It caches the verdict per tree in a `ConditionalWeakTable` (04-skeptic item 6). `internal static HoldsHidingCarrierAsync(Document)` serves fixes that change other documents | SHARED-FIX-012 |
| All 105 providers and provider bases that derived from `CodeFixProvider` | Mechanical change. They now derive from `ReihitsuCodeFixProviderBase`, and `public … RegisterCodeFixesAsync` became `protected … RegisterCodeFixesCoreAsync`, moved into a `#region ReihitsuCodeFixProviderBase` (RH7401). No body changed | SHARED-FIX-012 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs` | `RenameWithoutConflictAsync` refuses a rename whose changed documents include one that holds a carrier. Both registration (`HasRenameConflictAsync`) and the `CasingFixAllProvider` path go through this method, so the casing Fix All that bypasses registration is covered (04-skeptic item 4) | SHARED-FIX-012 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Design/RH2001…CodeFixProvider.cs` | Registration also checks every document `ExcludedCodeUtilities.GetReferringDocuments` gives for the private property (the type's partial parts) (04-skeptic item 5) | SHARED-FIX-012 |

### Policy owners

- `rg -n "HoldsHidingCarrier" src` finds one definition, `ReihitsuFormatterHelpers.HoldsHidingCarrier`. Its callers are the two whole-document entry points, the public `ReihitsuFormatter.HoldsHidingCarrier`, the CLI, the Playground, the code-fix base (cached), and the casing base and RH2001 through the base's `HoldsHidingCarrierAsync`.
- `rg -n '"endif"|"elif"' src` finds only the definition's word check.

### Notes

- The node-level entry points do not check carriers, as FMT-PIPE-033 requires; `NodeLevelEntryPointsDoNotCheckForACarrier` passes.
- `PipelineTraceCommand` (`scripts/trace.sh`) does not check carriers. It runs the phases for diagnosis, as before, and no spec covers it.

## thoenissen/Reihitsu#1150

### Changes

| File | What changed | Requirement |
|---|---|---|
| `ReihitsuCodeFixProviderBase.cs` | `protected virtual bool FormatsThroughFormatter => false`. When it is true, the base registers nothing in a tree with a syntax error. The verdict is `ReihitsuFormatterHelpers.HasSyntaxErrors`, the formatter's own FMT-PIPE-005 check, cached per tree | SHARED-FIX-011 |
| `EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase` (RH5412–RH5416), `ExpressionBodyToBlockCodeFixProviderBase<TNode>` (RH3205–RH3208), RH3202, RH3203, RH3004, RH2001, RH5101, RH5102, RH5110, RH5111, RH5112, RH5113, RH5201, RH5205, RH5206, RH5301–RH5308, RH5401, RH5403, RH5404, RH5408, RH5417, RH7101 | `protected override bool FormatsThroughFormatter => true` | SHARED-FIX-011 |

This is every provider whose result goes through `FormatNodeInDocumentAsync`, `FormatNodeInDocumentWithContextAsync`, or `FormatNode`. I checked them with `rg "ReihitsuFormatter\.(FormatNode|FormatNodeInDocument)"` and through the helpers `BlankLineCodeFixUtilities`, `FluentChainCodeFixHelper`, and `ExpressionBodyCodeFixUtilities`.
- RH7101 is not in the test list. Its analyzer reports nothing in a tree with a syntax error, so nothing changes visibly.
- The fixes that only call `OrganizeUsingDirectivesInDocumentAsync` or `FormatDocumentationElementInDocumentAsync`, and the text-edit fixes such as RH6023, keep the default. `FixIsStillOfferedInAFileWithASyntaxError` passes.

## thoenissen/Reihitsu#1274

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/Precheck/PrecheckPhase.cs` | `FindUnsupportedSyntax` walks `DescendantNodesAndSelf` once. For each `ConditionalExpressionSyntax` it finds the first comment, directive, or disabled text in the `?` trailing trivia, the `WhenTrue` first-token leading trivia, the `:` trailing trivia, or the `WhenFalse` first-token leading trivia. The location is the trivia's full span. Across both shapes it reports the one that starts first. New `ConditionalOperatorCommentReason`. A conditional that is not inside the node is not looked at (FMT-UNSUP-003) | FMT-UNSUP-008, FMT-UNSUP-003, FMT-UNSUP-007 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/TernaryLineBreakRewriter.cs` | Deleted both refusals: `WouldJoinAcrossUnjoinableTrivia(questionToken, WhenTrue first token)` in `MoveQuestionTokenToNextLine`, and `WouldJoinAcrossUnjoinableTrivia(colonToken, WhenFalse first token)` in `NormalizeColonTokenPosition` | change doc *Special cases / Deleted* |

RH5308-005 and the RH7101 test pass without a fix-side change: both fixes already ask `FindUnsupportedSyntax` at registration (SHARED-FIX-010).

### Deleted special cases

- `TernaryLineBreakRewriter.MoveQuestionTokenToNextLine`: the `WouldJoinAcrossUnjoinableTrivia` refusal (change doc, *Deleted*, first bullet).
- `TernaryLineBreakRewriter.NormalizeColonTokenPosition`: the same refusal for the `:` (change doc, *Deleted*, second bullet).
- I found no further special case for this shape: `rg -n "WouldJoinAcrossUnjoinableTrivia|ConditionalExpression" src/Reihitsu.Formatter` shows only the shared join guard and the kept items of *Checked and kept*.

## thoenissen/Reihitsu#1143

### Changes

| File | What changed | Requirement |
|---|---|---|
| RH3001, RH3102, RH5110, RH7004, RH8204 providers | `GetFixAllProvider` returns `DocumentOrderFixAllProvider.Create(new <rule>Analyzer())` instead of `BatchFixer`. Their fixes overlap or nest, so the batch merge dropped one | SHARED-FIX-002 |
| `TypeMemberOrderingCodeFixProviderBase` + RH7102, RH7103, RH7109 | The constructor takes the analyzer, and Fix All is `DocumentOrderFixAllProvider`. Two moves in one type overlap in text | SHARED-FIX-002 |
| `CommentSafeSpanReplacementCodeFixProviderBase` | `GetFixAllProvider` is no longer `sealed` | SHARED-FIX-002 |
| RH6016 provider | Overrides `GetFixAllProvider` with `DocumentOrderFixAllProvider`, because the `?` and `.` replacements share the gap between them. The other derived classes of the base keep `BatchFixer` | SHARED-FIX-002 |
| `src/Reihitsu.Analyzer.CodeFixes/Core/DocumentOrderFixAllProvider.cs` | The first pass covers only the starting diagnostics whose fix the provider offers on the starting document (new `GetDiagnosticsWithOfferedFixAsync`). Moving RH7103 to it otherwise broke the existing `OneFixAllApplicationCorrectsOnlyTheDiagnosticsWhoseFixIsOfferedOnTheStartingCode`. There, `_b`'s guard releases once `_a` moved, and SHARED-FIX-002 says such a diagnostic is not covered. Re-analysis passes after a lost diagnostic are unchanged, so nested diagnostics (RH3004) are still fixed. Class summary updated | SHARED-FIX-002 |

### Policy owners

- `rg -ln "DocumentOrderFixAllProvider.Create" src` finds the owner of in-order Fix All.
- Its existing users are RH3004, RH3005, RH3101, RH3105, RH3202, RH5417, and `EmptyParenthesesCodeFixProviderBase`. They all get the starting-code filter, and every one of their tests passes.

## thoenissen/Reihitsu#1673

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5308…CodeFixProvider.cs` | Registration needs a format target (enclosing statement or member). Before, a missing target registered the fix (`is not { } target \|\| …`); now it withholds the fix. `ApplyCodeFixAsync`'s null check stays as a defensive branch | RH5308-006, SHARED-FIX-005 |

## thoenissen/Reihitsu#1678

No change (Q5 A), and no tests were handed over.

## Whole run

### Production diff size

`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 120 files changed, 1476 insertions(+), 379 deletions(-).

- About 105 of the files are the mechanical base-class change: base, override name, and region. Their bodies did not change.
- New type: `ReihitsuCodeFixProviderBase` (approved by the maintainer, Q3 B).
- New public member: `ReihitsuFormatter.HoldsHidingCarrier(SyntaxTree, CancellationToken)`. The Playground has no `InternalsVisibleTo`, so it needs a public query. The CLI uses it too, so that every caller outside the formatter asks the same public method.
- New record member: `PlaygroundFormatResult.HoldsHiddenCode`.
- New counter: `FileProcessResult.SkippedHiddenCodeCount`.

### Tests run

All runs in Debug.

- `scripts/test.sh --configuration Debug --project formatter`: Passed 4930, Failed 0.
- `--project cli`: Passed 279. `--project playground`: Passed 23. `--project architecture`: Passed 117. `--project core`: Passed 651. `--project tooling`: Passed 126.
- `dotnet test src/Reihitsu.Analyzer.Test -c Debug --no-build`: Failed 2, Passed 9087, Total 9089. The two failures are the disputed tests below.
- `scripts/test.sh --project analyzer` sometimes aborts with "Test host process crashed", out of memory, when MSBuild nodes are still resident. The test host peaks at about 10.7 GB RSS. The base commit peaked at about 11.4 GB in the same measurement, so this is environmental, not a regression of this change. Running after `dotnet build-server shutdown` completes.
- The Release build (`scripts/build.sh`) is clean, with no warnings.
- `scripts/format.sh --force` over every changed production file: 0 files still need formatting.

### Disputed tests or scope

1. **`RH4103MethodNameCasingAnalyzerTests.FixAllLeavesADocumentWithAHidingCarrierUnchanged` and `RH6024BinaryOperatorsMustBeSpacedCorrectlyAnalyzerTests.FixAllLeavesADocumentWithAHidingCarrierUnchanged` (SHARED-FIX-012).**
   - **What they assert:** after Fix All, the carrier document keeps its marked diagnostic (`{|RH4103:useTotal|}` and `{|RH6024:-|}` in `FixedState.Sources`).
   - **Why they fail:** the framework's default `FixedState.MarkupHandling` ignores fixable-diagnostic markup in the fixed state. It expects 0 diagnostics and finds the 1 the spec requires to stay. The message is "Context: Diagnostics of fixed state … expected "0" actual "1"" at `/0/Test1.cs(5,20)` and `(12,17)`.
   - **Evidence:** I added `test.FixedState.MarkupHandling = MarkupMode.Allow; test.BatchFixedState.MarkupHandling = MarkupMode.Allow;` locally and did not commit it. With that, both tests pass against this implementation. That is the pattern `RH7103…OneFixAllApplicationCorrectsOnlyTheDiagnosticsWhoseFixIsOfferedOnTheStartingCode`, RH8204, and RH5030 already use for a diagnostic that stays.
   - **Spec agreement:** the behavior is the spec's (SHARED-FIX-012: no fix in the carrier document; the diagnostic stays). Only the test configuration is wrong. The fix is the test implementer's.
2. **Scope growth to report.**
   - The new base type (approved) and the mechanical change of 105 providers.
   - The public `ReihitsuFormatter.HoldsHidingCarrier`.
   - The starting-code filter in the shared `DocumentOrderFixAllProvider`. It now also applies to RH3004, RH3005, RH3101, RH3105, RH3202, RH5417, and the empty-parentheses rules. It narrows their Fix All to SHARED-FIX-002's covered set.
   - `CommentSafeSpanReplacementCodeFixProviderBase.GetFixAllProvider` is no longer sealed.
3. **`SameLineGapRulePhaseTests.DocumentationCommentsAroundTheQuestionMarkKeepTheirPlace`.** The test implementer left it unchanged. It passes. Its `b ?/** y */1` part is unreachable through any entry point now, because the precheck refuses it. Dropping that part is the cleaner's call.

### Architecture findings

_None._ The run's shared decisions each have one owner:
- the carrier definition: `ReihitsuFormatterHelpers.HoldsHidingCarrier`;
- the unsupported-shape predicate: `PrecheckPhase.FindUnsupportedSyntax`;
- in-order Fix All: `DocumentOrderFixAllProvider`;
- the per-document fix offering: `ReihitsuCodeFixProviderBase`.

### Follow-up candidates

- **`PlaygroundFormatter.HasSyntaxErrors`** is a private copy of `ReihitsuFormatterHelpers.HasSyntaxErrors` (FC-015). It predates this run. The Playground could ask a public formatter query, as it now does for carriers. Small.
- **`scripts/trace.sh` (`PipelineTraceCommand`)** traces the phases of a file that whole-document formatting leaves alone under FMT-PIPE-033, without saying so. For syntax errors and generated files it reports the skip (`PipelineTraceCommand.cs:201`, `:208`). Decide whether it should report this skip too. Small, tooling.
