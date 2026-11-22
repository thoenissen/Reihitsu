Role: spec checker
Commit: 1a35546
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Read: `00-lead.md`, `01`-`07`, the three change docs, the spec diff, and the test diff.

`scripts/test.sh --project architecture`: `Passed! - Failed: 0, Passed: 198, Skipped: 0, Total: 198`.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 4 | `sdd/specs/shared/casing.md` header, `sdd/specs/README.md` line 25 | The diff adds the type `CasingRenameSafetyUtilities` (it carries the SHARED-CASING-013 guard that withholds a fix needing `@`), and it is not among the spec's code owners. Low: SpecConsistencyTests pass; add it to both lists if the spec author judges it an owner of the requirement. | spec author |

## Checked

### thoenissen/Reihitsu#971
- 1 ok: behavior change; `casing-fix-name-derivation.md` Accepted, has `Visible output changes`, `Decisions` records batch 1 (Q1, Q10 answered A).
- 2 ok: SHARED-CASING-006 rewritten (leading run, first word only); RH4105/RH4106/RH4107/RH4115/RH4117/RH2001 examples consistent with the diff; format is followed.
- 3 ok: no new ID.
- 4 ok (see finding 1 for the run).
- 5 ok: no rule page example involves acronyms; no page change needed.
- 6 ok: Q1 and Q10 answered.
- 7 ok: architecture green; SHARED-CASING-006 has 9 tagged tests.
- 8 ok: Core and rule tests carry the tag; spec "Examples for the tests" are all tagged.

### thoenissen/Reihitsu#1072
- 1 ok: `casing-fix-keyword-names.md` Accepted, Q6 answer recorded.
- 2 ok: SHARED-CASING-013 stated by cause; RH4105/RH4115/RH4117 specs updated.
- 3 ok: SHARED-CASING-013 inserted in `requirement-ids.txt` at its sorted position, none reused.
- 4 finding 1.
- 5 ok.
- 6 ok.
- 7 ok: 3 tagged tests.
- 8 ok.

### thoenissen/Reihitsu#1451
- 1 ok: change doc section 3-6 and Decisions (Q2-Q5 answered A).
- 2 ok: SHARED-CASING-001/002/003/006, RH4005-002, RH4121-003 match the diff and the tests.
- 3 ok.
- 4 ok: RH4005/RH4121 owners exist and are listed in `sdd/specs/README.md`.
- 5 ok: RH4005/RH4121 pages carry no prefix-with-underscore example; nothing to update.
- 6 ok.
- 7 ok.
- 8 ok: RH4005-002, RH4121-003, SHARED-CASING-001..003 tagged.

### thoenissen/Reihitsu#1434
- 1 ok: `casing-names-dictated-elsewhere.md` Accepted; Q7-Q9, Q11, Q12 answered A.
- 2 ok: SHARED-CASING-014 plus amendments of 007/009 and RH4102-RH4104, RH4112-RH4114, RH4116 specs match the diff.
- 3 ok: SHARED-CASING-014 is the next free ID (the issue's proposed 011 was taken).
- 4 finding 1 (no new owner for the dictated-name logic beyond `CasingAnalyzerBase`, which is listed).
- 5 ok.
- 6 ok: the RH4120 follow-up is under `Follow-up candidates` in `00-lead.md`; confirm the issue manager picks it up (the lead's list still reads "(none yet)" in its section, with the candidate only in the progress log).
- 7 ok: 7 tagged tests for SHARED-CASING-014.
- 8 ok. Note: the "same in another project of the solution" variant is tested for methods only (04, *Not covered*); not a finding.
- 9 pending: `sdd/work/casing-fix-names/` is the only work folder; the lead deletes it before publishing.
