# Formatter — Line endings

| | |
|---|---|
| ID prefix | FMT-EOL |
| Status | Active |
| Code owners | `LineEndingNormalizationPhase` |
| Uses | FMT-PIPE-002, FMT-PIPE-007 |

This spec covers `LineEndingNormalizationPhase`: as the last phase, it rewrites every line break that is still not the file's detected line ending (FMT-PIPE-007) — in code, in directives, and in documentation comments — and leaves the line breaks inside literals alone.

## Non-goals

- How the line ending is detected, and which line ending the earlier phases write — FMT-PIPE-007.
- Whether the file ends with a line break — FMT-CLEAN-005 to FMT-CLEAN-007.
- Line breaks inside multi-line block comments and disabled text. The phase currently leaves them as written; whether it should normalize them is undecided.

## Requirements

### FMT-EOL-001 — Every line break between tokens uses the detected line ending

The formatter MUST rewrite every end-of-line trivia whose text differs from the detected line ending to that line ending. This MUST cover every line terminator C# recognizes: `\r\n`, `\n`, a lone `\r`, and the Unicode terminators U+0085, U+2028, and U+2029.

Why: A file with mixed line endings produces whole-file diffs and editor warnings (FMT-PIPE-007). The phase runs last, so it also catches a line break an earlier phase copied from the input instead of writing it fresh. An exotic terminator is a line break to the compiler but often not to other tools, so it is replaced like any other.

Example — the input has one `\r\n` among `\n` line breaks (`␍` marks a `\r`):

```csharp
class C
{
    int x;␍
    int y;
}
```

```csharp
class C
{
    int x;
    int y;
}
```

A lone `\r` is a line break of its own:

```csharp
class C
{
    int x;␍    int y;
}
```

```csharp
class C
{
    int x;
    int y;
}
```

The same holds when the terminator after `int x;` is U+0085, U+2028, or U+2029 (not shown as blocks because the characters are invisible).

### FMT-EOL-002 — Line breaks that end directives are normalized

The formatter MUST apply FMT-EOL-001 to the line break that ends a preprocessor directive.

Why: A directive's line break lives inside the directive's structured trivia, not between tokens, but it is a line of the file like any other.

Example:

```csharp
class C
{
#if DEBUG␍
    int x;
#endif
    int y;
}
```

```csharp
class C
{
#if DEBUG
    int x;
#endif
    int y;
}
```

### FMT-EOL-003 — Line breaks inside documentation comments are normalized

The formatter MUST rewrite every line break inside a documentation comment — single-line `///` and delimited `/** */` — that differs from the detected line ending to that line ending.

Why: The lines of a documentation comment are separated by newline tokens of the documentation structure, not by end-of-line trivia, so they need their own pass; to the reader they are lines of the file like any other. The documentation-comment phase keeps whatever separator it finds (FMT-DOC), relying on this phase to normalize it.

Example — a `\r\n` file in which one documentation line ends in `\n`:

```csharp
class C␍
{␍
    /// <summary>
    /// Gets the value.␍
    /// </summary>␍
    int A;␍
}␍
```

```csharp
class C␍
{␍
    /// <summary>␍
    /// Gets the value.␍
    /// </summary>␍
    int A;␍
}
```

### FMT-EOL-004 — Line breaks inside literals are kept

The formatter MUST NOT change a line break inside a string literal — verbatim, interpolated verbatim, or raw — even when it differs from the detected line ending.

Why: A line break inside a literal is part of the string's value (FMT-PIPE-002). Normalizing it would change what the program prints, compares, or hashes.

Example — a `\n` file whose verbatim and raw literals contain `\r\n`, left unchanged:

```csharp
class C
{
    string s = @"a␍
b";
    string t = $@"c␍
{1}";
    string u = """␍
               d␍
               """;
}
```
