Role: spec checker
Commit: 2876d04
Verdict: consistent

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
_None._

## Checked
1. ok - no change doc: output unchanged from main (maintainer answer A), clarification only; no behavior added, changed or retired.
2. ok - FMT-STRUCT-021 clarification states today's output; Why present, example added with the normative sentence first; referenced IDs (FMT-STRUCT-022, FMT-UNSUP-004/010, FMT-PIPE-029) exist.
3. ok - no new ID; `requirement-ids.txt` untouched.
4. ok - no owner changed; architecture project run covers SpecConsistencyTests.
5. ok - no rule page affected (RH7101 page unchanged, user-visible behavior unchanged).
6. ok - the one output question has answer A under Maintainer answers; follow-ups are listed in 00-lead.md; nothing written into the spec as an open question.
7. ok - `scripts/test.sh --project architecture`: Passed! - Failed: 0, Passed: 180, Skipped: 0, Total: 180. FMT-STRUCT-021 is covered by `[Requirement]` tests in the diff (no new requirement).
8. ok - every test in the diff's Tests table carries `[Requirement("FMT-STRUCT-021")]` (fix tests also RH7101-004); the spec example is tagged on the verbatim test; the untagged characterization test verifies no requirement.
9. ok - only `sdd/work/field-split-type-break/` exists (the lead deletes it before publishing).
