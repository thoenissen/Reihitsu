# Shared-spec prefixes collide with main after the merge (#997)

| | |
|---|---|
| Date | 2026-10-20 |
| Escalation | 9 |
| Run | issue #997 |

## Question

Merging `main` brought `shared/statement-braces.md` (prefix SHARED-BRACE) and `shared/empty-type-declarations.md` (prefix SHARED-EMPTY), which use the same prefixes as this run's `shared/blank-line-after-brace-fixes.md` (SHARED-BRACE-001) and `shared/empty-parentheses.md` (SHARED-EMPTY-001, SHARED-EMPTY-002). Three IDs were defined twice. This run's IDs had never been on `main`. Options: rename this run's prefixes to SHARED-PARENS and SHARED-AFTERBRACE, or to other names.

## Answer

Rename this run's IDs: SHARED-EMPTY-001/-002 of the empty-parentheses spec become SHARED-PARENS-001/-002, and SHARED-BRACE-001 of the blank-line-after-brace spec becomes SHARED-AFTERBRACE-001. `main`'s IDs stay. The filed open-question issues that cite the old IDs are updated.

## Reason

none given.
