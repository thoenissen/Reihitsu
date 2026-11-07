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

| # | Kind | Question | Answer |
|---|---|---|---|
| Q1 | Output question (#1391) | Continuation line inside the only type of a base list (`class B : System. // c⏎IDisposable`, also `enum E : System. // c⏎Int32`) | A — aligned under the first token of the type (column of `System`), like a two-type list's continuation; not left at column 0 |
| Q2 | Output question (#1611) | Raw string whose closing line is indented with tabs | A — each tab counts as one column, as RH5202 counts it (`12 spaces + 4 tabs` for the example), matching the RH5202 fix's output; tab-width alignment stays with #1432 |

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use `claude/serene-cori-ckkue0` instead of `claude/batch-1243-<slug>` | The session may push only to the designated branch | Branch naming is internal | claim commit |
| 3 | Skeptic | Arch. finding #1611: closing-line width measured by a private spaces-only counter in the formatter and by Core `RawStringLiteralUtilities.GetLeadingWhitespaceLength` in RH5202 and its fix | Change the structure in this run: the formatter uses the shared Core measure; no private counter stays | The narrow fix (widening the private counter) would keep two statements of one measure (FC-015); the change stays in components the run touches | The output is decided by FMT-RAW-001/-002 and RH5202-003; tabs are output question 2 | — |
| 4 | Skeptic | Arch. finding #1243: "add a line break unless the gap already holds one" is written separately in each line-break rewriter | The three rewriters the fix touches (binary operator, ternary `?`, ternary `:`) use one shared check in the line-break phase instead of three new copies; migrating the rewriters that already carry their own check is a follow-up | A narrow fix would add three copies; consolidating the existing ones widens scope past the run's mechanism | Output demanded by unchanged FMT-BREAK-030 | — |
| 5 | Skeptic | FMT-BREAK-015 and clarified FMT-BREAK-016 both claim the gap in front of a parameter default `=` | One owner: FMT-BREAK-016 defers to FMT-BREAK-015 inside parameter lists; route to spec author | One rule stated in one place; both give the same output, so parameters need no production change | Same output either way | — |
| 6 | Skeptic | #1149 expected RH5111 fix output in 01 (`A = 1` without comma) | Assert `A = 1,`: the trailing comma stays | Observed fix output; the comma is user code | No output changes beyond the join | — |
| 7 | Skeptic | #952 already fixed on main | Regression test only; `Closes #952` stays since the issue's purpose (regression test) is met | Triage comment says the issue closes once the test lands | Nothing changes | — |
| 8 | Skeptic | Order: skeptic amendment (decision 5) before or after the output-question batch | Ask the batch now, then route the amendment together with the answers in one spec-author spawn | The amendment does not touch the questions; one spawn instead of two | Internal ordering | — |
| 2 | Lane | Lane for #1149 and #1383, whose bodies say behavior change | Bug fix, all six | The later triage comments on both issues classify them as bugs against FMT-PIPE-014 with spec clarifications; the issues are `agent-filed` | Lane is internal; the outputs are demanded by FMT-PIPE-014 (to be confirmed by the skeptic) | — |

## Follow-up candidates

1. Anonymous-object member `new { X⏎= 1 }`: `= 1` lands at the statement column; recommended join (01-spec-author).
2. Using alias `using Alias⏎    = System.String;`: `=` line moves to column 0; recommended join (01-spec-author).
3. List pattern: a comment in front of a list pattern's `]` stays in the `]` column; recommended one level deeper (01-spec-author).
4. First type argument pushed onto its own line by a comment lands at block column (02-skeptic).
5. Lambda `=>` alone on its line (02-skeptic).
6. RH5111 does not report compound assignments — P11 question (02-skeptic).
7. RH5106/RH5107 do not report indexer parameter lists — P11 question (02-skeptic).
8. Query clauses at statement column outside an argument-like list (02-skeptic).
9. Line-break rewriters that already carry a private "line break unless one exists" check: migrate to the shared check (decision 4).

## Worklist / next step

- Done: claim, draft PR, prepare.sh; spec author 01 at b023a19 (clarifications for #1383, #1149, #1611; no output questions). Lane check after 01: confirmed.
- Points for the skeptic: #1149 enum-member `=` treated as already required (RH5111-004, P5-2); #1611 leftward shift removes non-space leading whitespace (derived from RH5202-003, SHARED-FIX-007, FMT-RAW-001).
- Skeptic 02 at 57ac318: all premises hold; #952 regression test only; #1391 omitted type argument and lone-CR parts already fixed (regression tests only).
- Output questions batch 1 answered: Q1 A, Q2 A.
- Next: spec author (03) for decision 5 and the answers Q1/Q2; then test implementer (04).
