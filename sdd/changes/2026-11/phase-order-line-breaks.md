# Line breaks are decided before switch braces and blank lines

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | FMT-PIPE-001 (changed), FMT-PIPE-024 (changed), FMT-PIPE-025 (added), FMT-PIPE-026 (added), FMT-BREAK-005 (retired), FMT-BREAK-006 (changed), FMT-BREAK-030 (changed), FMT-BREAK-036 (added), FMT-BLANK-004 (changed), FMT-BLANK-005 (changed), FMT-BLANK-021 (added), FMT-SWITCH-007 (changed), FMT-SWITCH-015 (added), FMT-SWITCH-016 (added) |
| Depends on | [Every switch section, label, and first statement starts its own line](switch-section-line-start.md), [The blank-line phase is the only owner of blank lines](blank-line-sole-owner.md) |

## What

The formatter's phase order changes from

… UsingDirectiveOrdering → **BlankLines → LineBreaks → SwitchCaseBraces** → HorizontalSpacing → Indentation → Cleanup

to

… UsingDirectiveOrdering → **LineBreaks → SwitchCaseBraces → BlankLines** → HorizontalSpacing → Indentation → Cleanup (FMT-PIPE-001).

- **No phase decides a line break or a blank line from a later phase's** (FMT-PIPE-025). FMT-PIPE-001 forbade this for columns and left line breaks and blank lines open. Every phase that adds or removes a line break between two tokens now runs before the blank-line phase, and the phases after it add and remove none.
- **Whether a comment starts its line is final after the line-break phase** (FMT-PIPE-026). No later phase moves a comment onto a line of its own or off one.
- **A comment behind the condition of an `if` stays there** (FMT-BREAK-036, FMT-BREAK-005 retired). It no longer moves onto a line of its own above the `if`; it stays behind the closing parenthesis, as a comment already does behind the head of `else if`, `else`, `while`, `for`, `foreach`, `using`, and `lock`. The node-level `else if` fact that only this move read is dropped from FMT-PIPE-024.
- **The blank-line phase adds blank lines to lines that are already broken.** A section on the line of the previous section's `break` gets its own line from the line-break phase (FMT-BREAK-033) and only the blank line from FMT-BLANK-004, which loses the sentence that let the blank-line phase end the `break`'s line. Two statements on one line are split by FMT-BREAK-006 before FMT-BLANK-005 separates them. The one line break the blank-line phase still writes to end a line is for a statement that the line-break phase leaves on its predecessor's line next to an empty statement (FMT-BLANK-021).
- **The switch-brace phase places its braces as the line-break phase places a section's block and writes no blank line** (FMT-SWITCH-015, FMT-SWITCH-016). The blank lines around and inside the blocks it inserts are decided by the blank-line phase, which now sees those blocks. FMT-SWITCH-007's blank line in front of a comment that leads the terminal `break` is the blank-line phase's, inserted in the same run; the blank lines the wrap carries are collapsed and removed by the blank-line phase after the wrap rather than before it.

## Why

FMT-PIPE-001 forbids a phase to compute a column from a later phase's move and leaves line breaks and blank lines open. Running the blank-line phase before the line-break phase, with no stated reason, put three decisions in two places:

- **A comment the line-break phase moved got its blank line in the next run.** FMT-BREAK-005 moved a comment behind an `if` condition onto a line of its own after the blank-line phase had decided every comment boundary. Where nothing else put a blank line above the `if`, the FMT-BLANK-011 blank line above the moved comment appeared only in the second run, against FMT-PIPE-004 (visible output change 3).
- **The blank-line phase split lines.** To give a blank line in front of a statement or a section that shared its line with the one before it, the blank-line phase wrote the line break that ends that line itself, a decision FMT-BREAK-006 and FMT-BREAK-033 give the line-break phase; FMT-BLANK-004 and FMT-BLANK-005 carried sentences that said so.
- **The switch-brace phase laid out its blocks after both layout phases had run.** It has to run after the line-break phase, because whether a section spans several lines is measured on that phase's output (FMT-SWITCH-003, FMT-SWITCH-008). With the blank-line phase in front of both, the blank lines around the blocks it inserts were decided before the blocks existed, and FMT-SWITCH-007 wrote that dependency in ("already in the output of the pass that inserts the braces"), while the spec's non-goals gave those blank lines to FMT-BLANK.

With the blank-line phase behind the line-break and switch-brace phases, each blank line is decided once, on the lines the output keeps.

The maintainer decided the comment behind an `if` condition before this change was written: the move is removed, so the comment stays where the author wrote it, as it does behind every other block-statement head (see *Decisions*). FMT-BREAK-005's reason — a comment behind the condition would end up between condition and brace or glued to the brace — does not occur: the brace starts the next line, and the comment stays on the condition's line, as it already does for `while` and `foreach`.

## Visible output changes

Each entry shows the formatter's output on `main` at aec8dc6 and after this change, from one run of the formatter. Only FMT-BREAK-005's retirement changes visible output. The reorder itself changes none: every other output of the formatter is intended to stay as it is on `main`, and an output that differs is a defect of this change.

### 1. A comment behind an `if` condition stays behind it

```csharp
// input
class C
{
    void M(bool a)
    {
        Foo();

        if (a) // why
        {
            Foo();
        }
    }
}
```

```csharp
// before
class C
{
    void M(bool a)
    {
        Foo();

        // why
        if (a)
        {
            Foo();
        }
    }
}
```

```csharp
// after
class C
{
    void M(bool a)
    {
        Foo();

        if (a) // why
        {
            Foo();
        }
    }
}
```

### 2. A delimited comment behind an `if` condition, with the body on the same line

```csharp
// input
class C
{
    void M(bool a)
    {
        Foo();

        if (a) /* m */ { Foo(); }
    }
}
```

```csharp
// before
class C
{
    void M(bool a)
    {
        Foo();

        /* m */
        if (a)
        {
            Foo();
        }
    }
}
```

```csharp
// after
class C
{
    void M(bool a)
    {
        Foo();

        if (a) /* m */
        {
            Foo();
        }
    }
}
```

### 3. An `if` that is the body of a loop

The input of thoenissen/Reihitsu#1498. On `main` the first run moves the comment and a second run inserts the blank line above it; after this change the input is kept unchanged in one run.

```csharp
// input
class C
{
    void M(bool a, bool b)
    {
        while (b)
            if (a) // why
            {
                Foo();
            }
    }
}
```

```csharp
// before (first run; the second run adds a blank line above "// why")
class C
{
    void M(bool a, bool b)
    {
        while (b)
            // why
            if (a)
            {
                Foo();
            }
    }
}
```

```csharp
// after
class C
{
    void M(bool a, bool b)
    {
        while (b)
            if (a) // why
            {
                Foo();
            }
    }
}
```

### 4. An `if` whose unbraced body gets braces

```csharp
// input
class C
{
    void M(bool a)
    {
        Foo();

        if (a) // c
            Foo();
    }
}
```

```csharp
// before
class C
{
    void M(bool a)
    {
        Foo();

        // c
        if (a)
        {
            Foo();
        }
    }
}
```

```csharp
// after
class C
{
    void M(bool a)
    {
        Foo();

        if (a) // c
        {
            Foo();
        }
    }
}
```

## Decisions

### Maintainer answers

1. **The comment behind an `if` condition stays where the author wrote it.** Asked before the run was claimed, with the evidence that on `main` a comment behind `while (a)` and `foreach (…)` stays behind the parenthesis and is stable in one run, while only `if (a) // i` and `if (a) /* m */ { … }` move it above the `if`. The maintainer chose to remove FMT-BREAK-005's move in this run (visible output changes 1 to 4) and to keep the phase reorder, which the issue's other defects motivate. The issue's example input and its desired output with the comment above the `if` are superseded by this answer.

### The new order

LineBreaks → SwitchCaseBraces → BlankLines. The switch-brace phase stays behind the line-break phase, because it measures multi-line sections on that phase's output (FMT-SWITCH-003, FMT-SWITCH-008); the blank-line phase moves behind both, so it sees every line break and every inserted block. The order of the other phases does not change.

### The rule for line breaks and blank lines is a requirement of its own

FMT-PIPE-001 keeps the order and the rule for columns and points to FMT-PIPE-025 for line breaks and blank lines. FMT-PIPE-026 is a third requirement because its reason differs: whether a comment starts its line decides both its blank line and its indentation, and the documentation phase, which moves documentation comments off code, runs before the line-break phase.

### A statement next to an empty statement is still split by the blank-line phase

FMT-BREAK-006 leaves an empty statement and its neighbour as written. Where a statement rule asks for a blank line between them, `main` splits the line in the blank-line phase (`a++; ; return a;` gives `;`, a blank line, and `return a;`), and the analyzer rules that use SHARED-STMT-001 report such a pair too. This change keeps that output, so FMT-BLANK-021 states that the blank-line phase writes the line break that ends the line together with the blank line in that case. The issue's goal that the blank-line phase insert no line break that ends a statement's line is narrowed to the pairs the line-break phase splits; leaving `; return a;` on one line without the blank line would change visible output that the issue did not ask about.

### The switch-brace phase uses the line-break phase's brace placement

The braces the switch-brace phase inserts start their lines as FMT-BREAK-008 places a section's block (FMT-SWITCH-015), by the line-break phase's own brace placement (`BracePlacer`), not by a placement of its own. The phase writes no blank line and removes none (FMT-SWITCH-016): it carries the blank lines it finds, and the blank-line phase decides them afterwards, which gives the output `main` gives — a blank line the wrap carries behind the new opening brace is removed by FMT-BLANK-007, and a run in front of a comment behind the new block collapses to one by FMT-BLANK-014.

### FMT-PIPE-024 loses the `else if` fact

The `else if` fact told the line-break phase that a target was the `if` of an `else` clause, so that it would not move the comment behind its condition. With the move gone no decision reads it, and FMT-PIPE-024 lists exactly the facts the phases use. The example in FMT-PIPE-024's reason that relied on it is replaced by the list-position example the requirement already shows.

### SHARED-BLANK-003 is unchanged

Its reason says that the formatter splits a same-line pair (FMT-BREAK) and then separates the statements (FMT-BLANK). That is the order this change introduces, so the sentence becomes accurate and stays.

## Alternatives considered

- **Keep the order and move FMT-BREAK-005's comment move into the structural transforms**, so that it happens before the blank-line phase — rejected: the maintainer removed the move, and the order would still leave the blank-line phase splitting lines and the switch-brace phase's blank lines decided before its blocks exist.
- **LineBreaks → BlankLines → SwitchCaseBraces** — rejected: the blank lines around the blocks the switch-brace phase inserts would still be decided before the blocks exist.
- **Let the line-break phase split an empty statement from its neighbour where a blank line is required** — rejected: the line-break phase would decide a line break from a blank-line rule of a later phase, which FMT-PIPE-025 forbids.
- **Let the blank-line phase write no line break at all**, so `; return a;` stays on one line without a blank line — rejected: it changes visible output that the issue did not ask about, and the analyzers report the pair.
