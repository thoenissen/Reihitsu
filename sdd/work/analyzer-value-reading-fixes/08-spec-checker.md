Role: spec checker
Commit: 581d340
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Findings

### thoenissen/Reihitsu#1071
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 7 | `sdd/specs/analyzer/RH4001.md:12`, `sdd/specs/analyzer/RH4009.md:12` | `Uses` is `—`, but the amended specs now reference SHARED-DIAG-002 (RH4001-002, RH4009-002) and RH0001-003 (RH4009-001). `scripts/test.sh --project architecture` fails `SpecStructureTests.UsesListsExactlyTheReferencedRequirements`: "analyzer/RH4001.md: SHARED-DIAG-002 is referenced but missing from Uses", "analyzer/RH4009.md: RH0001-003 ... missing", "analyzer/RH4009.md: SHARED-DIAG-002 ... missing". Set the rows to list those requirements. | spec author |

### thoenissen/Reihitsu#1268
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 2 | 7 | `sdd/specs/analyzer/RH2003.md:12` | `Uses` is `—`, but RH2003-001 now references SHARED-DIAG-002 (same architecture failure). Add SHARED-DIAG-002. | spec author |

### thoenissen/Reihitsu#1289
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 3 | 7 | `sdd/specs/analyzer/RH2109.md:12` | `Uses` is `—`, but RH2109-001 now references SHARED-DIAG-003 (same architecture failure). Add SHARED-DIAG-003. | spec author |
| 4 | 2 | `sdd/specs/analyzer/RH2109.md` RH2109-001 vs `RH2109RazorCodeBlocksShouldNotBeUsedAnalyzer.cs` | Known and accepted, not a defect of this diff: the spec (via SHARED-DIAG-003) is wider than the narrowed implementation (lead decisions 7 and 8); the remaining hosts are under Follow-up candidates in `00-lead.md`. Listed so the auditor sees it; no action. | none |

### thoenissen/Reihitsu#974
No finding. RH0001-003 text and example match the diff; RH4009-001 matches the RH4009 and RH8402 tests; no CLI change (decision 2).

## Checked
1. Change doc: ok. Bug-fix lane, only clarifications of existing requirements, no requirement added, retired, or changed in intent, no rule/phase/surface added or removed; no change doc in the diff, none required. The maintainer's visible-output answers Q1-Q3 are recorded in `00-lead.md`.
2. Specs match the diff: ok apart from finding 4 (accepted). Requirement format kept (normative sentence, Why, examples, `{|RH####:…|}` markup). The RH4009-002 wording of the `A.B` example matches the test.
3. IDs: ok. No new IDs; `sdd/specs/requirement-ids.txt` is not in the diff; nothing renumbered or deleted.
4. Code owners: ok. No type was added, renamed, or removed (diff adds no type; implementer reused existing helpers); `SpecConsistencyTests` pass; `sdd/specs/README.md` unchanged and still correct.
5. User documentation: ok. No rule page under `documentation/rules/` is in the diff; the normal case of every rule is unchanged, so no page or `### Correction` is affected. No diagnostic ID or help link changed.
6. Open questions: ok. The only output questions (spec author 01) are answered under `Maintainer answers` (Q1-Q3); spec author 03 and the skeptic raised none; follow-up candidates outside the issues are under `Follow-up candidates`; none is written into a spec.
7. Coverage and consistency: findings 1-3. `scripts/test.sh --project architecture` result line: `Failed!  - Failed:     1, Passed:   116, Skipped:     0, Total:   117` (the one failure is `UsesListsExactlyTheReferencedRequirements`, findings 1-3). Every requirement the spec author named under `Examples for the tests` (RH4001-002/-003/-005/-006, RH4009-002, RH2003-001/-003, RH2109-001, SHARED-DIAG-003, RH0001-003) has tagged new tests in the diff.
8. Requirement tags: ok. All 71 test methods added or reworked in `src/Reihitsu.Analyzer.Test/` carry `[Requirement]` (no untagged added test found); every requirement they carry (including RH2109-003, RH0001-002/-004, RH8402-001, RH4001-004, RH2003-002/-004, RH4009-001) appears in the `Tests` tables of `04-test-implementer.md`; the repaired test of `06-test-implementer.md` keeps its tags.
9. Nothing left behind: ok. `sdd/work/` holds only this run's directory (the lead deletes it before publishing).
