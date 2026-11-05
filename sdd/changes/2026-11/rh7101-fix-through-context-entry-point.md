# Code fixes reach the formatter only through `ReihitsuFormatter`; RH7101 splits a field through the context entry point

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-PIPE-005, FMT-PIPE-006, FMT-PIPE-009, FMT-PIPE-029 (new), FMT-PIPE-030 (new), FMT-PIPE-031 (new), FMT-STRUCT-026, FMT-USING (scope sentence, non-goal), RH7101-001, RH7101-002, RH7101-004, RH7101-005 |
| Depends on | [The context entry point's target keeps its first line, and its other lines follow it](context-target-first-line.md) |

## What

Code fixes reach a formatter phase or structural transform only through `ReihitsuFormatter` (FMT-PIPE-009). Two fixes did not, and change:

- **RH7101** stops running the field-split transform on its own. It formats the reported field declaration with `FormatNodeInDocumentWithContextAsync`, the field as target and the type that declares it as context. The structural phase splits the field in the context as document-level formatting splits it (FMT-STRUCT-021, FMT-STRUCT-022), the later phases lay the fields out, and the entry point writes back every field the split makes of the target, and nothing else of the type (FMT-PIPE-029). The first field keeps the target's leading trivia and its first token's line and column in the document (FMT-PIPE-017, FMT-PIPE-018); the last keeps the target's trailing trivia (FMT-PIPE-010); everything between them is the phases' layout. No field shares a line with another member: a member in front of or behind the declaration on its line has the first field, or itself, start the next line, with one line break and no blank line; a comment in that gap stays behind the code in front of it, and a documentation comment goes with the member it documents (SHARED-FIX-006, SHARED-FIX-007). The fix is offered exactly where the formatter splits the declaration; it asks `ReihitsuFormatter.SplitsFieldDeclaration` (FMT-PIPE-031) instead of calling the transform's guard.
- **RH7201–RH7207** stop running the using-directive phase themselves. They call `ReihitsuFormatter.OrganizeUsingDirectivesInDocumentAsync` with the scope of the reported directive (FMT-PIPE-030), which runs that phase alone and writes back the scope's using block. Their output does not change; the scope's indentation in front of a directive the phase moves onto its own line stays that entry point's specified layout (FMT-USING-003, SHARED-USING-010).

The field split loses the layout it wrote only for the RH7101 fix: the indentation in front of each field it creates and of each comment it moves in front of one, and the line break in front of a created field where no separator needs one. It keeps the separators FMT-STRUCT-026 requires of every structural transform, and the two pieces of layout that FMT-STRUCT-022 decides and no later phase decides again: one space in front of a comment that trailed a separating comma, the line break behind such comments when one of them spans lines, and the line break behind a comment it moves onto its own line.

The RH7101 analyzer reports nothing in a file with a syntax error (SHARED-JOIN-002) and nothing in an extension block, where the formatter would not split, so that where it reports, a fix is offered.

The node-level entry points that only code fixes call — `FormatNode`, `FormatNodeInDocumentAsync`, `FormatNodeInDocumentWithContextAsync`, and the new `OrganizeUsingDirectivesInDocumentAsync` — no longer check the generated-code header (FMT-PIPE-006); the analyzers' decision on generated code governs the fixes (SHARED-DIAG-001). `FormatDocumentAsync`, `FormatSyntaxTree`, and the CLI keep the check.

The callers that reach the change (`AGENTS.md`, *Supported inputs*): the RH7101 fix (FMT-PIPE-029, FMT-PIPE-031), whose target is a field declaration with more than one declarator and without a directive, and whose context is the class, struct, record, or interface declaration whose member the field is; the RH7201–RH7207 fixes (FMT-PIPE-030), which pass the compilation unit, namespace, or file-scoped namespace of the reported directive, whose block may be reordered; and every code fix that formats through a node-level entry point, for FMT-PIPE-006.

## Why

The RH7101 fix ran a formatter component outside the pipeline, so the component had to lay out its own output with decisions that later phases own in the pipeline. That layout fell short of SHARED-FIX-007, which requires a fix's changed code to be laid out as the formatter lays it out: in each RH7101 case below except 9, 10, and 11, formatting today's fix result changes the fields the fix wrote (measured with `scripts/format.sh` on today's results). The fix also visited the whole type, nested types included, so one application split every combined field there, which SHARED-FIX-006 rules out (case 9). The using fixes ran a phase directly, so the path from them into the formatter was named in no pipeline requirement. Routing every code fix through `ReihitsuFormatter` names each path, leaves one layout owner for the RH7101 fields — the pipeline's phases — and, for the using fixes, keeps the phase's own layout as the specified output of one entry point.

Once the RH7101 fix's whole edit is a formatting call, every input the formatter refuses while the analyzer reports would give an offered fix that changes nothing (SHARED-FIX-005). The maintainer's principle settles each such input on the reporting side: where an analyzer reports, a fix is offered. So the analyzer does not report where the formatter cannot split (a syntax error, an extension block), and the node-level entry points do not refuse a file the analyzers analyze (the generated-code header). The fix keeps only the withholding RH7101-005 already states, for a declaration with a directive (SHARED-FIX-003).

## Visible output changes

The "before" outputs were measured with `scripts/apply-fix.sh RH7101` on `claude/zen-mayer-s173ql` at `15fc57c`, whose code equals `main` at `df4f828`, under LF and CRLF alike. The "after" outputs are derived from FMT-PIPE-029 and FMT-PIPE-017/018: the fields as `scripts/format.sh` lays out the same input, with the first field's leading trivia and first-token column kept as in the document.

The RH7201–RH7207 fixes' results do not change.

The RH7101 plain case keeps its output, as do a documented declaration, a documentation comment written in front of the field at another column, a delimited documentation comment in front of the `;`, and a declaration whose comments trail its separating commas:

```csharp
// input, before, and after
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

### 1. A comment in front of a later declarator gets a blank line above it

```csharp
// input
class C
{
    private int _x, // horizontal
                /* vertical */ _y;
}
```

```csharp
// before
class C
{
    private int _x; // horizontal
    /* vertical */
    private int _y;
}
```

```csharp
// after
class C
{
    private int _x; // horizontal

    /* vertical */
    private int _y;
}
```

The same holds when the author wrote a blank line in front of the comment: today's fix drops it, the change keeps one.

### 2. A documentation comment in front of a later declarator gets a blank line above it and the formatter's shape

```csharp
// input
class C
{
    private int _x,
                /// <summary>Second field.</summary>
                _y,
                /** Third field. */
                _z;
}
```

```csharp
// before
class C
{
    private int _x;
    /// <summary>Second field.</summary>
    private int _y;
    /** Third field. */
    private int _z;
}
```

```csharp
// after
class C
{
    private int _x;

    /// <summary>
    /// Second field.
    /// </summary>
    private int _y;

    /** Third field. */
    private int _z;
}
```

### 3. A first declarator on a later line than the type

```csharp
// input
class C
{
    private int
/* c */ _x, _y;
}
```

```csharp
// before
class C
{
    private int
/* c */ _x;
    private int
_y;
}
```

```csharp
// after
class C
{
    private int
    /* c */ _x;
    private int
    _y;
}
```

### 4. Spacing inside the split declaration

```csharp
// input
class C
{
    private  int[]  _a = { 1,2 } ,  _b;
}
```

```csharp
// before
class C
{
    private  int[]  _a = { 1,2 } ;
    private  int[]  _b;
}
```

```csharp
// after
class C
{
    private int[] _a = { 1, 2 };
    private int[] _b;
}
```

### 5. Attribute lists of the split declaration

```csharp
// input
using System;

class C
{
    [NonSerialized, Obsolete] private static readonly string _a = "a", _b = "b";
}
```

```csharp
// before
using System;

class C
{
    [NonSerialized, Obsolete] private static readonly string _a = "a";
    [NonSerialized, Obsolete] private static readonly string _b = "b";
}
```

```csharp
// after
using System;

class C
{
    [NonSerialized]
    [Obsolete]
    private static readonly string _a = "a";
    [NonSerialized]
    [Obsolete]
    private static readonly string _b = "b";
}
```

### 6. Initializers and structural rewrites inside the split declaration

The whole field is formatted, so the structural rewrites the node-level entry points apply (FMT-PIPE-011 keeps only accessor shapes) and the layout of every later phase reach its initializers.

```csharp
// input
using System;

class C
{
    private static bool _f;
    private Action _a = () => { if (_f) return; }, _b;
    private int[] _c = { 1, 2, }, _d;
}
```

```csharp
// before: the fix of `_a, _b`, which today also splits `_c, _d` (case 9)
using System;

class C
{
    private static bool _f;
    private Action _a = () => { if (_f) return; };
    private Action _b;
    private int[] _c = { 1, 2, };
    private int[] _d;
}
```

```csharp
// after: the fixes of both declarations
using System;

class C
{
    private static bool _f;
    private Action _a = () =>
                        {
                            if (_f)
                            {
                                return;
                            }
                        };
    private Action _b;
    private int[] _c = { 1, 2 };
    private int[] _d;
}
```

An array creation such as `new int[]{3}` becomes `new int[] {3 }`, the initializer-brace spacing thoenissen/Reihitsu#1440 tracks.

### 7. Another member on the declaration's line

```csharp
// input
class C
{
    private int _p; /* c */ private int _x, _y; private int _q;
}
```

```csharp
// before
class C
{
    private int _p; /* c */ private int _x;
private int _y; private int _q;
}
```

```csharp
// after
class C
{
    private int _p; /* c */
    private int _x;
    private int _y;
    private int _q;
}
```

The same holds with a member on one side only. A declaration behind the `{` or in front of the `}` of its type keeps its line.

A documentation comment in the gap goes with the member behind it, onto the next line, and gets no blank line above it, where document-level formatting writes one:

```csharp
// input
class C
{
    private int _p; /** X. */ private int _x, _y;
}
```

```csharp
// before
class C
{
    private int _p; /** X. */ private int _x;
private int _y;
}
```

```csharp
// after
class C
{
    private int _p;
    /** X. */ private int _x;
    private int _y;
}
```

```csharp
// input
class C
{
    private int _a, _b; /** Q. */ private int _q;
}
```

```csharp
// before
class C
{
    private int _a;
    private int _b; /** Q. */ private int _q;
}
```

```csharp
// after
class C
{
    private int _a;
    private int _b;
    /** Q. */ private int _q;
}
```

A comment that spans lines in a gap leaves the member behind its closing `*/` where document-level formatting leaves it, and a comment that spans lines behind a separating comma ends the line of the field in front of it:

```csharp
// input
class C
{
    private int _p; /* a
       b */ private int _x, _y;
}
```

```csharp
// before
class C
{
    private int _p; /* a
       b */ private int _x;
private int _y;
}
```

```csharp
// after
class C
{
    private int _p; /* a
       b */ private int _x;
    private int _y;
}
```

```csharp
// input
class C
{
    private int _p; private int _x, /* a
       b */ _y;
}
```

```csharp
// before
class C
{
    private int _p; private int _x; /* a
       b */
private int _y;
}
```

```csharp
// after
class C
{
    private int _p;
    private int _x; /* a
       b */
    private int _y;
}
```

A documentation comment in a gap starts the next line with the member it documents also when a comment in the gap spans lines, the documentation comment itself or one in front of it, because document-level formatting starts that documentation comment's line (FMT-DOC-001). Whether a member moves is read from the phases' layout of the type, not predicted from the gap (FMT-PIPE-029). A comment in front of the documentation comment stays behind the code in front of it, and no blank line is written above the documentation comment:

```csharp
// input
class C
{
    private int _p; /** a
       b */ private int _x, _y; /** c
       d */ private int _q;
}
```

```csharp
// before
class C
{
    private int _p; /** a
       b */ private int _x;
private int _y; /** c
       d */ private int _q;
}
```

```csharp
// after
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

```csharp
// input
class C
{
    private int _p; /* a
       b */ /** c */ private int _x, _y; /* d
       e */ /** f */ private int _q;
}
```

```csharp
// before
class C
{
    private int _p; /* a
       b */ /** c */ private int _x;
private int _y; /* d
       e */ /** f */ private int _q;
}
```

```csharp
// after
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

The same holds for a one-line comment followed by a documentation comment that spans lines, and for a comment that spans lines followed by one; FMT-PIPE-029 shows both. Where something in the gap already starts a line — the declaration's first token or the next member's, or a documentation comment in front of it — the gap stays as it stands, as before.

### 8. A declaration off its member column, with spaces or a tab

```csharp
// input
class C
{
      private int _x, _y;
}
```

```csharp
// before
class C
{
      private int _x;
      private int _y;
}
```

```csharp
// after
class C
{
      private int _x;
    private int _y;
}
```

With a tab in front of the declaration (shown as `→`), the first field keeps the tab, and the later field stands at the member column, indented with spaces:

```csharp
// input
class C
{
→private int _x, _y;
}
```

```csharp
// before
class C
{
→private int _x;
→private int _y;
}
```

```csharp
// after
class C
{
→private int _x;
    private int _y;
}
```

### 9. One fix splits only the reported declaration

```csharp
// input
class C
{
    private int _a, {|RH7101:_b|};

    class D
    {
        private int _c, {|RH7101:_d|};
    }
}
```

```csharp
// before: the fix of the first diagnostic
class C
{
    private int _a;
    private int _b;

    class D
    {
        private int _c;
        private int _d;
    }
}
```

```csharp
// after: the fix of the first diagnostic; `_c, _d` keeps its diagnostic and its fix
class C
{
    private int _a;
    private int _b;

    class D
    {
        private int _c, _d;
    }
}
```

The same holds for a second combined declaration in the same type: today one fix splits `private int _a, _b;` and `private int _c, _d;` alike. Fix All still splits every reported declaration.

### 10. A file with a syntax error

```csharp
// input
class C
{
    private int _x, _y;

    void M()
    {
        var a = ;
    }
}
```

Before: RH7101 is reported on `_y`, and the fix splits the declaration as in the plain case. After: RH7101 is not reported until the syntax error is gone; then it is reported with its fix.

### 11. A field in an extension block

```csharp
// input
static class E
{
    extension(int i)
    {
        private static int _x, _y;
    }
}
```

Before: RH7101 is reported on `_y`, and a fix is offered that changes nothing. After: RH7101 is not reported. The compiler rejects the field (CS9282).

### 12. A code fix in a file whose header marks it generated, analyzed under `generated_code = false`

```csharp
// input, with `generated_code = false` for the file in `.editorconfig`
// <auto-generated/>
class C
{
    private int _x, {|RH7101:_y|};
}
```

Before: the RH7101 fix splits with its own layout, as in the plain case. After: the same fields, laid out by the context entry point. Every other fix that formats through a node-level entry point now formats in such a file as in any other, where today the entry point returns the document unchanged: a fix whose whole edit is the formatting call (for example RH5417) was offered and changed nothing, and a fix that edits first left its edit unformatted.

## Decisions

### The maintainer's answers

The maintainer took every recommendation of the batch asked before the tests:

- Cases 1–5 and 9: the fix result follows the formatter's layout; the issue's goal "the fix output stays as today" is dropped.
- Case 6: the whole field is formatted, initializers and structural rewrites included.
- Case 8: the first field keeps its column, a tab counting as one column; later fields stand at the type's member column.
- Case 10: the RH7101 analyzer does not report while the file has a syntax error.
- Case 12: the node-level entry points do not check the generated-code header; the analyzers' decision governs; document-level formatting and the CLI keep the check.
- The using fixes go through a `ReihitsuFormatter` entry point that runs only the using-directive phase; their outputs and the scope indentation stay.
- Code-fix code runs no formatter phase or transform except through `ReihitsuFormatter`; an architecture test enforces it; formatter helpers stay callable. The rule is keyed on the phase, transform, and rewriter types and `FormattingPipeline`, not on a namespace, so a utilities class such as `ExpressionBodiedTransformUtilities` stays callable.
- Principle: where an analyzer reports, a code fix is offered.

Case 7 follows SHARED-FIX-007's shared-line bullet, which is unchanged: a comment in the gap stays behind the code in front of it, and a documentation comment goes with its member, where document-level formatting places both; the gap gets no blank line (SHARED-FIX-006). FMT-PIPE-018 and SHARED-FIX-006 decide the documentation-comment variants, which keep the comment where it is in the trivia.

### RH7101 formats through the context entry point

`FormatNodeInDocumentWithContextAsync` with the field as target and its type as context is the one existing entry point that can make the 1→N replacement: the structural phase splits a field only as a member of its type, and the entry point writes back only its target. `FormatNodeInDocumentAsync` formats the field as a root of its own, where no split happens. The entry point already keeps the target's leading trivia, so the blank lines and comments above the declaration stay as written (SHARED-FIX-006), and lays out the target's other lines from its first token (FMT-PIPE-017). FMT-PIPE-029 adds what the entry point does when the phases replace its target by several fields.

### The split states its applicability once

The fix must be offered exactly where formatting splits the declaration. `ReihitsuFormatter.SplitsFieldDeclaration` answers that from the split's own conditions (FMT-STRUCT-021, FMT-STRUCT-023), so the fix keeps no copy of them and calls no member of the transform. The syntax-error and generated-code refusals of the entry point need no guard in the fix: the analyzer does not report in a file with a syntax error, and the entry point no longer checks the header.

### A field in an extension block is not reported

Today RH7101 reports such a field and offers a fix that changes nothing. Two outcomes satisfy SHARED-FIX-005: withhold the fix, or do not report. The maintainer's principle that a reported diagnostic gets a fix picks the second. It is also the more useful report: the compiler already rejects a field in an extension block (CS9282), so RH7101 there would ask to split code that cannot exist in a compiling program.

### The field split keeps the layout FMT-STRUCT-022 decides

The issue proposed that the split write no line break beyond the separators FMT-STRUCT-026 requires. Formatting the shapes such a split would leave shows that three pieces of its layout are decisions no later phase makes again: the spacing phase keeps the gap in front of a trailing comment as it finds it (`_x;// h` stays), and the line-break phase leaves a leading comment on the line of the member behind it (`/* v */private int _y;` stays). The third is the line break behind the comments that trailed a separating comma when one of them spans lines: the field behind the closing `*/` does not start its line (SHARED-COMMENT-012), and the line-break phase leaves a member behind such a comment where it stands (`b */private int _y;` stays). Dropping them would change document-level output, which this change does not touch. The indentation the split wrote is redone by the indentation phase, and the line break in front of a field behind a single-line comment or behind no comment by the line-break phase (one member per line), so the split writes neither.

### The using fixes get an entry point of their own

A using block is a list, not a node, so no node-level entry point can take it as target, and formatting the block through every phase would insert a blank line above a comment that leads a directive where the author wrote none, which FMT-USING-010 keeps under SHARED-FIX-006. `OrganizeUsingDirectivesInDocumentAsync` runs only the using-directive phase on one scope and writes back that scope's block, so the seven fixes keep the output RH7201-004 to RH7207-004 specify, and the path is named in FMT-PIPE. Like today's fixes, it checks no syntax error and no header.

## Alternatives considered

- **A general entry point that runs one named phase or transform on a node** — rejected: for RH7101 the transform would keep a second layout of its output beside the phases'; the using entry point is specific to one phase whose output is the specified result.
- **RH7101 builds the split fields itself and formats each through `FormatNodeInDocumentAsync`** — rejected: a second implementation of FMT-STRUCT-021 and FMT-STRUCT-022, and a field that does not start its line keeps its leading trivia there (FMT-PIPE-010), so the line break between two fields would again be the fix's own layout.
- **The RH7101 fix withholds itself where the formatter refuses (syntax error, generated header, extension block)** — rejected: a guard in the fix copies the entry point's checks, and the maintainer's principle asks for a fix wherever the analyzer reports.
- **Later fields placed under the first field's column (case 8)** — rejected by the maintainer's answer.
- **The using fixes format the scope through the context entry point and write back the block** — rejected for the reasons under *Decisions*.
