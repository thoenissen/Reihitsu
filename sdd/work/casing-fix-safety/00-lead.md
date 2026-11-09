# Lead — casing-fix-safety

- Issues (prompt order): thoenissen/Reihitsu#1597, #1598, #1016, #1015, #1084 — all labelled `agent-filed` (goals are proposals).
- Slug: `casing-fix-safety`
- Branch: `claude/relaxed-franklin-o3mkrk` (session-designated branch; see decision 1)
- Draft PR: thoenissen/Reihitsu#1675
- Lane: **bug fix** (all five)
  - #1597 — observed vs expected: casing fixes rebind `foreach`/query clauses or leave a `[CallerArgumentExpression]` literal behind (SHARED-FIX-003).
  - #1598 — suspected (unconfirmed) excluded-code miss in a multi-targeted project (SHARED-FIX-003); must be reproduced first.
  - #1016 — casing conversion produces names the same rule still reports (SHARED-FIX-001).
  - #1015 — RH4102 Fix All corrects only one event kind (SHARED-FIX-002).
  - #1084 — after the triage split (comment on the issue), only the bug part remains: an interface named `_` becomes `I_`, still reported; the fix must be withheld where no usable name remains (SHARED-CASING-008, SHARED-FIX-001). Keeping an existing prefix and every identifier character moved to #1451 and is out of this run.
- Roles: skeptic → (spec author only for a clarification) → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist.
- Maintainer clarifications: #1598 triage comment — unconfirmed, reproduce with a test before building anything. #1084 triage comment — scope narrowed as above.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session-designated branch `claude/relaxed-franklin-o3mkrk` instead of `claude/batch-1597-<slug>` | The session environment only permits pushing to this branch | Branch name is internal | claim commit |

## Follow-up candidates

(none yet)

## Worklist / next step

- Next: skeptic (premise + reachability for all five issues; whether a spec clarification is needed).
