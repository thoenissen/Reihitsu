Role: issue manager
Commit: 0ebdc2e
Verdict: filed

## Issues
| Candidate | Outcome | Issue | Labels | Parent |
|---|---|---|---|---|
| Balanced `#if` group in a wrapped argument list of an expression body indented inconsistently (02-skeptic) | not filed: not a defect. Reproduced with `scripts/format.sh` on whole-document input: the active branch's content and `)` share one column; the disabled branch is disabled text, which FMT-INDENT-011 keeps untouched. The skeptic's mixed columns came from reading the converted output with the other branch active | none | n/a | n/a |
| Five copies of "remove trailing whitespace trivia" (06-implementer) | extended: duplicate of the open issue; body now lists all copies, notes the formatter owner is not reachable from Core/CodeFixes, and asks for one Core owner | #1583 | analyzer, formatter, maintenance, agent-filed (unchanged) | none |
| RH3202 family keeps member and sibling on one line across a comment, against SHARED-FIX-007; `/** c */` gap missed (12-test-implementer, 21-auditor) | extended: same decision as the open issue on the shared-line break; added the comment-gap and `/** */` facts, observed inputs, and done-when | #1775 | bug, analyzer, maintenance, agent-filed (unchanged) | none |
