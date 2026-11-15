Role: spec checker
Commit: 9681cf8
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict. I checked the diff `origin/main...HEAD` against the specs, the two change docs, the rule page, and the handovers 01-08.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 2 | `sdd/specs/shared/comment-placement.md` SHARED-COMMENT-003 | The diff changes where a diagnostic on a `///` or `/** */` comment lies for every rule that uses `CommentPositionAnalyzerBase` and for RH5048 (`CommentPositionUtilities.GetCommentLocation`: from the first `///` or `/**` to the end of the text, without the line break). Lead decision 13 accepts it, and RH5204-005 states the span for RH5204 only. SHARED-COMMENT-003 says only "located on that comment", so no requirement states the span for RH5033-RH5049. A one-sentence clarification (a documentation comment is reported from its first `///`/`/**` to the end of its text, without the line break) would close it. Minor: the tests assert the span, and no output that existed on `main` changes, because documentation comments were not reported there | spec author |

## Checked
1. Change doc exactly when required: ok. `rh5109-parameters-on-one-line.md` (RH5109-001 changed, RH5109-003 example) and `rh7304-fix-for-comment-led-region-line.md` (RH7304-004 changed) are `Accepted`, have `Visible output changes` and no task list; every listed visible change has its answer under `Decisions` and in `00-lead.md` (answers 2, 3: OQ1 A, OQ4 A, OQ5 A). No change doc for SHARED-INIT-001, RH7304-001, RH5204-005, SHARED-ATTR-019/020, SHARED-COMMENT-001, RH5111-005 and the SHARED-DIAG-010 exception is right: each is a clarification or keeps `main`'s output (OQ2 B and OQ3 B; RH5111 has no production change).
2. Specs match the diff: finding 1; otherwise ok. Checked against the code: RH5109 (parameters alone decide, comment/directive cases), RH7304 first element (line-start comment, earlier-line comment), SHARED-ATTR-019 shape span and SHARED-ATTR-020 stretch, SHARED-INIT-001 (only the line-start comment is checked), SHARED-COMMENT-001 detached-documentation rule, RH5305-001 (bracket column skipped behind a line-start comment; the spec text "no comment in front of it on the bracket's line" already says this). Format of changed requirements (normative sentence, `Why:`, examples with markup) ok.
3. IDs: ok. No requirement ID added, retired, or renumbered (headings of SHARED-ATTR-019/020 and RH5111-005 renamed only); `requirement-ids.txt` is untouched and needs no line.
4. Code owners: ok. `scripts/test.sh --project architecture`: "Passed! - Failed: 0, Passed: 118, Skipped: 0, Total: 118". Owners of the touched specs exist; no type renamed, added, or removed in a spec header; `sdd/specs/README.md` untouched and still matches.
5. User documentation: ok. `documentation/rules/RH5109.md` Description matches change-doc entry 6 and the maintainer's answer (OQ4 A); short, no formatter mention, footer present, Violation/Correction examples still reported and equal to the fix output. `RH7304.md` unchanged as the change doc states. Diagnostic range and help-link pages unchanged.
6. Open questions routed: ok. OQ1-OQ5 are answered in `00-lead.md` (Maintainer answers 3); no output question from `01`, `02`, `03` is open; follow-up candidates F1-F6 are under `Follow-up candidates` and none is written into a spec.
7. Requirements covered: ok. Every new or changed active requirement has a `[Requirement]` test in the diff: RH5109-001/-003, RH7304-001/-004, RH5204-005, RH5111-005 and SHARED-DIAG-010 (characterization), SHARED-ATTR-019/-020 (RH5506, RH5514, RH5531, RH5524, RH5529 and the 11 rules), SHARED-INIT-001, SHARED-COMMENT-001/-011 and the per-rule instances.
8. Test tags: ok. Tag counts in the diff agree with the `Tests` tables of 04, 05 and 07 (for example RH5204-005 x13, RH5109-001 x8, RH7304-001 x4, SHARED-ATTR-019 x28, the eleven `RH55xx-004` once each); every requirement named under `Examples for the tests` in 01 is tagged on at least one added or newly tagged test.
9. Nothing left behind: ok. `git ls-files sdd/work` shows only `sdd/work/comment-directive-layout-reports/`, which the lead deletes before publishing.

## Whole run
Only finding 1 (minor, spec wording). Nothing blocks the auditor; the lead may fold the clarification into a short spec author round or record the gap as a decision.

## thoenissen/Reihitsu#1147
ok (SHARED-INIT-001, RH7304-001 clarifications match the shared helper in `CommentPositionUtilities`; RH5305-001 wording unchanged on purpose).

## thoenissen/Reihitsu#1152
ok (RH5204-005 states the span; tests tagged).

## thoenissen/Reihitsu#1386
finding 1 (span for documentation comments in RH5033-RH5049 not stated); SHARED-COMMENT-001 clarification matches `IsDocumentingComment`.

## thoenissen/Reihitsu#1387
ok (SHARED-ATTR-019/020 match `TargetAttributeListShapeAnalyzerBase`; RH5102-004, RH5109-004, RH5111-005 conditional parts unchanged and covered by characterization tests; RH3002-009 audit finding is F2).

## thoenissen/Reihitsu#1594
ok (RH7304-001/-004 and the change doc match the analyzer and fix).

## thoenissen/Reihitsu#1655
ok (spec aligned to `main` per OQ2 B/OQ3 B; no production change; SHARED-DIAG-010 names the exception).

## thoenissen/Reihitsu#1736
ok (change doc, RH5109-001/-003, rule page, and analyzer agree).
