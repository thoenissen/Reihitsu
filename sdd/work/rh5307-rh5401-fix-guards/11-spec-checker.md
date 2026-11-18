Role: spec checker
Commit: cbdfc74
Verdict: consistent

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| | | | None. | |

## Checked
1. Change doc: ok. `sdd/changes/2026-11/rh5307-rh5401-fix-the-reported-construct.md` is Accepted, has `Visible output changes` and no task list. Every visible change has a maintainer answer in its `Decisions` (#1217 Q1/Q3, #1219 Q1, run questions 1 to 6). The questions 3 and 5 decisions state that question 6 supersedes their offered fix.
2. Specs match the diff: ok. RH5307-001 (clarified), RH5307-004, RH5401-003, RH5401-004 state what the code fixes and the tests assert; both specs follow the format (normative sentence, Why, examples, markup). RH5401-003 no longer carries the earlier no-blank-line exception, consistent with the escalation-9 answer. `Uses` gains FMT-STRUCT-028 and FMT-BLANK-011.
3. IDs: ok. No new IDs; the four are in `requirement-ids.txt` unchanged.
4. Code owners: ok. Headers name existing types, `sdd/specs/README.md` lists both specs with matching owners.
5. User documentation: ok. RH5401 page sentence matches maintainer answer 2B. RH5307 page gains the `?[ ]` sentence; it is a scope note, kept on the maintainer's #1217 Q1 decision (lead decision 2), not a withheld-fix note. Corrections and footers are unchanged and no formatter mention was added.
6. Open questions routed: ok. Every output question (01 Q1-Q3, 03 Q1-Q4, 04 Q5, escalation 9) has an answer in `00-lead.md`; the FMT-BREAK-014 `c?[1,⏎2]` item is under Follow-up candidates.
7. Coverage: ok. `scripts/test.sh --project architecture --no-install`: "Passed! - Failed: 0, Passed: 198, Skipped: 0, Total: 198". Every new or changed requirement has a covering `[Requirement]` test in the diff (RH5307-001 null-conditional, RH5307-004, RH5401-003, RH5401-004 including every withheld and offered example).
8. Tags: ok. Every added test method in the test implementer's tables carries the `[Requirement]` attributes listed there (spot-checked through the diff of both test files); the tables name all requirements the tests carry.
9. Nothing left behind: ok for now. `sdd/work/` holds only this run's directory; the lead deletes it before publishing.
