# RH3001-004 still open: shift operators in front of the negation (#997)

| | |
|---|---|
| Date | 2026-10-20 |
| Escalation | 7 |
| Run | issue #997 |

## Question

After the targeted RH3001-004 fix, the auditor still blocked: the restated exclusion covered a `!` directly after a `>` operator, but `>>` and `>>>` close type argument lists too, so with enough open `<` the fix still writes a generic invocation (`M(G < A, H < B >> !x)` with a user-defined `>>` taking `bool`). Every other token in front of `!` that was tried compiles. Options: also exclude `>>` and `>>>` (not `=>`), pin and file the cases; or publish and only file them.

## Answer

Also exclude a `!` directly after `>>` or `>>>`, keeping `=>` inside the requirement. Pin the first counterexample with an untagged test, add the shift inputs to #1245, and complete the declarations of the earlier filed input. Then the cleaner, the auditor on this change only, and the validator run before publishing.

## Reason

none given.
