# Existing tests contradict the amended SHARED-COMMENT-001 (#1139)

| | |
|---|---|
| Date | 2026-10-21 |
| Escalation | 4 |
| Run | issue #1139 |

## Question

The amended SHARED-COMMENT-001 (P13) makes a `///` or `/** */` comment that documents nothing — the compiler reports CS1587 for it — an ordinary comment. Seven existing tests tagged SHARED-COMMENT-001 assert the opposite: RH5033 does not report a documentation-looking comment behind an attribute list, and RH5038 does not report a `///` inside an argument list. The code does not conform, so the tests cannot be flipped, and the run's rules forbid a characterization test that contradicts a requirement. No follow-up covers RH5033–RH5049. Options: (a) delete the contradicting tests and trim the contradicting half of the two mixed tests, and file the shapes in the spec's open-questions issue as decided by P13; (b) keep the tests unchanged with only the SHARED-COMMENT-001 tag removed; (c) as (a), plus a drafted follow-up bug instead of an open question.

A second question: should the SHARED-COMMENT-001 example show the principle (the non-conforming shapes as reported) or only shapes that conform today?

## Answer

(a): the test implementer removes the contradicting tests and the contradicting half of the mixed tests and keeps the conforming parts; the lead files the RH5033–RH5049 shapes in a new open-questions issue for `shared/comment-placement.md`, marked as decided by P13. The spec example shows only shapes that conform today; the requirement text still states P13 in full.

## Reason

none given.
