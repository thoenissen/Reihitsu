# The formatter spaces `fixed` like the other statement keywords

| | |
|---|---|
| Status | Accepted |
| Date | 2026-10 |
| Affected requirements | FMT-SPACE-012 |

The maintainer accepted this change in advance, when answering a question of the line-start run.

## What

The formatter puts exactly one space after `fixed`, as it already does after `if`, `for`, `foreach`, `while`, `switch`, `catch`, `using`, `lock`, `return`, and `throw` (FMT-SPACE-012). `fixed(int* p = items)` becomes `fixed (int* p = items)`, and `fixed/* c */(int* p = items)` becomes `fixed/* c */ (int* p = items)`, as `if/* c */(c)` becomes `if/* c */ (c)` (FMT-SPACE-013). The same space follows `fixed` in a fixed-size buffer field, so a comment glued to the buffer's type gets one space behind it: `public fixed/* c */int _b[4];` becomes `public fixed/* c */ int _b[4];`, and `public fixed /* c */int _b[4];` becomes `public fixed /* c */ int _b[4];`. Today both are kept as written.

## Why

RH6001-001 lists `fixed` among the keywords that must be separated from their `(`, and its analyzer has always checked it. The formatter's keyword spacing does not list `fixed`, so no spacing rule covers the pair, and FMT-SPACE-002 keeps `fixed(` as written. Formatted code therefore raises RH6001. That breaks RH6001-001's own *Why* ("formatted code raises no RH6001") and SHARED-DIAG-010, which says a layout rule must not report a shape the formatter keeps. A user who runs only `reihitsu-format` keeps an RH6001 diagnostic for good.

## Decisions

### The formatter follows the rule

The keyword gets the same space as every other statement keyword whose parenthesis would otherwise read like a call. The analyzer and the rule page already treat `fixed` that way, so only the formatter changes.

### No exception for the fixed-size buffer

`fixed` also starts a fixed-size buffer field (`fixed int _buffer[4];`), and there, too, exactly one space follows it.

- **Whitespace only between `fixed` and the type.** The output is the same as today: FMT-SPACE-002 already collapses that whitespace to one space and never removes it. For example, `public fixed  int _c[4];` becomes `public fixed int _c[4];`, before and after this change.
- **A comment glued to the type.** The output changes. FMT-SPACE-013 applies the one space between the comment and the type, so `fixed/* c */int` becomes `fixed/* c */ int`. Today no spacing rule covers the pair, so FMT-SPACE-002 keeps the missing gap.

A comment in front of the buffer's type and a comment in front of the statement's `(` get the same treatment, so the keyword reads the same in both places.

## Alternatives considered

- **Drop `fixed` from RH6001** — rejected: the maintainer decided that RH6001 is meant to check `fixed`.
- **Keep both and record `fixed` as an exception to SHARED-DIAG-010** — rejected: it would change a shared requirement to keep a diagnostic that the formatter never clears.
