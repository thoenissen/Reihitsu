Role: issue manager
Commit: 0fa1778c
Verdict: filed

## Issues
| Candidate | Outcome | Issue | Labels | Parent |
|---|---|---|---|---|
| Consolidate RH3002's private parse helpers onto the shared check; `HaveSameTokensAndStructure` depth guard | created | #1603 | agent-filed, maintenance, analyzer | none |
| `-004` conditions (RH3001, RH3005, RH3101, RH3105) leave safe inputs unspecified; RH3201-005 comment at line start; RH3201-007 adjacent empty comments | created | #1606 | agent-filed, maintenance, documentation, analyzer | none |
| Formatter layout-only mode for the node-level entry point; multi-line `///` continuation columns | created | #1605 | agent-filed, maintenance, formatter | none |
| RH7004 / `TypeMemberOrderingCodeFixProviderBase` reformat moved code; shared "move lines by a delta" helper (also covers the RH3004 raw-string follow-up) | created (+ commented on #1335 and #969, related) | #1604 | agent-filed, bug, analyzer | none |
| Shared helper for the formatter's `RawStringLineShifts`, RH3004, RH7004, member ordering | not filed separately: merged into #1604 | #1604 | | |
| RH3004-006 first-character measure vs. comment continuation; expression-body parameter-list comment layout; inactive column-0 directive predicate | created (one issue) | #1607 | agent-filed, maintenance, analyzer | none |
| RH3004: offer the fix again for directives, disabled text, multi-line strings/comments | created | #1601 | agent-filed, enhancement, analyzer | none (related #1436 is about `static`/parentheses, not duplicate) |
| RH3106: array-element positions by target | created | #1602 | agent-filed, enhancement, analyzer | none |
| Safety-rail bug: hidden `#` line makes edited text a string in another build (fixes and whole-document formatting) | created | #1600 | agent-filed, bug, analyzer, formatter | none |

Cross-links: comments on #1279 (#1600, #1601, #1604, #1605, #1607) and #1281 (#1602, #1603, #1606).
