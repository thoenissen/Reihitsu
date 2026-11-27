Role: spec author
Commit: 4dc2373
Verdict: change doc written

The spec is written on the issue's option A and on my recommendation for every neighbouring question below; the change doc stays `Draft` until the maintainer answers. My prompt carried no intended fix or hoped-for verdict beyond "write the change the issue proposes", which is the lead's assignment, not a verdict.

Change doc: `sdd/changes/2026-11/using-block-directive-report.md`.

## Changed requirements

- SHARED-USING-009 — Blocks with preprocessor directives are not reordered — changed: the formatter still does not reorder such a block and the RH7201–RH7207 fixes are not offered, but the analyzers report it as their own requirements state, judging what the analyzed build compiles (SHARED-DIAG-008); RH7207 reports only the order there, not the blank lines (SHARED-DIAG-010, FMT-USING-004). `Uses` gains SHARED-DIAG-008, SHARED-DIAG-010, FMT-USING-004.
- RH7201-001, RH7202-001, RH7203-001, RH7204-001, RH7205-001, RH7206-001 — changed: the condition "when SHARED-USING-009 allows the block to be reordered" is removed, and the "is not reported" example becomes a reported one.
- RH7207-001 — changed: a block SHARED-USING-009 forbids to reorder is reported only for its order, not its blank lines; reported example plus a negative blank-line example.
- RH7201-004 to RH7207-004 — clarified: one sentence that no fix is offered for a diagnostic in such a block (the code already withholds it in `UsingDirectiveOrderingCodeFixProviderBase` and `RH7207UsingDirectivesShouldBeOrganizedIntoGroupsCodeFixProvider`; before this change no diagnostic ever reached it).

No other spec changes: FMT-USING-004 (formatter keeps such a block) and `pipeline.md` stay as they are. No rule page changes: `documentation/rules/general-notes.md` already says a diagnostic may be reported without a fix where a preprocessor directive sits in the code a fix would change.

Checked: today's "not reported" for the issue example, the `#nullable` header, and the trailing conditional group (non-DEBUG build) via `scripts/apply-fix.sh RH7203|RH7207`; the formatter leaves the issue, header, pragma-between-tokens, and blank-line examples' using blocks unchanged via `scripts/format.sh` on copies. `scripts/test.sh --project architecture`: 198 passed.

## Examples for the tests

SHARED-USING-009 (all examples, the DEBUG and non-DEBUG builds of the first), RH7201-001, RH7202-001, RH7203-001 (both builds), RH7204-001, RH7205-001, RH7206-001, RH7207-001 (reported and the negative blank-line example), RH7201-004 to RH7207-004 (no fix offered). Existing tests asserting silence that flip: `BlockWithPreprocessorDirectiveIsNotReported` and `DisabledConditionalUsingBlocksAreNotReportedWhenTheyCannotBeSafelyReordered` in all seven test classes, RH7207 `NoDiagnosticWhenRegionDirectivePreventsSafeReordering` and `BlockLedByANullableDirectiveIsNotReported`, RH7203 `PragmaBetweenTokensOfDirectiveIsNotReported`, `ConditionalGroupBetweenTokensOfDirectiveIsNotReported`, `ConditionalGroupAcrossTokensOfTwoDirectivesIsNotReported`. The no-fix tests (`NoCodeFixWhen...`) stay valid.

## Output questions

All inputs analyzed in a build that does not define `DEBUG` unless stated. Location of every diagnostic is unchanged (each rule's -003); the formatter's output is unchanged for every case; no code fix is offered in any block that holds a directive (as today's fix guard already does). None of these is an open question.

**1 · RH7201–RH7207 · using directive ordering — out-of-order block that holds a preprocessor directive** (the issue's question)

Input:
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

Before (`main`): not reported.

After, A (recommended): reported, no fix offered; the formatter keeps the block.
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
(In a DEBUG build, RH7203 also reports `System.Diagnostics`.)

After, B: not reported (unchanged).

Why A: the order is content, and content rules report what they judge even where neither the fix nor the formatter can rewrite it, as every other rule family already does after the "rules judge the current build" decision.

**2 · RH7201–RH7207 · which preprocessor directives — a directive in front of the block**

Input:
```csharp
#nullable enable
using System.Linq;
using System;

class C
{
}
```

Before (`main`): not reported.

After, A (recommended) — every directive kind (`#if`, `#nullable`, `#pragma`, `#region`, …):
```csharp
#nullable enable
using {|RH7207:System.Linq|};
using {|RH7203:System|};

class C
{
}
```

After, B — only `#if`/`#elif`/`#else`/`#endif`: not reported (unchanged).

Why A: every kind was silent for the same reason, and this block is the easiest to correct by hand. (Note: `#nullable enable` headers are common, so A adds fix-less diagnostics to such files; follow-up candidate 1 would give them a fix.)

**3 · RH7201–RH7207 · comparison across a directive — conditional usings at the end of the block, build that defines `DEBUG`**

Input:
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

Before (`main`): not reported in a DEBUG build (in a non-DEBUG build, RH7207 reports and fixes the missing blank line, unchanged).

After, A (recommended) — compare across the whole block:
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

After, B — compare only directives no preprocessor directive separates: not reported. (Under B the issue's own example in question 1 is also not reported, since `System` and `System.Linq` are separated by the `#if` group.)

Why A: the rules already compare a directive with every earlier one of its group whatever lies between, and B would leave the issue's own case unreported.

**4 · RH7207 · Using directives should be organized into groups — blank lines in a block that holds a directive**

Input:
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

Before (`main`): not reported.

After, A (recommended): not reported (only the order is checked in such a block; this one is in order).

After, B: reported for the missing blank line in front of `Microsoft.Win32`, no fix offered.
```csharp
using {|RH7207:System|};
#if DEBUG
using System.Diagnostics;
#endif
using Microsoft.Win32;

class C
{
}
```

Why A: the formatter keeps the blank lines of such a block, and a layout check does not report a layout the formatter keeps.

## Internal choices

- SHARED-USING-009 rewritten in place (ID and title kept) rather than a new requirement for the report; alternative: retire part and add SHARED-USING-012. In place keeps every existing `[Requirement("SHARED-USING-009")]` tag meaningful.
- The withheld fix is restated as one sentence in each RH720x-004; alternative: rely on SHARED-USING-009 alone.
- Rule pages left alone (withheld fix is covered by general-notes); RH7207's page says nothing about directive blocks today.

## Follow-up candidates

**1 · RH7201–RH7207 fixes and the formatter · a directive only in front of the first using directive**

Input:
```csharp
#nullable enable
using System.Linq;
using System;

class C
{
}
```

Before (after this change): reported by RH7203 and RH7207, no fix offered; the formatter keeps the block.

After, A (recommended to file): fix offered and the formatter reorders; the directive stays on top.
```csharp
#nullable enable
using System;
using System.Linq;

class C
{
}
```

After, B: unchanged.

Why A: when every preprocessor directive of the block stands in front of its first using, reordering moves no using across a directive, a cheap trivia check; `#nullable enable` headers are common code, so option A of question 2 otherwise leaves many fix-less diagnostics. Outside this issue, which asks only about reporting.
