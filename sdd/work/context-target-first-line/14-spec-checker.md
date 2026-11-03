Role: spec checker
Commit: 8a8adac
Verdict: consistent

Repair round 1 (`git diff 827b7ac..HEAD`, read against the full diff to `origin/main`). The repair changes production code in `LayoutComputer` (the root offset is measured without the target-position fact), one comment in `ReihitsuFormatter`, and adds ten tests. It touches no spec, ledger, or change doc.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| – | – | – | None | – |

## Checked
1. Change doc: ok. No change doc is added or needed for the repair. It restores behavior the doc already states: line 24 says the fluent-chain fixes' output does not change, and the Unchanged paragraph (line 633) lists the targets whose first token the layout leaves in place. Status is Accepted; the repair adds no visible output change.
2. Specs match the diff: ok. FMT-PIPE-017 (pipeline.md:485) defines the offset as the document column of the first token less the column the phase gives it at the depth. FMT-PIPE-017 (line 502) says a target that starts the context's first line gets the context's layout. The repaired code does both, and so does the `ShiftToRootFirstTokenColumn` summary. FMT-PIPE-024 gives the fact to lines the target lays out and does not touch the offset. No requirement contradicts the repair, and none needs a change.
3. IDs: ok. The repair adds none. `SHARED-CHAIN-009` is in the ledger (line 1379).
4. Code owners: ok. No spec header changed. `SpecConsistencyTests` passed within the architecture project.
5. User documentation: ok. No rule page is affected.
6. Open questions: ok. The repair raises none. The implementer's odd-looking raw-string probe output is identical on `main` and is not claimed as a defect.
7. Requirements covered: ok. `scripts/test.sh --configuration Debug --project architecture`: "Passed!  - Failed: 0, Passed: 92, Skipped: 0, Total: 92". The repair adds no requirement.
8. Test tags: ok. All 10 added tests (7 analyzer, 3 formatter) carry `[Requirement]` as the table in `11-test-implementer.md` lists them: FMT-PIPE-017 on all, and SHARED-CHAIN-009 additionally on the 7 analyzer tests. The table names every requirement the tests carry.
9. Nothing left behind: ok. `sdd/work/` holds only `context-target-first-line`.
