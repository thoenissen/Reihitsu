# The blank-line phase is the only owner of blank lines

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | FMT-BLANK-008 (changed), FMT-BLANK-009 (changed), FMT-BLANK-011 (changed), FMT-BLANK-017 (added), FMT-BLANK-018 (added), FMT-BLANK-019 (added, clarification), FMT-BLANK-020 (added, clarification), FMT-BREAK-030 (added), FMT-PIPE-024 (changed), RH5025-005 (added) |

## What

The blank-line phase decides every blank line of the formatter's output; the line-break phase only adds and removes line breaks and keeps, in every gap it does not join, the blank lines it finds.

- **The opening brace of a block statement is not a header continuation.** FMT-BLANK-008 no longer removes the blank line above the `{` of a block statement that follows another statement of the same block or switch section (FMT-BLANK-017). The blank lines above such a block are decided like those above any other statement: SHARED-BLANK-003 asks for one after a statement that ends in `}` (placed behind a directive by FMT-BLANK-005), FMT-BLANK-004 asks for one after a `break` statement, and after any other statement the author's blank line is kept and none is inserted. FMT-BLANK-009, which only refines FMT-BLANK-008 around directives, no longer applies to that brace either.
- **RH5025 gets the matching exemption** (RH5025-005): it does not report the blank line above the `{` of such a block statement.
- **A comment glued to a brace gets no FMT-BLANK-011 blank line** (FMT-BLANK-018). A comment that starts its line and shares it with the brace, bracket, or parenthesis behind it — the braces of every construct except the braces of an array initializer, an extension block, and an interpolation and the opening braces of a property pattern and a complex element initializer, the closing bracket of a collection expression or list pattern, and the closing parenthesis of a parenthesized pattern — gets no blank line above it from FMT-BLANK-011, and a blank line the author wrote above it is removed. This keeps today's output for such a comment: today the blank-line phase inserts or keeps the line and the line-break phase removes it again. When a directive stands in the gap, FMT-BLANK-018 removes nothing: below a region directive SHARED-BLANK-006 asks for the blank line (FMT-BLANK-013), and the other blank lines are the author's. A comment glued to the opening brace of a block statement that follows a statement is the exception: FMT-BLANK-017 decides the blank lines above it. The blank lines of SHARED-BLANK-003 and FMT-BLANK-004 are not affected.
- **The blank line before a closing bracket moves to the blank-line phase** (FMT-BLANK-019). Today only the line-break phase removes a blank line before the closing `]` of a collection expression or a list pattern and before the closing `)` of a parenthesized pattern. FMT-BLANK-019 writes that output down as a blank-line rule, so it stays when the line-break phase stops removing blank lines. Behind a non-region directive, FMT-BLANK-009 now covers these tokens too, as it does braces; below a region directive the blank line stays, as SHARED-BLANK-006 requires.
- **The blank line behind a comment before a brace moves to the blank-line phase** (FMT-BLANK-020). Today only the line-break phase removes the blank lines between an own-line comment and the brace or bracket behind it (`// c⏎⏎}`); the blank-line phase reads FMT-BLANK-007 and FMT-BLANK-008 as "directly before the token" and keeps them. FMT-BLANK-020 writes that output down for the tokens FMT-BLANK-018 names — the same tokens, for the same reason: the line-break phase normalizes exactly these. The opening brace of a block statement that follows a statement is the exception: FMT-BLANK-017 decides the blank lines above it.
- **The line-break phase keeps blank lines** (FMT-BREAK-030). FMT-BREAK's non-goal "Where this phase moves a token onto a new line it inserts no blank line" becomes a requirement: in every gap the phase does not join, it leaves the number of blank lines as it found it.
- **Node-level formatting gives a block statement a list position like any other statement** (FMT-PIPE-024). The exception that FMT-PIPE-024 recorded until this change — a block statement got a list position only behind an own-line comment, because the line-break phase decided the gap above its brace — is removed.

RH5020 is unchanged: it keeps reporting a comment that starts its line glued to a `{` without a blank line above it, which the formatter now leaves unseparated by decision. RH5030 is unchanged: it keeps reporting a closing brace followed by a block statement without a blank line, which the formatter no longer produces.

### Precedence

The blank-line requirements that insert a blank line (SHARED-BLANK-003, FMT-BLANK-001 to FMT-BLANK-005, FMT-BLANK-011, FMT-BLANK-012, and FMT-BLANK-013 for SHARED-BLANK-005 and SHARED-BLANK-006) and the ones that remove one (FMT-BLANK-007, FMT-BLANK-008, FMT-BLANK-009, FMT-BLANK-018, FMT-BLANK-019, FMT-BLANK-020) are scoped so that no blank line is decided by two of them. Where an insertion and a removal met, the scope of one of them is narrowed instead of ranking one over the other:

- FMT-BLANK-008 and FMT-BLANK-009 do not reach the opening brace of a block statement that follows a statement, and FMT-BLANK-020 does not reach the comments above it, so SHARED-BLANK-003 and FMT-BLANK-004 decide the line above it (FMT-BLANK-017).
- FMT-BLANK-011 does not reach a comment glued to a token FMT-BLANK-018 names, so no rule inserts a blank line there.
- FMT-BLANK-018 and FMT-BLANK-019 do not reach the blank line directly below a region directive. SHARED-BLANK-006 exempts only an `#endregion` whose next line starts with `}`; a line that starts with a glued comment, a `]`, or a `)` is not exempt, so FMT-BLANK-013 inserts the blank line there and nothing removes it.
- FMT-BLANK-018 removes no blank line of a gap that holds any other directive either: FMT-BLANK-011 inserts none directly behind a directive, and the blank lines the author wrote there are kept.

FMT-BLANK-013's own removal of the blank lines between an `#endregion` and a following `}` is SHARED-BLANK-006's exemption, so it meets no insertion. Elsewhere a removal rule removes what no insertion rule asks for.

## Why

Two phases decide the blank line next to a brace, with contradicting rules, and the later one wins:

- After `}`, the blank-line phase inserts the blank line SHARED-BLANK-003 requires above a block statement, and the line-break phase removes it while it places the block's `{`. The formatted output has no blank line, so RH5030 reports it; if the blank line stayed, RH5025 would report it. The formatter's output can satisfy both rules only when one of them yields.
- Above a comment glued to `{`, the blank-line phase inserts the FMT-BLANK-011 blank line and the line-break phase removes it again, in every pass. The output is stable only because the line-break phase runs later.

A block statement's `{` starts a statement; it continues no header, which is what FMT-BLANK-008 and RH5025 protect. Letting the statement rules decide the blank line above it makes the formatter's output satisfy RH5030 and RH5025 at once, and with one owner per blank line no phase undoes another, the line-break phase no longer needs guards against the blank-line phase, and node-level formatting no longer predicts the line-break phase's removal.

## Visible output changes

Entries that depend on an output question still open in the run name it. The run numbers them: 1 — a block statement after `break`; 2 — which block statements are exempt; 3 — an own-line comment and a blank line above a block statement; 4 — an own-line comment in front of a comment glued to a brace; 5 — a directive in front of a comment glued to a brace. Every entry shows the output of the option the spec is written on.

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

This entry depends on output question 1 (see `Decisions`).

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

### 7. An own-line comment and a blank line above a block statement keep the author's blank line

This entry depends on output question 3, and its `break` case also on output question 1.

```csharp
class C
{
    void M()
    {
        A();
        // c

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

        // c
        {
            B();
        }
    }
}
```

After:

```csharp
class C
{
    void M()
    {
        A();

        // c

        {
            B();
        }
    }
}
```

The same holds after a statement that ends in `}` and after `break`: the blank line above the comment is inserted as before (SHARED-BLANK-003 or FMT-BLANK-004, and FMT-BLANK-011), and the author's blank line between the comment and `{` is kept instead of removed. Without a blank line between the comment and `{` in the input, none is inserted, before and after.

```csharp
class C
{
    void M(bool a)
    {
        if (a)
        {
        }
        // c

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
        }

        // c
        {
            B();
        }
    }
}
```

After:

```csharp
class C
{
    void M(bool a)
    {
        if (a)
        {
        }

        // c

        {
            B();
        }
    }
}
```

### 8. After `break`, a comment glued to the block's brace is separated by the FMT-BLANK-004 blank line

This entry depends on output question 1.

```csharp
class C
{
    void M(int a)
    {
        while (a > 0)
        {
            break;
            /* c */ {
                a++;
            }
        }
    }
}
```

Before: unchanged input; RH5011 reports the `break` and RH5020 reports the comment.

After:

```csharp
class C
{
    void M(int a)
    {
        while (a > 0)
        {
            break;

            /* c */ {
                a++;
            }
        }
    }
}
```

### 9. An own-line comment in front of a glued comment gets the FMT-BLANK-011 blank line

This entry depends on output question 4.

```csharp
class C
{
    void M()
    {
        var q = new P
                {
                    X = 1
                    // note
                /* d */ };
    }
}
```

Before: unchanged input; RH5020 reports `// note`.

After:

```csharp
class C
{
    void M()
    {
        var q = new P
                {
                    X = 1

                    // note
                /* d */ };
    }
}
```

With blank lines on both sides of `// note` in the input, today's output keeps only the one below it, and the output after the change keeps only the one above it:

```csharp
class C
{
    void M()
    {
        var q = new P
                {
                    X = 1

                    // note

                /* d */ };
    }
}
```

Before:

```csharp
class C
{
    void M()
    {
        var q = new P
                {
                    X = 1
                    // note

                /* d */ };
    }
}
```

After:

```csharp
class C
{
    void M()
    {
        var q = new P
                {
                    X = 1

                    // note
                /* d */ };
    }
}
```

The same holds in front of a comment glued to an opening brace:

```csharp
class C
{
    void M()
    {
        var q = new P
                {
                    Child =
                    // note

                    /* c */ {
                                X = 2
                            }
                };
    }
}
```

Before: unchanged input; RH5020 reports `// note`.

After:

```csharp
class C
{
    void M()
    {
        var q = new P
                {
                    Child =

                    // note
                    /* c */ {
                                X = 2
                            }
                };
    }
}
```

### 10. No blank line is inserted in front of a directive that precedes a glued comment

This entry depends on output question 5.

```csharp
class C
{
    void M(int a)
    {
        a++;
#pragma warning disable CS0168

    /* d */ }
}
```

Before:

```csharp
class C
{
    void M(int a)
    {
        a++;

#pragma warning disable CS0168

    /* d */ }
}
```

After: the input is kept unchanged.

Today the line-break phase inserts the blank line in front of the first directive of the gap whenever a blank line stands between a directive and a comment glued to the token behind it; it does so for `#if`/`#endif` and in initializers as well. No analyzer reports either output. A blank line the author wrote in front of the directive is kept, before and after.

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

- The blank lines between an own-line comment and the brace, bracket, or parenthesis behind it are removed, before and after (FMT-BLANK-020), for every token FMT-BLANK-018 names except the opening brace of a block statement that follows a statement (entry 7):

  ```csharp
  class C
  {
      void M(int a)
      {
          a++;

          // c
      }
  }
  ```

  keeps its output when the input has a blank line between `// c` and `}`.

- A comment glued to the opening brace of a complex element initializer, to a brace of an extension block, or to the closing brace of an interpolation keeps its FMT-BLANK-011 blank line, as it does above an array initializer's brace and a property pattern's opening brace:

  ```csharp
  class C
  {
      Dictionary<int, int> D = new()
                               {
                                   { 1, 2 },

                                   /* c */ { 3, 4 }
                               };
  }
  ```

- Below a region directive, the blank line in front of a glued comment, a `]`, or a `)` stays and a missing one is inserted (SHARED-BLANK-006), and behind a non-region directive the blank line in front of a `]` or `)` is removed, as in front of a brace (FMT-BLANK-009):

  ```csharp
  class C
  {
      void M(int a)
      {
          #region R

          a++;

          #endregion // R

      /* d */ }
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

The exemption covers a block statement that follows another statement of the same block or switch section — the two cases the maintainer named, after `}` and after any other statement. A block statement that is the first statement of its block or switch section, and a block that is not in a statement list (a top-level block of a file with top-level statements, the block of a labeled statement), stay under FMT-BLANK-008 and RH5025. Pending output question 2.

### FMT-BLANK-004 applies above a block statement after `break`

The maintainer's "after any other statement, the author's blank line is kept" is read as "no rule of the brace removes or demands it": FMT-BLANK-004's blank line after `break` asks for one above a block statement like above any other statement, as RH5011 does. Pending output question 1.

### FMT-BLANK-018 keeps today's output for glued comments instead of following the issue's wording

The issue exempts a comment that shares its line with "the `{` behind it". Today's output has no blank line above a comment glued to the braces of most constructs, to the closing brace of each of them, to the closing bracket of a collection expression or list pattern, and to the closing parenthesis of a parenthesized pattern; it has one above a comment glued to the braces of an array initializer, an extension block, or an interpolation and to the opening brace of a property pattern or a complex element initializer — the tokens the line-break phase does not normalize. FMT-BLANK-018 names exactly the tokens where today's output has none, so no glued comment without an own-line comment or a directive in front of it changes its output. The issue's wording would add a blank line above a comment glued to a closing brace, bracket, or parenthesis, and remove one above a comment glued to the excluded braces.

### A directive in front of a glued comment keeps its blank lines

FMT-BLANK-018 removes no blank line of a gap that holds a directive. Below a region directive, SHARED-BLANK-006 requires the blank line, since the next line starts with the comment, not with `}`; removing it would make RH5032 report the formatter's output, and FMT-BLANK-013 would insert what FMT-BLANK-018 removes. Below any other directive, today's output keeps the author's blank line. What changes is only the blank line today's line-break phase inserts in front of the first directive of such a gap (visible output change 10): no blank-line rule asks for it, and FMT-BLANK-016's reason — a blank line in front of a directive lands inside the region of the directive above or separates the directive from code grouped with it — speaks against writing one. Pending output question 5.

### FMT-BLANK-020 is a clarification with FMT-BLANK-018's tokens

The removal of the blank lines between an own-line comment and the brace, bracket, or parenthesis behind it is today's output, and only the line-break phase produces it; the blank-line phase reads "directly before" in FMT-BLANK-007 and FMT-BLANK-008 as "with nothing between", so a comment ends that gap. Today's output keeps those blank lines before the braces of an array initializer, an extension block, or an interpolation, the opening brace of a property pattern or a complex element initializer, and the chained keywords — exactly the tokens the line-break phase does not normalize, so FMT-BLANK-020 uses FMT-BLANK-018's set and no output changes. It is a requirement of its own rather than a sentence in FMT-BLANK-007, FMT-BLANK-008, and FMT-BLANK-019, because its token set is FMT-BLANK-018's and not theirs: those three cover every brace directly in front of the token. Behind a non-region directive before a closing bracket or parenthesis, FMT-BLANK-009 is extended for the same reason: today the line-break phase removes the blank line between the directive and the token, as FMT-BLANK-009 does before a brace.

### The blank lines below an own-line comment above a block statement follow the statement rules

FMT-BLANK-017 excludes the opening brace of a block statement that follows a statement from FMT-BLANK-020 as from FMT-BLANK-008: after a statement, the author's blank line between a comment and the next statement is kept (`A();⏎// c⏎⏎B();` keeps it today), and a block statement is decided like any other statement (visible output change 7). Pending output question 3.

### An own-line comment in front of a glued comment is separated like any other

`// note` in front of `/* d */ };` does not share its line with a brace, so FMT-BLANK-011 asks for a blank line above it, and FMT-BLANK-018 removes the blank line between it and the glued comment (visible output change 9). Today the line-break phase removes the line above `// note` and keeps the one below it, because it normalizes the run in front of the gap's first content as if the gap were empty. The maintainer's RH5020 decision covers a comment glued to a brace, not a comment on its own line. Pending output question 4.

### FMT-BLANK-019 is a clarification

The removal of a blank line before the closing bracket of a collection expression or list pattern and before the closing parenthesis of a parenthesized pattern is today's output; only its owner changes. It is written down as a blank-line requirement because the line-break phase no longer removes blank lines.

### The RH5025 rule page stays as it is

The page describes the blank line between a declaration and its opening brace. The block-statement exemption is an exception and does not change what the page tells a user in the normal case.

## Alternatives considered

- **FMT-BLANK-008 wins for a block statement, and RH5030 gains an exemption** (thoenissen/Reihitsu#1396) — rejected by the maintainer: a block statement's brace starts a statement and continues no header.
- **FMT-BLANK-011 wins for a comment glued to a brace** — rejected: the outputs the issue counts (eight) would gain a blank line above a comment that RH5020 reports anyway and that the maintainer wants fixed by hand.
- **One precedence ranking for all insertion and removal rules** — rejected: after narrowing FMT-BLANK-008 and FMT-BLANK-011, no blank line is decided by two rules, so a ranking would decide nothing and invite overlaps.
- **Leave the blank line before a closing bracket to the author once the line-break phase stops removing it** — rejected: it would change today's output without a request to change it.
- **FMT-BLANK-020 for every brace and chained keyword** — not part of this change: it would remove blank lines today's output keeps behind a comment, before the braces FMT-BLANK-018 excludes and before `else`, `catch`, `finally`, and `while`, which no request asks for. RH5024, RH5025, and RH5026 report several of those outputs today; that is a separate question.
- **State the comment rule inside FMT-BLANK-007, FMT-BLANK-008, and FMT-BLANK-019** — rejected: their scope is every brace or bracket directly in front of the token, the comment rule's is FMT-BLANK-018's, so each would have to repeat the exceptions.
- **Keep the blank line the line-break phase inserts in front of a directive** — rejected on the spec's recommendation, pending output question 5: it needs a rule that inserts a blank line in front of a directive, which FMT-BLANK-016 forbids for the blank lines FMT-BLANK-001 to FMT-BLANK-005 insert.
