Role: auditor
Commit: 3bd7303d253ffd45208005247cbe3220d51cabee
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing was ignored.

## Scope

- Full audit of the run (first audit). Head `3bd7303`. Base `origin/main` `c24bc1d`, equal to `git ls-remote origin refs/heads/main`, and an ancestor of the head. The diff is what the handovers describe (20 production files, 3 new; the tests of `04`/`08`; the specs, the two change docs, `AGENTS.md`, `squad/`). `scripts/lane-check.sh behavior-change`: confirmed.
- Evidence from scratch copies of head and `origin/main` (`git archive`, built in Release), all outside the repository:
  - a probe that calls `FormatNodeInDocumentAsync` and `FormatNode` on both builds;
  - `scripts/apply-fix.sh` on both trees, for RH6023, RH6024, RH5111, RH5204, RH5405, and RH5417.
- The issues are thoroughly checked against the code: thoenissen/Reihitsu#1489 (both examples, Done-when) and thoenissen/Reihitsu#1527.

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/ChainLineBreakRewriter.cs` `IsOutermostChainNode`; `src/Reihitsu.Core/FluentChain.cs` `ContinuesChain` (assignment arm, `assignment.Parent is ConditionalAccessExpressionSyntax`) | FC-001, FC-016, FC-030 | – | PR-introduced | A code fix's output changes, and the change is not listed. The target is an assignment that is the `WhenNotNull` of a conditional access. The chain decision for its `Left` is made at a node *inside* the root, and it reads the root's parent. The `ChainContinuation` fact is read only at the root (`IsRootIn` requires `Parent == null`), so it never reaches that decision. With the root now always detached, `Left` counts as an outermost chain and gets re-laid out. On `main`, that happened only when the target held a tab. Reached by RH6023, which formats exactly that assignment (`OperatorSpacingCodeFixProviderBase`). Input `a?.B⏎  .C=5;`: on `main` the fix writes `a?.B⏎  .C = 5;`. On head it writes `a?.B.C = 5;`, so an operator-spacing fix now joins a line that whole-document formatting keeps (`a?.B⏎ .C = 5;`). This contradicts the change doc ("Every other node-in-document output stays as it is"; *Visible output changes* lists only RH5204 and RH5405). It also breaks FMT-PIPE-023 ("what a phase needs from there MUST come from the facts"), and it is not a maintainer answer. **Failing members** (RH6023 output, under LF and CRLF): a wrapped `Left` behind a member binding (`a?.B⏎.C=5`); behind an element binding (`a?[0]⏎.C=5` → `a?[0].C = 5`); a nested conditional access (`a?.B?.B⏎.C=5`); a compound assignment (`+=`); and a longer left chain (`a?.B()⏎.D⏎.C =5` → `a?.B().D⏎      .C = 5`, re-laid out). **Members that hold:** `Left` on one line (`a?.B.C=5`), and `Left` that is a bare binding (`a?.B=a`). Through RH5111 the output is unchanged against `main`, because `LineBreakAssignmentRewriter` changes the root before the chain rewriter on `main` as well. No other reader of a node inside the root reads past the root: checked with `rg` for grandparent and ancestor reads in every phase before the indentation phase and in the Core helpers they call. See *Architecture findings*. | The chain decision for the root's direct `Left` child should take its answer from the fact when the root is an assignment without a parent, or `FluentChain` should take the root's continuation as an input, so that one owner answers. Add an RH6023 code-fix regression test (LF and CRLF) on `a?.B⏎  .C=5;` with `main`'s output as the expected value. Add a FMT-PIPE-024 formatter test with a tab and with a space. Owners: implementer and test implementer. The change doc needs no change once `main`'s output is restored. |
| 2 | medium | `sdd/specs/formatter/pipeline.md` header and `sdd/specs/README.md` (code owners) | — (repository convention, `sdd/README.md` *Code owners*) | – | same requirement | FMT-PIPE-024 specifies the facts. The run added `PrecedingTokenFacts` and `RootListPosition` as code owners, but the new fact types `RootConstruct` (the seven construct flags) and `TriviaFact` (the kind and text of a trivia, with `ToTrivia`), and their reader `RootConstructExtensions.IsRootIn`, own no spec. That breaks the convention that a type whose behavior a spec specifies belongs to that spec. It also misses the issue's *Proposed Behavior* ("their types get a spec owner"). Text-only, cannot reach compiled behavior. | Add `RootConstruct`, `TriviaFact`, and `RootConstructExtensions` to the code owners of `formatter/pipeline.md` and to the index. Owner: spec author. |
| 3 | low | `src/Reihitsu.Formatter/Pipeline/BlankLines/Rewriter/BlankLineTriviaBoundaryRewriter.cs:236-238`; `src/Reihitsu.Formatter/Pipeline/BlankLines/Rewriter/BlankLineCollapser.cs:143-146` | FC-031 | – | PR-introduced | These remarks still explain the facts with "a formatting root that an earlier phase/subphase detached", and say the detached root "is measured exactly like an attached one". Since FMT-PIPE-023, every node-level root is detached before the first phase, so no attached case remains. The summaries describe the replaced mechanism. Comment-only. | Reword: the root has no parent (FMT-PIPE-023), so its first token has no previous token in its tree. Owner: implementer (comment-only). |
| 4 | low | `src/Reihitsu.Formatter/Utilities/ReihitsuFormatterHelpers.cs` `ComputeDepth` | FC-015 (logic, not policy) | – | PR-introduced | The loop repeats `SyntaxIndentationUtilities.ComputeStatementIndentLevel`, which differs only in its `StatementSyntax` parameter. The per-step policy (`GetChildIndentLevel`) has one owner, so nothing diverges today. But the two walks can drift, and the summary even names the Core method it copies. | Widen `ComputeStatementIndentLevel` to `SyntaxNode` (or add a `SyntaxNode` overload) and call it. Owner: implementer. |

Undecided visible output: finding 1 is a code-fix result (RH6023) that changed without a requirement or a maintainer answer demanding it. The suggested repair restores `main`'s output, so no output question arises. If the lead instead keeps the new output, it becomes an output question.

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| `else if` | capture: `target.Parent is ElseClauseSyntax` (`IsElseIfBranch(target, None)`); reader: `node.Parent is ElseClauseSyntax` or root with flag | `LineBreakContainedBlockRewriter.Visit` on the original node | same predicate, same span | holds (RH5405: line comment, block comment, comment inside the condition, three-branch chain, `else` body behind it; all keep the comment) |
| Chain continuation | capture: `GetOutermostChainNode(target) != target` ≡ `ContinuesChain(target.Parent, target)`; reader: the flag at the root only | `FluentChain.IsOutermostChainNode` for every chain node, including the root's direct children | agree at the root; disagree for the `Left` of a root assignment under a conditional access, whose decision reads `root.Parent` | **finding 1** |
| One-line property | capture: `IsSingleLineExcludingAttributeLists` over the property from the first token after its attribute lists to `Span.End`; reader: the flag, the root's `Span` on one line, and no line break in its leading trivia | the same predicate on the current tree in document formatting | The parts outside the root are fixed at capture. The reader omits the trailing trivia of the root's last token, which the entry point replaces and which no rewriter changes without a next token. No code fix passes an accessor or an accessor list (RH5417 and RH7107 pass the member; RH5204 reaches an accessor list only in a multi-line property, where the flag is false) | holds for reachable inputs |
| List element | capture: `LiesInListElement`, walking from the target itself (element → true; `IsOwningConstruct(current, default)` → false); reader: the innermost element inside the root, else the root when the flag is set, then `FindOwningConstruct(token, root)`, which leaves the root out | document formatting: `FindOwningConstruct(token, E)` over the nodes inside the root, the root, and its ancestors up to E | Equal. The token-dependent owner, a collection expression's `[`, is the root's first token and is never joined on either path (`element.GetFirstToken() == token` vs `root.FullSpan` check) | holds (RH6024 on five argument, lambda, collection, and parenthesized shapes: head = `main`) |
| Element-access arguments | `originalNode.Parent is ElementAccessExpressionSyntax or ImplicitElementAccessSyntax` | `LineBreakListRewriter.CanSafelyCollapseBracketedArguments` | same predicate | holds |
| Case-label pattern | capture: walk from the target through binary and unary patterns; reader: walk from the nested pattern, which stops at the root (`Parent == null`) and then takes the flag | the same walk in the document | same walk; a parenthesized pattern stops it on both paths | holds |
| Assigned collection initializer | `node.Parent is AssignmentExpressionSyntax`, collection kind | `LineBreakInitializerRewriter.VisitInitializerExpression` | same predicate | holds |
| `IsRootIn` | `node.Parent == null` | "is the formatting root" | Every reader passes the node as it entered the rewriter. The only re-visit of a parentless copy (`LineBreakListRewriter.IsMultiLineAfterAttributeFormatting`) visits list elements, which never hold an accessor root | holds |
| Depth | `ComputeDepth`: the sum of `GetChildIndentLevel` from the node to the tree root | `LayoutComputer`: the same per-step function, top-down | additive, so the order does not matter | holds (see finding 4 for the duplicated loop) |
| Statement-kind spacing | `PreviousStatementKind` / `PreviousStatementLastTokenKind` | the former `previous is …` and `previous.GetLastToken().IsKind(CloseBrace)` | `YieldStatementSyntax` ≡ `YieldReturnStatement`/`YieldBreakStatement`; the other types map one to one | holds |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Chain outermost-ness | `FluentChain.IsOutermostChainNode`/`ContinuesChain`/`GetOutermostChainNode` (Core); `ChainLineBreakRewriter.IsOutermostChainNode(original, facts)`; `ReihitsuFormatterHelpers.CaptureRootConstruct`; `MethodChainAlignmentContributor` (`FluentChain.Create(root)`) | more than one owner; finding 1, *Architecture findings* |
| One-line property | `AttributeTargetFormattingRewriter.StandsInOneLineProperty`; `Reihitsu.Core.AttributeTargetUtilities.IsAttributeListOnSingleLinePropertyAccessor` (pre-existing, analyzer side); both delegate to `SyntaxNodeUtilities.IsSingleLineExcludingAttributeLists` | pre-existing second statement, dropped under decision 9; not reached by a node-level root |
| List element and owning construct | `ListElementInteriorUtilities.IsListElement`/`IsOwningConstruct`/`FindOwningConstruct`/`LiesInListElement` | one owner |
| `else if`, element access, case label, assigned collection initializer | one `internal static` predicate each, in the reading rewriter; the capture calls it with `RootConstruct.None` | one owner each |
| Node-level depth | `ReihitsuFormatterHelpers.ComputeDepth`; `SyntaxIndentationUtilities.ComputeStatementIndentLevel`; `LayoutComputer` (all on `GetChildIndentLevel`); `SyntaxIndentationUtilities.ComputeBaseIndentLevel` kept for `TargetAttributePlacementCodeFixProviderBase` (out of scope, change doc) | policy has one owner; the loop is duplicated (finding 4) |
| Section ends in `break` | `BlankLineBreakSpacingRewriter.EndsInBreak`, reused by `RootListPosition.From` | one owner |
| Preceding-token line end and comment | `PrecedingTokenFacts.From` (`SyntaxTriviaUtilities.EndsLine`/`IsCommentTrivia`) | one owner |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `ReihitsuFormatterDetachedRootTests` (31 tests), node-in-document ones | exact document text under LF and CRLF, with a space and with a tab, and a second pass that changes nothing | any byte difference, a tab-dependent output, a second-pass change | `AssertFormatsTarget` | adequate |
| `FormatNode` ones | exact text at the given depth, with the tab and LF/CRLF variants, and a second pass at the depth | wrong column or leading line break | `AssertFormatNode` | adequate |
| `…GetsTheResultOfTheNodeWithoutParent` | the attached result equals the result for a parentless copy at the FMT-PIPE-012 depth | any difference | `AssertFormatNodeMatchesNodeWithoutParent` (plus `AssertFormatNode` for two of them) | adequate; equivalence is the requirement's own statement |
| `EmbeddedIfTargetBehindMultiLineCommentTakesTheIndentationPhaseDepth` | text from `*/ if (b)` to the end, and a stable second pass | wrong brace or body column | ends-with | adequate for the decided columns; the whole document is pinned by the RH5204 code-fix test |
| `AssignedCollectionInitializerTargetKeepsItsOpeningBraceOnTheAssignmentLine` | tab = space, `L = {` kept, stable | — it passes without the flag (`04-test-implementer.md`) | contains | Weak, but no caller passes that target (`07-spec-author.md`; RH5301, RH5303, and RH5304 pass the enclosing node), so it is not a finding (`AGENTS.md`, *Supported inputs*) |
| `FormattingContextFactsHoldOnlyPlainValues` | no field of the context, or of a formatter type it holds (generic arguments and element types expanded), is syntax | a syntax-typed field | reflection | adequate |
| `ReihitsuFormatterTests.FormatNodeWithPreviousTokenOutsideTargetPreservesTrailingDocumentation` (changed) | the FMT-PIPE-012 example output | a leading line break | exact | adequate |
| RH5405 `VerifyFixKeepsTheTrailingCommentOfAnElseIfCondition` (+CRLF) | the whole fixed document | comment lost, layout changed | `Verify` | adequate; fails on `main` (`08-test-implementer.md`) |
| RH5204 `VerifyEmbeddedIfBehindMultiLineCommentIsFixedInOnePass` (+CRLF) | the whole fixed document, one incremental and one Fix All iteration | non-convergence, wrong columns | `Verify` with iteration counts | adequate |
| (missing) RH6023 on a conditional-access assignment | `main`'s output | — | — | missing; part of finding 1 |

## Visible output is decided

- RH5405: the comment is kept because SHARED-FIX-003 requires it. The brace column 13 is unchanged from `main` (thoenissen/Reihitsu#1521).
- RH5204: the columns follow maintainer answer 6. The blank line behind `while (a)` is unchanged from `main` (decision 13).
- Node-level formatter tests: their outputs are not reachable through a code fix, except the two above (`AGENTS.md`, *Supported inputs*). The changed `FormatNode` expectation does not reach RH5401, which reads only `ToString()` for a line break.
- No rule page changed.
- The only uncovered visible change is finding 1.

## Failure classes dismissed

- FC-002: each flag's qualifier ("directly", "on the right of", "through `and`/`or`/`not`") is an explicit condition of its one predicate.
- FC-003: one boundary is untested, the one-line property's "root holds a line break" side. No caller passes an accessor or an accessor list with a one-line property (see *Guard scope*), so this is not a finding.
- FC-004, FC-019, FC-020, FC-021: comments.
  - The `else if` comment class of thoenissen/Reihitsu#1527 was enumerated through RH5405: `//`, `/* */`, a comment inside the condition, a three-branch chain, and an `else // other` behind it. Every comment is kept.
  - The issue's own example (`if (x) Bar(); else if (y) // note Foo();`) gives braces in column 8 with the comment kept.
  - Directives: no new decision reads a directive. The list position is unchanged in substance.
- FC-005: no guard covers comments only.
- FC-006, FC-022 to FC-026, FC-036: no rewrite of literals, identifiers, modifiers, or member kinds.
- FC-007, FC-008, FC-009: the new code-fix tests pin convergence in one iteration. Finding 1's output converges in one iteration as well.
- FC-010, FC-011, FC-012: every new formatter test checks a second pass under LF and CRLF. No loop was added.
- FC-013, FC-014: no analyzer changed.
- FC-017: see *Assertion adequacy*.
- FC-018: no rename.
- FC-027: enclosing scopes were enumerated per flag, and finding 1 is the scope the inventory missed.
- FC-028: the `FormatNode` / node-in-document differences are the specified ones (FMT-PIPE-012 vs. FMT-PIPE-024).
- FC-029: not involved.
- FC-032: the last compiled change (`5e44c16`) precedes the implementer's and the cleaner's full runs.
- FC-033, FC-034: the code-fix tests use the reported shapes under both line endings.
- FC-035: the expected values come from the spec, the change doc, and `main`'s output, checked against `main` (`08-test-implementer.md`).
- FC-037: no text-only claim.
- Zero-offset depth for `FormatNodeInDocumentWithContextAsync`: the same mechanism as maintainer answer 6. The skeptic's corpus found no change.
- The column-0 previous-token approximations (decision 6): probed on head and `main` with an attributed class and a class with a doc comment under a file-scoped namespace, and top-level chain statements. No difference.
- Whole-document formatting time: head vs `main` on a 361 KB file is within run-to-run noise.

## Architecture findings

**Chain outermost-ness has more than one owner, and finding 1 is a reachable member of it.** Which node of a member-access chain is the outermost, and so gets its links laid out, is decided by:

- `FluentChain.ContinuesChain`/`IsOutermostChainNode` in Core, which reads the document through `Parent`, two levels up in the conditional-access assignment arm;
- the `ChainContinuation` fact, captured by `ReihitsuFormatterHelpers.CaptureRootConstruct` and applied by `ChainLineBreakRewriter.IsOutermostChainNode` only at the root;
- `MethodChainAlignmentContributor`, through `FluentChain.Create(root)` (lead's decision 7, unreachable member).

Input that shows the fact and Core's predicate disagreeing: RH6023 on `a?.B⏎  .C=5;` (finding 1). A narrow repair in the formatter would restate `ContinuesChain`'s assignment arm next to the wrapper, a second statement of the rule. Under `squad/README.md`, *Architecture findings*, that is the case for changing the structure in this run, within the components the run already changes. The lead decides.

## Hints

- thoenissen/Reihitsu#1527 expects the braces in the `else if`'s column. The run's RH5405 test pins column 13 for an `else if` whose own body is the only unbraced one; the issue's own example (both bodies unbraced) gets column 8. The PR text should say that the column-13 shape stays with thoenissen/Reihitsu#1521 when it closes thoenissen/Reihitsu#1527.
- `IndentationPhase.Execute` still re-roots a root that has a parent. Since FMT-PIPE-023, no production entry point passes one (`FormattingPipeline.Execute` callers pass tree roots or detached copies), so the branch is defensive only.
- `PrecedingTokenFacts.Resolve` now copies an in-tree previous token's trailing trivia into `TriviaFact`s and rebuilds trivia on every `TrailingTrivia` read, also in whole-document formatting. That is an allocation only the root's first token needs; no slowdown was measurable.
- Several construct flags (`OneLineProperty`, `AssignedCollectionInitializer`, and the inner-chain-part use of `ChainContinuation`; `CaseLabelPattern` and `ElementAccessArguments` were not settled) serve node-level inputs that, per the handovers, no code fix passes. They were added on maintainer answer 3, before `AGENTS.md`, *Supported inputs*, existed.
