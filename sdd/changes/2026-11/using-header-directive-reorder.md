# A using block whose preprocessor directives all stand in front of it is reordered

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | SHARED-USING-009 (changed), FMT-USING-004 (examples changed), FMT-USING-009 (clarified), FMT-USING-012 (added), RH7201-001, RH7202-001, RH7203-001, RH7204-001, RH7205-001, RH7206-001, RH7207-001 (clarified), RH7201-004, RH7202-004, RH7203-004, RH7204-004, RH7205-004, RH7206-004, RH7207-004 (changed) |
| Depends on | [A using block that holds a preprocessor directive is reported without a fix](using-block-directive-report.md) |

## What

SHARED-USING-009 kept the formatter and the code fixes of RH7201–RH7207 away from every using block that holds a preprocessor directive in the trivia of its directives, also from a block whose only directive stands in front of its first using, such as a `#nullable enable` header. The guard narrows: a preprocessor directive in the leading trivia of the block's first directive — in front of the block — no longer makes the block unsafe, unless it is a conditional directive (`#if`, `#elif`, `#else`, `#endif`). Such a block is reordered by the formatter, fixed by the RH7201–RH7207 code fixes and Fix All, and the whole leading trivia of its original first directive — the directives, comments, and blank lines in front of the block — stays at the top of the block (FMT-USING-012).

What stays: a preprocessor directive in the leading trivia of any other token of the block — in front of a later directive, which puts it between two directives, or between the tokens of a directive, the first one included — still makes the block unsafe, and so does a conditional directive in front of the block, whether its group ends in front of the block or encloses it. Such a block is reported, no code fix is offered, and the formatter only splits directives that share a line (FMT-USING-004). A directive after the last using still belongs to the next declaration and does not count.

Because such a block is no longer unsafe, RH7207 also checks its blank lines between groups, and its fix and the formatter lay them out (SHARED-USING-008, FMT-USING-002), as for a block without a directive.

## Why

The guard exists because moving a using across a directive changes which directives are compiled (`#if`) or the context they are compiled in (`#nullable`, `#pragma`). A directive in front of the first using stands in front of every using of the block whatever order they take, so a reorder moves no using across it and changes neither. The maintainer decided the issue's output question for this (option A): this is the input a fix-less diagnostic hits most often in real files, a `#nullable enable` or `#pragma warning disable` header or a `#region` around the usings.

The check stays cheap and syntactic: which token's leading trivia holds the directive, and, in front of the block, which kind of directive it is. A directive can only stand in leading trivia, so "in the leading trivia of the first token of the first directive" is exactly "in front of the block", and every other position is between or inside directives.

## Visible output changes

"Before" is today's output on `main`: the formatter's output is confirmed with `scripts/format.sh` for every case, the code fixes' with `scripts/apply-fix.sh RH7203` and `RH7207` for cases 1, 2, 3, and 7, and for case 8 with `scripts/apply-fix.sh RH7203`. The code fixes that format a using block are those of RH7201–RH7207, with their Fix All; each organizes the whole block through the using phase alone (FMT-PIPE-030), so their result is the formatter's using-phase output, shown under *Fix*. No other code fix formats a using block.

### 1. A `#nullable enable` header (the issue's case)

```csharp
#nullable enable
using System.Linq;
using System;

class C
{
    int x;
}
```

Before: RH7207 reports `System.Linq` and RH7203 reports `System`; no code fix is offered; the formatter keeps the block.

After: the same diagnostics; the code fixes and Fix All, and the formatter, give:

```csharp
#nullable enable
using System;
using System.Linq;

class C
{
    int x;
}
```

### 2. A header in front of a block in canonical order without its group blank line

```csharp
#nullable enable
using System;
using Microsoft.Win32;

class C
{
    int x;
}
```

Before: not reported — RH7207 does not check the blank lines of an unsafe block; the formatter keeps the block.

After: RH7207 reports `System`, as it reports the same block without the header (SHARED-USING-008):

```csharp
#nullable enable
using {|RH7207:System|};
using Microsoft.Win32;

class C
{
    int x;
}
```

and its fix and the formatter give:

```csharp
#nullable enable
using System;

using Microsoft.Win32;

class C
{
    int x;
}
```

### 3. A `#region` around the block

```csharp
#region Usings
using System.Linq;
using System;
#endregion

class C
{
    int x;
}
```

The `#endregion` is the leading trivia of `class C`, behind the block, and does not count.

Before: RH7207 and RH7203 report the block, no code fix is offered; the formatter keeps the order and only frames the region (FMT-BLANK-013) and completes its `#endregion` comment:

```csharp
#region Usings

using System.Linq;
using System;

#endregion // Usings

class C
{
    int x;
}
```

After, fix (the using phase alone; the region is not framed):

```csharp
#region Usings
using System;
using System.Linq;
#endregion

class C
{
    int x;
}
```

After, formatter:

```csharp
#region Usings

using System;
using System.Linq;

#endregion // Usings

class C
{
    int x;
}
```

### 4. A comment header above the directive

```csharp
// Header

#nullable enable
using System.Linq;
using System;

class C
{
    int x;
}
```

Before: reported, no code fix; the formatter keeps the block.

After, fix and formatter — the directive stays at the top although it follows the header's blank line, where FMT-USING-009 alone would move it with `System.Linq` (FMT-USING-012):

```csharp
// Header

#nullable enable
using System;
using System.Linq;

class C
{
    int x;
}
```

### 5. A comment directly below the directive

```csharp
#nullable enable
// About Linq
using System.Linq;
using System;

class C
{
    int x;
}
```

Before: reported, no code fix; the formatter keeps the block.

After, fix and formatter — the comment stays at the top with the directive, and no blank line is added (FMT-USING-012):

```csharp
#nullable enable
// About Linq
using System;
using System.Linq;

class C
{
    int x;
}
```

### 6. A comment after a blank line below the directive

```csharp
#nullable enable

// About Linq
using System.Linq;
using System;

class C
{
    int x;
}
```

Before: reported, no code fix; the formatter keeps the block.

After, fix and formatter — the comment behind the blank line stays at the top with the directive and the blank line, where FMT-USING-009 alone would move it with `System.Linq` (FMT-USING-012):

```csharp
#nullable enable

// About Linq
using System;
using System.Linq;

class C
{
    int x;
}
```

### 7. A conditional group in front of the block

Analyzed and formatted in a build that does not define `DEBUG` — the CLI's build, which defines no symbols:

```csharp
#if DEBUG
using System.Diagnostics;
#endif
using System.Linq;
using System;

class C
{
    int x;
}
```

In that build `#if DEBUG`, the disabled text `using System.Diagnostics;`, and `#endif` are in the leading trivia of `using System.Linq;`, the block's first directive.

Before and after: reported, no code fix offered; the formatter keeps the block. Nothing visible changes: a conditional directive in front of the block keeps it unsafe (SHARED-USING-009), and in a build that defines `DEBUG` the `#endif` stands between two directives, as before. The case is listed because the narrowed guard would otherwise have reordered it.

### 8. A conditional group that encloses the block

Analyzed and formatted in a build that does not define `LEGACY`:

```csharp
#if !LEGACY
using System.Linq;
using System;
#endif

class C
{
    int x;
}
```

`#if !LEGACY` is in the leading trivia of `using System.Linq;`, the block's first directive, and `#endif` in the leading trivia of `class C`, behind the block.

Before and after: reported, no code fix offered; the formatter keeps the block. Nothing visible changes: the `#if` in front of the block keeps it unsafe (SHARED-USING-009). The case is listed because the narrowed guard would otherwise have reordered it.

### Unchanged

A preprocessor directive between two usings, or between the tokens of any using, the first one included, still makes the block unsafe: reported, no code fix offered, and the formatter only splits directives that share a line. A comment header without a directive keeps FMT-USING-009's split at its last blank line.

```csharp
using
#pragma warning disable CS0105
    System.Linq;
using System;

class C
{
    int x;
}
```

A block without a preprocessor directive, a directive after the block, and a line that starts with `#` inside a comment are reported, fixed, and formatted as before.

The rule pages do not change: what the rules report and how to fix a block in the normal case stays the same, and `documentation/rules/general-notes.md` already states that a fix may be withheld where a preprocessor directive sits in the code it would change.

## Decisions

### Maintainer answer (2026-11-27, in the issue)

Option A of the issue's output question: when every preprocessor directive of a using block stands in front of its first using, RH7201–RH7207's fixes, Fix All, and the formatter reorder the block, and the directive stays on top (case 1). A block with a directive between or after its usings stays reported without a fix. The narrowed guard stays a cheap syntactic check.

### Maintainer answers (2026-11-28, in the chat)

- **Case 2** — RH7207 reports a header-led block in canonical order without its group blank line, and its fix and the formatter add the blank line.
- **Case 4** — a comment header, a blank line, and `#nullable enable` in front of the block: the fixes and the formatter reorder it, and the header and the directive stay on top.
- **Case 5** — a comment directly below the header directive stays on top as part of the header; no blank line is added.
- **Case 6** — a comment behind a blank line below the header directive stays on top as well. The answer covers blocks with a header directive only; FMT-USING-009 is unchanged for a block without one.
- **Case 7** — a conditional group in front of the block, with disabled text, keeps the block reported without a fix, and the formatter keeps it: "don't make it complicated".
- A preprocessor directive inside a using block is not declared an unsupported shape.
- **Case 8** — a conditional group that encloses the block keeps it reported without a fix, and the formatter keeps it: any `#if` in front of the block means no fix, whether or not the group's other branch holds a using.

### "After its usings" means after the first using

A directive behind the last using belongs to the next declaration and never kept the fix away (SHARED-USING-009, unchanged). The answer's "between or after its usings" contrasts with "in front of its first using", so it is read as "after the first using"; the literal reading would withdraw an existing fix nobody asked to withdraw.

### The boundary is the first token's leading trivia

A preprocessor directive makes the block unsafe unless it stands in the leading trivia of the first token of the block's first directive. Directives only ever stand in leading trivia, so this is exactly the position in front of the block; a directive in the leading trivia of a later directive stands between two directives, and one in the leading trivia of any other token of a directive, the first directive included, stands between that directive's tokens.

### A conditional directive in front of the block keeps it unsafe

Following the answers on cases 7 and 8, the block is unsafe when the leading trivia of the first token of its first directive holds a conditional directive — `#if`, `#elif`, `#else`, or `#endif`. Its group either ends in front of the block, holding code another build compiles as disabled text there, or encloses the block, which another build does not compile; the check does not judge a reorder for one build at a time, nor whether another branch holds a using. Disabled text in front of the block always follows a conditional directive in the same leading trivia, so the kind of directive is all the check reads. Every other directive in front of the block — `#nullable`, `#pragma`, `#define`, `#region` — keeps the fix (cases 1–6).

### The whole leading trivia stays at the top

Following the answers on cases 5 and 6, when the leading trivia of the original first directive holds a preprocessor directive, the whole of it — directives, comments, and blank lines — stays at the top of the block, and FMT-USING-009's split at the last blank line does not apply to it (FMT-USING-012). Keeping the directive there is what makes the reorder safe; keeping the comments and blank lines around it with it is one rule with no second split point. A leading trivia without a directive keeps FMT-USING-009 as it is.

### RH7207 checks the blank lines of such a block

RH7207 skips the blank lines of an unsafe block only because the formatter does not lay out its groups (FMT-USING-004). A block whose directives all stand in front of it is no longer unsafe, the formatter lays out its groups, and no other requirement writes a blank line between its usings, so RH7207 checks its blank lines as in any other block (case 2).

### A directive inside a using block is not declared unsupported

Declaring a preprocessor directive between the directives of a using block, or between the tokens of one, an unsupported shape would delete the formatter's line split for an unsafe block and the fixes' own safety check, but it would refuse every file with an `#if` or a `#pragma warning disable` around one using between others, which multi-target code writes commonly, and such a file would get no formatting at all. The maintainer decided not to declare it.

## Alternatives considered

- **Keep every directive unsafe** (the issue's option B) — rejected by the maintainer.
- **Split the trivia behind the last directive at its last blank line, as FMT-USING-009 does** (case 6) — the comment behind the blank line would move with `System.Linq`. Rejected by the maintainer: a block with a directive in front keeps its whole header together.
- **Move a comment directly below the header directive with the first directive** (case 5) — rejected by the maintainer.
- **Reorder a block with a conditional group in front of it in a build where the group's code is disabled text** (case 7) — no using would cross a directive in any build, but it needs a judgement per build; rejected by the maintainer.
- **Reorder a block that a conditional group encloses** (case 8) — the group leaves no disabled text in front of the block, so a check for disabled text alone would allow it; rejected by the maintainer: any `#if` in front of the block means no fix.
- **Allow a directive between the tokens of the first directive** — rejected: it stands inside the directive, not in front of the block, and reordering moves that directive's tokens past other usings.
- **Declare a directive inside a using block unsupported** — rejected by the maintainer.
