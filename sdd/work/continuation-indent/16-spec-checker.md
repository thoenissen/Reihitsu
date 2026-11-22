Role: spec checker
Commit: d0dd9fb
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Scope: all four issues share one diff, one set of specs, and five change docs, so findings are listed once, with the issues they touch named.

Architecture run: `scripts/test.sh --project architecture` -> `Passed!  - Failed: 0, Passed: 198, Skipped: 0, Total: 198` (includes SpecConsistencyTests).

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 1 | `sdd/changes/2026-11/continuation-indent.md` and `continuation-join.md`, *Affected requirements* | The specs change FMT-BLANK-018 (two examples move to the continuation column) and RH2001-004 (one sentence now says RH2001-009 covers "a comment or directive" between modifiers or before the type), but no change doc lists either. Minor: both edits only follow FMT-INDENT-034 and RH2001-009, which are listed. | spec author |
| 2 | 8 | `UnsupportedExpressionBodyArrowLeadingCommentTests.CommentInFrontOfTheArrowOfASwitchArmGivesTheReasonOfTheArm` | The test carries `[Requirement("FMT-UNSUP-013")]` (new in the diff), but no `Tests` table of 07, 09, 12, or 14 names FMT-UNSUP-013 against it. Minor: either add the requirement to the table row or drop the tag (the test verifies FMT-UNSUP-013's "other side", so keeping the tag is fine). | test implementer |

## Checked
1. Change doc exactly when required: finding 1 (minor). Five change docs, all `Accepted`: `continuation-indent`, `continuation-join`, `expression-body-join-across-blank-line`, `omitted-type-argument-comment-unsupported`, `assignment-comment-in-arm-unsupported`. Each has *Visible output changes* and *Decisions* naming the maintainer's answer (batch 1b/1c/2, answers 2A, 3A, 4b A, 6A, 8) or the standing instruction (lead rows 5-8, 11, 12, 14, 18, 20). The new-requirement set (FMT-BREAK-041, FMT-INDENT-034/035, FMT-UNSUP-017/018/019) and the retired exception (FMT-INDENT-015) are all covered. FMT-UNSUP-019 deletes no special case; its *Special cases* section says so and the declaration rests on the maintainer's explicit answer 8 (lead row 9), not a finding.
2. Specs match the diff: ok. Every new requirement has one heading, the normative sentence first, `Why:`, and examples; the changed ones (FMT-BREAK-015/016/017/022/031, FMT-INDENT-002/013/015/026, FMT-SPACE-001/010, FMT-STRUCT-006/007/021, FMT-PIPE-017/024, FMT-UNSUP-009..013, RH2001/5111/5401/5419/7101, SHARED-*) are all in a change doc except as noted in 1. Tests from 12 (formatter 7086/0, analyzer 11912/0) agree with the specs.
3. IDs: ok. FMT-BREAK-041, FMT-INDENT-034/035, FMT-UNSUP-017/018/019 are the next free numbers, inserted in sorted order in `requirement-ids.txt` (file sorted); nothing reused, renumbered, or deleted.
4. Code owners: ok. SpecConsistencyTests pass; `sdd/specs/README.md` lists the new owners (`ContinuationJoinRewriter`, `JoinedGap`, `JoinedGapUtilities`, `PatternLineStartUtilities`, `ContinuationLineContributor`, `QueryClauseContributor`); the AGENTS.md phase table is updated for the precheck, line-break, and indentation phases.
5. User documentation: ok. The diff touches no `documentation/rules/` page and none states anything the change makes false: the code-fix output changes (RH3202/3205/3206/3208 across a blank line, RH5111, RH5401, RH5419, RH7101, RH2001 withholdings) are edge-case outputs that the pages do not show, and no page carries a note about a withheld fix.
6. Open questions routed: ok. Every output question of 01, 03, 04, 05, 06 has an answer or a recorded decision (rows 5-8, 11, 12, 14, 18, 20) in `00-lead.md`; 08, 11, 13 have none. Escalation 10 (S5, declaring the remaining comment-held lines) is open for the lead before the auditor, as the lead's notes state. 09's follow-up (unreachable `SameLineGapRulePhaseTests` input) is decided (row 15). No question sits only in a spec or the PR.
7. Requirements covered: ok. Each new active requirement has covering `[Requirement]` tests in the diff (FMT-BREAK-041: 24 tags, FMT-INDENT-034: 34, FMT-INDENT-035: 7, FMT-UNSUP-017: 45, FMT-UNSUP-018: 13, FMT-UNSUP-019: 8); changed ones are tagged too (FMT-BREAK-016/017, FMT-SPACE-010, FMT-STRUCT-006, FMT-INDENT-026, FMT-PIPE-024, RH2001-009, RH5401-003/004, RH5419-004).
8. Test tags: finding 2 (minor). All other tags in the diff appear in a `Tests` table; every requirement named under *Examples for the tests* (06, 11, 13) is tagged on a test the diff adds, including RH2001-009's `Third`, the RH3202/3205/3206/3208 `=>⏎⏎2` fixes (FMT-STRUCT-006), FMT-BREAK-041, and the RH5111 convergence test.
9. Nothing left behind: ok. `sdd/work/` holds only `continuation-indent`, which the lead deletes before publishing.

## By issue
- thoenissen/Reihitsu#953: FMT-INDENT-034, FMT-INDENT-026, FMT-BREAK-041, FMT-BREAK-017, FMT-UNSUP-017/018/019 and their tests are consistent; findings 1 and 2 touch it only as noted.
- thoenissen/Reihitsu#1852: the `return` value join (FMT-BREAK-041) and the expression-body join across a blank line (FMT-STRUCT-006, SHARED-BODY-001, RH3202/3205/3206/3208) are consistent; no finding.
- thoenissen/Reihitsu#1896: FMT-INDENT-035 and its seven tagged tests are consistent; no finding.
- thoenissen/Reihitsu#1818: the declarator behind `#endif` is placed by FMT-INDENT-034 (answer 6A, kept out of FMT-UNSUP-017); consistent; no finding.
