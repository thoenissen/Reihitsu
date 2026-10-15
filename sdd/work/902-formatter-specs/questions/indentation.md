# Open questions — indentation.md (FMT-INDENT)

## Q1 — Continuation lines that no alignment rule owns fall back to the block column

Block indentation (FMT-INDENT-002) counts only brace scopes, switch sections, and unbraced embedded statements. A line that continues an expression or declaration gets a different column only when an alignment contributor owns it (FMT-INDENT-012 to FMT-INDENT-030). Every other continuation line is placed at the column of the enclosing statement or member, as if it started a new one.

Input:

```csharp
class C
{
    void M()
    {
        var x = 1 + // c
            2;
    }
}
```

Output (`Indentation, Cleanup`, idempotent):

```csharp
class C
{
    void M()
    {
        var x = 1 + // c
        2;
    }
}
```

The same mechanism produces:

- structural-transforms.md Q2 (routed here): `int Get() =>` + `#pragma` + `42;` puts `42;` at the member column; so does the active branch of `#if`/`#else` after `=>`, and the last declarator `_y;` of a field kept intact by FMT-STRUCT-023.
- structural-transforms.md Q4 (routed here): `int Other()` + `#region Body` + `=> 42;` puts `=> 42;` at the member column. The region directives themselves are at the member column as FMT-INDENT-010 requires; the blank lines are FMT-BLANK's.
- A pattern case label wrapped after `or` (switch-case-braces author's note), phases `Indentation, Cleanup`:

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1 or
                2:
                break;
        }
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
            case 1 or
            2:
                break;
        }
    }
}
```

  `BinaryExpressionContributor` aligns only an operator that starts a line, never an operand.

- The content of a parenthesized pattern that is not a property pattern, phases `LineBreak, Indentation, Cleanup`:

```csharp
class C
{
    bool M(object o)
    {
        return o is (
        int or long
            );
    }
}
```

```csharp
class C
{
    bool M(object o)
    {
        return o is
                 (
        int or long
                 );
    }
}
```

  The parentheses are aligned (FMT-INDENT-026), the pattern between them stays at the statement column.

Why it looks wrong: a continuation line at the column of a statement reads as a new statement. FMT-BREAK joins most such lines, so the fallback is only visible when a comment, a directive, or disabled text holds the line break. Should a continuation line get one indentation level beyond the line it continues (or the column of the construct it continues), and should that be a general rule rather than one contributor per construct? The spec lists the fallback as a non-goal and does not specify it.

## Q2 — A directive directly after disabled text keeps its indentation

Input:

```csharp
class C
{
    void M()
    {
        var x = 0;
            #if !NEVER
        x = 1;
          #else
        x = 2;
                #endif
    }
}
```

Output (`Indentation, Cleanup`, idempotent):

```csharp
class C
{
    void M()
    {
        var x = 0;
#if !NEVER
        x = 1;
#else
        x = 2;
                #endif
    }
}
```

`#if` and `#else` move to column 0 (FMT-INDENT-009), but `#endif`, which follows the disabled `#else` branch, keeps its sixteen spaces. When the branch before `#endif` is active (`#if NEVER … #else … #endif`), `#endif` does move to column 0. `IndentationRewriter.RebuildLeadingTrivia` stops treating the trivia as "at line start" after a disabled-text trivia, so the whitespace in front of the following directive is kept as written. FMT-INDENT-009 is stated without this exception. Is the whitespace in front of the directive part of what FMT-PIPE-002 protects as disabled text (then FMT-INDENT-009 needs the exception), or a gap in the rewriter?

## Q3 — Who owns the line starts inside a multi-line interpolation hole of a raw string (raw-string-alignment.md Q1)

See raw-string-alignment.md Q1 for the input and the two alternating outputs (`Indentation, RawStringAlignment, Cleanup`, NOT-IDEMPOTENT).

From the indentation side: the tokens inside the hole (`name`, `}`) are real tokens that start their lines, so the indentation phase gives them block columns (the statement column), and FMT-RAW then shifts every line after the opening line, hole lines included, by the literal's delta. The two decisions add up instead of agreeing.

Suggested ownership: the line starts inside a multi-line raw string literal, including the lines of its interpolation holes, belong to FMT-RAW alone — the indentation phase should skip tokens that start a line inside a multi-line raw interpolated string, so FMT-RAW's uniform shift is the only move and FMT-RAW-002 (relative indentation kept) covers the hole lines too. The alternative — FMT-INDENT owns the hole lines and FMT-RAW skips them — would have to define a column for code inside the literal relative to content it does not otherwise touch. FMT-INDENT lists the case as a non-goal until this is decided.

## Q4 — `SwitchExpressionContributor` documents a different anchor than it uses

`SwitchExpressionContributor`'s summary says the braces align to the `switch` keyword column, and an integration test is named `SwitchExpressionArmsRemainAlignedWithSwitchKeyword`, but the code aligns them to the first token of the governing expression (`return x switch` puts `{` under `x`). FMT-INDENT-027 specifies the observed behavior, which is also what every existing test asserts. Not a behavior question; a comment fix for whoever touches the contributor next.
