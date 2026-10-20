# Allow a further repair round after the auditor blocked

| | |
|---|---|
| Date | 2026-10-20 |
| Escalation | 7 |
| Run | issue #996 |

## Question

The auditor blocked after the spec-checker repair round and the extra repair the maintainer had already allowed. Its three findings concern only text and tests the run added: RH1002-001 claims every bound `GroupBy`/`Join`/`GroupJoin` call is reported, but LINQ query syntax is not; RH1001-003 says a declaration whose creations all pass a custom comparer is not reported, but `switch` and conditional expressions with a `throw` arm and a collection expression with `with(comparer)` are reported, and SHARED-EQUAL-002 counts a comparer argument that can only produce `null` or throw as custom; and the not-reported side of several SHARED-EQUAL-002/RH1001-003 forms has no tagged test. Two non-blocking hints were offered for folding in: SHARED-EQUAL-002's "can produce" wording against a local holding `null`, and `PropertySymbolExtensions.IsAutoProperty`, used only by RH2001 and claimed by no spec. Options: one more repair including the hints, one more repair without the hints, or stop and leave the draft.

## Answer

One more repair including the two hints: the spec author fixes wording and coverage and drafts the new open-question texts, the test implementer adds characterization tests and tests of the not-reported side, then the cleaner and the spec checker run, the auditor audits the repair only, and the validator runs.

## Reason

None given.
