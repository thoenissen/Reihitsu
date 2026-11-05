# A blank line above a comment does not split a using group

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | SHARED-USING-008 (changed), SHARED-USING-011 (added), FMT-USING-002 (changed), FMT-USING-010 (added), FMT-BLANK-011 (changed), RH7207-001 (changed), RH7207-002 (changed), RH7201-004, RH7202-004, RH7203-004, RH7204-004, RH7205-004, RH7206-004, RH7207-004 (changed) |
| Depends on | [The blank-line phase is the only owner of blank lines](blank-line-sole-owner.md) |

## What

A blank line above a comment that leads a using directive of the same group no longer splits the group:

- **The using rules accept it** (SHARED-USING-011). A directive that continues the group of its predecessor may be preceded by a blank line when a comment leads it — any comment, on a line of its own or on the directive's line. Only the line breaks in front of the first comment count; the blank lines between the comment and the directive are not the using rules' to decide. The blank line is allowed, not required: the same directive without it is organized too, as it is today. SHARED-USING-008 keeps its rule for every directive without a leading comment and points to the exception.
- **RH7207 does not report it** (RH7207-001, RH7207-002), however many blank lines stand there, as it already does not count the blank lines at a group boundary.
- **The using phase keeps it** (FMT-USING-002, FMT-USING-010). In a block it rebuilds, the phase writes exactly one blank line above the first comment of such a directive when a blank line stood directly above that comment in the source — also when the directive moves, and also when that blank line is the one that ends the file header FMT-USING-009 keeps at the top — and none otherwise. The RH7201–RH7207 code fixes run this phase on the block they fix (RH7201-004 to RH7207-004), so their output keeps the blank line too; their `Why:` no longer speaks of one organized form, because the form keeps the author's choice here.
- **FMT-BLANK-011's open case is decided.** The comment rule applies to a comment that leads a using directive of the same group as to any other comment, with no new exception; FMT-BLANK-011 no longer leaves the case to SHARED-USING-008. RH5020 does not change.

This includes an ordinary comment glued to the directive (`/* c */ using System.Linq;`, FMT-BLANK-018). The blank-line phase inserts no blank line above it but keeps the author's, so once the using phase stops removing it, the formatter keeps the blank line the author wrote there — the one formatter output this change alters (visible output change 6). RH5020 keeps reporting such a comment without a blank line above it, and its fix inserts one, which RH7207 now accepts.

This implements thoenissen/Reihitsu#965 and supersedes the opposite decision thoenissen/Reihitsu#1497 was first filed with (the blank line removed, and FMT-BLANK-011 and RH5020 exempting the comment).

## Why

For a comment that leads a directive of the same group, the formatter and the using rules disagree on every layout:

- With the blank line — the formatter's output for a comment on its own line, and the RH5020 fix's output for an ordinary comment — RH7207 reports the block, and its code fix, the using phase alone, removes the blank line.
- Without it, RH5020 reports an ordinary comment and its fix inserts the blank line again, and the formatter's blank-line phase puts it back above a comment on its own line and above a documentation comment (FMT-BLANK-011).

Inside the formatter the using phase removes the blank line and the blank-line phase inserts it again in every pass, so two phases own one blank line and undo each other, and the RH5020 and RH7207 fixes undo each other too. The blank-line rules are the general ones and already decide the blank line above a comment everywhere else; letting the using rules accept it leaves one owner and gives every such comment a layout that every rule accepts.

## Visible output changes

The maintainer accepted every entry (see *Decisions*). The formatter's output changes only for a blank line above an ordinary comment on the directive's line (entry 6); for every other comment the blank-line phase already places this blank line, and the using phase now keeps it instead of removing it first. Otherwise what changes is RH7207's diagnostic and the result of the RH7201–RH7207 code fixes, which run only the using phase.

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

### 6. The blank line above an ordinary comment on the directive's line is kept

Input:

```csharp
using System;

/* c */ using System.Linq;

class C
{
}
```

Before (`main`): RH7207 is reported on `System`; its code fix and the formatter remove the blank line (the formatter also writes `class C;`):

```csharp
using System;
/* c */ using System.Linq;

class C
{
}
```

After: not reported; the code fix is not offered, and the formatter keeps the blank line:

```csharp
using System;

/* c */ using System.Linq;

class C;
```

The same input without the blank line keeps its output: the formatter inserts none, RH5020 reports the comment, and its fix inserts the blank line, which RH7207 no longer reports.

### Unchanged

- **No blank line above the comment in the source.** A code fix writes none, also when it moves the directive into the middle of its group; formatting the fix's result adds it, and RH5020 keeps reporting the comment, as it did before the fix (maintainer answer 6):

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

- **A directive that starts a new group** keeps the group's blank line above its comment (FMT-USING-008), and a directive of the same group without a comment keeps getting no blank line.
- **The formatter's output**, for every input other than entry 6.

## Decisions

### The maintainer's answers to the run's output questions

Answered on 2026-11-01 in the run of thoenissen/Reihitsu#1497:

1. RH7207 does not report one or more blank lines above a comment that leads a directive of the same group (entry 1) — accepted.
2. A code fix keeps the blank line above the comment of a directive it moves (entry 2) — accepted.
3. A code fix keeps that blank line for a directive that stays in place (entry 3) — accepted.
4. A code fix writes exactly one blank line where several stood (entry 4) — accepted.
5. A code fix keeps the blank line that ends a file header above a comment that moves away from the top (entry 5) — accepted.
6. A comment-led directive that a code fix moves into the middle of its group, with no blank line above the comment in the source: the fix writes none, and RH5020 keeps reporting the comment (*Unchanged*).
7. A blank line above an ordinary comment on the directive's line: RH7207 does not report it, and the RH7201–RH7207 code fixes and the formatter keep it; RH5020 still reports the comment without one (entry 6). This departs from the issue's statement that the formatter's output does not change.

### The blank line is allowed, not required

The using rules accept the blank line but do not ask for it: a comment-led directive of the same group without the blank line stays organized, as RH7207 has always treated it. Whether a comment needs a blank line above it is the blank-line rules' decision — FMT-BLANK-011 in the formatter, and RH5020 for an ordinary comment in the analyzer — so a using rule that required it would give one blank line two owners again.

### Every leading comment counts

The allowance does not depend on the kind or position of the comment. A comment glued to the directive gets no blank line from the blank-line phase, but the phase keeps the author's (FMT-BLANK-018), and RH5020 reports an ordinary one without a blank line, so its fix inserts one. If the using rules rejected the blank line above it, the RH5020 and RH7207 fixes would undo each other there. Accepting every leading comment also keeps the using rules free of the blank-line rules' comment classification, so no second copy of it is needed.

### The using phase keeps what it finds and does not predict the comment rule

The using phase writes the blank line only where one stood directly above the comment in the source; it does not insert one where none stood. Inserting it would make the using phase decide the comment rule's blank line a second time — the duplicated ownership this change removes. Where the source has none, the formatter's blank-line phase adds it as for any comment, and a code fix leaves the comment as RH5020 already reported it (answer 6). Formatting such a fix's result therefore adds the blank line; that line lies between the comment and the code above it, not in what the using rules report, so SHARED-FIX-006 and the *Blank lines* bullet of SHARED-FIX-007 keep it as the author wrote it. The issue's motivation that formatting the output of an RH7201–RH7207 fix changes nothing is met for every input that had the blank line, and narrowed by this case.

### The blank line the header keeps counts for the comment below it

When FMT-USING-009 keeps a file header at the top and the comment after the header's blank line moves away with its directive, the blank line that stood directly above that comment is the header's. It still stood above the comment, so the comment keeps one in its new place; otherwise the fix would remove the only blank line above a comment and make RH5020 report it.

### Exactly one blank line

Where several blank lines stood above the comment, the using phase writes one, as it writes exactly one at a group boundary (FMT-USING-002). RH7207 does not report several, as it does not at a group boundary (RH7207-002): how many blank lines a run may have is decided for the whole file by the blank-line rules.

### Only the line breaks in front of the first comment count

As SHARED-USING-008 already states, the using rules count only the line breaks in front of the first comment that leads a directive. A blank line below the comment or between two comments is not decided by them; the formatter keeps such lines, and RH7207 does not report them.

### The organized form leaves the author a choice

RH7201-004 to RH7207-004 said that a block has one organized form. With a blank line that is allowed but not required, a block has one organized form for each source: the fixes and the formatter keep the author's choice instead of making one. Their `Why:` says so.

## Alternatives considered

- **Remove the blank line, and exempt the comment in FMT-BLANK-011 and RH5020** (the form thoenissen/Reihitsu#1497 was first filed with) — rejected: it gives the comment rule two exceptions and changes the formatter's output for every such block.
- **Require the blank line in the using rules** — rejected: RH7207 would report a line the blank-line rules already decide, and the using phase would insert a blank line the blank-line phase owns.
- **Accept the blank line only above a comment that the blank-line phase separates, not above an ordinary comment glued to the directive** — rejected by the maintainer's answer 7: it keeps the formatter's output unchanged, but RH7207 would keep reporting the RH5020 fix's output there and the two fixes would undo each other, and the using rules would need a copy of the blank-line rules' comment classification.
- **Let the using phase insert the blank line above every comment that leads a directive of the same group, also where the source had none** — rejected by the maintainer's answer 6: the using phase would predict the comment rule (FMT-BLANK-011), and every code fix on a block would change the surroundings of comments the finding is not about.
- **Keep every blank line the source had above the comment** — rejected: the using phase rebuilds the block's layout and writes one blank line at every boundary it keeps; several would only be collapsed by the blank-line phase in the formatter and left in a code fix's output.
