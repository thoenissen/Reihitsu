Role: spec checker
Commit: 490e50f
Verdict: consistent

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## #1400

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

### Checked
1. Change doc: ok. Only clarifications (FMT-INDENT-010, -011, -028, FMT-REGION Non-goals/-005 wording); every statement describes today's output, pinned by characterization tests; no requirement added, changed, or retired; no `sdd/changes` file in the diff, none required.
2. Specs match the diff: ok. No production code changed; requirements keep format (normative sentence, `Why:`, examples); FMT-INDENT-011 retitled in place (ID unchanged).
3. IDs: ok. No ID added, removed, or renumbered; `requirement-ids.txt` untouched.
4. Code owners: ok. No owner changed; `Uses` additions (FMT-REGION-006/-007, FMT-TAB-001..003, SHARED-BLANK-007) resolve (architecture run below).
5. User documentation: ok, no rule page affected.
6. Open questions: ok. None raised; decisions 3 to 5, 7, 8, 10 in `00-lead.md` cover the skeptic's choices.
7. Coverage: `scripts/test.sh --project architecture`: Passed! Failed: 0, Passed: 247, Skipped: 0, Total: 247. No new requirement; the clarified ones are covered by the new tests.
8. Tags: ok. All 9 new tests carry `[Requirement]` (FMT-INDENT-010 x4, FMT-INDENT-011 x3, FMT-INDENT-028 x2), matching the test implementer's table; every requirement the spec author named (010, 011, 028) is tagged.
9. Nothing left behind: ok. `sdd/work/` holds only this run's directory (the lead deletes it before publishing).

## #1936

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

### Checked
1. Change doc: ok, none required (`Why:` wording of FMT-INDENT-021 only).
2. Specs match the diff: ok. The new `Why:` (continuation line per FMT-INDENT-034, same column as the section's statements, the colon that ends the label) contradicts no requirement; behavior is unchanged and pinned by existing tests named in 04-test-implementer.
3. IDs: ok. 4. Owners: ok. 5. User docs: ok, none affected. 6. Open questions: ok, none. 7. Architecture run as above. 8. Tags: ok, no new test for this issue (text only). 9. ok.

## Whole run

- Verdict: consistent. Diff against `origin/main`: two spec files (`indentation.md`, `region-formatting.md`), two new test files, this run's `sdd/work/` files.
- Lane note: tests lane matches the diff (spec text plus tests, no production code).
- Follow-up candidates: none.
