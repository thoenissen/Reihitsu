Role: spec checker
Commit: cf1ee510
Verdict: consistent

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. ok. The change doc `sdd/changes/2026-11/rh5201-first-call-behind-broken-root-line.md` exists because RH5201 reports new inputs and RH5201-006 is added. It has no task list, has `Visible output changes`, is `Accepted`, and `Maintainer answers` records Q1-Q8 as in `00-lead.md`.
2. ok. SHARED-CHAIN-005 (first-invoked-link rule, exception wording), FMT-BREAK-019, RH5201-001/-002/-006 and the examples match the diff and the tests, including Q7 (`f`) and Q8 (`g`). The examples use the CS8602-free `C Next` + `C? Opt` form. Requirement format is followed.
3. ok. RH5201-006 is inserted in sorted position in `sdd/specs/requirement-ids.txt`, and nothing else in the ledger changed.
4. ok. The diff adds no owner type to a spec header, and the RH5201 owners are unchanged. The `SpecConsistencyTests` run is covered by check 7.
5. ok. `documentation/rules/RH5201.md` gains one Description bullet and the How-to-fix wording, as the maintainer approved in Q5. The page is short, has no formatter mention and no corner-case note, and the footer is kept. The Q7 and Q8 cases are deliberately not on the page.
6. ok. The skeptic's and spec authors' output questions are all answered under `Maintainer answers` (Q1-Q8). The spec authors' 07 and 08 handovers and the cleaners' handovers list no open question and no follow-up candidate.
7. ok. `scripts/test.sh --project architecture`: "Passed! - Failed: 0, Passed: 198, Skipped: 0, Total: 198". Every new active requirement (RH5201-006) and the changed RH5201-002 have covering `[Requirement]` tests in the diff.
8. ok. The new tests carry `[Requirement]` tags that match the `Tests` tables in 04 and 09, including `f` and `g` (RH5201-002, RH5201-005) and the two existing tests retagged RH5201-006. Every requirement named under `Examples for the tests` in 03, 07 and 08 is tagged on at least one test.
9. ok. `sdd/work/` holds only `rh5201-remaining-chain-layouts`; the lead deletes it before publishing.
