Role: spec checker
Commit: 43087b1
Verdict: findings

Prompt carried no intended fix or verdict beyond the files named. Architecture project run on this head: `Passed!  - Failed: 0, Passed: 117, Skipped: 0, Total: 117`.

## thoenissen/Reihitsu#1140

Narrowed (decisions 3, 20). Spec side: RH5204-006 rewritten to "only the reported line", `Uses` gains SHARED-FIX-006, intro sentence updated; RH5403-003 and SHARED-BRACE-002 are back to `origin/main` (no diff in `RH5403.md`, `statement-braces.md`); `requirement-ids.txt` has no line for them. The RH5204 spec matches the diff (production re-indents one line; analyzer change is #1561). Tags: the added RH5204 fix tests carry RH5204-006.

Judgement for the lead (not a defect): RH5204-006 is a changed requirement, and the README ("When a change doc is required") lists "changed" literally. No change doc exists; decision 13 records why (alignment to unchanged SHARED-FIX-006, maintainer answer 1A). The PR description must state that reason ("when in doubt ... the lead decides"). Same for RH5203-003 (#1144).

## thoenissen/Reihitsu#1141

SHARED-OPSPACE-004 states the narrowed scope and gains the wrapped-gap/operands example; RH6023/RH6024 requirements refer to it and need no change. Added RH6023/RH6024 tests carry SHARED-OPSPACE-004. The RH6024 SHARED-FIX-010 test was rewritten to a "fix offered" test tagged SHARED-OPSPACE-004 (correct: the fix no longer calls the formatter).

FMT-PIPE-024 tag (06's question): the fix no longer calls a node-level entry point (`OperatorSpacingCodeFixProviderBase` now uses `FormattingTextAnalysisUtilities.IsOperatorGapViolation`), so no RH6023 test exercises the target-position fact. The six remaining tags (`RH6023...Tests.cs` lines 483, 587, 691, 745, 795, 845) are stale; each test is a RH6023-002 / SHARED-OPSPACE-004 layout test. Judgement: the tag is wrong, not incidental, and should be removed (finding 1).

## thoenissen/Reihitsu#1144

RH5203-003 (comment line moves into the alignment column, example added, `Uses` gains SHARED-BLANK-002) matches the answer-2A recorded in `00-lead.md` and the diff; tests carry RH5203-003. RH5205-004 added (ledger line inserted at its sorted position), tagged by `NoFixIsOfferedForALambdaWithAnAttributeList`. RH5417/RH3202/RH3102/RH3201/RH5409 shared-line and blank-line results are decided by unchanged SHARED-FIX-006/007; their tests carry SHARED-FIX-007. No spec text states the `else if` column any more (decision 20).

Judgement for the lead: RH5205-004, RH5103-005 and SHARED-ATTR-021 add requirements that withhold a fix that `main` offered; each follows a rare-class decision (7, 8, 9), but the rule says "added with new behavior" needs a change doc, so the PR description should carry the reason as for RH5204-006.

## thoenissen/Reihitsu#1145

RH5103-003 extended (embedded/labeled/local-function predecessors, line of the preceding statement in a block); RH5103-005 added, ledger line present, `Uses` gains FMT-INDENT-028. Tests: twelve added `FixMovesAStatementBehind...` tests tag RH5103-003, two RH5103-005 tests tag RH5103-005. Every row of the spec example has a test. No finding.

## thoenissen/Reihitsu#1094

SHARED-ATTR-021 added (ledger line), SHARED-ATTR-008 points to it, `attribute-targets.md` `Uses` gains FMT-INDENT-028, and the fourteen placement-rule specs name it (RH5513-005 checked: title, sentence, Why, Uses). Rule pages untouched, which is right for a withheld fix. The tests `NoFixIsOfferedWhenTheEnclosingBraceIsOffItsDepthColumn` (SHARED-ATTR-021, RH5513-005) and `FixIsOfferedWhenTheEnclosingLambdaBraceIsAtItsDepthColumn` exist; RH5517 has a SHARED-ATTR-021 test too. The five RH5511 tests tag SHARED-FIX-007. `FixInTabIndentedCodeIndentsTheDeclarationWithSpacesToTheAttributeListsColumn` (RH5513) carries no tag, though 06 states it as a SHARED-FIX-007 whitespace case; untagged characterization tests are allowed, so this is only a note (finding 2).

## thoenissen/Reihitsu#1393

Characterization test `FixMovesACommentBehindTheSecondOfTwoStatementsAboveItWithoutABlankLine` tags RH5104-005; no spec change needed, none made. No finding.

## thoenissen/Reihitsu#1511

Covered by RH5204-006 (example "first type of a file, behind a documentation comment"); tests `FixMovesTheFirstTypeOfAFile...` tag RH5204-006. No finding.

## thoenissen/Reihitsu#1561

No spec change, by decision 11 and answer 4A: RH5204-001 plus SHARED-BLANK-008 already decide it. The analyzer diff (the line of a token behind a multi-line comment's closing line is not checked) agrees with SHARED-BLANK-008 and with the examples in `blank-line-spacing.md` (the `///` brace and the file-start type are still reported). Tests `...BehindTheClosingLineOfAMultiLineCommentIsNotReported` tag RH5204-001 and SHARED-BLANK-008. No finding.

## Whole run

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 8 | `src/Reihitsu.Analyzer.Test/Formatting/RH6023AssignmentOperatorsMustBeSpacedCorrectlyAnalyzerTests.cs` lines 483, 587, 691, 745, 795, 845 | Six tests still carry `[Requirement("FMT-PIPE-024")]`, but the RH6023 fix no longer calls a node-level formatter entry point, so FMT-PIPE-024 (the target-position facts) is not exercised. Remove the tag from all six; keep RH6023-002 and, on the last, SHARED-OPSPACE-004. | test implementer |
| 2 | 8 (note, not blocking) | `RH5513MethodAttributesMustFollowPlacementRulesAnalyzerTests.cs`, `FixInTabIndentedCodeIndentsTheDeclarationWithSpacesToTheAttributeListsColumn` | Untagged; 06 attributes the same case to SHARED-FIX-007 in RH5511. Optional tag. | test implementer |
| 3 | 1 (judgement) | PR description | Changed requirements RH5204-006 and RH5203-003 and the added withheld-fix requirements (RH5103-005, RH5205-004, SHARED-ATTR-021) have no change doc; decisions 13 and 7-9 record the reason. State it in the PR. | lead |

### Checked
1. Change doc: no `sdd/changes` file in the diff; ok under decision 13 plus the PR note (finding 3). No visible output change is listed anywhere that lacks a maintainer answer: answers 1A, 2A, 3A, 4A cover the visible ones.
2. Specs match the diff: ok (format of new requirements checked: one heading each, normative sentence first, `Why:`, examples, markup).
3. IDs: RH5103-005, RH5205-004, SHARED-ATTR-021 inserted at sorted positions; nothing reused, renumbered or deleted; ok.
4. Code owners: no owner renamed, added or removed; `sdd/specs/README.md` unchanged and still matches; `SpecConsistencyTests` inside the 117 passing.
5. User documentation: no `documentation/rules/` file changed. RH5204 page's Correction is still the unchanged-scope example and the page names no formatter, no withheld-fix note; ok.
6. Open questions: spec author handovers 02 and 05 list none; maintainer answers 1-4 are recorded; ok.
7. Coverage: architecture 117/117 passed (`RequirementCoverageTests` included). New active requirements RH5103-005, RH5205-004, SHARED-ATTR-021 each have a covering tagged test.
8. Tags: finding 1 (and note 2). Every requirement the spec author named under *Examples for the tests* (RH5204-006 examples, RH5203-003, RH5103-003 rows, RH5103-005, RH5205-004, SHARED-ATTR-021, SHARED-OPSPACE-004) is tagged on a test the diff adds; the removed RH5403-003/SHARED-BRACE-002 examples no longer exist.
9. Left behind: `sdd/work/fix-scope-and-layout/` only; no other work folder in the diff. The lead deletes it before publishing.
