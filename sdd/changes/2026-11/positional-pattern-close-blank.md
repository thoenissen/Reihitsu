# No blank line before a positional pattern's closing parenthesis

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | FMT-BLANK-019 (changed), FMT-BLANK-009 (changed), FMT-BLANK-018 (changed), FMT-BLANK-020 (changed) |
| Depends on | [positional-pattern-layout.md](positional-pattern-layout.md), [own-line-comment-before-every-brace.md](own-line-comment-before-every-brace.md) |

## What

FMT-BLANK-019 removes the blank lines directly before the closing `)` of the positional pattern clause of a recursive pattern — `(1, 2)` in `o is (1, 2)` and in `o is P(1, 2)`, behind `is`, in a switch expression arm, and in a `case` label — as it already does before the closing `]` of a list pattern and the closing `)` of a parenthesized pattern.

The requirements that read FMT-BLANK-019's set of tokens follow without a rule of their own, because they name the tokens FMT-BLANK-019 names:

- behind a non-region directive or disabled text in front of that `)`, the blank lines between the last directive and the `)` are removed and the one above the directive is kept (FMT-BLANK-009);
- above an ordinary comment glued to that `)`, the blank lines are removed (FMT-BLANK-018);
- between an ordinary comment on its own line and that `)`, the blank lines are removed (FMT-BLANK-020).

This change covers thoenissen/Reihitsu#1935.

## Why

[positional-pattern-layout.md](positional-pattern-layout.md) gave a wrapped positional pattern the layout of a list pattern: one subpattern per line, the `)` on its own line in the column of the `(` (FMT-BREAK-022, FMT-INDENT-025), and a comment in front of the `)` indented into the subpatterns (FMT-INDENT-006). FMT-BLANK-019 was not extended with it, so a blank line in front of the positional `)` is kept while the same blank line in front of a list pattern's `]` is removed. FMT-BLANK-019's reason — the closing token ends a construct laid out one element per line, and a blank line in front of it separates no two elements — holds for the positional clause in the same way.

## Visible output changes

Whole-document formatting; "Before" is the output on `main`, observed with `scripts/format.sh`, and stable on a second pass. "After" is the output this change requires; for the comment and directive cases it is the output `main` already gives for the same input with a list pattern's `[`…`]`.

### 1. A blank line directly before the `)` (FMT-BLANK-019)

Input:

```csharp
class P
{
    public void Deconstruct(out int a, out int b)
    {
        a = 0;
        b = 0;
    }
}

class C
{
    void M(object o, (int, int) t)
    {
        var a = o is P(
            1,
            2

        );
        var b = t is (
            1,
            2

        );
        var c = o switch
        {
            P(
                1,
                2

            ) => 1,
            _ => 0,
        };
    }
}
```

Before:

```csharp
class P
{
    public void Deconstruct(out int a, out int b)
    {
        a = 0;
        b = 0;
    }
}

class C
{
    void M(object o, (int, int) t)
    {
        var a = o is P(
                          1,
                          2

                      );
        var b = t is (
                         1,
                         2

                     );
        var c = o switch
                {
                    P(
                         1,
                         2

                     ) => 1,
                    _ => 0,
                };
    }
}
```

After:

```csharp
class P
{
    public void Deconstruct(out int a, out int b)
    {
        a = 0;
        b = 0;
    }
}

class C
{
    void M(object o, (int, int) t)
    {
        var a = o is P(
                          1,
                          2
                      );
        var b = t is (
                         1,
                         2
                     );
        var c = o switch
                {
                    P(
                         1,
                         2
                     ) => 1,
                    _ => 0,
                };
    }
}
```

### 2. A comment on its own line, a comment glued to the `)`, and a directive in front of the `)` (FMT-BLANK-020, FMT-BLANK-018, FMT-BLANK-009)

Input:

```csharp
class P
{
    public void Deconstruct(out int a, out int b)
    {
        a = 0;
        b = 0;
    }
}

class C
{
    void M(object o)
    {
        var a = o is P(
            1,
            2
            // last

        );
        var b = o is P(
            1,
            2

            /* last */ );
        var c = o is P(
            1,
            2

#if DEBUG
#endif

        );
    }
}
```

Before:

```csharp
class P
{
    public void Deconstruct(out int a, out int b)
    {
        a = 0;
        b = 0;
    }
}

class C
{
    void M(object o)
    {
        var a = o is P(
                          1,
                          2

                          // last

                      );
        var b = o is P(
                          1,
                          2

                      /* last */ );
        var c = o is P(
                          1,
                          2

#if DEBUG
#endif

                      );
    }
}
```

After — the blank line between `// last` and `)` goes, the one FMT-BLANK-011 puts above `// last` stays; the blank line above the glued `/* last */` goes; the blank line between `#endif` and `)` goes, the one above `#if` stays:

```csharp
class P
{
    public void Deconstruct(out int a, out int b)
    {
        a = 0;
        b = 0;
    }
}

class C
{
    void M(object o)
    {
        var a = o is P(
                          1,
                          2

                          // last
                      );
        var b = o is P(
                          1,
                          2
                      /* last */ );
        var c = o is P(
                          1,
                          2

#if DEBUG
#endif
                      );
    }
}
```

### 3. Code-fix results

Every code fix that formats a node through the formatter's node-level entry points runs the blank-line phase over that node, so its result changes in the same way when the node holds a wrapped positional pattern with a blank line in front of its `)`. No code fix writes such a blank line itself; the blank line is always the author's. Observed with `scripts/apply-fix.sh RH3202` (RH3202 · Expression-style methods should not be used):

Input:

```csharp
class P
{
    public void Deconstruct(out int a, out int b)
    {
        a = 0;
        b = 0;
    }
}

class C
{
    bool M(object o) => o is P(
        1,
        2

    );
}
```

Before (the fix's result on `main`, the class `P` unchanged):

```csharp
class C
{
    bool M(object o)
    {
        return o is P(
                         1,
                         2

                     );
    }
}
```

After:

```csharp
class C
{
    bool M(object o)
    {
        return o is P(
                         1,
                         2
                     );
    }
}
```

### Unchanged

- A blank line behind the `(` of a positional pattern is kept, as behind the `[` of a list pattern (FMT-BLANK-019).
- The closing parenthesis of any other node — an argument list, a parameter list, a tuple expression, a parenthesized variable designation such as `var (x, y)` — is not named, and its blank lines are not changed by this change.
- Below a region directive the blank line in front of the `)` stays (SHARED-BLANK-006, FMT-BLANK-013), as in front of a list pattern's `]`.
- No rule page changes: no analyzer rule reports a blank line in front of a pattern's closing delimiter.

## Decisions

### Option A of the output question on thoenissen/Reihitsu#1935

Decided by the maintainer (2026-11-22): the blank line in front of a positional pattern's closing `)` is removed, like the one in front of a list pattern's `]`, and the requirements that refer to FMT-BLANK-019 follow. Visible output changes 1 and 3 are that answer; visible output change 2 is the same token set read by FMT-BLANK-009, FMT-BLANK-018, and FMT-BLANK-020.

### The node that owns the `)` decides

FMT-BLANK-019 names the token by its kind and its parent node — the closing parenthesis whose parent is a positional pattern clause — never by the layout of the clause. A positional pattern written on one line has no blank line in front of its `)`, and a wrapped one is laid out by FMT-BREAK-022 before the blank-line phase runs, so no layout question arises; the blank-line phase reads only the syntax (FMT-PIPE-025). The tuple-shaped clause without a type, `o is (1, 2)`, is the same node as `o is P(1, 2)` and is named with it.

### One set for the four requirements

FMT-BLANK-009, FMT-BLANK-018, and FMT-BLANK-020 each name the tokens FMT-BLANK-019 names, so the positional `)` joins all four at once. Leaving it out of one of them would give the positional `)` a blank-line rule of its own behind a comment or a directive, which is what [own-line-comment-before-every-brace.md](own-line-comment-before-every-brace.md) removed for the braces.

## Alternatives considered

- **Option B — keep the blank line and state that FMT-BLANK-019 does not cover the positional clause** — rejected by the maintainer's answer.
- **Remove the blank line directly before the `)` only, and keep it behind a comment or a directive** — rejected: the four requirements share one token set, and a split would make the positional `)` the one closing delimiter whose blank lines depend on what stands in front of it.
- **Name every closing parenthesis** — not part of this change: neither the issue nor the maintainer's answer asks for it, and it would change the output for argument lists, parameter lists, and tuples, which FMT-BLANK-019's reason does not cover.
