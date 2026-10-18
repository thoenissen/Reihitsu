Role: lead
Commit: 53990ed
Verdict: lane claimed — tooling

## Issue

thoenissen/Reihitsu#1068 — specify unsafe fixes, per-rule convergence, fixes that undo each other, and no-op fixes in `sdd/specs/shared/code-fixes.md`, and add one sentence to `AGENTS.md` (*Code-fix coverage*). Source of the policy: principle decisions P1 and P7 on thoenissen/Reihitsu#912 (comment 6253795346). Claim: draft PR thoenissen/Reihitsu#1100.

Branch: `claude/admiring-euler-a4zxhb` (the session's designated branch, used instead of `claude/issue-1068-<slug>` because this session may push only there).

Slug: `shared-fix-safety`.

## Lane

Tooling. The diff will touch `AGENTS.md` (a tooling path), `sdd/specs/` (docs), and test files (covering tests for the new requirements of the enforced spec `shared/code-fixes.md`). The narrowest lane admitting all three is tooling.

## Roles

| Role | Run | Reason |
|---|---|---|
| Spec author | run | Added beyond the tooling column: the run changes an enforced spec and the `AGENTS.md` text, and the tests lane that tooling widens from runs the spec author. It writes the spec diff, the requirement IDs, and the `AGENTS.md` sentence. |
| Skeptic | run | Added beyond the tooling column, as in the tests lane: checks the spec diff against the code and against the other `shared/` specs. |
| Test implementer | run | Covering tests, tagged with the new requirement IDs. |
| Implementer | skip | The maintainer ruled out any production change; every covering test must pass today. A covering test that fails today goes back to the lead, not to an implementer. |
| Cleaner | run | Formats the changed test files, warning-free Release build. |
| Spec checker | run | |
| Auditor | run | |
| Validator | run | |

## Maintainer clarifications (summarized)

- No change doc. SHARED-FIX-003, -004, -005 and the per-rule clarification of SHARED-FIX-001 write down policy the repository already intends (`AGENTS.md` *Code-fix coverage*). Code that violates them is a bug handled by separate follow-up issues (thoenissen/Reihitsu#1073, #1075, #1076 and others linked from #912), not in this run.
- SHARED-FIX-003 is written without the exception for tabs in region names; thoenissen/Reihitsu#1083 adds that exception later.
- Every covering test must pass on today's code: each new requirement is covered by an existing case the code already handles correctly (for example, a casing fix that is withheld when the new name collides, for SHARED-FIX-003). No production change.
- In scope exactly: the four spec changes the issue lists (SHARED-FIX-001 clarified per rule; new SHARED-FIX-003, -004, -005), the narrowed non-goals (the two failure classes now covered — fix cycles and no-op fixes — are removed from the non-goals; "a fix leaves diagnostics of other rules behind" stays a non-goal), and the one `AGENTS.md` sentence the issue gives.
- Out of scope: fixing any rule that violates the new requirements; any requirement the issue does not name; `sdd/work/coordinator/`. No new issues except open questions filed as `sdd/README.md` describes, and those only through the lead.
- Parallel runs: #1069 edits the neighbouring `shared/diagnostics.md`; the only shared file is `sdd/specs/requirement-ids.txt`.
