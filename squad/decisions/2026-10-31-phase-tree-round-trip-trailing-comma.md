# Trailing-comma removal that moves a comment's owner

| | |
|---|---|
| Date | 2026-10-31 |
| Escalation | 5 |
| Run | issue #1488 |

## Question

`TrailingCommaRemovalUtilities.PreserveTrailingTrivia` appends the removed comma's comments behind an existing line break, so on reparse the comment belongs to `}` and the formatter is not idempotent on those inputs. It violates FMT-PIPE-021 but is not in the change doc's What. Options: A fix it in this run and add it to What; B exempt it in FMT-PIPE-021 and file a follow-up.

## Answer

A — fixed in this run; the change doc's What names it.

## Reason

None given beyond accepting the recommendation.
