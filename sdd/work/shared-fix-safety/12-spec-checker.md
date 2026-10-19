Role: spec checker
Commit: 26dfae9
Verdict: consistent

My prompt carried the issue, the slug, the handovers, and the decision. It carried no intended fix, suspected cause, or hoped-for verdict.

Diff checked: `git diff 4bc1822 HEAD -- . ':(exclude)sdd/work'`. It holds `sdd/specs/shared/code-fixes.md` (two lines of SHARED-FIX-003), `squad/decisions/2026-10-19-agents-sentence-and-auto-layout.md` (new), and `src/Reihitsu.Analyzer.Test/Ordering/RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests.cs`. `AGENTS.md`, `requirement-ids.txt` and `sdd/specs/README.md` are unchanged. `scripts/lane-check.sh tooling`: "LANE CHECK: confirmed — claimed tooling, diff needs tooling".

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Earlier findings
- Finding 1 (relaxed assertion in the RH7103 active-conditional test): closed. `Assert.HasCount(1, actions)` is restored, and the test now applies the single action and compares the full changed text to the move described by RH7103-006 (`Create` in front of `Run`, `#if DEBUG` pair intact). `09-test-implementer.md` explains the edit and records a watch-fail. The class passes.
- Finding 2 (`AGENTS.md` sentence versus SHARED-FIX-003): closed by the decision `2026-10-19-agents-sentence-and-auto-layout.md`, answer 1. The sentence stays as the issue words it. The gap (relocation and reordering of comments, directives, disabled text; FC-004) is a follow-up that extends SHARED-FIX-003, for the lead to draft for the maintainer's approval and to list in the PR. It is not a finding of this round.
- Nit "The second and third condition": fixed ("conditions").
- Nit "one `Why:` for four conditions": the `Why:` now argues the run-time bounds (new name or accessibility is the correction; a layout-setting field order is behavior). The single requirement stays as the issue prescribes.

## Checked
1. Change doc: ok. None is required; the repair is a wording clarification, a test assertion, and a decision record.
2. Specs match the diff: ok. SHARED-FIX-003's wording changed only; no condition, bound, or example changed. The run-time bullet and the RH7109 example agree with decision answer 2: structs are sequential unless `Auto` or `Explicit`, other types only with `[StructLayout(Sequential)]`; an Auto struct and a plain class are outside. The new `Why:` sentence is consistent with those bounds.
3. IDs: ok. No ID added, removed, or moved; `requirement-ids.txt` is unchanged.
4. Code owners: ok. Header unchanged; `SpecConsistencyTests` passes inside the architecture run.
5. User documentation: ok. No file under `documentation/rules/` is affected or in the diff.
6. Open questions: ok. The RH7302 `#endregion` question is Q2 on thoenissen/Reihitsu#1110 (`SDD open questions: sdd/specs/shared/code-fixes.md`, open; same inputs and readings (a), (b), (c) as before). thoenissen/Reihitsu#1126 is closed as a duplicate (`state_reason: duplicate`). No file outside `sdd/work/` mentions #1126. `10-spec-author.md` line 35 still names #1126; it is an earlier statement in a work file the lead deletes before publishing, so the PR should cite #1110 Q2. The `AGENTS.md` sentence gap and the other follow-up candidates from `08-spec-checker.md` and `05-spec-author.md` (stale non-goals in `RH2005.md` and `RH7103.md`, the RH7103 region follow-up, the RH7109 layout follow-up) are for the lead to list in the PR.
7. Coverage: ok. `scripts/test.sh --project architecture`: `Passed!  - Failed:     0, Passed:    72, Skipped:     0, Total:    72`. `scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Ordering.RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests|TestCategory=SHARED-FIX-003"`: `Passed!  - Failed:     0, Passed:    48, Skipped:     0, Total:    48`. No new requirement was added, so no new covering test is due.
8. Nothing left behind: ok. `sdd/work/` holds only `shared-fix-safety`, which the lead deletes before publishing.
