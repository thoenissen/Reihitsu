# Which questions reach the maintainer

| | |
|---|---|
| Date | 2026-10-31 |
| Escalation | 2 |
| Run | issue #1489 |

## Question

After four escalation rounds in one run, the maintainer asked how the squad can work more independently: what matters to the maintainer is the formatter's output, everything an end user sees; technical details should be implemented well but need not be decided by the maintainer, and the loops make a session expensive.

## Answer

For the rest of the run of issue #1489, the lead decides questions that do not change what an end user sees — spec wording, which internal facts or structure carry a decision, the text of follow-up issues for internal work, and approval of change-doc amendments that change no visible output — on its own recommendation, and records each such decision in `squad/decisions/` and the run report. It asks the maintainer only when an output the end user sees is in question: formatter output, a diagnostic, a code-fix result, or a rule page. A permanent change of the escalation rules is a separate tooling change.

## Reason

The maintainer's attention is reserved for user-visible behavior; internal decisions follow the lead's recommendation anyway, and every stop costs a round trip of the session.
