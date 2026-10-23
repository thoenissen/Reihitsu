# P37 on the statement blank-line fixes without a conforming shape (#1380)

| | |
|---|---|
| Date | 2026-10-22 |
| Escalation | 1 |
| Run | issue #1380 |

## Question

The shared side of P37 (a required blank line goes after directives, never directly before one) has no conforming shape: every statement blank-line fix (RH5001–RH5019, RH5021, RH5029) inserts its line break at the start of the statement's leading trivia, so a directive in the gap always ends up below the new blank line (tracked by #1385); a statement whose last trivia is a directive is not reported (SHARED-STMT-003). Options: (a) write a new SHARED-STMT requirement and tag only shapes that cannot fail, as for SHARED-DIAG-004; (b) record P37 now only on the formatter side (FMT-BLANK-016) and let #1385 add the shared requirement with its fix and tests; (c) amend SHARED-STMT-004 instead of minting an ID.

## Answer

(b): this run writes P37 only as FMT-BLANK-016 in the formatter spec; the shared requirement is added by #1385 together with the fix and its tests.

## Reason

none given.
