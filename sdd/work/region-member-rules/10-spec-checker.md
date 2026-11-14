Role: spec checker
Commit: 1c90aa7
Verdict: #1264: consistent; #1333: consistent; #1334: consistent; #1339: consistent; #1337: consistent; #1412: consistent; #1592: consistent

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## thoenissen/Reihitsu#1264

### Findings
None.

### Checked
Changes: RH7308, RH7309, RH7311, SHARED-REGGROUP specs and rule pages RH7308/RH74xx match the diff (innermost-region judging, enum ownership, branch regions). Examples carry the analyzer markup. No requirement is added, retired, or renumbered.

## thoenissen/Reihitsu#1333

### Findings
None.

### Checked
SHARED-REGGROUP-005 (declarator judged each on its own) and the RH7411 example match the diff and the RH7403/7407/7411 tests.

## thoenissen/Reihitsu#1334

### Findings
None.

### Checked
SHARED-REGGROUP-006 / SHARED-DIAG-009 text and examples match the diff; the answer Q2 is recorded in `00-lead.md`.

## thoenissen/Reihitsu#1339

### Findings
None.

### Checked
RH7102-005/-006 and RH7109-005/-006 match the rewritten tests; the SHARED-ORDER-004 shape is pinned with no fix and tagged.

## thoenissen/Reihitsu#1337

### Findings
None.

### Checked
RH7105/RH7106/RH7107/RH7108, RH5102, RH5109, RH5111 specs add SHARED-DIAG-008 under `Uses` and match the diff. Q3 and Q4 answers are recorded.

## thoenissen/Reihitsu#1412

### Findings
None.

### Checked
RH7105/RH7106 declaring-part behaviour is covered by tagged tests; no spec contradicts it.

## thoenissen/Reihitsu#1592

### Findings
None.

### Checked
RH7309-004/-005 fix behaviour matches the diff.

## Whole run

| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| none | | | | |

### Checked
1. ok: bug-fix lane, no change doc in the diff (`sdd/changes` untouched), and none is required.
2. ok: every behavior the diff changes is stated by a requirement; the new examples follow the format.
3. ok: no requirement heading added or removed; `requirement-ids.txt` is unchanged and needs no edit.
4. ok: owners (`SpecConsistencyTests`) pass; `sdd/specs/README.md` is unchanged and needs no edit.
5. ok: rule pages RH7308 and RH7401–RH7412 are short, mention no formatter and no withheld fixes; the footer is unchanged. No `### Correction` is touched.
6. ok: Q1, Q1b, Q2, Q3, Q4 are under `Maintainer answers` in `00-lead.md`; the follow-up candidates sit under `Follow-up candidates`; the handovers hold no open output question.
7. ok: `scripts/test.sh --project architecture` gave `Passed! - Failed: 0, Passed: 118, Skipped: 0, Total: 118`.
8. ok: a scan of every test method the diff adds found none without a `[Requirement]` tag; the rewritten #1339 and #1311 tests carry RH7102-005/-006, RH7109-005/-006, RH7311-001, plus SHARED-ORDER-004.
9. ok: the only `sdd/work/` is this run's, which the lead deletes before publishing.
