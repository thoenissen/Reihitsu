Role: lead
Commit: 0fa1778c (origin/main at claim)
Verdict: lane claimed — bug fix

# Run withhold-unsafe-fixes

Branch: `claude/sweet-archimedes-fzb7hv` (the session's designated branch; see Decisions taken #1). Draft PR: thoenissen/Reihitsu#1599.

## Issues

| Issue | agent-filed | Lane | Reason |
|---|---|---|---|
| thoenissen/Reihitsu#1257 | yes | bug fix | RH3001/RH3005/RH3101/RH3105 code fixes produce a generic invocation that does not compile; SHARED-FIX-003 already demands withholding. |
| thoenissen/Reihitsu#1279 | yes | bug fix | RH3004 code fix. Narrowed by triage comment: keeping `static`/`async` and parenthesizing moved to #1436 (out of this run). Remaining: withhold the fix where the rewrite would not compile or would change meaning (cast and binary operands, e.g. `() => 1 + h`, CS0019; SHARED-FIX-003), and lay out the result as the formatter does (SHARED-FIX-007, e.g. `= delegate (int x)` with `{` on the next line). |
| thoenissen/Reihitsu#1281 | yes | bug fix | RH3005, RH3101, RH3106, RH3201 fixes break code or change what runs; report stays, fix withheld (P1, P23). Its RH3103 part is superseded by #1417 (triage comment) and is not handled under #1281. |
| thoenissen/Reihitsu#1282 | yes | bug fix | RH3101, RH3104, RH3105, RH3106, RH3107 fixes delete comment text; fix withheld (SHARED-FIX-003). The issue suggests doing RH3106/RH3107 on their shared empty-list removal. |
| thoenissen/Reihitsu#1417 | yes | bug fix | RH3103 must not report where `T?` is not valid C# (P38, SHARED-DIAG-011); `using N = System.Nullable<int>;` reported only from C# 12; extra instance `using static System.Nullable<int>;` (issue comment). Triage comments: bug-fix lane, no change doc; RH3103-006 and other rule-spec text aligned to SHARED-DIAG-011 as a clarification in the same PR. The body's "behavior change / change doc" line no longer applies. |
| thoenissen/Reihitsu#1422 | yes | bug fix | RH3104 must not report `new S();` as a statement (`default(S);` is CS0201); RH5417 must not report below C# 11 where the line breaks would be CS8967. Triage comment: bug-fix lane, no change doc; rule-spec text aligned to SHARED-DIAG-011 as a clarification. |

All six issues are `agent-filed`: goals are proposals.

## Roles

Lane bug fix runs: spec author (clarifications only: rule specs that still disagree with SHARED-DIAG-011 / SHARED-FIX-003 for #1417 and #1422, and any other clarification the issues expose), skeptic (premise and reachability check, incl. whether #1481 or other recent main commits already fixed instances), test implementer (reproductions), implementer, cleaner, spec checker, auditor, validator. Issue manager once before publishing if follow-up candidates exist.

## Maintainer clarifications

Triage comments on the issues (written under the maintainer's account) narrow #1279 (static/async and parenthesizing moved to #1436), remove RH3103 from #1281 (superseded by #1417), and move #1417 and #1422 into the bug-fix lane without a change doc.

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name `claude/batch-1257-<slug>` per lead.md vs the session's designated branch | Use `claude/sweet-archimedes-fzb7hv` | The session environment allows pushes only to its designated branch | Branch names are internal | claim commit |
| 2 | Claim | Lanes of #1417/#1422 (body says behavior change) | Bug fix for all six issues | Triage comments on both issues move them to bug fix: SHARED-DIAG-011 already requires the behavior | Lanes are internal | claim commit |

## Follow-up candidates

None yet.
