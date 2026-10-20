# RH3001-004's claim that the fix adds no other parentheses (#997)

| | |
|---|---|
| Date | 2026-10-20 |
| Escalation | 7 |
| Run | issue #997 |

## Question

After the shift-operator change, the auditor closed both of its findings, but RH3001-004's negative claim that the fix adds no parentheses where the comparison would not bind differently was still false: besides assignment operators other than `=`, unneeded parentheses appear in named attribute arguments, `goto case`, and query `select`, `group`, and `orderby … descending`. The lead had narrowed the sentence once for assignment operators (`>>>=`, `??=`) without asking. Options: drop the negative claim and keep only the positive one; narrow the sentence again by the newly found positions; or publish and only file the positions.

## Answer

Drop the negative claim: RH3001-004 keeps only that the fix adds parentheses where the comparison would bind differently, and says nothing about where it adds none. Every position with unneeded parentheses is filed on #1245 Q3, and the shift-operator input filed as a description is given as complete text. Then the cleaner, the auditor on this change only, and the validator run before publishing.

## Reason

none given.
