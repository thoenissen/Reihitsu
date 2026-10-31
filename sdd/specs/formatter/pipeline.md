# Formatter — Pipeline

| | |
|---|---|
| ID prefix | FMT-PIPE |
| Status | Active |
| Code owners | `ReihitsuFormatter`, `FormattingPipeline`, `IFormattingPhase`, `PreparationPhase`, `FormattingContext`, `ReihitsuFormatterHelpers`, `LanguageVersionResolver`, `LineEndingUtilities` |
| Uses | SHARED-FIX-001, SHARED-FIX-006, SHARED-FIX-007, FMT-BLANK-011, FMT-BLANK-013, FMT-BLANK-014, FMT-BREAK-001, FMT-BREAK-011, FMT-BREAK-012, FMT-BREAK-014, FMT-BREAK-016, FMT-BREAK-021, FMT-BREAK-022, FMT-BREAK-026, FMT-BREAK-029, FMT-CLEAN-001, FMT-CLEAN-005, FMT-EOL-001, FMT-EOL-002, FMT-EOL-003, FMT-EOL-004, FMT-INDENT-007, FMT-INDENT-008, FMT-INDENT-030, FMT-RAW-001, FMT-RAW-005, FMT-RAW-007, FMT-SPACE-002, FMT-STRUCT-001, FMT-STRUCT-004, FMT-STRUCT-017, FMT-STRUCT-021, FMT-STRUCT-026, FMT-TAB-001, FMT-TAB-002, FMT-TAB-003 |

This spec covers what holds for the formatter as a whole: the public entry points, the fixed phase order, and the invariants every phase must keep. What an individual phase decides is specified in that phase's own spec.

## Non-goals

- The decisions of a single phase — each phase has its own spec, or two for the indentation phase, listed in FMT-PIPE-001.
- File and directory walking, skipped file names, `--check`, `--dry-run`, and exit codes — those belong to the CLI specs.
- Configuration. The formatter applies one fixed style and reads no `.editorconfig`, rule set, or option.
- Formatting of code with syntax errors or of generated code (see FMT-PIPE-005 and FMT-PIPE-006).

## Requirements

### FMT-PIPE-001 — Fixed phase order

The formatter MUST apply its phases in this order, each phase receiving the output of the one before it:

| # | Phase | Spec |
|---|---|---|
| 1.1 | `PreparationPhase/TabReplacement` | [tab-replacement.md](tab-replacement.md) (FMT-TAB) |
| 1.2 | `PreparationPhase/LineEndings` | [line-endings.md](line-endings.md) (FMT-EOL) |
| 2 | `StructuralTransformPhase` | [structural-transforms.md](structural-transforms.md) (FMT-STRUCT) |
| 3 | `RegionFormattingPhase` | [region-formatting.md](region-formatting.md) (FMT-REGION) |
| 4 | `DocumentationCommentFormattingPhase` | [documentation-comments.md](documentation-comments.md) (FMT-DOC) |
| 5 | `UsingDirectiveOrderingPhase` | [using-directives.md](using-directives.md) (FMT-USING) |
| 6 | `BlankLinePhase` | [blank-lines.md](blank-lines.md) (FMT-BLANK) |
| 7 | `LineBreakPhase` | [line-breaks.md](line-breaks.md) (FMT-BREAK) |
| 8 | `SwitchCaseBracePhase` | [switch-case-braces.md](switch-case-braces.md) (FMT-SWITCH) |
| 9 | `HorizontalSpacingPhase` | [horizontal-spacing.md](horizontal-spacing.md) (FMT-SPACE) |
| 10 | `IndentationPhase` | [indentation.md](indentation.md) (FMT-INDENT) and [raw-string-alignment.md](raw-string-alignment.md) (FMT-RAW) |
| 11 | `CleanupPhase` | [cleanup.md](cleanup.md) (FMT-CLEAN) |

The first phase is the preparation phase, whose two sub-phases, TabReplacement and LineEndings, run in this order. Each sub-phase is a phase of its own: it receives the output of the step before it and is reported to the pipeline observer and the trace under the name in the table. The preparation phase is not reported as a step of its own; every other phase is reported under its type name.

A phase MUST NOT compute the column of a token, a comment, or a line from a move or a replacement that a phase after it applies, neither by repeating that phase's rule nor by adjusting for its result: the tab width of FMT-TAB-001 and the raw-string move of FMT-RAW-001 are applied before or in the phase that measures them. This requirement specifies columns only; it does not decide whether a phase may anticipate a later phase's line breaks or blank lines.

Why: Later phases rely on the shape earlier phases produce — tabs and line endings are normalized before any other phase runs (FMT-PIPE-019), line breaks are decided before spacing and indentation measure columns, and raw strings are aligned in the phase that places their opening quotes. A phase may assume the phases before it ran; it must not assume anything about the phases after it. A phase that predicts a later one makes one decision in two places, and every difference between the prediction and the later phase is output that the next pass changes again (FMT-PIPE-004).

### FMT-PIPE-002 — User-authored content survives

The formatter MUST preserve the text of every comment, preprocessor directive, disabled-text region, and literal. It MAY move such trivia, re-indent it, or normalize the whitespace and line breaks around it, but it MUST NOT delete it or change its text, except where a phase spec explicitly allows a normalization of that text (for example region names in FMT-REGION or documentation comments in FMT-DOC).

Why: Formatting is non-destructive. A formatter that drops a comment, a `#if` branch the current build does not compile, or a character inside a string literal loses information the user cannot recover from the formatted file. When a phase cannot rewrite a span without losing such content, it leaves that span as it is.

Example:

```csharp
class C{
    // keep  me
    void M(){
#if NEVER
   this   is   disabled   text
#else
        var s="a  b"; /* inline  comment */
#endif
    }
}
```

```csharp
class C
{
    // keep  me
    void M()
    {
#if NEVER
   this   is   disabled   text
#else
        var s = "a  b"; /* inline  comment */
#endif
    }
}
```

### FMT-PIPE-003 — Program meaning is unchanged

The formatter MUST NOT change what the formatted code compiles to. Phases that rewrite syntax (FMT-STRUCT, FMT-USING, FMT-SWITCH) MUST only produce equivalent code and MUST leave a construct unchanged when they cannot prove the rewrite equivalent.

Why: Users run the formatter over whole repositories without reviewing every line. A layout tool that can change behavior cannot be trusted at that scale.

### FMT-PIPE-004 — Idempotency

Formatting already formatted output MUST return it unchanged: running the formatter twice MUST give the same text as running it once.

Why: The CLI's `--check` mode, the analyzer code fixes that delegate to the formatter, and CI gates all assume that formatted code stays formatted. A non-idempotent result shows up as an endless diff.

### FMT-PIPE-005 — Code with syntax errors is left untouched

`FormatDocumentAsync`, `FormatSyntaxTree`, `FormatNodeInDocumentAsync`, and `FormatNodeInDocumentWithContextAsync` MUST return their input unchanged — for the two node-in-document entry points, the document, whichever node of it is the target or the context — when the syntax tree contains any error-level diagnostic. `FormatNode` checks no syntax errors (FMT-PIPE-012).

Why: Roslyn's error recovery invents missing tokens and attaches skipped text in unpredictable places. Rewriting trivia around them can move or lose code the user is in the middle of typing.

Example:

```csharp
class C{void M(){int x=1}}
```

is returned unchanged, because the `;` after `1` is missing. The same document is returned unchanged by `FormatNodeInDocumentAsync` with the method `M` as target, and by `FormatNodeInDocumentWithContextAsync` with `M` as target and the class `C` as context.

### FMT-PIPE-006 — Generated code is left untouched

`FormatDocumentAsync`, `FormatSyntaxTree`, `FormatNodeInDocumentAsync`, and `FormatNodeInDocumentWithContextAsync` MUST return their input unchanged — for the two node-in-document entry points, the document, whichever node of it is the target or the context — when a comment in the leading trivia of the file's first token contains `<auto-generated` or `<autogenerated` (case-insensitive). A marker anywhere else in the file MUST NOT disable formatting. `FormatNode` checks no marker (FMT-PIPE-012).

Why: Generated files are rewritten by their generator; formatting them only produces churn. Only the file header counts, so a comment that merely mentions the marker in the middle of a file does not switch the formatter off.

Example:

```csharp
// <auto-generated />
class C{void M(){int x=1;}}
```

is returned unchanged, also by `FormatNodeInDocumentAsync` with the method `M` as target and by `FormatNodeInDocumentWithContextAsync` with `M` as target and the class `C` as context, while a marker inside the type does not count:

```csharp
class C{
// <auto-generated />
void M(){int x=1;}}
```

```csharp
class C
{
    // <auto-generated />
    void M()
    {
        int x = 1;
    }
}
```

### FMT-PIPE-007 — The file's predominant line ending is used

The formatter MUST detect the file's predominant line ending by counting end-of-line trivia and documentation-comment newlines, use `\r\n` when it occurs at least as often as `\n`, and fall back to `Environment.NewLine` when the input contains no line break at all. A line break of a single character, such as a lone `\r`, MUST count as `\n`, so a file whose line breaks are all a lone `\r` gets `\n`. Every line break the formatter writes MUST use the detected line ending, and FMT-EOL rewrites the existing line breaks between tokens and inside documentation comments to it. Line breaks inside literals, inside other comments, and inside disabled text keep their original line ending (FMT-PIPE-002).

Why: Mixed line endings produce whole-file diffs in version control. The majority wins because it is what the file's author most likely uses; a tie goes to `\r\n` so the decision is deterministic. A lone `\r` is not a line ending the formatter writes, so a file that uses it gets the one-character line ending it does write.

Example — the input has four `\r\n` line breaks and one `\n` (`␍` marks a `\r`); the missing final line break is FMT-CLEAN's doing:

```csharp
class C␍
{␍
void M(){}␍
  int x;
}␍
```

```csharp
class C␍
{␍
    void M()␍
    {␍
    }␍
    int x;␍
}
```

Example — the input has two lone `\r` line breaks and one `\r\n` (`␍` marks a `\r`: at the end of a line it is the first half of a `\r\n`, inside a line it is a lone `\r`). The two lone `\r` count as `\n`, so `\n` outnumbers `\r\n`:

```csharp
class C␍{␍void M(){}␍
}
```

```csharp
class C
{
    void M()
    {
    }
}
```

Every line break of the result is `\n`. With a second `\r\n` behind the closing brace, the count is a tie and every line break of the result is `\r\n`. In a file whose line breaks are all a lone `\r`, every line break of the result is `\n`.

### FMT-PIPE-008 — Version-dependent rules follow the language version

Rules that depend on the C# language version MUST read it from these parse options:

- `FormatDocumentAsync` and `FormatSyntaxTree`: the parse options of the syntax tree they format — for a document, the tree its project parsed;
- `FormatNode`: the parse options of the tree that Roslyn reports as the given node's `SyntaxTree`, whether or not the node has a parent;
- `FormatNodeInDocumentAsync` and `FormatNodeInDocumentWithContextAsync`: the parse options of the document's project.

Roslyn decides which tree a node passed to `FormatNode` reports, by how the node was obtained:

- A node inside a parsed tree reports that tree.
- A node returned by a `SyntaxFactory.Parse…` method called with parse options, or the root of a tree created by `CSharpSyntaxTree.Create` with parse options, reports a tree with those parse options.
- A node returned by an edit that produces a new root — `ReplaceNode`, `ReplaceToken`, `ReplaceTrivia`, `RemoveNode`, the `Insert…` methods, `NormalizeWhitespace`, and `WithLeadingTrivia`, `WithTrailingTrivia`, and `WithoutTrivia` — reports a new tree with the parse options of the edited node's tree, and so do the nodes inside it.
- These nodes report a tree with the default parse options: a node returned by a `SyntaxFactory.Parse…` method called without parse options, a node built with `SyntaxFactory`, a node returned by a generated `With…` or `Add…` method or by `WithAdditionalAnnotations` when the call changes the node, and every node inside such a node. A later edit of the previous kind keeps the default parse options. A `With…` call that changes nothing returns the node itself, which keeps its tree.

`Default`, `Latest`, and `LatestMajor` MUST resolve to the newest supported version (C# 14), and `Preview` or any newer version MUST be clamped to it. Without C# parse options, the newest supported version applies.

Why: The formatter must not produce syntax the project's compiler rejects, such as an expression-bodied property in a C# 5 project. Clamping keeps the formatter's output stable when Roslyn learns a version the formatter has not been reviewed against.

Example — the same input under C# 5 and under C# 6:

```csharp
class C
{
    int P { get { return 1; } }
}
```

C# 5:

```csharp
class C
{
    int P
    {
        get
        {
            return 1;
        }
    }
}
```

C# 6:

```csharp
class C
{
    int P => 1;
}
```

The node-level entry points keep accessor shapes (FMT-PIPE-011), so they show the version through another version-dependent rule, the semicolon body of an empty type (FMT-STRUCT-017). The class `Empty` as the target of `FormatNodeInDocumentAsync` in a C# 11 project, as the target of `FormatNodeInDocumentWithContextAsync` with the compilation unit as context in a C# 11 project, or given to `FormatNode` from a tree parsed as C# 11:

```csharp
class Empty
{
}
```

stays as it is. In a C# 12 project, or from a tree parsed as C# 12, the result is:

```csharp
class Empty;
```

Given to `FormatNode` without a parent, the class `Empty` from a tree parsed as C# 11 stays as it is after `ReplaceToken`, `WithoutTrivia`, or `NormalizeWhitespace`, and so does the compilation unit after `ReplaceNode` puts a changed class into it. Parsed with `SyntaxFactory.ParseCompilationUnit` or `SyntaxFactory.ParseMemberDeclaration` with C# 11 parse options, it stays as it is as well. The same class gets the C# 12 result after `WithIdentifier` with a new identifier token, after `WithAdditionalAnnotations`, or after `SyntaxFactory.CompilationUnit().AddMembers` puts it into a new compilation unit; parsed without parse options or built with `SyntaxFactory`, it gets the C# 12 result too, because the default parse options resolve to the newest supported version.

### FMT-PIPE-009 — Host-independent entry points

The formatter MUST expose its behavior through `ReihitsuFormatter` on Roslyn objects — `FormatDocumentAsync`, `FormatSyntaxTree`, `FormatNode`, `FormatNodeInDocumentAsync`, and `FormatNodeInDocumentWithContextAsync` — and MUST NOT read files, the console, or configuration itself. `FormatDocumentAsync` and `FormatSyntaxTree` MUST produce the same text for the same input and language version.

Why: The CLI, the analyzer code fixes, and the Playground share one engine. Keeping file access and reporting out of the formatter means every host formats identically and a host-specific behavior can never leak into another host.

### FMT-PIPE-010 — Node-level entry points format only their target

`FormatNodeInDocumentAsync` and `FormatNodeInDocumentWithContextAsync` MUST rewrite only the target node and keep the rest of the document as it is. The one exception is the whitespace that ended the line before a documentation comment the target moves onto its own line, which MUST be removed as document-level formatting removes it. `FormatNodeInDocumentAsync` MUST keep the target's leading trivia when the target does not start its line or is a direct child of the root; `FormatNodeInDocumentWithContextAsync` MUST keep it in every case (FMT-PIPE-018). Both MUST keep the target's trailing trivia in every case.

Why: These entry points serve code fixes, which change only the code their diagnostic is about (SHARED-FIX-006). Reformatting the surrounding document would turn a one-line fix into an unrelated diff.

Example — `FormatNodeInDocumentAsync` with the property `P` as target:

```csharp
class C
{
    int P { get { return 1; } }
    int Q { get { return 2; } }
}
```

```csharp
class C
{
    int P
    {
        get
        {
            return 1;
        }
    }
    int Q { get { return 2; } }
}
```

### FMT-PIPE-011 — Node-level entry points keep accessor shapes

`FormatNode`, `FormatNodeInDocumentAsync`, and `FormatNodeInDocumentWithContextAsync` MUST NOT convert single-statement accessor blocks to expression bodies and MUST NOT convert get-only properties or indexers to expression-bodied members; only the document- and tree-level entry points apply those transforms (FMT-STRUCT).

Why: A code fix that reformats the member it changed must not also change the member's shape — that is a separate decision the user did not ask the fix to make. The example under FMT-PIPE-010 shows the accessor block kept.

### FMT-PIPE-012 — Detached nodes skip the file-level guards

`FormatNode` MUST format the node it is given without checking for syntax errors or the generated-code marker, and MUST take the indentation level from its `indentLevel` argument when that is zero or greater, otherwise from the node's position in its tree.

Why: `FormatNode` receives freshly generated or detached subtrees that have no file header and whose tree diagnostics say nothing about the file they will be inserted into. The caller is responsible for passing a node that is safe to format.

### FMT-PIPE-013 — Wrapping code in a block keeps the comments' layout

When the formatter wraps code in braces or a block — the braces of a switch section (FMT-SWITCH), the braces of an `if` or `else` body, or the block body of an expression-bodied member (FMT-STRUCT) — it MUST keep the layout the author gave the comments that the wrapped code carries:

- the space between two comments on the same line;
- a comment's place on the line of its statement, whether the comment stands in front of the statement or behind it;
- the blank lines between the comments that lead a statement.

A comment on a line of its own above the statement stays on a line of its own, and a comment behind the statement stays behind it. The phases after the wrap decide for the wrapped comments what they decide for any other comment, such as collapsing a run of blank lines to one.

Why: A wrap adds braces; it is not a decision about the comments. The space between two comments, a comment written on its statement's line, and a blank line that groups comments are the author's layout, and the same comments keep it in code that needs no wrap. FMT-PIPE-002 lets a phase normalize the whitespace around comments; a wrap that loses this layout only because it rebuilt the trivia around the new braces is no such normalization.

Example — the comment in front of `Foo();` stays on its own line, and the comments behind `42` and `Foo();` stay on their lines:

```csharp
class C
{
    int Get() => 42 /* why */;

    void M(bool a)
    {
        if (a)
            // leading
            Foo(); // trailing
    }
}
```

```csharp
class C
{
    int Get()
    {
        return 42 /* why */;
    }

    void M(bool a)
    {
        if (a)
        {
            // leading
            Foo(); // trailing
        }
    }
}
```

### FMT-PIPE-014 — One treatment per syntax family

The formatter MUST lay out and space every member of a syntax family alike. Each of these is one family:

- every parameter list, the bracketed parameter list of an indexer included;
- every assignment operator: `=` and every compound assignment operator, such as `+=` and `??=`;
- every arrow `=>`, whatever construct it belongs to;
- every colon that ends an attribute target, such as the colon of `return:`, `assembly:`, or `field:`;
- every closing bracket of an initializer-like block, such as the `}` of an object or collection initializer and the `]` of a collection expression.

This requirement does not override the layout a construct's own requirement chooses for it inside its family; it requires that no member of a family is left out of the treatment the requirements give that family. These choices stay as their requirements make them: a function-pointer parameter list is joined onto one line (FMT-BREAK-014); an array initializer keeps the author's layout while an object or collection initializer is laid out over several lines (FMT-BREAK-021); a collection expression written on one line stays on it (FMT-BREAK-022); the arrow of an expression-bodied property or indexer joins the declaration's line (FMT-BREAK-026); and the arms of a switch expression keep the line breaks the author wrote. Within those choices, an indexer's parameter list is laid out as the parameter list of a method is, a compound assignment as an assignment with `=` is, every attribute-target colon is spaced alike, a comment in front of a closing `]` is indented as one in front of a closing `}` is, and every arrow is spaced alike.

Why: A reader learns the layout of a construct once and expects it wherever the construct occurs. When the formatter lays out one member of a family and leaves another as it was written, the same shape stays formatted in one place and unformatted in the next, and the author cannot predict which. A construct's own requirement chooses its layout for reasons of its own, such as keeping literal data lists short; the family statement does not undo those reasons, it keeps a member from being left out because the treatment was written for its neighbors only.

Example — the parameter lists of a constructor and a method are laid out alike (FMT-BREAK-011, FMT-BREAK-012), and so are two assignments wrapped before and after their `=` (FMT-BREAK-016):

```csharp
class C
{
    C(int a
      , int b)
    {
    }

    void M(int a
           , int b)
    {
        a
            = b;
        b =
            a;
    }
}
```

```csharp
class C
{
    C(int a,
      int b)
    {
    }

    void M(int a,
           int b)
    {
        a = b;
        b = a;
    }
}
```

### FMT-PIPE-015 — Columns are final after the indentation phase

The phases after `IndentationPhase` MUST NOT change the column of any token, comment, or directive, nor the indentation of a content line or the closing line of a multi-line raw string literal. They MAY remove whitespace that ends a line and line breaks at the end of the file (FMT-CLEAN-001, FMT-CLEAN-005).

Why: The indentation phase measures every anchor at its final column (FMT-INDENT-030). That holds only when no later phase moves text within a line; a later move would force the indentation phase to predict it, which FMT-PIPE-001 rules out.

Example — the indentation phase places `2` under `1` and moves the raw string under its opening quotes; the output of that phase already has every line in its final column, and the phases after it only remove the final line break:

```csharp
class C
{
void M()
{
var s = """
    a
    """;

    Call(1,
2);
}
}
```

```csharp
class C
{
    void M()
    {
        var s = """
                a
                """;

        Call(1,
             2);
    }
}
```

When a line of the input ends in spaces, the cleanup phase removes them, and no token moves. When the input has a tab in front of a trailing comment, such as `Call(1,`, a tab, `// c`, that tab is already four spaces when the indentation phase runs (FMT-PIPE-019), so the comment keeps the column the indentation phase left it in.

### FMT-PIPE-016 — No phase after the first sees or writes a tab in whitespace

Status: Retired — the tab rule is now an invariant of the preparation phase; FMT-PIPE-019 states it unchanged for tabs and extends it to line endings.

### FMT-PIPE-017 — Node-level targets take their columns from the indentation phase

`FormatNodeInDocumentAsync` and `FormatNodeInDocumentWithContextAsync` MUST NOT change the column of a line after the last phase: every line they write back MUST end in the column the indentation phase gives it (FMT-PIPE-015), apart from the leading trivia they keep (FMT-PIPE-010, FMT-PIPE-018) and the recorded gap below. The indentation phase MUST lay out the node they format — the target, or the context node — with the columns it gives that node at its depth, shifted by the node's offset: the column of the node's first token in the document, less the column the phase gives that token at the node's depth. The first token therefore stays in its column, a line placed by the depth keeps its place relative to the first token, and a line aligned to an anchor follows the anchor where the phase puts it. This holds for every line written back, also for a line that starts inside a comment that began on an earlier line, for the lines aligned to an anchor on such a line (FMT-INDENT-030), and for the content, hole, and closing lines of a multi-line raw string literal (FMT-RAW-001, FMT-RAW-007). When the first token stands behind the last line of a multi-line comment, the phase does not place its line (FMT-INDENT-007), the token keeps its column, and the offset is zero.

The column of the first token in the document is the number of characters in front of it on its line, each one column, with two exceptions:

- A tab in whitespace that belongs to the node and that the entry point does not keep — the leading whitespace of a target of `FormatNodeInDocumentAsync` that starts its line and whose leading trivia FMT-PIPE-010 does not keep — counts with the width the preparation phase gives it (FMT-PIPE-019). `FormatNodeInDocumentWithContextAsync` keeps its target's leading trivia in every case (FMT-PIPE-018), so for it every character in front of the context's first token counts as one column.
- A byte order mark at the start of the text counts as zero columns, as in the layout the phases compute; the raw-string move still counts it as FMT-RAW-005 states.

So a tab in the whitespace in front of a node that does not start its line, which belongs to the code in front of the node and is kept, counts as one column, and so does a tab inside a comment or a literal, which is kept (FMT-TAB-003). The leading trivia that FMT-PIPE-010 and FMT-PIPE-018 keep is written back as it stands and is outside this requirement, and so are the lines of the context node outside the target, which `FormatNodeInDocumentWithContextAsync` does not write back.

Recorded gap — a target of `FormatNodeInDocumentWithContextAsync` whose first token the phases place in another line or column than the one it keeps in the document. The entry point writes back only the target, with its leading trivia as in the document, the whitespace in front of its first token included (FMT-PIPE-010, FMT-PIPE-018), and keeps everything in front of it on its line as it stands. So the target's first token stays where it stood in the document, while the other lines of the target take the columns the phases gave them; a line aligned to an anchor on the target's first line stands where the anchor stood in the phases' layout, not where the anchor ends up. This requirement does not cover the target's first line or the lines whose column depends on it. The gap has three known cases:

- The target starts a later line of its context, and the phases give that line another column than the whitespace in front of it in the document.
- The target does not start its line, and the phases break the line in front of it. The line break they add is not written back, so the target stays on the line where it started, in its column in the document, while its other lines take the columns the phases gave them on the new line.
- The target does not start its line, and the phases move the line it starts on, such as a content line of a multi-line raw string literal that the indentation phase moves under its opening quotes (FMT-RAW-001). What stands in front of the target on that line is outside the target and is written back as in the document, so the target's first token keeps its column in the document, while its other lines take the columns the phases gave them on the moved line.

A target whose first token the phases leave in its line and column in the document is not in the gap: what stands in front of it is written back unchanged, and the phases laid the target out from that column. That covers a target that starts the context's first line, and a target that does not start its line and in front of which the phases change nothing on that line.

Example — the second case: with the argument `Wrap(…)` of `        var x = Call(1, Wrap("""` / `    a` / `    """));` as target and the statement as context, the phases put the argument on a line of its own under `1`, in column 21, and the content line and the closing marker of the raw string literal under its opening quotes in column 26. The line break is not written back, so `Wrap(` stays on the first line, the opening quotes stand in column 29, and the content line and the closing marker stand in column 26, three columns left of them.

Example — the third case: `FormatNodeInDocumentWithContextAsync` with the invocation `Call(1, 2)` as target and the local declaration as context. The phases move the literal's lines five columns to the right, under the opening quotes, and place `2` under `1` in column 25. Only the target is written back, so the literal's content line, its closing line, and `Call(1,` keep their columns in the document, with `1` in column 20, while `2` stands in the phases' column 25:

```csharp
class C
{
    void M()
    {
        var s = $"""
            a {Call(1,
            2)} b
            """;
    }
}
```

```csharp
class C
{
    void M()
    {
        var s = $"""
            a {Call(1,
                         2)} b
            """;
    }
}
```

Example — the first case: the RH5111 fix of the declarator `b` in `        int a = 1,` / `              b =` / `                  Other(1,` / `                        2);`, with the statement as context: the result is `              b = Other(1,` / `                  2);`, `b` keeps column 14, and `2` stands in column 18, under the column `1` had with `b` in the phases' column 8, while `1` ends up in 24.

Why: A code fix formats only the code its diagnostic is about (FMT-PIPE-010), and the columns of that code have to be the columns the formatter gives it, so that formatting the result changes nothing in it (SHARED-FIX-007). When the node is formatted as if it started at another column and its lines are moved afterwards, the move reaches only lines that start with whitespace: the line behind a multi-line comment's last line and the lines of a raw string literal stay where the phases left them, while the lines aligned to them move, and a tab in the node's own leading whitespace counted as one column by the move but as four by the phases puts the node a few columns off. Laying out the node at its own column leaves nothing to move, so the indentation phase is the only owner of a line's final column for node-level targets too, as it is for documents (FMT-PIPE-001). Whitespace in front of the node that the entry point does not rewrite is counted as it stands, as the rules that report the code count it, so a fix's output is measured by its rule in the column the fix gave it (SHARED-FIX-001).

Example — `FormatNodeInDocumentAsync` with the `EqualsValueClauseSyntax` `= /* a … List<int> …` as target; `new` stands behind the last line of a multi-line comment, in column 24, and the braces and the element are laid out from it, as document-level formatting lays them out:

```csharp
using System.Collections.Generic;

class C
{
    void M()
    {
        var d = /* a
                   b */ new List<int>
          {
            1
          };
    }
}
```

```csharp
using System.Collections.Generic;

class C
{
    void M()
    {
        var d = /* a
                   b */ new List<int>
                        {
                            1
                        };
    }
}
```

Example — with the `EqualsValueClauseSyntax` `= """ …` of `        var s = """` / `    a` / `    """;` as target, the content line and the closing marker move under the opening quotes in column 16, as in document-level formatting.

Example — in a class whose members are indented with two spaces, the RH3202 fix converts `  int Get() => 1;` and formats the method: the first token keeps column 2, and the lines placed by the depth keep their place relative to it, so the result is `  int Get()`, `  {`, `      return 1;`. The same fix on `  string S() => """` / `      a` / `      """;` gives `      return """` with the opening quotes in column 13, and the content line and the closing marker in column 13 under them.

Example — not shown as input blocks because the inputs contain tabs: the statement `Call(1,` / `2);` is indented with two tabs, as is the method around it, and is the target. The tabs belong to the target's leading whitespace, so its first token counts as column 8, and the output is `        Call(1,` / `             2);`, with eight and thirteen spaces, while the lines of the method outside the target keep their tabs. In `→→var d = new List<int>` with its braces out of place and the `EqualsValueClauseSyntax` as target, the two tabs stand in front of the target and are kept, so they count as one column each: `new` stands in column 10, and the braces are written with ten spaces in front of them.

### FMT-PIPE-018 — The context entry point writes back its target's leading trivia as in the document

`FormatNodeInDocumentWithContextAsync` MUST write back the target's leading trivia as it stands in the document, in every case: its blank lines, its comments and directives in their columns, and the whitespace in front of the target's first token. The blank-line requirements (FMT-BLANK-011, FMT-BLANK-013, FMT-BLANK-014) and the columns the indentation phase gives those lines do not change it. This requirement does not decide the leading trivia of a target of `FormatNodeInDocumentAsync`, which FMT-PIPE-010 states.

Why: The entry point serves code fixes whose target starts a later line of the context, such as the RH5111 fix of an enum member, a parameter, or a declarator. A fix must not change the blank lines between the code it changes and its neighbours (SHARED-FIX-006), and writing back the trivia as it stands keeps them. Writing back the phases' trivia instead, with the document's blank lines put back, would need the entry point to decide again, for each part of the trivia, whether it starts its line — a decision the documentation phase makes differently when the token in front of the target lies inside the context; that is left to a restructuring of the entry point. The cost is the gap that FMT-PIPE-017 records: the target's first line keeps its column in the document.

Example — the RH5111 fix of the enum member `B` with the enum as context; the two blank lines above it stay, where document-level formatting would collapse them to one, and `B` keeps its column:

```csharp
enum E
{
    A,


  B =
  2
}
```

```csharp
enum E
{
    A,


  B = 2
}
```

Example — the same with a comment line above `B`; no blank line is added above the comment, where document-level formatting would add one, and the comment and `B` keep their columns:

```csharp
enum E
{
    A,
      // b
  B =
  2
}
```

```csharp
enum E
{
    A,
      // b
  B = 2
}
```

### FMT-PIPE-019 — After the preparation phase, no phase sees or writes a tab in whitespace or a foreign line ending

The preparation phase MUST replace the tabs in the whitespace of the code and between directive tokens (FMT-TAB-001, FMT-TAB-002) and rewrite every line break FMT-EOL covers to the detected line ending (FMT-PIPE-007, FMT-EOL-001, FMT-EOL-002, FMT-EOL-003), and a later phase MUST NOT write a tab into that whitespace or another line ending into those line breaks. So every phase after the preparation phase receives and returns:

- the whitespace of the code and between directive tokens without a tab, so it counts every character of it as one column;
- every end-of-line trivia, the line break that ends a directive, and every line break inside a documentation comment as the detected line ending, also where the phase writes a new one.

The whitespace inside a documentation comment, a comment, a literal, or disabled text, and a directive's free text, keep their tabs (FMT-TAB-003) and are outside this requirement: a tab in front of a continuation line's `///` reaches the indentation phase, which replaces that whitespace as a whole (FMT-INDENT-008). The line breaks inside a literal (FMT-EOL-004), inside a comment that is not a documentation comment, and inside disabled text keep their characters and are outside this requirement too.

Why: All layout is computed in spaces and on lines. A tab that survives into a later phase has a width that each phase has to agree on, and a phase that counts it differently from the phase that finally replaces it places a token where the next pass moves it (FMT-PIPE-004). A line break of another kind is a line to the tree but can fuse with a line break a phase writes next to it — a lone `\r` followed by a written `\n` is one line break in the text where the tree holds two — so a phase that numbers lines through the text works on wrong lines. Normalizing both first leaves every later phase one form to rely on, and a phase that writes another form is a defect a check of that phase's output finds, not a difference a final pass repairs.

Example — not shown as an input block because the input contains tabs: in

```csharp
class C
{
    void M()
    {
        Call(1,
             2);
    }
}
```

when the indentation of every line is written with tabs, the output of the TabReplacement sub-phase holds four spaces for each of those tabs, every later phase receives that text without a tab, and the result is the block above.

Example — in a file whose line breaks are `\n`, wrapping the statement below in braces (FMT-STRUCT) writes the line break behind the opening brace as `\n` (FMT-STRUCT-026), and the line break behind the comment, which the author wrote, is `\n` as well, so the output of every phase holds only `\n`:

```csharp
class C
{
    void M(bool a)
    {
        if (a)
            // leading
            Foo();
    }
}
```

```csharp
class C
{
    void M(bool a)
    {
        if (a)
        {
            // leading
            Foo();
        }
    }
}
```

Under `\r\n` line endings, the output of every phase holds only `\r\n`.

### FMT-PIPE-020 — The layout does not depend on the input's line endings

`FormatDocumentAsync` and `FormatSyntaxTree` MUST give a file the same output as they give its copy in which every line break that FMT-EOL rewrites (FMT-EOL-001, FMT-EOL-002, FMT-EOL-003) is already the line ending the formatter detects for the file (FMT-PIPE-007). This covers every line terminator FMT-EOL-001 lists, mixed in any proportion. The copy keeps the line breaks inside literals, inside comments that are not documentation comments, and inside disabled text as the file has them. This requirement does not cover the node-level entry points, which keep the document around their target as it stands (FMT-PIPE-010).

Why: A line ending is a property of the file, not of its code. A file saved with other line breaks than its author's usual ones must not get another layout, and a layout that depends on them is changed again by the next pass, once the first pass has written the detected line ending (FMT-PIPE-004).

Example — the class below saved with a lone `\r` after every line (`␍` marks a lone `\r`; the file holds no other line break, and the block's own line breaks are only for display):

```csharp
class C␍
{␍
    #region A␍
    int a;␍
    #endregion␍
␍
    #region B␍
    int b;␍
    #endregion␍
}␍
```

FMT-PIPE-007 detects `\n` for it, and the result is the result of the same class saved with `\n`, with every line break `\n`:

```csharp
class C
{
    #region A

    int a;

    #endregion // A

    #region B

    int b;

    #endregion // B
}
```

The same holds when every line break is U+0085, U+2028, or U+2029 (not shown as blocks because the characters are invisible), and for a file whose line breaks mix `\r\n`, `\n`, and lone `\r`.

### FMT-PIPE-021 — Every phase returns a tree its text parses back to

Every phase, each sub-phase of the preparation phase included, MUST return a tree whose text, parsed again with the parse options of the syntax tree the formatter was given — its language version and its preprocessor symbols, as they stand, not as FMT-PIPE-008 resolves them — gives the same tree:

- the same nodes in the same structure, and the same tokens with the same kinds, texts, and values at the same places — a token's value is what the parser gives its text, such as the string a raw-string text token stands for;
- no token the parser has to add or skip, so no syntax error;
- every comment, documentation comment, preprocessor directive, and disabled-text span as trivia of the same token, on the same side of it — leading or trailing.

Whitespace and line breaks need only stand at the same places: the parser may give a line break or whitespace to the other of the two tokens around it, and may join two neighbouring runs of whitespace into one. This requirement covers the tree that `FormatDocumentAsync` and `FormatSyntaxTree` pass through the phases; it does not cover the node that `FormatNode`, `FormatNodeInDocumentAsync`, and `FormatNodeInDocumentWithContextAsync` pass through them.

Why: Every phase after the one that wrote a tree, the documentation phase, which reads the text, and the trace, which prints the text and parses it again between passes, have to see the program the tree is. A tree whose text is another program — two tokens that read as one, or a token inside a comment — is formatted correctly only as long as a later phase happens to repair it, and a token pair that no spacing rule covers is never repaired (FMT-SPACE-002). Ownership of a comment decides what the phases do with it: a comment that trails a token stays at the end of that token's line (FMT-BREAK-001), while a comment that leads one starts its own line; a tree whose comment the parser gives to another token is formatted one way in memory and another way once its text is read. Which token holds a line break or whitespace decides nothing a phase relies on, and the line-break phase writes a line break in front of the token it moves, where the parser gives it to the token before (FMT-BREAK-001, FMT-BREAK-029). The text is parsed with the options the tree came with: with only the language version, a project symbol such as `DEBUG` would be missing, and the code under `#if DEBUG` would read back as disabled text.

Example — in the class below, the structural-transform phase converts `Get` and braces both `if` bodies (FMT-STRUCT-001, FMT-STRUCT-004):

```csharp
class C
{
    int Get() => 42;

    void M(bool a)
    {
        if (a)
            Foo(); // trailing
        else
            Bar();
    }
}
```

The phase's output MUST NOT read `int Get(){return42;}`, where `return42` parses as one identifier, and MUST NOT put the inserted `}` directly behind `// trailing`, where it is part of the comment. It reads `int Get(){return 42;}`, and a line break stands between `// trailing` and the inserted `}` (FMT-STRUCT-026).

Example — the same tree where the parser gives the whitespace and the line break to the other token: the line-break phase moves the `{` of `if (a) {` onto its own line by writing the line break in front of `{`, behind the space that ends `if (a) `; parsed again, the space and the line break are both trivia of `)`. The tree is the same. A tree in which `// leading` stands behind an inserted `{` on the brace's line, as leading trivia of the statement after it, is not the same: parsed again, the comment trails the brace.

Example — a token's value: in `var s = $"""`, followed by the lines `a {Call(1,`, `2)} b`, and `""";`, each starting in column 0, the closing quotes strip no indentation, and the text token in front of the hole stands for `a `. When the indentation phase moves the literal's lines under its opening quotes, every line gains the same indentation, the closing line included, and the moved text token still stands for `a `, as its new text does when it is parsed again; a token that kept its new text, indentation included, as its value would not be the same token.

### FMT-PIPE-022 — Every phase keeps the annotations it does not own

Every phase MUST return each annotation that its input carries on a node or a token of the code on that node or token, for every one its output still holds — also where the phase changes the token's text, the trivia around it, or the node's children, or moves it into a node the phase creates. This holds for the start token, the content tokens, and the closing token of a multi-line raw string literal that the indentation phase moves (FMT-RAW-001), and for every node and token inside its holes. A node or token the phase removes takes its annotations with it, and one the phase creates carries none of the input's, also where it replaces a removed token of the same kind, such as the `;` of a converted expression body. This requirement covers the nodes and tokens of the code only: trivia, and the nodes and tokens inside structured trivia — documentation comments and directives — are outside it. The annotations a phase adds itself are outside it too, and so is a node or token the phase writes more than once, such as the modifiers each field of a split declaration repeats (FMT-STRUCT-021).

Why: A caller marks the nodes it needs to find again with annotations, and the node-level entry points take nodes of the code as targets. `FormatNodeInDocumentWithContextAsync` finds its target in the phases' output through an annotation on the target's first token (FMT-PIPE-018), and a code fix may carry annotations of its own through the formatter. A phase that rebuilds an element instead of editing it drops them: the caller then finds nothing, or a node in another place, and an entry point that cannot find its target cannot format it.

Example — an annotation on the token `Call` inside the interpolation hole of the literal below stays on that token after the indentation phase moves the literal's content lines, its hole line, and its closing line under the opening quotes, and so does an annotation on the text token that holds the content `a `:

```csharp
class C
{
    void M()
    {
        var s = $"""
            a {Call(1,
            2)} b
            """;
    }
}
```

```csharp
class C
{
    void M()
    {
        var s = $"""
                 a {Call(1,
                         2)} b
                 """;
    }
}
```
