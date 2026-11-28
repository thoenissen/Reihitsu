# A using block whose preprocessor directives all stand in front of it is reordered

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | SHARED-USING-009 (changed), FMT-USING-004 (example changed), FMT-USING-012 (added), RH7201-001, RH7202-001, RH7203-001, RH7204-001, RH7205-001, RH7206-001, RH7207-001 (clarified), RH7201-004, RH7202-004, RH7203-004, RH7204-004, RH7205-004, RH7206-004, RH7207-004 (changed) |
| Depends on | [A using block that holds a preprocessor directive is reported without a fix](using-block-directive-report.md) |

## What

SHARED-USING-009 kept the formatter and the code fixes of RH7201–RH7207 away from every using block that holds a preprocessor directive in the trivia of its directives, also from a block whose only directive stands in front of its first using, such as a `#nullable enable` header. The guard narrows: a preprocessor directive in the leading trivia of the block's first directive — in front of the block — no longer makes the block unsafe. Such a block is reordered by the formatter, fixed by the RH7201–RH7207 code fixes and Fix All, and every preprocessor directive in front of it stays at the top of the block (FMT-USING-012).

What stays: a preprocessor directive in the leading trivia of any other token of the block — in front of a later directive, which puts it between two directives, or between the tokens of a directive, the first one included — still makes the block unsafe. Such a block is reported, no code fix is offered, and the formatter only splits directives that share a line (FMT-USING-004). A directive after the last using still belongs to the next declaration and does not count.

Because such a block is no longer unsafe, RH7207 also checks its blank lines between groups, and its fix and the formatter lay them out (SHARED-USING-008, FMT-USING-002), as for a block without a directive.

## Why

The guard exists because moving a using across a directive changes which directives are compiled (`#if`) or the context they are compiled in (`#nullable`, `#pragma`). A directive in front of the first using stands in front of every using of the block whatever order they take, so a reorder moves no using across it and changes neither. The maintainer decided the issue's output question for this (option A): this is the input a fix-less diagnostic hits most often in real files, a `#nullable enable` or `#pragma warning disable` header or a `#region` around the usings.

The check stays cheap and syntactic: which token's leading trivia holds the directive. A directive can only stand in leading trivia, so "in the leading trivia of the first token of the first directive" is exactly "in front of the block", and every other position is between or inside directives.

## Visible output changes

"Before" is today's output on `main`: the formatter's output is confirmed with `scripts/format.sh` for every case, the code fixes' with `scripts/apply-fix.sh RH7203` and `RH7207` for cases 1, 2, 3, and 7. The code fixes that format a using block are those of RH7201–RH7207, with their Fix All; each organizes the whole block through the using phase alone (FMT-PIPE-030), so their result is the formatter's using-phase output, shown under *Fix*. No other code fix formats a using block.

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

After, fix and formatter — no blank line stands behind the directive, so the comment stays at the top, as FMT-USING-009 keeps a comment directly above the first directive (FMT-USING-012):

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

After, fix and formatter — the comment behind the blank line moves with its directive and keeps a blank line above it (FMT-USING-009, FMT-USING-010, FMT-USING-012):

```csharp
#nullable enable

using System;

// About Linq
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

In that build `using System.Diagnostics;` is disabled text, and the whole conditional group is the leading trivia of `using System.Linq;`, the block's first directive.

Before: reported, no code fix; the formatter keeps the block.

After, fix and formatter:

```csharp
#if DEBUG
using System.Diagnostics;
#endif
using System;
using System.Linq;

class C
{
    int x;
}
```

In a build that defines `DEBUG`, `System.Diagnostics` is the block's first directive and `#endif` stands in front of the second, so the block is unsafe there: it is reported, no code fix is offered, and the result of the other build is reported in it as before (RH7203 reports `System`, which sorts before the earlier `System.Diagnostics`). The reorder of the other build moves no using across a directive, so every build compiles the same directives as before.

### Unchanged

A preprocessor directive between two usings, or between the tokens of any using, the first one included, still makes the block unsafe: reported, no code fix offered, and the formatter only splits directives that share a line.

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

### The boundary is the first token's leading trivia

A preprocessor directive makes the block unsafe unless it stands in the leading trivia of the first token of the block's first directive. Directives only ever stand in leading trivia, so this is exactly the position in front of the block; a directive in the leading trivia of a later directive stands between two directives, and one in the leading trivia of any other token of a directive, the first directive included, stands between that directive's tokens.

### Everything up to the last directive in front of the block stays at the top

The leading trivia of the first directive is split into a header that stays at the top and the rest that moves with the directive (FMT-USING-009), by the last blank line. A preprocessor directive in that trivia always stays at the top (FMT-USING-012): moving it with the directive would move a using across it. FMT-USING-009 then decides only the trivia behind the last such directive, unchanged: the comments up to the last blank line there stay, the rest moves, and without a blank line behind the directive everything stays (case 5), the conservative choice FMT-USING-009 makes for a comment directly above the first directive.

### RH7207 checks the blank lines of such a block

RH7207 skips the blank lines of an unsafe block only because the formatter does not lay out its groups (FMT-USING-004). A block whose directives all stand in front of it is no longer unsafe, the formatter lays out its groups, and no other requirement writes a blank line between its usings, so RH7207 checks its blank lines as in any other block (case 2).

### The build decides the first directive

The check reads the directives the parsed build compiles (SHARED-DIAG-008). A conditional group in front of the block's compiled first directive is in its leading trivia, also when a using in it is compiled in another build (case 7). The reorder moves no using across a directive in any build, so it is safe in every build.

## Alternatives considered

- **Keep every directive unsafe** (the issue's option B) — rejected by the maintainer.
- **Move a comment directly below the header directive with the first directive** (case 5) — the comment would then stand above `using System.Linq;` rather than at the top. Rejected on the recommendation: FMT-USING-009 keeps a comment with no blank line below it at the top, because nothing separates it from a header; a directive above it does not change that.
- **Treat disabled text in front of the block as unsafe** (case 7) — a cheap check as well, but it would withhold the reorder for a conditional group in front of the block, which moves no using across a directive in any build. Rejected on the recommendation.
- **Allow a directive between the tokens of the first directive** — rejected: it stands inside the directive, not in front of the block, and reordering moves that directive's tokens past other usings.
