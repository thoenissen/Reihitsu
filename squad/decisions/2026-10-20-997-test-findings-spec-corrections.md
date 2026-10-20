# How to route the test implementer's two spec findings (#997)

| | |
|---|---|
| Date | 2026-10-20 |
| Escalation | 4 |
| Run | issue #997 |

## Question

The test implementer found two points in this run's own new specs. SHARED-BODY-001's third example (a balanced `#if` group) marks the diagnostic as ending after `#endif`, while the analyzer reports only the expression body up to the disabled text; the example's fixed output is right. RH3103-003 does not say whether an alias-qualified name such as `S::Nullable<int>` counts as a qualified name; there the diagnostic covers `Nullable<int>` and the fix replaces `S::Nullable<int>`. Options: the spec author corrects both; correct the example and file the alias-qualified case as an open question; or correct the example only.

## Answer

The spec author corrects both: the example's span is set to the span the analyzer reports, and RH3103-003's coverage excludes alias-qualified names, whose test stays untagged. The tests stay as they are.

## Reason

none given.
