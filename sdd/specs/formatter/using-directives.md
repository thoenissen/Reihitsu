# Formatter — Using directives

| | |
|---|---|
| ID prefix | FMT-USING |
| Status | Active |
| Code owners | `UsingDirectiveOrderingPhase`, `UsingDirectiveOrderingRewriter`, `UsingGrouping`, `UsingLeadingTriviaBuilder`, `UsingTrailingTriviaBuilder` |
| Uses | SHARED-USING-001, SHARED-USING-004, SHARED-USING-007, SHARED-USING-008, SHARED-USING-009, SHARED-USING-010, SHARED-USING-011, SHARED-BLANK-001, FMT-PIPE-003, FMT-PIPE-030, FMT-BLANK-011, FMT-BLANK-013, FMT-CLEAN-001, SHARED-FIX-006, SHARED-FIX-007 |

This spec covers what the `UsingDirectiveOrderingPhase` decides on its own: how a using block is rebuilt once its canonical order is known — blank lines between groups, one directive per line, and where header, leading, and trailing comments end up. The order itself and the safety guard are the shared policy in SHARED-USING. The code fixes of RH7201–RH7207 organize the one block they fix through `ReihitsuFormatter.OrganizeUsingDirectivesInDocumentAsync`, which runs this phase alone on the block's scope (FMT-PIPE-030), so its output is also their output.

## Non-goals

- The canonical group order, the order within a group, and when a block is safe to reorder — SHARED-USING-001 to SHARED-USING-009.
- The blank line the blank-line phase inserts before a comment where none stood, also before a comment that leads a using directive, and the blank lines between the using block and the code around it (FMT-BLANK).
- The final indentation of each directive in document-level formatting (FMT-INDENT). This phase only places a split-off directive at the scope's indentation (SHARED-USING-010) and keeps the indentation at the top of a reordered block (FMT-USING-011); in the formatter's output the indentation phase decides the column, and in the output of FMT-PIPE-030 that placement is final.
- Adding, removing, or deduplicating using directives, and moving them into or out of a namespace.

## Requirements

### FMT-USING-001 — Safe blocks are rewritten into canonical order

For every using block with at least two directives that SHARED-USING-009 allows to be reordered, the formatter MUST rewrite the block into the canonical order of SHARED-USING-001 to SHARED-USING-007. Each directive MUST keep its own text; only its position and the trivia around it change.

Why: The formatter and the RH72xx code fixes must produce the order the analyzer checks, or a formatted file would still carry diagnostics. Reordering whole directives without touching their text keeps the rewrite equivalent (FMT-PIPE-003). The examples under SHARED-USING-001 to SHARED-USING-007 show the order.

### FMT-USING-002 — Exactly one blank line between groups

In a block it reorders, the formatter MUST separate two adjacent groups (SHARED-USING-004) by exactly one blank line and MUST remove every blank line between two directives of the same group, except the blank line FMT-USING-010 keeps above a comment that leads the second directive. This applies whether or not the order changed.

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

For a block that SHARED-USING-009 forbids to reorder, the using phase MUST keep the order of the directives, their blank lines, and all their trivia, and MUST only separate directives that share a line, as in FMT-USING-003. It MUST NOT add group-separating blank lines to such a block. This binds the using phase only: blank lines that another requirement writes inside such a block, such as those that frame a `#region` or `#endregion` between its directives (FMT-BLANK-013), are not affected.

Why: Splitting a line moves no directive across a preprocessor directive and changes nothing that is compiled, so it is safe where reordering is not. Adding blank lines between groups would only half-organize a block whose order the formatter is not allowed to fix. The requirement is scoped to the using phase because the blank-line phase runs later over the whole document and frames a region inside the block as it frames any region, so the formatter's output can hold blank lines in such a block that the using phase did not write.

Example:

```csharp
using System.Linq; using System;
#pragma warning disable CS0105
using Microsoft.CodeAnalysis;

class C
{
    int x;
}
```

```csharp
using System.Linq;
using System;
#pragma warning disable CS0105
using Microsoft.CodeAnalysis;

class C
{
    int x;
}
```

and a block with disabled text in front of it is unsafe as well (SHARED-USING-009), so in a build that does not define `DEBUG` only its shared line is split:

```csharp
#if DEBUG
using System.Diagnostics;
#endif
using System.Linq; using System;

class C
{
    int x;
}
```

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

When the reorder moves the block's original first directive away from the top, the comments in its leading trivia up to and including the last blank line among them MUST stay at the top of the block, and only the comments after that blank line MUST move with the directive. When no blank line separates the comments from the directive, all of them MUST stay at the top. What counts as a blank line is SHARED-BLANK-001. When that leading trivia holds a preprocessor directive, FMT-USING-012 applies instead.

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

and a line that holds only whitespace ends the header like an empty line — in the input, the line below `// Header` holds four spaces; the comment after it moves with its directive and keeps the blank line above it (FMT-USING-010), and the cleanup phase removes the four spaces (FMT-CLEAN-001):

```csharp
// Header
    
// About Linq
using System.Linq;
using System;

class C
{
    int x;
}
```

```csharp
// Header

using System;

// About Linq
using System.Linq;

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

### FMT-USING-010 — The blank line above a comment that leads a directive of the same group is kept

In a block it reorders, the formatter MUST place exactly one blank line above the first comment that leads a directive continuing the group of its predecessor when, in the source, a blank line stood directly above that comment, and MUST place none when no blank line stood there. Every comment counts, on a line of its own or on the directive's line (SHARED-USING-011). This applies whether the directive moves or stays, and whether or not the order changed. When the directive is the block's original first directive and the comment is the first that moves with it (FMT-USING-009), the blank line that ends the header kept at the top stood directly above the comment and counts. The trivia from that comment to the directive is kept as written (FMT-USING-008).

Why: SHARED-USING-011 accepts this blank line; removing it would make this phase undo the blank-line phase (FMT-BLANK-011) in every pass and leave the RH7201–RH7207 code fixes with an output that RH5020 reports for an ordinary comment. The phase keeps what the author wrote instead of inserting the blank line itself, so the blank line above a comment keeps one owner, the blank-line phase, which inserts it where none stood. Where none stood, the code fixes therefore leave the comment without one, and formatting their result may add it: that blank line lies between the comment and the code above, which is not what the using rules report, so SHARED-FIX-006 and the *Blank lines* bullet of SHARED-FIX-007 keep it as the author wrote it. Several blank lines become one, as at a group boundary (FMT-USING-002).

Example — the blank line stays, also when the block is reordered around it:

```csharp
using Microsoft.CodeAnalysis;
using System;

// About Linq
using System.Linq;

class C
{
    int x;
}
```

```csharp
using System;

// About Linq
using System.Linq;

using Microsoft.CodeAnalysis;

class C
{
    int x;
}
```

and when its directive moves:

```csharp
using Microsoft.CodeAnalysis;

// About Linq
using System.Linq;
using System;

class C
{
    int x;
}
```

```csharp
using System;

// About Linq
using System.Linq;

using Microsoft.CodeAnalysis;

class C
{
    int x;
}
```

and above a comment on the directive's line:

```csharp
using Microsoft.CodeAnalysis;
using System;

/* c */ using System.Linq;

class C
{
    int x;
}
```

```csharp
using System;

/* c */ using System.Linq;

using Microsoft.CodeAnalysis;

class C
{
    int x;
}
```

and several blank lines become one:

```csharp
using System.Linq;
using System;



// About Text
using System.Text;

class C
{
    int x;
}
```

```csharp
using System;
using System.Linq;

// About Text
using System.Text;

class C
{
    int x;
}
```

and the blank line that ends a header counts for the comment that moves away from the top:

```csharp
// Header

// About Linq
using System.Linq;
using System;

class C
{
    int x;
}
```

```csharp
// Header

using System;

// About Linq
using System.Linq;

class C
{
    int x;
}
```

while a comment without a blank line above it in the source gets none, also when its directive moves into the middle of its group:

```csharp
using System;
using Microsoft.CodeAnalysis;
// About Linq
using System.Linq;

class C
{
    int x;
}
```

```csharp
using System;
// About Linq
using System.Linq;

using Microsoft.CodeAnalysis;

class C
{
    int x;
}
```

### FMT-USING-011 — A header changes no directive's indentation

In a block it reorders, the directive that takes the block's first position MUST start at the whitespace that stands at the start of the line on which the block's original first directive stands, kept as written, a tab included. When a comment leads that directive (FMT-USING-008), its first comment MUST start at that whitespace instead, and the lines below that comment keep their own. When the original first directive moves away from the top, it MUST keep the whitespace at the start of its own line, and every comment that moves with it (FMT-USING-009) the whitespace at the start of its own line. This holds:

- whether or not comments stand above the original first directive — a header kept at the top (FMT-USING-009) changes no directive's indentation, and its own lines stay as written;
- whether or not a blank line separates those comments from the directive, so on both sides of the split FMT-USING-009 makes;
- in every scope the phase organizes — a compilation unit, a file-scoped namespace, and a namespace with a body, at any depth — also where the directives of a compilation unit or a file-scoped namespace are indented.

A directive that shared its line with its predecessor is placed as SHARED-USING-010 states.

Why: The comments above the first directive are a header or belong to one directive (FMT-USING-009); they say nothing about where the block is indented. The same block without them is organized with every directive at the original first directive's whitespace, and the comments must not change that. The RH7201–RH7207 code fixes take this phase's output as their final layout (FMT-PIPE-030), so a directive the phase leaves in column 0 stays there in the fix's result; in a namespace with a body that is not the column the formatter gives it (SHARED-FIX-007). The whitespace is kept as written, a tab included, as the fix keeps it in front of the same block without the comments; replacing a tab is the preparation phase's decision, not this phase's (SHARED-FIX-007, *Whitespace in front of the changed code*).

Example — the output of `OrganizeUsingDirectivesInDocumentAsync` with the namespace as scope, as the RH7203 fix passes it; the header stays at the top, and both directives stand in column 4:

```csharp
namespace N
{
    // Header
    using System.Linq;
    using System;

    class C
    {
        int x;
    }
}
```

```csharp
namespace N
{
    // Header
    using System;
    using System.Linq;

    class C
    {
        int x;
    }
}
```

and, on the other side of the split, with a blank line below the header, the directive that takes the first position stands in column 4 below the blank line:

```csharp
namespace N
{
    // Header

    using System.Linq;
    using System;

    class C
    {
        int x;
    }
}
```

```csharp
namespace N
{
    // Header

    using System;
    using System.Linq;

    class C
    {
        int x;
    }
}
```

and when a comment of its own leads the directive that takes the first position, that comment stands in column 4:

```csharp
namespace N
{
    // Header

    using System.Linq;
    // About System
    using System;

    class C
    {
        int x;
    }
}
```

```csharp
namespace N
{
    // Header

    // About System
    using System;
    using System.Linq;

    class C
    {
        int x;
    }
}
```

The same holds without the header: both blocks above without `// Header` are organized with both directives in column 4. When the namespace's lines are indented with a tab instead of four spaces, the tab stays in front of `// Header`, `using System;`, and `using System.Linq;`.

In a file-scoped namespace, or a compilation unit, whose directives are indented, the directives keep that indentation as well:

```csharp
namespace N;

    // Header
    using System.Linq;
    using System;

class C
{
    int x;
}
```

```csharp
namespace N;

    // Header
    using System;
    using System.Linq;

class C
{
    int x;
}
```

Formatting the whole document then moves the comment and both directives to column 0 (FMT-INDENT); the code fixes do not.

### FMT-USING-012 — The leading trivia of a block with a directive in front stays at the top

When the leading trivia of a reordered block's original first directive holds a preprocessor directive (SHARED-USING-009) and the reorder moves that using directive away from the top, the whole leading trivia — every preprocessor directive, comment, and blank line in it — MUST stay at the top of the block, in its order, and none of it moves with the using directive. FMT-USING-009's split at the last blank line does not apply to such trivia; it applies unchanged to a leading trivia that holds no preprocessor directive. The trivia that stays at the top is the header of FMT-USING-009, so FMT-USING-010 and FMT-USING-011 treat it as they treat that header. A block whose first directive keeps its position keeps its leading trivia as written, as FMT-USING-009 states.

Why: A preprocessor directive in front of the block stands in front of every directive of the block; only because it stays at the top does a reorder move no using across it (SHARED-USING-009), so no split may carry it away with the using directive, also when it follows a header's blank line. The maintainer decided that the comments around it stay at the top as well, also a comment behind a blank line below the directive, rather than splitting the trivia once more behind the last directive: a block with a directive in front keeps its whole header together, which is one rule with no second split point.

Example — a directive behind a header's blank line stays at the top, where FMT-USING-009 alone would move it with `System.Linq`:

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

and a blank line behind the directive stays behind it:

```csharp
#nullable enable

using System.Linq;
using System;

class C
{
    int x;
}
```

```csharp
#nullable enable

using System;
using System.Linq;

class C
{
    int x;
}
```

and a comment directly below the directive stays at the top, with no blank line added:

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

and a comment behind a blank line below the directive stays at the top as well, where FMT-USING-009 alone would move it with `System.Linq`:

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

Example — the formatter frames a `#region` in front of the block as any region (FMT-BLANK-013); the `#endregion` behind the block is the leading trivia of `class C` and stays where it is:

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

The output of FMT-PIPE-030, which the RH7201–RH7207 code fixes use, keeps the region as written: `#region Usings` directly above `using System;`, and `#endregion` directly below `using System.Linq;`.

A leading trivia without a preprocessor directive keeps FMT-USING-009's split: a comment behind the header's blank line moves with its directive, as FMT-USING-009's examples show.
