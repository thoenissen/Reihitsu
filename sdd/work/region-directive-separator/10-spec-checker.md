Role: spec checker
Commit: 4cbd05d
Verdict: consistent

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Inputs: `00-lead.md`, handovers 01 to 09, the change doc, and `git diff f8dc7ef...HEAD`.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|

None.

## Checked
1. Change doc: ok. `sdd/changes/2026-11/region-directive-separator.md` exists, is `Accepted`, has `Visible output changes` and no task list. Every listed visible change has its maintainer answer (1 to 7) or a lead decision derived from one (8, 9) under `Decisions`. A new requirement (FMT-REGION-007, FMT-PIPE-028) and changed ones justify it.
2. Specs match the diff: ok. FMT-REGION-001/004/007, FMT-PIPE-024/028, and FMT-TAB-002 state the changed behavior (line form, `#endregion` separator and trailing whitespace, bare keywords, node-level skip via `formatsRegionDirectives: false`). Format is followed (normative sentence, `Why:`, examples). No other spec example contradicts it; remaining multi-space `#region` examples are inputs of whole-document or analyzer specs, or node-level inputs in FMT-PIPE-028.
3. IDs: ok. FMT-PIPE-028 and FMT-REGION-007 are the next free numbers, inserted in sorted position in `requirement-ids.txt`; none reused or removed.
4. Code owners: ok. Owner types are unchanged and exist; `sdd/specs/README.md` rows match. The architecture project (SpecConsistencyTests included) passes.
5. User documentation: ok. No rule page changed, none is affected (no rule reports the separator).
6. Open questions: ok. No output question is open; all are answered under `Maintainer answers` in `00-lead.md`. Follow-up candidates 2 and 3 are listed there; candidate 1 is recorded as absorbed (decision 12).
7. Coverage: ok. `scripts/test.sh --project architecture`: `Passed!  - Failed: 0, Passed: 92, Skipped: 0, Total: 92`. FMT-REGION-004, FMT-REGION-007, and FMT-PIPE-028 each have covering `[Requirement]` tests in the diff.
8. Tags: ok. Every test method the diff adds carries the `[Requirement]` its handover row names; the existing tests newly tagged (TabReplacement, ReihitsuFormatterTests, RH7301 and RegionFormatting renames) are in the 04 and 08 tables. Every method name added appears in handovers 04, 06, or 08. Every requirement under `Examples for the tests` (FMT-REGION-004, FMT-REGION-007, FMT-PIPE-028) is tagged on a new test.
9. Left behind: ok. `sdd/work/` holds only this run's directory (the lead deletes it before publishing).
