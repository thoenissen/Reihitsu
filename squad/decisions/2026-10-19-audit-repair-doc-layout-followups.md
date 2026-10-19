# Second repair after the audit, doc-comment layout, and follow-ups

| | |
|---|---|
| Date | 2026-10-19 |
| Escalation | 7 |
| Run | issue #1068 |

## Question

1. The auditor blocked after the repair round had already been used on the spec checker's findings. Medium: the RH7103 example of SHARED-FIX-003 has no result block and its test asserts only that an action exists and the order of substrings, so it misses the fix reformatting an unrelated member (#969). Low: an RH4106 test duplicates an existing one. Options: allow one more repair; drop the RH7103 example; publish as is.
2. SHARED-FIX-003 treats the `///` exterior and whitespace and line breaks next to XML tags in a documentation comment as layout, combining two earlier answers. Options: confirm; or narrow to the indentation of `///` lines.
3. Create follow-up issues for the RH7109 layout bug, the SHARED-FIX-003 relocation/disabled-text extension, the stale RH2005/RH7103 non-goals, and the newly found RH7309 initializer-order bug? Options: all four; only the first three; none.

## Answer

1. Allow one more repair: the RH7103 example gets an input whose result touches no unrelated member and an exact result block; the test asserts one action and the exact text; the duplicate RH4106 test is removed and the existing one tagged instead. Cleaner, spec checker, auditor (repair only), and validator follow.
2. Confirmed: in a documentation comment, the `///` exterior and whitespace and line breaks next to XML tags are layout; the words are content.
3. Create all four, the relocation issue extended to disabled text, and link them from the PR.

## Reason

none given
