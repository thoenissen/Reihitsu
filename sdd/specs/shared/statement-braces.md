# Shared — Statement braces

| | |
|---|---|
| ID prefix | SHARED-BRACE |
| Status | Active |
| Code owners | `StatementBracesCodeFixProviderBase` |
| Uses | FMT-STRUCT-001, SHARED-FIX-003, SHARED-FIX-006, SHARED-FIX-007 |

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

A code fix that uses this requirement MUST wrap the reported body, and only that body, in a block:

- **The braces.** The braces the fix creates are laid out as formatting the document lays them out (SHARED-FIX-007): each starts its own line, in the column of the first token on the line of the `if` or `else` that owns the body. When that `if` statement starts its line, that is the column of the `if`; for the body of an `else if` that is the column of `else`.
- **The body.** The body is code the fix moves into the new block (SHARED-FIX-007, moved code). It keeps its own layout — the spacing between its tokens and its line breaks — and only the indentation of its lines follows the block, one level inside the braces. A comment that stands with the body (SHARED-FIX-003), such as a comment behind it at the end of its line or a comment that starts the line above it, moves into the block with it and appears there once.
- **Everything else.** The rest of the `if` statement — its condition, a comment behind the condition, and a body of another branch that has no braces — and the blank lines above and below the statement stay as written (SHARED-FIX-006). A body of another branch keeps its own diagnostic and its own fix.

Why: The braces are the code the diagnostic asks for, so the formatter's layout of them is the result, and formatting it changes nothing (SHARED-FIX-007). The body is the author's code that only moves into the block; reformatting it, inserting or removing a blank line around the statement, or bracing another branch would hide the correction in changes nobody asked for (SHARED-FIX-006). A comment written behind the body describes that statement and travels with it; writing it a second time behind the closing brace would change the text the author wrote (SHARED-FIX-003).

Example — RH5405:

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

Example — RH5405, a comment behind the body, a body with its own spacing, and a blank line the author left out:

```csharp
class C
{
    void M(bool a, int b)
    {
        if (a)
            {|RH5405:M(a, b);|} // trail
        var c = b;
        if (a) {|RH5405:M( a,b );|}
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
        var c = b;
        if (a)
        {
            M( a,b );
        }
    }
}
```

The comment stays behind `M(a, b);` and is not repeated behind the closing brace. `M( a,b );` keeps its spacing, and no blank line is inserted between `var c = b;` and the `if` statement.

Example — RH5405, the fix of the `if` body leaves the `else` body, and the braces of an `else if` body stand in the column of `else`:

```csharp
class C
{
    void M(bool a, bool b)
    {
        if (a)
            {|RH5405:M(a, b);|}
        else
            {|RH5405:M(b, a);|}

        if (a)
        {
            M(a, b);
        }
        else if (b) // note
            {|RH5405:M(b, a);|}
    }
}
```

Applying the fix to the first diagnostic only:

```csharp
class C
{
    void M(bool a, bool b)
    {
        if (a)
        {
            M(a, b);
        }
        else
            M(b, a);

        if (a)
        {
            M(a, b);
        }
        else if (b) // note
            M(b, a);
    }
}
```

Applying the fix to the third diagnostic only:

```csharp
class C
{
    void M(bool a, bool b)
    {
        if (a)
            M(a, b);
        else
            M(b, a);

        if (a)
        {
            M(a, b);
        }
        else if (b) // note
        {
            M(b, a);
        }
    }
}
```
