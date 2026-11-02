# Line breaks are decided before switch braces and blank lines

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-PIPE-001 (changed), FMT-PIPE-024 (changed), FMT-PIPE-025 (added), FMT-PIPE-026 (added), FMT-BREAK-002 (changed), FMT-BREAK-005 (retired), FMT-BREAK-006 (changed), FMT-BREAK-030 (changed), FMT-BREAK-035 (changed), FMT-BREAK-036 (added), FMT-BLANK-004 (changed), FMT-BLANK-005 (changed), FMT-BLANK-018 (changed), FMT-SWITCH-007 (changed), FMT-SWITCH-015 (added), FMT-SWITCH-016 (added), SHARED-COMMENT-012 (changed) |
| Depends on | [Every switch section, label, and first statement starts its own line](switch-section-line-start.md), [The blank-line phase is the only owner of blank lines](blank-line-sole-owner.md) |

## What

The formatter's phase order changes from

… UsingDirectiveOrdering → **BlankLines → LineBreaks → SwitchCaseBraces** → HorizontalSpacing → Indentation → Cleanup

to

… UsingDirectiveOrdering → **LineBreaks → SwitchCaseBraces → BlankLines** → HorizontalSpacing → Indentation → Cleanup (FMT-PIPE-001).

- **No phase decides a line break or a blank line from a later phase's** (FMT-PIPE-025). FMT-PIPE-001 forbade this for columns and left line breaks and blank lines open. Whether two tokens with text stand on one line is now decided before the blank-line phase, which adds and removes only blank lines, and the phases after it change neither. Two phases may still read one shared statement of a condition from the syntax; that is not a prediction.
- **Whether a comment starts its line is final after the line-break phase** (FMT-PIPE-026). No later phase moves a comment onto a line of its own or off one.
- **A comment behind the condition of an `if` stays there** (FMT-BREAK-036, FMT-BREAK-005 retired). It no longer moves onto a line of its own above the `if`; it stays behind the closing parenthesis, as a comment already does behind the head of `else if`, `else`, `while`, `for`, `foreach`, `using`, and `lock`. The node-level `else if` fact that only this move read is dropped from FMT-PIPE-024.
- **The blank-line phase adds blank lines to lines that are already broken.** A section on the line of the previous section's `break` gets its own line from the line-break phase (FMT-BREAK-033) and only the blank line from FMT-BLANK-004, which loses the sentence that let the blank-line phase end the `break`'s line. Two statements on one line are split by FMT-BREAK-006 before FMT-BLANK-005 separates them. FMT-BREAK-006 now also splits an empty statement from its neighbour where a statement rule asks for a blank line between them, which the blank-line phase did before; the blank-line phase writes no line break that ends a statement's line. FMT-BLANK-018 no longer predicts where the line-break phase breaks a chain link off a multi-line comment's closing line, because that line break now stands when the blank-line phase runs.
- **The switch-brace phase places its braces on the lines a section's block gets from the line-break phase and writes no blank line** (FMT-SWITCH-015, FMT-SWITCH-016). The blank lines around and inside the blocks it inserts are decided by the blank-line phase, which now sees those blocks; the wrap carries the blank lines that lead a comment and, as on `main`, none without one. FMT-SWITCH-007's blank line in front of a comment that leads the terminal `break` is the blank-line phase's, inserted in the same run; the blank lines the wrap carries are collapsed and removed by the blank-line phase after the wrap rather than before it.

## Why

FMT-PIPE-001 forbids a phase to compute a column from a later phase's move and leaves line breaks and blank lines open. Running the blank-line phase before the line-break phase, with no stated reason, put three decisions in two places:

- **A comment the line-break phase moved got its blank line in the next run.** FMT-BREAK-005 moved a comment behind an `if` condition onto a line of its own after the blank-line phase had decided every comment boundary. Where nothing else put a blank line above the `if`, the FMT-BLANK-011 blank line above the moved comment appeared only in the second run, against FMT-PIPE-004 (visible output change 3).
- **The blank-line phase split lines.** To give a blank line in front of a statement or a section that shared its line with the one before it, the blank-line phase wrote the line break that ends that line itself, a decision FMT-BREAK-006 and FMT-BREAK-033 give the line-break phase; FMT-BLANK-004 and FMT-BLANK-005 carried sentences that said so.
- **The switch-brace phase laid out its blocks after both layout phases had run.** It has to run after the line-break phase, because whether a section spans several lines is measured on that phase's output (FMT-SWITCH-003, FMT-SWITCH-008). With the blank-line phase in front of both, the blank lines around the blocks it inserts were decided before the blocks existed, and FMT-SWITCH-007 wrote that dependency in ("already in the output of the pass that inserts the braces"), while the spec's non-goals gave those blank lines to FMT-BLANK.

With the blank-line phase behind the line-break and switch-brace phases, each blank line is decided once, on the lines the output keeps.

The maintainer decided the comment behind an `if` condition before this change was written: the move is removed, so the comment stays where the author wrote it, as it does behind every other block-statement head (see *Decisions*). FMT-BREAK-005's reason — a comment behind the condition would end up between condition and brace or glued to the brace — does not occur: the brace starts the next line, and the comment stays on the condition's line, as it already does for `while` and `foreach`.

## Visible output changes

Entries 1 to 4 show the formatter's output on `main` at aec8dc6 and after this change, from one run of the formatter; entry 5 shows a code fix's result. Only FMT-BREAK-005's retirement changes visible output, on every surface that formats through the formatter: the CLI, whole-document formatting, and the code fixes that delegate their layout to it. The reorder itself changes none: every other output of the formatter and of the code fixes is intended to stay as it is on `main`, and an output that differs is a defect of this change.

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

### 5. The code fixes of RH5405, RH5406, and RH5407 keep the comment behind the `if` condition

The braces these fixes insert are laid out by the formatter's node-level entry point (`FormatNodeInDocumentAsync`), so the comment behind the condition of the `if` they brace stays behind it, as in entry 4. The example is RH5405 (Braces must not be omitted); RH5406 (Braces must not be omitted from multi-line child statements) and RH5407 (Use braces consistently) give the same result for an `if` they brace, and every other code fix that formats a node containing such an `if` keeps the comment too. "Before" is the fix's result on this change's base, from `scripts/apply-fix.sh RH5405`.

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

1. **The comment behind an `if` condition stays where the author wrote it.** Asked before the run was claimed, with the evidence that on `main` a comment behind `while (a)` and `foreach (…)` stays behind the parenthesis and is stable in one run, while only `if (a) // i` and `if (a) /* m */ { … }` move it above the `if`. The maintainer chose to remove FMT-BREAK-005's move in this run (visible output changes 1 to 4, and 5 for the code fixes that format through the formatter) and to keep the phase reorder, which the issue's other defects motivate. The issue's example input and its desired output with the comment above the `if` are superseded by this answer.
2. **A statement behind the closing line of a multi-line comment keeps its place.** Asked after the change was implemented, because SHARED-COMMENT-012 asked the formatter to break the line in front of such a statement while the formatter, on `main` as after this change, leaves it behind the `*/` in two cases: behind a comment that starts on its own line, and between two statements no blank-line requirement separates. The maintainer chose to keep that output: it is accepted behavior, not a defect, since the formatter leaves code behind a comment on its line in many places, and no follow-up is filed for it. Where a blank-line requirement separates the statement from the one before it and the comment starts on that statement's line, the statement still starts the line below the comment, as on `main`. FMT-BREAK-006 states the placement, FMT-BREAK-002 and FMT-BREAK-035 point to it for a first statement, and SHARED-COMMENT-012 names the statement as the exception to its formatter clause. No output changes:

   ```csharp
   A(); /* x
   */ B();
   a++;

   /* y
   */ return a;
   ```

### The new order

LineBreaks → SwitchCaseBraces → BlankLines. The switch-brace phase stays behind the line-break phase, because it measures multi-line sections on that phase's output (FMT-SWITCH-003, FMT-SWITCH-008); the blank-line phase moves behind both, so it sees every line break and every inserted block. The order of the other phases does not change.

### The rule for line breaks and blank lines is a requirement of its own

FMT-PIPE-001 keeps the order and the rule for columns and points to FMT-PIPE-025 for line breaks and blank lines. FMT-PIPE-026 is a third requirement because its reason differs: whether a comment starts its line decides both its blank line and its indentation, and the documentation phase, which moves documentation comments off code, runs before the line-break phase.

### The line-break phase splits an empty statement from a neighbour that a statement rule separates

FMT-BREAK-006 leaves an empty statement and its neighbour as written. Where a statement rule asks for a blank line between them, `main` splits the line in the blank-line phase (`a++; ; return a;` gives `;`, a blank line, and `return a;`), and the analyzer rules that use SHARED-STMT-001 report such a pair too. This change keeps that output and moves the split into the line-break phase: FMT-BREAK-006's exemption for empty statements stops where FMT-BLANK-001 to FMT-BLANK-005 ask for a blank line between the pair, and the blank-line phase adds only the blank line. Both phases read which pairs a statement rule separates from one shared statement of that rule, so the line-break phase does not predict the blank-line phase; FMT-PIPE-025 allows such a shared condition and forbids only a prediction or a copy.

The split has to happen before the switch-brace phase: that phase measures whether a section spans several lines (FMT-SWITCH-003), and a section such as `case 1: ; return 1;` is multi-line only once `return 1;` has its own line. Left to the blank-line phase, after the measure, the section would be braced only in the second run, against FMT-PIPE-004. With the split in the line-break phase, the issue's goal that the blank-line phase insert no line break that ends a statement's line holds as written.

### The blank-line phase no longer predicts chain line breaks

FMT-BLANK-018 classified a comment in front of a chain link as on its own line where the line-break phase would later break the link off the closing line of a multi-line comment, so its blank line would stand in the first run. After the reorder that line break already stands when the blank-line phase runs, so the comment is on its own line by the ordinary rule, and the prediction is removed. Output does not change.

### The switch-brace phase places its braces on a section block's lines

The braces the switch-brace phase inserts start the lines that FMT-BREAK-008 gives the braces of a block that forms a section's body (FMT-SWITCH-015). The requirement states those line positions, which are what a test can observe. The issue's goal that the phase get them through the line-break phase's `BracePlacer` rather than from code of its own is dropped from this change: the phase keeps its own placement, which predates the change and gives the same lines, because a section without a terminal `break` needs the token behind the section moved, outside the section the phase rewrites. Routing the inserted braces through `BracePlacer` is left to a follow-up issue. The phase writes no blank line (FMT-SWITCH-016): it carries the leading trivia that holds a comment across the new braces with its blank lines as written, drops leading trivia without a comment as `main` does, and the blank-line phase decides the carried blank lines afterwards, which gives the output `main` gives — a blank line the wrap carries behind the new opening brace is removed by FMT-BLANK-007, and a run in front of a comment behind the new block collapses to one by FMT-BLANK-014.

### FMT-PIPE-024 loses the `else if` fact

The `else if` fact told the line-break phase that a target was the `if` of an `else` clause, so that it would not move the comment behind its condition. With the move gone no decision reads it, and FMT-PIPE-024 lists exactly the facts the phases use. The example in FMT-PIPE-024's reason that relied on it is replaced by the list-position example the requirement already shows.

### A statement behind a multi-line comment's closing line

SHARED-COMMENT-012 asked the formatter to break the line in front of every token behind the closing `*/` of a comment that spans several lines where a requirement asks the token to start its line, and FMT-BREAK-006 asked every statement to start its own line. For a statement the formatter did not do this on `main` and does not after this change, except where a line must end anyway: where a blank-line requirement separates the statement from the previous one and the comment starts on that statement's line, and a first statement behind a comment that starts behind the opening brace or the section's label. Maintainer answer 2 keeps that output, so the specs now state it: FMT-BREAK-006 gives the placement and its two conditions — where the comment starts, and whether a blank-line requirement separates the pair — with an example on each side, and SHARED-COMMENT-012's formatter clause names the statement as its exception. Its clause for analyzer rules is unchanged.

### SHARED-BLANK-003 is unchanged

Its reason says that the formatter splits a same-line pair (FMT-BREAK) and then separates the statements (FMT-BLANK). That is the order this change introduces, so the sentence becomes accurate and stays.

## Alternatives considered

- **Keep the order and move FMT-BREAK-005's comment move into the structural transforms**, so that it happens before the blank-line phase — rejected: the maintainer removed the move, and the order would still leave the blank-line phase splitting lines and the switch-brace phase's blank lines decided before its blocks exist.
- **LineBreaks → BlankLines → SwitchCaseBraces** — rejected: the blank lines around the blocks the switch-brace phase inserts would still be decided before the blocks exist.
- **Keep the split of an empty statement from its neighbour in the blank-line phase**, which writes the line break together with the blank line — rejected: the switch-brace phase measures the section before that line break exists, so `case 1: ; return 1;` and `case 1: ; throw null;` are braced only in the second run (FMT-PIPE-004).
- **Leave empty statements out of the switch-brace phase's multi-line measure** — rejected: `case 1: ; return 1;` would stay unbraced, which changes visible output.
- **Let the switch-brace phase predict the blank-line phase's line break** — rejected: a prediction, which FMT-PIPE-025 forbids.
- **Route the braces the switch-brace phase inserts through `BracePlacer` in this change** — rejected for this change: the outputs already agree, the phase's own placement predates the change, and a section without a terminal `break` needs the token behind the section moved, which lies outside the section the phase rewrites; it is a follow-up.
- **Give the line-break phase its own list of the statements a blank line separates** — rejected: a second statement of the blank-line rule, which can drift from the first.
- **Let the blank-line phase write no line break at all and leave the pair as written**, so `; return a;` stays on one line without a blank line — rejected: it changes visible output that the issue did not ask about, and the analyzers report the pair.
