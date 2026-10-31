Role: skeptic
Commit: 9b73683
Verdict: question for the maintainer

The prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing was ignored.

## Premise

**The issue's two examples are real; the change doc's claim about the node-in-document entry points is not.** The change doc (*What*) says "The outputs that change are those of `FormatNode`; the node-in-document entry points keep their output", and the maintainer approved it on that basis (`squad/decisions/2026-10-31-node-root-detached-change-doc.md`). That claim rests on the formatter and analyzer test suites and on blank-line probes. It does not hold for the phases that read across the root boundary through `Parent`, `Ancestors`, or `GetPreviousToken`/`GetNextToken` outside the five blank-line readers, the documentation phase, and the comment contributor that FMT-PIPE-024 lists.

How it was established:

- Two copies of the tree at `cba39e1` were extracted into the scratchpad. One stayed as it was. In the other, `ReihitsuFormatter` gave every node-level root to the pipeline through `CSharpSyntaxTree.Create(node, options).GetRoot()` and computed the depth by walking `SyntaxIndentationUtilities.GetChildIndentLevel` from the node up to the root. That is the spec author's simulation, rebuilt independently. The facts were still captured from the attached target.
- A probe referenced each copy's `Reihitsu.Formatter` and ran `FormatNode`, `FormatNodeInDocumentAsync`, and `FormatNodeInDocumentWithContextAsync` (with the parent as context). The corpus was the 10 826 distinct C# raw-string inputs in `src/Reihitsu.Formatter.Test` and `src/Reihitsu.Analyzer.Test`. Inputs with syntax errors were skipped, and up to 25 candidate nodes per input were sampled: statements, members, switch sections, accessors and accessor lists, `else` clauses, argument lists and arguments, lambdas, initializers, attribute lists, invocations, assignments, switch-expression arms, anonymous-object members, conditionals, binaries, object creations, and collection expressions. That gave 80 726 runs of each of `FormatNode` and `FormatNodeInDocumentAsync`, and 67 633 runs of the context entry point. Outputs of the two copies were compared byte for byte.
- Results:
  - `FormatNodeInDocumentWithContextAsync`: 0 differences, which confirms its root is already detached.
  - `FormatNodeInDocumentAsync`: **334 differences**.
  - `FormatNode`: 11 542 differences.
    - 11 166 differ only in leading line breaks or in a uniform depth shift. These are the expected changes.
    - 315 differ in line structure.
    - 61 differ only in columns.
- Every `FormatNodeInDocumentAsync` difference that was checked reproduces on unmodified code (`9b73683`) when a tab is put inside the target, because the tab replacement then detaches the root. So the new outputs are today's detached-path outputs, and the change makes that path the only one. Checked cases:
  - `Use(new⏎Example(⇥));`
  - `"hello".Trim(⇥)` in a chain
  - `get;⇥set;` with an accessor attribute
  - `else if (y⇥) // note`

Observed `FormatNodeInDocumentAsync` differences, as old output followed by new output (the full classes are under *Breaking inputs*):

```csharp
// Comment dropped. The target is the `if` of `else if`; the same happens with `/* explanation */`.
        else if (y) // note        // old
        else if (y)                // new: the comment is gone from the document
             {
             }

// Target `new⏎Example()` inside an argument list. The join of the wrapped list element is not applied.
        Use(new Example());        // old
        Use(new                    // new
            Example());

// Target: the accessor list, or an accessor, of a property with an accessor attribute.
    public int Value { [field: System.Obsolete] get; set; }      // old
    public int Value { [field: System.Obsolete]                  // new
                         get; set; }

// Target: the prefix `source.Where(…).Select(…)` of a longer chain.
        var x = source             // old
                .Where(i => i > 0)
                .Select(i => i * 2)
            .ToList();
        var x = source.Where(i => i > 0)   // new
                      .Select(i => i * 2)
            .ToList();
```

The comment drop also exists on `main` without a tab, through a code fix. `scripts/apply-fix.sh RH5405` on the following input removes `// note` under both LF and CRLF. The brace transform changes the root first, so the root is already detached, and it places the braces in column 13:

```csharp
        else if (y) // note
            Foo();
```

`FormatNode` on the same `if` with a tab shows where the comment goes: the phases move it in front of the root (`// note` above `if (y)`). `FormatNodeInDocumentAsync` then writes the document's leading trivia back over it, because that target does not start its line (FMT-PIPE-010).

The issue's own statements were checked by the spec author against `main`. I re-ran both issue examples in the simulation and they give the desired outputs. The issue is real for `FormatNode` as it states. Its inventory of cross-boundary readers (`PrecedingTokenFacts.Resolve` in five places, `IsWrittenBehindRootPrecedingToken`, `CommentIndentationContributor`, and the two list-position readers) is incomplete, and the spec's fact table copies that inventory.

## Mechanism and candidates

Mechanism: a phase decision about the root, its first token, or a construct that contains the root reads the root's ancestors or neighbours. While the root is attached, the read reaches the document. Once the root is detached, it finds no parent and no previous token, so the phase decides as if the root were a complete construct of its own that starts its line. FMT-PIPE-024 names no fact that would carry what the read found.

Dispatch: reads of `Parent`, `Ancestors`, and `GetPreviousToken`/`GetNextToken` in the phases that run before or without re-rooting. Those phases are the structural transforms, the line-break rewriters (`LineBreakPhase.CreateRewriters`), and the horizontal-spacing rules. The readers that the corpus differences lead to, located with `rg`:

- `ListElementInteriorJoinRewriter`: a `token.Parent` walk up to the list element (line 114). This covers the joins inside argument, parameter, attribute, and type-argument list elements: `new`/type, `static`/lambda, an attribute target's `:`, and a lambda in a default parameter value.
- `ChainLineBreakRewriter` together with `TokenLocator`/`TokenGapNormalizer` previous-token reads: chain layout when the root is a prefix or a member-binding part of a longer chain, including an assignment to a chain member (`a?.B⏎.C = 5`).
- `AttributeTargetFormattingRewriter`: `accessorDeclaration.Parent?.Parent is BasePropertyDeclarationSyntax` (line 59), which keeps the accessor list single-line.
- `LineBreakContainedBlockRewriter`: `isElseIfBranch = node.Parent is ElseClauseSyntax` (line 262). This is the `else if` branch, whose trailing comment moves in front of the root.
- `ControlFlowBraceTransform` (`ifStatement.Parent` / `body.Parent`, lines 113 and 132) and `LineBreakInitializerRewriter` (`node.Parent` at 177, case-pattern ancestors at 110–115) read across the boundary too. No corpus difference was traced to them; their readings were not settled.
- The indentation phase: when the first token stands behind the last line of a multi-line comment, FMT-PIPE-017 gives an offset of zero. The depth then reaches the node-in-document output, which the change doc says it never does.

## Breaking inputs

Failure classes that reach the change:

- FC-028: `FormatNode` and `FormatNodeInDocumentAsync` disagree. The change removes some of these disagreements and adds others.
- FC-027: another enclosing scope (argument, chain, accessor list, `else` clause, initializer).
- FC-021: a comment on the header line of a rewritten construct, or a `/* */` ending mid-line.
- FC-004: a comment is dropped.
- FC-016: sibling shapes.
- FC-015: Core's `ComputeBaseIndentLevel` stays as a second depth rule; see *Follow-up candidates*.
- FC-031: Core documentation comments describe formatter callers that go away.
- FC-032: formatter output feeds every code fix.

Class 1, a cross-boundary decision without a fact (`FormatNodeInDocumentAsync`). Corpus counts, old output followed by new:

| Enclosing context the decision reads | Target kinds seen | Count | Result |
|---|---|---|---|
| Longer member-access or conditional-access chain | Inner invocation, assignment to a chain member | 186 + 4 | Prefix laid out as a chain of its own: links joined or split differently, alignment that does not match the rest of the chain |
| Property whose accessor list stays single-line | Accessor, accessor list | 65 + 52 | Accessor attributes split onto their own lines, `get;` in an odd column |
| Argument, parameter, or attribute list element | Object creation, lambda (`static` modifier, default parameter value), attribute list | 6 + 3 + 1 | The join of the wrapped list element is not applied, or a line break is added inside the element |
| `else` clause owning an `if` | `if` statement with a comment after its condition | 2 | **Comment dropped** (`//` and `/* */`) |
| Multi-line comment ending on the first token's line, depth differing | Initializer (`/* a⏎ b */ {` inside a dictionary initializer), binary expression inside a collection initializer | 14 + 1 | Comment line moves from column 12 to 8. Document formatting puts it in column 20, so both outputs are off, the new one further |

Class 2, the depth reaches the node-in-document output through a zero offset (FMT-PIPE-017's multi-line-comment clause). The members span two axes:

- the source of the depth difference: an unbraced embedded statement (+1), an initializer (−1), or an anonymous object (−1);
- whether the root holds lines that the depth places.

| Member | Result |
|---|---|
| Embedded `if` behind `/* note⏎ */`, as the target of `if (a)` | Fails the "unchanged" claim. `{` and `X();` move from columns 8/12 to 12/16. The new columns match FMT-INDENT-004 |
| Initializer | Fails. Column 12 becomes 8, against 20 in document formatting |
| Anonymous object | Same depth difference as the initializer; not probed |
| Lambda block and object creation inside an initializer behind such a comment | Holds: the alignment contributors anchor those lines |

`FormatNode` classes: the same five contexts produce the 315 structural differences. Counts include invocation 185, accessor 65, accessor list 52, assignment 4, lambda 3, object creation 2, `if` 2, argument 1, attribute list 1. The 61 column-only differences are the multi-line-comment initializer and assignment members. FMT-PIPE-012 makes these outputs the intended ones ("a node that is part of a tree gets the result that the same node without a parent gets"). The change doc does not list them. No `FormatNode` output loses a comment. No `PropertyDeclarationSyntax` output changes beyond leading lines and depth, so RH5401's `CanCollapse` keeps its answer on the corpus.

Candidate inputs the test implementer must cover, whatever the maintainer decides:

1. The issue's two examples, with and without the tab.
2. The `else if (y) // note` and `else if (b) /* explanation */` targets of `FormatNodeInDocumentAsync`, which must keep the comment.
3. RH5405 on the `else if … // note` input above.
4. A chain-prefix target.
5. An accessor-list target with an accessor attribute.
6. `Use(new⏎Example())` with the object creation as target.
7. The embedded `if` behind a multi-line comment as target.
8. The initializer behind `/* a⏎ b */`.

Each runs under LF and CRLF and is checked for a second-pass no-op (FC-010).

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `ReihitsuFormatter` node-level entry points | Must change (detach, facts, depth) |
| `FormattingContext`, `PrecedingTokenFacts`, `RootListPosition` | Must change (plain values) |
| Blank-line phase readers, documentation phase, `CommentIndentationContributor`, the two list-position readers | Must change (read facts only) |
| Line-break phase readers listed above, `ControlFlowBraceTransform`, `LineBreakInitializerRewriter` | Must change under FMT-PIPE-023 as written: they read ancestors or neighbours, and the spec gives them no fact. Not named by the issue, the change doc, or FMT-PIPE-024 |
| Indentation phase (`LayoutComputer` depth, zero-offset shift) | Must stay, except for the depth source; its zero-offset clause is where the depth leaks |
| `Reihitsu.Core` `SyntaxIndentationUtilities` | Must stay (`ComputeBaseIndentLevel` keeps its code-fix caller). The documentation comments of `IsIndentingAncestor` (around line 215, "its only uncancelled consumer … `ReihitsuFormatter.FormatNode` and the document-scoped overloads") and of `ComputeStatementIndentLevel` describe formatter use that goes away (FC-031). `ComputeStatementIndentLevel` already walks `GetChildIndentLevel`; only its parameter type is `StatementSyntax` |
| Code fixes | Not changed. Every fix that formats a target that does not start its line, or that lies inside a chain, argument, or accessor list, gets the changed output: RH5405, RH5406, RH5407, RH5101, RH5102, RH5111, RH5205, RH5302, RH5301, RH5303–RH5308, `FluentChainCodeFixHelper`. The spec author's simulation kept all 6312 analyzer tests green, so none of their tests pins these shapes |
| Rule pages | Not involved |
| `FormatDocumentAsync` / `FormatSyntaxTree`, CLI | Not involved |

## Simpler option

The narrowest change that delivers the issue's two desired outputs gives only `FormatNode` the detached root and the indentation-phase depth, and leaves `FormatNodeInDocumentAsync` attached-until-changed. It does not meet *Done when* ("no phase reads … across a node-level root's boundary, and no context field holds a syntax node or trivia list"), and it keeps the tab-dependent output of `FormatNodeInDocumentAsync`. It is option C below, not a recommendation.

## Questions for the maintainer

**Context.** Issue thoenissen/Reihitsu#1489, skeptic stage, after the change doc was approved. The approval rested on "the node-in-document entry points keep their output". A simulation of the approved behavior over 80 726 `FormatNodeInDocumentAsync` runs changes 334 outputs. All of them are cases where a line-break decision today reads the target's ancestors or neighbours: chain prefixes, accessor attributes, joins inside argument and attribute list elements, `else if`, and a depth leak behind a multi-line comment. FMT-PIPE-024 lists no fact for any of them. With the root detached, the phases decide as if the target stood alone. One class drops a user comment:

```csharp
        else if (y) // note      // input; FormatNodeInDocumentAsync with the `if` as target
        else if (y)              // output after the change: `// note` is gone
```

Each case already happens on `main` when the target holds a tab. The comment drop also happens on `main` through the RH5405 code fix.

**Question.** How should the run treat the node-in-document outputs that change?

- **A.** Proceed as approved, and amend the change doc to list these outputs as changed.
  - Example: `Use(new⏎Example())` stays split, and accessor attributes are split onto their own lines.
  - Consequence: the comment drop becomes reachable for every `else if` target with a trailing comment, which SHARED-FIX-003 and the formatter's non-destructive rule forbid. Layout regressions follow in every code fix that formats such targets.
- **B.** Amend the change doc so that FMT-PIPE-024 also carries facts for these decisions (escalation 5, scope grows). The facts would say:
  - that the root lies inside an argument, parameter, attribute, or type-argument list element;
  - that it is part of a longer chain;
  - that its property keeps its accessor list on one line;
  - that it is the `if` of an `else` clause;
  - and something for the zero-offset depth.

  Consequence: the node-in-document outputs keep today's no-tab output, the tab-dependent difference disappears in the right direction, and the RH5405 comment drop is likely closed with it. More production paths are involved, and each new fact needs its own requirement text.
- **C.** Narrow the run to `FormatNode` (detached root, depth rule, no preceding-token or list-position facts), and leave `FormatNodeInDocumentAsync` attached until the missing facts are specified in a follow-up.
  - Example: the issue's two inputs give the desired output; `else if (y) // note` keeps its comment.
  - Consequence: *Done when* is not met for `FormatNodeInDocumentAsync`, the plain-value facts stay half done, and the issue stays open (`Refs #1489`).
- **D.** Stop the run, and file the detached-path defects as issues to fix first:
  - the comment drop (with RH5405);
  - list-element joins;
  - chain prefixes;
  - accessor attributes;
  - the zero-offset depth leak.

  Then resume this run unchanged. Consequence: the issue waits, and the follow-up issues need the facts B describes anyway.

**Recommendation: B.** The issue's design already decides that everything a phase needs from the document comes in as facts, and these decisions are what the inventory missed. A or C either ships a reachable comment drop or leaves the attached path in place. If the added scope is too large for this package, D is the next choice, because it keeps the detached-path defects from becoming the default before they are fixed.

## Architecture findings

**Two components own the leading trivia of a `FormatNodeInDocumentAsync` target that does not start its line.**

- The components: the phases, which move content into the root's first-token leading trivia, and the entry point, which writes the document's leading trivia back over that result (FMT-PIPE-010; `ReihitsuFormatter.FormatNodeInDocumentAsync`, the `keepsLeadingTrivia` branch).
- The decision: what stands in front of the target's first token.
- Whatever the phases put there is discarded. Members, settled:
  - A line break in front of the first token. The body `{` of `void M() { Foo(1, 2); }` stays on the declaration's line. The spec author found this, and the maintainer answered 2B: thoenissen/Reihitsu#1521, `squad/decisions/2026-10-31-node-root-detached-block-target-brace.md`.
  - A comment that the phases relocate from inside the target to its front. The `if` of `else if (y) // note` or `else if (b) /* explanation */` loses the comment. This happens on `main` through RH5405 and with a tab; after this change it happens for every such target.
  - Blank lines that the phases add in front of the target are discarded too. That is harmless and matches FMT-PIPE-010.
- `FormatNodeInDocumentWithContextAsync` has the same write-back (FMT-PIPE-018). No member reached it in the corpus: there the `else if` stays attached inside its context.
- This finding reaches the run because the change makes the relocating path, the detached root, the default for every target. Escalation 12 applies.

## Follow-up candidates

- The RH5405 code fix drops `// note` and places the braces in column 13 on `else if (y) // note⏎    Foo();`. This happens on `main`, independently of the change (escalation 9 if the maintainer treats it apart from option B).
- `FormatNodeInDocumentAsync` inserts a blank line between `if (a)` and an embedded-statement target that carries a leading comment (`// note` or `/* note */` on its own line). Old and new give the same output, so the change does not affect it. FMT-PIPE-024 leaves that blank-line decision unspecified (change doc, *Out of scope*).
- FMT-PIPE-012's *Why* says the depth rule "lands [the node] in the column document-level formatting gives it". That holds only where the indentation phase places the node by depth. It does not hold for anchored positions other than initializers: a statement in a lambda block passed as an argument gets column 12 against 16 in document formatting, and a switch-expression arm gets column 8 against 20. Today's rule gives the same columns. This is a wording correction for the spec author, not a different reading.
- FMT-CLEAN-008 now also covers the roots of the two node-in-document entry points. Those roots' trailing trivia is always restored (FMT-PIPE-010), so tests can observe the requirement only through `FormatNode`.
- `SyntaxIndentationUtilities.ComputeBaseIndentLevel` stays as a second depth rule for `TargetAttributePlacementCodeFixProviderBase` (FC-015). Its structural cause is the one the issue removes from the formatter. Whether that code fix misplaces an attribute inside an unbraced embedded statement was not checked.
