# The auditor blocks after the run's repair round (#997)

| | |
|---|---|
| Date | 2026-10-20 |
| Escalation | 7 |
| Run | issue #997 |

## Question

The auditor blocked after the run's one repair round, which had fixed the spec checker's findings. Its findings: RH3103-002 claims a qualified `Nullable<int>` inside `nameof` is skipped, but the dotted forms are reported and the offered fix throws; fixes in the scope of RH3005-004, RH3001-004, RH3101-004 and RH3105-004 can turn an argument list into a generic call that does not compile; three RH3002-001 positions have no test; two wording points. Options: allow one more repair round for all findings; repair only the RH3103 finding and file the rest; or publish as is and file the cases.

## Answer

Allow one more repair round for all findings: the spec author narrows the requirements and fixes the wording, the test implementer adds the characterization and position tests, the lead files the cases in the existing open-question issues and in a new one for RH3001, then the cleaner, spec checker, auditor (repair only), and validator run.

## Reason

none given.
