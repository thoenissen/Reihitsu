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
- The column of the braces for an `if` or `else` that has other code in front of it on its line than an `else` or a closing brace, such as a `case` label.

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

A code fix that uses this requirement MUST wrap the reported body in a block and MUST change nothing else (SHARED-FIX-006): the `if` or `else` that owns the body, with its condition and the comments behind it, the other body of the same `if` statement, whether it has braces or not, and the blank lines around the statement stay as written, and the fix MUST NOT insert a blank line in front of the statement. An `else` that stands behind a closing brace on its line stays there.

The braces the fix creates are laid out as the formatter lays them out (SHARED-FIX-007, FMT-STRUCT-001): each starts its own line, at the column of the first token of the line on which the `if` or `else` that owns the body starts, where nothing but an `else` or a closing brace stands in front of it on that line. That is the column of the `if` for an `if` statement that starts its line, the column of the `else` for the body of an `else` that starts its line and for the body of an `else if` whose `else` starts its line, and the column of the closing brace for an `else` behind it. The body's first token starts the line below the opening brace, one indentation level inside the braces.

The body is code the fix wraps into a new block (SHARED-FIX-007, *Moved code*): the spacing between its tokens, its line breaks, and the blank lines inside it stay as written, whether the body stood on the line of its `if` or `else` or below it, also where formatting the result would change them. A comment behind the body on its last line stays there, inside the block, and is written once: the fix MUST NOT repeat it behind the closing brace (SHARED-FIX-003).

Every line break the fix creates or keeps in the code it changes, from the end of the condition's closing parenthesis or the `else` through the line break behind the closing brace, MUST be the line ending SHARED-FIX-008 specifies, also a line break inside the body outside a literal and outside the text of a comment.

Why: The diagnostic names the body, so wrapping it is the whole correction; the header, the other branch, and the blank lines around the statement are a layout the author chose (SHARED-FIX-006). The braces are new code, so they get the layout the formatter gives them, in one application. The body is code the author wrote and laid out; reformatting it on the way would hide the braces in a diff of layout changes nobody asked for, while the formatter and the layout rules still correct it on their own. Formatting the document keeps a comment behind the body with the statement it describes; a second copy behind the closing brace would change the text the author wrote.

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

Example — line endings: in a document whose line breaks are all CRLF, every line break of the result of the first example is CRLF, the three the fix creates included. In a document whose line breaks are all a lone `\r`, the line breaks the fix creates, and the line break behind the closing brace, are `\n`, and so is the line break behind `if (a)` where the body stood on the line below it; the line breaks outside that span stay a lone `\r`.
