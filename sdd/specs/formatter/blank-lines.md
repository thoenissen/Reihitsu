# Formatter — Blank lines

| | |
|---|---|
| ID prefix | FMT-BLANK |
| Status | Active |
| Code owners | `BlankLinePhase`, `BlankLineTokenCleanupRewriter`, `BlankLineTriviaBoundaryRewriter`, `BlankLineRegionDirectiveRewriter`, `BlankLineStatementSpacingRewriter`, `BlankLineBreakSpacingRewriter`, `BlankLineCollapser`, `BlankLineEditor` |
| Uses | FMT-PIPE-002, SHARED-USING-008, SHARED-BLANK-001, SHARED-BLANK-002, SHARED-BLANK-003, SHARED-BLANK-004, SHARED-BLANK-005, SHARED-BLANK-006, SHARED-BLANK-007, FMT-DOC-001, FMT-DOC-003 |

This spec covers `BlankLinePhase`: where the formatter inserts a blank line (before statements and comments, after closing braces and `break`, around region directives), where it removes one (next to braces, chained keywords, and documentation comments), and how it collapses runs of blank lines. What counts as a blank line, the closing-brace and terminal-`break` rules, and the region-directive rules are shared with the analyzer and specified in SHARED-BLANK.

## Non-goals

- Blank lines between type members. The phase neither inserts nor removes them; it only collapses runs (FMT-BLANK-014) and separates comments (FMT-BLANK-011).
- Line breaks that are not blank lines — braces, statements, and members on their own lines are FMT-BREAK.
- Moving documentation comments written behind code onto their own line (FMT-DOC-001) and deciding which of them document nothing (FMT-DOC-003).
- The node-level preservation of a target's surrounding trivia (FMT-PIPE-010).

## Requirements

### FMT-BLANK-001 — Jump and compound statements are preceded by a blank line

The formatter MUST insert one blank line before a `try`, `if`, `while`, `do`, `using`, `foreach`, `for`, `return`, `goto`, `continue`, `throw`, `switch`, `checked`, `unchecked`, `fixed`, or `lock` statement that is not the first statement of its block or switch section. An existing blank line MUST be kept, not doubled.

Why: These statements start a new step of control flow; the blank line makes that step visible. The formatter mirrors RH5001–RH5009 and RH5012–RH5018, so formatted code raises none of them. The first statement of a block needs no separator — the opening brace is one (FMT-BLANK-007).

Example:

```csharp
class C
{
    void M(int a)
    {
        a++;
        if (a > 0)
        {
            a--;
        }
        a++;
        return;
    }
}
```

```csharp
class C
{
    void M(int a)
    {
        a++;

        if (a > 0)
        {
            a--;
        }

        a++;

        return;
    }
}
```

The blank line before the second `a++` is SHARED-BLANK-003.

### FMT-BLANK-002 — Local declarations are separated from other statements

The formatter MUST insert a blank line before a local declaration whose preceding statement is not a local declaration, and before an expression statement whose preceding statement is a local declaration, unless that expression statement is an assignment.

Why: A run of declarations reads as one unit — the setup of the code that follows. The assignment exception lets a declaration be followed directly by the statement that completes its initialization. The formatter mirrors RH5029 and RH5021.

Example:

```csharp
class C
{
    void M(int a)
    {
        a++;
        var b = 1;
        var c = 2;
        a++;
        var d = 3;
        a = 4;
        var e = 5;
        M(e);
    }
}
```

```csharp
class C
{
    void M(int a)
    {
        a++;

        var b = 1;
        var c = 2;

        a++;

        var d = 3;
        a = 4;

        var e = 5;

        M(e);
    }
}
```

### FMT-BLANK-003 — `yield` statements are separated from other statements

The formatter MUST insert a blank line before a `yield return` or `yield break` statement whose preceding statement is not itself a `yield` statement.

Why: Consecutive `yield` statements form one sequence of produced values and stay together; the sequence as a whole is separated from the surrounding logic. The formatter mirrors RH5019.

Example:

```csharp
class C
{
    System.Collections.Generic.IEnumerable<int> M(int a)
    {
        a++;
        yield return 1;
        yield return 2;
        a++;
        yield break;
    }
}
```

```csharp
class C
{
    System.Collections.Generic.IEnumerable<int> M(int a)
    {
        a++;

        yield return 1;
        yield return 2;
        a++;

        yield break;
    }
}
```

### FMT-BLANK-004 — `break` is separated on both sides

The formatter MUST insert a blank line before a `break` statement that is not the first statement of its block or switch section and not exempt under SHARED-BLANK-004, and after a `break` statement that is followed by another statement in the same list. It MUST insert a blank line before a switch section whose preceding section ends in a `break` statement; a section ending in any other statement gets none.

Why: `break` leaves the enclosing loop or switch, so it is set off from the work before it, and code after it is a new path. Sections that end in `break` are the common case and are separated so each case reads as its own unit; sections ending in `return` or `throw` keep their compact form because only the `break` rule (RH5011) asks for the line. The formatter mirrors RH5010 and RH5011.

Example:

```csharp
class C
{
    void M(int a)
    {
        a++;
        while (true)
        {
            a++;
            break;
        }
        foreach (var x in new int[0])
        {
            break;
            a++;
        }
    }
}
```

```csharp
class C
{
    void M(int a)
    {
        a++;

        while (true)
        {
            a++;

            break;
        }

        foreach (var x in new int[0])
        {
            break;

            a++;
        }
    }
}
```

Switch sections:

```csharp
class C
{
    void M(int a)
    {
        switch (a)
        {
            case 1:
                a++;
                break;
            case 2:
                return;
            case 3:
                break;
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
                a++;
                break;

            case 2:
                return;
            case 3:
                break;
        }
    }
}
```

### FMT-BLANK-005 — The blank line after a closing brace goes behind a directive

The formatter MUST apply SHARED-BLANK-003. When directives stand between the closing brace and the next statement, it MUST insert the blank line after the last of those directives: directly before the statement, or, when comments stand between that directive and the statement, in front of those comments (FMT-BLANK-016). When both statements share a line, it MUST insert the blank line as well; FMT-BREAK then puts them on separate lines.

Why: SHARED-BLANK-003 has no directive exemption, but a blank line inserted at the start of the gap would land inside the conditional region the directive opens or closes. Placing it after the last directive keeps it outside. Same-line pairs do not survive formatting, so the formatter decides them as if they were already split.

Example:

```csharp
class C
{
    void M(int a)
    {
        if (a > 0)
        {
        }
#pragma warning disable CS0168
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
        }
#pragma warning disable CS0168

        a++;
    }
}
```

A same-line pair:

```csharp
class C
{
    void M(int a)
    {
        a++;
        if (a > 0) { a++; } a++;
    }
}
```

```csharp
class C
{
    void M(int a)
    {
        a++;

        if (a > 0)
        {
            a++;
        }

        a++;
    }
}
```

### FMT-BLANK-006 — A directive directly before a statement suppresses its statement-kind blank line

The formatter MUST NOT insert the blank line of FMT-BLANK-001 to FMT-BLANK-004 before a statement whose first token is directly preceded by a preprocessor directive.

Why: The directive already separates the statement from what precedes it, and a blank line inserted in front of the directive would change which code the author grouped with the condition. The analyzer rules apply the same exemption. SHARED-BLANK-003 is not affected (FMT-BLANK-005).

Example — no blank line before `if` or `return`, but one after `#endif` because the `if` statement ends with a closing brace:

```csharp
class C
{
    void M(int a)
    {
        a++;
#if !NEVER
        if (a > 0)
        {
        }
#endif
        a++;
#pragma warning disable CS0168
        return;
    }
}
```

```csharp
class C
{
    void M(int a)
    {
        a++;
#if !NEVER
        if (a > 0)
        {
        }
#endif

        a++;
#pragma warning disable CS0168
        return;
    }
}
```

### FMT-BLANK-007 — No blank lines inside braces or at the start of a file

The formatter MUST remove every blank line directly after an opening brace, directly before a closing brace, and before the first line of the file.

Why: Braces already delimit their content; a blank line next to them adds height without separating anything. The formatter mirrors RH5022, RH5024, and RH5028.

Example:

```csharp
class C
{

    void M(int a)
    {

        a++;

    }

}
```

```csharp
class C
{
    void M(int a)
    {
        a++;
    }
}
```

At the start of a file:

```csharp


// header
namespace N;
```

```csharp
// header
namespace N;
```

Directly after an opening brace that a region directive follows; the blank line between the two directives stays (SHARED-BLANK-006):

```csharp
class C
{
    #region A
    #endregion // A
}
```

```csharp
class C
{
    #region A

    #endregion // A
}
```

### FMT-BLANK-008 — No blank line before an opening brace or a chained keyword

The formatter MUST remove the blank lines directly before an opening brace and before the `else`, `catch`, `finally`, and do-statement `while` keywords.

Why: These tokens continue the statement above them; a blank line would visually detach the continuation from the part it belongs to. The formatter mirrors RH5025, RH5026, and RH5027.

Example:

```csharp
class C
{
    void M(int a)
    {
        try
        {
            a++;
        }

        catch
        {
        }

        finally
        {
        }
        if (a > 0)
        {
        }

        else
        {
        }
        do
        {
        }

        while (a > 0);
    }
}
```

```csharp
class C
{
    void M(int a)
    {
        try
        {
            a++;
        }
        catch
        {
        }
        finally
        {
        }

        if (a > 0)
        {
        }
        else
        {
        }

        do
        {
        }
        while (a > 0);
    }
}
```

### FMT-BLANK-009 — Around a directive, only the blank line next to the token is removed

When a non-region directive or disabled text stands in the gap before a brace or a chained keyword covered by FMT-BLANK-007 or FMT-BLANK-008, the formatter MUST keep the blank line before the directive, collapsed to one, and MUST remove the blank lines between the last directive and the token.

Why: The blank line above the directive is the author's separator in front of the conditional code, not a blank line next to the brace, and the analyzer rules do not report it. Only the blank line that actually touches the token is the one RH5024–RH5027 forbid.

Example:

```csharp
class C
{
    void M(bool flag)
    {
        if (flag)
        {
        }

#pragma warning disable CA1801
        else
        {
        }
        if (flag)
        {
        }
#pragma warning disable CA1801

        else
        {
        }
    }
}
```

```csharp
class C
{
    void M(bool flag)
    {
        if (flag)
        {
        }

#pragma warning disable CA1801
        else
        {
        }

        if (flag)
        {
        }
#pragma warning disable CA1801
        else
        {
        }
    }
}
```

Before a closing brace:

```csharp
class C
{
    void M(int a)
    {
        a++;
#if DEBUG
        a++;
#endif

    }
}
```

```csharp
class C
{
    void M(int a)
    {
        a++;
#if DEBUG
        a++;
#endif
    }
}
```

### FMT-BLANK-010 — No blank line after a documentation comment

The formatter MUST remove the blank lines between a documentation comment and the next line of its token's leading trivia, whether that line holds the documented code or another comment.

Why: A documentation comment belongs to the code below it; a blank line would visually detach it. The formatter mirrors RH8302.

Example:

```csharp
class C
{
    /// <summary>
    /// Doc
    /// </summary>

    // comment
    void M()
    {
    }
}
```

```csharp
class C
{
    /// <summary>
    /// Doc
    /// </summary>
    // comment
    void M()
    {
    }
}
```

### FMT-BLANK-011 — A comment on its own line is preceded by exactly one blank line

The formatter MUST ensure exactly one blank line before the first comment that starts its own line (SHARED-BLANK-002) in a token's leading trivia, counted as SHARED-BLANK-001 defines it. It MUST NOT insert one when the comment is the first content of a block, a type body, or a switch section, or when a preprocessor directive directly precedes the comment. This applies to ordinary and documentation comments alike, including a comment before a closing brace. This requirement does not yet decide a comment that leads a using directive of the same group as the directive above it; the blank lines inside a using block are SHARED-USING-008.

Why: A comment introduces the code below it; the blank line groups it with that code instead of with the code above. At the start of a block the opening brace or the case label already provides the boundary, and after a directive the comment belongs to the directive's group. The formatter mirrors RH5020 and RH8303.

Example:

```csharp
class C
{
    void M(int a)
    {
        a++;
        // comment
        a++;


        // second
        a++;
#pragma warning disable CS0168
        // after directive
        a++;
    }
    // trailing
}
```

```csharp
class C
{
    void M(int a)
    {
        a++;

        // comment
        a++;

        // second
        a++;
#pragma warning disable CS0168
        // after directive
        a++;
    }

    // trailing
}
```

First in a block or a switch section:

```csharp
class C
{
    void M(int a)
    {
        // first in block
        a++;
        switch (a)
        {
            case 1:
                // first in section
                break;
        }
    }
}
```

```csharp
class C
{
    void M(int a)
    {
        // first in block
        a++;

        switch (a)
        {
            case 1:
                // first in section
                break;
        }
    }
}
```

### FMT-BLANK-012 — Documentation written behind code is separated from it

The formatter MUST put one blank line above a documentation comment that FMT-DOC-001 moved onto its own line, and MUST move a delimited documentation comment (`/** */`) written behind code onto its own line below a blank line, when the comment precedes the code it documents. A documentation comment that documents nothing (FMT-DOC-003) MUST stay on the line where it was written; once the author put it on its own line, FMT-BLANK-011 applies.

Why: Once a documentation comment stands on its own line it is a comment like any other and needs the separation of FMT-BLANK-011. Delimited documentation is not relocated by FMT-DOC, so this phase produces both line breaks at once, keeping the result stable in one run. A stranded comment sits on a line the author already filled; breaking it off is a placement decision the formatter leaves to the author.

Example:

```csharp
class C
{
    int A; /// <inheritdoc/>
    int B;
}
```

```csharp
class C
{
    int A;

    /// <inheritdoc/>
    int B;
}
```

A delimited documentation comment:

```csharp
enum E
{
    First, /** doc */
    Second
}
```

```csharp
enum E
{
    First,

    /** doc */
    Second
}
```

A stranded documentation comment:

```csharp
class C
{
    void M()
    {
        var x = 1; /** stray */
    }
}
```

is left unchanged, while the same comment on its own line is separated:

```csharp
class C
{
    void M()
    {
        var x = 1;
        /// stray
    }
}
```

```csharp
class C
{
    void M()
    {
        var x = 1;

        /// stray
    }
}
```

### FMT-BLANK-013 — Region directives are framed by blank lines

The formatter MUST apply SHARED-BLANK-005, SHARED-BLANK-006, and SHARED-BLANK-007, inserting the missing blank line directly above or below the directive. It MUST remove the blank lines between an `#endregion` and a closing brace that follows it.

Why: The rules are shared so the formatter never produces a region that RH5031 or RH5032 reports. The blank line before the closing brace is FMT-BLANK-007's rule; it is stated here because the `#endregion` sits between the two and would otherwise hide the gap from it.

Example:

```csharp
class C
{
    int _a;
    #region R

    int _b;

    #endregion // R

}
```

```csharp
class C
{
    int _a;

    #region R

    int _b;

    #endregion // R
}
```

A switch label is not an opening brace, so a region directly below it is separated:

```csharp
class C
{
    void M(int a)
    {
        switch (a)
        {
            case 1:
                #region R

                a++;
                break;

            #endregion // R
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

                #region R

                a++;
                break;

            #endregion // R
        }
    }
}
```

A comment that is the first content after an opening brace needs no blank line above it (FMT-BLANK-011), but it does not stand in for the blank line above a region directive below it:

```csharp
class C
{
    // c
    #region A
    #endregion // A
}
```

```csharp
class C
{
    // c

    #region A

    #endregion // A
}
```

### FMT-BLANK-014 — Runs of blank lines collapse to one

The formatter MUST replace two or more consecutive blank lines with a single blank line wherever they occur between tokens, including after a directive or a documentation comment.

Why: One blank line is enough to separate two units; more only spreads the code out. The formatter mirrors RH5023. The rules above that keep a blank line keep exactly one.

Example:

```csharp
class C
{
    void M(int a)
    {

        a++;


        a++;



        a--;

    }

}
```

```csharp
class C
{
    void M(int a)
    {
        a++;

        a++;

        a--;
    }
}
```

After a directive:

```csharp
class C
{
    void M(int a)
    {
        a++;
#pragma warning disable CS0168



        a++;
    }
}
```

```csharp
class C
{
    void M(int a)
    {
        a++;
#pragma warning disable CS0168

        a++;
    }
}
```

### FMT-BLANK-015 — Blank lines inside literals, comments, and disabled text are kept

The formatter MUST NOT add, remove, or collapse blank lines inside a string literal, a multi-line comment, or disabled text.

Why: Those lines are part of the literal's value, the comment's text, or code the current build does not compile (FMT-PIPE-002). The phase only edits whitespace and line-break trivia between tokens.

Example — only the blank line before the block comment is inserted (FMT-BLANK-011):

```csharp
class C
{
    void M(int a)
    {
        a++;
#if NEVER
        a++;



        a++;
#endif
        var s = @"x



y";
        /* one


           two */
    }
}
```

```csharp
class C
{
    void M(int a)
    {
        a++;
#if NEVER
        a++;



        a++;
#endif
        var s = @"x



y";

        /* one


           two */
    }
}
```

### FMT-BLANK-016 — A required blank line goes after directives, never directly in front of one

When the formatter inserts a blank line that FMT-BLANK-001 to FMT-BLANK-005 ask for into a gap that holds a preprocessor directive, it MUST insert the blank line after the last directive of the gap, in front of the comments that stand between that directive and the code behind it, and it MUST NOT insert a blank line directly in front of a directive. Whether such a gap needs the blank line at all is decided by the requirement that asks for it, together with FMT-BLANK-006 for a directive directly in front of a statement. This requirement covers only the blank lines the formatter inserts; a blank line the author wrote in front of a directive is decided by the other requirements, such as FMT-BLANK-009.

Why: A directive opens or closes a conditional region, or changes the compiler's state for the code behind it. A blank line inserted in front of it lands inside the region of the directive above, or separates the directive from code the author grouped with it. Behind the last directive, the blank line separates what the requirement asks it to separate: the code that follows, with the comments that describe it, from what comes before.

Example — no blank line is inserted in front of `#if` or `#pragma`, and the blank line after the `if` statement's closing brace goes after `#endif`:

```csharp
class C
{
    void M(int a)
    {
        a++;
#if !NEVER
        if (a > 0)
        {
        }
#endif
        a++;
#pragma warning disable CS0168
        return;
    }
}
```

```csharp
class C
{
    void M(int a)
    {
        a++;
#if !NEVER
        if (a > 0)
        {
        }
#endif

        a++;
#pragma warning disable CS0168
        return;
    }
}
```

A comment between the last directive and the statement stays with the statement, so the blank line goes in front of the comment:

```csharp
class C
{
    void M(int a)
    {
        if (a > 0)
        {
        }
#pragma warning disable CS0168
        // why
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
        }
#pragma warning disable CS0168

        // why
        a++;
    }
}
```
