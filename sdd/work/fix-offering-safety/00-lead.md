# Lead — fix-offering-safety

- Run: issues thoenissen/Reihitsu#1600, #1150, #1274, #1143, #1673, #1678 (prompt order)
- Slug: `fix-offering-safety`
- Branch: `claude/trusting-babbage-audv24` (session-designated branch; see decision 1)
- PR: thoenissen/Reihitsu#1718 (draft claim)
- Lane: **bug fix**

## Issues

| Issue | agent-filed | Lane | Reason |
|---|---|---|---|
| #1600 | yes | bug fix | Reported observed-vs-expected difference against SHARED-FIX-003 (fixes/formatter edit text that is string/comment content in another build configuration) |
| #1150 | yes | bug fix | Fixes offered where the formatter changes nothing (SHARED-FIX-005/007); carries spec clarification text the maintainer decided on 2026-11-06 (spec text only, no change doc) |
| #1274 | yes | bug fix | RH5308 fix does not converge on one input (SHARED-FIX-001/005, P7) |
| #1143 | yes | bug fix | Fix All needs a second application in several rules (SHARED-FIX-002) |
| #1673 | yes | bug fix | Fix registered although its action returns the document unchanged (SHARED-FIX-005) |
| #1678 | yes | bug fix | SHARED-FIX-003 gap for reorder fixes; the issue itself is an output question (A: record gap as decision in SHARED-ORDER-008; B: withhold field reorders in every partial type) |

All goals are proposals (all `agent-filed`).

## Roles

Bug-fix lane: spec author (clarification — #1150's maintainer-decided spec text; #1678 per the maintainer's answer), skeptic (premise), test implementer (reproduction), implementer, cleaner, spec checker, auditor, validator. Issue manager once before publishing if follow-up candidates exist.

## Maintainer clarifications (from issues, summarized)

- #1150: maintainer decided 2026-11-06 — in a file with a syntax error the diagnostic stays and the fix is not offered; generated code is never a reason for the formatter to decline a fix's input; a formatter-delegating fix that changes nothing is a defect of the formatter or of the diagnostic, unless a named formatter requirement declines the input. Spec text to write is listed in the issue (new SHARED-FIX requirement after SHARED-FIX-005, edits in pipeline.md FMT-PIPE-005/006 Why, line-join-rules.md SHARED-JOIN-002 Why, RH7101-001 Why, code-fixes.md non-goal).
- #1143: maintainer (2026-10-19) — Fix All must finish in one application; a guard-dependent second pass may stay with a comment naming the guard.
- #1600, #1678: triaged critical for 1.2.1 by the maintainer.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | claim | Branch name | Use the session-designated branch `claude/trusting-babbage-audv24` instead of `claude/batch-1600-<slug>` | The session environment permits pushing only to that branch | Branch name is internal | claim commit |

## Follow-up candidates

- Formatter places a conditional's true branch that follows a `//` comment on the `?` line in the statement's column (column 8 in the #1274 input) — 01-spec-author.md (#1274).

## State

- Last step finished: 01 spec author (e36792d): SHARED-FIX-011 written for #1150; open output questions drafted for #1600, #1274, #1678.
- Next: skeptic (02), then batch output questions before the test implementer.
