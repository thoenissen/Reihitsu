Role: implementer
Commit: a895bcf
Verdict: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing was ignored.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/ReihitsuFormatter.cs` | `FormatNode` and `FormatNodeInDocumentAsync` pass a detached copy of their node or target to the pipeline. All three entry points take the depth from `ComputeDepth`. `FormatNodeInDocumentAsync` also supplies `RootConstruct`. Remarks updated; the context copy is commented as already detached. | FMT-PIPE-012, FMT-PIPE-023, FMT-PIPE-024, FMT-CLEAN-008 |
| `src/Reihitsu.Formatter/Utilities/ReihitsuFormatterHelpers.cs` | New `ComputeDepth` (`GetChildIndentLevel` over the node and its ancestors), `CaptureRootConstruct` (calls each reader's own predicate on the attached target), and `Detach` (`CSharpSyntaxTree.Create` with the tree's options). | FMT-PIPE-012, FMT-PIPE-023, FMT-PIPE-024 |
| `src/Reihitsu.Formatter/Data/FormattingContext.cs` | New `RootConstruct` property and constructor parameter. Docs of `BaseIndentLevel` and `RootPrecedingToken` updated. | FMT-PIPE-024 |
| `src/Reihitsu.Formatter/Data/Enumerations/RootConstruct.cs` (new) | `[Flags]` enum with the seven construct facts. | FMT-PIPE-024 |
| `src/Reihitsu.Formatter/Extensions/RootConstructExtensions.cs` (new) | `IsRootIn(facts, node, flag)`: true when the node has no parent and the flag is set. | FMT-PIPE-024 |
| `src/Reihitsu.Formatter/Data/TriviaFact.cs` (new) | Kind and text of one trivia. `ToTrivia()` builds a trivia that belongs to no tree. | FMT-PIPE-024 |
| `src/Reihitsu.Formatter/Data/PrecedingTokenFacts.cs` | Holds `ImmutableArray<TriviaFact>` plus `EndsLine`/`HasTrailingComment` flags captured at `From`. `TrailingTrivia` is now a computed list that belongs to no tree. | FMT-PIPE-024 |
| `src/Reihitsu.Formatter/Data/RootListPosition.cs` | Holds the previous statement's kind and the kind of its last token, `IsBehindSectionEndingInBreak`, and `IsTerminalDirectSwitchSectionBreak`. It holds no nodes. | FMT-PIPE-024 |
| `Pipeline/BlankLines/Rewriter/BlankLineStatementSpacingRewriter.cs`, `BlankLineBreakSpacingRewriter.cs` | The statement-spacing and break-spacing decisions take kinds instead of the previous statement. `EndsInBreak` is now `internal` so that the capture reuses it. | FMT-PIPE-024 |
| `Pipeline/LineBreaks/Rewriter/LineBreakContainedBlockRewriter.cs` | `IsElseIfBranch(node, facts)` | `else if` fact |
| `Pipeline/LineBreaks/Rewriter/ChainLineBreakRewriter.cs` | `IsOutermostChainNode(original, facts)` wraps `FluentChain.IsOutermostChainNode` | Chain continuation |
| `Pipeline/LineBreaks/Rewriter/AttributeTargetFormattingRewriter.cs` | `StandsInOneLineProperty(accessor or list)`. The reader takes the fact at the root only while the root holds no line break (see *Disputed*, item 2). | One-line property |
| `Pipeline/Core/Utilities/ListElementInteriorUtilities.cs`, `Pipeline/LineBreaks/Rewriter/ListElementInteriorJoinRewriter.cs`, `LineBreakPhase.cs` | `LiesInListElement` is the capture. The join rewriter now takes the context, and when the root contains no list element and the fact is set, it treats the root as the element. | List element |
| `Pipeline/LineBreaks/Rewriter/LineBreakListRewriter.cs` | `IsElementAccessArgumentList(original, facts)` | Element-access arguments |
| `Pipeline/LineBreaks/Rewriter/LineBreakInitializerRewriter.cs` | `IsIntroducedByCaseLabel(node, facts)` and `IsAssignedCollectionInitializer(node, facts)` | Case-label pattern, assigned collection initializer |
| `TabReplacementPhase.cs`, `LineEndingNormalizationPhase.cs`, `IndentationPhase.cs`, `DocumentationCommentFormattingPhase.cs` | Comments only: roots are detached already. | FMT-PIPE-023 |
| `src/Reihitsu.Core/SyntaxIndentationUtilities.cs` | Comment only: `IsIndentingAncestor` no longer names formatter callers. The attribute-placement code fixes are the only remaining consumer. | FMT-PIPE-012 |

Twenty production files changed, three of them new. The handovers named no production path list, so I measured scope against the change doc. Every file is a reader the change doc or `03-spec-author.md` names, a fact type, or an entry point.

## Policy owners

- `rg -n "Parent is ElseClauseSyntax|IsOutermostChainNode|IsSingleLineExcludingAttributeLists|IsListElement\(|ImplicitElementAccessSyntax|CasePatternSwitchLabelSyntax|Parent is AssignmentExpressionSyntax|GetChildIndentLevel\(|EndsInBreak\(" src/Reihitsu.Formatter src/Reihitsu.Core`
  - Every flag has one predicate, in the rewriter that reads it. `CaptureRootConstruct` calls that same predicate with `RootConstruct.None` on the attached target.
  - Chain continuation is captured as `FluentChain.GetOutermostChainNode(target) != target`, the same `ContinuesChain` owner.
  - `LiesInListElement` reuses `IsListElement` and `IsOwningConstruct`.
  - Depth reuses `SyntaxIndentationUtilities.GetChildIndentLevel`, the indentation phase's owner. Its loop has the shape of `ComputeStatementIndentLevel`, which only accepts a `StatementSyntax`. I did not widen that public Core method.
  - Pre-existing second statement, not introduced here: `Reihitsu.Core.AttributeTargetUtilities.IsAttributeListOnSingleLinePropertyAccessor` restates the formatter's one-line-property rule for the analyzers. Formatter code does not call it. See *Follow-up candidates*.
- `rg -n "ComputeBaseIndentLevel" src --type cs`: no formatter production caller remains.
  - `ReihitsuFormatterHelpers.ComputeBaseIndentLevel` stays, because `ReihitsuFormatterHelpersTests` (five tests) call it, and I may not remove tests. It is now dead production code. The lead decides whether the test implementer removes those tests and the wrapper.
  - The Core method stays for `TargetAttributePlacementCodeFixProviderBase`, as the change doc says.

## Tests run

```shell
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~ReihitsuFormatterDetachedRootTests|FullyQualifiedName~FormatNodeWithPreviousTokenOutsideTargetPreservesTrailingDocumentation"
```

- Before the change: Failed 24, Passed 8, Total 32, as handed over.
- After: Passed 32, Total 32.

Whole projects at `a895bcf`, Debug: formatter Passed 3433/3433, architecture 89/89, analyzer 6312/6312, core 535/535, cli 251/251. The Debug build has 0 warnings and 0 errors. `scripts/format.sh` over every changed file changed nothing.

One architecture failure appeared during the work and is fixed. `EnumerationsTests.OtherTypesShouldNotBeInEnumerationsNamespace` rejected the extension class in `Data/Enumerations`, so it moved to `Reihitsu.Formatter.Extensions`.

### Corpus probe (not committed)

I compared this tree with `origin/main` (`e7853b6`), following the skeptic's method.

- **Corpus and runs:** every string and raw-string literal containing a type declaration in `Reihitsu.Formatter.Test` and `Reihitsu.Analyzer.Test`, skipping inputs with syntax errors. Up to 25 candidate nodes per input went through `FormatNodeInDocumentAsync`, 66 351 runs in total.
- **Result:** 88 outputs differ. The first run showed 96; the 8 extra were the one-line-property class, fixed below.
- **Document-formatting check:** where noted below, I formatted the input through `scripts/format.sh`.

| Class | Count | Example target | Main | Here | Document formatting |
|---|---|---|---|---|---|
| Zero-offset initializer comment (accepted, `…-zero-offset-depth.md`) | 6 | complex element initializer behind `/* a⏎b */` | comment line in column 12 | column 8 | — |
| Assigned collection initializer whose root an earlier phase changed | 2 | `Values = {⏎1,⏎2⏎/* Keep close. */ }` as target | blank line in front of `/* Keep close. */ }` removed, or none added | blank line kept, or one added | same as here in both inputs |
| Inner chain part whose root an earlier phase (blank-line phase or initializer layout) changed | 80 | `a⏎// keep wrapped⏎.Prop.Foo()⏎.Bar().Baz()` with `a…Bar` or `a…Baz` as target; `a.B()?.C().D = 5` assignment targets | chain laid out as a chain of its own | layout kept, because the root continues a chain | lays out the whole outer chain, a third result |

## Disputed tests or scope

No test disputed. Three findings for the lead are about the spec and the change doc, not the tests.

1. **The change doc's "node-in-document outputs keep their output" does not hold for 82 corpus outputs beyond the zero-offset class.**
   - The flags are captured before the pipeline runs (FMT-PIPE-024). On `main`, an attached root that an earlier phase changed is already detached when the line-break phase reads it, so it decides as a construct of its own.
   - With the flag, the decision no longer depends on that, which is what FMT-PIPE-023 requires. But it changes `main`'s output wherever the blank-line phase or an earlier line-break rewriter changed the root.
   - The change doc claims unchanged outputs only "while the target holds nothing an earlier phase changes", but its *What* says "the node-in-document entry points keep their output, with one exception". That sentence is wider than what holds.
   - These outputs are end-user visible only through a code fix that formats an inner chain part or an assigned collection initializer as its target. No analyzer test changed.
   - The lead should decide whether the change doc needs the class listed. Under the maintainer's rule, it is the lead's call whether this counts as end-user-visible output to escalate.
2. **One-line property: I narrowed the fact's reading.** The spec says the flag is whether the property "stands on one line", captured from the document.
   - Read literally, a root that an earlier rewrite expands keeps its accessor attributes on one line: `int Q { [A] /* c */ [B] get; set; }` with the accessor list as target gave `[A] /* c */ [B] get;` on an expanded property.
   - Document formatting splits them, because it measures the property after the expansion. `main` does the same.
   - The reader now also requires that the root holds no line break in its span or leading trivia. The captured flag is unchanged.
   - With that, all 8 corpus outputs of that class match `main` and document formatting, and the handed-over accessor tests still pass.
   - This reading is narrower than FMT-PIPE-024's row says. The spec author may want the row to state it.
3. **FMT-PIPE-024 says every phase answers what stands in front of the root's first token from the preceding-token fact. I did not change the line-break and spacing readers that still use a root-boundary approximation.** Examples:
   - `LineBreakTriviaUtilities.HasLeadingEndOfLine` uses the token's own leading trivia when no previous token exists;
   - `TokenLocator.TryGetPreviousToken`;
   - `HorizontalSpacingRewriter.TrimGapBeforeToken`.

   `03-spec-author.md` lists them as already served by the fact. They are not: they call `GetPreviousToken()` and get none at the root, as they did on `main` for every root an earlier phase changed. Changing them was not needed for any test, and the corpus shows no class traced to them. It would add about eight production paths. One case where the approximation could differ from the document: a target that starts its line in column 0, with no leading whitespace. The lead should decide whether to narrow the spec sentence or to have them read the fact in a follow-up.

## Architecture findings

**Chain outermost-ness has two owners for an inner chain-part root.** The line-break phase now reads the chain-continuation fact. `MethodChainAlignmentContributor` (indentation phase) calls `FluentChain.Create(root)`, which treats the parentless root as outermost and aligns it as a chain.

Example: the target is `new List<int> { 1, 2, 3 }⏎.Where(…)⏎.Select(…)⏎.ToList` in front of `()`. The three outputs are:

```csharp
// branch
                     }
                                     .Where(x => x > 0)
                    .Select(x => x * 2)

// main
                     }.Where(x => x > 0)
                    .Select(x => x * 2)

// document formatting
                     }.Where(x => x > 0)
                      .Select(x => x * 2)
```

- On `main` the same mismatch already exists for an inner chain-part target that no earlier phase changes. The flag extends it to every inner chain-part target.
- FMT-PIPE-024's reader list gives the indentation phase only the depth, the column, and the preceding token. Its general sentence ("a phase MUST read a fact where it would read … the ancestor itself") would also cover the contributor.
- I made no change there: it would change outputs the change doc says stay, and it is a second reader of the same decision.

## Follow-up candidates

- `ReihitsuFormatterHelpers.ComputeBaseIndentLevel` and its five tests in `ReihitsuFormatterHelpersTests` have no production caller any more.
- `Reihitsu.Core.AttributeTargetUtilities.IsAttributeListOnSingleLinePropertyAccessor` is a second statement of the one-line-property rule the formatter owns in `AttributeTargetFormattingRewriter` (pre-existing).
- `PrecedingTokenFacts.From` now copies the trailing trivia into an array for every token whose preceding token a blank-line or comment-alignment decision resolves. The formatter project's run time did not change noticeably (26–31 s).
