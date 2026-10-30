Role: test implementer
Commit: 0cabe33
Verdict: tests written

This is the second test-implementer pass. It works from `06-implementer.md`, `07-spec-author.md`, and the decisions `squad/decisions/2026-10-30-line-start-fact-{end-of-file-region,one-contributor-test,implementer-findings,amended-change-doc}.md`. The prompt carried no intended fix or hoped-for verdict.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Regression/Spacing/StrandedDocumentationTokenSpacingTests.KeepsGapBetweenBlockCommentAndTerminatorBelowStrayDocumentationComment` (was `RemovesGapBetween…`, disputed 1) | FMT-SPACE-001 | `AssertRuleResult` | green |
| `Regression/FullPipeline/StrandedDocumentationCommentTextTests.TwoLineRemarksBehindArgumentKeepsItsText` (disputed 2; `)` 8 → 10) | FMT-DOC-003, FMT-INDENT-002 | `AssertRuleResult` | green |
| `Unit/Pipeline/PhaseStabilityCheckTests.CheckCatchesLineBreakPhaseStrippingIndentationOfClosingAngleBracketBehindOmittedTypeArgument` (disputed 3) | — | — | deleted, see below |
| `Regression/Indentation/ZeroWidthTokenLineStartTests.ClosingAngleBracketBehindOmittedTypeArgumentAndCommentTakesBlockColumn` (amended SHARED-BLANK-008 example; was `ClosingAngleBracketBehindOmittedTypeArgumentTakesBlockColumn`) | SHARED-BLANK-008, FMT-INDENT-015 | `AssertRuleResult` | green |
| `…ZeroWidthTokenLineStartTests.AlignedClosingAngleBracketBehindOmittedTypeArgumentAndCommentTakesBlockColumn` (amended FMT-INDENT-015 example, `>` 64 → 8) | FMT-INDENT-015 | `AssertRuleResult` | green |
| `…ZeroWidthTokenLineStartTests.ClosingAngleBracketBehindOmittedTypeArgumentAndDirectiveTakesBlockColumn` (a directive keeps the list wrapped) | FMT-INDENT-015 | `AssertRuleResult` | green |
| `…ZeroWidthTokenLineStartTests.WrappedUnboundGenericIsJoined` (the issue's example 2, as amended; was `ClosingAngleBracketOfUnboundGenericInColumnZeroTakesBlockColumn`) | FMT-BREAK-014 | `AssertRuleResult` | red |
| `…ZeroWidthTokenLineStartTests.WrappedListWithSeparatorOnSecondLineIsJoined`, `…WrappedListOfSingleOmittedTypeArgumentIsJoined`, `…WrappedListOfThreeOmittedTypeArgumentsIsJoined`, `…WrappedListWithSeparatorAndClosingAngleBracketOnSecondLineIsJoined` (the change doc's `Func<` / `,,>` case), `…WrappedListWithOverIndentedClosingAngleBracketIsJoined` (each was a `…TakesBlockColumn` test of the same input) | FMT-BREAK-014 | `AssertRuleResult` | red |
| `Regression/LineBreaks/WrappedAngleBracketListTests.UnboundGenericWithWrappedOmittedTypeArgumentsIsJoined` (was `UnboundGenericWithZeroWidthAnchorIsNotAlignedToADegenerateColumn`; renamed because the issue's *Done when* item is superseded by answer 2B) | FMT-BREAK-014 | `AssertRuleResult` | red |
| `Unit/Indentation/LayoutContributorOwnershipTests.AtMostOneAlignmentContributorPlacesEachLineOfNestedConstructs` (12 rows, each nesting an aligned construct inside another) | — (issue *Done when*) | `LayoutComputer.Compute`, `LayoutModel.GetAlignmentContributors` | red in 2 rows, see below |
| `…LayoutContributorOwnershipTests.AtMostOneAlignmentContributorPlacesEachLineOfShippedSources` (every `.cs` under `src/` of Formatter, Core, Analyzer, Analyzer.CodeFixes, Cli) | — (issue *Done when*) | same | red, see below |
| `Unit/Pipeline/TokenContainmentUtilitiesTests.ContainsTokenReturnsTrueForOwnedToken`, `…ContainsTokenReturnsFalseForDefaultToken` (moved from `TokenLocatorTests`) | — | direct call | green |
| `Unit/Pipeline/TriviaListUtilitiesTests.RemoveTrailingWhitespaceRemovesTrailingWhitespaceAndKeepsCommentSpacing` (moved from `LineBreakTriviaUtilitiesTests.StripTrailingWhitespace…`) | — | direct call | green |

The omitted-array-size rows, the own-line-comment rows, and the trailing-comment row of `ZeroWidthTokenLineStartTests` stand unchanged. A comment keeps those lists wrapped, and no requirement joins a rank specifier (`07-spec-author.md`).

## The disputed tests, re-derived

1. **`RemovesGapBetweenBlockCommentAndTerminatorBelowStrayDocumentationComment`: the implementer is right.**
   - The input is `return 1 /// stray` / `/* b */ ;`.
   - The gap between `1` and `;` holds the line break that ends the `///`, so FMT-SPACE-001 as changed leaves the whole gap unchanged, including `/* b */ ;`.
   - The `//` counterpart keeps `/* b */ ;` today, and the change doc makes that the target. FMT-SPACE-014 covers a delimited comment written between two tokens on one line, not a gap across a line break.
   - The test now expects the input unchanged and is renamed.
2. **`TwoLineRemarksBehindArgumentKeepsItsText`: the implementer is right.** I missed this one in the first pass.
   - The comment's text, including its continuation line in column 8, is kept (FMT-DOC-003).
   - `)` is the first token of the line behind it and is aligned with the first argument at column 10, as behind `// x` (FMT-INDENT-002).
3. **`PhaseStabilityCheckTests.CheckCatchesLineBreakPhaseStrippingIndentationOfClosingAngleBracketBehindOmittedTypeArgument`: deleted, because no input exists.**
   - The test needs a final formatter output in which the line-break phase alone strips the indentation of a closing angle bracket, which the indentation phase then writes back.
   - Under answer 2B such a list is either joined (no `>` line remains) or kept wrapped by a comment or a directive.
   - I traced these shapes on cb59e98 with `scripts/trace.sh --passes 2` or `--passes 3`, starting from their formatted output, and none of them changed in any phase:
     - `Dictionary<, // c` / `>`;
     - `Dictionary<,` / `#if X` / `#endif` / `>`;
     - `new List<int // c` / `>()`;
     - `List<int` / own-line `// c` / `>()`;
     - `int[,` / `]`;
     - `M<int, // c` / `string>()`;
     - `class C<T, // c` / `U>`;
     - `[System.Obsolete("a") // c` / `]`;
     - `Dictionary<` / own-line `// c` / `,>`.
   - What the check catches, a line-break-phase strip that the indentation phase restores, stays covered by `CheckCatchesLineBreakPhaseStrippingIndentationOfParenthesizedLambdaArgument` in the same class. That test asserts the same check, phase, and cause on an input whose reversal still exists.

## One alignment contributor per line: red today

On cb59e98, `LayoutModel.GetAlignmentContributors` reports a second contributor on 172 lines in 56 shipped source files, plus two of my nested fixtures. Every overlap pairs `ArgumentAlignmentContributor` with a contributor of a construct inside the argument:

| Pair | Lines |
|---|---|
| `ArgumentAlignmentContributor`, `BinaryExpressionContributor` | 107 |
| `ArgumentAlignmentContributor`, `ConditionalExpressionContributor` | 44 |
| `ArgumentAlignmentContributor`, `MethodChainAlignmentContributor` | 21 |
| `ArgumentAlignmentContributor`, `AngleBracketListAlignmentContributor` | 4 |

Reproducing fixtures, both rows of `AtMostOneAlignmentContributorPlacesEachLineOfNestedConstructs`:

- `return Run(a` / `? b` / `: c,` / `b + c` / `+ 1);`. Lines 6 and 7 are placed by Argument and Conditional, and line 9 by Argument and Binary.
- `return Run(items.Where(x => x > 0)` / `.Select(x => x),` / `2);`. Line 8 is placed by Argument and MethodChain.

Examples from the shipped sources: `src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs` lines 445 and 598–601, and `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3103UseShorthandForNullableTypesCodeFixProvider.cs` lines 86–91.

The issue states that this property "holds today". With the contributor record the implementer added, it does not. The decision (`…-one-contributor-test.md`, option A) routes this back to the implementer. If the property is meant more narrowly, for example the contributor that wrote the line last, or only contributors of unrelated nodes, that is a question for the maintainer, not a test change. The other ten nested rows pass:

- lambda block body in an argument;
- object initializer in an argument;
- switch expression in a lambda;
- collection expression in an argument;
- recursive pattern with `&&`;
- anonymous object with a wrapped chain;
- base list and constraints;
- wrapped attribute arguments;
- anonymous object with a wrapped argument list inside a chain;
- string concatenation with a wrapped argument.

## Not covered, and why

- **The joined forms with CRLF and a second pass.** `AssertRuleResult` covers both line endings, the second pass, and the phase-level check, so no separate test is needed.
- **A comment behind the closing `>` of a wrapped omitted list** (FMT-BREAK-014: "a comment behind the closing delimiter does not block it"). The text-bearing lists already cover it under FMT-BREAK-014. The omitted list reaches the same join code, but I have no static proof of that and did not add a row. Candidate if the auditor wants it.

## Tests that reference types answer 1B or the *Done when* moves

- **`SpacingPolicy`.** `src/Reihitsu.Formatter.Test/Unit/HorizontalSpacing/SpacingPolicyTests.cs` has `using Reihitsu.Formatter.Pipeline.HorizontalSpacing.Utilities;`. Moving `SpacingPolicy` out of that namespace breaks the using.
  - `SpacingPolicy` depends on `Reihitsu.Formatter.Pipeline.HorizontalSpacing.Rules`.
  - `src/Reihitsu.ArchitectureTests/Formatter/NamespaceTests.cs` (`SpacingRulesShouldBeInRewriterNamespace`, `OtherTypesShouldNotBeInSpacingRuleNamespace`) requires every `ISpacingRule` type, and only those, in `Pipeline.HorizontalSpacing.Rules`. Moving the rules along with the policy breaks those two tests.
- **The unary-operator guard.** `UnaryOperatorSpacingUtilities` already lives in `Reihitsu.Core` (`src/Reihitsu.Core/UnaryOperatorSpacingUtilities.cs`). It is tested by `src/Reihitsu.Core.Test/UnaryOperatorSpacingUtilitiesTests.cs` and used by RH6018–RH6020. It is not in a phase folder, and moving it to `Pipeline/Core` would cut the analyzers off from it.
- **`LineStartIndex`.** No test names it. The tests under `Unit/Indentation/` and `Integration/` that use `Reihitsu.Formatter.Pipeline.Indentation.Utilities` (`LayoutModelTests`, `LayoutComputerTests`, `TokenLayoutTests`, `LayoutContributorOwnershipTests`, `IndentationPhaseIntegrationTests`, `PhaseCombinationTests`) need that namespace for `LayoutComputer`, `LayoutModel`, and `TokenLayout`. Moving only `LineStartIndex` leaves them intact.
- **The forwarders.** `TokenLocatorTests` and `LineBreakTriviaUtilitiesTests` no longer call `TokenLocator.ContainsToken` or `LineBreakTriviaUtilities.StripTrailingWhitespace`. No test references either one, so both can be deleted.

## Whole run (0cabe33, production code at cb59e98)

- Formatter: `Failed: 10, Passed: 3244`. The ten are the seven join tests and the three contributor-test results above.
- Architecture: `Passed: 89`.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~ZeroWidthTokenLineStartTests|FullyQualifiedName~WrappedAngleBracketListTests|FullyQualifiedName~StrandedDocumentationTokenSpacingTests|FullyQualifiedName~StrandedDocumentationCommentTextTests|FullyQualifiedName~PhaseStabilityCheckTests|FullyQualifiedName~LayoutContributorOwnershipTests|FullyQualifiedName~TokenContainmentUtilitiesTests|FullyQualifiedName~TriviaListUtilitiesTests|FullyQualifiedName~TokenLocatorTests|FullyQualifiedName~LineBreakTriviaUtilitiesTests"
scripts/test.sh --project architecture
```
