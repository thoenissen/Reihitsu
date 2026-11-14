# Formatter — Blank lines

| | |
|---|---|
| ID prefix | FMT-BLANK |
| Status | Active |
| Code owners | `BlankLinePhase`, `BlankLineTokenCleanupRewriter`, `BlankLineTriviaBoundaryRewriter`, `BlankLineRegionDirectiveRewriter`, `BlankLineStatementSpacingRewriter`, `BlankLineBreakSpacingRewriter`, `BlankLineCollapser`, `BlankLineEditor`, `BraceBlankLinePolicy` |
| Uses | FMT-PIPE-002, FMT-BREAK-019, FMT-BREAK-022, FMT-BREAK-030, FMT-BREAK-033, SHARED-USING-011, SHARED-BLANK-001, SHARED-BLANK-002, SHARED-BLANK-003, SHARED-BLANK-004, SHARED-BLANK-005, SHARED-BLANK-006, SHARED-BLANK-007, SHARED-COMMENT-012, FMT-DOC-001, FMT-DOC-003, FMT-BREAK-006, FMT-PIPE-001, FMT-PIPE-025, RH5011-002 |

This spec covers `BlankLinePhase`: where the formatter inserts a blank line (before statements and comments, after closing braces and `break`, around region directives), where it removes one (next to braces, closing brackets, chained keywords, and documentation comments, and between a comment and the brace or bracket behind it), and how it collapses runs of blank lines. The phase runs after the line-break and switch-brace phases (FMT-PIPE-001), so it decides every blank line on the lines the output keeps. What counts as a blank line, the closing-brace and terminal-`break` rules, and the region-directive rules are shared with the analyzer and specified in SHARED-BLANK.

## Non-goals

- Blank lines between type members. The phase neither inserts nor removes them; it only collapses runs (FMT-BLANK-014) and separates comments (FMT-BLANK-011).
- Line breaks that are not blank lines — braces, statements, switch sections, and members on their own lines are FMT-BREAK. This phase writes no line break that ends the line of a statement or a switch section: every blank line it inserts goes between lines that the line-break phase has already broken (FMT-PIPE-025).
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

The formatter MUST insert a blank line before a `break` statement that is not the first statement of its block or switch section and not exempt under SHARED-BLANK-004, and after a `break` statement that is followed by another statement in the same list. It MUST insert a blank line before a switch section whose preceding section ends in a `break` statement, unless a directive directly precedes the section (FMT-BLANK-006); a section ending in any other statement gets none. Every section starts its own line before this phase runs (FMT-BREAK-033), so the phase adds only the blank line in front of it.

Why: `break` leaves the enclosing loop or switch, so it is set off from the work before it, and code after it is a new path. Sections that end in `break` are the common case and are separated so each case reads as its own unit; sections ending in `return` or `throw` get no blank line between them because only the `break` rule (RH5011) asks for the line, and they still start their own lines (FMT-BREAK-033). The line break in front of a section is the line-break phase's, which runs before this phase and inserts no blank line (FMT-BREAK-030); this phase only adds the blank line, in the same run (FMT-PIPE-025).

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

A section on the line of the `break` statement gets its own line (FMT-BREAK-033) and the blank line in the same run:

```csharp
class C
{
    void M(int a)
    {
        switch (a)
        {
            case 1:
                a++;
                break; case 2:
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
                break;
        }
    }
}
```

### FMT-BLANK-005 — The blank line after a closing brace goes behind a directive

The formatter MUST apply SHARED-BLANK-003. When directives stand between the closing brace and the next statement, it MUST insert the blank line after the last of those directives: directly before the statement, or, when comments stand between that directive and the statement, in front of those comments (FMT-BLANK-016). A pair the author wrote on one line stands on separate lines when this phase runs (FMT-BREAK-006) and gets the blank line like any other pair.

Why: SHARED-BLANK-003 has no directive exemption, but a blank line inserted at the start of the gap would land inside the conditional region the directive opens or closes. Placing it after the last directive keeps it outside.

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

### FMT-BLANK-006 — A directive directly before a statement or a switch section suppresses its blank line

The formatter MUST NOT insert the blank line of FMT-BLANK-001 to FMT-BLANK-004 before a statement whose first token is directly preceded by a preprocessor directive, and MUST NOT insert the blank line FMT-BLANK-004 asks for in front of a switch section whose first token is directly preceded by a preprocessor directive. A token is directly preceded by a directive when the last trivia in front of it, apart from whitespace and line breaks, is a directive; a whole conditional group, with the disabled text between its directives, ends with its `#endif` and so counts as directly preceding. A comment between the last directive and the token means the token is not directly preceded: the blank line is inserted, after the last directive (FMT-BLANK-016).

Why: The directive already separates the statement or section from what precedes it, and a blank line inserted in front of the directive would change which code the author grouped with the condition. The analyzer rules apply the same exemption; RH5011 does not report a `break` whose next trivia is a directive (RH5011-002), in front of a statement and in front of a section alike. SHARED-BLANK-003 is not affected (FMT-BLANK-005).

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

A directive, or a whole conditional group whose branch the build does not compile, directly in front of a switch section behind a section that ends in `break` gets no blank line, so this switch is left unchanged:

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
#pragma warning disable CS0168
            case 2:
                a--;
                break;
#if NEVER
            case 3:
                break;
#endif
            case 4:
                a++;
                break;
        }
    }
}
```

With a comment between the directive and the section, the section is not directly preceded by the directive, and the blank line goes after the directive, in front of the comment (FMT-BLANK-016):

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
#pragma warning disable CS0168
            // two
            case 2:
                a--;
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
#pragma warning disable CS0168

            // two
            case 2:
                a--;
                break;
        }
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

When a non-region directive or disabled text stands in the gap before a brace or a chained keyword covered by FMT-BLANK-007 or FMT-BLANK-008 — not the opening brace of a block statement that FMT-BLANK-017 names — or before a closing bracket or parenthesis covered by FMT-BLANK-019, the formatter MUST keep the blank line before the directive, collapsed to one, and MUST remove the blank lines between the last directive and the token when nothing but whitespace stands between the two. When a comment stands between the last directive and the token, the blank lines between the directive and the comment are kept, and FMT-BLANK-020 decides the ones between the comment and the token. The formatter MUST NOT insert a blank line in front of the first non-region directive or disabled text of such a gap when the author wrote none, also when a region directive ends the gap and FMT-BLANK-013 inserts the blank line between the two; this holds before a closing brace, the closing bracket of a collection expression or list pattern, and the closing parenthesis of a parenthesized pattern alike. In front of a directive that stands directly below a region directive, FMT-BLANK-013 decides (SHARED-BLANK-006).

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

When a region directive ends the gap, no blank line is inserted in front of the first directive; the blank line between the two is the region's, kept in the type body and inserted in the method body. The same holds in front of the `]` of a collection expression and the `)` of a parenthesized pattern:

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

The formatter MUST remove the blank lines between a documentation comment and the next line of its token's leading trivia, whether that line holds the documented code or another comment. When that next line is an active `#region` or `#endregion` directive, the formatter MUST NOT remove the blank line that SHARED-BLANK-005 requires above it: the gap keeps one blank line, as FMT-BLANK-013 states.

Why: A documentation comment belongs to the code below it; a blank line would visually detach it. The formatter mirrors RH8302. The blank line above a region directive belongs to the directive, which RH8302 does not report either; it has one owner, FMT-BLANK-013, so the result does not depend on which of the two requirements is applied last.

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

Example — a region directive keeps its blank line above it:

```csharp
class C
{
    int _a;

    /// <summary>
    /// Doc
    /// </summary>
    #region A

    void M()
    {
    }

    /// <summary>
    /// Doc
    /// </summary>


    #endregion // A

    int B;
}
```

```csharp
class C
{
    int _a;

    /// <summary>
    /// Doc
    /// </summary>

    #region A

    void M()
    {
    }

    /// <summary>
    /// Doc
    /// </summary>

    #endregion // A

    int B;
}
```

### FMT-BLANK-011 — A comment on its own line is preceded by exactly one blank line

The formatter MUST ensure exactly one blank line before the first comment in a token's leading trivia that is not glued to the token — a comment on its own line, or a documentation comment that starts the token's line, in the positions FMT-BLANK-018 classifies every comment into (SHARED-BLANK-002) — counted as SHARED-BLANK-001 defines it. It MUST NOT insert one when the comment is the first content of a block, a type body, or a switch section, or when a preprocessor directive directly precedes the comment. When the first comment of the gap is glued to the token, whatever that token is, this requirement inserts no blank line in the gap; FMT-BLANK-018 decides the blank lines above that comment. This applies to ordinary and documentation comments alike, including a comment before a closing brace. A comment that leads a using directive of the same group as the directive above it is no exception; the using rules accept the blank line above it (SHARED-USING-011).

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

A comment that leads a using directive of the same group as the directive above it, with or without a blank line in the input, a single-line comment and a documentation comment alike:

```csharp
using System;
// About Linq
using System.Linq;

/** About Text */
using System.Text;

class C
{
    int x;
}
```

```csharp
using System;

// About Linq
using System.Linq;

/** About Text */
using System.Text;

class C
{
    int x;
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

When the formatter inserts a blank line that FMT-BLANK-001 to FMT-BLANK-005 ask for into a gap that holds a preprocessor directive, it MUST insert the blank line after the last directive of the gap, in front of the comments that stand between that directive and the code behind it, and it MUST NOT insert a blank line directly in front of a directive. This holds for every gap in front of a statement or a switch section, whichever of these requirements asks for the blank line: the gap in front of a compound or jump statement (FMT-BLANK-001), a local declaration or the statement behind one (FMT-BLANK-002), a `yield` statement (FMT-BLANK-003), a `break` statement, the statement behind a `break`, and the switch section behind a section that ends in `break` (FMT-BLANK-004), and the statement behind a closing brace (FMT-BLANK-005), also when the gap holds a whole conditional group, with the disabled text between its directives. Whether such a gap needs the blank line at all is decided by the requirement that asks for it, together with FMT-BLANK-006 for a directive directly in front of a statement or a switch section. This requirement covers only the blank lines the formatter inserts; a blank line the author wrote in front of a directive is decided by the other requirements, such as FMT-BLANK-009.

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

A gap that holds a conditional group whose branch the build does not compile gets the blank line after its `#endif`, in front of the comment that leads the `if` statement (FMT-BLANK-001):

```csharp
class C
{
    void M(int a)
    {
        a++;
#if NEVER
        a--;
#endif
        // why
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
        a++;
#if NEVER
        a--;
#endif

        // why
        if (a > 0)
        {
        }
    }
}
```

The blank line in front of a local declaration (FMT-BLANK-002) and the one behind a `break` (FMT-BLANK-004) go after the directive, in front of the comment that leads the statement; the `while` statement directly behind `#endif` gets none (FMT-BLANK-006):

```csharp
class C
{
    void M(int a)
    {
        a++;
#if !NEVER
        // setup
        var b = a;
#endif
        while (a > 0)
        {
            break;
#if !NEVER
            // scope
            {
                a++;
            }
#endif
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
#if !NEVER

        // setup
        var b = a;
#endif
        while (a > 0)
        {
            break;
#if !NEVER

            // scope
            {
                a++;
            }
#endif
        }
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

### FMT-BLANK-018 — A comment glued to a token belongs to the token's line

Every comment in the leading trivia of a token stands in exactly one of two positions relative to that token:

- **On the token's line** when no line break stands between the comment and the token other than one inside the text of a comment. A delimited comment that spans several lines belongs to the line it starts on, and the token behind its closing `*/` continues that line (SHARED-BLANK-002, SHARED-COMMENT-012), so a comment that starts on an earlier line and ends on the token's line is on the token's line, and so is every comment in front of it on the line it starts on: in `/* a */ /* x⏎ */ }` both comments are on the line of `}`. Any part of such a comment shares the token's line, counted this way.
- **On its own line** when a line break outside the text of a comment stands between the comment and the token. Such a comment shares its line with no token: the leading trivia starts behind the line break that ends the previous token's line, and a comment that trails the previous token is in that token's trailing trivia, not in this gap.

One case puts a comment on its own line although no such line break stands behind it: the end-of-file token has no text and starts no line, so every comment in front of it is on its own line. Where the formatter breaks the line in front of the token behind the closing `*/` of a comment that spans several lines (SHARED-COMMENT-012), as FMT-BREAK-019 does for a link of a wrapped chain, that line break is the line-break phase's and stands when this phase runs (FMT-PIPE-001), so the comments of that line are on their own line by the rule above; this phase MUST NOT predict that line break (FMT-PIPE-025).

The comments on the token's line are glued to it when the first of them is an ordinary comment. When the first of them is a documentation comment, none of them is glued, and FMT-BLANK-011 applies above it as above a comment on its own line. The formatter MUST NOT insert the blank line of FMT-BLANK-011 above a comment glued to a token, and MUST decide the blank lines above the first glued comment as it decides the blank lines directly before that token:

- where FMT-BLANK-007, FMT-BLANK-008, or FMT-BLANK-019 removes the blank lines directly before the token — every opening and closing brace other than the opening brace of a block statement that follows another statement, the `else`, `catch`, `finally`, and do-statement `while` keywords, the closing bracket of a collection expression or a list pattern, and the closing parenthesis of a parenthesized pattern — it MUST remove the blank lines above the glued comment;
- above the opening brace of a block statement that follows another statement, FMT-BLANK-017 decides, and above the first token of any other statement, the statement requirements decide: the blank line they insert goes above the glued comment, and the author's blank line is kept where they insert none;
- above any other token — the first token of a member declaration, the closing parenthesis of an argument list — no requirement decides the blank lines directly before the token, so none is inserted and the author's are kept, collapsed to one (FMT-BLANK-014).

Which token the comment is glued to decides, and only its kind and the construct it belongs to: never the layout of that construct, and never another comment on the token's line. When a preprocessor directive or disabled text stands in the gap before the comment, this requirement removes no blank line of that gap: the line below a region directive starts with the comment, not with a closing brace, so SHARED-BLANK-006 requires it to be blank and FMT-BLANK-013 inserts it, and every other blank line of such a gap is kept as written, collapsed to one (FMT-BLANK-014).

FMT-BLANK-011 and FMT-BLANK-020 read the same two positions: FMT-BLANK-011 applies above the first comment of a gap unless that comment is glued, and FMT-BLANK-020 applies behind an ordinary comment on its own line, never behind a comment on the token's line. No comment is in both positions, and none is in neither, so no comment shape falls to a requirement that was not written for it.

Why: A comment that shares its line with code is not a comment that introduces the code below it; it belongs to the line of the token it is glued to, so the blank lines above it are the blank lines above that token. A delimited comment that spans lines shares its last line with the token as much as its first: the token behind `*/` does not start a line (SHARED-COMMENT-012), and a classification with a third position would leave a comment to a fallback that no rule chose for it. Such a comment is not wanted: the author is expected to move it by hand — RH5020 keeps reporting it — and the formatter gives it no layout of its own, so the rule names no construct and reads no layout another phase changes. A directive in the gap closes conditional code or a region in front of the glued line; the blank line below a region directive is the region's (SHARED-BLANK-006), and the others are the author's, as they have been in the formatter's output.

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

Above a comment glued to an array initializer's brace or to a chained keyword, the author's blank line is removed, as directly before those tokens; above a comment glued to a statement, a member, or the closing parenthesis of an argument list, none is inserted and the author's is kept:

```csharp
class C
{
    int[] G { get; } =

    /* b */ { 1, 2 };
    int H;
    /* m */ int I;

    void M(int a)
    {
        a++;

        /* s */ a++;
        Foo(1,
            2
            /* e */ );

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

```csharp
class C
{
    int[] G { get; } =
    /* b */ { 1, 2 };
    int H;
    /* m */ int I;

    void M(int a)
    {
        a++;

        /* s */ a++;
        Foo(1,
            2
            /* e */ );

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

The opening brace of a complex element initializer is decided like any other opening brace, whether the element stays on one line or spans several:

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

So is the closing brace of a collection initializer, whether or not it is assigned to a member:

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

A documentation comment between the glued comment and the brace does not change what the comment is glued to; a documentation comment that starts the line is not glued, and FMT-BLANK-011 separates it:

```csharp
public class TestClass

/* x */ /** Doc */ {
}

public class TestClass2
/** Doc */ {
}
```

```csharp
public class TestClass
/* x */ /** Doc */ {
}

public class TestClass2

/** Doc */ {
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

Above a comment glued to a block statement's brace, FMT-BLANK-017 decides, as above the brace itself: after a statement that ends with `}`, the blank line of SHARED-BLANK-003 goes above the comment, and after any other statement the author's blank line is kept:

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

A delimited comment that starts on an earlier line and ends on the token's line is glued like one that starts and ends there, before a statement as before a brace, and so is a comment in front of it on its first line. A comment in front of a chain link that the formatter moves onto a line of its own, and a comment in front of the end-of-file token, are on their own line; FMT-BLANK-011 inserts the blank line above both:

```csharp
class C
{
    void M(dynamic a)
    {
        A();
        /* a */ B();
        /* b
         */ B();
        var r = a.B()
                 .C()
                 /* c
                  */ .D();
    }

    void N(int a)

    /* d
     */ {
        a++;

        /* e */ /* f
         */ }
}
// trailing
```

```csharp
class C
{
    void M(dynamic a)
    {
        A();
        /* a */ B();
        /* b
         */ B();

        var r = a.B()
                 .C()

                 /* c
                  */
                 .D();
    }

    void N(int a)
    /* d
     */ {
        a++;
        /* e */ /* f
         */ }
}

// trailing
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

### FMT-BLANK-020 — Behind a comment on its own line, no blank line before a brace, closing bracket, or chained keyword

When the last content in the gap before one of the following tokens is an ordinary comment on its own line, as FMT-BLANK-018 classifies it, the formatter MUST remove the blank lines between that comment and the token:

- an opening or closing brace, whatever construct it belongs to — a type, a block, an accessor list, a switch statement, an initializer of any kind, an array initializer and a nested element of one included, an anonymous object, a switch expression, a property pattern, an extension block, and the closing brace of an interpolation — other than the opening brace of a block statement that follows another statement, above which FMT-BLANK-017 decides the blank lines, comments included;
- the `else`, `catch`, `finally`, and do-statement `while` keywords;
- the closing bracket of a collection expression or a list pattern;
- the closing parenthesis of a parenthesized pattern.

These are the tokens FMT-BLANK-007, FMT-BLANK-008, and FMT-BLANK-019 remove a blank line directly in front of.

What stands in front of the comment on its line does not matter: a line that holds `/* a */ /* b */`, `/** d */ // c`, or the end of a multi-line comment followed by `// c` ends with an ordinary comment like a line that holds `// c` alone. The kind of the token decides, and for an opening brace whether it opens a block statement that follows another statement, never the layout of the construct: the opening brace of a complex element initializer is named whether the element stays on one line or spans several, and both braces of a collection initializer assigned to a member are named like the braces of any other collection initializer. The blank lines above that comment are decided by the other requirements, such as FMT-BLANK-011 for the first comment of the gap. A comment that trails the previous token on its line is not in the gap's own lines; the blank lines behind it are directly before the token (FMT-BLANK-007, FMT-BLANK-008). A comment on the token's line is FMT-BLANK-018's, also a delimited comment that starts on an earlier line and ends on the token's line (`// c⏎⏎/* x⏎ */ }`: FMT-BLANK-018 removes the blank line above `/* x`), and a documentation comment that ends its line is FMT-BLANK-010's.

Why: The comment describes the token below it, or the end of the construct the token closes; a blank line between the two detaches it, as a blank line directly before the token would (FMT-BLANK-007, FMT-BLANK-008, FMT-BLANK-019). The formatter mirrors RH5024, RH5025, RH5026, and RH5027, which report such a blank line before a closing brace, an opening brace, `else`, `catch`, or `finally`, and a do-statement's `while`, whatever construct the brace belongs to; one set of tokens for the blank line directly in front of them and behind a comment keeps the formatter's output free of their reports. The set is decided by syntax alone, because a layout that the line-break phase changes later cannot decide a blank line the blank-line phase writes.

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

Behind two comments on a line of their own, behind a comment above the opening brace of a collection initializer assigned to a member, and behind a comment above the opening brace of a complex element initializer that stays on one line. FMT-BLANK-011 puts a blank line above the comment that follows the `=` of the assignment, as above any other comment that starts its own line:

```csharp
class C
{
    void M(int a)
    {
        var p = new P
                {
                    L =
                    // c

                    {
                        1
                    }
                };
        var d = new Dictionary<int, int>
                {
                    { 1, 2 },

                    // e

                    { 3, 4 }
                };
        a++;

        /* a */ /* b */

    }
}
```

```csharp
class C
{
    void M(int a)
    {
        var p = new P
                {
                    L =

                    // c
                    {
                        1
                    }
                };
        var d = new Dictionary<int, int>
                {
                    { 1, 2 },

                    // e
                    { 3, 4 }
                };

        a++;

        /* a */ /* b */
    }
}
```

Behind a comment on its own line in front of the braces of an array initializer, a nested element of one, an extension block, a property pattern, and a collection initializer assigned to a member, in front of the closing brace of an interpolation, and in front of `else`, `catch`, `finally`, and a do-statement's `while`. FMT-BLANK-011 puts a blank line above each comment, as above any other comment that starts its own line:

```csharp
using System.Collections.Generic;

static class E
{
    extension(int i)
    // g

    {
        public int D => i;
        // h

    }
}

class P
{
    public List<int> L { get; } = new();
}

class C
{
    int[] G =
    {
        1
        // d

    };

    int[,] H =
    {
        { 1 },
        // e

        { 2 }
    };

    void M(object o, int a)
    {
        int[] g;
        g = new int[]
            // c

            {
                1,
                2
                // d

            };

        if (o is C
            // e

            { })
        {
        }

        var p = new P
                {
                    L =
                    {
                        1
                        // d

                    }
                };

        var s = $$"""
            {{
                a
                // f

            }}
            """;

        if (a > 0)
        {
            a++;
        }
        // c

        else
        {
            a--;
        }

        try
        {
            a++;
        }
        // t

        catch
        {
        }
        // u

        finally
        {
        }

        do
        {
            a++;
        }
        // f

        while (a > 0);
    }
}
```

```csharp
using System.Collections.Generic;

static class E
{
    extension(int i)

    // g
    {
        public int D => i;

        // h
    }
}

class P
{
    public List<int> L { get; } = new();
}

class C
{
    int[] G = {
                  1

                  // d
              };

    int[,] H = {
                   { 1 },

                   // e
                   { 2 }
               };

    void M(object o, int a)
    {
        int[] g;
        g = new int[]

            // c
            {
                1,
                2

                // d
            };

        if (o is C

        // e
        { })
        {
        }

        var p = new P
                {
                    L = {
                            1

                            // d
                        }
                };

        var s = $$"""
                  {{
                      a

                      // f
                  }}
                  """;

        if (a > 0)
        {
            a++;
        }

        // c
        else
        {
            a--;
        }

        try
        {
            a++;
        }

        // t
        catch
        {
        }

        // u
        finally
        {
        }

        do
        {
            a++;
        }

        // f
        while (a > 0);
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
