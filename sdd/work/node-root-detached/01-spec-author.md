Role: spec author
Commit: 7586822
Verdict: change doc written

Change doc: `sdd/changes/2026-10/node-root-detached.md` (`Status: Draft`). It needs the maintainer's approval of What and Why before tests are written (escalation 2). The decisions the maintainer should look at most closely are listed under *Decisions inside the change doc* below.

The prompt carried no intended fix or suspected cause beyond the issue text; nothing was ignored.

## Statements of thoenissen/Reihitsu#1489 checked against `main`

Checked at `7586822` (= `e7853b6` plus the lead's handover commits) by reading the code and running a scratch console program against `Reihitsu.Formatter` (kept outside the repository). Every statement holds unless noted.

- Node-level entry points pass the target itself — true for `FormatNode` and `FormatNodeInDocumentAsync`. **Not true for `FormatNodeInDocumentWithContextAsync`**: it passes `contextNode.ReplaceNode(targetNode, annotatedTarget)`, which has no parent, so its root is detached already today. No target behavior changes; the change doc records it.
- A phase that returns its input keeps the attachment; the first changing phase detaches — true. The tab replacement keeps it on purpose — true; **the line-ending normalization (added by B1) does the same**, by its own comment. The indentation phase re-roots an attached node — true.
- Example 1 reproduces exactly: `FormatNode` on the attached `Call(1, 2);` returns a line break followed by `        Call(1, 2);`; with a tab after the comma, `        Call(1, 2);`. The collapse happens in `BlankLineTokenCleanupRewriter` only when no previous token resolves — true.
- The four unspecified context facts (`RootPrecedingToken`, `RootListPosition`, `PreserveRootDocumentationBoundary`, `BaseIndentLevel`) — true. **Moot part:** `FormattingContext` now also carries `RootFirstTokenColumn` (B2), which FMT-PIPE-017 already specifies and which is a plain `int?`.
- `PrecedingTokenFacts` holds a `SyntaxTriviaList`, `RootListPosition` holds a `StatementSyntax` and a `SwitchSectionSyntax` — true. Readers: list position in `BlankLineStatementSpacingRewriter` and `BlankLineBreakSpacingRewriter`; `PrecedingTokenFacts.Resolve` in exactly five blank-line places (`BlankLineRegionDirectiveRewriter`, `BlankLineCollapser`, `BlankLineTriviaBoundaryRewriter`, `BlankLineTokenCleanupRewriter`, `BlankLineEditor`); `DocumentationCommentFormattingPhase.IsWrittenBehindRootPrecedingToken`; `CommentIndentationContributor` — true. The "document-text path" is `root.SyntaxTree.GetText` in `DocumentationCommentFormattingPhase.Execute`, which reads the whole document while the root is attached.
- `ComputeBaseIndentLevel` omits the embedded-statement level and adds initializer levels; Core calls it "deliberately inexact" — true. Example 2 reproduces: `Call(` in column 8, document formatting column 12.
- "It reaches the output in `FormatNode` and wherever the node-in-document column shift does not cancel it" — **the second half is moot**: since B2, FMT-PIPE-017 shifts every node-in-document root that is not the document root, and the document root is a compilation unit at depth 0 under both rules. `FormatNodeInDocumentAsync` on the same embedded statement already gives column 12. The depth reaches output only through `FormatNode`.
- The block-statement prediction (`RootListPosition.KeepsBlankLineAboveBlockStatement`) exists — true.
- FMT-CLEAN-008 names "a node that is not part of a syntax tree" — **already broader in behavior**: the indentation phase detaches every root before the cleanup phase, so an attached `FormatNode` root already loses its trailing line breaks (example 1 output ends without one). The change is wording only.
- `ComputeBaseIndentLevel` has a caller outside the formatter, `TargetAttributePlacementCodeFixProviderBase`; Done-when only asks that the formatter stop calling it.

## Simulation of the target behavior

To answer "which node-level test expectations change", a copy of the tree at `7586822` was extracted with `git archive` into the scratchpad (not the repository), and `ReihitsuFormatter` there was changed to detach every node-level root (`CSharpSyntaxTree.Create` with the node's parse options) and to compute the depth by walking `SyntaxIndentationUtilities.GetChildIndentLevel` from the root to the node. Results with `scripts/test.sh --no-install`:

- `--project formatter`: 3401 passed, 1 failed — `ReihitsuFormatterTests.FormatNodeWithPreviousTokenOutsideTargetPreservesTrailingDocumentation` expects `\n    /// <summary>…`, gets `    /// <summary>…` (the leading line break is gone because the token in front of the node is no longer read).
- `--project analyzer`: 6312 passed, 0 failed — no code-fix output changes.
- Both issue inputs give the desired output in the simulation (`        Call(1, 2);` with and without the tab; `            Call(1,` / `                 2);`). The initializer member `P = Call(1,` / `2)` gives column 8 (today 12, document 20); an anonymous-object member likewise 8.

Further probes on `main`, comparing `FormatNodeInDocumentAsync` with and without a tab in the target (block statement with a blank line or a comment above it, `else` behind a blank line, statement behind a `case` label and a blank line, first statement after `{` behind a blank line, statement after a local declaration, member behind `#region`, documentation comment behind the previous statement): identical in every case. `FormatNodeInDocumentWithContextAsync` with and without a tab: identical in every probed case. `FormatNode` differed in each case where a token precedes the node and the node's leading trivia holds a line break, as the change doc lists.

## Changed requirements

- FMT-PIPE-023 — Node-level entry points format a detached root — added
- FMT-PIPE-024 — Node-level input facts are plain values — added
- FMT-PIPE-012 — `FormatNode` skips the file-level guards and lays out the node at its depth — changed (depth rule, no reads around the node, title)
- FMT-PIPE-010 — Node-level entry points format only their target — changed (one sentence pointing at FMT-PIPE-023 and FMT-PIPE-024; no new obligation)
- FMT-CLEAN-008 — A node-level root does not end with a line break — changed (every node-level root other than a compilation unit; title; example added)

Also: `PrecedingTokenFacts` and `RootListPosition` added as code owners of `formatter/pipeline.md` (header and `sdd/specs/README.md`); `Uses` of `pipeline.md` gained FMT-INDENT-002, -003, -004, -012, -022, -024; `Uses` of `cleanup.md` swapped FMT-PIPE-012 for FMT-PIPE-023; ledger gained FMT-PIPE-023 and FMT-PIPE-024. If the implementer renames or replaces the two fact types, the code-owner lists must follow. No rule page changes: nothing a user sees from an analyzer changes.

`scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec"`: every spec structure and consistency test passes; `RequirementCoverageTests.EveryRequirementOfAnEnforcedSpecIsCovered` fails for the new requirements until their tests exist, as expected.

## Decisions inside the change doc

These are decided in the change doc rather than asked, because the issue text fixes them; the maintainer approves or overrules them with the change doc:

- `FormatNode` supplies no preceding-token and no list-position fact, also for an attached node. Required by the issue's first desired output; it is what changes `FormatNodeWithPreviousTokenOutsideTargetPreservesTrailingDocumentation`.
- The depth rule adds no level for an initializer or anonymous object, so `FormatNode` on an initializer member moves from column 12 to column 8 (document: 20). Keeping Core's initializer level would restate the second depth rule, which the issue rejects; no production caller reaches it (RH5401 passes property declarations and reads only whether the result is one line).
- `FormatNodeInDocumentWithContextAsync` keeps supplying no preceding-token and no list-position fact, as today; its root is detached already.
- The block-statement exception of the list position stays until thoenissen/Reihitsu#1496.

## Examples for the tests

- FMT-PIPE-023 — the statement after `var a = 1;` and a blank line: `FormatNode` with and without the tab, and `FormatNodeInDocumentAsync` with and without the tab.
- FMT-PIPE-012 — the embedded statement of `if (a)` (also `else` and a loop body), `indentLevel: 0` and `indentLevel: 3`, the initializer member (column 8), and the property `Other` with the documentation comment behind the previous property.
- FMT-PIPE-024 — `FormatNodeInDocumentAsync` inserting the blank line after a local declaration from the list position; the facts' plain-value rule (no field of the context or its fact types holds a `SyntaxNode`, `SyntaxToken`, `SyntaxTrivia`, `SyntaxTriviaList`, or `SyntaxNodeOrToken`) is a candidate for a structural test.
- FMT-CLEAN-008 — `FormatNode` on an attached statement returns no trailing line break.

## Non-goals

- The gap FMT-PIPE-017 records for the context entry point (thoenissen/Reihitsu#1501).
- `FormatNodeInDocumentAsync`'s blank-line decisions above a target that starts its line beyond moving their inputs into plain values.
- Removing `ComputeBaseIndentLevel` from Core.
- Extending FMT-PIPE-021 to node-level roots.

## Open questions for the maintainer

1. **A member-body target that does not start its line keeps its brace behind the declaration.** Input, `FormatNodeInDocumentAsync` with the body of `M` as target:

   ```csharp
   class C
   {
       void M() { Foo(1, 2); }
   }
   ```

   Real output on `main` (identical with a tab in the body, so this run does not change it):

   ```csharp
   class C
   {
       void M() {
                    Foo(1, 2);
                }
   }
   ```

   Why it looks wrong: FMT-BREAK-002 says the braces of a body a node-level entry point is given are placed as in a document, but the `{` stays on the declaration's line, because FMT-PIPE-010 keeps the leading trivia of a target that does not start its line, so the line break the phases put in front of `{` is not written back; the body is then laid out from the brace's column (FMT-PIPE-017). Requirement IDs: FMT-BREAK-002, FMT-PIPE-010, FMT-PIPE-017. Readings: **A** — the output is intended; FMT-BREAK-002 gains a sentence that the line break in front of a target's own first token is not written back when the target does not start its line (clarification). **B** — the entry point writes back that line break, so `{` gets its own line under `void` and the body is indented from it (behavior change of FMT-PIPE-010, change doc). Recommendation: **B** as an approved follow-up issue outside this run — the output contradicts the requirement it is meant to satisfy, and this run does not change it. Whether a code fix passes such a target today was not checked.

## Architecture findings

None beyond what the issue itself addresses (two depth rules; facts read across the root boundary in two ways).
