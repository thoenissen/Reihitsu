Role: spec checker
Commit: 9e0e64799468260a87787ba27c4ca79f10248f52
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Inputs: issue thoenissen/Reihitsu#1138 (body and its one comment), thoenissen/Reihitsu#1363, handovers 00 to 05, and `git diff origin/main...HEAD -- . ':(exclude)sdd/work'`.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 2 (low, outside the lead's stated scope) | `sdd/specs/analyzer/RH7103.md`, non-goal "How much of the type the fix reformats" | The non-goal still says that limiting the fix's reformat to the moved member "is an open question". SHARED-FIX-006 (Formatting bullet) now decides it: a fix formats only the node it replaces and never the type that contains it. A spec must not carry an open question (`sdd/README.md`, *Open questions*), and this one is now answered. The skeptic listed it as a follow-up candidate (`02-skeptic.md`); the lead's scope said "nothing else", so this is the lead's call: reword it in this PR, or leave it to the RH7103 bug issue. The code still formats the whole type, which is a bug against SHARED-FIX-006, not a spec problem. | spec author (optional) |

No other finding.

## Checked
1. Change doc exactly when required: ok. None added. The issue and the lead's lane decision say the requirements write down decided policy (thoenissen/Reihitsu#912, #969, #1094, #1096, #1062); none is a new rule, phase or surface. No `sdd/changes/` file in the diff.
2. Specs match the diff: ok, with finding 1 as the only stale text.
   - Decided texts against the issue body and its comment: SHARED-FIX-006 (P9, P14a, P14d), SHARED-FIX-007 (P10, P14b, P14c), SHARED-FIX-008 (P24), the SHARED-FIX-002 clarification with the RH7103 example, the P23 sentence in SHARED-FIX-003, the replaced RH3202 layout example ("two blank lines above a get-only method stay two"), and the FMT-PIPE-010 `Why:` all match. P26 and the RH7501 exception are not written, as scoped.
   - Additions beyond the decided text, all named in the spec author's handovers and none contradicting a decision: the "unless those blank lines are the code its diagnostic is about" clause in the P14a bullet; the "single blank line stays" and mirror-shape ("other member comes first") sentences; the literal and comment-text exemption in SHARED-FIX-008, which cites FMT-PIPE-007 and SHARED-FIX-003; the SHARED-FIX-007 Blank lines bullet (the precedence of P14a over P10, from the decisions on thoenissen/Reihitsu#1209, #1220, #1263). The maintainer may want to see these when reviewing the PR.
   - Format: one requirement per heading, normative sentence first, `Why:` present, examples with `{|RH####:…|}` markup. The `{|RH5417:{|}` form is the one `analyzer/RH5417.md` uses.
   - The diff changes no production code, so no requirement contradicts it. Existing tests that pin behavior the new requirements forbid are left unchanged on purpose, as the issue says (`RH3202…VerifyFixChangesOnlyTheLayoutOfDocumentationCommentsAndRawStringLiterals`, `RH3202…VerifyFixCollapsesTwoBlankLinesAboveTheMethod`, RH5104-005's tests, and the others `02-skeptic.md` lists). The bug issues own them.
3. IDs: ok. SHARED-FIX-006, -007 and -008 are the next free numbers and sit after SHARED-FIX-005, before SHARED-GAP-001, in `requirement-ids.txt`; the diff adds three lines and removes none. No ID is reused, renumbered or retired.
4. Code owners exist: ok. `shared/code-fixes.md` keeps `—` as owners; `Uses` gains FMT-PIPE-007 and `formatter/pipeline.md` `Uses` gains SHARED-FIX-006, each referenced outside non-goals. `sdd/specs/README.md` needs no change (no prefix or owner changed). `SpecConsistencyTests` and `SpecStructureTests` pass.
5. User documentation: ok. No file under `documentation/rules/` is in the diff, and none needs to be: no rule's behavior, message, or fix output changed, and the examples in the new requirements restate existing rule-page corrections only where tests already pin them.
6. Open questions recorded: ok. Thoenissen/Reihitsu#1363 (`SDD open questions: sdd/specs/shared/code-fixes.md`) holds the spec author's P14b question (Q2), the skeptic's RH5104 comment question (Q1), the type's closing brace question (Q3), the disabled-text question (Q4, from `03-spec-author.md` Decisions needed), and two P24 bug candidates (Q5, Q6, where Q6 is the skeptic's lone-CR question). None is written into the spec.
7. Requirements covered, specs consistent: ok.
   - `scripts/test.sh --project architecture`: `Passed!  - Failed: 0, Passed: 72, Skipped: 0, Total: 72`. This includes `EveryRequirementOfAnEnforcedSpecIsCovered`.
   - `scripts/test.sh --project analyzer --filter "TestCategory=SHARED-FIX-002|TestCategory=SHARED-FIX-006|TestCategory=SHARED-FIX-007|TestCategory=SHARED-FIX-008"`: `Passed!  - Failed: 0, Passed: 244, Skipped: 0, Total: 244`.
   - Each new or changed requirement has a covering `[Requirement]` test in the diff: SHARED-FIX-002 (RH7103 Fix All), -006 (RH6024, RH4106, RH3202 tag, RH5002 tag), -007 (RH7303, RH7310 between members and tag, RH5417 own line), -008 (RH5405 CRLF and LF, literal and comment line-break tests, RH7303/RH7310 CRLF tags). Every test passes on unchanged production code, and the test implementer's handover records mutation evidence that the CRLF and Fix All assertions can fail.
   - Coverage notes, not findings: the "shares its line" side of SHARED-FIX-007 and the removed-directive cases at the start and end of a body have no conforming shape today (thoenissen/Reihitsu#1144, #1263), so only the own-line, between-members and between-statements shapes are tagged. The RH6024 lambda, RH3203 and RH5101 examples in the spec are likewise untagged (thoenissen/Reihitsu#1141, #1284, #1144). The requirements as a whole are covered.
   - Test infrastructure added (`AssertFormattingChangesNothing` in `AnalyzerTestsBase<TAnalyzer, TCodeFix>`) is test-only.
8. Nothing left behind: ok for now. `sdd/work/` holds only `fix-scope-and-layout/`; the lead deletes it before publishing. `scripts/lane-check.sh tests`: `LANE CHECK: confirmed — claimed tests, diff needs tests`.
