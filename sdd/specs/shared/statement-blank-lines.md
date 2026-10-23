# Shared — Blank line before a statement

| | |
|---|---|
| ID prefix | SHARED-STMT |
| Status | Active |
| Code owners | `StatementShouldBePrecededByABlankLineAnalyzerBase<TStatement>`, `StatementShouldBePrecededByABlankLineCodeFixProviderBase` |
| Uses | SHARED-BLANK-001, FMT-BLANK-001, SHARED-FIX-008 |

This spec covers the rules that ask for a blank line in front of one kind of statement: when such a statement is reported, which statements are never checked, and what the code fix inserts. Each rule that uses this spec names its statement kind, where its diagnostic is located, and any exception of its own. The formatter's counterpart is FMT-BLANK-001.

## Non-goals

- Which statement kinds a rule checks, where its diagnostic is located, and its own exceptions — each rule's spec.
- A blank line after a statement that ends with a closing brace — SHARED-BLANK-003.
- More than one blank line in a row — RH5023.
- Whether a comment above the statement needs a blank line of its own — RH5020.

## Requirements

### SHARED-STMT-001 — A statement that follows another statement needs a blank line

A rule that uses this spec MUST report a statement of its kind whose parent is a block or a switch section and that is not the first statement there — the token before the statement is neither `{` nor the `:` of a case label — when the trivia between that token and the statement contains no blank line as SHARED-BLANK-001 defines it. A statement on the same line as the one before it is reported too. Comments between the two do not count as a blank line.

Why: The statements these rules name start a new step in the control flow — a decision, a loop, a protected region, a jump; the blank line makes that step visible. The first statement of a block or section needs no separator, because the brace or the case label already is one.

Example — RH5002, for `if` statements:

```csharp
class C
{
    void M(int a)
    {
        if (a > 0)
        {
        }
        a++;
        {|RH5002:if|} (a > 1)
        {
        }
        a++;
        // note
        {|RH5002:if|} (a > 3)
        {
        }
        a++; {|RH5002:if|} (a > 5) { }
        switch (a)
        {
            case 1:
                if (a > 8) { }
                a++;
                {|RH5002:if|} (a > 9) { }
                break;
        }
    }
}
```

### SHARED-STMT-002 — A statement that is not part of a statement list is not checked

A rule that uses this spec MUST NOT report a statement whose parent is not a block or a switch section: a top-level statement, the `if` of an `else if`, and a statement that is the embedded body of another statement, such as an unbraced `if`, `while`, or `for` body.

Why: An `else if` continues the chain of the `if` above it, and an embedded statement is bound to its header; a blank line in either place would tear a single statement apart. The formatter never puts a blank line in front of a top-level statement, so reporting one would leave a diagnostic that formatted code keeps.

Example:

```csharp
class C
{
    void M(int a)
    {
        if (a > 1)
        {
        }
        else if (a > 2)
        {
        }

        while (a > 3)
            if (a > 4)
                a--;
    }
}
```

is not reported by RH5002.

### SHARED-STMT-003 — A directive directly above the statement is a separator

A rule that uses this spec MUST NOT report a statement when the last trivia in front of it, apart from whitespace and line breaks, is a preprocessor directive.

Why: A directive line already sets the statement apart from the code above it, and inserting a blank line between a directive and the code it governs would separate them instead. The formatter makes the same decision.

Example:

```csharp
class C
{
    void M(int a)
    {
        a++;
#if DEBUG
        a--;
#endif
        if (a > 6)
        {
        }
    }
}
```

is not reported by RH5002.

### SHARED-STMT-004 — The fix inserts one blank line above the statement and its comments

The code fix MUST make exactly one blank line appear in front of the reported statement. When comments sit directly above the statement on their own lines, the blank line MUST go above the first of them. When the statement shares its line with the statement before it, the fix MUST move it to a new line, indented like the statement it follows. Inserted line breaks MUST use the line ending SHARED-FIX-008 specifies.

Why: A comment directly above the statement describes it, so it stays with the statement; putting the blank line between them would leave the comment describing the code above. The fix changes only the gap in front of the statement.

Example — RH5002:

```csharp
class C
{
    void M(int a)
    {
        a++;
        {|RH5002:if|} (a > 1)
        {
        }

        a++;
        // note
        {|RH5002:if|} (a > 3)
        {
        }

        a++; {|RH5002:if|} (a > 5) { }
    }
}
```

```csharp
class C
{
    void M(int a)
    {
        a++;

        if (a > 1)
        {
        }

        a++;

        // note
        if (a > 3)
        {
        }

        a++;

        if (a > 5) { }
    }
}
```
