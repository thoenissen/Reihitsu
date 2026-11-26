Role: issue manager
Commit: d747a1d
Verdict: filed

## Issues
| Candidate | Outcome | Issue | Labels | Parent |
|---|---|---|---|---|
| 1 RH5404 skips a type with members and a `;` behind its braces | created (output question; names the second has-semicolon check at `RH5404ElementMustNotBeOnSingleLineAnalyzer.cs:52`) | #1959 | agent-filed, enhancement, analyzer | none |
| 2 Formatter indents a `;` on its own line behind a type's closing brace; blank line and indent with a comment or directive before it | created (output questions 1 and 2) | #1960 | agent-filed, enhancement, formatter | none |
| 3 Fix All needs two iterations when two reported declarations share a line | created; no duplicate found (#1775 and #1773 touch the same helper but concern the indentation of the break and gap-only fixes, not Fix All iterations) | #1961 | agent-filed, bug, analyzer | none |

The "Test note" line (invert `UnionWithBracesAndSemicolonIsNotReported`) is not a candidate and was not filed. Duplicate search covered the titles of the first 200 open and closed `agent-filed` issues, with no search tool available.
