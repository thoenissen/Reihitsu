# A using block that holds a preprocessor directive is reported without a fix

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | SHARED-USING-009 (changed), RH7201-001, RH7202-001, RH7203-001, RH7204-001, RH7205-001, RH7206-001, RH7207-001 (changed), RH7201-004, RH7202-004, RH7203-004, RH7204-004, RH7205-004, RH7206-004, RH7207-004 (clarified) |
| Depends on | [Rules judge the code the current build compiles](report-across-conditional-directives.md) |

## What

SHARED-USING-009 used to keep the using-ordering rules RH7201–RH7207 silent on a using block that holds a preprocessor directive of any kind in the trivia of its directives — in front of its first directive, between two of them, or between the tokens of one. The silence is lifted: the analyzers report such a block as their own requirements state, judging the directives the analyzed build compiles (SHARED-DIAG-008). A using directive that a conditional directive excludes is absent, and every directive the build compiles is compared with the other directives of its block whatever preprocessor directives lie between them, as SHARED-USING-007 already states for directives of other groups.

What stays: neither the formatter nor a code fix reorders such a block. The code fixes of RH7201–RH7207 are not offered for a diagnostic in it, Fix All leaves it unchanged, and the formatter only splits directives that share a line (FMT-USING-004). The block is corrected by hand.

RH7207 checks two things, the order and the blank lines between groups. In a block that holds a preprocessor directive it reports only the order: the formatter keeps the blank lines of such a block (FMT-USING-004), so a missing or an extra blank line there is a shape the formatter keeps, which a layout check does not report (SHARED-DIAG-010).

## Why

The using-ordering rules judge the order of directives, which is content, not layout. SHARED-DIAG-010 lets a content rule report what it judges also where the formatter keeps the code, and *Code-fix coverage* in `AGENTS.md` makes a diagnostic without a fix a supported end state. SHARED-USING-009 gave a different reason for its silence: a reported order that the fix and the formatter may not produce would leave a diagnostic the user cannot resolve automatically. That reason is the one the decision behind [Rules judge the code the current build compiles](report-across-conditional-directives.md) overruled for every other rule — a fix that would have to move code across a directive is withheld, and the diagnostic stays. That change kept SHARED-USING-009 on its stated reason; this change removes the last rule family that stays silent for it.

The order of a block that holds a directive can always be corrected by hand: a directive in front of the block stays in front of it, and a conditional group can be moved as a whole or split into several groups. General notes already tell the user that a diagnostic may be reported without a fix where a preprocessor directive sits in the code a fix would have to change.

## Visible output changes

Every input below is analyzed in a build that does not define `DEBUG`, unless the case says otherwise. "Before" is today's output on `main`, confirmed with `scripts/apply-fix.sh` for the first and the last two cases. The formatter's output does not change for any of them, and no code fix that formats a using block changes its result: the fixes of RH7201–RH7207 are the only ones that format a block, and they stay withheld for it.

### 1. A conditional group between two directives

```csharp
using System.Linq;
#if DEBUG
using System.Diagnostics;
#endif
using System;

class C
{
}
```

Before: not reported.

After: reported, no code fix offered; the formatter keeps the block as written.

```csharp
using {|RH7207:System.Linq|};
#if DEBUG
using System.Diagnostics;
#endif
using {|RH7203:System|};

class C
{
}
```

In a build that defines `DEBUG`, RH7203 also reports `System.Diagnostics`, which sorts before the earlier `System.Linq`.

### 2. A directive in front of the block

```csharp
#nullable enable
using System.Linq;
using System;

class C
{
}
```

Before: not reported.

After: reported, no code fix offered; the formatter keeps the block as written.

```csharp
#nullable enable
using {|RH7207:System.Linq|};
using {|RH7203:System|};

class C
{
}
```

### 3. Each order rule

Before: none of these blocks is reported. After: each is reported as marked, and no code fix is offered; the formatter keeps every block as written. Where RH7207 reports the same span as another rule, the text below the block says so.

RH7201:

```csharp
using {|RH7207:Microsoft.CodeAnalysis|};
#if DEBUG
using System.Diagnostics;
#endif
using {|RH7201:System|};

class C
{
}
```

RH7202:

```csharp
using {|RH7202:A|} = {|RH7207:System.Text|};
#region Imports
using System;
#endregion

class C
{
}
```

RH7204:

```csharp
using B = {|RH7207:System.Text|};
#pragma warning disable CS0105
using {|RH7204:A|} = System.IO;

class C
{
}
```

RH7205 — RH7207 reports `System.Math` too:

```csharp
using static {|RH7205:System.Math|};
#region Imports
using System;
#endregion

class C
{
}
```

RH7206 — RH7207 reports `System.Math`:

```csharp
using static System.Math;
#if DEBUG
using System.Diagnostics;
#endif
using static {|RH7206:System.Console|};

class C
{
}
```

In a build that defines `DEBUG`, RH7205 reports `System.Math` in the last block as well, because the compiled `System.Diagnostics` is a regular directive below it.

### 4. A directive between the tokens of a using directive

```csharp
using System.Linq;
using System
#pragma warning disable CS0105
    .Text;
using System;

class C
{
}
```

Before: not reported.

After: reported, no code fix offered.

```csharp
using {|RH7207:System.Linq|};
using System
#pragma warning disable CS0105
    .Text;
using {|RH7203:System|};

class C
{
}
```

### 5. Conditional directives at the end of the block

```csharp
using System;
using Microsoft.Win32;
#if DEBUG
using System.Diagnostics;
#endif

class C
{
}
```

In a build that does not define `DEBUG`, the conditional group is the leading trivia of `class C`, after the block, and nothing changes: RH7207 reports the missing blank line in front of `Microsoft.Win32` and its fix adds it, as today. In a build that defines `DEBUG`:

Before: not reported.

After: reported, no code fix offered.

```csharp
using {|RH7207:System|};
using Microsoft.Win32;
#if DEBUG
using {|RH7201:System.Diagnostics|};
#endif

class C
{
}
```

### Unchanged

RH7207 does not report the blank lines of a block that holds a directive. This block is in canonical order but has no blank line in front of `Microsoft.Win32`, and stays unreported:

```csharp
using System;
#if DEBUG
using System.Diagnostics;
#endif
using Microsoft.Win32;

class C
{
}
```

A block without a preprocessor directive, a directive after the block, and a line that starts with `#` inside a comment are reported, fixed, and formatted as before.

The rule pages do not change. `documentation/rules/general-notes.md` already states that a diagnostic may be reported without a code fix where a preprocessor directive sits in the code a fix would have to change.

## Decisions

### Every kind of preprocessor directive

The silence is lifted for every directive kind SHARED-USING-009 names — conditional directives, `#nullable`, `#pragma`, `#region`, `#endregion`, `#define`, and the rest — not only for conditional directives. The silence had one reason for all of them, the fix that may not reorder, and that reason no longer keeps a content rule silent.

### The block as the analyzed build compiles it

The directives are compared across the whole block, whatever preprocessor directives lie between them, as SHARED-USING-007 compares them across directives of other groups. A using directive inside a branch the build excludes is disabled text and absent (SHARED-DIAG-008).

### RH7207 reports the order, not the blank lines

In a block that holds a preprocessor directive, RH7207 reports a block whose directives differ from the canonical order and does not check its blank lines. The formatter keeps those blank lines (FMT-USING-004), and SHARED-DIAG-010 keeps a layout check from reporting a shape the formatter keeps. RH7207's order check is a content check and is not covered by that requirement.

### The location and the fix stay as they are

Each rule reports where its own requirement places the diagnostic (RH7201-003 to RH7207-003). The code fixes already refuse a block that holds a directive; their requirements now say so, because before this change the analyzers never reported such a block.

## Alternatives considered

- **Keep the silence** — the issue's option B. Rejected on the recommendation of this change: its only reason is that the fix may not reorder the block, which SHARED-DIAG-010 and *Code-fix coverage* do not accept as a reason for a content rule to stay silent.
- **Lift the silence only for conditional directives** — rejected: `#nullable`, `#pragma`, and `#region` kept the block silent for the same reason, and the block in front of which `#nullable enable` stands is the easiest to correct by hand.
- **Compare only directives that no preprocessor directive separates** — rejected: SHARED-USING-007 compares a directive with every earlier directive of its group whatever lies between them, and the issue's own example, `System` below a conditional group under `System.Linq`, would stay unreported.
- **Let RH7207 report the blank lines of such a block too** — rejected: the formatter keeps them (FMT-USING-004), and SHARED-DIAG-010 does not let a layout check report a shape the formatter keeps.
