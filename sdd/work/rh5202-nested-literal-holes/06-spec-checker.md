Role: spec checker
Commit: d328e75
Verdict: consistent

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. Change doc: ok. None added and none required; RH5202-003 is clarified and RH5202-004/005 are added as clarifications of behavior SHARED-FIX-003/007 and FMT-RAW-002/009 already decide; the diff has nothing under `sdd/changes/`.
2. Specs match the diff: ok. Fix skips lines via `RawStringLineUtilities.MovesLine` (nested-text starts, empty lines), as RH5202-003/004/005 state; formatter behavior unchanged. Format (one requirement per heading, normative sentence, `Why:`, examples with `{|RH5202:…|}`) holds.
3. IDs: ok. RH5202-004 and -005 are the next free numbers, inserted in sorted position; none reused or removed.
4. Code owners: ok. The header's owners are unchanged and exist; `RawStringLineUtilities` is a new internal helper, not a spec owner. The spec list in `sdd/specs/README.md` needs no change. `Uses` gained FMT-RAW-009, SHARED-FIX-003, SHARED-FIX-007.
5. User documentation: ok. `documentation/rules/RH5202.md` is untouched; the normal-case correction does not change.
6. Open questions: ok. The spec author and test implementer list none; Q1 and Q2 are answered in `00-lead.md`.
7. Coverage: ok. `scripts/test.sh --project architecture`: Passed! Failed: 0, Passed: 115, Total: 115. RH5202-004 and -005 each have covering `[Requirement]` tests in the diff.
8. Tags: ok. All 41 added tests carry RH5202-004 or RH5202-005; every spec-author example (RH5202-004 all five, RH5202-005 both) is tagged on a test. Existing RH5202-003 tests keep their tag. Minor: the test implementer's handover table is accurate by name; its `Commit:` line cites 083eb2a (not the tests commit 5f2ff33), which does not affect the content.
9. Left behind: ok. `sdd/work/` holds only `rh5202-nested-literal-holes/`, which the lead deletes before publishing.
