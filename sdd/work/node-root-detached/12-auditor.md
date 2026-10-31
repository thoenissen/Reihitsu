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

## Round 2 (repair round 1)

Commit: 906313ef22074c78730cfd7381591ee0b538b2b0
Verdict: findings

### Scope

- Audit of the repair delta only: `git diff 4ebb825 906313e`.
  - Handovers `13-test-implementer.md`, `14-implementer.md`, and `15-spec-author.md`; round 2 of `10-cleaner.md` and `11-spec-checker.md`; decisions 14–19 in `00-lead.md`.
- `origin/main` is still `c24bc1d` (`ls-remote`) and is merged. The checkout is `906313e`.
- Scope did not grow beyond the components the run already changes. Decision 14 moves the chain rule's input into Core (`FluentChain`, the new `FluentChainBoundary`) and has the chain alignment ask it too. Both are counterparts that round 1 named.
- Evidence: a fresh `git archive` copy of `906313e` and the round-1 `main` copy, both in the scratchpad. `scripts/apply-fix.sh` was run on both trees:
  - RH6023, RH6024, RH5111, RH5405, and RH5417 over every round-1 fixture;
  - RH5204 over three new chain shapes;
  - new fixtures `fxr2a` (RH6023) and `fxr2b` (RH5111).
  
  The coordinator's message carried no intended fix or hoped-for verdict.

### Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | RH6023 over `fx6023` and `fx6023b` (wrapped `Left` behind a member binding and behind an element binding, nested conditional access, compound assignment, longer left chain, one-line `Left`, bare binding): head output equals `main` under LF and CRLF. One owner (`FluentChain.ContinuesChain` with `FluentChainBoundary`) now answers for every node of the root. The rule reads at most two levels up, and both reads that can leave the root (the root's parent, and the parent of a root assignment) are answered by the boundary. The two `FluentChain.Create(node)` calls in `ChainLineBreakRewriter` that act on visited, parentless copies keep `default`, so a copy is not mistaken for the root. `LayoutComputer` passes `MethodChainAlignmentContributor` only nodes of the attached layout tree (`root.DescendantNodesAndSelf()`), so only the root is parentless there. The 7 RH6023 tests and the formatter test pin `main`'s output under LF and CRLF, through whole-document `Verify` |
| 2 | closed | `RootConstruct`, `RootConstructExtensions`, `TriviaFact`, and `FluentChainBoundary` are code owners of `formatter/pipeline.md` and in the index; architecture project 92/92 (`11-spec-checker.md`, round 2) |
| 3 | closed | Both remarks now state that a node-level root has no parent (FMT-PIPE-023) |
| 4 | closed | One loop, `SyntaxIndentationUtilities.ComputeIndentLevel`. `ComputeStatementIndentLevel` delegates to it, and `ReihitsuFormatterHelpers.ComputeDepth` is gone |

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 5 | high | `src/Reihitsu.Core/FluentChain.cs` (`IsConditionalAccessBranch`, now read with the root's boundary); `sdd/changes/2026-10/node-root-detached.md` *What* and *Visible output changes* | FC-030, FC-032 | – (see below) | undecided output | Two code fixes now give a result that differs from `main`, and the change doc does not list it. The target is an assignment inside a conditional access whose root an earlier line-break rewriter changes. On `main` and in round 1, `LineBreakAssignmentRewriter` changed the root first, so the chain rewriter saw a parentless root and laid the wrapped left side out as a chain of its own (joined). The boundary now answers for that root as well, so the wrap is kept. Reached through RH5111, which formats the assignment, and through RH6023 when the value also stands on the next line. **Failing members (output differs from `main`, LF and CRLF):** RH5111 on `a?.B⏎  .C =⏎    5;` (`main`: `a?.B.C = 5;`; head: `a?.B⏎  .C = 5;`); RH5111 behind an element binding (`a?[0]⏎  .C =⏎5;`); RH5111 on a nested conditional access (`a?.B?.B⏎    .C =⏎5;`, head: `.C = 5;` in column 13); RH6023 on `a?.B⏎  .C=⏎    5;` (same split as RH5111). **Members that hold:** RH5111 with `Left` on one line (`a?.B.C =⏎5;`); RH6023 on a compound assignment with the value on the next line (`a?[0]⏎.C+=⏎5;`, which `LineBreakAssignmentRewriter` does not join on either tree); RH6023 without a wrapped value (finding 1's members). The new outputs keep the user's wrap, as whole-document formatting does (one column apart, as decision 17 records for RH6023). They converge in one iteration. But no unchanged requirement demands them; FMT-PIPE-024's conditional-access-branch row is this run's own requirement. No maintainer answer covers them, and no test pins them. The change doc's implementer-stage bullet ("a target that a phase before the line-break phase changes … Two kinds of target change") names only inner chain parts and assigned collection initializers. *Visible output changes* lists only RH5204 and RH5405. **Not a repeat of #1:** round 1 listed RH5111 as holding (equal to `main`). Its output now changes because the chain rule and the fact agree for a root an earlier rewriter changed: the intended effect of decision 14, under the mechanism of decision 4. It is not the same decision failing between the same components. | Ask the output question. If the new output is kept: the spec author adds the RH5111 case (and RH6023 with a wrapped value) to *Visible output changes* and to the implementer-stage bullet, and the test implementer adds an RH5111 code-fix test (LF and CRLF) on `a?.B⏎  .C =⏎    5;`. If `main`'s output is wanted, it cannot come back without making the chain decision depend on whether an earlier rewriter changed the root, which contradicts FMT-PIPE-023. Owners: lead, then spec author and test implementer. |

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Outermost chain node | `ContinuesChain(parent, child, boundary)`: the parent, and the grandparent through `IsConditionalAccessBranch` for an assignment; at a parentless node the boundary answers | `FluentChainBoundary.From`: the same two predicates on the attached target with `default` | same predicates, same span; complete for the rule's two-level reach | holds (finding 5 is its intended output, not a disagreement) |
| Parentless node = root | `node.Parent == null` inside `ContinuesChain` / `IsConditionalAccessBranch` | "is the formatting root" | The boundary is passed only by `NormalizeIfOutermost` (original nodes) and `MethodChainAlignmentContributor` (attached layout tree). Visited copies and analyzer callers pass `default`, which reproduces the earlier behavior (`_ => false` for a null parent) | holds |
| Depth | `ComputeIndentLevel`: sum of `GetChildIndentLevel` | `LayoutComputer` per-child step | same owner | holds |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Outermost chain node | `FluentChain.ContinuesChain` / `IsConditionalAccessBranch` (Core), called by `IsOutermostChainNode`, `GetOutermostChainNode`, `Create`, and `FluentChainBoundary.From`; formatter readers `ChainLineBreakRewriter`, `MethodChainAlignmentContributor` | one owner. `Reihitsu.Analyzer.Core.FluentChainAnalysisHelper.GetOutermostChainNode` is a pre-existing separate walk on attached trees that the run does not touch (hint) |
| Node depth | `SyntaxIndentationUtilities.ComputeIndentLevel` (`ComputeStatementIndentLevel` delegates) | one owner |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| 7 RH6023 tests (`VerifyFixKeeps…`, `VerifyFixOnAnAssignedMemberBinding…`) | the whole fixed document equals `main`'s, under LF then CRLF, default iterations | joined or re-laid chain, wrong column | `VerifyUnderBothLineEndings` → `Verify` | adequate; both sides of the boundary are tested |
| `ConditionalAccessAssignmentTargetKeepsTheWrappedLinkOfItsAssignedMember` | exact document text with a space and a tab, LF and CRLF, and a second pass | joined link | `AssertFormatsTarget` | adequate |
| (missing) RH5111 / RH6023 with a wrapped value | — | — | — | missing; part of finding 5 |

### Failure classes dismissed (delta)

- FC-001, FC-002: the boundary's two flags are the rule's own predicates.
- FC-003: both sides of the `Left`-wrap boundary are tested for RH6023.
- FC-004, FC-019 to FC-021: the repair touches no trivia decision. RH5405 still keeps every comment member, and #1527's example still gets column 8.
- FC-007: finding 5's outputs converge in one iteration.
- FC-010: the new formatter test has a second pass under LF and CRLF.
- FC-013, FC-014: no analyzer changed.
- FC-015: one owner (see the table).
- FC-028: unchanged.
- FC-031: the summaries of `FluentChain`, `FluentChainBoundary`, `RootConstruct`, `FormattingContext`, `MethodChainAlignmentContributor`, and `ReihitsuFormatterHelpers` describe the new code.
- FC-032: the analyzer project passed after the last compiled change (`14-implementer.md`), and the cleaner rebuilt in Release.
- Decision 18 (inner chain part no longer aligned as a chain): RH5204 reports no chain-link line, including a dangling `.` and argument lines (`fx5204c`; head equals `main`). RH5307 passes a statement, clause, argument, or bracketed list. The chain fixes pass the outermost node or a statement or member context. No caller reaches it.
- RH6024, RH5417, and RH5204 over the round-1 fixtures: head equals `main`.

### Architecture findings

_None._ Decision 14 gave the outermost-chain decision one owner, and the line-break and indentation phases now ask it with the same boundary.

### Hints

- `FluentChainBoundary` is a Core type owned by `formatter/pipeline.md`, and its `From` is public API used only by the formatter. This is acceptable under `AGENTS.md`, *Supported inputs*. Noted because the shared spec `shared/fluent-chain-layout.md` names `FluentChain` but not the boundary.

## Round 3 (repair round 2)

Commit: 49dc0ef221d310207571181395983762ac6b1cb4
Verdict: pass

### Scope

This round audits the repair delta only: `git diff d57929d 49dc0ef`.

- Head and base:
  - `origin/main` is still `c24bc1d` (`ls-remote`) and is merged.
  - The checkout is `49dc0ef`.
- The delta holds tests and documentation only:
  - three RH5111 code-fix tests;
  - one RH6023 code-fix test;
  - the change doc;
  - the handovers `13` (round 2), `16`, `10` (round 3), `11` (round 3), and maintainer answer 8 in `00-lead.md`.
- No file under `src/Reihitsu.Core`, `src/Reihitsu.Formatter`, `src/Reihitsu.Analyzer`, or `src/Reihitsu.Analyzer.CodeFixes` changed. So round 2's code-fix evidence for `906313e` still holds for this head.
- The coordinator's message carried no intended fix or hoped-for verdict.

### Previous findings

| # | Status | Evidence |
|---|---|---|
| 1–4 | closed (round 2) | no production change since |
| 5 | closed | Maintainer answer 8 decides A (the member keeps its line break). The four new tests pin exactly the outputs round 2 measured on `906313e` with `apply-fix.sh`: RH5111 `a?.B⏎  .C = 5;`, `a?[0]⏎  .C = 5;`, and `a?.B?.B⏎     .C = 5;` (`.C` in column 13), and RH6023 on `a?.B⏎  .C=⏎    5;` → `a?.B⏎  .C = 5;`. Each test checks the whole fixed document under LF and then CRLF with default iterations, through `VerifyUnderBothLineEndings`, with one marked diagnostic. `scripts/test.sh --project analyzer --filter "FullyQualifiedName~VerifyFixKeepsTheWrappedLinkOfAnAssignedMember"`: Passed 7/7 (the 3 new RH5111 tests and 4 RH6023 tests). The change doc lists the case in *What* (the implementer-stage bullet, a third kind) and under *Visible output changes* (input, before `a?.B.C = 5;`, after, plus the element-binding, nested, RH6023, and one-line variants). A *Decisions* section records answer A and why `main`'s join cannot return (FMT-PIPE-023). The change doc matches the measured outputs, and the one-line member (`a?.B.C =⏎5;` → `a?.B.C = 5;`) is correctly stated as unchanged |

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| _None._ | | | | | | | |

### Guard scope

_N/A — no predicate or guard changed in this round._

### Policy ownership

_N/A — no production code changed in this round; ownership is as in round 2._

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RH5111…Tests.VerifyFixKeepsTheWrappedLinkOfAnAssignedMember{BehindAMemberBinding,BehindAnElementBinding,InANestedConditionalAccess}` | whole fixed document per answer A, LF and CRLF | `main`'s join, a wrong `.C` column, a second iteration | `VerifyUnderBothLineEndings` → `Verify` | adequate |
| `RH6023…Tests.VerifyFixKeepsTheWrappedLinkOfAnAssignedMemberWhoseValueIsWrapped` | same | same | same | adequate |

### Visible output is decided

Every expected value the delta adds is covered by maintainer answer 8 and recorded in the change doc's *Decisions*. No rule page changed.

### Failure classes dismissed (delta)

- FC-003: the boundary's other sides (member on the conditional access's line; compound assignment with a wrapped value) have unchanged output. The one-line side is pinned for RH6023 by round 1's `VerifyFixKeepsAnAssignedMemberOnOneLineBehindAMemberBinding`.
- FC-017: see above.
- FC-031: the new summaries describe their tests.
- FC-032: no compiled production change.
- FC-033: the inputs are the reported shapes under both line endings.
- FC-035: the expected values were checked against `apply-fix.sh` on the branch, and against answer A by the test implementer. The tests fail on `main`, which joins.

### Architecture findings

_None._

### Hints

_None._
