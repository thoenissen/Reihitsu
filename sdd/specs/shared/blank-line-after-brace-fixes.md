# Shared — Blank line inserted after a closing brace

| | |
|---|---|
| ID prefix | SHARED-AFTERBRACE |
| Status | Active |
| Code owners | — |
| Uses | SHARED-BLANK-003, FMT-BLANK-005, SHARED-FIX-008, SHARED-STMT-004, RH5030-003 |

This spec covers where a code fix puts the blank line that SHARED-BLANK-003 requires between a statement ending with `}` and the next statement of the same block or switch section — the RH5030 fix, which inserts it for the diagnostic it corrects, and the RH3208 fix, which inserts it after the block it creates. Whether the blank line is needed is SHARED-BLANK-003; each rule's spec states when its fix inserts one.

## Non-goals

- Whether a blank line is required — `shared/blank-line-spacing.md`.
- Blank lines that code fixes insert in other places, such as in front of a statement of a particular kind — `shared/statement-blank-lines.md`.

## Requirements

### SHARED-AFTERBRACE-001 — The blank line goes directly before the next statement

When a line break outside any comment stands between the closing brace and the next statement, the code fix MUST insert one line break, in the line ending SHARED-FIX-008 specifies, at the start of the next statement's leading trivia, or directly after the last preprocessor directive in that trivia when there is one. A comment between the brace and the statement therefore stays directly above the statement, below the new blank line, and a directive stays directly below the brace.

Why: The comment describes the statement that follows it, so it stays with that statement. A blank line in front of a directive would land inside the region the directive opens or closes; behind the directive it separates the statement without moving the directive (FMT-BLANK-005 makes the same choice).

Example — RH5030:

```csharp
class C
{
    void M(int a)
    {
        if (a > 0)
        {
            a++;
        {|RH5030:}|}
        a++;
        if (a > 0)
        {
            a++;
        {|RH5030:}|} // note
        // next
        a++;
        if (a > 0)
        {
        {|RH5030:}|}
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
            a++;
        }

        a++;
        if (a > 0)
        {
            a++;
        } // note

        // next
        a++;
        if (a > 0)
        {
        }
#pragma warning disable CS0168

        a++;
    }
}
```

Example — RH3208, after the block its fix creates:

```csharp
class C
{
    int M()
    {
        int Twice(int x) {|RH3208:=> x * 2|};
        // Use it.
        return Twice(1);
    }

    int N()
    {
        int Twice(int x) {|RH3208:=> x * 2|};
#pragma warning disable CS0168
        return Twice(1);
    }
}
```

```csharp
class C
{
    int M()
    {
        int Twice(int x)
        {
            return x * 2;
        }

        // Use it.
        return Twice(1);
    }

    int N()
    {
        int Twice(int x)
        {
            return x * 2;
        }
#pragma warning disable CS0168

        return Twice(1);
    }
}
```

### SHARED-AFTERBRACE-002 — A statement behind a comment that holds every line break moves below it

When at least one line break stands between the closing brace and the next statement and every such line break lies inside a comment, so that the statement starts behind the comment on the comment's last line, the RH3208 code fix MUST move the statement onto a new line below the comment's last line, with exactly one blank line in front of it, indented like the statements of its block or switch section. The comment keeps its text and its place behind the brace, and no whitespace stays at the end of its last line. Inserted line breaks MUST use the line ending SHARED-FIX-008 specifies. The RH5030 code fix withholds itself there instead (RH5030-003).

Inside an object initializer or anonymous object, the RH3208 code fix reads the column of the statement it would move as SHARED-STMT-004 states, with the converted local function as the statement it follows, and MUST NOT be offered in every case SHARED-STMT-004 withholds its fix for: when the line on which the local function starts is started by neither the first token of a statement of its block or switch section nor the first token of a label of its switch section — such as the last line of a comment, a wrapped continuation of an earlier statement, or a statement nested inside an earlier one — and when that line is a continuation line of a `case` label and no statement of the switch section other than the statement it would move starts its own line. The diagnostic is still reported. The fix stays offered when the local function's line starts with a statement of the block or section, such as the local function itself, when it starts with the label, and when another statement of the section starts its own line below a label's continuation line, such as a `break;`.

Why: Inserting only a line break at the start of the statement's leading trivia, as SHARED-AFTERBRACE-001 does, would land behind the comment, leave whitespace at the end of its line, and start the statement at the first column. Moving the statement out from behind the comment gives the output that the statement blank-line fixes give for a statement behind a comment that starts behind the statement before it (SHARED-STMT-004), and that the formatter gives for the same input. Inside an initializer, a line that no statement of the list and no label starts holds no column of the list as written, for the reason SHARED-STMT-004 gives; the class is rare, so the RH3208 fix is withheld there as the statement fixes are, rather than moving the statement to a column read from a comment, a continuation, or a nested statement (`AGENTS.md`, *Code-fix coverage*).

Example — RH3208:

```csharp
class C
{
    void M()
    {
        int L() {|RH3208:=> 1|}; /* a
        b */ L();
    }
}
```

```csharp
class C
{
    void M()
    {
        int L()
        {
            return 1;
        } /* a
        b */

        L();
    }
}
```

A gap with no line break at all is outside this requirement, even when a comment stands in it: the statement stays behind the comment on the closing brace's line.

```csharp
class C
{
    void M()
    {
        int L() {|RH3208:=> 1|}; /* a */ L();
    }
}
```

```csharp
class C
{
    void M()
    {
        int L()
        {
            return 1;
        } /* a */ L();
    }
}
```

Example — inside an object initializer, the local function stands behind a `case` label's continuation line, and no other statement of the section starts its own line; the continuation is one level deeper than the label in the first section and in the label's column in the second. RH3208 reports both, and no code fix is offered:

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
                        Check(value): int One() {|RH3208:=> 1|}; /* a
                        b */ One(); return 1;
                    case 2 when
                    Check(value): int Two() {|RH3208:=> 2|}; /* a
                    b */ Two(); return 2;
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
}
```

Example — inside an object initializer, the local function starts on the continuation line of a wrapped argument list, so no statement or label starts that line. RH3208 reports it, and no code fix is offered:

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
                var a = Add(value,
                            1); int M() {|RH3208:=> 1|}; /* a
                b */ M();
                return a;
            },
        };
    }

    static int Add(int left, int right)
    {
        return left + right;
    }
}
```
