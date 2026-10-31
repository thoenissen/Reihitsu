Role: issue manager
Commit: 5f481b4
Verdict: filed

## Issues
| Candidate | Outcome | Issue | Labels | Parent |
|---|---|---|---|---|
| 1. Delimited documentation comment behind `{`, a switch-label colon, or an earlier comment (output questions 1-3) | created; no duplicate found (#1077 covers a `/** */` that documents nothing, not a comment that documents a declaration) | #1533 | agent-filed, enhancement, formatter | none |
| 2. Documentation comment before a statement, behind an attribute, or inside an expression | extended: duplicate of the open agent-filed #962; its text gained the attribute, accessor, type-parameter, base-list, else/catch instances and the output readings | #962 | agent-filed, enhancement, formatter (unchanged) | none |
| 3. Documentation comment behind `?`/`:` of a conditional against FMT-BREAK-018, RH5308 no progress | created; no duplicate found (#1243, #1273, #1274 concern lone operators and a line comment, not documentation comments) | #1534 | agent-filed, enhancement, formatter | none |

Candidates 1 and 3 carry their output questions in the form of AGENTS.md. No sub-issue links: the candidates are behavior changes, not packages of #1482. Today's outputs for candidate 3 were re-run with scripts/format.sh on the branch.
