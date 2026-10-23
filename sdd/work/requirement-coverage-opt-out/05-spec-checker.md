Role: spec checker
Commit: f5bf175
Verdict: consistent

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. Change doc exactly when required: ok. The run changes process (opt-out coverage, new tag duty); `sdd/changes/2026-10/requirement-coverage-opt-out.md` has `Affected requirements: none — process change`, `Depends on` an existing change doc (`introduce-sdd.md`), `Status: Accepted` (approval in `squad/decisions/2026-10-23-998-change-doc-approved.md`), no task list or plan. A change doc is right for a rulebook and role-process change; no spec requirement is touched.
2. Specs match the diff: ok. `sdd/specs/` is untouched (`git diff origin/main...HEAD -- sdd/specs` is empty). The rulebook text (*Tests and requirements*, *Checks*) matches `RequirementCoverageTests`: every spec enforced unless `ExemptedSpecs` (empty, reasoned) exempts it; the `Uses` rule for exempted `shared/` specs is kept; a reasonless exemption fails; exempted specs are reported without failing; no per-test check. No other file still describes the opt-in list (`rg` for "enforced today", "not yet enforced", `EnforcedSpecPaths`, `SpecsNotYetEnforced` finds no stale hit outside history and this run's handovers).
3. IDs: ok. No requirement added, retired, or renumbered; `requirement-ids.txt` is untouched.
4. Code owners exist: ok. No spec header or owner changed; `sdd/specs/README.md` untouched. `SpecConsistencyTests` and `SpecStructureTests` run in the architecture project below.
5. User documentation: ok. No rule page or analyzer is affected.
6. Open questions: ok. The spec author's handover files none; the implementer and test implementer list one follow-up candidate (check that every `ExemptedSpecs` key names an existing spec file), which is not a spec open question and is for the lead to mention in the run report.
7. Requirements covered and specs consistent: ok. The diff does not touch `sdd/specs/`; run anyway: `scripts/test.sh --project architecture --no-install` -> `Passed!  - Failed:     0, Passed:    83, Skipped:     0, Total:    83`, which includes the real-repository `EveryRequirementOfAnEnforcedSpecIsCovered` with the empty exemption list.
8. New tests carry their requirement's tag: ok. The test implementer's `Tests` table lists every added or changed test against "none" (no requirement); the diff adds no `[Requirement]` attribute (the one match in the diff is XML documentation text), so table and tags agree. The spec author named no requirement under `Examples for the tests`. The untagged tests verify the coverage tool, not a requirement, which the rulebook allows. Role-file text agrees with the change doc: `squad/roles/test-implementer.md` *Requirement tags*, `squad/roles/spec-checker.md` check 8 and its Inputs line, `squad/README.md` role-table row. Nothing refers to check numbers, so the renumbering of the old check 8 to 9 breaks no reference. Decisions entries `2026-10-23-998-untagged-tests-allowed.md` and `2026-10-23-998-change-doc-approved.md` follow the `squad/decisions/README.md` format (escalations 5 and 2). `squad/roles/skeptic.md` and `squad/failure-classes.md` are untouched.
9. Nothing left behind: ok for now. `sdd/work/` holds only `requirement-coverage-opt-out/`; the lead deletes it before publishing.

## Notes for the lead
- The untagged-tests decision entry names "the spec checker or the auditor"; the change doc and role files settle on the spec checker. The entry is history and is not edited.
- The test implementer's `Requirement / failure class` column carries prose and failure-class IDs next to "none"; no requirement ID appears other than none, so check 8 holds.

## Repair round (auditor 06 worklist; head e691641)
Verdict: consistent

All nine checks re-run on the diff to `origin/main`; no findings.

1. Change doc: ok. Unchanged, still `Accepted`, `none — process change`.
2. Specs match the diff: ok. `sdd/specs/` still untouched. The rewritten `sdd/README.md` *Checks* bullets match `IsEnforced`: an uncovered requirement fails when its spec is not exempted, or when it belongs to an exempted `shared/` spec that a non-exempted spec lists under `Uses`; the exempted-spec report never fails; the exemption-without-reason bullet is kept. They agree with *Tests and requirements* and the change doc. The `FindUncoveredRequirements` summary states the same condition.
3. IDs: ok. Ledger untouched.
4. Code owners: ok. No spec or owner change.
5. User documentation: ok. No rule page affected.
6. Open questions: ok. None new; auditor hints 1 and 2 are follow-up candidates for the lead, not spec questions.
7. Architecture tests: `scripts/test.sh --project architecture --no-install` -> `Passed!  - Failed:     0, Passed:    84, Skipped:     0, Total:    84` (83 plus the new directory-key test).
8. Tags on new tests: ok. The new `CoverageCheckDoesNotExemptTheSpecsOfAnExemptedDirectory` carries no `[Requirement]` and the test implementer's table lists it against "none"; the corrected FC-001 claim for `CoverageCheckSkipsOnlyTheExemptedSpec` is now accurate. The four `[Requirement(...)]` matches added in `src` are fixture strings, not attributes. `.claude/agents/spec-checker.md` description now matches the `squad/README.md` role-table row (auditor hint 3).
9. Nothing left behind: ok for now; `sdd/work/` holds only this run's directory, which the lead deletes.
