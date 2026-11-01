# A blank line above a comment does not split a using group

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | SHARED-USING-008 (changed), SHARED-USING-011 (added), FMT-USING-002 (changed), FMT-USING-010 (added), FMT-BLANK-011 (changed), RH7207-001 (changed), RH7207-002 (changed), RH7201-004, RH7202-004, RH7203-004, RH7204-004, RH7205-004, RH7206-004, RH7207-004 (changed) |
| Depends on | [The blank-line phase is the only owner of blank lines](blank-line-sole-owner.md) |

## What

A blank line above a comment that leads a using directive of the same group no longer splits the group:

- **The using rules accept it** (SHARED-USING-011). A directive that continues the group of its predecessor may be preceded by a blank line when the first comment of its leading trivia is one the comment rule separates: a comment on a line of its own, or a documentation comment, also one on the directive's line. The blank line stands above that comment, and the comment stays directly above the directive. The blank line is allowed, not required: the same directive without it is organized too, as it is today. SHARED-USING-008 keeps its rule for every other directive and points to the exception.
- **RH7207 does not report it** (RH7207-001, RH7207-002), however many blank lines stand there, as it already does not count the blank lines at a group boundary.
- **The using phase keeps it** (FMT-USING-002, FMT-USING-010). In a block it rebuilds, the phase writes exactly one blank line above the first comment of such a directive when a blank line stood directly above that comment in the source — also when the directive moves, and also when that blank line is the one that ends the file header FMT-USING-009 keeps at the top — and none otherwise. The RH7201–RH7207 code fixes run this phase on the block they fix (RH7201-004 to RH7207-004), so their output keeps the blank line too, and RH7202-004's "one organized form" includes it.
- **FMT-BLANK-011's open case is decided.** The comment rule applies to a comment that leads a using directive of the same group as to any other comment, with no new exception; FMT-BLANK-011 no longer leaves the case to SHARED-USING-008. RH5020 does not change.

An ordinary comment glued to the directive (`/* c */ using System.Linq;`, FMT-BLANK-018) is not covered: the comment rule gives it no blank line, so the using rules keep rejecting one above it, and its output is unchanged.

This implements thoenissen/Reihitsu#965 and supersedes the opposite decision thoenissen/Reihitsu#1497 was first filed with (the blank line removed, and FMT-BLANK-011 and RH5020 exempting the comment).

## Why

For a comment on its own line that leads a directive of the same group, the formatter and the using rules disagree on every layout:

- With the blank line, which is the formatter's output, RH7207 reports the block, and its code fix — the using phase alone — removes the blank line.
- Without it, RH5020 reports the comment, and the formatter's blank-line phase puts the blank line back (FMT-BLANK-011).

Inside the formatter the using phase removes the blank line and the blank-line phase inserts it again in every pass, so two phases own one blank line and undo each other. The comment rule is the general one and already decides this blank line everywhere else; letting the using rules accept it leaves one owner, gives the comment a layout that every rule accepts, and changes none of the formatter's output.

## Visible output changes

The formatter's output does not change for any input: the blank-line phase already places this blank line, and the using phase now keeps it instead of removing it first. What changes is RH7207's diagnostic and the result of the RH7201–RH7207 code fixes, which run only the using phase.

### 1. RH7207 does not report a blank line above a comment that leads a directive of the same group

The formatter's output for such a block, with a single-line comment and with a delimited documentation comment:

```csharp
using System;

// About Linq
using System.Linq;

class C;
```

```csharp
using System;

/** About Text */
using System.Text;

class C;
```

Before (`main`): RH7207 is reported on `System` of `using System;`, and its code fix removes the blank line.

After: not reported. The same holds with several blank lines above the comment.

### 2. A code fix keeps the blank line above the comment of a directive that it moves

Input, reported by RH7207 on `Microsoft.CodeAnalysis`:

```csharp
using Microsoft.CodeAnalysis;

// About Linq
using System.Linq;
using System;

class C
{
}
```

Before (`main`):

```csharp
using System;
// About Linq
using System.Linq;

using Microsoft.CodeAnalysis;

class C
{
}
```

After:

```csharp
using System;

// About Linq
using System.Linq;

using Microsoft.CodeAnalysis;

class C
{
}
```

### 3. A code fix keeps the blank line above the comment of a directive that stays in place

Input, reported by RH7207 on `Microsoft.CodeAnalysis`:

```csharp
using Microsoft.CodeAnalysis;
using System;

// About Linq
using System.Linq;

class C
{
}
```

Before (`main`):

```csharp
using System;
// About Linq
using System.Linq;

using Microsoft.CodeAnalysis;

class C
{
}
```

After:

```csharp
using System;

// About Linq
using System.Linq;

using Microsoft.CodeAnalysis;

class C
{
}
```

### 4. A code fix writes one blank line where several stood above such a comment

Input, reported by RH7207 on `System.Linq`:

```csharp
using System.Linq;
using System;



// About Text
using System.Text;

class C
{
}
```

Before (`main`):

```csharp
using System;
using System.Linq;
// About Text
using System.Text;

class C
{
}
```

After:

```csharp
using System;
using System.Linq;

// About Text
using System.Text;

class C
{
}
```

### 5. A code fix keeps the blank line that ends a file header above a comment that moves away from the top

Input, reported by RH7207 on `System.Linq`:

```csharp
// Header

// About Linq
using System.Linq;
using System;

class C
{
}
```

Before (`main`):

```csharp
// Header

using System;
// About Linq
using System.Linq;

class C
{
}
```

After:

```csharp
// Header

using System;

// About Linq
using System.Linq;

class C
{
}
```

### Unchanged

- **No blank line above the comment in the source.** A code fix writes none, also when it moves the directive into the middle of its group; the formatter's blank-line phase adds it, and RH5020 keeps reporting the comment, as it did before the fix:

  ```csharp
  using System;
  using Microsoft.CodeAnalysis;
  // About Linq
  using System.Linq;

  class C
  {
  }
  ```

  ```csharp
  using System;
  // About Linq
  using System.Linq;

  using Microsoft.CodeAnalysis;

  class C
  {
  }
  ```

- **An ordinary comment glued to the directive.** RH7207 keeps reporting the blank line above it, and the code fix and the formatter keep removing it:

  ```csharp
  using System;

  /* c */ using System.Linq;

  class C
  {
  }
  ```

  ```csharp
  using System;
  /* c */ using System.Linq;

  class C
  {
  }
  ```

- **A directive that starts a new group** keeps the group's blank line above its comment (FMT-USING-008), and a directive of the same group without a comment keeps getting no blank line.
- **The formatter's output**, for every input above and every other.

## Decisions

### The blank line is allowed, not required

The using rules accept the blank line but do not ask for it: a comment-led directive of the same group without the blank line stays organized, as RH7207 has always treated it. The comment rule (FMT-BLANK-011, RH5020) already requires the blank line, so a second rule that requires it would report the same line twice and give two owners to one decision again.

### The using phase keeps what it finds and does not predict the comment rule

The using phase writes the blank line only where one stood directly above the comment in the source; it does not insert one where none stood. Inserting it would make the using phase decide the comment rule's blank line a second time — the duplicated ownership this change removes. Where the source has none, the formatter's blank-line phase adds it as for any comment, and the code fix leaves the comment as RH5020 already reported it.

### The blank line the header keeps counts for the comment below it

When FMT-USING-009 keeps a file header at the top and the comment after the header's blank line moves away with its directive, the blank line that stood directly above that comment is the header's. It still stood above the comment, so the comment keeps one in its new place; otherwise the fix would remove the only blank line above a comment that RH5020 did not report and make it report one.

### Exactly one blank line

Where several blank lines stood above the comment, the using phase writes one, as it writes exactly one at a group boundary (FMT-USING-002). RH7207 does not report several, as it does not at a group boundary (RH7207-002): how many blank lines a run may have is decided for the whole file by the blank-line rules.

### A comment glued to the directive is not covered

An ordinary comment that shares the directive's line is glued to it (FMT-BLANK-018); FMT-BLANK-011 gives it no blank line, the formatter's output has none above it, and RH5020 keeps reporting it so the author moves it by hand. Accepting a blank line above it would change the formatter's output and give such a comment a layout of its own, which the blank-line rules deliberately do not.

## Alternatives considered

- **Remove the blank line, and exempt the comment in FMT-BLANK-011 and RH5020** (the form thoenissen/Reihitsu#1497 was first filed with) — rejected: it gives the comment rule two exceptions and changes the formatter's output for every such block; RH5020 would still have to be taught the exception to leave a clean layout.
- **Require the blank line in the using rules** — rejected: RH7207 would report a line RH5020 already reports, and the using phase would insert a blank line the blank-line phase owns.
- **Let the using phase insert the blank line above every comment that leads a directive of the same group, also where the source had none** — rejected: the using phase would predict the comment rule (FMT-BLANK-011) and the code fixes would change a comment's surroundings that the finding is not about.
- **Keep every blank line the source had above the comment** — rejected: the using phase rebuilds the block's layout and writes one blank line at every boundary it keeps; several would only be collapsed by the blank-line phase in the formatter and left in a code fix's output.
