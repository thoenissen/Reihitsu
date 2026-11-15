Role: spec checker
Commit: 1250f19
Verdict: findings

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 4, 7 | `sdd/specs/analyzer/RH3002.md` header, `Uses` row | RH3002-010 names SHARED-DIAG-008 outside its non-goals, but the row does not list it. `SpecStructureTests.UsesListsExactlyTheReferencedRequirements` fails: "analyzer/RH3002.md: SHARED-DIAG-008 is referenced but missing from Uses". | spec author |
| 2 | 4, 7 | `sdd/specs/shared/diagnostics.md` header, `Uses` row | SHARED-DIAG-008 now references RH3002-010 (a requirement of another spec), but the row does not list it. Same test: "shared/diagnostics.md: RH3002-010 is referenced but missing from Uses". | spec author |

## Checked
1. Change doc: ok. No intended behavior changes (maintainer answer B restates today's output, scope 3B is spec text only), so no change doc, and `sdd/changes/` is untouched. No visible output change is listed.
2. Specs match the diff: ok. RH3002-010 has the normative sentence, `Why:`, an example with `{|RH3002:...|}` markup. The code was not changed. Behavior matches the skeptic's runs. The RH3002-009 and SHARED-DIAG-008 clarifications state existing behavior.
3. IDs: ok. RH3002-010 is the next free number, inserted after RH3002-009 in `requirement-ids.txt`; nothing reused or removed.
4. Code owners: findings 1 and 2 (`Uses` rows). The owner types are unchanged and exist. `sdd/specs/README.md` needs no change (no spec added or removed, owners unchanged).
5. User documentation: ok. `documentation/rules/RH3002.md` is untouched; the spec author's reasoning stands (`general-notes.md` already covers excluded code).
6. Open questions: ok. No output questions open; the maintainer's answers are in `00-lead.md`; the follow-up (RH2005-005 / RH2001-007 into #1759) is under Follow-up candidates and not written into the spec.
7. Coverage and consistency: `scripts/test.sh --project architecture` -> "Failed!  - Failed: 1, Passed: 117, Skipped: 0, Total: 118" (the one failure is `UsesListsExactlyTheReferencedRequirements`, findings 1 and 2). RH3002-010 is covered by 11 `[Requirement]` tests in the diff.
8. Tags: ok. All 11 new tests carry `[Requirement("RH3002-010")]`, `VerifyFixAllLeavesParenthesesWithConditionalDirectiveReported` also RH3002-009, matching the test implementer's table. The spec author's example (the issue's input) is tagged by `VerifyConditionalDirectiveInsideReturnedParenthesesIsReportedWithoutCodeFix`.
9. Nothing left behind: ok. `sdd/work/` holds only `rh3002-conditional-directive` (the lead deletes it before publishing).
