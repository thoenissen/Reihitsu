Role: spec checker
Commit: 700620a
Verdict: consistent

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| | | | none | |

## Checked
1. Change doc: ok. `sdd/changes/2026-11/raw-string-tab-indentation.md` is required (new FMT-RAW-010, RH5202-007, RH5202-008), Accepted, has `Visible output changes`, and `Decisions` records the maintainer's answers 1A-4A.
2. Specs match the diff: ok. FMT-RAW-001/002/006, FMT-INDENT-011, FMT-TAB-003, RH5202-003/007/008, RH5601-002 (Why only) and the summary paragraphs agree with the implemented behavior; AGENTS.md IndentationPhase row names the rewrite.
3. IDs: ok. FMT-RAW-010, RH5202-007, RH5202-008 are inserted at sorted positions in `requirement-ids.txt`; none reused, renumbered, or deleted.
4. Code owners: ok. No owner changed; `sdd/specs/README.md` unchanged and consistent; SpecConsistencyTests passes within the architecture run.
5. User documentation: ok. RH5202 and RH5601 rule pages unchanged and hold no statement the change contradicts (withheld fix in a rare class is covered by general-notes.md).
6. Open questions: ok. Answers are in `00-lead.md`; the RH5202 non-report of an aligned tab literal is under Follow-up candidates, not in the spec.
7. Coverage: ok. `scripts/test.sh --project architecture`: Passed! Failed: 0, Passed: 198, Skipped: 0, Total: 198. FMT-RAW-010, RH5202-007, RH5202-008 each have covering [Requirement] tests.
8. Test tags: ok. Every test in the two new files carries a `[Requirement]`; the added or changed tests in the other files carry FMT-RAW-010 / RH5202-008 as the `04` and `06` tables list; the spec examples are tagged.
9. Leftovers: ok. Only this run's `sdd/work/raw-string-tab-alignment/` is on the branch; the lead deletes it before publishing.
