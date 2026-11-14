Role: auditor
Commit: 79610f4a0716e35167e5805fbb46b2e03fe68a52
Verdict: #1529: pass, text cleanup; #1543: pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## thoenissen/Reihitsu#1529

### Scope

Head 79610f4 (checked out, clean tree), base `origin/main` b34c0f1 (equals the SHA in my prompt and is an ancestor of head). Repair delta against 01672a1, the commit of the first audit (`08-auditor.md`). The delta is what `09-spec-author.md` through `13-spec-checker.md` describe and nothing else: the FMT-PIPE-024 *Conditional-access branch* row removed and its SHARED-CHAIN-001 bullet reworded (`sdd/specs/formatter/pipeline.md`); the change doc gains FMT-PIPE-024 under *Affected requirements*, a *Decisions* subsection, and a reworded alternative E; `FluentChainBoundary.RootIsConditionalAccessBranch`, its constructor parameter, and its `From` argument deleted; `FluentChain.IsConditionalAccessBranch` deleted and its non-root branch inlined into the assignment arm of `ContinuesChain`; the test `ReihitsuFormatterDetachedRootTests.ConditionalAccessAssignmentTargetKeepsTheWrappedLinkOfItsAssignedMember` deleted (decision 7). Scope did not grow: no other surface and no other requirement changed.

Previous findings:

| # | Status | Why |
|---|---|---|
| 1 | closed | The flag, its constructor parameter, its `From` argument, the `node.Parent == null` branch, and the FMT-PIPE-024 row are gone; `rg -i "ConditionalAccessBranch\|RootIsConditional"` over `src`, `sdd/specs`, `documentation` finds nothing. One summary outside the diff still describes the fact: new finding #2. |

The first audit's statement that "no test sets" the fact was wrong: it searched for `FluentChainBoundary`, and the deleted test reached the fact through `FormatNodeInDocumentAsync`. The implementer caught it by running the formatter project (`10-implementer.md`); nothing of it is left.

Equivalence of the repaired rule, traced statically. Old assignment arm: `Left == child && (assignment.Parent == null ? boundary.RootIsConditionalAccessBranch : Parent is ConditionalAccess && WhenNotNull == assignment)`. New arm: the second alternative alone, which is `false` when `assignment.Parent == null`. The two differ only when the assignment is the parentless root and the old flag was `true`, and `From(target)` set the flag only when the target was the `WhenNotNull` of a conditional access. So the output changes only for a `FormatNodeInDocumentAsync` target that is an assignment behind `?`. Callers checked, none passes one:

- RH5111: `GetFormattingContextNode` returns `FluentChain.GetOutermostChainNode(assignment)`, and `ContinuesChain(conditionalAccess, assignment, default)` is always `true` for such an assignment, so context ≠ target and the fix takes `FormatNodeInDocumentWithContextAsync`, which supplies no boundary.
- `FormatNodeInDocumentWithContextAsync` delegates to `FormatNodeInDocumentAsync` only when target == context. Its callers are RH5111 (above), `FluentChainCodeFixHelper` (chain node, statement or member context), RH3208 (local function), and RH7101 (field declaration).
- Every other `FormatNodeInDocumentAsync` caller (`rg -n "FormatNodeInDocumentAsync\(" src/Reihitsu.Analyzer.CodeFixes`) passes a declaration, statement, argument list, lambda, object creation, binary expression, outermost chain node, or a target from the `TryGetFormatTarget` of RH5206/RH5303–RH5307. Those targets are a `StatementSyntax`, `EqualsValueClauseSyntax`, `ArrowExpressionClauseSyntax`, `LambdaExpressionSyntax`, `SwitchExpressionArmSyntax`, `ArgumentSyntax`, or the matched construct itself, and none of them is an assignment.
- `FormatNode` and whole-document formatting use the default boundary, whose flag was `false`, so they are unchanged.

The public constructor `FluentChainBoundary(bool, bool)` became `FluentChainBoundary(bool)`. Its only caller is `From` (`rg "FluentChainBoundary\("`), and no assembly is a public API (`AGENTS.md`, *Supported inputs*).

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 2 | low | `src/Reihitsu.Formatter/Data/FormattingContext.cs:152-158`, summary of `RootChainBoundary` (not in the diff) | FC-031 | – | PR-introduced | The summary still says that the facts are "whether the root's parent continues the root's chain, and whether the root is the branch of a conditional access", and that the rule reads them "for every node of the root whose decision reads above the root, not only for the root itself". After the repair the boundary holds one fact, and only the root's own decision reads it (`ContinuesChain` with `parent == null`). The assignment arm no longer reads above the root. Text only: `FormattingContext` is an `internal record`, so this is no public API documentation, and the change reaches no compiled behavior, diagnostic, or rule metadata. | Describe the one fact, whether the root's parent continues the root's chain, read for the root itself, and drop the conditional-access clause and "not only for the root itself". |

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Whether an assignment continues the chain of its left side (`ContinuesChain`, assignment arm) | the assignment's actual parent; `false` when the assignment has none | RH5111 `GetFormattingContextNode` → `GetOutermostChainNode`, which walks the same `ContinuesChain` with `default` on the attached tree | same function; a difference is possible only at a parentless root, which no caller passes as an assignment behind `?` (traced above) | agree |
| Whether the root's parent continues the root's chain (`RootContinuesChain`) | unchanged: `From(root)` = `ContinuesChain(root.Parent, root, default)` | read in `ContinuesChain` when `parent == null` | unchanged by the repair | agree |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Whether an assignment behind `?` continues the chain of its left side | `rg -n -i "ConditionalAccessBranch\|WhenNotNull ==" src --glob '!*Test*'`: only the inlined arm in `FluentChain.ContinuesChain`. `FluentChainAnalysisHelper.GetOutermostChainNode` (analyzer) has its own walk that predates this run and does not take the assignment step; it is unchanged by the repair and was assessed in the first audit | one owner for the changed policy |
| Boundary facts captured for node-level roots | `FluentChainBoundary.From`, its one consumer `ReihitsuFormatter.cs:226`, read through `FormattingContext.RootChainBoundary` and `ChainLineBreakRewriter.cs:393` | one owner |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `ConditionalAccessAssignmentTargetKeepsTheWrappedLinkOfItsAssignedMember` (deleted) | was: a `FormatNodeInDocumentAsync` target that is an assignment behind `?` keeps `.C` on its own line | — | — | The deletion is correct. The input comes only from a direct call (`AGENTS.md`, *Supported inputs*), and FMT-PIPE-024 now leaves it undecided. FMT-PIPE-023 keeps 9 sibling tests with the same `⇥` fixture. The chain-continuation fact keeps `ChainPrefixTargetKeepsTheLayoutOfTheLongerChain` and further FMT-PIPE-024 tests. The layout itself stays covered through the reachable path by the RH5111-006 tests and the five `…MatchesFormattingTheDocument` parity tests from the first audit, which this round leaves byte-identical. |

No test was added or changed in the round. Check 10 (visible output decided) has nothing new to cover: no expected value was added or changed, and no rule page was touched.

### Failure classes dismissed

- FC-001, FC-002, FC-003: the repair removes a condition and narrows no guard. The remaining condition has no qualifier in the changed spec text, and both sides of the reachable boundary (an assignment behind `?` and one that is not) stay tested by the RH5111-006 boundary tests.
- FC-015, FC-016: one owner (*Policy ownership*); no sibling copy of the deleted fact remains.
- FC-004, FC-005, FC-019–FC-021: no trivia handling changed.
- FC-006, FC-022–FC-027, FC-029, FC-036: no rewrite, literal, identifier, or member-kind logic changed.
- FC-028: `FormatNode` used the default boundary before and after, and the context entry point supplies none, so the node-level paths agree as before.
- FC-007–FC-009, FC-018, FC-030: no code-fix code changed. The RH5111 path does not read the deleted fact (traced above).
- FC-010–FC-014: no phase logic changed for any reachable input; no rule added.
- FC-017: no test added.
- FC-032: the implementer and test implementer ran core, analyzer, formatter, and architecture after the compiled change; the cleaner built Release. No earlier result is reused.
- FC-033–FC-035, FC-037: no reproduction, no new expected value, no text-only claim.
- FC-038–FC-043: no shape declared unsupported; `FindUnsupportedSyntax` call sites unchanged.
- FC-031: finding #2. The changed summaries of `ContinuesChain` and `FluentChainBoundary` describe the new code.
- Tab-width matrix: no column decision changed.

### Architecture findings

_None._ The repair removes a fact that a node-level entry point passed across the boundary for a root no caller produces.

### Hints

_None._

## thoenissen/Reihitsu#1543

Nothing in the repair round touches this issue: the delta changes no file of the blank-line phase, FMT-BLANK-020, its change doc, or its tests. The first audit (`08-auditor.md`, no finding) stands.

## Whole run

- `scripts/lane-check.sh behavior-change`: confirmed.
- The added source lines contain no `TODO` and no tracker number.
- Scope: every changed file is named by a repair handover or by `00-lead.md` decisions 6 and 7.
- Tests not re-run: no finding turns on a test result.
- Verdict `pass, text cleanup` for #1529: finding #2 is confined to the XML summary of an internal property. It cannot reach compiled behavior, diagnostics, public API documentation, or rule metadata.
