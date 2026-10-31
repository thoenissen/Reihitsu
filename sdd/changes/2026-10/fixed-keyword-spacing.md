# The formatter spaces `fixed` like the other statement keywords

| | |
|---|---|
| Status | Accepted |
| Date | 2026-10 |
| Affected requirements | FMT-SPACE-012 |

The maintainer accepted this change in advance, when answering the decision recorded in `squad/decisions/2026-10-31-line-start-fact-fixed-spacing.md`.

## What

The formatter puts exactly one space after `fixed`, as it already does after `if`, `for`, `foreach`, `while`, `switch`, `catch`, `using`, `lock`, `return`, and `throw` (FMT-SPACE-012). `fixed(int* p = items)` becomes `fixed (int* p = items)`, and `fixed/* c */(int* p = items)` becomes `fixed/* c */ (int* p = items)`, as `if/* c */(c)` becomes `if/* c */ (c)` (FMT-SPACE-013).

## Why

RH6001-001 lists `fixed` among the keywords that must be separated from their `(`, and its analyzer has always checked it. The formatter's keyword spacing does not list `fixed`, so no spacing rule covers the pair, and FMT-SPACE-002 keeps `fixed(` as written. Formatted code therefore raises RH6001. That breaks RH6001-001's own *Why* ("formatted code raises no RH6001") and SHARED-DIAG-010, which says a layout rule must not report a shape the formatter keeps. A user who runs only `reihitsu-format` keeps an RH6001 diagnostic for good.

## Decisions

### The formatter follows the rule

The keyword gets the same space as every other statement keyword whose parenthesis would otherwise read like a call. The analyzer and the rule page already treat `fixed` that way, so only the formatter changes.

### No exception for the fixed-size buffer

`fixed` also starts a fixed-size buffer field (`fixed int _buffer[4];`). There, too, exactly one space follows it. That is the same output the formatter gives today, because FMT-SPACE-002 collapses the whitespace between `fixed` and the type to one space and never removes it.

## Alternatives considered

- **Drop `fixed` from RH6001** — rejected: the maintainer decided that RH6001 is meant to check `fixed`.
- **Keep both and record `fixed` as an exception to SHARED-DIAG-010** — rejected: it would change a shared requirement to keep a diagnostic that the formatter never clears.
