Role: spec checker
Commit: cf4a346
Verdict: consistent

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. Change doc: ok. No change doc is required; FMT-PIPE-027 writes down behavior that already exists (empty exception list on `main`, suite green), and the diff touches only a spec and test-project files. `sdd/changes/2026-11/` holds no file of this run.
2. Specs match the diff: ok. FMT-PIPE-027 has one heading, the normative sentence first, `Why:`, two examples; FMT-PIPE-004 gains one pointer sentence and keeps its rule and `Why:`. Neither contradicts the diff (the test project checks the production phases alone, under LF and CRLF, on the output text). `Uses` gains FMT-USING-010, which the example cites.
3. IDs: ok. FMT-PIPE-027 is the next free number and sits in sorted position after FMT-PIPE-026; no ID reused, renumbered, or deleted.
4. Code owners: ok. The pipeline spec header owners are unchanged and `sdd/specs/README.md` matches them.
5. User documentation: ok. No rule page or user document is affected; no remaining reference to `PhaseReversal*`, `DataRowKey`, or `PhaseStabilityRecorder` outside `sdd/work/`.
6. Open questions: ok. The spec author's and skeptic's handovers carry no output question; follow-up candidates are under `Follow-up candidates` in `00-lead.md`.
7. Coverage: ok. `scripts/test.sh --project architecture`: Passed! Failed: 0, Passed: 92, Skipped: 0, Total: 92. FMT-PIPE-027 is covered by tagged tests in `PhaseStabilityCheckTests`.
8. Tags: ok. `[Requirement("FMT-PIPE-027")]` sits on `CheckRunsTheProductionPhasesInPipelineOrder`, `CheckFindsNoChangeInFormattedSource`, `CheckFindsNoChangeAboveCommentLedUsingDirectiveOfTheSameGroup` (also FMT-USING-010), `CheckFindsNoChangeInArgumentAlignedUnderTheFirstArgument`, and `CheckFindsNoChangeInTheOutputOfAnInputTheFormatterFormats`, matching the test implementer's table. Example 1, example 2, and the formatted case of the first qualifier are each tagged on a test.
9. Nothing left behind: ok. `sdd/work/` holds only `per-phase-idempotency`, which the lead deletes before publishing.
