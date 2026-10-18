Role: spec checker
Commit: b3d4590
Verdict: findings

Checked the diff c1483939e2a6fde3da5b02653c9da62ece924138..b3d4590 (lane: specs and tests for existing behavior; `scripts/lane-check.sh tests` confirms the lane). Inputs read: `00-lead.md` through `05-cleaner.md`, the shared spec, the 15 analyzer specs, the 13 affected rule pages, and thoenissen/Reihitsu#1029, #1030, #1031. The prompt carried no intended fix or hoped-for verdict. Both findings are minor, in user documentation only; they do not affect the specs or tests.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 5 | `documentation/rules/RH5530.md` (Description, first paragraph) | The page names only `get`, `set` and `init` accessors and a "property". RH5530-001 also checks `add` and `remove` accessors of events (its example reports `{|RH5530:[A]|} add`) and accessors of indexers. A user of an event gets a diagnostic the page does not describe. | spec author |
| 2 | 5 | `documentation/rules/RH5527.md` (no Exceptions section) | RH5527-001 and SHARED-ATTR-006 specify that `[return: A]` on an accessor of an inline auto-property such as `int P { [return: A] get; set; }` is never reported. The page, unlike `documentation/rules/RH5530.md`, states no exception, so it reads as if every `return:` list is reported. Evidence: the SHARED-ATTR-006 example and the tests tagged with it. | spec author |

## Checked
1. Change doc exactly when required: ok. The diff states existing behavior only (new specs and tests, no production code, no requirement changed or retired), so a clarification lane needs none; none was added.
2. Specs match the diff: ok. All 15 analyzer specs and the shared spec follow the format; a script confirmed every requirement heading has a normative MUST/SHOULD first sentence and a `Why:`. Examples use `{|RH####:…|}` markup. Correctness against the code is the skeptic's job and was done in `02-skeptic.md` and `03-spec-author.md`.
3. IDs: ok. 74 new IDs (RH55xx-00n and SHARED-ATTR-001 to -012) are at sorted positions in `sdd/specs/requirement-ids.txt`; none removed or reused.
4. Code owners: ok. `sdd/specs/README.md` has 16 new rows with matching owners; no type is an owner twice.
5. User documentation: findings 1 and 2. No rule page changed in the diff. The other 11 pages match their specs. Their examples and Corrections were not re-run, since they are not in the diff.
6. Open questions: ok. All six questions of `03-spec-author.md` are filed and match their handover text: Q1 in thoenissen/Reihitsu#1029 (RH5530 spec), Q2 to Q5 as Q1 to Q4 in #1030 (shared spec), Q6 as Q1 in #1031 (formatter line breaks). All three are sub-issues of #912. No spec mentions an open question or an issue number.
7. Coverage and consistency: ok. `scripts/test.sh --project architecture` -> "Passed!  - Failed:     0, Passed:    72, Skipped:     0, Total:    72". Every active requirement has a covering `[Requirement]` test (`RequirementCoverageTests` passes). `scripts/test.sh --project analyzer --filter "FullyQualifiedName~MustFollowPlacementRulesAnalyzerTests"` -> "Passed!  - Failed:     0, Passed:   172, Skipped:     0, Total:   172". The diff outside `sdd/` is only the 15 test files.
8. Nothing left behind: ok for now. `sdd/work/` holds only `attribute-placement-specs/`, which the lead deletes before publishing. `origin/main` has not moved since the base, so there is no merge with the parallel run on thoenissen/Reihitsu#979 yet.
