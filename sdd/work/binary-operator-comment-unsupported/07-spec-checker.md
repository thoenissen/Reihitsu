Role: spec checker
Commit: ee62e27
Verdict: consistent

My prompt named the issue, slug, lane, and handovers. It carried no intended fix or suspected cause.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. Change doc: ok. `sdd/changes/2026-11/binary-operator-comment-unsupported.md` exists, is `Accepted`, has `Visible output changes` and no task list; maintainer answers 1-8 (A) are recorded in `Decisions`. A requirement is added (FMT-UNSUP-011) and changed, so a change doc is required.
2. Specs match the diff: ok. FMT-UNSUP-011 (format, `Why:`, examples), FMT-BREAK-017 `Why:`, RH5302-005, RH2001-007, RH7101-007, and the indentation non-goal match the production diff (precheck case, deleted join guard, deleted RH5302 guard). `AGENTS.md`'s PrecheckPhase row names the fifth shape.
3. IDs: ok. FMT-UNSUP-011 is the next free number, sorted after -010 in `requirement-ids.txt`. Nothing was reused, renumbered, or deleted; RH5302-005 was rewritten in place.
4. Code owners: ok. The architecture suite passes (below). The `Uses` lists of the touched specs carry FMT-UNSUP-011 (and -007/-008/-009 where the text relies on them). No owner type was renamed, added, or removed, so `sdd/specs/README.md` is unchanged and correct.
5. User docs: ok. The rule pages RH5302, RH2001, and RH7101 are untouched. The diff withholds fixes only, which `general-notes.md` covers, and the pages carry the footer. No user document lists the unsupported shapes.
6. Open questions: ok. The spec author's and the skeptic's output questions are answered under `Maintainer answers` in `00-lead.md` (batch 1). The RH7101 whole-type formatting and the update of #953 are under `Follow-up candidates`. Neither is written into a spec.
7. Coverage: ok. `scripts/test.sh --project architecture`: `Passed! - Failed: 0, Passed: 185, Skipped: 0, Total: 185`. FMT-UNSUP-011 has 28 added `[Requirement]` tests. `lane-check.sh behavior-change`: confirmed.
8. Tags: ok. The added tags are FMT-UNSUP-011 (28), RH5302-005 (7), RH2001-007 (2), RH7101-007 (5), and one each of FMT-UNSUP-001/-003/-005, FMT-BREAK-017, and SHARED-FIX-010. All are named in the test implementer's table. Every requirement under the spec author's `Examples for the tests` is tagged on a test the diff adds or changes.
9. Nothing left behind: ok for now. The only `sdd/work/` content is this run's directory (plus the standing `AGENTS.md`). The lead deletes it before publishing.
