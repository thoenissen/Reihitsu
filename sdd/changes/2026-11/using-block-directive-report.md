# A using block that holds a preprocessor directive is reported without a fix

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | SHARED-USING-009 (changed), RH7201-001, RH7202-001, RH7203-001, RH7204-001, RH7205-001, RH7206-001, RH7207-001 (changed), RH7201-004, RH7202-004, RH7203-004, RH7204-004, RH7205-004, RH7206-004, RH7207-004 (clarified) |
| Depends on | [Rules judge the code the current build compiles](report-across-conditional-directives.md) |

## What

SHARED-USING-009 used to keep the using-ordering rules RH7201–RH7207 silent on a using block that holds a preprocessor directive of any kind in the trivia of its directives — in front of its first directive, between two of them, or between the tokens of one. The silence is lifted: the analyzers report such a block as their own requirements state, judging the directives the analyzed build compiles (SHARED-DIAG-008). A using directive that a conditional directive excludes is absent, and every directive the build compiles is compared with the other directives of its block whatever preprocessor directives lie between them, as SHARED-USING-007 already states for directives of other groups.

What stays: neither the formatter nor a code fix reorders such a block. The code fixes of RH7201–RH7207 are not offered for a diagnostic in it, Fix All leaves it unchanged, and the formatter only splits directives that share a line (FMT-USING-004). The block is corrected by hand.

RH7207 checks two things, the order and the blank lines between groups. In a block that holds a preprocessor directive it reports only the order: the formatter's using phase does not lay out the group boundaries of such a block (FMT-USING-004), and other requirements of the formatter write blank lines inside it, such as those around a `#region` (FMT-BLANK-013), so a check of its blank lines would report the formatter's own output, which a layout check whose fix lays out as the formatter does may not (SHARED-DIAG-010).

## Why

SHARED-USING-009 kept the rules silent for one reason: a reported order that the fix and the formatter may not produce would leave a diagnostic the user cannot resolve automatically. That reason is the one the decision behind [Rules judge the code the current build compiles](report-across-conditional-directives.md) overruled for every other rule: a directive keeps the fix and the formatter away, not the rule. SHARED-DIAG-008 judges a construct as the analyzed build compiles it, a conditional, `#region`, `#endregion`, or `#pragma` directive in the judged span does not by itself keep it from being reported, and a fix that would have to move code across a directive is withheld while the diagnostic stays — as for trailing commas and attribute-list shapes next to a conditional directive, which the formatter keeps as well. *Code-fix coverage* in `AGENTS.md` makes a diagnostic without a fix a supported end state. That change kept SHARED-USING-009 on its stated reason; this change removes the last rule family that stays silent for it.

The basis is not that using order is content the formatter does not judge: the formatter does organize using blocks, so SHARED-DIAG-010's exemption for order rules is not what lets these rules report. They report because SHARED-DIAG-008 asks every rule to judge the analyzed build, and no reason that holds in every build keeps them silent.

The order of a block that holds a directive can always be corrected by hand: a directive in front of the block stays in front of it, and a conditional group can be moved as a whole or split into several groups. Every build's directives are a subsequence of one canonical order, so an arrangement that no build reports always exists, also for `#if`/`#else` alternatives (case 6). General notes already tell the user that a diagnostic may be reported without a fix where a preprocessor directive sits in the code a fix would have to change.

## Visible output changes

Every input below is analyzed in a build that does not define `DEBUG`, unless the case says otherwise. "Before" is today's output on `main`, confirmed with `scripts/apply-fix.sh` for cases 1, 2, 5, and 6; the formatter keeps the using block of each case, confirmed with `scripts/format.sh` for cases 1, 2, 4, and 6. The formatter's output does not change for any of them, and no code fix that formats a using block changes its result: the fixes of RH7201–RH7207 are the only ones that format a block, and they stay withheld for it.

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

In a build that does not define `DEBUG`, the conditional group is the leading trivia of `class C`, after the block, and nothing changes: RH7207 reports the missing blank line in front of `Microsoft.Win32` and its fix adds it, as today, and so does the formatter. In a build that defines `DEBUG` — the usual configuration of an IDE — the block is reported, with or without that blank line, so the formatter's own output is reported in that build:

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

The hand correction moves the group behind `using System;`; then neither build reports the block.

### 6. `#if`/`#else` alternatives

Analyzed in a build that defines `NETFRAMEWORK`:

```csharp
using System;
#if NETFRAMEWORK
using Newtonsoft.Json;
#else
using System.Text.Json;
#endif
using Microsoft.Extensions.Logging;

class C
{
}
```

Before: not reported.

After: reported, no code fix offered; the formatter keeps the block as written.

```csharp
using {|RH7207:System|};
#if NETFRAMEWORK
using Newtonsoft.Json;
#else
using System.Text.Json;
#endif
using Microsoft.Extensions.Logging;

class C
{
}
```

The `Microsoft` group sorts before the `Newtonsoft` group. In a build that does not define `NETFRAMEWORK`, the block is in order and not reported. No single position of the `#if`/`#else` group satisfies both builds; the hand correction splits it into an `#if !NETFRAMEWORK` group behind `using System;` and an `#if NETFRAMEWORK` group behind `using Microsoft.Extensions.Logging;`.

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

### Maintainer answers (2026-11-27)

The maintainer answered the run's batch of output questions with A, as recommended, for each:

1. RH7201–RH7207 report an out-of-order using block that holds a preprocessor directive, no code fix is offered, and the formatter keeps the block (cases 1 to 6), also where the reported block is the formatter's own output in another build (case 5) and where only a split of an `#if`/`#else` group satisfies every build (case 6).
2. Every kind of preprocessor directive lifts the silence — conditional directives, `#nullable`, `#pragma`, `#region`, `#endregion`, and the rest — not only conditional directives (case 2).
3. The directives the build compiles are compared across the whole block, whatever preprocessor directives lie between them (cases 1 and 5).
4. A preprocessor directive in the trivia of a using block is not declared unsupported (escalation 10). Declaring it would delete no special case: the formatter's check that keeps such a block in order would still serve the node-level organizing of using directives, and the fixes' guard would only become a withheld fix under SHARED-FIX-010; it would refuse every file with a `#nullable enable` header or a conditional using, which is common code.

### Every kind of preprocessor directive

The silence is lifted for every directive kind SHARED-USING-009 names — conditional directives, `#nullable`, `#pragma`, `#region`, `#endregion`, `#define`, and the rest — not only for conditional directives. The silence had one reason for all of them, the fix that may not reorder, and SHARED-DIAG-008 does not accept that reason: a directive keeps the fix and the formatter away, not the rule.

### The block as the analyzed build compiles it

The directives are compared across the whole block, whatever preprocessor directives lie between them, as SHARED-USING-007 compares them across directives of other groups. A using directive inside a branch the build excludes is disabled text and absent (SHARED-DIAG-008).

### RH7207 reports the order, not the blank lines

In a block that holds a preprocessor directive, RH7207 reports a block whose directives differ from the canonical order and does not check its blank lines. The formatter's using phase does not lay out the group boundaries of such a block (FMT-USING-004), and the blank-line phase writes blank lines around a `#region` inside it (FMT-BLANK-013): formatting

```csharp
using System;
#region Extra
using System.Linq;
#endregion
using Microsoft.Win32;
using Alpha;
```

gives a stable block with a blank line between `using System;` and `#region Extra`, inside one group. RH7207's blank-line check is a layout check whose fix lays the block out as the formatter does, so SHARED-DIAG-010 keeps it from reporting that output. This was decided in the run, not asked: an unchanged requirement demands it, and the output is the one today's code already gives. Its order check reports the block, as SHARED-DIAG-008 asks for every rule.

### The location and the fix stay as they are

Each rule reports where its own requirement places the diagnostic (RH7201-003 to RH7207-003). The code fixes already refuse a block that holds a directive; their requirements now say so, because before this change the analyzers never reported such a block.

## Alternatives considered

- **Keep the silence** (question 1, B; the issue's option B) — rejected by the maintainer: its only reason is that the fix and the formatter may not reorder the block, the reason SHARED-DIAG-008 overruled for every other rule.
- **Lift the silence only for conditional directives** (question 2, B) — rejected by the maintainer: `#nullable`, `#pragma`, and `#region` kept the block silent for the same reason, SHARED-DIAG-008 already says a `#region`, `#endregion`, or `#pragma` directive does not by itself keep a construct from being reported, and the block in front of which `#nullable enable` stands is the easiest to correct by hand.
- **Compare only directives that no preprocessor directive separates** (question 3, B) — rejected by the maintainer: SHARED-USING-007 compares a directive with every earlier directive of its group whatever lies between them, SHARED-DIAG-008 does not let a conditional directive inside the judged span keep a construct from being reported, and the issue's own example, `System` below a conditional group under `System.Linq`, would stay unreported.
- **Declare a preprocessor directive in a using block unsupported** (escalation 10, B) — rejected by the maintainer: it deletes no special case and refuses common code.
- **Let RH7207 report the blank lines of such a block too** — rejected in the run: it would report the formatter's own output (FMT-USING-004, FMT-BLANK-013), which SHARED-DIAG-010 does not let a layout check report.
