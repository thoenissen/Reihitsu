# A comment in front of a hole's closing brace takes the brace's column

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-INDENT-006 (changed), FMT-RAW-007 (clarified) |

## What

A comment on its own line in front of the closing brace of an interpolation takes the column of that brace when the interpolated string is a regular, verbatim, or single-line raw one. Until now it was indented one level deeper than the brace, as in front of every closing brace. The interpolations of a multi-line raw string literal keep that rule.

## Why

FMT-INDENT-006 indents a comment in front of a closing brace one level deeper than the brace because the content a brace closes stands one level deeper — a block's statements, an initializer's elements. The hole of a regular, verbatim, or single-line raw interpolated string does not: its lines, the closing brace included, are continuation lines of the statement (FMT-INDENT-034), not a scope one level deeper. The comment in front of the closing brace was therefore the one line of the hole four columns to the right of the brace, also when written in its column. In a multi-line raw string literal the hole's lines move with the literal (FMT-RAW-007), where content one level deeper than the braces is the usual shape, so the rule fits there and stays.

## Visible output changes

Each "before" is the output of `scripts/format.sh` on this branch's base (fe7b8f8). Each "after" is the layout this change specifies; it is not built yet.

The issue showed `// e`, `a`, and the closing brace in column 8 on `main` at the time it was filed; on fe7b8f8 they stand in column 12 (FMT-INDENT-034), and only the comment in front of the brace differs.

### 1. Regular interpolated string

```csharp
class C
{
    void M(int a)
    {
        var t = $"{
            // e
            a
            // f
            }";
    }
}
```

Before — `// f` in column 16, also when written in column 8 or 12:

```csharp
        var t = $"{
            // e
            a

                // f
            }";
```

After:

```csharp
        var t = $"{
            // e
            a

            // f
            }";
```

The blank line in front of `// f` is FMT-BLANK-011's, which puts one above every comment that starts its own line behind code, in a hole as anywhere else; this change does not touch it.

### 2. Verbatim, single-line raw, and a regular string nested in a multi-line raw literal's hole

```csharp
class C
{
    string M(int a)
    {
        var v = $@"{
            a
            // f
            }";
        var s = $"""{
            a
            // f
            }""";
        var r = $"""
            {$"{
                a
                // f
                }"}
            """;

        return v + s + r;
    }
}
```

Before:

```csharp
        var v = $@"{
            a

                // f
            }";
        var s = $"""{
            a

                // f
            }""";
        var r = $"""
                 {$"{
                     a

                         // f
                     }"}
                 """;
```

After:

```csharp
        var v = $@"{
            a

            // f
            }";
        var s = $"""{
            a

            // f
            }""";
        var r = $"""
                 {$"{
                     a

                     // f
                     }"}
                 """;
```

### 3. The hole's content in another column than its closing brace

```csharp
class C
{
    string M(int a, int b)
    {
        var u = $"{a +
            b
            // f
            }";

        return u;
    }
}
```

Before — the operand `+ b` aligns under `a` in column 19, the closing brace stands in column 12, and `// f` in column 16, in neither:

```csharp
        var u = $"{a
                   + b

                // f
            }";
```

After — the comment takes the column of the brace it precedes:

```csharp
        var u = $"{a
                   + b

            // f
            }";
```

### 4. Not changed: a multi-line raw string literal's own hole

```csharp
class C
{
    void M(int a)
    {
        var r = $$"""
                  {{
                      a

                      // f
                  }}
                  """;
    }
}
```

is left unchanged, before and after.

### Code-fix results

Every code fix that formats a node holding such a comment gives the new column. Checked with `scripts/apply-fix.sh`: RH5205 on a misaligned block lambda whose body holds `return $"{⏎f⏎// f⏎}";` puts `// f` four columns right of the hole's closing brace today, and in the brace's column after this change. RH5101 and RH5111 withhold their fix for an argument or a value that holds the comment, before and after.

No rule page changes: RH5204, which checks the column of a comment in front of a closing brace, checks only the braces of scopes, not the brace of an interpolation, and reports neither layout.

## Decisions

### Maintainer answer (2026-11-27)

Asked as an output question with the inputs of entries 1 and 3: **A** the comment takes the column of the hole's closing brace and keeps the blank line above it, **A′** the column of the hole's content line above it, **B** unchanged, **C** as A without the blank line. The maintainer chose **A**, as recommended.

### The boundary is the kind of interpolated string

The rule follows the string the interpolation belongs to: an interpolation of a multi-line raw string literal keeps the one-level-deeper rule, every other interpolation's closing brace is treated like the closing delimiter of any other construct, which FMT-INDENT-006 already places the comment in front of at the delimiter's column. A regular string nested in a multi-line raw literal's hole is a regular string: its interpolation's brace takes the comment to its column.

### The blank line stays

The issue left the blank line in front of the comment to be decided with the column. FMT-BLANK-011 puts it there, as in front of every comment that starts its line behind code, and FMT-BLANK-020 names the closing brace of an interpolation among the braces behind such a comment, so the blank line is specified and stays.

## Alternatives considered

- **Keep the column one level deeper and state it** (the issue's option B) — rejected by the maintainer's answer: it keeps the hole's lines in two columns.
- **The column of the hole's content line instead of the brace's** (the issue's wording, "the column of the hole's other content lines") — rejected by the maintainer's answer: it differs from the brace's column when the content is aligned (entry 3, `// f` in column 19), and the formatter has no notion of a hole's content column; a comment takes the column of the token it precedes, as in front of every other closing delimiter whose content is not one level deeper.
- **Drop the blank line in front of the comment as well** — rejected by the maintainer's answer: it would need an exception to FMT-BLANK-011 for interpolations, and the same comment in a multi-line raw literal's hole keeps its blank line.
- **Apply the brace's column to multi-line raw literals too** — rejected: their hole content is usually one level deeper than the braces, and the comment would then stand left of it.
