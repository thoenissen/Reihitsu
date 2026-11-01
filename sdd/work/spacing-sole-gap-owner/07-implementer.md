Role: implementer
Commit: 5d58174
Verdict: test disputed

The prompt named the inputs and the extra items (code owners, `AGENTS.md` rows, the `NoSpaceSpacingRule` comment, the full test runs, and a self-format check). It carried no intended fix, suspected cause, or hoped-for verdict.

Every red test of `04-test-implementer.md` and `06-test-implementer.md` passes except `RelationalPatternSpacingTests.DocumentationCommentInFrontOfTheOperandKeepsItsPlace`. That test, and 16 existing tests that assert the behavior the spec diff replaces, are disputed below. No test was changed.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Utilities/LineBreakTriviaUtilities.cs` | New `CreateJoinSeparator` (no whitespace, or one space where `TokenSeparationUtilities.WouldMergeIntoOneToken` says the pair would not read back) and `CreateJoinedTrailingTrivia`. `CollapseTokenToSameLine` writes the separator where it removes the previous token's line break. | FMT-BREAK-031 |
| `…/LineBreaks/Rewriter/ListElementInteriorJoinRewriter.cs` | The per-kind separator table (`CreateSeparator`, `IsTightBefore`, `IsTightAfter`) is removed. The join uses `CreateJoinedTrailingTrivia`. | FMT-BREAK-031, FMT-BREAK-015 |
| `…/LineBreaks/Rewriter/LineBreakAssignmentRewriter.cs` | The joins no longer add a space behind `=` or behind the target. They write the minimal separator. | FMT-BREAK-031, FMT-BREAK-016 |
| `…/LineBreaks/Rewriter/TernaryLineBreakRewriter.cs` | The branch behind `?` or `:` gets the minimal separator instead of a written space. | FMT-BREAK-031, FMT-BREAK-018 |
| `…/LineBreaks/Rewriter/SwitchCaseWhenLineBreakRewriter.cs` | `when` is joined with the minimal separator. | FMT-BREAK-031, FMT-BREAK-028 |
| `…/LineBreaks/Rewriter/BinaryOperatorLineBreakRewriter.cs` | The operator and the right operand are joined with the minimal separator, so `- -a`, `/ *p`, and `as string` no longer fuse. | FMT-BREAK-031, FMT-BREAK-017 |
| `…/LineBreaks/Rewriter/PropertyLayoutLineBreakRewriter.cs` | `CollapseExpressionBody` only joins, through `CollapseTokenToSameLine`. The spaces it wrote around the arrow, also where nothing was joined, are gone. | FMT-BREAK-031, FMT-BREAK-026 |
| `…/LineBreaks/Utilities/AccessorListLayout.cs` | `BuildAccessorTriviaReplacementMap`, which spaced the collapsed list, is removed. The collapse is joins only. Summary updated ("stays on its line"). | FMT-BREAK-031, FMT-BREAK-024 |
| `…/LineBreaks/Rewriter/LineBreakListRewriter.cs`, `ChainLineBreakRewriter.cs` | The joins of `(`, `,`, `)`, and chain gaps use `CreateJoinedTrailingTrivia`, so every join asks the shared helper. | FMT-BREAK-031 |
| `…/StructuralTransforms/Rewriter/AccessorExpressionBodyTransform.cs`, `GetOnlyMemberExpressionBodyTransform.cs` | The created `=>` carries no whitespace. | FMT-STRUCT-026 |
| `…/HorizontalSpacing/Rules/` (12 new types) | `AttributeTargetColonSpacingRule`, `ArrowSpacingRule`, `ConditionalOperatorSpacingRule`, `NameColonSpacingRule`, `WhenSpacingRule`, `DeclaredNameSpacingRule`, `AccessorListSpacingRule`, `RelationalPatternSpacingRule`, `PrefixKeywordSpacingRule`, `WithSpacingRule`, `LambdaReturnTypeSpacingRule`, `FunctionPointerCallingConventionSpacingRule` | FMT-SPACE-016 to FMT-SPACE-027 |
| `…/HorizontalSpacing/Utilities/SpacingPolicy.cs` | The rules are registered in the precedence order of FMT-SPACE-002. | FMT-SPACE-002 |
| `…/HorizontalSpacing/Rules/KeywordSpacingRule.cs` | The attribute-target branch is removed (FMT-SPACE-016 decides first), and the summary is updated. | FMT-SPACE-012 |
| `…/HorizontalSpacing/Rewriter/HorizontalSpacingRewriter.cs` | `TrimGapBeforeToken` becomes `NormalizeGapBehindLeadingComment`. It applies the rule's decision, 0 or N, to the whitespace between a comment in the right token's leading trivia and that token. `VisitToken` now decides the gap behind a token from the token as it stands in the tree. Before, the next token and the rules were evaluated on a rewritten, detached token, which has no parent and no next token, so that gap stayed undecided in that pass. | FMT-SPACE-014 (see *Disputed*), FMT-SPACE-016 to -027 comment cases |
| `…/HorizontalSpacing/Rules/NoSpaceSpacingRule.cs` | The remark that pointed at the chain rewriter now gives FMT-SPACE-006's reason (row 7 / spec author item 8). | — (comment) |
| `sdd/specs/formatter/horizontal-spacing.md`, `sdd/specs/README.md` | The 12 rule types are added to the code owners. | — |
| `AGENTS.md` | The `LineBreakPhase` row now says a join writes only the minimal separator and names `CreateJoinSeparator`/`CreateJoinedTrailingTrivia`. The `HorizontalSpacingPhase` row now says it is the only owner of same-line spaces and names `NormalizeGapBehindLeadingComment`. | — |

## Policy owners

- Join separator: `rg -n "WouldMergeIntoOneToken" src` → `Reihitsu.Core.TokenSeparationUtilities` (owner). Its callers are `LineBreakTriviaUtilities.CreateJoinSeparator` (every line-break join), `ExpressionBodyToBlockConverter` (FMT-STRUCT-026), and the RH3002 code fix. No private copy is left: the per-kind table in `ListElementInteriorJoinRewriter` is deleted.
- Spaces written in the line-break phase: `rg -n "SyntaxFactory\.Space" src/Reihitsu.Formatter/Pipeline/LineBreaks` → only `LineBreakTriviaUtilities.CreateJoinSeparator`. Outside the phase, `Reihitsu.Core.AttributeTargetUtilities.MergeAttributeListGroup` (lines 271, 296) is the exception FMT-BREAK-031 records, unchanged. `DeclarationModifierUtilities` and `RegionDirectiveUtilities` are not called from `LineBreaks/`.
- Spacing decisions: `rg -n "SpacingPolicy\." src/Reihitsu.Formatter` → only `HorizontalSpacingRewriter` (2 calls).
- Attribute-target colon: `rg -ln "AttributeTargetSpecifierSyntax" src/Reihitsu.Formatter` → only `AttributeTargetColonSpacingRule` (the `KeywordSpacingRule` branch is removed).

## Tests run

- Focused, Debug: the command of `04-test-implementer.md` (with `--configuration Debug`). Result: `Failed: 11, Passed: 336, Total: 347`. The 11 failures are the 10 `LineBreakRewriterTests.Collapses…` and `DocumentationCommentInFrontOfTheOperandKeepsItsPlace`, all disputed below.
- Release, `scripts/test.sh --project <p>` on 5d58174:
  - formatter `Failed: 17, Passed: 3924, Total: 3941`. All 17 are listed under *Disputed*.
  - analyzer `Passed: 6394`. This includes the new RH5408 and RH5401 tests.
  - cli `Passed: 251`.
  - architecture `Passed: 92`.
  - core `Passed: 563`, tooling `Passed: 125`, playground `Passed: 17`.
- `scripts/test.sh --project selfhosting` (on cab4c49, production code as now apart from comments and one rule refactor): `Failed: 1, Passed: 3785`. The failure is `WorkingDocumentsDirectoryContainsNoFiles`, which fails because `sdd/work/` exists; that folder is deleted before publishing.
- Self-format: `scripts/format.sh . --check` with the changed formatter gives `0 of 1821 file(s) need formatting` (4 skipped with syntax errors). `scripts/format.sh <changed .cs files> --check` gives `0 of 34`. Formatting the repository's own sources produces no diff.

## Disputed tests or scope

Each test below asserts output that a spec requirement of this run, or a maintainer answer, contradicts. They passed on `main` and fail now. None was changed.

1. **`LineBreakRewriterTests` (`src/Reihitsu.Formatter.Test/Unit/LineBreaks/LineBreakRewriterTests.cs`), 10 tests, line-break phase alone:**
   - `CollapsesAssignmentValueToEqualsLine` asserts `string result = CalculateValue();`.
   - `CollapsesVariableEqualsAndValueToTargetLine` asserts `int x = 42;`.
   - `CollapsesVariableEqualsToTargetLine` and `CollapsesVariableValueToEqualsLine` assert `var value = "test";`.
   - `CollapsesFieldValueToEqualsLine` asserts `_name = "default";`.
   - `CollapsesIndexAssignmentEqualsToTargetLine` asserts `values[0] = 42;`.
   - `CollapsesMemberInitializationValueToEqualsLine` asserts `Name = "test"`.
   - `CollapsesPropertyAssignmentEqualsToTargetLine` asserts `Value = "test";`.
   - `CollapsesRawMultilineStringAssignmentToEqualsLine` asserts `var text = """`.
   - `CollapsesInterpolatedRawMultilineStringAssignmentToEqualsLine` asserts `var text = $"""`.

   The phase now writes `string result =CalculateValue();` and `Value= "test";`, because FMT-BREAK-031 forbids the phase's own spaces. That is what `JoinSeparatorPhaseTests.AssignmentJoinsWriteNoWhitespace` asserts (`var x =1;`, `var y= 2;`). The two sets cannot both pass. The final output is unchanged: FMT-SPACE-010 spaces `=`. These tests sit next to the five `LineBreakRewriterTests` collapse tests that `04` updated, and were missed.
2. **`ExpressionBodiedTransformTests.ConvertsGetOnlyPropertyToExpressionBodiedProperty` and `ConvertsGetOnlyIndexerToExpressionBodiedIndexer`** (`src/Reihitsu.Formatter.Test/Unit/StructuralTransforms/ExpressionBodiedTransformTests.cs`, structural phase alone):
   - Input `int Value { get => _value; }` is expected to give `int Value => _value;`; the phase now gives `int Value=>_value;`.
   - The indexer test is expected to give `int this[int i] => _data[i];`; the phase now gives `int this[int i]=>_data[i];`.

   Amended FMT-STRUCT-026 says the transforms write no whitespace next to the created `=>`. The updated `StructuralTransformSeparatorTests.GetOnlyMemberTransformWritesNoWhitespaceAround…` asserts the empty gaps.
3. **`LineBreakPhaseIntegrationTests.HandlesExpressionBodiedPropertyAcrossLines`** (`src/Reihitsu.Formatter.Test/Integration/LineBreakPhaseIntegrationTests.cs:165`, line-break phase alone). Input `public int Value⏎    => 42;` is expected to give `public int Value => 42;`; the phase now gives `public int Value=> 42;`. This matches FMT-BREAK-031's example (`public int P=> 1;`) and `JoinSeparatorPhaseTests.PropertyArrowJoinsWriteNoWhitespace`.
4. **`ConditionalExpressionFullPipelineTests.SingleLineConditionalWithoutColonSpaceIsNotRewritten`** (`src/Reihitsu.Formatter.Test/Regression/FullPipeline/ConditionalExpressionFullPipelineTests.cs:566`). It asserts that `var x = cond ? a :b;` stays unchanged; the output is now `var x = cond ? a : b;`. FMT-SPACE-018 and answer 4B demand the space.
5. **`SpacingPolicyTests.NoRuleAppliesBetweenTypeAndIdentifier`** (`src/Reihitsu.Formatter.Test/Unit/HorizontalSpacing/SpacingPolicyTests.cs:109`). It asserts that no rule decides `Foo` and `bar` in `Foo bar=null;`. FMT-SPACE-021 (answer 7C) decides it: one space between a variable declaration's type and its name. The policy now returns 1.
6. **`IndentationTests.ImplicitStackAllocInitializerAlignsToStackAllocKeyword`** (`src/Reihitsu.Formatter.Test/Regression/Indentation/IndentationTests.cs:3270`). It expects `Span<int> p = stackalloc[]`; the output is now `stackalloc []`. FMT-SPACE-024 and answer 10B demand this (change doc entry 9: `stackalloc [] { 1 }`). The initializer's column is unchanged (22).
7. **`RelationalPatternSpacingTests.DocumentationCommentInFrontOfTheOperandKeepsItsPlace`** (new, `src/Reihitsu.Formatter.Test/Regression/Spacing/RelationalPatternSpacingTests.cs:220`):
   - Input: `return x is > /** a */5;`
   - Expected: `return x is > /** a */ 5;`
   - Actual: `return x is >`, then a blank line, then `/** a */5;` on its own line.

   `scripts/trace.sh` shows that `DocumentationCommentFormattingPhase` moves the `/** a */` onto a line of its own. It does this on `origin/main` too (traced in a `main` worktree), and also for the input `> /** a */ 5`. So the expected output is not the formatter's output on any commit. The spacing rule itself puts the space there: `SameLineGapRulePhaseTests` covers the same gap at phase level and passes. `04` lists the relational operator among the positions where the comment "stays on the line"; that claim does not hold. The test belongs at phase level, like the other doc-comment cases.
8. **Spec gap, FMT-SPACE-014.** Its text only demands that whitespace behind a right-carried `/** */` is *removed* when no space is allowed. The new comment tests also demand that one space is *inserted* there when the rule asks for one: `case (1, 2) /** a */when b:` gives `/** a */ when`, `int? /** a */b` gives `/** a */ b`, `M(b /** x */ :b)` gives `/** x */: b`, and `y =>/** a */y` gives `/** a */ y`. I implemented the general form: the rule's decision applies to the whitespace behind the comment, as FMT-SPACE-013 does for a left-carried comment. FMT-SPACE-014's second sentence does not say this yet. This is visible output only for a `/** */` written between two tokens. Every analyzer and CLI test passes with it.
9. **Adjacent fix inside the change (a few lines, `HorizontalSpacingRewriter.VisitToken`).** On `main`, when `TrimGapBeforeToken` rewrote a token, `GetNextToken()` and the rules then ran on the detached token. A detached token has no next token, so the gap behind it was left undecided in that pass: `[return /** x */ :Obsolete]` kept `:Obsolete`. The new comment tests need that gap decided, so the rewriter now evaluates the original token. No other output changed in the full test runs.

Scope: the production paths are the ones the handovers name (line-break joiners, spacing rules, the two transforms), plus the spacing rewriter for item 9. There is no new public member: the new helpers on the `internal` `LineBreakTriviaUtilities` are `public static` like its existing members.

## Architecture findings

_None._ The one shared decision this run touches, the join separator, now has a single owner: `TokenSeparationUtilities`, reached only through `LineBreakTriviaUtilities.CreateJoinSeparator`. The private per-kind copy in `ListElementInteriorJoinRewriter` is deleted. The attribute-list merge's spaces are the exception FMT-BREAK-031 records (thoenissen/Reihitsu#1492).

## Follow-up candidates

- FMT-SPACE-014 wording for the inserted space behind a right-carried comment (item 8): a spec-author amendment in this run, or a follow-up.
