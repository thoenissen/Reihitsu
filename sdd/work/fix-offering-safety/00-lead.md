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
| 2 | skeptic | #1150: SHARED-FIX-011 example (RH6023 no longer formats), FMT-PIPE-005 "the one reason" ignoring unsupported-shape refusal, first sentence readable as blanket compare | Route corrections to a fresh spec author after the maintainer's answers; affected-fix list per 02-skeptic.md | Skeptic observed it on this tree; the maintainer's text stays, only example and consistency fixed | Spec wording only; behavior decided by the maintainer's 2026-11-06 answer | pending |
| 3 | skeptic | #1150 item 2 (RH5405–RH5407 directive body) | Drop: already resolved and tested (SHARED-BRACE-003) | Spec author and skeptic both confirm | Nothing changes | n/a |
| 4 | skeptic | #1673 scope | Narrow to RH5308 only; modifier ordering, RH7107, RH7108 do not reach an unchanged result / already check at registration | Skeptic evidence; agent-filed goal ("drop a fix where no input reaches") | Withholding a no-op fix is decided by SHARED-FIX-005 | pending |
| 5 | skeptic | #1143 scope | Proceed on RH6016, RH8204, RH5110, RH3001, RH3102, RH7102 and the remaining `NumberOfFixAllIterations = 2` tests; RH6024/6023/5506/5204 already resolved; RH5104 and RH5405 if/else are guard-dependent (keep pass with comment) | Skeptic evidence; issue allows guard-dependent passes | SHARED-FIX-002 decides the output | pending |
| 6 | lead | #1274 option B (withhold fix) | Dropped from the output question | Contradicts SHARED-FIX-011 (this run) and SHARED-DIAG-010 | Removing an option, not deciding an output |  n/a |
| 7 | lead | #1600 fix-side line kinds | `#elif`/`#else`/`#endif` lines in a multi-line string or `/* */` that starts inside a conditional group, not any `#` line | A plain `#` check withholds fixes on Markdown raw strings; skeptic shows only these lines can hide code | Only offers more fixes than the issue's wording; rare-class withholding | pending |

## Follow-up candidates

- #1150: SHARED-FIX-011 third sentence conflicts with RH5401-004 (RH5401 reports a property the formatter keeps) — 02-skeptic.md.
- Formatter class: a continuation line whose previous line ends with a `//` comment is laid out in the enclosing statement's/member's column (`a ? // c`, `b + // c`, `int A => // c`) — 02-skeptic.md #1274 (widens the spec author's candidate below).
- Formatter places a conditional's true branch that follows a `//` comment on the `?` line in the statement's column (column 8 in the #1274 input) — 01-spec-author.md (#1274).

## State

- Last step finished: 01 spec author (e36792d): SHARED-FIX-011 written for #1150; open output questions drafted for #1600, #1274, #1678.
- 02 skeptic done (14a8b5a).
- Next: batch 1 of output questions + escalations 9/10 posted to the maintainer; waiting. After answers: fresh spec author (spec corrections for #1150, answers' spec text/change doc; lane may widen to behavior change), then test implementer.
