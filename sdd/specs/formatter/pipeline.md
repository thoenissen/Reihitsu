# Formatter — Pipeline

| | |
|---|---|
| ID prefix | FMT-PIPE |
| Status | Active |
| Code owners | `ReihitsuFormatter`, `FormattingPipeline`, `IFormattingPhase`, `FormattingContext`, `ReihitsuFormatterHelpers`, `LanguageVersionResolver`, `LineEndingUtilities` |
| Uses | — |

This spec covers what holds for the formatter as a whole: the public entry points, the fixed phase order, and the invariants every phase must keep. What an individual phase decides is specified in that phase's own spec.

## Non-goals

- The decisions of a single phase — each phase has its own spec, listed in FMT-PIPE-001.
- File and directory walking, skipped file names, `--check`, `--dry-run`, and exit codes — those belong to the CLI specs.
- Configuration. The formatter applies one fixed style and reads no `.editorconfig`, rule set, or option.
- Formatting of code with syntax errors or of generated code (see FMT-PIPE-005 and FMT-PIPE-006).

## Requirements

### FMT-PIPE-001 — Fixed phase order

The formatter MUST apply its phases in this order, each phase receiving the output of the one before it:

| # | Phase | Spec |
|---|---|---|
| 1 | `StructuralTransformPhase` | [structural-transforms.md](structural-transforms.md) (FMT-STRUCT) |
| 2 | `RegionFormattingPhase` | [region-formatting.md](region-formatting.md) (FMT-REGION) |
| 3 | `DocumentationCommentFormattingPhase` | [documentation-comments.md](documentation-comments.md) (FMT-DOC) |
| 4 | `UsingDirectiveOrderingPhase` | [using-directives.md](using-directives.md) (FMT-USING) |
| 5 | `BlankLinePhase` | [blank-lines.md](blank-lines.md) (FMT-BLANK) |
| 6 | `LineBreakPhase` | `line-breaks.md` (FMT-BREAK, not yet written) |
| 7 | `SwitchCaseBracePhase` | `switch-case-braces.md` (FMT-SWITCH, not yet written) |
| 8 | `HorizontalSpacingPhase` | `horizontal-spacing.md` (FMT-SPACE, not yet written) |
| 9 | `IndentationPhase` | `indentation.md` (FMT-INDENT, not yet written) |
| 10 | `RawStringAlignmentPhase` | [raw-string-alignment.md](raw-string-alignment.md) (FMT-RAW) |
| 11 | `CleanupPhase` | `cleanup.md` (FMT-CLEAN, not yet written) |
| 12 | `LineEndingNormalizationPhase` | `line-endings.md` (FMT-EOL, not yet written) |

Why: Later phases rely on the shape earlier phases produce — line breaks are decided before spacing and indentation measure columns, raw strings are aligned to the indentation that was just computed, and line endings are normalized last so every inserted line break is covered. A phase may assume the phases before it ran; it must not assume anything about the phases after it.

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

The formatter MUST detect the file's predominant line ending by counting end-of-line trivia and documentation-comment newlines, use `\r\n` when it occurs at least as often as `\n`, and fall back to `Environment.NewLine` when the input contains no line break at all. Every line break the formatter writes MUST use the detected line ending, and FMT-EOL rewrites the existing line breaks between tokens and inside documentation comments to it. Line breaks inside literals, inside other comments, and inside disabled text keep their original line ending (FMT-PIPE-002).

Why: Mixed line endings produce whole-file diffs in version control. The majority wins because it is what the file's author most likely uses; a tie goes to `\r\n` so the decision is deterministic.

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

`FormatNodeInDocumentAsync` and `FormatNodeInDocumentWithContextAsync` MUST rewrite only the target node and keep the rest of the document as it is. The one exception is the whitespace that ended the line before a documentation comment the target moves onto its own line, which MUST be removed as document-level formatting removes it. The target's leading trivia MUST be kept when the target does not start its line or is a direct child of the root, and its trailing trivia MUST be kept in every case.

Why: These entry points serve code fixes, which may only change the code their diagnostic is about. Reformatting the surrounding document would turn a one-line fix into an unrelated diff.

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
