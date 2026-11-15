Role: issue manager
Commit: ff20012
Verdict: filed

## Issues
| Candidate | Outcome | Issue | Labels | Parent |
|---|---|---|---|---|
| FU1 SHARED-FIX-007 shared line for gap-only fixes (with RH5111 declarator and enum-member examples) | created | #1773 | agent-filed, enhancement, analyzer | none |
| FU2 RH5303–RH5308, RH3201 unmeasured instances | commented | #1140 | unchanged | n/a |
| FU3 entry point for changed code whose first token moves (RH5110, RH5403/RH5404 header); RH5110 empty generic type | commented | #1140 | unchanged | n/a |
| FU4 write-back step for parts inside a construct (M3, M5, spanning-part question) | created | #1774 | agent-filed, enhancement, analyzer | none (related, not attached: layout-only mode issue #1605) |
| FU5 + addendum one owner for the break in front of a shared-line member | created | #1775 | agent-filed, bug, analyzer, maintenance | none |
| FU6 PrecedingTokenFacts/TriviaFact cleanup | created | #1776 | agent-filed, maintenance, formatter | none |
| FU7 RH5113 guards (syntax error, refused shape) | created | #1777 | agent-filed, enhancement, analyzer | none |

## Notes

- No exact duplicate found among the open issues listed (searched by title; no search tool was available, so closed issues were not scanned beyond the newest page).
- Possibly overlapping, not duplicates: #1521 (write-back of what phases put in front of a target) and #1474 (blank lines in front of a node-in-document target) concern the leading-trivia behavior this run changed (D5, FMT-PIPE-010/024). The lead may want to check whether the PR makes them obsolete.
- The #1140 comment lists RH5401, RH5408, RH5412–RH5416 as unmeasured for whole-member formatting; that line is carried from the issue's own list, not measured here.
