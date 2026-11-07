Role: spec checker
Commit: 6801683
Verdict: findings

My prompt carried no intended fix or hoped-for verdict. I worked from `00-lead.md`, handovers 01 to 09, and `git diff origin/main` at 6801683.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 1 | `sdd/changes/2026-11/raw-string-tab-one-column.md` (header, What, Visible output changes) | The doc lists only the tab as a visible change of the raw-string measure, and its What says "the tab now does too", as if the other whitespace characters (no-break space, em space, vertical tab, form feed) were already counted. 01-spec-author (#1611) measured main differently: with four U+00A0 in front of the closing marker the formatter counted none of them (closing quotes land in column 20, not 16). The diff changes FMT-RAW-001 and FMT-RAW-002 for all of them (a leftward shift now removes any leading whitespace character, not only spaces). FMT-RAW-002 is changed but missing from `Affected requirements`. If the NBSP behavior is a bug fix against RH5202-003 and SHARED-FIX-007 (lead's lane reading), the doc should say so and not claim the tab was the only character left out; if it is a visible change, it needs its own entry under `Visible output changes` and a recorded decision. Doc and spec disagree about main's behavior either way. | spec author |

## Checked
1. Change doc: required (extending the tab non-goal, `sdd/README.md` Spec format) and present; Status Accepted; has `Visible output changes`; Q1 and Q2 each have a `Decisions` entry matching `00-lead.md` Maintainer answers; no task list. Finding 1 (completeness of Affected requirements and the NBSP statement). No change doc is needed for the clarifications (#1243, #1383, #1149, #952, #1391 parts).
2. Specs match the diff: ok. Changed requirements (FMT-PIPE-014, FMT-BREAK-011, -012, -013, -015, -016, FMT-INDENT-006, -013, FMT-RAW-001, -002, RH5111-001, -004, non-goals of FMT-RAW and FMT-TAB) carry normative sentence, `Why:`, and examples; decision 5 (FMT-BREAK-015 owns the parameter default gap, -016 defers) is stated in both; the enum example shows no trailing comma (decision 10) in both the spec and the change doc. Analyzer markup `{|RH5111:…|}` used.
3. IDs: ok. The diff adds no requirement, so `requirement-ids.txt` is untouched and no ID is reused or removed.
4. Code owners: ok. `RawStringLiteralUtilities`, `RawStringLineShifts`, `LineBreakTriviaUtilities`, `BaseTypeListContributor`, `LineBreakAssignmentRewriter`, `LineBreakListRewriter`, and the other touched types are listed in their spec headers and in `sdd/specs/README.md`; the new members are private or on existing owners. `Uses` rows gained FMT-PIPE-014 and FMT-TAB-003 consistently.
5. User documentation: ok. The diff changes no file under `documentation/`; RH5111's rule page needs no change (the analyzer already reported the enum member; the spec only names it). RH5202 and its fix output are unchanged.
6. Open questions: ok. Q1 and Q2 are answered in `00-lead.md`; the spec author's, skeptic's, and test implementer's candidates are in `Follow-up candidates` (1 to 13), and none is written into a spec.
7. Coverage: ok. No new active requirement, so no new `[Requirement]` test is owed. `scripts/test.sh --project architecture`: "Passed! - Failed: 0, Passed: 115, Skipped: 0, Total: 115" (includes SpecConsistencyTests).
8. Tags: ok. Every test in the `Tests` tables of 04 and 07 carries the listed requirement; every requirement tagged in the diff (22 IDs, including FMT-RAW-003, FMT-INDENT-015, -030, FMT-BREAK-017, -018) appears in the tables; the requirements named under `Examples for the tests` in 01, 03, and 05 (FMT-BREAK-011, -012, -015, -016, FMT-INDENT-006, -013, FMT-RAW-001, -002, RH5111-001, -004) are each tagged on at least one test the diff adds or newly tags.
9. Nothing left behind: `sdd/work/formatter-layout-fixes/` is the only `sdd/work/` on the branch; the lead deletes it before publishing.
