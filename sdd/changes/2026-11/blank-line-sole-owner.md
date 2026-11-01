# The blank-line phase is the only owner of blank lines

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | FMT-BLANK-008 (changed), FMT-BLANK-009 (changed), FMT-BLANK-011 (changed), FMT-BLANK-017 (added), FMT-BLANK-018 (added), FMT-BLANK-019 (added, clarification), FMT-BREAK-030 (added), FMT-PIPE-024 (changed), RH5025-005 (added) |

## What

The blank-line phase decides every blank line of the formatter's output; the line-break phase only adds and removes line breaks and keeps, in every gap it does not join, the blank lines it finds.

- **The opening brace of a block statement is not a header continuation.** FMT-BLANK-008 no longer removes the blank line above the `{` of a block statement that follows another statement of the same block or switch section (FMT-BLANK-017). The blank lines above such a block are decided like those above any other statement: SHARED-BLANK-003 asks for one after a statement that ends in `}` (placed behind a directive by FMT-BLANK-005), FMT-BLANK-004 asks for one after a `break` statement, and after any other statement the author's blank line is kept and none is inserted. FMT-BLANK-009, which only refines FMT-BLANK-008 around directives, no longer applies to that brace either.
- **RH5025 gets the matching exemption** (RH5025-005): it does not report the blank line above the `{` of such a block statement.
- **A comment glued to a brace gets no FMT-BLANK-011 blank line** (FMT-BLANK-018). A comment that starts its line and shares it with the brace, bracket, or parenthesis behind it — the braces of every construct except an array initializer's braces and a property pattern's opening brace, the closing bracket of a collection expression or list pattern, and the closing parenthesis of a parenthesized pattern — gets no blank line above it from FMT-BLANK-011, and a blank line the author wrote above it is removed. This keeps today's output for every such comment: today the blank-line phase inserts or keeps the line and the line-break phase removes it again. A comment glued to the opening brace of a block statement that follows a statement is the exception: FMT-BLANK-017 decides the blank lines above it. The blank lines of SHARED-BLANK-003 and FMT-BLANK-004 are not affected.
- **The blank line before a closing bracket moves to the blank-line phase** (FMT-BLANK-019). Today only the line-break phase removes a blank line before the closing `]` of a collection expression or a list pattern and before the closing `)` of a parenthesized pattern. FMT-BLANK-019 writes that output down as a blank-line rule, so it stays when the line-break phase stops removing blank lines.
- **The line-break phase keeps blank lines** (FMT-BREAK-030). FMT-BREAK's non-goal "Where this phase moves a token onto a new line it inserts no blank line" becomes a requirement: in every gap the phase does not join, it leaves the number of blank lines as it found it.
- **Node-level formatting gives a block statement a list position like any other statement** (FMT-PIPE-024). The exception that FMT-PIPE-024 recorded until this change — a block statement got a list position only behind an own-line comment, because the line-break phase decided the gap above its brace — is removed.

RH5020 is unchanged: it keeps reporting a comment that starts its line glued to a `{` without a blank line above it, which the formatter now leaves unseparated by decision. RH5030 is unchanged: it keeps reporting a closing brace followed by a block statement without a blank line, which the formatter no longer produces.

### Precedence

The blank-line requirements that insert a blank line (SHARED-BLANK-003, FMT-BLANK-001 to FMT-BLANK-005, FMT-BLANK-011, FMT-BLANK-012) and the ones that remove one (FMT-BLANK-007, FMT-BLANK-008, FMT-BLANK-009, FMT-BLANK-019) are scoped so that they do not decide the same blank line. Where they met before this change, the scope of the removal or insertion rule is narrowed instead of ranking one over the other: FMT-BLANK-008 does not reach the opening brace of a block statement that follows a statement, so SHARED-BLANK-003 and FMT-BLANK-004 decide the line above it; FMT-BLANK-011 does not reach a comment glued to the braces FMT-BLANK-018 names, so no rule inserts one there. Elsewhere a removal rule removes what no insertion rule asks for.

## Why

Two phases decide the blank line next to a brace, with contradicting rules, and the later one wins:

- After `}`, the blank-line phase inserts the blank line SHARED-BLANK-003 requires above a block statement, and the line-break phase removes it while it places the block's `{`. The formatted output has no blank line, so RH5030 reports it; if the blank line stayed, RH5025 would report it. The formatter's output can satisfy both rules only when one of them yields.
- Above a comment glued to `{`, the blank-line phase inserts the FMT-BLANK-011 blank line and the line-break phase removes it again, in every pass. The output is stable only because the line-break phase runs later.

A block statement's `{` starts a statement; it continues no header, which is what FMT-BLANK-008 and RH5025 protect. Letting the statement rules decide the blank line above it makes the formatter's output satisfy RH5030 and RH5025 at once, and with one owner per blank line no phase undoes another, the line-break phase no longer needs guards against the blank-line phase, and node-level formatting no longer predicts the line-break phase's removal.

## Visible output changes

### 1. A block statement after a statement that ends in `}` gets the blank line

```csharp
class C
{
    void M(bool a)
    {
        if (a)
        {
            A();
        }
        {
            B();
        }
    }
}
```

Before:

```csharp
class C
{
    void M(bool a)
    {
        if (a)
        {
            A();
        }
        {
            B();
        }
    }
}
```

RH5030 reports the first closing brace.

After:

```csharp
class C
{
    void M(bool a)
    {
        if (a)
        {
            A();
        }

        {
            B();
        }
    }
}
```

Neither RH5030 nor RH5025 reports it. `FormatNodeInDocumentAsync` with the block statement as target gives the same result (FMT-PIPE-024); code fixes that format a block statement through it, and the expectations of `ReihitsuFormatterNodeBlankLineTests` for a block statement behind `}`, change with it.

### 2. Behind a directive, the blank line goes after the directive

```csharp
class C
{
    void M(bool a)
    {
        if (a)
        {
        }
#if DEBUG
        {
            B();
        }
#endif
    }
}
```

Before: unchanged input; RH5030 reports the closing brace.

After:

```csharp
class C
{
    void M(bool a)
    {
        if (a)
        {
        }
#if DEBUG

        {
            B();
        }
#endif
    }
}
```

### 3. A comment glued to the block's brace after `}` is separated by the SHARED-BLANK-003 blank line

```csharp
class C
{
    void M(bool a)
    {
        if (a)
        {
        }
        /* c */ {
            B();
        }
    }
}
```

Before: unchanged input; RH5030 reports the closing brace and RH5020 reports the comment.

After:

```csharp
class C
{
    void M(bool a)
    {
        if (a)
        {
        }

        /* c */ {
            B();
        }
    }
}
```

### 4. The author's blank line above a block statement after any other statement is kept

```csharp
class C
{
    void M()
    {
        A();

        {
            B();
        }
    }
}
```

Before:

```csharp
class C
{
    void M()
    {
        A();
        {
            B();
        }
    }
}
```

After: the input is kept unchanged. Without the blank line in the input, none is inserted, before and after. RH5025 no longer reports the blank line above the brace.

The same holds behind a directive: in

```csharp
class C
{
    void M()
    {
        A();
#pragma warning disable CS0168

        {
            B();
        }
    }
}
```

the blank line below the directive is removed today and kept after the change. So is the author's blank line above a comment glued to the block statement's brace:

```csharp
class C
{
    void M()
    {
        A();

        /* c */ {
            B();
        }
    }
}
```

Before:

```csharp
class C
{
    void M()
    {
        A();
        /* c */ {
            B();
        }
    }
}
```

After: the input is kept unchanged.

### 5. A block statement after `break` gets the FMT-BLANK-004 blank line

This entry depends on output question 1 of the run (see `Decisions`).

```csharp
class C
{
    void M(int a)
    {
        while (a > 0)
        {
            break;
            {
                a++;
            }
        }
    }
}
```

Before: unchanged input; RH5011 reports the `break`.

After:

```csharp
class C
{
    void M(int a)
    {
        while (a > 0)
        {
            break;

            {
                a++;
            }
        }
    }
}
```

### 6. RH5025 does not report the blank line above a block statement that follows a statement

```csharp
class C
{
    void M()
    {
        A();

        {
            B();
        }
    }
}
```

Before: RH5025 reports the blank line above `{`. After: not reported. The blank line above the `{` of a block statement that is the first statement of its block or switch section is still reported (see `Decisions`).

### Unchanged

- A comment glued to `{` keeps today's output, with no blank line above it; RH5020 keeps reporting it:

  ```csharp
  class C
  {
      void M()
      {
          var p = new P
                  {
                      Child =
                      /* c */ {
                                  X = 2
                              }
                  };
      }
  }
  ```

- The rule page of RH5025 is unchanged: it describes the blank line between a declaration and its opening brace, which the exemption does not touch.

## Decisions

### The opening brace of a block statement is exempt from FMT-BLANK-008 and RH5025

Decided by the maintainer (revision of thoenissen/Reihitsu#1496, 2026-10-28): FMT-BLANK-008 and RH5025 do not apply to the `{` of a block statement, not only after `}`. After a statement that ends in `}` the SHARED-BLANK-003 blank line is required; after any other statement the author's blank line is kept and not reported. This supersedes two earlier decisions:

- thoenissen/Reihitsu#1396 — RH5030 exempting a closing brace followed by a block statement. RH5030 keeps reporting it; the formatter now inserts the blank line RH5030 asks for.
- thoenissen/Reihitsu#1081, item 1, for the standalone block — RH5025 reporting the blank line above a brace that starts a standalone block. The other braces of that item (an initializer element, a switch arm, a braced switch section) stay reported.

### RH5020 keeps reporting a comment glued to `{`

Decided by the maintainer: RH5020 gets no exemption. A comment that starts its line glued to a brace is not wanted; it is fixed by hand, and the formatter leaves it without a blank line above, as today.

### "Follows a statement" bounds the exemption

The exemption covers a block statement that follows another statement of the same block or switch section — the two cases the maintainer named, after `}` and after any other statement. A block statement that is the first statement of its block or switch section, and a block that is not in a statement list (a top-level block of a file with top-level statements, the block of a labeled statement), stay under FMT-BLANK-008 and RH5025. Pending output question 2 of the run.

### FMT-BLANK-004 applies above a block statement after `break`

The maintainer's "after any other statement, the author's blank line is kept" is read as "no rule of the brace removes or demands it": FMT-BLANK-004's blank line after `break` asks for one above a block statement like above any other statement, as RH5011 does. Pending output question 1 of the run.

### FMT-BLANK-018 keeps today's output for glued comments instead of following the issue's wording

The issue exempts a comment that shares its line with "the `{` behind it". Today's output has no blank line above a comment glued to the braces of most constructs, to the closing brace of each of them, to the closing bracket of a collection expression or list pattern, and to the closing parenthesis of a parenthesized pattern; it has one above a comment glued to an array initializer's braces and to a property pattern's opening brace. FMT-BLANK-018 names exactly the tokens where today's output has none, so no glued comment changes its output. The issue's wording would add a blank line above a comment glued to a closing brace, bracket, or parenthesis, and remove one above a comment glued to an array initializer's brace or a property pattern's opening brace.

### FMT-BLANK-019 is a clarification

The removal of a blank line before the closing bracket of a collection expression or list pattern and before the closing parenthesis of a parenthesized pattern is today's output; only its owner changes. It is written down as a blank-line requirement because the line-break phase no longer removes blank lines.

### The RH5025 rule page stays as it is

The page describes the blank line between a declaration and its opening brace. The block-statement exemption is an exception and does not change what the page tells a user in the normal case.

## Alternatives considered

- **FMT-BLANK-008 wins for a block statement, and RH5030 gains an exemption** (thoenissen/Reihitsu#1396) — rejected by the maintainer: a block statement's brace starts a statement and continues no header.
- **FMT-BLANK-011 wins for a comment glued to a brace** — rejected: the outputs the issue counts (eight) would gain a blank line above a comment that RH5020 reports anyway and that the maintainer wants fixed by hand.
- **One precedence ranking for all insertion and removal rules** — rejected: after narrowing FMT-BLANK-008 and FMT-BLANK-011, no blank line is decided by two rules, so a ranking would decide nothing and invite overlaps.
- **Leave the blank line before a closing bracket to the author once the line-break phase stops removing it** — rejected: it would change today's output without a request to change it.
