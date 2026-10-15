# Formatter — Using directives

| | |
|---|---|
| ID prefix | FMT-USING |
| Status | Active |
| Code owners | `UsingDirectiveOrderingPhase`, `UsingDirectiveOrderingRewriter`, `UsingGrouping`, `UsingLeadingTriviaBuilder`, `UsingTrailingTriviaBuilder` |
| Uses | SHARED-USING-001, SHARED-USING-002, SHARED-USING-003, SHARED-USING-004, SHARED-USING-005, SHARED-USING-006, SHARED-USING-007, SHARED-USING-008, SHARED-USING-009, SHARED-USING-010, FMT-PIPE-002, FMT-PIPE-003, FMT-PIPE-007 |

This spec covers what the `UsingDirectiveOrderingPhase` decides on its own: how a using block is rebuilt once its canonical order is known — blank lines between groups, one directive per line, and where header, leading, and trailing comments end up. The order itself and the safety guard are the shared policy in SHARED-USING. The code fixes of RH7201–RH7207 run this phase on the one block they fix, so its output is also their output.

## Non-goals

- The canonical group order, the order within a group, and when a block is safe to reorder — SHARED-USING-001 to SHARED-USING-009.
- The blank line the blank-line phase inserts before a comment, also before a comment that leads a using directive, and the blank lines between the using block and the code around it (FMT-BLANK).
- The final indentation of each directive (FMT-INDENT). This phase only places a split-off directive at the scope's indentation (SHARED-USING-010); the indentation phase decides the column.
- Adding, removing, or deduplicating using directives, and moving them into or out of a namespace.

## Requirements

### FMT-USING-001 — Safe blocks are rewritten into canonical order

For every using block with at least two directives that SHARED-USING-009 allows to be reordered, the formatter MUST rewrite the block into the canonical order of SHARED-USING-001 to SHARED-USING-007. Each directive MUST keep its own text; only its position and the trivia around it change.

Why: The formatter and the RH72xx code fixes must produce the order the analyzer checks, or a formatted file would still carry diagnostics. Reordering whole directives without touching their text keeps the rewrite equivalent (FMT-PIPE-003). The examples under SHARED-USING-001 to SHARED-USING-007 show the order.

### FMT-USING-002 — Exactly one blank line between groups

In a block it reorders, the formatter MUST separate two adjacent groups (SHARED-USING-004) by exactly one blank line and MUST remove every blank line between two directives of the same group, also one above a comment that leads the second directive. This applies whether or not the order changed. The blank line the blank-line phase later inserts before such a comment is not this phase's decision (see Non-goals).

Why: The block's layout is derived from its groups alone, which is what SHARED-USING-008 and RH7207 require. Rebuilding it from scratch also means stray blank lines inside a group and extra blank lines between groups disappear in one pass.

Example:

```csharp
using System;
using System.Linq;



using Microsoft.CodeAnalysis;

class C
{
    int x;
}
```

```csharp
using System;
using System.Linq;

using Microsoft.CodeAnalysis;

class C
{
    int x;
}
```

and a blank line inside a group is removed while one is added between groups:

```csharp
using System;

using System.Linq;
using Microsoft.CodeAnalysis;

class C
{
    int x;
}
```

```csharp
using System;
using System.Linq;

using Microsoft.CodeAnalysis;

class C
{
    int x;
}
```

### FMT-USING-003 — One directive per line

The formatter MUST place every using directive on a line of its own. A directive that shares a line with its predecessor MUST get a line break before it — replacing the whitespace that separated the two — and MUST be placed at the scope's indentation (SHARED-USING-010). When the two directives belong to different groups, the blank line of FMT-USING-002 is added as well.

Why: One directive per line is what RH5114 requires, and it is the only layout in which the group structure and the per-directive comments of this phase are well defined.

Example:

```csharp
using System; using System.Linq;

class C
{
    int x;
}
```

```csharp
using System;
using System.Linq;

class C
{
    int x;
}
```

and across groups:

```csharp
using Microsoft.CodeAnalysis; using System;

class C
{
    int x;
}
```

```csharp
using System;

using Microsoft.CodeAnalysis;

class C
{
    int x;
}
```

### FMT-USING-004 — Unsafe blocks only get one directive per line

For a block that SHARED-USING-009 forbids to reorder, the formatter MUST keep the order of the directives, their blank lines, and all their trivia, and MUST only separate directives that share a line, as in FMT-USING-003. It MUST NOT add group-separating blank lines to such a block.

Why: Splitting a line moves no directive across a preprocessor directive and changes nothing that is compiled, so it is safe where reordering is not. Adding blank lines between groups would only half-organize a block whose order the formatter is not allowed to fix.

Example:

```csharp
#nullable enable
using System.Linq; using System;
using Microsoft.CodeAnalysis;

class C
{
    int x;
}
```

```csharp
#nullable enable
using System.Linq;
using System;
using Microsoft.CodeAnalysis;

class C
{
    int x;
}
```

### FMT-USING-005 — A line break inside a comment already separates

The formatter MUST NOT insert a line break between two directives of the same group that were neighbors in the source when the gap between them already contains a line break, also one inside a multi-line block or documentation comment. When the reorder makes two directives neighbors that were not, only a block comment trailing the first directive or a documentation comment leading the second that spans lines counts, because the rest of the gap was written for other neighbors.

Why: The second directive already starts on a later line than the first, so the layout is one directive per line in the sense RH5114 checks. Breaking the line again would split the comment's closing line from the directive the user wrote it in front of.

Example:

```csharp
using System; /* first
   second */ using System.Linq;

class C
{
    int x;
}
```

is left unchanged.

### FMT-USING-006 — Trailing comments travel with their directive

A comment that trails a directive on its line MUST stay on that directive's line when the directive moves. When the moved directive is no longer the last of the block, a single-line comment it carries MUST be followed by a line break, and when it shares a line with a following directive, a block comment it carries MUST stay on its line and the line break MUST come after the comment.

Why: A trailing comment describes the directive it trails, so it moves with it. A `//` comment runs to the end of its line, so it must be closed before the next directive or it would swallow that directive.

Example:

```csharp
using System.Text; // text
using System.Linq; // linq
using System;

class C
{
    int x;
}
```

```csharp
using System;
using System.Linq; // linq
using System.Text; // text

class C
{
    int x;
}
```

and a block comment stays on the line it trails:

```csharp
using System; /* note */ using Microsoft.CodeAnalysis;

class C
{
    int x;
}
```

```csharp
using System; /* note */

using Microsoft.CodeAnalysis;

class C
{
    int x;
}
```

### FMT-USING-007 — The block keeps its ending

The trivia that ended the original last directive — its line break or the whitespace before the next token — MUST end the rebuilt block, whichever directive is last after the reorder. A single-line comment carried by the new last directive MUST be closed by a line break before that ending when the ending contains none.

Why: What follows the block (a blank line, a namespace, the end of the file, a closing brace) was laid out for the end of the block, not for one particular directive. Moving the ending with the directive would leave the rest of the file with a layout it never had.

Example — `using System;` moves up with its `// core` comment, which gets a line break of its own, while the block's ending stays after `using System.Text;`:

```csharp
using System.Text;
using System; // core

class C
{
    int x;
}
```

```csharp
using System; // core
using System.Text;

class C
{
    int x;
}
```

and the block's ending is kept even when nothing separates it from the next declaration:

```csharp
using System.Text;
using System;
using System.Linq;
class C
{
    int x;
}
```

```csharp
using System;
using System.Linq;
using System.Text;
class C
{
    int x;
}
```

### FMT-USING-008 — Leading comments travel with their directive

A comment on the lines directly above a directive that is not the block's first MUST move with that directive and stay directly above it. When the directive starts a new group, the group's blank line MUST be placed above the comment.

Why: A comment above a directive is written for that directive, like a trailing one. Placing the separator above the comment keeps the comment attached to its directive and the group boundary visible (SHARED-USING-008).

Example:

```csharp
using Alpha.Zeta;
// Keep with Alpha
using Alpha.Alpha;

class C
{
    int x;
}
```

```csharp
// Keep with Alpha
using Alpha.Alpha;
using Alpha.Zeta;

class C
{
    int x;
}
```

and across groups:

```csharp
using Reihitsu.Core;
// About Analysis
using Microsoft.CodeAnalysis;
using System;

class C
{
    int x;
}
```

```csharp
using System;

// About Analysis
using Microsoft.CodeAnalysis;

using Reihitsu.Core;

class C
{
    int x;
}
```

### FMT-USING-009 — The header above the first directive stays at the top

When the reorder moves the block's original first directive away from the top, the comments in its leading trivia up to and including the last blank line among them MUST stay at the top of the block, and only the comments after that blank line MUST move with the directive. When no blank line separates the comments from the directive, all of them MUST stay at the top.

Why: The comments above the first using of a file are most often a file header — a copyright banner or a description of the file — and do not belong to whichever using happens to be first. A blank line is the only signal that separates a header from a comment written for the directive; without it, keeping the comment in place is the conservative choice.

Example:

```csharp
// Copyright header

using System.Linq;
using System;

class C
{
    int x;
}
```

```csharp
// Copyright header

using System;
using System.Linq;

class C
{
    int x;
}
```

and a comment after the header's blank line moves with its directive:

```csharp
// Header

// About Analysis
using Microsoft.CodeAnalysis;
using System;

class C
{
    int x;
}
```

```csharp
// Header

using System;

// About Analysis
using Microsoft.CodeAnalysis;

class C
{
    int x;
}
```

while a comment directly above the first directive stays at the top:

```csharp
// About Linq
using System.Linq;
using System;

class C
{
    int x;
}
```

```csharp
// About Linq
using System;
using System.Linq;

class C
{
    int x;
}
```
