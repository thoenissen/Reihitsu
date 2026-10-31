# Node-level roots are detached, and their input facts are plain values

| | |
|---|---|
| Status | Accepted |
| Date | 2026-10 |
| Affected requirements | FMT-PIPE-023 (new), FMT-PIPE-024 (new), FMT-PIPE-010 (reference to the facts), FMT-PIPE-012 (depth rule, title), FMT-CLEAN-008 (every node-level root, title) |
| Depends on | [Every formatter phase returns a tree that round-trips and keeps foreign annotations](phase-tree-round-trip.md) |

Amendment: the maintainer accepted this change doc on 2026-10-31. The skeptic then showed that its claim "the node-in-document entry points keep their output" did not hold: decisions of the line-break phase read the target's ancestors, and a detached root gives them no answer. The maintainer decided to add plain-value facts for those decisions, to keep the owner of a target's leading trivia as it is and close only the `else if` comment loss through those facts, to leave the comment loss of the RH5405 fix on `main` to a follow-up (thoenissen/Reihitsu#1527), and to give a target behind a multi-line comment the indentation phase's depth. The passages marked *(amended 2026-10-31, skeptic stage)* carry that amendment, which the maintainer accepted on 2026-10-31. The passages marked *(amended 2026-10-31, implementer stage)* correct the list of changed outputs to what the implementation measured, align the reading of one fact with the implementation, and name a case left out of scope. The passages marked *(amended 2026-10-31, audit stage)* give the outermost-chain rule one owner, which answers for every node of the root from two chain facts, and record what that changes. The passages marked *(amended 2026-10-31, process stage)* apply the rule that a component has to work only for the inputs the repository's own callers pass (`AGENTS.md`, *Supported inputs*): they drop the follow-ups this doc promised for cases no caller reaches and add the visible output changes. Everything not marked is as accepted.

## What

`FormatNode`, `FormatNodeInDocumentAsync`, and `FormatNodeInDocumentWithContextAsync` give the pipeline a root without a parent (FMT-PIPE-023). Today `FormatNode` and `FormatNodeInDocumentAsync` pass the node itself; it stays part of its document until the first phase that changes it, and the indentation phase detaches it for itself and the phases after it. `FormatNodeInDocumentWithContextAsync` already passes a detached root: the copy of the context that carries the marked target.

Everything a phase needs from the document around a node-level root comes in as input facts that are plain values — kinds, flags, counts, a column, and the kinds and texts of trivia — and no fact holds a node, token, or trivia of the document (FMT-PIPE-024). FMT-PIPE-024 lists which entry point supplies which fact. No phase reads the document beyond its root and those facts.

*(amended 2026-10-31, skeptic stage)* Besides the facts about the code in front of the root, `FormatNodeInDocumentAsync` supplies seven flags about the construct the target stands in, one for each decision of the structural-transform and line-break phases that reads the target's ancestors today: whether the target is the `if` of an `else` clause, whether its parent continues its member-access chain, whether it is an accessor or accessor list of a property that stands on one line, whether it is or lies in an element of an argument-like list whose interior wraps are joined, whether it is the bracketed argument list of an element access, whether a `case` label introduces it as a pattern, and whether it is a collection initializer assigned to a member. With them, `FormatNodeInDocumentAsync` gives a target the output it gives it today while the target holds nothing an earlier phase changes, and it gives that output also when the target holds a tab (FMT-PIPE-023). What follows the target's last token needs no fact: the entry point writes back the document's trailing trivia there (FMT-PIPE-010).

The depth of a root that `FormatNode` lays out without an explicit `indentLevel` is the depth the indentation phase gives the node's first token at its position (FMT-INDENT-002 to FMT-INDENT-004), not the ancestor count of `SyntaxIndentationUtilities.ComputeBaseIndentLevel` (FMT-PIPE-012). The formatter no longer calls `ComputeBaseIndentLevel`; the two node-in-document entry points take the same depth rule for their target or context.

FMT-CLEAN-008 states what already happens: no node-level root ends with a line break, because the indentation phase detaches every root before the cleanup phase runs.

The outputs that change are those of `FormatNode`, and *(amended 2026-10-31, implementer stage)* the outputs of `FormatNodeInDocumentAsync` for the targets listed last: a target whose first token stands behind the last line of a multi-line comment, and a target that a phase before the line-break phase changes. Every other node-in-document output stays as it is:

- `FormatNode` on a node that is part of a tree no longer reads the token in front of it. The leading blank lines of the node are collapsed, and a documentation comment written behind that token no longer gets the line break in front of it that moves it off that token's line, as already happens when an earlier phase detaches the node. `Call(1, 2);` behind `var a = 1;` and one blank line gives `        Call(1, 2);` instead of a leading line break followed by that line; the property `Other` whose documentation comment stands behind the `}` of the property above it gives `    /// <summary>` … `    public int Other { get; set; }` without the leading line break the test `ReihitsuFormatterTests.FormatNodeWithPreviousTokenOutsideTargetPreservesTrailingDocumentation` expects today. The same holds for a statement behind a `case` label and a blank line, and for a member behind `#region` and blank lines.
- `FormatNode` on an unbraced embedded statement places it one level deeper than its owner: `Call(1,` in the body of `if (a)` starts in column 12, where it starts in column 8 today.
- `FormatNode` on a node inside an object or collection initializer or an anonymous object adds no level for that initializer: `P = Call(1,` inside `new D { … }` in a method body starts in column 8, where it starts in column 12 today. Document-level formatting aligns it to the initializer's brace (FMT-INDENT-022), which a depth cannot express.
- *(amended 2026-10-31, skeptic stage)* `FormatNode` on a node whose layout today depends on the construct around it gets the layout of the same node standing alone, as it already does when the node holds a tab: an inner part of a longer member-access chain, such as `source.Where(…).Select(…)` in front of `.ToList()`, is laid out as a chain of its own; an accessor or accessor list of a one-line property puts each accessor attribute on its own line; a node inside an argument, parameter, or attribute list element keeps a wrap inside it that the list-element join removes in a document, such as the line break in `new⏎Example()`; and the `if` of `else if (y) // note` gets the trailing comment moved in front of it, so the result starts with `// note` on its own line above `if (y)`. No comment is lost: the caller decides where the node goes.
- *(amended 2026-10-31, skeptic stage)* `FormatNodeInDocumentAsync` on a target whose first token stands behind the last line of a multi-line comment, which the indentation phase does not move (FMT-PIPE-017), places the target's other lines at the indentation phase's depth instead of Core's ancestor count, because nothing shifts them there. An `if` that is the unbraced body of `if (a)` and starts behind `*/` gets its braces and body one level deeper than today, in the columns FMT-INDENT-004 gives them; a node inside an initializer, one level shallower. The maintainer chose this output over keeping Core's count for the case.
- *(amended 2026-10-31, implementer stage)* `FormatNodeInDocumentAsync` on a target that a phase before the line-break phase changes — the blank-line phase, or an earlier rewriter of the line-break phase — now gets the flags' answer where it got none. On `main`, such a target is already detached when the line-break phase reads its ancestors, so it is laid out as a construct of its own; the flags are captured before the pipeline runs, so it now gets the output it gets when no earlier phase changes it. Two kinds of target change:
  - An inner part of a longer member-access chain keeps its layout instead of being laid out as a chain of its own. Neither output is the one document-level formatting gives, which lays out the whole outer chain; this case is out of scope (see *Out of scope*). *(amended 2026-10-31, audit stage)* The chain alignment of the indentation phase now asks the same rule, so it no longer aligns such a target as a chain of its own either.
  - An assigned collection initializer gets the blank line in front of a comment that leads its closing brace that document-level formatting gives it. With `Values = {`, `1,`, `2`, and `/* Keep close. */ }` on lines of their own inside an object initializer, as target, `main` writes no blank line between `2` and `/* Keep close. */ }`; the change writes one, as document-level formatting does. Where the input holds that blank line, `main` removes it and the change keeps it.

  Run over the C# inputs of the formatter and analyzer test projects with up to 25 targets each, 88 of 66 351 `FormatNodeInDocumentAsync` outputs differ from `main`: 6 of the zero-offset case above, 80 inner chain parts, and 2 assigned collection initializers. No analyzer or code-fix test changes; a node-in-document output reaches a user only through a code fix, and no code fix formats an inner chain part.

A simulation that detached every node-level root and applied the depth rule, run against the formatter and analyzer test projects at `7586822`, changed one expectation, the documentation test named above; every other test of the two projects — the code-fix tests included — kept its expectation. *(amended 2026-10-31, skeptic stage)* The test suites do not pin the decisions that read the target's ancestors: run over every C# input of the formatter and analyzer test projects with up to 25 targets each, the same simulation changed 334 of 80 726 `FormatNodeInDocumentAsync` outputs and 11 542 of 80 726 `FormatNode` outputs. The 334, and the `FormatNode` outputs other than leading line breaks and depth, come from the decisions the seven flags above now answer, and from the comment case of FMT-PIPE-017's zero offset. Each of them is today's output for the same target with a tab in it.

## Why

A node that is still part of its document has two answers to every read across its boundary, `GetPreviousToken` and `GetNextToken` included: one while no phase has changed it, another once a phase returned a changed copy. Which answer a phase gets depends on whether an earlier phase changed something, so `Call(1, 2);` and the same statement with a tab after the comma, which the tab replacement detaches, are formatted differently. The tab replacement and the line-ending normalization keep the attachment on purpose and the indentation phase removes it, so the boundary moves within one run.

The facts that replace those reads — the token in front of the root, the root's place in its statement list, the documentation boundary, the depth, and *(amended 2026-10-31, skeptic stage)* the construct the root stands in — are not named by any requirement, and two of them hold live nodes and trivia of the document under the rule "only kinds and last tokens may be read, never their positions", which nothing enforces. Facts made of plain values can be read only as values.

`ComputeBaseIndentLevel` is a second depth rule. It omits the level of an unbraced embedded statement and adds levels for initializers, while the indentation phase counts depth with `GetChildIndentLevel`; Core calls the difference deliberately inexact. The shift of FMT-PIPE-017 cancels the difference for the node-in-document entry points, so it reaches the output through `FormatNode`, as the embedded-statement output above shows, and *(amended 2026-10-31, skeptic stage)* through the node-in-document entry points where FMT-PIPE-017's offset is zero. One depth rule is one owner of the decision (FMT-PIPE-001).

*(amended 2026-10-31, skeptic stage)* A reader of the target's ancestors that gets no fact decides as if the target were a construct of its own. For the `if` of `else if (y) // note`, the line-break phase then moves the trailing comment in front of the `if`, and `FormatNodeInDocumentAsync` writes the document's leading trivia back over it, because that target does not start its line (FMT-PIPE-010): the comment is gone. Facts for those readers keep the target's layout as the document has it and keep the comment where it is.

## Visible output changes

*(amended 2026-10-31, process stage)* A node-level output is visible only where a caller of the repository passes its input (`AGENTS.md`, *Supported inputs*): the code fixes for `FormatNodeInDocumentAsync`, `FormatNodeInDocumentWithContextAsync`, and `FormatNode`. The CLI and whole-document formatting do not call the node-level entry points, and this change does not change what `FormatDocumentAsync` and `FormatSyntaxTree` do. Of the outputs listed under *What*, two are reached through a code fix.

The RH5204 fix on an unbraced embedded `if` that starts behind the last line of a multi-line comment. The fix formats that `if`, whose first token keeps its column (FMT-PIPE-017), and its braces and body take the indentation phase's depth, as the maintainer decided:

```csharp
// input
class C
{
    void M(bool a, bool b)
    {
        while (a)
            /* note
            */ if (b)
        {
        X();
        }
    }

    void X()
    {
    }
}
```

```csharp
// before: the fix does not converge; after two iterations
        while (a)

        /* note
            */ if (b)
        {
            X();
        }
```

```csharp
// after: the fix converges in one iteration
        while (a)

            /* note
            */ if (b)
            {
                X();
            }
```

The RH5405 fix on an `else if` whose condition carries a trailing comment. The fix formats the `if`, and the `else if` fact keeps the comment where it stands. The braces in column 13 are the same before and after (thoenissen/Reihitsu#1521):

```csharp
// input
class C
{
    void M(bool x, bool y)
    {
        if (x)
        {
            Foo();
        }
        else if (y) // note
            Foo();
    }

    void Foo()
    {
    }
}
```

```csharp
// before: the comment is lost
        else if (y)
             {
                 Foo();
             }
```

```csharp
// after: the comment stays
        else if (y) // note
             {
                 Foo();
             }
```

No other listed output is reached:

- `FormatNode`: its one caller, the RH5401 fix, passes a property declaration and reads only whether the result is one line. A property declaration is never an embedded statement or an initializer member, and none of the construct-dependent classes applies to it.
- The inner chain part: the chain code fixes pass the outermost chain node.
- The assigned collection initializer as target: the initializer fixes (RH5301, RH5303, RH5304) pass the object creation or the statement, equals-value clause, or arrow clause that holds it, so the initializer is never the root. The RH5204 fix reports no line of the probed initializer.
- The node inside an initializer behind a multi-line comment: no code fix was found that passes such a target.

## Decisions

### Every node-level root is detached

The three entry points pass the node they format as the root of a tree of its own (FMT-PIPE-023), as the indentation phase already does for itself. Detaching is what the first changing phase does today, so the outputs that change are the ones where no phase before the blank-line phase changed the root; with a tab in the root, today's output is already the new one.

The tab replacement and the line-ending normalization still return their input when it holds nothing to change; with a detached input, that no longer keeps anything attached, and their comments say so. Whether a phase returns its input or a copy is no longer visible to the phases after it.

### The facts, as plain values

FMT-PIPE-024 lists the facts and the entry points that supply them. Besides the line ending (FMT-PIPE-007), the language version (FMT-PIPE-008), and the transforms the node-level entry points skip (FMT-PIPE-011), which are specified already:

- the depth of the root, a number of levels;
- the column of the root's first token in its document (FMT-PIPE-017), a number or none;
- whether the root is something other than a compilation unit, a flag that keeps one line break in front of documentation that leads the root;
- the token in front of the root in its document: its kind, whether it is the colon of a switch label, and the kind and text of each of its trailing trivia in order — none when the root starts its file;
- the root's place in its statement or switch-section list: the kind of the statement in front of it and the kind of that statement's last token, whether the switch section in front of it ends in a `break` statement, and whether the root is the terminal `break` directly inside its switch section.

The kinds and texts of the trailing trivia are what the readers of today's trivia list use: whether the token ends its line, whether a comment trails it, and where a line starts behind it. A trivia list from the document would carry positions in a tree the root no longer belongs to.

The list position keeps its block-statement prediction: a block statement gets a position only when an own-line comment is the last content in front of its opening brace, because the line-break phase decides the gap above that brace in a way the list-level rules cannot reproduce. The prediction goes when the blank-line phase is the only owner of blank lines (thoenissen/Reihitsu#1496). It decides at capture time whether a position is given; the position itself is plain values.

### The construct the target stands in *(amended 2026-10-31, skeptic stage)*

Seven decisions of the structural-transform and line-break phases read the target's ancestors, and each gets one flag, captured from the document before the pipeline runs:

- **`else if`** — whether the target is the `if` statement of an `else` clause. The line-break phase leaves a comment behind the condition of an `else if` where it is; for any other `if`, it moves such a comment onto its own line in front of the `if`, which for an `else if` would split `else` and `if` across two lines.
- **Chain continuation** — whether the target's parent continues the target's member-access chain (SHARED-CHAIN-001), so that the target is not the outermost node of its chain. The chain rewriter lays out only the outermost node of a chain; an inner part keeps its layout. *(amended 2026-10-31, audit stage)* One flag about the root was not enough: the rule also reads two levels up for the left side of an assignment that is the part of a conditional access behind its `?`, so the chain of `.C` in `a?.B⏎.C = 5` is decided by whether that assignment is such a branch. With the assignment as root, that read leaves the root, and the RH6023 fix, which formats the assignment, laid the chain out differently from `main`. A second flag, **conditional-access branch**, answers it. The outermost-chain rule has one owner, `FluentChain` in Core. It takes both flags as one boundary value for the root, answers for every node of the root, not only the root itself, and the line-break phase and the chain alignment of the indentation phase both ask it. A second statement of the rule in the formatter, next to the flag, was rejected: it would copy the rule's assignment arm.
- **One-line property** — whether the target is an accessor or an accessor list of a property, indexer, or event declaration that stands on one line apart from its attribute lists. The attribute-list placement then keeps an accessor's attribute lists on the property's line. *(amended 2026-10-31, implementer stage)* It reads the flag only while the target itself holds no line break, in its text or its leading trivia: once an earlier rewrite has spread the target over several lines, the property no longer stands on one line, and document-level formatting puts the attribute lists on their own lines, as `main` does.
- **List element** — whether the target is, or lies inside, an element of an argument, attribute-argument, parameter, type-parameter, function-pointer-parameter, tuple, attribute, or type-argument list, with no construct between the target and that element that lays out its own lines (a brace scope, an initializer, an anonymous object, a switch expression, a collection expression, a recursive, list, or parenthesized pattern, an accessor list, an interpolation hole, or a query body). The list-element join then joins the interior wraps of the target as it joins them in a document.
- **Element-access arguments** — whether the target is the bracketed argument list of an element access or of an implicit element access, the lists whose arguments the line-break phase may collapse onto one line.
- **Case-label pattern** — whether a `case` label introduces the target as its pattern, directly or through `and`, `or`, and `not` patterns. A recursive pattern a case label introduces keeps its opening brace on the label's line.
- **Assigned collection initializer** — whether the target is a collection initializer on the right of an assignment, whose opening brace stays on the assignment's line.

The list comes from reading every access to a parent, an ancestor, or a neighbouring token in the phases that run on the attached root today — every phase before the indentation phase, which detaches the root for itself — and keeping those that can leave the root. The accesses to the token in front of the root's first token read the preceding-token fact, which already carries what they use: the token's kind and whether its trailing trivia ends the line. *(amended 2026-10-31, implementer stage)* The blank-line phase, the documentation phase, and the comment alignment of the indentation phase read it. Some line-break and spacing helpers still ask whether a token without a previous token in its tree starts its line from that token's own leading trivia; no output was found where that answer differs from the fact's. *(amended 2026-10-31, process stage)* No caller of the repository was shown to reach a difference there, so the helpers stay as they are and no follow-up is filed. The accesses to the token behind the root's last token decide only the trivia behind that token — the line break the brace transform writes behind an inserted `}` and the spacing behind the last token — which `FormatNodeInDocumentAsync` replaces with the document's trailing trivia (FMT-PIPE-010) and which `FormatNode` leaves to its caller; they get no fact and decide as at the end of a file.

`FormatNodeInDocumentAsync` supplies the seven flags; `FormatNode` and `FormatNodeInDocumentWithContextAsync` do not. `FormatNode` keeps the result of the same node without a parent (FMT-PIPE-012), as accepted. The context entry point's root is detached today and reads none of these ancestors; the skeptic's corpus run found no output of it that changes.

The flags make `FormatNodeInDocumentAsync` independent of whether an earlier phase changed the target: a tab in the target gives the output the same target with spaces gives (FMT-PIPE-023). That is the observable check that the list is complete.

### The comment behind `else if` stays without a new owner of the leading trivia *(amended 2026-10-31, skeptic stage)*

The phases and `FormatNodeInDocumentAsync` both decide what stands in front of a target that does not start its line: the phases may move a line break or a comment there, and the entry point writes the document's leading trivia back over it (FMT-PIPE-010). This change does not change that ownership; the maintainer folded it into thoenissen/Reihitsu#1521. Within this change, the `else if` flag keeps the line-break phase from moving the comment of `else if (y) // note` in front of the `if`, so nothing the entry point discards holds a comment. Whether the flag also closes the comment loss of the RH5405 fix, which braces the body first and so detaches the target on `main` today, is not claimed; that loss is a separate follow-up. *(amended 2026-10-31, process stage)* The implementation shows that it does: the RH5405 fix keeps the comment (see *Visible output changes*).

### Which entry point supplies which fact

`FormatNodeInDocumentAsync` supplies every fact above, for its target. `FormatNodeInDocumentWithContextAsync` supplies the depth, the column, and the documentation flag for its context, and neither the preceding token nor the list position, as today: it writes back only its target, with the target's leading trivia as in the document (FMT-PIPE-018), and its root is detached already. `FormatNode` supplies the depth and the documentation flag only: its result is inserted by its caller, which decides what stands in front of it and behind it, and it has no document whose blank lines it could keep. That is why `Call(1, 2);` behind a blank line gives `        Call(1, 2);` whether or not it holds a tab, the output the issue asks for.

### The depth of a root is the indentation phase's depth at its position

Without an explicit `indentLevel`, a root's depth is the number of levels FMT-INDENT-002, FMT-INDENT-003, and FMT-INDENT-004 give its first token at its position in its tree: one per enclosing scope, one per unbraced embedded statement the root is or lies in, none for an `else if`, and none for an initializer or anonymous object, whose members the indentation phase aligns to an anchor rather than to a depth. A node without a parent has depth 0. `FormatNode`'s `indentLevel` argument, when zero or greater, keeps overriding it.

For a node inside an initializer, no depth gives the column document-level formatting gives it; `FormatNode` lays it out at the depth of the statement or declaration that holds the initializer. `ComputeBaseIndentLevel`'s extra level per initializer is just as far from the anchored column and is a second rule, which the issue rejects. The only production caller of `FormatNode`, the RH5401 code fix, passes a property declaration, which never lies in an initializer, and reads only whether the result is one line.

The node-in-document entry points shift the layout so that the root's first token keeps its column in the document (FMT-PIPE-017), so the depth they use does not reach their output; they take the same rule so that no depth rule other than the indentation phase's remains.

*(amended 2026-10-31, skeptic stage)* The shift does not cancel the depth where FMT-PIPE-017's offset is zero: a first token behind the last line of a multi-line comment keeps its column, and the lines the depth places take that depth's columns. There the depth rule reaches the output, as listed under What: an unbraced embedded `if` gets the columns FMT-INDENT-004 gives it, which Core's count misses, and a node inside an initializer moves one level left, where neither count gives the column document-level formatting gives it (the anchor outside the root, which FMT-PIPE-017 does not cover). Keeping today's output there would keep Core's count as a second depth rule for that one case, so the maintainer chose the indentation phase's depth there too.

### FMT-CLEAN-008 covers every node-level root

Today the requirement names a node that is not part of a syntax tree. The indentation phase detaches every root before the cleanup phase runs, so the line breaks at the end of an attached node's last token are already removed. The requirement now names every root a node-level entry point formats, other than a compilation unit, whose end is the end of a file (FMT-CLEAN-005). `FormatNode` returns that result; the node-in-document entry points write back the target's trailing trivia from the document (FMT-PIPE-010).

### Out of scope

- The gap FMT-PIPE-017 records for `FormatNodeInDocumentWithContextAsync` stays (thoenissen/Reihitsu#1501).
- The blank lines `FormatNodeInDocumentAsync` decides above a target that starts its line keep their current, partly unspecified behavior; this change only moves the facts they read into plain values.
- `SyntaxIndentationUtilities.ComputeBaseIndentLevel` stays in Core for its caller outside the formatter, the attribute-placement code fixes.
- FMT-PIPE-021 keeps excluding the roots of the node-level entry points.
- *(amended 2026-10-31, skeptic stage)* Who owns what stands in front of a target that does not start its line (thoenissen/Reihitsu#1521), and the comment loss of the RH5405 fix on `main` (thoenissen/Reihitsu#1527).
- *(amended 2026-10-31, skeptic stage)* The construct facts for `FormatNode` and `FormatNodeInDocumentWithContextAsync`.
- *(amended 2026-10-31, implementer stage)* A target that is an inner part of a longer member-access chain. The line-break phase reads the chain-continuation fact and keeps the part's layout, while the chain alignment of the indentation phase still aligns the part as a chain of its own, so the two decisions disagree and neither gives the layout of document-level formatting. No code fix formats an inner chain part — the chain code fixes pass the outermost chain node — so the case is left to a follow-up that gives chain outermost-ness one owner. *(amended 2026-10-31, process stage)* No caller of the repository reaches the case, so it stays as it is and no follow-up is filed. *(amended 2026-10-31, audit stage)* The two decisions no longer disagree: the chain alignment asks the same owner as the line-break phase, so neither lays out or aligns the part as a chain of its own. The `.Where` line that the alignment put in column 37 for such a target is no longer aligned as the first link of a chain of its own. The output still differs from document-level formatting, which lays out the whole outer chain, and no caller reaches it.

## Alternatives considered

- **Keep the root attached for every phase and require each phase to preserve the attachment** — rejected: every phase that changes a node would have to splice it back into the document, and a read across the boundary would still see the document's unformatted neighbours, not the facts a requirement names.
- **State Core's ancestor count in FMT-PIPE-012** — rejected: it writes the second depth rule into the spec, and an unbraced embedded statement would keep the wrong column.
- **Give `FormatNode` the preceding-token and list-position facts of an attached node, as `FormatNodeInDocumentAsync` has them** — rejected: `Call(1, 2);` behind a blank line would keep a leading line break, which the issue rules out, and the facts describe a document that `FormatNode` does not write back.
- **Lay out an attached `FormatNode` root from its first token's column in the document, as the node-in-document entry points do** — rejected: `FormatNode`'s result is meant to be inserted at its depth, and a misindented input would keep its column.
- **Keep the trailing trivia of the preceding token as a trivia list and forbid reading its positions** — rejected: the rule cannot be enforced, and the issue asks that no context field hold trivia of the document.
- **Give `FormatNodeInDocumentWithContextAsync` the preceding-token facts of its context** — rejected for this change: it writes back only its target, with the target's leading trivia as in the document, and no output was found that would change; the context's own blank lines are not written back.
- *(amended 2026-10-31, skeptic stage)* **List the changed `FormatNodeInDocumentAsync` outputs and proceed** — rejected by the maintainer: the `else if` comment would be lost for every such target, and layouts the code fixes produce today would change in every fix that formats a chain part, an accessor, or a list element.
- *(amended 2026-10-31, skeptic stage)* **Narrow the change to `FormatNode` and leave `FormatNodeInDocumentAsync` attached** — rejected by the maintainer: the issue's Done-when would not be met, and the output would keep depending on a tab.
- *(amended 2026-10-31, skeptic stage)* **One generic fact: the kinds of the root's ancestors** — rejected: the decisions need more than kinds — which slot of its parent the root fills, whether a property stands on one line, whether an owning construct stands between the root and a list element — and a list of ancestor kinds would invite phases to rebuild those answers each in its own way.
- *(amended 2026-10-31, skeptic stage)* **Give `FormatNode` the construct facts as well** — rejected: FMT-PIPE-012 as accepted gives a node that is part of a tree the result of the same node standing alone, and its caller decides where the node goes.
- *(amended 2026-10-31, skeptic stage)* **Make the entry point the only owner of the target's leading trivia in this change** — deferred by the maintainer to thoenissen/Reihitsu#1521.
