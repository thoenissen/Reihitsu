Role: issue manager
Commit: 451ef38d0f359da24c3f99ef74406cb6bb6eaf40
Verdict: filed

## Issues
| Candidate | Outcome | Issue | Labels | Parent |
|---|---|---|---|---|
| Parenthesized and positional pattern content loses its indentation | created (output question, readings A/B/C) | #1542 | agent-filed, enhancement, formatter | none |
| RH5026 reports the formatter's output for `// f⏎⏎finally` and `else` | commented on open duplicate (facts added; braces counterpart split off) | #1392 | unchanged (bug, formatter, agent-filed) | none |
| Region directives inside collection expressions, patterns, initializers | commented on open #1399 (item 20 gains list pattern and parenthesized pattern instances) | #1399 | unchanged (enhancement, formatter, agent-filed) | none |
| Blank line between own-line comment and array-initializer, extension-block, property-pattern `{` etc. (RH5024/RH5025, widened by row 23) | created (output question; kept separate from #1392, which is about chained keywords) | #1543 | agent-filed, bug, formatter, analyzer | none |
| AGENTS.md phase-ownership table lacks BraceBlankLinePolicy and FMT-BREAK-030 | created (internal, tooling lane) | #1544 | agent-filed, maintenance, documentation | none |
| Analyzer coverage for comments sharing a line with code / around `=` | created as new-rule request; gap check: RH5042 covers the `=` part, RH5020 exempts trailing comments, RH5033-RH5049 are construct-specific, #1160 covers spacing only | #1546 | agent-filed, enhancement, analyzer | none |
| `verify-text-only.sh --strict-docs` treats public members of internal types as public API | created (internal, tooling lane) | #1545 | agent-filed, maintenance | none |

Search covered the 300 most recent issues plus open issues down to #973 by title; bodies read for #1399, #1442 (not a match: `case (` breaks), #1392, #1507 (not a match), #1160 (not a match).
