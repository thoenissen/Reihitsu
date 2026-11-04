# The RH7101 fix splits a field through the context entry point

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | FMT-PIPE-029 (new), FMT-STRUCT-026, RH7101-004, RH7101-006 (new) |
| Depends on | [The context entry point's target keeps its first line, and its other lines follow it](context-target-first-line.md) |

## What

The RH7101 code fix stops running the field-split transform on its own. It formats the reported field declaration with `FormatNodeInDocumentWithContextAsync`, the field as target and the type that declares it as context. The structural phase splits the field in the context as document-level formatting splits it (FMT-STRUCT-021, FMT-STRUCT-022), the later phases lay the fields out, and the entry point writes back every field the split makes of the target, and nothing else of the type (FMT-PIPE-029). The first field keeps the target's leading trivia and its first token's line and column in the document (FMT-PIPE-017, FMT-PIPE-018), except that it starts a line of its own when another member stands in front of it on its line (SHARED-FIX-007); the last keeps the target's trailing trivia (FMT-PIPE-010); everything between them is the phases' layout.

The field split loses the layout it wrote only for the fix: the indentation in front of each field it creates and of each comment it moves in front of one, and the line break in front of a created field where no separator needs one. It keeps the separators FMT-STRUCT-026 requires of every structural transform, and the two pieces of layout that FMT-STRUCT-022 decides and no later phase decides again: one space in front of a comment that trailed a separating comma, and the line break behind a comment it moves onto its own line. FMT-STRUCT-026 states that layout instead of an exception "until the fix formats through an entry point the pipeline spec names".

The fix is not offered in a document with a syntax error (RH7101-006), because the entry point leaves such a document untouched (FMT-PIPE-005) and an offered fix has to change the document (SHARED-FIX-005).

The RH7201–RH7207 fixes keep running the using-directive phase on the block they fix. FMT-USING, SHARED-USING-010, and the phase's scope indentation do not change; see *Decisions*.

The only caller that reaches FMT-PIPE-029 is the RH7101 fix (`AGENTS.md`, *Supported inputs*): its target is a field declaration with more than one declarator and without a directive (RH7101-005), and its context is the class, struct, record, or interface declaration whose member the field is.

## Why

The RH7101 fix ran a formatter component outside the pipeline, so the component had to lay out its own output with decisions that later phases own in the pipeline. That layout fell short of SHARED-FIX-007, which requires a fix's changed code to be laid out as the formatter lays it out: in each case under *Visible output changes* except 8 and 9, formatting today's fix result changes the fields the fix wrote (measured with `scripts/format.sh` on today's results). The fix also visited the whole type, nested types included, so one application split every combined field there, which SHARED-FIX-006 rules out (case 8). And FMT-STRUCT-026 had to carry an exception for the field split, because the split's layout was the fix's layout.

Formatting through the context entry point gives the fix the formatter's layout for the code it changes, keeps every other member of the type as it stands, and leaves the field split with only the layout its own requirements decide.

## Visible output changes

Every change is an RH7101 code-fix result. The "before" outputs were measured with `scripts/apply-fix.sh RH7101` on `claude/zen-mayer-s173ql` at `15fc57c`, whose code equals `main` at `df4f828`, under LF and CRLF alike. The "after" outputs are derived from FMT-PIPE-029 and FMT-PIPE-017/018: the fields as `scripts/format.sh` lays out the same input, with the first field's leading trivia and first-token column kept as in the document.

The plain case keeps its output, as do a documented declaration, a documentation comment written in front of the field at another column, a delimited documentation comment in front of the `;`, and a declaration whose comments trail its separating commas:

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

### 6. A declaration behind another member on its line

```csharp
// input
class C
{
    private int _p; private int _x, _y;
}
```

```csharp
// before
class C
{
    private int _p; private int _x;
private int _y;
}
```

```csharp
// after
class C
{
    private int _p;
    private int _x;
    private int _y;
}
```

The first field starts a line of its own, as SHARED-FIX-007 requires of a member a fix changes; the context entry point otherwise keeps its target behind the code in front of it (FMT-PIPE-018). A declaration behind the `{` of its type keeps its line.

### 7. A declaration off its member column

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

The first field keeps its column (FMT-PIPE-017); the later field stands where the type's depth places a member, as every line the context entry point places by the depth does.

### 8. One fix splits only the reported declaration

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

### 9. A document with a syntax error

```csharp
// input
class C
{
    private int _x, {|RH7101:_y|};

    void M()
    {
        var a = ;
    }
}
```

Before: the fix is offered and splits the declaration as in the plain case. After: the diagnostic is reported, and no code fix is offered.

## Decisions

### The fix formats through the context entry point

`FormatNodeInDocumentWithContextAsync` with the field as target and its type as context is the one existing entry point that can make the 1→N replacement: the structural phase splits a field only as a member of its type, and the entry point writes back only its target. `FormatNodeInDocumentAsync` formats the field as a root of its own, where no split happens. The entry point already keeps the target's leading trivia, so the blank lines and comments above the declaration and its first line stay as written (SHARED-FIX-006), and lays out the target's other lines from its first token (FMT-PIPE-017). FMT-PIPE-029 adds what the entry point does when the phases replace its target by several fields.

### The field split keeps the layout FMT-STRUCT-022 decides

The issue proposed that the split write no line break beyond the separators FMT-STRUCT-026 requires. Formatting the shapes such a split would leave shows that two pieces of its layout are decisions no later phase makes again: the spacing phase keeps the gap in front of a trailing comment as it finds it (`_x;// h` stays), and the line-break phase leaves a leading comment on the line of the member behind it (`/* v */private int _y;` stays). Dropping them would change document-level output, which this change does not touch. They are FMT-STRUCT-022's decisions, written in every entry point, and FMT-STRUCT-026 now states them. The indentation the split wrote is redone by the indentation phase, and the line break in front of a field without a comment in front of it by the line-break phase (one member per line), so the split writes neither.

### The RH7201–RH7207 fixes keep running the using-directive phase

The issue proposed that these fixes format through a node-level entry point as well and that the phase stop writing the scope's indentation. The change keeps both as they are:

- No entry point can take the block as target. A using block is a list, not a node; the only node that holds it is the compilation unit or the namespace, and formatting either as the target formats everything in it, which SHARED-FIX-006 rules out.
- Formatting the block through every phase would change what the seven fixes write, against requirements that stay. FMT-USING-010 records that the fixes keep a comment that leads a directive without a blank line above it where the author wrote none, while the blank-line phase inserts one (SHARED-FIX-006). The phase's output is what RH7201-004 to RH7207-004 specify.
- The dependency the issue wanted named is named: FMT-USING's scope sentence states that the fixes run the phase on their block, so a change to the phase is a change to their output. The scope indentation of a split-off directive is FMT-USING-003 and SHARED-USING-010, which already say that the indentation phase decides the column in the formatter's output and that the rule is visible in the fixes.

### The fix is withheld in a document with a syntax error

The context entry point returns the document unchanged when its tree has a syntax error (FMT-PIPE-005), so the fix would be offered and change nothing, which SHARED-FIX-005 rules out. The diagnostic stays without a fix, a supported end state (`documentation/rules/general-notes.md`).

## Alternatives considered

- **A new entry point that runs one phase or transform on a node, with its own layout contract** — rejected: the component would keep a second layout of its output beside the one the pipeline's phases decide, which is the duplication this change removes, and the fix's output would keep differing from the formatter's.
- **The fix builds the split fields itself and formats each through `FormatNodeInDocumentAsync`** — rejected: a second implementation of FMT-STRUCT-021 and FMT-STRUCT-022, and a field that does not start its line keeps its leading trivia there (FMT-PIPE-010), so the line break between two fields would again be the fix's own layout.
- **The first field stays behind a member in front of it on its line (case 6)**, as FMT-PIPE-018 keeps every other target of the context entry point — rejected: SHARED-FIX-007 gives a member a fix changes a line of its own, and the RH5417 fix already does so.
- **Later fields placed under the first field's column (case 7)** — rejected: FMT-PIPE-017 places every line the context entry point lays out by the depth where the context's depth places it; a split field is no exception.
- **A fall-back to today's split in a document with a syntax error (case 9)** — rejected: it keeps the code-fix-only layout this change removes, for documents the formatter itself leaves alone.
- **The RH7201–RH7207 fixes format the scope through the context entry point and write back the block** — rejected for the reasons under *Decisions*.
