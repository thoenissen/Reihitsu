Role: implementer
Commit: 75ee131
Verdict: tests pass

Repair round 1 covers auditor findings 1, 3, and 4 (`12-auditor.md`) and decisions 14 and 15 (`00-lead.md`). The coordinator's message named targets but no intended fix, suspected cause, or hoped-for verdict, so nothing was ignored.

The one test that fails is in the architecture project. It flags a comment in the test implementer's new RH6023 test file, not production code; see *Disputed tests or scope*.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/FluentChainBoundary.cs` (new) | New public `readonly struct FluentChainBoundary` holding two plain flags about the code above a root: `RootContinuesChain` and `RootIsConditionalAccessBranch` (the root is a conditional access's `WhenNotNull`). `From(root)` captures both through the chain rule's own predicates. `default` describes a root with nothing above it. | FMT-PIPE-023, FMT-PIPE-024, SHARED-CHAIN-001 |
| `src/Reihitsu.Core/FluentChain.cs` | `ContinuesChain(parent, child, boundary)` is now `internal` and is the one statement of the rule. When `parent` is null, the boundary answers whether the root's parent continues its chain. The assignment arm reads the new `IsConditionalAccessBranch(node, boundary)`, which asks the boundary when the assignment is the root. New overloads: `IsOutermostChainNode(node, boundary)` and `Create(node, requireLink, boundary)`. The old overloads pass `default`, so every existing caller keeps its behavior; a parentless node read as "does not continue" before and still does. | same |
| `src/Reihitsu.Formatter/Data/FormattingContext.cs` | New `RootChainBoundary` property and constructor parameter. | FMT-PIPE-024 |
| `src/Reihitsu.Formatter/Data/Enumerations/RootConstruct.cs` | Removed the `ChainContinuation` flag, which now lives in `FluentChainBoundary`. Renumbered the remaining flags and updated the summary. | FMT-PIPE-024 |
| `src/Reihitsu.Formatter/ReihitsuFormatter.cs` | `FormatNodeInDocumentAsync` passes `FluentChainBoundary.From(targetNode)`. All three entry points take the depth from `SyntaxIndentationUtilities.ComputeIndentLevel`. | FMT-PIPE-012, FMT-PIPE-024 |
| `src/Reihitsu.Formatter/Utilities/ReihitsuFormatterHelpers.cs` | Removed `ComputeDepth` and the chain-flag capture. Updated the class summary. | finding 4 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/ChainLineBreakRewriter.cs` | Removed the root-only wrapper `IsOutermostChainNode(original, RootConstruct)`. `NormalizeIfOutermost` now calls `FluentChain.IsOutermostChainNode(original, _context.RootChainBoundary)` for every node. | finding 1 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Contributors/MethodChainAlignmentContributor.cs` | Calls `FluentChain.Create(node, false, context.RootChainBoundary)`. Summary updated. | decision 14 (folded in; see below) |
| `src/Reihitsu.Core/SyntaxIndentationUtilities.cs` | New public `ComputeIndentLevel(SyntaxNode)` holds the one depth loop. `ComputeStatementIndentLevel(StatementSyntax)` delegates to it, so its callers behave as before. | finding 4, FMT-PIPE-012 |
| `src/Reihitsu.Formatter/Pipeline/BlankLines/Rewriter/BlankLineTriviaBoundaryRewriter.cs`, `BlankLineCollapser.cs` | Comments only: the remarks now say that a node-level root has no parent (FMT-PIPE-023), instead of "a root that an earlier phase detached". | finding 3 |

### Design

The decision is which node of a member-access chain is the outermost one. One rule, `FluentChain.ContinuesChain`, owns it, and it reads up to two levels above a node. A node of a detached root can reach above the root in two ways:

- the root itself, through its parent;
- a direct child of a root assignment (the assignment's `Left`), through the root's parent.

The boundary answers exactly those two reads, so the rule answers for every node of the root, not only the root itself. There is no second copy of the rule: the formatter keeps no chain predicate of its own, and the capture (`FluentChainBoundary.From`) calls the rule's own predicates.

Folding `MethodChainAlignmentContributor` in cost one argument. It now gets the same answer as the line-break phase.

- **Reachable outputs:** I checked them by running every analyzer and formatter test. All code-fix tests pass, including the 7 new RH6023 tests pinned to `main`'s output.
- **Unreachable outputs that change:** `FormatNodeInDocumentAsync` on an inner chain part of a longer chain. That node is no longer aligned as a chain of its own. Example: the `.Where` line in column 37 that `05-implementer.md` showed.
- **Why that is acceptable:** decision 7 records that no code fix passes such a target (RH5112 and RH5201 pass the outermost node), so under `AGENTS.md`, *Supported inputs*, this is not a visible output. If the lead reads the coordinator's "no visible output other than `main`'s" condition differently, revert the contributor hunk and the 4-line `Create` overload; everything else stands without them.

## Policy owners

- `rg -n "ContinuesChain|IsOutermostChainNode|GetOutermostChainNode|IsConditionalAccessBranch|FluentChain.Create" src --type cs`
  - In Core, the rule's only statement is `ContinuesChain` and `IsConditionalAccessBranch`. `IsOutermostChainNode`, `GetOutermostChainNode`, `Create`, and `FluentChainBoundary.From` call it.
  - The formatter's callers are `ChainLineBreakRewriter` and `MethodChainAlignmentContributor`, both with the boundary. The other `FluentChain.Create(node)` calls in `ChainLineBreakRewriter` act on nodes that have already passed the gate.
  - Analyzer side: `FluentChainCodeFixHelper` calls `GetOutermostChainNode` on attached trees, with no change. `FluentChainAnalysisHelper.GetOutermostChainNode` in `Reihitsu.Analyzer` is a separate, pre-existing walk with a different model. This run does not touch it, and no node-level root reaches it.
- `rg -n "GetChildIndentLevel\(|ComputeIndentLevel|ComputeStatementIndentLevel" src --type cs`
  - The one loop is `SyntaxIndentationUtilities.ComputeIndentLevel`.
  - `LayoutComputer` uses `GetChildIndentLevel` top-down per child, which is the same per-step owner.

## Tests run

```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH6023AssignmentOperatorsMustBeSpacedCorrectlyAnalyzerTests"
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~ReihitsuFormatterDetachedRootTests"
```

The formatter class includes the new `ConditionalAccessAssignmentTargetKeepsTheWrappedLinkOfItsAssignedMember`.

| Run (Debug) | Result |
|---|---|
| RH6023 class | Passed 22/22 |
| `ReihitsuFormatterDetachedRootTests` | Passed 32/32 |
| formatter project | Passed 3427/3427 |
| analyzer project | Passed 6323/6323 |
| core project | Passed 535/535 |
| architecture project | Failed 1, Passed 91, Total 92 (see below) |

The Debug build has 0 warnings and 0 errors. `scripts/format.sh` over the changed files changed nothing on its last run.

## Disputed tests or scope

- **Architecture failure: a test file, not production.** `SelfReferentialTrackerReferenceTests.RepositoryCommentsContainNoSelfReferentialTrackerReferences` fails on line 839 of `src/Reihitsu.Analyzer.Test/Formatting/RH6023AssignmentOperatorsMustBeSpacedCorrectlyAnalyzerTests.cs`, which was added in `4778920`. The line is `/// Verifies the diagnostic marked <c>#0</c> …`: the test reads the markup location `#0` as a tracker number. I may not change a test, so this goes to the test implementer: reword the comment, for example "the first marked diagnostic location".
- **Scope: new public members in Core.** `FluentChainBoundary`, `FluentChain.IsOutermostChainNode(node, boundary)`, `FluentChain.Create(node, requireLink, boundary)`, and `SyntaxIndentationUtilities.ComputeIndentLevel`. Under `AGENTS.md`, *Supported inputs*, no assembly is a public API. They are reported here as the role asks.
- **Spec text:** FMT-PIPE-024's "Chain continuation" row names one fact. The implementation needs a second plain value with it: whether the root is the `WhenNotNull` branch of a conditional access. Without it, the decision for a root assignment's `Left` reads above the root. That is finding 1, and it is required for `main`'s RH6023 output. The spec author should extend the row; the same applies to the code-owner list of finding 2, which should add `FluentChainBoundary`. With the depth now in Core's `ComputeIndentLevel`, FMT-PIPE-012's owner text may want to name it too.

## Architecture findings

_None_ beyond decision 14. With this change the chain decision has one owner. The capture, the line-break phase, and the indentation phase all ask `FluentChain` with the same boundary.

## Follow-up candidates

_None._
