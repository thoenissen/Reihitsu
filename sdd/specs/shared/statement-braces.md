# Shared — Statement braces

| | |
|---|---|
| ID prefix | SHARED-BRACE |
| Status | Active |
| Code owners | `StatementBracesCodeFixProviderBase` |
| Uses | FMT-STRUCT-001, SHARED-FIX-003, SHARED-FIX-006, SHARED-FIX-007, SHARED-FIX-008 |

This spec covers what the rules that report an `if` or `else` body without braces share: which bodies they check, and what their code fix produces. Each rule that uses this spec states which of those bodies it reports.

## Non-goals

- Which unbraced bodies a rule reports — each rule's spec, such as RH5405, RH5406, and RH5407.
- The layout of a body that already has braces — SHARED-BLOCK-001.

## Requirements

### SHARED-BRACE-001 — The bodies of `if` and `else` are checked

A rule that uses this requirement MUST check the body of an `if` statement and the body of its `else` clause, except an `else` whose body is itself an `if` statement: an `else if` is checked as the `if` statement it is, so each body of an `else if` chain is checked on its own. It MUST NOT check the body of a `while`, `do`, `for`, `foreach`, `using`, `lock`, or `fixed` statement.

Why: An `if` or `else` body without braces invites the classic error of adding a second statement that only looks guarded. The `else if` exception keeps a chain flat, as the formatter does (FMT-STRUCT-001).

Example — RH5405, which reports a body without braces on one line:

```csharp
class C
{
    void M(bool a, bool b)
    {
        if (a)
            {|RH5405:M(a, b);|}
        else if (b)
            {|RH5405:M(a, b);|}
        else
            {|RH5405:M(a, b);|}
        while (a)
            M(a, b);
    }
}
```

### SHARED-BRACE-002 — The fix wraps the body in braces

Where SHARED-BRACE-003 does not withhold it, a code fix that uses this requirement MUST wrap the reported body in a block and MUST change nothing else (SHARED-FIX-006): the `if` or `else` that owns the body, with its condition and the comments behind it, the other body of the same `if` statement, whether it has braces or not, and the blank lines around the statement stay as written, and the fix MUST NOT insert a blank line in front of the statement. An `else` that stands behind a closing brace on its line stays there.

The braces the fix creates are laid out as the formatter lays them out (SHARED-FIX-007, FMT-STRUCT-001): each starts its own line, in the column of the first token of the line on which the `if` or `else` that owns the body starts. That is the column of the `if` for an `if` statement that starts its line, the column of the `else` for the body of an `else` that starts its line and for the body of an `else if` whose `else` starts its line, and the column of the closing brace for an `else` behind it. The whitespace in front of each brace is a copy of the whitespace in front of that first token.

The body is code the fix wraps into a new block (SHARED-FIX-007, *Moved code*): the spacing between its tokens, its line breaks, and the blank lines inside it stay as written, also where formatting the result would change them. Which of its lines move depends on where it starts relative to the header's last line, the line of the token that ends the header of the `if` or `else` that owns the body — the closing parenthesis of the condition, or the `else`. That is the line of the `if` or `else` keyword when the header stands on one line, and the line of the closing parenthesis when the condition spans several lines. The body starts below the header's last line when a line break outside a comment stands between that token and the body's first token, and on the header's last line otherwise: a line break inside a delimited comment between them does not count, so in `if (a) /* x` ⏎ `y */ M(a, b);` the body starts on the header's last line, and the comment stays behind the closing parenthesis. The braces take their column from the line on which the `if` or `else` starts, as stated above, wherever the header ends.

- **On the header's last line.** The body is one line (SHARED-BRACE-003 withholds the fix for a body there that spans several lines). It moves onto the line below the opening brace, its first token one indentation level, four columns, right of the braces, written as spaces behind the braces' whitespace.
- **Below the header's last line.** The body keeps every line as written, in its column, whatever its indentation: also a body whose first token does not stand one indentation level, four columns, right of the braces' column keeps that column, and a body that spans several lines keeps its continuation lines in their columns. Blank lines directly below the header's last line are removed, so the opening brace takes the line below it. From the first line that holds a comment or the body on, every line stays as written: a comment line above the body stays inside the block, above the body, and a blank line between that comment and the body stays.

The end of the body's last line is decided as follows. A comment behind the body on its last line stays there, inside the block, and is written once: the fix MUST NOT repeat it behind the closing brace (SHARED-FIX-003). Whitespace at the end of the body's last line stays there, and the fix MUST NOT write whitespace behind the closing brace. When other code follows the body, and the comments behind it, on that line:

- **The `else` of the same `if` statement** stays on that line behind the closing brace, separated from it by one space, with what follows it.
- **Any other code**, such as the next statement, starts its own line below the closing brace, in the column of the closing brace. The whitespace in front of it on the body's line is not kept.

When nothing follows the body's last line, because the body, with the comments behind it, ends the document without a line break behind it, the fix is offered as for any other body: it creates the line break in front of the closing brace, and it writes none behind it, so the document still ends without a line break. This is the case for a top-level statement at the end of a file without a final line break.

Every line break the fix creates MUST be the line ending SHARED-FIX-008 specifies. The line breaks the fix keeps are moved code; SHARED-BRACE-003 withholds the fix where one of them is another line ending.

Why: The diagnostic names the body, so wrapping it is the whole correction; the header, the other branch, and the blank lines around the statement are a layout the author chose (SHARED-FIX-006). The braces are new code, so they get the layout the formatter gives them, in one application, and code that would stand behind a brace the fix created goes onto the line the formatter gives it, except the `else`, which the fix must not move. The body is code the author wrote and laid out; reformatting it on the way would hide the braces in a diff of layout changes nobody asked for, while the formatter and the layout rules still correct it on their own. A blank line directly below the header would stand behind the new opening brace, where the formatter lays out none. Formatting the document keeps a comment behind the body with the statement it describes; a second copy behind the closing brace would change the text the author wrote. Whitespace at the end of the body's line belongs to that line, which the fix moves as written; the closing brace is new code and gets none.

Example — RH5405, a body on the line of its `if`:

```csharp
class C
{
    void M(bool a, bool b)
    {
        if (a) {|RH5405:M(a, b);|}
    }
}
```

```csharp
class C
{
    void M(bool a, bool b)
    {
        if (a)
        {
            M(a, b);
        }
    }
}
```

Example — RH5405, the header, the body, and the blank lines stay as written:

```csharp
class C
{
    void M(bool a, int b)
    {
        b++;
        if ( a )
            {|RH5405:M(a,b);|}
    }
}
```

```csharp
class C
{
    void M(bool a, int b)
    {
        b++;
        if ( a )
        {
            M(a,b);
        }
    }
}
```

No blank line is inserted between `b++;` and the `if`, the spaces inside `( a )` stay, and the body keeps `M(a,b)`, although formatting the result would change all three.

Example — RH5405, a header that spans several lines:

```csharp
class C
{
    void M(bool a, bool b)
    {
        if (a &&
            b) {|RH5405:M(a, b);|}

        if (a &&
            b)
            {|RH5405:M(a, b);|}
    }
}
```

```csharp
class C
{
    void M(bool a, bool b)
    {
        if (a &&
            b)
        {
            M(a, b);
        }

        if (a &&
            b)
        {
            M(a, b);
        }
    }
}
```

In the first statement, the body starts on the header's last line, the line of `b)`, so it moves below the opening brace, although it starts on a later line than the `if` keyword. In the second, the body starts below `b)` and keeps its line. In both, the braces start the line below `b)`, in the column of the `if`, and the condition keeps its lines.

Example — RH5406, a body below its `if` that spans several lines keeps its lines:

```csharp
class C
{
    void M(bool a, int b)
    {
        if (a)
            {|RH5406:M(a,
                b);|}
    }
}
```

```csharp
class C
{
    void M(bool a, int b)
    {
        if (a)
        {
            M(a,
                b);
        }
    }
}
```

The continuation line keeps column 16, although formatting the result would align `b` under `a`.

Example — RH5405, a body below its `if` that is not indented one level deeper keeps its column:

```csharp
class C
{
    void M(bool a, int b)
    {
        if (a)
          {|RH5405:M(a, b);|}
    }
}
```

```csharp
class C
{
    void M(bool a, int b)
    {
        if (a)
        {
          M(a, b);
        }
    }
}
```

The body stays in column 10, although formatting the result would move it to column 12; the layout rules still report it. A body that spans several lines keeps every line likewise: `M(a,` in column 10 with `b);` in column 14 below it keeps both columns.

Example — RH5405, the other body stays as written:

```csharp
class C
{
    void M(bool a, int b)
    {
        if (a)
            {|RH5405:M(a,b);|}
        else
            M(a,
              b);
    }
}
```

```csharp
class C
{
    void M(bool a, int b)
    {
        if (a)
        {
            M(a,b);
        }
        else
            M(a,
              b);
    }
}
```

The `else` body, which RH5406 reports, keeps its shape and gets no braces from this fix.

Example — RH5405, the body of an `else if`:

```csharp
class C
{
    void M(bool a, bool b)
    {
        if (a)
        {
            M(a, b);
        }
        else if (b)
            {|RH5405:M(a, b);|}
    }
}
```

```csharp
class C
{
    void M(bool a, bool b)
    {
        if (a)
        {
            M(a, b);
        }
        else if (b)
        {
            M(a, b);
        }
    }
}
```

The braces are in the column of the `else`, not in the column of the inner `if`.

Example — RH5407, an `else` behind a closing brace:

```csharp
class C
{
    void M(bool a, bool b)
    {
        if (a)
        {
            M(a, b);
        } else {|RH5407:M(a, b);|}
    }
}
```

```csharp
class C
{
    void M(bool a, bool b)
    {
        if (a)
        {
            M(a, b);
        } else
        {
            M(a, b);
        }
    }
}
```

The `else` stays behind the closing brace, and the new braces are in the column of that brace.

Example — RH5405, an `else` behind the body on its line:

```csharp
class C
{
    void M(bool a, bool b)
    {
        if (a) {|RH5405:M(a, b);|} else {|RH5405:M(a, b);|}
    }
}
```

```csharp
class C
{
    void M(bool a, bool b)
    {
        if (a)
        {
            M(a, b);
        } else M(a, b);
    }
}
```

The fix is applied to the `if` body. The `else` stays on its line, behind the new closing brace, and its body, which RH5405 reports on its own, is not changed. With the body below the `if`, `if (a)` ⏎ `    M(a, b); else M(a, b);`, the result is the same.

Example — RH5405, a comment behind the body:

```csharp
class C
{
    void M(bool a, int b)
    {
        if (a)
            {|RH5405:M(a, b);|} // trail
        b++;
    }
}
```

```csharp
class C
{
    void M(bool a, int b)
    {
        if (a)
        {
            M(a, b); // trail
        }
        b++;
    }
}
```

Example — RH5405, a statement behind the body on its line:

```csharp
class C
{
    void M(bool a, int b)
    {
        if (a) {|RH5405:M(a, b);|} /* c */ b++;
    }
}
```

```csharp
class C
{
    void M(bool a, int b)
    {
        if (a)
        {
            M(a, b); /* c */
        }
        b++;
    }
}
```

The comment stays behind the body, and `b++;` starts its own line in the column of the closing brace. With the body below the `if`, `if (a)` ⏎ `    M(a, b); b++;`, `b++;` likewise starts the line below the closing brace.

Example — RH5405, blank lines and a comment between the `if` and its body:

```csharp
class C
{
    void M(bool a, int b)
    {
        if (a)

            // c

            {|RH5405:M(a, b);|}
    }
}
```

```csharp
class C
{
    void M(bool a, int b)
    {
        if (a)
        {
            // c

            M(a, b);
        }
    }
}
```

The blank line below `if (a)` is removed; the comment and the blank line below it stay. Without the comment, both blank lines stand directly below `if (a)` and are removed.

Example — RH5405, whitespace at the end of the body's line: in the input below, `·` stands for a space.

```csharp
class C
{
    void M(bool a, int b)
    {
        if (a)
            {|RH5405:M(a, b);|} /* x */···
    }
}
```

```csharp
class C
{
    void M(bool a, int b)
    {
        if (a)
        {
            M(a, b); /* x */···
        }
    }
}
```

The three spaces stay at the end of the body's line, and nothing stands behind the closing brace. The same holds without the comment, for `M(a, b);···`, and for a body on the line of its `if`, `if (a) M(a, b);···`; RH5406 and RH5407 behave alike.

Example — RH5405, a body that ends the document, in a file of top-level statements without a final line break:

```csharp
if (args.Length > 0) {|RH5405:System.Console.WriteLine();|}
```

```csharp
if (args.Length > 0)
{
    System.Console.WriteLine();
}
```

No line break stands behind the closing brace, as none stood behind the body. Likewise, `if (args.Length > 0)` ⏎ `    System.Console.WriteLine(); // c` at the end of such a file becomes the same statement with `// c` behind the body, inside the block, and the closing brace on the last line, with nothing behind it.

Example — line endings: in a document whose line breaks are all CRLF, every line break of the result of the first example is CRLF, the three the fix creates included; in a document whose line breaks are all LF, every one is LF. In a document whose line breaks are all a lone `\r`, or whose line breaks mix line endings, SHARED-BRACE-003 decides whether the fix is offered.

### SHARED-BRACE-003 — The fix is withheld where it cannot wrap the body as written

A code fix that uses this requirement MUST NOT be offered for a reported body in each of the following cases. The diagnostic is still reported.

- **A body on the header's last line that spans several lines.** The body starts on the last line of the header of the `if` or `else` that owns it, as SHARED-BRACE-002 decides it, and it ends on a later line, counting a comment behind its last token on that line: a delimited comment behind the body that continues onto a later line makes it span several lines. A body below the header's last line keeps the fix, whatever number of lines it spans, also when the header itself spans several lines, and so does a body on that line that ends on it, with a single-line comment or a delimited comment closed on that line behind it.
- **Other code in front of the `if` or `else` on its line.** Something other than whitespace stands in front of the `if` or `else` that owns the body on the line on which it starts, and it is not an `else` whose clause holds that `if` (an `else if`), nor a closing brace that ends the other body of the same `if` statement and stands directly in front of that `else`, with only whitespace between them. Such code is a statement, a `case` label, an opening brace, the header of an `if` or loop whose body the `if` is, the body in front of an `else`, or a comment.
- **A directive or disabled text between the header and the body.** A preprocessor directive or disabled text stands between the token that ends the header of the `if` or `else` that owns the body — the closing parenthesis of the condition, or the `else` — and the body's first token. A directive behind the body, in front of the next statement, and a directive between two lines of a body below its header do not withhold the fix.
- **A tab in front of the `if` or `else`.** The whitespace in front of the first token of the line on which the `if` or `else` that owns the body starts holds a tab. A tab elsewhere — between two tokens of a line, or inside a comment or a string — does not withhold the fix.
- **Another line ending in the code the fix moves.** The code from the end of the token that ends the header through the line break that ends the body's last line, that line break included, holds a line break other than the line ending SHARED-FIX-008 detects for the document, also one inside a literal or inside the text of a comment (SHARED-FIX-008, second paragraph). In a document whose line breaks are all a lone `\r`, the detected line ending is `\n`, so the fix is withheld for every body that a line break ends. A line break in another line ending outside that code, such as behind the other body, does not withhold the fix, and a document whose line breaks are all LF, or all CRLF, keeps it.

Why: Each case is a rare input that the fix could only correct with more than a cheap check (`AGENTS.md`, *Special cases do not grow the source*). The continuation lines of a body that moves off the line of its `if` have no column that keeps both common continuation styles as written, and the formatter's columns would need a formatting mode that lays out only columns. Code in front of the `if` or `else` is something the formatter moves onto another line, so the braces cannot be laid out as the formatter lays them out without moving the header, which SHARED-FIX-006 forbids. A directive between the header and the body governs code that the braces would enclose or leave out, and the formatter does not brace such a body either. A tab in front of the header has no column the braces could copy as spaces, and the layout rules report it. The body moves as written, without the formatter, as SHARED-FIX-008 states for such moves.

Example — RH5406, a body on the line of its `if` that spans several lines:

```csharp
class C
{
    void M(bool a, int b)
    {
        if (a) {|RH5406:M(a,
                 b);|}
    }
}
```

The diagnostic is reported, and no code fix is offered. Below its `if`, the same body keeps the fix (SHARED-BRACE-002). With RH5405, `if (a) {|RH5405:M(a, b);|} /* one` ⏎ `two */` is reported and no code fix is offered, while `if (a) {|RH5405:M(a, b);|} /* one */` gets the fix.

Example — RH5405, code in front of the `if` on its line:

```csharp
class C
{
    void M(bool a, bool b, int i)
    {
        b = a; if (a) {|RH5405:M(a, b, i);|}
        if (b) {|RH5405:if (a) {|RH5405:M(a, b, i);|}|}
        switch (i)
        {
            case 1: if (a) {|RH5405:M(a, b, i);|}
                break;
        }
    }
}
```

The diagnostic on each body of an inner `if` is reported, and no code fix is offered for it. The body of `if (b)`, `if (a) M(a, b, i);`, is reported on its own and gets the fix, because `if (b)` starts its line:

```csharp
        if (b)
        {
            if (a) M(a, b, i);
        }
```

after which the inner `if` starts its line, and its body gets the fix too. Likewise, in `if (a) M(a, b, i); else {|RH5405:M(a, b, i);|}` the `else` body gets no fix until the `if` body is wrapped and the `else` stands behind its closing brace.

Example — RH5405, a directive between the `if` and its body:

```csharp
class C
{
    void M(bool a, int b)
    {
        if (a)
#pragma warning disable CS0618
            {|RH5405:M(a, b);|}
#pragma warning restore CS0618
    }
}
```

The diagnostic is reported, and no code fix is offered. The `#pragma warning restore` behind the body alone does not withhold the fix; `#region` and `#endregion` around the body withhold it as `#pragma` does.

Example — RH5405, a tab in front of the `if`: in the input below, `→` stands for a tab.

```csharp
class C
{
→void M(bool a, int b)
→{
→→if (a)
→→→{|RH5405:M(a, b);|}
→}
}
```

The diagnostic is reported, and no code fix is offered.

Example — line endings: in a document whose line breaks are CRLF, except the line break behind `if (a)`, which is LF, the RH5405 fix for

```csharp
        if (a)
            {|RH5405:M(a, b);|}
```

is not offered, because that LF lies in the code the fix moves. With that LF behind `b++;` on a line above the `if` instead, the fix is offered, and every line break it creates is CRLF. In a document whose line breaks are all a lone `\r`, the fix is not offered for that input, nor for the first example of SHARED-BRACE-002.
