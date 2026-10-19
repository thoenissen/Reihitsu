Role: spec checker
Commit: 4de5d4e
Verdict: consistent

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Inputs: `00-lead.md` to `05-cleaner.md` (`03-spec-author.md` over `01`) and `git diff origin/main`. The lane is tooling (widened from tests by the `squad/` decision record); `scripts/lane-check.sh tooling` reports `LANE CHECK: confirmed — claimed tooling, diff needs tooling`.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. Change doc: ok. None is required and none exists. Every requirement states existing behavior, no production file changed, and no requirement was added with new behavior, changed, or retired. `sdd/changes/` is untouched.
2. Specs match the diff: ok. The 14 analyzer specs and SHARED-DOC-014 to SHARED-DOC-016 follow the format: 37 requirement headings, each with a `Why:` and an example, a normative sentence first, and a "This requirement covers" paragraph on the 14 requirements that state coverage. The tests extract the spec examples verbatim and pass, so the markup matches the behavior. The rule-spec headers (ID prefix, category, severity, enabled by default) match `DiagnosticCategory.Documentation`; the titles match the resource strings for RH8101, RH8102, RH8108 to RH8111. Every ID in an analyzer spec's `Uses` exists; the body references of each spec are listed in its `Uses`, except SHARED-DOC-005 in RH8203's Non-goals, which is a pointer and not a dependency.
3. IDs: ok. SHARED-DOC-014 to 016 follow SHARED-DOC-013; the 37 new IDs sit at their sorted positions in `sdd/specs/requirement-ids.txt` (`sort -c` is silent); the diff only adds lines, so nothing is reused, renumbered, or deleted. RH8107-006 and RH8107-007 stay where they are and are referenced under RH8202's `Uses`, as `squad/decisions/2026-10-19-rh8202-uses-rh8107-fix-requirements.md` decides.
4. Code owners: ok. `scripts/test.sh --no-install --project architecture` passes, SpecConsistencyTests included. `sdd/specs/README.md` gains 14 rows whose owners equal the spec headers. `XmlDocumentationElementOrderingUtilities` is claimed by no spec, as the lead decided.
5. User documentation: ok. Only `RH8201.md` and `RH8202.md` changed; the other twelve pages exist (including RH8110 and RH8111) and need no change.
   - `RH8201.md` gains an *Exceptions* bullet equal to RH8201-003 (`#if` in a file of the type, a partial part, or an interface; overrides and explicit implementations always reported).
   - `RH8202.md` now says properties and indexers and that a `<value>` in `<code>`/`<example>` is not reported, equal to RH8202-001. It narrows what the page promised on `main`; the PR description should say so (03-spec-author.md, skeptic B5).
   - No page mentions the formatter, repeats `general-notes.md`, or lacks the footer. `RuleDocumentationExampleTests`, which compares each `### Correction` with the code fix's output: 2 of 2 pass.
6. Open questions: ok, to be filed by the lead. None is recorded in a spec or only in the PR; all are listed in `03-spec-author.md` under *Open questions to file*: S1 to S4 for `shared/element-documentation.md`; Q1 and Q2 for RH8106; Q1 for RH8201; Q1 to Q3 for RH8202; Q1 and Q2 for RH8203; Q1 to Q3 for RH8204; plus the #1153 and #1091 instance comments. The skeptic's A2 (two top-level `<inheritdoc>`) and C7 (`name="1x"`) are excluded by coverage and are not in that list; the author's handover names C7 under the #1153 comment, and A2 is stated by the code and not filed.
7. Requirements covered: ok. `scripts/test.sh --no-install --project architecture`: `Passed!  - Failed:     0, Passed:    72, Skipped:     0, Total:    72`, with `RequirementCoverageTests.EveryRequirementOfAnEnforcedSpecIsCovered` among them, so every new active requirement has a tagged test.
8. Nothing left behind: ok for now. `git ls-files sdd/work` shows only `sdd/work/parameter-return-inheritdoc-docs/`; the lead deletes it before publishing.
