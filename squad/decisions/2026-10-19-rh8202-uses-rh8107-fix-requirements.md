# May RH8202 reference RH8107's fix requirements instead of moving them to shared/?

| | |
|---|---|
| Date | 2026-10-19 |
| Escalation | none of 1–10; asked under the run instruction to escalate before any requirement moves out of `analyzer/RH8107.md` |
| Run | issue #984 |

## Question

RH8202's fix removes a `<value>` element through the same helper whose behavior RH8107-006 (removal of the element and the lines it owns) and RH8107-007 (no fix without an end tag) specify. `sdd/README.md` places behavior that two specs rely on under `shared/`, but moving these two requirements retires both RH8107 IDs. Options: RH8202 lists RH8107-006 and RH8107-007 under `Uses`, the first rule-to-rule reference on `main`; move both to `shared/element-documentation.md` as new SHARED-DOC IDs and retire the RH8107 ones; or give RH8202 its own duplicate requirements.

## Answer

RH8202 lists RH8107-006 and RH8107-007 under `Uses`; no requirement moves or retires in this run. Moving them to `shared/` can be proposed later as a follow-up.

## Reason

None given.
