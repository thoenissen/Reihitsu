# Shared — Blank-line spacing

| | |
|---|---|
| ID prefix | SHARED-BLANK |
| Status | Active |
| Code owners | `BlankLineSpacingPolicy`, `TokenGapAnalysis`, `RegionDirectiveBlankLineUtilities`, `FormattingTextAnalysisUtilities` |
| Uses | FMT-BLANK-001, FMT-BLANK-004, FMT-PIPE-002 |

This spec covers the blank-line rules that analyzer, code fixes, and formatter decide through the same `Reihitsu.Core` code: what counts as a blank line and as the start of a line, the blank line after a closing brace, the exemption of a switch section's terminal `break`, and the blank lines around `#region` and `#endregion`. The formatter's own blank-line decisions are specified in FMT-BLANK.

## Non-goals

- The statement-kind rules (blank line before `if`, `return`, local declarations, and so on, RH5001–RH5019, RH5021, RH5029), comment boundaries (RH5020), and blank lines next to braces (RH5022, RH5024–RH5028). Each analyzer and the formatter implement these separately; the formatter's side is FMT-BLANK.
- Collapsing consecutive blank lines (RH5023 and FMT-BLANK). The analyzer and the formatter do not share code for it.
- Where a code fix or the formatter writes the inserted line break, beyond what a requirement here states.

## Requirements

### SHARED-BLANK-001 — A blank line holds nothing but whitespace

Analyzer and formatter MUST count as a blank line only a line between two tokens that holds nothing but whitespace before its line break. A line inside the text of a comment MUST NOT count, even when it is empty.

Why: A blank line is a visual separator the reader sees between pieces of code. An empty line inside a multi-line block comment belongs to the comment's text and separates nothing; counting it would let a comment satisfy a rule that asks for a separator in front of the code that follows. `TokenGapAnalysis` is the one measure, used by the formatter's blank-line phase and by the code fixes of RH5001–RH5010, RH5012–RH5019, RH5021, and RH5029 to decide how many line breaks to insert.

Example — the empty line inside the block comment does not separate the following comment:

```csharp
class C
{
    void M()
    {
        var x = 1; /* a

        b */
        // next
        x++;
    }
}
```

```csharp
class C
{
    void M()
    {
        var x = 1; /* a

        b */

        // next
        x++;
    }
}
```

### SHARED-BLANK-002 — Trivia starts its line only after a line break that ends the previous content

Analyzer and formatter MUST treat a comment or other trivia as starting its line only when a line break follows the last content in front of it — the preceding token or earlier trivia. A line break inside the text of a comment MUST NOT count, so trivia behind a multi-line comment that ends on its line does not start that line; a single-line comment or a directive in front of it does end the previous line. Without a preceding token (the start of a file or of a detached formatting root), trivia starts its line when nothing but whitespace precedes it.

Why: Rules about "a comment on its own line" — whether it needs a blank line above it, or how it is indented (RH5204 and the formatter's indentation) — must agree on which comments are on their own line. A comment behind `b */` sits on a line the author already filled with code and comment text; treating it as line-starting would make the formatter break the line, which is a placement decision, not spacing.

Example — the documentation comment behind `b */` does not start its line and is left unchanged:

```csharp
class C
{
    void M(int a)
    {
        var x = 1; /* a
        b */ /// stray
    }
}
```

while a comment on the next line does start its line and is separated (SHARED-BLANK-001 example, and):

```csharp
class C
{
    void M()
    {
        var x = 1; /* a
        b */
        // next
        x++;
    }
}
```

```csharp
class C
{
    void M()
    {
        var x = 1; /* a
        b */

        // next
        x++;
    }
}
```

### SHARED-BLANK-003 — A blank line follows a statement that ends with a closing brace

When a statement whose last token is `}` is followed by another statement in the same block or switch section, analyzer and formatter MUST require a blank line between them. No blank line is required when one is already there, or when the following statement is the terminal `break` of SHARED-BLANK-004. A preprocessor directive between the two does not exempt the pair. The analyzer and code fixes MUST NOT require the blank line when both statements share one line.

Why: A closing brace ends a visual block, and the blank line marks where the next unit of work starts. The same-line exemption keeps RH5030 from reporting compact code such as `if (x) { } Consume();`, whose layout is a line-break question; the formatter splits such a line anyway (FMT-BREAK) and then separates the statements (FMT-BLANK). RH5030 reports through this policy, and RH3208's code fix uses it to decide whether the block it creates needs a blank line after it.

Example:

```csharp
class C
{
    void M(int a)
    {
        if (a > 0)
        {
            a++;
        }
        a++;
        while (a > 0)
        {
            a--;
        }

        a++;
    }
}
```

```csharp
class C
{
    void M(int a)
    {
        if (a > 0)
        {
            a++;
        }

        a++;

        while (a > 0)
        {
            a--;
        }

        a++;
    }
}
```

The blank line before `while` comes from its own statement-kind rule (FMT-BLANK-001), not from this requirement.

### SHARED-BLANK-004 — The terminal `break` of a switch section is exempt

Analyzer and formatter MUST treat a `break` statement as exempt from the "blank line before `break`" rule and from SHARED-BLANK-003 only when it is the last statement owned directly by a switch section. A `break` that is not the section's last statement, or that sits inside a block the section owns, MUST NOT be exempt.

Why: The terminal `break` closes its section; separating it from the section's body by a blank line would split the section visually from its own end. The canonical braced section `case 1: { … } break;` depends on this, so the switch-section brace rewriting (FMT-SWITCH) keeps such a `break` outside the block it creates by the same predicate. A `break` inside a block, or in the middle of a section, ends a path like any other jump statement and keeps its blank line. RH5010 and RH5030 apply the exemption through `BlankLineSpacingPolicy`.

Example:

```csharp
class C
{
    void M(int a)
    {
        switch (a)
        {
            case 1:
                {
                    a++;
                }
                break;
            case 2:
                {
                    a++;
                    break;
                }
        }
    }
}
```

```csharp
class C
{
    void M(int a)
    {
        switch (a)
        {
            case 1:
                {
                    a++;
                }
                break;

            case 2:
                {
                    a++;

                    break;
                }
        }
    }
}
```

The blank line before `case 2:` is FMT-BLANK-004.

### SHARED-BLANK-005 — A blank line precedes each region directive

Analyzer and formatter MUST require the line directly above an active `#region` or `#endregion` directive to be blank. No blank line is required when the directive is on the first line, or when the line above ends with an opening brace `{` followed by nothing but whitespace. Only the single line directly above counts: a comment there does not satisfy the requirement, and a blank line further up does not either. A brace line that carries a trailing comment, and a switch-label line, are not exempt.

Why: Regions structure a file into sections; a blank line above the directive makes the section boundary visible. Right after an opening brace the brace itself is the boundary, so a blank line there would only add the empty line RH5022 forbids. A trailing comment on the brace line makes it a content line like any other. RH5031 reports this, its code fix inserts the line, and the formatter's blank-line phase applies the same rule.

Example:

```csharp
class C
{
    #region Fields
    private int _x;
    #endregion // Fields
    #region Methods
    void M()
    {
    }
    #endregion // Methods
}
```

```csharp
class C
{
    #region Fields

    private int _x;

    #endregion // Fields

    #region Methods

    void M()
    {
    }

    #endregion // Methods
}
```

A comment directly above the directive does not count as the blank line:

```csharp
class C
{
    int _a;
    // header
    #region R

    int _b;

    #endregion // R
}
```

```csharp
class C
{
    int _a;

    // header

    #region R

    int _b;

    #endregion // R
}
```

A trailing comment on the brace line removes the brace exemption:

```csharp
class C
{ // trailing
    #region R
    int _x;

    #endregion // R
}
```

```csharp
class C
{ // trailing

    #region R

    int _x;

    #endregion // R
}
```

### SHARED-BLANK-006 — A blank line follows each region directive

Analyzer and formatter MUST require the line directly below an active `#region` or `#endregion` directive to be blank. No blank line is required when the directive is on the last line, or when it is an `#endregion` whose next line starts with a closing brace `}`. The closing-brace exemption applies to `#endregion` only.

Why: The blank line below a directive separates the section header or footer from the code it frames. An `#endregion` that closes the last section of a type is already framed by the type's closing brace. RH5032 reports this and its code fix inserts the line. The example under SHARED-BLANK-005 shows both sides, including the `#endregion` before `}` that keeps no blank line.

### SHARED-BLANK-007 — Region directives in disabled code are not considered

Analyzer and formatter MUST apply SHARED-BLANK-005 and SHARED-BLANK-006 only to region directives the compiler keeps. A region directive inside a branch of `#if` that is not taken MUST NOT be reported or padded.

Why: Inside a skipped branch the directive and the lines around it are disabled text, which the formatter must not rewrite (FMT-PIPE-002). A line break inserted next to such a directive merges into that text where no blank-line query finds it again, so every later run would insert another one.

Example:

```csharp
class C
{
#if NEVER
    #region R
    int _x;
    #endregion // R
#endif
    int _y;
}
```

is left unchanged.
