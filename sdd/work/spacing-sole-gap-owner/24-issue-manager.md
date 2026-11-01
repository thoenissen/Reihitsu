Role: issue manager
Commit: 8744749
Verdict: filed

All ten candidates are reachable by whole-document formatting (the CLI), so none is dropped. Candidates sharing a cause are merged. None is attached as a sub-issue: no candidate belongs to the restructuring package (#1482).

## Issues
| Candidate | Outcome | Issue | Labels | Parent |
|---|---|---|---|---|
| 1. Wrap inside a declaration's attribute list (`[return` ⏎ `:Obsolete]`) | created, merged with 4 (both are a line break at an attribute list that no requirement decides) | #1549 | agent-filed, enhancement, formatter | none |
| 2. Wrapped pattern parts lose their column | commented (parenthesized, positional, `is not`, `is C(...)` fit the open question; the property-pattern member is noted as belonging to #953 if #1542 stays limited to parentheses) | #1542 | existing | none |
| 3. Member modifier in front of a tuple type | commented, as output question 1 (merged with 7) | #1440 | existing | none |
| 4. Lambda with an attribute list broken after the list | created, merged into candidate 1 | #1549 | agent-filed, enhancement, formatter | none |
| 5. Output question: `ref` / `ref readonly` in front of a tuple type | created, text from `05-spec-author.md` | #1550 | agent-filed, enhancement, formatter | none |
| 6. Wrapped `ref` ⏎ `(int, int)` never joined | created, merged into candidate 5 (same position; the join depends on the spacing answer) | #1550 | agent-filed, enhancement, formatter | none |
| 7. Local function modifier in front of a tuple type | commented, merged into candidate 3 | #1440 | existing | none |
| 8. Wrapped `with` expression loses its column | commented (one instance of the continuation-line class) | #953 | existing | none |
| 9. The space behind `when` | commented, as output question 2 | #1440 | existing | none |
| 10. Gap in front of a relational pattern's operator after a combinator (`or>=`) | commented, as output question 3 | #1440 | existing | none |

#1492 was checked and does not fit: it moves the attribute-list split and merge into the structural transforms with unchanged output. #1494 does not fit either: it covers parenthesized lambdas' `(` and interpolation holes.
