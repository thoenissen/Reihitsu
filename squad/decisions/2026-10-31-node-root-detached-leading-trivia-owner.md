# Two owners of what stands in front of a node-in-document target that does not start its line

| | |
|---|---|
| Date | 2026-10-31 |
| Escalation | 12 |
| Run | issue #1489 |

## Question

The phases move a line break or a comment in front of the first token of a `FormatNodeInDocumentAsync` target that does not start its line, and the entry point writes the document's leading trivia back over it (FMT-PIPE-010), so the brace of a member body stays behind its declaration and the comment of `else if (y) // note` is dropped. (A fix narrowly in this run through the `else if` fact and widen follow-up #1521 to the structural change; B change the structure first in this run; C stop the run)

## Answer

A — fix narrowly in this run; the structural change is folded into follow-up #1521, whose widened text the maintainer approves before it is edited.

## Reason

None given beyond accepting the recommendation.
