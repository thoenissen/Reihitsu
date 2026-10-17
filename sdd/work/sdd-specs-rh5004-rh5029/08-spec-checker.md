Role: spec checker
Commit: 10614a4
Verdict: consistent

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| — | — | — | None. | — |

## Checked
1. Change doc exactly when required: ok. No `sdd/changes/` file in the diff. The diff adds specs for existing behavior, tags and adds tests, and clarifies two rule pages (RH5005 using declaration, RH5019 consecutive yield exemption) with behavior the code already has (tests pass). No requirement is added with new behavior, changed, or retired against shipped code; `git diff --name-status origin/main...HEAD` shows no production file under `src/` (tests only). `scripts/lane-check.sh tests`: "LANE CHECK: confirmed — claimed tests, diff needs tests".
2. Specs match the diff: ok. 16 specs, each in the analyzer-spec format (header rows, one requirement per heading, normative sentence first, `Why:`, examples with `{|RH####:…|}` markup, no open question written down). RH5005-002 and RH5006-002 are qualified by "starts with the keyword" and leave the `await` form unwritten (thoenissen/Reihitsu#946), as `05-spec-author.md` describes. RH5021/RH5029 name no deconstruction example or definition (thoenissen/Reihitsu#1001). I read every spec and example against the tests that carry it; the new tests carry the examples in their shape (`06-test-implementer.md`).
3. IDs: ok. 34 new IDs (RH5004-001 … RH5029-002), each in sorted position in `sdd/specs/requirement-ids.txt`, nothing removed or moved (the diff of the ledger is additions only). `SpecConsistencyTests` and `SpecStructureTests` pass.
4. Code owners exist: ok. Each spec names its analyzer and code-fix provider; `sdd/specs/README.md` has 16 new rows with matching owners. Covered by `SpecConsistencyTests` and `SpecStructureTests` in the architecture run below.
5. User documentation: ok. Pages RH5004–RH5009, RH5012–RH5019, RH5021, RH5029 exist (the `helpLinkUri` targets), the ID range RH5xxx matches Category Layout, each ends with the general-notes footer, none mentions the formatter or `reihitsu-format`. A scripted check on all 16 pages found `### Correction` equal to `### Violation` with exactly one blank line inserted. The two edited pages agree with their specs (RH5005: using declaration belongs to RH5029; RH5019: a yield after a yield is not reported). Pages RH5013–RH5016 do not mention the expression forms (`throw`/`switch`/`checked`/`unchecked` expressions); that is an omission the page format permits, not a mismatch.
6. Open questions recorded: ok, with one residual. `01-spec-author.md` Q1 (multi-line `case` label over-indent, SHARED-STMT-004) is filed as thoenissen/Reihitsu#1000 (`SDD open questions: sdd/specs/shared/statement-blank-lines.md`). `02-skeptic.md` deconstruction question (RH5021/RH5029) is filed as thoenissen/Reihitsu#1001 (`SDD open questions: sdd/specs/analyzer/RH5029.md`). `03-spec-author.md`, `04-skeptic.md`, `05-spec-author.md` add none. Residual: I did not open the issues on GitHub (the prompt said not to touch GitHub), so I confirmed the mapping from the handovers and the lead's prompt only, not the issue text.
7. Requirements covered and specs consistent: ok. `scripts/test.sh --project architecture`: "Passed!  - Failed:     0, Passed:    72, Skipped:     0, Total:    72, Duration: 31 s", including `EveryRequirementOfAnEnforcedSpecIsCovered`. All 34 new IDs have a covering `[Requirement]` test (`06-test-implementer.md`). The over-indent test is deliberately untagged (thoenissen/Reihitsu#1000).
8. Nothing left behind: ok. `sdd/work/` holds only `sdd-specs-rh5004-rh5029/`, this run's; the lead deletes it before publishing.

## Follow-up candidates (for the lead, not findings)
- `sdd/specs/analyzer/RH5002.md` still has a non-goal that calls the block-comment case an open question (decided as a bug, thoenissen/Reihitsu#972); the rulebook forbids naming an open question in a non-goal. Out of this run's scope (from `01-spec-author.md`).
- The object-initializer indentation paragraph on the rule pages of RH5001–RH5029 reads as implementation detail on an end-user page (from `01-spec-author.md`). Out of scope.
