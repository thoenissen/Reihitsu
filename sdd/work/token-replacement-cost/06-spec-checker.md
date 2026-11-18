Role: spec checker
Commit: 06b26a2
Verdict: consistent

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. Change doc: ok. Bug-fix lane, output byte-identical, no requirement added, changed, or retired; no change doc needed.
2. Specs match the diff: ok. The diff touches no file under `sdd/specs/`; no spec names `ReplaceTokens` or the replacement mechanism, so none is contradicted.
3. IDs: ok. No new IDs; `requirement-ids.txt` untouched.
4. Code owners: ok. No spec header touched; `CleanupPhase`, `IndentationRewriter`, `TabReplacementPhase` keep their names. The new internal `TokenRewriter` is no spec owner.
5. User documentation: ok. No rule page or analyzer affected.
6. Open questions: ok. No output questions in the handovers; follow-up candidates are under `Follow-up candidates` in `00-lead.md`.
7. Requirement coverage: not applicable (diff does not touch `sdd/specs/`); architecture suite was reported green by the implementer (198 passed).
8. Test tags: ok. New tests carry `[Requirement]` for the IDs their tables list (FMT-CLEAN-001/004/005/008, FMT-INDENT-002/006 to 011, FMT-RAW-001 to 003, FMT-TAB-001 to 003, FMT-PIPE-023); the two untagged ones are declared as characterization/cancellation tests verifying no requirement.
9. Nothing left behind: ok. `sdd/work/` holds only `token-replacement-cost`.
