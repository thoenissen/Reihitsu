# Every switch section, label, and first statement starts its own line

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | FMT-BREAK-033 (added), FMT-BREAK-034 (added), FMT-BREAK-035 (added), FMT-BREAK-006 (changed), FMT-BLANK-004 (changed) |
| Depends on | [One line-start fact for analyzers and every formatter phase](../2026-10/line-start-fact.md), [The blank-line phase is the only owner of blank lines](blank-line-sole-owner.md) |

## What

The line-break phase decides the lines a switch statement's sections start on:

- **Every section starts its own line** (FMT-BREAK-033), whatever statement ends the section before it. Until now, only a section behind a `break` statement got a line of its own, and only as a side effect of the blank line FMT-BLANK-004 asks for; behind `return`, `throw`, `goto`, or `continue` the sections stayed on one line.
- **Every label of a section starts its own line** (FMT-BREAK-034): `case 1: case 2:` becomes two lines.
- **The first statement of a section starts its own line** (FMT-BREAK-035), behind the section's labels, as every later statement of the section already does (FMT-BREAK-006). An empty statement (`;`) that is the first statement stays where it is, as FMT-BREAK-006 leaves it between statements.

"Starts its own line" is the shared line-start fact, SHARED-BLANK-008: a section, a label, or a statement behind a `//` comment or a directive already starts its line; one behind the last line of a multi-line comment does not. Where the phase inserts a line break, a comment that trailed the token before stays on that token's line (FMT-BREAK-001).

FMT-BREAK-006 is scoped to what it already decided — a statement that follows another statement of the same list — and points to FMT-BREAK-002 and FMT-BREAK-035 for the first statement of a block and of a section.

FMT-BLANK-004 keeps its rule — a blank line before a section whose preceding section ends in `break`, none otherwise — and states that it holds whether or not the section starts its line: in front of a section on the line of the `break`, the blank-line phase writes the line break that ends that line together with the blank line, so the first run gives the blank line. The line break in front of every other section is the line-break phase's.

## Why

No phase owns the line a switch section starts on:

- The only code that writes a line break in front of a section is FMT-BLANK-004's. It inserts exactly one line break, whether or not the section already starts its line. In front of a section on the line of the `break`, that line break only ends the line, and the blank line FMT-BLANK-004 requires appears in the second run, against FMT-PIPE-004. The statement rules of the same phase insert two line breaks in the same situation.
- Behind a section that ends in any other statement, nothing separates the sections, so `case 1: return 1; case 2: return 2;` stays on one line.
- The line-break phase places a section's block (FMT-BREAK-008) and its later statements (FMT-BREAK-006), but not its first statement. A statement behind its label stays there, at the label column (FMT-INDENT-003), unless the switch-brace phase braces the section and moves it.

A switch is read by its labels. A section that shares a line with the section before it, a label behind another label, and a statement on the label's line all hide part of that structure at the left margin.

## Visible output changes

Each entry shows the formatter's output on `main` and after this change, from one run of the formatter.

### 1. Sections behind a section that does not end in `break` start their own line

```csharp
class C
{
    int M(int x)
    {
        switch (x) { case 1: return 1; case 2: return 2; }

        return 0;
    }
}
```

Before:

```csharp
class C
{
    int M(int x)
    {
        switch (x)
        {
            case 1: return 1; case 2: return 2;
        }

        return 0;
    }
}
```

After:

```csharp
class C
{
    int M(int x)
    {
        switch (x)
        {
            case 1:
                return 1;
            case 2:
                return 2;
        }

        return 0;
    }
}
```

The same holds behind `throw`, `goto case`, and `continue`. No blank line separates these sections (FMT-BLANK-004).

### 2. A section on the line of a `break` gets its blank line in the first run

```csharp
class C
{
    void M(int x)
    {
        switch (x) { case 1: A(); break; case 2: B(); break; }
    }
}
```

Before, after one run (a second run adds the blank line in front of `case 2:`):

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1: A();
                break;
            case 2: B();
                break;
        }
    }
}
```

After, already after one run:

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1:
                A();
                break;

            case 2:
                B();
                break;
        }
    }
}
```

The same holds for a section behind the last line of a multi-line comment that trails the `break`.

### 3. The first statement of a section moves behind its labels

This includes a `break` statement and a statement behind a pattern label with a `when` guard. A comment between the label and the statement stays on the label's line.

```csharp
class C
{
    void M(int x, object o)
    {
        switch (x)
        {
            case 1: /* c */ A();
                break;

            default: break;
        }

        switch (o)
        {
            case int i when i > 0: A();
                break;
        }
    }
}
```

Before: unchanged.

After:

```csharp
class C
{
    void M(int x, object o)
    {
        switch (x)
        {
            case 1: /* c */
                A();
                break;

            default:
                break;
        }

        switch (o)
        {
            case int i when i > 0:
                A();
                break;
        }
    }
}
```

### 4. Labels that share a line are stacked

Pending output question 1 of the run.

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1: case 2:
                A();
                break;
        }
    }
}
```

Before: unchanged.

After:

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1:
            case 2:
                A();
                break;
        }
    }
}
```

### 5. Code fixes that format a switch through the formatter

A code fix that formats a node through the formatter lays out the switch sections inside that node as entries 1 to 4 show. For RH5417 on

```csharp
class C
{
    int M(int x) { switch (x) { case 1: return 1; case 2: return 2; } return 0; }
}
```

the result was:

```csharp
class C
{
    int M(int x)
    {
        switch (x)
        {
            case 1: return 1; case 2: return 2;
        }

        return 0;
    }
}
```

and becomes:

```csharp
class C
{
    int M(int x)
    {
        switch (x)
        {
            case 1:
                return 1;
            case 2:
                return 2;
        }

        return 0;
    }
}
```

No diagnostic and no rule page changes.

## Decisions

### The line-break phase owns the line a section, a label, and a first statement start on

Three requirements in FMT-BREAK, one per decision, each deciding "starts its own line" by SHARED-BLANK-008 and inserting the line break in front of the moved token as FMT-BREAK-001 states. The first section of a switch is not part of FMT-BREAK-033: it starts its line behind the switch's opening brace (FMT-BREAK-002). A block that forms a section's body already starts its line (FMT-BREAK-008); FMT-BREAK-035 names it only so the requirement covers every kind of first statement. For a node-level target, the requirements decide the lines inside the target; the gap in front of the target is kept (FMT-PIPE-010).

### FMT-BREAK-006 decides statements behind statements

FMT-BREAK-006's "every statement … MUST start its own line" was read, by the code and by RH5103-001, as a rule between statements of one list; its example shows only statement pairs. Its wording now says so, and it points to FMT-BREAK-002 for the first statement of a block and to FMT-BREAK-035 for the first statement of a section, so each first statement has one owner.

### The blank-line phase writes the line breaks of the blank line it requires

The blank-line phase runs before the line-break phase, and the line-break phase inserts no blank line (FMT-BREAK-030). Where FMT-BLANK-004 requires a blank line in front of a section that does not start its line, the blank line can only come from the blank-line phase, and it needs two line breaks there: one that ends the `break` statement's line and one for the blank line — the count the statement rules already use for a statement on its predecessor's line. Where no blank line is required, the blank-line phase writes no line break in front of a section; FMT-BREAK-033 places it.

The issue asked that the blank-line phase write no line break that ends a line. That cannot hold together with a blank line in the first run: with the phases in their order, the line-break phase would have to insert the blank line, which FMT-BREAK-030 forbids, or the blank-line phase would have to predict the line-break phase's insertion (FMT-PIPE-001). The output is the same either way; only which phase writes the first of the two line breaks differs, and FMT-BLANK-004 now states it.

### Labels each start a line

Pending output question 1 of the run; the spec is written on the recommended option. Stacked labels are how the formatter already writes them when it braces a section (FMT-SWITCH-006), and one label per line puts every value a section handles at the label column.

### An empty first statement stays where it is

Pending output question 2 of the run; the spec is written on the recommended option. FMT-BREAK-006 leaves an empty statement and its neighbor as written, because moving it only moves a stray `;` around; FMT-BREAK-035 applies the same exception to the first statement, so `case 1: ; break;` stays on one line.

### Analyzers are unchanged

RH5103 does not report the first statement of a section on its label's line (RH5103-001) or a statement of another section on the same line; RH5204 checks columns, not line starts. The formatter may do more than the analyzers ([P11](../../README.md#the-formatter-may-do-more-than-the-analyzers-p11)); whether a rule should report these shapes is outside this change.

## Alternatives considered

- **Every section starts its own line, and the first statement may stay behind its label** (the issue's alternative) — rejected: the first statement would stay at the label column while the section's other statements stand one level deeper (FMT-INDENT-003), and FMT-BREAK-006 already gives every later statement its own line.
- **Labels of one section keep the lines the author wrote** — the option B of output question 1; rejected on the recommendation, pending the answer.
- **The line-break phase writes the blank line in front of a section behind `break`** — rejected: FMT-BREAK-030 makes the blank-line phase the only owner of blank lines.
- **The blank-line phase writes only the blank line's line break and predicts that the line-break phase ends the line** — rejected: it would predict a later phase (FMT-PIPE-001), and with the phases in their order the result has no blank line.
