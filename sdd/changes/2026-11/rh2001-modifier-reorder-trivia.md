# RH2001 writes reordered modifiers on one line and withholds a fix that moves a modifier across a comment

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | RH2001-004 (changed), RH2001-009 (added) |

## What

- **The reorder is specified.** When the RH2001 fix adds `readonly` to a get-only property whose modifiers are not in RH7105's order, it writes the field's modifiers in that order, as RH7105 does (SHARED-MODORDER-002): the trivia in front of the property's first modifier stays once in front of the field's first modifier, each modifier is followed by one space, and the modifiers stay on one line (RH2001-004). A blank line above the property stays above the field.
- **A comment or directive that a reordered modifier would cross withholds the fix** (RH2001-009). A comment or directive between modifiers that keep their order keeps its place, and the fix stays available.
- **`readonly` goes behind a comment that stands at its place** (RH2001-009): between the modifiers RH7105 puts in front of it and those it puts behind it, or in front of the type when none follows it.

## Why

The fix reorders the modifiers today, but no requirement said so or said what happens to the trivia between and in front of them. RH2001-007 left a comment between modifiers uncovered, so the fix moved the comment with whichever keyword it was attached to, and it carried the trivia of the first modifier, including a blank line above the property, into the middle of the modifier list.

Other outputs of the same fix change in this run without a change of intended behavior: the gap between the reordered modifiers violates SHARED-FIX-007, and the fix withheld for a comment, documentation comment, or directive above the property is required by RH2001-004. They are bug fixes and are not listed below.

## Visible output changes

Code-fix output measured with `scripts/apply-fix.sh` at `acf0007`.

A comment between two modifiers that the reorder puts the other way round:

```csharp
class C
{
    static /* a */ private int Value { get; }
}
```

Before:

```csharp
class C
{
    private     static /* a */ readonly int _value;
}
```

After: no code fix is offered; the diagnostic stays. The same holds for `static // a` on a line of its own above `private int Value { get; }` (before: `private     static // a`, then `readonly int _value;`) and for `new /* a */ static private int Value { get; }`.

A blank line above a property whose modifiers the fix reorders:

```csharp
class C
{
    private int _x;

    static private int Value { get; }
}
```

Before:

```csharp
class C
{
    private int _x;

    private
    static readonly int _value;
}
```

After:

```csharp
class C
{
    private int _x;

    private static readonly int _value;
}
```

A comment between the last modifier and the type, with modifiers the fix reorders:

```csharp
class C
{
    static private /* a */ int Value { get; }
}
```

Before:

```csharp
class C
{
    private /* a */     static readonly int _value;
}
```

After:

```csharp
class C
{
    private static /* a */ readonly int _value;
}
```

Unchanged: `private static /* b */ int Value { get; }` still becomes `private static /* b */ readonly int _value;`, and `private /* c */ unsafe string Value { get; }` still becomes `private /* c */ readonly unsafe string _value;`. Also unchanged: `private /* a */ static int Value { get; }` still becomes `private /* a */ static readonly int _value;`, and `private /* a */ new static int Value { get; }` still becomes `private /* a */ static new readonly int _value;`.

## Decisions

### A comment that a reordered modifier would cross withholds the fix

The maintainer's answer (2026-11-08, question 1, option A). The comment's place next to a keyword is the author's decision, and RH7105 already withholds its fix for a comment between modifiers (SHARED-MODORDER-003).

### Only a crossed comment withholds the fix

A comment between modifiers that keep their order is not crossed and keeps its fix, as on `main`. A wider guard, such as SHARED-MODORDER-003's "a comment follows a modifier on that modifier's line", would withhold fixes that are correct today.

### `readonly` goes behind a comment at its place

The maintainer's answer (2026-11-08, batch 2, option A). It keeps today's output where the modifiers are already in order (`private static /* b */ readonly`, `private /* c */ readonly unsafe`) and changes only the reordered case, whose output today has a gap and moves the comment in front of `static`.

### The modifiers stay on one line below a blank line

The maintainer's answer (2026-11-08, question 2, option A). The line break came only from the blank line's trivia that the fix carried into the modifier list; the author wrote the modifiers on one line.

## Alternatives considered

- **Keep the comment next to the keyword it follows** (`private static /* a */ readonly int _value;`) — rejected by the maintainer's answer (option B).
- **Move the comment in front of the list** (`private /* a */ static readonly int _value;`) — rejected by the maintainer's answer (option C).
- **Add `readonly` without reordering** (`static private readonly int _value;`), leaving the order to RH7105 — rejected: the fix's evident intent and the issue both produce RH7105's order, and SHARED-MODORDER-002 already states how to reorder safely.
- **Write `readonly` in front of the comment** (`private static readonly /* a */ int _value;`) — rejected by the maintainer's answer (batch 2, option B).
- **Withhold the fix for a comment between the last modifier and the type when the fix reorders** — rejected by the maintainer's answer (batch 2, option C).
- **Keep the line break below the blank line** — rejected by the maintainer's answer (option B).
