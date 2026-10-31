# One alignment contributor per line, and the join's spacing prediction

| | |
|---|---|
| Date | 2026-10-30 |
| Escalation | 12 |
| Run | issue #1487 |

## Question

1. The one-contributor-per-line test finds 172 lines in shipped sources placed by `ArgumentAlignmentContributor` and an inner construct's contributor. (A make it hold with unchanged output; B narrow the property to "the inner contributor writes last"; C drop the test and draft a follow-up)
2. Moving `SpacingPolicy` to `Pipeline/Core` drags the `ISpacingRule` types along, which two architecture namespace tests pin. (A move policy and rules, retarget the tests; B remove the line-break phase's spacing prediction and leave the joined gap to the horizontal-spacing phase, falling back to A if output changes)

## Answer

The maintainer delegated these and every later escalation of this run to the lead's recommendation, and provisionally accepted any further change doc of the run, so that the run continues to the pull request without waiting. Applied: 1A, 2B.

## Reason

The maintainer is unavailable for the rest of the run.
