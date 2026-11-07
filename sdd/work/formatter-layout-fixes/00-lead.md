# 00 — Lead

Role: lead
Branch: claude/serene-cori-ckkue0 (the session may push only to this branch; it stands in for `claude/batch-1243-<slug>`)
PR: thoenissen/Reihitsu#1644 (draft)
Slug: formatter-layout-fixes
Lane: bug fix

## Issues (prompt order)

| Issue | agent-filed | Lane on its own | Reason |
|---|---|---|---|
| thoenissen/Reihitsu#1243 | yes | bug fix | Observed vs expected: the line-break phase writes a blank line when joining a lone operator, against FMT-BREAK-030 |
| thoenissen/Reihitsu#1383 | yes | bug fix | Triage comment (2026-10-24): behavior already required by FMT-PIPE-014, so bug fix without a change doc; rule texts that disagree are aligned as a clarification. Attribute-target colon part already resolved on main |
| thoenissen/Reihitsu#1149 | yes | bug fix | Triage comment P5-2: bug against FMT-PIPE-014 (enum member and parameter-default equals-value in the assignment family); FMT-BREAK-016 gains both forms as a clarification |
| thoenissen/Reihitsu#1391 | yes | bug fix | Bug against existing FMT-INDENT-012/-013/-016. Two of four instances already resolved on main (omitted type argument, lone-CR raw string); open: base-list continuation, empty wrapped list `)` |
| thoenissen/Reihitsu#952 | yes | bug fix | Bug against FMT-INDENT-009; triage says resolved on main, only a regression test is missing |
| thoenissen/Reihitsu#1611 | yes | bug fix | Formatter measures a raw string's closing-line whitespace differently from RH5202 (SHARED-DIAG-010, FMT-RAW-009) |

All six are `agent-filed`: their goals, "Done when" items, and prescribed solutions are proposals.

## Roles

Bug-fix lane: spec author (runs, for the clarifications #1149 and #1383 call for, and any other spec text the skeptic finds out of line) → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. Issue manager before publishing when follow-up candidates exist. No role skipped.

## Maintainer clarifications

None in this run so far. The issues' comments carry triage decisions (summarized above).

## Maintainer answers

_None yet._

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use `claude/serene-cori-ckkue0` instead of `claude/batch-1243-<slug>` | The session may push only to the designated branch | Branch naming is internal | claim commit |
| 2 | Lane | Lane for #1149 and #1383, whose bodies say behavior change | Bug fix, all six | The later triage comments on both issues classify them as bugs against FMT-PIPE-014 with spec clarifications; the issues are `agent-filed` | Lane is internal; the outputs are demanded by FMT-PIPE-014 (to be confirmed by the skeptic) | — |

## Follow-up candidates

1. Anonymous-object member `new { X⏎= 1 }`: `= 1` lands at the statement column; recommended join (01-spec-author).
2. Using alias `using Alias⏎    = System.String;`: `=` line moves to column 0; recommended join (01-spec-author).
3. List pattern: a comment in front of a list pattern's `]` stays in the `]` column; recommended one level deeper (01-spec-author).

## Worklist / next step

- Done: claim, draft PR, prepare.sh; spec author 01 at b023a19 (clarifications for #1383, #1149, #1611; no output questions). Lane check after 01: confirmed.
- Points for the skeptic: #1149 enum-member `=` treated as already required (RH5111-004, P5-2); #1611 leftward shift removes non-space leading whitespace (derived from RH5202-003, SHARED-FIX-007, FMT-RAW-001).
- Next: skeptic (02).
