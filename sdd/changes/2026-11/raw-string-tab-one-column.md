# A tab counts as one column when a raw string is aligned

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-RAW-001 (changed), FMT-RAW Non-goals (narrowed), FMT-TAB Non-goals (wording aligned), FMT-INDENT-013 (clarified) |

## What

- **A tab in front of a raw string's closing marker counts as one column.** FMT-RAW-001 measures the column of the closing marker before the move as the width of the whitespace in front of it. Every whitespace character counts as one column, and the tab now does too: it counts as one column, not as the width of a tab stop, and it stays as written (FMT-TAB-003, FMT-RAW-002). A literal whose indentation holds tabs used to be outside FMT-RAW (Non-goals); it is now aligned by the same measure as any other literal.
- **Only tab-width alignment stays out of scope.** The FMT-RAW non-goal is narrowed to counting a tab as more than one column, or replacing it with spaces. That is the planned extension tracked in thoenissen/Reihitsu#1432. The FMT-TAB non-goal states the same.
- **For context: a line inside the only type of a base list is aligned (FMT-INDENT-013).** FMT-INDENT-013 aligns a line inside an element of "a list from FMT-INDENT-016", and FMT-INDENT-016 speaks of base lists "with two or more types". The two texts allowed two readings for a base list with one type. The maintainer decided the reading in which every base list counts, an enum's underlying type included. This is a clarification of existing text, not an extension, and is recorded here because the same run decided it.

## Why

The RH5202 analyzer and its code fix count every whitespace character in front of a raw string's closing marker as one column, a tab included. The formatter used to count only spaces. Its output for a tab-indented literal was therefore reported by RH5202, and it moved the RH5202 fix's output back again, against SHARED-FIX-007 and SHARED-DIAG-010. Thoenissen/Reihitsu#1611 brings the formatter onto the measure RH5202 uses for every whitespace character. Leaving the tab out would have needed a deliberate exception in that measure and kept the formatter and the fix undoing each other. Because tabs were an FMT-RAW non-goal, extending the requirement to them is a behavior change (`sdd/README.md`, *Spec format*).

## Visible output changes

**1 · Formatter · a raw string whose closing line is indented with tabs** (`→` is one tab)

```csharp
// input
class C
{
    void M()
    {
        var a = """
→→→→x
→→→→""";
    }
}
```

```csharp
// before: 16 spaces + 4 tabs; RH5202 reports it
        var a = """
                →→→→x
                →→→→""";
```

```csharp
// after: 12 spaces + 4 tabs; RH5202 does not report it, the same as the RH5202 fix's output
        var a = """
            →→→→x
            →→→→""";
```

**2 · Formatter · a line inside the only type of a base list**

```csharp
// input
class B : System. // c
    IDisposable
{
    public void Dispose()
    {
    }
}

enum E : System. // c
    Int32
{
    A,
}
```

```csharp
// before: IDisposable and Int32 at column 0
class B : System. // c
IDisposable
{
    public void Dispose()
    {
    }
}

enum E : System. // c
Int32
{
    A,
}
```

```csharp
// after
class B : System. // c
          IDisposable
{
    public void Dispose()
    {
    }
}

enum E : System. // c
         Int32
{
    A,
}
```

## Decisions

### Q2 — a tab counts as one column (maintainer: A)

Each tab in front of a raw string's closing marker counts as one column, as RH5202 counts it. The formatter's result is the RH5202 fix's output, and RH5202 does not report it. Tab-width alignment stays with thoenissen/Reihitsu#1432.

### Q1 — a single-type base list is aligned like a list of two or more types (maintainer: A)

A line inside the only type of a base list, including an enum's underlying type, aligns under that type's first token, as in a list of two or more types. It is not left at column 0.

## Alternatives considered

- **Keep tabs as a non-goal and exclude the tab from the shared measure** — rejected: the formatter would keep producing output RH5202 reports and keep undoing the RH5202 fix's output (SHARED-FIX-007, SHARED-DIAG-010).
- **Align tab-indented literals at a tab width, or replace the tabs with spaces (thoenissen/Reihitsu#1432)** — rejected for this change: it changes the literal's indentation characters and needs its own decision; it stays a non-goal.
- **Leave a single-type base list at column 0** — rejected: a line at column 0 reads as a new declaration, and a two-type list's continuation is already aligned.
