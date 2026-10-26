# Formatter — Pipeline

| | |
|---|---|
| ID prefix | FMT-PIPE |
| Status | Active |
| Code owners | `ReihitsuFormatter`, `FormattingPipeline`, `IFormattingPhase`, `FormattingContext`, `ReihitsuFormatterHelpers`, `LanguageVersionResolver`, `LineEndingUtilities` |
| Uses | SHARED-FIX-001, SHARED-FIX-006, SHARED-FIX-007, FMT-BLANK-011, FMT-BLANK-013, FMT-BLANK-014, FMT-BREAK-011, FMT-BREAK-012, FMT-BREAK-014, FMT-BREAK-016, FMT-BREAK-021, FMT-BREAK-022, FMT-BREAK-026, FMT-CLEAN-001, FMT-CLEAN-005, FMT-EOL-001, FMT-INDENT-007, FMT-INDENT-008, FMT-INDENT-030, FMT-RAW-001, FMT-RAW-005, FMT-RAW-007, FMT-TAB-001, FMT-TAB-002, FMT-TAB-003 |

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
| 1 | `TabReplacementPhase` | [tab-replacement.md](tab-replacement.md) (FMT-TAB) |
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
| 12 | `LineEndingNormalizationPhase` | [line-endings.md](line-endings.md) (FMT-EOL) |

A phase MUST NOT compute the column of a token, a comment, or a line from a move or a replacement that a phase after it applies, neither by repeating that phase's rule nor by adjusting for its result: the tab width of FMT-TAB-001 and the raw-string move of FMT-RAW-001 are applied before or in the phase that measures them. This requirement specifies columns only; it does not decide whether a phase may anticipate a later phase's line breaks or blank lines.

Why: Later phases rely on the shape earlier phases produce — tabs are gone before any layout is computed, line breaks are decided before spacing and indentation measure columns, raw strings are aligned in the phase that places their opening quotes, and line endings are normalized last so every inserted line break is covered. A phase may assume the phases before it ran; it must not assume anything about the phases after it. A phase that predicts a later one makes one decision in two places, and every difference between the prediction and the later phase is output that the next pass changes again (FMT-PIPE-004).

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

The document- and tree-level entry points MUST return their input unchanged when the syntax tree contains any error-level diagnostic.

Why: Roslyn's error recovery invents missing tokens and attaches skipped text in unpredictable places. Rewriting trivia around them can move or lose code the user is in the middle of typing.

Example:

```csharp
class C{void M(){int x=1}}
```

is returned unchanged, because the `;` after `1` is missing.

### FMT-PIPE-006 — Generated code is left untouched

The document- and tree-level entry points MUST return their input unchanged when a comment in the leading trivia of the file's first token contains `<auto-generated` or `<autogenerated` (case-insensitive). A marker anywhere else in the file MUST NOT disable formatting.

Why: Generated files are rewritten by their generator; formatting them only produces churn. Only the file header counts, so a comment that merely mentions the marker in the middle of a file does not switch the formatter off.

Example:

```csharp
// <auto-generated />
class C{void M(){int x=1;}}
```

is returned unchanged, while a marker inside the type does not count:

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

Rules that depend on the C# language version MUST read it from the parse options of the formatted tree, or of the document's project for the document- and node-in-document entry points. `Default`, `Latest`, and `LatestMajor` MUST resolve to the newest supported version (C# 14), and `Preview` or any newer version MUST be clamped to it. Without C# parse options, the newest supported version applies.

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

The phases after `IndentationPhase` MUST NOT change the column of any token, comment, or directive, nor the indentation of a content line or the closing line of a multi-line raw string literal. They MAY remove whitespace that ends a line and line breaks at the end of the file (FMT-CLEAN-001, FMT-CLEAN-005), and rewrite the characters of a line ending (FMT-EOL-001).

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

When a line of the input ends in spaces, the cleanup phase removes them, and no token moves. When the input has a tab in front of a trailing comment, such as `Call(1,`, a tab, `// c`, that tab is already four spaces when the indentation phase runs (FMT-PIPE-016), so the comment keeps the column the indentation phase left it in.

### FMT-PIPE-016 — No phase after the first sees or writes a tab in whitespace

The formatter MUST replace the tabs in the whitespace of the code and between directive tokens (FMT-TAB-001, FMT-TAB-002) in its first phase, and a later phase MUST NOT write a tab into that whitespace, so every phase after the first receives and returns it without tabs and counts every character of it as one column. The whitespace inside a documentation comment, a comment, a literal, or disabled text, and a directive's free text, keep their tabs (FMT-TAB-003) and are outside this requirement: a tab in front of a continuation line's `///` reaches the indentation phase, which replaces that whitespace as a whole (FMT-INDENT-008).

Why: All layout is computed in spaces. A tab that survives into a later phase has a width that each phase has to agree on, and a phase that counts it differently from the phase that finally replaces it places a token where the next pass moves it (FMT-PIPE-004). Replacing every tab first leaves no width to agree on.

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

when the indentation of every line is written with tabs, the output of the first phase holds four spaces for each of those tabs, every later phase receives that text without a tab, and the result is the block above.

### FMT-PIPE-017 — Node-level targets take their columns from the indentation phase

`FormatNodeInDocumentAsync` and `FormatNodeInDocumentWithContextAsync` MUST NOT change the column of a line after the last phase: every line they write back MUST end in the column the indentation phase gives it (FMT-PIPE-015), apart from the leading trivia they keep (FMT-PIPE-010, FMT-PIPE-018) and the recorded gap below. The indentation phase MUST lay out the node they format — the target, or the context node — with the columns it gives that node at its depth, shifted by the node's offset: the column of the node's first token in the document, less the column the phase gives that token at the node's depth. The first token therefore stays in its column, a line placed by the depth keeps its place relative to the first token, and a line aligned to an anchor follows the anchor where the phase puts it. This holds for every line written back, also for a line that starts inside a comment that began on an earlier line, for the lines aligned to an anchor on such a line (FMT-INDENT-030), and for the content, hole, and closing lines of a multi-line raw string literal (FMT-RAW-001, FMT-RAW-007). When the first token stands behind the last line of a multi-line comment, the phase does not place its line (FMT-INDENT-007), the token keeps its column, and the offset is zero.

The column of the first token in the document is the number of characters in front of it on its line, each one column, with two exceptions:

- A tab in whitespace that belongs to the node and that the entry point does not keep — the leading whitespace of a target of `FormatNodeInDocumentAsync` that starts its line and whose leading trivia FMT-PIPE-010 does not keep — counts as four columns, because the first phase replaces it (FMT-TAB-001). `FormatNodeInDocumentWithContextAsync` keeps its target's leading trivia in every case (FMT-PIPE-018), so for it every character in front of the context's first token counts as one column.
- A byte order mark at the start of the text counts as zero columns, as in the layout the phases compute; the raw-string move still counts it as FMT-RAW-005 states.

So a tab in the whitespace in front of a node that does not start its line, which belongs to the code in front of the node and is kept, counts as one column, and so does a tab inside a comment or a literal, which is kept (FMT-TAB-003). The leading trivia that FMT-PIPE-010 and FMT-PIPE-018 keep is written back as it stands and is outside this requirement, and so are the lines of the context node outside the target, which `FormatNodeInDocumentWithContextAsync` does not write back.

Recorded gap — a target of `FormatNodeInDocumentWithContextAsync` that starts a later line of its context. The phases lay out the context with the target's first line at the column they give it, but the entry point writes back the target's leading trivia as in the document, the whitespace in front of the target's first token included (FMT-PIPE-018). So the target's first line keeps its column in the document, while the other lines of the target take the columns the phases gave them; a line aligned to an anchor on the target's first line stands where the anchor stood in the phases' layout, not where the anchor ends up. This requirement does not cover the target's first line or the lines whose column depends on it. A target that does not start its line, or that starts the context's first line, is not in the gap: what stands in front of it is written back unchanged, and the phases already laid it out from that column. Example — the RH5111 fix of the declarator `b` in `        int a = 1,` / `              b =` / `                  Other(1,` / `                        2);`, with the statement as context: the result is `              b = Other(1,` / `                  2);`, `b` keeps column 14, and `2` stands in column 18, under the column `1` had with `b` in the phases' column 8, while `1` ends up in 24.

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
