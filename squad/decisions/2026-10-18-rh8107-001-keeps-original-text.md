# Should RH8107-001 follow a narrowed SHARED-DOC-004?

| | |
|---|---|
| Date | 2026-10-18 |
| Escalation | 4 (the spec under construction would change an already-approved reference in RH8107) |
| Run | issue #982 |

## Question

The skeptic showed that when an ordinary comment, a directive, or disabled text separates two documentation comments, the compiler drops the earlier ones while the analyzers read the first one. That case becomes an open question and SHARED-DOC-004 is narrowed to exclude it. RH8107-001 referenced SHARED-DOC-004 through a clause approved in `2026-10-18-rh8107-shared-doc-reference-clauses.md`, so it would narrow too, although its text on `main` already specified "the first one in the member's leading trivia". Options: restore RH8107-001's original text and keep only SHARED-DOC-008 under `Uses`, or keep the clause and narrow RH8107-001 along with SHARED-DOC-004.

## Answer

Restore RH8107-001 to its text on `main`, remove SHARED-DOC-004 from RH8107's `Uses`, and keep the SHARED-DOC-008 clause on RH8107-004. This supersedes the RH8107-001 part of the earlier entry.

## Reason

None given.
