# Formatter — Pipeline

| | |
|---|---|
| ID prefix | FMT-PIPE |
| Status | Active |
| Code owners | `ReihitsuFormatter`, `FormattingPipeline`, `IFormattingPhase`, `PreparationPhase`, `FormattingContext`, `PrecedingTokenFacts`, `RootListPosition`, `RootConstruct`, `RootConstructExtensions`, `TriviaFact`, `FluentChainBoundary`, `ReihitsuFormatterHelpers`, `LanguageVersionResolver`, `LineEndingUtilities`, `BlockWrapTriviaUtilities` |
| Uses | RH3208-003, RH7101-006, RH7201-004, SHARED-BLANK-002, SHARED-BLANK-008, SHARED-CHAIN-001, SHARED-COMMENT-012, SHARED-DIAG-001, SHARED-FIX-001, SHARED-FIX-005, SHARED-FIX-006, SHARED-FIX-007, SHARED-USING-009, SHARED-USING-010, FMT-BLANK-011, FMT-BLANK-013, FMT-BLANK-014, FMT-BLANK-018, FMT-BREAK-001, FMT-BREAK-006, FMT-BREAK-007, FMT-BREAK-011, FMT-BREAK-012, FMT-BREAK-014, FMT-BREAK-016, FMT-BREAK-021, FMT-BREAK-022, FMT-BREAK-026, FMT-BREAK-029, FMT-BREAK-033, FMT-BREAK-036, FMT-CLEAN-001, FMT-CLEAN-005, FMT-CLEAN-007, FMT-CLEAN-008, FMT-DOC-001, FMT-EOL-001, FMT-EOL-002, FMT-EOL-003, FMT-EOL-004, FMT-INDENT-002, FMT-INDENT-003, FMT-INDENT-004, FMT-INDENT-007, FMT-INDENT-008, FMT-INDENT-012, FMT-INDENT-018, FMT-INDENT-022, FMT-INDENT-024, FMT-INDENT-028, FMT-INDENT-030, FMT-RAW-001, FMT-RAW-005, FMT-RAW-007, FMT-SPACE-002, FMT-STRUCT-001, FMT-STRUCT-004, FMT-STRUCT-017, FMT-STRUCT-021, FMT-STRUCT-023, FMT-STRUCT-026, FMT-SWITCH-003, FMT-SWITCH-008, FMT-TAB-001, FMT-TAB-002, FMT-TAB-003, FMT-USING-003, FMT-USING-010 |

This spec covers what holds for the formatter as a whole: the public entry points, the fixed phase order, and the invariants every phase must keep. What an individual phase decides is specified in that phase's own spec.

## Non-goals

- The decisions of a single phase — each phase has its own spec, or two for the indentation phase, listed in FMT-PIPE-001.
- File and directory walking, skipped file names, `--check`, `--dry-run`, and exit codes — those belong to the CLI specs.
- Configuration. The formatter applies one fixed style and reads no `.editorconfig`, rule set, or option.
- Formatting of code with syntax errors or of generated code (see FMT-PIPE-005 and FMT-PIPE-006).
- Node-level inputs that none of the repository's own callers passes (`AGENTS.md`, *Supported inputs*). The node-level requirements decide the nodes a code fix passes. Their examples call the entry points directly with such nodes, and a node no caller passes is decided only as far as a requirement names it.

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
| 6 | `LineBreakPhase` | [line-breaks.md](line-breaks.md) (FMT-BREAK) |
| 7 | `SwitchCaseBracePhase` | [switch-case-braces.md](switch-case-braces.md) (FMT-SWITCH) |
| 8 | `BlankLinePhase` | [blank-lines.md](blank-lines.md) (FMT-BLANK) |
| 9 | `HorizontalSpacingPhase` | [horizontal-spacing.md](horizontal-spacing.md) (FMT-SPACE) |
| 10 | `IndentationPhase` | [indentation.md](indentation.md) (FMT-INDENT) and [raw-string-alignment.md](raw-string-alignment.md) (FMT-RAW) |
| 11 | `CleanupPhase` | [cleanup.md](cleanup.md) (FMT-CLEAN) |

The first phase is the preparation phase, whose two sub-phases, TabReplacement and LineEndings, run in this order. Each sub-phase is a phase of its own: it receives the output of the step before it and is reported to the pipeline observer and the trace under the name in the table. The preparation phase is not reported as a step of its own; every other phase is reported under its type name.

A phase MUST NOT compute the column of a token, a comment, or a line from a move or a replacement that a phase after it applies, neither by repeating that phase's rule nor by adjusting for its result: the tab width of FMT-TAB-001 and the raw-string move of FMT-RAW-001 are applied before or in the phase that measures them. This requirement specifies columns; FMT-PIPE-025 states the same rule for line breaks and blank lines.

Why: Later phases rely on the shape earlier phases produce — tabs and line endings are normalized before any other phase runs (FMT-PIPE-019); line breaks are decided before the switch-brace phase measures whether a section spans several lines (FMT-SWITCH-003, FMT-SWITCH-008); blank lines are decided after the line-break phase has put every statement, section, and brace on its line and after the switch-brace phase has inserted its blocks, so the blank-line phase counts them on the lines the output keeps; line breaks and blank lines are final before spacing and indentation measure columns; and raw strings are aligned in the phase that places their opening quotes. A phase may assume the phases before it ran; it must not assume anything about the phases after it. A phase that predicts a later one makes one decision in two places, and every difference between the prediction and the later phase is output that the next pass changes again (FMT-PIPE-004).

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

Formatting already formatted output MUST return it unchanged: running the formatter twice MUST give the same text as running it once. FMT-PIPE-027 requires the same of every phase that runs alone on the output.

Why: The CLI's `--check` mode, the analyzer code fixes that delegate to the formatter, and CI gates all assume that formatted code stays formatted. A non-idempotent result shows up as an endless diff.

### FMT-PIPE-005 — Code with syntax errors is left untouched

`FormatDocumentAsync`, `FormatSyntaxTree`, `FormatNodeInDocumentAsync`, and `FormatNodeInDocumentWithContextAsync` MUST return their input unchanged — for the two node-in-document entry points, the document, whichever node of it is the target or the context — when the syntax tree contains any error-level diagnostic. `FormatNode` checks no syntax errors (FMT-PIPE-012), and neither does `OrganizeUsingDirectivesInDocumentAsync` (FMT-PIPE-030).

Why: Roslyn's error recovery invents missing tokens and attaches skipped text in unpredictable places. Rewriting trivia around them can move or lose code the user is in the middle of typing.

Example:

```csharp
class C{void M(){int x=1}}
```

is returned unchanged, because the `;` after `1` is missing. The same document is returned unchanged by `FormatNodeInDocumentAsync` with the method `M` as target, and by `FormatNodeInDocumentWithContextAsync` with `M` as target and the class `C` as context.

### FMT-PIPE-006 — Generated code is left untouched

`FormatDocumentAsync` and `FormatSyntaxTree` MUST return their input unchanged when a comment in the leading trivia of the file's first token contains `<auto-generated` or `<autogenerated` (case-insensitive). A marker anywhere else in the file MUST NOT disable formatting. The entry points that only code fixes call — `FormatNode` (FMT-PIPE-012), `FormatNodeInDocumentAsync`, `FormatNodeInDocumentWithContextAsync`, and `OrganizeUsingDirectivesInDocumentAsync` (FMT-PIPE-030) — MUST NOT check the marker: they format their input whatever the file's header says.

Why: Generated files are rewritten by their generator; formatting them only produces churn. Only the file header counts, so a comment that merely mentions the marker in the middle of a file does not switch the formatter off. Whether code is generated is the analyzers' decision for the code fixes (SHARED-DIAG-001): a fix runs only where an analyzer reported, and a file the analyzers analyze — such as one the user marks `generated_code = false` — is one whose fixes must work. A second check in the entry points would refuse the fix's formatting where the analyzer reported, so the fix would change nothing or leave its edit unformatted.

Example:

```csharp
// <auto-generated />
class C{void M(){int x=1;}}
```

is returned unchanged by `FormatDocumentAsync` and `FormatSyntaxTree`, while a marker inside the type does not count:

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

Example — `FormatNodeInDocumentAsync` with the method `M` as target formats `M` in a file whose header marks it as generated:

```csharp
// <auto-generated />
class C
{
    void M(){int x=1;}
}
```

```csharp
// <auto-generated />
class C
{
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

The formatter MUST expose its behavior through `ReihitsuFormatter` on Roslyn objects — `FormatDocumentAsync`, `FormatSyntaxTree`, `FormatNode`, `FormatNodeInDocumentAsync`, `FormatNodeInDocumentWithContextAsync`, `OrganizeUsingDirectivesInDocumentAsync` (FMT-PIPE-030), and `SplitsFieldDeclaration` (FMT-PIPE-031) — and MUST NOT read files, the console, or configuration itself. `FormatDocumentAsync` and `FormatSyntaxTree` MUST produce the same text for the same input and language version. A code fix MUST reach a formatter phase or structural transform only through these members: it MUST NOT run a phase, a transform, or another rewriter a phase runs, nor call a member of one, nor use `FormattingPipeline`. The rule is keyed on those types, not on a namespace: a formatter helper or utility class that is none of them stays callable, such as `ReihitsuFormatterHelpers` or a utilities class that a transform also calls, like `ExpressionBodiedTransformUtilities`, wherever it is declared.

Why: The CLI, the analyzer code fixes, and the Playground share one engine. Keeping file access and reporting out of the formatter means every host formats identically and a host-specific behavior can never leak into another host. A code fix that runs a phase or a transform on its own takes that component out of the pipeline, so the component has to lay out its own output for the fix, and a change to the pipeline's division of work cannot see the fix; through `ReihitsuFormatter`, every path a code fix takes into the formatter is one this spec names.

### FMT-PIPE-010 — Node-level entry points format only their target

`FormatNodeInDocumentAsync` and `FormatNodeInDocumentWithContextAsync` MUST rewrite only the target node and keep the rest of the document as it is. There are two exceptions. The first is the whitespace that ended the line before a documentation comment the target moves onto its own line, which MUST be removed as document-level formatting removes it. The second is the whitespace between a target of `FormatNodeInDocumentWithContextAsync` that the split makes several fields of and another member on its line, which FMT-PIPE-029 replaces by a line break. `FormatNodeInDocumentAsync` MUST keep the target's leading trivia when the target does not start its line or is a direct child of the root; `FormatNodeInDocumentWithContextAsync` MUST keep it in every case (FMT-PIPE-018). Both MUST keep the target's trailing trivia, apart from the whitespace FMT-PIPE-029 replaces behind the target. They format the target, or the context that holds it, as a root of its own (FMT-PIPE-023), and what the phases know of the document around that root is the facts FMT-PIPE-024 lists.

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

### FMT-PIPE-012 — `FormatNode` skips the file-level guards and lays out the node at its depth

`FormatNode` MUST format the node it is given without checking for syntax errors or the generated-code marker. It MUST lay out the node at the depth its `indentLevel` argument gives when that is zero or greater, and otherwise at the depth the indentation phase gives the node's first token at its position in its tree: one level per enclosing scope (FMT-INDENT-002, FMT-INDENT-003) and one per unbraced embedded statement that the node is or lies in (FMT-INDENT-004), so an `else if` adds none. An object or collection initializer or an anonymous object that the node lies in MUST NOT add a level. A node without a parent has depth 0.

`FormatNode` MUST NOT read the code in front of the node or behind it: it supplies no preceding token and no list position (FMT-PIPE-024), so a node that is part of a tree gets the result that the same node without a parent gets at the same depth.

Why: `FormatNode` receives freshly generated or detached subtrees that have no file header and whose tree diagnostics say nothing about the file they will be inserted into. The caller is responsible for passing a node that is safe to format, and it inserts the result where it decides, so the code around the node in the tree it came from says nothing about the result's surroundings. The depth is the indentation phase's own, so a node that the indentation phase places by its depth lands in the column document-level formatting gives it; a second depth rule would disagree with it. A node that the indentation phase aligns to an anchor outside the node instead — a member of an initializer (FMT-INDENT-022 to FMT-INDENT-024), a statement in a lambda block passed as an argument, an arm of a switch expression — lands where its depth puts it, not where the anchor would: no depth expresses the anchor's column, and `FormatNode` does not read the code around the node.

Example — `FormatNode` on the embedded statement of `if (a)`:

```csharp
class C
{
    void M(bool a)
    {
        if (a)
            Call(1,
            2);
    }
}
```

returns the statement one level deeper than the `if`, with `2` aligned under `1` (FMT-INDENT-012):

```csharp
            Call(1,
                 2);
```

The same holds for the embedded statement of `else` and of a loop. With `indentLevel: 0`, the result is `Call(1,` and `     2);` with five spaces; with `indentLevel: 3`, it is the block above.

Example — `FormatNode` on the assignment `P = Call(1,` / `2)` inside `var x = new D` / `{` / … / `};` in the body of `M`: the initializer adds no level, so `P` starts in column 8 and `2` in column 17. Document-level formatting puts `P` in column 20, four columns right of the initializer's brace, which `new` places in column 16 (FMT-INDENT-022).

Example — `FormatNode` on the property `Other`, whose documentation comment is written behind the closing brace of the property above it:

```csharp
class C
{
    public int Value { get; set; } /// <summary>Trailing summary.</summary>
    public int Other { get; set; }
}
```

returns the property with the comment in front of it and no line break in front of the comment, because the token in front of the node is not read; the result starts with `    /// <summary>`, followed by `    /// Trailing summary.`, `    /// </summary>`, and `    public int Other { get; set; }`.

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

`FormatNodeInDocumentAsync` and `FormatNodeInDocumentWithContextAsync` MUST NOT change the column of a line after the last phase: every line they write back MUST end in the column the indentation phase gives it (FMT-PIPE-015), apart from the leading trivia they keep (FMT-PIPE-010, FMT-PIPE-018). The indentation phase MUST lay out the node they format — the target, or the context node — with the columns it gives that node at its depth, shifted by the node's offset: the column of the node's first token in the document, less the column the phase gives that token at the node's depth. The first token therefore stays in its column, a line placed by the depth keeps its place relative to the first token, and a line aligned to an anchor follows the anchor where the phase puts it. This holds for every line written back, also for a line that starts inside a comment that began on an earlier line, for the lines aligned to an anchor on such a line (FMT-INDENT-030), and for the content, hole, and closing lines of a multi-line raw string literal (FMT-RAW-001, FMT-RAW-007). When the first token stands behind the last line of a multi-line comment, the phase does not place its line (FMT-INDENT-007), the token keeps its column, and the offset is zero.

The column of the first token in the document is the number of characters in front of it on its line, each one column, with two exceptions:

- A tab in whitespace that belongs to the node and that the entry point does not keep — the leading whitespace of a target of `FormatNodeInDocumentAsync` that starts its line and whose leading trivia FMT-PIPE-010 does not keep — counts with the width the preparation phase gives it (FMT-PIPE-019). `FormatNodeInDocumentWithContextAsync` keeps its target's leading trivia in every case (FMT-PIPE-018), so for it every character in front of the context's first token counts as one column.
- A byte order mark at the start of the text counts as zero columns, as in the layout the phases compute; the raw-string move still counts it as FMT-RAW-005 states.

So a tab in the whitespace in front of a node that does not start its line, which belongs to the code in front of the node and is kept, counts as one column, and so does a tab inside a comment or a literal, which is kept (FMT-TAB-003). The leading trivia that FMT-PIPE-010 and FMT-PIPE-018 keep is written back as it stands and is outside this requirement, and so are the lines of the context node outside the target, which `FormatNodeInDocumentWithContextAsync` does not write back.

The target of `FormatNodeInDocumentWithContextAsync` MUST keep its first token on the line and in the column it has in the document, also where the phases' layout of the context would put that token on another line or in another column, and every other line of the target MUST end in the column the indentation phase gives it when it lays the target out as if that token stood in that column: a line aligned to an anchor follows the anchor where it ends up (FMT-INDENT-030), and a line the indentation phase places by the depth stays where the context's depth places it (FMT-INDENT-002). The entry point writes back only the target and keeps everything in front of it as it stands (FMT-PIPE-010, FMT-PIPE-018), so the column of the target's first token in the document is the number of characters in front of it on its line, each one column, a tab included; a byte order mark at the start of the text counts as zero columns. The indentation phase takes that column, and the target's first token in the root, from the target-position fact (FMT-PIPE-024). Where the phases' layout leaves the target's first token in the root, and what they write in front of it on its line there, is not written back and decides no column of the result: the result's first line is the document's text in front of the target followed by the target's first line, and every other line of the target is laid out from the column the fact gives.

This holds in each way the phases' layout of the context can differ from the document in front of the target, and the code fixes reach each of them:

- The target starts a later line of its context, and the context's layout gives that line another column or joins it to the line in front of it: the RH5111 fix of a variable declarator in its declaration — a local declaration, a field, or the initializer of a `for` statement, the first declarator on the line after its type included —, of a parameter in its parameter list, the first parameter on the line after `(` included, which the layout joins to `(`, and of an enum member in its enum.
- The target does not start its line, and the context's layout breaks the line in front of it or changes the spacing in front of it on that line: the RH5111 fix of a parameter behind another parameter and of an enum member behind `{`, which the layout puts on a line of their own, and of a variable declarator, a parameter, or an enum member behind more than one space — behind its type, behind `(`, or behind the sibling in front of it —, which the layout puts one space behind it; and the RH3208 fix of a local function behind a label and more than one space.
- The target does not start its line, and the phases move the line it starts on, such as the closing line of a multi-line raw string literal that the indentation phase moves under its opening quotes (FMT-RAW-001): the RH5111 fix of a variable declarator or a parameter behind that closing line.

A target whose first token the context's layout leaves on its line and in its column is laid out the same way, and its result is the context's layout: that covers a target that starts the context's first line, a target on a later line whose column the layout keeps, and a target that does not start its line and in front of which the phases change nothing on that line, such as the first parameter directly behind `(`, or a variable declarator, a parameter, or an enum member one space behind the sibling in front of it in the RH5111 fix, and a local function directly or one space behind its label in the RH3208 fix.

Example — the first way: the RH5111 fix of the declarator `b`, which passes `b` as target and the local declaration as context. The context's layout puts `b` in column 8; `b` keeps column 14, and `2` stands under `1` in column 24:

```csharp
class C
{
    static int Other(int x, int y)
    {
        return x + y;
    }

    void M()
    {
        int a = 1,
              b =
                  Other(1,
                        2);
    }
}
```

```csharp
class C
{
    static int Other(int x, int y)
    {
        return x + y;
    }

    void M()
    {
        int a = 1,
              b = Other(1,
                        2);
    }
}
```

Example — the first way, with lines aligned to keywords on the target's first line: the same fix with an object creation as the value puts the braces under `new` in column 18 and the member one level deeper (FMT-INDENT-022), and with a lambda as the value puts the block under `(` in column 18 (FMT-INDENT-028):

```csharp
using System;

class C
{
    int P { get; set; }

    void M()
    {
        C a = null,
              b =
                  new C
                  {
                      P = 1
                  };
        Action c = null,
              d =
                  () =>
                  {
                      M();
                  };
    }
}
```

```csharp
using System;

class C
{
    int P { get; set; }

    void M()
    {
        C a = null,
              b = new C
                  {
                      P = 1
                  };
        Action c = null,
              d = () =>
                  {
                      M();
                  };
    }
}
```

Example — the first way, for an enum member and a parameter: the RH5111 fix of `B`, with the enum as context, and of `c`, with the parameter list as context. `B` keeps column 6 and `+` stands under `1` in column 10 (FMT-INDENT-018); `string` keeps column 16 and `+` stands under `"x"` in column 27:

```csharp
enum E
{
    A = 1,
      B =
      1 +
      2
}

class C
{
    void M(int a,
                string c
               = "x" +
                 "y")
    {
    }
}
```

```csharp
enum E
{
    A = 1,
      B = 1
          + 2
}

class C
{
    void M(int a,
                string c = "x"
                           + "y")
    {
    }
}
```

Example — the second way: the RH5111 fix of the parameter `c`, with the parameter list as context. The context's layout puts `string c` on a line of its own; the line break is not written, `string` keeps column 18, and `+` stands under `"x"` in column 29:

```csharp
class C
{
    void M(int a,
           int b, string c
               = "x" +
                 "y")
    {
    }
}
```

```csharp
class C
{
    void M(int a,
           int b, string c = "x"
                             + "y")
    {
    }
}
```

Example — the second way: the RH3208 fix of the local function `F`, which passes `F` as target and the labeled statement as context. The context's layout puts one space behind `L:`; `int` keeps column 14, `int b` stands under `int a` in column 20 (FMT-INDENT-012), and the block, which the depth places, starts in column 8 (RH3208-003):

```csharp
class C
{
    void M()
    {
        L:    int F(int a,
                    int b) => a + b;
    }
}
```

```csharp
class C
{
    void M()
    {
        L:    int F(int a,
                    int b)
        {
            return a + b;
        }
    }
}
```

Example — the third way: the RH5111 fix of the declarator `b`, with the local declaration as context. The phases move the literal's lines under its opening quotes, but only `b` is written back, so the closing line keeps column 12, `b` keeps column 17, and `+` stands under `"y"` in column 21:

```csharp
class C
{
    void M()
    {
        string a = """
            x
            """, b =
            "y" +
            "z";
    }
}
```

```csharp
class C
{
    void M()
    {
        string a = """
            x
            """, b = "y"
                     + "z";
    }
}
```

Example — the first way, with a join: the RH5111 fix of the first parameter `a` on the line after `(`, with the parameter list as context. The context's layout joins `int a` to `(`; the join is not written, `int` keeps column 8, and `+` stands under `1` in column 16:

```csharp
class C
{
    void M(
        int a
            = 1 +
              2)
    {
    }
}
```

```csharp
class C
{
    void M(
        int a = 1
                + 2)
    {
    }
}
```

Example — the second way, with a line break: the RH5111 fix of the enum member `A` behind `{`, with the enum as context. The context's layout puts `A` on a line of its own; the line break is not written, `A` keeps column 9, and `+` stands under `1` in column 13:

```csharp
enum E { A =
           1 +
           2 }
```

```csharp
enum E { A = 1
             + 2 }
```

Example — the second way, with spacing: the RH5111 fix of the declarator `a` behind its type and four spaces, with the local declaration as context. The context's layout puts one space between `int` and `a`; `a` keeps column 15, and `2` stands under `1` in column 25:

```csharp
class C
{
    static int Other(int x, int y)
    {
        return x + y;
    }

    void M()
    {
        int    a =
            Other(1,
                  2);
    }
}
```

```csharp
class C
{
    static int Other(int x, int y)
    {
        return x + y;
    }

    void M()
    {
        int    a = Other(1,
                         2);
    }
}
```

Example — the other side: the RH3208 fix of `F` one space behind its label. The context's layout leaves `int` in column 11, and `int b` stands under `int a` in column 17:

```csharp
class C
{
    void M()
    {
        L: int F(int a,
                 int b) => a + b;
    }
}
```

```csharp
class C
{
    void M()
    {
        L: int F(int a,
                 int b)
        {
            return a + b;
        }
    }
}
```

Why: A code fix formats only the code its diagnostic is about (FMT-PIPE-010), and the columns of that code have to be the columns the formatter gives it, so that formatting the result changes nothing in it (SHARED-FIX-007). When the node is formatted as if it started at another column and its lines are moved afterwards, the move reaches only lines that start with whitespace: the line behind a multi-line comment's last line and the lines of a raw string literal stay where the phases left them, while the lines aligned to them move, and a tab in the node's own leading whitespace counted as one column by the move but as four by the phases puts the node a few columns off. Laying out the node at its own column leaves nothing to move, so the indentation phase is the only owner of a line's final column for node-level targets too, as it is for documents (FMT-PIPE-001). Whitespace in front of the node that the entry point does not rewrite is counted as it stands, as the rules that report the code count it, so a fix's output is measured by its rule in the column the fix gave it (SHARED-FIX-001). The target of the context entry point stays where the document has it, because everything in front of it is written back as it stands; laying out its other lines from the column the context's layout would have given it put a line aligned to an anchor on the target's first line off that anchor, a result the next formatting moves again and that an alignment rule reports where one covers the line, such as RH5203 for a wrapped argument and RH5301 for the braces of an object initializer. Keeping the target's first token in place is the decision `FormatNodeInDocumentAsync` takes for its own target, whose first token keeps its column too.

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

`FormatNodeInDocumentWithContextAsync` MUST write back the target's leading trivia as it stands in the document, in every case: its blank lines, its comments and directives in their columns, and the whitespace in front of the target's first token. The blank-line requirements (FMT-BLANK-011, FMT-BLANK-013, FMT-BLANK-014) and the columns the indentation phase gives those lines do not change it. The one exception is a target that FMT-PIPE-029 moves off the line of another member in front of it: its leading trivia is written back as it stands, but the line break written in front of it puts what starts the new line where the context's depth places a member, not in the column the target's first token has in the document. This requirement does not decide the leading trivia of a target of `FormatNodeInDocumentAsync`, which FMT-PIPE-010 states.

Writing that trivia back MUST NOT move a line of the target that the indentation phase lays out: the trivia written back puts the target's first token on its line and in its column in the document (in the exception above, in the column the line break gives it), and the indentation phase lays out the target's other lines from that column, which the target-position fact gives it (FMT-PIPE-017, FMT-PIPE-024), not from the column the token has in the root. What the phases do in the root with that trivia and with the code in front of the target — a blank line they add or remove in it, a comment or a directive they move, a line break they add or remove in front of the target's first token, the spacing they write there, or a move of the line that token stands on — is not written back and decides no line or column of the result. For a target that does not start its line, what stands in front of it on that line is outside the target and stays as in the document (FMT-PIPE-010). The write-back also restores the tabs and line endings the preparation phase rewrites in every root (FMT-PIPE-019).

Why: The entry point serves code fixes whose target starts a later line of the context, such as the RH5111 fix of an enum member, a parameter, or a declarator. A fix must not change the blank lines between the code it changes and its neighbours (SHARED-FIX-006), and keeping the trivia as it stands keeps them. The phases lay out the whole context and may decide that trivia differently, but the entry point discards their decision; a line of the target laid out from the place the discarded decision gave the first token — the lines aligned to that token — would stand off it. Laying those lines out from the column the written-back trivia gives the first token makes the write-back agree with the layout, whatever the phases did with the trivia in the root, so no phase needs to leave the trivia alone for it; and the preparation phase can replace tabs and line endings in the whole root before any other phase reads it while a tab-indented file keeps its tabs in front of the target.

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

### FMT-PIPE-023 — Node-level entry points format a detached root

`FormatNode`, `FormatNodeInDocumentAsync`, and `FormatNodeInDocumentWithContextAsync` MUST give the pipeline a root without a parent — the node, the target, or the copy of the context that marks the target — as the root of a tree of its own. A phase MUST NOT read a token, a trivia, a node, or the text of the document outside that root, neither through the root's ancestors or its neighbouring tokens nor through the text of the tree the root came from; what a phase needs from there MUST come from the facts FMT-PIPE-024 lists. So the output of a node-level entry point MUST NOT depend on whether a phase changes the root before another phase reads it: a target that holds a tab in its whitespace, which the preparation phase replaces (FMT-PIPE-019), MUST get the output the same target with spaces gets.

Why: A root that is still part of its document has two answers to every read across its boundary: the document's neighbour while no phase has changed the root, and none once a phase has returned a changed copy. Which answer a phase gets would depend on whether an earlier phase changed something, such as a tab the preparation phase replaces (FMT-PIPE-019), and the same code would be formatted in two ways. A detached root gives one answer, and the facts say what the document holds.

Example — `FormatNode` on the statement `Call(1, 2);`:

```csharp
class C
{
    void M()
    {
        var a = 1;

        Call(1, 2);
    }
}
```

returns `        Call(1, 2);`, without a line break in front of it, and returns the same when the statement holds a tab after the comma instead of the space. `FormatNodeInDocumentAsync` with the same statement as target keeps the blank line above it in both cases, as it decides that blank line from the facts about the code in front of the target (FMT-PIPE-024), and returns the document as it is, the tab replaced by one space.

Example — `FormatNodeInDocumentAsync` with the `if` of the `else if` as target keeps `// note` behind its condition, with and without a tab in the target's condition:

```csharp
class C
{
    void M(bool x, bool y)
    {
        if (x)
        {
        }
        else if (y) // note
        {
        }
    }
}
```

```csharp
class C
{
    void M(bool x, bool y)
    {
        if (x)
        {
        }
        else if (y) // note
             {
             }
    }
}
```

The braces follow the target's first token, which keeps its column in the document (FMT-PIPE-017).

### FMT-PIPE-024 — Node-level input facts are plain values

The node-level entry points MUST give the phases the facts in the table below about the document around their root, and no other, besides the line ending (FMT-PIPE-007), the language version (FMT-PIPE-008), and the transforms and rewrites they skip (FMT-PIPE-011, FMT-PIPE-028). Every fact MUST be a plain value — a kind, a flag, a number, the kind and text of a trivia, or an annotation that marks a token of the root — and MUST NOT hold a node, a token, or a trivia of the document.

| Fact | Value | `FormatNode` | `FormatNodeInDocumentAsync` | `FormatNodeInDocumentWithContextAsync` |
|---|---|---|---|---|
| Depth | The number of levels of the root, as FMT-PIPE-012 states it for a root without an explicit `indentLevel` | the node, or its `indentLevel` | the target | the context |
| First-token column | The column of the root's first token in the document (FMT-PIPE-017), or none when the root is the document's root | none | the target | the context |
| Target position | For the target of a context: the column of the target's first token in the document as FMT-PIPE-017 counts it, and the target marked in the root by an annotation on its first token (FMT-PIPE-022) | none | none | the target |
| Documentation boundary | Whether the root is something other than a compilation unit; when it is, one line break in front of a single-line documentation comment that leads the root is kept while the root's leading blank lines are removed | yes | yes | yes |
| Preceding token | The kind of the token in front of the root in the document, whether it is the colon of a switch label, and the kind and text of each of its trailing trivia, in order; none when no token stands in front of the root | none | the target | none |
| List position | The kind of the statement in front of the root in its block or switch section and the kind of that statement's last token, whether the switch section in front of the root ends in a `break` statement, and whether the root is a `break` statement that ends its switch section | none | the target, when it starts its line | none |
| Chain continuation | Whether the root's parent continues the root's member-access chain (SHARED-CHAIN-001), so that the root is not the outermost node of its chain | none | the target | none |
| Conditional-access branch | Whether the root is the part of a conditional access behind its `?`, so that an assignment that is the root continues the chain of its left side | none | the target | none |
| One-line property | Whether the root is an accessor or an accessor list of a property, indexer, or event declaration that stands on one line apart from its attribute lists; it counts only while the root itself holds no line break, in its text or its leading trivia | none | the target | none |
| List element | Whether the root is, or lies inside, an element of an argument, attribute-argument, parameter, type-parameter, function-pointer-parameter, tuple, attribute, or type-argument list, with no construct between the root and that element that lays out the lines inside it: a brace scope, an initializer, an anonymous object, a switch expression, a collection expression, a recursive, list, or parenthesized pattern, an accessor list, an interpolation hole, or a query body | none | the target | none |
| Element-access arguments | Whether the root is the bracketed argument list of an element access or of an implicit element access | none | the target | none |
| Case-label pattern | Whether a `case` label introduces the root as its pattern, directly or through `and`, `or`, and `not` patterns | none | the target | none |
| Assigned collection initializer | Whether the root is a collection initializer on the right of an assignment | none | the target | none |

`FormatNodeInDocumentAsync` gives a list position only to a target that starts its line and is a statement directly inside a block or a switch section, or a switch section. A block statement gets one like any other statement.

These decisions MUST read the facts instead of the neighbour or the ancestor they would read if the root were part of the document:

- the blank-line phase decides the root's leading blank lines from the preceding token — its kind, and whether it ends its line — the list position, and the documentation boundary;
- the documentation phase decides from the preceding token whether a documentation comment that leads the root is written behind code;
- the rule of SHARED-CHAIN-001 that only the outermost node of a chain is laid out as a chain decides, for every node of the root, whether that node is the outermost node of its chain; where the rule would read above the root, it reads the chain-continuation and conditional-access-branch facts instead. The line-break phase lays out, and the indentation phase aligns, a chain only at the node the rule names;
- the line-break phase, from the other construct facts, keeps the attribute lists of an accessor of a one-line property on the property's line while the root holds no line break, joins the interior wraps of a root inside a list element, may collapse the arguments of an element access onto one line, keeps the opening brace of a recursive pattern a `case` label introduces on the label's line, and keeps the opening brace of an assigned collection initializer on the assignment's line;
- the indentation phase lays out the root from the depth and the first-token column, lays out the lines of the target the target position marks as if that target's first token stood in that fact's column, and aligns the comments that lead the root from the preceding token. The target position is read by the indentation phase alone: where the earlier phases leave the target's first token in the root, and what they write in front of it or in its leading trivia there, is not written back and decides no column of the result (FMT-PIPE-017, FMT-PIPE-018).

What follows the root's last token MUST be decided as at the end of a file: the node-in-document entry points write back the target's trailing trivia from the document (FMT-PIPE-010), and the caller of `FormatNode` decides what follows the node.

Why: A fact that holds a node or trivia of the document carries positions in a tree the root no longer belongs to, and a rule that forbids reading them cannot be enforced. The values in the table are what the phases use: whether the token in front ends its line, whether a comment trails it, where a line starts behind it, the kinds the blank-line rules compare, and, for the construct the root stands in, the one answer each line-break decision takes from the root's ancestors. A decision that got no fact would decide as if the root were a construct of its own: a statement target behind `var a = 1;` would be decided as the first statement of a block and get no blank line in front of it (example below). The entry points differ because their results do: `FormatNodeInDocumentAsync` writes its target back into the document and decides the target's blank lines, documentation, and layout from it, `FormatNodeInDocumentWithContextAsync` writes back its target's leading trivia as it stands in the document (FMT-PIPE-018) and has the indentation phase lay out the target's other lines from the column that trivia leaves its first token in (FMT-PIPE-017), and the caller of `FormatNode` decides what stands around the node (FMT-PIPE-012).

Example — `FormatNodeInDocumentAsync` with the statement `Call(1, 2);` as target, behind `var a = 1;` without a blank line: the list position says that a local declaration stands in front of the statement, so the blank line that FMT-BLANK requires there is inserted, as document-level formatting inserts it:

```csharp
class C
{
    void M()
    {
        var a = 1;
        Call(1, 2);
    }
}
```

```csharp
class C
{
    void M()
    {
        var a = 1;

        Call(1, 2);
    }
}
```

`FormatNode` with the same statement returns `        Call(1, 2);`: it supplies no list position, and its caller decides what stands in front of the node.

### FMT-PIPE-025 — No phase decides a line break or a blank line from a later phase's

A phase MUST NOT decide whether two tokens with text stand on one line, or where a blank line stands, from the line positions or the blank lines that a phase after it gives the output, neither by predicting them with a copy of that phase's rule nor by adjusting for its result. Two phases MAY base their decisions on one shared statement of a condition read from the syntax each of them receives — such as which pairs of statements a blank-line requirement separates; that is one rule in one place, not a prediction.

Whether two tokens with text stand on one line is decided by the phases up to and including the switch-brace phase, before the blank-line phase (FMT-PIPE-001). The blank-line phase adds and removes only blank lines, between lines those phases have already broken, so it decides each blank line on the lines the output keeps. The phases after it MUST NOT change whether two tokens with text stand on one line and MUST NOT add or remove a blank line between them. The line breaks the cleanup phase removes in front of the end-of-file token and at the end of a node-level root (FMT-CLEAN-005, FMT-CLEAN-007, FMT-CLEAN-008) are outside this requirement: the end-of-file token has no text, and nothing stands behind a root's last token.

Why: A phase that predicts a later phase's line breaks or blank lines makes one decision in two places, as FMT-PIPE-001 states for columns, and every difference between the prediction and the later phase is output that the next run changes again (FMT-PIPE-004). With the blank-line phase in front of the line-break phase, a blank line that depends on where a line breaks had to be decided before the line broke: the blank-line phase wrote the line break that ends a statement's or a section's line itself, a decision FMT-BREAK-006 and FMT-BREAK-033 give the line-break phase, the blank-line phase predicted where the line-break phase would break a chain link off the closing line of a comment that spans several lines (FMT-BLANK-018), and the switch-brace phase, which inserts its blocks after both layout phases, had to carry blank lines that the blank-line phase had decided before the blocks existed. A shared statement of a condition is not such a prediction: both phases read the same rule from the syntax in front of them, so they cannot disagree.

Example — sections on one line get their own lines from the line-break phase and the blank line behind `break` from the blank-line phase, in one run:

```csharp
class C
{
    void M(int x)
    {
        switch (x) { case 1: break; case 2: break; }
    }
}
```

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1:
                break;

            case 2:
                break;
        }
    }
}
```

### FMT-PIPE-026 — Whether a comment starts its line is final after the line-break phase

A phase after `LineBreakPhase` MUST NOT change whether a comment starts its line (SHARED-BLANK-002): it MUST NOT move a comment that shares a line with code onto a line of its own, and MUST NOT move a comment that starts its line behind code.

Why: Whether a comment starts its line decides the blank line in front of it (FMT-BLANK-011, FMT-BLANK-018) and the column it is indented to (FMT-INDENT). The blank-line phase decides that blank line once, on the lines it finds; a comment that a later phase moved onto a line of its own would get its blank line only in the next run (FMT-PIPE-004). A documentation comment written behind code is moved by the documentation phase (FMT-DOC-001), before the line-break phase.

Example — the comment behind the condition stays behind it (FMT-BREAK-036), and the output is stable in one run:

```csharp
class C
{
    void M(bool a, bool b)
    {
        while (b)
            if (a) // why
            {
                A();
            }
    }
}
```

is kept unchanged.

### FMT-PIPE-027 — No phase changes the formatter's output

Every phase, each sub-phase of the preparation phase included, MUST leave the output of `FormatDocumentAsync` and `FormatSyntaxTree` unchanged when it runs alone on it: the output's text, parsed with the parse options of the syntax tree the formatter was given and formatted with the line ending FMT-PIPE-007 detects in it, gives a tree that the phase MUST return with the same text.

- **Output of a run that formatted.** The requirement covers the output of an input the formatter formats. An input with a syntax error or a generated-code marker in its file header is returned unchanged (FMT-PIPE-005, FMT-PIPE-006); its output is not formatted output, and the requirement does not cover it. `class C{void M(){int x=1;}}` is formatted, and no phase changes its output; `class C{void M(){int x=1}}` comes out unchanged, and the requirement says nothing about a phase run alone on it.
- **Document and tree entry points.** The requirement covers `FormatDocumentAsync` and `FormatSyntaxTree`. It does not cover the output of `FormatNode`, `FormatNodeInDocumentAsync`, and `FormatNodeInDocumentWithContextAsync`: their phases receive a detached root and the facts of FMT-PIPE-024, not a parsed text. A method that a code fix formats with `FormatNodeInDocumentAsync` is covered once the document that holds it is formatted with `FormatDocumentAsync`.
- **Text, not tree.** The phase's output is compared with the formatter's output as text. A phase that returns the same text with a run of whitespace or a line break given to the other of the two tokens around it (FMT-PIPE-021) changes nothing.

Why: FMT-PIPE-004 compares the text of two whole passes. Two phases that undo each other in every pass — one removes a blank line and a later one writes it back, one strips the indentation of a line and a later one restores it — give the same text in every pass and meet FMT-PIPE-004, although the formatter's own rules disagree about that output: the two phases own one decision, which FMT-PIPE-001 and FMT-PIPE-025 rule out, and an analyzer code fix that applies the rule of one of them writes the output the other one reverts. Running every phase alone on the output finds such a pair, because the phase that undoes the change is not there to repair it. The node-level entry points run the same phases as the document entry points; a phase run alone on their output text would receive neither their root nor their facts, so it would check another input than the one they format.

Example — the blank line above the comment that leads `using System.Linq;` is kept by the using phase (FMT-USING-010) and asked for by the blank-line phase (FMT-BLANK-011), so neither phase changes this output when it runs alone on it:

```csharp
using System;

// About Linq
using System.Linq;

class C;
```

Example — the line-break phase keeps `2);` on a line of its own, and the indentation phase keeps it in the column of the first argument (FMT-INDENT-012), so neither phase changes this output when it runs alone on it:

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

### FMT-PIPE-028 — Node-level entry points apply no region requirement

`FormatNode`, `FormatNodeInDocumentAsync`, and `FormatNodeInDocumentWithContextAsync` MUST NOT apply any of the region phase's requirements (FMT-REGION) to the code they format — not the capitalization of a description, not the form of a `#region` line, not the comment or the space behind an `#endregion`, and not the whitespace at the end of a region directive line. Only `FormatDocumentAsync` and `FormatSyntaxTree` apply them. This requirement exempts region directives from the region phase and from nothing else: the preparation phase runs for these entry points as for every entry point (FMT-PIPE-019), and what it does to a directive, it does to a region directive at node level too.

Why: These entry points serve code fixes, and a code fix changes only the code its diagnostic is about (SHARED-FIX-006). A region's name, its line form, and its `#endregion` comment are what the region rules and whole-document formatting decide; a fix for an unrelated diagnostic that rewrote them would hand the user a second edit nobody asked for, also to a user who does not use the region rules. The preparation phase is not part of the region requirements: it runs before every phase for every entry point, so that no later phase sees what it removes (FMT-PIPE-019), and the node-level entry points share it as a whole.

Example — `FormatNodeInDocumentAsync` with the class `C` as target:

```csharp
class C
{
    #region    fields

    int P { get { return 1; } }

    #endregion
}
```

```csharp
class C
{
    #region    fields

    int P
    {
        get
        {
            return 1;
        }
    }

    #endregion
}
```

Example — not shown as an input block because the input contains tabs: `FormatNodeInDocumentAsync` with the class `C` as target, in a class that holds `#region`, a tab, `bar`, the property `int P { get { return 1; } }`, and `#endregion`, a tab, `// Stale`, lays out the property as in the example above and applies no region requirement: `bar` stays lower-case and the `#endregion` keeps its stale comment. The preparation phase still runs, so the tab behind each keyword becomes four spaces (FMT-TAB-002):

```csharp
class C
{
    #region    bar

    int P
    {
        get
        {
            return 1;
        }
    }

    #endregion    // Stale
}
```

`FormatDocumentAsync` with the same input writes `#region Bar` and `#endregion // Bar`, as the region phase decides.

### FMT-PIPE-029 — The context entry point writes back every field the split makes of its target

When the target of `FormatNodeInDocumentWithContextAsync` is a field declaration that the structural phase splits in the context (FMT-STRUCT-021), the entry point MUST write back, in place of the target, every field the split makes of it, in source order, and nothing else of the context (FMT-PIPE-010). The first of these fields stands in for the target in FMT-PIPE-017 and FMT-PIPE-018: it keeps the target's leading trivia as it stands in the document, and its first token keeps the line and the column the target's first token has there. The last keeps the target's trailing trivia (FMT-PIPE-010), apart from the whitespace in front of another member behind it (below). Every other line of the fields — the lines of the later fields, of the comments in front of them, and the line breaks and blank lines between two of the fields — is written as the phases lay out the context, and ends in the column FMT-PIPE-017 gives the target's other lines: a later field's first line, which the indentation phase places by the depth, stands where the context's depth places a member. A tab in front of the target's first token counts as one column (FMT-PIPE-017), and a later field's indentation is written as the phases write it, in spaces.

The fields MUST NOT share a line with another member of the context, as SHARED-FIX-007 requires of a member a fix changes:

- When another member stands in front of the target on the target's first line, the first field MUST start the next line instead of keeping the target's line and column: the whitespace between that member and the target MUST be replaced by a line break and the indentation the phases lay out there, so the first field's line — the target's leading trivia, such as a documentation comment, and then its first token — starts where the context's depth places a member.
- When another member stands behind the target on the target's last line, that member MUST start the next line: the whitespace between the last field and that member MUST be replaced by a line break and the indentation the phases lay out there, and the member keeps everything else as it stands, its leading trivia included.
- What stands in such a gap stays with the token whose trivia it is. A comment in the trailing trivia of the token in front of the gap — the last token of the other member in front, or of the target — stays where it stands, behind that token, and only the whitespace behind the comment is replaced. A documentation comment is leading trivia of the token behind the gap and goes with its member onto the next line: in front of the target, it starts the first field's line; behind the target, it starts the other member's line.
- The gap gets exactly one line break and no blank line, also where document-level formatting writes a blank line there (SHARED-FIX-006). No whitespace is left at the end of a line.
- Whether another member stands on the target's first or last line is a result of the phases, read from the context as they lay it out; the entry point does not decide it again from the gap in the document (FMT-PIPE-001). It is a question only where nothing in the gap starts a line in the document: no line break in the gap separates the token in front of it — the last token of the other member in front, or of the target — from the token behind it — the target's first token, or the first token of the member behind the target. The line breaks that count are those SHARED-BLANK-008 counts: an end-of-line, and the line break that ends a `///` comment or a directive; a line break inside the text of a comment that spans lines does not count (SHARED-BLANK-002). Where something in the gap starts a line — the first token behind the gap, or a comment or documentation comment in front of it — the target's leading trivia and the trivia behind the target stay as they stand (FMT-PIPE-018, FMT-PIPE-010), also where the token behind the gap does not start its line, such as `private` in `_p;⏎    /** x */ private int _x, _y;`. Where the question arises, the first field, or the member behind the target, starts the next line exactly when, in the context as the phases lay it out, its first token or a documentation comment in its leading trivia starts a line, and keeps its place when neither does. Two phases start such a line: the documentation-comment phase starts the line of a documentation comment written behind code (FMT-DOC-001), and the line-break phase starts a member's line where the gap holds no line break (FMT-BREAK-007). So a gap whose comments each start and end on one line, such as `_p; /* c */ private int _x, _y;`, puts both on one line; a gap that holds a documentation comment puts the member it documents on the next line, also when a comment in the gap spans lines; and a member behind the closing `*/` of a comment that spans lines, with no documentation comment between, keeps its place (SHARED-COMMENT-012). See the examples below.

A target behind or in front of a token that is not a member, such as the `{` or the `}` of its type, keeps its line.

This requirement decides the input the RH7101 code fix passes (`AGENTS.md`, *Supported inputs*): a field declaration with more than one declarator that the split does not refuse (FMT-STRUCT-023), as target, and the class, struct, record, or interface declaration whose member it is, as context. Another combined field declaration of the context, or of a type nested in it, is split in the root like the target and is not written back.

Why: The RH7101 fix replaces one declaration by several, a change the structural phase makes only to a member of a type, so the fix needs the type as context. Writing back the split fields, and only them, leaves every other member of the type as it stands and keeps the blank lines and comments above the declaration (SHARED-FIX-006), while the phases lay out the fields the fix creates as document-level formatting lays them out (SHARED-FIX-007). The blank lines between two of the fields are inside the code the fix changes, not between it and a neighbour, so they are the phases' decision. A later field is a line the context's depth places, like any line of a target that the indentation phase places by the depth (FMT-PIPE-017). A member that would share its line with another member after the fix is the one place where the fix changes what stands around the declaration, because SHARED-FIX-007 gives each member a fix changes a line of its own and has the other member start on the next line. A comment in that gap stays with the code it was written behind, and a documentation comment goes with the member it documents, as document-level formatting places both: the target's leading trivia is written back as it stands (FMT-PIPE-018), and the other member's leading trivia is code the fix does not change (SHARED-FIX-006). The gap gets no blank line, because the blank lines between the changed code and its neighbours are not the fix's to change (SHARED-FIX-006). Whether another member shares the target's line is decided by the phases, by more than one of them: the documentation-comment phase moves a documentation comment written behind code, and its member with it, before the line-break phase decides whether a gap needs a line break (FMT-DOC-001, FMT-BREAK-007). The entry point reads that decision from the context the phases have laid out instead of predicting it from the gap in the document by a rule of its own (FMT-PIPE-001): a prediction restates one phase's rule and misses what an earlier phase changed, so it would move a member document-level formatting leaves, or leave one it moves. Read from the phases' result, the fix moves exactly the members document-level formatting moves. Where something in the gap already starts a line in the document, the entry point keeps the gap's trivia as it stands (FMT-PIPE-010, FMT-PIPE-018), even where document-level formatting moves a documentation comment in it: what starts that line does not share a line with the member, and the trivia is code the fix does not change (SHARED-FIX-006). A documentation comment that starts its line counts like the token behind it, because the phases start the line of that comment, not of the token: asking the question there would write a second line break into a gap that already has one.

Example — `FormatNodeInDocumentWithContextAsync` with the field `_x, _y` as target and the class as context; the comment in front of `_y` gets the blank line above it that the blank-line phase writes, and the other field of the class and the field of `D` stay as they are:

```csharp
class C
{
    private int _x, // horizontal
                /* vertical */ _y;
    private int _a, _b;

    class D
    {
        private int _c, _d;
    }
}
```

```csharp
class C
{
    private int _x; // horizontal

    /* vertical */
    private int _y;
    private int _a, _b;

    class D
    {
        private int _c, _d;
    }
}
```

Example — the target off its member column: the first field keeps the target's column, and the later field stands where the class's depth places a member:

```csharp
class C
{
      private int _x, _y;
}
```

```csharp
class C
{
      private int _x;
    private int _y;
}
```

Example — the target behind another member on its line: the first field starts the next line:

```csharp
class C
{
    private int _p; private int _x, _y;
}
```

```csharp
class C
{
    private int _p;
    private int _x;
    private int _y;
}
```

Example — a member behind the target on its line, and a comment in each gap: `/* c */` stays behind `_p;` and `/* d */` behind `_y;`, and each field and `_q` start a line of their own:

```csharp
class C
{
    private int _p; /* c */ private int _x, _y; /* d */ private int _q;
}
```

```csharp
class C
{
    private int _p; /* c */
    private int _x;
    private int _y; /* d */
    private int _q;
}
```

Example — a comment that spans lines in each gap: document-level formatting leaves `private int _x` behind `b */` and `private int _q` behind `d */`, so the first field keeps the target's line and column, `_q` keeps its place, and only the later field `_y` starts a line of its own:

```csharp
class C
{
    private int _p; /* a
       b */ private int _x, _y; /* c
       d */ private int _q;
}
```

```csharp
class C
{
    private int _p; /* a
       b */ private int _x;
    private int _y; /* c
       d */ private int _q;
}
```

Example — a member in front of the target on its first line, and a comment that spans lines behind the separating comma: the first field starts the next line at the member column, the comment ends the first field's line, and `_y` starts a line of its own behind it (FMT-STRUCT-026):

```csharp
class C
{
    private int _p; private int _x, /* a
       b */ _y;
}
```

```csharp
class C
{
    private int _p;
    private int _x; /* a
       b */
    private int _y;
}
```

Example — a documentation comment in each gap: each goes with the member behind it, and no blank line is written above it, where document-level formatting writes one:

```csharp
class C
{
    private int _p; /** X. */ private int _x, _y; /** Q. */ private int _q;
}
```

```csharp
class C
{
    private int _p;
    /** X. */ private int _x;
    private int _y;
    /** Q. */ private int _q;
}
```

Example — a documentation comment that spans lines in each gap: the documentation-comment phase starts the line of each (FMT-DOC-001), so the first field and `_q` start the next line with it, its later line stays as it stands, and no blank line is written above it:

```csharp
class C
{
    private int _p; /** a
       b */ private int _x, _y; /** c
       d */ private int _q;
}
```

```csharp
class C
{
    private int _p;
    /** a
       b */ private int _x;
    private int _y;
    /** c
       d */ private int _q;
}
```

Example — a comment that spans lines and then a documentation comment in each gap: the comment stays behind the code in front of it, and the documentation comment starts the next line with its member, as in the example above; the first field and `_q` do not keep their place behind `b */` and `e */`, because the documentation comment between starts a line:

```csharp
class C
{
    private int _p; /* a
       b */ /** c */ private int _x, _y; /* d
       e */ /** f */ private int _q;
}
```

```csharp
class C
{
    private int _p; /* a
       b */
    /** c */ private int _x;
    private int _y; /* d
       e */
    /** f */ private int _q;
}
```

Example — a one-line comment and then a documentation comment that spans lines in each gap:

```csharp
class C
{
    private int _p; /* a */ /** b
       c */ private int _x, _y; /* d */ /** e
       f */ private int _q;
}
```

```csharp
class C
{
    private int _p; /* a */
    /** b
       c */ private int _x;
    private int _y; /* d */
    /** e
       f */ private int _q;
}
```

Example — a comment that spans lines and then a documentation comment that spans lines in each gap:

```csharp
class C
{
    private int _p; /* a
       b */ /** c
       d */ private int _x, _y; /* e
       f */ /** g
       h */ private int _q;
}
```

```csharp
class C
{
    private int _p; /* a
       b */
    /** c
       d */ private int _x;
    private int _y; /* e
       f */
    /** g
       h */ private int _q;
}
```

Example — the other side of the condition: the target's first token and `_q`'s first token already start their lines in the document, so the documentation comments stay where they stand, although document-level formatting moves each onto a line of its own; a `///` comment behind `_p;` and behind `_y;` stays the same way:

```csharp
class C
{
    private int _p; /** a
       b */
    private int _x, _y; /** c
       d */
    private int _q;
}
```

```csharp
class C
{
    private int _p; /** a
       b */
    private int _x;
    private int _y; /** c
       d */
    private int _q;
}
```

Example — a documentation comment that starts its line in front of the target and in front of the member behind it, with its member behind it on that line: the gap holds a line break, so the question does not arise; the first field keeps the target's line and column behind `/** x */`, and `/** q */ private int _q;` stays as it stands:

```csharp
class C
{
    private int _p;
    /** x */ private int _x, _y;
    /** q */ private int _q;
}
```

```csharp
class C
{
    private int _p;
    /** x */ private int _x;
    private int _y;
    /** q */ private int _q;
}
```

Example — a target on two lines with a member behind its last line: the member starts the next line:

```csharp
class C
{
    private int _x,
                _y; private int _q;
}
```

```csharp
class C
{
    private int _x;
    private int _y;
    private int _q;
}
```

Example — a tab in front of the target (shown as `→`): the first field keeps the tab, and the later field stands at the member column, indented with spaces:

```csharp
class C
{
→private int _x, _y;
}
```

```csharp
class C
{
→private int _x;
    private int _y;
}
```

### FMT-PIPE-030 — The using entry point organizes one scope's using block

`OrganizeUsingDirectivesInDocumentAsync` MUST rewrite the using block of the scope it is given — a compilation unit, a namespace, or a file-scoped namespace of the document — as the using-directive phase rewrites it (FMT-USING), with the document's line ending (FMT-PIPE-007) and language version (FMT-PIPE-008), and MUST change nothing else of the document: not the directives of a namespace nested in the scope, and not the code after the block. It MUST run no other phase, so the phase's output is the block's final layout, the scope's indentation in front of a directive that the phase moves onto a line of its own included (FMT-USING-003, SHARED-USING-010). The phase MUST read the scope where it stands in the document, not a copy of it as a root of its own (FMT-PIPE-023 does not apply to this entry point): the indentation SHARED-USING-010 takes is that of the document line on which the block's first directive starts, also when the scope does not start that line. It checks neither syntax errors (FMT-PIPE-005) nor the generated-code marker (FMT-PIPE-006).

This requirement decides the input the RH7201–RH7207 code fixes pass (`AGENTS.md`, *Supported inputs*): the scope of the reported directive, whose block holds at least two directives and may be reordered (SHARED-USING-009).

Why: The seven fixes produce the block the phase produces (RH7201-004), and their result must be what FMT-USING specifies, so the phase's output is theirs; formatting the block through every phase would insert a blank line above a comment that leads a directive where the author wrote none, which FMT-USING-010 keeps as written under SHARED-FIX-006. A using block is a list, not a node, so no node-level entry point can take it as its target. An entry point of its own names the path from the fixes into the formatter (FMT-PIPE-009), so a change to the phase is seen as a change to the fixes.

Example — `OrganizeUsingDirectivesInDocumentAsync` with the namespace as scope, as the RH7203 fix passes it; `using System;`, which shared the line of `System.Linq`, comes first, each directive stands on a line of its own at the scope's indentation, and the class is not formatted:

```csharp
namespace N
{
    using System.Linq; using System;

    class C{ }
}
```

```csharp
namespace N
{
    using System;
    using System.Linq;

    class C{ }
}
```

Example — the same with the namespace `B` as scope, which does not start its line: the first directive starts a line without indentation, so `using System.Linq;`, moved onto a line of its own, gets none, and the code behind the block stays on its line:

```csharp
namespace A { namespace B { using System.Linq; using System; class C { } } }
```

```csharp
namespace A { namespace B { using System;
using System.Linq; class C { } } }
```

### FMT-PIPE-031 — The formatter answers whether it splits a field declaration

`SplitsFieldDeclaration` MUST return whether the structural phase splits the field declaration it is given: true exactly when the declaration has more than one declarator, is a member of a class, struct, record, or interface (FMT-STRUCT-021), and carries no directive or disabled text (FMT-STRUCT-023).

This requirement decides the input the RH7101 code fix passes (`AGENTS.md`, *Supported inputs*): the field declaration of a diagnostic.

Why: The RH7101 fix splits a declaration only through formatting it (FMT-PIPE-029), so it must not be offered where formatting does not split it (SHARED-FIX-005); RH7101-006 withholds it also for a declaration that formatting splits. The split states its own applicability once; the fix asks the formatter instead of keeping a copy of the split's conditions, which could drift apart from them.

Example — in

```csharp
class C
{
    private int _a, _b;
    private int _c,
#if DEBUG
                _d,
#endif
                _e;
}
```

the method returns true for `_a, _b` and false for `_c, _d, _e`.
