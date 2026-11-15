Role: issue manager
Commit: d287cdb
Verdict: filed

Re-check on the branch head: a comma that starts a line (no comment or directive in front of it) still falls to the enclosing column in switch-expression arms, positional patterns, and base lists (`, _ => 3` and `, 2` at the statement's column, `, IB` at column 0). Reproduced by running `scripts/format.sh` on the three inputs. FMT-UNSUP-009 does not refuse them.

## Issues
| Candidate | Outcome | Issue | Labels | Parent |
|---|---|---|---|---|
| RH5107 fix delegates layout to the formatter | created | #1758 | agent-filed, maintenance, analyzer | none |
| Switch-expression arms: comma that starts a line | commented (output question; part 1 of the issue covers the arms' layout) | #956 | unchanged | none |
| Positional pattern: comma that starts a line | commented (output question; the issue covers the pattern's wrapped layout) | #1542 | unchanged | none |
| Base list: comma that starts a line | commented (output question; the issue decides the base types' column) | #1633 | unchanged | none |
| FMT-DOC-001 should state placement of a doc comment behind a comma | commented (the issue decides relocation of doc comments that document nothing, and rewrites FMT-DOC-001 there) | #962 | unchanged | none |
