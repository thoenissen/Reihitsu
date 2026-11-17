Role: issue manager
Commit: aaa6880
Verdict: filed

## Issues
| Candidate | Outcome | Issue | Labels | Parent |
|---|---|---|---|---|
| Declare a comment, directive, or disabled text in front of an expression body's `=>` unsupported (maintainer answer batch 1 Q6 = A, own change); absorbs 01-spec-author's `/* c */ {` follow-up and 02-skeptic follow-ups 1 and 2 | created | #1851 | enhancement, formatter, analyzer, agent-filed | none |
| 02-skeptic follow-ups 3 and 4: value of a `return` on the next line lands in the statement's column (also via the expression-body conversion, blank line behind the arrow, `return // c`); open output question | created | #1852 | enhancement, formatter, agent-filed | none |

Duplicate search: open and closed issues since 2026-11-14 and all `agent-filed` issues scanned; the only related ones are the closed #1730 and #1731 (superseded by the run's issue #1732), which are not reopened. No existing issue covers either candidate. No milestone set. Both issues are self-contained, with the "before" output the skeptic observed with `scripts/format.sh` and `scripts/apply-fix.sh` on main; for #1852 the `return // c` getter case was observed as a fixture by the skeptic but its exact text is not quoted in a handover.
