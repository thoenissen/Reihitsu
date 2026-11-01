Role: spec checker
Commit: 629e656
Verdict: consistent

Scope: repair round 1, diff `ba3cf9d..629e656`. It touches only test files (Core tests added; formatter and analyzer tests removed), the removal of one redundant `using` in `UsingLeadingTriviaBuilder.cs`, and `sdd/work/`. `sdd/specs/`, `sdd/changes/`, `documentation/` and `requirement-ids.txt` are unchanged. The prompt carried no intended fix or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|

_None._

## Checked
1. Change doc: ok. Unchanged since `5b0404c`; Accepted, and its Decisions cover maintainer answers 1-7. Answer 8 withdraws changes to fix output, so no visible output change is added or dropped, and no change-doc entry names the spanning-comment shape (grep of specs and change doc finds none).
2. Specs match the diff: ok. No spec text describes the withdrawn spanning-trailing-comment behavior, so removing its tests leaves no contradiction. The production diff is a using removal only.
3. IDs: ok. Ledger untouched.
4. Code owners: ok. No owner touched.
5. User documentation: ok. No rule page affected.
6. Open questions: ok. Maintainer answer 8 is recorded in `00-lead.md`; decisions 4-6 are marked withdrawn and 7-8 recorded; follow-up candidates: none, as the maintainer asked.
7. Coverage: ok. `scripts/test.sh --project architecture`: `Passed!  - Failed: 0, Passed: 92, Skipped: 0, Total: 92`. SHARED-USING-008 and SHARED-USING-011 now also have direct Core `[Requirement]` tests; FMT-USING-010 and the RH7207 requirements keep their covering tests (the removals were of tests for the withdrawn shape only).
8. Test tags: ok. The 17 tagged Core tests carry SHARED-USING-008 or SHARED-USING-011, matching `10-test-implementer.md`. Two Core tests (`HasBlankLineAbove...BehindAPredecessorWhoseLineDoesNotEnd`) are untagged mechanism tests of the pipeline-internal predecessor shape; allowed by `sdd/README.md` (Tests and requirements), not a finding.
9. Nothing left behind: ok. `sdd/work/` holds only `using-comment-blank-line/`, which the lead deletes before publishing.

Note, not a finding: the kept tests with a spanning trailing comment (members b and e, the FMT-USING-005 pin) were removed in `8909de6`, so the handover `11-test-implementer.md` table is stale for them; `13-test-implementer.md` is current.
