Role: spec checker
Commit: 6f06dc3
Verdict: consistent

No finding for any of the four issues. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| – | – | – | none | – |

## Checked

### thoenissen/Reihitsu#1695
1. ok. `sdd/changes/2026-11/fix-line-end-as-formatter-writes.md` is `Accepted`, has `Visible output changes` (RH3202 whitespace behind the target) and `Decisions` with the maintainer's Q1=B. It is required because the whitespace half changes FMT-PIPE-010 for a target whose last line formatting leaves unchanged; the line-ending half is a clarification of SHARED-FIX-008 and is stated as such.
2. ok. FMT-PIPE-010 states the line-ending and whitespace rules, with two examples (mixed `\r\n`/`\n`, trailing whitespace behind `/* c */`); `ReihitsuFormatter` (`EndTargetLineAsFormatted` at the three write-back sites) matches. `Uses` gained SHARED-FIX-008.
3. ok. No new IDs; `requirement-ids.txt` is untouched, as it must be.
4. ok. No owner added or removed; `sdd/specs/README.md` needs no change.
5. ok. No rule page is affected (the whitespace result equals what the existing RH3202 page shows); no page diff.
6. ok. Q1 is under `Maintainer answers` in `00-lead.md`. The RH5403/RH5404 candidate is under `Follow-up candidates`; none is written into the spec.
7. ok. `scripts/test.sh --project architecture`: `Passed! - Failed: 0, Passed: 118, Skipped: 0, Total: 118`. FMT-PIPE-010 has covering tagged tests (`ReihitsuFormatterNodeTrailingLineEndTests`, RH3202, RH5111, RH7101 classes).
8. ok. Every added test carries `[Requirement]` for the requirements in the test implementer's table (FMT-PIPE-010, plus SHARED-FIX-008 on the mixed-document ones).
9. ok.

### thoenissen/Reihitsu#1696
1. ok. Same change doc: RH8402-002 (fix part) and RH8402-005 are listed, Q2=B is recorded under `Decisions`, and the output change is shown (`\r` to `\n`, `\n` to `Environment.NewLine`).
2. ok. RH8402-002 now limits the first-line ending to the analyzer's comparison; RH8402-005 states the detected ending, with a mixed-ending example. The `Why:` of both is updated, and `Uses` gained SHARED-FIX-008 and FMT-PIPE-007. The code fix (`LineEndingUtilities.DetectEndOfLine`, `NormalizeLineEndings(header, endOfLine)`, removal of `GetPreferredLineEnding`) matches. The RH8402-006 text names no line ending that now contradicts.
3. ok. No new IDs.
4. ok. Both owners already exist and are unchanged.
5. ok. `documentation/rules/RH8402.md` mentions no line ending and needs no change.
6. ok. Q2 is under `Maintainer answers`.
7. ok. Covered in the architecture result above; RH8402-005 and RH8402-002 have tagged tests.
8. ok. The nine RH8402 tests are tagged `RH8402-005` (one also `RH8402-006`); the analyzer-comparison test is tagged `RH8402-002`.
9. ok.

### thoenissen/Reihitsu#1707
1. ok. No change doc: the RH3003 fix change only makes the code keep the trivia that RH3003-004 and SHARED-FIX-006 already require, so it is a bug fix. The RH3005 part is characterization only.
2. ok. RH3003-004 ("keep the trivia in front of and after the literal") and RH3005-004 already state the behavior; the diff contradicts neither. Nothing new to specify.
3. ok. No new IDs.
4. ok. No owner change.
5. ok. No rule page change needed.
6. ok. The RH3005 non-reproduction is recorded as Decision 7 in `00-lead.md`; it is not a visible output question.
7. ok. The new RH3003 and RH3005 tests are tagged and covered by the architecture run above.
8. ok. All twelve added tests carry `SHARED-FIX-006` plus `RH3003-004` or `RH3005-004`.
9. ok.

### thoenissen/Reihitsu#1721
1. ok. No change doc is needed; the diff changes nothing for this issue (Decision 6).
2. ok. The observed output matches FMT-PIPE-007 and FMT-EOL-004; no spec text changed for it.
3-5. ok. Nothing touched.
6. ok. Decision 6 is recorded in `00-lead.md` and nothing is open for the maintainer.
7-8. ok. The issue adds no requirement, and no test was added for it (the existing pinning test is named in Decision 6).
9. ok. `sdd/work/` holds only `detected-line-endings`, which the lead deletes before publishing.
