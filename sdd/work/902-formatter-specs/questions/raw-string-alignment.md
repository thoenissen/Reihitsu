# Open questions — raw-string-alignment.md

## Q1 — A multi-line interpolation hole makes the result non-idempotent

Input:

```csharp
class C
{
    void M()
    {
        var name = "x";
        var a = $"""
    Hello {
        name
    }
    """;
    }
}
```

First pass (phases `Indentation, RawStringAlignment, Cleanup`, NOT-IDEMPOTENT):

```csharp
class C
{
    void M()
    {
        var name = "x";
        var a = $"""
                 Hello {
                     name
                     }
                 """;
    }
}
```

Second pass:

```csharp
class C
{
    void M()
    {
        var name = "x";
        var a = $"""
                 Hello {
        name
        }
                 """;
    }
}
```

The indentation phase re-indents the tokens inside the hole, and this phase then shifts every line after the first by the same delta, including the lines of the hole. The two phases disagree, so the output flips between passes, violating FMT-PIPE-004. Which phase owns the line starts inside a multi-line interpolation hole? Not specified; listed under FMT-RAW non-goals.

## Q2 — A raw string nested in an interpolation hole is non-idempotent

Input:

```csharp
class C
{
    void M()
    {
        var name = "x";
        var a = $"""
    Outer {"""
        inner
        """}
    """;
    }
}
```

First pass (phases `RawStringAlignment, Cleanup`, NOT-IDEMPOTENT):

```csharp
class C
{
    void M()
    {
        var name = "x";
        var a = $"""
                 Outer {"""
                     inner
                     """}
                 """;
    }
}
```

Second pass:

```csharp
class C
{
    void M()
    {
        var name = "x";
        var a = $"""
                 Outer {"""
                        inner
                        """}
                 """;
    }
}
```

The phase collects a replacement for the outer and for the inner literal from the original tree; `ReplaceNodes` uses the outer replacement and discards the inner one, so the inner literal is only aligned on the next pass. Violates FMT-PIPE-004; the expected fixed point is the second-pass output. Not specified; listed under FMT-RAW non-goals.

## Q3 — Tab-indented raw strings get spaces in front of the tabs

Input (the content line and the closing line of the literal start with two tab characters, shown as `→→`):

```csharp
class C
{
    void M()
    {
        var a = """
→→Hello
→→""";
    }
}
```

Output (phases `RawStringAlignment, Cleanup`, idempotent):

```csharp
class C
{
    void M()
    {
        var a = """
                →→Hello
                →→""";
    }
}
```

The phase counts only leading spaces, so it treats the closing marker as being at column 0 and prepends 16 spaces in front of the existing tabs. The value is preserved and the result is stable, but the line now mixes spaces and tabs and the closing quotes do not visually start under the opening quotes. Should the phase replace the leading whitespace of a tab-indented literal (the closing line's whitespace is a common prefix of every content line, so this is value-preserving), or leave such a literal alone? FMT-RAW-001 is silent on tabs.
