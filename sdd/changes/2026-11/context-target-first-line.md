# The context entry point's target keeps its first line, and its other lines follow it

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | FMT-PIPE-017, FMT-PIPE-018, FMT-PIPE-024, SHARED-FIX-007, RH5111-004 |
| Depends on | [Node-level roots are detached, and their input facts are plain values](../2026-10/node-root-detached.md), [One line-start fact for analyzers and every formatter phase](../2026-10/line-start-fact.md) |

## What

`FormatNodeInDocumentWithContextAsync` formats a context node and writes back only its target, with the target's leading trivia as it stands in the document (FMT-PIPE-018). Until now the phases laid out the context without knowing that, so where their layout put the target's first token on another line or in another column than the document, the target kept its place in the document while its other lines took the columns the phases had computed from the discarded place. FMT-PIPE-017 recorded this as a gap.

With this change the target's first-line placement is an input fact of the phases, the target-position fact (FMT-PIPE-024): the column of the target's first token in the document, counted as the entry point keeps it (each character in front of it one column), and the target marked in the root.

- The line-break phase keeps the target's first token on its line: it neither breaks the line in front of it nor joins it to the line in front.
- The documentation, line-break, and blank-line phases keep the target's leading trivia as the preparation phase leaves it.
- The indentation phase places the target's first token in that column, leaves the lines of the leading trivia where they stand, and lays out the target's other lines from that column: a line aligned to an anchor follows the anchor (FMT-INDENT-030), and a line placed by the depth stays where the context's depth places it.

The entry point still writes the leading trivia back as it stands in the document. That write-back now changes no line and no column; it only restores the tabs and line endings that the preparation phase rewrites in every root (FMT-PIPE-019). FMT-PIPE-017's recorded gap is replaced by this behavior, FMT-PIPE-018 states that the phases keep the trivia, SHARED-FIX-007 and RH5111-004 drop the gap they recorded from the code-fix side, and `AGENTS.md` no longer names the context entry point as an exception to the rule that no step after the last phase changes a column.

The callers that reach the change (`AGENTS.md`, *Supported inputs*) are three code fixes, the only callers of `FormatNodeInDocumentWithContextAsync`:

- The RH5111 fix passes a variable declarator with its declaration statement, a parameter with its parameter list, or an enum member with its enum as context. All three ways the context's layout can differ from the document in front of the target occur: the target starts a later line whose column the layout changes; the target shares its line with a parameter in front of it and the layout breaks the line in front of it; and the target stands behind the closing line of a multi-line raw string literal that the indentation phase moves.
- The RH3208 fix passes a local function with the labeled statement whose line it shares. The layout changes the spacing behind the label, so the target's first token stands in another column in the layout than in the document.
- The fluent-chain fixes (RH5112, RH5201, through `FluentChainCodeFixHelper`) pass the outermost chain node with the statement or member that starts its line. They use the result only when the chain has the same text and the same first-token column as when the whole context is formatted, and fall back to formatting the context otherwise. When the context's layout leaves the chain's first token in place, the change gives the result it gives today; when it moves the token, both results differ and the fall-back is taken, as today. Their output does not change.

## Why

The phases decided where the target's first token goes, and the entry point discarded that decision while keeping everything the phases had computed from it. A line aligned to an anchor on the target's first line was left where the anchor stood in the discarded layout, so the fix wrote misaligned code that the alignment rules report and the next formatting moves again. The decision has to be made once, as an input the phases build on, the way they already take the column of the context's first token (FMT-PIPE-017) and the facts of FMT-PIPE-024.

## Visible output changes

Every change is a code-fix result. The outputs below are derived from FMT-PIPE-017 as changed here; the "before" outputs were measured on `main` at `21c2ff4`.

### 1. RH5111 — a declarator on a later line of its declaration, with a wrapped value

```csharp
// input
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
// before: `2` stands in column 18, off `1` in column 24
        int a = 1,
              b = Other(1,
                  2);
```

```csharp
// after
        int a = 1,
              b = Other(1,
                        2);
```

### 2. RH5111 — a declarator on a later line, whose value is an object creation or a lambda

```csharp
// input
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
```

```csharp
// before: the braces stand in column 12, off `new` and `(` in column 18
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
```

```csharp
// after
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
```

### 3. RH5111 — an enum member and a parameter on a later line, with a wrapped value

```csharp
// input
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
// before: `+` stands in column 8 and column 22, off `1` in column 10 and `"x"` in column 27
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

```csharp
// after
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

### 4. RH5111 — a parameter behind another parameter on its line, with a wrapped value

```csharp
// input
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
// before: `+` stands in column 22, under the place `"x"` has when `string c` gets a line of its own
    void M(int a,
           int b, string c = "x"
                      + "y")
```

```csharp
// after
    void M(int a,
           int b, string c = "x"
                             + "y")
```

### 5. RH5111 — a declarator behind the closing line of a raw string literal that formatting moves

```csharp
// input
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
// before: `+` stands in column 28, under the place `"y"` has once the literal is moved
        string a = """
            x
            """, b = "y"
                            + "z";
```

```csharp
// after
        string a = """
            x
            """, b = "y"
                     + "z";
```

### 6. RH3208 — a local function behind a label and several spaces, with wrapped parameters

```csharp
// input
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
// before: `int b` stands in column 17, off `int a` in column 20
        L:    int F(int a,
                 int b)
        {
            return a + b;
        }
```

```csharp
// after
        L:    int F(int a,
                    int b)
        {
            return a + b;
        }
```

Unchanged: every target whose first token the context's layout leaves in place (the first parameter behind `(`, a local function one space behind its label), every target whose other lines align to nothing on its first line (a one-line enum member such as `B = 2`, a one-line parameter), the blank lines, comments, and directives above a target (FMT-PIPE-018's examples), the tabs in front of a target in a tab-indented file, and the output of the fluent-chain fixes.

## Decisions

### The target's first token keeps its line and column

The first token stays where the document has it — on its line and in its column — instead of taking the place the context's layout gives it. Everything in front of the target is outside it and is written back as it stands (FMT-PIPE-010), so a first token that the layout puts on another line or in another column could only get there by rewriting code outside the target, or, for a target that starts its line, by changing its leading whitespace, which FMT-PIPE-018 keeps. `FormatNodeInDocumentAsync` already keeps its target's first token in its column (FMT-PIPE-017), so both node-in-document entry points take the same decision. Formatting the document later moves the first token, and the lines aligned to it move with it.

### The write-back of the leading trivia stays, and changes no column

The issue proposed that the entry point stop replacing the target's leading trivia after the last phase and write back what the phases produce. The preparation phase replaces every tab in whitespace and every foreign line ending in the root before any other phase reads it (FMT-PIPE-019), the target's leading trivia included, and FMT-PIPE-018 writes that trivia back as it stands: a declarator in a tab-indented file keeps its tabs. Writing back the phases' trivia would change that output. So the phases keep the trivia as the preparation phase leaves it and place the first token in its document column, and the entry point's write-back restores only what the preparation phase rewrote; it changes no line and no column, which is what FMT-PIPE-017 requires of every step after the last phase.

### Lines placed by the depth do not follow the target's first token

The indentation phase lays out the target's lines from its first token where they align to an anchor; lines it places by the depth — such as the block of a local function behind a label (RH3208-003) — stay where the context's depth places them. Shifting them by the first token's displacement, as FMT-PIPE-017 shifts the whole root by the root's offset, would move the block of the RH3208 fix away from the start of its line.

### The examples are code-fix inputs

FMT-PIPE-017's examples of the second and third way the layout can differ — an argument `Wrap(…)` of a call, and an invocation in an interpolation hole of a raw string literal, each with the statement as context — are inputs no caller passes: no code fix formats an argument or an invocation within its statement. They are replaced by RH5111 and RH3208 inputs, which reach the same ways.

## Alternatives considered

- **Keep the recorded gap** — rejected: the fixes write misaligned code that the alignment rules report and formatting moves again.
- **Give the target's first line the column the context's layout gives it** — rejected: it changes the whitespace in front of the target that FMT-PIPE-018 keeps, while `FormatNodeInDocumentAsync` keeps its target's first token in its column, and for a target that does not start its line it would have to rewrite code in front of the target, outside what the entry point writes back.
- **Write back the phases' leading trivia instead of the document's** — rejected: a tab-indented file would get spaces in front of the fixed construct (see *Decisions*).
- **Shift every line of the target by the first token's displacement** — rejected: it moves lines the depth places, such as the block of a labeled local function.
