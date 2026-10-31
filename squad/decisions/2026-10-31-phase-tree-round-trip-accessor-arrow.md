# Gap around the `=>` a structural transform creates

| | |
|---|---|
| Date | 2026-10-31 |
| Escalation | 4 |
| Run | issue #1488 |

## Question

FMT-STRUCT-026 forbade the accessor and get-only transforms to write a space around the `=>` they create, but no spacing rule covers accessor keyword plus `=>`, so FMT-SPACE-002 keeps the zero gap and 31 final outputs change (`get=> "";` instead of `get => "";`). Options: A name the space on both sides of a created `=>` as a separator the transforms write; B add a spacing rule for the arrow of an expression-bodied accessor or member (new formatting behavior, wider scope); C accept the changed output.

## Answer

A, on the condition that it is a small change; if it turns out to be more involved, implement B directly instead. If the run stays with A, the maintainer approves in advance a follow-up issue for the spacing rule of option B.

## Reason

None given beyond the condition.
