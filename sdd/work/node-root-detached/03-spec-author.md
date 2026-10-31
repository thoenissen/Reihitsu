Role: spec author
Commit: d686a76
Verdict: open question

This is a repair round after the skeptic's handover `02-skeptic.md` and the maintainer's answers in `squad/decisions/2026-10-31-node-root-detached-missing-facts.md` (B: add plain-value facts for the decisions that read outside a detached root; node-in-document outputs stay as they are with or without a tab), `…-leading-trivia-owner.md` (A: no change to who owns a target's leading trivia; close the `else if` comment loss narrowly through this run's facts; the structural change goes to thoenissen/Reihitsu#1521), and `…-rh5405-comment-drop.md` (A: the RH5405 comment loss on `main` is a separate follow-up). The change doc goes back to `Status: Draft` and returns for approval.

The coordinator's message carried no intended fix beyond those answers; nothing was ignored.

One point is a blocking question (below): the zero-offset depth. I wrote the rest of the amendment. The change doc presents my recommendation for that point and says it is open.

## What the amendment changes

Change doc `sdd/changes/2026-10/node-root-detached.md`: every amended passage is marked *(amended 2026-10-31, skeptic stage)*, and an amendment note under the header names the three decisions.

- **What.** `FormatNodeInDocumentAsync` gets seven more plain-value flags. Each answers one decision that today reads the target's ancestors:
  - `else if`
  - chain continuation
  - one-line property
  - list element
  - element-access arguments
  - case-label pattern
  - assigned collection initializer

  With these flags, its output stays today's no-tab output, also when the target holds a tab. The claim that the node-in-document entry points keep their output is corrected: it holds except where FMT-PIPE-017's offset is zero (the open question). The changed `FormatNode` outputs now also list chain parts, accessors of one-line properties, list-element interiors, and the `else if` comment moved in front of the `if`. These are the 315 structural and 61 column-only differences the skeptic found. The test-suite simulation claim stays and is put next to the skeptic's corpus counts.
- **Why.** A reader of the ancestors that gets no fact decides as if the target stood alone. For `else if (y) // note`, the comment then moves in front of the `if`, and the entry point writes the document's leading trivia over it (FMT-PIPE-010), so the comment is lost.
- **Decisions.**
  - A new section lists the seven flags and their readers, and says how the list was derived.
  - It decides that what follows the root's last token needs no fact.
  - It decides that `FormatNode` and the context entry point get none of the flags.
  - It names the tab-equality check as the observable test that the list is complete.
  - A second new section records that the `else if` flag closes the comment loss without changing who owns the leading trivia (thoenissen/Reihitsu#1521). It does not claim that the flag closes the RH5405 loss.
- **Alternatives.** Five were added, with the maintainer's rejections: proceed and list the outputs; narrow the run to `FormatNode`; a generic list of ancestor kinds; the flags for `FormatNode`; making the entry point the owner of the leading trivia now.

## How the facts were derived

I read every access to `Parent`, `Ancestors`, `GetPreviousToken`/`GetNextToken`, `SyntaxTree`, and line spans in the phases that run on the root while it is still attached. That is every phase before `IndentationPhase`, which re-roots the node for itself (`rg` over `src/Reihitsu.Formatter/Pipeline` and the Core helpers they call: `FluentChain`, `AttributeTargetUtilities`, `ListElementInteriorUtilities`, `BlankLineSpacingPolicy`). An access that starts at a token or node inside the root and walks toward the root can leave the root only from the root itself or from its first or last token. I kept the decisions where that happens:

| Fact | Reader on `main` | What it reads outside the root |
|---|---|---|
| `else if` | `LineBreakContainedBlockRewriter.Visit` (`node.Parent is ElseClauseSyntax`) → `MoveTrailingConditionCommentToOwnLine` | whether the root `if` is an else clause's statement |
| Chain continuation | `ChainLineBreakRewriter.NormalizeIfOutermost` → `FluentChain.IsOutermostChainNode` → `ContinuesChain(node.Parent, node)` | whether the root's parent continues the chain (including `IsInvoked` and the conditional-access assignment case) |
| One-line property | `AttributeTargetFormattingRewriter.ShouldKeepAccessorListsSingleLine` (`accessor.Parent?.Parent is BasePropertyDeclarationSyntax`, `IsSingleLineExcludingAttributeLists`) | the owning property and its one-line shape, for an accessor or accessor-list root |
| List element | `ListElementInteriorJoinRewriter.FindInnermostListElement` and `ListElementInteriorUtilities.FindOwningConstruct` (both walk `Parent` past the root); `IsListElement` reads the parent of a `ParameterSyntax` or `TypeSyntax` root | the innermost list element around the root, and whether an owning construct stands between |
| Element-access arguments | `LineBreakListRewriter.CanSafelyCollapseBracketedArguments` (`originalNode.Parent is ElementAccessExpressionSyntax or ImplicitElementAccessSyntax`) | the owner of a bracketed-argument-list root |
| Case-label pattern | `LineBreakInitializerRewriter.IsIntroducedByCaseLabel` (walks `Parent` through `BinaryPatternSyntax`/`UnaryPatternSyntax` to `CasePatternSwitchLabelSyntax`) | the label that introduces a pattern root |
| Assigned collection initializer | `LineBreakInitializerRewriter.VisitInitializerExpression` (`originalParent is AssignmentExpressionSyntax`) | the parent of an initializer root |

Reads I judged to need no new fact:

- **The token in front of the root's first token.** The readers are `LineBreakTriviaUtilities.HasLeadingEndOfLine`, `TokenLocator.TryGetPreviousToken`, `TokenGapNormalizer`, the chain and list rewriters, `MemberLineStartRewriter`, `PropertyLayoutLineBreakRewriter`, `HorizontalSpacingRewriter.TrimGapBeforeToken`, and `LineBreakInitializerRewriter`. They use the token's kind and whether a line break separates it from the first token, which the existing preceding-token fact carries. FMT-PIPE-024 now says every phase answers that question from the fact.
- **The token behind the root's last token.** The readers are `ControlFlowBraceTransform.GetTokenBehindInsertedCloseBrace`/`GetTokenBehindIfStatement` (including the `else if` branch), `HorizontalSpacingRewriter.VisitToken`, and `CleanupPhase`. They decide only the last token's trailing trivia, which `FormatNodeInDocumentAsync` replaces with the document's and which `FormatNode` leaves to its caller. They decide as at the end of a file.
- **Within-root reads.** `AttributeTargetUtilities.TryResolveLayoutTarget` on an attribute-list root: the owner is not visited, attached or not. `ListElementInteriorJoinRewriter`'s `ParameterList { Parent: … }` check: it applies only to `(`, which as the root's first token is never joined. `LineBreakDetection`/`AccessorListLayout` positions, `SwitchCaseBraceRewriter`'s bound-name walk (stops at the section), and the `token.Parent` checks of the spacing rules all stay inside the root.
- **`FormatNodeInDocumentWithContextAsync`.** Its root is detached today, and the skeptic's corpus run found 0 changed outputs.

This list comes from reading code, not from a run with the facts implemented. The completeness check is the observable FMT-PIPE-023 now states: a target with a tab gets the output the same target with spaces gets. A reader the list misses shows up as a difference there, and would be a spec change mid-run (escalation 4).

## Changed requirements

- FMT-PIPE-023 — Node-level entry points format a detached root — changed:
  - new normative sentence: the output must not depend on whether a phase changes the root first, so a target with a tab gets the output the same target with spaces gets;
  - new `else if` example. On `main` the example gives this output without the tab and loses `// note` with the tab, which I checked.
- FMT-PIPE-024 — Node-level input facts are plain values — changed:
  - seven table rows, supplied by `FormatNodeInDocumentAsync` only;
  - the reader paragraph is now a list: every phase answers what stands in front of the root's first token from the preceding token, and one line-break item covers the seven flags;
  - new rule: what follows the last token is decided as at the end of a file;
  - Why extended with the comment-loss mechanism.
- FMT-PIPE-012 — `FormatNode` skips the file-level guards and lays out the node at its depth — Why wording only. I agree with the skeptic that it overstated the column: it now says that only a node the indentation phase places by depth lands where document formatting puts it, and that a node aligned to an anchor outside it does not (initializer member, statement in a lambda block passed as an argument, switch-expression arm). The normative text is unchanged.
- `pipeline.md` `Uses` gained SHARED-CHAIN-001.
- Requirement-ID ledger: no new IDs, so `sdd/specs/requirement-ids.txt` is unchanged; FMT-PIPE-023 and FMT-PIPE-024 are already there.
- FMT-CLEAN-008 and FMT-PIPE-010 are unchanged in this round.
- Architecture spec tests: every structure and consistency test passes. Only the coverage test fails, for FMT-PIPE-023 and FMT-PIPE-024, until their tests exist.

## Examples for the tests

- **FMT-PIPE-023:**
  - the `else if (y) // note` target with braces already in place, with and without a tab in the condition. The expected output is in the spec; on `main` the tab variant loses the comment;
  - the `else if (b) /* explanation */` variant;
  - the skeptic's candidates 4 to 6, each with and without a tab, where the expected output is today's no-tab output:
    - a chain-prefix target (`source⏎.Where(…)⏎.Select(…)` in front of `.ToList()`);
    - an accessor or accessor list with an accessor attribute in `public int Value { [field: System.Obsolete] get; set; }`;
    - `Use(new⏎Example())` with the object creation as target.
- **FMT-PIPE-024:** one test per flag, comparing the attached no-tab output with the tab output:
  - a bracketed argument list of an element access;
  - a recursive pattern under a `case` label, also through `not`;
  - a collection initializer on the right of an assignment;
  - a parameter as target, for the list-element case where the root itself is the element;
  - the existing local-declaration blank-line example.
- **FMT-PIPE-012:** unchanged from `01-spec-author.md`.

## Decisions needed

**Context.** Issue thoenissen/Reihitsu#1489, spec-author repair round after the skeptic. Answer B of `…-missing-facts.md` asks that the node-in-document outputs stay as they are. One class cannot stay as it is without keeping Core's depth rule.

FMT-PIPE-017 gives an offset of zero when the target's first token stands behind the last line of a multi-line comment. The token keeps its column, and the target's other lines take the absolute columns of its depth, so the depth reaches the output. Today that depth is `ComputeBaseIndentLevel`; after the change it is the indentation phase's depth (FMT-PIPE-012). Input (verified on `main` at `d686a76`), `FormatNodeInDocumentAsync` with the inner `if` as target:

```csharp
        if (a)
            /* note
            */ if (b)
        {
        X();
        }
```

Today `{`/`X();` land in columns 8/12, which is Core's count. The indentation phase's depth gives 12/16, the columns FMT-INDENT-004 gives an unbraced embedded statement. In the skeptic's corpus, the same mechanism moves a node inside an initializer one level left: the comment line goes from column 12 to 8, where document formatting gives 20. The anchor is outside the root, which FMT-PIPE-017 does not cover. Those 15 outputs are the only node-in-document outputs that change in the skeptic's corpus beyond the cases the seven flags answer.

**Question.** Which depth do the lines of a zero-offset node-in-document target take?

- **A** — The indentation phase's depth, as for every other root.
  - Example: the embedded `if` above gets `{` in column 12 and `X();` in 16; a node inside an initializer behind such a comment moves one level left.
  - Consequence: these outputs change and are listed in the change doc. The formatter has one depth rule, and Done-when is met. The initializer case stays as wrong as it is today, in another column.
- **B** — Core's ancestor count for the node-in-document entry points, so their outputs stay as they are.
  - Example: the outputs above stay at 8/12 and 12.
  - Consequence: the formatter keeps calling `ComputeBaseIndentLevel`. That contradicts Done-when and the accepted FMT-PIPE-012 decision. The embedded `if` keeps a column FMT-INDENT-004 does not give.
- **C** — Change FMT-PIPE-017 so that a zero-offset target is shifted by another anchor.
  - Example: the shift is taken from the document column of the comment's first line.
  - Consequence: a new behavior change to FMT-PIPE-017 with outputs not yet probed. This is beyond the issue's scope (escalation 5).

**Recommendation: A** — it is what the accepted depth rule already implies. It fixes the embedded case. It leaves the initializer case in a gap that neither depth closes. B would keep the second depth rule the issue exists to remove.

## Open questions for the maintainer

1. **A target that does not start its line takes the brace columns of its own first token.** `FormatNodeInDocumentAsync` with the `if` of `else if (y) // note` as target, its body `{`/`}` already in column 8, writes the braces in column 13, under `if`, on `main` today without a tab. Document formatting keeps them in column 8. This follows FMT-PIPE-017 as written: lines placed by depth keep their place relative to the first token, which keeps its column. But the result is a layout that the next document formatting moves back.
   - Requirement IDs: FMT-PIPE-017, FMT-PIPE-010, FMT-INDENT-002.
   - Readings: A — intended; it belongs to the ownership question of thoenissen/Reihitsu#1521. B — a target that does not start its line should lay out its own later lines at their document depth.
   - Recommendation: A for this run. Add the case to the widened text of thoenissen/Reihitsu#1521, because it is the same two-owner question about where a non-line-start target's lines start.

## Architecture findings

None new. The two-owner finding on a target's leading trivia was answered (`…-leading-trivia-owner.md`, A). The facts in this round do not change that ownership.
