# The blank-line phase is the only owner of blank lines

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-BLANK-008 (changed), FMT-BLANK-009 (changed), FMT-BLANK-011 (changed), FMT-BLANK-017 (added), FMT-BLANK-018 (added), FMT-BLANK-019 (added, clarification), FMT-BLANK-020 (added), FMT-BREAK-030 (added), FMT-PIPE-024 (changed), FMT-SWITCH-007 (changed), RH5025-005 (added) |

## What

The blank-line phase decides every blank line of the formatter's output; the line-break phase only adds and removes line breaks and keeps, in every gap it does not join, the blank lines it finds.

- **The opening brace of a block statement is not a header continuation.** FMT-BLANK-008 no longer removes the blank line above the `{` of a block statement that follows another statement of the same block or switch section (FMT-BLANK-017). The blank lines above such a block are decided like those above any other statement: SHARED-BLANK-003 asks for one after a statement that ends in `}` (placed behind a directive by FMT-BLANK-005), FMT-BLANK-004 asks for one after a `break` statement, and after any other statement the author's blank line is kept and none is inserted. FMT-BLANK-009, which only refines FMT-BLANK-008 around directives, no longer applies to that brace either.
- **RH5025 gets the matching exemption** (RH5025-005): it does not report the blank line above the `{` of such a block statement.
- **A comment glued to a token belongs to the token's line** (FMT-BLANK-018). FMT-BLANK-018 states one classification of a comment's position relative to the token behind it, and FMT-BLANK-011 and FMT-BLANK-020 read it: every comment of the gap is either on the token's line — no line break outside the text of a comment stands between it and the token, so a delimited comment that starts on an earlier line and ends on the token's line is on it too — or on its own line. The comments on the token's line are glued to it when the first of them is an ordinary comment; they get no blank line from FMT-BLANK-011, whatever the token is. The blank lines above it are decided as the blank lines directly before the token: removed before every brace, a chained keyword, the closing bracket of a collection expression or list pattern, and the closing parenthesis of a parenthesized pattern (FMT-BLANK-007, FMT-BLANK-008, FMT-BLANK-019); decided by the statement rules above a statement, and by FMT-BLANK-017 above the opening brace of a block statement that follows a statement; kept as the author wrote them elsewhere, such as above a member or the closing parenthesis of an argument list. The rule names no construct and reads no layout: today's exceptions — the braces of an array initializer, an extension block, or an interpolation, the opening brace of a property pattern or of a complex element initializer on one line, the closing brace of a collection initializer assigned to a member, a documentation comment between the comment and the token, and every token that is not a brace or closing bracket — kept a blank line above such a comment only to keep its earlier output, and they go (visible output changes 12 and 14 to 17). The braced switch section follows the same rule: a comment glued to the terminal `break` behind the new block gets no blank line (FMT-SWITCH-007, visible output change 19). A delimited comment that spans lines and ends on the token's line is glued like a comment that starts there (visible output change 20); only in front of a chain link that the line-break phase moves onto a line of its own behind such a comment, and in front of the end of the file, is it a comment on its own line. When a directive stands in the gap, FMT-BLANK-018 removes nothing: below a region directive SHARED-BLANK-006 asks for the blank line (FMT-BLANK-013), and the other blank lines are the author's.
- **The blank line before a closing bracket moves to the blank-line phase** (FMT-BLANK-019). Today only the line-break phase removes a blank line before the closing `]` of a collection expression or a list pattern and before the closing `)` of a parenthesized pattern. FMT-BLANK-019 writes that output down as a blank-line rule, so it stays when the line-break phase stops removing blank lines. Behind a non-region directive, FMT-BLANK-009 now covers these tokens too, as it does braces; below a region directive the blank line stays, as SHARED-BLANK-006 requires.
- **No blank line is inserted in front of a directive that a region directive follows in the gap before a closing token** (FMT-BLANK-009). Today the line-break phase inserts a blank line in front of the first directive of a gap before a closing brace, bracket, or parenthesis when a non-region directive or disabled text starts the gap, a region directive ends it, and a blank line stands between the two. No blank-line rule asks for it; FMT-BLANK-009 now states that it is not inserted.
- **The blank line behind a comment before a brace moves to the blank-line phase** (FMT-BLANK-020). Today only the line-break phase removes the blank lines between a comment that ends its line and the brace or bracket behind it (`// c⏎⏎}`, also `/* a */ /* b */⏎⏎}`); the blank-line phase reads FMT-BLANK-007 and FMT-BLANK-008 as "directly before the token" and keeps them. FMT-BLANK-020 writes that output down for the tokens the line-break phase normalized, with two changes that a decision of the blank-line phase cannot avoid: the opening brace of every complex element initializer is named, not only of one that spans several lines, and the opening brace of a collection initializer assigned to a member is named (visible output changes 13 and 18). The opening brace of a block statement that follows a statement is the exception: FMT-BLANK-017 decides the blank lines above it.
- **The line-break phase keeps blank lines** (FMT-BREAK-030). FMT-BREAK's non-goal "Where this phase moves a token onto a new line it inserts no blank line" becomes a requirement: in every gap the phase does not join, it leaves the number of blank lines as it found it. The blank line FMT-BLANK-011 inserts above a comment behind the `=` of an assigned collection initializer, which the line-break phase removed until this change, therefore stays (visible output change 13), and so does the blank line the author wrote above a directive there, which FMT-BLANK-009 keeps (visible output change 22).
- **Node-level formatting gives a block statement a list position like any other statement** (FMT-PIPE-024). The exception that FMT-PIPE-024 recorded until this change — a block statement got a list position only behind an own-line comment, because the line-break phase decided the gap above its brace — is removed.

RH5020 is unchanged: it keeps reporting a comment that starts its line without a blank line above it. That includes every comment glued to a token that the formatter now leaves unseparated by decision — glued to a brace as before, and glued to a bracket, a parenthesis, a chained keyword, a statement, a member, or any other token (visible output changes 12 and 14 to 17, 19, and 20). RH5030 is unchanged: it keeps reporting a closing brace followed by a block statement without a blank line, which the formatter no longer produces.

### Precedence

The blank-line requirements that insert a blank line (SHARED-BLANK-003, FMT-BLANK-001 to FMT-BLANK-005, FMT-BLANK-011, FMT-BLANK-012, and FMT-BLANK-013 for SHARED-BLANK-005 and SHARED-BLANK-006) and the ones that remove one (FMT-BLANK-007, FMT-BLANK-008, FMT-BLANK-009, FMT-BLANK-018, FMT-BLANK-019, FMT-BLANK-020) are scoped so that no blank line is decided by two of them. Where an insertion and a removal met, the scope of one of them is narrowed instead of ranking one over the other:

- FMT-BLANK-008 and FMT-BLANK-009 do not reach the opening brace of a block statement that follows a statement, and FMT-BLANK-020 does not reach the comments above it, so SHARED-BLANK-003 and FMT-BLANK-004 decide the line above it (FMT-BLANK-017).
- FMT-BLANK-011 does not reach a comment glued to a token, so the blank lines above it are decided only as those directly before the token are (FMT-BLANK-018): by a removal rule, by a statement rule, or by FMT-BLANK-017.
- FMT-BLANK-018 and FMT-BLANK-019 do not reach the blank line directly below a region directive. SHARED-BLANK-006 exempts only an `#endregion` whose next line starts with `}`; a line that starts with a glued comment, a `]`, or a `)` is not exempt, so FMT-BLANK-013 inserts the blank line there and nothing removes it.
- FMT-BLANK-018 removes no blank line of a gap that holds any other directive either: FMT-BLANK-011 inserts none directly behind a directive, and the blank lines the author wrote there are kept.

FMT-BLANK-013's own removal of the blank lines between an `#endregion` and a following `}` is SHARED-BLANK-006's exemption, so it meets no insertion. Elsewhere a removal rule removes what no insertion rule asks for.

## Why

Two phases decide the blank line next to a brace, with contradicting rules, and the later one wins:

- After `}`, the blank-line phase inserts the blank line SHARED-BLANK-003 requires above a block statement, and the line-break phase removes it while it places the block's `{`. The formatted output has no blank line, so RH5030 reports it; if the blank line stayed, RH5025 would report it. The formatter's output can satisfy both rules only when one of them yields.
- Above a comment glued to `{`, the blank-line phase inserts the FMT-BLANK-011 blank line and the line-break phase removes it again, in every pass. The output is stable only because the line-break phase runs later, and for a complex element initializer whose elements the line-break phase expands, the two phases disagree and a second pass changes the output.

A block statement's `{` starts a statement; it continues no header, which is what FMT-BLANK-008 and RH5025 protect. Letting the statement rules decide the blank line above it makes the formatter's output satisfy RH5030 and RH5025 at once, and with one owner per blank line no phase undoes another, the line-break phase no longer needs guards against the blank-line phase, and node-level formatting no longer predicts the line-break phase's removal.

## Visible output changes

Entries that the maintainer decided through an output question of the run name it (all answered with option A on 2026-11-01; see `Decisions`). The run numbers them: 1 — a block statement after `break`; 2 — which block statements are exempt; 3 — an own-line comment and a blank line above a block statement; 4 — an own-line comment in front of a comment glued to a brace; 5 — a directive in front of a comment glued to a brace; and, asked during the implementer stage, a directive in front of a region directive that ends the gap before a closing brace (entry 11), whose collection-expression `]` and parenthesized-pattern `)` case was asked before the audit; and, after the audit, 1A — a comment glued to a complex element initializer's `{` (entry 12) — and 2A — an own-line comment and a blank line above an assigned collection initializer's `{` (entry 13). Entries 14 to 17, 19, and 20 change the output for comments that share a line with code, and entry 13 also the blank line above a comment between an assignment's `=` and the assigned value; the maintainer's guidance of 2026-11-01 covers them (see `Decisions`). 1A also decides entry 18, the same rule for a comment that does not share a line with code. Entries 21 and 22 are outputs that unchanged requirements demand and that today's line-break phase overrode. Every entry shows the chosen output.

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
#if !NEVER
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
#if !NEVER

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

Decided with output question 1 (see `Decisions`).

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

Decided with output question 3, and its `break` case also with output question 1.

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

Decided with output question 1.

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

Decided with output question 4.

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

Decided with output question 5.

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

### 11. No blank line is inserted in front of a directive that a region directive follows before a closing brace, bracket, or parenthesis

Decided with the output question asked during the implementer stage for a closing brace, and with the output question asked before the audit for the closing `]` of a collection expression and the closing `)` of a parenthesized pattern.

```csharp
class C
{
    #region Fields

#pragma warning disable CS0169
    int x;
#pragma warning restore CS0169

    #endregion // Fields
}

class D
{
    void M(int a)
    {
        #region R

        a++;
#pragma warning disable CS0168
        #endregion // R
    }
}
```

Before:

```csharp
class C
{
    #region Fields

#pragma warning disable CS0169
    int x;

#pragma warning restore CS0169

    #endregion // Fields
}

class D
{
    void M(int a)
    {
        #region R

        a++;

#pragma warning disable CS0168

        #endregion // R
    }
}
```

After:

```csharp
class C
{
    #region Fields

#pragma warning disable CS0169
    int x;
#pragma warning restore CS0169

    #endregion // Fields
}

class D
{
    void M(int a)
    {
        #region R

        a++;
#pragma warning disable CS0168

        #endregion // R
    }
}
```

The blank line between the directive and `#endregion` is the region's (SHARED-BLANK-005): it is kept where the author wrote it and inserted where not, before and after. Today the line-break phase additionally inserts a blank line in front of the first directive of the gap, the same mechanism as entry 10. The same holds for `#if … #endif` in front of the region directive. It holds as decided before the closing `]` of a collection expression and the closing `)` of a parenthesized pattern whose last element is followed by such directives: today a blank line is inserted in front of the first directive, and after the change none is. The example shows braces only, because the column of a region directive inside a collection expression or pattern is not decided by any requirement. A blank line the author wrote in front of the first directive is kept, before and after. No analyzer reports either output.

### 12. A comment glued to the opening brace of a complex element initializer gets no blank line, whatever the element's layout

Decided with output question 1A after the audit.

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

Before:

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

After: the input is kept unchanged, and a blank line the author wrote above `/* c */` is removed. Today the blank line above such a comment depends on whether the element stays on one line; an element whose contents the line-break phase expands onto several lines (a lambda with a block body, an object or collection initializer, an anonymous object) decides it differently in two passes. After the change, no blank line stands above the comment in either pass.

### 13. Behind an assigned collection initializer's `=`, a comment gets the FMT-BLANK-011 blank line, and the blank line between the comment and `{` is removed

The removal of the blank line between the comment and `{` is decided with output question 2A after the audit. The blank line above the comment is covered by the maintainer's guidance: the comment stands between the `=` of an assignment and the assigned value.

```csharp
class C
{
    void M()
    {
        var p = new P
                {
                    L =
                    // c

                    {
                        1
                    }
                };
    }
}
```

Before: the input is kept unchanged.

After:

```csharp
class C
{
    void M()
    {
        var p = new P
                {
                    L =

                    // c
                    {
                        1
                    }
                };
    }
}
```

Without the blank line between `// c` and `{` in the input (`L =⏎// c⏎{`), the output before is the input unchanged, and the output after is the one above. Today the blank-line phase inserts the FMT-BLANK-011 blank line above `// c` and the line-break phase removes it again in every pass; once the line-break phase keeps blank lines (FMT-BREAK-030), FMT-BLANK-011 decides it, with no exemption for a comment behind `=`. Above the opening brace of an assigned object initializer (`I =⏎⏎// c⏎{`) and behind every other `=` — of a field, a local, an assignment statement, or another member of an object initializer — the comment has the blank line before and after.

### 14. A comment glued to the closing brace of an assigned collection initializer gets no blank line

Covered by the maintainer's guidance.

```csharp
class C
{
    void M()
    {
        var p = new P
                {
                    L = {
                            1
                        /* l */ }
                };
    }
}
```

Before:

```csharp
class C
{
    void M()
    {
        var p = new P
                {
                    L = {
                            1

                        /* l */ }
                };
    }
}
```

After: the input is kept unchanged, and a blank line the author wrote above `/* l */` is removed, as at the closing brace of any other collection initializer.

### 15. A documentation comment between a glued comment and the brace no longer separates the comment

Covered by the maintainer's guidance.

```csharp
public class TestClass
/* x */ /** Doc */ {
}
```

Before:

```csharp
public class TestClass

/* x */ /** Doc */ {
}
```

After: the input is kept unchanged, and a blank line the author wrote above `/* x */` is removed. A documentation comment that starts the line (`/** Doc */ {`) is not glued and keeps its FMT-BLANK-011 blank line, before and after.

### 16. A comment glued to the brace of an array initializer, an extension block, an interpolation, or a property pattern, or to a chained keyword, gets no blank line

Covered by the maintainer's guidance.

```csharp
class C
{
    int[] G =
    /* b */ { 1, 2 };

    void M(int a)
    {
        if (a > 0)
        {
            a++;
        }
        /* k */ else
        {
            a--;
        }
    }
}
```

Before:

```csharp
class C
{
    int[] G =

    /* b */ { 1, 2 };

    void M(int a)
    {
        if (a > 0)
        {
            a++;
        }

        /* k */ else
        {
            a--;
        }
    }
}
```

After: the input is kept unchanged, and a blank line the author wrote above `/* b */` or `/* k */` is removed. The same holds for a comment glued to either brace of an extension block, to a brace of an interpolation, and to the opening brace of a property pattern, and for the `catch`, `finally`, and do-statement `while` keywords: the blank lines above the comment are removed, as FMT-BLANK-007 and FMT-BLANK-008 remove them directly before the token.

### 17. A comment glued to a statement, a member, or the closing parenthesis of an argument list gets no inserted blank line

Covered by the maintainer's guidance.

```csharp
class C
{
    int H;
    /* m */ int I;

    void M(int a)
    {
        a++;
        /* s */ a++;
        Foo(1,
            2
            /* e */ );
    }
}
```

Before:

```csharp
class C
{
    int H;

    /* m */ int I;

    void M(int a)
    {
        a++;

        /* s */ a++;
        Foo(1,
            2

            /* e */ );
    }
}
```

After: the input is kept unchanged. A blank line the author wrote above such a comment is kept, before and after, and a blank line a statement rule asks for is inserted above the comment, before and after (`}⏎/* s */ a++;` gets the SHARED-BLANK-003 blank line).

### 18. A blank line between an own-line comment and a complex element initializer on one line is removed

Decided with output question 1A after the audit, which treats every complex element initializer's opening brace like the other initializer braces. It follows from the rule of entry 12: FMT-BLANK-020 names the opening brace of every complex element initializer, because the blank-line phase cannot read whether the line-break phase leaves the element on one line. The comment stands on its own line between two elements and shares no line with code.

```csharp
class C
{
    Dictionary<int, int> D = new()
                             {
                                 { 1, 2 },

                                 // c

                                 { 3, 4 }
                             };
}
```

Before: the input is kept unchanged.

After:

```csharp
class C
{
    Dictionary<int, int> D = new()
                             {
                                 { 1, 2 },

                                 // c
                                 { 3, 4 }
                             };
}
```

Before an element that spans several lines, the blank line is removed before and after. Today the blank line before an element whose contents the line-break phase expands is decided differently in two passes; after the change it is removed in one.

### 19. A comment glued to the terminal `break` of a section the formatter braces gets no blank line

Covered by the maintainer's guidance.

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1:
                A();
                B();
                /* b */ break;
        }
    }
}
```

Before:

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1:
                {
                    A();
                    B();
                }

                /* b */ break;
        }
    }
}
```

After:

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1:
                {
                    A();
                    B();
                }
                /* b */ break;
        }
    }
}
```

The comment is glued to the terminal `break` (FMT-BLANK-018), above which no statement requirement inserts a blank line (SHARED-BLANK-004), as above a terminal `break` without a comment (`}⏎break;`). A blank line the author wrote above the comment is kept, and a comment on a line of its own in front of the `break` keeps its FMT-BLANK-011 blank line, before and after (FMT-SWITCH-007).

### 20. A delimited comment that spans lines and ends on the token's line is glued to the token

Covered by the maintainer's guidance: such a comment shares its last line with code.

FMT-BLANK-018 classifies a comment that starts on an earlier line and ends on the token's line as on the token's line (`/* x⏎ */ T`), and so a comment in front of it on its first line (`/* a */ /* x⏎ */ T`). Where the first of them is an ordinary comment, they are glued, and the entries above apply to them as to a comment that starts on the token's line. Today the blank-line phase treats such a comment as a comment on its own line and FMT-BLANK-011 inserts a blank line above it.

```csharp
class C
{
    int[] G =
    /* g
     */ { 1, 2 };
    int H;
    /* m
     */ int I;

    void M(int a)
    {
        a++;
        /* s
         */ a++;

        if (a > 0)
        {
            a++;
        }
        /* k
         */ else
        {
            a--;
        }
    }
}
```

Before:

```csharp
class C
{
    int[] G =

    /* g
     */ { 1, 2 };
    int H;

    /* m
     */ int I;

    void M(int a)
    {
        a++;

        /* s
         */ a++;

        if (a > 0)
        {
            a++;
        }

        /* k
         */ else
        {
            a--;
        }
    }
}
```

After: the input is kept unchanged. A blank line the author wrote above `/* g` or `/* k` is removed, as directly before an array initializer's brace or `else` (entry 16), and one above `/* m` or `/* s` is kept (entry 17). The same holds for every other token: above a comment glued to the brace of an extension block, an interpolation, a property pattern, or a complex element initializer (entry 12), the closing brace of an assigned collection initializer (entry 14), and `catch`, `finally`, or a do-statement `while` (entry 16), the blank lines are removed; above a comment glued to an attribute list, a constraint clause, a parameter, an argument, a binary or conditional operator, an array element, a query clause, a switch-expression arm, a lambda arrow, the closing parenthesis of an argument list, or the first link of a chain, none is inserted and the author's is kept (entry 17). `/* x⏎ */ /** d */ {` is glued through the documentation comment (entry 15). Where a statement rule asks for a blank line above the statement — `return` (FMT-BLANK-001), `case` after `break` (FMT-BLANK-004), a statement after `}` (SHARED-BLANK-003) — it goes above the comment, before and after. No rule asks for a blank line in front of a member after `}`: SHARED-BLANK-003 covers statements only, so above a comment glued to such a member (`}⏎/* x⏎ */ int I;`) none is inserted and the author's is kept, as above `/* m`.

Before the tokens whose blank lines today's line-break phase normalized, the output is `main`'s, unchanged: no blank line above the comment, and the author's removed, before every opening and closing brace that FMT-BLANK-020 names (a method body's or a type's `{`, a closing brace, an object initializer's braces, an assigned collection initializer's `{`), the closing bracket of a collection expression or a list pattern, and the closing parenthesis of a parenthesized pattern:

```csharp
class C
{
    void M(int a)
    /* x
     */ {
        a++;

        /* y
         */ }
}
```

gives, before and after:

```csharp
class C
{
    void M(int a)
    /* x
     */ {
        a++;
        /* y
         */ }
}
```

The answered entries hold for these comments too: after a statement that ends in `}` the SHARED-BLANK-003 blank line goes above a comment glued to a block statement's brace (entry 3) and after `break` the FMT-BLANK-004 one (entry 8), the author's blank line above such a comment in front of a block statement after another statement is kept (entry 4), and an own-line comment in front of such a comment gets the FMT-BLANK-011 blank line, with the one between the two removed (entry 9: `a++;⏎// c⏎⏎/* x⏎ */ }` gives `a++;⏎⏎// c⏎/* x⏎ */ }`).

Unchanged, before and after: a documentation comment that starts the token's line is not glued (`/** d⏎ */ {` keeps its FMT-BLANK-011 blank line); a comment in front of the end of the file is on its own line; and a comment in front of a chain link that the line-break phase moves onto a line of its own because the link stands behind such a comment (FMT-BREAK-019) is on its own line in the output, so FMT-BLANK-011 inserts the blank line above it:

```csharp
class C
{
    void M(dynamic a)
    {
        var r = a.B()
                 .C()
                 /* c
                  */ .D();
    }
}
```

gives, before and after:

```csharp
class C
{
    void M(dynamic a)
    {
        var r = a.B()
                 .C()

                 /* c
                  */
                 .D();
    }
}
```

Code fixes that format through the formatter change with it. The RH5204 code fix on an unbraced embedded statement behind such a comment gave a blank line between `while (a)` and the comment; it now keeps the author's lines, because the comment is glued to the statement and no statement rule asks for a blank line in front of an embedded statement:

```csharp
class C
{
    void M(bool a, bool b)
    {
        while (a)
            /* note
            */ if (b)
        {
        X();
        }
    }
}
```

gives, before:

```csharp
class C
{
    void M(bool a, bool b)
    {
        while (a)

            /* note
            */ if (b)
            {
                X();
            }
    }
}
```

and after:

```csharp
class C
{
    void M(bool a, bool b)
    {
        while (a)
            /* note
            */ if (b)
            {
                X();
            }
    }
}
```

### 21. Behind a directive, the blank line in front of a `while` statement is kept

Demanded by FMT-BLANK-008 and FMT-BLANK-009, which name only the `while` of a do statement; both are unchanged by this change.

```csharp
class C
{
    void M(int a)
    {
        a++;
#pragma warning disable CS0168

        while (a > 0)
        {
            a--;
        }
    }
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
        while (a > 0)
        {
            a--;
        }
    }
}
```

After: the input is kept unchanged. Today the line-break phase removes the blank line between a trailing non-region directive and every `while` keyword; the `while` of a while statement starts a statement and continues none, so no requirement removes the blank line in front of it. Behind a directive in front of a do statement's `while`, the blank line is removed, before and after (FMT-BLANK-009). Without the directive (`a++;⏎⏎while`), the blank line is kept before and after.

### 22. Behind the `=` of an assigned collection initializer, the blank line above a directive is kept

Demanded by FMT-BLANK-009, which keeps the blank line in front of a directive in the gap before a brace, and by FMT-BREAK-030, under which the line-break phase does not remove a blank line in a gap it does not join.

```csharp
class C
{
    void M()
    {
        var p = new P
                {
                    L =

#pragma warning disable CS0168
                    {
                        1
                    }
                };
    }
}
```

Before:

```csharp
class C
{
    void M()
    {
        var p = new P
                {
                    L =
#pragma warning disable CS0168
                    {
                        1
                    }
                };
    }
}
```

After: the input is kept unchanged. Today the line-break phase pulls the gap in front of the initializer's `{` and removes the blank line above the directive with it; it now leaves a gap that holds a comment, a directive, or disabled text alone, as for the comment of entry 13. A blank line between the directive and `{` (`L =⏎#pragma …⏎⏎{`) is removed, and none is inserted when the author wrote none, before and after.

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

- The blank lines between a comment that ends its line and the brace, bracket, or parenthesis behind it are removed, before and after (FMT-BLANK-020), for every token FMT-BLANK-020 names except the opening brace of a block statement that follows a statement (entry 7), also when other comments stand in front of that comment on its line (`/* a */ /* b */`, `/** d */ // c`):

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

- Behind a comment that ends its line, the author's blank line before a brace of an array initializer, an extension block, or an interpolation, before the opening brace of a property pattern, and before the closing brace of a collection initializer assigned to a member is kept, before and after (FMT-BLANK-020 does not name them):

  ```csharp
  class C
  {
      void M()
      {
          var p = new P
                  {
                      L = {
                              1

                              // c

                          }
                  };
      }
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

### The maintainer's answers to the run's output questions

On 2026-11-01 the maintainer chose option A for every output question of the run:

1. A block statement after `break` gets the FMT-BLANK-004 blank line, also with a comment glued to its brace (visible output changes 5 and 8).
2. Only a block statement that follows another statement of its block or switch section is exempt from FMT-BLANK-008 and RH5025; the first statement of a block or switch section, a top-level block, and a labeled block are unchanged (visible output change 6).
3. Behind an own-line comment above a block statement that follows a statement, the author's blank line above `{` is kept (visible output change 7).
4. An own-line comment in front of a comment glued to a brace gets the FMT-BLANK-011 blank line, and the glued comment gets none (visible output change 9).
5. No blank line is inserted in front of a directive that precedes a comment glued to a brace; the output equals the input (visible output change 10).
6. Behind a directive, the SHARED-BLANK-003 blank line above a block statement after `}` goes after the directive (visible output change 2).
7. A comment glued to the brace of a block statement after `}` gets the SHARED-BLANK-003 blank line above the comment (visible output change 3).

On 2026-11-01, during the implementer stage, the maintainer chose option A for one more output question: no blank line is inserted in front of a non-region directive that starts the gap before a closing brace when a region directive ends it and a blank line stands between the two; the output equals the input (visible output change 11).

On 2026-11-01, before the audit, the maintainer chose option A for the same case before the closing `]` of a collection expression and, through the same rule, the closing `)` of a parenthesized pattern: no blank line is inserted in front of the directive, as before a closing brace (visible output change 11).

On 2026-11-01, after the audit, the maintainer chose option A for two more output questions:

1. Every opening brace of a complex element initializer is treated like the other initializer braces, whether or not the element spans lines: no blank line above a comment glued to it, and an author's blank line there is removed (visible output change 12), also between an own-line comment and that brace (visible output change 18).
2. The blank line between an own-line comment and the opening brace of a collection initializer assigned to a member is removed, as above an assigned object initializer (visible output change 13).

With these answers the maintainer gave guidance for the rest of the run, and clarified it the same day: comments that share a line with code (glued to a brace or another token) and comments between the `=` of an assignment and the assigned value are unwanted; comments belong only where they do not affect indentation. The formatter is not made more complex to give such comments a particular layout: a rule or exception that exists only to keep a specific output for them is replaced by the simplest uniform rule, and the output changes that follow are covered by the guidance and listed here (visible output changes 14 to 17 and 19, and the blank line above the comment in visible output change 13). Rules for comments on their own line between statements or members are not in its scope.

The spec is written on these answers.

### The opening brace of a block statement is exempt from FMT-BLANK-008 and RH5025

Decided by the maintainer (revision of thoenissen/Reihitsu#1496, 2026-10-28): FMT-BLANK-008 and RH5025 do not apply to the `{` of a block statement, not only after `}`. After a statement that ends in `}` the SHARED-BLANK-003 blank line is required; after any other statement the author's blank line is kept and not reported. This supersedes two earlier decisions:

- thoenissen/Reihitsu#1396 — RH5030 exempting a closing brace followed by a block statement. RH5030 keeps reporting it; the formatter now inserts the blank line RH5030 asks for.
- thoenissen/Reihitsu#1081, item 1, for the standalone block — RH5025 reporting the blank line above a brace that starts a standalone block. The other braces of that item (an initializer element, a switch arm, a braced switch section) stay reported.

### RH5020 keeps reporting a comment glued to `{`

Decided by the maintainer: RH5020 gets no exemption. A comment that starts its line glued to a brace is not wanted; it is fixed by hand, and the formatter leaves it without a blank line above, as today. RH5020 gets no exemption for a comment glued to any other token either: under the maintainer's guidance such a comment is unwanted as well, and RH5020 reports every glued comment the formatter leaves without a blank line above it.

### "Follows a statement" bounds the exemption

The exemption covers a block statement that follows another statement of the same block or switch section — the two cases the maintainer named, after `}` and after any other statement. A block statement that is the first statement of its block or switch section, and a block that is not in a statement list (a top-level block of a file with top-level statements, the block of a labeled statement), stay under FMT-BLANK-008 and RH5025. Decided by the maintainer: option A of output question 2.

### FMT-BLANK-004 applies above a block statement after `break`

The maintainer's "after any other statement, the author's blank line is kept" is read as "no rule of the brace removes or demands it": FMT-BLANK-004's blank line after `break` asks for one above a block statement like above any other statement, as RH5011 does. Decided by the maintainer: option A of output question 1.

### FMT-BLANK-018 decides a glued comment as part of its token's line

The issue exempts a comment that shares its line with "the `{` behind it" from FMT-BLANK-011. An earlier draft of this change kept today's output for every glued comment: it named the tokens above whose glued comment today's output has no blank line, which needed exceptions for the braces of array initializers, extension blocks, and interpolations, the opening braces of property patterns and of complex element initializers on one line, the closing braces of collection initializers assigned to a member, and a documentation comment between the comment and the token. The audit showed that one of them, the layout of a complex element initializer, cannot be decided by the blank-line phase without predicting the line-break phase, and the maintainer chose to treat every complex element brace alike (answer 1A). Under the maintainer's guidance, the remaining exceptions go too: each existed only to keep a specific output for a comment that shares a line with code.

The rule that replaces the list is the simplest one that needs no construct of its own: a glued comment belongs to the line of its token, gets no FMT-BLANK-011 blank line, and the blank lines above it are those directly before the token. It reuses the rules that already decide the blank lines directly before every token — FMT-BLANK-007, FMT-BLANK-008, FMT-BLANK-019, the statement rules, and FMT-BLANK-017 — so the exception for a comment glued to the opening brace of a block statement that follows a statement follows from FMT-BLANK-017 instead of being stated, and the maintainer's answers 3, 4, 5, and 7 give the same output as before. It also reaches glued comments in front of statements, members, and other tokens that today get the FMT-BLANK-011 blank line (visible output change 17); those comments share a line with code as well. A documentation comment that starts its line stays under FMT-BLANK-011, FMT-BLANK-012, and RH8303: it is not an ordinary comment, and treating it as glued would remove the blank line RH8303 asks for above documentation that follows a member.

### One classification of a comment's position

FMT-BLANK-011, FMT-BLANK-018, and FMT-BLANK-020 each used to test a comment's position with a predicate of its own — "starts its own line", "shares that line with the token", "ends its line" — and a delimited comment that starts on an earlier line and ends on the token's line satisfied none of the last two: it was neither glued nor a comment that ends its line, so FMT-BLANK-011 inserted a blank line above it and nothing decided the blank lines between it and a brace. Each audit of this change found another comment shape on that axis. FMT-BLANK-018 now states one classification that the three requirements read: a comment is on the token's line when no line break outside the text of a comment stands between it and the token, and on its own line otherwise. The classification follows SHARED-COMMENT-012 and SHARED-BLANK-002, under which a delimited comment that spans lines belongs to the line it starts on and the token behind its `*/` continues that line, so any part of the comment on the token's line puts the comment there. Before the braces, brackets, and pattern parentheses whose gap today's line-break phase normalized, this gives today's output (visible output change 20); before every other token it is a comment on a line with code, covered by the guidance.

Two cases are on their own line although no line break stands behind them. The end-of-file token has no text, so no comment shares its line. And a chain link that stands behind such a comment does not start its line, so FMT-BREAK-019 breaks the line in front of it (SHARED-COMMENT-012): in the formatter's output the comment ends its line. Classifying it as on the link's line would leave it without a blank line in the first pass and give it one in the second (FMT-PIPE-004), so it is classified by the line it ends up on, which keeps today's output. The blank-line phase decides it from the same facts the line-break phase reads — the chain and the comment in front of the link — which no phase between the two changes.

### A directive in front of a glued comment keeps its blank lines

FMT-BLANK-018 removes no blank line of a gap that holds a directive. Below a region directive, SHARED-BLANK-006 requires the blank line, since the next line starts with the comment, not with `}`; removing it would make RH5032 report the formatter's output, and FMT-BLANK-013 would insert what FMT-BLANK-018 removes. Below any other directive, today's output keeps the author's blank line. What changes is only the blank line today's line-break phase inserts in front of the first directive of such a gap (visible output change 10): no blank-line rule asks for it, and FMT-BLANK-016's reason — a blank line in front of a directive lands inside the region of the directive above or separates the directive from code grouped with it — speaks against writing one. Decided by the maintainer: option A of output question 5.

### A region directive at the end of the gap does not bring a blank line in front of the first directive

FMT-BLANK-009 keeps the author's blank line in front of the first non-region directive of a gap before a closing token and inserts none. A region directive that ends the gap gets its own blank line above it from FMT-BLANK-013 (SHARED-BLANK-005), which does not reach the directive in front of it; only the line-break phase inserted a blank line there, by treating the gap as if it held nothing but the region directive. With FMT-BREAK-030 that insertion goes, and FMT-BLANK-009 says so explicitly, with the exception of a directive directly below a region directive, where FMT-BLANK-013 decides. Decided by the maintainer: option A of the output question asked during the implementer stage for a closing brace, and option A of the output question asked before the audit for a collection expression's `]` and a parenthesized pattern's `)` (visible output change 11).

### FMT-BLANK-020 names its own tokens

The removal of the blank lines between a comment that ends its line and the brace, bracket, or parenthesis behind it is today's output, and only the line-break phase produces it; the blank-line phase reads "directly before" in FMT-BLANK-007 and FMT-BLANK-008 as "with nothing between", so a comment ends that gap. Today's output keeps those blank lines before the braces of an array initializer, an extension block, or an interpolation, the opening brace of a property pattern or of a complex element initializer that stays on one line, the closing brace of a collection initializer assigned to a member, and the chained keywords — the tokens the line-break phase does not normalize — and it removes them whatever stands in front of the comment on its line. FMT-BLANK-020 writes that output down with two changes: every complex element initializer's opening brace is named, because the blank-line phase cannot read the layout the line-break phase gives the element (answer 1A; visible output changes 12 and 18), and the opening brace of a collection initializer assigned to a member is named (answer 2A; visible output change 13). The other exclusions stay: they decide the blank line behind a comment on its own line, which the maintainer's guidance does not cover, and RH5024 and RH5025 reporting some of those outputs is a separate question. The set was FMT-BLANK-018's until FMT-BLANK-018 stopped naming tokens; FMT-BLANK-020 now states it itself. It is a requirement of its own rather than a sentence in FMT-BLANK-007, FMT-BLANK-008, and FMT-BLANK-019, because its token set is not theirs: those three cover every brace directly in front of the token. Behind a non-region directive before a closing bracket or parenthesis, FMT-BLANK-009 is extended for the same reason: today the line-break phase removes the blank line between the directive and the token, as FMT-BLANK-009 does before a brace.

### The blank lines below an own-line comment above a block statement follow the statement rules

FMT-BLANK-017 excludes the opening brace of a block statement that follows a statement from FMT-BLANK-020 as from FMT-BLANK-008: after a statement, the author's blank line between a comment and the next statement is kept (`A();⏎// c⏎⏎B();` keeps it today), and a block statement is decided like any other statement (visible output change 7). Decided by the maintainer: option A of output question 3.

### An own-line comment in front of a glued comment is separated like any other

`// note` in front of `/* d */ };` does not share its line with a brace, so FMT-BLANK-011 asks for a blank line above it, and FMT-BLANK-018 removes the blank line between it and the glued comment (visible output change 9). Today the line-break phase removes the line above `// note` and keeps the one below it, because it normalizes the run in front of the gap's first content as if the gap were empty. The maintainer's RH5020 decision covers a comment glued to a brace, not a comment on its own line. Decided by the maintainer: option A of output question 4.

### FMT-BLANK-019 is a clarification

The removal of a blank line before the closing bracket of a collection expression or list pattern and before the closing parenthesis of a parenthesized pattern is today's output; only its owner changes. It is written down as a blank-line requirement because the line-break phase no longer removes blank lines.

### The RH5025 rule page stays as it is

The page describes the blank line between a declaration and its opening brace. The block-statement exemption is an exception and does not change what the page tells a user in the normal case.

## Alternatives considered

- **FMT-BLANK-008 wins for a block statement, and RH5030 gains an exemption** (thoenissen/Reihitsu#1396) — rejected by the maintainer: a block statement's brace starts a statement and continues no header.
- **FMT-BLANK-011 wins for a comment glued to a brace** — rejected: the outputs the issue counts (eight) would gain a blank line above a comment that RH5020 reports anyway and that the maintainer wants fixed by hand.
- **FMT-BLANK-018 names the tokens whose glued comment has no blank line today** — the earlier draft of this change; replaced under the maintainer's guidance: its exceptions exist only to keep the output of unwanted comments, and one of them reads a layout the line-break phase changes later (answer 1A).
- **A delimited comment that ends on the token's line is a comment on its own line** — the reading of an earlier draft of this change: it gave such a comment the FMT-BLANK-011 blank line before every token, also before the braces, brackets, and pattern parentheses where today's output has none, and it left the blank lines between the comment and a brace to no requirement. Rejected: it changes today's output where no request asks for it and keeps a third position no rule was written for.
- **Glue such a comment only before the tokens FMT-BLANK-020 names** — rejected: it restores today's output only by a second predicate for one comment shape, which is the kind of special case for comments on a line with code that the maintainer's guidance rules out.
- **Treat a documentation comment that starts its line as glued** — not part of this change: RH8303 asks for a blank line above documentation that follows a member, and the guidance leaves documentation comments' own blank lines alone.
- **One precedence ranking for all insertion and removal rules** — rejected: after narrowing FMT-BLANK-008 and FMT-BLANK-011, no blank line is decided by two rules, so a ranking would decide nothing and invite overlaps.
- **Leave the blank line before a closing bracket to the author once the line-break phase stops removing it** — rejected: it would change today's output without a request to change it.
- **FMT-BLANK-020 for every brace and chained keyword** — not part of this change: it would remove blank lines today's output keeps behind a comment, before the braces FMT-BLANK-018 excludes and before `else`, `catch`, `finally`, and `while`, which no request asks for. RH5024, RH5025, and RH5026 report several of those outputs today; that is a separate question.
- **State the comment rule inside FMT-BLANK-007, FMT-BLANK-008, and FMT-BLANK-019** — rejected: their scope is every brace or bracket directly in front of the token, the comment rule's is FMT-BLANK-020's, so each would have to repeat the exceptions.
- **Keep the blank line the line-break phase inserts in front of a directive** — rejected by the maintainer (output question 5 and the implementer-stage question, option A each): it needs a rule that inserts a blank line in front of a directive, which FMT-BLANK-016 forbids for the blank lines FMT-BLANK-001 to FMT-BLANK-005 insert.
