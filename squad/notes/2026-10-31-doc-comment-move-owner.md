# doc-comment-move-owner

| | |
|---|---|
| Date | 2026-10-31 |
| Run | issue #1491 |
| Pull request | #1532 |

## Questions to the maintainer

None. The change doc lists no visible output change, and the one conditional output question (brace placement of `{ /** d */ code`) disappeared when the lead dropped that `agent-filed` goal.

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|

## Cost

- Stops: none.
- Repair rounds: 1 of 3. The first audit found a reachable output change the skeptic had set aside as "not moved today": a delimited documentation comment behind `?`/`:` of a conditional. The new owner wrote the line break into a different token's trivia than the old one, and a later phase reads which token carries it.
- Roles spawned more than once: spec author twice (skeptic amendments), implementer three times (initial, a structural move of a shared fact out of another phase's folder, repair), test implementer twice, cleaner, spec checker and auditor twice each.
- Most expensive: the differential corpora (main vs head CLI over thousands of generated fixtures) by the skeptic, the implementer, and both audits. They were also what found the regression and proved its repair.

## Decisions taken

16 decisions by the lead. Ones the maintainer may want to see: decision 5 (the issue's goal "brace placement no longer moves a documentation comment" dropped to keep today's output), decision 12 (the "first in block" fact moved to `Pipeline/Core`), and decision 14 (the repair reached the using-directive phase).

## Suggestions

- An ownership move between phases should state *where* the moved trivia lands (which token's leading or trailing trivia), not only *which phase* writes it: later phases read that, and a move that keeps the text but changes the carrier changes output. The skeptic's "pre-moved twin" check compared text inputs, which cannot see the carrier.
- The lead's playbook names the branch `claude/issue-<N>-<slug>`, but a cloud session may be bound to a designated branch; the run used the designated branch and recorded it as a decision.
