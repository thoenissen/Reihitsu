Role: spec checker
Commit: c645d04
Verdict: consistent

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|

_None._

## Checked
1. Change doc: ok. `sdd/changes/2026-11/rh5115-rh5116-index-and-null-forgiving-line.md` exists for the new rules, is `Accepted`, has `Visible output changes` and no task list, and its `Decisions` records answers 1A to 5A.
2. Specs match the diff: ok. RH5115 and RH5116 each have six requirements. The FMT-BREAK-040 Why and the fluent-chain-layout introduction and non-goals are reworded to name the new rules; no requirement contradicts the diff.
3. IDs: ok. RH5115-001..006 and RH5116-001..006 are inserted in sorted position after RH5114-003; nothing is reused or removed.
4. Code owners: ok. `sdd/specs/README.md` lists both specs with matching owners. The moved `AttachedPartGapUtilities` still exists and stays listed as a FMT-BREAK owner. `scripts/test.sh --project architecture` passed (Passed 198, Failed 0, Skipped 0, Total 198).
5. User documentation: ok. RH5115.md and RH5116.md follow the rule-page format, with no formatter mention and the general-notes footer. `RuleDocumentationExampleTests` passed, so the Correction equals the fix output. The RH5116 sentence about `!.` and `!?.` states the rule's scope, not a withheld fix (decision 3 in `00-lead.md`). The ID fits the Layout range, and the Release.ruleset, package README and sln rows are present.
6. Open questions: ok. Q1 to Q5 are answered in `00-lead.md`. The query-clause layout question and the thoenissen/Reihitsu#1827 comment sit under Follow-up candidates, and none is written into a spec.
7. Coverage: ok. The architecture result is quoted under check 4. Every new requirement has a tagged test (`EveryRequirementOfAnEnforcedSpecIsCovered` passes). The analyzer filter run over RH5115, RH5116, RuleDocumentation and AnalyzerPackageMetadata: Passed 125, Failed 0, Skipped 0, Total 125.
8. Test tags: ok. Every ID RH5115-001..006 and RH5116-001..006 is tagged on at least one test, and each ID the spec author named under `Examples for the tests` is covered.
9. Nothing left behind: ok. Only `sdd/work/attached-part-line-break/` is tracked under `sdd/work/`; the lead deletes it before publishing.
