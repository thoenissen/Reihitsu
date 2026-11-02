Role: issue manager
Commit: 8f7649b
Verdict: filed

## Issues
| Candidate | Outcome | Issue | Labels | Parent |
|---|---|---|---|---|
| 1. CS8967: anonymous method in a hole of a regular interpolated string below C# 11 | created. Reachable: whole-document formatting with `--lang-version 10` reproduced the non-compiling output (verified on the branch head, which does not touch regular strings, so it is `main`'s behavior; `main` itself not built). Code-fix path through a C# 10 project not run. No duplicate found (open list, and the closed titles scanned) | #1566 | bug, formatter, agent-filed | none |
| 2. Lambda behind a modifier or return type on the next line | created as a feature request with the output question (A join, recommended; B indent; C keep). Today's output verified (`static` then `(int x) => x` in column 8) | #1567 | enhancement, formatter, agent-filed | none |
| 3. Value continued after `return`, a lambda's `=>`, `(`, a switch arm's `=>` | not filed: the indentation spec states these continuation lines as a non-goal ("the expression after `=>` or `=` that stays on its own line"), tracked in #953. Commented on #953 with the four gaps | #953 (commented) | existing | none |
| 4. Switch arm after a multi-line arm stays on the brace line | not filed: "one switch-expression arm per line" is the decided behavior change of #956, and the line-breaks spec lists it as a non-goal tracked there. Commented on #956 with the input and the verified output | #956 (commented) | existing | none |
| 5. Tab in a raw literal's content prefix counts as one column for hole alignment | UNVERIFIED, nobody reproduced it. Not filed as a bug; added as an explicitly unverified note to the open tab-indented raw string issue #1432, with the instruction to test first and drop it if the prefix is already replaced | #1432 (commented) | existing | none |
| 6. `/** */` or `///` comment inside an expression is moved with a blank line | not filed: #962 (open) already covers a documentation comment that documents nothing inside an expression, with reading B (leave in place). Commented with the two shapes (behind `(`, between `delegate` and `{`) | #962 (commented) | existing | none |
| 7. Anonymous method outside holes: scope-depth block vs. aligned like a block lambda | created with the output question (A keep; B align under `delegate`, recommended, per the maintainer's "a hole has no layout of its own"). Today's output and the block-lambda comparison verified | #1568 | enhancement, formatter, agent-filed | none |

No parent was attached: none of the candidates belongs to the restructuring tracked by #1482.
No milestone was set.
