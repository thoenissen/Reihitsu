# Shared — Statement braces

| | |
|---|---|
| ID prefix | SHARED-BRACE |
| Status | Active |
| Code owners | `StatementBracesCodeFixProviderBase` |
| Uses | FMT-STRUCT-001 |

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

A code fix that uses this requirement MUST wrap the reported body in a block by laying out the `if` statement that owns it as the formatter does (FMT-STRUCT-001): the braces start their own lines at the column of the `if`, and the body is indented one level inside them.

Why: The formatter owns the braced layout of a control-flow statement; delegating to it applies that layout instead of a second one, and works the same whether the body was on the line of the `if` or below it.

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
