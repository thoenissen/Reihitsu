Role: spec checker
Commit: 75d9ebda (head; repair diff vs 82d818ae)
Verdict: consistent

Scope: the text repair of decision 10 (`12-spec-author.md`): `sdd/specs/formatter/pipeline.md` (FMT-PIPE-024 row and line-break bullet) and the change doc's `Affected requirements`. Nothing else changed outside `sdd/work/`.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| – | – | – | none | – |

## Checked
1. Change doc: still Accepted; it now lists FMT-PIPE-024 as clarified, no behavior change of its own. No new visible output change. ok.
2. Specs match the code: `ReihitsuFormatterHelpers.CaptureRootConstruct` sets `ElementAccessArguments` through `IsElementAccessArgumentList`, which delegates to the shared index predicate, so the fact holds for the three parents FMT-BREAK-014 names (and not for a fixed-size buffer). The new row wording ("a bracketed argument list that holds an index, as FMT-BREAK-014 defines it") and the bullet ("may join the root's list when it holds an index (FMT-BREAK-014)") say exactly that and agree with `ConditionalElementAccessArgumentListTargetGetsItsArgumentsCollapsed`. FMT-BREAK-014 is again the single statement; the phrase "holds an index" matches its definition (line-breaks.md:1009) and RH5307-001. The label "Element-access arguments" matches `RootConstruct.ElementAccessArguments`. ok.
3. IDs: none added or changed. ok.
4. Code owners: no spec header touched. ok.
5. Rule pages: none touched in the repair. ok.
6. Open questions: none. ok.
7. `scripts/test.sh --project architecture`: Passed! - Failed: 0, Passed: 198, Skipped: 0, Total: 198. No new requirement, so no new covering test is needed; the existing FMT-PIPE-024 test covers the clarified wording. ok.
8. Tags: no tests added in the repair. ok.
9. `sdd/work/` holds only this run's folder. ok (the lead deletes it before publishing).
