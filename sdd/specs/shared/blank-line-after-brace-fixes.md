# Shared — Blank line inserted after a closing brace

| | |
|---|---|
| ID prefix | SHARED-AFTERBRACE |
| Status | Active |
| Code owners | `BlankLineCodeFixUtilities` |
| Uses | SHARED-BLANK-003, FMT-BLANK-005 |

This spec covers where a code fix puts the blank line that SHARED-BLANK-003 requires between a statement ending with `}` and the next statement of the same block or switch section — the RH5030 fix, which inserts it for the diagnostic it corrects, and the RH3208 fix, which inserts it after the block it creates. Whether the blank line is needed is SHARED-BLANK-003; each rule's spec states when its fix inserts one.

## Non-goals

- Whether a blank line is required — `shared/blank-line-spacing.md`.
- Blank lines that code fixes insert in other places, such as in front of a statement of a particular kind — `shared/statement-blank-lines.md`.

## Requirements

### SHARED-AFTERBRACE-001 — The blank line goes directly before the next statement

When a line break outside any comment stands between the closing brace and the next statement, the code fix MUST insert one line break, in the line ending the document already uses, at the start of the next statement's leading trivia, or directly after the last preprocessor directive in that trivia when there is one. A comment between the brace and the statement therefore stays directly above the statement, below the new blank line, and a directive stays directly below the brace.

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
