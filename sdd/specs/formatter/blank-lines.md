# Formatter — Blank lines

| | |
|---|---|
| ID prefix | FMT-BLANK |
| Status | Active |
| Code owners | `BlankLinePhase`, `BlankLineTokenCleanupRewriter`, `BlankLineTriviaBoundaryRewriter`, `BlankLineRegionDirectiveRewriter`, `BlankLineStatementSpacingRewriter`, `BlankLineBreakSpacingRewriter`, `BlankLineCollapser`, `BlankLineEditor`, `BraceBlankLinePolicy` |
| Uses | FMT-PIPE-002, FMT-BREAK-022, SHARED-USING-008, SHARED-BLANK-001, SHARED-BLANK-002, SHARED-BLANK-003, SHARED-BLANK-004, SHARED-BLANK-005, SHARED-BLANK-006, SHARED-BLANK-007, FMT-DOC-001, FMT-DOC-003 |

This spec covers `BlankLinePhase`: where the formatter inserts a blank line (before statements and comments, after closing braces and `break`, around region directives), where it removes one (next to braces, closing brackets, chained keywords, and documentation comments, and between a comment and the brace or bracket behind it), and how it collapses runs of blank lines. What counts as a blank line, the closing-brace and terminal-`break` rules, and the region-directive rules are shared with the analyzer and specified in SHARED-BLANK.

## Non-goals

- Blank lines between type members. The phase neither inserts nor removes them; it only collapses runs (FMT-BLANK-014) and separates comments (FMT-BLANK-011).
- Line breaks that are not blank lines — braces, statements, and members on their own lines are FMT-BREAK.
- Moving documentation comments written behind code, single-line or delimited, onto a new line (FMT-DOC-001) and deciding which of them document nothing (FMT-DOC-003). This phase inserts no line break in front of a documentation comment; it only decides the blank lines around one that starts its line.
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

The formatter MUST remove the blank lines directly before an opening brace and before the `else`, `catch`, `finally`, and do-statement `while` keywords. This does not apply to the opening brace of a block statement that follows another statement (FMT-BLANK-017).

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

When a non-region directive or disabled text stands in the gap before a brace or a chained keyword covered by FMT-BLANK-007 or FMT-BLANK-008 — not the opening brace of a block statement that FMT-BLANK-017 names — or before a closing bracket or parenthesis covered by FMT-BLANK-019, the formatter MUST keep the blank line before the directive, collapsed to one, and MUST remove the blank lines between the last directive and the token when nothing but whitespace stands between the two. When a comment stands between the last directive and the token, the blank lines between the directive and the comment are kept, and FMT-BLANK-020 decides the ones between the comment and the token. The formatter MUST NOT insert a blank line in front of the first non-region directive or disabled text of such a gap when the author wrote none, also when a region directive ends the gap and FMT-BLANK-013 inserts the blank line between the two. In front of a directive that stands directly below a region directive, FMT-BLANK-013 decides (SHARED-BLANK-006).

Why: The blank line above the directive is the author's separator in front of the conditional code, not a blank line next to the brace, and the analyzer rules do not report it. Only the blank line that actually touches the token is the one RH5024–RH5027 forbid. No rule asks for a blank line in front of the directive; one inserted there would separate the directive from the code the author grouped with it, as FMT-BLANK-016 states for the blank lines the statement rules insert. A region directive at the end of the gap is framed by its own blank lines and does not change that.

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

Before the closing bracket of a collection expression, the blank line above the directive stays and the one below it is removed:

```csharp
class C
{
    int[] F = [
        1,

#if DEBUG
        2,
#endif

    ];
}
```

```csharp
class C
{
    int[] F = [
                  1,

#if DEBUG
        2,
#endif
              ];
}
```

With a comment behind the directive, the blank line between the directive and the comment stays:

```csharp
class C
{
    void M(int a)
    {
#if NEVER
        a++;
#endif

        // c
    }
}
```

is left unchanged.

When a region directive ends the gap, no blank line is inserted in front of the first directive; the blank line between the two is the region's, kept in the type body and inserted in the method body:

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

The formatter MUST ensure exactly one blank line before the first comment that starts its own line (SHARED-BLANK-002) in a token's leading trivia, counted as SHARED-BLANK-001 defines it. It MUST NOT insert one when the comment is the first content of a block, a type body, or a switch section, when a preprocessor directive directly precedes the comment, or when the comment is glued to a token as FMT-BLANK-018 states it — to a token FMT-BLANK-018 names, or to the opening brace of a block statement that follows another statement, above which FMT-BLANK-017 decides the blank lines. This applies to ordinary and documentation comments alike, including a comment before a closing brace. This requirement does not yet decide a comment that leads a using directive of the same group as the directive above it; the blank lines inside a using block are SHARED-USING-008.

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

### FMT-BLANK-012 — Documentation moved off code is separated from it

The formatter MUST give a documentation comment that FMT-DOC-001 moved onto a new line, single-line or delimited, the blank line of FMT-BLANK-011 in the same run, like a comment the author wrote on its own line: one blank line above it, none when it is the first content of a block, a type body, or a switch section. A documentation comment that FMT-DOC-003 keeps behind code does not start its line and gets no blank line; once the author put it on its own line, FMT-BLANK-011 applies.

Why: Once a documentation comment stands at the start of its line it is a comment like any other and needs the separation of FMT-BLANK-011. FMT-DOC-001 moves it before this phase runs, so this phase only adds the blank line and never decides where the comment starts its line. A stranded comment sits on a line the author already filled; it is not separated from code it shares a line with.

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

A comment moved off an opening brace is the first content of the type body and gets no blank line:

```csharp
class C
{ /// <inheritdoc/>
    void M()
    {
    }
}
```

```csharp
class C
{
    /// <inheritdoc/>
    void M()
    {
    }
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

### FMT-BLANK-017 — The opening brace of a block statement that follows a statement keeps the statement rules

The formatter MUST decide the blank lines above the opening brace of a block statement that follows another statement of the same block or switch section as it decides them above any other statement: it MUST insert the blank line of SHARED-BLANK-003 after a statement that ends with `}` (placed as FMT-BLANK-005 and FMT-BLANK-016 state) and the blank line of FMT-BLANK-004 after a `break` statement, and after any other statement it MUST keep a blank line the author wrote, collapsed to one (FMT-BLANK-014), and MUST NOT insert one. FMT-BLANK-008, FMT-BLANK-009, and FMT-BLANK-020 do not apply to that brace; behind a comment above it, the blank lines between the comment and the brace are kept as the author wrote them, collapsed to one, as between a comment and any other statement. A block statement is a block that stands in the statement list of a block or a switch section. A block that does not follow another statement of its list — the first statement of a block or a switch section — stays under FMT-BLANK-007 and FMT-BLANK-008, and so does a block that is not in such a list, such as the body of an `if` statement or a method.

Why: The opening brace of a block statement starts a statement; it continues no header above it, which is what FMT-BLANK-008 protects. Deciding the blank line above it by the statement rules gives the output that RH5030 and RH5011 ask for, and RH5025 does not report it. The first statement of a list has its boundary in the brace or the case label above it, which FMT-BLANK-007 and FMT-BLANK-008 keep tight.

Example — after a statement that ends with `}`:

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

After a statement that does not end with `}`, the author's blank line is kept and none is inserted:

```csharp
class C
{
    void M()
    {
        A();

        {
            B();
        }

        A();
        {
            B();
        }
    }
}
```

is left unchanged.

After a `break` statement:

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

Behind a directive, the blank line of SHARED-BLANK-003 goes after the directive, and a blank line the author wrote between a directive and the brace is kept:

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
        A();
#pragma warning disable CS0168

        {
            B();
        }
    }
}
```

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

        A();
#pragma warning disable CS0168

        {
            B();
        }
    }
}
```

The first statement of a switch section and the body of an `if` statement are not block statements that follow a statement; their blank lines are removed:

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
        }

        if (a > 0)

        {
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
        }

        if (a > 0)
        {
        }
    }
}
```

### FMT-BLANK-018 — A comment glued to a brace gets no blank line of its own

A comment is glued to a token when it is an ordinary comment that starts its line and shares that line with the token behind it, with nothing but whitespace and ordinary comments between the two. Above a comment glued to a token, the formatter MUST NOT insert the blank line of FMT-BLANK-011 and MUST remove the blank lines the author wrote, when that token is:

- an opening or closing brace, other than
  - a brace of an array initializer, of an extension block, or of an interpolation,
  - the opening brace of a property pattern,
  - the opening brace of a complex element initializer (a braced element of a collection initializer, such as `{ 3, 4 }`) whose closing brace stands on the same line,
  - the closing brace of a collection initializer that is assigned to a member inside an object initializer (`L = { 1 }`);
- the closing bracket of a collection expression or a list pattern;
- the closing parenthesis of a parenthesized pattern.

Above a comment glued to any other token, FMT-BLANK-011 applies, and so it does above an ordinary comment that shares its line with a documentation comment in front of the token: that comment is not glued to the token. When a preprocessor directive or disabled text stands in the gap before the comment, this requirement removes no blank line of that gap: the line below a region directive starts with the comment, not with a closing brace, so SHARED-BLANK-006 requires it to be blank and FMT-BLANK-013 inserts it, and every other blank line of such a gap is kept as written, collapsed to one (FMT-BLANK-014). A comment glued to the opening brace of a block statement that follows another statement is not affected: FMT-BLANK-017 decides the blank lines above it, so it gets the blank line of SHARED-BLANK-003 after a statement that ends with `}` and keeps the author's blank line after any other statement.

Why: A comment that shares its line with a brace or a closing token is not a comment that introduces the code below it; it is glued to the token that opens or closes a construct, and a blank line above it would split that construct's layout. The author is expected to move such a comment by hand — RH5020 keeps reporting it. The tokens named are the ones whose glued comment has had no blank line above it in the formatter's output; the braces of array initializers, extension blocks, and interpolations, the opening braces of property patterns and of complex element initializers on one line, the closing braces of collection initializers assigned to a member, and a comment in front of a documentation comment on the token's line have had one. A directive in the gap closes conditional code or a region in front of the glued line; the blank line below a region directive is the region's (SHARED-BLANK-006), and the others are the author's, as they have been in the formatter's output.

Example — no blank line is inserted above a comment glued to the brace of an initializer, a block statement, or a closing brace, and the author's blank line above a comment glued to an initializer's brace is removed:

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

Without a blank line in the input, nothing changes:

```csharp
class C
{
    void M()
    {
        A();
        /* a */ {
            B();
        }

        var p = new P
                {
                    Child =
                    /* c */ {
                                X = 2
                            }
                /* d */ };
    }
}
```

is left unchanged.

A comment glued to an array initializer's brace or to the closing parenthesis of an argument list is separated:

```csharp
class C
{
    int[] G =
    /* b */ { 1, 2 };

    void M()
    {
        Foo(1,
            2
            /* e */ );
    }
}
```

```csharp
class C
{
    int[] G =

    /* b */ { 1, 2 };

    void M()
    {
        Foo(1,
            2

            /* e */ );
    }
}
```

A comment glued to the opening brace of a complex element initializer that stays on one line is separated too; at one whose closing brace stands on a later line, it is not:

```csharp
class C
{
    Dictionary<int, int> D = new()
                             {
                                 { 1, 2 },
                                 /* c1 */ {
                                     3,
                                     4
                                 },
                                 /* c2 */ { 5, 6 }
                             };
}
```

```csharp
class C
{
    Dictionary<int, int> D = new()
                             {
                                 { 1, 2 },
                                 /* c1 */ {
                                              3,
                                              4
                                          },

                                 /* c2 */ { 5, 6 }
                             };
}
```

A comment glued to the closing brace of a collection initializer assigned to a member is separated; at the closing brace of a collection initializer that is not assigned, it is not:

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
        var q = new List<int>
                {
                    1
                /* m */ };
    }
}
```

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
        var q = new List<int>
                {
                    1
                /* m */ };
    }
}
```

A comment in front of a documentation comment on the brace's line is not glued to the brace and is separated; without the documentation comment, it is glued:

```csharp
public class TestClass
/* x */ /** Doc */ {
}

public class TestClass2
/* y */ {
}
```

```csharp
public class TestClass

/* x */ /** Doc */ {
}

public class TestClass2
/* y */ {
}
```

Behind a region directive, the blank line SHARED-BLANK-006 requires is inserted above the glued comment; behind another directive, the blank lines are kept as written, and none is inserted in front of the directive:

```csharp
class C
{
    void M(int a)
    {
        #region R

        a++;

        #endregion // R
    /* d */ }

    void N(int a)
    {
        a++;
#pragma warning disable CS0168

    /* e */ }
}
```

```csharp
class C
{
    void M(int a)
    {
        #region R

        a++;

        #endregion // R

    /* d */ }

    void N(int a)
    {
        a++;
#pragma warning disable CS0168

    /* e */ }
}
```

Above a comment glued to a block statement's brace, FMT-BLANK-017 decides: after a statement that ends with `}`, the blank line of SHARED-BLANK-003 goes above the comment, and after any other statement the author's blank line is kept:

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

        A();

        /* d */ {
            B();
        }
    }
}
```

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

        A();

        /* d */ {
            B();
        }
    }
}
```

### FMT-BLANK-019 — No blank line before the closing bracket of a collection expression or pattern

The formatter MUST remove the blank lines directly before the closing bracket of a collection expression or a list pattern and before the closing parenthesis of a parenthesized pattern. A blank line behind the opening bracket or parenthesis is kept. This requirement does not remove the blank line directly below a region directive: the line below it starts with the closing bracket or parenthesis, not with a closing brace, so SHARED-BLANK-006 requires it to be blank and FMT-BLANK-013 inserts it. Behind another directive, FMT-BLANK-009 decides which blank lines are removed, and behind a comment that ends its line, FMT-BLANK-020.

Why: The closing token ends a construct that FMT-BREAK-022 lays out one element per line; a blank line in front of it adds height without separating two elements, as a blank line before a closing brace does (FMT-BLANK-007). The blank line behind the opening token is the author's, and no rule has removed it. Below a region directive the shared rule asks for the blank line, and RH5032 would report its removal.

Example:

```csharp
class C
{
    int[] F = [
        1,
        2

    ];

    void M(object o)
    {
        if (o is [
                1,
                2

            ])
        {
        }
    }
}
```

```csharp
class C
{
    int[] F = [
                  1,
                  2
              ];

    void M(object o)
    {
        if (o is [
                     1,
                     2
                 ])
        {
        }
    }
}
```

### FMT-BLANK-020 — Behind a comment on its own line, no blank line before a brace or closing bracket

When the last content in the gap before a token that FMT-BLANK-018 names is an ordinary comment that starts its own line (SHARED-BLANK-002) and ends it, the formatter MUST remove the blank lines between that comment and the token. The blank lines above that comment are decided by the other requirements, such as FMT-BLANK-011 for the first comment of the gap. This does not apply to the opening brace of a block statement that follows another statement: FMT-BLANK-017 decides the blank lines above it, comments included. A comment that trails the previous token on its line is not in the gap's own lines; the blank lines behind it are directly before the token (FMT-BLANK-007, FMT-BLANK-008). A comment that shares its line with the token is FMT-BLANK-018's, and a documentation comment is FMT-BLANK-010's.

Why: The comment describes the token below it, or the end of the construct the token closes; a blank line between the two detaches it, as a blank line directly before the token would (FMT-BLANK-007, FMT-BLANK-008, FMT-BLANK-019). The formatter mirrors RH5024 and RH5025, which report such a blank line before a closing and an opening brace. The tokens are the ones whose blank line behind a comment has been removed in the formatter's output.

Example — before a closing bracket, the opening brace of an object initializer, and a closing brace; the blank line between two comments stays, and FMT-BLANK-011 puts one above the first comment of each gap:

```csharp
class C
{
    int[] F = [
        1
        // end of list

    ];

    void M(int a)
    {
        var p = new P
        // initializer

        {
            X = 1
        };
        a++;
        // c

        // d

    }
}
```

```csharp
class C
{
    int[] F = [
                  1

              // end of list
              ];

    void M(int a)
    {
        var p = new P

                // initializer
                {
                    X = 1
                };

        a++;

        // c

        // d
    }
}
```

Before the first statement of a block, which is not a block statement that follows a statement:

```csharp
class C
{
    void M()
    {
        // c

        {
            B();
        }
    }
}
```

```csharp
class C
{
    void M()
    {
        // c
        {
            B();
        }
    }
}
```

Behind a comment that trails the previous token, the blank line is directly before the token:

```csharp
class C
{
    void M(int a)
    {
        a++; // c

    }
}
```

```csharp
class C
{
    void M(int a)
    {
        a++; // c
    }
}
```

Before the opening brace of a block statement that follows a statement, FMT-BLANK-017 decides, and the author's blank line behind the comment is kept:

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

is left unchanged.
