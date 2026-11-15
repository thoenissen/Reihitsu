Role: spec checker
Commit: 49de8bc
Verdict: consistent

Repair round 1. The prompt carried no intended fix, suspected cause, or hoped-for verdict. Diff checked: `origin/main` (4838e9a, unchanged and the merge base) to head, with the repair delta `31f19c0..49de8bc` read in full.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. Change doc: ok. `sdd/changes/2026-11/fix-layout-scope.md` stays `Accepted`; `Affected requirements` gains RH5113-004 and the new *Decisions* subsection "RH5113 keeps its withholding" records the D26 decision. No visible output change was added (D25 applies unchanged SHARED-FIX-007; D26 keeps `main`).
2. Specs match the diff: ok. RH5113-004 (normative sentence, `Why:`, three examples with `{|RH5113:;|}` markup) matches the code: the fix stays withheld on a syntax error and for a declaration `FindUnsupportedSyntax` refuses; the code comments and summaries name RH5113-004. RH5113 `Uses` lists FMT-PIPE-005, FMT-UNSUP-001, -004, -007 as the spec references them. D25 needs no requirement text (SHARED-FIX-007 already demands the outputs).
3. IDs: ok. RH5113-004 is the next free number and sits at its sorted position in `requirement-ids.txt`; nothing reused, renumbered, or deleted.
4. Code owners: ok. No owner renamed, added, or removed in the repair round; `SpecConsistencyTests` is part of the architecture run below.
5. User documentation: ok. No rule page changed; `RH5113.md` needs no note (withheld fix in a rare input is covered by `general-notes.md`).
6. Open questions routed: ok. 12-spec-author lists none; FU7 (offer the RH5113 fix there) and the FU5 addendum sit under `Follow-up candidates` in `00-lead.md`.
7. Coverage: ok. `scripts/test.sh --project architecture` result line: `Passed!  - Failed: 0, Passed: 118, Skipped: 0, Total: 118`. RH5113-004 is covered by three tagged tests.
8. Test tags: ok. All 10 new D25 test methods (RH2001 x2, RH5110 x2, RH5111 x2, RH5412 x2, RH5413 x2) carry `[Requirement("SHARED-FIX-007")]`; the retagged syntax-error test and the two new RH5113 spec-example tests carry `[Requirement("RH5113-004")]`; 13-test-implementer's table names each.
9. Nothing left behind: ok. `sdd/work/` holds only `fix-layout-scope` (the lead deletes it before publishing).
