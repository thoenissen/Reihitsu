# Bound SHARED-EQUAL-002 instead of keeping an exhaustive list

| | |
|---|---|
| Date | 2026-10-20 |
| Escalation | 7 |
| Run | issue #996 |

## Question

The auditor's re-audit closed every earlier finding, but the repair had turned SHARED-EQUAL-002 (and RH1001-003) into an exhaustive list of comparer-argument forms that do not count as custom, and the code disagreed on three kinds of argument: a constant holding `null` is reported (the code decides constants by value; the spec said any field counts as custom), `default` of a type parameter constrained to the comparer interface is reported (the spec named only reference and nullable value types), and a `switch` expression without arms behaves opposite to what the list implied. Each round on the exhaustive list had surfaced new gaps. Options: bound the list (state the decided forms, leave every other form unspecified through a coverage sentence, add a `null` constant as not custom with a tagged test, file `default` of a type parameter as a struct-equality question with a characterization test), fix the three cases inside the exhaustive list, or stop and leave the draft.

## Answer

Bound the list: drop the exhaustive claim, let a coverage sentence leave every unlisted argument form unspecified, specify a constant with the value `null` as not custom with a tagged test, and file `default` of a type parameter as a struct-equality open question pinned by a characterization test. Then the spec checker, the auditor on the repair only, and the validator run.

## Reason

None given.
