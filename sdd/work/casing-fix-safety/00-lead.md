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
| 2 | Skeptic | #1597 goal "rename the `CallerArgumentExpression` literal" | Dropped; the fix is withheld instead | SHARED-FIX-003 forbids changing a user literal; the issue already allows withholding | Withholding is the issue's own required output; rare class | — |
| 3 | Skeptic | #1597 refusal shape | Cheap refusal: withhold RH4103 when the new name is a compiler pattern-lookup name; withhold a parameter rename when a `CallerArgumentExpression` string names the old or new name. No binding comparison | Binding comparison is machinery for a rare class (AGENTS.md, Special cases) | Withholding a fix for a rare class | — |
| 4 | Skeptic | Architecture finding (#1016, #1084): acceptability of the new name decided in several places, none asks the rule's style check | Change the structure in this run: one check in the casing base withholds the fix when the rule would still report the new name | Narrow per-converter fixes would add further copies of the rule; stays within the casing base and its converters | Brings the code back to SHARED-FIX-001 / SHARED-CASING-008; withholding only | — |
| 5 | Skeptic | Spec clarifications | Spawn the spec author for SHARED-CASING-007 (#1597) and SHARED-CASING-008 (#1016) clarifications | Specs currently contradict SHARED-FIX-003 / leave the inputs out | Clarifications follow existing SHARED-FIX requirements | 5fed711 |
| 6 | Spec author | Change doc for SHARED-CASING-011 (withholds fixes `main` offers, e.g. a lone `getEnumerator`) | No change doc; bug-fix lane holds | Withholding a fix for a rare class (lower-case pattern-method names already violate RH4103) | Withholding for a rare class puts nothing in question (README, What reaches the maintainer) | 5fed711 |
| 7 | Spec author | Unbuilt list members of SHARED-CASING-011 | Keep the list as written; test implementer covers representative members | Same mechanism; cheap fixed list | Withholding only | 5fed711 |

## Follow-up candidates

- Renamer rewrites call sites to keep binding (`values.Select(...)` → `Enumerable.Select(values, ...)`, `box.Add(1)` → `box.Add((object)1)`), conflicting with SHARED-FIX-006; carries an output question (01-skeptic.md).

## Worklist / next step

- Done: skeptic (01-skeptic.md, eb79b88). No output questions.
- Done: spec author (02-spec-author.md, 5fed711). No output questions. Lane check: confirmed.
- Done: test implementer (03-test-implementer.md, 1c84020). #1597, #1016, #1015, #1084 reproduced (74 red tests); #1598 NOT reproduced (5 green characterization tests in RH4109 tests).
- PR link for #1598 changed to `Refs #1598`.
- OPEN: escalation 4 for #1598 asked to the maintainer; the whole run waits for the answer.
- Next after answer: implementer for #1597, #1016, #1015, #1084 (note: merging the two RH4102 providers would require changing existing tests that name `RH4102CustomEventNameCasingCodeFixProvider`).
