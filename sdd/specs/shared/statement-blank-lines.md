# Shared — Blank line before a statement

| | |
|---|---|
| ID prefix | SHARED-STMT |
| Status | Active |
| Code owners | `StatementShouldBePrecededByABlankLineAnalyzerBase<TStatement>`, `StatementShouldBePrecededByABlankLineCodeFixProviderBase`, `BlankLineCodeFixUtilities` |
| Uses | SHARED-BLANK-001, FMT-BLANK-001, FMT-INDENT-003, SHARED-FIX-008 |

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

The code fix MUST make exactly one blank line appear in front of the reported statement, in front of its first token — for an `await using` or `await foreach` statement, in front of `await`, wherever the rule puts its diagnostic. Where that blank line goes depends on the comments in front of the statement:

- When comments stand directly above the statement on lines of their own, the blank line MUST go above the first of them.
- When a comment that ends on the statement's line in front of the statement starts a line — the statement's own line, or an earlier line when the comment spans several lines — the blank line MUST go above the line on which that comment starts, and the comment stays in front of the statement. This holds for a block comment too.
- When a comment that ends on the statement's line in front of the statement starts behind the statement before it, so that every line break between the two statements lies inside that comment, the fix MUST move the statement to a new line below the comment's last line, with the blank line above it. The comment keeps its text and its place, and no whitespace stays at the end of its last line.

When the statement shares its line with the statement before it, the fix MUST move it to a new line. A statement the fix moves MUST be indented like the statements of its block or switch section:

- Outside an object initializer or anonymous object, it takes the column of its nesting depth, the column in which the formatter puts the statements of its block or switch section (FMT-INDENT-003).
- Inside an object initializer or anonymous object, it takes the column at which the statements of its block or switch section start their lines as written. The fix reads that column from the text and does not predict how the formatter lays out the initializer.

Both hold whether the statement it follows starts its line or not — also when the statement it follows stands behind a `case` label on the last line of that label, such as a label whose `when` clause continues on a second line: the moved statement then takes the column of the section's statements, such as the column of the `break;` below it, not a column read from the label's continuation line. Inserted line breaks MUST use the line ending SHARED-FIX-008 specifies.

Inside an object initializer or anonymous object, the fix reads that column from the line on which the statement it follows — the statement before it in the same block or switch section — starts, and only when one of these starts that line, with nothing but whitespace in front of it on the line:

- the first token of a statement of the same block or switch section — the statement it follows or an earlier one: the moved statement takes that line's column;
- the first token of a label of its switch section: the moved statement takes the column one indentation level deeper than that line's;
- a token inside a label of its switch section behind the label's first token, so that the line is a continuation line of the label: the moved statement takes the column of another statement of the section that starts its own line, such as a `break;` below.

In every other case the fix MUST NOT be offered for a statement it would move, and the diagnostic is still reported: when that line starts with the last line of a comment, a comment of any kind, a wrapped continuation of an earlier statement, a statement nested inside an earlier one such as the unbraced body of an `if`, or the closing brace of an earlier block, and when it is a continuation line of a label and no statement of the section other than the reported one starts its own line. A statement the fix does not move keeps the fix wherever it stands.

Why: A comment directly above the statement describes it, so it stays with the statement; putting the blank line between them would leave the comment describing the code above. A comment that starts behind the statement before it belongs to that statement's line, so the statement moves out from behind it, as the formatter does. The fix changes only the gap in front of the statement. The statement belongs to the same list as the one it follows, so it takes that list's column; a column read from a line that a label's continuation leads is the label's, not the list's. Inside an initializer, the formatter's column depends on how it anchors the whole initializer, which a fix of one gap cannot know without predicting the indentation phase; the column of the list as written is the one the reader sees next to the moved statement. Only a line that a statement of the list or a label of the section starts holds the list's column as written; the whitespace in front of anything else — a comment's last line, a wrapped argument, a nested statement, a closing brace, a label's continuation — is that construct's layout, and any column the fix derived from it would be a prediction. When the section's statements stand only behind a label that spans lines, the text holds no column of the section at all. The class is rare — the statement the fix would move shares its line with another statement, an RH5103 violation already — so the fix is withheld for the whole class, keyed on what starts the line, rather than taught to predict the formatter construct by construct (`AGENTS.md`, *Code-fix coverage*).

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

Example — a comment that ends on the statement's line: a block comment that starts the statement's line, a comment that starts an earlier line, and a comment that starts behind the statement before it, for RH5002:

```csharp
class C
{
    void M(int a)
    {
        a++;
        /* block */ {|RH5002:if|} (a > 1) { }

        a++;
        /* a
        b */ {|RH5002:if|} (a > 2) { }

        a++; /* a
        b */ {|RH5002:if|} (a > 3) { }
    }
}
```

```csharp
class C
{
    void M(int a)
    {
        a++;

        /* block */ if (a > 1) { }

        a++;

        /* a
        b */ if (a > 2) { }

        a++; /* a
        b */

        if (a > 3) { }
    }
}
```

Example — inside an object initializer, the statement follows a statement that stands behind a `case` label spanning two lines, for RH5029:

```csharp
class Holder
{
    public System.Action<int> A { get; set; }
}

class C
{
    Holder Create()
    {
        return new Holder
        {
            A = value =>
            {
                switch (value)
                {
                    case 1 when
                        Check(value): Method(1); {|RH5029:var|} y = 1;
                        break;
                }
            },
        };
    }

    static bool Check(int value)
    {
        return value > 0;
    }

    static void Method(int value)
    {
    }
}
```

```csharp
class Holder
{
    public System.Action<int> A { get; set; }
}

class C
{
    Holder Create()
    {
        return new Holder
        {
            A = value =>
            {
                switch (value)
                {
                    case 1 when
                        Check(value): Method(1);

                        var y = 1;
                        break;
                }
            },
        };
    }

    static bool Check(int value)
    {
        return value > 0;
    }

    static void Method(int value)
    {
    }
}
```

`var y = 1;` takes the column of the section's statements as written, the column of `break;`, not one level deeper than the label's continuation line. The same input in a method body, outside an initializer, gives the same column, there as the column of the statement's nesting depth.

Example — inside an object initializer, the statement follows a statement behind a `case` label's continuation line, and no other statement of the section starts its own line; the continuation is one level deeper than the label in the first section and in the label's column in the second. RH5029 reports both, and no code fix is offered:

```csharp
class Holder
{
    public System.Func<int, int> F { get; set; }
}

class C
{
    Holder Create()
    {
        return new Holder
        {
            F = value =>
            {
                switch (value)
                {
                    case 1 when
                        Check(value): Method(1); {|RH5029:var|} y = 1; return y;
                    case 2 when
                    Check(value): Method(2); {|RH5029:var|} z = 2; return z;
                    default:
                        return 0;
                }
            },
        };
    }

    static bool Check(int value)
    {
        return value > 0;
    }

    static void Method(int value)
    {
    }
}
```

Example — inside an object initializer, the statement follows a statement that starts on the last line of a block comment, so no statement or label starts that line. RH5002 reports the `if`, and no code fix is offered:

```csharp
class Holder
{
    public System.Action<int> A { get; set; }
}

class C
{
    Holder Create()
    {
        return new Holder
        {
            A = value =>
            {
                value++; /* x
                        y */ value++; {|RH5002:if|} (value > 2) { }
            },
        };
    }
}
```

Had `value++; value++; if (value > 2) { }` stood on one line that `value++;` starts, the fix would be offered and the `if` would take the column of that line.
